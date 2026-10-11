# Eldoria Toolchain Automation v2

Date: 2026-10-01
Status: canonical production orchestration contract; zero-spend by default.

## Goal

Use Tripo, Blender and Unity as one production system instead of three isolated tools.

Canonical intent:

`visual need -> route decision -> existing-asset proof -> Tripo only for proven geometry gaps -> Blender production processing -> Unity environment-art integration -> official-camera validation -> optimization`

## Routing rule

1. **Composition problem** -> Unity first. Use modular assemblies, terrain/rock integration, shared materials, decals/blending and lighting. Do not generate geometry.
2. **Surface problem** -> Unity material/blending pass first; Blender bake/cleanup only when the source asset needs it.
3. **Geometry problem** -> prove that existing modules cannot solve it. Only then allow Tripo.
4. **Animated character/creature problem** -> separate character route; do not mix it with environment production.

## Tripo role

Tripo is treated as a geometry source/transform stage, not only image-to-GLB.

Capabilities to exploit when validated in our automation:
- single-image and multiview generation;
- generation/segmentation by parts;
- part completion;
- retopology / target polygon budget;
- PBR texture generation/editing;
- rigging for future creatures/characters.

Automation rule:
- every potentially credit-consuming operation is disabled unless a request contains a fresh explicit credit authorization;
- zero-spend probe/staging may run before approval;
- if a capability is UI-only or not yet proven through our bridge/CLI, the automation must report `CAPABILITY_NOT_AUTOMATED` instead of pretending it ran.

## Blender role

Blender is the production processor.

Canonical candidate stages:
- diagnostics and source identity;
- semantic/component split;
- cleanup/normals/UV;
- geometry reduction;
- LOD generation;
- high->low bake proof for normal/AO when useful;
- material consolidation;
- reusable procedural modifiers / Geometry Nodes where they solve repeated production work;
- export + metrics.

Do not apply every stage to every asset. Processing is profile-driven.

## Unity role

Unity is the final environment-art compositor.

Canonical candidate stages:
- modular assembly/prefab variants;
- shared/master materials;
- MaterialPropertyBlock variation;
- decals;
- terrain/mesh integration and seam treatment;
- lighting + probes;
- fog/atmospheric separation;
- LODGroup/instancing/culling after visual proof;
- deterministic 19/12/9/mobile captures;
- gameplay signature protection.

## Profiles

### environment_composition
No Tripo. Existing geometry only. Unity composition/material/light pass.

### environment_surface
No Tripo. Unity materials/blending/decals first; optional Blender bake/cleanup.

### environment_new_geometry
Requires `geometry_gap_proven=true`. Tripo is allowed only after exact-input + visible cost + fresh authorization. Blender + Unity follow automatically after source exists.

### animated_asset
Future character/creature route. Kept separate until rigging/animation automation is certified.

## Safety

- Budget 0 € by default.
- No Tripo spend without fresh explicit authorization.
- Main production topology, hotspots, colliders and circulation remain authoritative.
- Experiments stay isolated until visual proof passes.
- A capability may be known to exist in a tool but still be marked unautomated in Eldoria.

## Canonical-entry rule

This contract is the mandatory first routing step for **all new Eldoria player-visible visual/art production work on `main`**, including Valoria, World 4X regions, future regions, buildings, environment families, creatures/characters and other presentation layers. Valoria remains the most mature reference implementation, not the scope boundary.

Before heavy execution:
1. classify the visual need through one of the supported profiles;
2. represent the intended route in `pipeline/art-production-requests/<workstream-id>.json` (the legacy singleton `pipeline/art-production-request.json` remains compatibility/history only);
3. validate the route with `tools/plan-art-production.mjs` or `.github/workflows/art-production-plan.yml`;
4. execute the selected stages through the existing canonical Unity/Blender/Tripo engines;
5. validate the integrated result at official 19/12/9/mobile views before promotion.

This does **not** mean every stage runs for every request, and it does not make experimental proof tooling a production dependency. The planner chooses the route; the existing execution engines perform the work.

## Current implementation

The request `pipeline/art-production-request.json` is validated by `tools/plan-art-production.mjs`.

The planner produces a deterministic route and blocks invalid combinations, especially:
- Tripo requested without a proven geometry gap;
- credit spend without explicit approval/cost;
- environment requests that skip the no-spend composition/surface proof.

The lightweight workflow `.github/workflows/art-production-plan.yml` runs the planner without waking the Windows runner.

This is the orchestration layer. Existing canonical Tripo/Blender/Unity workflows remain the execution engines until each advanced capability is individually proven and promoted.


## Certification result — 2026-10-01

Final zero-spend proof run: `36859609770`.

