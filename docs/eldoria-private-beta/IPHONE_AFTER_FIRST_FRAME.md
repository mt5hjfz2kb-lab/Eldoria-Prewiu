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


## Physical failure and structural pivot — 2026-11-08

The owner physically retested candidate3: the game remains visible for about one
second and returns to the landing. Candidate3 is FAILED on that iPhone. Reducing
reserved Wasm capacity from 1.58 GB to 1.39 GB did not resolve the symptom.
Memory pressure is still a hypothesis; there is no iOS process-kill log proving it.
Run42423856431 also failed: documented Quit completed, but reload timed out.

The beta branch now contains four narrowly claimed C# changes: staged exact world
initialization, host readiness withheld until the map is complete, release of the
temporary Blender verification dictionary after mandatory validation, transformation
of owned mesh-accessor copies in place, and a readiness wait in the existing native
journey validator. Geometry, terrain data, game rules and the normal tutorial are not
intentionally replaced. C# syntax passed; this is NOT native compilation or gameplay
evidence.

Native run42428875558, source92d68b3a84c34952522286e246e5c358d2a9e306,
is the current attempted comparison/build. At this checkpoint it is still
materializing the isolated Unity source; NO native PASS or fresh WebGL exists yet.
Its baseline is948ffa9837168a0b68dab80be76b66350422b23a plus exactly the four
overlays recorded by run_structural.ps1. The probe exercises two actual scene
loads, hashes terrain meshes and query samples, checks navigation stays closed
during staging, and preserves canonical saves. Editor-update memory sampling is
not an intra-frame peak measurement and is not an iPhone memory measurement.
After parity, the unweakened canonical journey validator and canonical WebGL
builder must pass on this exact input.

Preparation lessons:
- Run42427811334 was retired ONLY while copying source, before Unity started.
  The frozen tree contains25.59 GiB, including14.24 GiB under docs.
- Run42428680863 failed before Unity because a file-only sparse checkout used
  cone mode. Corrected to non-cone mode.
- Current preparation borrows committed local Git objects into its own repository,
  includes all Unity source but not historical documentation captures, and does not
  reset another working tree or share its Unity Library. This still involves a
  large materialization/import. Do not launch redundant native jobs or another
  full repository download while it runs.
- The live main registry grants this beta workstream Windows revision8. Re-read
  live ownership before any dispatch; never infer availability from job state.

Independent preparation already checked:
- reuse_verified_texture_transforms.py reproduces candidate3 BYTE FOR BYTE from
  the frozen original: all54,338 objects checked,106 changed texture objects
  using102 unique verified transforms. No new quality/compression decisions.
  This lets a fresh engine retain previous savings only where texture input bytes
  match exactly. It does not make the failed old candidate a working one.
- prepare_structural_site.py keeps engine files from the fresh build, verifies the
  transformed data belongs to that build, and requires native parity/WebGL
  certificates. Synthetic extraction/packaging and mixed-data rejection passed;
  actual fresh-build packaging remains pending.
- qa_structural.cjs requires the real REGION1_CANON_WORLD_INSTALLED marker, a
  visible runtime for60s, a genuine capture and no runtime errors. It traces
  WebAssembly memory growth in the isolated QA browser.
- The canonical tools/valoria-reference-clean/playtest_web.cjs documents that
  Linux SwiftShader rendered fewer than5 frames in35s. Beta QA now follows its
  Windows D3D11 route using installed Chromium and records the actual renderer.
  This avoids equating the earlier software-renderer stall with the iPhone defect.
  Browser QA, visible saved-name restoration, and physical iPhone remain unverified.

The public candidate3 link is unchanged. No new tester invitations, no false
mobile PASS, no gameplay analytics collection, and no new owner retry yet.

## Native input provenance and first execution, 2026-11-08 00:52 UTC

The public candidate3 remains physically failing; no new mobile success is claimed.
The already imported native project has frozen Git code 948ffa9837168a0b68dab80be76b66350422b23a plus 147 existing generated/normalized inputs (111158344 bytes), now path/hash pinned in tools/eldoria-private-beta/native/existing-input-audit.json. Generated Bastion III-V prefabs are required by the actual scene and absent from the frozen Git tree. Do not discard them or call this a clean Git-only source. The reversible native wrapper preserves these inputs, four source overlays, ProjectSettings and prior outputs; verifies hashes before execution and restoration afterwards.

