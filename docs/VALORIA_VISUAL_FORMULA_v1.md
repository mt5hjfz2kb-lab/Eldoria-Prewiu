# VALORIA VISUAL FORMULA v1

Status: VALIDATED v1 / FROZEN PRODUCTION BASELINE
Owner intent: canonical visual-direction formula for Valoria until superseded by a validated revision.
Updated: 2026-09-29.

## Purpose

Make Valoria converge visually without reinventing materials, lighting and integration per building. Every new production asset should inherit this formula unless a documented exception is approved.

## Core identity

Valoria is a vertical dark-fantasy fortress-city embedded into rock and mountain: ancient, inhabited, functional, sober, powerful, semi-realistic/stylized, never toy-like and never a collection of unrelated asset-store pieces.

## Material families

### Eldoria Rock
- darker than architectural stone;
- cooler/neutral;
- highly rough;
- strong macro breakup, restrained micro-noise;
- anchors buildings into terrain.

### Eldoria Stone
- medium-value neutral/warm grey;
- readable mortar/joints and AO;
- lower contrast than hero accents but clearly distinct from rock;
- rough rather than glossy.

### Eldoria Timber
- desaturated medium/dark brown;
- controlled warmth;
- visible structural grain and wear without orange saturation;
- separates production/residential architecture from stone mass.

### Eldoria Roof
- among the darkest architectural values;
- slate/charcoal or muted brown-grey;
- low gloss;
- silhouette role is more important than micro-detail.

### Eldoria Ground
- muted earth/stone;
- low saturation;
- supports route readability without competing with architecture.

## Value hierarchy

Default visual hierarchy:
`dark rock / roof / voids -> medium stone / ground -> warm timber accents -> restrained focal accents`

If official-camera captures collapse into one similar grey value, SURFACE FAIL even if geometry is technically correct.

## Lighting baseline

Use the LookDev rig rather than per-building custom lighting.
- warm but restrained directional key;
- cooler/desaturated ambient fill;
- readable soft shadows;
- moderate fog for depth, never enough to wash materials;
- light angle must reveal form at the official isometric camera.

`neutral-overcast` is the canonical production LookDev for v1, matching the real `VisualWorld` environment: ambient 0.70 neutral, fog 0.63 from 28–62, white directional sun 0.90, shadow strength 0.55, rotation 55/-25. Historical candidates remain evidence only and are not active production alternatives.

## Detail hierarchy

Production effort order:
1. silhouette and mass;
2. terraces / stairs / entrances / roofs / large architecture-rock transitions;
3. windows / doors / beams / balconies / buttresses / visible wear;
4. micro texture.

Detail that does not survive official zoom 9/12/19 is not a production priority.

## Architecture-rock integration

Buildings must appear grown from / built into terrain, not placed on top:
- bury bases slightly;
- overlap rock and architecture deliberately;
- use terrain/seam fillers for transitions;
- avoid clean floating cuts;
- preserve circulation/hotspots independently of visual mesh.

## Functional identity

Military: stone-dominant, robust, guarded access, restrained heraldic cues.
Production: more visible timber/function, practical volumes, clear work identity.
Residential: warmer, more roof variation, reduced monumentality.
Hero/Bastion: strongest verticality, architecture-rock drama, macro detail visible from distance.

## Camera rule

Final visual evidence is always integrated at the official fixed isometric camera and supported zooms 19/12/9. Close-up beauty never overrides failure at gameplay distance.

## Mobile rule

Visual quality is allocated where the camera can use it. Triangle and texture budgets are role-specific and measured; the old ~49.8K triangle target remains a reproducible default gate, not a universal final-art requirement.

## Validation sequence

### Gate A — Aserradero
Do not regenerate geometry. Diagnose imported PBR/material structure, then use LookDev and surface corrections to produce `Surface v1`.

### Gate B — Cuartel
Apply the same material/light logic to a second dedicated building. If the recipe needs wholly unrelated treatment, the formula is not yet reusable.

### Gate C — Hero fragment
Apply the same families at higher hero quality to one bounded architecture+rock fragment. Confirm the formula scales upward.

### Gate D — Integrated district
Verify Aserradero + Cuartel + surrounding stone/rock/ground under the same environment. Judge COMPOSITION, SURFACE and IDENTITY separately.

All four gates are green. Gate C uses GateStreetRiseRock MV1 only as a bounded HERO visual fragment; its historical traversal/interface failure remains failed and was not reopened. This document is therefore **VALIDATED v1** and frozen as the production baseline.

## Defect routing

- wrong mass / route / access -> COMPOSITION / geometry;
- washed out / flat -> SURFACE: albedo, roughness, AO, normals, lighting, exposure;
- generic/unrelated style -> IDENTITY;
- floating building / visible seams -> integration/terrain;
- detail invisible at zoom -> simplify / strengthen macro read;
- performance issue -> role budget / LOD / texture policy, not art-direction regression.

Never regenerate paid geometry to fix a confirmed SURFACE-only defect.

## Validation evidence

- Aserradero/Cuartel authored PBR preserved in Unity: run `36498200151`, artifact `11004024446`.
- Canonical LookDev evidence: run `36529204672`; production formula evidence: run `36529730848`, artifact `11015563918`.
- Hero fragment final visual gate: run `36534386365`, artifact `11017959075`, commit `4331a060e534fab14a9e8897f5bda006e8a2b6c9`; 49,799 triangles, geometry unchanged, zero Tripo credits.
- Production environment source of truth: `Unity/Assets/Eldoria/Scripts/Presentation/VisualWorld.cs`.

## Freeze rule

Do not reopen broad tool/look-dev investigation after v1 validation. Revisions require new integrated official-camera evidence of a concrete composition/surface/identity failure or measured target-device constraint. Surface-only defects never justify paid geometry regeneration.

## Current production target

Use this formula to build the first visually finished Valoria district from the real playable topology, reusing certified historical geometry before requesting new paid assets.

## Automation references

- `docs/ELDORIA_SURFACE_PIPELINE.md`
- `docs/ELDORIA_VISUAL_CONVERGENCE_PIPELINE.md`
- `.github/workflows/valoria-lookdev.yml`
- `.github/workflows/aserradero-surface-diagnostic.yml`
- `tools/tripo_module_blender.py`

## Machine-readable production companions

The visual formula is consumed together with:
- `pipeline/visual-quality-profiles.json` — SUPPORT / PRIMARY / HERO asset roles;
- `pipeline/valoria-material-families.json` — canonical Rock / Stone / Timber / Roof / Ground families;
- `pipeline/mobile-visual-performance-gate.json` — mobile evidence required before performance-driven visual compromises;
- `.github/workflows/valoria-visual-formula.yml` — canonical scene-pattern capture gate using real runtime Valoria.

These files prevent future chats/tools from re-inventing quality levels, material logic or validation criteria.
