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

		self.SPHERE_ZERO_COUNT = 0
		self.SPHERE_ZERO_STAGES = ["Cold-Dry Desert", "A.M Village", "Arid Hole", "Roadway Rally", "Squabble Spillway", "Aviator Highway"]
		
		self.GATE_STAGE_COUNT = [10, 10, 11, 11, 11]

		self.BOSS_CENTERS = (
			(-1.32, 2.3),
			(3.0, 1.25),
			(4.0, -3.4),
			(2.5, -7.5)
		)
		self.UTOPIA_CENTER = (9.0, -7.75)
		self.GATE_CENTERS = (
			(-3.75, 1.8),
			(1.0, 2.75),
			(5.0, -0.4),
			(1.25, -4.25),
			(5.5, -7.3)
		)
		self.STAGE_DIST = 0.75

		self.SPEED_PERCENT = 0
		self.SCORE_PERCENT = 0
		self.total_score = 0
		self.total_speed = 0
		
		self.UTOPIA_HUNT_MEDALS = False
		
		self.sanities = ["base"]
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
		if stage['name'] in self.stage_regions:
			stage_region = self.stage_regions[stage['name']]
			region = [stage_region[0], stage_region[2]]
		else:
			region = self.setup_stage_region(world, stage, event)
			self.stage_regions[stage['name']] = [region[0], stage, region[1]]
		if (gate): gate.connect(region[0], f"Entrance to {stage['name']}")
		if not event:
			self.spoiler_text += f"\t{stage['name']}\n"
	
	def setup_stage_region(self, world, stage, event=None):
		stage_region = Region(stage["name"], world.player, world.multiworld)
		world.multiworld.regions += [stage_region]
		has_explore = False
		shuffled_locations = []
		
		explore_locations = []
		for region in stage["regions"]:
			new_region = Region(f"{stage['name']} {region['name']}", world.player, world.multiworld)
			if region["name"] == "Start":
				stage_region.connect(new_region, f"{stage['name']} {region['name']}")
			region_locs = {}
			event_locations = []
			world.multiworld.regions += [new_region]
			for check in region["checks"]:
				if "event_item" in check:
					event_locations.append(check)
				if check["sanity"] in self.sanities:
					region_locs[f"{stage['name']} {check['name']}"] = location_name_to_id[f"{stage['name']} {check['name']}"]
					shuffled_locations.append(check['name'])
				if stage["type"] != "boss":
					if check["sanity"] == "speedgold" and world.speed_type & 1: self.total_speed += 1
					if check["sanity"] == "speeddia" and world.speed_type & 2: self.total_speed += 1
					if check["sanity"] == "scoregold" and world.score_type & 1: self.total_score += 1
					if check["sanity"] == "scoredia" and world.score_type & 2: self.total_score += 1
				if check["sanity"] == "explore":
					has_explore = True
					explore_locations.append(f"{stage['name']} {check['name']}")
			if event and region["name"] == "Goal":
				for loc in region_locs.keys():
					new_region.add_event(loc, event, location_type=Spark3Location, item_type=Spark3Item)
			else:
				new_region.add_locations(region_locs)
			if "event" in region:
				event_locations.append({"name": "Access", "sanity": "base", "event_item": region["event"]})
			for event_loc in event_locations:
				event = f"{event_loc['name']} Event" if event_loc['name'] in shuffled_locations else event_loc['name']
				rule = CanReachLocation(f"{stage['name']} {event_loc['name']}") if event_loc['name'] in shuffled_locations else True_()
				new_region.add_event(
					f"{stage['name']} {event}", event_loc['event_item'],
					location_type=Spark3Location, item_type=Spark3Item,
					rule=rule, show_in_spoiler=False
				)
		completion_region = world.get_region(f"{stage['name']} Goal")
		if has_explore:
			for xl in explore_locations:
				event_name = f"{xl} Explore Event" if "explore" in self.sanities else xl
				stage_region.add_event(
					event_name, f"{stage['name']} Event Medal",
					location_type=Spark3Location, item_type=Spark3Item,
					rule=CanReachLocation(xl) if "explore" in self.sanities else None, show_in_spoiler=False
				)
				if world.explore_hunt == 1:
					stage_region.add_event(
						f"{xl} HUNT EVENT", f"{stage['name']} Explore Medal",
						location_type=Spark3Location, item_type=Spark3Item,
						rule=CanReachLocation(xl), show_in_spoiler=False
					)
			if self.UTOPIA_HUNT_MEDALS:
				completion_region.add_event(
					f"{stage['name']} Explored", "Stage Explored",
					location_type=Spark3Location, item_type=Spark3Item,
					rule=Has(f"{stage['name']} Explore Medal", count=10), show_in_spoiler=False
				);
			else:
				rule = True_()
				for xl in explore_locations:
					rule = CanReachLocation(xl) & rule
				completion_region.add_event(
					f"{stage['name']} Explored", "Stage Explored",
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
			world.get_region("World Map").connect(page, f"World Map to {page.name}")
		
		i = 0
		for page in self.PAGES.values():
			locs = {}
			self.add_loc_to_dict(page, self.SHOP_LOCATIONS, locs) # I could refactor, or I could abuse the function :)
			shop_pages[i].add_locations(locs, Spark3Location)
			i += 1

	def position_stages_in_gate(self, world, startidx, gate, gateidx):
		curstage = startidx
		self.spoiler_text += f"Gate {gateidx}:\n"
		slots_taken = []
		radius = 0
		curx = 0
		cury = 0
		for i in range(self.GATE_STAGE_COUNT[gateidx]):
			self.gate_data[gateidx].append([self.stages[curstage]["id"], curx*self.STAGE_DIST + self.GATE_CENTERS[gateidx][0], cury*self.STAGE_DIST + self.GATE_CENTERS[gateidx][1]])
			self.add_stage_to_gate(world, gate, self.stages[curstage])
			slots_taken.append((curx, cury))
			while (curx, cury) in slots_taken:
				curx += 1
				if curx > radius:
					cury += 1
					curx = -radius
				if cury > radius:
					radius += 1
					curx = -radius
					cury = -radius
			curstage += 1
		if not self.regen:
			world.rules_state.SPEED_REQUIREMENTS[gateidx] = int(math.ceil(self.total_speed * self.SPEED_PERCENT))
			world.rules_state.SCORE_REQUIREMENTS[gateidx] = int(math.ceil(self.total_score * self.SCORE_PERCENT))
		return curstage
	
	def setup_locations(self, world):
		worldmap = Region("World Map", world.player, world.multiworld)
		world.multiworld.regions += [worldmap]
		
		dive_locations = {}
		for i in range(self.ENDLESS_COUNT):
			dive_locations[f"Endless Dive #{i+1}"] = location_name_to_id[f"Endless Dive #{i+1}"]

		dive_region = Region("Endless Dive", world.player, world.multiworld)
		world.multiworld.regions += [dive_region]
		worldmap.connect(dive_region, "World Map to Endless Dive", Has("Out Of Bounds") & Has(COMBAT))
		dive_region.add_locations(dive_locations, Spark3Location)
		
		if world.spark2:
			self.stages += self.spark2
		
		for stage in self.stages:
			stage_region = self.setup_stage_region(world, stage)
			self.stage_regions[stage['name']] = [stage_region[0], stage, stage_region[1]]
		for boss in self.bosses:
			stage_region = self.setup_stage_region(world, boss)
			self.stage_regions[boss['name']] = [stage_region[0], boss, stage_region[1]]
		stage_region = self.setup_stage_region(world, self.utopia, event="Victory")
		self.stage_regions[self.utopia['name']] = [stage_region[0], self.utopia, stage_region[1]]
		
			
		if (world.progression_mode == 1): self.setup_gates(world, worldmap)
		if (world.progression_mode == 2): self.setup_gates(world, worldmap)
		if (world.progression_mode == 3): self.setup_level_unlocks(world, worldmap)

	def setup_gates(self, world, worldmap):
		gates = [
			Region("Gate 0", world.player, world.multiworld),
			Region("Gate 1", world.player, world.multiworld),
			Region("Gate 2", world.player, world.multiworld),
			Region("Gate 3", world.player, world.multiworld),
			Region("Gate 4", world.player, world.multiworld)
		]
		world.multiworld.regions += gates
		worldmap.connect(gates[0], "World Map to Gate 0")
		if self.regen:
			old_stages = self.stages.copy()
			old_bosses = self.bosses.copy()
			self.stages.clear()
			self.bosses.clear()
			for gate in range(len(self.gate_data)-1): # last gate is utopia
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
			world.random.shuffle(self.SPHERE_ZERO_STAGES)
			s0 = []
			names = self.SPHERE_ZERO_STAGES[:self.SPHERE_ZERO_COUNT]
			for stage in self.stages.copy():
				if stage["name"] in names:
					s0.append(stage)
					self.stages.remove(stage)
			world.random.shuffle(self.stages)
			world.random.shuffle(self.bosses)
			self.stages = s0 + self.stages
		
		self.spoiler_text += f"\nLevel Gates for {world.multiworld.player_name[world.player]}:\n"
		total = 0
		for i in range(5):
			total = self.position_stages_in_gate(world, total, gates[i], i)
		
		self.add_stage_to_gate(world, gates[0], self.utopia, event="Victory")
		self.gate_data[5].append([self.utopia["id"], self.UTOPIA_CENTER[0], self.UTOPIA_CENTER[1]])
		
		self.spoiler_text += "Boss Order:\n"
		for i in range(4):
			self.add_stage_to_gate(world, gates[i+1], self.bosses[i])
			boss_region = self.stage_regions[self.bosses[i]["name"]][0]
			goal_region = self.stage_regions[self.bosses[i]["name"]][2]
			gates[i].connect(boss_region, f"Gate {i} to Boss")
			if world.combat_option == 2: boss_region.connect(gates[i+1], f"Boss to Gate {i+1}")
			else: goal_region.connect(gates[i+1], f"Boss to Gate {i+1}")
			self.boss_data.append([self.bosses[i]["id"], self.BOSS_CENTERS[i][0], self.BOSS_CENTERS[i][1]])
	
	def setup_level_unlocks(self, world, worldmap):
		for stage_name in self.stage_regions:
			stage_region = self.stage_regions[stage_name][0]
			worldmap.connect(stage_region, f"World Map to {stage_name}", Has(f"{stage_name} Unlocked"))
