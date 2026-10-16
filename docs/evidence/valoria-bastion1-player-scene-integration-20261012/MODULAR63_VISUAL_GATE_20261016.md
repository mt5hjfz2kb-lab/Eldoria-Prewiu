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
