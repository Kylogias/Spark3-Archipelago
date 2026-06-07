using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using MelonLoader;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json.Linq;
using HarmonyLib;

[assembly: MelonInfo(typeof(Sparkipelago.Sparkipelago), "Sparkipelago", "0.1.0", "Kylogias")]
[assembly: MelonGame("Feperd Games", "Spark the Electric Jester 3")]


namespace Sparkipelago {
	public class Sparkipelago : MelonMod {
		public static Sparkipelago instance;
		public static string currentScene;
		
		public static ArchipelagoSession currentSession;
		public static DeathLinkService deathLink;
		static Dictionary<string, object> slotDataDict;
		int currentSaveSlot = -1;
		public static GameObject player;

		private float itemTimer;
		private static Queue<ItemIds> itemQueue;

		private static Queue<string> messages;
		class DisplayedMessage {
			public GameObject go;
			public float timeLeft;
		}
		private static DisplayedMessage[] messageText;
		
		private GameObject redWorld;
		private GameObject grayWorld;
		public static GameObject flint;
		public static List<GameObject> flintList;
		public static GameObject playerRed;
		public static GameObject playerGray;

		public static GameObject eBubble;
		public static GameObject eCapsule;
		public static GameObject hCapsule;
		public static GameObject sCapsule;
		public static GameObject copter;
		
		public static Dictionary<ItemIds, int> itemState;
		public static string[] shopItems;
		
		public static Texture2D apTexture;
		public static Texture2D settingsTexture;
		public static Texture2D labTexture;

		void loadTexture(ref Texture2D tex, string path) {
			tex = new Texture2D(1, 1);
			tex.hideFlags = HideFlags.DontUnloadUnusedAsset;
			byte[] bytes = File.ReadAllBytes(Path.Combine(Application.dataPath, "../apassets", path));
			ImageConversion.LoadImage(tex, bytes);
		}
		
		public override void OnInitializeMelon() {
			instance = this;
			itemState = new Dictionary<ItemIds, int>();
			foreach(long id in APShared.itemIDs) {
				itemState.Add((ItemIds)id, 0);
			}
			shopItems = new string[26];
			
			LabMode.initPrefs();
			itemQueue = new Queue<ItemIds>();
			messages = new Queue<string>();
			messageText = new DisplayedMessage[5];
			for (int i = 0; i < messageText.Length; i++) {
				messageText[i] = new DisplayedMessage();
			}
			flintList = new List<GameObject>();
			new SlotData();
			
			loadTexture(ref apTexture, "aplogo.png");
			loadTexture(ref settingsTexture, "settings.png");
			loadTexture(ref labTexture, "lab.png");
			
			MusicRandomization.registerMusic();
		}
		
