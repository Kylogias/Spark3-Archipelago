W.I.P and UNSTABLE

# Installation (client)
Drop the latest client into the Spark 3 installation folder

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