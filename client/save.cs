using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
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
	
	public enum MusicType {
		Vanilla = 0,
		PerStage = 1,
		PerLoad = 2,
		PerLoop = 3
	}

	public enum EnemyType {
		Vanilla = 0,
		EnemiesOnly = 1,
		BossesOnEnemies = 2
	}

	public enum TrackType {
		None,
		NearestAny,
		NearestUseful,
		NearestProgress,
		Energy,
		IncludeAll
	};
	
	public enum ChatType {
		NoMessages,
		SentAndReceived,
		AllMessages
	};
	
	[Serializable]
	public class APClientOptions {
		public MusicType musicRando;
		public EnemyType enemyRando;
		public int diveFloors;
		
		public double chatDelay;
		public double chatVis;
		public ChatType displayedMessages;
		
		public int scoreAmt;
		public int scoreMax;
		public double comboAmt;
		public double comboMax;
		public double timeAmt;
		public double timeMax;
		public double energyAmt;
		public double energyMax;
		
		public double trapTime;
		public int shopMaxBit;
		public double shopDiscount;

		public bool labmodeDestroy;
		public bool labTrackCheckpoint;
		public bool labTrackBubble;
		public bool labTrackCapsule;
		public bool labTrackMedal;
		public bool labTrackCoin;
		public bool labTrackBattery;

		public bool deathLink;
		public bool trapLink;
		public bool destroyCoins;
		public bool capsuleArrows;
		public bool bubbleArrows;
		public bool checkpointArrows;
		public bool exploreArrows;
		public bool coinArrows;
		public bool batteryArrows;
		public bool ddbuttonArrows;

		public TrackType trackerMode;

		public APClientOptions() {
			musicRando = MusicType.Vanilla;
			enemyRando = EnemyType.Vanilla;
			diveFloors = 1;
			chatDelay = 0;
			chatVis = 5;
			displayedMessages = ChatType.SentAndReceived;
			scoreAmt = 10;
			scoreMax = 30;
			comboAmt = 1;
			comboMax = 1;
			energyAmt = 1.7;
			energyMax = 10;
			timeAmt = 0.9;
			timeMax = 0.5;
			shopMaxBit = 1000;
			shopDiscount = 0.9;
			trapTime = 2;
			labmodeDestroy = true;
			labTrackCheckpoint = true;
			labTrackBubble = true;
			labTrackCapsule = true;
			labTrackMedal = true;
			labTrackCoin = true;
			labTrackBattery = true;
			deathLink = false;
			trapLink = false;
			destroyCoins = true;
			capsuleArrows = false;
			bubbleArrows = false;
			checkpointArrows = false;
			exploreArrows = false;
			coinArrows = false;
			batteryArrows = false;
			trackerMode = TrackType.NearestAny;
		}
	}
	
	class APSave {
		public static Font sparkFont;
		public static Texture cursorTex;
		public static Sprite buttonSprite;

		[Serializable]
		public class APSavefile {
			public APSavedata[] slots;
			public APConnectionInfo[] connect;
			public APClientOptions client;

			public APSavefile(int amm) {
				slots = new APSavedata[amm];
				connect = new APConnectionInfo[amm];
				client = new APClientOptions();
				for (int i = 0; i < amm; i++) {
					connect[i] = new APConnectionInfo();
					slots[i] = new APSavedata(true);
				}
			}
		}
		
		[HarmonyPatch(typeof(GameProgressMenuController), "SetCompletionPercentage")]
		private class CompletionPercentPatch {
			private static bool Prefix(ref float __result, int savefileIndex) {
				APSavedata data = file.slots[savefileIndex];
				__result = (float)data.numChecked / (float)data.numLocations;
				Save.GetSaveFile(savefileIndex).CompletionPercentage = __result;
				return false;
			}
		}

		// +----------------+
		// | CLIENT OPTIONS |
		// +----------------+

		public static void addOptions() {
			Options.TutorialCategory settings = Options.addCategory("SETTINGS", Sparkipelago.settingsTexture);
			new Options.RangeIten(
				settings, "Endless Dive Floors", "How many floors should be completed to send a check?", 1, 10, 1,
				(double newV) => {APSave.file.client.diveFloors = (int)newV; return ((int)newV).ToString();},
				() => {return (double)APSave.file.client.diveFloors;}
			);
			new Options.RangeIten(
				settings, "Tracker Mode", "What is the default mode of the tracker arrow?", 0, SlotData.labMode ? 5 : 3, 1,
				(double newV) => {APSave.file.client.trackerMode = (TrackType)newV; return ((TrackType)newV).ToString();},
				() => {return (double)APSave.file.client.trackerMode;}
			);
			new Options.RangeIten(
				settings, "Music Rando", "How should music be randomized? (Requires Stage Restart)", 0, 3, 1,
				(double newV) => {APSave.file.client.musicRando = (MusicType)newV; return ((MusicType)newV).ToString();},
				() => {return (double)APSave.file.client.musicRando;}
			);
			new Options.RangeIten(
				settings, "Enemy Rando", "How should enemies be randomized", 0, 2, 1,
				(double newV) => {APSave.file.client.enemyRando = (EnemyType)newV; return ((EnemyType)newV).ToString();},
				() => {return (double)APSave.file.client.enemyRando;}
			);
			new Options.RangeIten(
				settings, "Displayed Chat Messages", "What chat messages should be displayed?", 0, 2, 1,
				(double newV) => {APSave.file.client.displayedMessages = (ChatType)newV; return ((ChatType)newV).ToString();},
				() => {return (double)APSave.file.client.displayedMessages;}
			);
			new Options.RangeIten(
				settings, "Chat Delay Time", "When the chat is full, how long until the next message is displayed?", 0, 20, 0.1,
				(double newV) => {APSave.file.client.chatDelay = newV; return newV.ToString();},
				() => {return APSave.file.client.chatDelay;}
			);
			new Options.RangeIten(
				settings, "Chat Visible Time", "How long should each chat message be visible before disappearing?", 0, 20, 0.1,
				(double newV) => {APSave.file.client.chatVis = newV; return newV.ToString();},
				() => {return APSave.file.client.chatVis;}
			);
			
			new Options.RangeIten(
				settings, "Score Amount", "How much should each score multiplier item add?", 0, 100, 1,
				(double newV) => {APSave.file.client.scoreAmt = (int)newV; return ((int)newV).ToString();},
				() => {return (double)APSave.file.client.scoreAmt;}
			);
			new Options.RangeIten(
				settings, "Score Max", "What is the maximum the score multiplier items can give?", 0, 100, 1,
				(double newV) => {APSave.file.client.scoreMax = (int)newV; return ((int)newV).ToString();},
				() => {return (double)APSave.file.client.scoreMax;}
			);
			new Options.RangeIten(
				settings, "Combo Amount", "How much should each combo multiplier item add?", 0, 10, 0.1,
				(double newV) => {APSave.file.client.comboAmt = newV; return newV.ToString();},
				() => {return APSave.file.client.comboAmt;}
			);
			new Options.RangeIten(
				settings, "Combo Max", "What is the maximum the combo multiplier items can give?", 0, 10, 0.1,
				(double newV) => {APSave.file.client.comboMax = newV; return newV.ToString();},
				() => {return APSave.file.client.comboMax;}
			);
			new Options.RangeIten(
				settings, "Time Stop Amount", "How much should each time stop item multiply the timer speed? A value of 1 means no effect, 0.5 slows by half, etc", 0, 1, 0.01,
				(double newV) => {APSave.file.client.timeAmt = newV; return newV.ToString();},
				() => {return APSave.file.client.timeAmt;}
			);
			new Options.RangeIten(
				settings, "Time Stop Max", "What is the maximum the time stop items can slow down time? A value of 1 means no effect, 0.5 is half, etc", 0, 1, 0.01,
				(double newV) => {APSave.file.client.timeMax = newV; return newV.ToString();},
				() => {return APSave.file.client.timeMax;}
			);
			new Options.RangeIten(
				settings, "Energy Amount", "How much energy per second should each progressive energy item generate?", 0, 20, 0.1,
				(double newV) => {APSave.file.client.energyAmt = newV; return newV.ToString();},
				() => {return APSave.file.client.energyAmt;}
			);
			new Options.RangeIten(
				settings, "Energy Max", "What is the maximum energy per second progressive energy can give", 0, 20, 0.1,
				(double newV) => {APSave.file.client.energyMax = newV; return newV.ToString();},
				() => {return APSave.file.client.energyMax;}
			);

			new Options.RangeIten(
				settings, "Shop Max Price", "At what price should shop items start getting discounted", 0, 10000, 50,
				(double newV) => {APSave.file.client.shopMaxBit = (int)newV; return ((int)newV).ToString();},
				() => {return (double)APSave.file.client.shopMaxBit;}
			);
			new Options.RangeIten(
				settings, "Shop Discount", "What should be the discount for shop items above the max price?", 0, 1, 0.001,
				(double newV) => {APSave.file.client.shopDiscount = newV; int perc = (int)(newV*100); return string.Format("{0}.{1}%", perc, (int)(newV*1000)-(perc*10));},
				() => {return APSave.file.client.shopDiscount;}
			);
			new Options.RangeIten(
				settings, "Item Timer", "How often should the in-stage items be collected", 0.1, 20, 0.1,
				(double newV) => {APSave.file.client.trapTime = newV; return newV.ToString();},
				() => {return APSave.file.client.trapTime;}
			);
			
			new Options.BoolIten(
				settings, "Death Link", "Should Death Link be enabled?",
				(bool newV) => {APSave.file.client.deathLink = newV; Bounce.updateTags(); return newV.ToString();},
				() => {return APSave.file.client.deathLink;}
			);
			new Options.BoolIten(
				settings, "Trap Link", "Should Trap Link be enabled?",
				(bool newV) => {APSave.file.client.trapLink = newV; Bounce.updateTags(); return newV.ToString();},
				() => {return APSave.file.client.trapLink;}
			);
			new Options.BoolIten(
				settings, "Destroy Collectathon Coins", "Should collected blue coins in collectathon stages be destroyed?",
				(bool newV) => {APSave.file.client.destroyCoins = newV; return newV.ToString();},
				() => {return APSave.file.client.destroyCoins;}
			);
			new Options.BoolIten(
				settings, "Capsule Check Arrows", "Should there be arrows above capsules denoting progressiveness?",
				(bool newV) => {APSave.file.client.capsuleArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.capsuleArrows;}
			);
			new Options.BoolIten(
				settings, "Bubble Check Arrows", "Should there be arrows above bubbles denoting progressiveness?",
				(bool newV) => {APSave.file.client.bubbleArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.bubbleArrows;}
			);
			new Options.BoolIten(
				settings, "Checkpoint Check Arrows", "Should there be arrows above checkpoints denoting progressiveness?",
				(bool newV) => {APSave.file.client.checkpointArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.checkpointArrows;}
			);
			new Options.BoolIten(
				settings, "Explore Medal Check Arrows", "Should there be arrows above explore medals denoting progressiveness?",
				(bool newV) => {APSave.file.client.exploreArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.exploreArrows;}
			);
			new Options.BoolIten(
				settings, "Collectathon Coin Check Arrows", "Should there be arrows above collectathon coins denoting progressiveness?",
				(bool newV) => {APSave.file.client.coinArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.coinArrows;}
			);
			new Options.BoolIten(
				settings, "Battery Check Arrows", "Should there be arrows above batteries denoting progressiveness?",
				(bool newV) => {APSave.file.client.batteryArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.batteryArrows;}
			);
			new Options.BoolIten(
				settings, "Downdash Button Check Arrows", "Should there be arrows above downdash buttons denoting progressiveness?",
				(bool newV) => {APSave.file.client.ddbuttonArrows = newV; return newV.ToString();},
				() => {return APSave.file.client.ddbuttonArrows;}
			);
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
			if (!sparkFont) {
				sparkFont = font;
				sparkFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
			}
			if (!cursorTex) {
				cursorTex = GameObject.Find("UI").transform.GetChild(6).GetChild(0).gameObject.GetComponent<Image>().mainTexture;
				cursorTex.hideFlags = HideFlags.DontUnloadUnusedAsset;
			}
			if (!buttonSprite) {
				Texture2D buttonTex = (Texture2D)GameObject.Find("UI/SaveSelect/Saves/Inp").GetComponent<Image>().mainTexture;
				Rect buttonSubTex = new Rect(0, 530, 2048, 497);
				Vector4 buttonBorder = new Vector4(406, 0, 144, 0);
				buttonSprite = Sprite.Create(buttonTex, buttonSubTex, Vector2.one*0.5f, 100, 0, SpriteMeshType.FullRect, buttonBorder, false);
				buttonSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
				
			}

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
				textComp.resizeTextForBestFit = true;
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
				inputText.resizeTextForBestFit = true;
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