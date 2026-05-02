using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System;
using MelonLoader;
using Newtonsoft.Json;

namespace Sparkipelago {
	[Serializable]
	public class APSavedata {
		public string room;
		public string slot;
		public string password;
		public string ip;
		public int port;

		public int lastItem;
		public int fpCount;
		public List<long> checkedLocations;
		public Save.SaveFile file;

		public APSavedata(bool dumm) {
			file = new Save.SaveFile();
			file.Dummy = dumm;
			room = "";
			slot = "";
			password = "";
			ip = "";
			port = 0;
			lastItem = -1;
			fpCount = 0;
			checkedLocations = new List<long>();
		}
	}
	
	class APSave {
		[Serializable]
		public class APSavefile {
			public APSavedata[] slots;

			public APSavefile(int amm) {
				slots = new APSavedata[amm];
				for (int i = 0; i < amm; i++) {
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