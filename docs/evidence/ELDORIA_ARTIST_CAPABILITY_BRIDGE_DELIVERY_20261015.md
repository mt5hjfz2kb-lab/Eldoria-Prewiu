# Eldoria — Recuperación de las cuatro capacidades del artista

**Fecha:** 15 de octubre de 2026. **Ámbito:** estabilización aislada sobre el estudio Qwen/Blender existente. **Fuente inicial:** `diagnostic/valoria-ai-eyes-brain-20261015` @ `184d5bcffbcaaca62eff27e3e701e8f5fcfed391`. **Trabajo:** `diagnostic/eldoria-artist-capability-bridge-20261015`. **Coste:** 0 €.

## Resultado ejecutado, sin confundirlo con arte aprobado

Se han preparado cambios concretos en el estudio existente `tools/art-direction/full-studio`, sin sustituirlo y sin tocar fuentes de modelo 3D, Unity, escena, juego, runner Windows ni la rama artística de otro propietario. El CI Ubuntu de esta rama comprueba importabilidad, sintaxis y regresiones unitarias, pero **no** prueba una mejora artística.

### 1. Recuperar habilidades arquitectónicas

`artist_capabilities_v1.py` introduce un catálogo de **intenciones arquitectónicas** que utiliza solo operaciones que ya existen en `contract.OPS` y `blender_toolbox.run_action` (mampostería y parapeto; contrafuerte y apoyo; huecos; piedra erosionada; materiales; reutilización). `artist_loop.py` lo entrega en el prompt Qwen del modo profesional completo. **Qwen sigue eligiendo objetos, operaciones y valores**: no se programó una torre ni se inventó un operador `geometry`.

### 2. Ojos y manos unidos mediante resultados auténticos

El `artist_loop.py` añade al análisis visual los campos de intención del plan real; `visual_intent_payload` verifica que existen imágenes PNG/JPEG auténticas de referencia/antes/después, su resolución antes-después, SHA-256 y plan validado. La crítica Qwen debe explicitar si la geometría visible coincide con la intención y **no puede promocionar** resultados que ella misma marque como fallidos, dudosos o diferentes. Esta revisión del modelo es un filtro temprano, **no** el revisor independiente ni `ART_APPROVED`. Las imágenes se envían realmente al modelo visual en la crítica existente.

### 3. Herramientas reales de Blender, sin fingir interacción libre

Las familias de operaciones disponibles (incluidos `sculpt`, `extrude`, `inset`, `bevel`, `modifier`, `material`, `uv`, `native_operator`, nodos de shader/geometría y reutilización) se exponen a Qwen como capacidades actuales del estudio, sin scripts arbitrarios. El bucle profesional existente inspecciona fuente, ejecuta acciones tipadas, reobserva el resultado y vuelve a decidir en el ciclo siguiente.

`transport_parity_v1.py` permite comprobar objetivamente una **misma propuesta original** con `direct` y `blendermcp`, misma fuente y cámara, validando SHA del addon, ejecución física, efectos geométricos y PNG/GLB reales. No presupone que MCP mejore estética. **No se ha ejecutado este A/B en Windows** en esta implementación; la línea artista y el recurso PC compartido no se han reclamado.

**Límite explícito:** disponer de un catálogo y de transporte MCP **no equivale** a acceso arbitrario a la totalidad de Blender ni a la experiencia de un artista 3D profesional. La conexión interactiva multi-turno de Qwen como cliente nativo de todas las herramientas MCP y la efectividad artística de las habilidades siguen por demostrar. No desactivar validadores ni dar ejecución Python arbitraria al modelo para simularlo.

### 4. Separar integración y calidad

`strict_preflight_plan` usa el validador canónico antes de Blender; `check_source_environment` detecta discrepancias de contratos y módulos antes de consumir inferencias. `failure_category` diferencia `INTEGRATION`, `SCHEMA_OR_PLAN`, `VISIBILITY`, `SPATIAL_SAFETY`, `ART_QUALITY` y `EXECUTION`, y el `artist_loop` añade esta atribución a los rechazos. Un `CI SUCCESS` solo es evidencia técnica.

El workflow independiente `.github/workflows/eldoria-artist-capability-bridge-static.yml` corre en **ubuntu-latest**, sin Windows/Blender/Ollama, e inspecciona únicamente sintaxis y pruebas. No sustituye al CI Blender que pertenece al estudio canónico.

## Pruebas y evidencias

- GitHub Actions: [primera prueba estática aprobada #38763104332](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38763104332); prueba posterior del comparador y 13 casos en la rama.
- `tests/test_artist_capabilities_v1.py`: catálogo basado en operadores disponibles, rechazo de `geometry`, no autoaprobación, simulación de imágenes reales con Pillow, rechazo de imagen falsa/resolución, intención discordante, clasificación de fallos, invariantes de comparador.
- `transport_parity_v1.py` no toca el original y no se activa automáticamente; para usarlo se debe escoger **un mismo plan previamente validado** y una fuente congelada, más BlenderMCP local SHA validado, con exclusividad del runner.

## Paso de integración y aceptación restante

1. El propietario de la rama diagnóstica revisa e integra la PR de esta rama sin mezclarla con cambios nuevos no verificados. El trabajo en la rama artística canónica continúa siendo de su propietario; **NO** aplicar parche directo a ella ni promover a main por un CI de sintaxis.
2. Con el runner libre y autorización vigente, ejecutar **el mismo plan** una vez por transporte, comparar los resultados y guardar el manifiesto. Cualquier divergencia se corrige en el transporte, no en la creatividad.
3. Ejecutar un ciclo Qwen→Blender con habilidades arquitectónicas e imágenes actuales, seguido de evaluación independiente A/B real. Si se ve peor se rechaza; no se hereda el candidato.
4. Solo con tres mejoras consecutivas aceptadas en misma torre, última nota >=8/10 y Unity A/B final se puede considerar `CALIBRATION_VALIDATED`, conforme a `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`. Bastión I, 4X, Unity producción y publicación permanecen pausados.

**Estado de entrega:** ingeniería de integración preparada y validación estática; integración en el propietario, comparación física y calidad artística no certificadas. No se atribuye entrenamiento nuevo ni aprendizaje de pesos a Qwen.
