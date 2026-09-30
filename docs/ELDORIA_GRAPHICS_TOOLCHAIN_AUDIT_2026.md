# Eldoria — Graphics Toolchain Audit 2026

Status: active toolchain decision record.  
Updated: 2026-09-29.

## Goal

Reach the intended Eldoria/Valoria visual quality with fewer paid generations, fewer manual transfers and fewer iterations that fix the wrong layer.

## Current stack and real gap

| Layer | Current tool | Assessment | Decision |
| --- | --- | --- | --- |
| Concept/reference | ChatGPT image generation + owner approval | Strong for exact art direction and isolated references | KEEP |
| Image/Multiview -> 3D | Tripo Studio/CLI | Strong geometry generation; already automated and identity-gated | KEEP as primary generator |
| Geometry cleanup | Blender | Reliable, scriptable, already on runner; currently underused | EXPAND |
| Surface/material | Tripo textures + ad-hoc Unity/Blender material handling | Current weakest production layer; Aserradero exposes this gap | EXPAND substantially |
| Runtime integration | Unity 6 URP | Correct target, strong diagnostics, mobile-oriented | KEEP |
| Visual QA | fixed-camera captures + human review | Good foundation, insufficient material/look-dev isolation | EXPAND |

## Primary conclusion

The project does not currently need a replacement for Unity, Blender or Tripo. It needs a stronger **surface/look-development stage** between generated geometry and final Unity integration.

Adding another generator without solving surface integration would increase asset volume while preserving the washed-out/material-coherence problem.

## Tool decisions

### Blender — KEEP + EXPAND NOW

Use Blender as more than a 49.8K decimator. Canonical future responsibilities:
- geometry cleanup and transforms;
- UV validation/generation;
- architecture-aware reduction experiments;
- source material/image diagnostics;
- AO/normal/roughness baking where useful;
- channel packing preparation;
- texture-size policy;
- GLB export identity.

Why: Blender can bake game-oriented normal/AO/material passes and is already scriptable and installed on the runner. This is the lowest-risk improvement.

Risk: baking low-quality AI geometry blindly can preserve artifacts instead of fixing them. Baking must be optional and follow geometry acceptance.

### Unity Rendering Debugger — IMPLEMENT NOW

Use URP Rendering Debugger as a required diagnostic when an integrated asset fails SURFACE. Inspect albedo, metallic, smoothness, AO, normals and lighting complexity before changing geometry.

Why: it identifies whether the defect is imported texture data, material values, lighting or geometry.

Risk: diagnostic views are not final-quality evidence; official-camera captures remain required.

### Poly Haven — APPROVED REFERENCE/SURFACE SOURCE

Poly Haven is approved as a low-risk CC0 source for HDRIs, PBR textures and reference materials. Use it for LookDev calibration and, where stylistically appropriate, source material bases. Do not let photoreal assets dictate Eldoria's identity.

Risk: raw photoreal textures can visually clash with the stylized/semi-realistic target; they require grading/resolution/performance adaptation.

### Material Maker — PILOT-CANDIDATE, FREE

Potential role: procedural tiling materials for stone, roof, timber, mud and terrain; command-line export can fit automation.

Benefit: zero license cost and procedural repeatability.

Risk: weaker mesh-aware painting/smart-mask workflow than dedicated texturing tools; another material graph format to maintain.

Decision: do not make it canonical yet. Pilot only if Blender procedural material authoring becomes a bottleneck.

### ArmorPaint 1.0 — BEST LOW-COST PAINTING PILOT

Potential role: artist-guided PBR painting, AO/curvature/thickness baking, Unity-oriented export. Stable binary is inexpensive compared with subscription tools.

Benefit: gives the project a dedicated PBR painting layer without committing to a recurring subscription.

Risk: smaller ecosystem/support base than Substance; development repository explicitly warns it may not be stable; do not build critical automation around alpha/dev versions.

Decision: first paid texturing tool to A/B if Blender-only look-dev cannot reach target quality. Purchase requires owner approval; no purchase has been made.

### Adobe Substance 3D Painter — PROFESSIONAL CEILING, DEFER PURCHASE

Potential role: hero buildings and hero fragments using baked curvature/AO/world-normal/thickness maps, smart materials/masks and scriptable export.

Benefit: mature mesh-aware surface workflow and strong automation APIs.

Risk: recurring license cost, runner activation/login complexity, extra dependency. Overkill for filler/background assets.

Decision: test only if Blender/ArmorPaint pilot establishes that dedicated smart-material authoring materially reduces iteration time or raises quality.

### Tripo H3.1 — PRIMARY HERO-GEOMETRY MODEL

Use when fidelity matters and the asset has already passed zero-credit V0/V1 proof. Do not default every asset to the most expensive route.

### Tripo P2 — CONTROLLED PRODUCTION A/B

Potential role: <=50K clean low-poly/quad output for game assets, reducing the destructive 1–2M -> 49.8K decimation step.

Benefit: if quality is sufficient, can eliminate a major geometry-loss stage.

Risk: Preview model; visual fidelity/topology must be demonstrated against H3.1 on the exact same reference. Any paid A/B requires explicit owner authorization.

### Meshy T2 / Meshy 7.1 — OPTIONAL SECONDARY GENERATOR