Actual Unity run 42431391139, source d5509a9cabd84fe13a6a9ed2b827bb1e92ec519d, executed the baseline, but the broad exception gate rejected an Editor Search indexing ArgumentOutOfRangeException. Its stack is UnityEditor.Search.SearchDatabase / SearchInit.IndexationOnStartup, not player code. Artifact 13598050201 (ZIP SHA256 86b1edd53ab213e968bae0e0e6e2e93adfb08624446a0dcd58f4c53490aa44e1) records failure and warm-cache-restoration.restored=true. No candidate compilation/build was certified. The exact Editor-only exception is now preserved separately in probe results; all other exceptions remain fatal. Repeat run 42431634641 uses source 67c0d4393f5f27d6c540b988beecc23e3fc0ac8e. Its result is pending.

The parser also reports preexisting DepthB10/layer_index.json.meta malformed YAML during asset import. This is outside the startup overlay and must not be silently presented as a clean import. Native canonical and WebGL gates remain required.

## Verified progress, 2026-11-08 01:48 UTC

Run 42431634641 compiled the staged source and passed both actual startup/scene reload comparisons. Artifact 13599540736, ZIP SHA256 9132a3123293833c78287986241a296497993a4fafaf89acec1a01b1475c88db. Permanent comparison: pipeline/eldoria-private-beta/native-startup-parity-42431634641.json.
- Terrain geometry/query hashes identical across both variants and reloads; 2142 verified sites and 17 cities.
- Verification dictionary retained buckets: 331431 original, 0 staged.
- Sampled Editor managed peaks decreased 97009664 and 54599680 bytes; managed memory after explicit full GC decreased about 59–61 MB.
- Total native allocation/reservation peaks did not show a consistent reduction. These results are NOT a total-process-RAM claim or physical iPhone certification.

The following canonical journey process did not finish and reached the 40-minute external deadline. Its launch used Editor delayCall, which is vulnerable to the observed Editor Search callback exception; this explanation was an inference, not a mobile crash diagnosis. The owned process was stopped by Actions cleanup, and its preserved source/outputs were recovered and hash-verified before the next run.

Run 42435095850, source 91734045e8d32cc90cc4843b4b7e08e57b1a0458, job128592182140:
- Recovery step succeeded.
- Baseline and staged startup comparisons passed again.
- Canonical I–V journey certificate passed: the pipeline advanced to ValoriaIntegratedWebBuild.Build at 01:46:15 UTC. Full artifact/restoration/browser evidence still pending.
- The launcher now uses Editor update, records checks, and has a 300-second runtime watchdog; native subprocess limit is 480 seconds, WebGL build limit 2400 seconds. Timeout stops only the owned process tree before restoring preserved inputs.
- Live phase is published in the beta/native-phase commit status; use the full /commits/<sha>/status response to read its description.
- The paired packaging/browser workflow source is 281c619331c8babafbf68d676b16bfd90c1fb338. It requires this exact native run to succeed before packaging, then requires D3D11 browser startup, actual entry/reload and visible nickname OCR. No public deployment has occurred.

Still blocked for external testers. Public mobile-v3 remains the physically failed candidate3.

### Packaging checkpoint 01:55 UTC

Current packaging/browser run: 42436120984, source 9b5cb13bfdeb28b97f240319d26a18406b909e61. Previous 42435148867/source281c619 was cancelled only while its Ubuntu package job was waiting for the native result; it had not started packaging or Windows browser tests. Native run42435095850/source917340 remains independent and is executing the WebGL build.

Corrected a real packaging defect before that step: the actual published index references play-c5232b4372dab399.js, not play.js. Both current script files were downloaded and proved byte-identical (SHA256 c5232b4372dab39994790f431da61b1beaf8ae7e26fa353cb1d891e91cb30696). The new packager writes a new content-hashed script and updates the real wrapper reference; a synthetic data/engine fixture using the actual published HTML verified that its new script exists and contains the correct diagnostic revision. This is a packaging check, not runtime certification.

