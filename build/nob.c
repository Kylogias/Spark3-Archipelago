#define NOB_IMPLEMENTATION
#include "nob.h"

#define CLEARCMD "clear"

// Pretty sure Nob_Cmd is just a string array
Nob_Cmd srcdirs = {.items=(const char*[]){"client"}, .count=1};

const char* cwd = NULL;

bool runout = false;

bool clargHelp(char* argv[]) {
	(void)argv;
	printf("Usage: ./nob {args}\n");
	printf("Arguments: \n");
	printf("  --license        Display Licenses for libraries used in the build system\n");
	printf("  -c               Clear the output before running\n");
	return 1;
}

bool clargLicense(char* argv[]) {
	(void)argv;
	printf(
"This command is vestigial. The only library used now is nob.h by tsoding\n"
	);
	return 1;
}

bool clargClear(char* argv[]) {
	(void)argv;
	Nob_Cmd clear = {0};
	nob_cmd_append(&clear, "clear");
	nob_cmd_run(&clear, 0);
	return 0;
}

bool clargRun(char* argv[]) {
	(void)argv;
	runout = true;
	return 0;
}

struct clarg {
	const char* name;
	bool(*process)(char* argv[]);
	int count;
};

struct clarg clargs[] = {
	{.name="--help", .process=clargHelp, .count=1},
	{.name="--license", .process=clargLicense, .count=1},
	{.name="-c", .process=clargClear, .count=1},
};
const int clargCount = sizeof(clargs)/sizeof(*clargs);

struct appendfile {
	Nob_Cmd* cmd;
	Nob_Procs* async;
};

bool appendfile(Nob_Walk_Entry entry) {
	struct appendfile* af = entry.data;
	const char* path = entry.path;
	int pathlen = strlen(path);
	if (entry.type == NOB_FILE_DIRECTORY) return true;
	
	char* cpath = strdup(path);
	int i;
	for (i = pathlen-1; i >= 0; i--) if (cpath[i] == '.') break;
	if (!strcmp(cpath+i, ".cs")) {
		nob_cmd_append(af->cmd, strdup(path));
	}
	
	*entry.action = NOB_WALK_CONT;
	return true;
}

bool mkdir_recursive(char* dir) {
	char* curstr = dir;
	while (*dir != '\0') {
		if (*dir == '/' || *dir == '\\') {
			char old = *dir;
			*dir = '\0';
			if (!nob_mkdir_if_not_exists(curstr)) return false;
			*dir = old;
		}
		dir += 1;
	}
	return true;
}

int main(int argc, char* argv[]) {
	NOB_GO_REBUILD_URSELF(argc, argv);
	
	for (int i = 1; i < argc;) {
		int j;
		for (j = 0; j < clargCount; j++) {
			if (!strcmp(argv[i], clargs[j].name)) break;
		}
		if (j == clargCount) {
			printf("Unknown Argument: %s\n", argv[i]);
			return 1;
		}
		if (i+clargs[j].count > argc) {
			printf("Not Enough Arguments: %s\n", argv[i]);
			return 1;
		}
		if (clargs[j].process(argv+i)) return 0;
		i += clargs[j].count;
	}
	
	Nob_Cmd linkcmd = {0};
	Nob_Procs async = {0};
	cwd = nob_get_current_dir_temp();
	
	#define SPARKDIR "sparkdir"
	#define UNITYBASE SPARKDIR "/Spark the Electric Jester 3_Data/Managed/"

	nob_cmd_append(
		&linkcmd, "mcs", "-target:library", "-sdk:4.7.2", "-out:mod/Mods/Sparkipelago.Mono.dll",
		"-reference:" SPARKDIR "/MelonLoader/net35/MelonLoader.dll",
		"-reference:" SPARKDIR "/MelonLoader/net35/0Harmony.dll",
		"-reference:" SPARKDIR "/UserLibs/Archipelago.MultiClient.Net.dll",
		"-reference:" SPARKDIR "/UserLibs/Newtonsoft.Json.dll"
	);
	
	const char* include_managed[] = {
		"Assembly-CSharp.dll", "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.UI.dll", "UnityEngine.AudioModule.dll",
		"Rewired_Core.dll", "UnityEngine.UnityWebRequestModule.dll", "UnityEngine.UnityWebRequestAudioModule.dll"
	};
	int numinclude = sizeof(include_managed)/sizeof(*include_managed);
	for (int i = 0; i < numinclude; i++) {
		char ref[512] = "-reference:" UNITYBASE;
		strcat(ref, include_managed[i]);
		nob_cmd_append(&linkcmd, strdup(ref));
	}

	struct appendfile af = {.cmd=&linkcmd, .async=&async};
	for (size_t i = 0; i < srcdirs.count; i++) nob_walk_dir(srcdirs.items[i], appendfile, .data=&af);
	
	bool succ = nob_procs_flush(&async);

	
	if (succ && nob_cmd_run(&linkcmd, 0)) {
		if (runout) {
			Nob_Cmd runcmd = {0};
		//	nob_cmd_append(&runcmd, "./"OUTEXE);
			nob_cmd_run(&runcmd, 0);
		}
		nob_copy_file("mod/Mods/Sparkipelago.Mono.dll", SPARKDIR "/Mods/Sparkipelago.Mono.dll");
	} else {
		fprintf(stderr, "Compile Error, see log files for more information\n");
	}
}
