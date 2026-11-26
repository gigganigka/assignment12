# Two-to-three-minute video guide

Use your own voice and perform these actions for real. Replace bracketed descriptions with what actually happened. This is a script, not a claim that the tests passed.

**0:00–0:30 — Data and levels**
“This is Coinbound, my Assignment 11 project. Player, enemy and item balance is stored in ScriptableObjects. These are the three LevelData assets. The same campaign scene builds terrain, enemies, items and the goal from the selected asset.”
Show the assets and one level in Play Mode.

**0:30–1:00 — Live change and HP**
“I am changing [moveSpeed / jumpVelocity] from [actual old value] to [actual new value] in the Inspector, without editing code. Now I start a new attempt and compare the movement. Health is carried between levels: I finish with [HP], and the next level starts with the same HP.”
Show the actual behaviour. Restore your preferred tuning afterwards.

**1:00–1:40 — Phone restart**
“Here I change music volume and complete a level. I pause to save. I close the app completely and open it again. The saved level, remaining health, unlocks and settings [describe the actual result].”
Use real phone footage. Do not substitute Unity Remote for an installed-app persistence test. Explain if you use the Web/Safari version.

**1:40–2:10 — Broken JSON and reset**
“This is the real save.json from persistentDataPath. With the game closed, I back it up and deliberately damage the JSON. When I reopen the game, it [describe the actual result]. Reset Progress has an in-game confirmation. Cancel preserves the data.”

**2:10–2:40 — Map and decisions**
“The catalog provides data to the level builder and combat scripts. RunProgress carries state, SaveService decides when to save, and SaveRepository writes JSON safely. The UI reads saved unlocks. My decision log explains the health reset bug and why I separated scene loading from saved state.”
Show SCRIPT_MAP.md and your DECISIONS.md. Export as a 2–3 minute MP4 file with your own English voice.