		public void ConnectToArchipelago(bool newServer) {
			if (newServer) {
				foreach (long id in APShared.itemIDs) {
					itemState[(ItemIds)id] = 0;
				}
				itemQueue.Clear();
				if (currentSession != null) {
					currentSession.Items.ItemReceived -= HandleItem;
					currentSession.MessageLog.OnMessageReceived -= OnMessageReceived;
					deathLink.DisableDeathLink();
					currentSession = null;
					deathLink = null;
				}
				new SlotData();
			}
			
			APSavedata data = APSave.getAPSave();
			APConnectionInfo connect = APSave.getAPConnect();
			currentSession = ArchipelagoSessionFactory.CreateSession(connect.ip, connect.port);
			LoginResult result;
			if (connect.password.Length != 0) result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", connect.slot, ItemsHandlingFlags.AllItems, password: connect.password);
			else result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", connect.slot, ItemsHandlingFlags.AllItems);
			
			if (result.Successful) {
				slotDataDict = ((LoginSuccessful)result).SlotData;
				currentSaveSlot = Save.CurrentSaveSlot;

				string curRoom = currentSession.RoomState.Seed;
				if (data.room != "" && data.room != curRoom) {
					string msg = "Client room does not match server room! Do you have the correct connection information?";
					MelonLogger.Error(msg);
					messages.Enqueue(msg);
					currentSession = null;
					return;
				}
				
				if ((long)slotDataDict["version"] != APShared.version) {
					string msg = string.Format("Client Version does not match APWorld (expected {0}, got {1})! Refusing connection", APShared.version, slotDataDict["version"]);
					MelonLogger.Error(msg);
					messages.Enqueue(msg);
					currentSession = null;
					return;
				}

				new SlotData(slotDataDict);
				currentSession.Items.ItemReceived += HandleItem;
				currentSession.MessageLog.OnMessageReceived += OnMessageReceived;
				deathLink = DeathLinkProvider.CreateDeathLinkService(currentSession);
				deathLink.OnDeathLinkReceived += HandleDeathLink;
				if (APSave.file.client.deathLink) deathLink.EnableDeathLink();
				messages.Enqueue("Successful Connection to Spark 3! You may now collect checks");
				data.room = curRoom;
				int i = 0;
				foreach (ItemInfo item in currentSession.Items.AllItemsReceived) {
					onItem(item, i < data.lastItem);
					i++;
				}
				data.lastItem = i;
				while (currentSession.Items.Any()) currentSession.Items.DequeueItem();
				foreach (long location in data.checkedLocations) {
					currentSession.Locations.CompleteLocationChecksAsync(null, location);
				}
			} else {
				MelonLogger.Error("Error while connecting to Archipelago");
				messages.Enqueue("Error while connecting to Archipelago");
				foreach(string e in ((LoginFailure)result).Errors) {
					MelonLogger.Error(e);
					messages.Enqueue(e);
				}
				currentSession = null;
			}
		}
		
		private static void onItem(ItemInfo item, bool catchup)  {
			MelonLogger.Msg("Receiving {0} with ID {1}", item.ItemDisplayName, item.ItemId);
			itemState[(ItemIds)item.ItemId] += 1;
			MelonLogger.Msg("Handling Item");
			if (Items.isStageItem((ItemIds)item.ItemId)) {
				if (!catchup) itemQueue.Enqueue((ItemIds)item.ItemId);
			} else Items.handleItem((ItemIds)item.ItemId, catchup);
		}
		
		public static void debugLog(string fmt, params object[] args) {
			MelonLogger.Msg(string.Format(fmt, args));
			if (SlotData.labMode && currentSession != null) {
				try {
					currentSession.Say(string.Format(fmt, args));
				} catch (Exception) {
					currentSession = null;
				}
			} 
		}

		public static bool hasItem(ItemIds item) {
			return itemState[item] > 0;
		}

		public static void OnMessageReceived(LogMessage message) {
			StringBuilder sb = new StringBuilder("", 65536);
			MelonLogger.Msg("Message Part Count: {0}", message.Parts.Count());
			foreach (MessagePart part in message.Parts) {
				if (!part.IsBackgroundColor) sb.AppendFormat("<color=#{0:X2}{1:X2}{2:X2}>", part.Color.R, part.Color.G, part.Color.B);
				sb.Append(part.Text);
				if (!part.IsBackgroundColor) sb.Append("</color>");
			}
			messages.Enqueue(sb.ToString());
			
			string playerName = APSave.getAPConnect().slot;
			string msgStr = message.ToString();
			if (msgStr.StartsWith(string.Format("{0}: .whereis", playerName))) Collectibles.trackCheckByName(msgStr.Substring(msgStr.IndexOf(' ')+1));
			if (msgStr.StartsWith(string.Format("{0}: .whereindex", playerName))) Collectibles.trackCheckByIndex(msgStr.Substring(msgStr.IndexOf(' ')+1));
			if (msgStr.StartsWith(string.Format("{0}: .whatgate", playerName))) WorldMap.findLevelGate(msgStr.Substring(msgStr.IndexOf(' ')+1));
		}
		
		public static void HandleItem(ReceivedItemsHelper itemHandler) {
			APSavedata data = APSave.getAPSave();
			while (itemHandler.Any()) {
				int oldIndex = itemHandler.Index;
				if (oldIndex <= data.lastItem) continue;
				ItemInfo item = itemHandler.DequeueItem();
				data.lastItem = oldIndex;
				onItem(item, false);
			}
		}

