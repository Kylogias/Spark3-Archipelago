using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using MelonLoader;

namespace Sparkipelago {
	class WorldMap {
		public static void initializeSave() {
			Save.SaveFile savefile = Save.Saves[Save.CurrentSaveSlot];
			
			// Disable the tutorial cards
			savefile.AllMovesReminder = true;
			savefile.WallWalk = true;
			savefile.Rail = true;
			savefile.FallDamage = true;
			savefile.ComboBar = true;
			savefile.CombatBasics = true;
			savefile.GameProgressInfo = true;
			savefile.SpecialMedals = true;
			savefile.Shop = true;
			savefile.CollectStage = true;
			savefile.RaidStage = true;
			savefile.EscapeStage = true;
			savefile.SuperMoves = true;
			
			savefile.Power_Fark_Unlocked = true;
			savefile.Power_Sfarx_Unlocked = true;
			savefile.Power_Float_Unlocked = true;
			savefile.Power_Reaper_Unlocked = true;
			
			// Make sure the save has enough freedom medals (and unlock every level just to be safe)
			int stagecount = savefile.StageUnlocked.Count();
			for (int i = 0; i < stagecount; i++) {
				savefile.StageUnlocked[i] = true;
				savefile.StageJustUnlocked[i] = false;
				if (i >= 200) {
					savefile.StageCompleted[i] = true;
				}
			}
			
			SceneController.LoadMapScreen();
		}
		
		static bool placeStage(JToken lvlinfo, LevelData level) {
			if ((int)((float)lvlinfo[0]) == level.ID) {
				Vector3 newpos;
				newpos.x = (float)lvlinfo[1];
				newpos.y = (float)lvlinfo[2];
				newpos.z = level.gameObject.transform.position.z;
				level.gameObject.transform.position = newpos;
				
				Transform line = level.gameObject.transform.Find("Line");
				if (line) {
					line.gameObject.SetActive(false);
				}
				Transform line1 = level.gameObject.transform.Find("Line_1");
				if (line1) {
					line1.gameObject.SetActive(false);
				}
				return true;
			}
			return false;
		}
		
		public static void onMapLoad() {
			// Randomize Entrances
			LevelData[] levels = GameObject.Find("Map/Stages").GetComponentsInChildren<LevelData>(true);
			
			int[] bossids = {9, 24, 37, 38};
			
			int[] freedomReqs = new int[10];
			int i = 0;
			foreach (JToken freq in ((JArray)Sparkipelago.slotData["freedom_requirements"])) {
				freedomReqs[i] = (int)freq;
				i++;
			}
			
			i = 0;
			foreach (JToken boss in ((JArray)Sparkipelago.slotData["bosses"])) {
				bossids[i] = (int)((float)boss[0]);
				i++;
			}
			
			Save.SaveFile save = Save.GetCurrentSave();
			foreach(LevelData level in levels) {
				bool unlocked = false;
				if (level.ID == -99) {
					Vector3 newpos;
					newpos.x = 0;
					newpos.y = 0.75f;
					newpos.z = level.gameObject.transform.position.z;
					level.gameObject.transform.position = newpos;
					unlocked = true;
				}
				
				i = 0;
				foreach (JToken boss in ((JArray)Sparkipelago.slotData["bosses"])) {
					if (placeStage(boss, level) && Sparkipelago.itemState[(long)ItemIds.FREEDOM_MEDAL] >= freedomReqs[i]) unlocked = true;
					i++;
				}
				
				i = 0;
				foreach (JToken gate in ((JArray)Sparkipelago.slotData["gates"])) {
					if (i > 0 && i < 5) {
						if (!save.StageCompleted[bossids[i-1]]) {i++; continue;}
					} else if (i == 5) {
						if (!(Sparkipelago.itemState[(long)ItemIds.FREEDOM_MEDAL] >= freedomReqs[4] && save.Power_Fark && save.Power_Sfarx)) {i++; continue;}
					}
					foreach (JToken lvlinfo in (JArray)gate) {
						if (placeStage(lvlinfo, level)) unlocked = true;
					}
					i++;
				}
				
				// Check IDs against those unlocked in server
				if (unlocked) {
					level.gameObject.SetActive(true);
				} else {
					level.gameObject.SetActive(false);
				}
			}

			UnityEngine.UI.Text fplabel = GameObject.Find("UI/WorldMapInfo/Fp/FpText").GetComponent<UnityEngine.UI.Text>();
			fplabel.text = Sparkipelago.itemState[(long)ItemIds.FREEDOM_MEDAL].ToString(); // Edit to match server count
		}
		
		[HarmonyPatch(typeof(ShopaloShop), "SwitchPage")]
		private static class PagePatch {
			private static void Postfix(ShopaloShop __instance, int page) {
				if (page == 0) {
					__instance.Index = 4;
				}
				int i = 0;
				foreach (ShopItenDetails iten in __instance.MainPageItens) {
					if (Sparkipelago.itemState[(long)ItemIds.SHOP_MOVES+i] == 0) iten.gameObject.SetActive(false);
					i++;
					if (i == 4) break;
				}
				ShopItenDetails[] itenlist = new ShopItenDetails[26];
				foreach (ShopItenDetails iten in __instance.MoveItens) {
					itenlist[Locations.getItenIndex(iten)] = iten;
				}
				foreach (ShopItenDetails iten in __instance.SpecialItens) {
					itenlist[Locations.getItenIndex(iten)] = iten;
				}
				foreach (ShopItenDetails iten in __instance.JesterItens) {
					itenlist[Locations.getItenIndex(iten)] = iten;
				}
				foreach (ShopItenDetails iten in __instance.UpgradeItens) {
					itenlist[Locations.getItenIndex(iten)] = iten;
				}
				
				const long id = 16295300000 + 6000;
				for (i = 0; i < 26; i++) {
					ShopItenDetails iten = itenlist[i];
					if (Sparkipelago.currentSession.Locations.AllLocations.Contains(id+i)) {
						iten.Description = Sparkipelago.shopItems[i];
					}
				}
			}
		}
		
		[HarmonyPatch(typeof(DisableButtons), "Start")]
		private static class DisableButtonPatch {
			private static void Prefix(DisableButtons __instance) {
				__instance.ButtonsToDisable = new Button[0];
			}
		}
		
		[HarmonyPatch(typeof(ShopItenDetails), "SetText")]
		private static class ItenTextPatch {
			private static void Postfix(ShopItenDetails __instance) {
				const long id = 16295300000 + 6000;
				int i = Locations.getItenIndex(__instance);
				Text textComponent = __instance.gameObject.transform.Find("bitsamm").GetComponent<Text>();
				if (!Sparkipelago.currentSession.Locations.AllLocations.Contains(id+i)) return;
				if (Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(id+i)) {
					textComponent.color = new Color(0.5f, 0.5f, 0.5f);
				} else {
					textComponent.color = new Color(0.25f, 1.0f, 0.25f);
				}
			}
		}
	}
}
