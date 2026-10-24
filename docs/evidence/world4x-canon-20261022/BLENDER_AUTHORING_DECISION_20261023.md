# Cambio de autoridad artística: Blender → GLB → Unity

Orden expresa del propietario recibida durante la ejecución: dejar de usar Blender como visor. El procedimiento `freeze_terrain.py` queda como evidencia histórica de inspección, con `artistic_operations: 0`; no se presenta como autoría.

## Línea de base y acceso comprobado

Último candidato anterior: `fc69cf45c389a8b20e928aff895393016f142e66`, ejecución [39923094256](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39923094256), artefacto 12487828202. Sus 18 casos pasan, pero Unity falla al cerrar (`Cleanup mono`): **no es QA nativa completa**. Sus PNG auténticos serán el control de Unity; ninguna captura de Blender se etiquetará como Unity.

El PC ejecutó Blender **5.2.2 LTS**, hash `d13f752e3b9c`, en el run 39921599849. Unity instalado y ejecutado: **6000.3.23f1**. El paquete `com.unity.cloud.gltfast` **6.14.1** ya está en el proyecto. Su [documentación oficial de importación en Editor](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/main/Packages/com.unity.cloud.gltfast/Documentation~/ImportEditor.md) confirma importación de GLB como prefabs nativos. No se instala otro importador, no se compra nada ni se consume Qwen/Tripo.

## Primera hipótesis macroscópica — antes de ejecutar

Crear una escena Blender vacía. Registrar en ella los tres PNG originales como imágenes de apoyo empaquetadas, ocultas en render/exportación. Trazar en el espacio de la referencia panorámica costa, cuencas, caminos, crestas y áreas forestales. Convertir esos trazados a coordenadas métricas mediante una calibración explícita alrededor de la Brecha (8,61), sin copiar el aspecto del PNG completo con sus títulos.

Las curvas Bézier son objetos editables de Blender. De ellas se esculpe una malla continua y sus atributos `forest_density`, `river_mask`, `path_mask`, `land_mask`, `breach_damage`. El cauce se talla antes de crear su superficie de agua. Los caminos siguen las alturas del mismo terreno. La Brecha es una depresión con ramificaciones talladas, no un decal luminoso. La vegetación en esta fase son volúmenes de masas, deliberadamente sin árboles individuales, hierba ni piedras.

El `.blend` conserva colecciones, curvas, malla, atributos, materiales Principled, luz, cámaras calibradas con los metadatos reales de Unity y el script de autoría. La exportación GLB contiene exclusivamente la nueva geografía; no las imágenes de referencia, cámaras de trabajo o luces. Los datos de consulta de alturas/máscaras se exportan desde esa misma malla para que Unity no invente otra geografía.

## Puertas de producción

1. Ejecutar esta autoría en Blender y revisar sus renders de composición. Este run de autoría **no sustituye las pruebas de Unity** ni concede ART_APPROVED.
2. Importar el GLB pronto en la escena integrada, desactivando el constructor matemático anterior de forma controlada. Mantener objetos, IDs, colisiones, progreso, UI y enemigos; verificar escala, orientación y anclas.
3. Comparar seis cámaras exactas con fc69cf4 y las tres referencias por composición, sin equivalencia pixel a pixel con la ilustración. Proponer encuadre nuevo por separado si hace falta.
4. Solo después de una composición convincente, crear familias de árboles, acabado de materiales y LOD. No disfrazar un blockout deficiente con detalle.

Máximo dos composiciones macroscópicas con este método antes de reconsiderarlo. Registrar qué formas se corrigen y por qué. No presentar actividad del proceso, cantidad de operaciones ni tests verdes como calidad artística.

Estado inicial de este documento: autoría preparada, aún sin render revisado ni integración del nuevo GLB. El propietario conserva la aprobación artística final. Sin merge ni publicación.
