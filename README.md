WIP and Unstable Archipelago integration for Spark the Electric Jester 3

[Archipelago](https://archipelago.gg/) is "a cross-game modification system which randomizes different games, then uses the result to build a single unified multi-player game. Items from one game may be present in another, and you will need your fellow players to find items you need in their games to help you complete your own," (copied from the website)

By default, level and boss completions may have archipelago items, though there are options to enable Score Medals, Speed Medals, Exploration Medals, and Shop Items.

The items that may be randomized include
- Bit/Energy Bubbles
- Health/Score/Energy Capsules
- Double Jump, Wall Jump/Walk, Dash, Jester Dash, Charged Jester Dash, Down Dash, Combat (can be disabled in the settings)
- Shop items
- Shop pages (if the shop locations are enabled)

Stages are locked behind level gates, akin to the Sonic Adventure 2 Archipelago, needing a certain amount of Freedom Medals to unlock a gate. Each gate (aside from the first) has a boss that must be defeated before accessing the stages within.

Music in stages can also be randomized. Enable it in your YAML, and drop any `.ogg` format music files into the `apmusic` folder

The goal of the randomizer is to complete Utopia Shelter and defeat the final boss within, Claritas Centralis

# Installation (client)
1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader)
2. Drop the latest client into the Spark 3 installation folder
3. Drop [MelonPreferencesManager](https://github.com/Bluscream/MelonPreferencesManager) (Mono) into the `Mods` folder
4. Drop [UniverseLib](https://github.com/sinai-dev/UniverseLib/releases/tag/1.5.1) (Mono, dependency of MelonPreferencesManager) into the `UserLibs` folder
5. One launched, press F5 to open the preference manager, go into the "Archipelago Connection" tab, and enter the connection information
6. Delete a save if necessary and start a new game, the client will automatically connect after selecting a save file

# Building

The build system is very much "it works on my machine"
Prerequisites:
1. A C compiler (for the buildsystem)
2. [Mono](https://mono-project.com), latest should be fine? The build system expects the `mcs` command
3. [Archipelago.MultiClient.Net](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net)
4. [MelonLoader](https://github.com/LavaGang/MelonLoader)

To compile
1. Edit the string on line 117 in `build/nob.c` to the path of your Spark 3 installation
2. Compile nob.c
3. Drop Archipelago.MultiClient.Net into the UserLibs directory of the Spark 3 installation
4. Try compiling