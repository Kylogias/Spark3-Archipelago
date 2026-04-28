from worlds.AutoWorld import World
from . import items, locations, rules
from . import options as opts
from .constants import GAME_NAME
from .apshared import location_name_to_id as loctoid
from .apshared import apshared

class Spark3World(World):
	game = GAME_NAME
	ut_can_gen_without_yaml = True
	
	location_name_to_id = loctoid
	item_name_to_id = items.static_item.ITEM_NAME_TO_ID

	options_dataclass = opts.Spark3Options
	options: opts.Spark3Options

	origin_region_name = "Gate 0"
	
	def __init__(self, multiworld, player):
		super().__init__(multiworld, player)
		print("Creating a new MultiWorld!")
		self.location_state = locations.LocationState()
		self.item_state = items.ItemState()
		self.rules_state = rules.RulesState()
	
	def generate_early(self):
		if self.options.scoresanity & 1:
			self.location_state.sanities.append("scoregold")
		if self.options.scoresanity & 2:
			self.location_state.sanities.append("scoredia")
		if self.options.speedsanity & 1:
			self.location_state.sanities.append("speedgold")
		if self.options.speedsanity & 2:
			self.location_state.sanities.append("speeddia")
		if self.options.exploresanity:
			self.location_state.sanities.append("explore")
		if self.options.coinsanity:
			self.location_state.sanities.append("coin")
		
		if self.options.shopsanity:
			self.shop_enabled = True
		else:
			self.shop_enabled = False
		
		if self.options.ability_rando:
			self.ability_rando = True
		else:
			self.ability_rando = False
		
		if self.options.spark2_stages:
			self.location_state.SPARK2 = True
			self.location_state.GATE_STAGE_COUNT = [10, 10, 11, 11, 11]
		else:
			self.location_state.SPARK2 = False
			self.location_state.GATE_STAGE_COUNT = [7, 8, 8, 8, 8]
		
		self.item_state.TRAP_CHANCE = self.options.trap_chance.value
		
		self.difficulty = apshared["difficulties"][self.options.difficulty]
		
		if self.options.labmode:
			self.location_state.SPARK2 = True
			self.location_state.bosses = [
				{"name": "LABTIME1", "id": 250, "type": "boss", "checks": []},
				{"name": "LABTIME2", "id": 250, "type": "boss", "checks": []},
				{"name": "LABTIME3", "id": 250, "type": "boss", "checks": []},
				{"name": "LABTIME4", "id": 250, "type": "boss", "checks": []}
			]
			self.ability_rando = False
			self.labbing = True
		else:
			self.labbing = False
		
		self.item_state.FREEDOM_COUNT = self.options.freedom_count.value
		req_freedom = int(self.item_state.FREEDOM_COUNT * (self.options.freedom_required.value * 0.01))
		self.rules_state.FREEDOM_REQUIREMENTS = [int(req_freedom/5), int(2*req_freedom/5), int(3*req_freedom/5), int(4*req_freedom/5), int(req_freedom)]
		
		re_gen_passthrough = getattr(self.multiworld, "re_gen_passthrough", {})
		if re_gen_passthrough and self.game in re_gen_passthrough:
			slot_data = re_gen_passthrough[self.game]
			self.rules_state.FREEDOM_REQUIREMENTS = slot_data["freedom_requirements"]
			self.difficulty = slot_data["difficulty"]
			self.location_state.sanities = slot_data["sanities"]
			self.location_state.gate_data = slot_data["gates"]
			self.location_state.boss_data = slot_data["bosses"]
			self.location_state.regen = True
			self.location_state.SPARK2 = True
			self.shop_enabled = True
	
	def create_regions(self):
		self.location_state.setup_gates(self)
		if self.shop_enabled:
			self.location_state.setup_shop(self)
	
	def create_items(self):
		self.item_state.create_items(self)
	
	def create_item(self, name):
		return self.item_state.construct_item(self, name)
	
	def get_filler_item_name(self):
		return self.item_state.get_filler_name(self)
	
	def set_rules(self):
		self.rules_state.set_stage_rules(self)
		if self.shop_enabled:
			self.rules_state.set_shop_rules(self)
	
	def write_spoiler(self, spoiler_handle):
		spoiler_handle.write(self.location_state.spoiler_text)
	
	def fill_slot_data(self):
		slot_data = {
			"version": apshared["version"],
			"freedom_requirements": self.rules_state.FREEDOM_REQUIREMENTS,
			"labmode": self.options.labmode.value,
			"sanities": self.location_state.sanities,
			"difficulty": self.difficulty,
			"gates": self.location_state.gate_data,
			"bosses": self.location_state.boss_data,
			"musicseed": self.random.randint(0, 2**31),
			"musicchoice": self.options.music_rando.value
		}
		return slot_data
	
	@staticmethod
	def interpret_slot_data(slot_data):
		return slot_data