Next: inspect successful native artifacts including restoration, then require all candidate/browser/name gates from run42436120984. Deployment remains unperformed. The local prepared Pages template must be filled with the actual successful package source/run, artifact ID/ZIP digest and transformed bundle SHA; never publish placeholders or failed QA.

## Compiled build checkpoint, 2026-11-08 02:16 UTC

Run42435095850/source91734045e8d32cc90cc4843b4b7e08e57b1a0458 completed native parity twice, the unchanged 225-check canonical I–V journey, and fresh WebGL compilation. Artifact13598859327 ZIP SHA25652a39d2be58704088348b944b3dfb31469e27171972c60d45c4d81f34040694a preserves the evidence. Editor managed peak deltas were -87797760 and -76234752 bytes; total native allocated/reserved peaks did not consistently improve. These are not iPhone RAM measurements.

The workflow subsequently failed restoration because Unity modified BakedSceneURP.asset and UniversalRenderPipelineGlobalSettings.asset. All174 backed-up inputs restored; existing_input_changes and conflicts are empty. Do not label the overall workflow successful. Run42437285033/sourcec54a6062bccafc67ecd83e35308953c20bcfb1d4 preserves the two modified files and diff, restores their exact baseline, and exports the already completed build without another Unity compilation. It is queued behind another native job at this checkpoint. Do not interrupt unrelated work.

The successor packaging workflow must distinguish compilation origin917340/run42435095850 from artifact exportc54a60/run42437285033. It must validate the export certificate, actual WebGL file hashes and the reviewed post-build diff before browser QA. The local package-export-aware.yml remains a draft with REVIEWED_DIFF_SHA256 placeholders; do not publish that draft until review. Public candidate3 is unchanged. Physical iPhone, candidate4 browser/reload, analytics delivery and external-tester release remain unverified.

## Preserved WebGL recovered, 2026-11-08 10:54 UTC

The original restoration/export run42437285033 stopped before mutation because the shared source HEAD changed. The correct recovery must not reset that newer source. Read-only recovery42473140363/source d32508f9ce380eb8ea9bc10fecd4fb2dfc44a2f2 succeeded, exporting the original completed build and its immutable native certificates. The first read-only attempt42473072765 failed on a null empty diff; wrapping Git diff in an array fixed that scripting error.

Recovered evidence artifact13614536700 ZIP SHA256693f40eb1f351546aa97e60fb358034a629de0f8abf8606f46446f9cf360d34d; WebGL artifact13614825021 ZIP SHA256226c413d5bd0153ba72d65a32e67e7239cc252806c83b18b2068c96062a5d6a4. Comparison, WebGL certificate, journey certificate, overlay and existing-input audit are byte-identical to pinned original artifact13598859327. All seven exported files sum to1481531944 bytes, exactly the original completed-build certificate. Actual per-file SHA256 are preserved in preserved-web-inventory.json.

The shared source is now5fcefd0060bc84bb72f8a2861e50b2da729fb594, with56 pending status entries. Both former URP restoration paths have empty Git diff against that current HEAD. Recovery did not change the source and does NOT claim the original frozen worktree was restored; the earlier failure remains recorded. The recovered build remains explicitly unverified for browser/iPhone, independent of current shared-source changes.

Candidate pipeline42473231989/source be998760db83e8e9096d2acdb70fd84dbfdd48a1 checks exact recovery provenance and all file hashes, binds the canonical225 checks to compiled source91734045, then packages and runs actual browser gates. Compilation origin remains42435095850; recovery artifact origin is42473140363. No new public deployment yet.

## Payload growth and actual QA infrastructure diagnosis, 2026-11-08 11:10 UTC

Package42473231989 passed byte integrity for57495 objects and104 changed textures, but site artifact13614293242 is1387757813 bytes, above the available Pages budget. It is NOT publishable. Browser job128710397966 failed before launching Chromium: http-error.log states Windows python.exe is the Microsoft Store alias without Python installed. This is not a runtime game failure. qa_static_server.cjs now uses installed Node; actual local HTTP HTML and unaltered gzip-byte tests passed.

