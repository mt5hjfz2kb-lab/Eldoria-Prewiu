# Camino de los Vigilantes — profundidad artística aislada (2026-11-03)

## Objeto y autoridad

- Objetivo del propietario: elevar visualmente una pantalla integrada 7,2/10 hacia ≥8,3/10, sin destruir la escena aprobada. No declarar puntuación ni `ART_APPROVED` antes de ver el A/B nativo en Unity.
- Base jugable para este trabajo: PR #136, HEAD inicial `178d12ca8e6467c00b78ac96a1a86bd7511e4df8`.
- PR #137 (`68cc0508a47facd99e79a4e8ac566a05ad7b4717`) sigue DRAFT y diverge; su primer `RoyalGoldV3LayeredFinish` ya fue incluido en la historia de PR #136 (ancestro `674455bf`). NO se ha realizado una fusión ciega ni se usa PR #137 como base de producción.
- Experimento trazable `art/vigilants-depth-candidate-20261103`. Integrar mediante PR canónica #136, solamente después de reconciliar HEAD exacto. No abrir una PR paralela.

## A auténtico / diagnóstico, no mockup

- Unity nativo histórico: run [41219845900](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41219845900) sobre SHA `89474863d7e1a85778774cf05240b411b391e115`.
- Artefacto `eldoria-retention-evolution-native-41219845900` (ID `13049753097`) con capturas reales: capítulo 1, cinco guardianes (5/10/15/20/25), victoria, derrota; paisaje y móvil 390×844, además 900×1600 / 1600×900 según caso.
- Déficits constatados en el A: hitos circulares como fichas superpuestas al paisaje; escenario pictórico potente pero con poca separación de masas; retrato ordinario parece la misma fotografía de paisaje recortada, Engendro 15 recortado sobre zona oscura sin suelo ni rebotado de luz.
- Estas imágenes son evidencia PREVIA, no una certificación del SHA candidato.

## B — intervención acotada en implementación real

1. `RoyalGoldV3VigilantsParallax`: cuatro contornos transparentes con UV y textura de la ilustración **original**; cresta/fortaleza, acantilado intermedio y márgenes de vegetación cercanos con distinta velocidad. Control 5Hz, sin duplicar PNG o `Canvas`. Se dibuja delante del original y detrás de neblina, hitos y controles.
2. `RoyalGoldV3EncounterGrounding`: dos planos del **enemigo auténtico** (trasero: rebote + sombra de contacto con el suelo; frontal: oclusión, borde pintado y bruma). Modalidad Engendro/guardián contextual, sin cambiar `Texture2D`, recorte o id de enemigo.
3. `RoyalGoldV3StageTrailGraphic`: las mismas 25 posiciones proyectadas sobre el camino aprobado, con sombra bajo la moneda, bisel iluminado y línea con relieve; los estados despejado/actual/guardián salen del mismo estado real.
4. `RoyalGoldV3EarlyRetentionScreens`: conecta los nuevos componentes con la textura y `RawImage.uvRect` existentes, y los desactiva fuera del Camino. Preserva texto, botones, navegación, `EarlyRetentionRules`, `LocalGateway`, recompensas y partida.

## A/B y gate para integrar — obligatorio

- Capturar con Unity en la candidata exacta `B`: etapa normal 1, guardianes 5/10/15/20/25 (al menos 1600×900, 900×1600 y 390×844 para cada uno); victoria y derrota también en móvil y panorámico. Mantener la identidad y la posición de los 25 hitos y banners.
- Comparar con el artefacto de `A` en mismos estados: diferencia inequívoca de profundidad y apoyo de criatura, sin duplicaciones, recortes, deformación de fortaleza ni árboles flotantes. Si la diferencia es invisible o empeora el cuadro, RECHAZAR B y revertir los archivos de presentación.
- Raycasts: nuevos `Graphic.raycastTarget=false`; comprobar `GraphicRaycaster` sobre Volver, Formar marcha, Combatir y experiencia en vertical/horizontal.
- Guardados: antes/después, victoria/derrota, progresión 1→25 y recompensas correctas; verificar no cambio en `EarlyRetentionRules`, contrato de combate, ni reseteos. No crear correos, etapas ni tropas falsas.
- Rendimiento: comparar medidas frame-time/GC/UI geometry en el mismo runner/editor y, cuando corresponda, Android físico. El mesh de paralaje se actualiza como máximo 5 veces/s; los planos del enemigo son estáticos. **No confundir este presupuesto teórico con FPS medido.**
- EditMode: `RoyalGoldV3VigilantsDepthTests` y batería existente; PlayMode: retención, UX, marcha, guardado y pruebas generales. Repetir en SHA exacto tras cualquier corrección.
- No fusionar #137 ni #136 a `main`, no publicar APK/WebGL ni afirmar ≥8,3/10 sin inspección real de B.

## Estado

Candidato de código en desarrollo; fuente y prueba EditMode preparadas. La prueba visual definitiva depende de ejecutar Unity con reserva Windows canónica. El `A` no sustituye el `B`.
