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
  "last_verified_source_head": "4a6240fd9e3e87155a850479c699dd5aca41cb86",
  "recorded_at": "2026-10-15T10:38:00Z",
  "mission_status": "BLOCKED_EXTERNAL_SECURITY_REVIEW",
  "production_paused": true,
  "selective_memory_qwen_validated": false,
  "next_action": {
    "id": "resolve-selective-benchmark-security-rejection",
    "kind": "external_intervention",
    "action": "Obtener revisión/resolución de las dos operaciones rechazadas por el canal de soporte/aprobación de la plataforma. No reintentar sus equivalentes. Después, revisar incorporación y ejecución autorizadas de la condición selectiva.",
    "avoid": "No reejecutar V2 esperando una tercera condición; no sustituir pruebas Qwen por estáticas."
  },
  "last_working_execution": {
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
    "current_windows_liveness": "NOT_VERIFIED"
  },
  "inputs": [
    {
      "path": "tools/art-direction/director-control/selective_memory.py",
      "git_blob_sha": "0373b91bcd179839859d3d20a244efafaf85ff1d"
    },
    {
      "path": "tools/art-direction/director-control/brain_memory_epistemic_v2.py",
      "git_blob_sha": "09e18053a881a4040ddcba450b9ae60327b2e056"
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
    }
  ],
  "blocked_operations": [
    {
      "operation": "mcp__GitHub__create_file",
      "path": "tools/art-direction/director-control/test_selective_memory.py",
      "purpose": "Automated selector tests",
      "reason": "Rechazo por controles de seguridad de OpenAI (registro recuperado; no se dispone aquí del payload completo original)."
    },
    {
      "operation": "mcp__GitHub__create_file",
      "path": "tools/art-direction/director-control/brain_memory_selective_ab.py",
      "purpose": "Three-condition Qwen benchmark",
      "reason": "La plataforma no pudo determinar el estado de seguridad (registro recuperado; no se dispone aquí del payload completo original)."
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
      "last_observation": "NOT_EXPOSED_IN_RECORDED_SESSION; workflow_dispatch declared in YAML is not a callable tool"
    },
    {
      "key": "windows_powershell",
      "inventory_tools": [],
      "last_observation": "HISTORICALLY_VERIFIED_VIA_ACTIONS_ONLY; no direct Windows shell exposed"
    },
    {
      "key": "ollama_qwen14b",
      "inventory_tools": [],
      "last_observation": "HISTORICALLY_VERIFIED_ON_WINDOWS; not a Linux localhost service"
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
    "16-case baseline is saturated; matching 16/16 would show no regression on these cases, not superiority over baseline or unseen-case generalisation."
  ]
}
```
<!-- ELDORIA_RECOVERY_CHECKPOINT_END -->
