from dataclasses import dataclass
from BaseClasses import Entrance, Region, Location
from .constants import *
from .items import Spark3Item
from worlds.generic.Rules import CollectionRule
from enum import Enum, IntFlag

@dataclass
class Moves(IntFlag):
	JESTER_DASH = 1
	DASH = 2
	CHARGED_DASH = 4
	DOWN_DASH = 8
	WALL_JUMP = 16
	DOUBLE_JUMP = 32
	COMBAT = 64
	
	ONE_CANCEL = CHARGED_DASH | DOUBLE_JUMP | DASH
	
	FARK = 128
	SFARX = 256

TWO_CANCEL = [Moves.CHARGED_DASH | Moves.DOUBLE_JUMP, Moves.DOUBLE_JUMP | Moves.DASH, Moves.DASH | Moves.CHARGED_DASH]

@dataclass
class StageInfo:
	name: str
	id: int
	explore: list[Moves] | None # Number of Explore Medals
	speed: int # Number of Speed Medals
	score: int # Number of Score Medals
	required_completion: list[Moves]

class ShopPage(Enum):
	MOVES = 0
	POWERS = 1
	UPGRADES = 2
	CHARACTERS = 3

@dataclass
class ShopInfo:
	name: str
	page: ShopPage

class Spark3Location(Location):
	game = GAME_NAME

