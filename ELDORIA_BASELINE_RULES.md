# Eldoria — Baseline and regression rules

## Active baseline
- Canonical editable source: `v0220/index.html`.
- Runtime/API: v0.23.14 at the 2026-09-19 documentation consolidation.
- Active development branch: `development/v0.23-clean`.
- Protected user-approved visual recovery point: commit `f139968ccbfdeb3e1d37f58568187374faf6d1f2`, branch `stable/visual-good-f139968c`. **Never modify it.**
- Historical v0.19/v0.20/v0.21 directories are reference/recovery only.

## Never repeat these regressions
1. Rebuild a new version from an older branch/file.
2. Roll back the approved visual layer while fixing logic.
3. Declare a gameplay bug fixed from syntax, grep or DOM presence alone.
4. Confuse fixture/state QA with uninterrupted player reachability.
5. Let world pan/touch handlers swallow object actions.
6. Render giant/global action bars for routine interactions. Actions belong to the selected object.
7. Reintroduce the obsolete long Aldric tutorial/intro.
8. Give Power as enemy loot.
9. Put cards/relics in Chest/inventory; they belong to Codex.
10. Delete/clean the legacy runtime before proving nothing active depends on it.
11. Use historical generation scripts to rebuild the active file without explicit recovery need.
12. Claim deployment before Pages has completed, or claim visual/mobile validation without an actual browser run.

## Protected behavior
- Opening Narrator → brief Aldric → gameplay.
- Valoria-first loop and mobile-first layout.
- Contextual building/node actions below object; timer above.
- Finite world nodes and real resource rewards.
- Fissure creates the need for Lyra; first Fissure event recruits her, then the Fissure can be defeated.
- Bastion progression, real troops, hero/inventory scaffolding, Forge/Aether/Maelis/March Trial/final Arc I scaffolding.
- Cards → Codex; gear/materials → inventory.
- Public path is built from canonical `v0220/index.html`.

## Acceptance discipline
For a gameplay fix: reproduce → modify canonical source → targeted real Playwright interaction → regressions → deploy → published Chromium check. If any stage fails, document it as open in `PROJECT_STATE.md`.

A visual recovery may compare against `f139968c`, but active logic must not be replaced by that old commit.
