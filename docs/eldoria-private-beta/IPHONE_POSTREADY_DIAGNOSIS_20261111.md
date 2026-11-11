# iPhone Safari P0 — post-ready recurrence and controlled phase diagnostic (2026-11-11)

## Physical result (owner, IMG_5066, 08:17 CET)

- `native325-mobile13-prebaked-collision-qa` physically **failed** on the owner's iPhone Safari despite successful native Unity, exact collision prebake, packaging, public file checks and desktop WebKit.
- `08:17:13 startup-progress 100; startup-post-run; startup-heap-mib 412; startup-ready; resize vertical`.
- `08:17:20 boot back_forward`, followed by `08:17:21 pageshow normal`.
- The owner saw the nickname prompt and a visual size change before the involuntary restart. Ready-to-boot interval: approximately 7s.
- The screenshot establishes a new page load, not **why** it happened. `back_forward` is not evidence of user navigation; the 412 MiB figure is a heap-capacity observation, not total iPhone process memory. Neither jetsam, GPU context loss nor watchdog termination is confirmed.

## Measured earlier desktop evidence (Mac WebKit artifact 13810653131, run 42916816001)

- Mac WebKit beta13 candidate entered, kept the nickname `Beta QA` and reloaded; **this did not reproduce or exclude the iPhone failure**.
- With the existing engine URL `eldoria_profile=1`, actual Unity log phases on the candidate started at `prefab-before` 22:52:30.232 UTC and ended at `REGION1_CANON_WORLD_INSTALLED` 22:52:37.114 UTC; approximately 6.9s for that first initialization sequence.
- The measured engine allocation rose from ~292,387,576 bytes at `prefab-before` to ~367,893,791 bytes at `creatures-after`. These are Unity Profiler allocator snapshots, NOT total WebContent RSS or GPU residency, and should not be added to texture/mesh inventory.
- Logs show `prefab-loaded`, `prefab-instantiated`, `verify-index`, `contact-ready`, `pigment-ready`, `placement-after`, `resource-models-after`, `creatures-after`, and `REGION1_CANON_WORLD_INSTALLED`. This is a genuine *post-Unity-start* heavy-initialization candidate window, not causality proof.
- Recorded Mac WebContent peak RSS in the exact source comparison was ~2,651,088 KiB beta13 versus ~2,640,304 KiB control. This is desktop RSS and does not establish physical iPhone memory pressure.

## Source analysis and hypothesis

- `RegionOneCanonicalLandscape.Start` begins creating world presentation once host bindings are ready, before the player explicitly navigates to World. Its `BuildStaged` instantiates the Blender geography and subsequently constructs contact cells, placement, resource models and creatures.
- `ValoriaIntegratedWorld` initially hides `WorldRoot`, but inactive rendering is not the same as avoiding `Resources.Load`, mesh instantiation and other runtime allocations.
- The physical failure occurring seven seconds after `startup-ready` is temporally consistent with heavy initialization seen on Mac. However there is no synchronized iPhone phase trace yet, and the OS crash reason is unknown.
- Previously rejected physical approaches (ASTC/texture-only reductions, stable-canvas-only, direct transport, static collider prebake) must not be repeated as asserted fixes. More speculative allocation-only micro-fixes without a material RSS reduction are unjustified.

## Implemented, staged and verified diagnostic (NOT a new fix)

- `iphone_ab/postready_profile.js` is an opt-in, locally persisted, bounded (48) recorder, capturing allowlisted Unity memory-phase labels and viewport changes. It deliberately records no player name/save content, event payloads of JS errors, analytics requests or server uploads. It does not change compiled WebAssembly, data, gameplay or navigation.
- `iphone_ab/stage_postready_profile.py` applies only the landing shell over the already certified beta13 archived artifact; verifies exact source SHA and prohibits any differences in `Build/` and `payload-decoded/`. The original beta10 is not in its output or changed.
- `iphone_ab/postready_profile.test.cjs` covers opt-in, bounded persistence, resize and cleanup. `test_postready_profile_stage.py` covers exact source fail-closed checks and immutable engine.
- GitHub Actions run [42983418050](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42983418050), Ubuntu stage success: **3 Python tests PASS, Node tests PASS, exact beta13 artifact staged with unmodified engine**. Mac Apple GPU WebKit checks subsequently PASS, but phone test remains outstanding.
- All results and artifacts are **owner-diagnostic only, not published or phone-certified**. A new instrumented replay must never overwrite the beta10 route or open external tester invitations.
- A follow-up Apple GPU run tests a **fast nickname entry without screenshot delay**, verifying that the local recorder actually persists Unity phases. It is automation, not physical evidence.

## Engineering direction (not yet implemented)

Unity recommends **keeping a nearly empty initial Web scene and loading large assets on-demand** using Addressables/AssetBundles, to avoid large initial virtual data files and redundant browser memory copies. This approach is a more structural option than further JSON/widget/viewport changes. Before using it, prove asset ownership/dependency boundaries, compile an isolated actual Unity candidate, preserve visual quality/geometry/terrain query tape/zone population/save, verify Bastion I-V and browser first-click gameplay, and measure Mac memory delta. A successful Mac comparison alone is not enough to claim an iPhone fix.

Official references:
- https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/memory
- https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/assetbundles
- https://unity.com/blog/engine-platform/unity-asset-bundles-tips-pitfalls

**Gate:** P0 #96 OPEN, PR #179 DRAFT, beta10 intact; no external tester invitations until physical iPhone stability plus other readiness requirements are verified.
