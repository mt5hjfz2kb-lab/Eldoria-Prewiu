# UNITY 6 — ELDORIA PROFESSIONAL ART FINISHING PLAYBOOK
Status: canonical specialist reference (owner decision 2026-10-12). Scope: Unity 6000.3.23f1, URP 17.3.0, glTFast 6.14.1, Android/iOS, zero paid credits.

## Objective and source of truth
Convert competent Blender assets into convincing mobile game environment artwork **in Unity**, without pretending lighting fixes geometry. Read AGENTS.md, live repository state, visual authority and concurrency rules before execution. This is a repeatable production process, not a claim that anyone has already mastered every Unity subsystem.

## Art-directed production cycle
1. Establish target image/style guide and the actual player camera, object screen occupancy, visual hierarchy, platform tier and acceptance criteria. Preserve Hero V3 as a frozen benchmark until superseded by owner.
2. Agent IA actively authors geometry in Blender/BlenderMCP. Check convincing silhouettes, masonry, scale, silhouette, dimensions, seams, sockets, ground contact, normals/tangents, UVs, material slots, legal texture provenance, polygon counts, transforms and pivots. Capture clay and lit renders at matched angles.
3. Export deterministic GLB with hashes. Unity imports via the project's glTFast package. Validate orientation/units, materials, normal-map channel convention and packed metallic-roughness textures: importer defaults and shader compatibility must be checked, not assumed.
4. Create an **isolated test scene** (never overwrite canonical scene) with the exact gate and target candidate, same camera/geometry for BASE vs LOOKDEV. Record asset hashes, scene SHA, screen resolution, render quality, time of day, camera matrix, and all changed settings.
5. Unity finishing: URP Lit PBR with controlled albedo variation, normal detail, physically plausible roughness and metallic properties; coherent sun/sky/exposure, main-light shadows, baked lighting for stationary geometry where appropriate, probes for movable objects, low-cost contact shading, selective decorative decals/vertex colors, vegetation scatter, broken-up paving seams and terrain transitions.
6. Evaluate static frame and mobile-scale readability, inspect seams from multiple cameras, physically correct contact, avoid floating features, material mismatch, repetitive patterns and excessive postprocessing. Pass/fail must be evidenced visually against the target and Hero V3, not scored only by Qwen.
7. Correct root cause: Blender for silhouette/proportions/geometry/UV, Unity for material/light/scene composition/camera. Repeat same-camera A/B. Reuse earlier valid iterations. Never silently replace approved architecture.
8. Profile on representative Android and iOS devices; measure FPS/frame time, CPU/GPU bottlenecks, draw calls, triangles, overdraw, memory, texture sizes, shader variants and device thermals. Only then choose LOD distances/texture tiers/shadows/postprocessing. WebGL or simulated phone screenshots are not physical-device certification.
9. Gates in order: SOURCE STRUCTURAL READY -> GLB/UNITY IMPORT READY -> ARTISTIC A/B PASS -> MOBILE PERFORMANCE PASS -> INTEGRATED GAMEPLAY/REGRESSION PASS -> authorized promotion. Record explicit failures, tests, evidence paths and final SHA.

## Unity 6 deep competency map (review for each relevant task)
- Engine fundamentals: GameObjects, Transform hierarchy, serialization, MonoBehaviour lifecycle, scenes/prefabs, packages, asset database, editor scripting, assembly definitions, C# tests, automated batchmode builds.
- URP: render pipeline asset and renderer asset settings, Forward/Forward+, depth and opaque texture cost, render passes/Render Graph compatibility, SRP Batcher, shader variants, platform graphics APIs, camera stacking, antialiasing.
- Materials and art: URP Lit, PBR color space, texture import (sRGB only for color), normal maps, tangents, channel-packed masks, triplanar vs authored UV, Shader Graph only if needed, texture arrays/atlases, material variants and instancing trade-offs.
- Lighting: directional sun/skybox, exposure/tone mapping, physically grounded color temperature, ambient/indirect light, baked lightmaps, UV2 generation, light probes, reflection probes, shadow cascades and bias, contact artifacts and mobile constraints. Baked lighting is not free: track atlas memory and build times.
- Scene assembly: grid/module joins, mesh terrain contacts, camera framing, hierarchy of focus, occlusion, vegetation, decals, weathering and grounded prop placement. Prefer lighting/material and scene detail to needless subdivision; never hide broken silhouette with bloom.
- Rendering analysis: Frame Debugger, Rendering Debugger, Profiler, Memory Profiler, platform profilers (Xcode, Android GPU Inspector/RenderDoc where supported), GPU bottleneck vs CPU thread, batching and overdraw.
- Runtime optimization: LODGroup and impostors at phone screen size, visibility and culling, static batching versus SRP batching/instancing compatibility, mesh compression, texture compression ASTC/ETC2 platform constraints, mipmaps, draw-call and pixel-fill budgets measured rather than invented.
- Build/release: platform player settings, Android/iOS quality tiers, reproducibility and binary/cache artifacts, platform-specific graphics API, real-device QA; protected GitHub Actions/Windows runner, no competing heavy CI execution.

