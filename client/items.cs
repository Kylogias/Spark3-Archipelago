using UnityEngine;
using HarmonyLib;
using Archipelago.MultiClient.Net.Models;

namespace Sparkipelago {
	class Items {
		public static void addBits(int num) {
			Objects_Interaction.BitAmmountLocal += num;
			Save.GetCurrentSave().Bits += num;
			ScoreManager.Bits += num;
			PlayerPrefs.SetInt("Bits" + SaveSlot.Slot, ScoreManager.Bits);
		}
		
		public static void handleItem(ItemInfo item) {
			Save.SaveFile save = Save.Saves[Save.CurrentSaveSlot];
			
			switch ((ItemIds)(item.ItemId-(long)ItemIds.PREFIX)) {
				case ItemIds.FREEDOM_MEDAL:
					if (Sparkipelago.currentScene == "[WORLD MAP]") WorldMap.onMapLoad();
					break;
				case ItemIds.SCORE_CAPSULE: // 1000 Score
					ScoreManager.AddStageScore(1000, "Score-Capsule");
					ScoreManager.AddMultiplier(0.1f);
					break;
				case ItemIds.HEALTH_CAPSULE: // 1 health
					PlayerHealthAndStats.PlayerHP += 1;
					break;
				case ItemIds.ENERGY_CAPSULE: PlayerHealthAndStats.AddEnergy(10); break; // 10 energy
				case ItemIds.BIT_BUBBLE: addBits(30); break; // 30 bits
				case ItemIds.ENERGY_BUBBLE: PlayerHealthAndStats.AddEnergy(20); break; // 20 energy
				case ItemIds.NIGHTMARE_TRAP: break;
				case ItemIds.LASER_TRAP: break;
				case ItemIds.DUST_TRAP: break;
				
				case ItemIds.SPIN_CHARGE: save.Move00_SpinCharge = true; save.Move00_SpinCharge_Enabled = true; break;
				case ItemIds.DUAL_AIR_KICK: save.Move01_DualAirKick = true; save.Move01_DualAirKick_Enabled = true; break;
				case ItemIds.DUAL_AIR_SLASH: save.Move02_DualAirSlash = true; save.Move02_DualAirSlash_Enabled = true; break;
				case ItemIds.EXTRA_FINISHER: save.Move03_ExtraFinisher = true; save.Move03_ExtraFinisher_Enabled = true; break;
				case ItemIds.SKYWARD_SLASH: save.Move04_SkywardSlash = true; save.Move04_SkywardSlash_Enabled = true; break;
				case ItemIds.DOUBLE_DOWN_SPIN: save.Move05_DownSlashSpin = true; save.Move05_DownSlashSpin_Enabled = true; break;
				case ItemIds.ABRUPT_FINISHER: save.Move07_AbruptFinisher = true; save.Move07_AbruptFinisher_Enabled = true; break;
				case ItemIds.DUPLEX_SLASH: save.Move08_DuplexSlash = true; save.Move08_DuplexSlash_Enabled = true; break;
				case ItemIds.SPEED_BUFF: save.Special01_SpeedBuff = true; break;
				case ItemIds.HYPER_SURGE: save.Special02_Explosion = true; break;
				case ItemIds.ENERGY_DASH: save.Special03_SpeedBlastBoost = true; break;
				case ItemIds.OVERCHARGE: save.Special04_PowerBuff = true; break;
				case ItemIds.SNAP_PORTAL: save.Special05_Teleport = true; break;
				case ItemIds.RADAR_SCOUT: save.Special06_Scouter = true; break;
				case ItemIds.MULTISHOT_BLAST: save.Special07_BlastMachineGun = true; break;
				case ItemIds.HEAL: save.Special12_Heal = true; break;
				case ItemIds.CLOUD_SHOT: save.Special13_Flutter = true; break;
				case ItemIds.TEMP_SHIELD: save.Special14_Shield = true; break;
				case ItemIds.CHARGED_SHOT: save.ChargedBlast = true; break;
				case ItemIds.RAIL_BOOST: save.RailBoost = true; break;
				case ItemIds.REGEN_BREAKING: save.RegenBreak = true; break;
				case ItemIds.JESTER_SWIPE: save.JesterSwipe = true; save.JesterSwipeEnabled = true; break;
				case ItemIds.REAPER: save.Power_Reaper = true; save.Power_Reaper_Unlocked = true; save.Power_Reaper_Unlock_Notif = true; break;
				case ItemIds.FLOAT: save.Power_Float = true; save.Power_Float_Unlocked = true; save.Power_Float_Unlock_Notif = true; break;
				case ItemIds.FARK: save.Power_Fark = true; save.Power_Fark_Unlocked = true; save.Power_Fark_Unlock_Notif = true; break;
				case ItemIds.SFARX: save.Power_Sfarx = true; save.Power_Sfarx_Unlocked = true; save.Power_Sfarx_Unlock_Notif = true; break;
			}
		}
		
		[HarmonyPatch(typeof(Action00_Regular), "ManageDash")]
		private static class DashPatch {
			private static void Prefix(Action00_Regular __instance) {
				if (Sparkipelago.itemState[(long)ItemIds.DASH] == 0) __instance.dashc = false;
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "HomingManagement")]
		private static class HomingPatch {
			private static void Prefix(ActionManager ___Actions) {
				if (Sparkipelago.itemState[(long)ItemIds.JESTER_DASH] == 0) ___Actions.Action02Control.HomingAvailable = false;
			}
		}
		
		[HarmonyPatch(typeof(Action08_SuperMoves), "FixedUpdate")]
		private static class SuperPatch {
			private static void Prefix(Action08_SuperMoves __instance) {
				if (Sparkipelago.itemState[(long)ItemIds.CHARGED_DASH] == 0 && __instance.AttackType == -2) {
					__instance.AttackType = 0;
					__instance.Actions.ChangeAction(0);
				}
				if (Sparkipelago.itemState[(long)ItemIds.DOWN_DASH] == 0 && __instance.AttackType == -1) {
					__instance.AttackType = 0;
					__instance.Actions.ChangeAction(0);
				}
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "ManageWallJump")]
		private static class WallJumpPatch {
			private static bool Prefix() {
				if (Sparkipelago.itemState[(long)ItemIds.WALL_JUMP] == 0) return false;
				return true;
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "DoDoubleJump")]
		private static class DoubleJumpPatch {
			private static void Prefix(Action01_Jump __instance) {
				if (Sparkipelago.itemState[(long)ItemIds.DOUBLE_JUMP] == 0) __instance.DoubleJumpAvailable = false;
			}
		}
		
		[HarmonyPatch(typeof(Action00_Regular), "ManageAttack")]
		private static class CombatPatch {
			private static bool Prefix() {
				if (Sparkipelago.itemState[(long)ItemIds.COMBAT] == 0) return false;
				return true;
			}
		}
	}
}
