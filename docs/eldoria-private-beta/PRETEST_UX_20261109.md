# PR179 — owner's pretester corrections

> **CURRENT CANDIDATE 2026-11-09 (OWNER BUG REPORT):** Mobile10 live remains the
> prior certified build and still has the fixed-right construction countdown.
> The new **world-anchored, compact construction timer** is implemented in
> PR179's frozen overlay and PR183's canonical Unity source, with identical
> Git blob SHA `caa48b2bfa70edde209055f53296ddfc5e3a0baf` and frozen beta
> overlay SHA256 `27c76ecbd5ad0137e8351d737eb6b32493f95e73ad56b4da10628d1d3e963982`.
> `PretestUXProbe.cs` now checks camera pan and off-screen hiding rather than
> falsely accepting a constant HUD position. Ubuntu overlay contract and pinned
> source audit passed; **Unity exact new source, phone captures, updated WebGL
> and owner-site publication are still pending**. Windows is currently leased
> to PR181, not PR179. Do not reuse the earlier mobile10 native run to approve
> this changed source.

Status: **MOBILE10 PUBLISHED — NATIVE, BROWSER AND LIVE HOSTED ENTRY/RELOAD PASS**.
The owner site now serves mobile10. The original mobile9 package and Sites version6 remain available for rollback.

User requested twelve pretester corrections after confirming beta9 was playable and initially stable.
These changes target that frozen runtime. They do not replace PR173's new interface, PR181's treasures work, their canonical source files, or their resource ownership.

## Implementation

| Requested correction | Candidate implementation | Required native evidence |
|---|---|---|
| Premature HUD controls | Existing bottom dock now shows available World/City and functional Mail; no Units/Build/Chronicles placeholders | Initial and progressed saves, both phone orientations |
| Vigilants early/unexplained | Eligible after Engendro or Bastion III, displayed only after per-save introduction | Introduction precedes shortcut; reload preserves acknowledgement |
| Vigilants artwork moves | Removed time-driven scenic-plane displacement and fallback drift | Plane positions unchanged over two seconds plus screenshots |
| Mail on HUD | Existing real mailbox via envelope in bottom dock; removes duplicate floating entry; empty mailbox can open | Actual raycast and mailbox screen |
| Levels drift with camera | Final projection after camera/layout; parent-local coordinates do not assume pivot | Target-to-label screen error under two pixels after pan |
| Premature training controls/Lyra | Hide locked tier, unavailable Heroes and untaught March; contextual March explanation | Real initial training controls and progression |
| Barracks opens training directly | Built barracks opens building sheet with Upgrade and Train choices | Real button hits, no auto-open |
| World/City and extra buttons | Same world button toggles title, icon and destination; remove Units | City → world → city |
| Unclear missions/blank close | Visible Ir/Misiones captions and geometry-based close X | Checklist expands and closes through controls |
| Oversized/moving build timers | Candidate: compact clock anchored to each genuine city building, projected above or below its silhouette; hidden offscreen, no duplicate selected timer. Existing mobile10 still used a fixed HUD dock | New native portrait/landscape captures and real camera-pan tracking + off-camera invariant |
| Scouts unexplained | Bastion III plus confirmed march, per-save introduction before shortcut | Intro and eligibility checks |
| Missing building upgrades | Authoritative level-II sawmill/barracks upgrade path and completion, existing costs/materials/cap preserved | Real upgrade, reload, full I–V regression journey |

## Evidence so far

- Domain/Application and actual HUD binding compile under .NET via PowerShell Add-Type.
- **58 behavioural assertions pass**: displayed level-II offers, costs, pending/completed reload, idempotent command retry, no premature production, building cap I–V, introductions and unlock persistence.
- Overlay transaction test passes: a later foreign input edit prevents every candidate write.
- Changed C# syntax parses including UNITY_EDITOR branches; PowerShell scripts parse.
- None of the above substitutes for Unity type checking, actual visual review, WebGL memory regression or physical owner verification.

## Frozen source packaging

