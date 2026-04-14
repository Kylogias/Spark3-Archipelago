using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Rewired;

namespace Sparkipelago {
	class Locations {
		[HarmonyPatch(typeof(WorldMedal), "SetExploreMedal")]
		private static class WorldMedalPatch {
			private static void Postfix(int medal) {
				MelonLogger.Msg("Sending " + Save.CurrentStageIndex.ToString() + " Medal " + medal.ToString());
				sendLocationCheck(Save.CurrentStageIndex, medal+5);
			}
		}
		
		
		[HarmonyPatch(typeof(ShopItenDetails), "PurchaseIten")]
		private static class PurchasePatch {
			private static bool Prefix(ShopItenDetails __instance, ShopaloShop shop) {
				// X button to check
				bool isChecking = shop.Rewinp.GetButton("AttackHeavy") || !__instance.Unlocked;
				Save.SaveFile s = Save.GetCurrentSave();
				if (isChecking && s.Bits >= __instance.BitsCost) {
					if (__instance.Unlocked) s.Bits -= __instance.BitsCost;
					
					Locations.sendLocationCheck(300, getItenIndex(__instance));
				}
				
				return true;
			}
			
			private static void Postfix(ShopItenDetails __instance, ShopaloShop shop) {
				Save.SaveFile s = Save.GetCurrentSave();
				switch (__instance.Special) {
					case SpecialType.SpeedBuff_1: s.Special01_SpeedBuff = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.SPEED_BUFF] != 0; break;
					case SpecialType.Explosion_2: s.Special02_Explosion = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.HYPER_SURGE] != 0; break;
					case SpecialType.SpeedBlastBoost_3: s.Special03_SpeedBlastBoost = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.ENERGY_DASH] != 0; break;
					case SpecialType.PowerBuff_4: s.Special04_PowerBuff = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.OVERCHARGE] != 0; break;
					case SpecialType.Teleport_5: s.Special05_Teleport = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.SNAP_PORTAL] != 0; break;
					case SpecialType.Scouter_6: s.Special06_Scouter = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.RADAR_SCOUT] != 0; break;
					case SpecialType.BlastMachineGun_7: s.Special07_BlastMachineGun = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.MULTISHOT_BLAST] != 0; break;
					case SpecialType.ReaperJester: s.Power_Reaper = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.REAPER] != 0; break;
					case SpecialType.Float: s.Power_Float = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.FLOAT] != 0; break;
					case SpecialType.Fark: s.Power_Fark = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.FARK] != 0; break;
					case SpecialType.Sfarx: s.Power_Sfarx = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.SFARX] != 0; break;
					case SpecialType.Heal_12: s.Special12_Heal = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.HEAL] != 0; break;
					case SpecialType.Flutter_13: s.Special13_Flutter = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.CLOUD_SHOT] != 0; break;
					case SpecialType.Shield_14: s.Special14_Shield = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.TEMP_SHIELD] != 0; break;
				}
				switch (__instance.Move) {
					case MovesType.SpinCharge_0: s.Move00_SpinCharge = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.SPIN_CHARGE] != 0; break;
					case MovesType.DualAirKick_1: s.Move01_DualAirKick = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.DUAL_AIR_KICK] != 0; break;
					case MovesType.DualAirSlash_2: s.Move02_DualAirSlash = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.DUAL_AIR_SLASH] != 0; break;
					case MovesType.ExtraFinisher_3: s.Move03_ExtraFinisher = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.EXTRA_FINISHER] != 0; break;
					case MovesType.SkywardSlash_4: s.Move04_SkywardSlash = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.SKYWARD_SLASH] != 0; break;
					case MovesType.DownSpinSlash_5: s.Move05_DownSlashSpin = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.DOUBLE_DOWN_SPIN] != 0; break;
					case MovesType.AbruptFinisher_7: s.Move07_AbruptFinisher = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.ABRUPT_FINISHER] != 0; break;
					case MovesType.DuplexSlash_8: s.Move08_DuplexSlash = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.DUPLEX_SLASH] != 0; break;
				}
				switch (__instance.Upgrade) {
					case UpgradeType.ChargedBlast: s.ChargedBlast = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.CHARGED_SHOT] != 0; break;
					case UpgradeType.RailBoost: s.RailBoost = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.RAIL_BOOST] != 0; break;
					case UpgradeType.RegenerativeBreaking: s.RegenBreak = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.REGEN_BREAKING] != 0; break;
					case UpgradeType.JesterSwipe: s.JesterSwipe = __instance.Unlocked = Sparkipelago.itemState[(long)ItemIds.JESTER_SWIPE] != 0; break;
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
		
		public static void sendLocationCheck(int level, int sub) {
			long id = 16295300000 + ((long)level * 20) + sub;
			Sparkipelago.currentSession.Locations.CompleteLocationChecks(id);
		}
		
		public static void onLevelComplete(int idx) {
			MelonLogger.Msg("Send Location For Level " + idx.ToString());
			sendLocationCheck(idx, 0);
			Save.SaveFile savefile = Save.Saves[Save.CurrentSaveSlot];
			if (savefile.SpeedGoldMedals[idx]) {
				MelonLogger.Msg("Send Gold Speed");
				sendLocationCheck(idx, 1);
			}
			if (savefile.SpeedDiaMedals[idx]) {
				MelonLogger.Msg("Send Diamond Speed");
				sendLocationCheck(idx, 2);
			}
			if (savefile.ScoreGoldMedals[idx]) {
				MelonLogger.Msg("Send Gold Score");
				sendLocationCheck(idx, 3);
			}
			if (savefile.ScoreDiaMedals[idx]) {
				MelonLogger.Msg("Send Diamond Score");
				sendLocationCheck(idx, 4);
			}
		}
	}
}
