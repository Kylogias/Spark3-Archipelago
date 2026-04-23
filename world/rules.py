from BaseClasses import CollectionState
from rule_builder.rules import Has, HasAny, And, Or, True_

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
	DOUBLE_JUMP  = 10
	COMBAT       = 11
	
	FARK   = 12
	SFARX  = 13
	FLOAT  = 14
	REAPER = 15
	
	ONE_CANCEL = 16
	TWO_CANCEL = 17

STRING_TO_TOKEN = ["asdf", "(", ")", "+", "|", "jd", "da", "cd", "dd", "wj", "dj", "co", "fa", "sf", "fl", "re", "1c", "2c"]

class RulesState:
	def __init__(self):
		self.FREEDOM_REQUIREMENTS = [4, 8, 12, 16, 20]
	
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
				case RuleToken.DOUBLE_JUMP: rule = self.add_to_rule(op, rule, Has(DOUBLE_JUMP))
				case RuleToken.COMBAT: rule = self.add_to_rule(op, rule, Has(COMBAT))
				case RuleToken.FARK: rule = self.add_to_rule(op, rule, Has(FARK))
				case RuleToken.SFARX: rule = self.add_to_rule(op, rule, Has(SFARX))
				case RuleToken.FLOAT: rule = self.add_to_rule(op, rule, Has(FLOAT))
				case RuleToken.REAPER: rule = self.add_to_rule(op, rule, Has(REAPER))
				case RuleToken.ONE_CANCEL: rule = self.add_to_rule(op, rule, HasAny(DOUBLE_JUMP, CHARGED_DASH, DASH))
				case RuleToken.TWO_CANCEL: rule = self.add_to_rule(op, rule, And(HasAny(DOUBLE_JUMP, CHARGED_DASH), HasAny(CHARGED_DASH, DASH), HasAny(DASH, DOUBLE_JUMP)))
		if rule == None:
			rule = True_()
		print(f"\t{rule}")
		return rule
	
	def parse_location_rules(self, world, location, rules):
		i = 0
		tokens = []
		rule = rules["base"]
		if world.difficulty in rules:
			rule = rules[world.difficulty]
		while i < len(rule):
			for tok in range(len(STRING_TO_TOKEN)):
				if rule[i:].startswith(STRING_TO_TOKEN[tok]):
					tokens.append(RuleToken(tok))
					i += len(STRING_TO_TOKEN[tok])
					break
		world.set_rule(location, self.recurse_tokens(tokens))
	
	def set_shop_rules(self, world):
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_MOVES}"), Has(SHOP_MOVES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_POWERS}"), Has(SHOP_POWERS))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_UPGRADES}"), Has(SHOP_UPGRADES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_CHARACTERS}"), Has(SHOP_CHARACTERS))
	
	def set_stage_rules(self, world):
		for i in range(4):
			gate_entrance = world.get_entrance(f"Gate {i} to Boss")
			world.set_rule(gate_entrance, Has(COMBAT) & Has(FREEDOM_MEDAL, count=self.FREEDOM_REQUIREMENTS[i]))
		utopia_entrance = world.get_entrance(f"Entrance to UTOPIA SHELTER")
		world.set_rule(utopia_entrance, Has(FREEDOM_MEDAL, count=self.FREEDOM_REQUIREMENTS[4]))
		
		for stage_name in world.location_state.stage_regions.keys():
			stage_region = world.location_state.stage_regions[stage_name]
			stage_data = stage_region[1]
			stage_region = stage_region[0]
			
			for check in stage_data["checks"]:
				if check["sanity"] in world.location_state.sanities:
					loc = world.get_location(f"{stage_data['name']} {check['name']}")
					self.parse_location_rules(world, loc, check['requires'])

		world.multiworld.completion_condition[world.player] = lambda state: state.has("Victory", world.player)