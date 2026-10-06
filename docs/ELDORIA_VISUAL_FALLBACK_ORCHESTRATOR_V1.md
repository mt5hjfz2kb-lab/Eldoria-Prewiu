# Eldoria Visual Fallback Orchestrator v1

Status: canonical zero-spend routing layer.
Date: 2026-10-06.

## Purpose

Turn the visual R&D audit into an operational fallback system so the owner does not need to name tools manually.

The orchestrator does **not** search for new tools. It classifies a demonstrated failure and selects the already-known route allowed by `pipeline/visual-fallback-policy.json`.

## Contract

Input: one diagnosed failure class.

Output:
- deterministic route;
- already-qualified tools/components;
- conditional tools whose evidence is not yet strong enough for automatic promotion;
- explicit exclusions;
- whether execution may continue automatically.

Unknown failures do not trigger web research or a new generator bake-off. They return `STOP_AND_CLASSIFY`. New R&D can only be reopened by the active convergence authority after a named gate is blocked.

## Failure classes

- `runtime_density_or_gpu`
- `source_contamination_or_framing`
- `geometry_identity_or_silhouette`
- `interaction_clickability_or_collision`
- `depth_or_occlusion_alignment`
- `surface_material_or_relief`
- `semantic_isolation`
- `camera_disocclusion_or_parallax`
- `unknown`

## Examples

`node tools/route-visual-fallback.mjs pipeline/visual-fallback-policy.json geometry_identity_or_silhouette`

Expected behavior: use the existing qualified geometry/correction lane (human/pro mesh, Blender, qualified donors) and explicitly avoid rejected hero routes.

`node tools/route-visual-fallback.mjs pipeline/visual-fallback-policy.json depth_or_occlusion_alignment`

Expected behavior: real geometry first, then existing depth/alignment components where justified; Marigold remains conditional until its integration proof is promoted.

## Safety

- zero spend by default;
- no commercial purchase;
- no credit spend;
- no new R&D family;
- no production-method pivot;
- rejected routes never auto-reopen;
- conditional/in-progress evidence is never silently treated as certified.

This router is subordinate to `VALORIA VISUAL CONVERGENCE & PRODUCTION v1` and does not mutate its current SHARP scale gate.


## Generic integration status — 2026-10-06

Implemented without touching the active SHARP/Unity convergence-owned files:

1. `tools/plan-art-production.mjs` now consumes an optional `failure_class` from any future art-production request and embeds the deterministic fallback route in the generated plan.
2. `.github/workflows/art-production-plan.yml` now invokes the fallback router when the request contains a diagnosed `failure_class`, and persists the route as evidence.
3. `.github/workflows/visual-fallback-route.yml` is a reusable zero-spend GitHub-hosted workflow. Future gates can call it with a diagnosed failure class without waking the Windows/Unity runner.
4. `tools/classify-visual-failure.mjs` provides conservative rule-based classification from explicit diagnostic signals. It auto-classifies only when exactly one known class matches; ambiguous or unknown cases stop instead of guessing.

### Intentionally not connected yet

The active central convergence workstream currently owns the SHARP clean-source, interactive-substrate, gameplay-selection, UnitySplats and related Unity/runtime gate files. This integration does not modify those files.

When that workstream reaches a stable gate boundary, its workflows can emit one of the canonical diagnostic classes/signals and call `visual-fallback-route.yml`. That is the point at which the loop becomes automatic from SHARP/Unity failure -> classification -> fallback selection.

Conditional components such as Marigold or SCoPE remain non-autonomous until their existing evidence is promoted by convergence. Paid/commercial routes remain hard-blocked without fresh owner approval.
