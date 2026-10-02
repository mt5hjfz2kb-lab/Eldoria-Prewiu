# VALORIA REFERENCE CONVERGENCE PASS v2

Status: IN EXECUTION  
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
