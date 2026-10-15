> **ESTADO VIGENTE 15-10-2026:** recuperación operativa y comparación diagnóstica Qwen selectiva COMPLETADAS. Run 38726028141: sin memoria 32/32, completa 31/32, selectiva 32/32; 96 inferencias reales y respuestas brutas preservadas en Git. Se evita la regresión one_mesh frente a memoria completa y se iguala el baseline. No hay superioridad general demostrada ni aprobación artística. Producción pausada.

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

## Recuperación reproducible del estado actual

Este es el arranque existente, no un segundo registro. La información operativa vigente está en el checkpoint único inferior; las versiones anteriores de este documento quedan conservadas en Git y en el informe diagnóstico como historia.

1. Descubrir las herramientas ACTUALES y hacer una lectura real de GitHub. Inventario solo acredita exposición; no prueba autenticación ni ejecución.
2. Leer main/AGENTS.md, este bootstrap y HEAD vivo de la rama indicada. Leer después la cabecera y conclusión vigente del informe de ESA rama. Un 404 de bootstrap en una rama antigua exige consultar main.
3. Con checkout: `node tools/eldoria-agent-preflight.mjs --root RUTA_REPO --tool-inventory RUTA_JSON`. Inventario opcional: array JSON de nombres actuales o `{"tools":[...]}`, sin secretos. Emite siguiente paso y comparación de blobs; sin inventario marca NOT_TESTED. Con solo conector, recuperar el mismo checkpoint mediante lectura del archivo.
4. Comparar las cuatro entradas del checkpoint antes de reutilizar gates. Si coinciden, no repetir el mismo benchmark ni las 13 comprobaciones de selector. Si faltan o cambian, recuperar los blobs y revisar únicamente el gate afectado; no inferir PASS.
5. **Estado actual completado:** run 38726028141 / job 116391981342 / fuente a9f1ffd83fa6fad6a88ed5e0a9e110aae205d52c terminó SUCCESS. No hay run pendiente que observar ni revisión externa necesaria para ESTE ensayo ya ejecutado. Recuperar el artefacto 11967828291 o el ZIP exacto preservado en Git y su informe, sin lanzar otro job.
6. **Ruta Windows probada:** GitHub Actions push-trigger existente en rama diagnóstica → PowerShell en DESKTOP-R10PE55 → Git preinstalado de GitHub Desktop → evidencia congelada → Ollama instalado, iniciado localmente en el mismo paso solo si hace falta → Qwen3 14B → artefacto. No existe terminal Windows directa expuesta aquí. Falta de dispatch callable no significa falta de ejecución por push. El inventario de salud existente es `.github/workflows/eldoria-local-director-inventory.yml`, run verificado 38722927795.
7. Para una NUEVA prueba autorizada, leer pausa/claims y cola antes de usar el recurso. `GET /repos/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs?branch=diagnostic%2Fvaloria-ai-eyes-brain-20261015&per_page=30`, después jobs/artifacts. Para HEAD usar `/git/ref/heads/diagnostic/valoria-ai-eyes-brain-20261015`. El endpoint /actions/runners fue rechazado por el conector; no implica runner offline. Un job histórico tampoco certifica salud permanente.
8. Los dos rechazos antiguos siguen documentados sin causa original recuperada. Después de la petición explícita del propietario de volver a presentar la operación, la MISMA herramienta create_file aceptó la MISMA ruta original del benchmark, commit 9bd2ae06da7df716de3b3fbc27dc7298d6cd2158. No se alteraron permisos/controles ni se usaron rutas alternativas. Esta aceptación permitió el ensayo completado. No propagar el bloqueo histórico como impedimento actual de ese ensayo; tampoco autoriza sortear una nueva denegación.
9. Detectar interfaces desaparecidas comparando inventario actual y capabilities: ausencia => UNAVAILABLE_IN_THIS_SESSION para esa interfaz, conservando el historial; inventario ausente => NOT_TESTED. Revalidar siempre en el host correcto. Localhost Linux no es el PC. Git público por terminal funcionó; el proxy git.chatgpt-team.site pidió credenciales ausentes. No buscar/extráer credenciales; lectura pública no prueba push.
10. **Siguiente paso actual:** preservar resultados y pausa; esperar nuevo alcance autorizado del propietario. No reanudar arte, Bastión I, 4X, Unity o publicación a partir de este resultado diagnóstico. No afirmar entrenamiento de pesos, superioridad sobre baseline ni ART_APPROVED. No prometer que otros chats lean automáticamente cambios del repositorio.

### Comprobaciones efectuadas del propio procedimiento

- Seis escenarios del preflight: contexto/proceso nuevo y cwd distinto; GitHub ausente; inventario inválido/ausente; input cambiado/ausente; checkpoint malformado/ausente; conservación de pausa/limitaciones.
- La ejecución final recuperó estado COMPLETED, siguiente paso vigente y cuatro blobs coincidentes, sin fingir una nueva comprobación Windows local.
- Un agente sin historial reconstruyó resultados y verificó por llamadas reales run/job/artefacto; encontró prosa antigua, que se reconcilió aquí. Esta prueba no equivale a una nueva sesión real del usuario ni garantiza credenciales futuras.
- `SESSION_HANDOFF.md` y `PROJECT_STATE.md` permanecen reservados por otros trabajos. Sus cabeceras históricas no reactivan tareas frente a la orden vigente de main.
- La comparación son 32 preguntas, una semilla y una ejecución por condición. Es evidencia de no regresión de decisión en esa muestra, no una garantía universal.

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
