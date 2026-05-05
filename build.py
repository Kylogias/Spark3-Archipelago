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
	"Assembly-CSharp.dll", "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.UI.dll",
	"UnityEngine.AudioModule.dll", "UnityEngine.JSONSerializeModule", "Rewired_Core.dll", "UnityEngine.UnityWebRequestModule.dll",
	"UnityEngine.UnityWebRequestAudioModule.dll", "UnityEngine.PhysicsModule.dll", "UnityEngine.InputLegacyModule", "UnityEngine.TextRenderingModule"
]

command = COMMANDBASE[0]

for i in COMMANDBASE[1:]:
	command = f"{command} {i}"

for i in INCLUDE_MANAGED:
	command = f"{command} -reference:{UNITYBASE}{i}"

with open("apshared.json", "r") as apjs:
	shared = json.load(apjs)

location_name_to_id = {}
item_name_to_id = {}

itemID = 16295350000
for item in shared["items"]:
	item_name_to_id[item["name"]] = itemID
	item["id"] = itemID
	itemID += 1

curID = 16295300000
for shop in shared["shop"]:
	shop["id"] = curID
	location_name_to_id[f"Shop {shop['page']} {shop['name']}"] = curID
	curID += 1

sanity_priority = ["base", "speedgold", "speeddia", "scoregold", "scoredia", "explore", "hunt", "coin"]
sanities = {}
for sanity in sanity_priority:
	sanities[sanity] = []

itemID = 16295351000
for stage in shared["stages"]:
	explore_rules = []
	for check in stage["checks"].copy():
		if check["sanity"] == "explore":
			explore_rules.append(f"({check['requires']})")
		if check["sanity"] in sanities:
			sanities[check["sanity"]].append([stage, check])
		else:
			stage["checks"].remove(check)
			continue
	if len(explore_rules):
		explore_name = f"{stage['name']} EXPLORE MEDAL"
		item_name_to_id[explore_name] = itemID + stage["id"]
		shared["items"].append({"name": explore_name, "itemtype": "EXPLORE2" if stage["type"] == "spark2" else "EXPLORE3", "id": itemID+stage["id"]})
		check = {"name": "EXPLORE HUNT", "sanity": "hunt", "requires": ""}
		stage["checks"].append(check)
		sanities["hunt"].append([stage, check])

for sanity in sanity_priority:
	for location in sanities[sanity]:
		stage = location[0]
		check = location[1]
		check["id"] = curID
		if isinstance(check["requires"], str):
			check["requires"] = {"base": check["requires"]}
		print(check["requires"])
		location_name_to_id[f"{stage['name']} {check['name']}"] = curID
		if not "index" in check:
			check["index"] = -1;
		curID += 1

with open("world/apshared.py", "w") as appy:
	appy.write("from .items import ItemType\n")
	appy.write("apshared = ")
	shared_str = json.dumps(shared, indent='\t')
	index = shared_str.find("itemtype")
	while True:
		index = shared_str.find("itemtype")
		if index == -1: break
		part = shared_str.partition('"itemtype": "')
		shared_str = "".join([part[0], '"type": ItemType.'])
		part = part[2].partition('"')
		shared_str = "".join([shared_str, part[0], part[2]])
	appy.write(shared_str)
	appy.write("\nlocation_name_to_id = ")
	appy.write(json.dumps(location_name_to_id, indent='\t'))
	appy.write("\nitem_name_to_id = ")
	appy.write(json.dumps(item_name_to_id, indent='\t'))

with open("client/apshared.cs", "w") as apcs:
	apcs.write("namespace Sparkipelago {\n")
	apcs.write("\tpublic enum ItemIds : long {\n")
	for item in shared["items"]:
		apcs.write(f"\t\t{item['name'].replace(' ', '_').replace('.', '').replace('-', '').upper()} = {item['id']},\n")
	apcs.write(f"\t\tBASE_EXPLORE_MEDAL = {itemID}\n")
	apcs.write("\t};\n")
	apcs.write("\tclass APShared {\n")
	apcs.write(f"\t\tpublic static int version = {shared['version']};\n")
	apcs.write("\t\tpublic static long[] itemIDs = {")
	for item in shared["items"]:
		apcs.write(f"{item['id']}")
		if item != shared["items"][-1]:
			apcs.write(", ")
	apcs.write("};\n")
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
			apcs.write(f"\t\t\t\tnew APStageCheck(\"{check['name']}\", \"{check['sanity']}\", new string[]")
			apcs.write("{")
			difficulties = []
			requires = []
			for k in check['requires'].keys():
				difficulties.append(k)
				requires.append(check['requires'][k])
			for k in difficulties:
				apcs.write(f"\"{k}\"")
				if k != difficulties[-1]:
					apcs.write(", ")
			apcs.write("}, new string[]{")
			for v in requires:
				apcs.write(f"\"{v}\"")
				if v != requires[-1]:
					apcs.write(", ")
			apcs.write("}")
			apcs.write(f", {check['id']}, {check['index']})")
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

