# Eldoria r7 — QA gates

This file is the acceptance checklist for consolidation. A box is checked only after it was actually verified.

## Structural
- [ ] Consolidated source contains no runtime dependency on `v0195-r4`, `v0195-r5`, `v0195-r6` or `r7patch.js`.
- [ ] One active build/schema identifier.
- [ ] One active save key.
- [ ] Fresh reset clears all active progression state.
- [ ] Critical actions have stable test identifiers.

## Canonical E2E
- [ ] Prologue.
- [ ] Chapter I reconstruction: Granary → Sawmill → Quarry → Forge → Barracks.
- [ ] Chapter I ceremonial closure/reward.
- [ ] Chapter II starts.
- [ ] First semi-automatic combat.
- [ ] Rift choice.
- [ ] First card claimed.
- [ ] Aldric contextual tutorial.
- [ ] Hero Hall reached from tutorial.
- [ ] Global-world introduction is mandatory.
- [ ] Safe Forest completed.
- [ ] Contested Quarry completed.
- [ ] Global Rift Zone decision completed.
- [ ] Short global-world assessment completed.
- [ ] Return to Valoria.
- [ ] Bastion becomes available only now.
- [ ] Second expedition.
- [ ] Remaining Chapter II progression.
- [ ] Relic collection/selection.
- [ ] Mandatory first Duel with Orin.
- [ ] Tutorial demonstrates N/E/S/W.
- [ ] Tutorial requires selecting a relic.
- [ ] Tutorial demonstrates center +1.
- [ ] Tutorial shows an Orin move.
- [ ] Tutorial explains opposed values through the board state.
- [ ] Tutorial demonstrates capture.
- [ ] Board becomes free only after tutorial sequence.
- [ ] Duel reward/final.
- [ ] Chapter II closes only here.
- [ ] Chapter III `Mar de Cristal` appears as teaser/locked.

## Mobile
- [ ] 390–430px width critical path checked.
- [ ] Modal close/continue actions reachable.
- [ ] No tutorial overlay hides its required target.
- [ ] Mission panel clearly identifies current global-world objective.
- [ ] Combat actions remain reachable with safe-area inset.
- [ ] Duel board/tutorial fits or scrolls without losing the required action.

## Truthfulness
- [ ] Static QA recorded separately from executable QA.
- [ ] No E2E claim unless a full run was actually executed.
- [ ] No publication claim unless GitHub Pages/root was actually changed and checked.
