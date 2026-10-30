# Eldoria Parallel Workstream Protocol v1

Status: **CANONICAL PROCESS**  
Effective: 2026-10-02

## Protección automática de PR duplicadas — 27/10/2026

**Problema real verificado:** dos sesiones crearon las PR #115 y #116 para recuperar la misma interfaz «Crónica Real». Se conservó la #115 y se cerró la #116 sin integrar. El registro exigía comprobar conflictos, pero no existía una barrera automática al abrir la segunda PR.

**Antes de crear rama, PR o lanzar Unity**, el ejecutor debe consultar en `main` la entrada `active` del workstream canónico y **todas las PR abiertas del repositorio**. Si el mismo encargo ya tiene una PR abierta, debe continuar en ella o efectuar un relevo explícito del propietario registrado; está prohibida otra PR paralela. Compartir un archivo no significa automáticamente compartir encargo: identificar intención y workstream, no bloquear cambios por simple nombre de archivo.

A partir de **PR #117**, toda PR nueva debe incluir en su cuerpo, en una línea propia, `Workstream-ID: <id canónico>`. El ID debe existir en el registro vivo de `main`, con `ownership_state: claimed` y titular válido. No se permite inventar otro ID para la misma misión. Si se requiere transferirla, actualizar el **mismo** ID y conservar su PR canónica. Cuando dos PR abiertas declaran el mismo ID, la más antigua es la válida y la segunda debe cerrar sin competir por Unity ni publicar. La PR #115 queda reconocida explícitamente como el workstream `eldoria-ui-v3-management-completion-20261021`. La #116 permanece cerrada e histórica. La PR #98 es un encargo distinto: no se debe bloquear la #115 solo por compartir archivos de HUD en su historia.

**Control de CI:** `.github/workflows/canonical-pr-ownership.yml` se dispara con nuevas PR o actualizaciones y ejecuta desde `main` (sin código del PR) `tools/workstream-lease/pr-ownership-gate.mjs`: compara ID, titular vivo, PR abiertas y alcances de otros propietarios. En nuevas PR rechaza ID ausente, no reclamado, duplicado, y edición sobre alcance reclamado ajeno; la falta de datos de GitHub provoca fallo, no aprobación optimista. Incluye ensayos de regresión `pr-ownership-gate.test.mjs` que reproducen #115/#116. No usa Windows ni recursos del juego.

**Límite operativo:** GitHub ejecuta este control **después** de abrir la PR, no puede impedir que un chat intente crearla ni cancelar su proceso externo. Debe configurarse como *required status check* en la protección de `main` para impedir merges con comprobación roja; mientras no esté verificada esa configuración, el fallo de CI es una señal de bloqueo pero no una garantía absoluta. Los agentes tienen que hacer la consulta de PR abiertas **antes** de crear una rama y detener cualquier ejecución duplicada al recibir una alerta. No cancelar trabajo ajeno, no cerrar la PR original y no usar este gate como autorización de merge/artística.

## Purpose

Prevent two ChatGPT/agent sessions from silently converging onto the same Eldoria work, duplicating experiments, overwriting canonical state, competing for the Windows runner, or spending Tripo credits twice.

The repository remains the source of truth. This protocol adds a small coordination layer; it does not replace project-state documents or certified evidence. Example file paths below are illustrative, not a source of runtime authority; actual Unity presentation and visual authority are decided by current code and canonical routing.

## Continuidad del pacto propietario–IA (ELDORIA_OWNER_AI_COLLABORATION_V1)

En todo relevo, el nuevo responsable recupera del HEAD de `main` el pacto obligatorio de [AGENTS.md](../AGENTS.md). **El propietario no es programador**: el agente conserva la responsabilidad de investigar, anticipar defectos, cuestionar métodos fallidos y proponer soluciones de oficio, aunque nadie le haya señalado aún el error. El relevo debe incluir objetivo del propietario, resultado visual/jugable realmente comprobado, hipótesis descartadas, estrategia elegida con su motivo, pruebas y siguiente acción segura. Progreso técnico no equivale a cumplimiento del objetivo; una sucesión de reintentos sin mejora exige pivotar tras dos ciclos. No alterar titularidad ni autorizaciones por este pacto.

