using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using HarmonyLib;
using Archipelago.MultiClient.Net.Enums;

namespace Sparkipelago {
	class Collectibles {
		static string[] MEDALNAMES = {"CYAN", "GREEN", "YELLOW", "RED", "MAGENTA", "PURPLE", "BLUE", "GREY", "WHITE", "BROWN"};
		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		static List<CollectableCoin> coins;
		static List<GameObject> batteries;
		static List<WorldMedal> medals;

		static int layers;
		static int stage;
		static CollectibleScout scout;
		
		class CollectibleScout {
			public class ScoutData {
				public GameObject go;
				public ItemFlags flags;
				public bool collected;
				public bool enabled;
			};
			
			public Dictionary<long, ScoutData> locids;
			
			public CollectibleScout() {
				locids = new Dictionary<long, ScoutData>();
			}

			public void addLocation(GameObject go, string sanity, int index) {
				if (sanity == "_FAKE") return;
				if (sanity == "explore" && !APSave.file.client.exploreArrows) return;
				if (sanity == "coin" && !APSave.file.client.coinArrows) return;
				if (sanity == "capsule" && !APSave.file.client.capsuleArrows) return;
				if (sanity == "bubble" && !APSave.file.client.bubbleArrows) return;
				if (sanity == "battery" && !APSave.file.client.batteryArrows) return;
				long key = Locations.getLocationByIndex(stage, sanity, index);
				ScoutData sd = new ScoutData();
				sd.go = go;
				if (key != -1) locids.Add(key, sd);
			}
			
			public void addLocation(GameObject go, string name) {
				long key = Locations.getLocation(stage, name);
				ScoutData sd = new ScoutData();
				sd.go = go;
				if (key != -1) locids.Add(key, sd);
			}

			public void sendScout() {
				List<long> keys = new List<long>();
				foreach (long key in locids.Keys) {
					keys.Add(key);
				}
				if (Sparkipelago.currentSession != null) Sparkipelago.currentSession.Locations.ScoutLocationsAsync(HandleScout, keys.ToArray());
			}

			private void HandleScout(Dictionary<long, Archipelago.MultiClient.Net.Models.ScoutedItemInfo> scouted) {
				foreach (long key in scouted.Keys) {
					Color color = new Color(0.2f, 1, 1, 1);
					if ((scouted[key].Flags & ItemFlags.NeverExclude) != 0) color = new Color(0.2f, 1, 0.2f, 1);
					if ((scouted[key].Flags & ItemFlags.Advancement) != 0) color = new Color(1, 0.8f, 0.2f, 1);
					if (Sparkipelago.currentSession.Locations.AllLocationsChecked.Contains(key)) {
						color = new Color(0.5f, 0.5f, 0.5f, 1);
						locids[key].collected = true;
					}
					locids[key].enabled = true;
					locids[key].flags = scouted[key].Flags;
					LineRenderer lr = locids[key].go.GetComponent<LineRenderer>();
					lr.material.SetColor("_EmissionColor", color);
				}
			}
		}
		
		static GameObject createCheckArrow(GameObject parent, string sanity, int index, Vector3 start, Vector3 end) {
			GameObject arrow = new GameObject("Arrow", typeof(LineRenderer));
			arrow.transform.SetParent(parent.transform);
			arrow.transform.localPosition = new Vector3(0, 0, 0);
			arrow.transform.localScale = new Vector3(1, 1, 1);
			LineRenderer lr = arrow.GetComponent<LineRenderer>();
			lr.startWidth = 2;
			lr.endWidth = 0.1f;
			lr.numCapVertices = 16;
			lr.useWorldSpace = false;
			lr.material.EnableKeyword("_EMISSION");
			lr.material.color = new Color(0, 0, 0, 1);
			lr.material.SetTexture("_EmissionMap", Texture2D.whiteTexture);
			lr.SetPositions(new Vector3[]{start, end});
			scout.addLocation(arrow, sanity, index);
			return arrow;
		}
		
		static void recurseGameObject<T>(GameObject parent, List<T> comps, bool getLayer, string sanity) where T : Component {
			ActivateOnDistance aod = parent.GetComponent<ActivateOnDistance>();
			if (parent.activeSelf || aod != null || parent.name == "Area_2 (Day)") {
				T comp = parent.GetComponent<T>();
				if (comp != null) {
					if (getLayer) layers |= 1 << parent.layer;
				//	if (Locations.hasLocationByIndex(stage, sanity, comps.Count)) {
						Vector3 start = Vector3.zero, end = Vector3.zero;
						if (sanity == "bubble" || sanity == "explore") {start = new Vector3(0, 2, 0); end = new Vector3(0, 1.25f, 0);}
						if (sanity == "capsule") {start = new Vector3(0, 6, 0); end = new Vector3(0, 4, 0);}
						if (sanity == "checkpoint") {start = new Vector3(0, 10, -20); end = new Vector3(0, 8, -20);}
						if (sanity == "coin") {start = new Vector3(0, 1.875f, 0); end = new Vector3(0, 1.25f, 0);}
						createCheckArrow(parent, sanity, comps.Count, start, end);
				//	}
					comps.Add(comp);
				}
				for (int i = 0; i < parent.transform.childCount; i++) {
					recurseGameObject(parent.transform.GetChild(i).gameObject, comps, getLayer, sanity);
				}
			}
		}

