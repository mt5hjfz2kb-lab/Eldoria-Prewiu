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
