using MelonLoader;
using UnityEngine;

using System.Collections.Generic;

namespace Sparkipelago {
	class LabMode {
		public class MoveDebugPref {
			KeyCode hotkey;
			public ItemIds itemID;
			string itemName;
			public string eName;
			MelonPreferences_Entry<bool> entry;
			
			public MoveDebugPref(string category, string entryName, ItemIds id, string name, KeyCode key) {
				entry = MelonPreferences.CreateEntry<bool>(category, entryName, true);
				entry.OnEntryValueChanged.Subscribe(onEvent, 100);
				eName = entryName;
				hotkey = key;
				itemID = id;
				itemName = name;
			}
			
			public void onUpdate() {
				if (Sparkipelago.hasItem(itemID) != entry.Value) Sparkipelago.itemState[itemID] = entry.Value ? 1 : 0;
				if (Input.GetKeyDown(hotkey)) {
					if (Sparkipelago.hasItem(itemID)) {
						entry.Value = false;
						Sparkipelago.debugLog("Disabling {0}", itemName);
					} else {
						entry.Value = true;
						Sparkipelago.debugLog("Enabling {0}", itemName);
					}
				}
			}

			public void onChange(bool newV) {
				entry.Value = newV;
			}
			private void onEvent(bool oldV, bool newV) {
				if (!SlotData.labMode) return;
				Sparkipelago.itemState[itemID] = newV ? 1 : 0;
			}
		}
		
		public static List<MoveDebugPref> movedbg;

		
		public static void initPrefs() {
			movedbg = new List<MoveDebugPref>();
			
			MelonPreferences.CreateCategory("AP Abilities");
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Jester Dash (Hotkey 5)",  ItemIds.JESTER_DASH,         "JESTER_DASH",         KeyCode.Alpha5));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Dash (Hotkey 6)",         ItemIds.DASH,                "DASH",                KeyCode.Alpha6));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Charged Dash (Hotkey 7)", ItemIds.CHARGED_JESTER_DASH, "CHARGED_JESTER_DASH", KeyCode.Alpha7));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Wall Jump (Hotkey 8)",    ItemIds.WALL_JUMP,           "WALL_JUMP",           KeyCode.Alpha8));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Wall Walk (Hotkey 9)",    ItemIds.WALL_WALK,           "WALL_WALK",           KeyCode.Alpha9));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Double Jump (Hotkey 0)",  ItemIds.DOUBLE_JUMP,         "DOUBLE_JUMP",         KeyCode.Alpha0));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Combat (Hotkey -)",       ItemIds.COMBAT,              "COMBAT",              KeyCode.Minus));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Down Dash (Hotkey +)",    ItemIds.DOWN_DASH,           "DOWN_DASH",           KeyCode.Equals));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Parry",                   ItemIds.PARRY,               "PARRY",               KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Car", ItemIds.CAR, "CAR", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Abilities", "Has Copter", ItemIds.COPTER, "COPTER", KeyCode.None));

			MelonPreferences.CreateCategory("AP Gimmicks");
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Springs", ItemIds.SPRINGS, "SPRINGS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Speed Boosters", ItemIds.SPEED_PADS, "SPEED_PADS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Dash Rings", ItemIds.DASH_RINGS, "DASH_RINGS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Pulleys", ItemIds.PULLEYS, "PULLEYS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Prison Rockets", ItemIds.PRISON_ROCKETS, "PRISON_ROCKETS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Jester Dash Rings", ItemIds.JESTER_DASH_RINGS, "JESTER_DASH_RINGS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Abyss Bracers", ItemIds.ABYSS_BRACERS, "ABYSS_BRACERS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Ramps", ItemIds.RAMPS, "RAMPS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Protestors", ItemIds.PROTESTORS, "PROTESTORS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Fans", ItemIds.FANS, "FANS", KeyCode.None));
			movedbg.Add(new MoveDebugPref("AP Gimmicks", "Has Abyss Boosters", ItemIds.ABYSS_BOOSTERS, "ABYSS_BOOSTERS", KeyCode.None));
		}
		
		public static void checkForInput() {
			foreach (MoveDebugPref move in movedbg) {
				move.onUpdate();
			}
			if (Input.GetKeyDown(KeyCode.Keypad0)) PlayerHealthAndStats.AddEnergy(100);
			if (Input.GetKeyDown(KeyCode.Keypad1) && CharacterAnimatorChange.StaticReference != null) {
				int newCharacter = CharacterAnimatorChange.Character + 1;
				if (newCharacter == 5) newCharacter = 0;
				CharacterAnimatorChange.StaticReference.Switch(newCharacter);
			}
			if (Input.GetKeyDown(KeyCode.Keypad2)) UnityEngine.Object.Instantiate(Sparkipelago.copter, Sparkipelago.player.transform.position, Quaternion.identity);
		}
	}
}