from dataclasses import dataclass
from Options import Choice, OptionGroup, PerGameCommonOptions, Range, Toggle, DefaultOnToggle, NamedRange

class Shopsanity(Toggle):
	"""
	Adds the 26 shop checks to the location pool
	"""
	display_name = "ShopSanity"

class AbilityRando(DefaultOnToggle):
	"""
	Randomize the abilities Spark normally can use into the item pool
		(Double Jump, Wall Jump/Walk, Dash, Charge Dash, Down Dash, Jester Dash, Combat)
	"""
	display_name = "Randomize Abilities"

class Speedsanity(Choice):
	"""
	Adds the 44 gold and/or 44 diamond speed medals to the location pool.
	WARNING: Due to the nature of these medals, they are hard to logically test.
		Logically, they require every ability to be unlocked. It's very possible to accidentally get these checks out of logic
		It's also possible that they require items from the shop
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
	Logic requires level completion plus Combat, though it may be possible to obtain some medals without combat
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

class FreedomCount(Range):
	"""
	How many freedom medals to put into the item pool.
	Under minimal settings (Ability Rando + No Sanities), the maximum is 24 (57 stages, 26 shop unlocks, 7 abilities)
	"""
	display_name = "Freedom Medal Count"

	range_start = 0
	range_end = 100

class FreedomRequired(NamedRange):
	"""
	What's the required percentage of freedom medals to unlock Utopia Shelter
	Note that you still need Fark and Sfarx to unlock Utopia Shelter
	"""
	display_name = "Required Percentage"
	
	range_start = 0
	range_end = 100
	special_range_names = {
		"open_world": 0
	}

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
	freedom_count: FreedomCount
	freedom_required: FreedomRequired
	shopsanity: Shopsanity
	speedsanity: Speedsanity
	scoresanity: Scoresanity
	exploresanity: Exploresanity
	music_rando: MusicChoice