# VALORIA REFERENCE CONVERGENCE PASS v2

Status: CLOSED — WORLD FRAME v1 PROMOTED / CROSS-GATE PASS  
Date: 2026-10-02  
Base objective: move the **entire real Valoria frame** toward the owner reference, not beautify isolated assets.

## Reference gaps targeted first

1. Monumental ruined architecture framing the city.
2. Stronger cliff/mountain integration and vertical depth.
3. A layered mountain horizon instead of empty neutral background.
4. Denser controlled vegetation around the compact core.
5. Warm occupation cues, smoke and atmospheric depth.
6. Preserve the certified Hero Bastion as the primary focal point.
7. Do not restore lateral residential sprawl.

## Routing

Planner validation:
- run: **36997739244 — SUCCESS**
- artifact: **11222157636**
- route: `environment_composition`
- stages: `unity_modular_assembly → unity_environment_art → official_camera_validation`
- `geometry_gap_proven=false`
- Tripo disabled / 0 credits


- profile: `environment_composition`
- `geometry_gap_proven=false`
- `allow_tripo=false`
- credit spend: **0**
- execution: Unity composition first.
- Existing library only for v2 iteration 1.

The first pass deliberately reuses existing Mega ruins, cliffs, mountains, vegetation and certified Valoria assets. No new GLB is generated.

## Gameplay protection

All new instances are visual-only:
- colliders disabled;
- hotspots removed;
- no route/floor/progression ownership;
- official gameplay signature must remain unchanged.

## Acceptance

The block does not pass because the skyline contains more objects. It passes only if matched 19/12/9/mobile screenshots move materially toward the supplied reference in:
- monumental frame;
- vertical layering;
- terrain/architecture continuity;
- lived-in density;
- atmospheric depth;
- fantasy-city finish.

TECH PASS and VISUAL PASS remain separate.

## Iteration 1 implementation

Implemented on the proof branch before waking the Windows runner:
- paired monumental broken arches from existing Mega ruin vocabulary;
- ruined tower/passages framing the Hero Bastion without adding residential width;
- side/rear cliff envelope using existing Valoria cliff/hill prefabs;
- five-layer distant mountain horizon;
- controlled tree/bush depth around the compact nucleus;
- restrained warm occupation lights and chimney smoke;
- cooler atmospheric depth while keeping a warm directional key;
- all reused instances are visual-only with colliders disabled and hotspots removed.

The Windows gate is intentionally serialized behind the already-running canonical Unity certification job. No Tripo or Blender work is requested.

## Iteration history

### Iteration 1 — composition + smoke
- Gate run: **36998107463 — FAILURE**
- Cause: presentation assembly does not reference Unity Particle System module.
- Decision: remove smoke rather than expand package/dependency surface for a secondary effect.

### Iteration 2 — smoke removed
- Multiple runner cancellations were caused by stale historical visual-proof jobs occupying or replacing the single Windows runner.
- Workflow governance was tightened so convergence runs supersede only older convergence runs.

### Iteration 3 — stable matched-camera gate
- Gate run: **37002042771 — SUCCESS**
- Artifact: **11223788190**
- TECH: PASS
- Gameplay signature: unchanged
- gameplay_topology_changed=false
- geometry_gap_proven=false
- Tripo credits: **0**
- Metrics before → after:
  - active renderers: 781 → 929
  - unique materials: 74 → 82
  - triangles: 1,647,618 → 2,146,404
  - active lights: 25 → 30
- VISUAL: FAIL
- Reason: monumental ruin prefabs rendered magenta because of incompatible legacy shaders.

### Iteration 4 — safe URP material fallback
- Gate run: **37002340801 — SUCCESS**
- Artifact: **11224421094**
- TECH: PASS
- Gameplay signature: unchanged
- gameplay_topology_changed=false
- geometry_gap_proven=false
- Tripo credits: **0**
- Metrics before → after:
  - active renderers: 781 → 929
  - unique materials: 74 → 77
  - triangles: 1,647,618 → 2,146,404
  - active lights: 25 → 30
- VISUAL: FAIL
- Reason: magenta was removed, but the monumental arches became large flat light blocks. The framing idea is useful, but the material conversion and scale/placement are not production quality.

### Iteration 5 — textured legacy→URP conversion + reframe
Current implementation:
- monumental arches reduced in scale;
- moved farther behind/flanking the Hero Bastion;
- destroyed towers/passages pushed rearward;
- legacy material conversion now preserves available base textures, UV scale/offset and normal maps;
- URP/Lit receives a restrained Valoria limestone tint instead of replacing the source surface with a flat material.

Gate queued as **37003803609**. No credits or new geometry involved.


## Later convergence iterations — ground, framing and runner cleanup

The pass continued through matched-camera 19/12/9/mobile proofs. The important production conclusions are:

- legacy paired gate/tower framing was rejected because it read as two competing fortresses;
- legacy hills/cliffs were rejected where their mixed materials created flat green/grey wedges;
- pass-owned foliage was removed after shader/readability failures;
- oversized prototype/reserve ground renderers were suppressed without touching colliders or gameplay topology;
- the global valley floor was reworked to remove visible tiling and board-edge seams;
- the visual-only `Valoria · Hero Frame valley terrain` was identified as the source of the artificial foreground corona;
- the corona/ring was removed while preserving gameplay;
- legacy `SM_Mountains_11` horizon tests were rejected because the mesh read as detached triangular/pyramidal silhouettes rather than a continuous production mountain wall;
- runner arbitration was corrected so `ValoriaReferenceConvergencePassV2.cs` no longer reserves the heavy Unity slice gate. The dedicated convergence workflow owns these visual-only proofs.

