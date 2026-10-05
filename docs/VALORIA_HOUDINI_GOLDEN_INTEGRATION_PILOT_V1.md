# VALORIA HOUDINI GOLDEN INTEGRATION PILOT v1

Status: DESIGN PREPARED / NOT EXECUTED
Date: 2026-10-05

## Objective

Test Houdini as the procedural INTEGRATION layer after a credible hero source exists.

This is a method pivot from "Python designs final visible geometry" to:
"hero source carries identity; Houdini carries repeatable environment integration."

## Inputs

Mandatory:
- winning Gate + Bridge GLB from Golden Generator Bake-Off v1
- canonical Gate/Bridge transforms
- Golden crop boundary
- road/access axis
- shore/water-height curve
- protected gameplay/collider mask
- official-camera visibility mask

Optional:
- high-level rock style masks derived from canonical reference
- vegetation exclusion/priority masks

## HDA concept

Target digital asset:
`ValoriaGoldenEnvironment.hda`

Inputs:
1. hero architecture mesh
2. bridge mesh
3. terrain receiver boundary / footprint
4. shore curve
5. road/access curve
6. protected mask geometry

Promoted parameters:
- `rock_large_scale`
- `rock_mid_scale`
- `erosion_strength`
- `ledge_frequency`
- `bridge_embed_depth`
- `gate_rock_blend`
- `abutment_width`
- `shore_width`
- `wetness_height`
- `vegetation_density`
- `seed`

## Houdini responsibilities

Allowed:
- build continuous terrain/cliff receivers around the hero source
- preserve hero mesh identity
- generate large/mid geological hierarchy
- create erosion/breakup using masks
- fuse bridge landing/abutment geometry into cliff
- generate shore/wet shelf continuity
- produce UVs and material masks
- produce deterministic vegetation scatter masks
- output game-ready meshes/masks for downstream processing

Not allowed:
- replace/redesign the Gate
- replace/redesign the Bridge identity
- move macro composition
- change gameplay route
- use procedural detail to conceal weak source geometry
- add unrelated district content

## Authoring strategy

The first pilot is intentionally small:
- one Golden crop
- one Gate
- one Bridge
- one cliff/shore interface

The HDA must be designed as reusable production tooling, not as a one-off scene graph.

The important product test is:
Can the same HDA accept a different Valoria structure later without rewriting the network?

## Execution modes to evaluate

A. Houdini interactive authoring for HDA construction
- build and tune the node network once
- art-direct masks and transition logic
- save a versioned HDA

B. Automated cooking after the HDA exists
- Houdini batch / Engine
- deterministic parameter JSON
- exported GLB/FBX + masks
- fixed preview renders
- artifact metrics

C. Unity Houdini Engine integration, only after isolated visual success
- cook HDA from Unity editor if useful
- bake final output before runtime
- runtime game must not require Houdini Engine

## First visual gate

Clay-only.

Metrics:
- hero identity preservation >=4/5
- Gate↔Bridge↔cliff contact >=4/5
- geological coherence >=4/5
- silhouette/environment integration >=4/5
- no visible white/background gaps
- official-camera readability >=4/5

If this fails because the winner source itself is weak, return to generator/source input. Do not overbuild Houdini around a bad hero asset.

## Surface handoff

Only after clay gate passes:
- UV/texel-density normalization
- bake AO/curvature/normal/height where useful
- stone/rock/ground/shore masks
- optional Substance Painter or other dedicated surface authoring
- Unity material integration

## Automation design

Automation owns:
- input identity
- parameter JSON
- HDA version identity
- batch cooking
- exported assets
- metrics
- fixed-camera renders
- QA
- artifact persistence

Art direction owns:
- the initial HDA construction
- promoted parameter ranges
- mask semantics
- reference matching

This separation is mandatory.

## Adoption gate

Houdini becomes canonical only if the Golden pilot demonstrates:
1. visual integration >=4/5,
2. reproducible output from fixed inputs/parameters,
3. materially less per-asset custom scripting than the current Blender Python route,
4. viable Unity import/performance path,
5. reuse potential for future Gate/Road/Stair/Bastion/building interfaces.
