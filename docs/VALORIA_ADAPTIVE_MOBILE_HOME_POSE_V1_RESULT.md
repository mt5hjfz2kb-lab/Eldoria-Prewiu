# VALORIA ADAPTIVE MOBILE HOME POSE v1 — RESULT

Status: CLOSED  
Final verdict: **TECH PASS / M0 MOBILE COMPOSITION VISUAL FAIL / M1 MOBILE COMPOSITION VISUAL FAIL / M2 MOBILE COMPOSITION VISUAL PARTIAL / NOT PROMOTED**  
Date: 2026-10-04

## Objective

Determine whether portrait-mobile Valoria can be compositionally resolved by an aspect-ratio-specific orthographic home pose without changing assets, materials, Final Look, Hero Bastion, defense, world layout, parcels or gameplay.

The proof reused the retained SOURCE VISUAL PASS pair:
- Granero: `Valoria_Granero_RCFv1`
- Cuartel: `Valoria_Cuartel_CFSv1`

No new production asset was created.

## Authoritative evidence

Run: **37212276777 SUCCESS**  
Artifact: **11307147019**  
Resolution: **390×844** for M0/M1/M2  
Gameplay-focused tests: **PASS**  
Gameplay collider/hotspot signature: **PRESERVED**  
Tripo credits: **0**  
Paid credits: **0**

Captures:
- `M0-mobile.png`
- `M1-mobile.png`
- `M2-mobile.png`

## Exact camera states

### M0 — current runtime canonical mobile baseline
- projection: ORTHOGRAPHIC
- position: `(18.2, 14.6, -25.8)`
- target: `(0, 3.35, 5.6)`
- orthographic size: **9.1**

### M1 — center adapted, same zoom
The camera and target are translated together so canonical viewing direction is preserved.

- center offset: `(+1.2, 0, -2.8)`
- position: `(19.4, 14.6, -28.6)`
- target: `(1.2, 3.35, 2.8)`
- orthographic size: **9.1**

### M2 — M1 center + mobile-specific orthographic size
- center offset: `(+1.2, 0, -2.8)`
- position: `(19.4, 14.6, -28.6)`
- target: `(1.2, 3.35, 2.8)`
- orthographic size: **12.2**

Candidate portrait threshold tested in code:
- mobile-specific branch at aspect ratio **<= 0.72**
- 16:9 and 4:3 explicitly remain on the existing canonical home pose

The candidate rule was **not integrated into runtime**, because M2 did not pass the visual gate.

## Technical result

**TECH PASS**

Preserved:
- orthographic projection
- canonical camera viewing direction
- RESET Final Look
- Hero Bastion source
- accepted defense
- terrain
- world composition
- parcels/routes/future reserves
- gameplay collider/hotspot signature
- focused interaction/progression tests

Because no camera variant was promoted, runtime navigation/panning/recenter behavior remains unchanged from canonical production code.

## Measured screen-space result

### M0
- Granero projected size: **281.6 × 164.6 px**
- Granero visible: **0.0%**
- Cuartel projected size: **278.2 × 161.2 px**
- Cuartel visible: **90.9%**
- Hero Bastion projected size: **768.5 × 708.9 px**
- Bastion projected height vs viewport: **84.0%**
- Bastion visible bounds: **43.8%**

Interpretation: the Granero is completely outside the portrait viewport while the Bastion dominates the frame.

### M1
- Granero projected size: **281.6 × 164.6 px**
- Granero visible: **3.8%**
- Cuartel visible: **84.8%**
- Hero Bastion projected height vs viewport: **84.0%**

Interpretation: center translation alone does not solve the horizontal crop. It marginally exposes the Granero but gives up Cuartel visibility and leaves the Bastion equally dominant.

### M2
- Granero projected size: **210.1 × 122.8 px**
- Granero visible: **27.4%**
- Cuartel projected size: **207.5 × 120.2 px**
- Cuartel visible: **100.0%**
- Hero Bastion projected size: **573.2 × 528.7 px**
- Bastion projected height vs viewport: **62.6%**
- Bastion visible bounds: **68.0%**

Interpretation: M2 is a meaningful composition improvement. The Bastion remains dominant but no longer consumes almost the whole portrait frame, the Cuartel enters fully, and more of the lower city becomes readable. However the Granero remains mostly outside the left edge. The required simultaneous Granero + Cuartel read is therefore not achieved.

## Visual verdict

