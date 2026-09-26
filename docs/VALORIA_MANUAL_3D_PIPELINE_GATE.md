# Valoria manual 3D asset pipeline gate

Date: 2026-09-26  
Status: **technical production blocker — do not resume procedural hero generation**

## Owner decision

The architectural art ceiling reached by primitive composition, simple procedural generation, and code-authored blockout geometry is accepted. These methods are no longer valid for Valoria final architecture.

The next accepted art path must produce one genuinely authored hero architectural asset before Valoria is rebuilt.

## Audit result

### 1. Blender — preferred route

Blender is the best zero-cost route.

Evidence already present in the repository:

- The previous isolated hero study used official Blender 4.5.14 LTS in a Linux authoring workspace.
- The Windows self-hosted Unity runner audit did **not** find `blender.exe` in PATH or under the Blender Foundation program directory.
- The Windows runner does have Unity 6000.3.23f1 and Python.
- The previous Blender asset was produced through `blender -b --python scripts/build_valoria_hero.py`. That proved Blender import/export compatibility, but it is **not an acceptable production method for the next hero asset**, because the geometry was still generated procedurally by code.

Conclusion: Blender is technically sufficient and compatible with the project, but the next asset requires interactive/manual mesh authoring rather than batch generation.

### 2. Equivalent free DCC tools

No equivalent installed Windows DCC was detected in the existing runner audit. `openscad.exe` was also not found. OpenSCAD would not be an appropriate final-art solution for this task anyway because it naturally biases toward constructive/procedural geometry.

### 3. ProBuilder

ProBuilder is not declared by the current Unity project. Even if added, it is approved only for blockout and proportion studies, not final architecture.

### 4. Unity

Unity remains the destination for:

- FBX import;
- material setup;
- URP validation;
- lighting;
- scale and human-reference checks;
- game-camera review;
- isolated art-test capture;
- later scene integration only after visual approval.

Unity must not become the modeling tool used to recreate the rejected primitive workflow.

## Capability boundary of the current Work/session

This session can modify the GitHub repository, run or alter CI workflows, validate imports, generate deterministic captures, and drive the existing Unity self-hosted runner.

It does **not** currently expose an interactive Blender viewport or equivalent DCC control surface in which the agent can perform genuine manual mesh modeling, sculpting, retopology, roof-junction editing, hand-authored facade work, or direct vertex/edge/face art passes.

Installing Blender on the runner and then generating another mesh through Python would technically run Blender but would violate the accepted art-direction constraint. Therefore no new hero asset should be fabricated in this session by batch geometry scripting.

## Required production route

Preferred route:

1. Install/use Blender 4.5 LTS or later compatible LTS on an interactive workstation.
2. Create one connected Valoria hero building manually, starting from silhouette rather than detail.
3. Author the requested tower, palace body, secondary turret, monumental gate, wall section, buttresses, roof hierarchy, balconies/passages, integrated ruin and rock transition as a coherent structure.
4. Hand-author depth-producing geometry: recessed windows, arches, cornices, roof junctions, broken masonry, irregular edges, stairs and transitions.
5. Create a small matching rock family: modular cliff, large rock, rocky edge and dirt/rock transition.
6. Use a first coherent material family for aged light stone, dark ruin stone, slate/tile roof, wood, metal and restrained dirt/moss. Any external texture source must be CC0 or otherwise explicitly compatible and documented.
7. Export the authored asset to FBX and commit both source provenance and the game-ready export.
8. Import into the existing isolated Unity art-test scene. Do **not** replace Valoria.
9. Capture:
   - close camera;
   - actual game-equivalent camera;
   - silhouette against sky;
   - visible human scale reference;
   - Valoria-equivalent lighting;
   - local terrain/rock context.
10. Judge the screenshots before technical green status. If the asset still reads as blockout/procedural, iterate the model; do not promote it.

## Acceptance gate

The first question is not whether EditMode/PlayMode/build pass.

The first question is:

> Does the architecture still look like a Unity/procedural prototype at the real game camera?

If yes, reject and continue authoring.

Only after the visual gate passes should Unity certification and any Valoria integration occur.

## Current decision

**STOP at the production-capability boundary.**

No Valoria scene changes are authorized from this checkpoint.  
No new procedural/code-generated architectural variant is authorized.  
No promotion of the rejected original hero study is authorized.

The project needs either:

- interactive Blender/DCC control available to the production agent, or
- a manually authored external hero asset supplied through a legitimate pipeline.

Until one of those is available, further automatic geometry generation would be misleading progress rather than solving the actual art bottleneck.