		static void recurseForTag(GameObject parent, List<GameObject> objects, string tag, string sanity) {
			ActivateOnDistance aod = parent.GetComponent<ActivateOnDistance>();
			if (parent.activeSelf || aod != null || parent.name == "Area_2 (Day)") {
				if (parent.tag == tag) {
			//		if (Locations.hasLocationByIndex(stage, sanity, objects.Count)) {
						
			//		}
					objects.Add(parent);
				}
				for (int i = 0; i < parent.transform.childCount; i++) {
					recurseForTag(parent.transform.GetChild(i).gameObject, objects, tag, sanity);
				}
			}
		}
		
		static List<GameObject> getAllWithTag(Scene scn, string tag, string sanity) {
			List<GameObject> tagged = new List<GameObject>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				recurseForTag(go, tagged, tag, sanity);
			}
			return tagged;
		}
		
		static List<T> getAllComponents<T>(Scene scn, bool getLayer, string sanity) where T : Component {
			List<T> rotring = new List<T>();
			foreach (GameObject go in scn.GetRootGameObjects()) {
				recurseGameObject<T>(go, rotring, getLayer, sanity);
			}
			
			return rotring;
		}
		
		public static TrackType trackType = TrackType.NearestAny;
		public static Transform trackXfrm;
		static GameObject playerArrow;

		public static void trackCheckByName(string name) {
			trackType = TrackType.FIXED;
		}

		public static void trackCheckByIndex(string sanity, int index) {
			trackType = TrackType.FIXED;
		}
		
		public static void updateTracker() {
			if (playerArrow == null) return;
			Vector3 playerPos = playerArrow.transform.position;
			if (trackType == TrackType.None) {
				trackXfrm = null;
			} else if (trackType != TrackType.FIXED) {
				Transform nearestAny = null;
				float anyDist = 10000000;
				Transform nearestUseful = null;
				float usefulDist = 10000000;
				Transform nearestProg = null;
				float progDist = 10000000;

				foreach (CollectibleScout.ScoutData sd in scout.locids.Values) {
					if (sd.collected || !sd.enabled || !sd.go) continue;
					Vector3 xfrmPos = sd.go.transform.position;
					float xfrmDist = Vector3.Distance(xfrmPos, playerPos);
					if ((sd.flags & ItemFlags.Advancement) != 0 && xfrmDist < progDist) {
						progDist = xfrmDist;
						nearestProg = sd.go.transform;
					}
					if ((sd.flags & (ItemFlags.NeverExclude | ItemFlags.Advancement)) != 0 && xfrmDist < usefulDist) {
						usefulDist = xfrmDist;
						nearestUseful = sd.go.transform;
					}
					if (xfrmDist < anyDist) {
						anyDist = xfrmDist;
						nearestAny = sd.go.transform;
					}
				}
				
				switch (trackType) {
					case TrackType.NearestAny:
						trackXfrm = nearestAny;
						break;
					case TrackType.NearestUseful:
						trackXfrm = nearestUseful;
						break;
					case TrackType.NearestProgress:
						trackXfrm = nearestProg;
						break;
				}
			}
			if (trackXfrm) {
				playerArrow.SetActive(true);
				playerArrow.transform.LookAt(trackXfrm);
			} else playerArrow.SetActive(false);
		}
		
