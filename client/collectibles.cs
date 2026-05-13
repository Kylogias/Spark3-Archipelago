using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using HarmonyLib;

namespace Sparkipelago {
	class Collectibles {
		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		static List<CollectableCoin> coins;

		static int layers;
		
		static void recurseGameObject<T>(GameObject parent, List<T> comps, bool getLayer) where T : Component {
			ActivateOnDistance aod = parent.GetComponent<ActivateOnDistance>();
			if (parent.activeSelf || aod != null || parent.name == "Area_2 (Day)") {
				T comp = parent.GetComponent<T>();
				if (comp != null) {
					if (getLayer) layers |= 1 << parent.layer;
					comps.Add(comp);
				}
				for (int i = 0; i < parent.transform.childCount; i++) {
					recurseGameObject(parent.transform.GetChild(i).gameObject, comps, getLayer);
				}
			}
		}
		
		static List<T> getAllComponents<T>(Scene scn, bool getLayer) where T : Component {
			List<T> rotring = new List<T>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				recurseGameObject<T>(go, rotring, getLayer);
			}
			
			return rotring;
		}
		
		public static void onSceneLoad(string name) {
			Scene scn = SceneManager.GetSceneByName(name);

			layers = 0;
			capsules = getAllComponents<RotateRing>(scn, true);
			checkpoints = getAllComponents<CheckPointData>(scn, false);
			bubbles = getAllComponents<MonitorData>(scn, true);
			coins = getAllComponents<CollectableCoin>(scn, false); // There's an easier way but shrug
			
			// The normal index isn't initialized yet
			int stage = GameObject.Find("[OffStageVaribales]").GetComponent<GameProgressVariables>().StageIndex;
			int coinLeft = 0;
			for (int i = 0; i < coins.Count; i++) {
				if (Locations.isLocationCompleteByIndex(stage, "coin", i)) {
					Material mat = coins[i].transform.Find("Pivot/CoinMesh").gameObject.GetComponent<MeshRenderer>().material;
					mat.color = new Color(1f, 1f, 1f, 1f);
				} else coinLeft += 1;
			}
			if (Locations.isLocationComplete(stage, "COMPLETION") && coins.Count > 0) {
				CollectablesController collect = GameObject.Find("[ Collectable UI ]").GetComponent<CollectablesController>();
				collect.StageTime = 30000;
				collect.MedalAmm = coinLeft > 0 ? coinLeft : 100;
			} 
			
			GameObject player = GameObject.Find("Player_Fark");
			Action_13_NewSuperMoves nsm = player.GetComponent<Action_13_NewSuperMoves>();
			nsm.ObjRadarMask = layers;

			GameObject scouter = nsm.ScouterUIObject;
			GameObject radar = scouter.transform.Find("Radar").gameObject;
			GameObject dot = radar.transform.Find("Blue (1)").gameObject;
			RectTransform[] dots = new RectTransform[200 + nsm.CollectableDots.Length];
			for (int i = 0; i < dots.Length; i++) {
				if (i < nsm.CollectableDots.Length) {
					dots[i] = nsm.CollectableDots[i];
					continue;
				}
				GameObject newDot = GameObject.Instantiate(dot);
				newDot.transform.SetParent(radar.transform);
				newDot.transform.localScale = dot.transform.localScale;
				newDot.transform.localRotation = dot.transform.localRotation;
				dots[i] = (RectTransform)newDot.transform;
			}
			nsm.CollectableDots = dots;
			
			MelonLogger.Msg("{0} Capsules, {1} Checkpoints, {2} Bubbles", capsules.Count, checkpoints.Count, bubbles.Count);
		}

		[HarmonyPatch(typeof(Action_13_NewSuperMoves))]
		private static class RadarPatch {
			private static MethodBase TargetMethod() {
				return typeof(Action_13_NewSuperMoves).GetMethod(
					"RadarPosition",
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new Type[]{typeof(RectTransform[]), typeof(Collider[]), typeof(int)},
					new ParameterModifier[0]
				);
			}
			private static void Prefix(RectTransform[] pointslist, Collider[] colliders, int index) {
				Collider col = colliders[index];
				Image img = pointslist[index].gameObject.GetComponent<Image>();
			//	MelonLogger.Msg(col.gameObject.name);
				if (col.tag == "Monitor") img.color = new Color(1f, 0.9289f, 0f, 1f);
				else if (col.tag == "Ring" || col.tag == "ScoreCapsule" || col.tag == "EnergyCap") img.color = new Color(0.5f, 1f, 0.5f, 1f);
				else img.color = new Color(0f, 0f, 0f, 0f);
			}
		}
		
		[HarmonyPatch(typeof(Monitors_Interactions), "OnTriggerEnter")]
		private static class MonitorsPatch {
			private static void Prefix(Monitors_Interactions __instance, Collider col) {
				if (col.tag != "Monitor") return;
				
				string type = "Unknown";
				MonitorData mon = col.GetComponent<MonitorData>();
				if (!bubbles.Contains(mon)) return;
				if (mon.Type == MonitorType.Ring) type = "Bit";
				if (mon.Type == MonitorType.Energy) type = "Energy";
				Sparkipelago.debugLog("Collected Bubble #{0} ({1})", bubbles.IndexOf(mon), type);
				bubbles[bubbles.IndexOf(mon)] = null;
			}
		}
		
		[HarmonyPatch(typeof(Objects_Interaction), "BasicCollectables")]
		private static class CollectiblePatch {
			private static void Prefix(Objects_Interaction __instance, Collider col) {
				if (col.tag != "Ring" && col.tag != "ScoreCapsule" && col.tag != "EnergyCap") return;
				
				RotateRing rotring = col.GetComponent<RotateRing>();
				if (!capsules.Contains(rotring)) return;
				Sparkipelago.debugLog("Collected Capsule #{0} ({1})", capsules.IndexOf(rotring), col.tag);
				capsules[capsules.IndexOf(rotring)] = null;
			}
		}
		
		[HarmonyPatch(typeof(LevelProgressControl), "SetCheckPoint")]
		private static class CheckpointPatch {
			private static void Prefix(LevelProgressControl __instance, CheckPointData check) {
				if (!checkpoints.Contains(check)) return;
				Sparkipelago.debugLog("Collected Checkpoint #{0}", checkpoints.IndexOf(check));
				checkpoints[checkpoints.IndexOf(check)] = null;
			}
		}
		
		[HarmonyPatch(typeof(CollectableCoin), "OnTriggerEnter")]
		private static class CoinPatch {
			private static void Prefix(CollectableCoin __instance, Collider col) {
				if (col.tag == "Player" && coins.Contains(__instance)) {
					int idx = coins.IndexOf(__instance);
					Sparkipelago.debugLog("Collected Coin #{0}", idx);
					Locations.sendLocationByIndex(Save.CurrentStageIndex, "coin", idx);
					coins[idx] = null;
				}
			}
		}
	}
}