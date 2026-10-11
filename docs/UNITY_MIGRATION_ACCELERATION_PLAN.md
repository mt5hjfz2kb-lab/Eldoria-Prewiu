# Eldoria — Unity Migration Acceleration Plan

Status: active migration execution policy.  
Updated: 2026-09-28.

Read with `UNITY_MIGRATION_PLAN.md`, `UNITY_CORE_CONTRACT.md`, `UNITY_ARCHITECTURE.md` and `UNITY_VERTICAL_SLICE.md`.

## Goal

Move validated product capability from the web reference into Unity faster without turning migration into a rewrite-everything project. The unit of progress is a player-visible, testable capability that becomes authoritative in Unity.

## Migration strategy: vertical capability slices

For every feature, migrate in this order:
1. Contract — pure data/rules and explicit invariants.
2. Fixture parity — deterministic input/output examples captured from the web reference where semantics must be preserved.
3. Unity domain implementation — no presentation dependency.
4. Presentation adapter — Unity UI/world representation.
5. Real interaction test — player path, not direct state mutation.
6. Owner review — focused build/state.
7. Retire duplicate uncertainty — record whether web remains reference, differs intentionally, or is superseded.

Avoid broad subsystem rewrites that cannot be played until several weeks later.

## Migration scoreboard

Every capability must be one of:
- REFERENCE_ONLY — validated only in web.
- CONTRACTED — semantics frozen enough to implement.
- DOMAIN_GREEN — Unity pure rules/tests pass.
- PLAYABLE — Unity player can reach/use it.
- UX_GREEN — novice-player acceptance passes.
- UNITY_AUTHORITY — Unity implementation is accepted as canonical for that capability.

A migration percentage without these states is misleading.

## Recommended execution order

Prioritize systems that unlock many later systems:
1. save/versioning + clock + command gateway + deterministic RNG;
2. wallet/tasks/building upgrade;
3. ownership roster + one canonical march composition;
4. world node lifecycle + travel/return;
5. deterministic combat report;
6. rewards/inventory;
7. mission/chapter orchestration;
8. heroes/equipment;
9. hospital/injuries;
10. Relicario/Codex presentation layers;
11. remaining Bastion I–X content.

## Parity harness

Create machine-readable scenario fixtures instead of comparing whole playthroughs manually. Each fixture contains starting snapshot version, command sequence, clock advances, expected state deltas, expected error codes and expected player-facing milestones.

Use fixtures for rules where parity matters. Visual/UI parity is not byte parity and must be judged separately.

## Build profiles

Unity 6 Build Profiles should eventually keep version-controlled configurations for:
- `Dev-Windows-Fast` — fast owner/CI iteration;
- `Dev-Mobile-Profile` — mobile-quality/performance validation;
- `Release-Mobile` — later signed distribution settings.

Do not switch platform/build profile repeatedly inside the same CI job when avoidable; target changes can force reimports/recompiles.

## Content loading architecture

Do not migrate all current `Resources/` immediately. Use direct references/Resources while the vertical slice is small and stable. Introduce Addressables when city/world content must load asynchronously by district/region, memory pressure requires unloadable groups, post-install content delivery becomes real, or dependency management becomes materially painful.

Premature Addressables would slow migration with little immediate player value.

## Fast-vs-full Unity CI

### Fast migration gate
For normal capability work: pure EditMode tests for affected assemblies, targeted PlayMode test, no unrelated historical renders and no Windows player build unless the change affects build/runtime boot.

### Production migration gate
When a playable capability is ready: all relevant EditMode/PlayMode, build, official owner review path and performance smoke on the affected scene.

### Replacement gate
Before declaring web capability superseded: uninterrupted reachable Unity path, save/reload/offline behavior, ES/EN where applicable, novice UX pass, measured mobile performance and explicit semantic-difference record.

## Mobile-first migration performance budgets

For each official Valoria zoom/device profile record frame time, main thread, render thread, GPU frame time when available, batches/draw calls, triangles/vertices, texture memory, GC allocations during normal interaction, load time and peak memory.

Budgets must be validated on actual target-class mobile hardware before final acceptance; Editor numbers are diagnostics only.

## Parallelizable work

Pure domain migration, art production in isolated gates, UI prototypes against deterministic fixtures, automated parity fixtures and performance benchmark tooling can proceed in parallel. Do not parallel-edit the same production scene/topology without an explicit integration owner.

## Definition of migration progress

A week that ports 10 systems but leaves none playable is weaker progress than a week that moves one end-to-end loop from REFERENCE_ONLY to UNITY_AUTHORITY. Optimize for closed, inspectable slices.
