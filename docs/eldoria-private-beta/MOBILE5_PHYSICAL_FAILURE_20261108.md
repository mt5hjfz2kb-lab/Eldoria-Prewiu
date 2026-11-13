# Mobile5: physical iPhone failure, 2026-11-08

Status: FAILED ON PHYSICAL IPHONE. Not releasable to testers.

## Observed
Owner reached the nickname screen, then returned involuntarily to landing.
Screenshots IMG_5047(1).jpeg and IMG_5048.jpeg:
- 15:32:41: startup ready, WASM capacity 412 MiB, ASTC supported.
- 15:32:44: successful grows to 495 and 594 MiB.
- 15:32:46: successful grow to 699 MiB.
- 15:32:48: boot back_forward, diagnostic v4, pageshow normal.
No allocation exception or context-loss marker is present. Do not report confirmed OOM.
Heap capacity is neither live allocations nor process RSS. APK success does not validate WebGL.

## Exact candidate
Bundle SHA256: 2c502a1d766aca83c2377347de290467d16d75efd8c6f5edad41766f4d3ce73a.
Published data: 500847489 bytes. Public runtime QA run 42488719340 passed desktop only.
Desktop peak WASM capacity: 1064108032 bytes.
The prior material mip optimization removed 298319872 serialized bytes, but did not solve this device failure.

## Additional binary audit
Reading the exact mobile5 bundle with UnityPy:
- Texture2D serialized object bytes: 540635200.
- Mesh serialized object bytes: 361295804; 851 meshes.
- Largest mesh: Bastion-1, 23688744 bytes, 388196 vertices, readable.
- Bastion_II_GI: 22494700 bytes, 366884 vertices, readable.
- Authored low uplands with continuous rounded geological incision: 22128620 bytes, 416506 vertices, readable.
These are serialized sizes, not simultaneous resident RAM measurements.

## Source findings and next experiment
RegionOneCanonicalLandscape eagerly builds the complete world while the host is still in city view.
RegionOneBlenderLandscape loads Resources World4X/Blender/RegionOneBlenderWorld.
Actual source path: Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RegionOneBlenderWorld.glb.
The transport splits downloads but retains the assembled Unity data payload; download chunking is not demand loading of game assets.
Do not unlink the mounted data.unity3d: subsequent Resources.Load calls require it.
Do not disable readability on all meshes: terrain fitting, resource grounding and reference placement read vertex arrays.
Do not simply defer the world and declare success: that could move the same failure to Mundo.

Next controlled experiment: externalize a verified world dependency group from startup data, load through Unity asset-bundle APIs, and release bundle archive storage after its required objects are loaded. Must first audit Resources and scene dependency overlap; otherwise the same objects can be duplicated in player and bundle.
Preserve normal tutorial, city/world transitions, terrain geometry and query parity, resources, heroes, saves, progression and clock.
This is a proposed experiment, NOT an implemented or compiled fix.

## Execution and gates
Windows-heavy resource is held by the Vigilia workstream, revision 21. Beta revision 11 requests the resource without overriding that owner. No Unity process may be cancelled or warm project reset.
Preparation and binary/source auditing can proceed independently.
Before publishing another device candidate:
1. Validate bundle exclusion from startup data and dependency identity.
2. Compile isolated WebGL build and run error-free city, nickname, tutorial, Mundo, return-to-city and reload/save checks.
3. Measure startup and world-transition peaks separately; distinguish WASM capacity from process RAM.
4. Inspect actual city/world captures.
5. Physical iPhone must stay playable beyond startup and survive world entry before the defect can be marked resolved.
