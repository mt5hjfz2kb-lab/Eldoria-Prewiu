La panorámica anterior recortaba la región y dependía de un atlas que se deformaba al intentar encajarlo. Este candidato reconstruye costa, ríos, lagos, rutas, bosques y relieve como capas geográficas explícitas, conservando la escena integrada y sus destinos reales.

## Resultado y alcance
- Costa de 76 puntos, 9 trazados fluviales, 2 lagos, 15 rutas y 36 grupos forestales; malla continua con costa facetada y vegetación de transición.
- Brecha en su anclaje real; proporciones del mapa corregidas en coordenadas vectoriales, sin comprimir las imágenes.
- Marcadores híbridos 2.5D de ciudad, cantera, ruina y madera. Se ocultan los donantes visuales antiguos y se conserva el Spawnling existente, sin nuevos hotspots, colisionadores ni campos de guardado.
- Comparación real antes/después: panorámica y regional × horizontal y vertical. Los cuatro pares mantienen exactamente posición, rotación, destino, zoom y formato de cámara.
- El ensayo P0 del otro chat se terminó en Unity y se rechazó por contaminación agua/bosque, trazados duplicados y desaparición de la Brecha. También se descartaron compresión de atlas y suelo de camuflaje procedural. Evidencia y límites: `docs/evidence/world4x-canon-20261022/REFERENCE_COMPOSITION_AND_NODES.md`.

## Validación
Código verificado: `038c9f513a415c4dab282f632302a50c8d930241`. [Unity nativo 39874832548](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39874832548): **7/7 PASS**, 26,6555606 s; [artefacto 12468311395](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39874832548/artifacts/12468311395), 12 PNG. ZIP completo y CRC verificados; SHA256 `1a8d65c35f4f6aafa9de1fea366d7562aedb42b5d8e422e9c6899587ee012db5`. El último commit solo añade evidencia y el generador de la comparación; no cambia ese código.

**Sin aprobación artística.** Hay un avance claro de composición y continuidad geográfica, pero el agua y el terreno del control conservan parte de su riqueza pintada. La reconstrucción no demuestra todavía paridad con las referencias. La edición guiada de pigmento falló en el servicio; no se utilizó una API de pago ni se fingió un resultado.

## Gates para integración y publicación
- [x] Compilación, gate de fuente y 7 pruebas PlayMode del alcance World4X sobre la escena integrada.
- [x] 12 capturas reales Unity; cuatro comparaciones A/B con cámara idéntica y originales inmutables disponibles.
- [ ] Revisión artística independiente frente a los originales y aprobación expresa del propietario.
- [ ] Regresión completa I–II: nodos, marchas, combate, guardados, entrada/salida y UI V3.
- [ ] WebGL, perfilado y estabilidad de iPhone físico de #96.
- [ ] Autorización de integración/publicación.

Mismo workstream `eldoria-world-region-1-canon-20261022`, propietario `chatgpt-world4x-hybrid-resume-82901015`, revisión de posesión 11; alcance limitado al adaptador World4XCanon, pruebas, workflow y evidencia autorizados. Se mantiene borrador: sin merge, publicación, gastos externos, Qwen ni afirmaciones de validación móvil. #95/#96 conservan sus gates independientes.
