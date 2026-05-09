using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Sparkipelago {
	class Collectibles {
		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		static List<CollectableCoin> coins;

		static void recurseGameObject<T>(GameObject parent, List<T> comps) where T : Component {
			ActivateOnDistance aod = parent.GetComponent<ActivateOnDistance>();
			if (parent.activeSelf || aod != null) {
				T comp = parent.GetComponent<T>();
				if (comp != null) comps.Add(comp);
				for (int i = 0; i < parent.transform.childCount; i++) {
					recurseGameObject(parent.transform.GetChild(i).gameObject, comps);
				}
			}
		}
		
		static List<T> getAllComponents<T>(Scene scn) where T : Component {
			List<T> rotring = new List<T>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				recurseGameObject<T>(go, rotring);
			}
			
			return rotring;
		}
		
		public static void onSceneLoad(string name) {
			Scene scn = SceneManager.GetSceneByName(name);
			
			capsules = getAllComponents<RotateRing>(scn);
			checkpoints = getAllComponents<CheckPointData>(scn);
			bubbles = getAllComponents<MonitorData>(scn);
			coins = getAllComponents<CollectableCoin>(scn); // There's an easier way but shrug
			
			MelonLogger.Msg("{0} Capsules, {1} Checkpoints, {2} Bubbles", capsules.Count, checkpoints.Count, bubbles.Count);
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