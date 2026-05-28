using System.Collections.Generic;

namespace Sparkipelago {
	public struct APStageCheck {
		public string name;
		public string sanity;
		public long id;
		public int index;
		
		public APStageCheck(string n, string s, long i, int idx) {
			name = n;
			sanity = s;
			id = i;
			index = idx;
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
}