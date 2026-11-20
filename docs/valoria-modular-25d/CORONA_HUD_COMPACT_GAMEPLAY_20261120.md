# Corona de la Brecha — HUD comercial compacto, objetivo y cierre al construir

Fecha: 20/11/2026. Rama aislada `experiment/valoria-corona-de-la-brecha-ui-20261120`, sin cambios en `main`, Unity Scene, `Modular25DWorld`, `Modular25DGameplay`, reglas, persistencia o costes.

## Causa y respuesta
La evidencia de navegador auténtico 390×844 [run 44808347374](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44808347374) mostraba la vieja organización con tres botones en toda la franja inferior, recursos con etiquetas pequeñas sobre metal y un objetivo flotante en el margen superior. El jugador pidió un HUD de estrategia móvil comercial más compacto y que **el panel desaparezca inmediatamente al pulsar CONSTRUIR**.

Implementado en `Modular25DInterface`:
- El listener del botón llama `gateway.BuildSawmill()` como antes; si, y solo si, `CommandResult.Ok` es verdadero llama `ClosePanels()`. La construcción y guardado reales se mantienen intactos. Si falla, el panel continúa abierto con el feedback.
- Los tres recursos se leen por icono original pintado + número grande, sin rótulos minúsculos superpuestos a la gema. Se conserva el orden madera/piedra/comida.
- En retrato: sello de identidad arriba a la izquierda, recursos compactos debajo; misión con ilustración abajo a la izquierda; OBRAS como principal medallón abajo a la derecha; CIUDAD pequeño al lado y ESTADO encima. Los tres ya no ocupan toda la anchura inferior.
- En horizontal: sello/recursos a extremos de la cabecera, misión bajo el sello y un único grupo de tres medallones en la esquina inferior derecha, dejando el mundo central visible.
- Al abrir Aserradero o Estado se oculta el grupo de medallones para no registrar pulsaciones bajo el panel. Al cerrar/terminar satisfactoriamente se restaura.
- No se añaden funciones falsas de mundo, héroes, cartas o alianzas.

## Validación implementada
- Test Unity EditMode→PlayMode del Bastión real: los tres iconos de recursos están presentes, misión no invade medallones en retrato, el comando Build descuenta 80 de madera una sola vez, **cierra** panel automáticamente, devuelve navegación y permite reabrir el panel para consultar tiempo real sin doble pago.
- Prueba Chromium: toca el OBRAS en nueva posición, pulsa Construir y exige que desaparezca el panel por diferencia real de píxeles antes/después; comprueba cambio del saldo 230→150, recarga en el mismo navegador y gestos de cámara con el nuevo botón CIUDAD.
- No aprobar nivel comercial por compilar. Pendiente nuevo render auténtico 390×844, 1170×2532, 844×390 y prueba iPhone físico. Los fotogramas antiguos y las simulaciones no prueban el diseño nuevo.
