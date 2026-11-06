# Player experience remediation — 6 November 2026

Workstream-ID: valoria-master-plan-layered-construction-20261016

Owner: codex-player-experience-fixes-23287cf1c126. PR #163, branch `fix/player-experience-audit-20261106`.

## Acceptance source

The completed manual Neral I–V playtest is documented in `PLAYER_AUDIT_20261106.md`. Its observations, not successful compilation, define acceptance. Preserve automatic combat, real resources/timers, existing saves and the single canonical HUD. Do not modify ConstructionDelight or Spawnling work owned by PRs #152/#151.

## Current state (not a completion certificate)

Candidate `833e3b6970080de0cbf2aec29b51f8cb7fb0a088` is under native Unity validation, run 42276531574. Domain probe dependency corrected in `72ad6be0621fcc287c397d8098dea55a39cd43ca`. The subsequent domain run exposed missing Barracks wording in the shortened tutorial; corrected locally while retaining the existing test. Further changes below remain candidates until the exact final commit passes native and player validation.

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

## Evidence still required

- Exact final domain regressions and Unity EditMode/PlayMode.
- Genuine native images of modified screens in both mobile orientations.
- Exact candidate WebGL build and a player-directed repeat of the initial journey. No seeded states or accelerated timers may count as that player session.
- Integration only after these checks; keep any remaining limitations explicit. Physical Android/touch/audio performance remain a separate device test.
