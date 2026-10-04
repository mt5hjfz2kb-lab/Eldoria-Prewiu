# Valoria Visual Bible

Status: **CANONICAL VISUAL AUTHORITY**  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**  
Effective: 2026-10-04  
Repository authority: live `main` wins over chat history.

## 0. Canonical approved reference image

The owner-approved visual reference is stored in the repository at `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.

This image is the canonical visual comparison anchor for Valoria. Major visual reviews must inspect it directly, not rely only on chat recollection or textual paraphrase.

The reference is a directional quality target, not a literal layout blueprint. Preserve Eldoria's certified gameplay, Flat Citadel geography, progression, navigable-city camera policy and future expansion needs rather than copying the reference composition one-for-one.

Valoria may be somewhat less dense than the reference. Matching its object count or pixel density is not required. A lower-density solution is acceptable only when the played frame still feels premium, cohesive, intentionally built and inhabited, with no large unjustified dead areas and with strong Bastion → functional buildings → lower-city hierarchy.

## 1. Authority and precedence

This document is the single visual-direction authority for player-facing Valoria production.

When visual-direction documents disagree, use this order:

1. `docs/VALORIA_VISUAL_BIBLE.md`
2. `docs/ELDORIA_VISUAL_PRODUCTION_RULES.md`
3. `docs/ELDORIA_VISUAL_BENCHMARK.md` for retained benchmark criteria only
4. `docs/ELDORIA_SURFACE_PIPELINE.md` and `docs/ELDORIA_VISUAL_CONVERGENCE_PIPELINE.md`
5. historical Valoria art-direction/proof documents as evidence only

This Visual Bible explicitly supersedes any historical instruction that makes a mountain, cliff-island or vertically embedded fortress the primary composition.

## 2. Canonical target

Valoria is a **premium medieval fantasy 4X capital under reconstruction**.

The target is:
- semi-realistic / stylized realism;
- luminous and epic, with real depth and contrast;
- monumental without becoming grimdark;
- dense, inhabited and functionally believable;
- readable at strategic mobile scale;
- rich enough that the city, not empty terrain, dominates the principal gameplay viewport.

The Hero Bastion is the main architectural focal point, but the surrounding city must be strong enough that it reads as a capital rather than one hero asset surrounded by prototypes.

## 3. Canonical composition

Preserve:
- Flat Citadel geography;
- parcel progression and future reserved footprints;
- certified gameplay topology, hotspots, colliders and circulation;
- bounded strategic panning;
- future Bastion/district expansion headroom.

The composition must present:
- monumental Bastion integrated into the city fabric;
- dense differentiated architecture around it;
- readable streets, stairs, terraces and functional yards;
- vegetation, cultivation, water and human-scale activity;
- foreground/midground/background depth;
- an exterior world that visibly continues beyond the walls.

The exterior landscape is **context**, not the protagonist.

Do not restore:
- a giant mountain as the primary frame;
- an isolated fortress-city embedded into one dominant mountain mass;
- cliff-island staging;
- a city sitting like a small maquette in a large grass field.

## 4. Architecture language

Core material/construction vocabulary:
- warm architectural stone;
- dark-to-medium timber;
- slate/charcoal roofing;
- restrained metal;
- blue/gold heraldic accents;
- vegetation and soil integrated into built space.

Architecture must communicate real construction.

Final production buildings should visibly contain, where appropriate:
- recessed doors and windows;
- jambs and lintels;
- cornices and moldings;
- buttresses;
- structural timber;
- roofs with thickness, fascia and eaves;
- plinths / foundation transitions;
- silhouette variation;
- selective wear/damage;
- reusable authored modules;
- macro detail that survives zoom 9/mobile.

A building is not production-ready merely because a simple volume receives a good material.

## 5. Quality hierarchy

1. Hero Bastion / government focal architecture
2. gates, walls, towers and upper civic architecture
3. primary functional buildings such as Aserradero, Cuartel, Granero, Forja and Hospital
4. residential/civic/workshop secondary architecture
5. props, life and micro-detail

Do not reduce Hero Bastion richness to match weaker secondary assets. Raise the secondary family toward the Hero standard.

## 6. Screen-space target

At normal gameplay presentation:
- Valoria should occupy most of the useful viewport;
- buildings must remain individually legible;
- the Bastion must feel monumental;
- city edges should transition naturally to world context;
- mobile must not read as a miniature city centered in empty terrain.

Zoom 9 and mobile are the first visual stop gate for new production art. Wider 12/19 validation follows only after the core reads clearly better.

## 7. Surface language

Canonical material families:
- Eldoria Stone
- Eldoria Timber
- Eldoria Slate
- Eldoria Ground
- Eldoria Rock
- Eldoria Metal / Accent

Production surfaces should support, where useful:
- base color/albedo;
- normal;
- perceptual roughness/smoothness control;
- ambient occlusion;
- reusable masks;
- macro variation;
- edge/wear variation;
- dirt;
- selective moss/humidity;
- coherent texel density;
- trim sheets;
- decals for breakup, contact and transitions.

Per-building identity should come from architecture, masks, wear, trim, props and functional cues rather than unrelated material systems.

## 8. Lighting and atmosphere

Valoria is bright/epic, not flat or washed out.

The final look should favor:
- clear key/fill separation;
- readable soft shadows;
- strong contact with the ground;
- warm inhabited accents;
- cooler or quieter distance layers;
- restrained HDR/tonemapping/color grading;
- atmospheric depth;
- reflected/indirect light where it materially improves the frame.

Renderer features are kept only after A/B evidence shows a visible benefit at reasonable cost.

## 9. Density and life

Density is added **after** architecture, surfaces and render quality are credible.

Useful density includes:
- houses and workshops;
- fences;
- varied trees and shrubs;
- gardens;
- fields/crops;
- carts, crates, barrels and work materials;
- smoke, chimneys and torches;
- soldiers and workers;
- paths, borders and small terrain changes;
- water;
- forest and distant relief;
- heraldry;
- functional environmental storytelling.

Every addition should improve at least one of:
- depth;
- scale;
- function;
- life;
- identity.

Do not add noise merely to occupy pixels.

## 10. Camera direction

The current orthographic camera remains canonical until a controlled comparison proves otherwise.

A strategic long-focal-length perspective camera may replace it only when an identical-scene A/B demonstrates a clear improvement in:
- monumentality;
- depth;
- reduction of maquette feeling;
- readability;
- mobile interaction.

Gameplay behavior is not changed for an aesthetic experiment.

## 11. Production-art rule

Production art is authored source geometry and surfaces, not procedural greybox promoted by persistence.

Final production architecture should normally originate from:
- authored Blender source;
- accepted cleaned modular source;
- later, selectively, approved generative geometry followed by Blender cleanup/re-authoring.

Unity is the integration, composition, lighting and runtime environment. It may author supportive environment elements, but primitive-based construction is not automatically final architecture.

## 12. Explicit rejection criteria

A candidate is a visual fail if it primarily reads as:
- cartoon;
- obvious low-poly;
- primitive/blockout geometry;
- a maquette on grass;
- a prefab collage;
- washed out;
- uniformly dark;
- repetitive fortress walls;
- isolated hero architecture surrounded by weak filler;
- decorative density hiding unresolved architecture/surface failures.

A cleaner prototype is not the target.

## 13. External reference question

Every major review must answer:

**Is the screen genuinely moving toward the approved premium fantasy 4X reference, or are we only producing a cleaner version of the prototype?**

If the answer is the latter, do not promote.

## 14. Historical-document disposition

`docs/VALORIA_VISUAL_FORMULA_v1.md`
- retained as validated historical surface/look-dev evidence;
- its “vertical dark-fantasy fortress-city embedded into rock and mountain” identity is superseded.

`docs/VALORIA_ENVIRONMENT_ART_DIRECTION_V1.md`
- retained as historical environment-art/process evidence;
- its useful layering/material/integration principles remain valid where compatible;
- it is no longer the top-level visual authority.

`docs/ELDORIA_VISUAL_BENCHMARK.md`
- retained as the original benchmark contract and review vocabulary;
- this Visual Bible now owns current Valoria direction.

Historical mountain/world-frame/proof documents:
- evidence only;
- may not reopen mountain-dominant composition unless a future explicit promoted revision replaces this Bible.

## 15. Promotion gate

Production promotion requires:
- real source identity and reproducibility;
- gameplay contracts preserved;
- matched BEFORE/AFTER;
- zoom 9 and mobile clear win first;
- 12/19 after the core passes;
- direct comparison against the approved reference;
- honest remaining defects;
- cost/credit record;
- evidence that the system can scale to the next building family.

CI success alone is never a visual pass.
