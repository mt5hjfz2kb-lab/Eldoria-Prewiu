# ELDORIA WORKFLOW QUARANTINE v1

Status: canonical governance protection.

## Why this exists

Eldoria preserves a large amount of historical visual R&D because failed experiments are useful evidence. Preservation must not mean that rejected or conditional research remains operationally active.

This guard separates the repository into two classes:

### Canonical brain / production orchestration — ACTIVE
- `pipeline/active-workstreams.json`
- workstream ownership protocol
- art-production routing requests
- `tools/plan-art-production.mjs`
- `tools/check-art-production-governance.mjs`
- known-evidence visual fallback policy/router
- repository architecture guard
- workflow governance
- Valoria full-frame convergence control
- Valoria canonical visual-authority routing
- Unity slice gate
- World Map Visual Formula gate

These are governance and production-control systems. They must not be disabled merely because older visual experiments are retired.

### Historical / conditional R&D — PRESERVED, MANUAL ONLY

Camera-First / Semantic 2.5D workflows are rejected as production methods and remain historical/manual-only.

The following capability workflows are useful only as conditional evidence/tooling and are also manual-only:
- Hunyuan3D capability
- InstantMesh capability
- TRELLIS capability
- TRELLIS2 capability
- TripoSR capability
- photogrammetry donor capability

They may run only when:
1. an active workstream owns the task;
2. the canonical art-production planner/fallback route selects that capability;
3. the owner explicitly dispatches the workflow.

They may not auto-run on push, pull request, schedule or repository dispatch.

## What this does NOT do

- It does not delete historical evidence.
- It does not erase the decision register.
- It does not remove tools that may still be conditionally useful.
- It does not disable the planner, fallback router, full-frame control, architecture guard, visual-authority guard or active production gates.
- It does not modify Region 1 runtime/gameplay.
- It does not authorize paid tools.

## Enforcement

Machine-readable policy:
`pipeline/workflow-quarantine-v1.json`

Checker:
`tools/check-workflow-quarantine.mjs`

CI:
`.github/workflows/workflow-quarantine.yml`

A failure means STOP: either a quarantined workflow regained an automatic trigger or a protected brain workflow disappeared/lost its required automatic governance trigger.

## Mental model

`owner intent → workstream claim → routing classification → planner → known-evidence fallback if needed → execution engine → TECH → ART SOURCE where applicable → VISUAL → promotion`

Historical R&D may inform routing, but it does not sit in that chain unless explicitly selected by the planner/fallback route.