		static bool isDeathLink;
		public static void HandleDeathLink(DeathLink deathLink) {
			if (player) {
				StringBuilder sb = new StringBuilder("", 65536);
				sb.Append("<color=#E02010>Death Link: ");
				if (deathLink.Cause != null) sb.Append(deathLink.Cause);
				else sb.Append(deathLink.Source);
				sb.Append("</color>");
				messages.Enqueue(sb.ToString());
				
				PlayerHealthAndStats.PlayerHP = -1;
				isDeathLink = true;
				player.GetComponent<HurtControl>().CheckForDeathAndKill();
			}
		}

		[HarmonyPatch(typeof(HurtControl), "PlayedDied")]
		private static class OnDeathPatch {
			private static void Prefix() {
				if (APSave.file.client.deathLink && !isDeathLink) {
					deathLink.SendDeathLink(new DeathLink(APSave.getAPConnect().slot));
				}
				isDeathLink = false;
			}
		}
		
		public static void HandleShopScout(Dictionary<long, ScoutedItemInfo> scouted) {
			for (int i = 0; i < 26; i++) {
				long id = APShared.shop[i].id;
				if (scouted.ContainsKey(id)) shopItems[i] = string.Format("{0} for {1}", scouted[id].ItemName, scouted[id].Player.Name);
				id += 1;
			}
		}
		
		public override void OnUpdate() {
			if (player) {
				if (itemTimer < 0 && itemQueue.Count > 0) {
					itemTimer = 2;
					ItemIds item = itemQueue.Dequeue();
					Items.handleItem(item, false);
				}
				itemTimer -= Time.deltaTime;
				foreach (GameObject fl in flintList) {
					if (fl == null) continue;
					if (Vector3.Distance(fl.transform.position, player.transform.position) > 100 && hasItem(ItemIds.COMBAT)) {
						Vector3 pos = player.transform.position;
						pos.y += 5;
						fl.transform.position = pos;
					}
				}
				Collectibles.updateTracker();
			}
			foreach (DisplayedMessage dm in messageText) {
				dm.timeLeft -= Time.unscaledDeltaTime;
				if (dm.go) {
					if (dm.timeLeft < 0) dm.go.SetActive(false);
					else dm.go.SetActive(true);
				}
			}
			if (messages.Count() > 0 && messageText[0].go != null) {
				int index = -1;
				bool found = false;
				for (index = 0; index < messageText.Count(); index++) {
					if (!messageText[index].go.activeSelf) {
						found = true;
						break;
					}
				}
				if (!found) {
					float maxY = -100000;
					index = -1;
					for (int i = 0; i < messageText.Count(); i++) {
						RectTransform xfrm = messageText[i].go.GetComponent<RectTransform>();
						if (xfrm.localPosition.y > maxY) {
							maxY = xfrm.localPosition.y;
							index = i;
						}
					}
					messageText[index].timeLeft = -1;
				} else {
					RectTransform textRect = messageText[index].go.GetComponent<RectTransform>();
					textRect.localPosition = new Vector3(0, -((Screen.height/2)-(Screen.height/16)), 0);
					textRect.sizeDelta = new Vector2(Screen.width, Screen.height/8);
					for (int i = 0; i < messageText.Count(); i++) {
						if (!messageText[i].go.activeSelf) continue;
						RectTransform moveup = messageText[i].go.GetComponent<RectTransform>();
						moveup.localPosition = new Vector3(0, moveup.localPosition.y + moveup.sizeDelta.y/2, 0);
					}

					messageText[index].timeLeft = 5;
					Text text = messageText[index].go.GetComponent<Text>();
					text.text = messages.Dequeue();
				}
			}
			if (SlotData.labMode && currentSession != null) {
				LabMode.checkForInput();
			}
		}
		
		public static void setupPrefabChildren(GameObject prefab, bool center, string[] exclude) {
			if (exclude != null && exclude.Contains(prefab.name)) return;
			prefab.hideFlags = HideFlags.HideAndDontSave;
			if (center) prefab.transform.position = new Vector3(0, 0, 0);
			for (int i = 0; i < prefab.transform.childCount; i++) {
				setupPrefabChildren(prefab.transform.GetChild(i).gameObject, center, exclude);
			}
		}
		
