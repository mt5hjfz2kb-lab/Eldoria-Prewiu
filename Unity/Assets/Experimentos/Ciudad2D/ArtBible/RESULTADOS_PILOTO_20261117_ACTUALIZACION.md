# Valoria 2.5D — Recuperación, tres candidatos y primera comparación Unity auténtica
**Fecha:** 17 noviembre 2026. **Alcance:** rama aislada `art/ciudad-2d-hitos-1-5-20261116`.  
**Dictamen:** tres fuentes gráficas + tres PNG RGBA archivados, galería A/B de Unity Editor **probada**, pero **DIRECCIÓN ARTÍSTICA Y MÉTODO DE CATÁLOGO NO APROBADOS**.

> Este documento actualiza el estado histórico de `RESULTADOS_PILOTO_20261117.md`; no reescribe las decisiones anteriores. Ningún cambio en `main`, escena de juego, partidas, combate, economía o scripts de progresión.

## 1. Fuentes recuperadas, bit a bit

Seis maestros originales continúan inmutables según `master_manifest_v1.json`. Su precedencia sigue siendo Bastión V > Bastión I > secundarios. La [ejecución técnica #44040992476](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44040992476) verificó sus SHA y los tres nuevos PNG del piloto.

| Pieza piloto | Fuente preservada | Sprite RGBA en cuarentena | Procedencia |
|---|---|---|---|
| Forja de la Guardia | `Pilots/forja_guardia/v1/source.jpg` (SHA `dd84edb2c00f…`) | `Sprites/AIPilot/Forja_Guardia_RGBA.png` (SHA `620fb58c48da…`, 875×875) | Runway nano-banana-2-lite, tarea `512b4dd8-4197-474f-8758-d06ab28e06b0` y máscara `a382070c-3986-4a01-b192-d33a9e68612f` |
| Observatorio de la Brecha | `Pilots/observatorio_brecha/v1/source.jpg` (SHA `627637f5ec3b…`) | `Sprites/AIPilot/Observatorio_Brecha_RGBA.png` (SHA `74d058a9b891…`, 1028×1028) | Runway nano-banana-2-lite, tarea `1aba8e1c-1d9c-4ae9-ab08-ec825470db9e` y máscara `5fec8a9e-b251-4654-9692-64fd1df6d4bc` |
| Archivo de Cartógrafos | `Pilots/archivo_cartografos/v1/source_original.png` (SHA `0f3a1ead1d0a…`) | `Sprites/AIPilot/Archivo_Cartografos_RGBA.png` (SHA `7bb221f5f766…`, 1496×1496) | Otro generador de imagen, `f785b806-0718-4667-bf58-251d3400d3e2`; el generador Runway gen-4-image-turbo anterior falló repetidamente |

Cada carpeta V1 conserva el original, máscara, PNG fuente o RGBA, registro SHA y versión normalizada. Para la imagen de Archivo se empleó un **recorte localizado por continuidad con el fondo oscuro**, cerrando únicamente pequeñas discontinuidades de la arquitectura y manteniendo el origen exacto; el procedimiento, no un filtro global, está en `tools/ciudad2d-ai/import_archivo.py`. La compatibilidad pictórica entre generadores **no está demostrada**. Véase `Pilots/generation_provenance_v2.json`: solo las dos primeras tareas conservan prompts completos; no inventar el prompt/seed de la tercera.

## 2. Evidencia auténtica Unity y controles

- [GitHub Actions #44040675403 — SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44040675403), fuente exacta `b7b914176821d6400a77fa6953acbc9469f1f1c1`, artefacto `ciudad2d-real-unity-44040675403` ID `14310200292`.
- `City2DAIPilotGallery.Capture` produjo **8 capturas reales del render Unity Editor**: original A / terna IA B × 390×844, 1170×2532, 844×390 y detalle 1170×2532 (zoom 1,42). `Ciudad2DAIPilotEvidence/evidence.json` contiene el SHA de cada archivo. Fueron comprobados los ocho SHA, dimensiones y presencia del reporte, también fuera del runner.
- **Misma cámara, posiciones y terreno**; comparación normalizada por ancho real de silueta alfa, y pies compensados por margen transparente. No se alteraron el Bastión V, las versiones antiguas ni la lógica del juego.
- La misma ejecución repitió exitosamente PlayMode/HUD/interacción de la **escena existente**. Esto **no prueba las tres edificaciones nuevas en PlayMode**: son decorado de galería sin nuevas mecánicas ni interacción jugable.
- La compilación preliminar de galería había pasado en [#44037579736](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44037579736). La compilación no equivale a aprobación visual. No existe prueba física iPhone/Android para esta terna.

## 3. Evaluación visual frente a Bastión V

**Veredicto: ninguno supera todavía el umbral de aceptación de la Biblia Visual (≥8/10 por edificio con ausencia de bloqueos).** Observaciones de la galería real y de los PNG a escala completa; juicios visuales, no mediciones automatizadas de belleza.

| Candidato | Fortalezas observables | Fallos pendientes que bloquean promoción |
|---|---|---|
| **Forja B1** | Silueta legible de taller, horno y puerta, pizarra azul e iluminación cálida; ya no tiene losas cuadradas artificiales | La segmentación SAM3 había eliminado parte del humo y del tejido oscuro del estandarte; algunos huecos bajos parecen vaciados, el edificio sigue siendo menos rico en mampostería que Bastión V y no tiene sombra de contacto separada |
| **Observatorio B1** | Función diferenciada por torre octogonal, instrumento superior y luz violeta comedida | Verticalidad frontal/rigidez incompatible con la lectura 3/4 más rica del Bastión, proporción dominante junto a la ciudad, detalles de bandera en recorte y apoyo débil al terreno |
| **Archivo B1** | Mayor riqueza visible en piedra, tejados y detalles funcionales; arquitectura diferente | El espacio del Granero original no permite ver el nuevo edificio completamente a 390×844; emblemas astrales se alejan del motivo vegetal/solar del maestro, perspectiva y luz aún no calibradas con los otros dos, sin sombra independiente |

**Resultado general:** sí han desaparecido plataformas visibles en los nuevos candidatos, pero la mera ausencia de plintos no demuestra una integración orgánica. Quedan por cerrar recorte limpio a escala original, cámara 3/4 común, encuadres individuales, contacto con terreno y coherencia entre la textura de Forja/Observatorio y la de Archivo/Bastión V. Las imágenes A/B hacen visible el problema, no lo resuelven por sí solas.

## 4. Corrección real y conservadora ya preservada: Forja B2

Se verificó en la foto original qué partes del pendón azul y humo estaban presentes y habían desaparecido en la máscara. El script `tools/ciudad2d-ai/restore_forja_v2.py` recupera **solo esos píxeles de la fuente original**, usando regiones delimitadas y alfa suave; deja intacta la arquitectura, la fuente V1 y los sprites originales. [GitHub Actions #44042502005 — SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44042502005).

- Variante B2: `Sprites/AIPilot/Versions/Forja_Guardia_RGBA_v2.png`, SHA `317b4d48e482fbc86da8ed761a15e0a98a1a6563e36ca24b6dc7cf6053f8e4fb`, 879×879.
- Capa separada de píxeles restaurados `Pilots/forja_guardia/v2/original_smoke_and_banner_FX_rgba.png` y comparativa offline en `Pilots/forja_guardia/v2/QA_BGR_COMPARISON_OFFLINE_NOT_UNITY.jpg`.
- B2 **no está aún comparada dentro de Unity**. Este cambio prueba recuperación técnica localizada del original, no que la arquitectura o identidad global ya haya alcanzado ≥8/10.

## 5. Comparación con Blender y lecciones conservadas

El control A/Blender previo tiene evidencia real en [#44033293154](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44033293154). Blender mantiene control exacto de mallas, cámara, sombra, materiales y revisiones de los objetos. **Sus primeros edificios parecían excesivamente simples**, por lo que no conviene regresar a esos modelos ni adoptar un proceso 3D obligatorio para cada sprite. El piloto IA revela mayor potencial de detalle, pero no domina aún los ángulos comunes y la coherencia de materiales.

**Fallo fundamental de reproducibilidad:** los dos primeros candidatos y el tercero no comparten el mismo modelo de generación. Incluso los dos primeros difieren en volumetría y riqueza. Los modelos de referencia por sí solos no fijan proyección ni color con precisión. El próximo método debería usar una referencia geométrica/cámara común para **guiar** la imagen pictórica (por ejemplo, un volumen 3D simplificado usado únicamente como guía de perspectiva) y un control consistente de materiales, no sustituir arte por bloques 3D ni volver a aplicar filtros de nitidez.

## 6. Puertas de calidad, todavía cerradas

- **Pendiente:** sombra de contacto RGBA independiente y comprobada sobre el terreno; revisión de alfa sobre negro/gris/verde/suelo a 100 %.
- **Pendiente:** vistas individuales sin recortes para los tres y escala comparable según huella arquitectónica, no según largo de antena o humo.
- **Pendiente:** corregir dibujo/perspectiva del Observatorio y símbolos/perspectiva del Archivo. No basta con mejorar nitidez.
- **Pendiente:** probar la Forja B2 corregida en Unity y pasar revisión visual frente a Bastión V.
- **Pendiente:** repetir el mismo proceso de generación y edición de referencias con tres edificios nuevos bajo un generador/control homogéneo; solo entonces hablar de catálogo reproducible.
- **Pendiente:** QA física móvil si el arte se propone para el juego real. Ninguna nueva construcción jugable ni ampliación del Bastión está autorizada.

**Distinción final:** recuperación implementada y auditada ✓; 3 PNG RGBA versionados ✓; Unity A/B **Editor** verificado ✓; PlayMode anterior revalidado ✓; Forja B2 corregida y archivada ✓; acabado artístico homogéneo, sombra, catálogo reproducible y dispositivos físicos **NO VERIFICADOS**.
