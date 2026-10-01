# Valoria Environment Art Direction v1

Date: 2026-10-01  
Status: **CANONICAL VISUAL DIRECTION / applies across future Valoria art chats**  
Repository authority: `main` remains the source of truth.

## Why this exists

The Hero Bastion and Hero District experiments clarified that Valoria's visual problem is not solved by continuously adding isolated 3D models.

The strongest current direction is to treat Valoria as an **environment-art system** built from:

1. composition and silhouette;
2. modular architectural assembly;
3. terrain/rock/architecture integration;
4. coherent shared materials;
5. surface blending and decals;
6. lighting and atmosphere;
7. controlled secondary detail;
8. optimization after visual quality is proven.

This document records the visual conclusions reached across 2026-09-30 and 2026-10-01 so future chats do not revert to the older pattern of “generate model → place model → capture” as the default production method.

---

## Proven facts from current Eldoria work

### 1. Hero architecture matters when silhouette quality is the bottleneck

The generated Hero Bastion demonstrated that one strong frame-specific architectural mass can materially improve the real Valoria frame.

Canonical optimized Hero Bastion:
- SHA-256: `afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`
- optimized geometry: ~49.8K triangles
- canonical review orientation: yaw 180°

The integrated proof established a visible category jump at the focal point without increasing overall scene complexity.

### 2. The Hero District recipe works

The validated Hero District Integration showed that the Bastion only became convincing when its immediate environment was treated as one composition:

**Hero anchor → terrain/terrace seating → rock/masonry transition → coherent surfaces → remove competing weak architecture → local light**

This passed TECH and VISUAL gates in the bounded Hero District frame.

### 3. Simply adding more existing houses/props does not solve mid-tier architecture

The Mid / Lower District experiment improved cleanliness, repetition, lighting and organization but still failed the visual gate.

Conclusion: ordinary placement/reuse of current houses and sheds is insufficient to bridge the quality gap between Hero District and the lower city.

However, this does **not yet prove** that the full modular library has reached its limit. Before new asset generation, architecture must be tested as **composed assemblies**, not as standalone prefabs.

---

## Canonical production principle

### Do not think in “models”. Think in layers and assemblies.

Default workflow should be:

**composition → modular assembly → terrain integration → material system → blending/decals → lighting/atmosphere → detail → optimization**

Not:

**generate model → optimize → place → repeat**

New Tripo generation is justified only when a specific geometric capability is missing after the existing system has been properly tested.

---

## Modular architecture direction

Existing assets should be treated as a **construction vocabulary**, not finished objects.

Examples of families to combine:
- TowerWallRock
- TerraceStairRock
- GateStreetRiseRock MV1
- ResidentialTerraceRock
- StreetLandingTransition
- RockTerrainSeamFiller
- HighStraightWall
- CornerWallL
- RockToWallTransition
- Terrain & Terrace Kit
- Stone Architecture v1
- compatible Slavic architecture
- Mega Fantasy Props where visually compatible
- Ground Kit
- urban props
- vegetation
- rocks
- walls
- gates
- towers
- usable roof elements

Allowed assembly operations:
- rotation;
- reasonable scale variation;
- partial burial;
- controlled overlap/intersection;
- hiding unused geometry inside another mass;
- using rock to absorb foundations;
- using walls as facade extensions;
- using towers as partial corner/vertical masses;
- terraces as architectural basements;
- reuse of roof elements from other prefabs when visually coherent;
- material/tint normalization;
- variation by instance;
- 2–8 modules combined into one apparent building/complex;
- replacing inferior procedural renderers visually while preserving gameplay authority underneath.

A successful assembly should read from the real game camera as **one authored building or urban mass**, not a collage of independent assets.

---

## Unity must be used as an environment-art tool, not only as a model viewer

Future visual work must actively consider the tools below when they materially improve the frame.

Do not add them mechanically; use them where they solve a visible problem.

### Material system

Build toward a compact Valoria material vocabulary:
- stone;
- rock;
- earth/dirt;
- timber;
- slate/roof;
- restrained metal;
- selective blue/gold heraldry;
- violet corruption as secondary threat language.

Prefer:
- shared/master materials;
- controlled instance variation;
- proper albedo/base color;
- normals;
- roughness/smoothness;
- occlusion;
- coherent texel/visual scale.

Avoid hundreds of unrelated materials that make each imported asset look like a different pack.

### Surface variation and blending

Use when appropriate:
- MaterialPropertyBlock;
- vertex color / vertex blending;
- height-based blending;
- triplanar mapping;
- tiled material breakup;
- decals;
- dirt/moss/wear masks;
- rock/terrain seam meshes.

