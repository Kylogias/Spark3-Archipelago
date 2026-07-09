using System.Collections.Generic;
using UnityEngine;

namespace Sparkipelago {
	public struct APStageCheck {
		public string name;
		public string sanity;
		public long id;
		public int index;
		public Vector3 pos;
		
		public APStageCheck(string n, string s, long i, int idx) {
			name = n;
			sanity = s;
			id = i;
			index = idx;
			pos = Vector3.zero;
		}
		
		public APStageCheck(string n, string s, long i, int idx, Vector3 p) {
			name = n;
			sanity = s;
			id = i;
			index = idx;
			pos = p;
		}
	}
	
	public struct APStageData {
		public string name;
		public string type;
		public int id;
		public APStageCheck[] checks;
		
		public APStageData(string n, string t, int i, APStageCheck[] c) {
			name = n;
			type = t;
			id = i;
			checks = c;
		}
	}

	public struct APItem {
		public string name;
		public string type;
		public long id;

		public APItem(string n, string t, long i) {
			name = n;
			type = t;
			id = i;
		}
	}
	
	public struct APShopItem {
		public string name;
		public string page;
		public long id;
		
		public APShopItem(string n, string p, long i) {
			name = n;
			page = p;
			id = i;
		}
	}

	public struct APJson {
		public APJsonStage[] stages;
	}
	public struct APJsonStage {
		public string name;
		public int id;
		public APJsonRegion[] regions;
	}
	public struct APJsonRegion {
		public string name;
		public APJsonCheck[] checks;
	}
	public struct APJsonCheck {
		public string name;
		public int index;
		public string sanity;
	}
}