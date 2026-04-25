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

class LabMode(Toggle):
	"""
	Overrides the ability randomizer and automatically opens all stages, for labbing checks.
	WARNING: For development purposes only
	"""
	display_name = "Labbing Mode"

class Speedsanity(Choice):
	"""
	Adds the 44 gold and/or 44 diamond speed medals to the location pool.
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
	Adds the 28 gold and/or 28 diamond score medals to the location pool.
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
	Adds the 300 exploration medals as checks
	"""
	display_name = "ExploreSanity"

class Coinsanity(Toggle):
	"""
	Adds the 72 collectable coins in the collectathon stages as checks
	"""
	display_name = "CoinSanity"

class FreedomCount(Range):
	"""
	How many freedom medals to put into the item pool.
	Under minimal settings (Ability Rando + No Sanities + No Spark 2), the maximum is 10 (43 stages, 26 shop unlocks, 7 abilities)
	"""
	display_name = "Freedom Medal Count"

	range_start = 0
	range_end = 100
	
	default = 10

class FreedomRequired(Range):
	"""
	What's the required percentage of freedom medals to unlock Utopia Shelter
	Note that you still need Fark and Sfarx to unlock Utopia Shelter
	"""
	display_name = "Required Percentage"
	
	range_start = 0
	range_end = 100
	
	default = 100

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

@dataclass
class Spark3Options(PerGameCommonOptions):
	ability_rando: AbilityRando
	difficulty: Difficulty
	labmode: LabMode
	spark2_stages: Spark2Stages
	freedom_count: FreedomCount
	freedom_required: FreedomRequired
	shopsanity: Shopsanity
#	speedsanity: Speedsanity
#	scoresanity: Scoresanity
	exploresanity: Exploresanity
	coinsanity: Coinsanity
	music_rando: MusicChoice