# World first-entry P0 — continuation evidence, 2026-11-14

Canonical PR: #194. Owner: chatgpt-world-p0-continuation-9df36d17, revision 80.

## Confirmed observations

- The physical iPhone failure reported by the owner occurs on entering the world. Its cause is **not confirmed** by an iOS memory or crash report.
- Native run 43633477587 was blocked by test compilation: `Application.LogCallback` and `Application.logMessageReceived` resolved against the project's `Eldoria.Application` namespace. Explicit `UnityEngine.Application` fixes that concrete compiler error; it does not diagnose the iPhone failure.
- Run 43638950225, source `8e2eac0ac4831059fabd9bb4639bb2667e3dac19`, passed the real Unity PlayMode test with original art: 51 audited Bastion III source files restored, 48 resource nodes and 36 logical creatures preserved, first world entry approximately 12.028 seconds, twelve reentries, actual probe save reload. This is desktop Unity evidence only.
- The first six-camera loop changed cameras without yielding between them. Those images are genuine Unity renders but do not certify settled streamed content. The next fixture waits for streaming and checks expected versus actual visible creature models in each camera.
- A hard cap of twelve resident creature models could leave otherwise-visible enemies without their representation. Source `5881df4e76da72c2897633c619449c60694b3552` removes that cap, retains two creations per refresh and exactly one current LOD per visible target. The logical population and original models remain unchanged.
- The native evidence directory now includes the run ID. Older output accidentally included stale `editor.log` and `missing-native-art.txt` alongside the current successful log; these stale files must not be treated as current results.
- Required frozen web CI failed because Playwright 1.55 did not support the new Ubuntu 26.04 image. Pinning that job to Ubuntu 24.04 restored execution without weakening tests. This legacy web CI is not Unity or iPhone validation.

## Loading-screen alternative

An overlay can communicate the initial wait but does not itself release the retained Unity assets, reduce a heap requirement, or establish the cause of a Safari process restart. The world already starts lazily. The substantive candidate change is bounded creation of visible resource/creature representations. No claim of measured memory savings is justified until the frozen WebGL candidate is built and compared. A future loading treatment should follow the actual readiness state, not a fixed timer or a second HUD/gateway.

## Exact-source verification chain

1. Run 43641168362 tests source `5881df4e76da72c2897633c619449c60694b3552`, including six settled camera coverage checks, repeated navigation and save reload. Passed. First entry 11.890 seconds in desktop Unity; six coverage pairs expected/actual: landscape close 1/1, regional 22/22, panorama 0/0; portrait close 0/0, regional 28/28, panorama 0/0. All six original PNG hashes verified and images visually reviewed. This confirms why the old twelve-model cap was insufficient in regional views. Artifact 14132713638, ZIP SHA256 `65e5155cec36292035240229509bf5270197e36fd17a0a10abb84c66f1aa1771`.
2. Build run 43644208819 is running from workflow commit `7ec789ab4a13cf4bf1267fa0cc5b8dc039579e61` and rejects a failed/native-logical-only result. Earlier preparation attempts stopped before Unity; they do not supply a build certificate. It compiles the **same frozen source**, separately recording its workflow commit. It reuses only the owned native checkout, rechecks original art and terrain hashes, executes the real UI and campaign methods, and preserves inputs.
3. Packaging reuses the exact already-established mobile texture profile through serialized-byte matching. It must retain exact source/UI/campaign/build certificates. It does not publish or authorize testers.
4. Browser comparison is prepared to operate real visible controls, earn and reload a sawmill, enter and leave the world twice, and retain screenshots/logs/process samples. This is desktop WebKit evidence, never physical iPhone acceptance.

## Outstanding

- Latest native checks and six-image review completed; no overall artistic approval implied.
- Exact WebGL compile and packaging, actual browser world-entry result and image review.
- Physical iPhone acceptance. No source check, desktop run, or cosmetic loading screen substitutes for this result.

## Reuse lessons

Qualify Unity's `Application` in this namespace; require actual NUnit XML rather than an Editor exit code; use unique run evidence directories; let deferred creation/destruction settle before screenshots; count visible saved targets rather than imposing a visually destructive resident cap; reuse the owned Git object store without cleaning another checkout; protect original assets and saved games; keep compiled and workflow SHAs distinct.

Live identity checked directly on 2026-11-14: the published beta19 source is `90465a9a910b89a8a8b74e4b33a9bbc88578e00e`, not the older a2e956 entry retained in registry history. Browser control must pin the observed live source.

