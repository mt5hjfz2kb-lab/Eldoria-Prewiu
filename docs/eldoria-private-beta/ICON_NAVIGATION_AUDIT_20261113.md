# Icon navigation and world-entry audit — 2026-11-13

Owner contract: icon-only navigation. A bag opens saved inventory; a castle returns to city; mail uses an envelope. Never erase the owner's existing save to reproduce a fresh player.

## Routes reviewed

| Control | Expected destination | Finding / correction |
| --- | --- | --- |
| Footer bag | Actual saved packet inventory | Previously selected sawmill. Opens `Issue186BagOverlay` through its canonical `OpenOnCanvas` API; closes building context; cancels pending world entry. |
| Footer castle | City, without building selection | Previously selected Bastion. Clears selection; calls city transition even when world is still loading. |
| Footer raven / return castle | World / city | Uses canonical world gate and deferred load. Pending request can be cancelled. World shows one return castle, avoiding two identical controls. |
| Footer swords | March preparation | Canonical narrative/progression gate remains. Cancels pending world entry before opening. |
| Footer helm | Heroes | Canonical heroine recruitment gate remains. Cancels pending world entry before opening. |
| Mail envelope | Introduced canonical mailbox | Bell replaced. Large mailbox painting was too small at control scale: compact engraved envelope reuses Vigilia's own drawing. Generic legacy icon17 is an envelope and must not be replaced with raven artwork. |
| Resource capsules | Saved resource packets | Preserve actual resource/packet counts; cancel pending world entry. Inventory selection highlight clears after close. |
| Built barracks swords | Actual training screen | New UI lacked a visible entry although direct-service native tests passed. Add icon-only contextual entry, keep independent improvement action, and test the real button through the first raycast receiver. |
| Building hammer | Canonical build/upgrade offer | Existing native blocked-action feedback and resource/save checks retained. No navigation control submits construction. |
| Chest | Actual daily treasure screen | Existing first-hit opening/closing and real saved reward tests retained. |
| Close / information / expand | Close destination / details / expand | Existing real-control tests retained; no replacement of callbacks or save authority. |
| Settings / Breach placeholder seals | No implemented destination | Remain absent; no inert controls implying working screens. |

## Regressions found in validation itself

- Browser construction QA previously clicked the bag to select sawmill. Corrected to click the visible construction-site control and the built sawmill itself. Run43451150406 passed against public a2e956.
- Training was opened directly through a bridge in native QA, so the test could pass while the player's entry was absent. Replaced by visible contextual-button validation and real first-hit click.
- `BreachWatchAdvancedNavigationEvidence.actualMailboxFirstHitClicks=0` is written before the later mail exercise. The full destination report, not the early snapshot, records actual mail clicks.
- Direct native callbacks/raycast tests do not replace physical Safari touch/release testing across frames.

## World restart: evidence and boundary

The owner reports a physical iPhone restart on world entry. Public a2e956 completed two world entries and two city returns in desktop WebKit run43449196470, with no browser errors. This does not disprove the owner's report. Sampled desktop WebContent RSS reached about2050MiB and GPU process RSS548MiB; these are separate process observations, not a measured iOS kill threshold.

Candidate0577b6 releases genuinely unused assets at the first deferred-world boundary before allocating Region1. It retains all live references, the scene, gateway, saves, gameplay geometry and population. Opt-in memory logs compare native/managed totals and top texture/mesh allocations before/after release and after construction. This is a measured mitigation candidate, not an asserted crash fix. Full native load/cancellation/reentry/save tests and actual WebKit construction/world/reentry screenshots must pass before delivery.

Unity6000.3 references: [Web memory](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/memory), [Resources lifetime](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/resources). Native object memory is not reclaimed merely because managed temporaries can be collected; unused-assets release is separate. No shared mesh CPU data is discarded.

## Verification status at authoring

- bf1f35c: native run43449861957 passed actual bag/city routes, saves and destination/reward checks; not published.
- 0577b6: grouped native/browser run43454132345 in progress; no result claimed in this document.
- Additional generic-envelope, contextual-training and closed-bag highlight changes: implemented, awaiting the next exact candidate gate.
- Physical iPhone acceptance and tester release remain pending. No owner save reset and no tester invitations.
