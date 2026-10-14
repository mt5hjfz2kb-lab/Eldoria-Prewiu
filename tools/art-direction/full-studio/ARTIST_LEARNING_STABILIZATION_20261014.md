# Eldoria — recuperación de capacidad artística y memoria verificable (2026-10-14)

## Alcance y separación de trabajos

Corrección **aislada** sobre el estudio existente en `tools/art-direction/full-studio/`.
**No** sustituye el autor Blender/Qwen, no reabre una línea de arte, no levanta
`owner_production_pause.active=true`, no activa Windows, no promociona piezas
ni modifica la escena Unity. Esta rama nace de
`art/eldoria-full-blender-artist-studio-20261014`.
El trabajo paralelo de validación profesional
`validation/eldoria-professional-art-fixed-source-20261014`
contiene gates geométricos adicionales y evoluciona por separado:
**reconciliar mediante diff y pruebas, no sobrescribir su árbol ni fusionar a ciegas**.

## Correcciones comprobables de esta entrega

1. **Rol artístico:** recuperar la instrucción canónica de ~4.9k caracteres en
   vez de cortarla a 530, conservando observación, diagnóstico, diseño,
   evaluación y reglas de referencia visual. Los números de geometría antigua
   están desautorizados como planes nuevos.
2. **Visión:** JPEG de resolución máxima de lado 1024 y calidad 88, sin
   deformar la relación de aspecto, con `--vision-max-edge` que permite
   640, 768, 1024, 1280 para medir el coste real. `--focus-image` añade una
   imagen genuina y focalizada, provista desde el mismo origen; no genera
   píxeles ni inventa capturas. El tamaño del modelo y VRAM pueden condicionar
   la resolución definitiva: comparar, no asumir mayor = mejor.
3. **Contexto Qwen:** presupuesto de salida por defecto 2048 tokens,
   respuesta bajo JSON Schema, con registro del modelo y sus decisiones
   originales. Ajustar para 2B/4B sólo después de medir truncamiento y tiempo.
4. **Memoria histórica:** ranking determinista por problema actual a partir
   de `learning_cases.json` con ID de ejecución, artefacto y SHA de respuesta,
   más lecciones seguras del diario existente. No copiar coordenadas ni planes
   rechazados. `learning-casebook-proof.json` identifica el contexto usado.
5. **Aprender de fallos y de los candidatos:** al finalizar un A/B real,
   incluso con `SCREENSPACE_REJECTED`, guardar
   `pending-learning-case.json` con SHA del plan, respuesta Qwen original,
   imágenes reales, comparación, GLB y BLEND. Sólo es
   `PENDING_INDEPENDENT_VERIFICATION`; Qwen no se autoevalúa como aprobado.
6. **Memoria duradera revisada:** `curate_learning.py` exige que un análisis
   visual independiente señale exactamente el manifiesto pendiente y los SHA
   originales de las imágenes, la ejecución y el artefacto. Produce un
   **candidato** nuevo de `learning_cases.json`, nunca modifica el canónico
   silenciosamente. `ART_SOURCE_PASS` no implica `ART_APPROVED` ni Unity
   certificado; un `VISUAL_REJECTED` verificado también enseña qué no hacer.
   El revisor declarado debe comprobar procedencia en los logs reales: un
   JSON formalmente correcto no demuestra identidad humana ni calidad estética.

## Flujo de aprendizaje verificable

`referencia real + imagen actual + memoria de fallos` →
`Qwen elige decisiones originales` →
`Blender ejecuta el plan tipado` →
`render fijo antes/después + gate técnico` →
`registro pendiente con SHA` →
`revisión visual independiente y estudio del defecto` →
`candidato de caso comprobado` →
`revisión del cambio Git + incorporación a learning_cases.json` →
`el siguiente encargo recupera automáticamente casos pertinentes`.

**Esto es aprendizaje por memoria y retroalimentación, no fine-tuning.**
`weights_updated=false`. El curador nunca obtiene autoridad para firmar
`ART_APPROVED` sólo porque el JSON esté bien formado. La calidad del juego
requiere todavía A/B en Unity desde la cámara del jugador, revisión artística
independiente y, para certificación final, verificación en móvil.

## Contrato de curation externa

Un revisor distinto del Qwen autor debe crear un archivo JSON de revisión,
con `reviewer_role="INDEPENDENT_ART_REVIEW"`,
`result="VISUAL_REJECTED"|"ART_SOURCE_PASS"`,
`pending_sha256`, `before_sha256`, `after_sha256`, `run_id`,
`artifact_id`, `diagnosis` y `safe_lesson`.
Se usa sólo tras contrastar el artefacto y los dos renders originales.
Ejemplo de paso final **sin reemplazar el casebook canónico**:

```bash
python3 tools/art-direction/full-studio/curate_learning.py \
  --pending /evidence/cycle-1/pending-learning-case.json \
  --review /independent/art-review.json \
  --casebook tools/art-direction/full-studio/learning_cases.json \
  --out /evidence/learning-candidate.json
```

## Correcciones estructurales aún por certificar en el entorno real

- **Geometría de mallas:** la rama de validación profesional ya prueba
  inspección de `matrix_world` (+Z arriba), landmarks, caras de mallas
  heredadas, operadores tipados y re-inspección tras cambios topológicos.
  Estos cambios deben reconciliarse con el presente trabajo antes de producir,
  conservando la identidad de la fuente y las operaciones que Qwen elige.
- **Acceso amplio a Blender:** el estudio ya ofrece `bpy`, `bmesh`,
  modificadores, UV, shader nodes y un subconjunto de Geometry Nodes.
  No significa acceso certificado a TODA la interfaz (rigging, animación,
  simulaciones, pintura libre). Ampliar por fallos reales, no por número de
  nombres de herramientas. Blender puede hacer operaciones de bajo nivel
  usando geometría conectada y volver a inspeccionar antes de otro cambio.
- **Modelo mayor:** NO descargar 8B ni comprar nada por hipótesis.
  Medir primero CPU, RAM, GPU/VRAM disponibles en Windows, modelos instalados,
  tiempo y errores de los mismos casos con 4B y 8B (si existe localmente).
  Fijar misma referencia, fuente SHA, plan y revisión; comparar
  proporción de planes válidos, fallos de coordenadas/topología, tiempo y
  mejoras A/B. El mayor modelo no es automáticamente el mejor.
- **Mejora artística real:** no existen aún pruebas suficientes de mejora
  premium repetible de un asset en Unity. La aceptación requiere elegir UNA
  pieza congelada, comparar A/B a cámara real, corregir defectos de silueta y
  unión, y contrastar el candidato con la referencia, sin tocar bastión/mundo
  activos mientras el trabajo de producción esté pausado.

## Pruebas de infraestructura sin Windows compartido

```bash
python3 -m unittest discover -s tools/art-direction/full-studio/tests -p 'test_*.py' -v
python3 -m py_compile \
  tools/art-direction/full-studio/artist_loop.py \
  tools/art-direction/full-studio/learning_context.py \
  tools/art-direction/full-studio/curate_learning.py
```

Este trabajo no cambia el sistema de aprendizaje canónico de Bastión I
(`tools/art-direction/learning-evidence/cycle-hooks.mjs`); integrar ambos
catálogos por evidencia si coinciden las mismas piezas, sin crear departamentos
nuevos. Los éxitos técnicos son pruebas de infraestructura, no aprobación
artística.