stages = [
	StageInfo(STAGE_ALPINE_CARRERA, 0, None, 2, 0, [Moves.CHARGED_DASH | Moves.DASH]),
	StageInfo(STAGE_DOUBLEMOON_VILLA, 1, [
		[], [Moves.DASH], [], [], [], [], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP], [], [], [Moves.DASH]
	], 2, 2, []),
	StageInfo(STAGE_HIGH_RISE_TRACKS, 2, [
		[], [], [], [], [], [], [], [], [], []
	], 2, 2, []),
	StageInfo(STAGE_COLD_DRY_DESERT, 3, None, 2, 0, []), # Can easily be completed by going down the center of the track LOL
	StageInfo(STAGE_AM_VILLAGE, 4, None, 0, 0, []),
	StageInfo(STAGE_LOST_RIVIERA, 5, [
		[], [], [], [], [], [], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP], [], [Moves.CHARGED_DASH, Moves.WALL_JUMP, Moves.DASH, Moves.DOUBLE_JUMP], []
	], 2, 2, []),
	StageInfo(STAGE_LOST_RAVINE, 6, [
		[], [], [], [], [], [Moves.COMBAT], [Moves.COMBAT, Moves.WALL_JUMP], [Moves.COMBAT, Moves.WALL_JUMP], [Moves.COMBAT, Moves.WALL_JUMP], []
	], 2, 0, [Moves.WALL_JUMP, Moves.COMBAT, Moves.DOUBLE_JUMP | Moves.DASH | Moves.CHARGED_DASH]),
	StageInfo(STAGE_CANYON_ZERO, 7, [
		[], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP], [], [Moves.WALL_JUMP], [], [Moves.COMBAT], [], [], [], []
	], 0, 2, []), # Raid Stage
	StageInfo(STAGE_BEATDOWN_TOWER, 13, None, 2, 0, [Moves.COMBAT]), # Combat Stage
	StageInfo(STAGE_ARID_HOLE, 11, None, 0, 0, []),
	StageInfo(STAGE_SPLASH_GROTTO, 12, None, 2, 0, []),
	StageInfo(STAGE_DISTRICT_5, 8, [
		[Moves.WALL_JUMP]+TWO_CANCEL, [], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP | Moves.CHARGED_DASH], [], [Moves.ONE_CANCEL], 
		[Moves.WALL_JUMP, Moves.ONE_CANCEL], [], [Moves.DASH | Moves.CHARGED_DASH], [Moves.DASH | Moves.CHARGED_DASH], []
	], 2, 2, []),
	StageInfo(STAGE_DISTRICT_6, 14, [
		[Moves.WALL_JUMP, Moves.ONE_CANCEL], [], [Moves.WALL_JUMP | Moves.DOUBLE_JUMP], [], [],
		[], [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH], [], [], []
	], 2, 2, [Moves.CHARGED_DASH | Moves.DASH]),
	StageInfo(STAGE_DISTRICT_9, 15, [
		[Moves.DASH | Moves.CHARGED_DASH], [Moves.DASH], [], [Moves.CHARGED_DASH], [Moves.DASH, Moves.CHARGED_DASH], [Moves.CHARGED_DASH], [], [Moves.ONE_CANCEL], [Moves.ONE_CANCEL] ,[Moves.ONE_CANCEL]
	], 2, 0, [Moves.DOUBLE_JUMP | Moves.DASH | Moves.CHARGED_DASH]),
	StageInfo(STAGE_DISTRICT_4, 16, None, 2, 0, [Moves.COMBAT]), # Combat Stage
	StageInfo(STAGE_DISTRICT_79, 17, [
		[Moves.DOUBLE_JUMP | Moves.CHARGED_DASH], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT, Moves.DOUBLE_JUMP],
		[Moves.COMBAT, Moves.DOUBLE_JUMP], [Moves.COMBAT, Moves.DOUBLE_JUMP], [], [Moves.COMBAT], [Moves.COMBAT]
	], 2, 2, [Moves.COMBAT, Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_DOWNTOWN_DISSENT, 10, [
		[], [], [Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [Moves.ONE_CANCEL], [Moves.CHARGED_DASH], [], [], [Moves.ONE_CANCEL], [Moves.DASH | Moves.CHARGED_DASH], [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH]
	], 2, 2, [Moves.DOUBLE_JUMP | Moves.WALL_JUMP]),
	StageInfo(STAGE_RESIDENTIAL_RATTLE, 22, [
		[], [], [], [Moves.CHARGED_DASH], [Moves.WALL_JUMP | Moves.DOUBLE_JUMP], [], [], [], [], []
	], 2, 2, []),
	StageInfo(STAGE_ROADWAY_RALLY, 23, None, 2, 0, []), # Car Stage, don't even need to jump
	StageInfo(STAGE_STRIKE_SEWERS, 20, [
		[Moves.WALL_JUMP], [], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP],
		[Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP],
		[Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP]
	], 2, 2, [Moves.WALL_JUMP | Moves.CHARGED_DASH | Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_SQUABBLE_SPILLWAY, 21, None, 0, 0, []),
	StageInfo(STAGE_NIGHTTIME_PHENOMENA, 18, [
		[Moves.WALL_JUMP, Moves.DASH], [], [Moves.DOUBLE_JUMP], [], [Moves.COMBAT],
		[Moves.COMBAT, Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT]
	], 2, 2, [Moves.COMBAT, Moves.DOUBLE_JUMP]), # Truth be told, I forgor where double jump is used. Maybe the one place I got bumped up?
	StageInfo(STAGE_DROPSHIP_DAYBREAK, 27, None, 0, 0, [Moves.COMBAT]), # Collectation Stage, doubt you could replace combat with anything
	StageInfo(STAGE_HEAVEN_PARK, 28, None, 2, 0, [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH]), # Timed Stage, idk if these are actually required
	StageInfo(STAGE_ENDLESS_HALL, 29, [
		[], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT],
		[Moves.COMBAT], [Moves.COMBAT], [Moves.COMBAT], [], []
	], 2, 2, [Moves.COMBAT]),
	StageInfo(STAGE_BALLOON_FIESTA, 30, [
		[], [], [Moves.JESTER_DASH, Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.JESTER_DASH], [Moves.JESTER_DASH],
		[Moves.JESTER_DASH, Moves.WALL_JUMP], [Moves.JESTER_DASH], [Moves.JESTER_DASH], [Moves.JESTER_DASH, Moves.DOUBLE_JUMP, Moves.DASH], [Moves.JESTER_DASH]
	], 2, 0, [Moves.JESTER_DASH, Moves.COMBAT]), # Technically you could substitute Jester Dash for either Wall Jump or Double Jump, but the route's a pain
	StageInfo(STAGE_AIRSTRIP_MADNESS, 25, [
		[], [], [], [], [], [], [], [], [], []
	], 2, 2, []),
	StageInfo(STAGE_TWO_STAGE_LIFTOFF, 33, None, 2, 0, [Moves.JESTER_DASH]), # Theoretically could remove Jester Dash like in BALLOON FIESTA
	StageInfo(STAGE_SUBORBITAL_SCRAMBLE, 34, [
		[Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [], [], [], [], [], [], [], [], [] 
	], 2, 2, [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP]),
	StageInfo(STAGE_AVIATOR_HIGHWAY, 31, None, 2, 0, []),
	StageInfo(STAGE_MAYDAY_MIDDAY, 32, None, 2, 0, [Moves.JESTER_DASH]),
	StageInfo(STAGE_DEEP_DESCENT, 26, [
		[], [Moves.WALL_JUMP, Moves.DASH, Moves.DOUBLE_JUMP], [Moves.JESTER_DASH], [Moves.JESTER_DASH, Moves.ONE_CANCEL], [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP, Moves.ONE_CANCEL],
		[Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [], [Moves.JESTER_DASH, Moves.ONE_CANCEL], []
	], 2, 0, [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.DASH | Moves.CHARGED_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP]),
	StageInfo(STAGE_SLOPE_JUMPING, 100, None, 0, 0, []),
	StageInfo(STAGE_JESTER_DASH, 101, None, 0, 0, [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.DASH | Moves.CHARGED_DASH]),
	StageInfo(STAGE_CHARGED_JESTER_DASH, 102, None, 0, 0, [Moves.CHARGED_DASH, Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_HIGH_SPEEDS, 103, None, 0, 0, [Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_WALL_JUMPING, 104, None, 0, 0, [Moves.WALL_JUMP]),
	StageInfo(STAGE_WALL_WALKING, 105, None, 0, 0, [Moves.WALL_JUMP]),
	StageInfo(STAGE_FALL_DAMAGE, 106, None, 0, 0, [Moves.DOUBLE_JUMP | Moves.DASH, Moves.DASH | Moves.CHARGED_DASH, Moves.CHARGED_DASH | Moves.DOUBLE_JUMP]), # This level requires 2 fall damage resets
	StageInfo(STAGE_FM_CITY, 141, [
		[], [], [Moves.DOUBLE_JUMP], [Moves.DOUBLE_JUMP], [Moves.DOUBLE_JUMP],
		[Moves.DOUBLE_JUMP], [Moves.CHARGED_DASH, Moves.DASH, Moves.DOUBLE_JUMP, Moves.WALL_JUMP, Moves.JESTER_DASH], [Moves.CHARGED_DASH],
		[Moves.CHARGED_DASH, Moves.DOUBLE_JUMP, Moves.DASH], [Moves.CHARGED_DASH]
	], 2, 2, [Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_FM_DOWNTOWN, 142, [
		[], [], [], [], [], [Moves.ONE_CANCEL | Moves.WALL_JUMP], [], [], [], []
	], 2, 2, []),
	StageInfo(STAGE_FLORIA_HIGHWAY, 143, [
		[Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.CHARGED_DASH | Moves.DASH], [Moves.DOUBLE_JUMP], [Moves.DOUBLE_JUMP],
		[Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP], [Moves.CHARGED_DASH, Moves.WALL_JUMP],
		[Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP], [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP],
		[Moves.DOUBLE_JUMP, Moves.DASH, Moves.CHARGED_DASH, Moves.WALL_JUMP], [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP],
		[Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP]
	], 2, 2, [Moves.DOUBLE_JUMP | Moves.CHARGED_DASH, Moves.WALL_JUMP]),
	StageInfo(STAGE_FLORIA_PLANT, 144, [
		[Moves.CHARGED_DASH], [], [Moves.WALL_JUMP, Moves.DASH, Moves.DOUBLE_JUMP, Moves.CHARGED_DASH], [], [Moves.WALL_JUMP]+TWO_CANCEL,
		[Moves.DOUBLE_JUMP | Moves.WALL_JUMP, Moves.DASH, Moves.CHARGED_DASH], [Moves.DOUBLE_JUMP | Moves.WALL_JUMP, Moves.CHARGED_DASH],
		[Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [Moves.DOUBLE_JUMP | Moves.WALL_JUMP, Moves.DASH], [Moves.DOUBLE_JUMP | Moves.WALL_JUMP]
	], 2, 2, [Moves.DOUBLE_JUMP | Moves.WALL_JUMP]),
	StageInfo(STAGE_FLORESTA_BLANCA, 145, [
		[], [], [], [], [Moves.DOUBLE_JUMP], [Moves.DOUBLE_JUMP], [Moves.DOUBLE_JUMP, Moves.WALL_JUMP], [Moves.DOUBLE_JUMP], [], []
	], 2, 2, [Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_CASTELA_BLANCA, 152, None, 2, 2, [Moves.WALL_JUMP, Moves.DASH | Moves.CHARGED_DASH]),
	StageInfo(STAGE_SHANTORIA_TOWN, 146, [
		[Moves.CHARGED_DASH], [Moves.CHARGED_DASH], [], [Moves.CHARGED_DASH, Moves.DOUBLE_JUMP], [Moves.DASH | Moves.CHARGED_DASH],
		[Moves.DOUBLE_JUMP], [], [Moves.CHARGED_DASH, Moves.DASH], [], []
	], 2, 2, []),
	StageInfo(STAGE_TECHNORIA_CITY, 147, [
		[Moves.WALL_JUMP, Moves.ONE_CANCEL], [Moves.JESTER_DASH], [Moves.JESTER_DASH], [Moves.JESTER_DASH], [Moves.JESTER_DASH],
		[Moves.JESTER_DASH], [Moves.JESTER_DASH], [Moves.JESTER_DASH], [Moves.JESTER_DASH, Moves.DOUBLE_JUMP | Moves.WALL_JUMP], [Moves.JESTER_DASH]
	], 2, 2, [Moves.JESTER_DASH]), # You could strafe to the springs manually out of logic
	StageInfo(STAGE_TERMINAL_DRAGON, 148, [
		[], [], [Moves.ONE_CANCEL], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.CHARGED_DASH | Moves.DASH], [Moves.ONE_CANCEL],
		[Moves.ONE_CANCEL], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP], [Moves.ONE_CANCEL], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.CHARGED_DASH, Moves.DASH], [Moves.ONE_CANCEL]
	], 2, 2, [Moves.DOUBLE_JUMP | Moves.DASH | Moves.CHARGED_DASH]),
	StageInfo(STAGE_SCARIA_STROPOLIS, 149, [
		[], [], [], [], [], [Moves.CHARGED_DASH], [], [], [Moves.CHARGED_DASH, Moves.DOUBLE_JUMP, Moves.WALL_JUMP], []
	], 2, 2, []),
	StageInfo(STAGE_TITANIC_TOWER, 150, [
		[], [], [], [], [], [], [], [], [], [] # To be fair, it's very vertical
	], 2, 2, []),
	StageInfo(STAGE_PLANETARY_STRIPE, 151, None, 2, 2, []),
	StageInfo(STAGE_HYPERATH_FLEET, 153, [
		# ok how does the game normally expect #3?
		[Moves.WALL_JUMP, Moves.ONE_CANCEL], [], [Moves.DASH], [Moves.ONE_CANCEL], [Moves.ONE_CANCEL, Moves.WALL_JUMP],
		[Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP | Moves.DOUBLE_JUMP], [Moves.CHARGED_DASH | Moves.DOUBLE_JUMP],
		[Moves.CHARGED_DASH | Moves.DOUBLE_JUMP], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP, Moves.CHARGED_DASH | Moves.DASH]
	], 2, 2, [Moves.CHARGED_DASH | Moves.DOUBLE_JUMP]),
	StageInfo(STAGE_APOCALYPSE_THRUSTER, 154, [
		[Moves.WALL_JUMP, Moves.ONE_CANCEL], [], [Moves.CHARGED_DASH], [], [],
		[Moves.DASH | Moves.CHARGED_DASH], [], [Moves.WALL_JUMP, Moves.DOUBLE_JUMP], [Moves.WALL_JUMP, Moves.ONE_CANCEL], []
	], 2, 2, [])
]

utopia = StageInfo(STAGE_UTOPIA_SHELTER, 50, None, 0, 0, [Moves.COMBAT, Moves.CHARGED_DASH, Moves.WALL_JUMP, Moves.DASH, Moves.DOUBLE_JUMP, Moves.FARK, Moves.SFARX]) # Ending Stage

bosses = [
	StageInfo(BOSS_SAW_MAN, 9, None, 2, 0, [Moves.COMBAT]),
	StageInfo(BOSS_ON_THE_RUN, 24, None, 0, 0, [Moves.COMBAT]),
	StageInfo(BOSS_THROWBACK, 37, None, 2, 0, [Moves.COMBAT]),
	StageInfo(BOSS_MECHA_MADNESS, 38, None, 2, 0, [Moves.COMBAT])
]

shop = [
	ShopInfo(SPIN_CHARGE, ShopPage.MOVES),
	ShopInfo(DUAL_AIR_KICK, ShopPage.MOVES),
	ShopInfo(DUAL_AIR_SLASH, ShopPage.MOVES),
	ShopInfo(EXTRA_FINISHER, ShopPage.MOVES),
	ShopInfo(SKYWARD_SLASH, ShopPage.MOVES),
	ShopInfo(DOUBLE_DOWN_SPIN, ShopPage.MOVES),
	ShopInfo(ABRUPT_FINISHER, ShopPage.MOVES),
	ShopInfo(DUPLEX_SLASH, ShopPage.MOVES),
	ShopInfo(SPEED_BUFF, ShopPage.POWERS),
	ShopInfo(HYPER_SURGE, ShopPage.POWERS),
	ShopInfo(ENERGY_DASH, ShopPage.POWERS),
	ShopInfo(OVERCHARGE, ShopPage.POWERS),
	ShopInfo(SNAP_PORTAL, ShopPage.POWERS),
	ShopInfo(RADAR_SCOUT, ShopPage.POWERS),
	ShopInfo(MULTISHOT_BLAST, ShopPage.POWERS),
	ShopInfo(HEAL, ShopPage.POWERS),
	ShopInfo(CLOUD_SHOT, ShopPage.POWERS),
	ShopInfo(TEMP_SHIELD, ShopPage.POWERS),
	ShopInfo(CHARGED_SHOT, ShopPage.UPGRADES),
	ShopInfo(RAIL_BOOST, ShopPage.UPGRADES),
	ShopInfo(REGEN_BREAKING, ShopPage.UPGRADES),
	ShopInfo(JESTER_SWIPE, ShopPage.UPGRADES),
	ShopInfo(REAPER, ShopPage.CHARACTERS),
	ShopInfo(FLOAT, ShopPage.CHARACTERS),
	ShopInfo(FARK, ShopPage.CHARACTERS),
	ShopInfo(SFARX, ShopPage.CHARACTERS)
]

class LocationState:
	def __init__(self):
		self.LOCATION_NAME_TO_ID = {}
		self.SPEEDSANITY_GOLD_LOCATIONS = {}
		self.SPEEDSANITY_DIA_LOCATIONS = {}
		self.SCORESANITY_GOLD_LOCATIONS = {}
		self.SCORESANITY_DIA_LOCATIONS = {}
		self.EXPLORESANITY_LOCATIONS = {}
		self.SHOP_LOCATIONS = {}
		self.STAGE_LOCATIONS = {}
		self.GATE_STAGE_COUNT = [10, 10, 11, 11, 11] # Gate 0 excludes Alpine Carrera in the count
		self.SPEEDSANITY_GOLD = False
		self.SPEEDSANITY_DIA = False
		self.SCORESANITY_GOLD = False
		self.SCORESANITY_DIA = False
		self.EXPLORESANITY = False
		
		self.stage_regions = {}
	
		# This data is passed to the client. The 5 lists are each gates
		# Each member in the gate list is a list containing 3 elements: Stage ID, X Pos, Y Pos
		self.gate_data = [[], [], [], [], [], []]
		self.boss_data = []
	
		self.spoiler_text = ""
	
		for stage in stages:
			self.add_locations_from_stage(stage)
		for boss in bosses:
			self.add_locations_from_stage(boss)
		self.add_locations_from_stage(utopia)
		
		idx = LOCATION_PREFIX + 6000 # The biggest stage ID the game recognizes is 299, 300*20 = 6000
		self.PAGES = [SHOP_MOVES, SHOP_POWERS, SHOP_UPGRADES, SHOP_CHARACTERS]
		for item in shop:
			self.add_location_to_id(self.PAGES[item.page.value], self.SHOP_LOCATIONS, item.name, idx)
			idx += 1
	
	def add_location_to_id(self, stage, active, name, id):
		self.LOCATION_NAME_TO_ID[f"{stage} {name}"] = id
		if stage in active:
			active[stage].append(f"{stage} {name}")
		else:
			active[stage] = [f"{stage} {name}"]
	
	def add_locations_from_stage(self, stage: StageInfo):
		stagespace = LOCATION_PREFIX + (stage.id * 20)
		self.add_location_to_id(stage.name, self.STAGE_LOCATIONS, "Completion", stagespace)
		if stage.speed:
			self.add_location_to_id(stage.name, self.SPEEDSANITY_GOLD_LOCATIONS, "Gold Speed Medal", stagespace + 1)
			self.add_location_to_id(stage.name, self.SPEEDSANITY_DIA_LOCATIONS, "Diamond Speed Medal", stagespace + 2)
		if stage.score:
			self.add_location_to_id(stage.name, self.SCORESANITY_GOLD_LOCATIONS, "Gold Score Medal", stagespace + 3)
			self.add_location_to_id(stage.name, self.SCORESANITY_DIA_LOCATIONS, "Diamond Score Medal", stagespace + 4)
		if stage.explore:
			for i in range(len(stage.explore)):
				self.add_location_to_id(stage.name, self.EXPLORESANITY_LOCATIONS, f"{MEDAL_NAMES[i]} Exploration Medal", stagespace + 5 + i)
	
	def add_loc_to_dict(self, stage, sanity, locs):
		if stage.name in sanity:
			location_names = sanity[stage.name]
			for i in location_names:
				locs[i] = self.LOCATION_NAME_TO_ID[i]
	
	def add_stage_to_gate(self, world, gate: Region, stage: StageInfo, event=None):
		region = self.setup_stage_region(world, stage, event)
		self.stage_regions[stage.name] = [region, stage]
		if (gate): gate.connect(region, f"Entrance to {stage.name}")
		if not event:
			self.spoiler_text += f"\t{stage.name}\n"
	
	def setup_stage_region(self, world, stage, event=None):
		region = Region(stage.name, world.player, world.multiworld)
		world.multiworld.regions += [region]
		locs = {}

		self.add_loc_to_dict(stage, self.STAGE_LOCATIONS, locs)
		if self.SPEEDSANITY_GOLD:
			self.add_loc_to_dict(stage, self.SPEEDSANITY_GOLD_LOCATIONS, locs)
		if self.SPEEDSANITY_DIA:
			self.add_loc_to_dict(stage, self.SPEEDSANITY_DIA_LOCATIONS, locs)
		if self.SCORESANITY_GOLD:
			self.add_loc_to_dict(stage, self.SCORESANITY_GOLD_LOCATIONS, locs)
		if self.SCORESANITY_DIA:
			self.add_loc_to_dict(stage, self.SCORESANITY_DIA_LOCATIONS, locs)
		if self.EXPLORESANITY:
			self.add_loc_to_dict(stage, self.EXPLORESANITY_LOCATIONS, locs)
		print(locs)
		
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
		for page in self.PAGES:
			locs = {}
			self.add_loc_to_dict(ShopInfo(page, ShopPage.MOVES), self.SHOP_LOCATIONS, locs) # I could refactor, or I could abuse the function :)
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
		
		shuffled_stages = stages.copy()
		shuffled_bosses = bosses.copy()
		world.random.shuffle(shuffled_stages)
		world.random.shuffle(shuffled_bosses)
		
		self.spoiler_text += f"\nLevel Gates for {world.multiworld.player_name[world.player]}:\n"
		self.spoiler_text += "Gate 0:\n"
		total = 0
		for i in range(self.GATE_STAGE_COUNT[0]):
			self.gate_data[0].append([shuffled_stages[total].id, (i-10)*0.75, 0])
			self.add_stage_to_gate(world, gates[0], shuffled_stages[total])
			total += 1
		self.spoiler_text += "Gate 1:\n"
		for i in range(self.GATE_STAGE_COUNT[1]):
			self.gate_data[1].append([shuffled_stages[total].id, (i-10)*0.75, 1*0.75])
			self.add_stage_to_gate(world, gates[1], shuffled_stages[total])
			total += 1
		self.spoiler_text += "Gate 2:\n"
		for i in range(self.GATE_STAGE_COUNT[2]):
			self.gate_data[2].append([shuffled_stages[total].id, (i-10)*0.75, 2*0.75])
			self.add_stage_to_gate(world, gates[2], shuffled_stages[total])
			total += 1
		self.spoiler_text += "Gate 3:\n"
		for i in range(self.GATE_STAGE_COUNT[3]):
			self.gate_data[3].append([shuffled_stages[total].id, (i-10)*0.75, 3*0.75])
			self.add_stage_to_gate(world, gates[3], shuffled_stages[total])
			total += 1
		self.spoiler_text += "Gate 4:\n"
		for i in range(self.GATE_STAGE_COUNT[4]):
			self.gate_data[4].append([shuffled_stages[total].id, (i-10)*0.75, 4*0.75])
			self.add_stage_to_gate(world, gates[4], shuffled_stages[total])
			total += 1
	#	self.spoiler_text += "Utopia:\n"
	#	self.add_stage_to_gate(world, gates[0], utopia)
	#	self.gate_data[5].append([utopia.id, 0, 5])
		
		self.add_stage_to_gate(world, gates[0], utopia, event="Victory")
		self.gate_data[5].append([utopia.id, -10*0.75, -5*0.75])
		
		self.spoiler_text += "Boss Order:\n"
		for i in range(4):
			self.add_stage_to_gate(world, gates[i+1], shuffled_bosses[i])
			boss_region = self.stage_regions[shuffled_bosses[i].name][0]
			gates[i].connect(boss_region, f"Gate {i} to Boss")
			boss_region.connect(gates[i+1], f"Boss to Gate {i+1}")
			self.boss_data.append([shuffled_bosses[i].id, 1, (i+1)*0.75])
		
	#	world.get_region(STAGE_UTOPIA_SHELTER).add_event("Defeat Claritas Centralis", "Victory", location_type=Spark3Location, item_type=Spark3Item)

static_loc = LocationState()