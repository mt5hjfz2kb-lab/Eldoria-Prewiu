# Bastión I — revisión artística real de la sustitución modular de LowerGateRightWing (2026-10-16)

**Decisión: ART_REJECTED para esta composición, no para la biblioteca. No publicar ni promocionar la candidata.**

Evidencia nativa Unity recuperada de Actions `38914488098` (SUCCESS técnico), artefacto `12052488046` / `bastion1-modular63-unity-review-38914488098`. Contiene `before-landscape.png`, `after-landscape.png` y `lookdev-landscape.png` más capturas móvil/retrato y metadatos de cámara. Los JSON de cámara A/B/C landscape coinciden exactamente en posición, rotación, tamaño ortográfico y proyección; 1440×1000. Comparación humana directa, no aprobación basada en pixel diff.

**Fallo visual localizado y reproducible:**
- La torre modular cuadrada se ubica en el borde de la muralla que mira **hacia la puerta antigua**, no en el extremo alejado; su volumen tapa parcialmente el torreón antiguo de la entrada.
- La nueva sección de muro a la derecha presenta remate bajo y rectilíneo, frente al parapeto almenado y más legible del original. La silueta global **empeora**.
- La prueba sigue usando la portada histórica Hero V3 y NO acredita todavía una entrada hecha de kit.
- El JSON de la prueba indica `HISTORICAL_WING_RENDERERS_DISABLED_8` y `FUNCTIONAL_COLLIDERS_PRESERVED_0`: la preservación de cero colliders no constituye certificación de transitabilidad ni jugabilidad.

**Hipótesis geométrica específica (requiere nueva captura para confirmar corrección):**
`Bastion1Modular63SceneReview.RebuildGateWingWithTower()` ancla siempre la torre en `aggregate.max.x`, sin determinar qué extremo de la sección mira hacia la puerta. En LowerGateRightWing el borde elegido apunta hacia la puerta central. Para una nueva tentativa, escoger exterior/inferior X según `gateBounds.center.x` vs `oldBounds.center.x`; exigir separación real de la puerta histórica, contacto válido con terreno y empalme estructural del muro. Evitar tapar o duplicar la vieja torre Hero V3. Verificar con nuevo A/B en Unity, nunca inferir el resultado por código.

**Criterio de aceptación:** ambas caras del muro con volumen creíble, almenas/rebates coherentes, torre conectada sin invadir el torreón de la puerta, apoyo sobre terreno, elementos funcionales verificados. Si un módulo no sirve, conservar la geometría visual anterior mientras se trabaja una sustitución mejor. Los otros 61 módulos no se declaran revisados por esta prueba.

No alterar `main`, otros pilotos, GLBs del artista ni publicar gameplay a partir de esta evidencia.

## 2026-10-16 — Exact Unity A/B for outboard correction (run 38924698349)

- Source `58089c800764ad6a9f45e1ab649b447da47e17f6`, Windows Unity Actions run `38924698349` succeeded, artifact `12056707431`. Before/after/lookdev landscape (1440×1000), mobile (780×520), portrait (390×844) exist. Same-camera files verified. This proves actual engine composition, not published gameplay.
- New recorded socket is `OUTBOARD_TOWER_SOCKET_MIN_X`, with two full-depth masonry bays, eight historical wing renderers disabled, no colliders in this wing (zero retained); imported source digests match selected V6 wall `928630408d47a1789a2d0e5080e40348979dc8338f4cc91ea8a3a64ba93257f3` and tower `73b99c2c8ec5a5f2c1a81467b3217acaf11e3e8e28d3ffa66315f572e60bf5f1`.
- **STRUCTURAL ADVANCE**: tower no longer obscures the entrance's historic tower. The masonry bay meets the tower footprint instead of using a front-surface decal.
- **ART FAIL, deliberately not promoted**: wall parapet is visibly flat/too low and new dark tower is stylistically disconnected from older gate. None of the 8/10 target, final game playability, or mobile performance is approved by these captures. Next experiment: true separate merlons on the top of the new wall, not a coincident wall face. Trial source `de6f94ed` had a cheap static assertion error (`len(unique)==7` with eight sources), fixed in `cc15c8bd` without using Windows. Validate the exact new run before judging.

## 2026-10-16 — Merlon-on-new-wall structural proof (run 38927157638)

- Windows Unity Actions `38927157638`, source `cc15c8bd6bf075e22af9189045a5a44e97e3dcbc`, result **TECH SUCCESS**, exact artefact `12056799562`. Saved A/B/C native frames landscape/mobile/portrait and isolated candidate scene; original preserved.
- Full-volume masonry bays 2; separate individually grounded crenellation merlons **6**, full-volume outboard tower 1; active material+GLB provenance verified: V6 stone wall `928630408d47a1789a2d0e5080e40348979dc8338f4cc91ea8a3a64ba93257f3`, square short tower `73b99c2c8ec5a5f2c1a81467b3217acaf11e3e8e28d3ffa66315f572e60bf5f1`, merlon `96541f0fac4ca8b4856c3b897ff2552ebec5560b2251df9b33b875efd06f7820`.
- Actual images inspected: merlons correctly rise from top of **new wall**, not front-cladding the old wall; external tower no longer overlaps Hero V3 entrance. **ART_FAIL remains** because the tower appears disproportionately tall, dark and bare versus the gate; original lower-right curtain has richer detailing. Scene-wide still far below 8/10 Valoria reference. Structural proof ≠ global art pass.
- Follow-up current branch source `7f11977097a66596ab7932488d5fd63230e0bc04`: uniform tower reduction to 0.86 and isolated C-only warm PBR grading without changing A/B source; Unity run `38929353461` pending exact visual inspection. Keep original entrance; no publication while art rejected. No changes to 63-asset artist and no paid credits.
