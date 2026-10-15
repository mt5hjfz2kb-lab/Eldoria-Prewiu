> **ESTADO VIGENTE 15-10-2026:** recuperación operativa y comparación Qwen selectiva COMPLETADAS. Run 38726028141: sin memoria 32/32, completa 31/32, selectiva 32/32; 96 inferencias reales y datos brutos verificados/preservados en Git. Cero regresiones selectivas de decisión en esta muestra, no superioridad sobre baseline ni autorización artística. El checkpoint actual sustituye los antiguos siguientes pasos BLOCKED/RUNNING que quedan como historia. Producción pausada.

# ELDORIA — Inicio de conversación: descubrimiento técnico obligatorio

**Finalidad:** nuevos chats normales/Work identifican automáticamente las rutas ya existentes. No inventar accesos ni exigir al propietario que repita el inventario. Este contrato de infraestructura NO autoriza a levantar la pausa de producción de 2026-10-14.

## Orden de arranque (sin gastar y sin tocar Windows)

1. Usar las herramientas conectadas de GitHub, cuando estén disponibles, para leer el **main/AGENTS.md vigente** (incluida la suspensión), HEAD de la rama encargada, las cabeceras vivas de `SESSION_HANDOFF.md` y `PROJECT_STATE.md`, y solo `active` de `pipeline/active-workstreams.json`. Si un acceso falla, registrar qué herramienta y permiso faltan, y probar alternativas autorizadas. No dar por hecho que un chat nuevo hereda credenciales o un terminal Windows.
2. Consultar los workflows existentes (`.github/workflows/`) y los últimos runs/jobs **del SHA pertinente**. Hay un runner local Windows con Unity y rutas históricamente probadas a Blender, Ollama y BlenderMCP; declararlo `históricamente verificado`, no `online ahora`, hasta ver el job actual.
3. Con checkout y Node disponibles, usar `node tools/eldoria-agent-preflight.mjs` para un diagnóstico barato y de solo lectura del **equipo en el que se ejecute**. Si únicamente hay conector GitHub, inspeccionar el script y las rutas mediante GitHub; no fingir que el script se ejecutó en Windows.
4. Nunca concluir que `127.0.0.1:11434` inaccesible desde Linux/ChatGPT implica que Ollama no está instalado en el PC. La evidencia de 2026-10-14 demostró que Ollama puede estar instalado y necesitar arranque autorizado bajo demanda en el runner Windows.
5. Distinguir cinco estados: `declarado en workflow`, `observado en ejecución anterior`, `accesible ahora desde el chat`, `comprobado en runner Windows en la ejecución actual`, `bloqueado`. **No son intercambiables.**
6. Elegir solo la ruta pertinente: GitHub connector para commits y lectura; Actions + self-hosted Windows para Unity o Blender/Ollama cuando se autorice; Ubuntu hosted para validaciones baratas; BlenderMCP cuando la edición artística lo requiera. No rebajar el alcance artístico a scripts procedurales solo porque el puente no sea visible al inicio.
7. Si falta ejecución/dispatch en el conector ChatGPT, verificar push-trigger existente o la alternativa autorizada, sin inventar endpoints ni modificar workflows productivos para sortear una prohibición. Si el recurso está reservado o la producción pausada, detener ejecución productiva.
8. Presupuesto 0 €; no instalar, descargar modelos, lanzar procesos de producción ni gastar créditos durante un descubrimiento. No tocar reservas ni trabajos de terceros.
9. **Regla permanente de calidad artística (2026-10-15):** antes de cualquier inferencia Qwen/Blender de Eldoria, leer la versión vigente de `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` junto con `main/AGENTS.md`. Verificar que el actual autor del artista conoce el contrato y que cualquier `ART_APPROVED`/`CALIBRATION_VALIDATED` se registrará con imágenes originales A/B, SHA y revisión independiente en `pipeline/art-visual-approvals/` usando el gate técnico de evidencias. Si el chat está trabajando en una rama antigua, la falta de ese archivo allí NO deroga la norma de `main`. No alterar workstreams ajenos ni levantar la pausa.


