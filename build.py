import os
import glob
import shutil
import json

os.system("clear")

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
	"Assembly-CSharp.dll", "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.UI.dll", "UnityEngine.UIModule.dll",
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
rules = {" ": "BASE WHITESPACE", "(": "BASE LPAREN", ")": "BASE RPAREN", "+": "BASE AND", "|": "BASE OR"}
for item in shared["items"]:
	if "rule" in item:
		for rule in rules.keys():
			if item["rule"].startswith(rule) or rule.startswith(item["rule"]):
				print(f"RULE CONFLICT: {item['name']} and {rules[rule]}")
		rules[item["rule"]] = item["name"]
	item_name_to_id[item["name"]] = itemID
	item["id"] = itemID
	itemID += 1

for macro in shared["rule_macros"]:
	for rule in rules.keys():
		if macro["rule"].startswith(rule) or rule.startswith(macro["rule"]):
			print(f"RULE CONFLICT: Macro {macro['rule']} and {rules[rule]}")
	rules[macro['rule']] = f"Macro {macro['rule']}"

curID = 16295300000
for shop in shared["shop"]:
	shop["id"] = curID
	location_name_to_id[f"Shop {shop['page']} {shop['name']}"] = curID
	curID += 1

sanity_priority = ["base", "speedgold", "speeddia", "scoregold", "scoredia", "explore", "hunt", "coin", "battery"]
sanities = {}
for sanity in sanity_priority:
	sanities[sanity] = []

itemID = 16295351000
for stage in shared["stages"]:
	explore_rules = []
	if not "coin_count" in stage:
		stage["coin_count"] = 0
		stage["coin_req"] = 0
	if stage["type"] == "endless":
		for i in range(110):
			stage["regions"][0]["checks"].append({"name": f"#{i+1}", "index": i+1, "sanity": "base", "requires": ""})
	sanity_max = {"checkpoint": -1, "capsule": -1, "bubble": -1}
	sanity_seen = {"checkpoint": [], "capsule": [], "bubble": []}
	for sanity in sanity_priority:
		sanity_max[sanity] = -1
		sanity_seen[sanity] = []
	for region in stage["regions"]:
		for entrance in region["entrances"].keys():
			if isinstance(region["entrances"][entrance], str):
				region["entrances"][entrance] = {"base": region["entrances"][entrance]}
		for check in region["checks"].copy():
			if check["sanity"] == "explore":
				explore_rules.append(f"({check['requires']})")
			if check["sanity"] in sanity_max and "index" in check and check["index"] > sanity_max[check["sanity"]]:
				sanity_max[check["sanity"]] = check["index"]
			if check["sanity"] in sanity_seen and "index" in check:
				if (check["index"] in sanity_seen[check["sanity"]]): print(f"WARNING: Found multiple {check['index']} in {stage['name']} {check['sanity']}")
				sanity_seen[check["sanity"]].append(check["index"])
			if check["sanity"] in sanities:
				sanities[check["sanity"]].append([stage, check])
			else:
				region["checks"].remove(check)
				continue
	for sanity in sanity_seen:
		for i in range(sanity_max[sanity]+1):
			if not i in sanity_seen[sanity]:
				print(f"WARNING: {i} missing in {stage['name']} {sanity}")
	if len(explore_rules):
		explore_name = f"{stage['name']} EXPLORE MEDAL"
		item_name_to_id[explore_name] = itemID + stage["id"]
		shared["items"].append({"name": explore_name, "itemtype": "EXPLORE2" if stage["type"] == "spark2" else "EXPLORE3", "id": itemID+stage["id"]})
		check = {"name": "EXPLORE HUNT", "sanity": "hunt", "requires": ""}
		for region in stage["regions"]:
			if region["name"] == "GOAL":
				region["checks"].append(check)
		sanities["hunt"].append([stage, check])

for sanity in sanity_priority:
	for location in sanities[sanity]:
		stage = location[0]
		check = location[1]
		check["id"] = curID
		if isinstance(check["requires"], str):
			check["requires"] = {"base": check["requires"]}
		location_name_to_id[f"{stage['name']} {check['name']}"] = curID
		if not "index" in check:
			check["index"] = -1;
		curID += 1

with open("world/apshared.py", "w") as appy:
	appy.write("from .items import ItemType\n")
	appy.write("apshared = ")
	shared_str = json.dumps(shared)
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
	appy.write(json.dumps(location_name_to_id))
	appy.write("\nitem_name_to_id = ")
	appy.write(json.dumps(item_name_to_id))

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
		stage_checks = []
		for region in stage["regions"]:
			for check in region["checks"]:
				stage_checks.append(check)

		apcs.write(f"\t\t\tnew APStageData(\"{stage['name']}\", \"{stage['type']}\", {stage['id']}, new APStageCheck[]")
		apcs.write("{\n")
		for check in stage_checks:
			apcs.write(f"\t\t\t\tnew APStageCheck(\"{check['name']}\", \"{check['sanity']}\"")
			apcs.write(f", {check['id']}, {check['index']})")
			if check != stage_checks[-1]:
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

os.system(command)
shutil.copyfile("mod/Mods/Sparkipelago.Mono.dll", f"{SPARKDIR}/Mods/Sparkipelago.Mono.dll")

