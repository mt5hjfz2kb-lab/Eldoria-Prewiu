# Eldoria — Fase 2: cierre técnico de entrada Qwen, aprendizaje y comparación

**Corte:** 22-10-2026. **Autoridad:** `main/AGENTS.md`, fuente viva y propietarios del registro. Presupuesto: 0 €. No se reabre ningún proceso artístico ni se modifican escenas Unity, APK, WebGL o modelos existentes.

## 1. Qwen: memoria obligatoria para nuevas vías, sin romper ejecuciones anteriores

- `tools/art-knowledge/run-artist.mjs` es la vía canónica de **nuevos ciclos Qwen**. Lee el catálogo de 54 fichas y el archivo de lecciones independientes; combina su selección con el inventario original de fuentes, que no debe perderse al introducir la memoria.
- Respeta `author_brief.py --inventory` (autor ya existente). La memoria y el inventario de asset deben conservar fuente, SHA de catálogo y de inventario, y el `artist-inference.json` de Qwen ha de confirmar el SHA exacto que realmente recibió.
- Un ciclo que solo prepara memoria informa `PREPARED_NOT_CONSUMED`; solo una inferencia real genera `EXACT_QWEN_INVENTORY_CONSUMED`. No basta con ver un fichero creado.
- `tools/art-knowledge/enforce-future-workflows.mjs` y el control de planificación impiden que **nuevos** workflows usen el ejecutor directo sin el adaptador de memoria. El catálogo inicial de 241 workflows preexistentes se congela como excepciones históricas; no se reescriben las rutas de Qwen reservadas por otros responsables.
- **Regla de aprobación de nueva obra:** `tools/check-art-production-governance.mjs` exige una prueba `EXACT_QWEN_INVENTORY_CONSUMED` con identidad y SHA para pasar a estados `ART_APPROVED` y posteriores en nuevos trabajos. Los cuatro trabajos que ya estaban en validación quedan expresamente protegidos.
- **Conservación automática:** el guard de nuevos workflows exige también `actions/upload-artifact` del `.cycle-feedback.json`, para no perder el registro al terminar el runner.
- **Limitación comprobable:** la regla se aplica a la planificación y a los nuevos workflows, no reescribe automáticamente las rutas antiguas. La migración de flujos Qwen preexistentes solo puede hacerse coordinando titularidad y comparando resultados. No declarar que TODAS las ejecuciones existentes están obligadas todavía.

### Ejemplo (autor existente, sin iniciar Unity, sin Tripo)

```sh
node tools/art-knowledge/run-artist.mjs \
  --request pipeline/art-production-requests/<WORKSTREAM>.json \
  --memory-prefix /tmp/unique-cycle \
  --source-inventory /tmp/inventario-original-del-asset.txt \
  --run -- \
  --reference /ruta/referencia-real.jpg \
  --unity-before /ruta/captura-unity-real.png \
  --target-object NOMBRE_REAL_DEL_OBJETO \
  --subject-kind creature --cpu-compact \
  --model qwen3-vl:2b-instruct --out /tmp/qwen-output-real
```

Sin `--run` realiza preparación solamente. La clave es que **no sustituye el archivo de la fuente** por instrucciones de memoria: conserva ambos contextos y prueba qué recibió el autor.

## 2. Aprendizaje: registro automático y filtro independiente

- `tools/art-knowledge/cycle-feedback.mjs` se invoca desde `run-artist.mjs --run` tras cada intento y produce un **registro inmutable del ciclo**, sea exitosa o fallida la inferencia. No utiliza la calidad del resultado para autoconceder etiquetas artísticas.
- Cuando existen `before`/`after` auténticos de la misma cámara, un revisor independiente puede emitir un informe con SHA, cámara, luz, resolución y diagnóstico. El comando `cycle-feedback.mjs review` lo comprueba y deja la revisión en estado `INDEPENDENT_EVIDENCE_RECORD_READY`.
- `tools/art-knowledge/reviewed-ledger.mjs` incorpora ese evento al catálogo auxiliar **solo después de cotejar de nuevo** las imágenes, la inferencia y el informe. La nueva entrada contiene lección concreta, siguiente acción, prueba y condición positiva o negativa, **sin `art_approved`**.
- `pipeline/art-knowledge/reviewed-lessons-v1.json` empieza vacío porque no hemos generado una nueva obra ya revisada en esta intervención. El selector recupera automáticamente sus entradas una vez que se consolidan en `main`.

**Límite importante:** la captura visual y su revisión artística independiente no pueden fabricarse por una automatización. El ciclo de **registro de intentos** es automático; la **admisión como conocimiento contrastado** está condicionada a esa evidencia y a consolidar el fichero mediante el procedimiento de Git existente. No se envían evaluaciones inventadas a la memoria.

## 3. Prueba comparativa real, mismo problema, sin gastos de servicio

`.github/workflows/eldoria-art-knowledge-controlled-ab.yml` ejecuta en **Linux GitHub-hosted** un ensayo Qwen visual de una sola pieza: el Engendro ya existente. La prueba recupera una fotografía auténtica de su fuente y una captura del videojuego; ambas se convierten a 400 px para CPU dejando SHA de origen.

Compara tres rutas, sin tocar ninguna escena:
- **Sin Qwen:** la política conocida evita una edición sin prueba; consume cero inferencias.
- **Qwen sin memoria histórica:** misma fotografía, modelo, temperatura y consigna, sin las fichas de experiencia.
- **Qwen con memoria:** el mismo modelo y fotografía, añadiendo solo las lecciones comprobadas de Engendro, animación y fallos de escultura.

Se archivan **segundos de inferencia, tokens, respuesta literal, verificaciones de contexto y SHA** por brazo. Las sugerencias no se llevan automáticamente a Blender y los datos jamás autorizan `ART_APPROVED`.

**Resultado REAL:** [run 39687896808](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39687896808) terminó con dos respuestas Qwen genuinas, ambas con propuesta genérica `assign_material` y **ninguna mejora artística demostrada**. Véanse [resultados completos](../evidence/eldoria-art-memory-real-ab/RESULT.md) y [recepción estructurada](../../pipeline/art-knowledge/benchmarks/engendro-qwen-memory-ab-20261021.json). Los tiempos 118,909s/14,568s no se pueden comparar por el calentamiento del modelo. **Qué puede demostrar:** que los contextos se pudieron contrastar con mismas imágenes y sin gasto de servicio; no una mejora gráfica. **Qué NO demuestra:** superioridad gráfica; eso requeriría aplicar ambas propuestas a copias del mismo modelo, render A/B en Unity y evaluación independiente. No asignar ahorro porcentual sin ese siguiente experimento de calidad.

### Criterio de cierre

- CI verde del selector, filtros, adaptador, salida/entrada Qwen, registro automático, lector de lecciones y rechazo de pruebas falsas.
- Ensayo con Qwen real solo se declarará completado cuando existan **dos inferencias auténticas y el informe íntegro** en Actions. Si el modelo Linux falla o caduca el artefacto, la comparación queda inconclusa; no se sustituye por un resultado simulado.
- La última frontera de **producción completa** sigue siendo migrar los workflows antiguos al nuevo adaptador cuando dejen de pertenecer a ejecutores activos, y validar si la calidad visual/velocidad supera realmente la alternativa sin Qwen.