### M0 — MOBILE COMPOSITION VISUAL FAIL
- Hero Bastion is excessively dominant.
- Granero is absent from the frame.
- Functional lower city cannot read as a complete district.
- Composition is not acceptable as the target mobile home pose.

### M1 — MOBILE COMPOSITION VISUAL FAIL
- Re-centering alone does not overcome portrait horizontal crop.
- Granero gains only negligible visibility.
- No meaningful functional-read improvement.
- Not promotable.

### M2 — MOBILE COMPOSITION VISUAL PARTIAL
Real improvement:
- much better Bastion/lower-city balance;
- less destructive crop around the central functional district;
- Cuartel fully visible;
- Hero Bastion still unmistakably primary;
- no excessive loss of architectural presence.

Remaining decisive failure:
- Granero is only ~27% visible;
- Granero + Cuartel do not become two simultaneous complete functional reads;
- zooming out further would continue trading architectural scale and premium presence merely to compensate for the current horizontal anchor spread.

Per the anti-loop rule, there is no M3/M4/M5.

## Required answers

### 1. ¿Una home pose mobile específica resuelve mejor la distribución de Valoria?
**SÍ, parcialmente.**

M2 proves that portrait mobile should not mechanically inherit the same home framing. A mobile-specific composition materially improves the hierarchy and useful lower-city space.

But camera-only adaptation does not fully resolve the current functional district.

### 2. ¿Granero y Cuartel pasan a leerse mejor sin cambiar los assets?
**Cuartel sí; Granero no lo suficiente.**

M2 gets Cuartel fully on screen and increases useful lower-city visibility, but Granero remains mostly cropped. Therefore the pair still fails the simultaneous functional-read criterion.

### 3. ¿Hero Bastion mantiene su jerarquía?
**SÍ en M2.**

At orthographic size 12.2 the Bastion still occupies roughly 62.6% of viewport height in projection and remains the dominant visual anchor. The failure is not loss of Bastion hierarchy.

### 4. ¿Debemos tener framing diferente según aspect ratio?
**SÍ como architectural/runtime principle, but the exact tested M2 is not yet promotable.**

The proof demonstrates that portrait mobile benefits from a different home-pose policy. 16:9 should keep its separate framing.

### 5. ¿M1 o M2 deben promoverse como mobile canonical home pose?
**NO.**

M1 fails. M2 is the best variant and a genuine improvement, but it is only PARTIAL because Granero remains mostly outside the frame.

No runtime home-pose rule is promoted from this block.

## Canonical decision

- Orthographic camera remains canonical.
- Current runtime home pose remains canonical for now.
- Do not promote M1.
- Do not promote M2.
- Do not rebuild Granero/Cuartel.
- Do not reopen Screen-Space Breakpoint.
- Forja/Hospital/Cantera rollout remains HOLD.
- No M3/M4/M5.

## Next structural change

The next valid block is **not another camera micro-pass**.

The evidence now points to the projected horizontal spread of the functional district itself. In portrait mobile, the current Granero/Cuartel anchors are too far apart in screen-horizontal space to be simultaneously complete while preserving a sufficiently large Hero Bastion.

Recommended next block:

**VALORIA MOBILE FUNCTIONAL DISTRICT ANCHOR REBALANCE v1**

Scope should be structural but bounded:
- reuse the existing SOURCE VISUAL PASS Granero and Cuartel unchanged;
- preserve Flat Citadel macrocomposition, Hero Bastion, defense, routes, gameplay targets and future reserves;
- adjust only functional-district anchor allocation / parcel presentation enough to bring Granero and Cuartel into one portrait-safe projected band;
- validate against both 16:9 and portrait mobile so the correction does not damage desktop framing;
- one controlled structural proposal, not another asset or camera loop.

## Final answer

**¿VALORIA NECESITA UNA HOME POSE ESPECÍFICA PARA MOBILE Y LA HEMOS RESUELTO? — NECESITA UNA POLÍTICA MOBILE ESPECÍFICA, SÍ; PERO NO LA HEMOS RESUELTO COMPLETAMENTE CON CÁMARA SOLA.**

M2 proves the principle and gives a visibly better portrait composition while keeping Hero Bastion dominant. But the Granero remains mostly cropped, so the exact home pose cannot be promoted. The remaining blocker is now the screen-projected horizontal distribution of the functional district, not asset quality and not another zoom/target tweak.
