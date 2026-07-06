from dataclasses import dataclass
from enum import Enum
from BaseClasses import Item, ItemClassification


from .constants import *

class Spark3Item(Item):
	game = GAME_NAME

class ItemType(Enum):
	CHARACTER = 1
	JUNK = 2
	TRAP = 3
	MOVE = 4
	FREEDOM = 5
	SHOP = 6
	ABILITY = 7
	EXPLORE3 = 8
	EXPLORE2 = 9
	COIN = 10
	VEHICLE = 11
	POWER = 12
	MULTIPLIER = 13
	GIMMICK = 14
	ENERGY = 15
	LEVEL2 = 16
	LEVEL3 = 17
	ENDLESS = 18
	UNIMPLEMENTED = 19
from .apshared import item_name_to_id, apshared

class ItemState:
	def __init__(self):
		self.ITEM_TO_CLASSIFICATION = {}
		self.FILLER_ITEMS = []
		self.TRAP_ITEMS = []
		self.MOVE_ITEMS = []
		self.PROGRESSION_ITEMS = []
		self.FREEDOM_ITEMS = []
		self.EXPLORE_ITEMS = []
		self.COIN_ITEMS = []
		self.MULTIPLIERS = []
		self.LEVEL_ITEMS = []
		self.FREEDOM_COUNT = 20
		self.TRAP_CHANCE = 0
	
		for i in apshared["items"]:
			if i["type"] == ItemType.FREEDOM:
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.progression
				self.FREEDOM_ITEMS.append(i["name"])
			if i["type"] in [ItemType.ABILITY, ItemType.SHOP, ItemType.GIMMICK, ItemType.CHARACTER, ItemType.VEHICLE, ItemType.POWER, ItemType.ENERGY, ItemType.ENDLESS]:
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.progression
				self.PROGRESSION_ITEMS.append([i["name"], i["type"]])
			if i["type"] in [ItemType.EXPLORE2, ItemType.EXPLORE3]:
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.progression
				self.EXPLORE_ITEMS.append([i["name"], i["type"]])
			if i["type"] in [ItemType.LEVEL2, ItemType.LEVEL3]:
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.progression
				self.LEVEL_ITEMS.append([i["name"], i["type"]])
			if i["type"] == ItemType.COIN:
				self.COIN_ITEMS.append([i["name"], i["count"]])
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.progression
			if i["type"] == ItemType.JUNK:
				self.FILLER_ITEMS.append(i["name"])
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.filler
			if i["type"] == ItemType.TRAP:
				self.TRAP_ITEMS.append(i["name"])
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.trap
			if i["type"] == ItemType.MOVE:
				self.MOVE_ITEMS.append(i["name"])
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.useful
			if i["type"] == ItemType.MULTIPLIER:
				self.ITEM_TO_CLASSIFICATION[i["name"]] = ItemClassification.useful
	
	def get_filler_name(self, world):
		if world.random.randint(0, 99) < self.TRAP_CHANCE:
			return world.random.choice(self.TRAP_ITEMS)
		return world.random.choice(self.FILLER_ITEMS)
	
	def construct_item(self, world, name: str):
		if name == "M-Combat":
			classif = ItemClassification.useful
			name = "Combat"
		else:
			classif = self.ITEM_TO_CLASSIFICATION[name]
		return Spark3Item(name, classif, item_name_to_id[name], world.player)
	
	def create_items(self, world):
		itempool: list[Item] = []
		precollect = []
		for i in self.PROGRESSION_ITEMS:
			if i[1] == ItemType.ABILITY:
				if (i[0] == COMBAT and world.combat_option == 3) or not world.ability_rando:
					precollect.append(i[0])
					continue
			if i[1] == ItemType.SHOP:
				if not world.shop_enabled:
					precollect.append(i[0])
					continue
			if i[1] == ItemType.GIMMICK:
				if not world.gimmick_rando:
					precollect.append(i[0])
					continue
			if i[1] == ItemType.ENDLESS:
				if world.location_state.ENDLESS_COUNT == 0:
					continue
			if not world.labbing:
				itempool.append(world.create_item(i[0]))
			else:
				precollect.append(i[0])
		for i in range(self.FREEDOM_COUNT):
			itempool.append(world.create_item(world.random.choice(self.FREEDOM_ITEMS)))
		for i in self.MOVE_ITEMS:
			if i == "Radar Scout" or not world.combat_moves:
				precollect.append(i)
			else:
				itempool.append(world.create_item(i))
		for i in world.multipliers:
			itempool.append(world.create_item(i))
		if world.coin_hunt:
			for i in self.COIN_ITEMS:
				for j in range(i[1]):
					itempool.append(world.create_item(i[0]))
		if world.explore_hunt == 2:
			for explore in self.EXPLORE_ITEMS:
				include = False
				if world.spark2 and explore[1] == ItemType.EXPLORE2:
					include = True
				if explore[1] == ItemType.EXPLORE3:
					include = True
				if include:
					for i in range(10):
						itempool.append(world.create_item(explore[0]))
		if world.progression_mode == 3:
			starting_stage = world.random.choice(world.location_state.SPHERE_ZERO_STAGES)
			for level in self.LEVEL_ITEMS:
				include = False
				if world.spark2 and level[1] == ItemType.LEVEL2:
					include = True
				if level[1] == ItemType.LEVEL3:
					include = True
				if include:
					if level[0] == f"{starting_stage} Unlocked":
						precollect.append(level[0])
					else:
						itempool.append(world.create_item(level[0]))
		
		num_items = len(itempool)
		unfilled_locs = len(world.multiworld.get_unfilled_locations(world.player))
		num_filler = unfilled_locs - num_items
		for i in range(num_filler):
			itempool.append(world.create_filler())
		
		world.multiworld.itempool += itempool
		
		for i in precollect:
			world.push_precollected(world.create_item(i))
