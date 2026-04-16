from worlds.AutoWorld import World
from . import items, locations, rules
from . import options as opts
from .constants import GAME_NAME

class Spark3World(World):
	game = GAME_NAME
	
	location_name_to_id = locations.static_loc.LOCATION_NAME_TO_ID
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
		
	def create_regions(self):
		if self.options.scoresanity & 1:
			self.location_state.SCORESANITY_GOLD = True
		if self.options.scoresanity & 2:
			self.location_state.SCORESANITY_DIA = True
		if self.options.speedsanity & 1:
			self.location_state.SPEEDSANITY_GOLD = True
		if self.options.speedsanity & 2:
			self.location_state.SPEEDSANITY_DIA = True
		if self.options.exploresanity:
			self.location_state.EXPLORESANITY = True
		
		if self.options.shopsanity:
			self.shop_enabled = True
		else:
			self.shop_enabled = False
		
		if self.options.ability_rando:
			self.ability_rando = True
		else:
			self.ability_rando = False
		
		self.item_state.FREEDOM_COUNT = self.options.freedom_count.value
		req_freedom = int(self.item_state.FREEDOM_COUNT * (self.options.freedom_required.value * 0.01))
		self.rules_state.FREEDOM_REQUIREMENTS = [int(req_freedom/5), int(2*req_freedom/5), int(3*req_freedom/5), int(4*req_freedom/5), int(req_freedom)]
		print(self.rules_state.FREEDOM_REQUIREMENTS)
		print(self.item_state.FREEDOM_COUNT)
		
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
			"version": 0,
			"freedom_requirements": self.rules_state.FREEDOM_REQUIREMENTS,
			"gates": self.location_state.gate_data,
			"bosses": self.location_state.boss_data,
			"musicseed": self.random.randint(0, 2**31),
			"musicchoice": self.options.music_rando.value
		}
		return slot_data