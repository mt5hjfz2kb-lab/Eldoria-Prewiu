# Direct-r1 physical failure and initialization-memory candidate — 2026-11-10

Owner screenshot IMG_5065: startup-ready 22:24:15, heap capacity 412 MiB,
new boot back_forward 22:24:23. The owner reports an involuntary restart.
Direct transport is therefore rejected as a fix; the Mac entry/reload PASS
does not override the physical result. No root cause has been established.

## Read-only compiled-bundle audit
The exact mobile12 decoded game data totals 503831429 bytes. Its
data.unity3d bundle contains 361295804 serialized mesh bytes, of which
359219828 are marked readable. This is serialized inventory, NOT measured
resident process memory or an achievable saving. The terrain mesh alone
has 416506 vertices, 13328192 vertex-buffer bytes and 8799456 index bytes.

102 groups have identical serialized mesh data after their names, with
51809484 duplicate bytes. No mesh aliases or Read/Write flags were changed:
runtime resource grounding reads vertices and clones meshes; shared-mesh
release could also break scene re-entry. Texture inventory is 540635200
serialized bytes; previous texture transforms are retained, not repeated.

## Implemented candidate, not built or published
RegionOneBlenderLandscape now transforms the owned Mesh.vertices return
array in place instead of using a second LINQ output array. Each call
avoids one additional Vector3[416506] (4998072 payload bytes) for this
exact terrain, with the same TransformPoint operations and order.

After FitImportedPigment completes, release its contactNormals reference.
Its only reader is CoincidentGroundNormal, called exclusively by that
initial fitting operation. Preserve the mesh's own normals, the fitted
render meshes, contact vertices, triangles, spatial cells, source assets
and saved-game state. This makes 4998072 managed payload bytes eligible
for collection on this exact terrain; it does not guarantee immediate GC,
a smaller WASM heap, or resolution of the iPhone restart.

The existing native structural probe now records retainedContactNormals
and requires zero in the staged candidate. It still compares geometry,
terrain queries, routes, population, saves and real scene re-entry.
Native Unity, WebGL visual/gameplay regression and physical iPhone
validation remain pending. No external tester release.

## iOS diagnostic
Run42905159087 attempts actual engine startup with native SafariDriver.
A programmatic click fallback is explicitly marked as not touch-input
certification. Earlier run42898705517 connected to Safari but never
started Unity; neither its timeout nor its unchanged page is a game test.
