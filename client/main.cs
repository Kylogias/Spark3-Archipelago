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
using Newtonsoft.Json.Linq;
using HarmonyLib;

[assembly: MelonInfo(typeof(Sparkipelago.Sparkipelago), "Sparkipelago", "0.1.0", "Kylogias")]
[assembly: MelonGame("Feperd Games", "Spark the Electric Jester 3")]


namespace Sparkipelago {
	public class Sparkipelago : MelonMod {
		public static Sparkipelago instance;
		public static string currentScene;
		
		public static ArchipelagoSession currentSession;
		static Dictionary<string, object> slotDataDict;
		public static int currentSaveSlot = -1;
		public static GameObject player;
		public static int levelsUnlocked;

		public static Queue<string> messages;
		class DisplayedMessage {
			public GameObject go;
			public float timeLeft;
		}
		private static DisplayedMessage[] messageText;
		
		public static GameObject copter;
		
		public static Dictionary<ItemIds, int> itemState;
		public static string[] shopItems;
		
		public static Texture2D apTexture;
		public static Texture2D settingsTexture;
		public static Texture2D labTexture;
		public static List<Texture2D> ddbtnTextures;

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
			messages = new Queue<string>();
			messageText = new DisplayedMessage[5];
			for (int i = 0; i < messageText.Length; i++) {
				messageText[i] = new DisplayedMessage();
			}
			new SlotData();
			
			loadTexture(ref apTexture, "aplogo.png");
			loadTexture(ref settingsTexture, "settings.png");
			loadTexture(ref labTexture, "lab.png");

			ddbtnTextures = new List<Texture2D>();
			for (int i = 0; i <= 5; i++) {
				Texture2D tex = null;
				loadTexture(ref tex, string.Format("dd{0}.png", i));
				ddbtnTextures.Add(tex);
			}
			
			MusicRandomization.registerMusic();
		}
		
		public static void ConnectToArchipelago(bool newServer) {
			if (newServer) {
				foreach (long id in APShared.itemIDs) {
					itemState[(ItemIds)id] = 0;
				}
				levelsUnlocked = 0;
				Traps.onDisconnect();
				if (currentSession != null) {
					currentSession.Items.ItemReceived -= HandleItem;
					currentSession.MessageLog.OnMessageReceived -= OnMessageReceived;
					currentSession = null;
					Bounce.onDisconnect();
				}
				new SlotData();
			}
			
			messages.Clear();
			foreach (DisplayedMessage dm in messageText) {
				messages.Enqueue("");
				messages.Enqueue("");
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

				int version = (int)(long)slotDataDict["version"];
				if (APShared.version < version || APShared.min_version > version) {
					string msg = string.Format("Client Version does not match APWorld (expected {0}-{2}, got {1})! Refusing connection", APShared.version, slotDataDict["version"], APShared.min_version);
					MelonLogger.Error(msg);
					messages.Enqueue(msg);
					currentSession = null;
					return;
				}

				new SlotData(slotDataDict);
				currentSession.Items.ItemReceived += HandleItem;
				currentSession.MessageLog.OnMessageReceived += OnMessageReceived;
				Bounce.onConnect();
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
				messages.Enqueue("<color=#E02010>Error while connecting to Archipelago</color>");
				foreach(string e in ((LoginFailure)result).Errors) {
					MelonLogger.Error(e);
					messages.Enqueue(string.Format("<color=#E02010>{0}</color>", e));
				}
				currentSession = null;
			}
		}
		
		private static void onItem(ItemInfo item, bool catchup)  {
			MelonLogger.Msg("Receiving {0} with ID {1}", item.ItemDisplayName, item.ItemId);
			itemState[(ItemIds)item.ItemId] += 1;
			if (Traps.isStageItem((ItemIds)item.ItemId)) {
				if (!catchup) Traps.itemQueue.Enqueue((ItemIds)item.ItemId);
			} else Items.handleItem((ItemIds)item.ItemId, catchup);
		}
		
		public static void debugLog(string fmt, params object[] args) {
			if (SlotData.labMode && currentSession != null) {
				try {
					MelonLogger.Msg(string.Format(fmt, args));
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
		
		public static void HandleShopScout(Dictionary<long, ScoutedItemInfo> scouted) {
			for (int i = 0; i < 26; i++) {
				long id = APShared.shop[i].id;
				if (scouted.ContainsKey(id)) shopItems[i] = string.Format("{0} for {1}", scouted[id].ItemName, scouted[id].Player.Name);
				id += 1;
			}
		}

		static float groundTime;
		public override void OnUpdate() {
			if (player) {
				bool onGround = player.GetComponent<PlayerBhysics>().Grounded;
				if (onGround) groundTime += Time.deltaTime;
				else groundTime = 0;
				if (groundTime > 1) {
					double energyRegen = 0;
					for (int i = 0; i < itemState[ItemIds.PROGRESSIVE_ENERGY]; i++) {
						energyRegen += APSave.file.client.energyAmt;
					}
					if (energyRegen > APSave.file.client.energyMax) energyRegen = APSave.file.client.energyMax;
					PlayerHealthAndStats.Energy += (float)energyRegen * Time.deltaTime;
				}
				Traps.onUpdate();
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
			for (int i = 0; i < prefabHolder.transform.childCount; i++) {
				prefabHolder.transform.GetChild(i).gameObject.SetActive(true);
			}
			
			GameObject prefabObject = new GameObject("AP Prefabs");
			prefabObject.SetActive(false);
			prefabObject.hideFlags = HideFlags.HideAndDontSave;
			Traps.collectPrefabs(prefabHolder, prefabObject);

			EnemyRando.setupEnemyRando(prefabHolder, prefabObject);
			
			GameObject copterPrefab = GameObject.Find("[PREFABS HOLDER]/[CityPrefabs]/PlayableCopter");

			copter = UnityEngine.Object.Instantiate(copterPrefab, prefabObject.transform);
			setupPrefabChildren(copter, true, null);
		}
		
		public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
			groundTime = 0;
			debugLog("Scene Loaded: {0}", sceneName);
			currentScene = sceneName;
			player = GameObject.Find("Player_Fark");

			GameObject canvasGO = null;
			if (APSave.sparkFont != null) {
				canvasGO = new GameObject("APCanvas", typeof(Canvas), typeof(CanvasScaler));
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
				DowndashButtons.createButtons();
				Collectibles.onSceneLoad(sceneName);
				Options.buildCategories();
				Traps.onSceneLoad(true);
			} else {
				Traps.onSceneLoad(false);
			}
			
			if (sceneName == "[LOGO]" && !Traps.initialized) {
				SceneManager.LoadScene("[STAGE 0X - LEVEL TEMPLATE]");
			}
			
			if (sceneName == "[STAGE 0X - LEVEL TEMPLATE]" && !Traps.initialized) {
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
				if (SlotData.goal == GoalType.Utopia) currentSession.SetGoalAchieved();
				else Locations.sendLocationCheck(50, "Completion");
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