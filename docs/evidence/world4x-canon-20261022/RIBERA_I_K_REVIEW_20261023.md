# Ribera I → K: mejora conservada y límites artísticos

Se conserva **K** como derivación editable de la versión I elegida por el propietario. No se amplía Región 1. La mejora visible está en las copas y en cuatro interrupciones físicas de la orilla. **No se ha alcanzado todavía el objetivo de 7,5–8/10 y no existe aprobación artística final.**

## Evidencia y decisiones

| Variante | Commit / ejecución nativa | Decisión visual |
|---|---|---|
| I, base del propietario | `14d134cf4c719824a6db6a28761556bebdbbf8c8` / [39953231775](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39953231775) | Intacta; referencia de comparación. |
| J, pigmento de río y márgenes, fuente reparada | `12ee1b5a809c5d8d83cda9a91d9b9b8d292f0def` / [39956977121](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39956977121) | Descartados los pigmentos: charcas demasiado planas y franjas aún reconocibles. Conservadas 14 piedras en cuatro acumulaciones y la reparación del empaquetado. |
| **K, candidata conservada** | `a911bd153713e3aadf7eb7422faa07d91bbf28d8` / [39957867110](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39957867110) | Copas más legibles, variedad en árboles seleccionados y orilla físicamente interrumpida. Recupera las imágenes de agua y margen de I. |
| L, tierra conectada a caminos | `2e40ade33959d1a9d921f521e56b2c814b4b4e05` / [39959264736](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39959264736) | Descartada: ensanchamientos abultados y demasiado dibujados. No mejora suficientemente a K. Fuente y capturas conservadas. |

Los intentos técnicos anteriores al J reparado tampoco se ocultan: `39955534575` pasó 18 pruebas pero mostró el terreno negro; `39956549482` recuperó los píxeles y reabrió Blender, pero falló al generar un manifiesto con referencias RNA caducadas. Véase `SOURCE_REOPEN_RECOVERY.md`.

## Autoría aplicada en Blender

K utiliza la misma geografía, caminos, distribución y escala de I. Sobre copias de la malla original de pino se esculpen sus islas de follaje: caras inferiores más planas, hombros elevados y puntas descendentes, con normales planas. Hay dos familias editables y 22 árboles seleccionados que las utilizan. Se conservan los 156 árboles con sus transformaciones exactas; no se redistribuye el bosque al cambiar el zoom. Los modelos originales permanecen recuperables.

Cuatro perfiles locales de orilla se rebajan en la topología existente. El borde de terreno y el labio del margen se editan conjuntamente, con una transición hacia el interior. Las piedras de J se apoyan visualmente junto a estas interrupciones. Se mantienen el curso del río y los caminos; no hay una segunda geografía matemática en Unity.

La textura final del suelo de I se recuperó exactamente del GLB original y se empaquetó. La igualdad de píxeles fue comprobada. K conserva esa riqueza; L no se adopta. La fuente K fue abierta de nuevo por Blender durante el estudio L, demostrando su reutilización real. Las imágenes activas están empaquetadas y también se entregan extraídas del GLB.

## Comparación visual y valoración razonada

Se examinaron los PNG canónicos originales y el fragmento de ribera de `01_vista_cercana.png`. La comparación con el concepto juzga lenguaje visual, superficies y siluetas; no se presenta como igualdad de píxeles ni se inventan los edificios, enemigos o nodos de la ilustración.

Las cuatro capturas Unity I/K tienen la misma posición `(1018,132,-115)`, rotación, tamaños ortográficos 34/40/46/52 y resolución 1280×960. No se cambia el encuadre para favorecer K. El render Blender está identificado como tal y tiene una cámara diferente.

| Criterio | K, valoración aproximada | Evidencia y defecto pendiente |
|---|---|---|
| Composición de la pequeña zona | 6,5/10 | Se mantiene la base I; no pretende resolver la composición de toda Región 1. |
| Terreno y superficies | 6,5/10 | Riqueza de I preservada; grandes praderas todavía demasiado homogéneas. |
| Bosques y siluetas | 7/10 | Las nuevas ramas se leen mejor y rompen la repetición cercana; numerosos árboles conservan aún la copa redondeada anterior. |
| Río y orillas | 6,5/10 | Los accesos bajos y las piedras mejoran el contacto físico; persisten estratos y líneas demasiado continuas en el agua. |
| Luz y asentamiento | 6,5/10 | Sombras reales y contacto legible; falta la riqueza de iluminación de la ilustración. |
| Continuidad 3D/2,5D | 7/10 | Misma colocación y fuentes para el atlas; el tramado del fundido sigue siendo perceptible en fotogramas intermedios. |

**Valoración global: aproximadamente 6,5–7/10.** Es un juicio artístico, no una medición objetiva ni una nota inferida de pruebas o píxeles cambiados. K mejora componentes importantes de I, pero no debe darse por patrón final aprobado del mundo. La aprobación corresponde al propietario.

No se continúa acumulando pigmentos J o L: ambos se rechazan. El problema de acabado pendiente requiere trabajar la lectura material del agua y el contraste entre tierra/pradera; ensanchar máscaras o añadir ruido no ha demostrado resolverlo.

## Validación y alcance

- K: trabajos nativos de fuente y Unity finalizados con `success`; **18/18 PlayMode**, sin pruebas eliminadas ni debilitadas.
- Se verifica igualdad de transformaciones de los **156 pinos**, cámaras y resoluciones frente a I.
- Secuencia de **97 capturas reales** de Unity a 960×720, zoom 34 → 52 → 34. Primer y último PNG idénticos. MP4 codificado a 16 fotogramas/s; no representa una medición de FPS del dispositivo.
- Atlas 2,5D obtenido de las mallas reales; se mantiene el intervalo de fundido existente 36–50. No se afirma ausencia absoluta de artefactos: el tramado intermedio es visible.
- 76.246 triángulos únicos y 312.672 triángulos instanciados, frente a 73.122 y 311.552 en I; 38 mallas. Son recuentos del GLB, no pruebas de rendimiento móvil.
- No se modifican los sistemas funcionales, la UI, los guardados ni los colisionadores. El piloto sigue siendo una activación temporal y reversible en la escena de comprobación; **no es la integración completa de Región 1**.
- No se realizaron pruebas físicas móviles ni nuevas capturas verticales de este piloto. No se confunden con pruebas anteriores del mundo.
- Sin Qwen, compras, generaciones de pago, publicación ni fusión a main.

## Fuentes y reutilización

[Artefacto nativo K](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39957867110/artifacts/12503436845).

- `.blend` K SHA256: `1089e26bcb66f874d41a3579a1027916d5b7619ccc2892f8ac9c6c00b4234566`.
- GLB K SHA256: `23450b6ee83015f970d685e4682b218555315864d158f3f4754b30689c69fcc9`.
- ZIP nativo SHA256: `26160267bfa280e9b07e3dfdb36510b73f21e7cbdad5b40120c6bc42ef266d15`.

El paquete persistente organiza la fuente editable, entradas fijadas por hash, estudios descartados, texturas activas del GLB, mapas históricos, scripts, código de representación Unity, referencias intactas, capturas y manifiesto de integridad. Las texturas históricas no se deben importar indiscriminadamente: `runtime/textures` contiene las utilizadas por el recurso exportado.

Unidades de autoría en metros; Blender Z arriba, glTF/Unity Y arriba, origen local cero. La colocación del piloto utiliza el contrato de cámara existente; no se escala ni deforma la geografía para cambiar de vista. El GLB y el shader del piloto son recursos de presentación, sin nuevos objetos jugables.