## Estado actual frente a histórico

El campo `active` del registro delimita exclusivamente los trabajos vivos. `history` documenta tareas antiguas, incluidos `STOP`, `handoff`, `blocked`, `review pending` y `cancelled`: **no es una lista de órdenes aplicables al siguiente chat**. Las cabeceras vigentes de `SESSION_HANDOFF.md` y `PROJECT_STATE.md` tienen prioridad sobre sus entradas cronológicas antiguas. No consultar todo el historial para decidir la próxima acción salvo que el trabajo exija recuperar una prueba o una decisión concreta.

Un agente que informa de una fase intermedia, verifica un gate o escribe un commit sigue siendo responsable de ejecutar el próximo paso permitido. Solo se libera el trabajo ante entrega comprobada o bloqueo/suspensión real, nunca por el mero hecho de enviar texto al usuario. Ante un gate rojo recuperable, reparar y revalidar; ante conflicto de propiedad, respetar el alcance y resolver o señalar **el conflicto exacto**, sin invadir archivos ajenos. No se prometen ejecuciones infinitas ni tareas en segundo plano.

## Permanent continuous-execution rule

Once an agent successfully claims a workstream, it must continue through diagnosis, correction, rerun, validation and closeout within the available live execution. This rule does not create unlimited session duration or background execution. **Progress updates are informational only and MUST NOT suspend execution.** The agent must not wait for the owner to say `continúa`, `sigue`, `reanuda` or equivalent when the next action is already determined by the repository, existing evidence, canonical planner/routing, tests or zero-cost tooling.

A failed gate is the beginning of the next bounded diagnostic/correction cycle; it is **not** a valid reason to end the work session.
### Autonomous closure without owner-review dependency

Owner/manual review is not a terminal gate by default. When the workstream's requested scope can be certified through canonical repository evidence, automated or device-equivalent probes, matched captures/screenshots, gameplay/state/persistence tests, release gates and verification of the actually published build, the owning agent must close autonomously once those gates pass.

Do not leave a workstream active with reasons such as “awaiting owner review”, “awaiting owner physical-device confirmation”, “ready for owner validation”, or equivalent. Absence of owner testing is not a blocker. If the owner later reports a concrete defect, that evidence can revoke/reopen the PASS and the same canonical workstream must resume from the real repository state.

A specific owner decision remains a genuine human blocker only when the unresolved criterion is inherently subjective and materially changes product direction, or when explicit spend/irreversible authorization, credentials, or owner-only input is truly required.
 Completing a sub-step, producing an artifact, dispatching a run, reaching a new phase, needing to inspect captures, or needing to reroute through the canonical planner are also not valid stopping points.

An owned workstream may stop only when one of these terminal conditions is true:

1. **COMPLETED / CLOSED** — the requested work is actually finished and all required technical, gameplay, visual and/or release gates for that workstream have passed with the required evidence.
2. **GENUINE HUMAN BLOCKER** — progress requires a decision or input that cannot be derived safely from the repository, existing evidence, canonical tooling or an available zero-cost route. Examples include fresh authorization to spend money/credits, credentials the agent cannot access, an irreducible owner-only creative choice between incompatible directions, or an external dependency that has no viable alternative path.
3. **SESSION/PLATFORM INTERRUPTION** — the execution environment itself is no longer available. This is not a project-state completion and must never be recorded as one. On the next live session, the matching workstream must be reconstructed from `main` and resumed without waiting for the owner to restate prior instructions.

Not valid reasons to stop include:

- a workflow/run is still executing when its result can be checked in the same live session;
- a test or gate failed but the failure is diagnosable/correctable;
- one phase or artifact completed while the workstream remains open;
- captures still need visual inspection;
- the planner/fallback router needs to be rerun;
- a known zero-cost alternative remains available;
- the agent already sent a progress message;
- the owner is offline/asleep;
- the task is long or has required many tool calls.

If a run must finish before the next action is known, the agent should continue checking it while the session remains live and proceed immediately when the result is available. A progress message must never be phrased as a request for permission to continue unless a genuine human blocker exists.

