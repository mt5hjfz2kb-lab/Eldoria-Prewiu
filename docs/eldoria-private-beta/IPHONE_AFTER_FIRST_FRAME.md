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

## Candidate2 result and candidate3

Candidate2 run42420500174 passed 45-second welcome observation with unchanged visible scene and no unreadable-texture errors. Wasm capacity decreased from1,580,924,928 to1,481,834,496 bytes (6.3%). This is reserved Unity linear memory, NOT total process memory. Candidate2 was not published as a standalone fix.

Candidate3 additionally encodes32 eligible material textures as ASTC4x4, preserving dimensions, mip chain, RGBA channels, color space and sampler metadata. Local validation reopened all54,338 objects; only accepted texture payload/format/length changed. Saved serialized bytes150,994,188. Bundle SHA25660b43335bb263568dd366faca06ab0d648506dac31024342b1d4edefd8a7c315. Twenty-two candidates failed quality limits and retain their original bytes. Six accepted normal maps satisfy p95 angular error <=1 degree for the first three mips (<=2 for tails) and p99 <=2/3 respectively. No normal-map channel swizzle is introduced.

Independent run42421746562 rebuilds candidate3 from the exact successful candidate2 artifact, repeats object/quality checks and holds both scenes45s. A separate real-entry test uses production phone pixel ratio, enters a nickname through the existing UI, observes60s, verifies browser-backed saved nickname, reloads and observes45s. These tests remain desktop Chromium/SwiftShader, not physical iPhone certification and not a Bastion I–V playthrough.

Publication remains gated on evidence. Do not mark tester readiness or physical iPhone success from these automated tests.

## Entry QA limitations found

- Candidate3 independent encoding produced the same bundle hash and passed startup45s. Wasm capacity1,394,147,328 bytes; no JS/Unity exception errors in that comparison.
- Run42422065126 (cancelled after stalled extended validation) produced real production-pixel-ratio captures: nickname Beta QA visible in HUD, city and normal Las Cenizas de Valoria tutorial after60s. This partial evidence is not a passing complete test.
- Runs42422779109 and42422948851 failed because the CDP compositor screenshot exceeded20s under SwiftShader. Do not repeat this capture method at high DPR as if it were a game crash.
- Run42423325534 successfully entered the name and observed60s, heap1,394,147,328; raw page.reload() then exceeded30s, and browser cleanup also timed out. Raw reload remains NOT certified.
- Synchronous canvas readback after the named Browser_mainLoop_runner callback is prepared as a capture alternative; it rejects blank/cleared frames, restores the original scheduler and does not change shipping code. Khronos explicitly permits synchronous drawing-buffer access following rendering: https://registry.khronos.org/webgl/specs/1.0/ .
- Run42423856431 tests documented Unity instance.Quit() before reopening. Even if it passes, this certifies graceful reopen only, not raw reload or physical iPhone stability. Official API: https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/webgl/building-distribution/templates/web-templates-build-configuration .
