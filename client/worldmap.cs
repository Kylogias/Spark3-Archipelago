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
		
		static bool placeStage(SlotData.Level lvlinfo, LevelData level) {
			if (lvlinfo.id == level.ID) {
				Vector3 newpos;
				newpos.x = lvlinfo.x;
				newpos.y = lvlinfo.y;
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
		
		public static void findLevelGate(string command) {
			string search = command.Substring(command.IndexOf(' ')+1);
			int stageID = -1;
			foreach (APStageData stage in APShared.stages) {
				if (stage.name == search) {
					stageID = stage.id;
					break;
				}
			}
			if (stageID == -1) {
				Sparkipelago.currentSession.Say(string.Format("Unable to find stage {0}", search));
				return;
			}
			int i = 0;
			foreach (SlotData.Level[] gate in SlotData.gates) {
				foreach (SlotData.Level level in gate) {
					if (level.id == stageID) {
						Sparkipelago.currentSession.Say(string.Format("Stage {0} is in Gate {1}", search, i));
						return;
					}
				}
				i += 1;
			}
			i = 1;
			foreach (SlotData.Level boss in SlotData.bosses) {
				if (boss.id == stageID) {
					Sparkipelago.currentSession.Say(string.Format("Boss {0} guards Gate {1}", search, i));
					return;
				}
				i += 1;
			}
		}
		
		public static void onMapLoad() {
			// Randomize Entrances
			LevelData[] levels = GameObject.Find("Map/Stages").GetComponentsInChildren<LevelData>(true);
			if (isGoalAvailable()) {
				Locations.sendLocationCheck(200, "Completion");
				if (SlotData.goal == GoalType.Reqs && Sparkipelago.hasItem(ItemIds.REQUIREMENTS_UNLOCKED)) Sparkipelago.currentSession.SetGoalAchieved();
			}
			
			switch (SlotData.progressionMode) {
				case ProgressionType.GATES:
					gateProgression(levels);
					break;
				case ProgressionType.VANILLA_ER:
					break;
				case ProgressionType.LEVEL:
					levelProgression(levels, false);
					break;
				case ProgressionType.OPEN:
					levelProgression(levels, true);
					break;
				default:
					foreach (LevelData level in levels) {
						level.gameObject.SetActive(false);
					}
					break;
			}

			int stageidx = Save.CurrentStageIndex;
			foreach (LevelData level in levels) {
				if (level.ID == stageidx || (Sparkipelago.levelsUnlocked == 1 && level.gameObject.activeSelf)) {
					GameObject reticule = GameObject.Find("Reticule");
					reticule.transform.position = level.gameObject.transform.position;
				}
			}

			UnityEngine.UI.Text fplabel = GameObject.Find("UI/WorldMapInfo/Fp/FpText").GetComponent<UnityEngine.UI.Text>();
			fplabel.text = Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL].ToString();
		}
		
		public static bool isGoalAvailable() {
			Save.SaveFile save = Save.GetCurrentSave();
			int numComplete = 0;
			int numExplore = 0;
			int numSpeed = 0;
			int numScore = 0;
			int i = 0;
			for (i = 0; i < save.StageCompleted.Length; i++) {
				if (Locations.isLocationComplete(i, "Completion")) save.StageCompleted[i] = true;
				if (save.StageCompleted[i]) numComplete++;
				if (save.SpeedGoldMedals[i] && (SlotData.speedType & MedalType.GOLD_FLAG) != 0) numSpeed += 1;
				if (save.SpeedDiaMedals[i] && (SlotData.speedType & MedalType.DIAMOND_FLAG) != 0) numSpeed += 1;
				if (save.ScoreGoldMedals[i] && (SlotData.scoreType & MedalType.GOLD_FLAG) != 0) numScore += 1;
				if (save.ScoreDiaMedals[i] && (SlotData.scoreType & MedalType.DIAMOND_FLAG) != 0) numScore += 1;
				if (SlotData.utopiaMedals) {
					if (Sparkipelago.itemState.ContainsKey(ItemIds.BASE_EXPLORE_MEDAL+i) && Sparkipelago.itemState[ItemIds.BASE_EXPLORE_MEDAL+i] >= 10) numExplore += 1;
				} else {
					if (Save.GetAmmountOfExploreMedalsInSaveFile(save, i) >= 10) numExplore += 1;
				}
			}
			
			i = 0;
			foreach (SlotData.Level boss in SlotData.bosses) {
				if (save.StageCompleted[boss.id]) numComplete--;
				if (save.SpeedGoldMedals[i] && (SlotData.speedType & MedalType.GOLD_FLAG) != 0) numSpeed -= 1;
				if (save.SpeedDiaMedals[i] && (SlotData.speedType & MedalType.DIAMOND_FLAG) != 0) numSpeed -= 1;
				if (boss.id > 200) save.StageCompleted[boss.id] = true;
				i++;
			}

			return Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL] >= SlotData.freedomReq[4]
				&& numComplete >= SlotData.completionReq[4]
				&& numExplore >= SlotData.exploreReq
				&& numSpeed >= SlotData.speedReq[4]
				&& numScore >= SlotData.scoreReq[4]
				&& ((save.Power_Fark && save.Power_Sfarx) || !SlotData.requireCharacters);
		}
		
		public static void gateProgression(LevelData[] levels) {
			levelProgression(levels, false);
			int[] bossids = {9, 24, 37, 38};
			bool[] bossUnlocked = {false, false, false, false};
			bool[] bossOpen = {false, false, false, false};
			
			Save.SaveFile save = Save.GetCurrentSave();
			
			int numComplete = 0;
			int numExplore = 0;
			int numSpeed = 0;
			int numScore = 0;
			int i = 0;
			for (i = 0; i < save.StageCompleted.Length; i++) {
				if (Locations.isLocationComplete(i, "Completion")) save.StageCompleted[i] = true;
				if (save.StageCompleted[i]) numComplete++;
				if (save.SpeedGoldMedals[i] && (SlotData.speedType & MedalType.GOLD_FLAG) != 0) numSpeed += 1;
				if (save.SpeedDiaMedals[i] && (SlotData.speedType & MedalType.DIAMOND_FLAG) != 0) numSpeed += 1;
				if (save.ScoreGoldMedals[i] && (SlotData.scoreType & MedalType.GOLD_FLAG) != 0) numScore += 1;
				if (save.ScoreDiaMedals[i] && (SlotData.scoreType & MedalType.DIAMOND_FLAG) != 0) numScore += 1;
				if (SlotData.utopiaMedals) {
					if (Sparkipelago.itemState.ContainsKey(ItemIds.BASE_EXPLORE_MEDAL+i) && Sparkipelago.itemState[ItemIds.BASE_EXPLORE_MEDAL+i] >= 10) numExplore += 1;
				} else {
					if (Save.GetAmmountOfExploreMedalsInSaveFile(save, i) >= 10) numExplore += 1;
				}
			}
			
			i = 0;
			foreach (SlotData.Level boss in SlotData.bosses) {
				bossids[i] = boss.id;
				bossOpen[i] = Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL] >= SlotData.freedomReq[i]
					&& numComplete >= SlotData.completionReq[i]
					&& numSpeed >= SlotData.speedReq[i]
					&& numScore >= SlotData.scoreReq[i];
				bossUnlocked[i] = bossOpen[i] && (save.StageCompleted[bossids[i]] || SlotData.optionalBosses);
				if (save.StageCompleted[bossids[i]]) numComplete--;
				if (save.SpeedGoldMedals[i] && (SlotData.speedType & MedalType.GOLD_FLAG) != 0) numSpeed -= 1;
				if (save.SpeedDiaMedals[i] && (SlotData.speedType & MedalType.DIAMOND_FLAG) != 0) numSpeed -= 1;
				if (bossids[i] > 200) save.StageCompleted[bossids[i]] = true;
				i++;
			}
			
			foreach(LevelData level in levels) {
				bool unlocked = false;
				bool force = false;
				if (Sparkipelago.hasItem(ItemIds.OUT_OF_BOUNDS) && level.ID == 155) unlocked = true;
				if (SlotData.labMode) {unlocked = true; force = true;}
				if (level.ID == -99) {
					Vector3 newpos;
					newpos.x = -2.5f;
					newpos.y = -0.5f;
					newpos.z = level.gameObject.transform.position.z;
					level.gameObject.transform.position = newpos;
					unlocked = true;
				}
				
				i = 0;
				foreach (SlotData.Level boss in SlotData.bosses) {
					if (placeStage(boss, level) && bossOpen[i]) unlocked = true;
					i++;
				}
				
				i = 0;
				foreach (SlotData.Level[] gate in SlotData.gates) {
					if (i > 0 && i < SlotData.gates.Count()-1) {
						if (!force && !bossUnlocked[i-1]) {i++; continue;}
					} else if (i == SlotData.gates.Count()-1) {
						if (!force && !isGoalAvailable()) {i++; continue;}
					}
					foreach (SlotData.Level lvlinfo in gate) {
						if (placeStage(lvlinfo, level)) unlocked = true;
					}
					i++;
				}
				
				if (unlocked) {
					level.gameObject.SetActive(true);
				}
			}
		}

		public static void levelProgression(LevelData[] levels, bool openWorld) {
			foreach (LevelData level in levels) {
				level.gameObject.SetActive(false);
				if (level.ID == -99) level.gameObject.SetActive(true);
				if (Sparkipelago.hasItem(ItemIds.OUT_OF_BOUNDS) && level.ID == 155) level.gameObject.SetActive(true);
				if (Sparkipelago.itemState.ContainsKey((ItemIds)(ItemIds.BASE_LEVEL_UNLOCK+level.ID))) {
					if (!isGoalAvailable()) {
						if ((ItemIds.BASE_LEVEL_UNLOCK+level.ID) == ItemIds.UTOPIA_SHELTER_UNLOCKED && SlotData.goal == GoalType.Utopia) continue;
						if ((ItemIds.BASE_LEVEL_UNLOCK+level.ID) == ItemIds.FREOM_MK0_UNLOCKED && SlotData.goal == GoalType.Freom) continue;
					}
					if ((ItemIds.BASE_LEVEL_UNLOCK+level.ID) == ItemIds.UTOPIA_SHELTER_UNLOCKED && !Sparkipelago.hasItem(ItemIds.UTOPIA_SHELTER_UNLOCKED)) continue;
					if ((ItemIds.BASE_LEVEL_UNLOCK+level.ID) == ItemIds.FREOM_MK0_UNLOCKED && !Sparkipelago.hasItem(ItemIds.FREOM_MK0_UNLOCKED)) continue;
					if (openWorld || Sparkipelago.hasItem((ItemIds)(ItemIds.BASE_LEVEL_UNLOCK+level.ID))) level.gameObject.SetActive(true);
				}
			}
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
					bool itenAvailable = Sparkipelago.currentSession.Locations.AllLocations.Contains(id+i);
					if (false) itenAvailable = itenAvailable && Sparkipelago.hasItem(Locations.getPageFromIdx(i));
					if (itenAvailable) {
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
				if (__instance.BitsCost >= APSave.file.client.shopMaxBit) __instance.BitsCost = (int)(__instance.BitsCost*(1-APSave.file.client.shopDiscount));
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

		// i hate you and i hope you die. screw you
		[HarmonyPatch(typeof(SpinyMachine), "Update")]
		private static class SpinyMachinePatch {
			private static bool Prefix() {
				return false;
			}
		}
	}
}
