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