This rule is permanent project governance. Any future prompt, chat habit or local workflow convention that asks an agent to stop at intermediate milestones is subordinate to this rule unless the owner explicitly instructs that specific workstream to pause.

## Canonical registry

Live ownership is stored in:

`pipeline/active-workstreams.json`

Every substantive session reads it before mutation and claims exactly one coherent workstream before beginning execution. Entries explicitly marked `ownership_state: available`, with `owner: null` and no resources, retain canonical work identity/evidence but are not exclusive writing claims.

## Relevo autorizado entre chats — vigente 21-10-2026

La orden actual del propietario de continuar o mejorar el mismo trabajo en un chat nuevo ya autoriza resolver su relevo. No exigir permiso del chat anterior ni otra confirmación del propietario únicamente por su nombre. Esta regla prevalece sobre identidades personales fijadas en notas históricas; no revoca pausas de contenido, límites de gasto ni gates de entrega.

1. Leer `main`, la entrada viva, cambios pendientes/concurrentes conocidos y runs/jobs pertinentes. Una cola vacía por sí sola no prueba que otro chat haya dejado de editar. No cancelar jobs ni sobrescribir cambios para hacerse con el trabajo. Si hay ejecución/edición incompatible concreta, continuar partes independientes y reconciliar ese conflicto.
2. Reutilizar el **mismo ID canónico**. Conservar alcance, fuentes, evidencias y pendientes; registrar el responsable previo en `ownership_history`, el nuevo `owner`, `ownership_state: claimed`, `write_scope_released: false` y aumentar `ownership_revision` (ausente equivale a 0). Registrar la instrucción actual que autoriza el relevo y la hora. Delimitar el alcance de la nueva mejora sin reabrir ni repetir fases ya certificadas.
3. Publicar el cambio con el SHA del archivo/HEAD leído, sin force. Si cambia, releer y fusionar solo el relevo autorizado. El nuevo chat puede entonces continuar; no debe volver a preguntar por una autorización ya recibida.
4. Todos los escritores deben releer responsable y revisión antes de cada commit o dispatch y abandonar la escritura si fueron relevados. El registro no interrumpe físicamente otro chat: las comprobaciones no sustituyen ese requisito. Un run preparado antes del cambio de responsable/revisión/alcance queda rechazado por el guard de ejecución.
5. Reservar `windows-runner-heavy` únicamente cuando haga falta. Los flujos canónicos leen el registro del commit del run **y `main` vivo**: `tools/workstream-lease/check.mjs` exige misma identidad/revisión/alcance, un solo responsable del recurso y reserva vigente. A continuación conservan su comprobación de procesos Unity/Blender. El nombre del chat no está fijado en esos flujos.

Al terminar una fase entregada sin próximo trabajo encargado, liberar la asignación. Para el ID canónico de Valoria, conservar su entrada de enrutado con `ownership_state: available`, `owner: null`, `write_scope_released: true`, recursos vacíos y revisión aumentada; el siguiente chat autorizado la reclama. El responsable anterior permanece en el historial. Las demás tareas terminadas o bloqueadas pasan a `history` con su pendiente exacto. Un bloqueo externo de acceso a una página no reserva la escena, los archivos artísticos ni Windows.

No hay caducidad automática por minutos de inactividad: podría desplazar a un chat que está editando. La disponibilidad explícita o el relevo autorizado resuelven el caso sin esa pérdida de protección.

## Claim procedure

