# PR179 — owner's pretester corrections

Status: **IMPLEMENTED CANDIDATE — NOT NATIVE VERIFIED OR PUBLISHED**.
The stable owner-tested mobile9 site remains unchanged.

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
| Oversized/moving build timers | Selected operation sheet 112 logical units; unselected compact fixed HUD timers; no duplicate selected timer | Phone captures, live timer updates, camera-pan position invariant |
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
