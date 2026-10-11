# Eldoria Production Art Source Pipeline

Status: **CANONICAL PRODUCTION CLASSIFICATION**  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**  
Effective: 2026-10-04

## Purpose

Separate layout/prototype geometry from player-facing final art so Eldoria stops trying to turn primitive construction into premium architecture by accumulating materials, lights and props.

## 1. Four source classes

### GREYBOX

Purpose:
- layout;
- massing;
- scale;
- parcel reservation;
- route/circulation studies;
- camera tests;
- composition experiments.

Typical sources:
- Unity `CreatePrimitive`;
- cubes, cylinders, cones;
- simple generated gable roofs;
- mathematical wall/tower modules;
- basic Python/Blender primitive assemblies.

GREYBOX may be visually useful, but it is not final architecture.

### SUPPORT

Purpose:
- invisible/low-salience technical support;
- collision;
- interaction proxies;
- occlusion helpers where justified;
- hidden seam/foundation support;
- distant/background geometry whose simplicity is not visible at intended camera.

SUPPORT can ship only when its simplicity is not player-facing at the accepted camera.

### TEMPORARY

Purpose:
- a visual placeholder that has a known production replacement path.

TEMPORARY must have:
- owner/role;
- replacement criterion;
- no claim of production-art completion.

### PRODUCTION ART SOURCE

Requirements:
- authored source identity;
- reproducible `.blend` or equivalent source;
- intentional mesh construction;
- UV/material assignment appropriate to role;
- export path to production GLB;
- Unity import/integration evidence;
- official-camera acceptance.

For architecture, source should normally include meaningful:
- silhouette design;
- wall/roof thickness;
- recessed openings;
- structural trim;
- base/foundation transitions;
- architectural hierarchy;
- reusable pieces;
- macro wear/breakup where visible.

## 2. Technology does not determine class

A Blender file is not automatically production art.

A Python script running inside Blender that creates cubes, cylinders and cones remains GREYBOX unless the resulting geometry independently meets production quality.

Likewise, a hand-placed Unity primitive does not become final art merely because it receives PBR textures.

Classification is based on the visible/source result, not the executable used to create it.

## 3. Current procedural rule

Any architecture whose primary form is created through:
- `CreatePrimitive`;
- cube stacks;
- cylinder stacks;
- low-sided cones;
- minimal generated gable/hip roofs;
- repeated simple mathematical modules;

defaults to **GREYBOX / SUPPORT / TEMPORARY**.

Exception:
it may be promoted only after real zoom 9/mobile evidence demonstrates that it is visually indistinguishable in quality from the accepted production family for its role.

The burden of proof is on promotion.

## 4. Canonical authoring workflow

Production visual authoring should be:

**interactive Blender/Unity authoring -> visual review -> save reproducible source -> deterministic export/integration -> CI certification**

Not:

**edit procedural code -> commit -> CI -> inspect PNG -> repeat indefinitely**

Automation remains essential for:
- reproducible export;
- source identity;
- validation;
- gameplay signature;
- captures;
- artifacts;
- regression testing.

Automation must not substitute for visual art iteration.

## 5. Minimum high-fidelity starter family

First production family:
- main gate;
- wall segment;
- tower;
- civic house;
- workshop / secondary functional building.

Each should share a coherent modular grammar while remaining visually differentiated.

Required architectural vocabulary where applicable:
- recessed doors/windows;
- jambs;
- lintels;
- cornices;
- moldings;
- buttresses;
- structural timber;
- roof thickness/eaves;
- plinths;
- foundation/ground transitions;
- silhouette offsets;
- selective damage/wear;
- reusable trim/modules.

Hero Bastion is the upper quality anchor.

## 6. Source layout

Recommended canonical structure:

`art-source/valoria/production/<family>/`
- `*.blend`
- source textures/masks or manifest references
- export manifest
- role/scale/orientation notes

`Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/<family>/`
- runtime GLB(s)
- generated/import metadata

`pipeline/evidence/<family>/`
- source SHA/identity
- mesh/material/texture report
- export report
- Unity evidence manifest

Do not store generated binary derivatives as the only source of truth.

## 7. Surface contract

Production source must declare material classes from the canonical families:
- Eldoria Stone
- Eldoria Timber
- Eldoria Slate
- Eldoria Ground
- Eldoria Rock
- Eldoria Metal/Accent

Authoring may use:
- trim sheets;
- tileables;
- unique masks;
- baked AO/normal/curvature where justified;
- decals after Unity integration.

Textures should be role-budgeted and validated at real zoom rather than maximized blindly.

## 8. Geometry acceptance

A source geometry gate records at minimum:
- source file SHA;
- exported GLB SHA;
- bounds;
- triangle count;
- material count;
- UV presence;
- normal/tangent state;
- orientation/scale;
- functional role;
- intended parcel/assembly interface.

Triangle count is a budget input, not an art-quality proxy.

## 9. Unity role

Unity owns:
- player-facing composition;
- official camera;
- real gameplay context;
- URP materials/shaders;
- lighting;
- decals;
- probes;
- post-processing;
- LOD/runtime settings;
- final integration and performance validation.

Unity may continue to use primitives for greybox and low-salience support.

Player-facing architecture should consume PRODUCTION ART SOURCE rather than being primarily synthesized at runtime from primitive geometry.

## 10. CI role

CI certifies:
- source/export reproducibility;
- required files;
- deterministic identity;
- Unity compile/import;
- gameplay/collider/hotspot preservation;
- official captures;
- artifacts;
- regression checks.

CI is not the primary art-authoring interface.

A green workflow can only establish TECH PASS. Visual promotion still requires visual evidence.

## 11. Density sequencing

Do not use props/life to hide weak base art.

Production order:
1. composition;
2. production architecture;
3. material/surface stack;
4. final render/look;
5. camera decision;
6. integration;
7. density/life/world exterior;
8. final performance optimization.

## 12. Generative geometry policy

Tripo or another generator may later enter as:
**approved reference -> multiview/high-quality generation -> semantic parts -> Blender cleanup/re-authoring -> modular source -> PBR -> Unity**

Do not restore:
**single image -> monolithic GLB -> destructive decimation -> Unity** as the normal production route.

Credit-consuming generation remains owner-authorized.

## 13. Promotion checklist

Before a piece becomes PRODUCTION ART:
- source is reproducible;
- it is not merely a renamed procedural primitive assembly;
- architecture contains real visible depth;
- materials follow the shared stack;
- source/export identity is recorded;
- Unity integration is correct;
- zoom 9/mobile visibly pass;
- wider official views do not regress;
- gameplay authority remains separate;
- performance is acceptable or has a documented optimization path.

## 14. Current migration rule

Existing procedural Nation 1 or historical visual elements are not deleted automatically.

For each element classify:
- KEEP AS PRODUCTION;
- KEEP AS SUPPORT;
- KEEP AS GREYBOX;
- TEMPORARY UNTIL REPLACED;
- RETIRE.

Do not destructively remove useful layout/composition work before the production replacement is accepted.
