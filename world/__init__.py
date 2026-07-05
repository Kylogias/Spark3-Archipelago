from worlds.AutoWorld import World, WebWorld
from . import items, locations, rules
from . import options as opts
from .constants import GAME_NAME, COMBAT
from .apshared import location_name_to_id as loctoid
from .apshared import item_name_to_id as itemtoid
from .apshared import apshared

import math

class Spark3WebWorld(WebWorld):
	game = GAME_NAME
	option_groups = opts.option_groups

class Spark3World(World):
	game = GAME_NAME
	ut_can_gen_without_yaml = True
	web = Spark3WebWorld()
	
	location_name_to_id = loctoid
	item_name_to_id = itemtoid

	options_dataclass = opts.Spark3Options
	options: opts.Spark3Options

	origin_region_name = "Gate 0"
	
	def __init__(self, multiworld, player):
		super().__init__(multiworld, player)
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
		if self.options.batterysanity:
			self.location_state.sanities.append("battery")
		if self.options.checkpointsanity:
			self.location_state.sanities.append("checkpoint")
		if self.options.coinsanity == 2:
			self.coin_hunt = 1
		elif self.options.coinsanity == 3:
			self.coin_hunt = 2
		else:
			self.coin_hunt = 0;
		
		if self.options.shopsanity:
			self.shop_enabled = True
		else:
			self.shop_enabled = False
		
		if self.options.ability_rando:
			self.ability_rando = True
		else:
			self.ability_rando = False

		if self.options.gimmick_rando:
			self.gimmick_rando = True
		else:
			self.gimmick_rando = False

		self.combat_option = self.options.combat_option.value
		if self.options.combat_option == 1:
			self.multiworld.local_early_items[self.player][COMBAT] = 1
		if self.options.combat_moves:
			self.combat_moves = True
		else:
			self.combat_moves = False
		
		self.multipliers = []
		for i in range(self.options.progressive_combo.value-1):
			self.multipliers.append("M-Combat")
		for i in range(self.options.progressive_score.value):
			self.multipliers.append("Progressive Score")
		for i in range(self.options.progressive_timestop.value):
			self.multipliers.append("Progressive Time Stop")
		
		if self.options.spark2_stages:
			self.spark2 = True
			self.location_state.GATE_STAGE_COUNT = [10, 10, 11, 11, 11]
		else:
			self.spark2 = False
			self.location_state.GATE_STAGE_COUNT = [7, 8, 8, 8, 8]

		self.explore_hunt = self.options.explore_hunt.value
		if self.explore_hunt:
			self.location_state.sanities.append("hunt")
		
		self.item_state.TRAP_CHANCE = self.options.trap_chance.value
		
		self.difficulty = apshared["difficulties"][self.options.difficulty]
		
		if self.options.labmode:
			self.spark2 = True
			self.ability_rando = False
			self.gimmick_rando = False
			self.labbing = True
		else:
			self.labbing = False
		
		self.item_state.FREEDOM_COUNT = self.options.freedom_count.value
		req_freedom = int(self.item_state.FREEDOM_COUNT * (self.options.freedom_required.value * 0.01))
		self.rules_state.FREEDOM_REQUIREMENTS = [int(req_freedom/5), int(2*req_freedom/5), int(3*req_freedom/5), int(4*req_freedom/5), int(req_freedom)]
		self.rules_state.REQUIRE_CHARACTERS = self.options.require_characters.value
		self.rules_state.ENERGY_LOGIC = self.options.energy_logic.value
		self.rules_state.CHARACTER_LOGIC = self.options.character_logic.value

		self.location_state.UTOPIA_HUNT_MEDALS = self.options.utopia_hunt_medals.value
		self.rules_state.EXPLORE_REQUIREMENT = int(math.ceil((self.options.required_explore.value*0.01) * (30 if self.spark2 else 18)))
		
		self.location_state.SPEED_PERCENT = self.options.required_speed.value * 0.01
		self.location_state.SCORE_PERCENT = self.options.required_score.value * 0.01
		self.speed_type = self.options.speed_type.value
		self.score_type = self.options.score_type.value
		
		self.location_state.ENDLESS_COUNT = self.options.endless_dive_checks.value

		self.rules_state.COMPLETION_REQUIREMENTS = []
		completion_total = 0
		for i in range(5):
			cur = int(math.ceil(self.location_state.GATE_STAGE_COUNT[i] * (self.options.required_completion.value * 0.01)))
			completion_total += cur
			self.rules_state.COMPLETION_REQUIREMENTS.append(completion_total)
		
		re_gen_passthrough = getattr(self.multiworld, "re_gen_passthrough", {})
		if re_gen_passthrough and self.game in re_gen_passthrough:
			slot_data = re_gen_passthrough[self.game]
			self.rules_state.FREEDOM_REQUIREMENTS = slot_data["freedom_requirements"]
			self.rules_state.COMPLETION_REQUIREMENTS = slot_data["completion_requirements"]
			self.rules_state.EXPLORE_REQUIREMENT = slot_data["explore_requirement"]
			self.rules_state.SPEED_REQUIREMENTS = slot_data["speed_requirements"]
			self.rules_state.SCORE_REQUIREMENTS = slot_data["score_requirements"]
			self.rules_state.REQUIRE_CHARACTERS = slot_data["require_characters"]
			self.rules_state.ENERGY_LOGIC = slot_data["energy_logic"]
			self.rules_state.CHARACTER_LOGIC = slot_data["character_logic"]
			self.speed_type = slot_data["speed_type"]
			self.score_type = slot_data["score_type"]
			self.combat_option = slot_data["combat_option"]
			self.location_state.UTOPIA_HUNT_MEDALS = slot_data["utopia_hunt_medals"]
			self.difficulty = slot_data["difficulty"]
			self.location_state.sanities = slot_data["sanities"]
			self.location_state.gate_data = slot_data["gates"]
			self.location_state.boss_data = slot_data["bosses"]
			self.location_state.ENDLESS_COUNT = slot_data["endless_checks"]
			self.location_state.regen = True
			self.spark2 = True
			self.shop_enabled = True
			self.explore_hunt = slot_data["explore_hunt"]
			self.coin_hunt = slot_data["coin_hunt"]
			return

		location_count = 43 + self.location_state.ENDLESS_COUNT
		if self.spark2: location_count += 14
		if self.shop_enabled: location_count += 26
		if self.options.scoresanity & 1: location_count += 28 if self.spark2 else 14
		if self.options.scoresanity & 2: location_count += 28 if self.spark2 else 14
		if self.options.speedsanity & 1: location_count += 44 if self.spark2 else 30
		if self.options.speedsanity & 2: location_count += 44 if self.spark2 else 30
		if self.options.exploresanity: location_count += 300 if self.spark2 else 180
		if self.options.coinsanity: location_count += 72
		if self.options.batterysanity: location_count += 13
		if self.options.checkpointsanity: location_count += 355 if self.spark2 else 237
		if self.explore_hunt: location_count += 30 if self.spark2 else 18

		reserved_items = 20 + len(self.multipliers)
		if self.combat_moves: reserved_items += 8
		if self.ability_rando: reserved_items += 9
		if self.gimmick_rando: reserved_items += 11
		if self.explore_hunt == 2: reserved_items += 300 if self.spark2 else 180
		if self.coin_hunt: reserved_items += 72
		if reserved_items > location_count:
			raise ValueError(f"Too many items in the pool! {reserved_items} items and {location_count} locations")
		if self.options.freedom_count.value > location_count - reserved_items:
			raise ValueError(f"Too many freedom medals in pool (have {self.options.freedom_count.value}, max {location_count - reserved_items})")
	
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
			"completion_requirements": self.rules_state.COMPLETION_REQUIREMENTS,
			"speed_requirements": self.rules_state.SPEED_REQUIREMENTS,
			"score_requirements": self.rules_state.SCORE_REQUIREMENTS,
			"require_characters": self.rules_state.REQUIRE_CHARACTERS,
			"energy_logic": self.rules_state.ENERGY_LOGIC,
			"character_logic": self.rules_state.CHARACTER_LOGIC,
			"speed_type": self.speed_type,
			"score_type": self.score_type,
			"explore_requirement": self.rules_state.EXPLORE_REQUIREMENT,
			"utopia_hunt_medals": self.location_state.UTOPIA_HUNT_MEDALS,
			"endless_checks": self.location_state.ENDLESS_COUNT,
			"labmode": self.options.labmode.value,
			"sanities": self.location_state.sanities,
			"explore_hunt": self.explore_hunt,
			"coin_hunt": self.coin_hunt,
			"combat_option": self.combat_option,
			"difficulty": self.difficulty,
			"gates": self.location_state.gate_data,
			"bosses": self.location_state.boss_data,
			"musicseed": self.random.randint(0, 2**31),
		}
		return slot_data
	
	@staticmethod
	def interpret_slot_data(slot_data):
		return slot_data