`tools/eldoria-private-beta/native/ux/manifest.json` lists each original source hash and candidate hash. `.cs.txt` files are deliberate isolated-runtime overlays, not canonical source changes. The native runner first installs the already-certified six mobile9 source overlays, verifies every UX input and candidate, then installs all UX sources. The warm wrapper backs up and restores all of those paths and fails closed if it encounters foreign edits. Existing generated-asset audit and original terrain query parity gates remain enforced.

The existing native I–V journey now follows the requested barracks choice and contextual march introduction instead of the removed Units shortcut. Its gameplay assertions remain. An additional editor probe exercises real button raycasts, both phone orientations, target anchoring, compact timers, mailbox, missions and stationary Vigilants artwork. It only seeds an explicitly disposable probe save and checks canonical saves remain unchanged.

Run local logic with:

```
pwsh -NoProfile -File tools/eldoria-private-beta/native/ux/test_domain.ps1 -Source <candidate-source>
python tools/eldoria-private-beta/native/ux/test_overlay.py
```

## Pending delivery

1. Owner explicitly confirmed Windows release on 2026-11-09. The registry handoff to PR179 revision 13 is recorded; native validation is running on the isolated beta checkout.
2. Native compile, complete journey and focused UX probe; inspect screenshots and correct any failures.
3. New WebGL package, paired browser/memory regression against stable beta9, then owner candidate publication only after passing checks.
4. User's real-device confirmation. No tester distribution is authorized by this correction request.

## Native investigation, 2026-11-09

- Run 42586848479 compiled the candidate in Unity and passed terrain/topology/route parity. The journey reached the new barracks choices, recruitment and march introduction. It failed when the test clicked the newly activated narrative in the same frame (graphic depth -1); the probe now yields before performing the real raycast. This is not a completed journey certificate.
- Original source and generated inputs were restored (`restored: true`, no conflicts or input changes).
- Reviewing the actual level overlay found a separate cause from guidance labels: `PlayerClarityRuntime` throttled projection together with expensive state scans to 0.20 seconds. The candidate now caches target/anchor/camera and reprojects on every frame and final Canvas render, preserving the 5 Hz state scan. Native assertions measure actual city and world label positions across consecutive camera pans. This additional fix is pending native verification.

- Run 42588844458 (source 689439dd9bbf3ad6f0df29daca5f60847aa1dcb0): actual complete I–V journey **PASS, 227 checks**. Original asset restoration remains required and was confirmed on downloaded evidence. The focused probe then failed compilation due to incorrect editor API signatures; `SessionState.GetString` now supplies its default and `CanvasRenderer.GetMesh()` returns the owned renderer mesh, which the test reads without destroying. No WebGL was built or published from this run.
- The compact card now applies only to city-building operations. A real native assertion caught lost outbound Engendro text when the compact layout was incorrectly applied to every operation; the corrected journey passed the original multiline viewport requirement.
- The isolated runner now compiles focused verification code during its first native launch, removes it before the canonical journey input certificate, then reinstalls it to run. This catches test API incompatibilities before spending time on the full journey.
- Batch-mode waits use frame yields plus Canvas geometry refresh, not `WaitForEndOfFrame`; Unity documents that the latter is unsupported in Editor batch mode: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/waitforendofframe. Canvas renderer signature: https://docs.unity3d.com/cn/6000.0/ScriptReference/CanvasRenderer.GetMesh.html.

- Focused-probe gesture correction: RoyalButton intentionally accepts clicks only after a matching pointer down/up sequence. The focused helper initially sent only click; it now uses the same real raycast plus down/up/click sequence already validated by the canonical journey. No direct `onClick.Invoke` bypass is used for controls under test.
- Focused UX now runs before the five-minute journey; after it passes its Editor-only source is removed before the journey certificate. Each native run creates its own GUID-named disposable save, so UX fixtures cannot seed the canonical journey. All terrain, UX, journey and WebGL gates remain mandatory.

## Candidate browser preparation