## First empirical experiment — Valoria gate / modular family
- Input: frozen Hero V3 plus straight wall, V3-derived tower, corner, connector, path/platform, cliff, props. Prefer immutable SHA-pinned sources.
- Create baseline scene with neutral URP-compatible imported materials, then duplicate into lookdev with exact geometry/camera/placement and high-quality **mobile-conscious** materials, lighting, environmental detail and color grading. Compare same-camera hero and strategic shots plus mobile read.
- Especially scrutinize the cliff silhouette, physically plausible joining of stone to rock, connection geometry, flagstone repetition, visible tower alignment, texture distortion and cost of V3's unusually high object count.
- A structural review may reject malformed or floating meshes even before lookdev; otherwise preserve them for Unity test before final rejection.
- Publish evidence as true engine captures with change list, metrics, hashes, PASS/REWORK per piece and explanation of whether corrections belong to Blender or Unity.
- Do not use the shared Windows Unity runner until actual ownership and lease permit it; don't modify canonical Valoria integration without separate gate.

## Official technical references
- Unity 6 Manual: https://docs.unity3d.com/6000.0/Documentation/Manual/
- URP performance: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/understand-performance.html
- URP performance settings: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/optimize-for-better-performance.html
- URP draw calls: https://docs.unity3d.com/6000.0/Documentation/Manual/reduce-draw-calls-landing-urp.html
- Lightmapping: https://docs.unity3d.com/6000.0/Documentation/Manual/Lightmapping.html
- Profiler: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Profiling.Profiler.html
- glTFast documentation: https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.14/manual/index.html

## Non-negotiable evidence / limitations
Documentation and design review alone never equal executable Unity demonstration. Do not imply Unity LOOKDEV/ART PASS if no real Unity capture exists; neither a green CI nor screenshot generated outside Unity verifies visual integration. Keep mobile budgets hardware- and scene-specific. Follow the active owner workstream, approved visual routing, and gameplay protection before any promotion.


## Practical lessons verified on the actual Windows Unity 6 runner (2026-10-12)
This chapter records real experiments, not speculative recommendations. Source-of-truth implementation: `integration/valoria-v3-unity-lookdev-20261012`; exact successful Unity run `38188882212`, source `c01867fb0b33932aa6369674d1b66c99726f7300`, artifact `11720770593` with 30 real engine screenshots and logs, including full-family gate+wall+terrain assembly. Work-approved family input commit `0abf13870d21725c30acf3f3e3a8c86385470073`; all seven exact source GLB SHA-256 plus frozen Hero V3 checksum were checked on the runner. Changes stayed in an isolated branch and used zero paid credits.

- The authenticated GitHub connector can write an isolated **push-triggered** workflow; the registered Windows Unity 6000.3.23f1 runner then runs the editor. Avoid claiming PC access impossible merely because a direct workflow-dispatch action is not exposed. Respect active runner ownership/concurrency.
- When genuine `Camera.Render` PNGs are required, **do not run Unity with `-nographics`**. The engine can report success with wholly empty images; validate image content as well as counts and dimensions.
- For batch-editor glTFast, `new GltfImport(null,new UninterruptedDeferAgent())` avoids an observed `DontDestroyOnLoad glTF-StableFramerate` exception in editor mode. Qualify `UnityEngine.Application.streamingAssetsPath` because Eldoria owns an `Application` namespace.
- Exact Blender-to-Unity preservation is meaningful only with approved source ref/SHA, per-GLB SHA-256, actual GLB reimport, matched camera, actual textures/material validation, and `before`/`after` camera captures, not a workflow-green status alone.
- Reset activation of all meshes after individual piece renders before assembling them; ensure camera bounds include ALL enabled mesh objects. Fixed-distance linear fog can erase an entire long-distance fortress; scale fog falloff against actual view distance and validate screenshot pixels.
- Reusing the installed `NatureStarterKit2` trees and shrubs for test set dressing plus a matching textured terrain, warm sun, and bounce lights visually improves contextual grounding. Verify asset licenses before promotion, and snap prefab bounds min-Y, **not object pivots**, to actual ground heights. This art-stage gain must not be misrepresented as improving the source meshes themselves.
- The exact test produced a valid Unity import/render and visible scene-level improvements, **but does not certify the aspirational integrated 9.5/10 Valoria target or mobile production performance**. Art acceptance remains independent of technical execution. The scenery still needs professionally art-directed UV/PBR texturing, connected rock/road/wall transition geometry, verified lightmapping/contact shadows, mobile profiling, and a deliberately composed final production scene.
- Reusable project files: `ValoriaFamilyV3Capture.cs` (editor capture), `ValoriaUnityFinishKit.cs` (URP materials/ground/bounce), `ValoriaNatureDresser.cs` (existing foliage), and `valoria-v3-unity-lookdev-probe.yml` (Windows-runner repeatability), all on the isolated integration branch. Keep this study separate from existing active world-region workstreams.
