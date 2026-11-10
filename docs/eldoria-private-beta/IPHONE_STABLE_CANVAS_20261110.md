# Current status — historical transport experiments rejected

The physical owner rejected stable-canvas-r1 and direct-r1. The live direct route now serves beta13 with a newly compiled collision-prebake and initialization-memory candidate; the original direct-r1 URL below is historical and no longer identifies current bytes. [Current implementation and verified evidence](IPHONE_INITIALIZATION_MEMORY_20261110.md). Beta13 passed native and desktop public WebKit tests, but physical iPhone stability is still unverified and testers remain blocked.

# Current result — physical failure supersedes desktop PASS

The owner reported that stable-canvas-r1 still restarts on the physical iPhone. It is rejected as a solution. Do not reinterpret the previous Mac comparison as physical success.

## Direct transport comparison (same compiled game)
A separate owner route, beta-iphone-clock-direct/?transport=direct-r1&eldoria_diag=1, removes in-browser copy/add reconstruction. Every one of 31 decoded parts matches mobile12; total503831429bytes. Stage source5555c2b3fed6b6b5c644679d42272254bffeedec. No new engine or timer change.
- Local Mac WebKit run42893674666 passed.
- Actual public WebKit run42895738602 passed entry/name/reload; artifact13801096099. Reload screenshot visibly retains Beta QA.
- Public verification42898405543 checked all181servedfiles byte-for-byte and confirmed original beta10 redirect/save origin unchanged.
- Physical direct-r1 FAILED: owner screenshot IMG_5065 reaches ready at22:24:15 and records another boot at22:24:23. This mitigation is rejected; never present its desktop PASS as a phone fix.

## Publication lesson
Run42895222597 deployed successfully but its final verifier had an overescaped regex. The script was corrected. A redundant deployment42896193107 hit the hard600000ms cap: deploy-pages@v4 explicitly clamps even a1800000ms input. Do not claim that a30minute input overrides this cap.
The corrected workflow first compares every served file with staged bytes and skips redeployment only on a complete match. Run42898405543 verified181/181 and passed final live checks without redeploying identical data.

## Simulator evidence boundary
Run42890758782 timed out creating Appium/WDA sessions. Run42894597804 could launch Safari but native SafariDriver timed out waiting for its RWIApplication; no game samples existed. Neither is a game failure or an iPhone stability test.
Warm-up attempt42897348434 failed even earlier: simctl could not open about:blank (LSApplicationWorkspace115). The next attempt launches com.apple.mobilesafari directly, waits for initialization, and permits one bounded session-creation retry. Do not retry every game variant if infrastructure cannot establish its first session.
Later simulator run42908325634 reached Unity ready in both arms but then recorded GPU-process crashes and a lost SimMetalHost connection. No valid120-second stability PASS exists. See the initialization-memory document for the corrected evidence.

---

# iPhone P0 — stable canvas candidate, 2026-11-10

Canonical workstream: eldoria-private-beta-i-v-20261107; PR #179.

## Facts and limits
The owner reproduced a restart in Safari. The supplied screenshot reached ready at 11:59:36, recorded four vertical resize events, then another document boot at 11:59:51. This establishes a restart/navigation observation, not its cause. The supplied earlier system report cannot identify this game tab and does not correspond to that reproduction. Do not publish the private report or blame another app.

Preserve beta10 and its original redirect/save origin. The candidate engine remains source 004d3ef9c26e9070762e9e38ce84d69d525efd4f. The actual live copy transport is SHA256 7b0717a4e4b655861084cf33f9c3df07e1e3c70660d2b8a6600d0c6a2ca98725, from source 836fce54e33e0c3c5cc87ea4ca480da5eb44835a / run 42785047968 / artifact 13749626125. Do not substitute the unpublished forward-only transport in this experiment.

## Rejected inference
Run 42793879891 disabled OMG JIT on the same Mac browser and files. Memory did not improve. Native stack samples alone do not establish compiler causality. No WASM-padding workaround is justified.

## Implemented change
stable_canvas.js decouples Unity render-target sizing from CSS height-only changes, preserving the existing 1.5 DPR cap. Width changes, including rotation, still update size with debounce. It changes no game logic, storage, history, engine bytes or transport.

Three focused tests pass: repeated height changes retain dimensions; rotation updates once; disposal removes work. The guarded stage helper rejects mismatched before/after hashes and gives the tested wrapper a content-hashed filename.

## Browser evidence
Run 42810897933: candidate entry/reload succeeded and screenshots show Beta QA after reload. The control was interrupted by a Node profiler write-after-end error, NOT a demonstrated game crash. Fix 052d8c65edf0191fa819d7d08438c6ab172ccf83 keeps both output pipes open until process close. Repeat comparison run 42812046053 is the publication gate.

The browser probe's earlyEntry secondsAfterReady includes the interaction duration, not only the initial click. It must not be described as a precisely timed two-second phone reproduction.

No desktop result certifies physical iPhone stability or a lower iPhone memory peak. A fixed render target is a candidate mitigation; the restart root cause remains unconfirmed. External testers remain blocked until physical stability and the remaining readiness gates pass.

## Completed repeat
Run 42812046053 passed with head 79b9fdf8862a502a2452be88571d6d42658b532d, artifact 13763721526. Both control and candidate passed entry/reload. Candidate render size stayed 585x1266 through all four viewport changes; control resized four times. Reload screenshot visually confirms Beta QA. Single-run Mac peak footprint was 3.7G control and 3.4G candidate; this is not a statistical or physical-phone memory claim. Publication is guarded by that exact successful run, using script SHA256 961e07c2da9490e5b0fd033440689219468d55ced6ae80b8f4a31c63bba14434.