Build preparation correction: GitHub documents that RUNNER_TEMP is emptied at each job boundary (https://docs.github.com/en/actions/reference/workflows-and-actions/variables). Therefore the initial build script must not require the preceding native checkout to survive. Recovery fetches only its pinned control script, finds installed Git explicitly, uses an owned persistent source location, and restores only the same hash-verified original assets and terrain. Run 43642412179 remained in checkout before Unity; the corrected retry supersedes that preparation without changing the verified gameplay source.


Continuation checkpoint, 22:25 UTC: exact native coverage remains valid. The compiled package is still pending; no owner deployment or browser trigger has been sent. Browser comparison now also samples the 12 seconds after the entered marker, exercises the production 1/2/3 zoom shortcuts, records WebContent/GPU resident samples and a read-only requestAnimationFrame gap observer, and rejects critical allocation/context/runtime errors. Shortcut input is not proof of changed views: actual screenshots require review. RAF samples are desktop browser observations, not iPhone timing or a Unity profiler trace.

Publication preparation retains the current 145-file public inventory by byte/hash recovery because the old Pages artifact has expired. Recovery and staging tests passed (3 and 8 respectively). Publication remains gated on the new build, actual browser evidence and independent review, exact staging governance and live source ownership. Current published beta19 and stable beta18 have not changed.


Exact build/package43644208819 completed successfully. Native job132598309828 retained artifact14134931795 (ZIP e9391cf620a0b62860edd45323dbd945d040ab7a7b8732a6d3e5bf6e54d5c5d4), restored inputs and reclaimed only generated cache, leaving70.1GiB. Windows resource released. Package14135597114, bundle15483a1f63f250f2e80ec57bfc45b6f8b608ea26c363545376bc8c707ef76a90, data bcce3c1def1cb85eb03648aecbd4ea31d931e99b126719965987d3214c188cc9; all57656 serialized objects verified with established texture profile.

Browser43649546135 retained artifact14136280792, ZIP3626e48cb335cd7fb73f0e24c713e64ff09f974e7ae7889c2db662bc4f84c48f. Candidate startup/reload passed and candidate world entered twice; whole job failed on control19 reentry. Image review found a HARNESS ERROR: control19 world footer has two buttons, with city at225,776, while candidate has three and city at128,776. Nickname remains visible in both scenes, so the old return assertion was insufficient. Control19 city-return-0 actually still shows the world. Do not call this a control crash. Both sets of keyboard zoom screenshots remained unchanged: integrated Valoria explicitly disables ZoneOneStrategicCamera.LateUpdate and routes actual zoom through SlicePresenter. Replace shortcut attempts with real wheel gestures and require substantial central-image changes. Verify actual saved sawmill after city return. Keep the failed artifact and repeat only affected world comparisons; reuse unchanged startup on the identical package and the original native certificates. Initial timing/memory samples cover close-view entry, not a validated zoom comparison, and cannot establish physical iPhone improvement.


DELIVERY REJECTION AND BASE RECONCILIATION (15 November Europe/Madrid): comparing the full source to the actual published90465 exposed that5881df inherited main's older navigation. It would undo bag-to-inventory routing, world/city toggle, envelope art and barracks training access. Native/world PASS did not catch that stale baseline. Do not publish588. Source d0b21fd5708c5070d1bd62dc6c6a98399c549051 consumes the full already-published90465 tree with the24 P0 source/tool entries carried forward and WorldEntryPending retained. All BreachWatch blobs are byte-identical to90465; no new authoring of the active artist's scope. PR194 targets the existing frozen feature/eldoria-private-beta-i-v-20261107 branch to describe only this world repair; that base branch and main UI are not edited. This follows the protocol's consumption of already committed/promoted outputs, not a takeover of unfinished work. Main integration of the delivered UI is separate and must not silently regress it.

15 focused lazy-world/UX tests passed on reconciled source. A single combined native run is prepared to certify six settled world images,12reentries/save, exact delivered UI, native I-V and WebGL with one cold import/cache. The prior native/package certificates remain evidence for the rejected588 source only; they cannot certify d0b21fd.

Second browser43651540315 artifact14137005353 ZIP a74efee964b7c6cd51e9855ab0afbe207f0604384557da4c2d19ef042506e847: actual wheel changed the first image, then stopped changing both versions. Reviewed first image shows central Entrar en Valoria moved under pointer195,420, so production UI correctly consumed further wheel gestures. Move real pointer to350,200 away from that central button; keep the substantial pixel-change assertion. Do not claim a game zoom failure or weaken the gate. Both approved sources retain the two-control world footer at225,776. Old588 remains rejected regardless of browser outcome.

Initial close-view process samples were mixed: WebContent peak decreased but GPU peak rose; simultaneous WebContent+GPU sampled maxima were3031536KiB(control) versus3167904KiB(rejected candidate) on first visit. These are desktop observations, not a memory-reduction certificate or physical-iPhone diagnosis.

## 2026-11-15 — reconciled native and real-browser proof; memory gate remains open

- Frozen runtime: `d0b21fd5708c5070d1bd62dc6c6a98399c549051`, published baseline `90465a9a910b89a8a8b74e4b33a9bbc88578e00e`; BreachWatch source preserved byte-for-byte.
- Combined build run `43653390105` passed native world PlayMode (12 reentries, actual saved-state reload, six settled world views), native UI, authored I–V checks, WebGL build and packaging. Windows cleanup completed and its claim was released. Cleanup log observed 59.28 GiB free; this is a historical measurement, not a current free-space guarantee.
- Original native artifact `14138742713`, ZIP SHA256 `fafba2ffe49a601194b67de56ed8531bf9b455cf14c4a786881db80b3de776d5`: XML 1 passed / 0 failed / 0 skipped; 54 world/UI screenshot hashes independently verified. Six world views and six phone destination captures reviewed. This is preservation/functional evidence, not new artistic approval or physical iPhone testing.
- Package proof `14139205643`, ZIP SHA256 `2ce39ec2d5d31ea1d3b6472ad6792673ace072a944e9f34affd9ec446aa7efc0`; bundle `6e33438d57aa5265e823d7730640432e0aa22421d71c7a62eb70fbf00bd74413`, delivered data `37b8b8b94d8566da52e695630c592be1e11019c9f394bb0e58d12f5dd49e2b79`.
- Corrected visible-control harness passed on published control in run `43656016744`. Job-level `runner.temp` expression prevented the preceding workflow from starting; move it to step environment. Preserve failed workflow `43655589496` as infrastructure failure, not game failure. Correct artifact upload must retain `world-evidence`, not re-upload native proof alone.
- Actual desktop WebKit comparison `43659627661`, workflow SHA `f03ac3c3083ab3e24d46b19ba58cb26c48a9c155`, passed both published control and new candidate: earned sawmill, nickname and construction reload, three real wheel zooms, two world entries and two verified city returns. Artifact `14140730101`, ZIP SHA256 `6dd689234e9019fa15a0bce177f0b3d9af6191d7b8cdc3c01d5534e3324c3dbb`.

| Observed desktop WebKit metric | Published 90465 | Candidate d0b |
|---|---:|---:|
| First entry observed wait (ms) | 12,085 | 12,845 |
| First entry maximum RAF gap (ms) | 2,687 | 2,561 |
| Peak sampled WebContent RSS (KiB) | 2,122,720 | 2,228,624 |
| Peak sampled GPU RSS (KiB), separate peak | 1,289,024 | 1,362,640 |
| Maximum simultaneous sampled WebContent + GPU RSS (KiB) | 2,534,336 | 2,908,560 |
| Last sampled used WASM after second entry (bytes) | 572,585,504 | 559,410,000 |

These are one desktop comparison, not a controlled physical-iPhone memory benchmark. Do not sum independent peaks or claim total memory savings from the small WASM improvement. Candidate default has **not** demonstrated the intended resident-memory improvement and must not be presented as a solved iPhone crash.

Follow-up: run `43661585648`, source `557fc6b2c50e34b4570855868f6e1ea61b5e81c1`, compares the identical mobile21 binary with default rendering versus the pre-existing opt-in `eldoria_p0_world_msaa_off=1`. No Unity rebuild, art replacement, source UI change or Windows claim. Require the actual opt-in Unity marker, full gameplay success, memory/RAF comparison and screenshot review before deciding whether that variant merits owner publication. Unity documents Camera.allowMSAA as controlling the camera MSAA target and URP performance guidance identifies MSAA attachment bandwidth as a measurable cost: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/camera/allowmsaa and https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/universal-render-pipeline/introduction/configure-for-better-performance . This supports testing the hypothesis, not declaring the iPhone cause.

Rollback preparation independently recovered all 145 currently public files, 961,110,684 bytes, against publication proof SHA256 `f220237c5f3a7b113b4317fed56f5de6d753d31e6d7b190411fc9e63996c113b`; deterministic tar SHA256 `a30724a10bb3aa04d1142a11d0f65dd1ae2c001828eebc574b7c453fd745d14c`. Publication remains untriggered. The current site still serves 90465. Physical-iPhone confirmation, proven root cause and tester release remain pending.

### Same-binary graphics trial and owner-only diagnostic publication

Run `43661585648` passed both default and opt-in world-MSAA-off gameplay. Artifact `14141205035`, SHA256 `bb56c5fe4b66af0ee5c12ae0312713856ff689d929ee3aff40429dda20098bdc`, independently downloaded and verified. Original world-camera opt-in marker present. Actual close/regional/return screenshots reviewed; small edge differences, existing HUD and geometry preserved; not a new formal artistic approval.

Observed default / opt-in: first-entry wait 15,635 / 12,013 ms; first-entry maximum RAF gap 3,067 / 2,162 ms; overview maximum gap 11,586 / 5,384 ms. However, maximum simultaneously sampled WebContent+GPU RSS was 2,590,528 / 2,876,928 KiB. This single paired desktop trial supports investigating frame-stall reduction; **it does not establish memory reduction or the iPhone crash cause**. No blanket graphics-quality default was changed. The selected owner URL explicitly enables the existing world-camera flag.

Owner-only diagnostic21 was deployed by `43663397708`, publication workflow commit `99e994cbd92eb912eec5fcf4645547b718875cf9`; job `132657443155` succeeded. All 145 public candidate/protected files verified against SHA256 and size after deployment, total staged bytes 961,114,883. Public identity independently read as exact d0b runtime and bundle6e33438d. Stable beta18 and city diagnostic routes preserved. Publication proof artifact `14141038195`, ZIP SHA256 `13dd6f3a478868d9ca72257db142186fad8ce5406629c2cbd263f08e1d4eb46f`, independently verified.

Durable complete rollback: draft release `445734680`, tag `archive-pages-before-world-p0-d0b21fd`, asset `749410777`, 961,239,040 bytes, SHA256 `a30724a10bb3aa04d1142a11d0f65dd1ae2c001828eebc574b7c453fd745d14c`. Original public bytes were recovered and checked before replacement; downloaded release bytes compared exactly before deployment.

Publication failures retained: `43663163061` did not start because YAML 1.1 serialization turned unquoted `on` into boolean `true`; always preserve the trigger as a string key. `43663220032` passed restoration/staging but stopped before deployment on release creation HTTP403 when targeting an unmerged workflow-bearing branch commit. Anchor the rollback tag to the authorized main publication commit (`GITHUB_SHA`), while recording the archived runtime's own identity and hashes. Successful retry preserves all prior gates; do not skip durable rollback.

Public player job `132658576862` is still running at this checkpoint. The exact owner link has been supplied in conversation:
https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-integrated-vigilia/?eldoria_p0_lazy_world=1&eldoria_p0_world_msaa_off=1&eldoria_diag=1&build=world21

Final physical-device gate remains open. Do not describe this publication as a confirmed iPhone fix or enable testers without actual device feedback. Windows remains released.

### Final public-origin verification — 2026-11-15

Publication run `43663397708` completed **SUCCESS**, including public player job `132658576862`. Public evidence artifact `14141976559`, ZIP SHA256 `63a4d61f07c95cd14b903b1648ee1748742e188ec76e723054db2d4431a63de6`, independently downloaded and verified. Both startup/residency and actual world reports pass with zero recorded browser errors. Exact d0b source/bundle6e33438d verified in public identity. Nickname and earned construction survive real reload; three wheel views visibly change; two world entries and two city returns expose the saved level-1 sawmill. World MSAA-off marker present. Public reload, world and returned-sawmill screenshots reviewed. Existing story-title truncation and overlapping world labels at wider zoom remain known visual issues; no new artistic approval.

**Remaining genuine external gate:** execute the supplied beta21 owner link in the owner's physical iPhone Safari and report whether world entry still closes/reloads the page. No physical device result has been received in this session, and the desktop comparisons did not reproduce that closure or establish a resident-memory reduction. Do not assert root-cause confirmation or resolved iPhone stability. No external tester release. Source workstream remains the existing PR194; Windows released, no native jobs left running and no supervision automation created.
