# Método de producción de sprites IA con referencias maestras · v1.0

**Estatus:** diseño de producción **implementado documentalmente**, hipótesis visual **no certificada aún**. Conservar Blender PBR como control B; nunca promocionarlo automáticamente. No ejecutar procesos de mejora de nitidez que intenten «rescatar» un mal sprite.

## Ciclo de una pieza
1. Registrar `asset_id`, función, dimensiones de huella, fase/level artístico, `master_manifest_v1.json` y versión de prompt **antes** de generar.
2. Cargar físicamente **Bastion_V_MASTER.png** + un secundario relevante; no asumir que el modelo conoce nuestra ciudad por un prompt. Identificar nombres de tarea/modelo y si el mismo tipo de referencia ha sido realmente consumido.
3. Generar **un único edificio por ejecución**, no lienzo de ciudad, UI o póster, fijando el bloque común: cámara, 3/4, materiales, pizarra azul, luz ámbar desde izquierda, sombras azul-violeta, riqueza pictórica y sin plinto. Añadir un bloque específico de función/masa.
4. Guardar prompt completo, referencias con SHA, modelo, task ID, versión y PNG fuente. Si la API no expone seed, anotarlo `unknown`; la repetición será *de especificación*, no del byte exacto.
5. Exportar RGBA con máscara confirmada: si el proveedor devuelve JPG/negro, segmentar el edificio con máscara revisada y guardar `mask.png`; la eliminación automática de fondo sin inspección se considera QA pendiente. No recortar piedra oscura ni humo luminoso mediante un simple umbral global.
6. Recortar por bbox RGBA + margen 3 %, centrar horizontalmente en la **huella**, alinear pivot de apoyo al borde inferior; no desplazar arquitectura verticalmente arbitrariamente. Sombra separada solo si la original es limpia y controlable; sin base/rombo de suelo.
7. Controlar a 1x y 3x sobre al menos cuatro fondos (transparente gris, negro, verde y terreno orgánico); evaluar identidad y espacio físico antes de pasarlo a Unity.
8. Integrar como **candidato B en escena aislada**; comparar misma cámara, mismo nivel/posición, render iPhone 390×844 y 1170×2532, 844×390 y zoom. Conservar la imagen A visible para rollback.
9. Gate técnica: PNG fuente+SHA, script de procesamiento/máscara+SHA, collider sin cambios, selección, PlayMode, snapshot de lectura sin escribir. Gate artística: ficha de puntuación contra seis maestros; si falla, identificar causa y regenerar solo pieza defectuosa.
10. Sólo promocionar método al demostrar **tres clases arquitectónicas distintas**, individualmente válidas y comparables. **No producir Bastión 10/15 ni nuevas regiones aún.**

## Prompts congelados (referencias visuales obligatorias)
Común `STYLE_BASE_v1`:
"Single standalone game-ready 2.5D isometric Valoria building. Use supplied reference masters only as strict visual style; orthographic high 3/4 camera, warm gray worn stone, weathered oak, heavy blue-slate roofs, dark navy/gold heraldry, warm late-day light from upper left, violet-blue shadow, richly detailed mature fantasy illustration. Entire isolated building, readable small-size silhouette, NO ground slab/platform, NO characters, text or UI. Export with clean alpha; do not change Valoria symbols or palette."
Sufijos `FUNCTION_v1`:
- FORJA: edificio bajo asimétrico en L, horno abierto, chimenea robusta, yunque y herrería de Valoria.
- OBSERVATORIO: volumen alto octogonal, balcón lateral, astrolabio de latón, cúpula/pizarra y cristal discreto de Brecha.
- ARCHIVO: cuerpo doble de biblioteca/cartógrafos, torre de lectura pequeña, mesa de mapas, arcos y ventanales cálidos.

## Pruebas de generador realizadas 17/11/2026
- Generación `nano-banana-2-lite` con master visual compuesto, una Forja completa: tarea `512b4dd8-4197-474f-8758-d06ab28e06b0`. El proveedor entregó **JPG**, no PNG RGBA: requiere segmentación real; **no está listo para Unity**.
- Segunda generación con la misma hoja maestra y modelo para Observatorio: tarea `1aba8e1c-1d9c-4ae9-ab08-ec825470db9e`; resultado generado, pendiente de extracción de archivo y evaluación.
- Modelo `gen-4-image-turbo` con referencias y bloque largo: tres tareas `ed19b700...`, `dcb99ac7...`, `49f9a92d...` fallaron `INTERNAL.BAD_OUTPUT.CODE01`; cuarto intento abreviado `7fc65a78...` también falló. No imputar a la Biblia un fallo del proveedor; no usar esa vía hasta investigar su disponibilidad.
- Modelo y servicios de edición con prompt libre sin control de referencia produjeron composiciones de ciudad/infografía en vez de sprites aislados. **Rechazadas como assets**, no contabilizar como éxitos.
- Comparativa Blender PBR 17/11: buen gate de PlayMode y archivos fuente reales, pero el acabado arquitectónico del piloto se percibe inferior en integración de las piezas originales. Mantenerlo para geometría/perspectiva puntuales.

## Convención de archivos
`ArtBible/Masters/...` copias originales inmutables; `ArtBible/Pilots/<id>/source.{png,jpg}`, `mask.png`, `sprite_rgba.png`, `shadow_rgba.png`, `asset_manifest.json`, `QA/native/A_original.png`, `QA/native/B_candidate.png`. Una carpeta por versión; no borrar muestras rechazadas.

## Definición de «reproducible» — honesta
La receta conserva fuentes de referencia, prompt, servicio/modelo y parámetros, pasos de segmentación, recorte/pivote, condiciones de importación y pruebas. **No** promete igualdad pixel a pixel si el modelo es estocástico o ha cambiado de versión. Versionar siempre la salida exacta para las compilaciones de Unity y comprobar SHA.

## Comparación provisional de métodos
| Factor | Ilustración IA con maestros (hipótesis) | Blender→sprite (probado en piloto) |
|---|---|---|
| Riqueza visual | Prometedora en originales; **no certificada en tres nuevos** | Inferior en el último A/B respecto a los originales |
| Consistencia | Puede variar cámara y detalle; requiere gate triple | Cámara y materiales físicamente repetibles |
| Producción de variantes | Pocos pasos para explorar, múltiples rechazos posibles | Geometría editable pero trabajo manual elevado |
| Transparencia | Fallo habitual: JPG/fondo, halos | RGBA y sombra por pases reproducibles |
| Integración | Todavía NO probada para tres nuevos | Editor Unity y PlayMode reales del par certificados |
| Riesgo dominante | deriva de estilo/ángulo, modelo no controlable | bloqueos genéricos, materiales sin riqueza |
**Decisión pendiente:** no declarar método vencedor hasta 3 nuevos sprites aceptados, capturas Unity A/B y validación visual por encima de Bastión V.
