# Valoria — Building Production Inventory v1

Status: ACTIVE PRODUCTION BASELINE  
Updated: 2026-10-02

## Purpose

Translate the validated web / Bastion I–X design into a concrete Unity art-production queue.

Rule: **the city and gameplay requirements dictate the assets; available assets do not dictate the city.**

Not every system needs a world-space building. A dedicated production asset is justified only when the player-facing function, silhouette, interaction or long-term city growth benefits from a physical structure.

## Production classes

### DEDICATED FUNCTIONAL
A recognizable world-space building with a gameplay role. Normally deserves its own production asset family and reserved upgrade envelope.

### HERO
A skyline / orientation / prestige structure. Requires the strongest silhouette and official-camera proof.

### DISTRICT / SUPPORT
Reusable housing, terraces, walls, workshops, storage, terrain fillers, props and visual density. May be assembled from rescued/certified modules and procedural/dressing layers.

### UI / META — NO BUILDING
A first-class game system that does **not** consume a physical Valoria plot unless a later explicit design decision changes this.

---

## Bastion I–X production inventory

| Bastion | Gameplay / system | Physical art requirement | Class | Current production state | Next art action |
|---|---|---|---|---|---|
| I | Bastion core | Central stronghold / progression anchor | HERO | **PRODUCTION / PASS — certified Hero Bastion recovered** | Preserve certified Hero Bastion identity; refine only through validated composition/surface work |
| I | Aserradero / wood | Dedicated production building | DEDICATED FUNCTIONAL | **PRODUCTION / PASS** | Preserve PBR and future upgrade envelope |
| II | Cuartel / troops | Dedicated military building | DEDICATED FUNCTIONAL | **PRODUCTION / PASS** | Preserve PBR; add training/military dressing by progression state |
| III | Granero / food | Dedicated food/storage building | DEDICATED FUNCTIONAL | **PRODUCTION / PASS** | Preserve dedicated Granero BIII asset and progression visibility |
| III | Food/survival district | Paths, stores, sacks, carts, civilian activity | DISTRICT / SUPPORT | Partial generic support only | Build with reusable dressing around Granero |
| IV | Priority: Production / Defense / Shelter | Visible consequence, not a new mandatory building | DISTRICT / SUPPORT | Design CLOSED; visual variants pending | Produce reversible dressing/state layers for the three priorities |
| V | Cantera / stone | Dedicated quarry/extraction area and functional identity | DEDICATED FUNCTIONAL | **MISSING** | Reserve extraction plot/envelope; produce Cantera family |
| V | Masonry recovery | Stone piles, hauling, cutting, repaired retaining walls | DISTRICT / SUPPORT | Historical modules reusable | Use Rock/Stone families + support props |
| VI | Forja / equipment | Major authored forge landmark | DEDICATED FUNCTIONAL / PRIMARY-HERO | **MISSING** | Produce after Cantera; must support heat/craft identity without breaking formula |
| VI | Craft district | Fuel, anvils, racks, smoke, materials | DISTRICT / SUPPORT | Missing production dressing | Build reusable craft kit |
| VII | Códice | No dedicated Valoria building required | UI / META — NO BUILDING | **DESIGN CLOSED** | UI/system implementation only; optional environmental knowledge cues |
| VII | Relicario | No dedicated Valoria building required | UI / META — NO BUILDING | **DESIGN CLOSED** | UI/system implementation only; optional mystic/cultural cues |
| VII | Cultural recovery | Civic/mystic dressing may increase | DISTRICT / SUPPORT | Not yet authored | Add only if it helps communicate progression; never fabricate a Códice/Relicario building |
| VIII | Wider-world / Maelis | No mandatory new functional building from current design | DISTRICT / SUPPORT | Narrative/system role defined | Increase inhabited/civic density and world-facing cues |
| IX | Independent mastery | No mandatory new building | DISTRICT / SUPPORT | Progression role defined | Mature military/economic/hero-prep dressing |
| X | Hospital / wounded recovery | Dedicated functional recovery building | DEDICATED FUNCTIONAL | **MISSING** | Reserve plot/envelope and produce before Unity Bastion X parity |
| I–X | Salón de Héroes / hero-equipment management | Physical-building requirement **not automatically implied by current UI responsibility** | REVIEW BEFORE ASSET | Function named in design; world-space necessity not yet proven | Do not purchase/generate until interaction/presentation decision explicitly requires a building |

## Existing reusable production/support inventory

### Production dedicated
- Hero Bastion — PASS / hero anchor.
- Aserradero — PASS.
- Cuartel — PASS.
- Granero BIII — PASS.

### Certified/rescued support families
- ResidentialTerraceRock — inhabited/civic density.
- RockTerrainSeamFiller — terrain/architecture seam burial.
- TerraceStairRock — vertical visual transition when not redundant.
- TowerWallRock — sparse defensive skyline/support.
- StreetLandingTransition — visual street/landing dressing only.
- GateStreetRiseRock MV1 — visual hero/landmark fragment only; historical traversal failure remains out of scope.

These assets can reduce new production cost, but they **do not replace** a missing functional building such as Cantera, Forja or Hospital unless a later dedicated adaptation actually communicates that function.

## Current dedicated-building gap

Known Arc-I dedicated functional art still missing:

1. **Cantera**
2. **Forja**
3. **Hospital**

Bastion remains a separate HERO line rather than an ordinary building gap.

This list is intentionally short. Do not generate a building merely because a menu/system exists.

## Production ordering rule

Default art-production sequence after the current district block:

1. close and validate the current production district;
2. maintain the Bastion Hero Pass only as far as needed to remove dominant provisional silhouette defects;
3. preserve the certified Granero envelope and reserve maximum upgrade envelopes for Cantera / Forja / Hospital in the approved Master Envelope;
4. produce the remaining dedicated functional buildings in **game chronology / district need order**, beginning with Cantera unless a current Unity milestone requires another first;
5. build reusable district dressing alongside each functional building;
6. validate at official zoom 19 / 12 / 9 before promoting;
7. profile representative mobile performance before broad city-scale replication.

## Purchase / generation rule

Before any paid generation, marketplace purchase or new asset-family search:

- identify the exact canonical building/system need;
- verify that an existing production/rescued asset cannot fulfill it honestly;
- define the plot and maximum upgrade envelope;
- define visual function/silhouette at official camera;
- determine whether it is DEDICATED, HERO, SUPPORT or UI/META;
- only then create/buy/generate the asset.

No paid asset should be acquired simply because it looks useful.

## Long-term rule

Bastion I–X is the prologue, not Valoria's ceiling. Every dedicated building family should be authored with enough upgrade headroom that later visual tiers can grow without moving certified roads, interaction corridors or neighboring plots.

Future XI+ buildings remain outside this inventory until their gameplay role is explicitly designed.

## Canonical asset-storage note — 2026-10-02

Asset Library Canonicalization Pass v1 confirms the current dedicated/hero production GLBs and support library are persisted in the repository with committed Unity metadata. The canonical audit is `pipeline/asset-library-canonicalization-audit.json` (26 GLBs / 0 missing `.meta`). Mid-Tier Architecture now lives under `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`; `Resources.Load("Valoria/MidTierArchitectureKit_v1/...")` remains unchanged because the relative Resources path is identical.
