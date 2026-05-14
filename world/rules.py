from rule_builder.rules import Has, HasAny, HasFromList, And, Or, True_

from .items import Spark3Item
from .constants import *

from enum import Enum

from Utils import visualize_regions

class RuleToken(Enum):
	UNK = 0
	LPAREN  = 1
	RPAREN  = 2
	AND     = 3
	OR      = 4
	
	JESTER_DASH  = 5
	DASH         = 6
	CHARGED_DASH = 7
	DOWN_DASH    = 8
	WALL_JUMP    = 9
	WALL_WALK    = 10
	DOUBLE_JUMP  = 11
	COMBAT       = 12
	
	FARK   = 13
	SFARX  = 14
	FLOAT  = 15
	REAPER = 16

	CAR    = 17
	COPTER = 18
	
	SPEED_BUFF = 19
	HYPER_SURGE = 20
	ENERGY_DASH = 21
	OVERCHARGE = 22
	SNAP_PORTAL = 23
	MULTISHOT_BLAST = 24
	CLOUD_SHOT = 25
	CHARGED_SHOT = 26
	RAIL_BOOST = 27
	REGEN_BREAKING = 28
	JESTER_SWIPE = 29
	ONE_CANCEL = 30
	TWO_CANCEL = 31


STRING_TO_TOKEN = [
	"asdf", "(", ")", "+", "|",
	"jd", "da", "cd", "dd", "wj", "ww", "dj", "co",
	"fa", "sf", "fl", "re", "ca", "pc",
	"sb", "hs", "ed", "oc", "sp", "mb", "cl", "cs", "rb", "br", "js",
	"1c", "2c"
]

