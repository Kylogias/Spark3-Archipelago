using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using HarmonyLib;

namespace Sparkipelago {
	class TrapBase {
		public ItemIds trapItem = ItemIds.DOWNTOWN_DISSENT_BACKTRACK;
		public string trapName = "Downtown Dissent Backtrack";
		public bool isTrap = true;

		public virtual void onStageLoad() {}
		public virtual void instanceUpdate() {} // Called before active traps are updated, only on the original instance
		public virtual void onUpdate() {} // Called only on active traps
		public virtual void onCollectItem() {}
	}
	
	class EnergyBubble : TrapBase {
		static GameObject eBubble;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			EnergyBubble instance = new EnergyBubble();
			
			GameObject eBubPrefab = GameObject.Find("[Core prefabs]/EnergyBubble");
			eBubble = GameObject.Instantiate(eBubPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(eBubble, true, null);
			
			instance.trapItem = ItemIds.ENERGY_BUBBLE;
			instance.trapName = "Energy Bubble";
			instance.isTrap = false;
			return instance;
		}

		public override void onCollectItem() {
			GameObject.Instantiate(eBubble, Sparkipelago.player.transform.position, Quaternion.identity);
		}
	}

	class EnergyCapsule : TrapBase {
		static GameObject eCapsule;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			EnergyCapsule instance = new EnergyCapsule();
			
			GameObject eCapPrefab = GameObject.Find("[Core prefabs]/EnergyCapsule");
			eCapsule = GameObject.Instantiate(eCapPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(eCapsule, true, null);
			
			instance.trapItem = ItemIds.ENERGY_CAPSULE;
			instance.trapName = "Energy Capsule";
			instance.isTrap = false;
			return instance;
		}

		public override void onCollectItem() {
			GameObject.Instantiate(eCapsule, Sparkipelago.player.transform);
		}
	}

	class HealthCapsule : TrapBase {
		static GameObject hCapsule;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			HealthCapsule instance = new HealthCapsule();

			GameObject hCapPrefab = GameObject.Find("[Core prefabs]/Capsule");
			hCapsule = GameObject.Instantiate(hCapPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(hCapsule, true, null);
			
			instance.trapItem = ItemIds.HEALTH_CAPSULE;
			instance.trapName = "Health Capsule";
			instance.isTrap = false;
			return instance;
		}

		public override void onCollectItem() {
			GameObject.Instantiate(hCapsule, Sparkipelago.player.transform);
		}
	}

	class ScoreCapsule : TrapBase {
		static GameObject sCapsule;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			ScoreCapsule instance = new ScoreCapsule();

			GameObject sCapPrefab = GameObject.Find("[Core prefabs]/Capsule_Score");
			sCapsule = GameObject.Instantiate(sCapPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(sCapsule, true, null);

			instance.trapItem = ItemIds.SCORE_CAPSULE;
			instance.trapName = "Score Capsule";
			instance.isTrap = false;
			return instance;
		}

		public override void onCollectItem() {
			GameObject.Instantiate(sCapsule, Sparkipelago.player.transform);
		}
	}
	
	class NightmareTrap : TrapBase {
		static GameObject redWorld;
		static GameObject playerRed;
		
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			NightmareTrap instance = new NightmareTrap();
			
			GameObject redPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RedGhostWorld");
			GameObject ragingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/RagingGhost");
			GameObject wanderingPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/MinorGhost");

