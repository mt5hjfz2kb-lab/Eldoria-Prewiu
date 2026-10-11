# Valoria manual hero fragment — production brief

Date: 2026-09-26  
Status: **next valid art task when interactive DCC control is available**

## Scope

Do **not** build Valoria, a city, or a complete fortress.

The next proof must be one small, manually authored architectural fragment whose quality is high enough to judge the production pipeline at the real game camera.

Target fragment:

- one dominant tower/facade junction;
- one manually solved roof transition;
- one deep arched opening or window family;
- one asymmetric buttress/cornice sequence;
- one balcony, passage, or projecting stone element;
- one localized ruined/broken section;
- one architecture-to-rock transition;
- one human-scale reference.

The object must read as a designed piece of a centuries-old fortress, not as assembled modules.

## Required Blender operations

The mesh must be authored through direct DCC operations rather than geometry-generation scripts:

- edit-mode vertex/edge/face work;
- extrude;
- inset;
- bevel;
- controlled booleans where they improve openings or intersections;
- sculpt only where it adds irregularity, erosion, broken stone, or rock transition;
- manually resolved roof intersections and eaves;
- manually recessed windows/arches;
- non-uniform buttresses/cornices;
- hand-authored broken masonry and silhouette damage;
- manual architecture/rock blending.

Modifiers are allowed when they support manual authorship, but they must not become a procedural building generator.

## Visual priorities

Priority order:

1. silhouette;
2. depth and overlap;
3. believable structural joins;
4. large/medium detail rhythm;
5. material response;
6. micro-wear.

The fragment must still look intentional with a neutral clay material.

Avoid:

- large flat cuboid faces;
- evenly repeated windows;
- perfectly periodic buttress spacing;
- disconnected roof planes;
- paper-thin openings;
- rock platforms that read as separate bases;
- texture detail used to compensate for weak geometry.

## Material test

Only after the clay silhouette works, prepare a compact family:

- aged light stone;
- darker ruined stone;
- slate/tile roof;
- wood;
- metal;
- restrained dirt/moss.

External textures may be used only with documented compatible licensing, preferably CC0.

## Unity art test

Import the authored FBX into an isolated Unity art-test scene. Do not modify the playable Valoria scene.

Required captures:

- close detail;
- real game-equivalent camera;
- silhouette against sky;
- oblique structural view;
- human scale visible;
- Valoria-equivalent lighting;
- modest terrain/rock context.

## Acceptance question

At real game-camera distance:

> Does this still look like a Unity blockout, procedural construction, or prefab collage?

If yes, reject it and return to Blender.

Technical green status is secondary. Only a visually approved fragment may justify scaling the workflow into the larger Valoria architecture family.

## Current execution limitation

The current ChatGPT session can manipulate GitHub, CI, Unity validation and captures, but has no interactive desktop/Blender viewport control. Plugin discovery also exposed no Blender/desktop integration that would provide direct mesh editing.

Therefore this fragment must **not** be approximated through Python, batch Blender, Unity primitives, ProBuilder final art, or generated geometry.

The next realistic production route is one of:

1. run the task in an environment/session that exposes interactive computer control over the owner's Blender workstation;
2. have a human 3D artist manually author this fragment in Blender from this brief, then commit the .blend/FBX for automated Unity validation;
3. obtain a legitimately licensed high-quality source mesh and manually adapt it in Blender under the same visual gate.

Until one of those routes exists, stop before geometry creation.