Goal: terrain, architecture and rock should not terminate in hard unrelated borders.

### Decals

A small reusable Valoria decal library is desirable for:
- base dirt;
- moisture;
- cracks;
- moss;
- masonry joints;
- traffic/wear paths;
- soot;
- edge breakup.

Decals should reduce repetition and connect surfaces without requiring unique geometry for every case.

### Terrain / hybrid terrain

Use Terrain or Terrain + mesh where it provides better:
- broad landform continuity;
- slope transitions;
- elimination of visible board edges;
- material blending;
- large-scale ground variation.

Gameplay collision/topology remains authoritative and must not be changed casually.

### Lighting and atmosphere

The city should share one lighting language.

Consider:
- URP lighting response;
- baked lighting/lightmaps where appropriate;
- Light Probes;
- Reflection Probes;
- local warm lights;
- atmospheric fog;
- depth separation;
- restrained color grading;
- warm inhabited areas versus cooler distant/background layers.

Avoid a frame where some imported buildings are blown out while others are nearly black.

### Prefab assemblies

When a modular combination works, convert it into reusable editor/prefab assemblies such as:
- residential mass variants;
- civic corner variants;
- workshop clusters;
- military/civic terrace assemblies;
- retaining-wall/rock transitions.

The goal is several visual buildings from a small vocabulary, not one GLB per building.

---

## Quality hierarchy

The intended city hierarchy is:

**Hero Bastion**
↓
**Hero District / upper terraces**
↓
**mid-tier architecture**
↓
**lower district / access**
↓
**secondary props and life**

Everything must not compete equally.

Hero architecture carries the strongest silhouette/detail.  
Mid-tier architecture bridges the hero and ordinary city.  
Secondary buildings support composition and human scale.

---

## Validation rule

No visual change passes because an isolated asset looks good.

All relevant work must be judged in the **real Valoria frame**.

Minimum deterministic review:
- zoom 19;
- zoom 12;
- zoom 9;
- mobile framing.

Questions:
- Does the full frame improve?
- Does the architecture belong to one city?
- Is the Hero hierarchy preserved?
- Is terrain/architecture integration convincing?
- Does the result survive mobile scale?
- Did gameplay topology remain unchanged?

TECH PASS is separate from VISUAL PASS.

A small decorative improvement is not enough for a visual gate.

---

## Optimization policy

Do not reduce visual quality pre-emptively based on guesses.

First prove the target look.

Then optimize using:
- LODGroup;
- GPU instancing;
- batching where appropriate;
- shared materials;
- culling;
- occlusion where justified;
- texture budget control;
- lightmap strategy;
- device profiling on a representative Android target when available.

A lower polycount is not automatically a better result.

---

## Tripo / Blender policy

Tripo remains useful for:
- hero masses;
- geometry families we demonstrably do not possess;
- specific missing modules after a failed modular-composition proof.

Do **not** default to generating complete buildings when the need can be solved by modular composition.

If a new kit is required, prefer a minimal **Mid-Tier Architecture Kit** containing reusable capabilities such as:
- rich facade;
- corner/vertical mass;
- roof;
- arch/entry;
- upper-floor module;
- balcony/gallery;
- architecture-rock transition;
- connector module.

The purpose is to create multiple buildings from the kit, not one monolithic asset per function.

Blender remains part of normalization/optimization/cleanup, not a substitute for art direction.

---

## Current strategic plan

1. Prove or disprove high-quality modular architecture composition using the existing library on one bad parcel.
2. If PASS, formalize reusable assembly prefabs and extend sector by sector.
3. If FAIL, define the minimum missing Mid-Tier Architecture Kit before any generation.
4. Establish Valoria master/shared material vocabulary.
5. Establish terrain/rock/architecture blending and small decal library.
6. Extend the validated visual language from Hero District through middle and lower districts.
7. Add secondary life/detail only after architecture/composition works.
8. Perform whole-city lighting/atmosphere unification.
9. Optimize only after visual target is reached.
10. Profile on representative mobile hardware before production certification.

---

## Non-negotiable constraints

- Budget: 0 €.
- No paid assets.
- No external hiring.
- No Tripo credit spend without fresh explicit owner authorization.
- Preserve gameplay topology, hotspots, colliders and circulation unless separately authorized.
- Do not destructively modify `Valoria.unity` for art experiments.
- Work in isolated experimental branches until visual proof is accepted.
- Main remains canonical and safe.
- The repo overrides chat recollection.

This direction should be reviewed before future Valoria environment-art work so later chats do not regress to isolated asset placement as the primary strategy.
