using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Archipelago.MultiClient.Net.Models;

namespace Sparkipelago {
	class Items {
		public static void addBits(int num) {
			Objects_Interaction.BitAmmountLocal += num;
			Save.GetCurrentSave().Bits += num;
			ScoreManager.Bits += num;
			PlayerPrefs.SetInt("Bits" + SaveSlot.Slot, ScoreManager.Bits);
		}

		private static void addDpadPower(int pow) {
			Save.SaveFile currentSave = Save.GetCurrentSave();
			if (currentSave.DpadUP == 0) {
				currentSave.DpadUP = pow;
				return;
			}
			if (currentSave.DpadLeft == 0) {
				currentSave.DpadLeft = pow;
				return;
			}
			if (currentSave.DpadDown == 0) {
				currentSave.DpadDown = pow;
				return;
			}
			if (currentSave.DpadRight == 0) {
				currentSave.DpadRight = pow;
				return;
			}
		}

		public static bool isStageItem(ItemIds item) {
			switch (item) {
				case ItemIds.SCORE_CAPSULE:
				case ItemIds.HEALTH_CAPSULE:
				case ItemIds.ENERGY_CAPSULE:
				case ItemIds.ENERGY_BUBBLE:
				case ItemIds.NIGHTMARE_TRAP:
				case ItemIds.LASER_TRAP:
				case ItemIds.FLINT_TRAP:
					return true;
				default:
					return false;
			}
		}
		
