using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using MelonLoader;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

[assembly: MelonInfo(typeof(Sparkipelago.Sparkipelago), "Sparkipelago", "0.1.0", "Kylogias")]
[assembly: MelonGame("Feperd Games", "Spark the Electric Jester 3")]


namespace Sparkipelago {
	public class Sparkipelago : MelonMod {
		public static Sparkipelago instance;
		public static string currentScene;
		
		public static ArchipelagoSession currentSession;
		public static Dictionary<string, object> slotData;
		int currentSaveSlot = -1;
		GameObject player;
		
		private GameObject redWorld;
		private GameObject grayWorld;
		public static GameObject playerRed;
		public static GameObject playerGray;
		
		public static Dictionary<ItemIds, int> itemState;
		public static string[] shopItems;
		public static int musicRando = 0;
		public static int musicSeed = 0;
		
		public override void OnInitializeMelon() {
			instance = this;
			itemState = new Dictionary<ItemIds, int>();
			foreach(long id in APShared.itemIDs) {
				itemState.Add((ItemIds)id, 0);
			}
			shopItems = new string[26];
			
			LabMode.initPrefs();
			
			MusicRandomization.registerMusic();
		}
		
		public void ConnectToArchipelago(bool newServer) {
			if (newServer) {
				foreach (long id in APShared.itemIDs) {
					itemState[(ItemIds)id] = 0;
				}
				if (currentSession != null) {
					currentSession.Items.ItemReceived -= HandleItem;
				}
			}
			
			APSavedata data = APSave.getAPSave();
			APConnectionInfo connect = APSave.getAPConnect();
			currentSession = ArchipelagoSessionFactory.CreateSession(connect.ip, connect.port);
			LoginResult result;
			if (connect.password.Length != 0) result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", connect.slot, ItemsHandlingFlags.AllItems, password: connect.password);
			else result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", connect.slot, ItemsHandlingFlags.AllItems);
			
			if (result.Successful) {
				slotData = ((LoginSuccessful)result).SlotData;
				currentSaveSlot = Save.CurrentSaveSlot;

				string curRoom = currentSession.RoomState.Seed;
				if (data.room != "" && data.room != curRoom) {
					MelonLogger.Error("Client room does not match server room! Do you have the correct connection information?");
					currentSession.Say("Client room does not match server room! Do you have the correct connection information?");
					currentSession = null;
					slotData = null;
					return;
				}
				
				if ((long)slotData["version"] != APShared.version) {
					MelonLogger.Error("Client Version does not match APWorld! Refusing connection");
					currentSession.Say("Client Version does not match APWorld! Refusing connection");
					currentSession = null;
					slotData = null;
					return;
				}

				currentSession.Items.ItemReceived += HandleItem;
				data.room = curRoom;
				int i = 0;
				foreach (ItemInfo item in currentSession.Items.AllItemsReceived) {
					onItem(item, i < data.lastItem);
					i++;
				}
				data.lastItem = i;
				while (currentSession.Items.Any()) currentSession.Items.DequeueItem();
				foreach (long location in data.checkedLocations) {
					currentSession.Locations.CompleteLocationChecks(location);
				}
				
				musicRando = (int)(long)slotData["musicchoice"];
				musicSeed = (int)(long)slotData["musicseed"];
			} else {
				MelonLogger.Error("Error while connecting to Archipelago");
				foreach(string e in ((LoginFailure)result).Errors) {
					MelonLogger.Error(e);
				}
			}
		}
		
		private static void onItem(ItemInfo item, bool catchup)  {
			MelonLogger.Msg("Receiving {0} with ID {1}", item.ItemDisplayName, item.ItemId);
			itemState[(ItemIds)item.ItemId] += 1;
			MelonLogger.Msg("Handling Item");
			Items.handleItem((ItemIds)item.ItemId, catchup);
		}
		
		public static void debugLog(string fmt, params object[] args) {
			MelonLogger.Msg(string.Format(fmt, args));
			if ((long)Sparkipelago.slotData["labmode"] != 0 && currentSession != null) {
				currentSession.Say(string.Format(fmt, args));
			} 
		}

		public static bool hasItem(ItemIds item) {
			return itemState[item] > 0;
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
		
		public override void OnUpdate() {
			if (slotData == null) return;
			if ((long)slotData["labmode"] != 0 && currentSession != null) {
				LabMode.checkForInput();
			}
		}
		
		private void setupPrefabChildren(GameObject prefab) {
			prefab.hideFlags = HideFlags.HideAndDontSave;
			prefab.transform.position = new Vector3(0, 0, 0);
			for (int i = 0; i < prefab.transform.childCount; i++) {
				setupPrefabChildren(prefab.transform.GetChild(i).gameObject);
			}
		}
		
		private void setupPrefabs() {
			GameObject prefabHolder = GameObject.Find("[PREFABS HOLDER]");
			prefabHolder.transform.GetChild(12).gameObject.SetActive(true); // Should be AbyssPrefabs
			
			GameObject prefabObject = new GameObject("AP Prefabs");
			prefabObject.SetActive(false);
			prefabObject.hideFlags = HideFlags.HideAndDontSave;
			
			GameObject redPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RedGhostWorld");
			GameObject grayPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/GrayGhostWorld");
			GameObject ragingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RagingGhost");
			GameObject wanderingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/MinorGhost");
			GameObject lazerPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/LazerFirerer");
			
			GameObject ragingInstance = UnityEngine.Object.Instantiate(ragingPrefab, prefabObject.transform);
			setupPrefabChildren(ragingInstance);
			GameObject wanderingInstance = UnityEngine.Object.Instantiate(wanderingPrefab, prefabObject.transform);
			setupPrefabChildren(wanderingInstance);
			GameObject lazerInstance = UnityEngine.Object.Instantiate(lazerPrefab, prefabObject.transform);
			setupPrefabChildren(lazerInstance);
			
			redWorld = UnityEngine.Object.Instantiate(redPrefab, prefabObject.transform);
			setupPrefabChildren(redWorld);
			RedWorldSequence redSeq = redWorld.GetComponent<RedWorldSequence>();
			redSeq.RagingGhost = ragingInstance;
			redSeq.WanderingGhost = wanderingInstance;
			
			grayWorld = UnityEngine.Object.Instantiate(grayPrefab, prefabObject.transform);
			setupPrefabChildren(grayWorld);
			GrayWorldSequence graySeq = grayWorld.GetComponent<GrayWorldSequence>();
			graySeq.GrayLazer = lazerInstance;
		}
		
		public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
			MelonLogger.Msg("Scene Loaded: " + sceneName);
			currentScene = sceneName;
			player = GameObject.Find("Player_Fark");
			if (player != null) {
				Collectibles.onSceneLoad(sceneName);
				
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
			
			if (sceneName == "[STAGE 0X - LEVEL TEMPLATE]") {
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