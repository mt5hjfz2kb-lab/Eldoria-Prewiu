# VALORIA VISIBLE FRAME COHESION v1 — RESULT

Status: **CLOSED / NOT PROMOTED**  
Date: 2026-10-04

## Final verdict

**TECH PASS / VISUAL PARTIAL-FAIL / NOT PROMOTED**

Authoritative matched A/B run: **37216176561 — SUCCESS**  
Artifact: **11309016498**  
Planner run: **37215004613 — SUCCESS**  
Credits: **0 Tripo / 0 paid**

The candidate is not enabled in production and its hook was removed from `ProductionVisualIntegration` after visual review.

## Objective

Test one significant structural cohesion intervention across the actual reachable Valoria frame:
- HOME mobile;
- PAN Granero;
- intermediate pan;
- PAN Cuartel;
- current 16:9 framing.

The intervention targeted:
- visible ground mass;
- edge/periphery continuity;
- architecture-ground contact;
- routes/plazas;
- lower-city cohesion;
- visible planning/support residue.

No new functional building, Hero asset, camera variant or paid/generated geometry was created.

## Baseline

Canonical baseline at block start:
- HEAD: `955fc41b8504d3d2fd14acbd38d32d30266631c3`
- mobile navigation run: **37214258187**
- artifact: **11307314172**

The promoted Mobile Navigable City camera policy remained authoritative throughout.

## Candidate

The final candidate used:
- visual-only irregular `ValoriaGroundKit.TerraceFloor` surfaces;
- existing authored `SlavicMudFlat`;
- existing authored `SlavicFlatRock`;
- existing authored `SlavicStoneFence`;
- no new source geometry;
- no Granero/Cuartel movement;
- no Hero/defense/material/Final Look changes.

It attempted to:
- create a common lower-city urban ground;
- strengthen Granero/Cuartel contact courts;
- connect lateral city shoulders;
- feather city edges into earth/rock;
- replace visible proof-route stones with less blockout-like ground treatment;
- add restrained low parcel/yard edges.

Final technical evidence:
- visual pieces built: **38**
- visible planning proxy renderers suppressed: **4**
- gameplay collider/hotspot signature: **PRESERVED**
- mobile camera policy: **PRESERVED**
- 16:9 framing: **PRESERVED**
- Granero/Cuartel moved: **NO**
- Hero Bastion changed: **NO**
- Final Look changed: **NO**

## Attempt history / anti-loop

### Attempt 1 — TECH PASS / VISUAL FAIL
Run **37215207719**, artifact **11307744277**.

The first technique used authored cobble strips for lateral continuity. Technically valid, but visually rejected because the long bright linear roads read as artificial walkways, especially in PAN Cuartel and 16:9.

Per the anti-loop rule, this was not color-tuned repeatedly; the technique was changed.

### Attempt 2 — TECH FAIL
Run **37215453121**, partial artifact **11308845103**.

Irregular `GroundSeam` was tried instead, but the helper spawned `RockCluster` colliders outside its visual root and therefore changed the gameplay collider signature.

The technique was corrected rather than accepted.

### Attempt 3 — infrastructure crash
Run **37215570203**.

The corrected visual-only source reached rendering, but Unity suffered a native crash inside `Camera.Render`. There was no candidate assertion failure and no useful artifact. An identical retry was justified to distinguish runner instability from a reproducible candidate defect.

### Final retry — authoritative
Run **37216176561 SUCCESS**, artifact **11309016498**.

Both matched capture and focused gameplay tests passed.

No additional aesthetic variant was opened after this evidence.

## Matched evidence

Artifact contains:
- `BEFORE-HOME-mobile.png`
- `AFTER-HOME-mobile.png`
- `BEFORE-PAN-granero-mobile.png`
- `AFTER-PAN-granero-mobile.png`
- `BEFORE-PAN-intermediate-mobile.png`
- `AFTER-PAN-intermediate-mobile.png`
- `BEFORE-PAN-cuartel-mobile.png`
- `AFTER-PAN-cuartel-mobile.png`
- `BEFORE-HORIZONTAL-16x9.png`
- `AFTER-HORIZONTAL-16x9.png`
- `evidence.json`