		public static void handleItem(ItemIds item, bool catchup) {
			Save.SaveFile save = Save.Saves[Save.CurrentSaveSlot];
			
			GameObject player = GameObject.Find("Player_Fark");
			switch (item) {
				case ItemIds.FREEDOM_MEDAL:
					if (Sparkipelago.currentScene == "[WORLD MAP]") WorldMap.onMapLoad();
					APSavedata data = APSave.getAPSave();
					data.fpCount = Sparkipelago.itemState[ItemIds.FREEDOM_MEDAL];
					break;
				case ItemIds.SCORE_CAPSULE: GameObject.Instantiate(Sparkipelago.sCapsule, player.transform); break;
				case ItemIds.HEALTH_CAPSULE: GameObject.Instantiate(Sparkipelago.hCapsule, player.transform); break;
				case ItemIds.ENERGY_CAPSULE: GameObject.Instantiate(Sparkipelago.eCapsule, player.transform); break;
				case ItemIds.BIT_BUBBLE: if (!catchup) addBits(30); break; // 30 bits
				case ItemIds.ENERGY_BUBBLE: GameObject.Instantiate(Sparkipelago.eBubble, player.transform.position, Quaternion.identity); break;
				case ItemIds.NIGHTMARE_TRAP:
					if (Sparkipelago.playerRed != null) Sparkipelago.playerRed.SetActive(true);
					break;
				case ItemIds.LASER_TRAP:
					if (Sparkipelago.playerGray != null) Sparkipelago.playerGray.SetActive(true);
					break;
				case ItemIds.FLINT_TRAP:
					Sparkipelago.flintList.Add(GameObject.Instantiate(Sparkipelago.flint, player.transform.position, Quaternion.identity));
					break;
				case ItemIds.SPIN_CHARGE: save.Move00_SpinCharge = true; save.Move00_SpinCharge_Enabled = true; break;
				case ItemIds.DUAL_AIR_KICK: save.Move01_DualAirKick = true; save.Move01_DualAirKick_Enabled = true; break;
				case ItemIds.DUAL_AIR_SLASH: save.Move02_DualAirSlash = true; save.Move02_DualAirSlash_Enabled = true; break;
				case ItemIds.EXTRA_FINISHER: save.Move03_ExtraFinisher = true; save.Move03_ExtraFinisher_Enabled = true; break;
				case ItemIds.SKYWARD_SLASH: save.Move04_SkywardSlash = true; save.Move04_SkywardSlash_Enabled = true; break;
				case ItemIds.DOUBLE_DOWN_SPIN: save.Move05_DownSlashSpin = true; save.Move05_DownSlashSpin_Enabled = true; break;
				case ItemIds.ABRUPT_FINISHER: save.Move07_AbruptFinisher = true; save.Move07_AbruptFinisher_Enabled = true; break;
				case ItemIds.DUPLEX_SLASH: save.Move08_DuplexSlash = true; save.Move08_DuplexSlash_Enabled = true; break;
				case ItemIds.SPEED_BUFF: save.Special01_SpeedBuff = true; if (!catchup) addDpadPower(1); break;
				case ItemIds.HYPER_SURGE: save.Special02_Explosion = true; if (!catchup) addDpadPower(2); break;
				case ItemIds.ENERGY_DASH: save.Special03_SpeedBlastBoost = true; if (!catchup) addDpadPower(3); break;
				case ItemIds.OVERCHARGE: save.Special04_PowerBuff = true; if (!catchup) addDpadPower(4); break;
				case ItemIds.SNAP_PORTAL: save.Special05_Teleport = true; if (!catchup) addDpadPower(5); break;
				case ItemIds.RADAR_SCOUT: save.Special06_Scouter = true; if (!catchup) addDpadPower(6); break;
				case ItemIds.MULTISHOT_BLAST: save.Special07_BlastMachineGun = true; if (!catchup) addDpadPower(7); break;
				case ItemIds.HEAL: save.Special12_Heal = true; if (!catchup) addDpadPower(12); break;
				case ItemIds.CLOUD_SHOT: save.Special13_Flutter = true; if (!catchup) addDpadPower(13); break;
				case ItemIds.TEMP_SHIELD: save.Special14_Shield = true; if (!catchup) addDpadPower(14); break;
				case ItemIds.CHARGED_SHOT: save.ChargedBlast = true; break;
				case ItemIds.RAIL_BOOST: save.RailBoost = true; break;
				case ItemIds.REGEN_BREAKING: save.RegenBreak = true; break;
				case ItemIds.JESTER_SWIPE: save.JesterSwipe = true; save.JesterSwipeEnabled = true; break;
				case ItemIds.REAPER_JESTER: save.Power_Reaper = true; if (!catchup) addDpadPower(8); break;
				case ItemIds.FLOAT: save.Power_Float = true; if (!catchup) addDpadPower(9); break;
				case ItemIds.FARK: save.Power_Fark = true; if (!catchup) addDpadPower(10); break;
				case ItemIds.SFARX: save.Power_Sfarx = true; if (!catchup) addDpadPower(11); break;
				case ItemIds.OOB_CLIP: if (!catchup) save.StageJustUnlocked[155] = true; break;
				case ItemIds.AM_VILLAGE_COIN:
					if (SlotData.coinHunt == CoinHunt.REQUIRE_VANILLA && Sparkipelago.itemState[item] >= 10) Locations.sendLocationCheck(4, "COMPLETION");
					if (SlotData.coinHunt == CoinHunt.REQUIRE_ALL && Sparkipelago.itemState[item] >= 15) Locations.sendLocationCheck(4, "COMPLETION");
					break;
				case ItemIds.ARID_HOLE_COIN:
					if (SlotData.coinHunt == CoinHunt.REQUIRE_VANILLA && Sparkipelago.itemState[item] >= 10) Locations.sendLocationCheck(11, "COMPLETION");
					if (SlotData.coinHunt == CoinHunt.REQUIRE_ALL && Sparkipelago.itemState[item] >= 15) Locations.sendLocationCheck(11, "COMPLETION");
					break;
				case ItemIds.SQUABBLE_SPILLWAY_COIN:
					if (SlotData.coinHunt == CoinHunt.REQUIRE_VANILLA && Sparkipelago.itemState[item] >= 13) Locations.sendLocationCheck(21, "COMPLETION");
					if (SlotData.coinHunt == CoinHunt.REQUIRE_ALL && Sparkipelago.itemState[item] >= 15) Locations.sendLocationCheck(21, "COMPLETION");
					break;
				case ItemIds.DROPSHIP_DAYBREAK_COIN:
					if (SlotData.coinHunt == CoinHunt.REQUIRE_VANILLA && Sparkipelago.itemState[item] >= 15) Locations.sendLocationCheck(27, "COMPLETION");
					if (SlotData.coinHunt == CoinHunt.REQUIRE_ALL && Sparkipelago.itemState[item] >= 28) Locations.sendLocationCheck(27, "COMPLETION");
					break;
			}
		}
		
		[HarmonyPatch(typeof(Action00_Regular), "ManageDash")]
		private static class DashPatch {
			private static void Prefix(Action00_Regular __instance) {
				if (!Sparkipelago.hasItem(ItemIds.DASH)) __instance.dashc = false;
			}
		}
		
