using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;
using System.Collections.Generic;
using HarmonyLib;

namespace Sparkipelago {
	class Collectibles {
		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		
		public class GOComparer : IComparer<Component> {
			public int Compare(Component a, Component b) {
				Vector3 pA = a.gameObject.transform.position, pB = b.gameObject.transform.position;
				if (pA.z.CompareTo(pB.z) != 0) return pA.z.CompareTo(pB.z);
				if (pA.x.CompareTo(pB.x) != 0) return pA.x.CompareTo(pB.x);
				if (pA.y.CompareTo(pB.y) != 0) return pA.y.CompareTo(pB.y);
				return 0;
			}
		}
		
		static List<T> getAllComponents<T>() where T : Component {
			Scene scn = SceneManager.GetActiveScene();
			List<T> rotring = new List<T>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				T[] newring = go.GetComponentsInChildren<T>(true);
				foreach (T ring in newring) {
					rotring.Add(ring);
				}
			}
			
			GOComparer goc = new GOComparer();
			rotring.Sort(goc);
			return rotring;
		}
		
		public static void onSceneLoad() {
			capsules = getAllComponents<RotateRing>();
			checkpoints = getAllComponents<CheckPointData>();
			bubbles = getAllComponents<MonitorData>();
			
			MelonLogger.Msg("{0} Capsules, {1} Checkpoints, {2} Bubbles", capsules.Count, checkpoints.Count, bubbles.Count);
		}
		
		[HarmonyPatch(typeof(Monitors_Interactions), "OnTriggerEnter")]
		private static class MonitorsPatch {
			private static void Prefix(Monitors_Interactions __instance, Collider col) {
				if (col.tag != "Monitor") return;
				
				string type = "Unknown";
				MonitorData mon = col.GetComponent<MonitorData>();
				if (mon.Type == MonitorType.Ring) type = "Bit";
				if (mon.Type == MonitorType.Energy) type = "Energy";
				MelonLogger.Msg("Collected Bubble #{0} ({1})", bubbles.IndexOf(mon), type);
				bubbles[bubbles.IndexOf(mon)] = null;
			}
		}
		
		[HarmonyPatch(typeof(Objects_Interaction), "BasicCollectables")]
		private static class CollectiblePatch {
			private static void Prefix(Objects_Interaction __instance, Collider col) {
				if (col.tag != "Ring" && col.tag != "ScoreCapsule" && col.tag != "EnergyCap") return;
				
				RotateRing rotring = col.GetComponent<RotateRing>();
				MelonLogger.Msg("Collected Capsule #{0} ({1})", capsules.IndexOf(rotring), col.tag);
				capsules[capsules.IndexOf(rotring)] = null;
			}
		}
		
		[HarmonyPatch(typeof(LevelProgressControl), "SetCheckPoint")]
		private static class CheckpointPatch {
			private static void Prefix(LevelProgressControl __instance, CheckPointData check) {
				MelonLogger.Msg("Collected Checkpoint #{0}", checkpoints.IndexOf(check));
				checkpoints[checkpoints.IndexOf(check)] = null;
			}
		}
	}
}