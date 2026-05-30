using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Sparkipelago {
	public class Options {
		public class TutorialCategory {
			public Transform icon;
			public Transform parent;
			public string name;
			public List<TutorialIten> itens;
		}
		
		public enum ItenType {
			DEFAULT,
			OPTION_BOOL,
			OPTION_INT,
			INVENTORY
		}
		
		public delegate string OnSetInt(int newV);
		public delegate string OnSetBool(bool newV);
		public delegate int OnGetInt();
		public delegate bool OnGetBool();
		
		public class TutorialIten {
			public OnSetInt optionSetInt;
			public OnGetInt optionGetInt;
			public int minInt;
			public int maxInt;
			
			public OnSetBool optionSetBool;
			public OnGetBool optionGetBool;
			public ItemIds invItem;

			public string name;
			public string desc;
			public ItenType type;
			public TutorialMenuIten component;
			public Text nameText;
		}
		
		static List<TutorialCategory> optCategories;
		static TutorialMenu tutorial;
		static GameObject templateInfo;
		static GameObject templateInput1;
		static GameObject templateInput2;
		static Color activeColor;
		static Color inactiveColor;

		public static TutorialCategory addCategory(string name, Texture2D tex) {
			TutorialCategory newCat = new TutorialCategory();
			newCat.name = name;
			newCat.itens = new List<TutorialIten>();

			Transform categories = tutorial.transform.Find("PauseBg/CategoryList/CategoryIcons");
			Transform gameIcon = categories.Find("Icon_Common");
			GameObject icon = GameObject.Instantiate(gameIcon.gameObject);
			icon.name = name;
			icon.transform.SetParent(categories);
			icon.transform.localScale = Vector3.one;
			icon.transform.localRotation = Quaternion.identity;
			icon.transform.localPosition = Vector3.zero;
			Image img = icon.GetComponent<Image>();
			img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width/2, tex.height/2));
			newCat.icon = icon.transform;
			
			Transform itens = tutorial.transform.Find("PauseBg/ItensList");
			GameObject parent = GameObject.Instantiate(itens.GetChild(0).gameObject);
			parent.name = name;
			parent.transform.SetParent(itens);
			parent.transform.localScale = Vector3.one;
			parent.transform.localRotation = Quaternion.identity;
			parent.transform.localPosition = itens.GetChild(0).localPosition;
			newCat.parent = parent.transform;
			for (int i = 0; i < parent.transform.childCount; i++) {
				GameObject.Destroy(parent.transform.GetChild(i).gameObject);
			}

			optCategories.Add(newCat);

			return newCat;
		}

		public static TutorialIten setupIten(TutorialCategory cat, GameObject template, string name, string desc) {
			TutorialIten iten = new TutorialIten();
			GameObject go = GameObject.Instantiate(template, cat.parent);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			iten.name = name;
			iten.nameText = go.transform.GetChild(1).gameObject.GetComponent<Text>();
			iten.nameText.text = name;
			TutorialMenuIten component = go.GetComponent<TutorialMenuIten>();
			iten.desc = desc;
			component.Description = desc;
			iten.component = component;
			cat.itens.Add(iten);
			return iten;
		}
		
		public static void addIten(TutorialCategory cat, string name, string desc) {
			TutorialIten iten = setupIten(cat, templateInfo, name, desc);
			iten.type = ItenType.DEFAULT;
		}

		public static void addIten(TutorialCategory cat, string name, string desc, int min, int max, OnSetInt optionSetInt, OnGetInt optionGetInt) {
			TutorialIten iten = setupIten(cat, templateInput2, name, desc);
			iten.type = ItenType.OPTION_INT;
			InputIcon icon1 = iten.component.gameObject.transform.GetChild(0).gameObject.GetComponent<InputIcon>();
			InputIcon icon2 = iten.component.gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<InputIcon>();
			icon1.Button = InputDevice.ControllerButton.BumperL;
			icon1.ChangeIcon();
			icon2.Button = InputDevice.ControllerButton.BumperR;
			icon2.ChangeIcon();
			iten.optionSetInt = optionSetInt;
			iten.optionGetInt = optionGetInt;
			iten.minInt = min;
			iten.maxInt = max;
		}

		public static void addIten(TutorialCategory cat, string name, string desc, OnSetBool optionSetBool, OnGetBool optionGetBool) {
			TutorialIten iten = setupIten(cat, templateInput1, name, desc);
			iten.type = ItenType.OPTION_BOOL;
			InputIcon icon1 = iten.component.gameObject.transform.GetChild(0).gameObject.GetComponent<InputIcon>();
			icon1.Button = InputDevice.ControllerButton.FaceBotton;
			icon1.ChangeIcon();
			iten.optionSetBool = optionSetBool;
			iten.optionGetBool = optionGetBool;
		}

		public static void addIten(TutorialCategory cat, ItemIds item) {
			TutorialIten iten = setupIten(cat, templateInfo, "", "");
			iten.type = ItenType.INVENTORY;
			iten.invItem = item;
		}
		
		public static void buildCategories() {
			tutorial = GameObject.Find("PlayerObjects").GetComponentInChildren<TutorialMenu>(true);
			optCategories = new List<TutorialCategory>();
			inactiveColor = new Color(0.8f, 0.3f, 0.2f);
			activeColor = new Color(0.1f, 0.7f, 0.3f);
			TutorialMenuIten[][] catItens = {
				tutorial.GameInfoItens,
				tutorial.CommonItens,
				tutorial.CombatItens,
				tutorial.JesterItens,
				tutorial.ReaperItens,
				tutorial.FloatItens,
				tutorial.FarkItens,
				tutorial.SfarxItens,
				tutorial.PowersItens,
				tutorial.UpgradeItens
			};
			
			RectTransform categoryXfrm = tutorial.transform.Find("PauseBg/CategoryList/CategoryIcons").gameObject.GetComponent<RectTransform>();
			categoryXfrm.sizeDelta = new Vector2(categoryXfrm.sizeDelta.x * 1.25f, categoryXfrm.sizeDelta.y);
			Transform itens = tutorial.transform.Find("PauseBg/ItensList");
			templateInfo = itens.GetChild(0).Find("Icon_FP").gameObject;
			templateInput1 = itens.GetChild(1).Find("Icon_Jump").gameObject;
			templateInput2 = itens.GetChild(1).Find("Icon_DownDash").gameObject;

			for (int i = 0; i < tutorial.CategoryIcons.Count(); i++) {
				TutorialCategory category = new TutorialCategory();
				category.icon = tutorial.CategoryIcons[i];
				category.parent = tutorial.CategoryParents[i];
				category.name = tutorial.CategoryName[i];
				category.itens = new List<TutorialIten>();
				foreach (TutorialMenuIten menuIten in catItens[i]) {
					TutorialIten iten = new TutorialIten();
					iten.component = menuIten;
					category.itens.Add(iten);
				}
				optCategories.Add(category);
			}
			
			APSave.addOptions();
			TutorialCategory inventory = addCategory("INVENTORY", Sparkipelago.apTexture);
			foreach (long id in APShared.itemIDs) {
				addIten(inventory, (ItemIds)id);
			}
			if (SlotData.labMode) {
				TutorialCategory lab = addCategory("LAB MODE", Sparkipelago.labTexture);
				foreach (LabMode.MoveDebugPref move in LabMode.movedbg) {
					addIten(lab, move.eName, "", (bool newV) => {move.onChange(newV); return newV.ToString();}, () => {return Sparkipelago.hasItem(move.itemID);});
				}
			}

			tutorial.CategoryIcons = new Transform[optCategories.Count()];
			tutorial.CategoryName = new string[optCategories.Count()];
			tutorial.CategoryParents = new Transform[optCategories.Count()];
			for (int i = 0; i < optCategories.Count(); i++) {
				tutorial.CategoryIcons[i] = optCategories[i].icon;
				tutorial.CategoryName[i] = optCategories[i].name;
				tutorial.CategoryParents[i] = optCategories[i].parent;
			}
		}

		[HarmonyPatch(typeof(TutorialMenu), "Update")]
		private class TutorialMenuUpdatePatch {
			private static void Postfix(TutorialMenu __instance, int ___Category, int ___Index) {
				bool A = __instance.Inp.Rewinp.GetButtonDown("Jump");
				bool LB = __instance.Inp.Rewinp.GetButtonDown("Parry");
				bool RB = __instance.Inp.Rewinp.GetButtonDown("LockOn");
				TutorialIten iten = optCategories[___Category].itens[___Index];
				switch (iten.type) {
					case ItenType.DEFAULT: break;
					case ItenType.OPTION_INT:
						if (LB || RB) {
							int direction = 0;
							if (LB) direction -= 1;
							if (RB) direction += 1;
							int oldV = iten.optionGetInt();
							if (oldV+direction <= iten.maxInt && oldV+direction >= iten.minInt) {
								iten.nameText.text = string.Format("{0}: {1}", iten.name, iten.optionSetInt(oldV+direction));
							}
						}
						break;
					case ItenType.INVENTORY: break;
					case ItenType.OPTION_BOOL:
						if (A) {
							bool oldV = iten.optionGetBool();
							Color newColor = inactiveColor;
							if (!oldV) newColor = activeColor;
							iten.component.gameObject.GetComponent<Image>().color = newColor;
							iten.nameText.text = string.Format("{0}: {1}", iten.name, iten.optionSetBool(!oldV));
						}
						break;
				}
			}
		}
		
		[HarmonyPatch(typeof(TutorialMenu), "SetCurrentCategory")]
		private class TutorialMenuCategoryPatch {
			private static bool Prefix(TutorialMenu __instance, int index, int ___DirectionPressed, ref int ___Category, List<TutorialMenuIten> ___CurrentItens) {				
				for (int i = 0; i < optCategories.Count(); i++) {
					if (optCategories[index].icon.gameObject.activeSelf) {
						break;
					}
					if (___DirectionPressed == -1) {
						___Category--;
						if (___Category < 0) {
							___Category = optCategories.Count() - 1;
						} else if (___Category > optCategories.Count() - 1) {
							___Category = 0;
						}
						index = ___Category;
					} else if (___DirectionPressed == 1) {
						___Category++;
						if (___Category < 0) {
							___Category = optCategories.Count() - 1;
						} else if (___Category > optCategories.Count() - 1) {
							___Category = 0;
						}
						index = ___Category;
					}
				}
				TutorialCategory cat = optCategories[index];
				__instance.CategoryTextName.text = cat.name;
				for (int j = 0; j < optCategories.Count(); j++) {
					optCategories[j].parent.gameObject.SetActive(value: false);
				}

				___CurrentItens.Clear();
				cat.parent.gameObject.SetActive(true);
				for (int k = 0; k < cat.itens.Count(); k++) {
					TutorialIten iten = cat.itens[k];
					switch (iten.type) {
						case ItenType.DEFAULT: break;
						case ItenType.OPTION_INT: {
							int oldV = iten.optionGetInt();
							if (oldV < iten.minInt) oldV = iten.minInt;
							if (oldV > iten.maxInt) oldV = iten.maxInt;
							iten.nameText.text = string.Format("{0}: {1}", iten.name, iten.optionSetInt(oldV));
							break;
						}
						case ItenType.OPTION_BOOL: {
							bool oldV = iten.optionGetBool();
							Color newColor = activeColor;
							if (!oldV) newColor = inactiveColor;
							iten.component.gameObject.GetComponent<Image>().color = newColor;
							iten.nameText.text = string.Format("{0}: {1}", iten.name, iten.optionSetBool(oldV));
							break;
						}
						case ItenType.INVENTORY:
							if (Sparkipelago.currentSession != null) {
								if (Sparkipelago.hasItem(iten.invItem)) iten.component.gameObject.SetActive(true);
								else iten.component.gameObject.SetActive(false);
								iten.nameText.text = string.Format("{1}x {0}", Sparkipelago.currentSession.Items.GetItemName((long)iten.invItem), Sparkipelago.itemState[iten.invItem]);
							}
							break;
					}
					if (iten.component.gameObject.activeSelf) {
						___CurrentItens.Add(iten.component);
					}
				}
				return false;
			}
		}
	}
}