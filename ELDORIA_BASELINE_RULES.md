# Eldoria — Baseline and regression rules

Read this file only for regression/recovery work. Current branch/version/task live in `SESSION_HANDOFF.md` and `PROJECT_STATE.md`.

## Protected recovery points
- Certified functional baseline: `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435` / `baseline/v0.24-certified`.
- Approved visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.
- Recovery points are read-only references. Never develop directly on them.
- Historical v0.19/v0.20/v0.21/r7 material is recovery/reference only.

## Never repeat these regressions
1. Rebuild active code from an older branch/file.
2. Roll back approved visuals while fixing logic.
3. Declare gameplay fixed from syntax/grep/DOM presence alone.
4. Confuse fixture QA with uninterrupted player reachability.
5. Let world pan/touch handlers swallow object actions.
6. Restore giant/global routine action bars or the obsolete long Aldric intro.
7. Give Power as enemy loot.
8. Put cards/relics in inventory instead of Codex.
9. Use historical generation tools in the normal build.
10. Claim a release published/verified before the deployed interaction is exercised.

## Protected behavior
Opening Narrator → brief Aldric → gameplay; mobile-first Valoria loop; object-local actions/timers; finite/renewable world-resource behavior as implemented on current main; Fissure creates Lyra need; Bastion/real troops/Forge/Aether/Maelis/March Trial/final Arc I scaffold; cards → Codex; equipment/materials → inventory.

For acceptance procedure, use `AGENTS.md` and `QA_AND_DEPLOY.md`; this file does not define a competing workflow.
