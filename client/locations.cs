using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Rewired;
using System.Linq;

namespace Sparkipelago {
	class Locations {
		static string[] MEDALNAMES = {"CYAN", "GREEN", "YELLOW", "RED", "MAGENTA", "PURPLE", "BLUE", "GREY", "WHITE", "BROWN"};
		
		[HarmonyPatch(typeof(ShopItenDetails), "PurchaseIten")]
		private static class PurchasePatch {
			private static bool Prefix(ShopItenDetails __instance, ShopaloShop shop) {
				// Y button to check
				ItemIds page = ItemIds.SHOP_MOVES;
				if (shop.MoveItens.Contains(__instance)) page = ItemIds.SHOP_MOVES;
				if (shop.SpecialItens.Contains(__instance)) page = ItemIds.SHOP_POWERS;
				if (shop.UpgradeItens.Contains(__instance)) page = ItemIds.SHOP_UPGRADES;
				if (shop.JesterItens.Contains(__instance)) page = ItemIds.SHOP_CHARACTERS;
				
				const long id = 16295300000;
				int idx = getItenIndex(__instance);
				bool canCheck = Sparkipelago.currentSession.Locations.AllLocations.Contains(id+idx)
					&& Sparkipelago.currentSession.Locations.AllMissingLocations.Contains(id+idx)
					&& Sparkipelago.hasItem(page);
				bool isChecking = (shop.Rewinp.GetButton("AttackHeavy") || !__instance.Unlocked) && canCheck;
				Save.SaveFile s = Save.GetCurrentSave();
				if (isChecking && s.Bits >= __instance.BitsCost) {
					if (__instance.Unlocked) s.Bits -= __instance.BitsCost;
					
					Locations.sendLocationCheck(getItenIndex(__instance), "__shop");
				}

				if (!Sparkipelago.hasItem(page)) {
					shop.StartTextBox("You haven't unlocked this page yet. Sorry, I don't make the rules");
					return false;
				}
				
				if (!canCheck && !__instance.Unlocked) {
					shop.StartTextBox("Thought you could pull a fast on over on Archipelago eh? Gonna have to unlock it the hard way");
					return false;
				}
				
				return true;
			}
			