Read-only payload audit42473845228/sourcea770bbe8, artifact13614682468 (ZIP0bf829411caa58c228c6eb3e58725da9f2c444c275d5d3a9518eb7a50497c34a), found449 Texture2D objects /290 names,3291285980 serialized texture bytes versus the older1089250768. StoneArchitectureKit basecolor/normal/rm each occur17 times; MidTierArchitectureKit textures occur6 times. These are serialized bytes, NOT process RAM. Actual data.unity3d is1701127443 bytes. Do not distribute this regression.

New reuse matching requires identical pixel bytes, mip data and every sampler/color/shape metadata field; only name/readability are ignored in the catalog key, and semantic reuse is allowed ONLY for already non-readable fresh textures. It changes only format, complete image size and image payload, preserving names/readability/all other fields. No new compression decisions or unmeasured quality relaxation.

First semantic run42474108636 stopped at immediate readback. Inspection of installed UnityPy1.25.4 source confirms read_typetree and get_raw_data read the original backing stream even after save_typetree stages bytes in obj.data. Correct validation hashes obj.data and validates actual reopened saved-bundle metadata/pixels after serialization. Current sourceec97416b67ded8bf73fc13a748903e6888937c02 / run42474478525 contains that correction, Node server, and a900000000-byte candidate size gate. Verification remains pending. Separate entry workflow42474257955 correctly blocked when its exact package failed; it is superseded by the unified Node-enabled run. No public deployment, iPhone PASS or tester readiness claimed.


## 2026-11-08: measured payload duplication; conservative candidate only

Run42474779454 passed semantic reuse (114 textures, all102 reference transformations), all57495 serialized objects verified, but the packaged site was1332582109bytes. Additional ASTC quality-checked material compression in run42475552834 accepted123 textures and preserved all object integrity checks, but site size1017726761bytes still exceeds the remaining Pages budget. Existing unity-owner payload503512300bytes must be preserved; combined limit1073741824bytes leaves570229524bytes, before safety margin. No further quality reduction is justified by these results.

Read-only audit42476471528 (source9a28444d98c11930f41bf48c2182327eb0009c4d) found146 redundant Texture2D objects totaling1923809944serialized bytes, including17 identical copies of architecture maps. This is serialized payload, NOT measured process RAM and NOT proof of the iOS crash cause. Audit scanned402563 references (667 into duplicate groups).768 opaque/unparseable objects require conservative protection of any candidate ID present in their bytes.

Candidate-only tool deduplicate_exact_textures.py (c132c9bf13b4fb99ec0434794f3a4ed246be9285) excludes CPU-readable textures, streamed textures, small/font/UI/hero textures and limits aliases to architecture/terrain name families. Any opaque or unresolved reference protects its target ID; pointer-bearing objects require byte-exact typetree roundtrip. Saved bundle is reopened and every surviving object's bytes and redirected reference are checked. The original build is untouched. Workflow source25c6b4c53c8e078a660cf29ef809e2309e3d7439 runs this verification; runtime/device outcome remains pending. Current public candidate3 remains the failed owner-tested version. No new URL or external-tester readiness claimed.


## Physical staged4 failure and diagnostic pivot — 2026-11-08

Owner evidence IMG_5046.jpeg (Library libfile_769ec576ab108191a2786dcfd420d94e): startup post-run and ready at 14:47:01 local, heap495MiB; boot back_forward14:47:05. Owner reports game visible about2seconds then returns. This revokes mobile readiness, not the narrower desktop startup evidence. No confirmed iOS crash cause and no measured process RSS.

Public desktop run42484725949/source7ee70c721322466e04e0fff25b7d6c4984a6941e passed world-ready plus60sec, artifact13619076890 SHA80e04bc1d8d7849e61801c9223014ec1c881b349b01807754facb49f2d660ff6. Its samples show WASM capacity727384064bytes at46.617sec and1168179200bytes at49.686sec, while JS used memory was about702MB. Do not add these metrics as a claimed process RSS. The old diagnostic sampled heap only in postRun, before substantial subsequent allocations. Thus ready495MiB is not a peak or world-ready assertion.

