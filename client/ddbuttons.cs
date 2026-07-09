using UnityEngine;
using System.Collections.Generic;
using MelonLoader;

namespace Sparkipelago {
	class DDButton : MonoBehaviour {
		public int ddLeft = 5;
		public int index;
		
		private void Start() {
			gameObject.GetComponent<MeshRenderer>().material.mainTexture = Sparkipelago.ddbtnTextures[ddLeft];
		}

		private void OnCollisionEnter(Collision col) {
			if (col.gameObject.tag != "Player") return;
			MelonLogger.Msg("Player has entered! Velocity {0}", col.relativeVelocity);
			if (Sparkipelago.hasItem(ItemIds.DOWN_DASH) && col.relativeVelocity.y < -80) {
				ddLeft -= 1;
				if (ddLeft <= 0) {
					ddLeft = 0;
					Sparkipelago.debugLog("Sending Check for Downdash Button #{0}!", index);
				}
				gameObject.GetComponent<MeshRenderer>().material.mainTexture = Sparkipelago.ddbtnTextures[ddLeft];
			}
		}
	}
	
	class DowndashButtons {
		static int ddCount;
		public static void createButtons() {
			ddCount = 0;
			int stageidx = GameObject.Find("[OffStageVaribales]").GetComponent<GameProgressVariables>().StageIndex;
			foreach (APStageData stage in APShared.stages) {
				if (stage.id != stageidx) continue;
				foreach (APStageCheck check in stage.checks) {
					if (check.sanity == "downdash" || check.sanity == "ddhard") {
						createDowndashButton(check.pos, 5, ddCount);
						ddCount += 1;
					}
				}
			}
		}
		
		public static void createDowndashButton(Vector3 pos, int count, int index) {
			GameObject button = new GameObject("Downdash Button", typeof(DDButton), typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
			
			DDButton dd = button.GetComponent<DDButton>();
			dd.ddLeft = count;
			dd.index = index;
			
			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<int> indices = new List<int>();
			List<int> topV = new List<int>();
			List<int> bottomV = new List<int>();
			vertices.Add(Vector3.up*-0.4f);
			uvs.Add(Vector2.one*0.5f);
			bottomV.Add(0);
			vertices.Add(Vector3.up*0.2f);
			uvs.Add(Vector2.one*0.5f);
			topV.Add(1);
			int curInd = 2;
			for (float i = 0; i < 32; i++) {
				Vector3 newVtx = new Vector3(Mathf.Cos((i/32)*2*Mathf.PI)*0.5f, 0f, Mathf.Sin((i/32)*2*Mathf.PI)*0.5f);
				Vector2 newUV = new Vector3(newVtx.x+0.5f, newVtx.z+0.5f);
				newVtx *= 8;
				newVtx.y = -0.4f;
				vertices.Add(newVtx);
				uvs.Add(newUV);
				bottomV.Add(curInd++);
				newVtx.y = 0.2f;
				vertices.Add(newVtx);
				uvs.Add(newUV);
				topV.Add(curInd++);
			}
			for (int i = 2; i < topV.Count; i++) {
				indices.Add(topV[0]);
				indices.Add(topV[i]);
				indices.Add(topV[i-1]);
			}
			indices.Add(topV[0]);
			indices.Add(topV[1]);
			indices.Add(topV[topV.Count-1]);
			for (int i = 2; i < bottomV.Count; i++) {
				indices.Add(bottomV[0]);
				indices.Add(bottomV[i-1]);
				indices.Add(bottomV[i]);
			}
			indices.Add(bottomV[0]);
			indices.Add(bottomV[bottomV.Count-1]);
			indices.Add(bottomV[1]);
			for (int i = 1; i < topV.Count-1; i++) {
				indices.Add(topV[i]);
				indices.Add(topV[i+1]);
				indices.Add(bottomV[i]);
				indices.Add(topV[i+1]);
				indices.Add(bottomV[i+1]);
				indices.Add(bottomV[i]);
			}
			indices.Add(topV[topV.Count-1]);
			indices.Add(topV[1]);
			indices.Add(bottomV[bottomV.Count-1]);
			indices.Add(topV[1]);
			indices.Add(bottomV[1]);
			indices.Add(bottomV[bottomV.Count-1]);

			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.RecalculateNormals();
			button.GetComponent<MeshFilter>().mesh = mesh;
			button.GetComponent<MeshCollider>().sharedMesh = mesh;
			
			button.transform.position = pos;
		}
		
		public static void addDowndashButton() {
			Vector3 pos = Sparkipelago.player.transform.position;
			ddCount += 1;
			Sparkipelago.debugLog("{{\"name\": \"Downdash Button #{0}\", \"sanity\": \"downdash\", \"pos\": [{1}, {2}, {3}], \"requires\": \"\"}},", ddCount, pos.x, pos.y, pos.z);
			createDowndashButton(pos, 5, ddCount-1);
		}
	}
}