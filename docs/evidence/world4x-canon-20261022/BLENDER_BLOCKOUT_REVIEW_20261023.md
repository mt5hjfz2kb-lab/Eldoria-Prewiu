# Blender blockout A — revisión real y cambio de construcción

Autoría: `dd87fa8a2584ee1974be514992fc1761467722df`, run [39925001666](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39925001666). Artefacto 12489300562, ZIP SHA256 `fb27b9c2a64081cf23240fd6d352cece19885f04e33b460db6de23db7701d761`. Contiene `.blend` editable, seis renders, GLB, consultas y manifiesto con cuatro clases de operaciones reales. No se importa terreno anterior.

Integración Unity: `1b393104a0a08ed92133e1a5659cb709f00527c7`, run [39926747679](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39926747679). Artefacto 12489752877, ZIP SHA256 `21d4eee5b6a7907dafc4db7d492438d06480781afbace284b376d6c245cbfcd3`. 18/18 pruebas y salida nativa correcta, 14 PNG auténticos; GLB importado, escala/orientación y altura contrastadas, ancla de Brecha preservada. Este éxito técnico NO es aprobación artística.

## Revisión de imágenes

La panorámica vertical de Blender y la de Unity presentan el mismo defecto de construcción: costa y franjas de biomas en cuadrados; relieve esquemático; bosques como volúmenes repetidos; fisura circular y demasiado ancha. En la cercana, caminos atraviesan agua sin una conexión convincente. La escena nueva existe y es editable, pero **todavía no es una mejora artística sustancial** sobre fc69cf4. No se añade acabado para ocultarlo.

Valoración razonada provisional del blockout, NO medición objetiva: composición 5/10 (disposición principal trazada pero grandes extensiones vacías); relieve 2/10; masas forestales 3/10 (posición reconocible, silueta insuficiente); agua/caminos 3/10; Brecha 2/10; materiales 2/10 (colores de identificación, no acabado); continuidad técnica de zooms comprobada, calidad visual cercana 2/10. No promediar estos criterios para fingir calidad final.

## Segunda intervención macroscópica — hipótesis previa

La cuadrícula regular de ~7.6 metros no representa cauces más estrechos que sus celdas ni los entrantes de costa. Cambiar resolución a ciegas costaría geometría y conservaría el problema de representación. Se cambia a triangulación restringida de Blender: las curvas de costa, orillas, rutas y fisuras se convierten en aristas obligatorias de la malla. La densidad se concentra en accidentes geográficos, no en toda la pradera. La Brecha deja de usar una máscara circular: su profundidad deriva de las ramificaciones talladas. Los colores de bioma se interpolan por vértice para eliminar escalones de material por cara.

Fuente técnica consultada: documentación oficial de `mathutils.geometry.delaunay_2d_cdt`, https://docs.blender.org/api/main/mathutils.geometry.html . Acceso directo a `/current/` falló; se leyó el contenido técnico indexado de la documentación oficial, incluida firma, restricciones, resultado y advertencia sobre reordenación de vértices. `output_type=0` conserva triangulación de la envolvente; las restricciones fuerzan las líneas geográficas. No se presupone conservar índices originales. Se usa BVH de la malla final para colocar caminos y exportar consultas de altura.

Esta es la segunda construcción macroscópica. Si las capturas no mejoran claramente, se reconsiderará el método antes de añadir árboles y detalle. La primera fuente y sus artefactos quedan intactos y recuperables. ART_NOT_APPROVED; sin publicación, gasto, Qwen ni cambios a main.
