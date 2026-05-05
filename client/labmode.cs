using MelonLoader;
using UnityEngine;

namespace Sparkipelago {
	class LabMode {
		private static MelonPreferences_Entry<bool> dbgHasDA;
		private static MelonPreferences_Entry<bool> dbgHasCD;
		private static MelonPreferences_Entry<bool> dbgHasJD;
		private static MelonPreferences_Entry<bool> dbgHasDD;
		private static MelonPreferences_Entry<bool> dbgHasDJ;
		private static MelonPreferences_Entry<bool> dbgHasWJ;
		private static MelonPreferences_Entry<bool> dbgHasCO;
		
		public static void initPrefs() {
			MelonPreferences.CreateCategory("APDebug");
			dbgHasJD = MelonPreferences.CreateEntry<bool>("APDebug", "Has Jester Dash (Hotkey 5)", true);
			dbgHasDA = MelonPreferences.CreateEntry<bool>("APDebug", "Has Dash (Hotkey 6)", true);
			dbgHasCD = MelonPreferences.CreateEntry<bool>("APDebug", "Has Charged Dash (Hotkey 7)", true);
			dbgHasWJ = MelonPreferences.CreateEntry<bool>("APDebug", "Has Wall Jump (Hotkey 8)", true);
			dbgHasDJ = MelonPreferences.CreateEntry<bool>("APDebug", "Has Double Jump (Hotkey 9)", true);
			dbgHasCO = MelonPreferences.CreateEntry<bool>("APDebug", "Has Combat (Hotkey 0)", true);
			dbgHasDD = MelonPreferences.CreateEntry<bool>("APDebug", "Has Down Dash (Hotkey -)", true);
			
			dbgHasDA.OnEntryValueChanged.Subscribe(onChangeDA, 100);
			dbgHasCD.OnEntryValueChanged.Subscribe(onChangeCD, 100);
			dbgHasJD.OnEntryValueChanged.Subscribe(onChangeJD, 100);
			dbgHasDD.OnEntryValueChanged.Subscribe(onChangeDD, 100);
			dbgHasDJ.OnEntryValueChanged.Subscribe(onChangeDJ, 100);
			dbgHasWJ.OnEntryValueChanged.Subscribe(onChangeWJ, 100);
			dbgHasCO.OnEntryValueChanged.Subscribe(onChangeCO, 100);
		}
		
		public static void checkForInput() {
			if (Sparkipelago.hasItem(ItemIds.DASH) != dbgHasDA.Value) Sparkipelago.itemState[ItemIds.DASH] = dbgHasDA.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.CHARGED_JESTER_DASH) != dbgHasCD.Value) Sparkipelago.itemState[ItemIds.CHARGED_JESTER_DASH] = dbgHasCD.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.JESTER_DASH) != dbgHasJD.Value) Sparkipelago.itemState[ItemIds.JESTER_DASH] = dbgHasJD.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.DOWN_DASH) != dbgHasDD.Value) Sparkipelago.itemState[ItemIds.DOWN_DASH] = dbgHasDD.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.DOUBLE_JUMP) != dbgHasDJ.Value) Sparkipelago.itemState[ItemIds.DOUBLE_JUMP] = dbgHasDJ.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.WALL_JUMP) != dbgHasWJ.Value) Sparkipelago.itemState[ItemIds.WALL_JUMP] = dbgHasWJ.Value ? 1 : 0;
			if (Sparkipelago.hasItem(ItemIds.COMBAT) != dbgHasCO.Value) Sparkipelago.itemState[ItemIds.COMBAT] = dbgHasCO.Value ? 1 : 0;
			
			if (Input.GetKeyDown(KeyCode.Alpha5)) {
				if (Sparkipelago.hasItem(ItemIds.JESTER_DASH)) {
					dbgHasJD.Value = false;
					Sparkipelago.debugLog("Disabling JESTER_DASH");
				} else {
					dbgHasJD.Value = true;
					Sparkipelago.debugLog("Enabling JESTER_DASH");
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha6)) {
				if (Sparkipelago.hasItem(ItemIds.DASH)) {
					dbgHasDA.Value = false;
					Sparkipelago.debugLog("Disabling DASH");
				} else {
					dbgHasDA.Value = true;
					Sparkipelago.debugLog("Enabling DASH");
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha7)) {
				if (Sparkipelago.hasItem(ItemIds.CHARGED_JESTER_DASH)) {
					dbgHasCD.Value = false;
					Sparkipelago.debugLog("Disabling CHARGED_JESTER_DASH");
				} else {
					dbgHasCD.Value = true;
					Sparkipelago.debugLog("Enabling CHARGED_JESTER_DASH");
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha8)) {
				if (Sparkipelago.hasItem(ItemIds.WALL_JUMP)) {
					dbgHasWJ.Value = false;
					Sparkipelago.debugLog("Disabling WALL_JUMP");
				} else {
					dbgHasWJ.Value = true;
					Sparkipelago.debugLog("Enabling WALL_JUMP");
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha9)) {
				if (Sparkipelago.hasItem(ItemIds.DOUBLE_JUMP)) {
					dbgHasDJ.Value = false;
					Sparkipelago.debugLog("Disabling DOUBLE_JUMP");
				} else {
					dbgHasDJ.Value = true;
					Sparkipelago.debugLog("Enabling DOUBLE_JUMP");
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha0)) {
				if (Sparkipelago.hasItem(ItemIds.COMBAT)) {
					dbgHasCO.Value = false;
					Sparkipelago.debugLog("Disabling COMBAT");
				} else {
					dbgHasCO.Value = true;
					Sparkipelago.debugLog("Enabling COMBAT");
				}
			}
			if (Input.GetKeyDown(KeyCode.Minus)) {
				if (Sparkipelago.hasItem(ItemIds.DOWN_DASH)) {
					dbgHasDD.Value = false;
					Sparkipelago.debugLog("Disabling DOWN_DASH");
				} else {
					dbgHasDD.Value = true;
					Sparkipelago.debugLog("Enabling DOWN_DASH");
				}
			}
		}
		
		private static void onChangeDA(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.DASH] = newV ? 1 : 0;
		}
		private static void onChangeCD(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.CHARGED_JESTER_DASH] = newV ? 1 : 0;
		}
		private static void onChangeJD(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.JESTER_DASH] = newV ? 1 : 0;
		}
		private static void onChangeDD(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.DOWN_DASH] = newV ? 1 : 0;
		}
		private static void onChangeDJ(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.DOUBLE_JUMP] = newV ? 1 : 0;
		}
		private static void onChangeWJ(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.WALL_JUMP] = newV ? 1 : 0;
		}
		private static void onChangeCO(bool oldV, bool newV) {
			Sparkipelago.itemState[ItemIds.COMBAT] = newV ? 1 : 0;
		}
	}
}