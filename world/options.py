from dataclasses import dataclass
from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle, DefaultOnToggle, NamedRange, OptionSet

from .apshared import apshared

class Shopsanity(Toggle):
	"""
	Adds the 26 shop checks to the location pool
	"""
	display_name = "ShopSanity"

class Difficulty(Choice):
	"""
	How difficult should the logic be
	WARNING: At the moment the difficulty is all over the place.
	If you have a suggestion or a question, please ask in the future game design thread

	normal: The easiest option, designed for those new to the game
	hard: You will need to be observant of the stage layout and know how to best utilize Spark's movement and simple tech
	expert: Requires knowledge for hard difficulty as well as less simple speedrun tricks. All tricks are fair game
	"""

	option_base = 0
	option_hard = 1
	option_expert = 2
	default = option_hard

class Spark2Stages(DefaultOnToggle):
	"""
	Adds the 14 Spark 2 stages
	"""
	
	display_name = "Spark 2 Stages"

class AbilityRando(DefaultOnToggle):
	"""
	Randomize the abilities Spark normally can use into the item pool
		(Double Jump, Wall Jump/Walk, Dash, Charge Dash, Down Dash, Jester Dash, Combat)
	"""
	display_name = "Randomize Abilities"

class GimmickRando(DefaultOnToggle):
	"""
	Puts the stage gimmicks into the item pool
	"""
	display_name = "Randomize Gimmicks"

class EnergyLogic(Toggle):
	"""
	Should energy-requiring moves be relevant in logic? You may be required to restart the stage multiple times
	"""
	display_name = "Energy Logic"

class CharacterLogic(DefaultOnToggle):
	"""
	Should characters be relevant in logic? Does not affect the unlock requirements for Utopia Shelter
	You may still be required to use energy with Fark
	"""
	display_name = "Character Logic"

class ProgressiveCombo(Range):
	"""
	How many combat items should be in the pool?
	These items are progressive, first one provides the ability to fight
	and subsequent items give you permanent combo (adjustable in client settings)
	"""
	display_name = "Progressive Combat"

	range_start = 1
	range_end = 11

	default = 1

class ProgressiveScore(Range):
	"""
	How many score multiplier options should be in the pool?
	These items are progressive and can be adjusted in client settings
	"""
	display_name = "Progressive Score"

	range_start = 0
	range_end = 10

	default = 0

class ProgressiveEnergy(Range):
	"""
	How many progressive energy items should be in the pool?
	Each item provides a configurable amount of energy per second
	while on the ground, configurable in the client
	"""
	display_name = "Progressive Energy"

	range_start = 0
	range_end = 10

	default = 0

class ProgressiveTimestop(Range):
	"""
	How many timestop items should be in the pool?
	These items slow down the in-game timer, making the speed medals easier to obtain
	These items are progressive and can be adjusted in client settings
	"""
	display_name = "Progressive Time Stop"

	range_start = 0
	range_end = 10

	default = 0

class LabMode(Toggle):
	"""
	Overrides the ability randomizer and automatically opens all stages, for labbing checks.
	WARNING: For development purposes only
	"""
	display_name = "Labbing Mode"

class Speedsanity(Choice):
	"""
	Adds the 30 gold and/or 30 diamond speed medals to the location pool. (44 each with Spark 2 stages)
	WARNING: Logic Not Implemented
	"""
	display_name = "SpeedSanity"
	
	option_none = 0
	option_gold = 1
	option_diamond = 2
	option_both = 3
	default = option_none

class Scoresanity(Choice):
	"""
	Adds the 14 gold and/or 14 diamond score medals to the location pool. (28 each with Spark 2 stages)
	"""
	display_name = "ScoreSanity"
	
	option_none = 0
	option_gold = 1
	option_diamond = 2
	option_both = 3
	default = option_none

class Exploresanity(Toggle):
	"""
	Adds the 180 exploration medals as checks (300 with Spark 2 stages)
	"""
	display_name = "ExploreSanity"

class Checkpointsanity(Toggle):
	"""
	Adds the 237 checkpoints as checks (326 with Spark 2 stages)
	"""
	display_name = "CheckpointSanity"

class Coinsanity(Choice):
	"""
	Adds the 72 collectable coins in the collectathon stages as checks and optionally put them in the item pool
	"""
	display_name = "CoinSanity"

	option_off = 0
	option_on = 1
	option_shuffle_coins = 2
	option_shuffle_and_require_all = 3
	default = option_off

class Batterysanity(Toggle):
	"""
	Adds the 13 batteries in car stages as checks (making the car not useless)
	"""
	display_name = "BatterySanity"

