# Actual validation status — 2026-11-19

Executed without Unity:
- Tools/verify_project.py: 65 resource/layout/source checks, including grammar parsing of all 72 C# files under Editor, Windows, Web and iOS preprocessor selections.
- Tools/verify_assignment11.py: unique asset GUIDs, data-reference resolution, three LevelData IDs/goals, supported enemy routes, connected platform surfaces, reachable coin estimates, shared scene/build registration, core balance defaults and save-path/pause-hook checks.
- Tools/verify_campaign.py: retained original scene enemy placements and updated data-campaign menu routing.
- Node test of the actual CoinboundSave.jslib: simultaneous flush requests serialize and pending writes trigger a subsequent flush. This does not test real browser IndexedDB persistence.
- git diff --check.

Added but not executed here: Tools → Assignment 11 → Run Data and Save Checks in Unity. It exercises missing/broken/unsupported/invalid saves, JSON replacement/round trips, actual RunProgress HP transfer/unlocks, dead-player completion rejection, explicit restart and reset behaviour in a temporary directory. It must pass in Unity before claiming runtime verification.

Not performed: Unity semantic compilation/import, actual playthrough or rendering, native builds, real iPhone restart/persistence, corruption testing of an installed app, screenshot capture, or the student's narrated MP4. No such evidence or passing result is fabricated.

The original maps were converted to LevelData; Level 2 has an added data-authored stepping stone at x=24 to replace the original moving-platform gap. Legacy scene/script identities are retained. New campaign geometry is generated at runtime, so offline geometry checks are estimates and do not establish collision correctness on a phone.

Keyboard follow-up: direct physical-device polling, compatible no-op serialized callbacks, legacy vector-composite normalization and an editor diagnostic were added. The same 72-file syntax and asset checks passed again. This is not a real keyboard playtest.
