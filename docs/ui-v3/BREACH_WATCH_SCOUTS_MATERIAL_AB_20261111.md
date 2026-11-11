# Vigilia / Exploradores: prueba de gradación de material — 11 noviembre 2026

**Estado:** experimento artístico en la PR #173, pendiente de evaluación nativa comparativa. **No aprobado para main ni publicación.**

## Diagnóstico visual de captura Unity comprobada (SHA ca587094)

Se inspeccionó la imagen nativa `real-unity-scouts-390x844.png` procedente de Actions [43078432537](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43078432537), artifact `breachwatch-first-native-ca587094f0fd9405c72f97ce848d6d774e6ddde4-43078432537`. La ilustración central de Exploradores conserva arte pictórico autorizado, pero su brillo amarillo/cian compite con la jerarquía piedra oscura/burdeos/dorado de Vigilia. El problema principal no es la nitidez ni la posición de los botones, por lo que mover pequeños márgenes no lo solucionaría.

## Hipótesis técnica elegida

Mantener **el mismo RawImage del mundo pintado original**, sin una fotografía añadida, sin nueva capa sobrepuesta, sin cambios de mapas, cartas, soldados, misiones, temporizadores, guardados ni recompensas. En `BreachWatchRetentionTheme.Apply(root,true)` convertir exclusivamente el material de Exploradores a `(0.78, 0.77, 0.95, alpha original)`, que reduce verdes/amarillos y acerca los fondos a un crepúsculo de Brecha. El adapter distingue el destino real mediante `RoyalGoldV3EarlyRetentionScreens.Destination.Scouts`. `Restore()` restaura el color previo cuando se cierra la pantalla.

Se ha contrastado una **simulación raster local solo como hipótesis visual**, no como prueba Unity, con el recorte de paisaje original. El efecto es visible, menos saturado, y conserva topología y contenido del mapa. Aceptarlo exige observar el render de Unity en tamaño real, especialmente textos sobre imagen, miniaturas de contratos y bosque panorámico. No usar la simulación como evidencia de QA.

## Condiciones de decisión

- Comparar con la captura **nativa exacta** de la baseline SHA `ca587094f0fd9405c72f97ce848d6d774e6ddde4`, a `390x844`, `900x1600`, `844x390` y `1600x900`. No evaluar solamente diferencias respecto a otra variante.
- Rechazar si la imagen queda sin profundidad, se pierden los pueblos/cursos de agua, se reducen demasiado la legibilidad o el contraste de marcadores, o se introduce un rectángulo/color que parece una lámina superpuesta.
- El test `BreachWatchDestinationNativeEvidence` debe certificar que la **misma** RawImage original tiene el color correcto en Exploradores y vuelve exactamente a blanco tras salir. Las ocho capturas de Vigilantes/Exploradores, la interfaz inicial, las demás pantallas, los mensajes y las pruebas de guardado deben seguir pasando.
- El estilo de los pergaminos y miniaturas no cambia; si el desajuste global persiste tras una primera iteración útil y otra comparación, **cambiar de técnica**: composición/pintura por planos o arte fuente propio, no sucesivos tintes mínimos.

Este registro no infiere un 8/10 ni suplanta la aprobación del responsable. La comparación contra la referencia original sigue siendo necesaria. También faltan pruebas físicas de rendimiento y estabilidad en iPhone/Android. No hay permiso para publicar ni fusionar la PR.
