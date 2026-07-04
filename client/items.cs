using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Archipelago.MultiClient.Net.Models;

namespace Sparkipelago {
	public enum DpadDir {
		None,
		Up,
		Left,
		Down,
		Right
	}
	public enum DpadPowers {
		None,
		SpeedBuff,
		HyperSurge,
		EnergyDash,
		Overcharge,
		SnapPortal,
		RadarScout,
		MultishotBlast,
		ReaperJester,
		Float,
		Fark,
		Sfarx,
		Heal,
		CloudShot,
		TempShield,
		End
	}
	
	class Items {
		public static void addBits(int num) {
			Objects_Interaction.BitAmmountLocal += num;
			Save.GetCurrentSave().Bits += num;
			ScoreManager.Bits += num;
			PlayerPrefs.SetInt("Bits" + SaveSlot.Slot, ScoreManager.Bits);
		}

		public static void addDpadPower(DpadPowers pow) {
			addDpadPower(pow, DpadDir.None);
		}
		
		public static void addDpadPower(DpadPowers pow, DpadDir dir) {
			Save.SaveFile currentSave = Save.GetCurrentSave();
			int powint = (int)pow;
			if (dir == DpadDir.None) {
				if (currentSave.DpadRight == 0) dir = DpadDir.Right;
				if (currentSave.DpadDown == 0) dir = DpadDir.Down;
				if (currentSave.DpadLeft == 0) dir = DpadDir.Left;
				if (currentSave.DpadUP == 0) dir = DpadDir.Up;
			}
			if (dir == DpadDir.Up) {
				currentSave.DpadUP = powint;
				Action_13_NewSuperMoves.UpPower = powint;
			}
			if (dir == DpadDir.Left) {
				currentSave.DpadLeft = powint;
				Action_13_NewSuperMoves.LeftPower = powint;
			}
			if (dir == DpadDir.Down) {
				currentSave.DpadDown = powint;
				Action_13_NewSuperMoves.DownPower = powint;
			}
			if (dir == DpadDir.Right) {
				currentSave.DpadRight = powint;
				Action_13_NewSuperMoves.RightPower = powint;
			}
			if (Sparkipelago.player) Sparkipelago.player.GetComponent<Action_13_NewSuperMoves>().SetIcons();
		}

		public static DpadPowers getDpadPower(DpadDir dir) {
			Save.SaveFile currentSave = Save.GetCurrentSave();
			if (dir == DpadDir.Up) return (DpadPowers)currentSave.DpadUP;
			if (dir == DpadDir.Left) return (DpadPowers)currentSave.DpadLeft;
			if (dir == DpadDir.Down) return (DpadPowers)currentSave.DpadDown;
			if (dir == DpadDir.Right) return (DpadPowers)currentSave.DpadRight;
			return DpadPowers.None;
		}

		public static bool hasDpadPower(DpadPowers pow) {
			switch (pow) {
				case DpadPowers.SpeedBuff: return Sparkipelago.hasItem(ItemIds.SPEED_BUFF);
				case DpadPowers.HyperSurge: return Sparkipelago.hasItem(ItemIds.HYPER_SURGE);
				case DpadPowers.EnergyDash: return Sparkipelago.hasItem(ItemIds.ENERGY_DASH);
				case DpadPowers.Overcharge: return Sparkipelago.hasItem(ItemIds.OVERCHARGE);
				case DpadPowers.SnapPortal: return Sparkipelago.hasItem(ItemIds.SNAP_PORTAL);
				case DpadPowers.RadarScout: return Sparkipelago.hasItem(ItemIds.RADAR_SCOUT);
				case DpadPowers.MultishotBlast: return Sparkipelago.hasItem(ItemIds.MULTISHOT_BLAST);
				case DpadPowers.Heal: return Sparkipelago.hasItem(ItemIds.HEAL);
				case DpadPowers.CloudShot: return Sparkipelago.hasItem(ItemIds.CLOUD_SHOT);
				case DpadPowers.TempShield: return Sparkipelago.hasItem(ItemIds.TEMP_SHIELD);
				case DpadPowers.ReaperJester: return Sparkipelago.hasItem(ItemIds.REAPER_JESTER);
				case DpadPowers.Float: return Sparkipelago.hasItem(ItemIds.FLOAT);
				case DpadPowers.Fark: return Sparkipelago.hasItem(ItemIds.FARK);
				case DpadPowers.Sfarx: return Sparkipelago.hasItem(ItemIds.SFARX);
			}
			return true;
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
				case ItemIds.SPRING_TRAP:
				case ItemIds.GRAVITY_TRAP:
				case ItemIds.ZOOM_TRAP:
				case ItemIds.BALD_TRAP:
				case ItemIds.DAMAGE_TRAP:
					return true;
				default:
					return false;
			}
		}
		