## Rutas/evidencias existentes (históricas, no autorización actual)

- **GitHub:** `mt5hjfz2kb-lab/Eldoria-Prewiu`. Las herramientas del chat deben comprobarse **en cada sesión**.
- **Unity Windows:** `.github/workflows/unity-slice.yml` declara `runs-on: [self-hosted, windows, unity-6000-3-23f1]` y usa `UNITY_EDITOR_PATH`. Unity 6000.3.23f1 en entorno del runner, sujeto a disponibilidad real y a pausa.
- **Blender PC:** evidencia Windows nativo Blender 5.2.2, job `38606958074`. Prueba de 26 operaciones: `38607499677`.
- **Ollama PC:** modelo ya instalado, Ollama 0.40.2; job `38609051105` detectó instalación pero sin listener. Job `38609357863` levantó el servicio efímeramente sin descarga. **No asumir que permanece iniciado.**
- **Qwen+Blender mismo PC:** `38610802289` probó inferencia real y edición de malla heredada. **BlenderMCP real:** `38612645467` probó el addon, con carga temporal; no implica que esté habilitado en cada sesión.
- **Fuente de estas evidencias:** `docs/ELDORIA_STABILIZATION_HANDOFF_20261014.md`; verificar fecha/sha antes de reutilizarlas.
- **GitHub Desktop:** instalado según propietario, pero no constituye interfaz remota directa del chat. Para automatización utilizar GitHub o la ruta Windows verificada.

## Resultados permitidos

- `CONNECTED_NOW`: comprobación actual directa en el contexto correcto.
- `KNOWN_CONFIGURED`: workflow/instalación o éxito histórico, requiere prueba actual para ejecutar.
- `UNAVAILABLE_IN_THIS_SESSION`: acceso directo ausente, después de comprobar alternativas.
- `PAUSED_BY_OWNER`: aún puede inspeccionarse; queda prohibida producción.
- `BLOCKED`: describir error exacto, intento de ruta alternativa y siguiente paso seguro.

`tools/eldoria-agent-preflight.mjs` **no** abre conexiones remotas de ChatGPT, no hace escrituras y no certifica runners a distancia. Su utilidad es evitar deducciones falsas en el entorno local. Ningún cambio en esta carpeta reinicia producción ni modifica el juego.

## Continuidad obligatoria y criterio de cierre (2026-10-15)

- Una comprobación estática, un commit, un archivo preparado o un workflow lanzado son **hitos**, no finalización. El agente continúa con el siguiente paso autorizado en la misma sesión, sin pedir al propietario que repita «continúa».
- Antes de cualquier cierre, registrar de forma concisa: rama y HEAD comprobados, objetivo verificable, último resultado medido, paso pendiente exacto, responsable/recurso, restricciones vigentes y evidencia enlazada. Consultar el handoff existente; no crear un sistema paralelo.
- En sesiones nuevas, recuperar primero el trabajo incompleto de la rama asignada. No repetir tests ya acreditados sobre el mismo SHA ni convertir una pausa artística en prohibición de diagnóstico autorizado.
- Si una operación recibe una denegación explícita de seguridad, **no** repetirla con nombres, rutas, API o herramientas equivalentes para eludir el control. Conservar evidencia, continuar únicamente trabajo independiente permitido y concretar qué autorización o intervención externa falta. No afirmar que el bloqueo ha desaparecido.
- No presentar «trabajo terminado» hasta obtener el resultado de aceptación de la misión. Si hay bloqueo genuino, distinguir claramente avance parcial y validación pendiente. Estas reglas no amplían permisos ni autorizan usar Windows o reanudar arte.

## Recuperación reproducible y estado estructurado (2026-10-15)

El siguiente bloque es el checkpoint **dentro de este arranque existente**, no un segundo registro. Actualizarlo tras una interrupción relevante; conservar el informe diagnóstico y los artefactos originales. `SESSION_HANDOFF.md` y `PROJECT_STATE.md` están reservados por otro workstream y sus cabeceras antiguas no sustituyen el HEAD de la rama diagnóstica.

