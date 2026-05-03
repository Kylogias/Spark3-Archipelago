using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using MelonLoader;
using Newtonsoft.Json;
using Rewired;

namespace Sparkipelago {
	[Serializable]
	public class APConnectionInfo {
		public string info;
		public string ip;
		public int port;
		public string slot;
		public string password;

		public APConnectionInfo() {
			info = "[EMPTY AP SLOT]";
			ip = "localhost";
			port = 38281;
			slot = "Player1";
			password = "";
		}
	}
	
	[Serializable]
	public class APSavedata {
		public string room;

		public int lastItem;
		public int fpCount;
		public int numLocations;
		public int numChecked;
		public List<long> checkedLocations;
		public Save.SaveFile file;

		public APSavedata(bool dumm) {
			file = new Save.SaveFile();
			file.Dummy = dumm;
			room = "";
			lastItem = -1;
			fpCount = 0;
			checkedLocations = new List<long>();
			numLocations = 1;
			numChecked = 0;
		}
	}
	
	class APSave {

		[HarmonyPatch(typeof(GameProgressMenuController), "SetCompletionPercentage")]
		private class CompletionPercentPatch {
			private static bool Prefix(ref float __result, int savefileIndex) {
				APSavedata data = file.slots[savefileIndex];
				__result = (float)data.checkedLocations.Count / (float)data.numLocations;
				Save.GetSaveFile(savefileIndex).CompletionPercentage = __result;
				return false;
			}
		}
		
		// +-------------------+
		// | CONNECTION SCREEN |
		// +-------------------+
		
		private static bool isChangingConnection;
		private static bool isNewSlot;
		private static int connectRow;
		private static GameObject connectUI;

		private static int port;
		private static string[] columns;
		private static Text[] textComps;
		private static Transform[] pivots;
		private static GameObject[] infoGO;
		
		public static void onSaveSelect() {
			isChangingConnection = false;
			isNewSlot = false;
			connectRow = 0;
			port = 0;

			columns = new string[6];
			textComps = new Text[6];
			pivots = new Transform[6];

			infoGO = new GameObject[5];
			for (int i = 0; i < 5; i++) {
				infoGO[i] = GameObject.Find(string.Format("UI/SaveSelect/Saves/ParentSaves/Slot#{0}/PERCENT/SaveNumText", i));
				infoGO[i].GetComponent<Text>().text = file.connect[i].info;
			}
			
			GameObject UIObject = GameObject.Find("UI");
			connectUI = new GameObject("APConnection");
			connectUI.transform.SetParent(UIObject.transform);
			connectUI.transform.position = UIObject.transform.position;
			connectUI.SetActive(false);

			Text versionText = GameObject.Find("UI/VersionNumber").GetComponent<Text>();
			Font font = versionText.font;

			float rowHeight = versionText.fontSize*2;
			string[] rows = {"Info", "IP", "Port", "Slot", "Password", "If KB+M, Press"};
			for (int i = 0; i < rows.Length; i++) {
				
				GameObject row = new GameObject(rows[i]);
				row.transform.SetParent(connectUI.transform);
				row.transform.position = new Vector3(0, (rowHeight*(4-i))-(rowHeight), 0) + connectUI.transform.position;
				GameObject text = new GameObject("Text", typeof(Text));
				text.transform.SetParent(row.transform);

				float width = connectUI.transform.position.x/2;
				GameObject pivot = new GameObject("Pivot");
				pivot.transform.position = new Vector3(-width, 0, 0) + row.transform.position;
				pivot.transform.SetParent(row.transform);
				pivots[i] = pivot.transform;
				
				text.transform.position = new Vector3(-width/2, 0, 0) + row.transform.position;
				Text textComp = text.GetComponent<Text>();
				textComp.text = rows[i];
				textComp.font = font;
				textComp.fontSize = (int)(versionText.fontSize*1.5);
				textComp.alignment = TextAnchor.MiddleCenter;
				RectTransform textRect = text.GetComponent<RectTransform>();
				textRect.sizeDelta = new Vector2(width, rowHeight);

				GameObject input = new GameObject("Input", typeof(Text));
				input.transform.SetParent(row.transform);
				input.transform.position = new Vector3(width/2, 0, 0) + row.transform.position;
				Text inputText = input.GetComponent<Text>();
				textComps[i] = inputText;
				inputText.font = font;
				inputText.fontSize = (int)(versionText.fontSize*1.5);
				inputText.alignment = TextAnchor.MiddleLeft;
				RectTransform inputRect = input.GetComponent<RectTransform>();
				inputRect.sizeDelta = new Vector2(width, rowHeight);
			}
		}