			private static void Postfix(ShopItenDetails __instance, ShopaloShop shop) {
				Save.SaveFile s = Save.GetCurrentSave();
				switch (__instance.Special) {
					case SpecialType.SpeedBuff_1: s.Special01_SpeedBuff = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.SPEED_BUFF); break;
					case SpecialType.Explosion_2: s.Special02_Explosion = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.HYPER_SURGE); break;
					case SpecialType.SpeedBlastBoost_3: s.Special03_SpeedBlastBoost = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.ENERGY_DASH); break;
					case SpecialType.PowerBuff_4: s.Special04_PowerBuff = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.OVERCHARGE); break;
					case SpecialType.Teleport_5: s.Special05_Teleport = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.SNAP_PORTAL); break;
					case SpecialType.Scouter_6: s.Special06_Scouter = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.RADAR_SCOUT); break;
					case SpecialType.BlastMachineGun_7: s.Special07_BlastMachineGun = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.MULTISHOT_BLAST); break;
					case SpecialType.ReaperJester: s.Power_Reaper = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.REAPER_JESTER); break;
					case SpecialType.Float: s.Power_Float = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.FLOAT); break;
					case SpecialType.Fark: s.Power_Fark = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.FARK); break;
					case SpecialType.Sfarx: s.Power_Sfarx = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.SFARX); break;
					case SpecialType.Heal_12: s.Special12_Heal = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.HEAL); break;
					case SpecialType.Flutter_13: s.Special13_Flutter = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.CLOUD_SHOT); break;
					case SpecialType.Shield_14: s.Special14_Shield = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.TEMP_SHIELD); break;
				}
				switch (__instance.Move) {
					case MovesType.SpinCharge_0: s.Move00_SpinCharge = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.SPIN_CHARGE); break;
					case MovesType.DualAirKick_1: s.Move01_DualAirKick = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.DUAL_AIR_KICK); break;
					case MovesType.DualAirSlash_2: s.Move02_DualAirSlash = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.DUAL_AIR_SLASH); break;
					case MovesType.ExtraFinisher_3: s.Move03_ExtraFinisher = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.EXTRA_FINISHER); break;
					case MovesType.SkywardSlash_4: s.Move04_SkywardSlash = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.SKYWARD_SLASH); break;
					case MovesType.DownSpinSlash_5: s.Move05_DownSlashSpin = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.DOUBLE_DOWN_SPIN); break;
					case MovesType.AbruptFinisher_7: s.Move07_AbruptFinisher = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.ABRUPT_FINISHER); break;
					case MovesType.DuplexSlash_8: s.Move08_DuplexSlash = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.DUPLEX_SLASH); break;
				}
				switch (__instance.Upgrade) {
					case UpgradeType.ChargedBlast: s.ChargedBlast = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.CHARGED_SHOT); break;
					case UpgradeType.RailBoost: s.RailBoost = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.RAIL_BOOST); break;
					case UpgradeType.RegenerativeBreaking: s.RegenBreak = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.REGEN_BREAKING); break;
					case UpgradeType.JesterSwipe: s.JesterSwipe = __instance.Unlocked = Sparkipelago.hasItem(ItemIds.JESTER_SWIPE); break;
				}
			}
		}

		public static int getItenIndex(ShopItenDetails __instance) {
			int idx = -1;
			switch (__instance.Special) {
				case SpecialType.SpeedBuff_1: idx = 8; break;
				case SpecialType.Explosion_2: idx = 9; break;
				case SpecialType.SpeedBlastBoost_3: idx = 10; break;
				case SpecialType.PowerBuff_4: idx = 11; break;
				case SpecialType.Teleport_5: idx = 12; break;
				case SpecialType.Scouter_6: idx = 13; break;
				case SpecialType.BlastMachineGun_7: idx = 14; break;
				case SpecialType.ReaperJester: idx = 22; break;
				case SpecialType.Float: idx = 23; break;
				case SpecialType.Fark: idx = 24; break;
				case SpecialType.Sfarx: idx = 25; break;
				case SpecialType.Heal_12: idx = 15; break;
				case SpecialType.Flutter_13: idx = 16; break;
				case SpecialType.Shield_14: idx = 17; break;
			}
			switch (__instance.Move) {
				case MovesType.SpinCharge_0: idx = 0; break;
				case MovesType.DualAirKick_1: idx = 1; break;
				case MovesType.DualAirSlash_2: idx = 2; break;
				case MovesType.ExtraFinisher_3: idx = 3; break;
				case MovesType.SkywardSlash_4: idx = 4; break;
				case MovesType.DownSpinSlash_5: idx = 5; break;
				case MovesType.AbruptFinisher_7: idx = 6; break;
				case MovesType.DuplexSlash_8: idx = 7; break;
			}
			switch (__instance.Upgrade) {
				case UpgradeType.ChargedBlast: idx = 18; break;
				case UpgradeType.RailBoost: idx = 19; break;
				case UpgradeType.RegenerativeBreaking: idx = 20; break;
				case UpgradeType.JesterSwipe: idx = 21; break;
			}
			return idx;
		}

		private static void sendLocationToServer(long location) {
			APSavedata data = APSave.getAPSave();
			if (Sparkipelago.currentSession == null || Sparkipelago.currentSession.Locations.AllMissingLocations.Contains(location)) {
				data.checkedLocations.Add(location);
				if (Sparkipelago.currentSession != null) {
					Sparkipelago.currentSession.Locations.CompleteLocationChecks(location);
					data.numLocations = Sparkipelago.currentSession.Locations.AllLocations.Count;
					data.numChecked = Sparkipelago.currentSession.Locations.AllLocationsChecked.Count;
				}
			}
		}

		public static bool hasLocationByIndex(int level, string sanity, int index) {
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.sanity == sanity && ch.index == index) {
							return Sparkipelago.currentSession.Locations.AllLocations.Contains(ch.id);
						}
					}
				}
			}
			return false;
		}

		public static bool hasLocation(int level, string check) {
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.name == check) {
							return Sparkipelago.currentSession.Locations.AllLocations.Contains(ch.id);
						}
					}
				}
			}
			return false;
		}

		public static long getLocationByIndex(int level, string sanity, int index) {
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.sanity == sanity && ch.index == index) return ch.id;
					}
				}
			}
			return -1;
		}
		
		public static long getLocation(int level, string check) {
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.name == check) return ch.id;
					}
				}
			}
			return -1;
		}
		
		public static bool isLocationCompleteByIndex(int level, string sanity, int index) {
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.sanity == sanity && ch.index == index) {
							return Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(ch.id);
						}
					}
				}
			}
			
			MelonLogger.Msg("Unable to find stage {0}", level);
			MelonLogger.Msg("Unable to find {0} sanity {1} index {2}", level, sanity, index);
			return false;
		}

		public static bool isLocationComplete(int level, string check) {
			if (check == "__shop") {
				return Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(APShared.shop[level].id);
			} else {
				foreach (APStageData stage in APShared.stages) {
					if (stage.id == level) {
						foreach (APStageCheck ch in stage.checks) {
							if (ch.name == check) {
								return Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(ch.id);

							}
						}
					}
				}
				
			//	MelonLogger.Msg("Unable to find stage {0}", level);
			//	MelonLogger.Msg("Unable to find {0} check {1}", level, check);
			}
			return false;
		}
		
		public static void sendLocationByIndex(int level, string sanity, int index) {
			bool foundStage = false;
			bool foundCheck = false;
			foreach (APStageData stage in APShared.stages) {
				if (stage.id == level) {
					foreach (APStageCheck ch in stage.checks) {
						if (ch.sanity == sanity && ch.index == index) {
							sendLocationToServer(ch.id);
							MelonLogger.Msg("Found Check! ID is {0}", ch.id);
							foundCheck = true;
							break;
						}
					}
					foundStage = true;
					break;
				}
			}
			
			if (!foundStage) MelonLogger.Msg("Unable to find stage {0}", level);
			if (!foundCheck) MelonLogger.Msg("Unable to find {0} sanity {1} index {2}", level, sanity, index);
		}
		
		public static void sendLocationCheck(int level, string check) {
			if (check == "__shop") {
				sendLocationToServer(APShared.shop[level].id);
			} else {
				bool foundStage = false;
				bool foundCheck = false;
				foreach (APStageData stage in APShared.stages) {
					if (stage.id == level) {
						foreach (APStageCheck ch in stage.checks) {
							if (ch.name == check) {
								sendLocationToServer(ch.id);
								foundCheck = true;
								break;
							}
						}
						foundStage = true;
						break;
					}
				}
				
				if (!foundStage) MelonLogger.Msg("Unable to find stage {0}", level);
				if (!foundCheck) MelonLogger.Msg("Unable to find {0} check {1}", level, check);
			}
		}
		
		public static void onLevelComplete(int idx) {
			MelonLogger.Msg("Send Location For Level " + idx.ToString());
			sendLocationCheck(idx, "COMPLETION");
			Save.SaveFile savefile = Save.Saves[Save.CurrentSaveSlot];
			if (savefile.SpeedGoldMedals[idx]) {
				MelonLogger.Msg("Send Gold Speed");
				sendLocationCheck(idx, "GOLD SPEED MEDAL");
			}
			if (savefile.SpeedDiaMedals[idx]) {
				MelonLogger.Msg("Send Diamond Speed");
				sendLocationCheck(idx, "DIAMOND SPEED MEDAL");
			}
			if (savefile.ScoreGoldMedals[idx]) {
				MelonLogger.Msg("Send Gold Score");
				sendLocationCheck(idx, "GOLD SCORE MEDAL");
			}
			if (savefile.ScoreDiaMedals[idx]) {
				MelonLogger.Msg("Send Diamond Score");
				sendLocationCheck(idx, "DIAMOND SCORE MEDAL");
			}
			int numExplore = Save.GetAmmountOfExploreMedalsInSaveFile(savefile, idx);
			long exploreHunt = (long)Sparkipelago.slotData["explore_hunt"];
			if (exploreHunt == 1 && numExplore == 10) sendLocationCheck(idx, "EXPLORE HUNT");
			if (Sparkipelago.itemState.ContainsKey(ItemIds.BASE_EXPLORE_MEDAL+idx))
				if (exploreHunt == 2 && Sparkipelago.itemState[ItemIds.BASE_EXPLORE_MEDAL+idx] >= 10) sendLocationCheck(idx, "EXPLORE HUNT");
		}
		
		[HarmonyPatch(typeof(WorldMedal), "SetExploreMedal")]
		private static class WorldMedalPatch {
			private static void Postfix(int medal) {
				MelonLogger.Msg("Sending " + Save.CurrentStageIndex.ToString() + " Medal " + medal.ToString());
				sendLocationCheck(Save.CurrentStageIndex, string.Format("{0} EXPLORATION MEDAL", MEDALNAMES[medal]));
			}
		}

		[HarmonyPatch(typeof(Arena), "Start")]
		private static class DiveShufflePatch {
			private static void Prefix(Arena __instance) {
				System.Random rng = new System.Random(Sparkipelago.musicSeed);
				for (int i = 0; i < 1000; i++) rng.Next();
				int[] idxShuffle = new int[__instance.FloorData.Length];
				ArenaSpawner[] newArena = new ArenaSpawner[__instance.FloorData.Length];
				for (int i = 0; i < __instance.FloorData.Length; i++) {
					idxShuffle[i] = -1;
				}

				for (int i = 0; i < __instance.FloorData.Length; i++) {
					int newIdx = -1;
					do {
						newIdx = rng.Next(__instance.FloorData.Length);
					} while (idxShuffle.Contains(newIdx));
					idxShuffle[i] = newIdx;
					newArena[newIdx] = __instance.FloorData[i];
				}
				__instance.FloorData = newArena;
			}
		}
		
		static int prevFloor;
		[HarmonyPatch(typeof(Arena), "Update")]
		private static class EndlessDivePatch {
			private static void Postfix(Arena __instance) {
				if (prevFloor != __instance.CurrentFloor) {
					sendLocationByIndex(155, "base", __instance.CurrentFloor / (int)(long)Sparkipelago.slotData["endless_floors"]);
					MelonLogger.Msg("Sending Check #{0}", __instance.CurrentFloor / (int)(long)Sparkipelago.slotData["endless_floors"]);
				}
				__instance.LivesMin = Sparkipelago.itemState[ItemIds.EXTRA_LIFE] + 2;
				prevFloor = __instance.CurrentFloor;
			}
		}
	}
}
