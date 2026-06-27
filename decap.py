shared_text = ""
start = '"stages": ['
target = '"name": "'
with open("apshared copy.json") as apshared:
	shared_text = apshared.read()

index = shared_text.find(start)+len(start)
title_most = shared_text[:index]
potential_entrances = []
while True:
	old_index = index
	index = shared_text.find(target, index)
	if index == -1:
		title_most += shared_text[old_index:]
		break
	index += len(target)
	title_most += shared_text[old_index:index]
	end = shared_text.find('"', index)
	name_string = shared_text[index:end]
	potential_entrances.append(f'"{name_string}"')
	name_string = name_string.title()
	i = 0
	while shared_text[index] != '"':
		title_most += name_string[i]
		i += 1
		index += 1

title_entrance = shared_text

for entrance in potential_entrances:
	index = 0
	old_title_entrance = title_entrance
	title_entrance = ""
	while True:
		old_index = index
		index = old_title_entrance.find(entrance, index)
		if index == -1:
			title_entrance += old_title_entrance[old_index:]
			break
		title_entrance += old_title_entrance[old_index:index]
		name_string = entrance.title()
		title_entrance += name_string
		index += len(name_string)

with open("apshared.json", "w") as apshared:
	apshared.write(title_entrance)