		private void setupPrefabs() {
			GameObject prefabHolder = GameObject.Find("[PREFABS HOLDER]");
			prefabHolder.transform.GetChild(0).gameObject.SetActive(true);
			prefabHolder.transform.GetChild(9).gameObject.SetActive(true);
			prefabHolder.transform.GetChild(12).gameObject.SetActive(true); // Should be AbyssPrefabs
			Transform bossXfrm = prefabHolder.transform.GetChild(2);
			
			GameObject prefabObject = new GameObject("AP Prefabs");
			prefabObject.SetActive(false);
			prefabObject.hideFlags = HideFlags.HideAndDontSave;

			EnemyRando.setupEnemyRando(prefabHolder, prefabObject);
			
			GameObject redPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RedGhostWorld");
			GameObject grayPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/GrayGhostWorld");
			GameObject ragingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RagingGhost");
			GameObject wanderingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/MinorGhost");
			GameObject lazerPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/LazerFirerer");
			GameObject flintPrefab = bossXfrm.Find("PowerFlintModel").gameObject;
			GameObject eCapPrefab = GameObject.Find("[Core prefabs]/EnergyCapsule");
			GameObject hCapPrefab = GameObject.Find("[Core prefabs]/Capsule");
			GameObject sCapPrefab = GameObject.Find("[Core prefabs]/Capsule_Score");
			GameObject eBubPrefab = GameObject.Find("[Core prefabs]/EnergyBubble");
			GameObject copterPrefab = GameObject.Find("[PREFABS HOLDER]/[CityPrefabs]/PlayableCopter");
			
			GameObject ragingInstance = UnityEngine.Object.Instantiate(ragingPrefab, prefabObject.transform);
			setupPrefabChildren(ragingInstance, true, null);
			GameObject wanderingInstance = UnityEngine.Object.Instantiate(wanderingPrefab, prefabObject.transform);
			setupPrefabChildren(wanderingInstance, true, null);
			GameObject lazerInstance = UnityEngine.Object.Instantiate(lazerPrefab, prefabObject.transform);
			setupPrefabChildren(lazerInstance, true, null);
			
			redWorld = UnityEngine.Object.Instantiate(redPrefab, prefabObject.transform);
			setupPrefabChildren(redWorld, true, null);
			RedWorldSequence redSeq = redWorld.GetComponent<RedWorldSequence>();
			redSeq.RagingGhost = ragingInstance;
			redSeq.WanderingGhost = wanderingInstance;
			
			grayWorld = UnityEngine.Object.Instantiate(grayPrefab, prefabObject.transform);
			setupPrefabChildren(grayWorld, true, null);
			GrayWorldSequence graySeq = grayWorld.GetComponent<GrayWorldSequence>();
			graySeq.GrayLazer = lazerInstance;

			eBubble = UnityEngine.Object.Instantiate(eBubPrefab, prefabObject.transform);
			setupPrefabChildren(eBubble, true, null);
			eCapsule = UnityEngine.Object.Instantiate(eCapPrefab, prefabObject.transform);
			setupPrefabChildren(eCapsule, true, null);
			hCapsule = UnityEngine.Object.Instantiate(hCapPrefab, prefabObject.transform);
			setupPrefabChildren(hCapsule, true, null);
			sCapsule = UnityEngine.Object.Instantiate(sCapPrefab, prefabObject.transform);
			setupPrefabChildren(sCapsule, true, null);
			flint = UnityEngine.Object.Instantiate(flintPrefab, prefabObject.transform);
			setupPrefabChildren(flint, true, null);
			copter = UnityEngine.Object.Instantiate(copterPrefab, prefabObject.transform);
			setupPrefabChildren(copter, true, null);
		}
		
		public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
			itemTimer = -1;
			flintList.Clear();
			debugLog("Scene Loaded: {0}", sceneName);
			currentScene = sceneName;
			player = GameObject.Find("Player_Fark");

