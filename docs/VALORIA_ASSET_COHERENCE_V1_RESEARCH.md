# Valoria Asset Coherence & Game Presentation v1 — applied research

Scope: locked Flat Citadel; source 578ae2c65201dad74f004ba8448faab8a34952ad / accepted visual c2894925c97aea7cd514b6e90b1cac81b802a63e. Baseline run 37141904595 / artifact 11280049716. No promotion or visual verdict before integrated review.

## Diagnosed causes from real baseline

- Hero source is much brighter and warmer than imported wall; Sawmill orange texture competes with blue/gold Hero; Cuartel/Granero baked dark timber differs in response.
- Source textures are atlases; tint-only normalization cannot selectively harmonize roof, timber and stone. Canonical maps and UVs must remain intact.
- Fused Hero rock has no semantic submesh; whole-object replacement would remove approved identity. A bounded height mask with world-space triplanar stone can retain geometry while reducing its separate bright geological read.
- Surrounding meadow is a 64x52 cube; zoom19 directly exposes rectangular limits. This is a visible technical edge, not a need for mountains.
- Cottages are still visible on R4/R5 despite suppression. ValoriaKit helpers can parent them outside the expected uplift subtree. Suppression must inspect the complete scene hierarchy.
- Existing editor capture creates SlicePresenter but never runs ReferenceUiArtPass coroutine. Runtime skin itself contains mockup-era chrome, so invoking it wholesale is unsuitable for the current real progression state.
- Primary action row lacks explicit childControlHeight/expansion and presents a square narrow button in baseline mobile capture. Native layout repair and minimum text size are applicable.

## Sources and decisions

1. Unity 6.3 custom URP lighting: https://docs.unity3d.com/6000.3/Documentation/Manual/urp/use-built-in-shader-methods-lighting.html
   Applied: Lighting.hlsl, UniversalFragmentPBR, main-light shadows, SH ambient and fog. A single material response preserves source albedo/normals, performs bounded saturation/value calibration and selective roof/rock blending. No unlit global replacement.
2. Unity URP Lit shader / maps: https://docs.unity.com/en-us/engine/6000.3/manual/materials-and-shaders/built-in/shaders-in-universalrp/prebuilt-shader-graphs-urp/lit
   Applied: masonry/wood non-metal, restrained smoothness and normal strength. Keep canonical texture detail; imported map aliases are audited rather than assumed.
3. Unity decal renderer reference: https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-feature-decal-reference.html
   Deferred: DBuffer changes/prepass are unnecessary for a fused Hero needing surface-wide localized blending. Screen-space decals remain an upgrade seam for authored dirt at road/retaining interfaces, not a way to separate architecture from fused geology.
4. Unity Mesh vertex colors: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Mesh-colors.html
   Applied: deterministic low-relief grid outside the wall with vertex ground-palette variation. Existing terrain/collision/layout remains untouched. Geometry extends beyond the official camera envelope; radial atmosphere masks the outer support field. No central landform or landscape island.
5. Unity uGUI CanvasScaler: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html
   Applied: responsive native HUD scaler and readable typography, explicit horizontal action sizing, gold panel framing. Existing resources/objectives/action listeners and unlock states remain owned by SlicePresenter. Decorative images/text do not intercept taps.
6. Unity mobile optimization: https://docs.unity3d.com/6000.3/Documentation/Manual/OptimizingGraphicsPerformance.html
   Applied: low foliage combined by shared palette; no new shadow lights; eight bounded ambient people. These choices reduce submissions relative to independent clumps, but representative-device performance still requires measurement.

## Rejected/deferred before execution

- More large retaining modules: prior Art Consolidation evidence already shows darker block seams and remaining visible pedestal. Selective surface masking is materially different and reversible.
- Blanket replacement of wall atlases: prior late consolidation variant was net-negative; preserve their detail and harmonize response instead.
- Whole-Hero mesh surgery: no semantic rock submesh proven; costly source editing is not required for the first interface test. Shader mask stays replaceable without canonical GLB changes.
- Giant mountain, terrain island, photo foreground, new city layout: prohibited by locked composition.
- Fake HUD screenshot, fake VIP/chat/unlocks: not used; all new skin operates on live controls.
- Paid regeneration: no proven new-geometry gap and no credit authorization.

## Iteration rule

Compare matched baseline/consolidation and coherence 19/12/9/mobile. Each correction must target a diagnosed cause. Revert net-negative techniques; max 2–3 variants per technique. Technical signature alone never grants VISUAL PASS.