## Visual review

### HOME mobile
**PARTIAL / effectively too small.**

The extra lower-city ground treatment slightly improves some building-to-ground contact, but the overall frame is almost unchanged. The central city was already dense enough that the candidate does not produce the requested obvious cohesion breakthrough.

### PAN Granero
**PARTIAL / insufficient.**

The Granero-side urban base is marginally more continuous, but the frame remains dominated by the same terrace/support forms and the visible improvement is too small to justify production promotion.

### Intermediate
**PARTIAL / insufficient.**

Some contact seams and shared ground masses become cleaner, but the result still reads largely like the baseline. The key visual hierarchy and weak support forms do not materially change.

### PAN Cuartel
**VISUAL FAIL for promotion.**

The candidate broadens the built ground around the Cuartel side, but the new large surface still reads as a placed platform. It does not convincingly dissolve the city/world seam and risks exchanging one maquette symptom for another.

### 16:9
**PARTIAL-FAIL.**

There is slightly more connected urban ground, but the broader frame still exposes the same fundamental gap: weak/temporary secondary/support architecture and under-authored terrain transitions around a strong Hero core. The candidate does not move the screen enough toward the premium reference.

## Success criteria

1. Less empty terrain: **PARTIAL**
2. Less maquette effect: **PARTIAL, not enough**
3. Buildings integrated into ground: **PARTIAL**
4. Routes/plazas form a continuous city: **PARTIAL**
5. Visible periphery feels finished: **FAIL**
6. Lower city feels more built: **PARTIAL**
7. More richness without losing legibility: **PARTIAL**
8. Mobile scale preserved: **PASS**
9. 16:9 preserved: **PASS technically / PARTIAL visually**
10. Clear convergence toward Visual Bible: **PARTIAL, below promotion threshold**

## Comparison with VALORIA_VISUAL_BIBLE

The candidate moves in the correct conceptual direction:
- city should dominate useful screen space;
- streets/terraces/yards should read as one urban fabric;
- buildings need believable foundation transitions;
- exterior terrain should be context rather than empty protagonist.

But the final captures show that **ground overlays are not the dominant remaining bottleneck**.

The largest visible mismatch is now the quality and integration of the secondary/support mass itself:
- large grey/temporary-looking retaining and support forms;
- lower-city architectural fronts that do not consistently match Hero richness;
- abrupt authored-terrain/support geometry around otherwise strong buildings;
- peripheral city/world transitions whose form language remains too prototype-like.

Adding more ground skins would decorate the current structural weakness rather than solve it.

The screen is still below the approved premium fantasy 4X reference.

## Canonical decision

Do not promote this candidate.

- `ValoriaVisibleFrameCohesionV1.Enabled` remains false.
- The candidate production hook is removed.
- Camera/mobile navigation remains unchanged.
- Granero/Cuartel remain unchanged.
- No Forja/Hospital/Cantera is opened.
- No further curb/road/ground-color variants are permitted from this block.

## Recommended next block

**VALORIA SECONDARY / SUPPORT FORM REPLACEMENT v1**

One bounded screen-space replacement pass targeting the largest visible temporary/proxy structural forms in HOME + pan envelope:
- retaining/support architecture around the lower/middle city;
- obviously greybox-like secondary masses;
- architecture-to-terrain interfaces that are currently large enough to dominate the frame.

Use existing accepted authored production sources where possible; Blender reauthoring only when necessary. Do not add small props or new functional buildings until these large forms are credible.

## Final answer

**¿Valoria se siente ahora más como una ciudad construida y cohesionada y menos como piezas colocadas sobre terreno?**

**Solo ligeramente en el candidato, no lo suficiente para promoción.**

The proof shows that further ground dressing is not the highest-impact route. The next visible leap must replace or reauthor the weak secondary/support masses themselves.