			if (APSave.sparkFont != null) {
				GameObject canvasGO = new GameObject("APCanvas", typeof(Canvas), typeof(CanvasScaler));
				Canvas canvas = canvasGO.GetComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				
				for (int i = 0; i < messageText.Count(); i++) {
					GameObject textGO = new GameObject("APText", typeof(Text), typeof(DeactivateAfterAWhile));
					messageText[i].go = textGO;
					messageText[i].timeLeft = 0;
					textGO.SetActive(false);
					textGO.transform.parent = canvasGO.transform;
					Text text = textGO.GetComponent<Text>();
					text.font = APSave.sparkFont;
					text.text = "chat looks a bit dead";
					text.alignment = TextAnchor.MiddleCenter;
					text.resizeTextForBestFit = true;
					text.fontSize = 1000;
					text.supportRichText = true;
					RectTransform textRect = textGO.GetComponent<RectTransform>();
					textRect.localPosition = new Vector3(0, -((Screen.height/2)-(Screen.height/16)), 0);
					textRect.sizeDelta = new Vector2(Screen.width, Screen.height/8);
					DeactivateAfterAWhile daaw = textGO.GetComponent<DeactivateAfterAWhile>();
					daaw.TimeToStop = 5.0f;
				}
			}

			if (player != null) {
				float score = APSave.file.client.scoreAmt * itemState[ItemIds.PROGRESSIVE_SCORE];
				if (score > APSave.file.client.scoreMax) score = APSave.file.client.scoreMax;
				ScoreManager.Charge = score;
				Collectibles.onSceneLoad(sceneName);
				Options.buildCategories();
				
				GameObject fogMesh = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera/FogMeshPlayer");
				if (fogMesh && redWorld && grayWorld) {
					playerRed = UnityEngine.Object.Instantiate(redWorld, fogMesh.transform);
					playerGray = UnityEngine.Object.Instantiate(grayWorld, fogMesh.transform);
					playerRed.GetComponent<SetParent>().Parent = fogMesh.transform;
					playerGray.GetComponent<SetParent>().Parent = fogMesh.transform;
				} else {
					playerRed = null;
					playerGray = null;
				}
			} else {
				playerRed = null;
				playerGray = null;
			}
			
			if (sceneName == "[LOGO]" && redWorld == null && grayWorld == null) {
				SceneManager.LoadScene("[STAGE 0X - LEVEL TEMPLATE]");
			}
			
			if (sceneName == "[STAGE 0X - LEVEL TEMPLATE]" && redWorld == null && grayWorld == null) {
				setupPrefabs();
				SceneManager.LoadScene("[LOGO]");
			}

			if (sceneName == "[SAVE SELECT]") {
				currentSaveSlot = -1;
				APSave.onSaveSelect();
			}
			
			if (sceneName == "[CUTSCENE 01 - INTRO CUTSCENE]") {
				WorldMap.initializeSave();
				APSave.clearAPSave();
			}
			
			if (sceneName == "[CUTSCENE 16 - ENDING CUTSCENE]") {
				currentSession.SetGoalAchieved();
			}
			
			if (sceneName == "[CUTSCENE 02 - FLINT CUTSCENE]" || sceneName == "[CUTSCENE 04 - FLINT SECOND CUTSCENE]") {
				SceneController.LoadMapScreen();
			}
			
			if (sceneName == "[WORLD MAP]") {
				if (currentSaveSlot != Save.CurrentSaveSlot) {
					ConnectToArchipelago(true);
				}
				
				WorldMap.onMapLoad();
			}

			if (sceneName == "[SHOP]") {
				long[] scout = new long[26];
				for (int i = 0; i < 26; i++) { // 26 shop locations
					scout[i] = APShared.shop[i].id;
				}
				currentSession.Locations.ScoutLocationsAsync(HandleShopScout, scout);
			}
			
			if (sceneName == "[STAGE COMPLETE SCREEN]") {
				int idx = StageConpleteControl.LevelJustInIndex;
				Locations.onLevelComplete(idx);
			}
		}
	}
}