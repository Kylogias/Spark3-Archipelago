import os
import glob
import shutil
import json

SPARKDIR = "sparkdir"
UNITYBASE = f"{SPARKDIR}/\"Spark the Electric Jester 3_Data\"/Managed/"

COMMANDBASE = [
	"mcs",
	"-target:library",
	"-sdk:4.7.2",
	"-out:mod/Mods/Sparkipelago.Mono.dll",
	f"-reference:{SPARKDIR}/MelonLoader/net35/MelonLoader.dll",
	f"-reference:{SPARKDIR}/MelonLoader/net35/0Harmony.dll",
	f"-reference:{SPARKDIR}/UserLibs/Archipelago.MultiClient.Net.dll",
	f"-reference:{SPARKDIR}/UserLibs/Newtonsoft.Json.dll"
]

INCLUDE_MANAGED = [
	"Assembly-CSharp.dll", "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.UI.dll", "UnityEngine.AudioModule.dll",
	"Rewired_Core.dll", "UnityEngine.UnityWebRequestModule.dll", "UnityEngine.UnityWebRequestAudioModule.dll", "UnityEngine.PhysicsModule.dll"
]

command = COMMANDBASE[0]

for i in COMMANDBASE[1:]:
	command = f"{command} {i}"

for i in INCLUDE_MANAGED:
	command = f"{command} -reference:{UNITYBASE}{i}"

with open("apshared.json", "r") as apjs:
	shared = json.load(apjs)

location_name_to_id = {}

curID = 16295300000
for shop in shared["shop"]:
	shop["id"] = curID
	location_name_to_id[f"Shop {shop['page']} {shop['name']}"] = curID
	curID += 1

sanity_priority = ["base", "speedgold", "speeddia", "scoregold", "scoredia", "explore", "coin"]
sanities = {}
for sanity in sanity_priority:
	sanities[sanity] = []

for stage in shared["stages"]:
	for check in stage["checks"].copy():
		if check["sanity"] in sanities:
			sanities[check["sanity"]].append([stage, check])
		else:
			stage["checks"].remove(check)
			continue

for sanity in sanity_priority:
	for location in sanities[sanity]:
		stage = location[0]
		check = location[1]
		check["id"] = curID
		location_name_to_id[f"{stage['name']} {check['name']}"] = curID
		if not "index" in check:
			check["index"] = -1;
		curID += 1

with open("world/apshared.py", "w") as appy:
	appy.write("apshared = ")
	appy.write(json.dumps(shared, indent='\t'))
	appy.write("\nlocation_name_to_id = ")
	appy.write(json.dumps(location_name_to_id, indent='\t'))

with open("client/apshared.cs", "w") as apcs:
	apcs.write("namespace Sparkipelago {\n")
	apcs.write("\tclass APShared {\n")
	apcs.write(f"\t\tpublic static int version = {shared['version']};\n")
	apcs.write("\t\tpublic static APShopItem[] shop = {\n")
	for shop in shared["shop"]:
		apcs.write(f"\t\t\tnew APShopItem(\"{shop['name']}\", \"{shop['page']}\", {shop['id']})")
		if shop != shared["shop"][-1]:
			apcs.write(",")
		apcs.write("\n")
	apcs.write("\t\t};\n")
	apcs.write("\t\tpublic static APStageData[] stages = {\n")
	for stage in shared["stages"]:
		apcs.write(f"\t\t\tnew APStageData(\"{stage['name']}\", \"{stage['type']}\", {stage['id']}, new APStageCheck[]")
		apcs.write("{\n")
		for check in stage["checks"]:
			apcs.write(f"\t\t\t\tnew APStageCheck(\"{check['name']}\", \"{check['sanity']}\", \"{check['requires']}\", {check['id']}, {check['index']})")
			if check != stage["checks"][-1]:
				apcs.write(",")
			apcs.write("\n")
		apcs.write("\t\t\t})")
		if stage != shared["stages"][-1]:
			apcs.write(",")
		apcs.write("\n")
	apcs.write("\t\t};\n")
	apcs.write("\t}\n")
	apcs.write("}")

for file in glob.iglob("client/**/*.cs", recursive=True):
	command = f"{command} {file}"

os.system("clear")
os.system(command)
shutil.copyfile("mod/Mods/Sparkipelago.Mono.dll", f"{SPARKDIR}/Mods/Sparkipelago.Mono.dll")