1. Descubrir el inventario ACTUAL de herramientas de la sesión; llamar una lectura real de GitHub. Inventario ≠ conexión verificada. Nunca dar por herendada una capacidad.
2. Leer este archivo desde `main`, la orden vigente en `main/AGENTS.md`, HEAD vivo de la rama del checkpoint y el informe diagnóstico de ESA rama. Un 404 del bootstrap en una rama antigua exige consultar `main`, no reconstruirlo.
3. Con checkout, ejecutar `node tools/eldoria-agent-preflight.mjs --root RUTA_REPO`. Opcional: `--tool-inventory RUTA_JSON` recibe un array JSON de nombres de herramientas actuales (o `{"tools":[...]}`); no tokens ni credenciales. Emite automáticamente pendiente, diferencias del inventario y estado de entradas. Sin checkout puede recuperarse el mismo checkpoint por conector y leer los blobs exactos.
4. Comparar SHA de entrada ANTES de repetir gates. Solo reutilizar el alcance de la prueba acreditada si la entrada coincide; fuente ausente o distinta requiere evaluación focalizada, nunca inventar PASS ni ejecutar todo de nuevo.
5. Consultar `GET /repos/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs?branch=diagnostic%2Fvaloria-ai-eyes-brain-20261015&per_page=30` y los `jobs`/`artifacts` pertinentes mediante GitHub. Distinguir job completado, en cola y ejecutándose. La lista `/actions/runners` fue rechazada por el conector como endpoint no permitido: no equivale a runner offline.
6. Para Windows, el último método real fue PowerShell del workflow A/B: Git preinstalado en GitHub Desktop → checkout → evidencia congelada → Ollama instalado (arranque solo si necesario, en el mismo paso) → Qwen3 14B → artefacto aun con gate rojo. No extraer credenciales del PC ni crear un puente paralelo. `git ls-remote https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu.git HEAD` funcionó como lectura pública; el proxy `git.chatgpt-team.site` pidió credenciales ausentes. Lectura pública no prueba push.
7. Si una herramienta antes expuesta desaparece, registrar `UNAVAILABLE_IN_THIS_SESSION` para esa interfaz y conservar su historial. Sin inventario, reportar `NOT_TESTED`. Un servicio histórico siempre requiere prueba actual en el host correcto antes de `CONNECTED_NOW`.
8. Antes de nuevos jobs, comprobar permisos de ejecución, SHA, pausa y concurrencia. Un rerun usa código original y NO prueba el selector. El benchmark actual no ofrece parámetro/callback/tercera condición selectiva; extenderlo con contenido equivalente al rechazado sigue bloqueado. La falta de dispatch es independiente del rechazo de seguridad.
9. Resolver primero el `next_action` del checkpoint. Mientras siga bloqueado, continuar solo labores independientes autorizadas. Para desbloquear, hace falta revisión/resolución externa de los rechazos; otro nombre, ruta, API, inline script o modificación del benchmark no son soluciones permitidas.
10. Al recuperar una interrupción, registrar rama/HEAD recién leído, último run/job/artefacto, gate y paso pendiente. No prometer que otros chats leerán automáticamente este archivo ni que una documentación puede garantizar acceso permanente de la plataforma.

**Prueba independiente:** ejecutar el preflight en un proceso nuevo y repetir con un inventario al que se retire GitHub. Debe conservar el pendiente, marcar las interfaces ausentes y NO declarar Windows online ni lanzar un job. Una prueba de agente sin historial puede limitarse a leer este bootstrap y resolver el siguiente paso; ninguna de estas pruebas valida Qwen selectivo ni equivale a una nueva sesión real del usuario.