### Current canonical proof

- HEAD: **b7c39254e1f7dfa32ee743f53979b254662b2580**
- Gate run: **37036363667 — SUCCESS** (rerun)
- Artifact: **11240396450**
- TECH: **PASS**
- matched cameras: **true**
- same scene before/after: **true**
- collider/hotspot signature: **unchanged**
- gameplay topology changed: **false**
- geometry gap proven: **false**
- Tripo credits: **0**

Metrics before → after:
- active renderers: **781 → 809**
- unique materials: **74 → 71**
- triangles: **1,647,618 → 1,693,163**
- active lights: **25 → 30**

### Visual verdict

**VISUAL FAIL against the full ELDORIA_VISUAL_BENCHMARK, but materially improved from the starting frame.**

Validated improvements:
- the large green/textured foreground ring is gone;
- the artificial foreground corona/map-edge read is gone;
- the Hero Bastion remains the only dominant monumental focal point;
- prototype planning surfaces no longer dominate the frame;
- ground/material repetition is reduced;
- atmosphere and warm occupation cues are more coherent;
- the compact city footprint remains intact rather than expanding laterally.

Remaining blocker:
- at strategic zooms the world still lacks a production-quality continuous mountain/terrain family capable of enclosing Valoria with believable side/rear mass;
- the available legacy mountain proxy was proven technically usable but visually unsuitable;
- further placement-only iterations with the current legacy terrain vocabulary are now low-return and risk reintroducing detached/floating silhouettes.

## Production conclusion

Reference Convergence v2 has reached the useful limit of **composition-only reuse of the current terrain library**.

Do not continue adding legacy mountain/hill proxies merely to increase object count.

The next high-return visual block should be a dedicated **Valoria World Frame / Mountain Terrain family** (or an equivalent production-quality terrain solution) designed for the official 19/12/9/mobile cameras, while preserving:
- the compact city footprint;
- the certified Bastion and district topology;
- the current gameplay collider/hotspot signature;
- the no-sprawl rule;
- the current visual-only separation between presentation geometry and gameplay authority.


## Final closure — World Frame v1 promoted

The composition-only convergence pass correctly identified the remaining blocker as **missing production-quality continuous world-frame terrain**, not missing city buildings or another hero asset.

That blocker has now been resolved by a dedicated visual-only world-frame implementation:

- runtime implementation: `ValoriaWorldFrameMountainTerrainV1`;
- production promotion commit: **d316adc80dc27d42a85e1051f744c3610f09a1e5**;
- low-poly pass-owned pines were subsequently rejected and removed in **13b67ab159c413fdad4bf5a8ce3cee2c32f37d25**;
- final promoted frame uses one continuous valley/mountain mesh, restrained rock material response and shader-safe boulder occupation only;
- no gameplay collider, hotspot, route or progression ownership was added;
- no Tripo generation and **0 credits** were used for the world-frame solution.

### Final production revalidation

Runtime/art state revalidated from **7e597593c00613c949d74c7c9c7bcd09327ca0ec**:

- **Unity slice source/editor gate:** run **37042030543 — SUCCESS**
- **Valoria World Frame / Mountain Terrain v1:** run **37042030550 — SUCCESS**
  - artifact **11243245466**
- **World Map Visual Formula gate:** run **37042030657 — SUCCESS**

The first Valoria Visual Formula revalidation run (**37042030518**) failed for a **CI staging defect**, not a visual/runtime regression: the clean runner did not stage the external Coherent Castle Proof resources that the canonical gate already expects.

The workflow was fixed to stage all external proof dependencies deterministically:
- Kenney Castle Kit CC0;
- Blender UV rewrap;
- Slavic World proof prefabs;
- Poly Haven Surface Cell PBR.

Final canonical Valoria Visual Formula:
- run **37043234440 — SUCCESS**
- artifact **11243131294**
- validated on **880f3ced1bc2ea4318f9628e173296a17092fb16**
- production geometry is unchanged from the already-promoted World Frame state; commits after `7e597593...` are CI-only.

## Final verdict

**REFERENCE CONVERGENCE v2: CLOSED.**

The accepted production direction is now:

- compact, vertical Valoria nucleus;
- Hero Bastion remains the dominant focal point;
- continuous side/rear mountain-valley frame instead of floating legacy proxies;
- no lateral residential sprawl;
- no pass-owned low-poly pine curtain;
- visual world-frame remains presentation-only and gameplay-authority-free;
- 19/12/9/mobile and world-map/formula regression gates are green;
- **0 additional Tripo credits**.

Do **not** reopen the old sequence of:
- twin gates/towers;
- legacy green hills/cliffs;
- floating PBR rock platforms;
- synthetic cliff curtains;
- procedural pine walls;
- repeated micro-iterations on `ValoriaReferenceConvergencePassV2` framing.

Future visual work should start from the promoted World Frame v1 and target the next benchmark gap inside the city/material/occupation hierarchy rather than rebuilding the world boundary again.