		public static void onSceneLoad(string name) {
			// The normal index isn't initialized yet
			stage = GameObject.Find("[OffStageVaribales]").GetComponent<GameProgressVariables>().StageIndex;

			Scene scn = SceneManager.GetSceneByName(name);
			scout = new CollectibleScout();

			GameObject arrowObject = new GameObject("Point", typeof(MeshFilter), typeof(MeshRenderer));
			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uv = new List<Vector2>();
			List<int> triangles = new List<int>();
			vertices.Add(new Vector3(0, 0, 1.5f));
			uv.Add(new Vector2(0.5f, 1.0f));
			int VCOUNT = 32;
			for (float i = 0; i < VCOUNT+1; i += 1) {
				float rad = (i/VCOUNT)*(2*Mathf.PI);
				float xpos = Mathf.Cos(rad)*0.5f;
				float ypos = Mathf.Sin(rad)*0.5f;
				vertices.Add(new Vector3(xpos, ypos, 0));
				uv.Add(new Vector2(i/VCOUNT, 0.5f));
				vertices.Add(new Vector3(xpos, ypos, 0.2f));
				uv.Add(new Vector2(i/VCOUNT, 0.6f));
			}
			for (int i = 1; i < VCOUNT+1; i += 1) {
				triangles.Add(0);
				triangles.Add(i*2);
				triangles.Add((i+1)*2);
				triangles.Add(i*2);
				triangles.Add(i*2+1);
				triangles.Add((i+1)*2);
				triangles.Add(i*2);
				triangles.Add(i*2-1);
				triangles.Add(i*2+1);
			}
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.uv = uv.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateNormals();
			arrowObject.GetComponent<MeshFilter>().mesh = mesh;

			playerArrow = new GameObject("Arrow");
			playerArrow.transform.SetParent(Sparkipelago.player.transform);
			playerArrow.transform.localPosition = new Vector3(0, 4.5f, 0);
			playerArrow.transform.localScale = Vector3.one;
			GameObject playerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Component.Destroy(playerSphere.GetComponent<SphereCollider>());
			playerSphere.transform.SetParent(playerArrow.transform);
			playerSphere.transform.localPosition = Vector3.zero;
			playerSphere.transform.localScale = Vector3.one;
			playerSphere.transform.localRotation = Quaternion.Euler(90, 0, 0);

			arrowObject.transform.SetParent(playerArrow.transform);
			arrowObject.transform.localPosition = Vector3.zero;
			arrowObject.transform.localScale = Vector3.one;

			Material mat = playerSphere.GetComponent<MeshRenderer>().material;
			arrowObject.GetComponent<MeshRenderer>().material = mat;
			mat.color = new Color(0, 0, 1, 1);
			mat.EnableKeyword("_EMISSION");
			mat.SetColor("_EmissionColor", new Color(0, 0.5f, 1, 1));
			if (Sparkipelago.eCapsule) {
				Material ecapMat = Sparkipelago.eCapsule.transform.Find("CapsuleModel").gameObject.GetComponent<MeshRenderer>().material;
				Texture tex = ecapMat.GetTexture("_Noise");
				mat.SetTexture("_EmissionMap", tex);
			}

			layers = 0;
			capsules = getAllComponents<RotateRing>(scn, true, "capsule");
			checkpoints = getAllComponents<CheckPointData>(scn, false, "checkpoint");
			bubbles = getAllComponents<MonitorData>(scn, true, "bubble");
			medals = getAllComponents<WorldMedal>(scn, false, "explore");
			coins = getAllComponents<CollectableCoin>(scn, false, "coin"); // There's an easier way but shrug
			batteries = getAllWithTag(scn, "Battery", "battery");
			scout.sendScout();
			
			int coinLeft = 0;
			for (int i = 0; i < coins.Count; i++) {
				if (Locations.isLocationCompleteByIndex(stage, "coin", i)) {
					GameObject.Destroy(coins[i].gameObject);
					CollectableCoin.CollectableCoinList.Remove(coins[i]);
				//	Material mat = coins[i].transform.Find("Pivot/CoinMesh").gameObject.GetComponent<MeshRenderer>().material;
				//	mat.color = new Color(1f, 1f, 1f, 1f);
				} else coinLeft += 1;
			}
			if (coins.Count > 0) {
				CollectablesController collect = GameObject.Find("[ Collectable UI ]").GetComponent<CollectablesController>();
				if (collect.MedalAmm > coinLeft) collect.MedalAmm = coinLeft;
				if ((Locations.isLocationComplete(stage, "COMPLETION") || (SlotData.coinHunt > 0))) {
					collect.StageTime = 30000;
					if (SlotData.coinHunt > 0) coinLeft *= 1000;
					collect.MedalAmm = coinLeft > 0 ? coinLeft : 1000000;
				}
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
			
			MelonLogger.Msg("{0} Capsules, {1} Checkpoints, {2} Bubbles, {3} Batteries", capsules.Count, checkpoints.Count, bubbles.Count, batteries.Count);
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

		[HarmonyPatch(typeof(WorldMedal), "SetExploreMedal")]
		private static class WorldMedalPatch {
			private static void Postfix(int medal) {
				MelonLogger.Msg("Sending " + Save.CurrentStageIndex.ToString() + " Medal " + medal.ToString());
				Locations.sendLocationCheck(Save.CurrentStageIndex, string.Format("{0} EXPLORATION MEDAL", MEDALNAMES[medal]));
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

		[HarmonyPatch(typeof(CarCollectables), "OnTriggerEnter")]
		private static class BatteryPatch {
			private static void Prefix(Collider col) {
				if (col.tag == "Battery" && batteries.Contains(col.gameObject)) {
					int idx = batteries.IndexOf(col.gameObject);
					Sparkipelago.debugLog("Collected Battery #{0}", idx);
					Locations.sendLocationByIndex(Save.CurrentStageIndex, "battery", idx);
					batteries[idx] = null;
				}
			}
		}
	}
}