		[HarmonyPatch(typeof(SaveFileMenu), "Update")]
		private class ConnectInfoPatch {
			private static bool Prefix(
				SaveFileMenu __instance,
				Player ___Rewinp,
				int ___Index,
				ref int ___AxisInput,
				bool ___SaveSelectMenu
			) {
				if (isChangingConnection) {
					// AxisManager is a private function
					MethodInfo axisManager = __instance.GetType().GetMethod("AxisManager", BindingFlags.NonPublic | BindingFlags.Instance);
					axisManager.Invoke(__instance, null);
					if (Input.inputString == "" || connectRow == 5) {
						if (___Rewinp.GetButtonDown("AttackLight") || ___Rewinp.GetButtonDown("JesterDash") || (___Rewinp.GetButtonDown("Jump") && isNewSlot)) {
							isChangingConnection = false;
							isNewSlot = false;
							connectUI.SetActive(false);
							GameObject.Find("UI/SaveSelect/Saves").SetActive(true);
							GameObject.Find("UI/SaveSelect/Difficulty").SetActive(true);
							return true;
						}
						if (___Rewinp.GetButtonDown("D_up") || ___AxisInput == 2) {
							connectRow -= 1;
							if (connectRow < 0) connectRow = 0;
						}
						if (___Rewinp.GetButtonDown("D_down") || ___AxisInput == -2) {
							connectRow += 1;
							if (connectRow >= 6) connectRow = 5;
						}
					}

					__instance.Pointer.position = Vector3.Lerp(__instance.Pointer.position, pivots[connectRow].position, Time.deltaTime * __instance.PointerSpeed);
					
					// Spark 3 doesn't play nice with InputField components, jury-rig our own
					foreach (char c in Input.inputString) {
						if (connectRow == 2) {
							if (c == '\b') port /= 10;
							if (c >= '0' && c <= '9' && port < 10000) {
								port *= 10;
								port += (int)(c-'0');
							}
						} else if (connectRow == 5) {} else {
							if (c == '\b') columns[connectRow] = columns[connectRow].Substring(0, columns[connectRow].Length-1);
							else if (c != '\n' && c != '\r') {
								columns[connectRow] = string.Concat(columns[connectRow], c);
							}
						}
					}

					file.connect[___Index].info = columns[0];
					file.connect[___Index].ip = columns[1];
					file.connect[___Index].port = port;
					file.connect[___Index].slot = columns[3];
					file.connect[___Index].password = columns[4];
					infoGO[___Index].GetComponent<Text>().text = columns[0];
					for (int i = 0; i < 6; i++) {
						textComps[i].text = columns[i];
					}
					textComps[2].text = port.ToString();
					
					return false;
				}
				if (___SaveSelectMenu && !__instance.YouSureMenu.activeSelf) {
					if (___Rewinp.GetButtonDown("AttackLight")) {
						isChangingConnection = true;
						connectUI.SetActive(true);
						GameObject.Find("UI/SaveSelect/Saves").SetActive(false);
	
						columns[0] = file.connect[___Index].info;
						columns[1] = file.connect[___Index].ip;
						port = file.connect[___Index].port;
						columns[3] = file.connect[___Index].slot;
						columns[4] = file.connect[___Index].password;
						columns[5] = "Left Click";
						return false;
					}
					if (___Rewinp.GetButtonDown("Jump") && !Save.GetSaveFile(___Index).SlotInUse) {
						isChangingConnection = true;
						isNewSlot = true;
						connectUI.SetActive(true);
						GameObject.Find("UI/SaveSelect/Saves").SetActive(false);
						columns[0] = "New AP Slot";
						columns[1] = "localhost";
						port = 38281;
						columns[3] = "Player1";
						columns[4] = "";
						columns[5] = "Space Here";
						return false;
					}
				}
				
				return true;
			}
		}
		
		// +---------------+
		// | SAVE REDIRECT |
		// +---------------+

		[Serializable]
		public class APSavefile {
			public APSavedata[] slots;
			public APConnectionInfo[] connect;

			public APSavefile(int amm) {
				slots = new APSavedata[amm];
				connect = new APConnectionInfo[amm];
				for (int i = 0; i < amm; i++) {
					connect[i] = new APConnectionInfo();
					slots[i] = new APSavedata(true);
				}
			}
		}
		public static APSavefile file;
		
		[HarmonyPatch(typeof(Save), "SaveAll")]
		class SaveAllPatch {
			static bool Prefix() {
				saveAPSave();
				return false;
			}
		}

		[HarmonyPatch(typeof(Save), "SaveCurrentFile")]
		class SaveCurrentFilePatch {
			public static bool Prefix() {
				saveAPSave();
				return false;
			}
		}

		[HarmonyPatch(typeof(Save), "CreateNewSaveData")]
		class CreateNewSaveDataPatch {
			public static void Prefix(int ___SlotAmmount) {
				file = new APSavefile(___SlotAmmount);
			}
		}

		[HarmonyPatch(typeof(Save), "LoadAll")]
		class LoadAllPatch {
			public static bool Prefix(Save __instance, ref bool __result, int ___SlotAmmount) {
				file = new APSavefile(___SlotAmmount);
				Save.Saves = new Save.SaveFile[___SlotAmmount];
				int num = 0;
				if (File.Exists(Application.dataPath + "/../apdata.save")) {
					string content = File.ReadAllText(Application.dataPath + "/../apdata.save");
					file = JsonConvert.DeserializeObject<APSavefile>(content);
					for (int i = 0; i < ___SlotAmmount; i++) {
						Save.Saves[i] = file.slots[i].file;
						num++;
					}
				}
				if (num >= ___SlotAmmount) {
					__result = true;
					return false;
				}
				__result = false;
				return false;
			}
		}

		public static APSavedata getAPSave() {
			return file.slots[Save.CurrentSaveSlot];
		}

		public static APConnectionInfo getAPConnect() {
			return file.connect[Save.CurrentSaveSlot];
		}

		public static void saveAPSave() {
			for (int i = 0; i < file.slots.Length; i++) {
				APSavedata data = file.slots[i];
				data.file = Save.Saves[i];
				file.slots[i] = data;
			}
			string content = JsonConvert.SerializeObject(file, Formatting.Indented);
			File.WriteAllText(Application.dataPath + "/../apdata.save", content);
		}

		public static void clearAPSave() {
			file.slots[Save.CurrentSaveSlot] = new APSavedata(true);
		}
	}
}