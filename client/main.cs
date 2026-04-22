using UnityEngine;
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
		private MelonPreferences_Category connectionCategory;
		private MelonPreferences_Entry<string> ip;
		private MelonPreferences_Entry<int> port;
		private MelonPreferences_Entry<string> username;
		private MelonPreferences_Entry<string> password;
		
		public static Sparkipelago instance;
		public static string currentScene;
		
		public static ArchipelagoSession currentSession;
		public static Dictionary<string, object> slotData;
		int currentSaveSlot = -1;
		GameObject player;
		
		int currentItem = -1;
		public static int[] itemState;
		public static string[] shopItems;
		public static int musicRando = 0;
		public static int musicSeed = 0;
		
		public override void OnInitializeMelon() {
			instance = this;
			itemState = new int[(long)ItemIds.END];
			shopItems = new string[26];
			
			connectionCategory = MelonPreferences.CreateCategory("Archipelago Connection");
			ip = MelonPreferences.CreateEntry<string>("Archipelago Connection", "IP", "localhost");
			port = MelonPreferences.CreateEntry<int>("Archipelago Connection", "Port", 38281);
			username = MelonPreferences.CreateEntry<string>("Archipelago Connection", "Username", "Player1");
			password = MelonPreferences.CreateEntry<string>("Archipelago Connection", "Password", "");
			
			MusicRandomization.registerMusic();
		}
		
		public void ConnectToArchipelago(bool newServer) {
			if (newServer) {
				currentItem = -1;
				Array.Clear(itemState, 0, itemState.Length);
			}
			
			currentSession = ArchipelagoSessionFactory.CreateSession(ip.Value, port.Value);
			LoginResult result;
			if (password.Value.Length != 0) result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", username.Value, ItemsHandlingFlags.AllItems, password: password.Value);
			else result = currentSession.TryConnectAndLogin("Spark the Electric Jester 3", username.Value, ItemsHandlingFlags.AllItems);
			
			if (result.Successful) {
				currentSession.Items.ItemReceived += HandleItem;
				slotData = ((LoginSuccessful)result).SlotData;
				currentSaveSlot = Save.CurrentSaveSlot;
				
				if ((long)slotData["version"] != APShared.version) {
					MelonLogger.Error("Client Version does not match APWorld! Refusing connection");
					currentSession.Say("Client Version does not match APWorld! Refusing connection");
					currentSession = null;
					return;
				}
				
				foreach (ItemInfo item in currentSession.Items.AllItemsReceived) {
					onItem(item, true);
				}
				while (currentSession.Items.Any()) currentSession.Items.DequeueItem();
				
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
		
		public static void HandleItem(ReceivedItemsHelper itemHandler) {
			while (itemHandler.Any()) {
				int oldIndex = itemHandler.Index;
				if (oldIndex <= instance.currentItem) continue;
				ItemInfo item = itemHandler.DequeueItem();
				instance.currentItem = oldIndex;
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
		
		public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
			MelonLogger.Msg("Scene Loaded: " + sceneName);
			currentScene = sceneName;
			player = GameObject.Find("Player_Fark");
			Collectibles.onSceneLoad();
			if (sceneName == "[CUTSCENE 01 - INTRO CUTSCENE]") {
				WorldMap.initializeSave();
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