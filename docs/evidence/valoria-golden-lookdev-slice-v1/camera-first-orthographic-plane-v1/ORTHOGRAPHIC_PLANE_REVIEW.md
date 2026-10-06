# VALORIA CAMERA-FIRST ORTHOGRAPHIC PLANE v1 — VISUAL/TECH REVIEW

Status: **CORE CAMERA FAMILY PASS / TWO PORTRAIT EXTENSIONS REQUIRED / UNITY PROOF AUTHORIZED**
Date: 2026-10-06

## Evidence

Canonical source:
`references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

Authoritative run:
- workflow: `[PROOF] Valoria Camera-First Orthographic Plane v1`
- run: **37425954567**
- artifact: **11394723424**
- artifact digest: `sha256:626e4cbc164b519a6662c7c3546cf236aa9527bb8fed7b7efcb73008d1218c82`
- credits: **0**
- Unity touched: **false**

Camera authority:
`pipeline/valoria-production-art-reset-run-request.json`

All official views in that request are:
- perspective: **false**
- yaw: **20°**
- pitch: **35°**

Therefore the official Valoria camera family is a locked orthographic orientation with pan/center and orthographic-span changes.

## Critical conclusion

The previous perspective-parallax stress proof was stricter than the actual official camera family.

For a fixed-orientation orthographic camera:
- pan does not require perspective novel-view synthesis;
- zoom is a crop/scale operation;
- the canonical reference can be mapped to one camera-aligned world plane without changing its internal perspective;
- Depth Anything is useful for visual layering, occlusion and selective 2.5D separation, but is **not required to preserve the reference during ordinary official pan/zoom**.

This is the first method tested in this workstream that preserves the approved visual target by construction instead of rebuilding and then trying to recover it.

## Exact official-view coverage

| View | Source coverage |
|---|---:|
| UNITY-source-3x2 | 100% |
| UNITY-16x9 | 100% |
| UNITY-mobile-landscape | 99.98% |
| UNITY-portrait-left | 100% |
| UNITY-portrait-right | 100% |
| UNITY-golden-close | 100% |
| UNITY-portrait-home | 80.47% |
| UNITY-portrait-entry | 91.80% |

Missing source:
- portrait-home: only **top extension**, ~183.79 source pixels;
- portrait-entry: only **bottom extension**, ~57.89 source pixels.

No hero-city reconstruction is needed to cover the other official views.

## Visual decision

### Covered official views

**VISUAL TARGET PRESERVATION: PASS**

The rendered/cropped image is the canonical source itself. There is no generator reinterpretation, procedural castle substitution, material downgrade or geometry-language drift.

### Portrait-home / portrait-entry

**PARTIAL / SOURCE EXTENSION REQUIRED**

The missing regions are outside the canonical source frame:
- home requires mountain/sky extension above;
- entry requires lower foreground extension below.

The existing hero pixels remain authoritative and must not be regenerated.

## Relation to Depth Anything 3 proof

DA3 BASE successfully generated a coherent depth map and a 57,600-vertex / 111,596-triangle receiver proof.

The single perspective depth-mesh stress test produced:
- base source preservation: usable;
- small perspective offset: ~4% disocclusion holes;
- medium offset: ~8.6–8.7% holes.

The multiplane reconstruction removed black holes at small offsets but introduced inpaint artifacts at stronger offsets.

Those results remain useful only if Eldoria later unlocks camera rotation/perspective.

They are **not the governing blocker for the current locked orthographic camera family**.

## Production architecture now recommended

### Static visual authority
A camera-aligned orthographic visual shell derived from the exact canonical reference.

### Optional depth layers
DA3/SAM-style masks may split:
- far mountains/sky;
- castle/upper city;
- mid city;
- lower foreground;
- vegetation;
- water.

Depth layers exist for:
- occlusion ordering;
- inserting dynamic characters/effects;
- selective animation;
- controlled atmosphere;
- future small camera embellishments.

They are not used to reinterpret the approved art.

### Gameplay authority
Existing:
- colliders;
- hotspots;
- navigation;
- building state;
- gameplay topology.

The visual shell may not become gameplay authority.

### Dynamic 3D / animated overlays
Keep real/dynamic:
- characters/troops;
- construction and upgrade FX;
- water motion where useful;
- smoke/fire/flags;
- interactive selection/feedback.

## Next gate

**VALORIA ORTHOGRAPHIC VISUAL SHELL — UNITY PROOF v1**

Allowed:
- isolated Unity proof;
- exact canonical reference bytes;
- current locked orthographic camera;
- preservation of gameplay collider/hotspot signature;
- visual suppression/replacement only inside the proof;
- official 16:9, mobile, portrait left/right and Golden close captures;
- zero paid generation.

Not allowed:
- production-scene destructive changes;
- camera rotation redesign;
- gameplay topology changes;
- Tripo/Meshy paid calls;
- claiming final production adoption before the Unity proof.

## Acceptance

The Unity proof passes only if:
1. official covered views retain reference-class visual fidelity;
2. no unexpected image warping occurs under pan/zoom;
3. gameplay collider/hotspot signature is unchanged;
4. mobile capture remains sharp/readable;
5. presentation layer can coexist with dynamic overlay objects;
6. no paid generation is required.

