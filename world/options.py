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
	"""
	
	option_base = 0
	default = option_base

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

class PerfectCombo(DefaultOnToggle):
	"""
	When enabled, puts permanent max combo into the item pool
	"""
	display_name = "Perfect Combo"

class ScoreMultiplier(Toggle):
	"""
	When enabled, randomizes an item that gives 30x score multiplier on stage load
	"""
	display_name = "Score Multiplier"

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
	WARNING: Logic Not Implemented
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

class MusicChoice(Choice):
	"""
	What music to use. If non-vanilla, music is chosen from "(game directory)/apmusic"
	"""
	display_name = "Music Randomization"
	
	option_vanilla = 0
	option_per_stage = 1
	option_per_load = 2
	option_per_loop = 3
	
	default = option_vanilla

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

class EndlessDiveFloors(Range):
	"""
	How many floors should you complete for a check in Endless Dive? Total floor count is the check amount times floor amount
	"""
	display_name = "Endless Dive Floors per Check"

	range_start = 1
	range_end = 10

	default = 1

class EnemyRando(Choice):
	"""
	How should enemies be randomized?
	"""
	display_name = "Enemy Rando"

	option_vanilla = 0
	option_only_enemies = 1
	option_bosses_on_enemies = 2

	default = option_vanilla

@dataclass
class Spark3Options(PerGameCommonOptions):
	ability_rando: AbilityRando
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
	batterysanity: Batterysanity
	endless_dive_checks: EndlessDiveChecks
	endless_dive_floors: EndlessDiveFloors

	perfect_combo: PerfectCombo
	score_multiplier: ScoreMultiplier
	trap_chance: TrapChance
	
	music_rando: MusicChoice
	enemy_rando: EnemyRando
	labmode: LabMode

option_groups = [
	OptionGroup(
		"Basic Options",
		[AbilityRando, Difficulty, Spark2Stages]
	),
	OptionGroup(
		"Gate Options",
		[FreedomCount, FreedomRequired, RequireCharacters, RequiredCompletion, RequiredSpeed, SpeedRequiredType, RequiredScore, ScoreRequiredType, RequiredExplore, ExplorePercentIsHunt]
	),
	OptionGroup(
		"Extra Checks",
		[ExploreHunt, Exploresanity, Scoresanity, Speedsanity, Shopsanity, Coinsanity, Batterysanity, EndlessDiveChecks, EndlessDiveFloors]
	),
	OptionGroup(
		"Item Options",
		[PerfectCombo, ScoreMultiplier, TrapChance]
	),
	OptionGroup(
		"Miscellaneous",
		[MusicChoice, EnemyRando, LabMode]
	)
]