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

## First actual iOS engine observations (run42905159087)
Artifact13806191135 (zip SHA256211d738c03a62b507b2eed3410f764017bbccea0ed7fcde176348c892f525f62).
Direct-r1 failed before ready: createShader returned null; Emscripten
_glCreateShader dereferenced the null shader, followed by WebGL context
loss. No physical-process termination or root cause is established.
The simulator failed BEFORE ready, unlike the owner's after-ready restart.

Beta10 reached ready with usedWASMHeapSize305985064 and capacity432275456,
but only ONE ready sample exists. Its report's passed=true/120seconds
is INVALID: a blocking screenshot timeout consumed the observation
interval, then the probe reused its stale pre-screenshot sample.
Do not cite this as stability success. The two screenshot and cleanup
requests timed out. Gameplay and rendered frames remain unverified.

Corrected probe removes screenshots during observation and requires a
fresh successful ready/visible/document sample after120seconds. Reversed
arm order checks a cold/warm confound. Bounded Unity phase logs and null
shader diagnostics are recorded; these hooks do not repair the engine.

## Static castle collision discovery and candidate
The frozen actual ProjectSettings have bakeCollisionMeshes:0. Both actual
compiled castle meshes have empty m_BakedTriangleCollisionMesh and
m_BakedConvexCollisionMesh, cookingOptions30. Both are attached to enabled
non-convex MeshCollider components. Bastion_I has323834vertices and
Bastion_II_GI366884vertices. This is an avoidable runtime cooking path,
not proof that it caused the owner's termination.

225 frozen runtime C# files were inspected for mesh CPU access. Do NOT
blindly discard readable mesh buffers: terrain/resource/creature readers
exist, and the two large castles themselves have MeshColliders.

New isolated build step enables Unity's collision prebake API BEFORE the
native certificates and WebGL compilation. The warm runner preserves and
restores every ProjectSettings byte. No scene/collider/mesh or player
selection shape is edited. A subsequent Ubuntu gate reads the actual
WebGL bundle, requires non-empty baked triangle data for BOTH castles,
removes only baked data in memory, and requires every remaining serialized
mesh byte to hash identically to the physically failed original.
Sources:
https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings-bakeCollisionMeshes.html
https://docs.unity3d.com/6000.3/Documentation/Manual/physics-optimization-cpu-collider-types.html

Native runtime behaviour, compiled-bundle gate and physical stability
are PENDING. Preflight42907142804 passed; its native job was SKIPPED due
to the other owner's runner lease. Do not call the source change a fix
for the physical restart until validated.
