from dataclasses import dataclass
from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle, DefaultOnToggle, NamedRange

class Shopsanity(Toggle):
	"""
	Adds the 26 shop checks to the location pool
	"""
	display_name = "ShopSanity"

class Difficulty(Choice):
	"""
	How difficult should the logic be
	WARNING: The only implemented difficulty at the moment is hard! Other difficulties will use hard logic settings

	normal: The easiest option, designed for those new to the game and GrimmyHunter
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
	Adds the 237 checkpoints as checks (355 with Spark 2 stages)
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

@dataclass
class Spark3Options(PerGameCommonOptions):
	ability_rando: AbilityRando
	gimmick_rando: GimmickRando
	character_logic: CharacterLogic
	energy_logic: EnergyLogic
	difficulty: Difficulty
	spark2_stages: Spark2Stages

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
	endless_dive_checks: EndlessDiveChecks

	progressive_combo: ProgressiveCombo
	progressive_score: ProgressiveScore
	progressive_timestop: ProgressiveTimestop
	trap_chance: TrapChance
	
	labmode: LabMode

option_groups = [
	OptionGroup(
		"Basic Options",
		[AbilityRando, GimmickRando, Difficulty, EnergyLogic, CharacterLogic, Spark2Stages]
	),
	OptionGroup(
		"Gate Options",
		[FreedomCount, FreedomRequired, RequireCharacters, RequiredCompletion, RequiredSpeed, SpeedRequiredType, RequiredScore, ScoreRequiredType, RequiredExplore, ExplorePercentIsHunt]
	),
	OptionGroup(
		"Extra Checks",
		[ExploreHunt, Exploresanity, Scoresanity, Speedsanity, Shopsanity, Coinsanity, Batterysanity, Checkpointsanity, EndlessDiveChecks]
	),
	OptionGroup(
		"Item Options",
		[ProgressiveCombo, ProgressiveScore, ProgressiveTimestop, TrapChance]
	),
	OptionGroup(
		"Miscellaneous",
		[LabMode]
	)
]