Most interesting role is not replacing Tripo wholesale. T2 can target clean low-poly output with separated parts at a low per-task credit cost; this could be useful as a cheap topology/preflight lane for support assets.

Risks: second vendor/API/credit pool, inconsistent art identity, more source formats and support surface. A second generator only earns a place if an exact-input A/B produces a measurable advantage.

Decision: no integration or spend yet. If tested, use one exact approved non-hero reference and compare geometry cleanliness, part segmentation, texture quality, elapsed time, credits and integrated Unity appearance.

### Sloyd / Hyper3D Rodin — WATCHLIST, NOT INTEGRATED

Both expose production-oriented APIs and configurable topology/face counts. Sloyd also supports quads, target face counts and commercial paid plans; Rodin exposes high-detail PBR generation.

Decision: no integration now. They would create a third/fourth generator lane before Tripo H3.1 vs P2 has even been characterized on Eldoria. Revisit only if a concrete asset class fails both Tripo routes, then run an exact-input A/B rather than adopting by demo quality.

### RizomUV — DEFER

Excellent specialist UV tooling, but our current failure is not demonstrated to be UV packing throughput. Blender already creates/validates UV0. Revisit if texel-density/packing becomes a measured bottleneck for hero assets.

### Marmoset Toolbag — DEFER

Excellent fast GPU baking and visual inspection, with Python scripting, but overlaps Blender baking and a future Painter/ArmorPaint surface lane. Its value is strongest if Blender baking iteration becomes artist-time constrained; not required for the automated baseline.

### Simplygon / InstaLOD — REJECT AT CURRENT SCALE

Technically strong automatic reduction/remeshing/material baking. Current commercial pricing is disproportionate to Eldoria, and Simplygon adds another Unity/USD integration dependency. Our Blender pipeline now supports role-specific triangle profiles; prove that insufficient before revisiting enterprise optimization middleware.

### Houdini / Houdini Engine — DEFER TO CITY-SCALE PROCEDURAL NEED

Houdini could become useful for repeatable district/terrain/prop variation once the visual grammar is stable. Introducing a procedural-authoring platform before material/style convergence would create more systems to maintain without solving today's bottleneck.

### SpeedTree — DEFER

Valuable later for vegetation variation/LOD/wind, but vegetation is not the current visual bottleneck. Adding it now would optimize the wrong layer.

### Adaptive Probe Volumes — DEFER UNTIL LOOKDEV BASELINE

Potential lighting improvement, but first solve material values, directional light, fog/exposure and post-processing. Profile on mobile before adoption.

### Occlusion Culling — DO NOT ADOPT BLINDLY

Current Valoria geometry is substantially runtime-generated. Unity's built-in occlusion culling is less suitable when occluding geometry itself is generated at runtime. Revisit only if production city geometry becomes sufficiently baked/static.

### Hunyuan/TRELLIS/local heavy 3D models — REJECT FOR CURRENT PRODUCTION

Do not add local generative-3D infrastructure merely to avoid credits. Current candidates bring licensing, GPU/OS or commercial-use uncertainty and duplicate a solved generation stage. Re-evaluate only when a clearly licensed Windows-compatible model materially exceeds Tripo/Meshy for our exact use case.

## Canonical selection by task

| Need | Default | Escalation |
| --- | --- | --- |
| City massing/circulation | Unity greybox | none; fix composition |
| Material/lighting diagnosis | Unity Rendering Debugger + LookDev | Blender bake / painting tool |
| Tiling stone/wood/roof | Blender/approved CC0 source | Material Maker pilot |
| Hero building geometry | Tripo H3.1 after V0/V1 | controlled P2/Meshy comparison |
| Support low-poly module | existing certified kit / P2 candidate | Meshy T2 controlled test |
| Hero surface polish | Blender baseline | ArmorPaint pilot -> Substance if justified |
| Vegetation | existing kit | SpeedTree only when density/LOD becomes bottleneck |

## Runtime visual-efficiency implications

- The historical ~49.8K module target is now a default validation profile, not a universal production budget.
- Use asset-role triangle targets and later LODs after official-camera/mobile profiling.
- Mipmap streaming is a candidate once texture memory becomes measurable; it should be budget-driven, not enabled blindly.
- ASTC is the preferred modern mobile texture-compression direction where target-device support permits; platform import settings belong in the mobile profiling phase.
- Built-in occlusion culling is not a first-line optimization while major Valoria geometry is runtime-generated; revisit if the city becomes substantially static/baked.
## No-tool rule

A tool is not added because it is popular or produces impressive demos. It is added only when it closes a measured Eldoria bottleneck and has an explicit input/output position in the pipeline.

## Next proof sequence

1. Run new Blender surface diagnostics on a known Tripo asset.
2. Use Valoria LookDev captures to establish a material/lighting baseline without changing production.
3. Fix Aserradero SURFACE using existing geometry.
4. Reuse the learned surface recipe on Cuartel.
5. Only if Blender/Unity surface workflow is still too slow or insufficient, run a small ArmorPaint pilot.
6. Separately, request owner approval for one exact H3.1 vs P2 or Tripo vs Meshy controlled geometry A/B only if it can answer a remaining geometry question.