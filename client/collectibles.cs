using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Sparkipelago {
	class Collectibles {
		struct TrimObject {
			public string level;
			public string[] trees;
			public TrimObject(string l, string[] t) {
				level = l;
				trees = t;
			}
		}

		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		static List<CollectableCoin> coins;

		static TrimObject[] trims = {
			new TrimObject("[STAGE 01 - VILLA] [INTRO STAGE]", new string[]{"[STAGE VILLA]", "[Colletables Holder]"}) // Alpine Carrera
		};
		
		static List<T> getAllComponents<T>(Scene scn, List<GameObject> trimmed) where T : Component {
			List<T> rotring = new List<T>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				T[] newring = go.GetComponentsInChildren<T>(true);
				foreach (T ring in newring) {
					bool isTrimmed = false;
					foreach (GameObject trim in trimmed) {
						if (ring.transform.IsChildOf(trim.transform)) isTrimmed = true;
					}
					if (!isTrimmed) rotring.Add(ring);
				}
			}
			
			return rotring;
		}
		
		public static void onSceneLoad(string name) {
			Scene scn = SceneManager.GetSceneByName(name);
			List<GameObject> trimmed = new List<GameObject>();
			foreach (TrimObject trim in trims) {
				if (name == trim.level) {
					foreach (GameObject go in scn.GetRootGameObjects()) {
						if (trim.trees.Contains(go.name)) trimmed.Add(go);
					}
					break;
				}
			}
			
			capsules = getAllComponents<RotateRing>(scn, trimmed);
			checkpoints = getAllComponents<CheckPointData>(scn, trimmed);
			bubbles = getAllComponents<MonitorData>(scn, trimmed);
			coins = getAllComponents<CollectableCoin>(scn, trimmed); // There's an easier way but shrug
			
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