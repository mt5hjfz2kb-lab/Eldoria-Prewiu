# Eldoria — puerta de producción de Tripo por módulos pequeños

Fecha: 2026-09-26. **Decisión de investigación, sin nueva generación ni ejecución Unity en este bloque.** Mantener `Valoria.unity`, `VisualWorld` y gameplay intactos.

## Evidencia y alcance de la decisión

Según la prueba y observaciones comunicadas por el propietario:

| Comprobación | Estado |
| --- | --- |
| Imagen → volumen | Tripo produce geometría 3D real. |
| Coherencia de pieza pequeña | Mejor que la del Bastión completo observado; valoración visual preliminar, sin aceptación Unity. |
| Generate in Parts / segmentación | Se observan múltiples partes separadas dentro de Tripo. **No consta aún que el GLB exportado preserve objetos independientes y pivotes útiles.** |
| Retopología interna | La petición de 5.000 polígonos produjo aproximadamente **743 caras / 691 vértices** en el resultado observado; control de objetivo no fiable en esta prueba. No interpretar caras y vértices como la misma unidad ni extrapolar a otros presets. |
| Texturizado final de partes | Pendiente de validar exportación, UV, resolución y apariencia en Unity. |

**Clasificación provisional:** Tripo es generador y segmentador de módulos pequeños, **no** solución demostrada de retopología/materiales lista para Unity. La evaluación anterior de un Bastión completo de una sola malla y tres texturas importadas de 4×4 permanece cerrada con resultado **C para aquel asset concreto**, no determina por sí misma el resultado de la nueva ruta modular. Sus medidas y cuatro capturas de Unity están en [la puerta previa](VALORIA_IMAGE_TO_3D_UNITY_GATE.md).

## Ruta siguiente, con un único módulo

`concept art → módulo pequeño en Tripo → Generate in Parts/segmentación → exportar GLB con partes separadas → limpieza/retopo/UV/materiales fuera de Tripo → Unity`.

El fragmento objetivo sigue siendo una torre o fachada reconocible, tramo breve de muro, base de roca y conexión corta de camino/terreno; puede entregarse como partes que se ensamblan en la escena aislada. **No generar otra fortaleza o ciudad completa ni repetir la reducción agresiva dentro de Tripo.**

Para entrar en la siguiente puerta, entregar **un GLB modular pequeño ya limpiado externamente** y, si está disponible sin repetir trabajo, una copia de exportación cruda para medir labor externa. Adjuntar captura/listado de la jerarquía de partes, recuento de triángulos por pieza, mapas/UV y resoluciones, escala/pivotes, herramienta y pasos externos, y tiempo manual invertido. No presuponer que la segmentación interna preserva módulos reutilizables hasta abrir el GLB exportado. El acabado de un módulo debe poder verse desde varios ángulos; una fachada frontal con dorso irrecuperable no basta.

En `Unity/Assets/Eldoria/ArtTests/ImageTo3D/`, con Unity 6.3 LTS y el importador glTFast ya declarado, importar **una copia** del GLB recibido en una nueva escena aislada. Conservar el asset recibido sin alterarlo; cualquier material o collider de prueba se guarda allí. Revisar jerarquía de MeshFilter/renderers y habilitar/deshabilitar partes por separado; contar materiales, texturas, triángulos y coste de memoria real. Exigir mapas de resolución útil y shader URP correcto: la puerta anterior importó 4×4 pese a que el paquete declaraba 2K. Normalizar escala con una medida real y comprobar orientación/pivotes antes de juzgar silueta.

Capturar en 1280×720 a ortográfica **19 / 12 / 9** con luz y encuadre constantes, más vista oblicua de volumen; comparar con la referencia del mismo módulo. Añadir colliders separados o un collider simplificado por parte seleccionable y comprobar un clic real en Play Mode, acierto en la pieza y ausencia de selección en vacío. Unir roca, terreno y camino en la escena aislada; inspeccionar frente, lateral y oblicua para detectar huecos, flotación, intersecciones o conexión rota. Medir coste y tiempo manual antes de proponer un kit para la ciudad. El capturador anterior espera un nombre exacto `Bastion_Optimized_v1.glb` y mide una malla; **no sirve sin adaptación para certificar separación modular**. No reutilizar sus métricas como prueba de esta entrega.

## Checklist corto de reentrada

1. **GLB modular recibido** → comprobar archivo, procedencia y jerarquía real tras exportación.
2. **Validación de partes** → aislar torre, muro, roca y conexión; inspeccionar volumen, pivotes, costuras y triángulos por pieza.
3. **Retopo externo** → registrar herramienta, operaciones, pérdida de silueta y minutos manuales; no usar la cifra solicitada a Tripo como recuento efectivo.
4. **UV/materiales** → verificar mapas reales, resolución importada, normales y apariencia URP desde dos ángulos.
5. **Importación Unity** → escena y copias solo en `ArtTests/ImageTo3D/`; escala y orientación documentadas.
6. **Tres zooms** → 19 / 12 / 9 a 1280×720 más vista oblicua, con referencia y capturas.
7. **Collider** → selección por clic real en cada parte prevista; vacío y suelo no seleccionan edificio.
8. **Terreno** → contacto físico y visual roca/suelo/camino sin huecos desde dos ángulos.
9. **Aceptación/rechazo** → registrar fidelidad, rendimiento, módulos reutilizables y horas externas por pieza antes de decidir si escala a Valoria.

**Puerta de decisión:** A si el pequeño kit conserva calidad, exporta partes reusables y su limpieza externa cuesta un tiempo asumible por módulo; B si generación/segmentación aceleran la base pero retopo/bake y montaje requieren intervención externa repetible; C si la separación, los materiales o la limpieza exigen reconstrucción manual comparable a modelar desde cero, o falla la lectura a 19/12/9. **La nueva ruta sigue pendiente de prueba**; no asignar A/B/C definitivos hasta recibir e importar ese GLB.
