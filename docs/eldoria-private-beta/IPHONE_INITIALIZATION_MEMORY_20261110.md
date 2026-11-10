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


## iOS Simulator evidence correction — 42908325634

The revised harness at 21343c34c6da4aae3e6a5280ba65e775e042a23b did not certify either arm. Both reached the Unity JavaScript promise (FPS 0); the next automation evaluation returned null. Neither produced 120 seconds of fresh healthy post-ready samples. The old beta10 120-second PASS in run 42905159087 is invalid: screenshot latency consumed the observation interval before a fresh sample.

Artifact 13805524368, SHA256 82a1dc366589d3e36defc1bda8cde0f7fd0280c5afdbea781462fc4b8a3fed85, records the simulator GPU process crashing in both arms. For beta10, at 21:53:42.426 UTC MTLSimDriver lost its SimMetalHost XPC connection, followed by GPUProcessProxy::gpuProcessExited reason=Crash. Direct-r1 also records GPUProcessProxy crashes (21:54:44 and 21:56:33). This is simulator evidence, not a physical iPhone termination report and not proof of the owner's restart cause. Do not label null automation responses as confirmed jetsam.

## Packaging verifier extension correction

The first collision verifier matched .data and .data.gz but Unity's current output uses .data.unityweb. The corrected read-only verifier recognizes all three, rejects ambiguous multiple payloads, and retains the exact collision and noncollision-byte assertions. Local fixtures verified all three suffixes reach the same exact bundle parser, and ambiguity fails. Native build and packaging evidence must remain separately scoped if this follow-up job fails only for the filename mistake. Packaging independently repeats the full verifier against the exact native artifact; no engine or geometry gate is skipped.


## Additional source audit during compilation (not included as a fix)

Frozen BastionIIIWorldPresentation.cs at 948ffa9837168a0b68dab80be76b66350422b23a and candidate branch have identical blob d596e96995fe6da799fceff7b23cb20227c162ef. Initialize creates the granary, stock, wolves, boars and private-fissure presentation before Apply hides locked level-III content. Deferral needs to preserve WorldHotspot identity and host target indexing; no untested lazy-loading edit was added to this candidate. Tier-V presentation already has LoadUnlockedAssets gated behind BastionLevel>=5; do not falsely report all level-V meshes loaded at level I.

The compiled texture inventory includes 135 format-48 ASTC objects and 89 DXT objects (58 DXT5, 31 DXT1). Inventory bytes are not measured residency. Legacy blanket statements that iPhones cannot use DXT are insufficient for this owner's iPhone 16: Apple's May-2026 Metal feature tables list BC filtering for Apple9/10, while older families differ. Exact WebGL extension support must be measured before asserting a decompression fallback. No new texture conversion or quality loss was introduced.


## Exact native result

Native run 42909656581, job 130156795850 completed SUCCESS at source 7ae5416613480ba526a839b429459d53629041e5. Evidence artifact 13809346377 (366275289 bytes), SHA256 6c02f606b22f481d768b0b57012af5648e7df1a0d27be6e4178bb98727ff75f7, was downloaded and verified. Canonical journey: 227 checks. Terrain: two parity passes with 119870 checked queries each, retainedContactNormals=0, retainedVerificationBuckets=0, canonicalSavesUnchanged=true, ready/worldComplete=true. warm-cache-restoration.json reports restored=true, no conflicts, no status changes, no existing input changes. Pre-existing authored/normalized asset differences were preserved, not discarded. WebGL compiled 1490774387 bytes in seven files; artifact 13807824950. Windows resource released in main commit 1e1cf1aec0365c1827b5c71861f8f3b57829a2cd.

The native workflow follow-up job 130177005433 failed exactly on its filename discovery assertion (zero .data/.data.gz paths); this does not invalidate the successful native job. Independent packaging run 42914592603 at 5a8509e35c69b81c2aed564a1c66f07fbc0d968a successfully passed the corrected full collision-byte verifier before proceeding to texture-preserving packaging. The full packaging/WebKit result and physical phone result are still pending at this checkpoint. Native shader/mesh/build proof is not a physical restart fix certificate.