<!-- ELDORIA_RECOVERY_CHECKPOINT_START -->
```json
{
  "schema_version": 1,
  "repository": "mt5hjfz2kb-lab/Eldoria-Prewiu",
  "branch": "diagnostic/valoria-ai-eyes-brain-20261015",
  "last_verified_source_head": "1bee49de140b748b3469086c2ff6d28041888eff",
  "recorded_at": "2026-10-15T11:23:03Z",
  "mission_status": "COMPLETED_OPERATIONAL_RECOVERY_AND_BOUNDED_SELECTIVE_MEMORY_DIAGNOSIS",
  "production_paused": true,
  "selective_memory_qwen_validated": true,
  "next_action": {
    "id": "preserve-result-and-owner-production-pause",
    "kind": "await_new_authorized_scope",
    "action": "La recuperación operativa y comparación real están completadas. Conservar evidencia/run y la pausa artística; continuar solo el siguiente alcance que autorice el propietario. No volver a ejecutar el mismo benchmark ni enviar a soporte como condición para este ensayo ya terminado.",
    "avoid": "No asumir producción habilitada, mejora general del modelo, entrenamiento de pesos o ART_APPROVED."
  },
  "last_working_execution": {
    "workflow": ".github/workflows/eldoria-brain-memory-ab.yml",
    "ref": "diagnostic/valoria-ai-eyes-brain-20261015",
    "source_sha": "a9f1ffd83fa6fad6a88ed5e0a9e110aae205d52c",
    "run_id": 38726028141,
    "job_id": 116391981342,
    "runner": "DESKTOP-R10PE55",
    "completed_at": "2026-10-15T11:19:19Z",
    "result": "success",
    "real_inferences": 96,
    "case_count": 32,
    "counts": {
      "without_memory": 32,
      "with_memory": 31,
      "selective_memory": 32
    },
    "selective_regressions_vs_baseline": [],
    "selective_recoveries_vs_full": [
      "one_mesh"
    ],
    "artifact_id": 11967828291,
    "artifact_sha256": "0ad49fd03f07b628ebe084bc32fbb59ab340b128eea26b52f50182b5478a508e",
    "three_arm_json_sha256": "d5b36db3aab797a959fa42dabacc48b0e69b29c0ba070318a735985865abd8a2",
    "persistent_archive": "docs/evidence/eldoria-memory-selective-20261015/three-arm-evidence.zip",
    "archive_blob_sha": "62c572aae74653b92289e8dccf6813c4dc6d2808",
    "current_windows_liveness": "Verified by actual execution at recorded completion time; recheck in a future session"
  },
  "inputs": [
    {
      "path": "tools/art-direction/director-control/selective_memory.py",
      "git_blob_sha": "0373b91bcd179839859d3d20a244efafaf85ff1d"
    },
    {
      "path": "tools/art-direction/director-control/brain_memory_epistemic_v2.py",
      "git_blob_sha": "09e18053a881a4040ddcba450b9ae60327b2e056"
    },
    {
      "path": "tools/art-direction/director-control/brain_memory_selective_ab.py",
      "git_blob_sha": "674c31f2a1625fe6ea874db8aec93be6693681ad"
    },
    {
      "path": ".github/workflows/eldoria-brain-memory-ab.yml",
      "git_blob_sha": "ba8403185d0deb9ca6fd9f26e6c13b92a0a83aea"
    }
  ],
  "completed_gates": [
    {
      "id": "routing-13",
      "result": "13/13 static routing assertions; NOT Qwen execution",
      "source_commit": "f26df80666f3cc2b1ce966a7b828bf7fb36d1820"
    },
    {
      "id": "director-unit",
      "run_id": 38715972496,
      "result": "success; excludes selective Qwen"
    },
    {
      "id": "real-three-arm-qwen-32",
      "run_id": 38726028141,
      "source_commit": "a9f1ffd83fa6fad6a88ed5e0a9e110aae205d52c",
      "result": "32/32 selective; 32/32 baseline;31/32 full; 0 selective decision regressions in this sample; raw payload hashes independently verified"
    }
  ],
  "blocked_operations": [
    {
      "operation": "mcp__GitHub__create_file",
      "path": "tools/art-direction/director-control/test_selective_memory.py",
      "purpose": "Automated selector tests",
      "reason": "Rechazo por controles de seguridad de OpenAI (registro recuperado; no se dispone aquí del payload completo original).",
      "current_status": "Not retried; static selector checks already exist and are not repeated. This test file is not required for the running protocol."
    },
    {
      "operation": "mcp__GitHub__create_file",
      "path": "tools/art-direction/director-control/brain_memory_selective_ab.py",
      "purpose": "Three-condition Qwen benchmark",
      "reason": "La plataforma no pudo determinar el estado de seguridad (registro recuperado; no se dispone aquí del payload completo original).",
      "current_status": "Normal create_file request on SAME ORIGINAL PATH accepted under owner's explicit resubmission request; commit 9bd2ae06da7df716de3b3fbc27dc7298d6cd2158. Historical cause remains unknown; no old rationale was recovered."
    }
  ],
  "capabilities": [
    {
      "key": "github_read",
      "inventory_tools": [
        "mcp__codex_apps__github_fetch_file",
        "mcp__codex_apps__github_fetch"
      ],
      "last_observation": "VERIFIED_THIS_RECORDED_SESSION: repository, source, Actions jobs/logs read; reverify in each new session"
    },
    {
      "key": "github_write",
      "inventory_tools": [
        "mcp__codex_apps__github_update_file"
      ],
      "last_observation": "VERIFIED_THIS_RECORDED_SESSION: additive continuity claim committed 3284d912de9f80c88169a4c2249bc4c4a14b447d; not blanket approval for rejected writes"
    },
    {
      "key": "actions_rerun",
      "inventory_tools": [
        "mcp__codex_apps__github_rerun_workflow_job"
      ],
      "last_observation": "EXPOSED_NOT_EXECUTED; reruns original SHA, does not add selective condition"
    },
    {
      "key": "actions_dispatch",
      "inventory_tools": [],
      "last_observation": "Direct dispatch not exposed; authorized existing push-trigger inventory verified operational in run 38722927795. This cannot bypass rejected benchmark writes."
    },
    {
      "key": "windows_powershell",
      "inventory_tools": [],
      "last_observation": "VERIFIED_AT_2026-10-15T10:49:15Z via actual Actions run 38722927795 / job 116381681682 on DESKTOP-R10PE55. No direct shell. Recheck freshness in a future session."
    },
    {
      "key": "ollama_qwen14b",
      "inventory_tools": [],
      "last_observation": "VERIFIED_AT_2026-10-15T10:49:15Z on Windows: Ollama 0.40.2 answered /api/version and /api/tags; qwen3:14b installed. No fresh inference. Probe may stop its own ephemeral server."
    },
    {
      "key": "blender",
      "inventory_tools": [],
      "last_observation": "HISTORICALLY_VERIFIED_ON_WINDOWS; no Blender production authorized"
    }
  ],
  "evidence": {
    "branch_document": "docs/evidence/ELDORIA_ART_BRAIN_V1_VALIDATION_20261015.md",
    "main_bootstrap_origin_commit": "6ba6faef6cf9b18719aeb4b67f804cb713899dab"
  },
  "limits": [
    "This checkpoint is historical on every future session; fetch live HEAD/jobs before conclusions.",
    "Tool inventory proves exposure only, never credentials, operation approval or Windows health.",
    "No automatic launch, installation, model download, credential search or retry of blocked modifications.",
    "16-case baseline is saturated; matching 16/16 would show no regression on these cases, not superiority over baseline or unseen-case generalisation.",
    "Diagnostic comparison is complete, but artistic production remains paused. The sample does not show superiority over no-memory baseline, nor artistic competence."
  ],
  "last_working_connection": {
    "observed_utc": "2026-10-15T10:49:15.2655125Z",
    "method": "Existing read-only inventory push-trigger on diagnostic branch; actual workflow executed PowerShell on owner PC",
    "workflow": ".github/workflows/eldoria-local-director-inventory.yml",
    "source_commit": "d7ace70f840c986dda61be58924a60e2081ca9da",
    "run_id": 38722927795,
    "job_id": 116381681682,
    "artifact_id": 11966897453,
    "artifact_sha256": "c54d53b7ecedbb72224b6fc66c3cd55e5017d7ffa75ec21521ce14fc7f38f04f",
    "runner": "DESKTOP-R10PE55",
    "result": "success",
    "ollama_version": "0.40.2",
    "qwen3_14b_installed": true,
    "new_inferences": 0,
    "blender_started": false,
    "production_resumed": false
  },
  "previous_measured_execution": {
    "workflow": ".github/workflows/eldoria-brain-memory-ab.yml",
    "ref": "diagnostic/valoria-ai-eyes-brain-20261015",
    "source_sha": "6d5342857e94dc8e7a9a03f5e2b74e2fcb28637f",
    "run_id": 38714648879,
    "job_id": 116354265361,
    "runner": "DESKTOP-R10PE55",
    "runner_id": 22,
    "completed_at": "2026-10-15T09:42:27Z",
    "execution": "32 inferencias Qwen3 14B completadas vía PowerShell/Actions; gate falló por regresión",
    "baseline": 16,
    "full_memory": 15,
    "case_count": 16,
    "regression": "one_mesh",
    "artifact_id": 11961931949,
    "artifact_sha256": "2f485ed8843ea7d5e9ef3b87acd42f2fb8323ee1e3778a552cd05c93b30b8cff",
    "paired_json_sha256": "ae60e31dcdf7d6cfb96620290665745a6b6c469cee46f766127941b7c6428d4b",
    "current_windows_liveness": "Separate fresh health probe SUCCESS at 2026-10-15T10:49:15Z; see last_working_connection, not permanent liveness."
  },
  "mission_complete": true,
  "validation_scope": "96 real queries, 32 claims (16 original+16 prospective phrasing holdout), one fixed-seed run; no general superiority claim"
}
```
<!-- ELDORIA_RECOVERY_CHECKPOINT_END -->

