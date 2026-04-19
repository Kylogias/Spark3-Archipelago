from BaseClasses import CollectionState
from worlds.generic.Rules import add_rule, set_rule

from .locations import Moves, stages, bosses, utopia
from .items import Spark3Item
from .constants import *

from Utils import visualize_regions

class RulesState:
	def __init__(self):
		self.FREEDOM_REQUIREMENTS = [4, 8, 12, 16, 20]
	
	def parse_location_rules(self, world, location, rules):
		for rule in rules:
			parsed = []
			if rule & Moves.JESTER_DASH: parsed.append(JESTER_DASH)
			if rule & Moves.DASH: parsed.append(DASH)
			if rule & Moves.CHARGED_DASH: parsed.append(CHARGED_DASH)
			if rule & Moves.DOWN_DASH: parsed.append(DOWN_DASH)
			if rule & Moves.WALL_JUMP: parsed.append(WALL_JUMP)
			if rule & Moves.DOUBLE_JUMP: parsed.append(DOUBLE_JUMP)
			if rule & Moves.COMBAT: parsed.append(COMBAT)
			if rule & Moves.FARK: parsed.append(FARK)
			if rule & Moves.SFARX: parsed.append(SFARX)
			
			add_rule(location, lambda state, r=parsed: state.has_any(r, world.player))
			print(f"\t{parsed}")
	
	def set_shop_rules(self, world):
		set_rule(world.get_entrance(f"Gate 0 to {SHOP_MOVES}"), lambda state: state.has(SHOP_MOVES, world.player))
		set_rule(world.get_entrance(f"Gate 0 to {SHOP_POWERS}"), lambda state: state.has(SHOP_POWERS, world.player))
		set_rule(world.get_entrance(f"Gate 0 to {SHOP_UPGRADES}"), lambda state: state.has(SHOP_UPGRADES, world.player))
		set_rule(world.get_entrance(f"Gate 0 to {SHOP_CHARACTERS}"), lambda state: state.has(SHOP_CHARACTERS, world.player))
	
	def set_stage_rules(self, world):
		for i in range(4):
			gate_entrance = world.get_entrance(f"Gate {i} to Boss")
			set_rule(gate_entrance, lambda state, idx=i: state.has(COMBAT, world.player) and state.has(FREEDOM_MEDAL, world.player, self.FREEDOM_REQUIREMENTS[idx]))
		utopia_entrance = world.get_entrance(f"Entrance to {STAGE_UTOPIA_SHELTER}")
		set_rule(utopia_entrance, lambda state: state.has(FREEDOM_MEDAL, world.player, self.FREEDOM_REQUIREMENTS[4]))
		
		for stage_name in world.location_state.stage_regions.keys():
			stage_region = world.location_state.stage_regions[stage_name]
			stage_data = stage_region[1]
			stage_region = stage_region[0]
			
			print(stage_name)
			for loc in stage_region.get_locations():
				print(loc.name)
			#	if STAGE_UTOPIA_SHELTER in loc.name: continue
				if loc.name.endswith("Completion"): self.parse_location_rules(world, loc, stage_data.required_completion)
				
				# I'm not sure what's required for speed medals so require everything :)
				if loc.name.endswith("Gold Speed Medal"): self.parse_location_rules(world, loc, [Moves.JESTER_DASH, Moves.DASH, Moves.CHARGED_DASH, Moves.DOWN_DASH, Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.COMBAT])
				if loc.name.endswith("Diamond Speed Medal"): self.parse_location_rules(world, loc, [Moves.JESTER_DASH, Moves.DASH, Moves.CHARGED_DASH, Moves.DOWN_DASH, Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.COMBAT])
				
				# Shouldn't need more than combat for score medals
				if loc.name.endswith("Gold Score Medal"): self.parse_location_rules(world, loc, stage_data.required_completion + [Moves.COMBAT])
				if loc.name.endswith("Diamond Score Medal"): self.parse_location_rules(world, loc, stage_data.required_completion + [Moves.COMBAT])
			
				if stage_data.explore:
					for i in range(len(stage_data.explore)):
						if loc.name.endswith(f"{MEDAL_NAMES[i]} Exploration Medal"): self.parse_location_rules(world, loc, stage_data.explore[i])

		world.multiworld.completion_condition[world.player] = lambda state: state.has("Victory", world.player)
	#	raise ValueError