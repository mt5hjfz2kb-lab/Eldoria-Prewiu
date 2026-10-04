# VALORIA LOWER-CITY URBAN MASSING REAUTHORING v1 — RESULT

Date: 2026-10-04
Status: **CLOSED / PROMOTED**
Final verdict: **TECH PASS / VISUAL PASS**

## Objective

Reauthor the dominant visible lower-city residential/frontage band so it reads as a small number of differentiated urban groups rather than a compressed cluster of repeated prefab houses.

## Canonical visual reference

Directly reviewed: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.

The reference establishes the target relationship: dominant Bastion, strong secondary architecture, layered urban masses, visible streets/yards between groups, varied roof rhythm and a city fabric that reads as one authored capital rather than many unrelated placed assets.

Valoria remains intentionally less dense than the reference. This block judged convergence in hierarchy, massing, repetition, articulation, cohesion and premium read rather than literal building count.

## Source audit / root cause

The repeated lower-city band was not one bad house family. It was layered presentation systems occupying the same screen region:

- six `Valoria · reused civil house` instances;
- Mid-Tier District v1 presentation layer, including repeated parcel clusters;
- Full Frame Architecture Batch v1 presentation layer, adding another set of similarly scaled urban clusters.

That overlap produced immediate repeated rooflines and compressed same-height masses.

## Route

- Toolchain profile: `environment_composition`
- Planner run: **37221347279 — SUCCESS**
- Existing geometry only
- Blender: **not used**
- New generation: **not used**
- Tripo: **0 credits**
- Paid cost: **0**

## Structural intervention

Redundant presentation renderers/lights from the three overlapping urban systems are suppressed visually. Gameplay objects and authority remain untouched.

Three new authored groups are built from existing certified sources:

1. **West craft court** — broad, lower frontage with an open court toward the central stair.
2. **East merchant front** — strongest secondary lower-city mass, taller core plus lower side house and one dark/slate roof accent.
3. **Upper terrace houses** — smaller elevated group with a clearer vertical silhouette and visible lane separation.

Sources reused:

- `Valoria/MidTierArchitectureKit_v1/Piece01..Piece04`
- `Valoria/TerrainTerraceKit_v1/BroadRockPlatform`
- `Valoria/TerrainTerraceKit_v1/SteppedRockTerrace`
- `Valoria/StoneArchitectureKit_v1/RockToWallTransition`

No primitive production architecture, no new generic house family, no defensive-module substitution, no ground-detail pass and no microprops.

## Authoritative A/B

Run: **37221732105 — SUCCESS**
Artifact: **11309549299**
Head: `5231a9e208cf99d9cc5d6a350f8de17bbdb6ae62`

Evidence:

- gameplay collider/hotspot signature preserved: **true**
- camera policy preserved: **true**
- groups built: **3**
- authored pieces built: **18**
- redundant renderers suppressed: **27**
- redundant lights suppressed: **10**
- Granero/Cuartel moved: **false**
- Hero Bastion changed: **false**
- camera changed: **false**
- Tripo / paid credits: **0 / 0**

Matched BEFORE/AFTER captured for:

- HOME mobile
- PAN Granero
- PAN intermediate
- PAN Cuartel
- 16:9

The artifact also includes the canonical approved reference image used for direct review.

## Visual verdict

### 1. Repeated prefab-cluster effect

**PASS.** A material part of the immediate repeated-house band disappears. The city no longer presents the same number of similarly scaled orange roofs compressed into one continuous strip.

### 2. Two–three differentiated groups

**PASS.** The three groups are legible by position, footprint, height and roof rhythm. They read as separate urban masses rather than one homogeneous band.

### 3. Height hierarchy

**PASS.** The east merchant front and upper terrace produce clearer vertical accents while the west craft court remains deliberately lower. Hero Bastion remains dominant.

### 4. Breathing / streets / courts

**PASS.** The gaps between groups are materially more visible in HOME, intermediate pan and 16:9. The visual structure benefits from negative space instead of filling every gap with another house.

### 5. Credible bases / terrain contact

**PASS for this block.** BroadRockPlatform / SteppedRockTerrace and bounded RockToWallTransition seams give each group a more intentional seat than the previous layered prefab presentation.

### 6. Authored secondary architecture

**PASS.** The secondary city reads more like composed frontage groups. It is still below Hero Bastion in richness, as intended, but the quality gap is smaller.

### 7. Mobile

**PASS.** HOME mobile shows the improvement at first glance: fewer same-height repeated roofs, clearer grouping and a less compressed lower-right mass. PAN Granero and PAN intermediate also gain stronger group separation.

### 8. 16:9

**PASS.** The before/after is especially clear horizontally: the old repeated strip becomes multiple urban clusters with breathing intervals and a stronger height rhythm.

## Direct comparison with canonical reference

### Hierarchy
Closer. Hero Bastion remains the dominant landmark while secondary architecture is now strong enough to support it instead of reading purely as filler.

### Massing
Clearly closer. The reference uses layered groups rather than one repeated roof band; the promoted Valoria frame now follows that principle more closely.

### Repetition
Improved substantially. Immediate repeated silhouettes are reduced, although some orange-roof repetition remains inside individual groups.

### Richness
Improved but still below the reference. The reference has more continuous authored frontage and richer mid-distance building variety.

### Cohesion / integration
Improved. Groups now have more credible bases and clearer spatial relationships. Valoria still exposes more large gray structural/support architecture than the reference.

### Density
Valoria is intentionally less dense. The new spacing does not read as empty because groups are stronger and the central circulation remains clear.

### Premium feeling
Materially improved, but not yet at reference quality. The biggest remaining premium gap is no longer the repeated lower-city housing cluster; it is the exposed gray structural/support vocabulary and uneven richness of some non-Hero secondary architecture.

## Promotion

`ValoriaLowerCityUrbanMassingReauthoringV1.Enabled = true`.

`ProductionVisualIntegration.City` now invokes the promoted pass after Mid-Tier District v1 and Full Frame Architecture Batch v1 are built, allowing the new layer to suppress their redundant presentation while preserving their underlying gameplay-independent infrastructure.

Post-promotion confirmation:

- run **37222027889 — SUCCESS**
- artifact **11309364965**
- captured head `9401da35920da56929546f89767aa73f4dff13e5`
- focused gameplay tests: **PASS**

## Direct answers

**YES.** The lower city now reads as three designed, hierarchical urban groups rather than one compressed cluster of repeated prefabs.

**YES.** This block visibly moves Valoria toward the approved reference even while remaining less dense. The convergence comes from stronger massing, hierarchy and breathing, not from adding more houses.

## Remaining defects

- large gray structural/support pieces are still more exposed and visually dominant than in the reference;
- some individual orange-roof modules still share similar silhouettes inside groups;
- mid-distance secondary frontage richness remains below the approved reference;
- outer environment/edge areas still read more sparse and technical than the capital fabric in the reference.

## Recommended next step

Do **not** reopen lower-city house spacing or add more houses.

Next highest-value block: **VALORIA STRUCTURAL FRAME / URBAN SUPPORT INTEGRATION v1** — replace or reauthor the remaining large gray gate/support masses that visually separate the new urban groups from Hero Bastion, using the now-promoted urban massing as fixed context. The objective should be to make the stone structural frame read as inhabited civic architecture rather than technical supports, without reopening camera, terrain dressing, Granero/Cuartel or lower-city group positions.
