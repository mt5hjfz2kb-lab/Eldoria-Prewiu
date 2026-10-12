# ELDORIA — Implantación verificable de autoría artística IA V1

**Estado: código y pruebas de contrato preparados en rama aislada; falta prueba en el Windows del artista y verificación visual real.** Alcance: Issue #34. Cero créditos de pago. No modificar trabajos concurrentes de Bastión I ni el runner ocupado.

## Proceso obligatorio para todos los siguientes encargos artísticos

1. **Diagnóstico en contexto:** comparar referencia aprobada y captura genuina de la **escena jugable** en Unity. Revisar inventario de los ocho assets originales, Hero V3, bibliotecas existentes y recursos CC0 con licencia; decidir explícitamente REUSE / MODIFY / NEW. No generar assets aislados sin demostrar que mejoran la escena.
2. **Diseño Qwen real:** `author_brief.py` llama a Ollama **local** con dos imágenes y `ARTIST_SYSTEM.md` pasado como rol `system`. El modelo decide silueta, operaciones espaciales, materiales y racional; no se limita a elegir parámetros `noise_amplitude`. La respuesta JSON lleva hash del perfil, identidad del modelo, diagnósticos e intención artística.
3. **Autoría Blender:** `execute_artist_plan.py` lee **sólo acciones tipadas permitidas**, sin ejecutar Python arbitrario proporcionado por Qwen: `sculpt_region`, `reprofile_crest`, `construct_mesh`, `assign_material`, `place_existing_asset`. Admite ejecución directa Blender para recuperación y transporte BlenderMCP con addon y SHA256 fijado. Registra el transporte real, acciones ejecutadas, diferencia geométrica y resultados GLB/.blend. Nunca sobrescribir originales congelados.
4. **Unity y evaluación:** importar GLB al Unity 6000.3.23f1 del juego, corregir UV/normal/PBR/iluminación/contactos y generar comparación Unity A/B a la misma cámara, luz, resolución y preset URP. Revisión visual independiente: si el resultado no mejora materialmente, `ART FAIL`. Tras dos correcciones marginales, replantear **la estructura** del modelo en lugar de añadir ruido. Mantener gameplay, cámara móvil, progresión y guardado.
5. **Evidencia trazable:** `artist-cycle.json` registra modelo, rol system, hash del perfil/log de inferencia, referencia/capturas Unity, explicación de autoría previa a modelar, acciones previstas/ejecutadas y cambios medidos, hashes de fuente y GLB/.blend, importación Unity, licencia y revisión visual/QA móvil. `verify-artist-cycle.mjs` comprueba archivos y SHA256 y niega ART_APPROVED si sólo hay presets, si no hubo cambios ejecutados o si faltan las pruebas. El sistema **no confunde un esquema con la calidad visual humana**.
6. **Gobernanza canónica:** se ha ampliado el `tools/check-art-production-governance.mjs` existente, sin crear departamento nuevo. Las solicitudes futuras requieren `ai_authorship_policy: required`; quedan exentos explícitamente sólo los dos workstreams abiertos antes de esta implementación para no romperlos. Los estados artísticamente aprobados precisan `artist_cycle_path` que pase el validador. `tools/plan-art-production.mjs` deriva automáticamente al artista IA y Blender ante déficits geométricos, incluso cuando el perfil sea `environment_composition`.
7. **Memoria artística:** reutilizar `tools/art-direction/ARTIST_LEARNING_JOURNAL.md` cuando se pueda conciliar su rama: defecto→hipótesis→edición→captura Unity→veredicto; no fingir reentrenamiento del modelo.
8. **Sin regresiones:** mantener runner Windows, escenas, `main`, Pages y otros archivos fuera de esta rama. Aceptación exige pruebas de sintaxis, tests positivos/negativos de evidencias, una inferencia real Ollama+Qwen, acción Blender/BlenderMCP real, GLB importado a Unity y mejora artística independiente. No hay autorización para gastos ni publicación prematura.

## Integración efectiva de la memoria y del inventario

- `tools/art-direction/ARTIST_LEARNING_JOURNAL.md` está preservado en esta promoción y **se inyecta automáticamente** como contexto de errores y aciertos recientes en el encargo Qwen; cada inferencia registra su SHA256. No equivale a reentrenar pesos.
- Si se omite `--inventory`, `author_brief.py` consulta rutas GLB reales del repositorio y prioriza recursos V3/Hero/terreno para decidir REUSE/MODIFY/NEW. Las licencias y fuentes deben verificarse antes de reutilizar o incorporar un recurso.
- `--target-mode existing` (predeterminado) prohíbe `construct_mesh` sobre mallas existentes; `--target-mode new` obliga a iniciar con `construct_mesh` para crear la nueva malla. Ambas rutas preservan originales congelados.
- La batería de evidencia incluye **14 pruebas positivas/negativas**, rechazo de artefactos falsos por firma de formato, symlinks externos y discordancia entre la decisión artística y la operación realmente ejecutada. Las pruebas de sintaxis/contrato no sustituyen el ejercicio genuino Qwen + Blender + Unity.

## Ejecución cuando se libere el runner y el propietario de escena

```bash
python3 tools/art-direction/ai-authoring/author_brief.py \
  --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
  --unity-before /ruta/captura-real-unity.png \
  --target-object 'Nombre exacto de malla Blender' \
  --inventory /ruta/inventario-licencias.txt \
  --out /ruta/trabajo-artista
blender -b --python-exit-code 1 --python tools/art-direction/ai-authoring/execute_artist_plan.py -- \
  --source /ruta/original-congelado.blend \
  --plan /ruta/trabajo-artista/artist-plan.json \
  --out /ruta/trabajo-artista/resultado \
  --transport blendermcp --mcp-addon /ruta/addon-validado.py --mcp-sha256 SHA256_REAL
node tools/art-direction/ai-authoring/verify-artist-cycle.mjs \
  /ruta/trabajo-artista/artist-cycle.json /ruta/repositorio
```

**Pruebas de bajo coste que no necesitan Windows:**
`node --test tools/art-direction/ai-authoring/verify-artist-cycle.test.mjs`,
`python3 -m py_compile tools/art-direction/ai-authoring/author_brief.py tools/art-direction/ai-authoring/execute_artist_plan.py`,
`node tools/check-art-production-governance.mjs`.

**Primera prueba artística obligatoria:** conservar Bastión I funcional, diseñar con Qwen una geometría de acantilados/terrazas orgánicos y continuos, editarla con BlenderMCP, integrar en Unity y comparar las capturas reales. Un render técnicamente correcto sin mejora visible NO es entrega. Registro de resultados en la Issue #34 y el diario artístico, nunca como falsa afirmación de ART PASS.

**Limitación técnica honesta:** el validador prueba trazabilidad y falta de señales obvias de aprobación ficticia; no puede demostrar por sí solo si las imágenes o logs son honestos, ni sustituir el juicio visual. La verificación cruzada con el workflow real y el motor Unity sigue siendo imprescindible.
