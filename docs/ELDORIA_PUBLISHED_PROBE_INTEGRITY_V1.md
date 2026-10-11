# Published probe integrity v1

Scope: acceptance correctness of the existing published macroloop probe only.
Owner authorization: 2026-10-08 independent audit follow-up — begin evidence-backed corrections.

## Verified defect

Run 37738951889 / artifact 11532469844 reported `checks.playablePass=true` while its logs included three engine errors: `The file 'level0' is corrupted! ... [Position out of bounds!]`. Its final state was Bastion I, sawmill 0, revision 4, wood 590, stone 150, gatheredWood 360, gatheredStone 0. It did not complete Bastion I-II. The navigation helper searched a horizontal band rather than requiring the logged named button position.

## Correction

- Reject console errors, page errors, dialogs and runtime failure markers; warnings remain distinct. No silent engine-error allowlist.
- Tap the logged named navigation control once, verify bounds and require the expected transition. No navigation-band search.
- Compare every logged persistent field exactly after reload: revision, wood, stone, gatheredWood, gatheredStone, sawmill, Bastion and march phase.
- Replace the ambiguous `playablePass` success field with `navigationGatherPersistencePass`. Explicitly declare unexecuted building/progression/combat/reset/portrait-interaction/device/human-quality coverage.
- Retain screenshot pixel statistics as content-presence checks; they do not certify visual or artistic quality.

The QA contract unit tests include the previously green engine-error regression, every persisted-field mismatch, offscreen navigation targets and forbidden broad certification claims. They do not substitute for the published probe or a human playthrough.

## Dependencies that remain open

`valoria-bastion-i-ii-presentation-parity-correction-v1` retains ownership of Unity presentation, game-side logging, Windows Unity builds, Pages publication and project-state closeout. This QA stream does not modify or take over those surfaces.

The correction may legitimately turn the current published probe red. That means the existing build failed stricter acceptance, not that this workstream repaired its engine/presentation defects. The owning production stream must diagnose the underlying engine errors, restore exact visible control alignment and provide a full published Bastion I-II test with state and matched orientation evidence.

Full remaining acceptance: fresh start → sawmill build → forest/quarry → scout → Bastion II → barracks → recruitment → configured march → Engendro victory/reward → Valoria return → exact reload → deliberate reset/reload. Compare web/Unity HUD at matched viewport and state. Distinguish native SHARP rendering from WebGL certified-frame presentation. Do not rename macroloop success into full gameplay or visual-quality PASS.

No gameplay, economy, art authority, visual assets, camera, saves, publication payload or paid credits are changed by this correction.

## Executed validation and closeout — 2026-10-08

Implementation commits: 78ea9b228b0cb3a6d17f92bf2924eca3502336e2 and 57369a0c4a5be91be3e261e722ad69ddab50e6f4.

- Contract tests: 5/5 PASS locally and in GitHub Actions.
- YAML parsing, embedded JavaScript syntax and diff whitespace checks: PASS locally.
- Final implementation architecture, workflow governance and quarantine checks: SUCCESS (runs 37741999838, 37741999918, 37741999718).
- Published probe: run 37741999794, job 113194609825, **FAIL**. It rejects three observed Unity `level0` corruption / position-out-of-bounds errors at certification. Earlier corrected run 37741874742 also rejected these errors.

QA acceptance-integrity implementation is complete. Published-game acceptance remains FAILED; full Bastion I-II, visual quality and independent human playthrough remain unverified. No underlying engine or presentation repair is claimed. The active presentation owner and its exclusive build/publication resources remain untouched.