Evidence:
- Toolchain planner + static Unity capability audit: artifact `11161505766`.
- Blender/Unity/Tripo integrated capability proof: artifact `11161572190`.
- Hosted Blender bake cross-check: artifact `11160897012`.

Certified now:
- deterministic Blender LOD family generation;
- Blender high-to-low normal-map bake as a technical stage;
- Unity import/gate of the baked low asset with UVs, normals, raycast and captures intact;
- zero-spend route planner;
- read-only Tripo Studio capability inspection with 0 clicks / 0 credits.

Measured bake proof on `ResidentialTerraceRock`:
- source: 49,800 tris / 4 materials;
- baked low: 17,430 tris / 1 material;
- baked normal: 1024x1024;
- Unity: 1 mesh / 1 renderer / 1 material / 1 texture;
- UV + normals present;
- positive raycast PASS;
- empty-space miss PASS;
- all isolated captures non-empty.

Current Tripo Studio session visibly exposes Smart Mesh P2.0 (quads/editing), UV Smart, humanoid rigging/text-to-motion and export. This is **availability evidence**, not authorization to execute those transforms. Parts/part-completion remain unverified in Eldoria automation.

The canonical safety rule remains unchanged: no Tripo credit spend without fresh explicit owner authorization.


## Full-frame convergence orchestration

Toolchain Automation v2 is an execution/routing layer inside the canonical Valoria full-frame loop defined by `docs/VALORIA_FULL_FRAME_CONVERGENCE_LOOP_V1.md`.

The production objective is no longer “successfully process one asset/defect”. The orchestration target is:

`capture full frame -> classify all visible gaps -> route each gap -> execute the complete currently actionable zero-credit batch -> recapture -> integrated visual verdict -> repeat`.

Rules:
- planning may route different defects to Unity, Blender or Tripo, but they remain members of one full-frame iteration;
- a successful individual tool stage cannot terminate the iteration;
- TECH PASS and LOCAL VISUAL PASS are non-terminal;
- only a materially improved integrated 19/12/9/mobile frame may receive FULL-FRAME VISUAL PASS;
- net-negative or imperceptible batches are reverted/disabled rather than accumulated;
- new paid geometry remains blocked by the existing exact-input/cost/fresh-authorization policy;
- after a promoted full-frame pass, owner-facing WebGL publication must be refreshed to that coherent `main` state.

Machine-readable loop contract: `pipeline/valoria-full-frame-convergence-v1.json`.
Validation: `node tools/validate-valoria-full-frame-convergence.mjs`.


## Blender Professional Authoring Pipeline v1 — canonical extension

Player-visible new or materially reauthored environment geometry now follows `docs/ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md` and `pipeline/blender-professional-authoring-standard.json`.

For `environment_new_geometry`, a zero-credit Blender route is a first-class production route. When Tripo is not authorized/required, the planner must route:

`geometry gap evidence -> screen-space art brief -> blender professional authoring -> isolated art-source review -> Unity integration -> official-camera validation`.

Do not treat Blender Python primitive assembly as professional authoring merely because it runs inside Blender. Visible source art must demonstrate deliberate silhouette, depth, repetition control, material relationship and camera-readability.

Required separation of verdicts:
- TECH PASS — source/export/integration/gameplay contract is valid;
- ART SOURCE PASS — isolated source looks intentionally authored and passes the professional-authoring questions;
- VISUAL PASS — integrated official-frame result materially improves the game.

All three are required for promotion of new high-salience geometry.


## Visual fallback orchestrator v1 — 2026-10-06

Toolchain Automation v2 now has a deterministic known-evidence fallback layer:

- policy: `pipeline/visual-fallback-policy.json`
- router: `tools/route-visual-fallback.mjs`
- contract: `docs/ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md`

Rule: after a gate failure is classified, the router selects only already-audited components for that failure class. It does not start broad research, reopen rejected methods, authorize paid work, or replace the active convergence authority.

The owner is not expected to name the technical tool. Production workflows should emit/derive a failure class and consume this router. Conditional components remain conditional until their existing evidence is promoted.

Current integration boundary: the router is implemented and testable independently. Existing active SHARP convergence files are intentionally not modified while their workstream owns them; that workstream may consume the router at its next bounded gate without reopening R&D.


## Universal workstream routing guard — 2026-10-06

Every active workstream must declare `art_production_routing=required|not_applicable` in `pipeline/active-workstreams.json`. `required` means a matching `pipeline/art-production-requests/<workstream-id>.json` must exist and pass the planner. `not_applicable` requires a concrete reason. A real visual failure requires the matching request to be refreshed before another correction iteration. The GitHub-hosted `.github/workflows/art-production-governance.yml` enforces this without consuming the Windows Unity runner.
