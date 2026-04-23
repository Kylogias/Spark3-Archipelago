from dataclasses import dataclass
from BaseClasses import Entrance, Region, Location
from .constants import *
from .items import Spark3Item
from worlds.generic.Rules import CollectionRule
from enum import Enum, IntFlag

from .apshared import apshared, location_name_to_id

class Spark3Location(Location):
	game = GAME_NAME

class LocationState:
	def __init__(self):
		self.SHOP_LOCATIONS = {}
		
		self.GATE_STAGE_COUNT = [10, 10, 11, 11, 11] # Gate 0 excludes Alpine Carrera in the count
		self.SPARK2 = True
		
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
		region = self.setup_stage_region(world, stage, event)
		self.stage_regions[stage['name']] = [region, stage]
		if (gate): gate.connect(region, f"Entrance to {stage['name']}")
		if not event:
			self.spoiler_text += f"\t{stage['name']}\n"
	
	def setup_stage_region(self, world, stage, event=None):
		region = Region(stage["name"], world.player, world.multiworld)
		world.multiworld.regions += [region]
		locs = {}

		for check in stage["checks"]:
			if check["sanity"] in self.sanities:
				locs[f"{stage['name']} {check['name']}"] = location_name_to_id[f"{stage['name']} {check['name']}"]
		
		if event:
			for loc in locs.keys():
				region.add_event(loc, event, location_type=Spark3Location, item_type=Spark3Item)
		else:
			region.add_locations(locs, Spark3Location)

		return region
	
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
		
		if self.SPARK2:
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
		else:
			world.random.shuffle(self.stages)
			world.random.shuffle(self.bosses)
		
		self.spoiler_text += f"\nLevel Gates for {world.multiworld.player_name[world.player]}:\n"
		self.spoiler_text += "Gate 0:\n"
		total = 0
		for i in range(self.GATE_STAGE_COUNT[0]):
			self.gate_data[0].append([self.stages[total]["id"], (i-10)*0.75, 0])
			self.add_stage_to_gate(world, gates[0], self.stages[total])
			total += 1
		self.spoiler_text += "Gate 1:\n"
		for i in range(self.GATE_STAGE_COUNT[1]):
			self.gate_data[1].append([self.stages[total]["id"], (i-10)*0.75, 1*0.75])
			self.add_stage_to_gate(world, gates[1], self.stages[total])
			total += 1
		self.spoiler_text += "Gate 2:\n"
		for i in range(self.GATE_STAGE_COUNT[2]):
			self.gate_data[2].append([self.stages[total]["id"], (i-10)*0.75, 2*0.75])
			self.add_stage_to_gate(world, gates[2], self.stages[total])
			total += 1
		self.spoiler_text += "Gate 3:\n"
		for i in range(self.GATE_STAGE_COUNT[3]):
			self.gate_data[3].append([self.stages[total]["id"], (i-10)*0.75, 3*0.75])
			self.add_stage_to_gate(world, gates[3], self.stages[total])
			total += 1
		self.spoiler_text += "Gate 4:\n"
		for i in range(self.GATE_STAGE_COUNT[4]):
			self.gate_data[4].append([self.stages[total]["id"], (i-10)*0.75, 4*0.75])
			self.add_stage_to_gate(world, gates[4], self.stages[total])
			total += 1
		
		self.add_stage_to_gate(world, gates[0], self.utopia, event="Victory")
		self.gate_data[5].append([self.utopia["id"], -10*0.75, -5*0.75])
		
		self.spoiler_text += "Boss Order:\n"
		for i in range(4):
			self.add_stage_to_gate(world, gates[i+1], self.bosses[i])
			boss_region = self.stage_regions[self.bosses[i]["name"]][0]
			gates[i].connect(boss_region, f"Gate {i} to Boss")
			boss_region.connect(gates[i+1], f"Boss to Gate {i+1}")
			self.boss_data.append([self.bosses[i]["id"], 1, (i+1)*0.75])
		
	#	world.get_region(STAGE_UTOPIA_SHELTER).add_event("Defeat Claritas Centralis", "Victory", location_type=Spark3Location, item_type=Spark3Item)
