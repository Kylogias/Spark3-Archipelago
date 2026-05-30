using UnityEngine;
using HarmonyLib;
using MelonLoader;

namespace Sparkipelago {
	class EnemyRando {
		static System.Random enemyRng;
		static GameObject enemies;
		static GameObject bosses;

		public static void setupEnemyRando(GameObject prefabHolder, GameObject prefabObject) {
			string[] enemiesExclude = {
				"PinkToten", "LazerToten", "BlueToten", "YellowToten", "TotenBase",
				"UfoFireMen", "SmallFireMen", "BigObserver_CarEnemy", "BigObserverMissle_CarEnemy",
				"[EnemySpawner] - Once", "[EnemySpawner] - EternalNew", "TotenHolder", 
				"PoliceCopterObject", "LineworkSparkModel"
			};
			
			enemyRng = new System.Random();
			enemies = prefabHolder.transform.GetChild(1).gameObject;
			bosses = prefabHolder.transform.GetChild(2).gameObject;
			enemies.transform.SetParent(prefabObject.transform);
			Sparkipelago.setupPrefabChildren(enemies, false, enemiesExclude);
			bosses.transform.SetParent(prefabObject.transform);
			Sparkipelago.setupPrefabChildren(bosses, false, enemiesExclude);
		}
		
		[HarmonyPatch(typeof(EnemySpawner), "SpawnInNormal")]
		private static class EnemyRandoPatch {
			private static void Prefix(EnemySpawner __instance) {
				if (APSave.file.client.enemyRando == EnemyType.Vanilla) return;
				int numEnemy = enemies.transform.childCount;
				if (APSave.file.client.enemyRando == EnemyType.BossesOnEnemies) numEnemy += bosses.transform.childCount;
				int idx = enemyRng.Next(numEnemy);
				if (idx >= enemies.transform.childCount) {
					idx -= enemies.transform.childCount;
					__instance.Enemy = bosses.transform.GetChild(idx).gameObject;
				} else __instance.Enemy = enemies.transform.GetChild(idx).gameObject;
			}
		}
	}
}