## Comprobación real posterior del PC — 15-10-2026 12:49 Europe/Madrid

**El PC sí respondió ahora**: [run 38722927795](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38722927795) SUCCESS, job `116381681682`, `DESKTOP-R10PE55`. PowerShell ejecutado, Ollama 0.40.2 respondió y `qwen3:14b` está instalado; RTX 3060 12.288 MiB. Esta prueba no arrancó Blender ni realizó inferencias, descargas o producción. El servicio puede cerrarse al terminar si lo inició la prueba.

**Ruta recuperable sin dispatch directo:** el workflow EXISTENTE `.github/workflows/eldoria-local-director-inventory.yml` tiene push-trigger en la rama diagnóstica y solo inspecciona el PC/modelos. Para una nueva comprobación de salud autorizada, inspeccionar versión actual, concurrencia y permisos; una actualización acotada y legítima de sus metadatos de diagnóstico puede disparar la prueba. Seguir el run del SHA exacto hasta fin y leer el artefacto. No repetir todo el benchmark ni suponer que la falta de dispatch equivale a falta de ejecución.

No usar este workflow para incorporar la condición selectiva rechazada ni para ejecutar código equivalente a las dos escrituras bloqueadas. La recuperación de conectividad **no** levanta esos rechazos.

Permisos GitHub consultados: ajuste específico `Allow all actions`; lecturas y escrituras actuales funcionan. No se cambiaron permisos. Ese ajuste no anula controles de seguridad de plataforma ni explica por sí solo los rechazos previos.

## Reanudación aceptada posteriormente — 15-10-2026

Esta nota y el checkpoint RUNNING sustituyen el siguiente paso de revisión externa para el benchmark. Tras la petición explícita del propietario de volver a presentar las operaciones, la herramienta normal `create_file` aceptó crear `tools/art-direction/director-control/brain_memory_selective_ab.py` en su **ruta original** (commit `9bd2ae06da7df716de3b3fbc27dc7298d6cd2158`). No se cambiaron permisos ni nombres ni herramientas, ni se desactivó revisión. No sabemos el motivo de los rechazos antiguos y esta aceptación no autoriza repetir una operación que reciba una nueva denegación.

El workflow existente de memoria ahora ejecuta la comparación aceptada: run `38726028141`, job `116391981342`, Windows `DESKTOP-R10PE55`. Leer el checkpoint y observar ESE run, no iniciar otro ni volver a pedir al propietario permisos generales. Son 96 inferencias con respuestas API/modelo completas, solicitudes/hashes, selecciones y guardado parcial por respuesta. La torre y la producción siguen intactas. Los resultados originales se conservan. No marcar PASS antes de recuperar y evaluar el artefacto.
