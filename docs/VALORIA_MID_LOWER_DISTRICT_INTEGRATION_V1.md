# Valoria Mid / Lower District Integration v1 — final experimental proof

Closed: 2026-10-01.  
Branch: `visual-proof/mid-lower-district-integration-v1`  
Final visual code: `bb4f0a2a108d0d35461222b3f2ac49ba700e13ea`  
Final run: **36832951872 — SUCCESS**  
Artifact: **11148141589**

Verdict: **TECH PASS / VISUAL FAIL / NOT PROMOTED TO PRODUCTION**.

## Baseline preserved

This proof starts from the already-validated Hero District Integration v1 state, not from raw main visuals.

Preserved:
- Hero Bastion optimized SHA-256 `afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`;
- Hero District integration recipe and bounds;
- certified physical central stair;
- gameplay topology, routes, hotspots and colliders;
- zero Tripo regeneration and zero credit spend.

The BEFORE captures in this block are the validated Hero District state.

## Scope

Only the visible mid/lower band in front of the Hero District was changed.

### Hidden / subordinated
- procedural west-rebuilder house shells;
- procedural lower-town homes and rebuilder shelters;
- redundant lower-band reused-house / hero-frame roofline instances;
- dedicated Cuartel material response was rebuilt in URP and darkened so it no longer competes as a blown-out white foreground focal point;
- dedicated Aserradero response was warmed/subordinated.

### Added / reused
- stable existing Slavic house/shed architecture in fewer, larger groups;
- two buried certified BroadRockPlatform supports;
- RockTerrainSeamFiller at the stair foot;
- smaller CornerWallL court-retaining fragments;
- shared lower approach/court ground skins;
- controlled edge vegetation;
- two restrained local warm lights.

Two attempted uses of `ResidentialTerraceRock` were rejected during iteration because its material path rendered black/blue in this gate. The final accepted code contains no visible instance of that failed treatment.

## Deterministic technical evidence

Matched same-scene BEFORE/AFTER:
- zoom 19;
- zoom 12;
- zoom 9;
- 390×844 mobile.

Safety evidence:
- `same_scene_before_after=true`;
- `collider_hotspot_signature_equal=true`;
- `validated_hero_district_preserved_as_before=true`;
- generated Hero Bastion colliders enabled: **false**;
- generated Hero Bastion hotspots added: **false**;
- Tripo credits: **0**.

Hero bounds remain:
- centre `(0, 7.14, 8.75)`;
- size `(12.8, 9.239, 10.962)`.

## Metrics

BEFORE — validated Hero District:
- 1,604,280 triangles
- 1,207 renderers
- 628 materials
- 24 lights

AFTER — final Mid/Lower proof:
- 1,876,381 triangles
- 1,112 renderers
- 578 materials
- 26 lights

Delta:
- +272,101 triangles
- -95 renderers
- -50 materials
- +2 local lights

Renderer/material count falls because many repetitive/provisional visual shells were removed. Triangle count rises from a small number of already-certified terrain/stone modules and authored replacements.

## Visual assessment

### Improvements that are real
- the front Cuartel no longer reads as the same blown-out white focal competitor;
- the lower-left procedural repetition is reduced;
- the right district has fewer repeated tiny huts;
- ground/court/terrain transitions are less empty and less uniformly flat;
- the stair foot has a more continuous visual transition into the lower city;
- Hero Bastion remains the dominant focal point.

### Why this is still VISUAL FAIL
At zoom 9 and especially mobile, the lower/middle architecture is still visibly a lower visual category than the Hero District:
- the reusable Slavic house/shed family is too simple in silhouette and surface detail;
- the lower civic/military buildings still read as isolated objects on broad surfaces rather than a convincing built-into-mountain urban fabric;
- the foreground still contains visible board-like open ground and weak building-to-ground interfaces;
- using more copies of the existing families would increase density but not close the quality gap;
- the failed ResidentialTerraceRock material experiment confirms that the currently reusable certified inventory does not provide a reliable high-quality residential/mid-tier architecture bridge in this frame.

Therefore further zero-credit rearrangement of the same inventory is unlikely to produce the required category jump without becoming density-for-density.

## Decision

**B) A concrete bottleneck prevents scaling the quality further.**

The bottleneck is now **mid-tier urban architecture + architecture/terrain interface quality**, not the Hero Bastion, stair topology, lighting, or basic terrain composition.

The next justified asset need, if authorized in a later block, is a very small cohesive mid/lower-district family rather than another hero building:
- 1 rock-integrated residential/civic mass;
- 1 secondary workshop/military mass;
- both authored for the real lower parcel sizes and official camera;
- shared Eldoria stone/timber/slate materials;
- built-in buried rock/retaining base;
- no gameplay ownership.

Do not spend Tripo credits or generate this family without explicit owner approval after exact input review.

`main` remains untouched.