		public static void makePlayerBald() {
			if (ButtonTips.Playing != PlayableUnit.Character) return;
			int ch = CharacterAnimatorChange.Character;
			switch (ch) {
				case 0: { // Spark
					GameObject.Find("FarkSkin/SparkModel/Crown").SetActive(false);
					GameObject.Find("FarkSkin/SparkModel/JesterHat").SetActive(false);
					GameObject.Find("FarkSkin/SparkModel/Ponpon").SetActive(false);
					break;
				}
				case 1: { // Reaper
					GameObject.Find("FarkSkin/ReaperJesterModel/Crown").SetActive(false);
					GameObject.Find("FarkSkin/ReaperJesterModel/JesterHat").SetActive(false);
					GameObject.Find("FarkSkin/ReaperJesterModel/Ponpon").SetActive(false);
					break;
				}
				case 2: { // Float
					GameObject.Find("FarkSkin/FloatPlayableBody/metarig/Root/hips/spine/chest/neck/Hat").transform.localScale = Vector3.zero;
					break;
				}
				case 3: { // Fark
					GameObject.Find("FarkSkin/FarkSmallModel/metarig/Root/hips/spine/chest/neck/Head/shoulder_L_002").transform.localScale = Vector3.one*0.001f;
					GameObject.Find("FarkSkin/FarkSmallModel/metarig/Root/hips/spine/chest/neck/Head/shoulder_R_002").transform.localScale = Vector3.one*0.001f;
					break;
				}
				case 4: { // Sfarx
					GameObject.Find("FarkSkin/SfarxModel/SfarxJesterHat").SetActive(false);
					break;
				}
			}
		}

		public static void makePlayerUnbald() {
			if (ButtonTips.Playing != PlayableUnit.Character) return;
			int ch = CharacterAnimatorChange.Character;
			switch (ch) {
				case 0: { // Spark
					GameObject.Find("FarkSkin/SparkModel/Crown").SetActive(true);
					GameObject.Find("FarkSkin/SparkModel/JesterHat").SetActive(true);
					GameObject.Find("FarkSkin/SparkModel/Ponpon").SetActive(true);
					break;
				}
				case 1: { // Reaper
					GameObject.Find("FarkSkin/ReaperJesterModel/Crown").SetActive(true);
					GameObject.Find("FarkSkin/ReaperJesterModel/JesterHat").SetActive(true);
					GameObject.Find("FarkSkin/ReaperJesterModel/Ponpon").SetActive(true);
					break;
				}
				case 2: { // Float
					GameObject.Find("FarkSkin/FloatPlayableBody/metarig/Root/hips/spine/chest/neck/Hat").transform.localScale = Vector3.one;
					break;
				}
				case 3: { // Fark
					GameObject.Find("FarkSkin/FarkSmallModel/metarig/Root/hips/spine/chest/neck/Head/shoulder_L_002").transform.localScale = Vector3.one;
					GameObject.Find("FarkSkin/FarkSmallModel/metarig/Root/hips/spine/chest/neck/Head/shoulder_R_002").transform.localScale = Vector3.one;
					break;
				}
				case 4: { // Sfarx
					GameObject.Find("FarkSkin/SfarxModel/SfarxJesterHat").SetActive(true);
					break;
				}
			}
		}
		
