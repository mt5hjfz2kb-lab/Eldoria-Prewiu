# Camino de los Vigilantes — B9: paisaje materializado sobre el recorrido real

## Criterio del propietario
Eliminar la sensación de fotografía pegada. Conservar el Engendro de la Brecha en la tarjeta y sus ilustraciones auténticas, conservar los otros cuatro guardianes candidatos con raycasts y texto correctos, mejorar la profundidad/iluminación y convertir los 25 hitos en un sistema físico; no modificar ninguna mecánica.

## Diagnóstico comprobado
- A: PR136 `111b5661dc5453260907a608e041b259217b2073`, [Unity run 41663824030](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41663824030) PASS.
- B8: `a1395202562d6419d330bcd6cec43a8edd146e94`, [Unity run 41667940691](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41667940691), snapshot available in artifact `eldoria-retention-evolution-native-41667940691` (ID `13245739005`). Los 25 cimientos de piedra ya proceden de la geometría jugable, pero el primer A/B es artísticamente insuficiente: para guardián 5 horizontal 1600×900 la diferencia global media es **0,136/255** (0,27% de píxeles difieren >12); sobre el área del paisaje la diferencia es 0,367/255. El material del fondo sigue demasiado parecido a una fotografía.
- B9 cambia de técnica, no agranda otro overlay: **reemplaza el QUAD del propio RawImage existente** por `RoyalGoldV3VigilantsLivingLandscape : RawImage`, malla con tratamiento por zonas de material y luces dinámicamente alineadas al mismo recorrido, no un panel adicional. Mantiene `texture`, `uvRect`, nombre y la identidad de ilustración, de modo que la validación existente de recursos sigue siendo aplicable.

## Diseño B9
- Delante/atrás: cielo y cordillera distantes con atmósfera más fría; fortaleza y río con sombra de distancia; primer plano de vegetación/piedra con más contraste, oclusión y peso.
- Camino: `SetLivingScene(active, actualTrail, LastClearedStage)` proyecta las 25 posiciones del mismo `RoyalGoldV3StageTrailGraphic`; un tratamiento visual de superficie calentada por puesta de sol separa la coronación de piedra de la sombra en los laterales. La progresión real controla el avance luminoso, nunca recompensas ni estado de juego.
- Conserva `RoyalGoldV3VigilantsRoadFoundation` de B8 para la geometría de contacto; mantiene `RoyalGoldV3VigilantsWorldLight` existente para antorchas/luz de hito, sin cámara ni Canvas extra.
- No hay doble foto: es la misma `RawImage` con la misma textura y la misma cobertura UV, ahora dividida en 28×24 parches colorizados por vértice (sin textura, shader o nueva asset paga). Es estática salvo cambio de etapa/relación de aspecto, en vez de hacer `SetVerticesDirty` cada frame.
- Exploradores conservan el tratamiento original: `SetLivingScene(false,...)` delega a `RawImage.OnPopulateMesh`.

## Cierre y límites de evidencia
Compilar y capturar B9 exacto en Unity 6000.3.23f1: guardianes 5/10/15/20/25, etapa normal, victoria, derrota, 390×844, 900×1600 y 1600×900. Verificar 25 marcadores, raycasts, guardado, marcha, recompensas reales y EditMode/PlayMode; medir impacto visual del paisaje versus B7. No confundir un sombreado visible con la prueba de que el objetivo ≥8,3/10 se haya alcanzado; no integrar main ni publicar ni cambiar cálculo del combate.

Decisión artística al cierre: si B9 todavía se ve como una foto detrás de UI, dejar `ART_APPROVED=false` y conservar evidencia; una reconstrucción auténtica de assets por múltiples máscaras de profundidad y texturas separadas será un encargo artístico adicional, no afirmar que una malla de vértices pueda sustituirla.