			GameObject ragingInstance = GameObject.Instantiate(ragingPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(ragingInstance, true, null);
			GameObject wanderingInstance = GameObject.Instantiate(wanderingPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(wanderingInstance, true, null);

			redWorld = GameObject.Instantiate(redPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(redWorld, true, null);
			RedWorldSequence redSeq = redWorld.GetComponent<RedWorldSequence>();
			redSeq.RagingGhost = ragingInstance;
			redSeq.WanderingGhost = wanderingInstance;
			
			instance.trapItem = ItemIds.NIGHTMARE_TRAP;
			instance.trapName = "Nightmare Trap";
			return instance;
		}

		public override void onStageLoad() {
			GameObject fogMesh = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera/FogMeshPlayer");
			playerRed = GameObject.Instantiate(redWorld, fogMesh.transform);
			playerRed.GetComponent<SetParent>().Parent = fogMesh.transform;
		}

		public override void onCollectItem() {
			playerRed.SetActive(true);
		}
	}

	class LaserTrap : TrapBase {
		static GameObject grayWorld;
		static GameObject playerGray;
		
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			LaserTrap instance = new LaserTrap();
			
			GameObject grayPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/GrayGhostWorld");
			GameObject lazerPrefab = GameObject.Find("[PREFABS HOLDER]/[AbyssPrefabs]/LazerFirerer");
			GameObject lazerInstance = GameObject.Instantiate(lazerPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(lazerInstance, true, null);

			grayWorld = GameObject.Instantiate(grayPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(grayWorld, true, null);
			GrayWorldSequence graySeq = grayWorld.GetComponent<GrayWorldSequence>();
			graySeq.GrayLazer = lazerInstance;
			
			instance.trapItem = ItemIds.LASER_TRAP;
			instance.trapName = "Laser Trap";
			return instance;
		}

		public override void onStageLoad() {
			GameObject fogMesh = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera/FogMeshPlayer");
			playerGray = GameObject.Instantiate(grayWorld, fogMesh.transform);
			playerGray.GetComponent<SetParent>().Parent = fogMesh.transform;
		}

		public override void onCollectItem() {
			playerGray.SetActive(true);
		}
	}
	
	class FlintTrap : TrapBase {
		static GameObject flint;
		public GameObject cur;
		
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			FlintTrap instance = new FlintTrap();
			
			Transform bossXfrm = prefabHolder.transform.GetChild(2);
			GameObject flintPrefab = bossXfrm.Find("PowerFlintModel").gameObject;
			flint = GameObject.Instantiate(flintPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(flint, true, null);
			
			instance.trapItem = ItemIds.FLINT_TRAP;
			instance.trapName = "Flint Trap";
			return instance;
		}

		public override void onCollectItem() {
			FlintTrap newFlint = new FlintTrap();
			newFlint.cur = GameObject.Instantiate(flint, Sparkipelago.player.transform.position, Quaternion.identity);
			Traps.activeTraps.Add(newFlint);
		}

		public override void onUpdate() {
			if (cur == null) {
				Traps.queuedDelete.Add(this);
				return;
			}
			if (Vector3.Distance(cur.transform.position, Sparkipelago.player.transform.position) > 100 && Sparkipelago.hasItem(ItemIds.COMBAT)) {
				Vector3 pos = Sparkipelago.player.transform.position;
				pos.y += 5;
				cur.transform.position = pos;
			}
		}
	}
	
	class SpringTrap : TrapBase {
		static GameObject spring;
		
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			SpringTrap instance = new SpringTrap();
			
			GameObject springPrefab = GameObject.Find("[Core prefabs]/SpringTire/SpringRoot");
			spring = GameObject.Instantiate(springPrefab, prefabObject.transform);
			Sparkipelago.setupPrefabChildren(spring, true, null);
			
			instance.trapItem = ItemIds.SPRING_TRAP;
			instance.trapName = "Spring Trap";
			return instance;
		}

		public override void onCollectItem() {
			Vector3 speedDir = Sparkipelago.player.GetComponent<PlayerBhysics>().SpeedDir * 0.01f;
			if (speedDir == Vector3.zero) speedDir = Vector3.down;
			GameObject newSpring = GameObject.Instantiate(spring, Sparkipelago.player.transform.position + speedDir, Quaternion.identity);
			newSpring.transform.LookAt(Sparkipelago.player.transform);
			newSpring.transform.localScale = Vector3.one*2;
		}
	}
	
	class GravityTrap : TrapBase {
		static StageProprietiesAlterations trapComponent;
		float trapTimer = 15;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			GravityTrap instance = new GravityTrap();
			instance.trapItem = ItemIds.GRAVITY_TRAP;
			instance.trapName = "Gravity Trap";
			return instance;
		}

		public override void onStageLoad() {
			GameObject canvas = GameObject.Find("APCanvas");
			canvas.AddComponent<StageProprietiesAlterations>();
			trapComponent = canvas.GetComponent<StageProprietiesAlterations>();
			trapComponent.Player = Sparkipelago.player.GetComponent<PlayerBhysics>();
			trapComponent.Actions = Sparkipelago.player.GetComponent<ActionManager>();
			trapComponent.UpForce = -1;
			trapComponent.ConstantUpForce = false;
		}
		
		public override void instanceUpdate() {
			if (trapComponent != null) trapComponent.ConstantUpForce = false;
		}
		
		public override void onUpdate() {
			if (trapComponent == null) return;
			trapComponent.ConstantUpForce = true;
			trapTimer -= Time.deltaTime;
			if (trapTimer < 0) Traps.queuedDelete.Add(this);
		}
		
		public override void onCollectItem() {
			Traps.activeTraps.Add(new GravityTrap());
		}
	}

