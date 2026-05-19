from dataclasses import dataclass
from BaseClasses import Entrance, Region, Location
from .constants import *
from .items import Spark3Item
from worlds.generic.Rules import CollectionRule
from enum import Enum, IntFlag
from rule_builder.rules import Has, CanReachLocation, True_
import math

from .apshared import apshared, location_name_to_id

class Spark3Location(Location):
	game = GAME_NAME

class LocationState:
	def __init__(self):
		self.SHOP_LOCATIONS = {}
		self.ENDLESS_COUNT = 0
		
		self.GATE_STAGE_COUNT = [10, 10, 11, 11, 11]

		self.SPEED_PERCENT = 0
		self.SCORE_PERCENT = 0
		self.total_score = 0
		self.total_speed = 0
		
		self.UTOPIA_HUNT_MEDALS = False
		
		self.sanities = ["base", "battery"]
		self.stages = []
		self.spark2 = []
		self.bosses = []
		self.utopia = []
		self.stage_regions = {}
	
		# This data is passed to the client. The 5 lists are each gates
		# Each member in the gate list is a list containing 3 elements: Stage ID, X Pos, Y Pos
		self.gate_data = [[], [], [], [], [], []]
		self.regen = False
		self.boss_data = []
	
		self.spoiler_text = ""
		
		for stage in apshared["stages"]:
			match stage["type"]:
				case "stage":
					self.stages.append(stage)
				case "spark2":
					self.spark2.append(stage)
				case "boss":
					self.bosses.append(stage)
				case "utopia":
					self.utopia = stage
		
		self.PAGES = {"Moves": SHOP_MOVES, "Powers": SHOP_POWERS, "Upgrades": SHOP_UPGRADES, "Characters": SHOP_CHARACTERS}
		for item in apshared["shop"]:
			self.add_location_to_id(self.PAGES[item["page"]], self.SHOP_LOCATIONS, item["name"])
	
	def add_location_to_id(self, stage, active, name):
		if stage in active:
			active[stage].append(f"{stage} {name}")
		else:
			active[stage] = [f"{stage} {name}"]
	
	def add_loc_to_dict(self, stage, sanity, locs):
		if stage in sanity:
			location_names = sanity[stage]
			for i in location_names:
				locs[i] = location_name_to_id[i]
	
	def add_stage_to_gate(self, world, gate: Region, stage, event=None):
		region = self.setup_stage_region(world, stage, event)
		self.stage_regions[stage['name']] = [region[0], stage, region[1]]
		if (gate): gate.connect(region[0], f"Entrance to {stage['name']}")
		if not event:
			self.spoiler_text += f"\t{stage['name']}\n"
	
	def setup_stage_region(self, world, stage, event=None):
		stage_region = Region(stage["name"], world.player, world.multiworld)
		completion_region = Region(f"{stage['name']} GOAL", world.player, world.multiworld)
		world.multiworld.regions += [stage_region, completion_region]
		locs = {}
		completion_locs = {}
		stage_region.connect(completion_region, f"{stage['name']} GOAL")
		has_explore = False
		shuffled_locations = []
		all_locations = []
		
		explore_locations = []
		for check in stage["checks"]:
			all_locations.append(check['name'])
			if check["sanity"] in self.sanities:
				if check["sanity"] in completion_sanities:
					completion_locs[f"{stage['name']} {check['name']}"] = location_name_to_id[f"{stage['name']} {check['name']}"]
				else:
					locs[f"{stage['name']} {check['name']}"] = location_name_to_id[f"{stage['name']} {check['name']}"]
				shuffled_locations.append(check['name'])
			if stage["type"] != "boss":
				if check["sanity"] == "speedgold" and world.speed_type & 1: self.total_speed += 1
				if check["sanity"] == "speeddia" and world.speed_type & 2: self.total_speed += 1
				if check["sanity"] == "scoregold" and world.score_type & 1: self.total_score += 1
				if check["sanity"] == "scoredia" and world.score_type & 2: self.total_score += 1
			if check["sanity"] == "explore":
				has_explore = True
				explore_locations.append(f"{stage['name']} {check['name']}")
		if event:
			for loc in completion_locs.keys():
				completion_region.add_event(loc, event, location_type=Spark3Location, item_type=Spark3Item)
		else:
			completion_region.add_locations(completion_locs, Spark3Location)
		if stage["type"] != "boss":
			event_names = ["COMPLETION", "GOLD SPEED MEDAL", "DIAMOND SPEED MEDAL", "GOLD SCORE MEDAL", "DIAMOND SCORE MEDAL"]
			event_items = ["Level Completion", "Gold Speed Medal", "Diamond Speed Medal", "Gold Score Medal", "Diamond Score Medal"]
			for i in range(len(event_names)):
				if not event_names[i] in all_locations: continue
				event = f"{event_names[i]} EVENT" if event_names[i] in shuffled_locations else event_names[i]
				rule = CanReachLocation(f"{stage['name']} {event_names[i]}") if event_names[i] in shuffled_locations else True_()
				completion_region.add_event(
					f"{stage['name']} {event}", event_items[i],
					location_type=Spark3Location, item_type=Spark3Item,
					rule=rule, show_in_spoiler=False
				)
		stage_region.add_locations(locs, Spark3Location)
		if has_explore:
			for xl in explore_locations:
				event_name = f"{xl} EXPLORE EVENT" if "explore" in self.sanities else xl
				stage_region.add_event(
					event_name, f"{stage['name']} EVENT MEDAL",
					location_type=Spark3Location, item_type=Spark3Item,
					rule=CanReachLocation(xl) if "explore" in self.sanities else None, show_in_spoiler=False
				)
				if world.explore_hunt == 1:
					stage_region.add_event(
						f"{xl} HUNT EVENT", f"{stage['name']} EXPLORE MEDAL",
						location_type=Spark3Location, item_type=Spark3Item,
						rule=CanReachLocation(xl), show_in_spoiler=False
					)
			if self.UTOPIA_HUNT_MEDALS:
				completion_region.add_event(
					f"{stage['name']} EXPLORED", "Stage Explored",
					location_type=Spark3Location, item_type=Spark3Item,
					rule=Has(f"{stage['name']} EXPLORE MEDAL", count=10), show_in_spoiler=False
				);
			else:
				rule = True_()
				for xl in explore_locations:
					rule = CanReachLocation(xl) & rule
				completion_region.add_event(
					f"{stage['name']} EXPLORED", "Stage Explored",
					location_type=Spark3Location, item_type=Spark3Item,
					rule=rule, show_in_spoiler=False
				);

		return [stage_region, completion_region]
	
	def setup_shop(self, world):
		shop_pages = [
			Region(SHOP_MOVES, world.player, world.multiworld),
			Region(SHOP_POWERS, world.player, world.multiworld),
			Region(SHOP_UPGRADES, world.player, world.multiworld),
			Region(SHOP_CHARACTERS, world.player, world.multiworld)
		]
		world.multiworld.regions += shop_pages
		
		for page in shop_pages:
			world.get_region("Gate 0").connect(page, f"Gate 0 to {page.name}")
		
		i = 0
		for page in self.PAGES.values():
			locs = {}
			self.add_loc_to_dict(page, self.SHOP_LOCATIONS, locs) # I could refactor, or I could abuse the function :)
			shop_pages[i].add_locations(locs, Spark3Location)
			i += 1
	
	def setup_gates(self, world):
		gates = [
			Region("Gate 0", world.player, world.multiworld),
			Region("Gate 1", world.player, world.multiworld),
			Region("Gate 2", world.player, world.multiworld),
			Region("Gate 3", world.player, world.multiworld),
			Region("Gate 4", world.player, world.multiworld)
		]
		world.multiworld.regions += gates
	#	gates[0].connect(gates[1], "Gate 0 to 1")
	#	gates[1].connect(gates[2], "Gate 1 to 2")
	#	gates[2].connect(gates[3], "Gate 2 to 3")
	#	gates[3].connect(gates[4], "Gate 3 to 4")
		
		dive_locations = {}
		for i in range(self.ENDLESS_COUNT):
			dive_locations[f"ENDLESS DIVE #{i+1}"] = location_name_to_id[f"ENDLESS DIVE #{i+1}"]

		dive_region = Region("ENDLESS DIVE", world.player, world.multiworld)
		world.multiworld.regions += [dive_region]
		gates[0].connect(dive_region, "Gate 0 to Endless Dive", Has("OoB Clip") & Has(COMBAT))
		dive_region.add_locations(dive_locations, Spark3Location)
		
		if world.spark2:
			self.stages += self.spark2
		
		if self.regen:
			old_stages = self.stages.copy()
			old_bosses = self.bosses.copy()
			self.stages.clear()
			self.bosses.clear()
			for gate in range(len(self.gate_data)-1): # last gate is utopia
				print(gate)
				self.GATE_STAGE_COUNT[gate] = 0
				cur_gate = self.gate_data[gate]
				for stage in cur_gate:
					for data in old_stages:
						if data["id"] == stage[0]:
							self.stages.append(data)
							break
					self.GATE_STAGE_COUNT[gate] += 1
			
			for boss in self.boss_data:
				for data in old_bosses:
					if data["id"] == boss[0]:
						self.bosses.append(data)
						break
		elif not world.labbing:
			world.random.shuffle(self.stages)
			world.random.shuffle(self.bosses)
		
		self.spoiler_text += f"\nLevel Gates for {world.multiworld.player_name[world.player]}:\n"
		self.spoiler_text += "Gate 0:\n"
		total = 0
		for i in range(self.GATE_STAGE_COUNT[0]):
			self.gate_data[0].append([self.stages[total]["id"], (i-10)*0.75, 0])
			self.add_stage_to_gate(world, gates[0], self.stages[total])
			total += 1
		if not self.regen
			world.rules_state.SPEED_REQUIREMENTS[0] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[0] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		self.spoiler_text += "Gate 1:\n"
		for i in range(self.GATE_STAGE_COUNT[1]):
			self.gate_data[1].append([self.stages[total]["id"], (i-10)*0.75, 1*0.75])
			self.add_stage_to_gate(world, gates[1], self.stages[total])
			total += 1
		if not self.regen
			world.rules_state.SPEED_REQUIREMENTS[1] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[1] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		self.spoiler_text += "Gate 2:\n"
		for i in range(self.GATE_STAGE_COUNT[2]):
			self.gate_data[2].append([self.stages[total]["id"], (i-10)*0.75, 2*0.75])
			self.add_stage_to_gate(world, gates[2], self.stages[total])
			total += 1
		if not self.regen
			world.rules_state.SPEED_REQUIREMENTS[2] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[2] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		self.spoiler_text += "Gate 3:\n"
		for i in range(self.GATE_STAGE_COUNT[3]):
			self.gate_data[3].append([self.stages[total]["id"], (i-10)*0.75, 3*0.75])
			self.add_stage_to_gate(world, gates[3], self.stages[total])
			total += 1
		if not self.regen
			world.rules_state.SPEED_REQUIREMENTS[3] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[3] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		self.spoiler_text += "Gate 4:\n"
		for i in range(self.GATE_STAGE_COUNT[4]):
			self.gate_data[4].append([self.stages[total]["id"], (i-10)*0.75, 4*0.75])
			self.add_stage_to_gate(world, gates[4], self.stages[total])
			total += 1
		if not self.regen
			world.rules_state.SPEED_REQUIREMENTS[4] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[4] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		
		self.add_stage_to_gate(world, gates[0], self.utopia, event="Victory")
		self.gate_data[5].append([self.utopia["id"], -10*0.75, 5*0.75])
		
		self.spoiler_text += "Boss Order:\n"
		for i in range(4):
			self.add_stage_to_gate(world, gates[i+1], self.bosses[i])
			boss_region = self.stage_regions[self.bosses[i]["name"]][0]
			goal_region = self.stage_regions[self.bosses[i]["name"]][2]
			gates[i].connect(boss_region, f"Gate {i} to Boss")
			goal_region.connect(gates[i+1], f"Boss to Gate {i+1}")
			self.boss_data.append([self.bosses[i]["id"], 1, (i+1)*0.75])
		
	#	world.get_region(STAGE_UTOPIA_SHELTER).add_event("Defeat Claritas Centralis", "Victory", location_type=Spark3Location, item_type=Spark3Item)
