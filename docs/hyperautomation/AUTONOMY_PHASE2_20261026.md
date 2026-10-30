# Eldoria — continuidad de trabajos activos y memoria real (26 octubre 2026)

> Alcance: integración incremental sobre la infraestructura canónica; no introduce nuevo orquestador, Work perpetuo, PR paralelo, gasto ni publicación. `AGENTS.md`, `pipeline/active-workstreams.json` y los propietarios actuales mantienen toda la autoridad.

## Entrega técnica comprobable

### 1. Vigilancia de misiones vivas con referencia exacta

El workflow ya existente `.github/workflows/mission-continuity-guard.yml` funciona aproximadamente cada hora, y su política `pipeline/mission-continuity-policy-v1.json` incluye ahora **observación report-only** de:
- `eldoria-world-region-1-canon-20261022`: propietario registrado en `main`, pruebas observadas de la rama `feature/world4x-region1-canon-20261022`, exclusivamente `World4X Region 1 canonical candidate source and PlayMode QA` y solo para HEAD exacto del candidato.
- `eldoria-tripo-art-brain-phase2-20261021`: propietario registrado en `main`, consulta específica del workflow `tripo-art-brain-preflight.yml`. Ejecuciones sobre un SHA anterior aparecen como `STALE_SOURCE_EVIDENCE`, **no** como aprobación o prueba del HEAD de `main`.

Las tres pistas históricas siguen disponibles y respetan el registro `active`; no se han resucitado trabajos viejos. Las pistas nuevas nunca crean ni actualizan issues de recuperación, no envían órdenes y no reservan recursos. Un propietario nulo o una pausa expresa impiden inferir continuidad.

El clasificador distingue `EXTERNAL_RUNNING`, `WAITING_EVIDENCE`, `NEEDS_RECOVERY`, `GITHUB_APPROVAL_REQUIRED`, `STALE_SOURCE_EVIDENCE`, `OWNER_PAUSED` y falta de titular. Éxito técnico no es cierre, arte aprobado ni prueba física. Se limita el historial examinado; las ventanas parciales **no** se convierten en ausencias verificadas.

**Prueba real del guard:** ejecutar `node --test tools/mission-continuity/core.test.mjs tools/mission-continuity/nightly-core.test.mjs`; comprobar después el workflow `Eldoria Mission Continuity Guard` y su artifact `mission-continuity-report-RUN_ID`. El informe nombra run, HEAD, rama, próximo paso seguro y evidencias pendientes.

### 2. Preparación de memoria obligatoria para nuevos prompts artísticos pertinentes

Para una **nueva** tarea de autoría con lecciones verificadas del objetivo `bastion1`, el responsable construye el prompt de entrada **sin sobrescribir** los archivos previos:

```bash
node tools/art-direction/learning-evidence/build-prompt.mjs \
  --base-prompt /ruta/encargo-artista.txt \
  --catalog tools/art-direction/learning-evidence/observations.json \
  --output /ruta/candidato-nuevo/prompt-enviado.txt \
  --target bastion1
```

El resultado son cuatro archivos: el prompt completo, `.history.txt`, `.history.provenance.json` y `.memory-verification.json`. Comprueba hashes del catálogo, la referencia vigente, el número de casos y que las lecciones se insertaron exactamente en el prompt. Si el catálogo no ofrece lecciones del objetivo o los archivos ya existen, **se rechaza**, sin inventar memoria ni destruir evidencias.

Este mecanismo **no** ejecuta Qwen, Tripo o Blender; no prueba que el modelo haya leído el texto y no puede aprobar el arte. Solo el titular del pipeline creativo está autorizado para conectar estos archivos a la llamada real y conservar los recibos de ejecución. Al terminar una revisión A/B genuina, el titular puede generar una lección `pending_verification` con `cycle-hooks.mjs stage`, que no actualiza automáticamente la memoria canónica.

### 3. Producción artística y objetivos aún abiertos

- El `PR #100` mantiene su titularidad y reserva Windows. Se conserva la revisión artística de las tres cámaras, la fuente editable y los criterios de mejoras perceptibles del gate World4X; ninguno ha sido modificado desde esta intervención operativa.
- Para convertir la validación del prompt en obligación dentro de Qwen/Tripo, el **mismo propietario del trabajo** debe conectarla exactamente antes de su inferencia y verificar el prompt realmente consumido. Hacerlo desde otro propietario violaría el registro de scopes.
- Un agente capaz de reparar código/arte después de cerrar un chat sigue **sin ejecutor persistente demostrado**. GitHub Actions reanuda jobs deterministas autorizados, no reabre sesiones Work ni toma decisiones creativas por sí solo. Implantar esa capacidad requeriría un ejecutor compatible, permisos explícitos, controles contra duplicación y evaluación de recursos/costes.
- No hay nueva autorización para hacer merge, publicar, consumir Tripo o cambiar arte/licencias.

### 4. Resultado mensurable y siguientes dependencias

Contabilizar por cada workstream: interrupciones detectadas, recuperaciones permitidas, reintentos evitados, esperas reales del runner, pruebas pertinentes que pasan, cantidad de prompts con memoria validada **y** trazas reales posteriores de inferencia. Los porcentajes de ahorro y las notas de calidad deben basarse en medidas antes/después, nunca en el número de workflows instalados.

Este incremento es **continuidad observada + herramientas de memoria verificadas**, no hiperautomatización autónoma completa.