		[HarmonyPatch(typeof(Action05_Rail), "RailMovement")]
		private static class RailDashPatch {
			private static void Prefix(Action05_Rail __instance, ref bool ___Dashed) {
				if (!Sparkipelago.hasItem(ItemIds.DASH)) ___Dashed = true;
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "HomingManagement")]
		private static class HomingPatch {
			private static void Prefix(ActionManager ___Actions) {
				if (!Sparkipelago.hasItem(ItemIds.JESTER_DASH)) ___Actions.Action02Control.HomingAvailable = false;
			}
		}
		
	//	[HarmonyPatch(typeof(Action02_Homing), "FixedUpdate")]
		private static class ExhaustedMagdashPatch {
			private static void Prefix(Action02_Homing __instance) {
				// There's no easily accessible variable so this'll have to do
				if (!Sparkipelago.hasItem(ItemIds.DASH)) __instance.MagnetDashMultiplier = 0.2f;
				else __instance.MagnetDashMultiplier = 0.65f;
			}
		}
		
		[HarmonyPatch(typeof(Action08_SuperMoves), "FixedUpdate")]
		private static class SuperPatch {
			private static void Prefix(Action08_SuperMoves __instance) {
				if (!Sparkipelago.hasItem(ItemIds.CHARGED_JESTER_DASH) && __instance.AttackType == -2) {
					__instance.AttackType = 0;
					__instance.Actions.ChangeAction(0);
				}
				if (!Sparkipelago.hasItem(ItemIds.DOWN_DASH) && __instance.AttackType == -1) {
					__instance.AttackType = 0;
					__instance.Actions.ChangeAction(0);
				}
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "ManageWallJump")]
		private static class WallJumpPatch {
			private static void Prefix(Action01_Jump __instance, ActionManager ___Actions) {
				if (!Sparkipelago.hasItem(ItemIds.WALL_JUMP)) __instance.OnWall = false;
				if (!Sparkipelago.hasItem(ItemIds.WALL_WALK)) ___Actions.Action09.Already = true;
			}
		}
		
		[HarmonyPatch(typeof(Action01_Jump), "DoDoubleJump")]
		private static class DoubleJumpPatch {
			private static void Prefix(Action01_Jump __instance) {
				if (!Sparkipelago.hasItem(ItemIds.DOUBLE_JUMP)) __instance.DoubleJumpAvailable = false;
				if (!Sparkipelago.hasItem(ItemIds.WALL_JUMP)) __instance.OnWall = false;
			}
		}
		
		[HarmonyPatch(typeof(Action00_Regular), "FixedUpdate")]
		private static class DoubleJumpPatch2 {
			private static void Prefix(Action00_Regular __instance) {
				if (!Sparkipelago.hasItem(ItemIds.DOUBLE_JUMP)) __instance.DoubleJumpEnabledAction0 = false;
			}
		}
		
		[HarmonyPatch(typeof(Action00_Regular), "ManageAttack")]
		private static class CombatPatch {
			private static bool Prefix() {
				if (!Sparkipelago.hasItem(ItemIds.COMBAT)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(Action_12_Block), "BlockInput")]
		private static class ParryPatch {
			private static void Prefix(Action_12_Block __instance) {
				if (!Sparkipelago.hasItem(ItemIds.PARRY)) __instance.BlockCounter = -1;
			}
		}
		
		[HarmonyPatch(typeof(StratoMech), "CheckForAttackAction")]
		private static class MechCombatPatch {
			private static bool Prefix() {
				if (!Sparkipelago.hasItem(ItemIds.COMBAT)) return false;
				return true;
			}
		}
		
		[HarmonyPatch(typeof(IntroStageSetup), "FixedUpdate")]
		private static class IntroCarPatch {
			private static void Prefix(IntroStageSetup __instance, ref bool ___GetInCar) {
				if (!Sparkipelago.hasItem(ItemIds.CAR)) ___GetInCar = true;
				__instance.GetInScript.IsAbleToExit = true;
			}
		}

		[HarmonyPatch(typeof(GetInCar), "GetIn")]
		private static class CarPatch {
			private static bool Prefix() {
				if (!Sparkipelago.hasItem(ItemIds.CAR)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(PlayableCopter), "GetIn")]
		private static class CopterPatch {
			private static bool Prefix() {
				if (!Sparkipelago.hasItem(ItemIds.COPTER)) return false;
				return true;
			} 
		}

		[HarmonyPatch(typeof(PlayerHealthAndStats), "ComboManager")]
		private static class CombatComboPatch {
			private static void Prefix() {
				if (Sparkipelago.hasItem(ItemIds.PERFECT_COMBO)) PlayerHealthAndStats.Combo = 1f;
			}
		}

