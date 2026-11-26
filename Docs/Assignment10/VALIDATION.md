# Validation — 2026-11-12 follow-up

Executed locally:
- `python Tools/verify_project.py`: 65 checks passed, including syntax parsing of all 52 C# files for Editor, Windows, Web and iOS preprocessor selections. This is grammar checking, not type checking or Unity compilation.
- `python Tools/verify_campaign.py`: both campaign scenes contain one installer with all three types; spawn colliders and patrol extents are supported by authored ground; initial spawns do not overlap platforms/spikes; roots, idle/walk resources and replay/menu routing resolve.
- Before the later jump fix, compared every pre-existing serialized object in level1/level2 with the original branch snapshot: unchanged. Only a new campaign root and its SceneRoots entry were added.
- 649 asset GUIDs checked for uniqueness, including all new script and texture metadata.
- `git diff --check`: passed.

Source review: acquired-target tracking ignores facing but preserves range and solid-world occlusion; last-known position is retained only for the configured memory; attack facing remains locked during wind-up; movement checks walls and ledges. Animation advances by travelled horizontal distance, uses faster cadence in chase and stops at rest. The optional lab shares the campaign spawner and keeps debug labels.

Campaign changes: Play/replay -> level1; three new enemies in level1 and level2; legacy level2 patrol roots disabled before spawning replacements; original boss unchanged; campaign respects the Enemies toggle. Spawn lists are saved in each CampaignEnemies component.

Patroller tuning from source review: acquisition range 6 -> 8, angle 150 -> 180, sight memory 0.8 -> 1.5 seconds, retention radius 10 added. These are implemented values, not before/after playtest evidence.

Not performed: Unity import/compilation, gameplay execution, Device Simulator rendering, physical iPhone tests, two-person playtest, measured attempts, Web/Windows/iOS builds or recorded video. No native IPA or 2–4-tries result is claimed.

Packaging: full project prepared for a separate task repository. The earlier GitHub repository and PR were not updated for this follow-up; remote repository creation is pending.

Later task: jump velocity and floating-platform collision changes are documented separately in Docs/JUMP_FIX_RU.md. Current source count is 53 C# files; the same 65 project checks and campaign placement checks passed again.