	class BaldTrap : TrapBase {
		float trapTimer = 30;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			BaldTrap instance = new BaldTrap();
			instance.trapItem = ItemIds.BALD_TRAP;
			instance.trapName = "Bald Trap";
			return instance;
		}
		
		public override void instanceUpdate() {
			makePlayerUnbald();
		}

		public override void onUpdate() {
			makePlayerBald();
			trapTimer -= Time.deltaTime;
			if (trapTimer < 0) Traps.queuedDelete.Add(this);
		}

		public override void onCollectItem() {
			Traps.activeTraps.Add(new BaldTrap());
		}
		
		void makePlayerBald() {
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

		void makePlayerUnbald() {
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
	}

	class ZoomTrap : TrapBase {
		float trapTimer = 15;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			ZoomTrap instance = new ZoomTrap();
			instance.trapItem = ItemIds.ZOOM_TRAP;
			instance.trapName = "Zoom Trap";
			return instance;
		}
		
		public override void instanceUpdate() {
			HedgeCamera hc = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera").GetComponent<HedgeCamera>();
			hc.CameraMaxDistance = CameraControl.InitialDistance;
		}

		public override void onUpdate() {
			HedgeCamera hc = GameObject.Find("PlayerObjects/Camera_Objects/Main Camera").GetComponent<HedgeCamera>();
			hc.CameraMaxDistance = 2;
			trapTimer -= Time.deltaTime;
			if (trapTimer < 0) Traps.queuedDelete.Add(this);
		}
		
		public override void onCollectItem() {
			Traps.activeTraps.Add(new ZoomTrap());
		}
	}

	class DamageTrap : TrapBase {
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			DamageTrap instance = new DamageTrap();
			instance.trapItem = ItemIds.DAMAGE_TRAP;
			instance.trapName = "Damage Trap";
			return instance;
		}

		public override void onCollectItem() {
			Objects_Interaction obj = Sparkipelago.player.GetComponent<Objects_Interaction>();
			PlayerHealthAndStats.PlayerHP -= 1;
			obj.Sounds.RingLossSound();
			obj.HurtCtrl.GetHurt(3);
			obj.Actions.ChangeAction(4);
			obj.Actions.Action04.InitialEvents();
		}
	}

	class ReverseTrap : TrapBase {
		static bool trapActive = false;
		float trapTimer = 15;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			ReverseTrap instance = new ReverseTrap();
			instance.trapItem = ItemIds.REVERSE_TRAP;
			instance.trapName = "Reverse Trap";
			return instance;
		}
		
		[HarmonyPatch(typeof(Rewired.Player))]
		private static class ReverseTrapPatch {
			private static MethodBase TargetMethod() {
				return typeof(Rewired.Player).GetMethod(
					"GetAxis",
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new Type[]{typeof(string)},
					new ParameterModifier[0]
				);
			}
			private static void Postfix(string actionName, ref float __result) {
				if (trapActive && (actionName == "LeftAnalogX" || actionName == "LeftAnalogY")) {
					__result = - __result;
				}
			}
		}
		
		public override void instanceUpdate() {
			trapActive = false;
		}

		public override void onUpdate() {
			trapActive = true;
			trapTimer -= Time.deltaTime;
			if (trapTimer < 0) Traps.queuedDelete.Add(this);
		}