Inspected loader/front-end has no history.back/navigation call on successful startup. showError returns to landing without a document navigation, so it does not by itself explain the new boot back_forward. Inspected RegionOneCanonicalLandscape.Start eagerly constructs the full region while the player is in the city. Memory pressure remains a hypothesis; delaying the world alone might merely move the failure to the first world visit, so it is NOT a certified fix.

Published diagnostic-only Site version3/source7dbe7f2a9bfc05a516076380470ba677e38157d9, deploymentappgdep_6af07ea7ac04819194c9dabca01fea92 succeeded. Same compiled game and payload. Opt-in ?eldoria_diag=1 now exposes Diagnóstico v4 and saves bounded local growth requests BEFORE allocation, grow success/errors, post-ready heap changes, world import/world-ready milestones, ASTC availability and context loss. No network reporting, nickname/save access or game-memory-policy alteration. Native WebAssembly grow return/RangeError behavior, disabled-path isolation, phase dedup and heap-change logging tested in Node; compressed/decoded payload hashes verified. This is NOT another claimed corrected game build. Physical evidence is needed from this diagnostic to distinguish post-ready growth failure from context loss or another termination. No Windows lease held.

Diagnostic URL: https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&probe=4


## Mobile5 correction — 2026-11-08

Implemented an explicit mobile material-resolution profile on the frozen staged4 build. Only existing authored lower mip levels are retained: no texture resampling/re-encoding, geometry, game code, tutorial, clock or save changes. Select only non-readable Texture2D material references with existing mip chains; exclude sprite references, CPU-readable/query textures, unsupported formats and streamed textures. 35 textures changed, 298319872 serialized bytes removed, all57375 objects and non-payload metadata verified after reopening. This is a lower-detail mobile profile, not pixel-identical texture data. City screenshot inspected; no obvious corruption. Full-world visual equivalence is not certified.

Source tools/mobile_mip_budget.py commitbd25e12bd21f91982118df6fff8a10a41c6f1e12; package generator commitad41880e219de2cb34b61446b6045563067f1285 corrects stale delivery metadata. Tested artifact source300b78e31a72bbdbd74c6681a75b04987d28bd31/run42487738914 SUCCESS: native object proof, actual NVIDIA startup/world initialization+60sec, entering nickname, graceful Quit+real reload,45sec and OCR persisted nickname. Artifact13619839245 SHA0c81412f97134dbd928182445171b00b65bdb87403a1e856ea6d3afc53c0329d. Runtime proof13619959652 SHA4191d53f87166838c1ae7ff5e24f73bb3e462fa1001bec59a844666fd92ea45e. OCR13620586439 SHA3b44d13a231b942663531e63c8c2ecba0c7407a7875c5c64e9f8e367a17549cb.

Bundle2c502a1d766aca83c2377347de290467d16d75efd8c6f5edad41766f4d3ce73a,493061804bytes; data500847489bytes; site455991538bytes. Peak structural WASM capacity1064108032bytes versus1168179200before (~99.25MiB reduction). After-name1064108032; reload727842816. These are capacities, NOT measured total process RSS. The actual peak reduction is smaller than serialized-byte savings; do not claim all284.5MiB as runtime RAM savings.

Published Pages run42487975751/sourcedacba45b0e686e11b512493bfc0b3f74888d3c18 SUCCESS after exact QA gate. All active payload now on Pages. Sites entry version4/source343251bba62e6ddd52d2f54c727aae57e154887c/deploymentappgdep_6af083414b6481919608003893646797 SUCCESS, no accounts. Actual public runtime verification run42488719340/source739076c7ff194af1384b5a5ad86cbd4706998d5b started; record final outcome in readiness. Public manifest matches prepared manifest byte-for-byte. Physical iPhone result remains UNKNOWN for mobile5; staged4 physical FAIL remains valid history. No external testers authorized by these narrower checks.

Owner trial URL: https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=mobile5
