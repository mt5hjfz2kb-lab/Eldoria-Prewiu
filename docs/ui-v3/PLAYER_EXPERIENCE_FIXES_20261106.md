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
