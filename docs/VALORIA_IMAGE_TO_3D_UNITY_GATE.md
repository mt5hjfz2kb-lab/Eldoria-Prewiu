# Bastión optimizado — resultado real de la prueba aislada Unity

Fecha: 2026-09-26. Código evaluado: `cc9f638b0ffd5470d88c29981d18d6d95d80ea81`. [Windows Actions run 36269097317, intento 2](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36269097317): preflight, EditMode, PlayMode, build y captura aislada terminados con estado **success**. El paso de preparación registró **“Verified optimized Bastion staged from Downloads; source GLB was not used.”** La prueba usó únicamente `Bastion_Optimized_v1.glb` (SHA-256 `1ba4ef4daa352265116e672dfb84d5bbe26a0bd22262a2955d94cd4682862521`); no reprocesó el original ~501k. El éxito del job significa que el importador y las comprobaciones programadas corrieron; **no significa aprobación artística**.

## Evidencia reproducible

[Artifact de la revisión, ID 10914954129](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36269097317/artifacts/10914954129): `ImageTo3DReviewCaptures/{strategic,city,detail,oblique}.png`, `metrics.json` y `Assets/Eldoria/ArtTests/ImageTo3D/BastionImageTo3DReview.unity`. Se inspeccionaron las cuatro capturas reales de Unity a **1280×720**; los tamaños ortográficos son respectivamente **19, 12 y 9**, con una cuarta vista oblicua/orbitada. La referencia pictórica muestra torres legibles, portales, tejados, muros y roca separados, unidos por un camino. En las cuatro capturas de Unity se distingue volumen y silueta general de castillo sobre roca, pero fachadas, huecos y tejados aparecen fundidos en una superficie brillante y multicolor. En detalle no se recupera la arquitectura de la referencia. **Falla la calidad exigida en los tres zooms.** La vista oblicua confirma volumen real, no un plano.

| Medida del reporte Unity | Resultado | Lectura |
| --- | ---: | --- |
| Mallas / renderers / materiales | 1 / 1 / 1 | Una pieza hero; ningún módulo independiente |
| Vértices / triángulos | 39.251 / 81.506 | Reducción del ~83,7 % desde ~501.472 triángulos del original inspeccionado |
| UV / normales | presentes / presentes | Atributos geométricos importados |
| Texturas enlazadas | 3 de **4×4** | El paquete inspeccionado declaraba tres imágenes 2048×2048; la instancia Unity no conservó esa resolución. Causa exacta de la sustitución **no demostrada** |
| Memoria de malla / texturas (Profiler Editor) | 5.725.808 / 15.660 bytes | Solo objetos reportados en este Editor; **no** memoria GPU total ni presupuesto de ciudad |
| Escala aplicada / caja resultante | 22,120018× / 13,03 × 14,00 × 22,00 unidades | Normalización visual; **no** escala arquitectónica calibrada en metros |
| MeshCollider + Physics.Raycast | acierto positivo / fallo correcto en espacio vacío | Colisión geométrica básica; el script de clic está conectado, interacción manual Play Mode no comprobada |
| Captura CPU (19 / 12 / 9 / oblicua) | 924 / 59 / 44 / 39 ms | Incluye primer fotograma y lectura/PNG en Editor; **no es FPS**, tiempo GPU ni benchmark móvil |

El GLB de entrada tenía 12.794.888 bytes y una sola malla, material PBR y tres imágenes 2K según inspección previa. En Unity, las tres texturas enlazadas aparecen como `image_0`, `image_2`, `image_1` de 4×4; la apariencia defectuosa concuerda con pérdida de mapas útiles, pero no permite distinguir por sí sola entre exportación del GLB, importación glTFast y configuración del material. Las torres y arcos derretidos también pueden incluir pérdida de forma por image-to-3D o por decimación: **no se aisló cuál de esos pasos la causó**. No atribuir al modelo un coste de 15 KB de texturas de producción: corresponde a la importación defectuosa medida.

La escena coloca la malla sobre un plano de revisión: **no verifica unión física convincente entre roca, terreno y camino**. El `MeshCollider` coincide con la malla completa; para clics por edificio, etapas, puentes o muros hacen falta colliders separados. No existen módulos semánticos, LOD, instanciación, múltiples estructuras, medidas GPU/FPS ni rendimiento de ciudad 4X. Una única malla fusionada exige separación/retopología y pivotes externos si se pretende kit modular; no se midieron horas manuales porque no se realizó esa limpieza.

## Checklist de reentrada para un único fragmento corregido

1. **GLB recibido:** comprobar SHA, integridad, resolución real de las tres imágenes embebidas y formas distinguibles en un visor externo; no volver a procesar el original de 501k.
2. **Importación:** copiar solo el optimizado a `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Source/Bastion_Optimized_v1.glb`; usar Unity 6.3 LTS con `com.unity.cloud.gltfast` 6.14.1. Ejecutar `Eldoria.EditorTools.BastionImageTo3DReview.Capture` en la escena aislada.
3. **Materiales:** exigir texturas enlazadas con resolución útil (2K antes de compresión, o tamaño reducido explícito) y base color / normal / metal-rugosidad correctos. Rechazar 4×4 y brillo metálico erróneo. Si falla, corregir exportación/bake o la importación del material antes de medir FPS.
4. **Escala y pivote:** calibrar una medida arquitectónica en DCC, eje Y vertical, base a suelo y pivotes aptos para colocación; anotar factor importado.
5. **Optimización:** registrar vértices, triángulos, número de materiales, VRAM y tiempos GPU en dispositivo objetivo; crear LOD/colliders simplificados fuera de Unity si procede. Esta prueba no fija un presupuesto aceptado de ciudad.
6. **Collider y terreno:** probar clic real en Play Mode, selección correcta frente a espacio vacío, y colocar una torre, tramo de muro, roca y camino con juntas visibles desde dos ángulos. Si siguen fusionados, registrar labor de separación.
7. **Tres zooms:** comparar `strategic/city/detail` a ortográfica 19/12/9 y 1280×720 con la misma referencia; guardar también `oblique`, métricas y decisión aceptación/rechazo. Solo pasar si geometría y materiales preservan identidad, unión y selección sin trabajo manual prohibitivo.

## Veredicto

**C — NO VIABLE COMO PIPELINE PRINCIPAL con este asset y este importado.** El tramo image-to-3D → malla real → reducción externa → importación Unity → raycast sí quedó demostrado; automatiza una **base geométrica de pieza hero**. La cadena falla hoy en texturas útiles importadas, legibilidad arquitectónica de los tres zooms y modularidad. No hay evidencia para declarar que una segunda optimización baste ni para estimar rendimiento o trabajo manual de Valoria. La siguiente alternativa técnica acotada es diagnosticar el bake/export de materiales 2K y limpiar/retopologizar **un solo fragmento reconocible** fuera de Work, medir horas y volver a esta puerta. Si requiere reconstrucción manual intensa o sigue fallando a 19/12/9, descartar image-to-3D como fuente primaria del kit y probar un kit 3D modular diseñado desde origen.

**Alcance de cambios:** esta actualización incorpora solo documentación. No modifica `Valoria.unity`, `VisualWorld` ni gameplay. El GLB no se publica en `main`; quedó disponible en Downloads del runner durante esta ejecución, de modo que la escena del artifact requiere el GLB exacto para reabrirse sin referencias rotas.
