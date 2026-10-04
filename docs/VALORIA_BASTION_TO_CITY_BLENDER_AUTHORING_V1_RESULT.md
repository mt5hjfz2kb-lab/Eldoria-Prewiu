# VALORIA BASTION-TO-CITY BLENDER AUTHORING v1 — RESULT

Date: 2026-10-04  
Status: **CLOSED**  
Final verdict: **TECH PASS / ART SOURCE FAIL / UNITY NOT RUN / VISUAL NOT RUN / NOT PROMOTED**  
Tripo credits: **0**  
Paid credits: **0**

## Canonical design authority

This source was built from the already-locked design package:

- `docs/VALORIA_BASTION_TO_CITY_FRAME_DESIGN_BRIEF_V1.md`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-spec.json`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-overlay.svg`
- canonical 16:9 base: `AFTER-HORIZONTAL-16x9.png`
- base run: **37221732105**
- base artifact: **11309549299**
- base SHA-256: `ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1`
- canonical reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

The approved chat target image was treated as visual direction only. Where it could conflict with protected regions, the canonical base + brief/spec remained authoritative.

## Source execution

Canonical successful source run:

- workflow: **[PRODUCTION] Valoria Bastion-to-City Blender Authoring v1**
- run: **37230275353 — SUCCESS**
- artifact: **11312479703**
- source persistence commit: **0e68cba321e02677c96b9964cf7c7fb713904afb**

Two earlier attempts were TECH-only failures before geometry generation:

- run **37230021110** — argv ingress failure
- run **37230133214** — Blender argv parsed instead of filtered source args
- run **37230248488** — cancelled as stale by concurrency after a newer canonical fix

These were not artistic iterations and did not create alternate source variants.

## Reproducible source

Blend:
- `art-source/valoria/production/bastion-to-city-blender-authoring-v1/Valoria_BastionToCity_BlenderAuthoring_v1.blend`
- SHA-256: `2b77f20a5b52afcda7f273ff45d10717595e36e8b5dcccc3e5f56550e3676d4e`
- bytes: **1,274,836**

GLB:
- `art-source/valoria/production/bastion-to-city-blender-authoring-v1/Valoria_BastionToCity_BlenderAuthoring_v1.glb`
- SHA-256: `935d66fb6cfb441e4829c2eb75e1ddb7754aa883ee875f8e4aaae423815675b4`
- bytes: **693,112**

Technical geometry metrics:
- production mesh objects: **32**
- evaluated vertices: **6,399**
- evaluated triangles: **12,662**

These metrics are recorded only for reproducibility. They are not evidence of art quality.

## Authoring method

This block intentionally changed method relative to the rejected scripted frame:

- irregular multi-ring lofted primary masses;
- controlled exact boolean voids;
- explicit side returns / wall thickness intent;
- non-uniform authored parapet ribbons;
- stepped receiver masses;
- continuous displaced rock-interlock meshes;
- selective bevel;
- weighted normals;
- procedural material-ready surfaces;
- no mirrored half;
- no Blender cube/cylinder primitive defining the main production silhouette.

The central stair used in isolated review is a **REVIEW_PROXY_ONLY** object and is excluded from production GLB export.

## Isolated evidence

Persisted evidence:

- `pipeline/evidence/valoria-bastion-to-city-blender-authoring-v1/source/01-silhouette-clay.png`
- `pipeline/evidence/valoria-bastion-to-city-blender-authoring-v1/source/02-lit-three-quarter.png`
- `pipeline/evidence/valoria-bastion-to-city-blender-authoring-v1/source/03-matched-camera-proxy.png`
- `pipeline/evidence/valoria-bastion-to-city-blender-authoring-v1/source/source-report.json`

## ART SOURCE review

### 1. Does the silhouette clearly reproduce the approved target image?

**FAIL.**

The overall west/east asymmetry exists numerically, but the read remains dominated by two large tapered retaining blocks flanking the stair. The approved target called for a layered civic retaining architecture with terraces, occupied depth, arcades and multiple visible setbacks. Those features do not dominate the silhouette.

### 2. Do west and east read as different but coherent volumes?

**PARTIAL.**

West is lower/broader and east is taller/heavier, so the intended hierarchy is visible. However, both still belong to the same large-support-block visual category rather than distinct inhabited/civic architectural bodies.

### 3. Is there real architectural depth rather than decoration on boxes?

**FAIL.**

The source report confirms true boolean operations and authored recess geometry were attempted, but the rendered evidence does not communicate the required nested architectural depth. From clay, lit and proxy views, the dominant faces remain broad and planar. Deep arcade/recess hierarchy is not screen-readable enough.

### 4. Is stone/rock transition really modeled?

**FAIL.**

There are organic rock-interlock meshes, but visually they read as limited edge patches rather than architecture genuinely entering and emerging from the cliff. The target requires the stone/rock boundary to be a defining construction logic, not a secondary attachment.

### 5. Does the piece visually belong to Hero Bastion and Valoria?

**PARTIAL-FAIL.**

The warm stone / darker retaining stone / restrained blue palette is directionally compatible, but source form quality is too generic and support-like to inherit the premium Hero Bastion identity.

### 6. At the matched camera, does it start eliminating the technical-support read?

**FAIL.**

The matched proxy still reads immediately as two technical retaining masses with a stair corridor between them. This is the exact read the block was required to remove.

### 7. Would an observer perceive final environment art candidate rather than sophisticated blockout?

**FAIL.**

The evidence reads as an improved structural blockout with material intent, not final environment art.

## Stop-gate decision

Hard questions **1, 3, 4 and 7 fail**.

Therefore:

**ART SOURCE FAIL**

Per the owner rule and BLENDER_PROFESSIONAL_V1 stop-gate:

- **Unity was not run.**
- No matched integrated BEFORE/AFTER was produced.
- No gameplay/collider/hotspot/route/parcel/camera state was touched.
- No production promotion occurred.

## Method limit proven

The source-first gate has now demonstrated something important:

Even after switching away from simple box assembly to lofted masses, booleans and more organic interlock tools, a code-authored Blender construction that defines the high-salience architecture parametrically still converges toward a structural/blockout read.

The failure is no longer “we forgot booleans” or “not enough modifiers”. The missing quality is **artist-led spatial authorship of the whole visible frame**: hand-built mass transitions, inhabited secondary layers, deliberate recess hierarchy, irregular wall returns, stone breakup and cliff integration judged continuously against the exact played-frame target.

## Anti-loop closure

Do **not** open:

- `VALORIA BASTION-TO-CITY BLENDER AUTHORING v2`
- `v3`
- A/B/C variants of this scripted source
- another chain of parameter micro-adjustments
- a recolor/detail pass to try to rescue this geometry

If the Bastion-to-city frame is revisited, the authoring method must change materially again. A valid next method would require direct artist-style DCC editing against the locked target image, or another owner-approved source path followed by substantial manual Blender reauthoring. Tripo remains blocked without fresh explicit authorization.

## Final status

**TECH PASS / ART SOURCE FAIL / UNITY NOT RUN / VISUAL NOT RUN / NOT PROMOTED**
