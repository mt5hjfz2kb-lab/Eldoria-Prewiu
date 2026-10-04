# Valoria Production Art Starter Family v1

Status: **SOURCE DESIGN READY / EXECUTION PENDING RESOURCE RELEASE**  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**

## Objective

Replace the primitive-authored secondary-architecture ceiling with a small reusable production family:

- main gate;
- wall segment;
- tower;
- civic house;
- workshop.

This is not a whole-city rollout.

## Authoring distinction

The new source intentionally changes technique from the Nation 1 primitive stack.

It uses:
- boolean-cut real door/window openings;
- recessed doors/windows;
- arched gate void;
- voussoir arch ring;
- cornices/string courses;
- tapered buttresses;
- machicolation/corbel language;
- roof solids with thickness and overhang;
- projecting floor beams;
- structural timber posts/braces;
- dormer construction;
- porch structure;
- plinth/foundation transitions;
- asymmetric workshop annex/function;
- authored modular trim.

The generator may use Blender primitives as low-level mesh operands, but the production forms are produced through architectural boolean/custom-mesh operations and are not merely a visible stack of cubes/cylinders/cones.

## Reproducible source

Builder:
`tools/valoria-production-art-starter/build_starter_family_v1.py`

Planned source:
`art-source/valoria/production/starter-family/Valoria_StarterFamily_v1.blend`

Planned runtime outputs:
`Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/StarterFamily/*.glb`

Each output receives a `*.production-art.json` manifest following:
`pipeline/valoria-production-art-source-schema.json`.

## Material staging

The source uses the canonical family names:
- Eldoria Stone
- Eldoria Timber
- Eldoria Slate
- Eldoria Metal Accent
- Eldoria Heraldry Blue

The initial Blender materials are authoring placeholders. They do **not** close the material-stack phase. Production PBR/Unity material treatment remains phase D and must replace/augment placeholder surface response before visual promotion.

## Stop gate

The family is not production-certified merely because Blender/export succeeds.

Required before promotion:
1. valid `.blend` + GLB source identity;
2. source audit passes;
3. integration into the accepted Nation 1/Valoria composition without gameplay ownership;
4. matched zoom 9 and mobile BEFORE/AFTER;
5. clear architectural-depth improvement;
6. only then extend to 12/19.

## Reuse interfaces

- Wall segment: repeatable curtain-wall module.
- Tower: corner/intermediate tower.
- Gate: front defensive focal element.
- Civic house: repeatable secondary urban building.
- Workshop: functional lower-city/production building.

Future Granero/Cuartel/Forja/Hospital should reuse the same stone/timber/slate grammar but gain function-specific silhouettes rather than cloning these five assets.

## Current blocker

Execution is intentionally not dispatched while `valoria-nation1-authored-production-v1` owns:
- `github-hosted-blender`;
- `windows-runner-heavy`;
- `valoria-production-composition`.

This is a concurrency constraint, not a technical inability.
