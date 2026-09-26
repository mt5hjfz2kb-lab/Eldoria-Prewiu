# Valoria — isolated image-to-3D production gate

Date: 2026-09-26. Scope: one Bastion tower/facade fragment, adjacent wall, rock foundation and connected path. The goal was to turn the accepted illustration into an actual reusable 3D asset, without changing `Valoria.unity`, `VisualWorld` or gameplay.

## Evidence obtained

- The current `main` reference images are `Unity/Assets/Eldoria/ArtTests/LayeredValoria/bastion-stage-{1,2}.png`. The prior six Unity captures and limitations are in `VALORIA_LAYERED_ART_TEST.md`.
- The tool registry exposed no image-to-3D, Blender/DCC or desktop-control tool. This Linux Work workspace has no `blender`, Unity Editor, GPU utility, `torch`, `bpy`, `trimesh` or `open3d`. The Windows Unity runner previously reported no Blender installed (`VALORIA_MANUAL_3D_PIPELINE_GATE.md`).
- [Tencent Hunyuan3D-2.1 Space](https://huggingface.co/spaces/tencent/Hunyuan3D-2.1) was reachable in the Work browser. Its live UI exposed `Gen Shape`, `Gen Textured Shape`, GLB export, optional simplify and target face count. The [official model page](https://huggingface.co/tencent/Hunyuan3D-2.1) describes image-conditioned mesh and texture generation. This establishes a candidate tool, **not** a successful conversion of Eldoria art.
- The committed `bastion-stage-1.png` was prepared unchanged for upload. The browser's documented file-chooser flow was attempted in both the embedded Space and its direct app view. Both attempts timed out before a file chooser was delivered; no image was transmitted, no generation ran and no GLB was produced. There is no measured generation time or output mesh to inspect.

## Acceptance status

| Gate | Result |
| --- | --- |
| Authentic tower/wall/rock/path geometry from the accepted art | **Not demonstrated** — no generated asset. |
| Three official zooms in Unity | **Not tested** for 3D. Previous PNG/Quad zoom captures are not evidence for a mesh. |
| Physical rock/building/path connection | **Not tested**. |
| Mesh collider and precise click | **Not tested**. |
| Modular separation/reuse and four Bastion stages | **Not tested**. A single-image mesh may require substantial separation, topology repair and reconstruction; the exact effort cannot be estimated without the actual output. |
| Unity import, mobile draw/memory budget | **Not tested**. |

## Decision and next minimal experiment

**C for this execution path: no viable end-to-end pipeline was demonstrated.** The blocker is access to a mesh-generating tool that can take Eldoria's reference image and return an exportable GLB, followed by an editable DCC surface. This is an operational conclusion, not proof that image-to-3D itself cannot work. Do not create a substitute scripted tower from primitives, do not promote the PNG Quads and do not purchase an asset from this incomplete test.

The next technical step is exactly one exportable GLB/FBX of a **small tower and adjoining wall**, made from the accepted image with an image-to-3D tool in an environment where upload/download works. Commit that raw mesh under an isolated art-test folder with its source/reference and usage rights. Then perform one cleanup pass in interactive Blender: separate wall, tower and rock if possible; fix any fused openings, missing backside and roof, connect a short real path/terrain mesh, set pivots, UVs/materials and simple colliders. Import into an isolated Unity scene, capture three zooms and report triangle count, materials/texture memory, mobile FPS if available, and actual hands-on minutes spent on generation plus repairs. **Stop after this one fragment.** If the output cannot be repaired into connected modules within a repeatable labor budget, reject image-to-3D as Valoria's primary asset pipeline.

No 3D asset or Unity capture is claimed for this gate. Production Valoria remains intact.
