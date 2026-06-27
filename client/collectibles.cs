using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using HarmonyLib;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Sparkipelago {
	class Collectibles {
		static string[] MEDALNAMES = {"Cyan", "Green", "Yellow", "Red", "Magenta", "Purple", "Blue", "Grey", "White", "Brown"};
		static List<RotateRing> capsules;
		static List<CheckPointData> checkpoints;
		static List<MonitorData> bubbles;
		static List<CollectableCoin> coins;
		static List<GameObject> batteries;
		static List<WorldMedal> medals;

		static int layers;
		static int stage;
		public static CollectibleScout scout;

		static int eBubCount;
		static int bBubCount;
		static int eCapCount;
		static int hCapCount;
		static int sCapCount;
		static int checkpointCount;
		static int batteryCount;
		static int coinCount;
		
		public class CollectibleScout {
			public class ScoutData {
				public GameObject go;
				public ItemFlags flags;
				public string sanity;
				public bool collected;
				public bool enabled;
			};
			
			public Dictionary<long, ScoutData> locids;
			public List<ScoutData> allLocations;
			
			public CollectibleScout() {
				locids = new Dictionary<long, ScoutData>();
				allLocations = new List<ScoutData>();
			}

			public void addLocation(GameObject go, string sanity, int index) {
				if (sanity == "_FAKE") return;
				long key = Locations.getLocationByIndex(stage, sanity, index);
				ScoutData sd = new ScoutData();
				sd.go = go;
				sd.sanity = sanity;
				allLocations.Add(sd);
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
					if (Locations.hasLocationByIndex(stage, sanity, comps.Count) || SlotData.labMode) {
						Vector3 start = Vector3.zero, end = Vector3.zero;
						if (sanity == "bubble" || sanity == "explore") {start = new Vector3(0, 2, 0); end = new Vector3(0, 1.25f, 0);}
						if (sanity == "capsule") {start = new Vector3(0, 6, 0); end = new Vector3(0, 4, 0);}
						if (sanity == "checkpoint") {start = Vector3.up*1.25f; end = Vector3.zero;}
						if (sanity == "coin") {start = new Vector3(0, 1.875f, 0); end = new Vector3(0, 1.25f, 0);}
						GameObject arrow = createCheckArrow(parent, sanity, comps.Count(), start, end);
						if (sanity == "checkpoint") {
							arrow.transform.rotation = Quaternion.identity;
							arrow.transform.localPosition = new Vector3(0, 8, -20);
						}
					}
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
					if (Locations.hasLocationByIndex(stage, sanity, objects.Count) || SlotData.labMode) {
						Vector3 start = Vector3.zero, end = Vector3.zero;
						if (sanity == "battery") {start = new Vector3(0, 6, 0); end = new Vector3(0, 4, 0);}
						createCheckArrow(parent, sanity, objects.Count(), start, end);
					}
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
		
		static bool trackFixed;
		public static Transform trackXfrm;
		static GameObject playerArrow;

		public static void trackCheckByName(string command) {
			string search = command.Substring(command.IndexOf(' ')+1);
			foreach (APStageData stagedata in APShared.stages) {
				if (stagedata.id == stage) {
					foreach (APStageCheck check in stagedata.checks) {
						if (check.name == search && scout.locids.ContainsKey(check.id)) {
							trackXfrm = scout.locids[check.id].go.transform;
							trackFixed = true;
							break;
						}
					}
					break;
				}
			}
		}

		public static void trackCheckByIndex(string command) {
			string[] args = command.Split(new Char[]{' '}, 3, StringSplitOptions.RemoveEmptyEntries);
			int index = -1;
			string sanity = args[2];
			if (Int32.TryParse(args[1], out index)) {
				switch (sanity) {
					case "explore":
						if (index < medals.Count() && medals[index] != null) {trackXfrm = medals[index].gameObject.transform; trackFixed = true;}
						break;
					case "capsule":
						if (index < capsules.Count() && capsules[index] != null) {trackXfrm = capsules[index].gameObject.transform; trackFixed = true;}
						break;
					case "bubble":
						if (index < bubbles.Count() && bubbles[index] != null) {trackXfrm = bubbles[index].gameObject.transform; trackFixed = true;}
						break;
					case "coin":
						if (index < coins.Count() && coins[index] != null) {trackXfrm = coins[index].gameObject.transform; trackFixed = true;}
						break;
					case "battery":
						if (index < batteries.Count() && batteries[index] != null) {trackXfrm = batteries[index].gameObject.transform; trackFixed = true;}
						break;
				}
			}
		}
		
		public static void updateTracker() {
			if (playerArrow == null) return;
			if (playerArrow.transform.parent == Sparkipelago.player.transform && !Sparkipelago.player.activeSelf) {
				playerArrow.transform.SetParent(null);
				playerArrow.transform.localScale = Vector3.one;
			}
			if (playerArrow.transform.parent != Sparkipelago.player.transform) {
				playerArrow.transform.position = (Vector3.up*4.5f) + Sparkipelago.player.transform.position;
				if (Sparkipelago.player.activeSelf) {
					playerArrow.transform.SetParent(Sparkipelago.player.transform);
					playerArrow.transform.localPosition = Vector3.up*4.5f;
					playerArrow.transform.localScale = Vector3.one;
				}
			}
			Vector3 playerPos = playerArrow.transform.position;
			TrackType trackType = APSave.file.client.trackerMode;
			foreach (CollectibleScout.ScoutData sd in scout.locids.Values) {
				if (!sd.go) continue;
				if (sd.sanity == "explore") {
					if (!APSave.file.client.exploreArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
				if (sd.sanity == "coin") {
					if (!APSave.file.client.coinArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
				if (sd.sanity == "capsule") {
					if (!APSave.file.client.capsuleArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
				if (sd.sanity == "bubble") {
					if (!APSave.file.client.bubbleArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
				if (sd.sanity == "battery") {
					if (!APSave.file.client.batteryArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
				if (sd.sanity == "checkpoint") {
					if (!APSave.file.client.checkpointArrows) sd.go.SetActive(false);
					else sd.go.SetActive(true);
				}
			}
			if (trackType == TrackType.None && !trackFixed) {
				trackXfrm = null;
			} else if (!trackFixed) {
				Transform nearestAny = null;
				float anyDist = 10000000;
				Transform nearestUseful = null;
				float usefulDist = 10000000;
				Transform nearestProg = null;
				float progDist = 10000000;

				if (trackType == TrackType.IncludeAll) foreach (CollectibleScout.ScoutData sd in scout.allLocations) {
					if (!sd.go) continue;
					if (sd.sanity == "checkpoint" && !APSave.file.client.labTrackCheckpoint) continue;
					if (sd.sanity == "capsule" && !APSave.file.client.labTrackCapsule) continue;
					if (sd.sanity == "bubble" && !APSave.file.client.labTrackBubble) continue;
					if (sd.sanity == "explore" && !APSave.file.client.labTrackMedal) continue;
					if (sd.sanity == "coin" && !APSave.file.client.labTrackCoin) continue;
					if (sd.sanity == "battery" && !APSave.file.client.labTrackBattery) continue;
					Transform xfrm = sd.go.transform;
					Transform goParent = xfrm.parent.parent; // Parent is the check
					bool isInactive = false;
					while (goParent != null) {
						if (!goParent.gameObject.activeSelf) {
							isInactive = true;
							break;
						}
						goParent = goParent.parent;
					}
					if (isInactive) continue;
					Vector3 xfrmPos = xfrm.position;
					float xfrmDist = Vector3.Distance(xfrmPos, playerPos);
					if (xfrmDist < anyDist) {
						anyDist = xfrmDist;
						nearestAny = xfrm;
					}
				} else if (trackType == TrackType.Energy) {
					foreach (MonitorData mon in bubbles) {
						if (!mon) continue;
						if (mon.Type != MonitorType.Energy) continue;
						Vector3 xfrmPos = mon.gameObject.transform.position;
						float xfrmDist = Vector3.Distance(xfrmPos, playerPos);
						if (xfrmDist < anyDist) {
							anyDist = xfrmDist;
							nearestAny = mon.gameObject.transform;
						}
					}
					foreach (RotateRing cap in capsules) {
						if (!cap) continue;
						if (cap.tag != "EnergyCap") continue;
						Vector3 xfrmPos = cap.gameObject.transform.position;
						float xfrmDist = Vector3.Distance(xfrmPos, playerPos) * 10;
						if (xfrmDist < anyDist) {
							anyDist = xfrmDist;
							nearestAny = cap.gameObject.transform;
						}
					}
				} else foreach (CollectibleScout.ScoutData sd in scout.locids.Values) {
					if (sd.collected || !sd.enabled || !sd.go) continue;
					Transform goParent = sd.go.transform.parent.parent; // Parent is the check
					bool isInactive = false;
					while (goParent != null) {
						if (!goParent.gameObject.activeSelf) {
							isInactive = true;
							break;
						}
						goParent = goParent.parent;
					}
					if (isInactive) continue;
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
					case TrackType.Energy:
					case TrackType.IncludeAll:
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
			} else {
				playerArrow.SetActive(false);
				trackFixed = false;
			}
		}
		
		public static void onSceneLoad(string name) {
			// The normal index isn't initialized yet
			stage = GameObject.Find("[OffStageVaribales]").GetComponent<GameProgressVariables>().StageIndex;

			Scene scn = SceneManager.GetSceneByName(name);
			scout = new CollectibleScout();
			hCapCount = 1;
			eCapCount = 1;
			sCapCount = 1;
			eBubCount = 1;
			bBubCount = 1;
			checkpointCount = 1;
			batteryCount = 1;
			coinCount = 1;

			playerArrow = new GameObject("Arrow");
			playerArrow.transform.SetParent(Sparkipelago.player.transform);
			playerArrow.transform.localPosition = new Vector3(0, 4.5f, 0);
			playerArrow.transform.localScale = Vector3.one;

			Vector2[] uvs = {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 1)};
			Vector3[] vertices = new Vector3[uvs.Length];
			int[] triangles = {
				0, 2, 3, 2, 1, 3,
				0, 3, 2, 1, 2, 3,
				4, 6, 7, 6, 5, 7,
				4, 7, 6, 5, 6, 7,
				2, 1, 5, 2, 4, 1,
				2, 5, 0, 2, 0, 4
			};
			Vector2 aspect = new Vector2(7.0f/9.0f, 1);
			for (int i = 0; i < uvs.Length/2; i++) {
				vertices[i] = new Vector3((uvs[i].x-0.5f)*aspect.x, 0, (uvs[i].y-0.5f)*aspect.y);
				vertices[i+uvs.Length/2] = new Vector3(0, (uvs[i].x-0.5f)*aspect.x, (uvs[i].y-0.5f)*aspect.y);
			}
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			
			GameObject arrowQuad = new GameObject("Quad", typeof(MeshRenderer), typeof(MeshFilter));
			arrowQuad.transform.SetParent(playerArrow.transform);
			arrowQuad.transform.localPosition = Vector3.zero;
			arrowQuad.transform.localScale = Vector3.one*2;
			arrowQuad.transform.localRotation = Quaternion.identity;
			arrowQuad.GetComponent<MeshFilter>().mesh = mesh;
			Material mat = arrowQuad.GetComponent<MeshRenderer>().material;
			mat.color = new Color(0, 0, 0, 1);
			mat.EnableKeyword("_EMISSION");
			mat.SetColor("_EmissionColor", new Color(1, 0.8402f, 0, 0));
			mat.SetTexture("_EmissionMap", APSave.cursorTex);

			layers = 0;
			capsules = getAllComponents<RotateRing>(scn, true, "capsule");
			checkpoints = getAllComponents<CheckPointData>(scn, false, "checkpoint");
			bubbles = getAllComponents<MonitorData>(scn, true, "bubble");
			medals = getAllComponents<WorldMedal>(scn, false, "explore");
			coins = getAllComponents<CollectableCoin>(scn, false, "coin"); // There's an easier way but shrug
			batteries = getAllWithTag(scn, "Battery", "battery");
			scout.sendScout();
			
			if (SlotData.labMode && APSave.file.client.labmodeDestroy) {
				try {
					string content = File.ReadAllText(Application.dataPath + "/../apshared.json");
					APJson json = JsonConvert.DeserializeObject<APJson>(content);
					foreach (APJsonStage jstage in json.stages) {
						if (jstage.id == stage) {
							foreach (APJsonRegion jregion in jstage.regions) {
								foreach (APJsonCheck jcheck in jregion.checks) {
									if (jcheck.sanity == "checkpoint") {
										Sparkipelago.player.GetComponent<LevelProgressControl>().SetCheckPoint(checkpoints[jcheck.index]);
									}
									if (jcheck.sanity == "capsule") {
										string tag = capsules[jcheck.index].gameObject.tag;
										if (tag == "Ring") hCapCount += 1;
										if (tag == "ScoreCapsule") sCapCount += 1;
										if (tag == "EnergyCap") eCapCount += 1;
										GameObject.Destroy(capsules[jcheck.index].gameObject);
									}
									if (jcheck.sanity == "bubble") {
										MonitorData mon = bubbles[jcheck.index];
										if (mon.Type == MonitorType.Ring) bBubCount += 1;
										if (mon.Type == MonitorType.Energy) eBubCount += 1;
										GameObject.Destroy(bubbles[jcheck.index].gameObject);
									}
									if (jcheck.sanity == "explore") GameObject.Destroy(medals[jcheck.index].gameObject);
									if (jcheck.sanity == "coin") {
										GameObject.Destroy(coins[jcheck.index].gameObject);
									}
									if (jcheck.sanity == "battery") {
										GameObject.Destroy(batteries[jcheck.index]);
									}
								}
							}
						}
					}
				} catch (IOException) {
					MelonLogger.Msg("Destroy Labbed Checks is on, but no APShared found");
				}
			}
			
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
				if ((Locations.isLocationComplete(stage, "Completion") || (SlotData.coinHunt > 0))) {
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

		[HarmonyPatch(typeof(WorldMedal), "OnTriggerEnter")]
		private static class WorldMedalPatch {
			private static void Postfix(WorldMedal __instance, Collider col) {
				if (__instance.MedalId == -1) return;
				if (col.tag == "Player" || col.tag == "PlayerVehicle") {
					int medal = __instance.MedalId;
					if (APSave.file.client.labTrackMedal)
						Sparkipelago.debugLog("{{\"name\": \"{0} Exploration Medal\", \"index\": {1}, \"sanity\": \"explore\", \"requires\": \"\"}},", MEDALNAMES[medal], medal);
						Locations.sendLocationCheck(Save.CurrentStageIndex, string.Format("{0} Exploration Medal", MEDALNAMES[medal]));
					Transform arrow = __instance.gameObject.transform.Find("Arrow");
					arrow.gameObject.SetActive(false);
					foreach (CollectibleScout.ScoutData sd in scout.allLocations) {
						if (sd.go == arrow.gameObject) sd.go = null;
					}
					__instance.MedalId = -1;
				}
			}
		}
		
		[HarmonyPatch(typeof(Monitors_Interactions), "OnTriggerEnter")]
		private static class MonitorsPatch {
			private static void Prefix(Monitors_Interactions __instance, Collider col) {
				if (col.tag != "Monitor") return;
				
				string type = "Unknown";
				MonitorData mon = col.GetComponent<MonitorData>();
				int count = -1;
				if (!bubbles.Contains(mon)) return;
				if (mon.Type == MonitorType.Ring) {type = "Bit"; count = bBubCount; bBubCount += 1;}
				if (mon.Type == MonitorType.Energy) { type = "Energy"; count = eBubCount; eBubCount += 1; }
				if (APSave.file.client.labTrackBubble)
					Sparkipelago.debugLog("{{\"name\": \"{0} Bubble #{1}\", \"index\": {2}, \"sanity\": \"bubble\", \"requires\": \"\"}},", type, count, bubbles.IndexOf(mon));
				bubbles[bubbles.IndexOf(mon)] = null;
			}
		}
		[HarmonyPatch(typeof(Objects_Interaction), "BasicCollectables")]
		private static class CollectiblePatch {
			private static void Prefix(Objects_Interaction __instance, Collider col) {
				if (col.tag != "Ring" && col.tag != "ScoreCapsule" && col.tag != "EnergyCap") return;
				
				RotateRing rotring = col.GetComponent<RotateRing>();
				if (!capsules.Contains(rotring)) return;
				string capType = "";
				int count = -1;
				if (col.tag == "Ring") {capType = "Health"; count = hCapCount; hCapCount += 1;}
				if (col.tag == "ScoreCapsule") {capType = "Score"; count = sCapCount; sCapCount += 1;}
				if (col.tag == "EnergyCap") { capType = "Energy"; count = eCapCount; eCapCount += 1; }
				if (APSave.file.client.labTrackCapsule)
					Sparkipelago.debugLog("{{\"name\": \"{0} Capsule #{1}\", \"index\": {2}, \"sanity\": \"capsule\", \"requires\": \"\"}},", capType, count, capsules.IndexOf(rotring));
				capsules[capsules.IndexOf(rotring)] = null;
			}
		}
		
		[HarmonyPatch(typeof(LevelProgressControl), "SetCheckPoint")]
		private static class CheckpointPatch {
			private static void Prefix(LevelProgressControl __instance, CheckPointData check) {
				if (!checkpoints.Contains(check)) return;
				int count = checkpointCount;
				checkpointCount += 1;
				if (APSave.file.client.labTrackCheckpoint)
					Sparkipelago.debugLog("{{\"name\": \"Checkpoint #{0}\", \"index\": {1}, \"sanity\": \"checkpoint\", \"requires\": \"\"}},", count, checkpoints.IndexOf(check));
				Transform arrow = check.gameObject.transform.Find("Arrow");
				arrow.gameObject.SetActive(false);
				foreach (CollectibleScout.ScoutData sd in scout.allLocations) {
					if (sd.go == arrow.gameObject) sd.go = null;
				}
				checkpoints[checkpoints.IndexOf(check)] = null;
			}
		}
		
		[HarmonyPatch(typeof(CollectableCoin), "OnTriggerEnter")]
		private static class CoinPatch {
			private static void Prefix(CollectableCoin __instance, Collider col) {
				if (col.tag == "Player" && coins.Contains(__instance)) {
					int idx = coins.IndexOf(__instance);
					int count = coinCount;
					coinCount += 1;
					if (APSave.file.client.labTrackCoin)
						Sparkipelago.debugLog("{{\"name\": \"Blue Coin #{0}\", \"index\": {1}, \"sanity\": \"coin\", \"requires\": \"\"}},", count, idx);
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
					int count = batteryCount;
					batteryCount += 1;
					if (APSave.file.client.labTrackBattery)
						Sparkipelago.debugLog("{{\"name\": \"Battery #{0}\", \"index\": {1}, \"sanity\": \"battery\", \"requires\": \"ca\"}},", count, idx);
					Locations.sendLocationByIndex(Save.CurrentStageIndex, "battery", idx);
					batteries[idx] = null;
				}
			}
		}
	}
}