Run 42595057735 (be49d62e3daf490f41dba0bf8123a8e2eee64480) has passed the focused UX gate and advanced to the full journey. Its screenshots still require review; no WebGL delivery is certified at this checkpoint. The mobile10 packaging workflow pins that exact native run and refuses packaging if it fails. It checks the focused completion/source plus the installed overlay manifest, original input restoration, 227 journey checks, native world parity and WebGL build hashes.

The Apple GPU comparison uses stable mobile9 as control. Owner publication preparation requires both browser runs to pass entry/reload, no larger WASM capacity, used-heap growth no greater than 5%, and no resident-peak rise over 15% without further investigation/reversed-order measurement. These are desktop regression bounds, not physical-iPhone certification. The release identity explicitly records changed tutorial/UX and leaves tester distribution and physical-device certification false.

## Verified native delivery candidate

Run [42595057735](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42595057735), exact source `be49d62e3daf490f41dba0bf8123a8e2eee64480`, completed successfully. Evidence artifact `13664816452`; compiled WebGL artifact `13665355023`.

- Focused real-control UX checks passed, including city/world labels over camera pans, mailbox, barracks choices and paid upgrades, fixed timers, missions close geometry, both Vigilants scenic meshes remaining stationary, and untouched canonical saves.
- The complete Bastion I–V journey passed all **227 checks**.
- All 16 focused portrait/landscape captures were inspected. Requested controls are present and early training shortcuts are absent. These are functional/layout checks, not a new premium-art approval.
- Terrain/topology/population/route parity passed. Tape SHA remains `013ebf8194be6974dbcadf27e3cf2ae43db7c7638e0e6cb22cfe838e037ed986`.
- Original isolated inputs restored: `restored=true`, `conflicts=[]`, `existing_input_changes=[]`. Windows reservation released after the successful job.
- Packaging/browser run [42596176998](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42596176998) is still pending at this checkpoint. Stable mobile9 remains live.

## Browser candidate verification

Run42596176998 completed successfully: package and macOS Apple GPU WebKit. Artifact13665617167 includes both control and candidate reports and screenshots. Candidate `after-name.png` and `reloaded.png` visibly retain **Beta QA**, the opening chronicle, and the sole unlocked Mail dock button. No runtime errors recorded.

| Same-host peak | Stable mobile9 | UX mobile10 |
|---|---:|---:|
| Used WASM bytes | 580219616 | 578527168 |
| WASM capacity bytes | 622592000 | 622592000 |
| WebContent RSS KiB | 2648432 | 2730736 |

Single paired desktop comparison: used heap -0.29%, capacity unchanged, resident peak +3.11%, within publication bounds. This is not a physical-iPhone stability certificate.

Candidate package13665750302 SHA256 `8097c6836d743347e5af536db89e3fe181a2547836ad21f91b9e6c8c97c5a62d`. Publication workflow main commit18f889fb85cf2ea2a900975b2d7a4f6958df0a72 started; final live version and hosted check still pending.

## Owner publication

Pages42600463889 succeeded; owner shell artifact13664913737. Existing Sites project retained its public audience and URL; version7 source `b24504b886d127a3f1bf58be17d33245c5922389`, deployment `appgdep_6af19fad4c94819191c2f297403a1286` succeeded at2026-11-09T10:26:02Z.

[Owner beta10](https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=native325-mobile10-pretest-ux). Hosted WebKit recheck42600894847 pending at this checkpoint. No physical device certificate, APK update, tester invitation or PR merge is claimed.

## Final hosted verification

[42600894847](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42600894847) **SUCCESS** against the actual owner Sites URL. Artifact13665603942 SHA256 `0d4d62f29ebd8b7db984a95a856117c2b372a973b98502baaf370fd4b4100357`. Exact mobile10 identity verified, early entry, 90-second initial observation, name entry, reload and 45-second post-reload observation passed without runtime errors. Downloaded `reloaded.png` visibly retains **Beta QA** and the opening chronicle; Mail is the only initial dock destination.

Implementation and web owner delivery are complete. User physical-device confirmation remains; this work does not claim an APK update, premium-art approval, external tester release or a merge of the frozen beta overlays into canonical main. Earlier pending entries above are historical checkpoints.
