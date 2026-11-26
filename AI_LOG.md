# Assignment 12 AI assistance record

Date: 2026-11-26. Tool: Codex. User request: implement every criterion in the attached Assignment 12 Sound Pass brief on the existing Coinbound project.

Starting point: complete local Assignment 11 working project, including direct keyboard polling, touch input, data-driven campaign, jump/one-way platform fixes and persistent remaining health/JSON progress.

Assistance: read brief; inspected sound/state/UI/save paths; implemented SoundBank/SoundPass, mixer/snapshots, event integration, generated original procedural WAVs and two tracks, added saved sliders and Web pause fallback; wrote source/license table, check tools, recording checklist and script map.

Review/correction: earlier code used one track and a jump sound for enemy shots; pause globally silenced music. These were replaced. A too-broad signal-boundary test was corrected to test the actual clip splice rather than ordinary high-frequency waveform slopes. No Unity execution or phone/video result is claimed.

See DECISIONS.md for choices and Docs/Assignment12/VALIDATION.md for test limits. The student should inspect the code, adjust the audible mix after device testing, and record their own decisions and English explanation. This document summarizes assistance; it is not a verbatim chat transcript.
