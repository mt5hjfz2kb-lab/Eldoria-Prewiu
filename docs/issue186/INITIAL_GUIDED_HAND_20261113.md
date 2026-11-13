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

## Exact-first-player Unity gate now authored (not yet run)

The existing real-scene `Issue186BagCombinedNativeEvidence.Capture` now checks the first saved onboarding objective `b1.build-sawmill`, verifies the new coach was installed on the **canonical** RoyalHUD, allows its actual MonoBehaviour Update/EventSystem to tick on a 390×844 render target and requires `Visible=true`, `TargetId=sawmill` plus the internal physics/first-hit verification. It captures the genuine first-sawmill frame or a failure frame with the specific `Rejection` reason **before** injecting advanced-state rewards into the same disposable, restorable OS-temp probe save. The rest of its four-size bag, mail, no-double-credit tests remain unchanged. Source CI protects the presence of this acceptance assertion.

This deliberately remains **UNTESTED IN UNITY** until PR179 releases the exclusive Windows runner and a source-pinned native workflow exercises this exact branch. A source CI SUCCESS or presence of `firstSawmillHandOnRealHit` in a report schema is not an observed true result. No launch or merge is authorized on that basis. Full iPhone/Safari world stability remains a separate open gate.

## Recovery of the actual first-use native acceptance lane (13 Nov, PR #190)

The prior `issue186-native.yml` was still triggered exclusively from the retired `feature/issue186-reward-attention-guidance-20261111` branch and expected the pre-coach **20-frame** report. That workflow could not certify this new hand, regardless of a green source test. The canonical workstream was reassigned to `chatgpt-eldoria-first-use-qa-recovery-20261113`, revision 56, without taking PR179's active Windows lease.

The same existing native workflow has been repaired on this branch (no new competing PR or runner): `REQUEST_FIRST_GUIDED_HAND_NATIVE` selects its **exact current source SHA** and disables the obsolete external Vigilia overlay; it requires `productionVigiliaInstalled`, `firstUseCoachPresent`, `firstSawmillHandOnRealHit`, 21 genuine screenshot hashes, the original bag/mail/save checks and 52 actual UI first-hit clicks. The Editor fixture explicitly enables `ELDORIA_NEW_UI_ONLY=1` and fails if the production-only Vigilia presentation is absent. Only the release-request file can trigger the new native run. Source/metadata/ownership checks on the latest commit are separate from real native certification.

**Still not tested:** the native hand capture, first physical iPhone screenshot/taps, first nickname and story hand placement (the existing bag fixture directly completes that setup), and the Barracks Training pointer on the updated PR179 source. Do not merge or publish PR190 until PR179's exclusive native run has exited, its exact source is promoted, the same-source first-hand native proof passes and actual captures are inspected. Do not infer physical Safari stability from any desktop fixture.

## Additional first-entry acceptance prepared — awaiting real Unity

The production-only first-entry fixture now attempts the opening journey **before** auto-completing the story: (1) pointer with genuine first-hit over the untouched nickname InputField, (2) pointer with genuine first-hit over the confirmation Button after typing Prueba Valoria, followed by a real Button pointer click that must save the canonical player name, and (3) pointer with genuine first-hit over the opening narrative Continue Button. It captures three additional 390×844 GPU frames, then checks the previously authored first-sawmill pointer before saved bag/mail and reload tests. The native workflow requires 24 screenshot hashes, 53 actual UI first-hit clicks and explicit PASS booleans for all four guide targets. Source CI protects this expanded contract.

**Evidence boundary:** these are newly committed test assertions, not observed Unity success, physical iPhone stability, visual approval or tester readiness. A native failure requires examining the reported pointer rejection reason and actual screenshot and fixing the user-visible interaction without weakening the first-hit contract. The existing beta WebGL compilation 43489752868 still owns exclusive Windows resources.

## Primera ejecución real fallida y corrección con evidencia

Ejecución nativa [43500945544](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43500945544), código fb311f3257: la interfaz Vigilia de producción arrancó y las capturas 390×844 acreditaron la mano sobre el campo del nombre, su confirmación y el botón real de continuar la historia. El botón real de confirmación guardó Prueba Valoria. La ejecución terminó en fallo verificable al buscar el primer Aserradero: el diagnóstico del componente fue no-real-physical-hit:sawmill; no se ejecutaron las fases posteriores de la Bolsa. Artefacto de 1,64 MB [14074455904](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43500945544/artifacts/14074455904), descargado e inspeccionado: cuatro capturas genuinas y report.json, 3 indicadores de entrada true y firstSawmillHandOnRealHit false.

La causa encontrada en código de producción: ValoriaJourneyGuidance crea el BuildingLabel real Guidance sawmill, cuya instancia RoyalButton.Select ya tiene el listener host.Select(id). Es la señal visible de Construir aquí: Aserradero en la pantalla, montada sobre el Canvas de Vigilia. El coach nuevo, en cambio, exigía una coordenada libre sobre el colisionador 3D y la señal visible lo tapaba. El jugador debe poder tocar la señal, no un píxel invisible de suelo. La corrección PR190 apunta primero al botón real de esa señal, conserva la verificación EventSystem de primer toque, y solo conserva el picker físico para los objetivos sin señal UI. No se inventa otro botón ni comando, ni se mueve la cámara.

Prueba ampliada pendiente de Unity: localizar el RoyalButton real, enviar la secuencia pointerDown/pointerUp/pointerClick (un OnClick sintético no sirve porque RoyalButton la rechaza), demostrar que selecciona el Aserradero en la sesión real y que la mano pasa al botón auténtico Construir; 25 capturas, 54 toques comprobados y exactitud de la Bolsa/correo/guardado. Además, las capturas del primer nombre y la historia mostraron que la placa redundante de instrucción tapaba parcialmente la pregunta/texto del relato. El coach ahora omite esa placa en los controles que ya se explican a sí mismos, y el test rechaza su presencia. Son cambios implementados, NO validados aún en Unity ni iPhone.
