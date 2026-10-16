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

## 2026-10-16 — Independent mesh-identity check of 63-base vs later PBR tower family

Fetched actual original-63 artifact `12010106460` (run `38813966671`) and successor PBR artifact `12033234180` (run `38871632375`), inspected GLBs directly with trimesh/NumPy on decoded world-space geometry (not rendered pixel assumptions):

| Module | Faces base/PBR | Vertices base/PBR | Face indices | Maximum vertex-position delta |
|---|---:|---:|---|---:|
| `tower_square_short` | 25,164/25,164 | 50,328/50,328 | identical | 7.58e-8 m |
| `wall_panel_crenellated_4m` | 4,968/4,968 | 9,936/9,936 | identical | 1.49e-9 m |
| `merlon_tall` | 324/324 | 648/648 | identical | 3.42e-10 m |

**Result:** base and later PBR candidates have matching geometry to floating-point precision; the artistically relevant contrast is chiefly their materials and rendering, not size or assembly sockets. The base-63 tower and merlon previews show distinctly **lighter limestone**; later PBR previews show darker weathered stone. Neither wins on timestamp alone. An isolated SAME-CAMERA Unity swap can decide which palette integrates better with Hero V3 and the approved Valoria visual target, without changing gameplay/collision geometry.

Exact alternative base-63 hashes: tower `a514d97fc3241477f4b98b7e4d04b1213d75a1718e53d662fca9e83be8dfd4ac`; crenellated wall `476d4cd3bb9d220845489833b3386311c1994b6db55ab3353571cf564a6dd05f`; tall merlon `022bf407bcfeeb7e717874b0a27def4e3f9831bb0c6edfb1933bc99f4f447bb9`. This is a geometry/evidence statement, **not** artistic approval.

## 2026-10-16 — Exact grounded buttress Unity proof and artistic result

- CI run [38940426182](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38940426182), source `5044e8fba927740eae35f227661f74f88e14f1dc`, technical Unity gate and postflight feedback **SUCCESS**, artifact `12062799612`. Landscape A/B/C source camera metadata is exactly equal (1440 × 1000, camera position (-32,49,76), orthographicSize 26); mobile and portrait files also delivered. Candidate remains unpublished; `art_approved=false`.
- Exact saved scene recipe: full-volume masonry bays ×2 (V6), separate supported merlons ×6, full-volume outboard short tower ×1, buttress supports ×2. Notes `BUILT_CONNECTED_BUTTRESS_GROUND_VERIFIED_BAY_0/1`; historical wing renderers disabled = 8; historic colliders found/preserved in this wing = 0. Zero cannot certify runtime movement or interactive gate logic, and no PlayMode runtime QA was executed in this workflow.
- **ART REJECTED (relative to Valoria approved 10/10 reference):** tower remains a featureless square brick volume next to more detailed existing defended entrance; new stone chroma and scale differ from the nearby historical portal, and sparse merlon/buttress rhythm does not restore the original parapet's richer silhouette. Contact physics PASS is meaningful, but it does not make the architecture visually superior to the original.
- **Actionable individual asset defect:** `tower_square_short`: improve true architectural silhouette and details (credible openings, stone variation, proper corner bonds, parapet relief) *in artist-owned library*, not by placing a decorative old tower underneath. Preserve the independently verified outboard joint and terrain foundation when revising the tower; source module versions must be reselected by actual same-camera A/B.
- Follow-on *isolated structural alternative* committed at `2b40c5c35fb9922a157666bdf3fb0af7b450b06e`: complete `wall_panel_crenellated_4m` GLB instead of overlapping separately added merlons. That trial must be independently rendered and judged before art promotion.

