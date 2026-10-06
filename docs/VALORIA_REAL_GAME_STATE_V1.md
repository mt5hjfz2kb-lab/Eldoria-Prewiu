# Valoria real game state v1 — BLOCKED, not completed

The same workstream `valoria-real-game-state-v1` remains the only unfinished block.
The closed visual reset remains LOCKED and unchanged. No new method, workstream,
expansion, paid credits or commercial assets are authorized.

## Exact latest failure and minimum recovery

Source run [37481653464](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37481653464)
failed before inference: the empty-ground PNG was truncated during transfer.
Its repository blob was `5ab43ed656e50f47eae122bec183b702a597cc70`, whereas the full
local bytes have Git blob `e42da612f2af5193bda56fe4e8ecc7f238ca584a`.
Commit `7eda2d6228170901b651975b10ec87bb6b05eb58` restores the complete file.
SHA-256: `e8b85859da339d8dd0f71a245e623ff1f1a24fc0a093314751e70ec5888ada73`.

The next/latest source run
[37481997235](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37481997235)
passed authority and ground-input fingerprint checks, loaded the same SHARP Space,
then failed at `/run_sharp` with:

> You have exceeded your ZeroGPU quota (180s requested vs. 163s left). Try again in 10:33:36. Authenticate with a Hugging Face token for more quota.

The rejection was logged at 2026-10-06 14:49:11 UTC. The service-indicated reset
is approximately 2026-10-07 01:22:47 UTC / 03:22:47 Europe/Madrid.
Artifact **11422270605** contains input/API/lineage metadata, **no PLY**.
The generated empty-ground bitmap exists; its SHARP Gaussian terrain does not yet exist.
Do not call this source generation PASS.

Recovery: rerun this existing source workflow after its free quota resets, or
use an owner-provided authorized Hugging Face credential through a runner secret,
after verifying the same route's free quota. Do not buy credits, change Space,
rotate anonymous identities, start another generator or reopen R&D.

## Evidence retained and limits

Runtime capture run **37479037112**, artifact **11420563514**, demonstrated real
`LocalGateway` commands, costs, due timers, chapter-I missions, Bastion ascent,
both parcel states, `FileStateStore` reload during both builds and idempotent
completion. All **81/81** semantic raycasts passed at the bounded pan/zoom combinations.
Its overall workflow failed in the real building-panel PlayMode test.

Direct review rejected the visual: transplanted terrain was blurry/discolored,
and separately rendered SHARP layers introduced ordering artifacts.
These captures are failed candidates, not promoted gameplay evidence.

| Requested criterion | Current evidence |
| --- | --- |
| TECH PASS | NOT COMPLETE: state capture passed; building-panel test failed |
| INITIAL GAME STATE PASS | Logical fresh levels correct; empty-parcel visual not approved |
| PARCEL STATE PASS | Four derived states demonstrated for existing sawmill/barracks |
| BUILD PROGRESSION PASS | Existing command/timer route demonstrated; integrated panel gate pending |
| SAVE/RELOAD PASS | PASS in state capture for both construction timers and completed state |
| INTERACTION PASS | 81/81 semantic rays; real panel gate pending |
| VISUAL REGRESSION PASS | FAIL: empty terrain and layer ordering |
| BOUNDED CAMERA PASS | PASS in state capture; correction must be rechecked |
| Overall closure | BLOCKED / NOT PROMOTED / NOT COMPLETED |

The original reset's visual PASS remains valid for its original built reference.
It does not imply PASS for this unfinished initial-game-state candidate.

## Reusable parcel production flow

`parcel region/mask → empty ground → runtime state → construction → built`

1. Declare a parcel in a versioned region manifest: stable parcel ID, existing
   gameplay building ID, image-coordinate space, mask shapes, ground source
   lineage, interaction anchor and variant bit. Geometry must not contain
   a second progression model.
2. Author only uncovered terrain with the canonical image as reference.
   Record exact bytes/hash, preserve the canonical authority separately, then
   run the **existing** SHARP Space route. Accept only a verified PLY and lineage.
3. Generate conditional full-scene variants from a common source. Preserve all
   records outside the declared masks. Completed parcels use original records.
   Use one renderer for the combined scene to preserve global splat sorting.
   Mask parsing/variant generation should iterate manifest entries; adding a
   parcel must not require another handcoded sawmill/camp branch.
4. Bind each parcel to the existing authoritative state fields, unlock rule,
   Build command, cost, shared queue and completion timer.
   Derive NOT_BUILT / AVAILABLE / UNDER_CONSTRUCTION / BUILT from that state.
5. Show terrain before completion, availability feedback at unlock, reused
   scaffold geometry during the real timer, and original building splats only
   when the existing completed level says BUILT.
6. Validate real panel commands, spending, locked/occupied queue rejection,
   timer completion, save/reload, duplicate command safety and tutorial/mission
   advancement for every parcel; compare HOME and bounded pan/zoom against
   the locked look. A screenshot with hidden objects is insufficient.

The concrete current mapping remains sawmill (left, AVAILABLE at Bastion I)
and barracks (right, NOT_BUILT until Bastion II). A camp visual is not authority
to invent a new gameplay building or start it completed.

## Representation inventory

| Element | Class | Representation |
| --- | --- | --- |
| Bridge, lower gate, road, stair, upper walls/bastion | Permanent map | Locked SHARP beauty; seven approved aligned real GLB support families hidden in beauty |
| Left sawmill/cabin, right military camp | Player construction | Original SHARP building records conditional on existing completed levels; no new editable building mesh claim |
| Empty parcel ground | Visual support | Pending verified same-route SHARP terrain derivative; never a building |
| Construction scaffold | Real geometry / temporary feedback | Existing ValoriaKit module, driven by authoritative construction timer |
| Parcel interaction | Real runtime collider / UI | Existing WorldHotspot, SlicePresenter building panel and gateway |
| Background, river, trees, cliffs outside masks | Visual support / SHARP | Preserve approved records and Gamma response |

## Safe resumption

A draft correction is preserved as `pending-correction.patch` beside this evidence.
It is **not applied or certified**: it swaps four full-scene assets in one renderer,
isolates the failed UI test, and stages a Windows player build. The draft still
requires manifest-driven generalization, ground artifact wiring, compilation,
actual UI testing and direct visual review. Do not blindly promote the patch.

Resume the same ID from live main; obtain the missing free-route ground PLY;
finish the generic region contract; integrate and rerun capture + real panel +
save/reload + bounded camera. Only after all requested gates PASS move to
completed. No Valoria expansion follows this block.