		public override void onCollectItem() {
			Traps.activeTraps.Add(new ReverseTrap());
		}
	}
	
	class SlowTrap : TrapBase {
		float trapTimer = 15;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			SlowTrap instance = new SlowTrap();
			instance.trapItem = ItemIds.SLOW_TRAP;
			instance.trapName = "Slow Trap";
			return instance;
		}

		public override void instanceUpdate() {
			if (Time.timeScale != 0) Time.timeScale = 1;
		}

		public override void onUpdate() {
			if (Time.timeScale != 0) {
				Time.timeScale = 0.5f;
				trapTimer -= Time.deltaTime / Time.timeScale;
				if (trapTimer < 0) Traps.queuedDelete.Add(this);
			}
		}

		public override void onCollectItem() {
			Traps.activeTraps.Add(new SlowTrap());
		}
	}

	class FastTrap : TrapBase {
		float trapTimer = 15;
		public static TrapBase onCollectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			FastTrap instance = new FastTrap();
			instance.trapItem = ItemIds.FAST_TRAP;
			instance.trapName = "Fast Trap";
			return instance;
		}

		public override void instanceUpdate() {
			if (Time.timeScale != 0) Time.timeScale = 1;
		}

		public override void onUpdate() {
			if (Time.timeScale != 0) {
				Time.timeScale = 4;
				trapTimer -= Time.deltaTime / Time.timeScale;
				if (trapTimer < 0) Traps.queuedDelete.Add(this);
			}
		}

		public override void onCollectItem() {
			Traps.activeTraps.Add(new FastTrap());
		}
	}
	
	class Traps {
		public static List<TrapBase> activeTraps;
		public static List<TrapBase> instances;
		public static List<TrapBase> queuedDelete;
		public static Queue<ItemIds> itemQueue;
		public static Queue<ItemIds> prioItemQueue;
		static double itemTimer;
		public static bool initialized = false;
		static Type[] trapTypes = {
			typeof(EnergyBubble),
			typeof(EnergyCapsule),
			typeof(HealthCapsule),
			typeof(ScoreCapsule),
			typeof(NightmareTrap),
			typeof(LaserTrap),
			typeof(FlintTrap),
			typeof(SpringTrap),
			typeof(GravityTrap),
			typeof(BaldTrap),
			typeof(ZoomTrap),
			typeof(DamageTrap),
			typeof(ReverseTrap),
			typeof(SlowTrap),
			typeof(FastTrap)
		};

		public static void collectPrefabs(GameObject prefabHolder, GameObject prefabObject) {
			activeTraps = new List<TrapBase>();
			instances = new List<TrapBase>();
			queuedDelete = new List<TrapBase>();
			itemQueue = new Queue<ItemIds>();
			prioItemQueue = new Queue<ItemIds>();
			foreach (Type trap in trapTypes) {
				instances.Add((TrapBase)trap.GetMethod("onCollectPrefabs").Invoke(null, new object[]{prefabHolder, prefabObject}));
			}
			initialized = true;
		}
		
		public static void onSceneLoad(bool stage) {
			if (!initialized) return;
			itemTimer = 0;
			activeTraps.Clear();
			if (stage) {
				foreach (TrapBase instance in instances) {
					instance.onStageLoad();
				}
			}
		}
		
		public static void onDisconnect() {
			itemQueue.Clear();
			prioItemQueue.Clear();
		}
		
		public static void onUpdate() {
			if (!initialized) return;
			queuedDelete.Clear();
			foreach (TrapBase instance in instances) {
				instance.instanceUpdate();
			}
			foreach (TrapBase trap in activeTraps) {
				trap.onUpdate();
			}
			itemTimer -= Time.deltaTime;
			if (prioItemQueue.Count > 0 && itemTimer < 0) {
				ItemIds nextTrap = prioItemQueue.Dequeue();
				trySpawnTrap(nextTrap, true);
				itemTimer = APSave.file.client.trapTime;
			}
			if (itemQueue.Count > 0 && itemTimer < 0) {
				ItemIds nextTrap = itemQueue.Dequeue();
				trySpawnTrap(nextTrap, false);
				itemTimer = APSave.file.client.trapTime;
			}
			foreach (TrapBase del in queuedDelete) {
				activeTraps.Remove(del);
			}
		}
		
		public static bool isStageItem(ItemIds item) {
			foreach (TrapBase instance in instances) {
				if (instance.trapItem == item) return true;
			}
			return false;
		}
		
		public static void trySpawnTrap(ItemIds trap, bool prio) {
			if (!initialized) return;
			foreach (TrapBase instance in instances) {
				if (instance.trapItem == trap) {
					if (instance.isTrap && !prio) Bounce.trySendTrap(trap);
					Sparkipelago.messages.Enqueue(string.Format("Now Receiving: {0}", instance.trapName));
					instance.onCollectItem();
					break;
				}
			}
		}

		public static ItemIds trapNameToId(string name) {
			foreach (TrapBase instance in instances) {
				if (instance.trapName == name) return instance.trapItem;
			}
			return (ItemIds)0;
		}

		public static string trapIdToName(ItemIds item) {
			foreach (TrapBase instance in instances) {
				if (instance.trapItem == item) return instance.trapName;
			}
			return "";
		}
	}
}