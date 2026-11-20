# Umbral II — diagnóstico de HUD móvil y contraste verificable

**Fecha:** 20-11-2026. **Ámbito:** únicamente `experiment/valoria-modular25d-ui-umbral-20261120`, sin cambios en la beta o `main`.

## Observación reproducida, NO supuesto

La ejecución nativa 44783636135 del SHA `3c6cc0a2caeade6cf75066e0089dac68b935cf6f` completó Unity 6000.3.23f1, 47 fotogramas auténticos, PlayMode y WebGL aislado. En Chromium real (390×844), los tres cartuchos de recursos contenían iconos pero **no se veían los números ni los nombres**. El navegador sí accionó el botón OBRAS, abrió el panel y ejecutó la orden real. Falló el gate visual con `Build did not change the real wood balance glyphs: 0.000`, no por un fallo en LocalGateway.

**Capturas de referencia:** artifact `umbral2-mod25d-chromium-webgl-ui-44783636135` en la [ejecución](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44783636135). Contrastar `390x844-real-world-hud.png`, `390x844-real-sawmill-build-panel.png` y `390x844-real-build-command.png` con el PNG de la nueva ejecución.

## Hipótesis técnica aplicada

Los `Text` de los contadores tenían cajas verticales de 14 y 20 px con `VerticalWrapMode.Truncate` y font SourceSans3. Las métricas reales de línea pueden exceder esa altura, eliminando la línea completa en el render WebGL. Se aumenta la caja vertical a 24/32 px, se admite overflow vertical sólo en los dos `Text` de cada chip y se permite reducción limitada de tamaño para cantidades largas. Se corrige igualmente la línea secundaria del sello y se ensancha la placa clicable del Aserradero. La implementación no añade botones inventados, edificios futuros ni capas que oculten el mapa.

La hipótesis **no queda demostrada sólo porque el código compile**. Los tests Unity comprueban que los contadores existen, se alimentan del snapshot y actualizan el saldo tras una acción real. El contrato browser mantiene la comparación de píxeles de glifos **antes/después del gasto 230→150**, exige recarga y comprobación de persistencia, y produce capturas reales. Si el gate sigue rojo, inspeccionar dimensiones efectivas, `cachedTextGenerator`, máscaras y fuente cargada en PlayMode, corregir la causa y repetir.

## Puerta de aceptación

- Unity: importar arte original, capturar escena real y ejecutar test del `LocalGateway`, Aserradero y HUD, sin degradar rutas, cámara ni guardado.
- Chromium 390×844 **y** 844×390: recursos numéricos legibles, valor de madera actualizado en pantalla después de pulsar el comando real, balance persistido al recargar, sin recortes de HUD ni fallos de consola.
- Revisión visual humana: el Bastión y Aserradero dominan; hay suficiente espacio futuro intencionado; los cartuchos no tapan las entradas; la apariencia de todo el HUD es comercial a escala móvil.
- Safari físico, memoria/FPS y aceptación artística final permanecen **pendientes** aunque Chromium y Unity pasen.
