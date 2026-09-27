# Valoria — Modular Production Kit

Status: active production inventory.  
Updated: 2026-09-27.

This file answers one question only: **what modular art do we actually have, what function does each family solve, and what is still missing?**

Do not count iterations of the same family as new production modules.

## Certified families

| Family | Functional role | Certified source | Optimized gate | Status |
| --- | --- | --- | --- | --- |
| **TowerWallRock** | defense, skyline, wall/rock mass | `Eldoria_Module_TowerWallRock_50K.glb` | 50,000 tris | ✅ CERTIFIED |
| **TerraceStairRock** | elevation, terrace, stair, vertical urbanism | raw `Eldoria_Module_TerraceStairRock.glb` → canonical 50K output | 49,800 tris | ✅ CERTIFIED |
| **GateStreetRiseRock MV1** | lower entry, open arch, visible ascent, upper landing | `Eldoria_Module_GateStreetRiseRock_MV1.glb` | 49,799 tris | ✅ CERTIFIED |

### Family identities

#### TowerWallRock
Use for:
- defensive anchor;
- tower silhouette;
- short wall;
- architecture integrated with rock.

Do not use it as:
- circulation;
- residential mass;
- repeated filler everywhere.

#### TerraceStairRock
Use for:
- vertical transition;
- intermediate terrace;
- visible stairs/elevation;
- stepped urban mass.

Do not use it as:
- primary gate;
- dominant skyline repeated many times.

#### GateStreetRiseRock MV1
Use for:
- district entrance;
- circulation spine;
- lower-to-upper connection;
- open monumental access.

Canonical Unity orientation: **180° yaw** relative to the original Tripo export.

Functional acceptance:
`lower entry → open arch → visible ascent → upper landing`.

## Current composition proof

**Micro-Valoria three-family** is the current composition proof.

Certified composition:
- 2 × TowerWallRock
- 2 × TerraceStairRock
- 1 × GateStreetRiseRock MV1
- 249,399 instanced triangles

Result:
- technical PASS;
- connected small-district / modular-language PASS;
- not final-art or full-city approval.

Reference: `docs/VALORIA_MICRO_CITY_THREE_FAMILY_GATE.md`.

## Missing kit functions

These are gaps, not approved names for final assets.

| Priority | Needed function | Why |
| --- | --- | --- |
| P1 | **Residential / civic small mass** | Current kit reads too fortress-heavy; Valoria needs inhabited architecture. |
| P1 | **Street / landing transition** | Needed between large modules to avoid dense seams and abrupt joins. |
| P1 | **Rock seam / terrain filler** | Hide pedestal joins and integrate modules into one terrain language. |
| P2 | **Plaza / courtyard edge** | Create breathing space and non-linear urban organization. |
| P2 | **Short wall / parapet / corner** | Flexible small-scale composition without adding another tower. |
| P2 | **Bridge / overhang connector** | Reinforce vertical city identity and cross-level circulation. |
| P3 | **Utility / workshop / market mass** | Storytelling and inhabited-city variety. |
| P3 | **Vegetation / prop clusters** | Final integration and lived-in feel after architecture stabilizes. |

## Production rule

Do not create a new module because it looks different.

Create it only when it fills a **missing functional role** in this inventory.

Before generating:
1. name the missing role;
2. define the connection interfaces;
3. define the official-camera acceptance test;
4. generate the smallest asset that proves that function.

## Active next module — ResidentialTerraceRock (working name)

**Production role:** inhabited residential/civic mass for the middle district.

This is the next P1 family to generate and validate.

Required visual/function brief:
- 2–4 connected residential/civic volumes, not a castle;
- integrated into a modest rock/terrace base;
- one readable small street or passage through/beside the buildings;
- one small landing/terrace that can overlap with TerraceStairRock or GateStreetRiseRock;
- asymmetrical roofline and lived-in silhouette;
- no dominant tower, gatehouse or keep;
- no huge isolated rock pedestal;
- open left/right composition edges so it can sit beside other modules;
- enough vertical variation to read at zoom 12/9 without becoming monumental.

Acceptance at official cameras:
- instantly reads as **inhabited architecture**, not defense;
- remains legible at 19/12/9;
- does not visually block its own street/passage;
- can be placed between existing modules without creating a new fortress silhouette.

Tripo generation strategy:
- start from one isolated 3/4 concept image on a neutral background;
- if spatial passage/terrace becomes ambiguous, escalate directly to dedicated multiview rather than iterating several single-view versions;
- geometry/function clarity is more important than decorative density.

## Next art milestone

The next milestone is **Micro-Valoria 2 — inhabited district**.

Entry conditions:
- current three-family proof stays intact;
- add enough small complementary families to reduce fortress repetition;
- prioritize residential/civic mass + street/landing transition + seam fillers.

Target:
- roughly 6–7 distinct functional families;
- 8–12 placed pieces;
- readable circulation;
- less fortress-only identity;
- still isolated from production `Valoria.unity`.

Only after that composition passes should the project move into final material language, texture consolidation, LOD/instancing and mobile budget work.
