# Prepared timing candidate; not applied to the active PR179 branch

The current deferred terrain index yields after every 256 triangles. The unchanged actual source terrain has 733288 triangles, imposing 2864 frame boundaries before resource (97 rows) and route (449 rows) work. This is an analytical scheduling floor, not a phone timing measurement.

The local alternative keeps the exact triangle/cell insertion and query order. It checks a 4 ms elapsed-work target after each complete 256-triangle batch and forces a yield by 16384 triangles even with a coarse clock. Resource and route scans preserve whole rows but yield on elapsed work or by eight rows. Reset is AFTER resuming so browser idle time does not consume the next slice. This target is not a hard 4 ms cap: a complete batch/row can run longer, and prefab import, verification and pigment fitting remain separately synchronous.

Actual-source laboratory: all 1169750 contact bucket entries and their ordered triangle indices have identical SHA256 4ce21951600a1a95e93283ec6ce875d733a885e2eea0a4ad2c07df80cfe83629. Original policy imposes2864 boundaries; local alternative uses45 on this Linux CPU. Do NOT extrapolate45 frames or the recorded CPU timings to Unity/WebGL/iPhone. Single-pass CPU comparisons are warmup-sensitive. Helper clock/quota regressions and geometry comparison total92 checks; six focused source checks pass; Roslyn parses the four world sources with zero syntax errors. No Unity API compile, renderer, route-query replay or phone result is established.

Reproduce with SDK8+ and Python:

    python3 tools/eldoria-private-beta/frame-slice-lab/run.py --repo . --glb Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RegionOneBlenderWorld.glb --output /tmp/contact-lab-result.json
    node --test tools/eldoria-private-beta/lazy-world-frame-budget.test.mjs

Runner repair is also prepared: resolve already installed Git before checkout, require supported version, refuse archive fallback, bound checkout step15min. Explicit fetch-depth1/filterblob:none documents the intended sparse fetch; checkout may already choose these defaults when Git works. The unavailable live checkout logs do NOT establish the root cause of the current checkout stall.

Application conditions: retain active HEAD8ce635c61bcbee5b4da2e5f12dd5ba5b43a0d47e while native job130766199765 is active. Do not update its branch/source underneath it. After that job ends, inspect result and restoration before any ownership change; give Windows to PR181 next, then187 and173. This prepared candidate initially sets request state WAITING_FOR_WINDOWS_TURN, so moving it to the canonical branch cannot authorize native execution without renewed explicit live CAS. If canonical HEAD advanced, reconcile the same PR179 rather than force-update or open another PR. On its next legitimate Windows turn, renew owner revision and request/pins, then demand exact Unity PlayMode/contact parity, real I-V journey, WebGL compile, Mac same-binary A/B and physical-device verification as applicable. No deployment or tester GO from this lab.

No geometry, materials, textures, economy, saves, placement ordering or camera changes are proposed.