1. Read `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, the live registry, and current `main` HEAD.
2. Define a narrow workstream ID and scope.
3. Compare the proposed scope/resources against every entry in `active`.
4. If there is no conflict, update the registry using the blob SHA just read.
5. If GitHub rejects the write because the SHA is stale, treat that as concurrent activity. Re-read the registry and reassess before retrying.
6. Only after the claim succeeds may the session perform substantive mutations or dispatch shared expensive/heavy resources.

Example active entry:

```json
{
  "id": "valoria-world-composition-v3",
  "title": "Valoria World Composition v3",
  "owner": "chat-valoria-composition",
  "scope": [
    "Unity/Assets/Eldoria/Scripts/Presentation/ValoriaParcelPresentation.cs",
    "docs/VALORIA_*"
  ],
  "resources": [
    "valoria-production-scene",
    "windows-runner-heavy"
  ],
  "status": "active",
  "claimed_at": "2026-10-02T10:40:00Z",
  "base_main_sha": "<sha>",
  "art_production_routing": "required"
}
```

## Conflict rules

A conflict exists when two claimed active workstreams would mutate or control the same logical production surface. An explicitly available canonical entry is not such a claim. Exact filename overlap is sufficient but not required. An owner-directed handoff of the same work uses the procedure above rather than a permanent conflict based on the previous chat name.

Treat these as exclusive unless explicitly narrowed:

- the same canonical source file or production request;
- the same Valoria district or asset family;
- the same promotion target;
- Tripo generation/credit spend for the same asset or reference;
- Windows-runner heavy Unity certification when concurrent execution would cause contention or invalidate evidence;
- recovery/hotfix work targeting the same failure;
- project-state closeout for a block still owned by another active session.

Read-only research does not need a claim if it cannot mutate project state, occupy shared runners, or spend credits. The moment it becomes implementation, it must claim.

## Dependencies without collision

One workstream may use outputs already committed/promoted by another. It may also record a dependency on an active workstream. It must not take over unfinished scope.

If work A discovers that the correct next step belongs to active work B:

- do not execute B independently or under a second conflicting claim; an owner-directed continuation may transfer B using the procedure above;
- record the dependency in A if useful;
- continue A only where scopes remain independent;
- consume B's result after B releases/completes it.

## Shared runner rule

The Windows self-hosted runner is not globally locked for all work. Only jobs that would materially contend or invalidate one another need the exclusive resource `windows-runner-heavy`.

Short zero-credit/read-only probes may coexist when safe. Tripo spend remains governed by the separate explicit owner-authorization gate regardless of workstream ownership.

### Runner dependency is not an automatic end of task

A workstream holding a runner resource and a job actually executing on that runner are separate facts. Check the live claim and real job queue. Never override another owner's claim merely because a phase/status string says it should be released. When a required Windows certification collides with a valid claim, do every independent safe step possible without using that resource; periodically recheck within the live execution. A GLB/importer passing code checks is an intermediate gate, **not a visually approved Unity integration**. Release/ownership discrepancies must be reconciled by the claimant or in a conflict-safe authorized handoff. End only when no actionable safe work remains and the dependence cannot resolve within the available session; record this specific dependency and exact next test, not a completed result.

## Main reconciliation

A workstream claim does not freeze `main`. Before each substantial promotion/merge/closeout:

1. re-read live `main` HEAD;
2. re-read the registry;
3. ensure another workstream has not changed a dependency;
4. adapt to the newer canonical state rather than restoring an older snapshot.

The rule `repo wins over chat` remains unchanged.

## Release and handoff

When the workstream is done, move it out of `active` into `history` and record, when applicable:

- final status;
- resulting commit SHA;
- workflow run/artifact IDs;
- blocker or handoff target;
- completion timestamp.

Allowed terminal states: `completed`, `blocked`, `handoff`, `cancelled`.

Do not leave stale claims merely because a chat ended. A resumed session must inspect and either reclaim/continue its matching entry or clean it up based on real repo/workflow state. For the persistent canonical Valoria routing entry, release its owner/resources to the explicitly available state above instead of deleting its ID or evidence.

## Scope naming guidance

Prefer stable purpose-based IDs:

- `asset-library-reprocessing-v2`
- `valoria-world-composition-v3`
- `toolchain-automation-v3`
- `hero-bastion-surface-pass-v2`

Avoid generic IDs such as `work`, `test`, `continue`, or IDs based only on a chat number.

## Failure mode this prevents

Without this protocol, two sessions can start from the same `main`, independently decide that the same bottleneck is next, and eventually both modify the same assets/pipeline. Because both correctly obey "repo wins", they then converge further and may duplicate execution.

With the registry, convergence becomes an explicit dependency instead of duplicated ownership.
