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

## Hallazgos de validación y técnica reusable — 5/11/2026

- **Falso PASS detectado**: el primer test visual solo comprobaba flags de progreso y un selector global `.aldric-cinematic .aldric-dialogue.done`. Podía hacer click sobre una escena distinta de la visible, acumulando 18 avances con el mismo narrador mientras los flags se completaban. No equivale a lectura visual correcta.
- **Corrección del test**: esperar `.aldric-dialogue.done` dentro del diálogo visible, verificar la retirada del mismo elemento de DOM tras Continuar, exigir aparición real de Sir Aldric y guardar capturas con el texto totalmente revelado. Esta técnica evita confundir éxito de estado con éxito visual.
- **Bug real descubierto por el test estricto**: `tutorialBeat()` se ejecutaba en el primer `render()`, antes de que `opening()` y `aldricIntro()` concluyeran; varias escenas podían quedar superpuestas. Los tutoriales iniciales del Aserradero y Mundo se condicionan ahora a `s.introSeen`. El ledger se escribe únicamente después de finalizar cada lectura.
- **Evidencia estática y de dominio**: 9 tests nuevos Node aprobados, compilaciones Ubuntu de dominios existentes y guardas de arquitectura/workflows aprobados. 
- **Evidencia de navegador**: primera partida normal pasa con control del diálogo visible en el HEAD `ab7f618168525f067ebb7ad638d8aa2252734596`; recorrido I–X del mismo HEAD en verificación al registrar esta nota. Un recorrido I–X anterior pasó en `a6e760e41abc848416c2dbf112a3ce0ef32a45b3`, con 17 checkpoints.
- **Límite**: este control detecta orden y ausencia de fallos automáticos; no mide por sí solo ritmo subjetivo ni legibilidad certificada en un dispositivo físico. Ni equivale al Unity Editor PASS.
- **Bloqueo de integración**: la PR #160 es draft; su comprobación de Workstream-ID falla por titularidades vigentes sobre GameState, RoyalHUDV1, ValoriaReferenceClean y el prototipo web de la fisura. Reconciliar con #134 y propietarios antes de promover nada. Ningún otro propietario ni main fue modificado.
