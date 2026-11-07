# Physical iPhone failure after first frame

Owner report: native325-mobile-candidate1 shows the game briefly (about one second), then involuntarily returns to the landing page. The defect remains open. Desktop startup PASS did not certify the phone. Do not repeat the same download-only intervention or claim proven OS jetsam without logs.

## Measured

- Original source948ffa9837168a0b68dab80be76b66350422b23a was loaded on Windows Unity6000.3.23f1 with isolated temporary-save probe, run42420011329. No canonical save bytes changed; the probe was removed and Windows released.
- Source inspection shows RegionOneCanonicalLandscape builds the whole world immediately after host.Ready, during initial welcome. RegionOneBlenderLandscape verifies the entire imported vertex set, constructs contact queries and retains verification data. Resource placement cooks a temporary MeshCollider and scans routes.
- At30s the native probe observed331,431 source-verification dictionary entries,416,506 contact vertices,2,199,864 contact triangle indices,25,764 contact cells,23,505 MeshRenderers (loaded resources include prefab objects, not necessarily all rendered).
- These Editor object measurements are NOT physical iPhone RAM, process memory or mobile frame rate. Do not add texture/mesh totals to allocator totals; their accounting differs.
- Candidate1 has32 material-only readable textures with191,889,452 bytes of pixel data. CPU-reader source scan excludes dynamic generated masks/readbacks; Sprite-linked textures and engendro/LUT/mask names are explicitly excluded from the experiment.

## Current controlled experiment

Candidate2 disables only m_IsReadable for those32 textures. All54,338 serialized objects reopened and verified; every other field and every pixel byte is unchanged. No scaling, resampling, color alteration, game-code change or terrain edit.

The browser comparison holds both builds45s after Unity readiness, checks JavaScript and unreadable-texture/Unity exception errors, and captures identical welcome views. A reduction in artifact size is NOT its acceptance criterion: compare actual wasm heap capacity and rendering. Do not publish if the experiment has no meaningful measured benefit.

## Structural follow-up

The full terrain source validation is appropriate in build QA, not necessarily on every player's startup. Moving it to a source-hash-gated build step and splitting initialization across frames are alternatives requiring native compile, exact scene/query equivalence and save/gameplay tests before shipping. They are not implemented by the candidate2 texture experiment.

Unity primary references: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/texture/isreadable and https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/webgl/develop/memory . Readable textures retain CPU data; Web managed GC can run only at frame boundaries. These facts support hypotheses but do not prove the cause of this specific Safari return.