class RulesState:
	def __init__(self):
		self.FREEDOM_REQUIREMENTS = [4, 8, 12, 16, 20]
		self.COMPLETION_REQUIREMENTS = [4, 4, 4, 4, 4]
	
	def add_to_rule(self, op, rule, add):
		print(f"{rule} {op} {add}")
		if op == RuleToken.UNK: return add
		if op == RuleToken.AND: return And(rule, add)
		if op == RuleToken.OR: return Or(rule, add)
	
	def recurse_tokens(self, tokens):
		op = RuleToken.UNK
		rule = None
		print("New Rule!")
		while len(tokens):
			token = tokens.pop(0)
			print(f"\t{token}")
			match token:
				case RuleToken.LPAREN: rule = self.add_to_rule(op, rule, self.recurse_tokens(tokens))
				case RuleToken.RPAREN: return rule
				case RuleToken.AND: op = RuleToken.AND
				case RuleToken.OR: op = RuleToken.OR
				case RuleToken.JESTER_DASH: rule = self.add_to_rule(op, rule, Has(JESTER_DASH))
				case RuleToken.DASH: rule = self.add_to_rule(op, rule, Has(DASH))
				case RuleToken.CHARGED_DASH: rule = self.add_to_rule(op, rule, Has(CHARGED_DASH))
				case RuleToken.DOWN_DASH: rule = self.add_to_rule(op, rule, Has(DOWN_DASH))
				case RuleToken.WALL_JUMP: rule = self.add_to_rule(op, rule, Has(WALL_JUMP))
				case RuleToken.WALL_WALK: rule = self.add_to_rule(op, rule, Has(WALL_WALK))
				case RuleToken.DOUBLE_JUMP: rule = self.add_to_rule(op, rule, Has(DOUBLE_JUMP))
				case RuleToken.COMBAT: rule = self.add_to_rule(op, rule, Has(COMBAT))
				case RuleToken.FARK: rule = self.add_to_rule(op, rule, Has(FARK))
				case RuleToken.SFARX: rule = self.add_to_rule(op, rule, Has(SFARX))
				case RuleToken.FLOAT: rule = self.add_to_rule(op, rule, Has(FLOAT))
				case RuleToken.REAPER: rule = self.add_to_rule(op, rule, Has(REAPER))
				case RuleToken.CAR: rule = self.add_to_rule(op, rule, Has(CAR))
				case RuleToken.COPTER: rule = self.add_to_rule(op, rule, Has(COPTER))
				case RuleToken.ONE_CANCEL: rule = self.add_to_rule(op, rule, HasAny(DOUBLE_JUMP, CHARGED_DASH, DASH))
				case RuleToken.TWO_CANCEL: rule = self.add_to_rule(op, rule, HasFromList(DOUBLE_JUMP, CHARGED_DASH, DASH, count=2))
				case RuleToken.SPEED_BUFF: rule = self.add_to_rule(op, rule, Has(SPEED_BUFF))
				case RuleToken.HYPER_SURGE: rule = self.add_to_rule(op, rule, Has(HYPER_SURGE))
				case RuleToken.ENERGY_DASH: rule = self.add_to_rule(op, rule, Has(ENERGY_DASH))
				case RuleToken.OVERCHARGE: rule = self.add_to_rule(op, rule, Has(OVERCHARGE))
				case RuleToken.SNAP_PORTAL: rule = self.add_to_rule(op, rule, Has(SNAP_PORTAL))
				case RuleToken.MULTISHOT_BLAST: rule = self.add_to_rule(op, rule, Has(MULTISHOT_BLAST))
				case RuleToken.CLOUD_SHOT: rule = self.add_to_rule(op, rule, Has(CLOUD_SHOT))
				case RuleToken.CHARGED_SHOT: rule = self.add_to_rule(op, rule, Has(CHARGED_SHOT))
				case RuleToken.RAIL_BOOST: rule = self.add_to_rule(op, rule, Has(RAIL_BOOST))
				case RuleToken.REGEN_BREAKING: rule = self.add_to_rule(op, rule, Has(REGEN_BREAKING))
				case RuleToken.JESTER_SWIPE: rule = self.add_to_rule(op, rule, Has(JESTER_SWIPE))
				
		if rule == None:
			rule = True_()
		print(f"\t{rule}")
		return rule
	
	def parse_location_rules(self, world, rules):
		i = 0
		tokens = []
		rule = rules["base"]
		if world.difficulty in rules:
			rule = rules[world.difficulty]
		print(rule)
		while i < len(rule):
			for tok in range(len(STRING_TO_TOKEN)):
				if rule[i:].startswith(STRING_TO_TOKEN[tok]):
					tokens.append(RuleToken(tok))
					i += len(STRING_TO_TOKEN[tok])
					break
		return self.recurse_tokens(tokens)
	
	def set_shop_rules(self, world):
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_MOVES}"), Has(SHOP_MOVES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_POWERS}"), Has(SHOP_POWERS))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_UPGRADES}"), Has(SHOP_UPGRADES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_CHARACTERS}"), Has(SHOP_CHARACTERS))
	
	def set_stage_rules(self, world):
		for i in range(4):
			gate_entrance = world.get_entrance(f"Gate {i} to Boss")
			has_freedom = HasFromList(count=self.FREEDOM_REQUIREMENTS[i])
			has_freedom.item_names = tuple(sorted(set(tuple(world.item_state.FREEDOM_ITEMS))))
			world.set_rule(gate_entrance, has_freedom & Has("Level Completion", count=self.COMPLETION_REQUIREMENTS[i]))
		utopia_entrance = world.get_entrance(f"Entrance to UTOPIA SHELTER")
		has_freedom = HasFromList(count=self.FREEDOM_REQUIREMENTS[4])
		has_freedom.item_names = tuple(sorted(set(tuple(world.item_state.FREEDOM_ITEMS))))
		world.set_rule(utopia_entrance, has_freedom & Has("Level Completion", count=self.COMPLETION_REQUIREMENTS[4]))
		
		for stage_name in world.location_state.stage_regions.keys():
			stage_region = world.location_state.stage_regions[stage_name]
			stage_data = stage_region[1]
			stage_region = stage_region[0]
			completion_entrance = world.get_entrance(f"{stage_name} GOAL")

			has_coin = False
			for check in stage_data["checks"]:
				if check["sanity"] in world.location_state.sanities or (check["sanity"] == "explore" and world.explore_hunt):
					loc = world.get_location(f"{stage_data['name']} {check['name']}")
					world.set_rule(loc, self.parse_location_rules(world, check['requires']))
					if check["sanity"] == "base":
						rule = self.parse_location_rules(world, check['requires'])
						world.set_rule(completion_entrance, rule)
						if world.coin_hunt == 1 and stage_data["coin_count"]:
							world.set_rule(loc, Has(f"{stage_name} COIN", count=stage_data["coin_req"]))
						if world.coin_hunt == 2 and stage_data["coin_count"]:
							world.set_rule(loc, Has(f"{stage_name} COIN", count=stage_data["coin_count"]))
					if check["sanity"] == "hunt" and world.explore_hunt:
						world.set_rule(loc, Has(f"{stage_name} EXPLORE MEDAL", count=10))

		world.multiworld.completion_condition[world.player] = lambda state: state.has("Victory", world.player)