class FreedomCount(Range):
	"""
	How many freedom medals to put into the item pool.
	"""
	display_name = "Freedom Medal Count"

	range_start = 0
	range_end = 100
	
	default = 0

class FreedomRequired(Range):
	"""
	What's the required percentage of freedom medals to unlock Utopia Shelter
	"""
	display_name = "Required Freedom Medal Percentage"
	
	range_start = 0
	range_end = 100
	
	default = 0

class RequiredCompletion(Range):
	"""
	What percentage of levels in a gate (rounding up) should be completed before unlocking the next?
	"""
	display_name = "Required Level Percentage"

	range_start = 0
	range_end = 100

	default = 40

class RequiredSpeed(Range):
	"""
	What percentage of speed medals should be required per level gate
	"""
	display_name = "Required Speed Medal Percentage"

	range_start = 0
	range_end = 100

	default = 0

class RequiredScore(Range):
	"""
	What percentage of score medals should be required per level gate
	"""
	display_name = "Required Score Medal Percentage"

	range_start = 0
	range_end = 100

	default = 0

class SpeedRequiredType(Choice):
	"""
	Should the required speed medals be gold, diamond or both
	"""
	display_name = "Required Speed Medal Type"

	option_gold = 1
	option_diamond = 2
	option_both = 3

	default = option_gold

class ScoreRequiredType(Choice):
	"""
	Should the required score medals be gold, diamond or both
	"""
	display_name = "Required Score Medal Type"

	option_gold = 1
	option_diamond = 2
	option_both = 3

	default = option_gold

class RequiredExplore(Range):
	"""
	What percentage of stages should be explored to unlock Utopia Shelter
	"""
	display_name = "Required Explored Levels"

	range_start = 0
	range_end = 100

	default = 0

class ExplorePercentIsHunt(Toggle):
	"""
	Should the exploration percentage use Explore Hunt medals?
	"""
	display_name = "Explore Hunt Medals for Utopia"

class RequireCharacters(DefaultOnToggle):
	"""
	Should Fark and Sfarx be required to unlock Utopia Shelter?
	"""
	display_name = "Require Characters for Utopia"

class TrapChance(Range):
	"""
	Chance to receive traps instead of filler
	"""
	display_name = "Trap Chance"

	range_start = 0
	range_end = 100
	
	default = 0

class ExploreHunt(Choice):
	"""
	Add a location to each stage for collecting all explore medals
	"""
	display_name = "Explore Hunt"

	option_off = 0
	option_locations_only = 1
	option_shuffle_medals = 2

	default = option_off

class EndlessDiveChecks(Range):
	"""
	How many checks should be in Endless Dive
	"""
	display_name = "Endless Dive Check Count"

	range_start = 0
	range_end = 110

	default = 0

class CombatOptions(Choice):
	"""
	Under the normal gating mode, Combat hard locks anything past the first gate.
	This means you are at the whim of the randomizer to progress past Gate 0
	These options can help you progress quicker

	Normal: Bosses must be defeated, Combat is placed purely at the randomizer's whim
	Local Early: Guarantees the required combat item is in this game and early sphere
	Optional Bosses: You do not need to defeat bosses in order to unlock the next gate
	Start: Start with combat, takes it out of the item pool
	"""
	display_name = "Combat Option"
	
	option_normal = 0
	option_local_early = 1
	option_optional_bosses = 2
	option_start = 3
	
	default = option_normal

class CombatMoves(DefaultOnToggle):
	"""
	Should the 8 combat moves be in the item pool?
	"""
	display_name = "Combat Moves"

class GuaranteedCompletions(Range):
	"""
	How many stages from the Sphere 0 list should be in the first gate? Increasing this can help with generation errors
	"""
	display_name = "Gate 0 Count"

	range_start = 0
	range_end = 100

	default = 2

class ProgressionMode(Choice):
	"""
	What should be the mode of progression for this player?

	Gates: Similar to Sonic Adventure 2 AP, levels are placed in gates with very configurable requirements to move to the next gate
	Vanilla Entrance Randomizer: The world map is unlocked like in vanilla Spark 3
	Level Items: You must find levels in the stages in order to progress. Levels are in their vanilla locations
	Open World: All levels are unlocked from the start, effectively Gates but with only Gate 0
		- The goal level is the only exception. Progression Options denote the requirements
	"""
	display_name = "Progression Mode"

	option_gates = 1
#	option_vanilla_entrance_randomizer = 2
	option_level_items = 3
#	option_open_world = 4

	default = option_gates

class StartingLocations(OptionSet):
	"""
	Which locations are you guaranteed to start with?
	If a goal stage is not in this list and has checks/goal, it will be put into the item pool
	If progression is set to Gates, these locations are guaranteed to always be accessible
	"""
	display_name = "Starting Locations"
	valid_keys = [stage['name'] for stage in apshared["stages"]]
	default = ["Utopia Shelter"]

