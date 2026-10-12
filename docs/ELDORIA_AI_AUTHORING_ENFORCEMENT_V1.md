# ELDORIA — Implantación verificable de autoría artística IA V1

**Estado: código y pruebas de contrato preparados en rama aislada; falta prueba en el Windows del artista y verificación visual real.** Alcance: Issue #34. Cero créditos de pago. No modificar trabajos concurrentes de Bastión I ni el runner ocupado.

## Proceso obligatorio para todos los siguientes encargos artísticos

1. **Diagnóstico en contexto:** comparar referencia aprobada y captura genuina de la **escena jugable** en Unity. Revisar inventario de los ocho assets originales, Hero V3, bibliotecas existentes y recursos CC0 con licencia; decidir explícitamente REUSE / MODIFY / NEW. No generar assets aislados sin demostrar que mejoran la escena.
2. **Diseño Qwen real:** `author_brief.py` llama a Ollama **local** con dos imágenes y `tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md` pasado como rol `system`. El modelo decide silueta, operaciones espaciales, materiales y racional; no se limita a elegir parámetros `noise_amplitude`. La respuesta JSON lleva hash del perfil, identidad del modelo, diagnósticos e intención artística.
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

## Ensayo estructural avanzado y preflight barato (Issue #34)

El diseño **no** depende ya solamente de amplitud/ruido. `sculpt_path` permite al artista escoger una sucesión de anclajes 3D y desplazamientos con interpolación continua para romper una silueta rígida; `bridge_boundary_loops` añade caras reales entre dos anillos de aristas abiertos de la MISMA malla, conservando la fuente mediante una variante .blend. La segunda operación exige índices de frontera auténticos: si no hay topología compatible, Qwen no debe inventarla; replantear la malla o crear una conexión diseñada explícitamente como geometría nueva.

Antes de ejecutar Qwen, inspeccionar el origen congelado, **sin alterar su contenido**, por ejemplo:

```bash
blender -b --python-exit-code 1 --python tools/art-direction/ai-authoring/execute_artist_plan.py -- \
  --source /sources/valoria-frozen.blend --out /proof/bastion-1 \
  --inspect --inspect-target-object Terrain
python3 tools/art-direction/ai-authoring/author_brief.py \
  --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
  --unity-before /proof/unity-before.png \
  --mesh-context /proof/bastion-1/mesh-context.json \
  --target-object Terrain --target-mode existing --out /proof/bastion-1
node tools/art-direction/ai-authoring/verify-artist-cycle.mjs \
  --preflight-plan /proof/bastion-1/artist-plan.json Terrain existing
```

Solo tras `ART_PLAN_PREFLIGHT_PASS` iniciar Blender de edición y Unity. Añadir `--expected-source-sha256 SHA256_DEL_BLEND_CONGELADO` al ejecutor para impedir trabajar sobre una fuente que haya cambiado. El manifiesto incluye nombre exacto, magnitudes e índices reales de anillos abiertos; la inferencia incluye su hash. **Un preflight PASS no sustituye Blender real:** los bordes se verifican una segunda vez dentro de Blender y se rechazan caras inválidas o aristas no abiertas.

Para permitir `ART_APPROVED`, el `artist-cycle.json` debe contener una referencia `authorship.plan_file` con ruta y SHA256 del `artist-plan.json` producido ANTES del modelado. El verificador coteja ese plan con las decisiones registradas en `artist-inference.json`, las acciones de `artist-execution.json`, los hashes de fuente y productos .blend/.glb, la referencia, las capturas Unity y el perfil `system`. Una diferencia se rechaza, incluso si un archivo aislado tiene un SHA256 formalmente válido. **Estos cruces aumentan la resistencia a pruebas simuladas, pero un registro manipulado intencionadamente aún requiere contrastarse con logs reales del runner y revisión independiente.**

Las fases registran `inference_seconds`, `elapsed_seconds` por operación y duración de ciclo; documentar después causas raíz, correcciones, mérito visual y costes de móvil en el diario existente. No rellenar A/B, importación, gameplay ni veredicto de dispositivo con evidencias inventadas. Las pruebas de simulación solo verifican contratos.

**Ejecución pendiente y propiedad protegida:** no lanzar la prueba real del terreno de Bastión I mientras otro workstream conserve `valoria-player-scene` y `windows-runner-heavy`; no tocar esa rama ni `main` hasta reconciliar claims y checks. El núcleo puede pasar Linux CI mientras el resultado visual se mantiene `VISUAL_REJECTED` o `AUTHORING`, nunca `ART_APPROVED`.
