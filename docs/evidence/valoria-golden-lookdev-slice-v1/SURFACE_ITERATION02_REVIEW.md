# Golden Surface iteration02 — VISUAL REVIEW

Verdict: **TECH PASS / VISUAL FAIL / DO NOT PROMOTE**

Authoritative run: **37359824789**  
Artifact: **11365384210**

## Technical / regression
- Unity capture: PASS.
- Focused gameplay tests: PASS.
- Surface authoring: PASS.
- Authored renderers: **33** (up from 20 in iteration01).
- Surface material instances: **71**.
- PBR channel stack: albedo + tangent normal + AO + metallic/smoothness.
- Water: bounded shallow→deep value response + multi-frequency normal response.
- Geometry positions/camera/gameplay: unchanged.
- Tripo credits: **0**.

## Visual verdict

Iteration02 materially expanded coverage to the actual Golden-crop renderers: Lower Gate, Bridge, threshold, roads, source03 contact pieces, adjacent ForegroundBank cliff, existing trees and shore. The water also stopped using the old 9× repeated UV field.

Despite that, the official close crop remains a prototype-quality read:
- stone/rock/ground response is still too weak at screen distance to create premium mid-frequency material separation;
- the adjacent cliff remains visibly faceted/low-poly despite richer shading;
- Gate/Bridge/Road still read as simple geometry carrying procedural material rather than authored premium surfaces;
- shallow/deep water improves value hierarchy but still reads as a simple rendered plane;
- vegetation remains visibly faceted;
- the frame does not approach the canonical reference strongly enough to score 4/5.

The failure is therefore no longer explained by insufficient renderer coverage.

## Estimated quality (0–5)
- material richness: **2.5**
- lighting depth: **2.5**
- contact quality: **2.5**
- terrain/architecture integration: **2.5**
- water/shore integration: **2.5**
- natural integration: **2**
- premium perception: **2**
- mobile readability: **4**

Golden gate: **FAIL**.

## Failure classification
Primary: **SURFACE METHOD / AUTHORED SIGNAL**.  
Secondary: **GEOMETRY** (faceted cliff/vegetation and simple edge profiles).

## Anti-loop decision

Do not spend iteration03 by merely increasing procedural contrast or tiling in Unity.

Iteration03 is reserved for the newly available **Blender-authored persistent Golden Surface V2** route:
- persistent source PNG maps;
- stone/rock/ground at 512²;
- shore/vegetation at 256²;
- albedo + normal + AO + smoothness per family;
- deterministic headless Blender authoring;
- same bounded renderer set and shallow/deep water;
- zero geometry change;
- zero Tripo.

If this third technically valid surface iteration still fails the Golden visual bar, the current surface-only method is exhausted and MUST change. The next method must be bounded local geometry + material visual-shell authoring, not a fourth surface micro-iteration.
