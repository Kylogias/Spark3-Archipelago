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
			public List<ListIten> itens;
		}
		
		public delegate string OnOptSet<T>(T newV);
		public delegate T OnOptGet<T>();

		public class ListIten {
			public string name;
			public string desc;
			public TutorialMenuIten component;
			public Text nameText;

			public void setup(string name, string desc, TutorialCategory cat, GameObject template) {
				GameObject go = GameObject.Instantiate(template, cat.parent);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				this.name = name;
				nameText = go.transform.GetChild(1).gameObject.GetComponent<Text>();
				nameText.text = name;
				TutorialMenuIten component = go.GetComponent<TutorialMenuIten>();
				this.desc = desc;
				component.Description = desc;
				this.component = component;
				cat.itens.Add(this);
			}

			virtual public void onDirection(int dir) {}
			virtual public void onToggle() {}
			virtual public void onEnable() {}
		}
		
		public class InfoIten : ListIten {
			public InfoIten(TutorialCategory cat, string name, string desc) {
				setup(name, desc, cat, templateInfo);
			}
		}
		
		public class BoolIten : ListIten {
			public OnOptSet<bool> optionSet;
			public OnOptGet<bool> optionGet;

			public BoolIten(TutorialCategory cat, string name, string desc, OnOptSet<bool> onSet, OnOptGet<bool> onGet) {
				setup(name, desc, cat, templateInput1);
				optionSet = onSet;
				optionGet = onGet;

				InputIcon icon1 = component.gameObject.transform.GetChild(0).gameObject.GetComponent<InputIcon>();
				icon1.Button = InputDevice.ControllerButton.FaceBotton;
				icon1.ChangeIcon();
			}
			
			public override void onToggle() {
				bool oldV = optionGet();
				Color newColor = inactiveColor;
				if (!oldV) newColor = activeColor;
				component.gameObject.GetComponent<Image>().color = newColor;
				nameText.text = string.Format("{0}: {1}", name, optionSet(!oldV));
			}
			public override void onEnable() {
				bool oldV = optionGet();
				Color newColor = activeColor;
				if (!oldV) newColor = inactiveColor;
				component.gameObject.GetComponent<Image>().color = newColor;
				nameText.text = string.Format("{0}: {1}", name, optionSet(oldV));
			}
		}
		
		public class RangeIten : ListIten {
			public OnOptSet<double> optionSet;
			public OnOptGet<double> optionGet;
			public double min;
			public double max;
			public double step;

			public RangeIten(TutorialCategory cat, string name, string desc, double minV, double maxV, double stepV, OnOptSet<double> onSet, OnOptGet<double> onGet) {
				setup(name, desc, cat, templateInput2);
				optionSet = onSet;
				optionGet = onGet;
				min = minV;
				max = maxV;
				step = stepV;
				
				InputIcon icon1 = component.gameObject.transform.GetChild(0).gameObject.GetComponent<InputIcon>();
				InputIcon icon2 = component.gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<InputIcon>();
				icon1.Button = InputDevice.ControllerButton.BumperL;
				icon1.ChangeIcon();
				icon2.Button = InputDevice.ControllerButton.BumperR;
				icon2.ChangeIcon();
			}

			public override void onDirection(int direction) {
				double oldV = optionGet();
				oldV += direction*step;
				double numstep = (int)(oldV/step);
				double stepfrac = (oldV/step)-numstep;
				if (stepfrac >= 0.5) numstep += 1;
				oldV = numstep*step;
				if (oldV < min) oldV = min;
				if (oldV > max) oldV = max;
				nameText.text = string.Format("{0}: {1}", name, optionSet(oldV));
			}
			public override void onEnable() {
				double oldV = optionGet();
				double numstep = (int)(oldV/step);
				double stepfrac = (oldV/step)-numstep;
				if (stepfrac >= 0.5) numstep += 1;
				oldV = numstep*step;
				if (oldV < min) oldV = min;
				if (oldV > max) oldV = max;
				nameText.text = string.Format("{0}: {1}", name, optionSet(oldV));
			}
		}
		
		public class InventoryIten : ListIten {
			public ItemIds invItem;

			public InventoryIten(TutorialCategory cat, ItemIds item) {
				setup("", "", cat, templateInfo);
				invItem = item;
			}

			public override void onEnable() {
				if (Sparkipelago.currentSession != null) {
					if (Sparkipelago.hasItem(invItem)) component.gameObject.SetActive(true);
					else component.gameObject.SetActive(false);
					nameText.text = string.Format("{1}x {0}", Sparkipelago.currentSession.Items.GetItemName((long)invItem), Sparkipelago.itemState[invItem]);
				}
			}
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
			newCat.itens = new List<ListIten>();

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
				category.itens = new List<ListIten>();
				foreach (TutorialMenuIten menuIten in catItens[i]) {
					ListIten iten = new ListIten();
					iten.component = menuIten;
					category.itens.Add(iten);
				}
				optCategories.Add(category);
			}
			
			APSave.addOptions();
			TutorialCategory inventory = addCategory("INVENTORY", Sparkipelago.apTexture);
			foreach (long id in APShared.itemIDs) {
				new InventoryIten(inventory, (ItemIds)id);
			}
			if (SlotData.labMode) {
				TutorialCategory lab = addCategory("LAB MODE", Sparkipelago.labTexture);
				new RangeIten(
					lab, "Score Item Count", "", 0, 10, 1,
					(double newV) => {Sparkipelago.itemState[ItemIds.PROGRESSIVE_SCORE] = (int)newV; return ((int)newV).ToString();},
					() => {return Sparkipelago.itemState[ItemIds.PROGRESSIVE_SCORE];}
				);
				new RangeIten(
					lab, "Combo Item Count", "", 0, 10, 1,
					(double newV) => {Sparkipelago.itemState[ItemIds.PROGRESSIVE_COMBO] = (int)newV; return ((int)newV).ToString();},
					() => {return Sparkipelago.itemState[ItemIds.PROGRESSIVE_COMBO];}
				);
				new RangeIten(
					lab, "Timestop Item Count", "", 0, 10, 1,
					(double newV) => {Sparkipelago.itemState[ItemIds.PROGRESSIVE_TIME_STOP] = (int)newV; return ((int)newV).ToString();},
					() => {return Sparkipelago.itemState[ItemIds.PROGRESSIVE_TIME_STOP];}
				);
				new RangeIten(
					lab, "Up Power", "", (int)Items.DpadPowers.None, (int)Items.DpadPowers.End-1, 1,
					(double newV) => {Items.addDpadPower((Items.DpadPowers)newV, Items.DpadDir.Up); return ((Items.DpadPowers)newV).ToString();},
					() => {return (double)Items.getDpadPower(Items.DpadDir.Up);}
				);
				new RangeIten(
					lab, "Left Power", "", (int)Items.DpadPowers.None, (int)Items.DpadPowers.End-1, 1,
					(double newV) => {Items.addDpadPower((Items.DpadPowers)newV, Items.DpadDir.Left); return ((Items.DpadPowers)newV).ToString();},
					() => {return (double)Items.getDpadPower(Items.DpadDir.Left);}
				);
				new RangeIten(
					lab, "Down Power", "", (int)Items.DpadPowers.None, (int)Items.DpadPowers.End-1, 1,
					(double newV) => {Items.addDpadPower((Items.DpadPowers)newV, Items.DpadDir.Down); return ((Items.DpadPowers)newV).ToString();},
					() => {return (double)Items.getDpadPower(Items.DpadDir.Down);}
				);
				new RangeIten(
					lab, "Right Power", "", (int)Items.DpadPowers.None, (int)Items.DpadPowers.End-1, 1,
					(double newV) => {Items.addDpadPower((Items.DpadPowers)newV, Items.DpadDir.Right); return ((Items.DpadPowers)newV).ToString();},
					() => {return (double)Items.getDpadPower(Items.DpadDir.Right);}
				);
				foreach (LabMode.MoveDebugPref move in LabMode.movedbg) {
					new BoolIten(lab, move.eName, "", (bool newV) => {move.onChange(newV); return newV.ToString();}, () => {return Sparkipelago.hasItem(move.itemID);});
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
		
		static float lbFrame;
		static float rbFrame;
		
		[HarmonyPatch(typeof(TutorialMenu), "Update")]
		private class TutorialMenuUpdatePatch {
			private static void Postfix(TutorialMenu __instance, int ___Category, int ___Index) {
				bool A = __instance.Inp.Rewinp.GetButtonDown("Jump");
				bool LB = __instance.Inp.Rewinp.GetButtonDown("Parry");
				bool RB = __instance.Inp.Rewinp.GetButtonDown("LockOn");
				ListIten iten = optCategories[___Category].itens[___Index];
				
				if (__instance.Inp.Rewinp.GetButton("Parry")) lbFrame += Time.unscaledDeltaTime;
				else lbFrame = 0;
				if (__instance.Inp.Rewinp.GetButton("LockOn")) rbFrame += Time.unscaledDeltaTime;
				else rbFrame = 0;

				if (lbFrame > 0.5f) {
					LB = true;
					lbFrame -= 0.05f;
				}
				if (rbFrame > 0.5f) {
					RB = true;
					rbFrame -= 0.05f;
				}
				
				if (A) iten.onToggle();
				if (LB || RB) {
					int direction = 0;
					if (LB) direction -= 1;
					if (RB) direction += 1;
					iten.onDirection(direction);
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
					ListIten iten = cat.itens[k];
					iten.onEnable();
					if (iten.component.gameObject.activeSelf) {
						___CurrentItens.Add(iten.component);
					}
				}
				return false;
			}
		}
	}
}