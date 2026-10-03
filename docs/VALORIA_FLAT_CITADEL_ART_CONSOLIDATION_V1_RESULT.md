# VALORIA FLAT CITADEL ART CONSOLIDATION v1

Status: **CLOSED — TECH PASS / VISUAL FAIL**

Branch: `visual-proof/valoria-flat-citadel-art-consolidation-v1`

Source implementation: Flat Citadel Production Uplift final validated implementation `ca018faf089d10e777566974f2bc6fd3e470ede0`.

Source closure evidence:
- run **37137964874**
- artifact **11278977322**
- verdict **TECH PASS / VISUAL FAIL**
- Flat Citadel macro direction locked

Final Art Consolidation implementation HEAD: `bd3c59467410065680aae36677e9015349d1c0a7`

Final implementation validation:
- run **37141270379 — SUCCESS**
- artifact **11280437080**
- artifact digest: `sha256:cec9608f7d14562dbcbe4635911f22cb54fc26fe2dff568a81a509245f326ba3`

Toolchain planner:
- request `valoria-flat-citadel-art-consolidation-v1`
- profile `environment_composition`
- route: Unity first
- Tripo disabled / zero spend
- planner run **37139573234 — SUCCESS**

## Parcel / progression result

The progression audit is recorded in `docs/VALORIA_PARCEL_RESERVATION_MAP.md`.

Confirmed Arc-I world-space requirements extracted from the canonical product progression in `v0220/js/gameplay.js`:

- Aserradero — initial / Bastion I arc
- Cuartel — Bastion II chapter
- Granero — Bastion III chapter
- Cantera de Valoria — Bastion V chapter
- Forja — Bastion VI chapter
- Hospital — Bastion X finale
- Códice and Relicario — UI/meta systems; **no city parcel reserved by default**

Long-range reservations follow `docs/VALORIA_PROGRESSION_MAP_V1.md` and `docs/VALORIA_MASTER_PLAN_V1.md` instead of inventing unsupported future buildings.

Protected plan:
- active: H0 Bastion, F1 Aserradero, F2 Cuartel, F3 Granero;
- Arc-I future: R4 Cantera, R5 Forja, R6 Hospital;
- protected roads: C0 gate→plaza→Bastion, C1 west service, C2 east service;
- future expansion interfaces: XW west, XE east, XU upper civic, XS late special/prestige.

R4/R5/R6 are rendered as prepared/removable construction ground and may not receive permanent houses, mature trees, monuments, large boulders or permanent VFX. The wall deliberately exposes replaceable west/east growth interfaces instead of locking later progression behind hero towers.

## Web / technical research applied

Research was implementation-directed rather than abstract.

### Modular wall / environment construction

- Unity-compatible environment workflow was informed by 80 Level, *Medieval Castle Production: Working with Modular Packs*: modular base pieces should remain simple, while variants, attachments, props, decals, vertex paint, lighting and world-space variation break repetition.
  https://80.lv/articles/001agt-medieval-castle-production-working-with-modular-packs
- 80 Level, *Vertical Slice: Building a Medieval City*: shared dimensions and interchangeable components are used to keep multiple building kits coherent while still producing unique silhouettes.
  https://80.lv/articles/vertical-slice-building-a-medieval-city

Applied result:
- old tower/curtain/tower repetition was replaced by hierarchy classes;
- one main gatehouse;
- quiet curtain groups;
- low pilaster/buttress language;
- only two rear watchtowers;
- explicit XW/XE future expansion gates;
- low rear wall so Hero Bastion remains dominant.

### PBR / URP calibration

- Unity URP Lit reference: non-metal materials use metallic 0; smoothness controls material microsurface response.
  https://docs.unity3d.com/Manual/urp/prebuilt-shader-graphs-urp-lit.html
- Unity smoothness reference: lower smoothness broadens/diffuses reflections and is appropriate to rough masonry compared with polished surfaces.
  https://docs.unity3d.com/Manual/StandardShaderMaterialParameterSmoothness.html

Applied result:
- imported wall/interface pieces are normalized into URP/Lit;
- stone is non-metallic;
- smoothness is constrained to a rough architectural range;
- normal strength is bounded;
- Aserradero / Cuartel / Granero receive controlled material harmonization without replacing the canonical building assets.

### Atmosphere / mobile readability

- Unity Shadow Distance documentation notes that distant real-time shadows are often unnecessary and that fog can disguise the transition while reducing wasted rendering work.
  https://docs.unity3d.com/Manual/shadow-distance.html

Applied result:
- restrained linear haze is used only to contain the empty far field;
- the pass does not recreate a panoramic mountain;
- one soft directional light remains the depth source rather than adding a field of expensive shadow-casting lights.

