# Player experience remediation — 6 November 2026

Workstream-ID: valoria-master-plan-layered-construction-20261016

Owner: codex-player-experience-fixes-23287cf1c126. PR #163, branch `fix/player-experience-audit-20261106`.

## Acceptance source

The completed manual Neral I–V playtest is documented in `PLAYER_AUDIT_20261106.md`. Its observations, not successful compilation, define acceptance. Preserve automatic combat, real resources/timers, existing saves and the single canonical HUD. Do not modify ConstructionDelight or Spawnling work owned by PRs #152/#151.

## Current state (not a completion certificate)

The candidate remains unmerged and unpublished. All domain/governance checks passed on `1b83c3c099896e0533c529ec9c00b1acfd1fc096`; its WebGL build is run 42279373482. Earlier native run 42276531574 compiled and captured the integrated journey, then exposed two outdated text assertions, now corrected. Run 42278064290 exposed an interaction the probe did not yet handle: the earned Lyra presentation correctly blocks the next world action until closed. The probe now captures that presentation in both phone orientations and clicks its real close control. Review of actual captures additionally found and corrected a clipped T2 requirement and a reading cue overlapping skill text. Native303 includes those corrections; final native tests and manual player replay are still required.

| Observation | Candidate correction | Required verification |
|---|---|---|
| Valid Granary / Lyra actions rejected | Revalidate remembered UI terms against current gateway state; preserve original command identity and duplicate guard | Passive production, competing build, duplicate payment, actual Lyra assault |
| Horizontal report inaccessible | Preserve content offset on periodic layout; compact short-view heading and reading hint | Scroll real saved report in both orientations across refresh |
| Terrain disappears after repeated focus | Convert orthographic focus displacement to ground-plane movement without changing projected location or altitude | Repeated selections and rotation, whole I–V session |
| Tiny HUD / clipped counters | Phone-specific canvas reference, wider counter, readable action/body type | 844×390 and 390×844 screenshots and visible controls |
| Invalid prepared march | Display actual availability instead of power zero; adjustment CTA opens canonical march preparation | Reserve troops for scouts, adjust and fight normally |
| Unexplained disabled controls | T2 barracks requirement; Lyra support/Heroes requirement; unavailable third hero/reliquary | Before and after unlocking Lyra/T2 |
| Empty back control | Supported `<` character | Both destinations and orientations |
| Level overlapping timer | Physical separation between upper facade label and roof timer | Active Bastion upgrades, portrait |
| Confusing world targets | Clear chapter-III labels, include boar/meat in render-boundary visibility, avoid occupied resource sites, real encounter thumbnail | Fissure, meat and hunts before/after reload; no route regression |
| Skills reading | Give actual skill space instead of compact locked reserves; smaller paragraph type and reading hint | Full source-bound text, wheel/touch drag, no clipping |
| Generic mailbox | Real Vigilant stage/name; explicit source and already received quantities on new march receipts; player-facing archive wording | Old save compatibility, correct quantities, no duplicate claims |
| Uninformed decisions | Current→next real production; priority rewards on choices | Correct prices/benefits before click; readable full text |
| Stale status / duration | Expiring feedback; action-specific duration; consistent minutes:seconds | Training, investigation, assault, completed action |
| Unclear available ending | Clear completed journey and honest unavailable VI, with existing follow-up activities | Bastion V 4/4 |
| Waiting and milestones | Parallel-activity guidance; Lyra's actual profile after acknowledged recruitment; chapter-completion banner and earned mastery change | No popup loop on reload; distinct chapter closure; real progression only |
| Portrait inconsistency | Reuse approved Aldric dialogue bust; request the same Lyra model portrait for dialogue/cards | Same identity, no missing/cropped portrait |

## Follow-up after real WebGL replay (native313)

- The first native313 candidate passed 216 EditMode, 77 PlayMode and 222 integration checks. Its exact WebGL was built before the latest follow-up corrections.
- An additional browser-directed regression session used the actual native313 WebGL, unmodified save and normal timers. It reached Bastion III, constructed the granary on the first click, hunted both animals, started the 20-minute fissure investigation, opened reports, and switched between 844×390 and 390×844. The independent first-play I–V audit is preserved separately; this follow-up did not complete a second I–V run.
- The new replay found an **unresolved in native313** horizontal world target cartouche whose description and rewards were clipped. The PR now includes a larger landscape reading viewport and a PlayMode guard, but those changes require an updated capture.
- The replay also found **hunt reports showing 0 rounds, 0% life and zero recorded loot** despite the reward being delivered. This was corrected by distinguishing hunting receipts from combat reports and directing the player to the genuine Recompensas entry. A new PlayMode regression covers that behavior.
- The native313 replay and screenshots are historical evidence only; do not present them as proof of the post-native313 commits. Keep the PR draft and public releases unchanged until the exact latest head passes certification and visual checks.

## Evidence still required

- Exact final domain regressions and Unity EditMode/PlayMode.
- Genuine native images of modified screens in both mobile orientations.
- Exact candidate WebGL build and a player-directed repeat of the initial journey. No seeded states or accelerated timers may count as that player session.
- Integration only after these checks; keep any remaining limitations explicit. Physical Android/touch/audio performance remain a separate device test.

## Follow-up native315 visual framing (2026-11-07)

