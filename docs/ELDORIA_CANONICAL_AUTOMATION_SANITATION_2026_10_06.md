# ELDORIA CANONICAL AUTOMATION SANITATION — 2026-10-06

Status: **PASS / CLOSED**

## Purpose

Close the rejected Semantic 2.5D / Camera-First R&D route without deleting evidence, and prevent historical experimental automation from governing or auto-reactivating future Valoria production.

## Canonical state after sanitation

- `pipeline/active-workstreams.json`: no active Golden Lookdev / Camera-First workstream.
- `pipeline/art-production-request.json`: disabled and marked `RETIRED_HISTORICAL`.
- `SESSION_HANDOFF.md` and `PROJECT_STATE.md`: top authority records the Semantic 2.5D FINAL FAIL / REJECT verdict.
- `AGENTS.md`: permanent directive states the projection-shell route is retired and non-authoritative.
- Historical evidence, source layers, Unity gates and artifacts are retained.

## Workflows retired to manual-only

The following experimental workflows no longer auto-trigger on push/request-file edits. They remain available only for explicit historical reproduction:

- `valoria-camera-first-depth-shell-unity-v1.yml`
- `valoria-camera-first-depth-shell-v1.yml`
- `valoria-camera-first-layered-v1.yml`
- `valoria-camera-first-orthographic-plane-v1.yml`
- `valoria-camera-first-projection-cpu-v1.yml`
- `valoria-camera-first-projection-layered-v2.yml`
- `valoria-camera-first-projection-rerender-v1.yml`
- `valoria-camera-first-projection-v1.yml`
- `valoria-camera-first-reset-target-rerender-v1.yml`
- `valoria-camera-first-reset-target-unity-v1.yml`
- `valoria-camera-first-reset-target-v1.yml`
- `valoria-camera-first-semantic-layers-v1.yml`

Functional Camera-First workflows that are not part of the rejected projection-shell R&D were not retired by this sanitation pass.

## Historical authority

Final gate:
`docs/VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md`

Final Unity proof:
- run **37435725415**
- artifact **11398459304**
- TECH PASS / VISUAL FAIL

## Result

**CANONICAL SANITATION PASS**

Historical evidence is preserved. Rejected routes cannot automatically restart from ordinary request-file edits. New Valoria visual work requires a new explicit workstream and a genuinely different production method.
