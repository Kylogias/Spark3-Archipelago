from rule_builder.rules import Has, HasAny, HasFromList, And, Or, True_

from .items import Spark3Item
from .constants import *
from .apshared import apshared

from enum import Enum

from Utils import visualize_regions

class RuleToken(Enum):
	UNK = 0
	LPAREN  = 1
	RPAREN  = 2
	AND     = 3
	OR      = 4


STRING_TO_TOKEN = [
	" ", "(", ")", "+", "|"
]

class RulesState:
	def __init__(self):
		self.FREEDOM_REQUIREMENTS = [4, 8, 12, 16, 20]
		self.COMPLETION_REQUIREMENTS = [4, 4, 4, 4, 4]
		self.EXPLORE_REQUIREMENT = 0
		self.SCORE_REQUIREMENTS = [0, 0, 0, 0, 0]
		self.SPEED_REQUIREMENTS = [0, 0, 0, 0, 0]
		self.REQUIRE_CHARACTERS = True
	
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
			if isinstance(token, RuleToken):
				match token:
					case RuleToken.UNK: pass
					case RuleToken.LPAREN: rule = self.add_to_rule(op, rule, self.recurse_tokens(tokens))
					case RuleToken.RPAREN: return rule
					case RuleToken.AND: op = RuleToken.AND
					case RuleToken.OR: op = RuleToken.OR
			else:
				rule = self.add_to_rule(op, rule, Has(token))
				
		if rule == None:
			rule = True_()
		print(f"\t{rule}")
		return rule

	def parse_rule_string(self, rule):
		tokens = []
		i = 0
		while i < len(rule):
			tok_added = False
			for tok in range(len(STRING_TO_TOKEN)):
				if rule[i:].startswith(STRING_TO_TOKEN[tok]):
					tokens.append(RuleToken(tok))
					i += len(STRING_TO_TOKEN[tok])
					tok_added = True
					break
			if not tok_added:
				for item in apshared["items"]:
					if "rule" in item and rule[i:].startswith(item["rule"]):
						tokens.append(item["name"])
						i += len(item["rule"])
						tok_added = True
						break
			if not tok_added:
				for macro in apshared["rule_macros"]:
					if rule[i:].startswith(macro["rule"]):
						tokens += self.parse_rule_string(macro["expansion"])
						i += len(macro["rule"])
						tok_added = True
						break
		return tokens
		
	def parse_location_rules(self, world, rules):
		rule = rules["base"]
		if world.difficulty in rules:
			rule = rules[world.difficulty]
		print(rule)
		return self.recurse_tokens(self.parse_rule_string(rule))
	
	def set_shop_rules(self, world):
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_MOVES}"), Has(SHOP_MOVES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_POWERS}"), Has(SHOP_POWERS))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_UPGRADES}"), Has(SHOP_UPGRADES))
		world.set_rule(world.get_entrance(f"Gate 0 to {SHOP_CHARACTERS}"), Has(SHOP_CHARACTERS))

	def set_gate_entrance_rule(self, world, entrance, i, extra):
		speed_rule = True_()
		if world.speed_type == 1: speed_rule = Has("Gold Speed Medal", count=self.SPEED_REQUIREMENTS[i])
		if world.speed_type == 2: speed_rule = Has("Diamond Speed Medal", count=self.SPEED_REQUIREMENTS[i])
		if world.speed_type == 3: speed_rule = HasFromList("Gold Speed Medal", "Diamond Speed Medal", count=self.SPEED_REQUIREMENTS[i])
		score_rule = True_()
		if world.score_type == 1: score_rule = Has("Gold Score Medal", count=self.SCORE_REQUIREMENTS[i])
		if world.score_type == 2: score_rule = Has("Diamond Score Medal", count=self.SCORE_REQUIREMENTS[i])
		if world.score_type == 3: score_rule = HasFromList("Gold Score Medal", "Diamond Score Medal", count=self.SCORE_REQUIREMENTS[i])
		
		has_freedom = HasFromList(count=self.FREEDOM_REQUIREMENTS[i])
		has_freedom.item_names = tuple(sorted(set(tuple(world.item_state.FREEDOM_ITEMS))))
		world.set_rule(entrance, has_freedom & extra & speed_rule & score_rule & Has("Level Completion", count=self.COMPLETION_REQUIREMENTS[i]))
		
	
	def set_stage_rules(self, world):
		for i in range(4):
			gate_entrance = world.get_entrance(f"Gate {i} to Boss")
			self.set_gate_entrance_rule(world, gate_entrance, i, True_())
		utopia_entrance = world.get_entrance(f"Entrance to UTOPIA SHELTER")
		utopia_rule = Has("Stage Explored", count=self.EXPLORE_REQUIREMENT)
		if (self.REQUIRE_CHARACTERS): utopia_rule = utopia_rule & Has(FARK) & Has(SFARX)
		self.set_gate_entrance_rule(world, utopia_entrance, 4, utopia_rule)
		
		for stage_name in world.location_state.stage_regions.keys():
			stage_region = world.location_state.stage_regions[stage_name]
			stage_data = stage_region[1]
			stage_region = stage_region[0]
			completion_entrance = world.get_entrance(f"{stage_name} GOAL")

			has_coin = False
			for region in stage_data["regions"]:
				entrance = world.get_entrance(f"{stage_data['name']} {region['name']}")
				entrance_rule = self.parse_location_rules(world, region['requires'])
				if region["name"] == "GOAL":
					if stage_data["coin_count"]:
						if world.coin_hunt == 1: entrance_rule = entrance_rule & Has(f"{stage_name} COIN", count=stage_data["coin_req"])
						if world.coin_hunt == 2: entrance_rule = entrance_rule & Has(f"{stage_name} COIN", count=stage_data["coin_count"])
				world.set_rule(entrance, entrance_rule)
				for check in region["checks"]:
					if check["sanity"] in world.location_state.sanities or check["sanity"] in ["explore"] or "event_item" in check:
						loc = world.get_location(f"{stage_data['name']} {check['name']}")
						world.set_rule(loc, self.parse_location_rules(world, check['requires']))
						if check["sanity"] == "hunt" and world.explore_hunt:
							world.set_rule(loc, Has(f"{stage_name} EXPLORE MEDAL", count=10))

		world.multiworld.completion_condition[world.player] = lambda state: state.has("Victory", world.player)