## Large-block execution

### 1. Wall consolidation

Production Uplift wall renderers are hidden visually, never removed from gameplay.

Final visual wall:
- one continuous low masonry ring with a restrained crenellation cap;
- authored Stone_Gate / Stone_Tower pieces reserved for hero events rather than every curtain span;
- one main gatehouse with twin towers;
- explicit west/east XW/XE future expansion gates;
- only two rear watchtowers outside the main gate;
- local earth/moss transitions.

A late in-scope simplification reduced the authored wall hero-module count from the earlier 35-module assembly to **11**. Real run 37141270379 shows a materially cleaner and more continuous defensive ring, with fewer large repeated tower assets.

**Visual result:** clear improvement over Production Uplift. The wall no longer dominates the whole city or repeats a full tower/curtain kit at every rhythm.

Remaining weakness: the continuous crenellation cadence is itself visibly regular, and the low masonry ring is still simpler than the best Hero Bastion stonework. This is preferable to the heavier 35-module variant, but it is not benchmark-quality final wall art.

### 2. Hero Bastion → city

The macro topography was not changed.

Added:
- split architectural retaining masses;
- retaining wall faces / returns;
- a deeper flared processional stair;
- stone plinths;
- a direct plaza → stair → Bastion axis.

**Visual result:** the access relationship is significantly clearer. The Bastion feels more connected to the plaza.

Remaining failure: the rock pedestal is intrinsic to the current Hero Bastion source and is still unmistakably visible. This block can mask/integrate it, but cannot make the fused rock disappear without editing/re-authoring the Hero asset itself.

### 3. Material language

103 renderers are reported as consolidated in the final evidence after the final wall simplification removed redundant authored curtain modules.

Real capture shows:
- wall and retaining stone now occupy a closer grey/warm-neutral value family;
- the very dark / nearly black imported-wall failure from the first iteration is gone;
- functional buildings remain recognizable while their values are pulled closer together.

Remaining failure:
- the Hero Bastion, Aserradero, Granero and darker support buildings still do not look as though they came from one authored material library;
- secondary roofs and timber response remain inconsistent.

### 4. Secondary architecture

Canonical dedicated building resources were retained:
- Aserradero
- Cuartel
- Granero

No generic replacement family was promoted.

Reserved cottages on future Cantera / Forja territory are visually suppressed in the consolidated result.

**Visual result:** current functions remain separated and readable, with operational yards and enough empty territory for growth.

Remaining failure: the secondary buildings still vary too much in intrinsic asset quality and silhouette language. This is now an **asset-coherence** problem, not a macrocomposition problem.

### 5. Atmosphere / life

Net-negative tree/house experiments from Production Uplift remain rejected.

Art Consolidation:
- suppresses the low-quality procedural perimeter pine family;
- uses only restrained low bush/boulder clusters outside reservation masks;
- retains work props and warm occupancy cues;
- uses contained fog / lighting rather than scenery density.

**Visual result:** cleaner than the earlier mismatched tree pass, but still sparse.

Remaining failure: the surround still reads as a large production test field rather than a finished natural city edge.

### 6. Real HUD presentation

The final artifact contains two complete capture families:

Environment review:
- `after-19.png`
- `after-12.png`
- `after-9.png`
- `after-mobile.png`

Actual canonical Unity HUD review:
- `game-19.png`
- `game-12.png`
- `game-9.png`
- `game-mobile.png`

The HUD is instantiated from the real `SlicePresenter`, not painted into the image.

Important limitation: the current Unity HUD contract is only the Bastion I-II slice. The capture therefore uses that real HUD state independently over the same mature art frame. This proves technical UI/scene coexistence but also exposes the current UI quality gap.

**Visual result:** TECH success, visual insufficiency. The real HUD remains sparse, small and prototype-grade compared with the approved visual reference. It does not hide or solve scene defects.

## Final technical evidence

Final `evidence.json` from artifact **11280437080**:

- gameplay signature preserved: **true**
- macro composition changed: **false**
- hidden Production Uplift wall renderers: **78**
- consolidated wall hero modules: **11**
- visible reserved Arc-I parcels: **3**
- material renderers consolidated: **103**
- Bastion interface modules: **8**
- confirmed future Arc-I plots: **Cantera / Forja / Hospital**
- meta systems without default world plot: **Códice / Relicario**
- long-range growth interfaces: **XW / XE / XU / XS**
- canonical HUD capture: **true**
- visual-state Bastion level: **6**
- Tripo credits: **0**

## Before / after judgment

Against Production Uplift v1:

**Improved clearly**
1. Wall repetition and dominance.
2. Main gate / curtain / tower hierarchy.
3. Growth-aware side-wall interfaces.
4. Bastion access readability.
5. Material response of imported wall pieces.
6. Empty parcels now have an explicit progression purpose.
7. Low-quality permanent dressing no longer consumes future plots.
8. Real gameplay HUD can be captured over the scene.
9. 19 / 12 / 9 / mobile all preserve the same city hierarchy.

**Still below target**
1. Hero Bastion fused rock pedestal still reads as a separate geological base.
2. Functional/support buildings do not yet share one convincing art-family standard.
3. City perimeter / natural surround is too empty and flat.
4. The simplified continuous wall improves hierarchy, but its regular crenellation cadence still reads as procedural/modular.
5. Current Unity HUD is far below the approved reference in visual density, hierarchy and finish.
6. No convincing NPC/population layer exists yet.
7. Lighting/material depth remains weaker than the benchmark.
8. Full frame still reveals that this is assembled from assets of different quality generations.

## Benchmark comparison

Against `docs/ELDORIA_VISUAL_BENCHMARK.md` and the approved Flat Citadel reference:

- **composition / gameplay readability:** substantially closer;
- **Bastion landmark hierarchy:** preserved;
- **growth headroom:** improved and now documented;
- **wall readability:** improved;
- **surface hierarchy:** improved;
- **asset coherence:** still insufficient;
- **natural-world integration:** insufficient;
- **life / occupancy:** insufficient;
- **HUD / finished-game presentation:** insufficient;
- **overall production feel:** still short of benchmark.

The frame is a more coherent and scalable prototype, but it does not yet read primarily as a finished commercial city-builder screenshot.

## TECH verdict

**TECH PASS**

- deterministic Unity capture succeeds;
- 19 / 12 / 9 / mobile BEFORE and AFTER exist;
- 19 / 12 / 9 / mobile canonical-HUD captures exist;
- gameplay collider/hotspot signature is unchanged;
- growth reservations are represented without gameplay ownership;
- no macrocomposition change;
- zero Tripo credits.

## VISUAL verdict

**VISUAL FAIL**

This is a genuine consolidation improvement, but the owner-defined pass condition was that the image stop reading mainly as a prototype. It has not reached that threshold.

The failure is no longer caused by Flat Citadel composition.

## Remaining defect classification

The remaining dominant gaps are now:

1. **IDENTITY / ASSET COHERENCE** — Hero Bastion / functional / support architecture come from visibly different quality and style generations.
2. **HERO ASSET INTERFACE** — the fused Hero Bastion rock pedestal cannot be fully hidden by environment assembly without degrading the scene.
3. **SURFACE / WORLD EDGE** — the natural perimeter needs a coherent production ground/foliage language.
4. **PRESENTATION / UI** — the real Unity HUD is functional but visually prototype-grade.
5. **LIFE / OCCUPANCY** — populated-city cues are below benchmark.

These are not reasons to reopen terrain or macrocomposition.

## Decision

**Keep Flat Citadel locked as Valoria's base. Keep the parcel reservation map as a spatial contract. Do not promote Art Consolidation v1 as final visual production.**

## Recommended next block

Proceed with **VALORIA ASSET COHERENCE & GAME PRESENTATION v1**.

Scope should be deliberately narrower than another environment redesign:

1. Hero Bastion interface audit at mesh/material-submesh level to determine whether the fused rock can be selectively masked/re-authored without replacing the whole hero identity.
2. One explicit shared material palette / trim / roof / timber calibration for Hero + Aserradero + Cuartel + Granero + support architecture.
3. Production-quality perimeter ground / low foliage family that respects `VALORIA_PARCEL_RESERVATION_MAP`.
4. A real Unity HUD visual-uplift pass using the existing UI hierarchy and interaction contract rather than a fake screenshot overlay.
5. Minimal population/activity representation only after the above is coherent.

Do **not** reopen:
- giant mountain;
- terrain islands;
- district-by-district terraces;
- scan plates;
- photographic foreground;
- voxel/fused terrain;
- density used to hide asset-quality problems.


## Closure reconciliation

A wall-simplification commit (`bd3c59467410065680aae36677e9015349d1c0a7`) landed inside this workstream immediately before the first closure bookkeeping completed. The workstream was briefly reopened only to reconcile documentation with the real branch HEAD.

The late variant was not accepted blindly:
- run **37141270379** completed successfully;
- artifact **11280437080** was inspected;
- gameplay signature remains preserved;
- the macrocomposition remains unchanged;
- the wall hero-module count falls to 11;
- the continuous masonry/crenellation ring is visually more coherent than the previous fragmented 35-module wall, although its repeated merlon rhythm remains a visible prototype-quality defect.

This reconciliation does not change the final verdict: **TECH PASS / VISUAL FAIL**.
