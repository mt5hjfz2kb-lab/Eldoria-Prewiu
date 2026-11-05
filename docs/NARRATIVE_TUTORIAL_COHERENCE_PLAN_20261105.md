# Eldoria — cierre de coherencia narrativa y tutorial (2026-11-05)

**Base:** main 6dca872164acc05d1bca217125829b7b671d99b8. **Branch:** fix/narrative-tutorial-coherence-20261105.
**Scope:** timing and copy of narrative, tutorial acknowledgement, first-use teaching and static QA. No combat resolver, art assets, scenes, paid tools or publication.

## Gates
1. **G0 preflight:** preserve the Bastion I-X and Fissure contracts; isolate branch from PR134 (combat) and PR152 (construction). No Windows runner claims.
2. **G1 Unity I-V:** canonical story order derived from saved state; no Lyra spoilers before meeting; Fissure vs Breach terminology; once-only persisted acknowledgements; tests in existing EditMode suite.
3. **G2 Web I-X:** do not mark story/tutorial seen until the user finishes; serialize dialogues and avoid concurrent intro windows; teach Relicario in stages, only mark completed after first choice; remove early spoilers.
4. **G3 cross-runtime QA:** targeted Node guard + existing test suites; add checks for skip/reload, spoiler stages, once-only; review exact diff.
5. **G4 Unity native:** complete EditMode/PlayMode on the exact candidate; mobile screen and real player journey I-V; full end-to-end I-X in the web reference. This requires a free runner and cannot be certified by source inspection.
6. **G5 integrate:** only after independent checks and conflict-free comparison with latest main and the combat workstream. No publication without explicit authorization.

## Acceptance
- No dialogue displays a character's undiscovered identity or claims an event before its state is earned.
- Ordering survives multiple progress steps between render frames and reloading.
- Read-state recorded only after final acknowledgement, not at scheduling time.
- Bastion VII explains Códice/Relicario without dumping rarity/directions and requires an actual use/conserve decision.
- Bastion IX/X narrative graduation must eventually be backed by a real valid combat challenge, not automatic victory.
- Functional checks are not artistic/rhythm certification.

## Known deferred cross-workstream work
- PR134 activates the approved combat after independent reconciliation; only then can Bastion III/X combat outcomes be re-evaluated honestly.
- Full native mobile A/B and normal-clock human playtest cannot be claimed from these source changes.
- An early optional Herald appears at level V in the legacy web prototype. Resolve whether it is explicitly a lesser world boss before moving or renaming this live activity; do not silently remove player content.
