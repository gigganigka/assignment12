# Real 2–3 minute recording plan

Record the actual build, game audio and your own English voice. This is a suggested outline, not proof that these actions were recorded. Leave short quiet gaps so warnings, music transitions and volume changes can be heard. You may edit real takes together, but keep the close/reopen settings test continuous and clearly show it. Use the gameplay recording from your iPhone as the phone deliverable; confirm with your instructor if Safari is accepted.

| Time | Show for real | Suggested narration (adapt to your words) |
| --- | --- | --- |
| 0:00–0:15 | Main menu, then NEW RUN; let music transition | “This is Coinbound. The menu and level have two different music tracks. They crossfade for one second. The audio manager survives scene changes.” |
| 0:15–0:40 | Jump, coin, shoot, take damage, collect heart | “Jump, hit, pickup, healing and shooting have separate sounds. Repeated short effects have a small pitch variation.” |
| 0:40–1:00 | Approach enemy, wait for warning, then dodge attack; show other enemy warnings using real takes | “Listen first. The warning starts when the enemy enters its wind-up state. The actual attack follows after the delay, so I have time to dodge.” |
| 1:00–1:15 | Pause and resume, leave voice gap | “Pause freezes the game, but the music continues quietly. The Paused snapshot lowers its gain and applies a lowpass. This Web build uses filtered copies of the same tracks because browser mixer effects are limited.” (Use the last sentence only in Web.) |
| 1:15–1:45 | Settings sliders with visible dB → close app/tab → reopen same build/address → Settings unchanged | “I am changing Music and SFX. The sliders save linear values in JSON. The mixer receives twenty times log ten. Here I close and reopen the game. Both values are restored.” |
| 1:45–2:05 | Real death/loss; real exit and final victory take | “Player death, the lose result, level clear and the final win have different cues. The level theme does not restart when the next level loads.” |
| 2:05–2:35 | Real mixer window/groups/snapshots, script map from README | “Gameplay requests events through Sfx. SoundBank holds clips and gain settings. SoundPass routes voices, changes music and controls snapshots. Settings use SaveService and SaveRepository to store JSON. PauseDuck is separate from the saved music volume.” |

The eight mandatory events are **jump, hit, pickup, enemy wind-up, death, UI click, win and lose**. Make every one actually audible. Include enemy attack releases too. A menu click while narrating may be difficult to hear: leave a gap. Film the enemy test while it is preparing, then show the attack: the warning must be audible before the damaging move or projectile arrives. Do not turn off fair visibility guards just to force offscreen attacks.

Do not reset or unlock the campaign by editing the save and present that as normal progress. Record the needed real victory/next-level takes in advance. Audition buttons help review clips, but are not a replacement for gameplay proof.

For the defense, be able to explain: why a singleton persists; why repeated level loads do not call Play again; why log10(0) needs a mute branch; how snapshots differ from exposed user parameters; why the Web fallback exists; how the wind-up event is emitted once; and where your settings are saved. Add your own listening decisions to DECISIONS.md after testing.