		public static void handleItem(ItemIds item, bool catchup) {
			Save.SaveFile save = Save.Saves[Save.CurrentSaveSlot];
			
			GameObject player = Sparkipelago.player;
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
				case ItemIds.SPRING_TRAP: {
					Vector3 speedDir = Sparkipelago.player.GetComponent<PlayerBhysics>().SpeedDir * 0.01f;
					if (speedDir == Vector3.zero) speedDir = Vector3.down;
					GameObject spring = GameObject.Instantiate(Sparkipelago.spring, player.transform.position + speedDir, Quaternion.identity);
					spring.transform.LookAt(Sparkipelago.player.transform);
					spring.transform.localScale = Vector3.one*2;
					break;
				}
				case ItemIds.GRAVITY_TRAP:
					Sparkipelago.gravityTimer = 15;
					break;
				case ItemIds.ZOOM_TRAP:
					if (Sparkipelago.player) {
						HedgeCamera hc = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera").GetComponent<HedgeCamera>();
						hc.CameraMaxDistance = 2;
						hc.CameraCombatMaxDistance = -2;
					}
					break;
				case ItemIds.BALD_TRAP:
					Sparkipelago.baldTimer = 30;
					break;
				case ItemIds.DAMAGE_TRAP:
					PlayerHealthAndStats.PlayerHP -= 1;
					Sparkipelago.player.GetComponent<HurtControl>().CheckForDeathAndKill();
					break;
				case ItemIds.SPIN_CHARGE: save.Move00_SpinCharge = true; save.Move00_SpinCharge_Enabled = true; break;
				case ItemIds.DUAL_AIR_KICK: save.Move01_DualAirKick = true; save.Move01_DualAirKick_Enabled = true; break;
				case ItemIds.DUAL_AIR_SLASH: save.Move02_DualAirSlash = true; save.Move02_DualAirSlash_Enabled = true; break;
				case ItemIds.EXTRA_FINISHER: save.Move03_ExtraFinisher = true; save.Move03_ExtraFinisher_Enabled = true; break;
				case ItemIds.SKYWARD_SLASH: save.Move04_SkywardSlash = true; save.Move04_SkywardSlash_Enabled = true; break;
				case ItemIds.DOUBLE_DOWN_SPIN: save.Move05_DownSlashSpin = true; save.Move05_DownSlashSpin_Enabled = true; break;
				case ItemIds.ABRUPT_FINISHER: save.Move07_AbruptFinisher = true; save.Move07_AbruptFinisher_Enabled = true; break;
				case ItemIds.DUPLEX_SLASH: save.Move08_DuplexSlash = true; save.Move08_DuplexSlash_Enabled = true; break;
				case ItemIds.SPEED_BUFF: save.Special01_SpeedBuff = true; if (!catchup) addDpadPower(DpadPowers.SpeedBuff); break;
				case ItemIds.HYPER_SURGE: save.Special02_Explosion = true; if (!catchup) addDpadPower(DpadPowers.HyperSurge); break;
				case ItemIds.ENERGY_DASH: save.Special03_SpeedBlastBoost = true; if (!catchup) addDpadPower(DpadPowers.EnergyDash); break;
				case ItemIds.OVERCHARGE: save.Special04_PowerBuff = true; if (!catchup) addDpadPower(DpadPowers.Overcharge); break;
				case ItemIds.SNAP_PORTAL: save.Special05_Teleport = true; if (!catchup) addDpadPower(DpadPowers.SnapPortal); break;
				case ItemIds.RADAR_SCOUT: save.Special06_Scouter = true; if (!catchup) addDpadPower(DpadPowers.RadarScout); break;
				case ItemIds.MULTISHOT_BLAST: save.Special07_BlastMachineGun = true; if (!catchup) addDpadPower(DpadPowers.MultishotBlast); break;
				case ItemIds.HEAL: save.Special12_Heal = true; if (!catchup) addDpadPower(DpadPowers.Heal); break;
				case ItemIds.CLOUD_SHOT: save.Special13_Flutter = true; if (!catchup) addDpadPower(DpadPowers.CloudShot); break;
				case ItemIds.TEMP_SHIELD: save.Special14_Shield = true; if (!catchup) addDpadPower(DpadPowers.TempShield); break;
				case ItemIds.CHARGED_SHOT: save.ChargedBlast = true; break;
				case ItemIds.RAIL_BOOST: save.RailBoost = true; break;
				case ItemIds.REGEN_BREAKING: save.RegenBreak = true; break;
				case ItemIds.JESTER_SWIPE: save.JesterSwipe = true; save.JesterSwipeEnabled = true; break;
				case ItemIds.REAPER_JESTER: save.Power_Reaper = true; if (!catchup) addDpadPower(DpadPowers.ReaperJester); break;
				case ItemIds.FLOAT: save.Power_Float = true; if (!catchup) addDpadPower(DpadPowers.Float); break;
				case ItemIds.FARK: save.Power_Fark = true; if (!catchup) addDpadPower(DpadPowers.Fark); break;
				case ItemIds.SFARX: save.Power_Sfarx = true; if (!catchup) addDpadPower(DpadPowers.Sfarx); break;
				case ItemIds.OUT_OF_BOUNDS: if (!catchup) save.StageJustUnlocked[155] = true; break;
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
			private static void Postfix(Action00_Regular __instance) {
				if (!Sparkipelago.hasItem(ItemIds.DASH)) __instance.DashAvailable = false;
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
		
		[HarmonyPatch(typeof(Action01_Jump), "FixedUpdate")]
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
			private static void Postfix() {
				double combo = APSave.file.client.comboAmt * (Sparkipelago.itemState[ItemIds.COMBAT]-1);
				if (combo > APSave.file.client.comboMax) combo = APSave.file.client.comboMax;
				if (PlayerHealthAndStats.Combo < (float)combo) PlayerHealthAndStats.Combo = (float)combo;
			}
		}

		[HarmonyPatch(typeof(StageTimer), "Update")]
		private static class TimestopPatch {
			private static void Prefix(StageTimer __instance) {
				if (Time.timeScale != 0f && !LevelProgressControl.LevelOver && __instance.deltaTime >= __instance.TimeToStartGame) {
					double time = 1;
					for (int i = 0; i < Sparkipelago.itemState[ItemIds.PROGRESSIVE_TIME_STOP]; i++) {
						time *= APSave.file.client.timeAmt;
					}
					if (time < APSave.file.client.timeMax) time = APSave.file.client.timeMax;
					StageTimer.time -= (1-(float)time) * Time.unscaledDeltaTime;
				}
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
	}
}