		[HarmonyPatch(typeof(Objects_Interaction), "OnTriggerEnter")]
		private static class ObjectEnterPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "Spring" && !Sparkipelago.hasItem(ItemIds.SPRINGS)) return false;
				if (col.tag == "SpeedPad") {
					// There are multiple gimmicks that use this component
					SpeedPadData pad = col.GetComponent<SpeedPadData>();
					if (pad.isDashRing) {
						if (col.GetComponentInChildren<HomingInfo>(true) != null) {
							if (!Sparkipelago.hasItem(ItemIds.JESTER_DASH_RINGS)) return false;
						} else if (!Sparkipelago.hasItem(ItemIds.DASH_RINGS)) return false;
					} else if (!Sparkipelago.hasItem(ItemIds.SPEED_PADS)) return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Objects_Interaction), "OnTriggerStay")]
		private static class ObjectStayPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "Spring" && !Sparkipelago.hasItem(ItemIds.SPRINGS)) return false;
				return true;
			}
		}

		// Base Game Bug: The game will attempt homing attack even after going past the target
		[HarmonyPatch(typeof(Action02_Homing), "FixedUpdate")]
		private static class ObjectHomingPatch {
			private static void Prefix(Vector3 ___direction, float ___Timer, ref Vector3 __state) {
				if (___Timer == 0) __state = new Vector3(0, 0, 0);
				else __state = ___direction;
			}
			private static void Postfix(Action02_Homing __instance, Vector3 ___direction, Vector3 __state, PlayerBhysics ___Player) {
				if (Vector3.Dot(__state, ___direction.normalized) < 0) {
					___Player.rigid.velocity = new Vector3(0, 0, 0);
					__instance.Actions.ChangeAction(1);
				}
			}
		}
		
		[HarmonyPatch(typeof(JetPulley), "OnTriggerEnter")]
		private static class PrisonRocketPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "Player" && !Sparkipelago.hasItem(ItemIds.PRISON_ROCKETS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(BouncyProtesters), "OnTriggerEnter")]
		private static class ProtestorPatch1 {
			private static bool Prefix(Collider col) {
				if (col.tag == "Player" && !Sparkipelago.hasItem(ItemIds.PROTESTORS)) return false;
				return true;
			}
		}
		[HarmonyPatch(typeof(BouncyProtesters), "OnTriggerStay")]
		private static class ProtestorPatch2 {
			private static bool Prefix(Collider col) {
				if (col.tag == "Player" && !Sparkipelago.hasItem(ItemIds.PROTESTORS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(AbyssBracer), "OnTriggerEnter")]
		private static class AbyssBracerPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "Player" && !Sparkipelago.hasItem(ItemIds.ABYSS_BRACERS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(SpeedUpGate), "OnTriggerStay")]
		private static class AbyssBoosterPatch {
			private static bool Prefix(SpeedUpGate __instance, Collider col) {
				if (col.tag == "Player") {
					if (__instance.GetComponentInParent<BoostPadData>() != null) {
						if (!Sparkipelago.hasItem(ItemIds.CAR_BOOST_PADS)) return false;
					} else if (!Sparkipelago.hasItem(ItemIds.ABYSS_BOOSTERS)) return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Pulley), "OnTriggerEnter")]
		private static class PulleyPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "Player" && !Sparkipelago.hasItem(ItemIds.PULLEYS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(SpeedThreadmill), "OnCollisionStay")]
		private static class RampPatch1 {
			private static bool Prefix(Collider col) {
				if (!Sparkipelago.hasItem(ItemIds.RAMPS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(SpeedThreadmill), "OnTriggerStay")]
		private static class RampPatch2 {
			private static bool Prefix(Collider col) {
				if (!Sparkipelago.hasItem(ItemIds.RAMPS)) return false;
				return true;
			}
		}

		[HarmonyPatch(typeof(CarCollectables), "OnTriggerEnter")]
		private static class BoostPadPatch {
			private static bool Prefix(Collider col) {
				if (col.tag == "BoostPad" && !Sparkipelago.hasItem(ItemIds.CAR_BOOST_PADS)) return false;
				return true;
			}
		}

	//	SPRINGS -- Jester Dash sends you flying
	//	JESTER_DASH_RINGS -- Ditto to springs
	//	RAMPS -- Crashes the game
	}
}