Native315 run 42307493841 failed a genuine pre-WebGL framing assertion after the Engendro march departed. On 844×390, the target occupied x=.46–.54, y=.40–.56, but the free rectangle reserved a horizontal full-width band for a chapter objective shown only at the top LEFT, leaving y=.55–.58 between it and the bottom context cartouche. The source of the false full-width exclusion was `RoyalGoldV2Hud.GetFreeSceneViewport`. Native316 keeps the legible context card intact and reserves the measured **left-hand horizontal footprint** of that quest for real World targets, leaving the right-hand world visible; left-side city building protection remains unchanged. The already-present outgoing Engendro assertion and both phone captures must pass. The compact native artifact now explicitly contains `integration-context-engendro-*.png` so that a Unity PASS never substitutes for visual inspection. No game mechanics, saves, timers, camera altitude or rewards change. Native316, browser evidence and exact final UI certificate are NOT yet PASS.

## Native316 visual findings and native317 corrective candidate (2026-11-07)

Native316 source `934110ccda820a3683bd6e1bb28bc19bf7df838e` built successfully in run 42311997693 (`INTEGRATION_PASS_ART_REVIEW_REQUIRED`), but exact-head shared UI certification run 42312005830 failed its forest context comparison: old V2 30.2% versus new V3 28.8% unobstructed landscape scene. Native316's original prebattle Engendro phone captures were covered by an unacknowledged Bastion-II dialogue and the portrait outbound body showed clipped explanation text. These are genuine presentation/evidence defects; the WebGL compilation PASS is not a player quality verdict.

The native317 candidate reuses the empty encounter cost row for portrait status and raises the description viewport to at least 60 logical units, avoids duplicating safety padding already applied by Focus on the side-by-side world region, and requires acknowledging the earned dialogue using the existing real button before evidence is captured. The native portrait capture explicitly guards multi-line viewport height. No combat, economy, save or timer rule changes. Require an exact native build, full UI certification, real-screen review and normal-clock browser replay before integration.

## Native317 full PlayMode compatibility finding and native318 fix (2026-11-07)

Native317 `4fdb95fdea71c05b28a6e2e28147fddcb0719c24` passed the isolated Unity I–V integration and generated genuine mobile prebattle/outbound Engendro images; the reviewer confirmed the description is now readable and the prebattle image shows the correct real card. However the **full** Unity UI Progression Certification run 42315563468 failed exactly two existing PlayMode cases: `ChronicleRealKeepsNativeBuildingIllustrationWithCanonicalOffer` and `SingleHudCompactObjectiveAndNoOverlayAtRest`. Both used an established reflection-based call to the zero-argument `GetFreeSceneViewport` API. Earlier world-context work had replaced that signature with a single optional-bool method; reflection still counts the optional parameter, so the compatibility tests were correct to fail. Native318 restores the exact public zero-argument overload delegating to the unchanged general viewport implementation while preserving the explicit bool world-only overload. This is an API compatibility correction, not a weakening of the tests, nor any modification of real game state or art. Re-run full exact-head native certification and isolated real browser proof before integration.

## Native318 landscape Barracks regression follow-up (7 November 2026)

The exact native318 head `f5346ae2c93c759ca881df066879940c8fc19da5` passed Unity UI Progression Certification [run 42320013476](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42320013476): 216/216 EditMode and 78/78 PlayMode cases. Isolated WebGL built in [run 42320008160](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42320008160). This is technical certification, not browser I–V acceptance.

A real Windows Chromium replay of native317 [run 42317742792](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42317742792) failed after the Bastion-II upgrade, selecting the unbuilt Barracks in landscape 844×390. Saved telemetry measured model bounds `[.613,.429,.750,.569]` against a falsely restricted free scene `[.009,.552,.991,.583]`. The top-left quest and lower context sheet were both excluded as full-width Y bands, leaving only 3.2% free scene height even though the unobstructed scene continued on the right. The browser input, camera trace and original screenshot are retained; no saved game state or clock was bypassed.

Follow-up source changes opt actual non-Bastion city parcels into the pre-existing side-by-side landscape quest exclusion, preserving world, portrait and Bastion contracts. The real native I–V integration now captures the unbuilt Barracks at 844×390 and 390×844; Unity's runtime `IsTargetFramed` guard must succeed before construction. **This follow-up is unverified until exact-head native certification, image review and a normal-clock browser replay pass.** Keep PR #163 DRAFT and do not publish.

## Native319 truthful failure and viewport root (7 November 2026)

The first candidate `c2766b6` **failed** the exact Unity UI Progression Certification [run 42362296272](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42362296272), at the NEW unbuilt Barracks 844×390 geometry assertion. Its PNG was reviewed: the selected plot was still near the lower sheet. A follow-up **diagnostic-only** revision `c532b96` retained the same hard failure and the subsequent [certification run 42362920220](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42362920220) recorded `framed=false`, projected `x=.60 y=.55 w=.14 h=.14`, unoccluded viewport `x=.35 y=.53 w=.64 h=.16`, city zoom `33.71`, landscape aspect `2.164`. This is a genuinely insufficient context viewport, not a test-only numerical error.

A second independent root was verified in source: `RoyalHUD.GetVisibleSceneViewport()` reserves the entire Y band of `Feedback` when its text is nonempty, while `RoyalHUD.Update()` holds feedback for **eight seconds**. The photographed scene showed that transient message after Bastion II. That temporary toast reduced the otherwise-right-hand city scene to 16% height even after the quest exclusion was fixed. The next candidate derives the upper limit for **landscape city parcel selections only** from the *real measured royal header* and excludes the compact quest by its real left X footprint. This avoids camera layout changing when feedback expires; the toast itself is not deleted, hidden, or altered. World, Bastion and portrait branches are unaffected, including the zero-argument API. The native regression now requires at least **25% unoccluded height** and real full-model framing, without replacing the actual plot or cancelling the message. Native320 is still a candidate until it passes a fresh exact-head Unity integration and real-browser replay.
