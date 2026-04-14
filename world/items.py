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

@dataclass
class ItemInfo:
	name: str
	type: ItemType

items = [
	ItemInfo(FREEDOM_MEDAL, ItemType.FREEDOM),
	ItemInfo(DASH, ItemType.ABILITY),
	ItemInfo(JESTER_DASH, ItemType.ABILITY),
	ItemInfo(CHARGED_DASH, ItemType.ABILITY),
	ItemInfo(DOWN_DASH, ItemType.ABILITY), # Likely not logically required by anything
	ItemInfo(WALL_JUMP, ItemType.ABILITY),
	ItemInfo(DOUBLE_JUMP, ItemType.ABILITY),
	ItemInfo(COMBAT, ItemType.ABILITY),

	# Filler
	ItemInfo(SCORE_CAPSULE, ItemType.JUNK),
	ItemInfo(HEALTH_CAPSULE, ItemType.JUNK),
	ItemInfo(ENERGY_CAPSULE, ItemType.JUNK),
	ItemInfo(BIT_BUBBLE, ItemType.JUNK),
	ItemInfo(ENERGY_BUBBLE, ItemType.JUNK),

	# Traps
	ItemInfo(NIGHTMARE_TRAP, ItemType.TRAP), # Red Bubble
	ItemInfo(LASER_TRAP, ItemType.TRAP), # Grey Bubble
	ItemInfo(DUST_TRAP, ItemType.TRAP), # Dust cloud, need to figure out

	# Moves
	ItemInfo(SPIN_CHARGE, ItemType.MOVE),
	ItemInfo(DUAL_AIR_KICK, ItemType.MOVE),
	ItemInfo(DUAL_AIR_SLASH, ItemType.MOVE),
	ItemInfo(EXTRA_FINISHER, ItemType.MOVE),
	ItemInfo(SKYWARD_SLASH, ItemType.MOVE),
	ItemInfo(DOUBLE_DOWN_SPIN, ItemType.MOVE),
	ItemInfo(ABRUPT_FINISHER, ItemType.MOVE),
	ItemInfo(DUPLEX_SLASH, ItemType.MOVE),
	
	# Fully possible that certain sanities need moves from here
	# Powers
	ItemInfo(SPEED_BUFF, ItemType.MOVE),
	ItemInfo(HYPER_SURGE, ItemType.MOVE),
	ItemInfo(ENERGY_DASH, ItemType.MOVE),
	ItemInfo(OVERCHARGE, ItemType.MOVE),
	ItemInfo(SNAP_PORTAL, ItemType.MOVE),
	ItemInfo(RADAR_SCOUT, ItemType.MOVE),
	ItemInfo(MULTISHOT_BLAST, ItemType.MOVE),
	ItemInfo(HEAL, ItemType.MOVE),
	ItemInfo(CLOUD_SHOT, ItemType.MOVE),
	ItemInfo(TEMP_SHIELD, ItemType.MOVE),
	
	# Upgrades
	ItemInfo(CHARGED_SHOT, ItemType.MOVE),
	ItemInfo(RAIL_BOOST, ItemType.MOVE),
	ItemInfo(REGEN_BREAKING, ItemType.MOVE),
	ItemInfo(JESTER_SWIPE, ItemType.MOVE),
	
	# Characters
	ItemInfo(REAPER, ItemType.CHARACTER),
	ItemInfo(FLOAT, ItemType.CHARACTER),
	ItemInfo(FARK, ItemType.CHARACTER),
	ItemInfo(SFARX, ItemType.CHARACTER),
	
	ItemInfo(SHOP_MOVES, ItemType.SHOP),
	ItemInfo(SHOP_POWERS, ItemType.SHOP),
	ItemInfo(SHOP_UPGRADES, ItemType.SHOP),
	ItemInfo(SHOP_CHARACTERS, ItemType.SHOP)
]

class ItemState:
	def __init__(self):
		self.ITEM_NAME_TO_ID = {}
		self.ITEM_TO_CLASSIFICATION = {}
		self.FILLER_ITEMS = []
		self.TRAP_ITEMS = []
		self.MOVE_ITEMS = []
		self.PROGRESSION_ITEMS = []
		self.FREEDOM_COUNT = 20
		self.TRAP_CHANCE = 0
	
		idx = ITEM_PREFIX
		for i in items:
			idx += 1
			self.ITEM_NAME_TO_ID[i.name] = idx
			if i.type == ItemType.FREEDOM:
				self.ITEM_TO_CLASSIFICATION[i.name] = ItemClassification.progression
			if i.type in [ItemType.ABILITY, ItemType.SHOP, ItemType.CHARACTER]:
				self.ITEM_TO_CLASSIFICATION[i.name] = ItemClassification.progression
				self.PROGRESSION_ITEMS.append([i.name, i.type])
			if i.type == ItemType.JUNK:
				self.FILLER_ITEMS.append(i.name)
				self.ITEM_TO_CLASSIFICATION[i.name] = ItemClassification.filler
			if i.type == ItemType.TRAP:
				self.TRAP_ITEMS.append(i.name)
				self.ITEM_TO_CLASSIFICATION[i.name] = ItemClassification.trap
			if i.type == ItemType.MOVE:
				self.MOVE_ITEMS.append(i.name)
				self.ITEM_TO_CLASSIFICATION[i.name] = ItemClassification.useful
	
	def get_filler_name(self, world):
		if world.random.randint(0, 99) < self.TRAP_CHANCE:
			return world.random.choice(self.TRAP_ITEMS)
		return world.random.choice(self.FILLER_ITEMS)
	
	def construct_item(self, world, name: str):
		return Spark3Item(name, self.ITEM_TO_CLASSIFICATION[name], self.ITEM_NAME_TO_ID[name], world.player)
	
	def create_items(self, world):
		itempool: list[Item] = []
		precollect = []
		for i in self.PROGRESSION_ITEMS:
			if i[1] == ItemType.ABILITY:
				if not world.ability_rando:
					precollect.append(i[0])
					continue
			if i[1] == ItemType.SHOP:
				if not world.shop_enabled:
					precollect.append(i[0])
					continue
			itempool.append(world.create_item(i[0]))
		for i in range(self.FREEDOM_COUNT):
			itempool.append(world.create_item(FREEDOM_MEDAL))
		for i in self.MOVE_ITEMS:
			itempool.append(world.create_item(i))
		
		num_items = len(itempool)
		unfilled_locs = len(world.multiworld.get_unfilled_locations(world.player))
		num_filler = unfilled_locs - num_items
		for i in range(num_filler):
			itempool.append(world.create_filler())
		
		world.multiworld.itempool += itempool
		
		for i in precollect:
			world.push_precollected(world.create_item(i))

static_item = ItemState()