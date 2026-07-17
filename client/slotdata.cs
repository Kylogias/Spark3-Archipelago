using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Sparkipelago {
	public enum CoinHunt {
		NONE,
		REQUIRE_VANILLA,
		REQUIRE_ALL
	}

	public enum ExploreHunt {
		NONE,
		VANILLA,
		SHUFFLED
	}

	public enum MedalType {
		NONE = 0,
		GOLD_FLAG = 1,
		DIAMOND_FLAG = 2
	}
	
	public enum ProgressionType {
		GATES = 1,
		VANILLA_ER = 2,
		LEVEL = 3
	}

	public enum GoalType {
		Utopia = 0,
		Freom = 1,
		Reqs = 2
	}
	
	public class SlotData {
		public struct Level {
			public int id;
			public float x;
			public float y;
			public Level(int i, float xpos, float ypos) {
				id = i;
				x = xpos;
				y = ypos;
			}
		}
		
		public static int version;
		public static ProgressionType progressionMode;
		public static GoalType goal;
		public static int musicSeed;
		public static int exploreReq;
		public static int[] freedomReq;
		public static int[] completionReq;
		public static int[] speedReq;
		public static int[] scoreReq;
		public static bool labMode;
		public static bool requireCharacters;
		public static bool utopiaMedals;
		public static bool optionalBosses;
		public static CoinHunt coinHunt;
		public static ExploreHunt exploreHunt;
		public static Level[][] gates;
		public static Level[] bosses;
		public static MedalType speedType;
		public static MedalType scoreType;

		private static int[] fillReqArray(JArray arr) {
			int[] reqs = new int[arr.Count];
			int i = 0;
			foreach (JToken freq in arr) {
				reqs[i] = (int)freq;
				i++;
			}
			return reqs;
		}
		
		public SlotData(Dictionary<string, object> data) {
			version = (int)(long)data["version"];
			progressionMode = (ProgressionType)(long)data["progression_mode"];
			goal = (GoalType)(long)data["goal"];
			musicSeed = (int)(long)data["musicseed"];
			exploreReq = (int)(long)data["explore_requirement"];
			freedomReq = fillReqArray((JArray)data["freedom_requirements"]);
			completionReq = fillReqArray((JArray)data["completion_requirements"]);
			speedReq = fillReqArray((JArray)data["speed_requirements"]);
			scoreReq = fillReqArray((JArray)data["score_requirements"]);
			labMode = (long)data["labmode"] != 0;
			requireCharacters = (long)data["require_characters"] != 0;
			utopiaMedals = (long)data["utopia_hunt_medals"] != 0;
			optionalBosses = (long)data["combat_option"] == 2;
			coinHunt = (CoinHunt)(long)data["coin_hunt"];
			exploreHunt = (ExploreHunt)(long)data["explore_hunt"];
			bosses = new Level[((JArray)data["bosses"]).Count()];
			for (int i = 0; i < bosses.Count(); i++) {
				JToken tok = ((JArray)data["bosses"])[i];
				int id = (int)(long)tok[0];
				float x = (float)tok[1];
				float y = (float)tok[2];
				bosses[i] = new Level(id, x, y);
			}
			gates = new Level[((JArray)data["gates"]).Count()][];
			for (int i = 0; i < gates.Count(); i++) {
				JArray slotGate = (JArray)((JArray)data["gates"])[i];
				gates[i] = new Level[slotGate.Count()];
				for (int j = 0; j < slotGate.Count; j++) {
					JToken tok = slotGate[j];
					int id = (int)(long)tok[0];
					float x = (float)tok[1];
					float y = (float)tok[2];
					gates[i][j] = new Level(id, x, y);
				}
			}
			speedType = (MedalType)(long)data["speed_type"];
			scoreType = (MedalType)(long)data["score_type"];
		}

		public SlotData() {
			version = 0;
			musicSeed = 0;
			exploreReq = 0;
			freedomReq = new int[0];
			completionReq = new int[0];
			speedReq = new int[0];
			scoreReq = new int[0];
			labMode = false;
			requireCharacters = false;
			utopiaMedals = false;
			coinHunt = CoinHunt.NONE;
			exploreHunt = ExploreHunt.NONE;
			gates = new Level[0][];
			bosses = new Level[0];
			speedType = MedalType.NONE;
			scoreType = MedalType.NONE;
		}
	}
}