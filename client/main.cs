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
	// RingGiverControl
	// EnergyGiverControl
	// Objects_Interaction
	// ScoreManager
	
	public enum ItemIds : long {
		FREEDOM_MEDAL = 1,
		DASH,
		JESTER_DASH,
		CHARGED_DASH,
		DOWN_DASH,
		WALL_JUMP,
		DOUBLE_JUMP,
		COMBAT,
		SCORE_CAPSULE, // 1000 Score
		HEALTH_CAPSULE, // 1 health
		ENERGY_CAPSULE, // 10 energy
		BIT_BUBBLE, // 30 bits
		ENERGY_BUBBLE, // 20 energy
		NIGHTMARE_TRAP,
		LASER_TRAP,
		DUST_TRAP,
		SPIN_CHARGE,
		DUAL_AIR_KICK,
		DUAL_AIR_SLASH,
		EXTRA_FINISHER,
		SKYWARD_SLASH,
		DOUBLE_DOWN_SPIN,
		ABRUPT_FINISHER,
		DUPLEX_SLASH,
		SPEED_BUFF,
		HYPER_SURGE,
		ENERGY_DASH,
		OVERCHARGE,
		SNAP_PORTAL,
		RADAR_SCOUT,
		MULTISHOT_BLAST,
		HEAL,
		CLOUD_SHOT,
		TEMP_SHIELD,
		CHARGED_SHOT,
		RAIL_BOOST,
		REGEN_BREAKING,
		JESTER_SWIPE,
		REAPER,
		FLOAT,
		FARK,
		SFARX,
		SHOP_MOVES,
		SHOP_POWERS,
		SHOP_UPGRADES,
		SHOP_CHARACTERS,
		END,
		PREFIX = 16295350000
	}
	
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
		
		public static int[] itemState;
		public static string[] shopItems;
		public static int musicRando = 0;
		public static int musicSeed = 0;
		
		public override void OnInitializeMelon() {
			instance = this;
			itemState = new int[(long)ItemIds.END];
			shopItems = new string[26];
			
			LabMode.initPrefs();
			
			MusicRandomization.registerMusic();
		}
		
		public void ConnectToArchipelago(bool newServer) {
			if (newServer) {
				Array.Clear(itemState, 0, itemState.Length);
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
				currentSession.Items.ItemReceived += HandleItem;
				slotData = ((LoginSuccessful)result).SlotData;
				currentSaveSlot = Save.CurrentSaveSlot;

				string curRoom = currentSession.RoomState.Seed;
				if (data.room != "" && data.room != curRoom) {
					MelonLogger.Error("Client room does not match server room! Do you have the correct connection information?");
					currentSession.Say("Client room does not match server room! Do you have the correct connection information?");
					currentSession = null;
					return;
				}
				
				if ((long)slotData["version"] != APShared.version) {
					MelonLogger.Error("Client Version does not match APWorld! Refusing connection");
					currentSession.Say("Client Version does not match APWorld! Refusing connection");
					currentSession = null;
					return;
				}

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
			MelonLogger.Msg("Receiving {0} with ID {1} (index {2})", item.ItemDisplayName, item.ItemId, item.ItemId-(long)ItemIds.PREFIX);
			itemState[item.ItemId-(long)ItemIds.PREFIX] += 1;
			MelonLogger.Msg("Handling Item");
			Items.handleItem(item, catchup);
		}
		
		public static void debugLog(string fmt, params object[] args) {
			MelonLogger.Msg(string.Format(fmt, args));
			if ((long)Sparkipelago.slotData["labmode"] != 0 && currentSession != null) {
				currentSession.Say(string.Format(fmt, args));
			} 
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