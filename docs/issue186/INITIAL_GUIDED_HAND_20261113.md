# First-use hand after nickname — implementation record (13 Nov 2026)

## Owner-observed defect
The published Vigilia owner build did not show the approved pointing hand while teaching the first city/building interaction. The actual main integration had `Issue186ArtKit.BindFirstUsePointer` attached only to the first packet `USAR` flow inside `Issue186BagOverlay`. It never pointed at the first sawmill target or the new real construction button. Possession of two PNG assets alone was not tutorial integration.

## Work implemented on this isolated branch
- Install `Issue186FirstUseCoach` on the existing canonical `RoyalHUD` during the same `Issue186BagOverlay.Initialize` call, without another Canvas or EventSystem.
- Reuse the real native PNG hand and halo in the already active independent Vigilia Canvas; all guide graphics are `raycastTarget=false`.
- Derive the active step from `SliceRules.CurrentObjectiveKey` and the real saved PlayerState; show the hand on the **real nickname InputField / confirmation Button** and on the **real first narrative Continue/Reveal Button** where pointer raycast permits; after the saved nickname and finished opening narrative, the first world step is `b1.build-sawmill`.
- Point at the actual physical game hotspot only when `ValoriaIntegratedWorld.Pick` returns that exact target for the proposed screen coordinate AND the EventSystem reveals no GUI blocker.
- After real selection, point to the active, first-hit `BreachWatchHud.BuildingAction`, or to the canonical Training/World/Army navigation buttons as the authoritative objective requires.
- Additional first-use steps mapped: forest, quarry, route, city return, Bastion ascent, barracks/building/training, March, Engendro and later granary/stoneworks. They remain hidden if not yet unlocked/visible.
- The hand is not clickable, does not perform actions or write state. Actual objective completion (or saved tutorial dismissal) changes what is shown; reloading a completed objective does not restore an old prompt.
- A short readable native-font guidance plaque follows the pointer without becoming an input overlay.
- Explicit diagnostics: `Visible`, `TargetId`, `ObjectiveKey`, `Rejection`.

## Evidence boundary
**Implemented in source, not yet certified in a real player build.** The current test addition checks Roslyn syntax, real component wiring, no second Canvas/command writer and actual target verification code. This is not equivalent to Unity's true physics/raycast or phone screenshot validation.

Before any merge/beta promotion, execute genuine Unity scenes on 390×844, 844×390, 900×1600 and 1600×900 with a fresh named player and a saved returning player. Verify at least: (1) genuine hand on real nickname input, then its confirm button after typing, (2) genuine pointer on story's active Continue/Reveal button, (3) one hand on a really selectable sawmill after the first story, (4) first scene tap selects that sawmill, (5) hand moves to genuine CONSTRUIR, (6) successful construction changes the real objective and hides/moves hand, (7) no false hand under unrelated overlays, (8) no HUD overlap/clipping, (9) reload persistence and bag first-use pointer unaffected. Inspect actual GPU frames and Safari/Android controls separately.

The beta workstream #179 still owns the exclusive Windows runner and exact source/publishing branch. This change is intentionally isolated and does not publish, mutate a save, interrupt Unity, or invite testers.
