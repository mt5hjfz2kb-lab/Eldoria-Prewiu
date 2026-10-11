# VALORIA SEMANTIC 2.5D CAMERA-FIRST — FINAL GATE

Date: 2026-10-06  
Status: **FINAL FAIL / REJECTED AS ELDORIA VISUAL PIPELINE**

## Scope reviewed

Camera-first / orthographic projection shell using semantic alpha layers, selective depth cards and bounded parallax over preserved functional 3D.

Canonical reference remains:
`docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`

No Tripo. No paid credits.

## Authoritative evidence

### Semantic source
- Source run: **37434039330**
- Artifact: **11397359251**
- Persisted source commit: **868d35aa45c90c05f70b01141f0e87d51101c556**
- Eight semantic/depth layers: far environment, cliff/ground/shore, vegetation mid, gate back/front, bridge back/front, vegetation foreground.

### First Unity integration
- Run: **37434455097**
- Artifact: **11397984610**
- TECH: PASS
- VISUAL: FAIL
- Failure: the far/context quad rendered as an obvious rectangular photograph and lower semantic masks exposed large holes.

### Edge-zero source correction
- Source run: **37434938898**
- Source persist commit: **04f7a3d3143a1065fad78bb998417b9e7791a75f**
- Correction: object-driven context support, 54 px crop-edge fade, border alpha forced to zero.
- Source report: `support_border_max_alpha = 0`.

### Unity alpha correction
- Run: **37435354553**
- Artifact: **11398437953**
- TECH: PASS
- Result: the rectangular border was removed successfully.
- VISUAL: FAIL
- Failure moved from alpha to structural registration: the projected Golden crop remained a visually separate second environment in front of the functional Valoria scene.

### Visual-only legacy renderer suppression
- Run: **37435725415**
- Artifact: **11398459304**
- TECH: PASS
- Gameplay collider/hotspot signature: preserved.
- Legacy renderers disabled visually only: **393**.
- VISUAL: FAIL.

Authoritative v4 inspection:
- HOME: shell remains spatially disconnected from the surviving Bastion/world and creates a large empty/foreign transition zone.
- GOLDEN CLOSE: reference fidelity inside the projected gate is high, but the surrounding transition to inherited world is visibly synthetic and incomplete.
- PAN LEFT / PAN RIGHT: severe disocclusions, duplicated/thickness fragments, exposed holes and card separation become obvious.
- The shell does not provide a production-safe bounded camera envelope without substantially expanding source coverage and hidden-content reconstruction.

## Final gate

| Gate | Verdict | Evidence |
|---|---|---|
| TECH | **PASS** | Unity runs complete, deterministic source, zero spend, no collider/hotspot mutation |
| VISUAL | **FAIL** | Home integration remains visibly detached from functional scene |
| CAMERA ENVELOPE | **FAIL** | Local pan exposes major disocclusions and card seams; zoom/alternate aspect ratios remain structurally fragile |
| OCCLUSION / DEPTH | **FAIL** | Layer ordering produces some parallax, but hidden-content reconstruction is insufficient for reliable foreground/background interaction |
| FUNCTIONAL 3D COMPATIBILITY | **PASS WITH VISUAL CAVEAT** | Functional 3D/colliders/hotspots can remain underneath, but hiding enough legacy renderers to avoid duplication creates unacceptable visual discontinuity |
| PRODUCTION SCALABILITY | **FAIL** | Making the method robust would require much larger per-camera source coverage, systematic disocclusion painting/inpainting, renderer suppression maps and camera-specific maintenance |

## Decision

**3. REJECT — DOES NOT SOLVE THE PRODUCTION PROBLEM**

The method proves one useful capability: a canonical reference can be reconstructed inside Unity as semantic depth cards while preserving gameplay colliders. It does **not** prove a viable Eldoria city pipeline.

To make this approach production-safe, Eldoria would need to author hidden content for the full required camera envelope and maintain camera-specific projection coverage for most of the visible city. That turns the technique into a large 2.5D scene-authoring problem instead of solving the premium-art production problem.

Therefore:
- do not promote this shell to production;
- do not continue micro-adjusting masks, parallax factors, alpha, or inpainting;
- retain all artifacts and code as historical R&D evidence;
- do not delete the functional 3D beneath it;
- do not start the requested BROKEN → REPAIRED state transition, because adoption prerequisite failed;
- zero paid credits spent.

## Canonical next-state rule

A future visual method must materially change the production model. It must not be another iteration of:
- whole-crop photo plates;
- semantic card subdivision of the same crop;
- larger inpainting margins around this shell;
- additional parallax tuning;
- more renderer hiding to make the projection fit.

Historical workflows may remain for reproducibility, but they must be marked non-authoritative and must not auto-govern future Valoria production.
