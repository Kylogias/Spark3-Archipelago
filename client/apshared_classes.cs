namespace Sparkipelago {
	public struct APStageCheck {
		public string name;
		public string sanity;
		public string requires;
		public long id;
		public int index;
		
		public APStageCheck(string n, string s, string r, long i, int idx) {
			name = n;
			sanity = s;
			requires = r;
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