using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using MelonLoader;

namespace Sparkipelago {
	class WorldMap {
		[HarmonyPatch(typeof(Save), "GetCurrentFP")]
		private class FPCountPatch {
			private static bool Prefix(ref int __result) {
				__result = 100000;
				Save.CurrentFP = 100000;
				return false;
			}
		}

		[HarmonyPatch(typeof(UtopiaGate), "Start")]
		private class LivesPatch {
			private static void Postfix(UtopiaGate __instance, ref int ___Lives) {
				___Lives = Sparkipelago.itemState[ItemIds.EXTRA_LIFE] + 3;
				__instance.LivesText.text = "YOU HAVE A TOTAL OF [" + ___Lives + "]. LIVES\nComplete more checks to increase your lives.";
			}
		}
		
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

			for (int i = 0; i < savefile.StageUnlocked.Count(); i++) {
				savefile.StageUnlocked[i] = true;
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
			
			Save.SaveFile save = Save.GetCurrentSave();
			int[] freedomReqs = new int[10];
			int[] complReqs = new int[10];
			int i = 0;
			foreach (JToken freq in ((JArray)Sparkipelago.slotData["freedom_requirements"])) {
				freedomReqs[i] = (int)freq;
				i++;
			}
			i = 0;
			foreach (JToken creq in ((JArray)Sparkipelago.slotData["completion_requirements"])) {
				complReqs[i] = (int)creq;
				i++;
			}
			
			int numComplete = 0;
			int numExplore = 0;
			int exploreReq = (int)(long)Sparkipelago.slotData["explore_requirement"];
			for (i = 0; i < save.StageCompleted.Length; i++) {
				if (Locations.isLocationComplete(i, "COMPLETION")) save.StageCompleted[i] = true;
				if (save.StageCompleted[i]) numComplete++;
				if ((bool)Sparkipelago.slotData["utopia_hunt_medals"]) {
					if (Sparkipelago.itemState.ContainsKey(ItemIds.BASE_EXPLORE_MEDAL+i) && Sparkipelago.itemState[ItemIds.BASE_EXPLORE_MEDAL+i] >= 10) numExplore += 1;
				} else {
					if (Save.GetAmmountOfExploreMedalsInSaveFile(save, i) >= 10) numExplore += 1;
				}
			}
			
			i = 0;
			foreach (JToken boss in ((JArray)Sparkipelago.slotData["bosses"])) {
				bossids[i] = (int)((float)boss[0]);
				if (save.StageCompleted[bossids[i]]) numComplete--;
				if (bossids[i] > 200) save.StageCompleted[bossids[i]] = true;
				i++;
			}

			
			foreach(LevelData level in levels) {
				bool unlocked = false;
				if (Sparkipelago.hasItem(ItemIds.OOB_CLIP) && level.ID == 155) unlocked = true;
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
					if (placeStage(boss, level) && Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL] >= freedomReqs[i] && numComplete >= complReqs[i]) unlocked = true;
					i++;
				}
				
				i = 0;
				foreach (JToken gate in ((JArray)Sparkipelago.slotData["gates"])) {
					if (i > 0 && i < 5) {
						if (!save.StageCompleted[bossids[i-1]]) {i++; continue;}
					} else if (i == 5) {
						if (!(Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL] >= freedomReqs[4]
							&& numComplete >= complReqs[i]
							&& numExplore >= exploreReq
							&& save.Power_Fark
							&& save.Power_Sfarx
						)) {i++; continue;}
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
			fplabel.text = Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL].ToString(); // Edit to match server count
		}

		static ShopaloShop shop;
		
		[HarmonyPatch(typeof(ShopaloShop), "SwitchPage")]
		private static class PagePatch {
			private static void Postfix(ShopaloShop __instance, int page) {
				shop = __instance;
				if (page == 0) {
					__instance.Index = 4;
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
				
				const long id = 16295300000;
				for (int i = 0; i < 26; i++) {
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
		
		[HarmonyPatch(typeof(ShopItenDetails), "Start")]
		private static class ItenBitPatch {
			private static void Prefix(ShopItenDetails __instance) {
				if (__instance.BitsCost >= 1000) __instance.BitsCost /= 10;
			}
		}
		
		[HarmonyPatch(typeof(ShopItenDetails), "SetText")]
		private static class ItenTextPatch {
			private static void Postfix(ShopItenDetails __instance) {
				ItemIds page = ItemIds.SHOP_MOVES;
				if (shop.MoveItens.Contains(__instance)) page = ItemIds.SHOP_MOVES;
				if (shop.SpecialItens.Contains(__instance)) page = ItemIds.SHOP_POWERS;
				if (shop.UpgradeItens.Contains(__instance)) page = ItemIds.SHOP_UPGRADES;
				if (shop.JesterItens.Contains(__instance)) page = ItemIds.SHOP_CHARACTERS;
				
				long i = 16295300000 + Locations.getItenIndex(__instance);
				Text textComponent = __instance.gameObject.transform.Find("bitsamm").GetComponent<Text>();
				if (!Sparkipelago.currentSession.Locations.AllLocations.Contains(i)) return;
				if (!Sparkipelago.hasItem(page)) {
					textComponent.color = new Color(0.75f, 0.2f, 0.1f);
				} else if (Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(i)) {
					textComponent.color = new Color(0.5f, 0.5f, 0.5f);
				} else {
					textComponent.color = new Color(0.25f, 1.0f, 0.25f);
				}
			}
		}
	}
}
