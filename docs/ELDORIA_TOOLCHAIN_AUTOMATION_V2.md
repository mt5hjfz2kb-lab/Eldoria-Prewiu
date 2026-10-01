# Eldoria Toolchain Automation v2

Date: 2026-10-01
Status: experimental automation contract; zero-spend by default.

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
