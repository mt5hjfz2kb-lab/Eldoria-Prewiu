# Eldoria — examen independiente del cerebro/ojos: primera evidencia real

**Fecha:** 2026-10-15. **Estado:** revisión de un ciclo histórico real, `ART_REJECTED`. No es un nuevo ciclo Qwen 4B/8B ni una aprobación de arte.

## Fuente auténtica y trazabilidad
- Torre original congelada: `Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_Tower_CohV1.glb`, SHA-256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
- Ejecución nativa Windows Qwen8B/Blender: [Actions #38694405904](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38694405904), artefacto ZIP **11953058281**. Modelo `qwen3-vl:8b-instruct-q4_K_M`.
- PNG originales inspeccionados y SHA-256: vista principal `c3718be36dff4dcba6acc4721d240071a60b30ab23a48ba18cbefad9edc48da2`; lateral `66cb09142ee48107ddca493a8dc627d7a3e86ea9881009e4a18828cc85dde6b3`; superior `e99f9566bf7bad598e70baa35bd043df06c5bf890623e71b012881bfa1ca6e9e`.
- Comparación Blender A/B fija: antes `88d2e725a0fc228f6b657c94a48d69db972795bfa66250be3632c41b2c21070a`, después `cec85648729636392513d34459aa225014e0dbad2ea18c32f26b5e39020a922e`. Plan bruto Qwen SHA-256 `8f6cff1374784504b7af27daf184a7c089f76ac8d75dcd47ac1194ab6bdc155b`.

## Observaciones visuales independientes — sin modificar el asset
1. La pieza original es una estructura de madera de planta compacta, muros de troncos/hileras y cubierta a cuatro aguas muy dominante en la silueta. Se ve desde la vista principal.
2. Una escalera exterior y su barandilla conectan visualmente con el lateral de la construcción. La zona de acceso es parte protegida; no se debe invadir sin demostrar su funcionalidad. Vista principal y lateral.
3. La cubierta contiene múltiples piezas solapadas o separadas por líneas oscuras. Las tres vistas muestran la disposición; las imágenes por sí solas **no prueban** que sean agujeros estructurales o errores de UV.
4. La vista superior deja ver el conjunto de cubiertas y aleros. Hace falta obtener contactos geométricos reales (rayos/BVH) para decidir dónde una riostra podría anclarse a una pared o a una viga, porque una caja global AABB incluye mucho espacio vacío.
5. La iluminación neutra del render deja superficies casi blancas; **no permite puntuar fidelidad PBR ni calidad de materiales del juego** sin consultar texturas, atlas y cámara Unity.
6. El después añadió dos pequeños elementos beige **separados visualmente de la cubierta**, arriba y a la derecha de la silueta. Ni la puerta ni la escalera mejoraron, y las nuevas piezas introducen un defecto evidente.

## Diagnóstico causal del razonamiento Qwen 8B
- **Semántica parcialmente correcta:** el plan identificó un problema plausible de soporte de alero y pidió dos riostras visibles, preservando la torre.
- **Plan espacial incorrecto:** en lugar de elegir puntos reales de apoyo sobre la malla, propuso dos primitivas `cylinder`, posiciones aproximadas `[0.75,1.0,2.5]` y `[0.75,1.0,2.7]`, con rotación `[0,90,0]`, sin prueba de contacto entre piezas y cubierta. Se invocó `ground contact` para una riostra de tejado sin definir una cadena de cargas creíble hacia pared/vigas. **No afirmar que la rotación de 90 sea grados o radianes sin revisar su intérprete Blender.**
- **Falso éxito técnico:** el gate `multi-part-world-contact-attempt-3.json` calculó solapamiento de AABB global, pero también anotó `actual_surface_contact_verified=false`. No constituye contacto físico real.
- **Autocrítica insuficiente:** el texto de la propia crítica Qwen declaró «No visible defects confirmed in the AFTER image» y a la vez «no pixel-level improvement», aunque la imagen muestra dos formas flotantes. El juicio independiente previo `reviews/review-qwen8b-floating-two-beams-38694405904.json` rechazó correctamente la pieza.
- **Medida técnica, no estética:** cambiaron 4.278 de 691.200 píxeles (0,0061892). No prueba mejora visual.
- **Atribución acotada:** Blender ejecutó el plan Qwen; el fallo observado apunta principalmente a diagnóstico/apoyo espacial y autocrítica, no a que Blender no sepa crear geometría. No demuestra que todo Qwen8B sea incapaz de diseñar, ni permite escoger entre 4B y 8B. El run 38694405904 solo ejecutó **1 caso 8B**; no contiene un par homogéneo 4B.

## Hipótesis artísticas futuras para diagnosticar, NO ejecutar ahora
1. **Arquitectura/anclaje:** señalar exactamente dos áreas de apoyo reales en malla desde vista lateral y elevada, razonar fuerza/continuidad, y declarar las comprobaciones BVH que lo confirmarían. **No se permite** resolverlo creando dos cilindros y suponiendo que tocarán.
2. **Composición/silueta:** decidir si una intervención de alero es realmente prioritaria frente al equilibrio entre cubierta y cuerpo. Explicar qué se vería desde cámara de juego y qué regiones no se tocarían.
3. **Materiales:** distinguir defectos confirmados de texturas/material de artefactos de iluminación sobreexpuesta; pedir evidencia adicional cuando no pueda determinarse.

## Siguiente examen comparativo, diseñado y pendiente de ejecución
Rama aislada `diagnostic/valoria-ai-eyes-brain-20261015`:
- `tools/art-direction/vision-diagnostic/render_source_views.py` prepara tres vistas PNG auténticas de la **misma fuente SHA** sin exportar ni guardar geometría.
- `tools/art-direction/vision-diagnostic/eyes_brain_exam.py` presenta a Qwen4B y Qwen8B las mismas tres vistas, referencia aprobada, bounds WORLD XYZ y memoria contrastada; exige exactamente tres defectos priorizados, evidencias, incertidumbres, y soluciones sin ejecutar Blender.
- Evaluar en revisión **independiente** la exactitud de observaciones, jerarquía de defectos, razonamiento de apoyo real, preservación de acceso/UV, y fiabilidad de la autocrítica. Registrar respuestas brutas, bytes/hash, tiempo y modelos locales. No inferir ganador antes de esta comparación emparejada.
- Si no hay acceso real a los modelos, publicar `NOT_EXECUTED`, **no** rellenar respuestas sintéticas. No instalar, descargar ni generar nuevos modelos, ni modificar escenas/Unity, ni reabrir ronda de modelado sin orden del propietario.

**Limitación de ejecución en este chat:** el conector GitHub permitió crear scripts pero bloqueó las dos operaciones para añadir un nuevo workflow de GitHub Actions. No se ha obtenido una ejecución en vivo 4B versus 8B de este examen; tampoco se ha obtenido permiso para sortear ese bloqueo. La orden de pausa del otro chat se registró en `main/AGENTS.md` commit `a847cf0d50927d28378fc256de5e8308fac9f042`, pero no cancela automáticamente otra sesión.
