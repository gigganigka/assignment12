# Assignment 10 decisions

- 2026-11-12: Asked Codex to add three different enemies to Coinbound using the existing player, Health and IDamageable interfaces.
- 2026-11-12: Implemented separate Patroller, Shooter and Charger state machines; common perception and movement stay in EnemyBrain2D. Added red wind-ups, cooldowns and camera visibility guards.
- 2026-11-12: Source review found runtime-created enemy fields would be inconvenient to tune persistently. Added three EnemyTuning assets so changes can be saved outside Play Mode.
- 2026-11-12: Used a separate enemy_lab encounter before the original campaign, preserving the original player and camera. This requires Unity/phone validation; no playtest result is invented.
- 2026-11-12 follow-up: moved all three enemy types into level1 and level2 through authored CampaignEnemies spawns; menu Play now starts level1 and the lab stays optional. Removed legacy patrol duplicates at runtime.
- 2026-11-12 follow-up: chase tracks acquired targets without reapplying the forward cone, uses collider-centred sight, last-seen position and larger retention radius. Patroller acquisition 6 -> 8, view angle 150 -> 180, memory 0.8 -> 1.5 seconds; retention radius 10 added. These are implementation changes from source review, not measured playtest outcomes.
- 2026-11-12 follow-up: reused matching supplied sprite pairs for distance-driven movement animation; stopped motion displays idle and warning colours remain controlled by AI.
- Student follow-up: after testing, replace this placeholder with what you changed, the real old/new values and why. Record both testers and attempt counts in Docs/Assignment10/PLAYTEST.csv.

- 2026-11-12 jump fix: chose velocity 12.5 with gravity scale 3 to provide margin above the authored 2.1-unit platforms. Made floating platforms passable from below to remove underside head collisions. This task is isolated in coinbound-jump-fix; runtime validation is pending.

## Assignment 11 — 2026-11-19

- Asked Codex to implement the full Data and Saves brief and fix health refilling at scene transitions. The original Health.Awake assigned maxHp every time; remaining HP now belongs to the run state.
- Moved active player, enemy, boss, item and settings balance to ScriptableObjects and used one campaign scene for three LevelData assets.
- Source review found the old moving-platform crossing was omitted from the first data conversion. Added a data-authored stepping stone at x=24 so the second map stays connected.
- Separated deterministic RunProgress from scene/file operations; added Unity editor checks that use a temporary save directory instead of the real player save. JSON is used with validation and defaults, never BinaryFormatter.
- Offline checks were run. Unity import, actual gameplay, iPhone restart proof, Inspector/save screenshots and my own English voice video still need to be performed; no device result is claimed.

- 2026-11-19: after the user reported no keyboard movement, removed the gameplay dependency on PlayerInput device pairing. Keyboard, gamepad and touch now have explicit input priority; serialized callbacks remain compatible without double jumps/shots. Actual keyboard testing in Unity remains pending.

## Assignment 12 — 2026-11-26

- Asked Codex to implement all Sound Pass criteria on the current working project, keeping the earlier input, jump, remaining-HP and save fixes. Found one music loop and the shooter reusing the jump clip; added distinct event cues and two original procedural compositions with reproducible CC0 provenance.
- Chose a persistent SoundPass with pooled routed SFX voices and two music sources. Changing campaign levels preserves the active track; switching menu/gameplay uses a one-second unscaled crossfade. Warning cues belong to enemy state entry, before the existing wind-up delay.
- Kept the existing linear JSON volume fields for compatibility and converted only at the mixer boundary with Log10 × 20. Added touch sliders and an independent PauseDuck bus so the Paused snapshot does not override saved music gain. The first offline boundary test examined full-amplitude high-frequency samples rather than just the splice; corrected it to inspect the first/last millisecond while retaining strict endpoint/DC/headroom checks.
- Source review found that Unity Web does not support native mixer lowpass effects. Added synchronized filtered copies of the same compositions for Web pause, while Windows/native iOS use the real Paused snapshot. The native mixer serialization was checked against Unity's sample type information; Unity import still must be verified.
- Offline asset/signal/source checks were performed; no Unity, iPhone, mixer screenshot or narrated game recording is available here. Device evidence, the student's final mix decisions and spoken explanation must be recorded honestly; commits use today's actual date.