class SphereZeroList(OptionSet):
	"""
	What is the list of stages that may be used for starting level with Level Items or guaranteed gate 0 stages with Gates
	"""
	display_name = "Sphere 0 List"
	valid_keys = [stage['name'] for stage in apshared["stages"]]
	default = ["Cold-Dry Desert", "A.M Village", "Arid Hole", "Roadway Rally", "Squabble Spillway", "Aviator Highway"]

class GoalLocation(Choice):
	"""
	What should the goal stage be? Goals are in their vanilla locations
	Utopia Shelter: Defeat the final boss within Utopia Shelter to goal
	Freom Mk-0: Defeat the secret Freom boss to goal
	Requirements Only: Goal automatically sends upon meeting the progression requirements and going to the World Map
	"""
	display_name = "Goal Location"

	option_utopia_shelter = 0
	option_freom_mk0 = 1
	option_requirements_only = 2

	default = option_utopia_shelter

class GoalChecks(OptionSet):
	"""
	Which goal locations should have checks inside them? If the current goal location is included, the completion checks will not be randomized
	The unlock requirements for each of these is the level item
	Utopia Shelter contains
		- 29 checkpoints
	"Requirements" sends a completion check when the "Requirements Only" goal would otherwise be reached
	"""
	display_name = "Goal Checks"
	valid_keys = ["Utopia Shelter", "Freom Mk-0", "Requirements"]
	default = []

class DowndashButtons(Toggle):
	"""
	By default, Down Dash isn't a very progressive item, only being useful for 1 check
	This option adds buttons that you need to logically Down Dash onto as locations
	The exact number may change between releases
	"""
	display_name = "Down Dash Buttons"

@dataclass
class Spark3Options(PerGameCommonOptions):
	goal_location: GoalLocation
	progression_mode: ProgressionMode
	ability_rando: AbilityRando
	gimmick_rando: GimmickRando
	character_logic: CharacterLogic
	energy_logic: EnergyLogic
	difficulty: Difficulty
	spark2_stages: Spark2Stages

	sphere_zero_list: SphereZeroList
	gate_zero_completions: GuaranteedCompletions
	starting_stages: StartingLocations
	
	freedom_count: FreedomCount
	freedom_required: FreedomRequired
	require_characters: RequireCharacters
	required_completion: RequiredCompletion
	required_speed: RequiredSpeed
	speed_type: SpeedRequiredType
	required_score: RequiredScore
	score_type: ScoreRequiredType
	required_explore: RequiredExplore
	utopia_hunt_medals: ExplorePercentIsHunt

	explore_hunt: ExploreHunt
	exploresanity: Exploresanity
	scoresanity: Scoresanity
	speedsanity: Speedsanity
	shopsanity: Shopsanity
	coinsanity: Coinsanity
	checkpointsanity: Checkpointsanity
	batterysanity: Batterysanity
	downdash_buttons: DowndashButtons
	goal_checks: GoalChecks
	endless_dive_checks: EndlessDiveChecks

	combat_moves: CombatMoves
	combat_option: CombatOptions
	progressive_combo: ProgressiveCombo
	progressive_score: ProgressiveScore
	progressive_timestop: ProgressiveTimestop
	progressive_energy: ProgressiveEnergy
	trap_chance: TrapChance
	
	labmode: LabMode

option_groups = [
	OptionGroup(
		"Basic Options",
		[GoalLocation, ProgressionMode, AbilityRando, GimmickRando, Difficulty, EnergyLogic, CharacterLogic, Spark2Stages, StartingLocations]
	),
	OptionGroup(
		"Progression Options",
		[SphereZeroList, FreedomCount, FreedomRequired, RequireCharacters, RequiredCompletion, RequiredSpeed, SpeedRequiredType, RequiredScore, ScoreRequiredType, RequiredExplore, ExplorePercentIsHunt]
	),
	OptionGroup(
		"Gating Options",
		[GuaranteedCompletions]
	),
	OptionGroup(
		"Extra Checks",
		[ExploreHunt, Exploresanity, Scoresanity, Speedsanity, Shopsanity, Coinsanity, Batterysanity, Checkpointsanity, DowndashButtons, GoalChecks, EndlessDiveChecks]
	),
	OptionGroup(
		"Item Options",
		[CombatMoves, CombatOptions, ProgressiveCombo, ProgressiveScore, ProgressiveTimestop, ProgressiveEnergy, TrapChance]
	),
	OptionGroup(
		"Miscellaneous",
		[LabMode]
	)
]