# Camino de los Vigilantes B6 — sistema, no fotografía (2026-11-03)

## Decisión vigente del propietario
- B5 se rechaza artísticamente: el paisaje continúa pareciendo fotografía pegada, hay falta de profundidad en el recorrido y el Engendro sobra dentro del paisaje. El enemigo **puede y debe permanecer en tarjeta** si está iluminado, anclado y sin recuadro fotográfico.
- Trabajo únicamente en la PR #136 / rama canónica `fix/player-feedback-context-ui-20261030`. Base exacta de referencia B5: `3d4786bc8d3707f7b67679be5234512639cb59cc`, Unity native run `41648354560` (SUCCESS). No fusionar PR #137 a ciegas: `RoyalGoldV3LayeredFinish` ya está en #136.
- El objetivo ≥8,3/10 es subjetivo. No aprobar automáticamente por Unity PASS.

## Implementación candidata B6
1. **Retorno del Engendro a la tarjeta:** la misma ilustración aprobada `engendro-reference-v3`, con su alpha e identidad, deja de habitar el mundo. Se reparenta bajo `Progression and expedition information`; conserva el RawImage original, el mismo `uvRect` y luz/borde de contacto, con raycast apagado. Su soporte comparte iluminación violeta/ocre con los cuatro guardianes; cero retratos rectangulares.
2. **Figuras reales dentro de la carta:** los cuatro PNG alfa creados para B5 permanecen distintos. Se elimina el arco oscuro y rígido que simulaba una fotografía enmarcada; se reemplaza por luz ambiental graduada y plataforma de piedra orgánica. `RoyalGoldV3GuardianReliefGraphic` sirve tanto a guardián como al Engendro. Ninguna imagen del paisaje aparece en la tarjeta.
3. **Diseño de dos columnas en tarjeta:** figura derecha; nombre, características, tropa, recompensa y experiencia a la izquierda, sin solapamientos con `Combatir` ni `Formar marcha`. Tarjeta horizontal > B5, paisaje panorámico sigue siendo protagonista; en vertical se conservan los dos bloques y controles reales.
4. **Camino como sistema de diorama:** `RoyalGoldV3VigilantsWorldLight` ilumina exclusivamente las antorchas que ya existen en el paisaje aprobado, la puesta de sol, la niebla de valle y la posición real de la siguiente etapa de los 25 hitos. Controlada por la persistencia `LastClearedStage`, no suma hitos, recompensas ni botones. Cinco regeneraciones de malla por segundo, cero textura extra, cámara o EventSystem. La iluminación está por debajo de ruta y primer plano real de vegetación/rocas.
5. **Preservación:** nombres y cinco guardianes 5/10/15/20/25, recompensas, `EarlyRetentionRules`, `LocalGateway`, reportes, XP, partidas, Canvas, HUD y configuración de marcha idénticos. PR136 DRAFT, no merge `main`, publicación o créditos nuevos.

## Prueba de integración y aceptación
- Comparar capturas Unity B5 **SHA 3d4786bc** vs B6 en las mismas resoluciones y estados, al menos etapa 1, guardianes 5/10/15/20/25, victoria y derrota; paisaje horizontal, vertical y móvil 390×844; stage15 también 844×390.
- Evaluar visualmente: **cero imagen fotográfica duplicada en tarjeta**, no Engendro sobre el camino, enemigo presente/no diminuto, profundidad/luces del sistema, 25 hitos y texto/button safe area. Un resultado funcional PASS no equivale a +8,3 artístico.
- EditMode: `RoyalGoldV3VigilantsDepthTests` incorporan world light/raycast y estado de hito, asentamiento del Engendro sin segundo retrato. `EarlyRetentionNativeCapture` exige el mismo `engendro-reference-v3` y el padre de la tarjeta, raycast OFF, y la luz real del hito15 sin crear otra escena. Ejecutar Unity de SHA exacto: EditMode, PlayMode, retención, marcha y V4.1. Inspeccionar imágenes.
- Rendimiento: el sistema de luz usa quads/radiales UI sin textura o cámara; comparar frame-time/GC con el origen en dispositivo cuando sea posible; no afirmar FPS móvil medido solo a partir de código.

## Evidencias
B5: [UI Progression Certification 41648354560](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41648354560). La futura B6 registrará run/sha exactos y su veredicto artístico aquí o en comentario PR136; no confundir documentación con A/B aprobado.
