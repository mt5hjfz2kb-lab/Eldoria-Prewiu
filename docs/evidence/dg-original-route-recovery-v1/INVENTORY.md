# Dirección General V2 — recuperación del rumbo existente
Fecha: 2026-10-08. Workstream: eldoria-dg-original-route-recovery-v1.
Autoridad: AGENTS.md → SESSION_HANDOFF.md → PROJECT_STATE.md → especializadas; main prevalece. Registro maestro: Issue #23. Este inventario sustituye cortes históricos como estado operativo, sin borrar sus evidencias.

## Modelo conservado
DG gobierna las quince responsabilidades D01–D15 del contrato docs/ELDORIA_STUDIO_AUTONOMY_OPERATIONS_V1.md. Los departamentos proponen y ejecutan dentro de rondas autorizadas; el Coordinador distribuye trabajos permitidos y dependencias; agentes concretos ejecutan; QA verifica de forma independiente; DG consolida e informa al propietario. Nuevas rondas y gasto exigen autorización. La infraestructura sirve al videojuego.
Continuidad #6059714994; principios #6060204454; rondas #6060515004 y Ronda 1 #6060629819. AGENTS.md más reciente permite cierre objetivo sin esperar revisión manual opcional.

## Inventario único
| Componente | Implementado | Prueba real | Automático actualmente | Aislado / conexión faltante |
|---|---|---|---|---|
| Dirección, 15 responsabilidades y rondas | Sí, contratos y Issue #23 | Decisiones y ronda registradas | Supervisión parcial por watchdog y avisos | No acredita 15 ejecutores desplegados |
| M16-A catálogo/DAG/dependencias | Sí | Run 37779779501 y 37823294134, 10/10 | Jobs Actions lo ejecutan | Catálogo de ejemplo; no distribuye por sí mismo todos los departamentos |
| M03-A certificador | Sí | Mismos runs, 14/14 | QA técnico en job B | No es certificado visual/jugable oficial |
| M03-B procedencia GitHub | Sí | Informe #6059858087: 22 pruebas locales; aquí no repetidas | Bajo invocación | Falta vínculo autenticado de afirmaciones a contenido y publicación |
| M03-C bytes/journal | Sí | Primera comprobación local de los 7 tests existentes: 7/7 PASS | Bajo invocación | Entradas sintéticas; no captura web ni almacén CAS externo |
| M04 incidencias | Sí | Primera comprobación local de suite existente: 11/15 PASS; 09–12 FAIL ILLEGAL_TRANSITION | Bajo invocación | No servicio durable conectado ni reparación general |
| A→B y gate negativo | Sí | Positivo 37779779501; negativo 37780770598, B skipped esperado | needs y artefactos | No confundir ensayo negativo esperado con regresión |
| Ollama/Qwen local | Sí | Windows 37802085508, 9/9 | Motor local bajo demanda | Instalación no equivale a organización autónoma |
| Coordinador→agente local→revisión | Sí | Worker 37811949263; coordinador 37812029139; 9/9 | Preparación, worker e intake | Una tarea fija, no todo el estudio |
| Primer mantenimiento JSON/BOM | Sí | Windows 37820089071, 6/6; artifact 11568258540 descargado/hash confirmado | Preparación, worker, revisión e intake 37820538652 | Cerrado y runner liberado; PR #26 no fusionada, no reiniciar |
| Coordinador→DG | Sí | Issue #23 #6065916435 y revisión intake | Informe registrado por Actions | Comentario GitHub no prueba notificación recibida |
| Dispatcher autorizado M16 | Sí | 37823281137→37823294134 SUCCESS | Token exacto de propietario, allowlist, Actions | Trabajo técnico fijo; no ejecutor genérico |
| Reducer autorización/QA/reintentos | Sí, main 109c2afc | GitHub 37822036668 SUCCESS | No integrado como servicio | Estados/eventos en memoria; actor declarado; no autenticación/CAS/dispatch general |
| Watchdog de workstreams | Sí | Informe bot #6066199211 | Horario, detección y deduplicación | No corrige ni cierra misiones |
| Work por evento PR | Sí, tareas existentes | Piloto PR #24 con last_run y contrato #6061055982 | GitHub PR→Work lectura | Arte→QA configurado pero sin evento admisible mientras M07 esté bloqueado |
| Avisos al propietario | Dos tareas horarias existentes activas | last_run observado; recepción no expuesta | Sondeo horario | No constancia de entrega/recepción; no crear otra vigilancia |
| M07 y Ronda 1 | M07 corrección activa de otro propietario | Tercera iteración pendiente de inspección | No handoff QA admisible | Conservar owner; no M11 ni ronda nueva |

## Evidencia comprobada directamente
Mantenimiento: ZIP SHA256 316c1c24424d4552ddf11aa51000d0494d8a818cdf192a2f429f8e6737a5209e. proof/task/fixture/final-check inspeccionados; seis casos (incluyendo {}, {"x":2}, {"b":false}, BOM, dos negativos); cleanup PASS.
M16: artifact 11569314853, ZIP SHA256 b22d15dd39a83d04f04b2a667cc9300655241813da5a5f6ddc55ea5328df52f2; source ab8dc2b74a2fa95dce08c12641017394a1414526, run 37823294134. Logs A 10/10 y B 14/14 comprobados.
Pruebas M03-C/M04: archivos exactos obtenidos vía conector de main; ejecución Node local aislada, no checkout completo ni CI integrado. No se alteraron sus implementaciones para convertirlos en PASS.

## Primer enlace reparado
El intake descargaba en evidence pero el manifest auténtico exigía pilot-handoff/source-sha.txt. Falso rechazo reproducido sobre el ZIP real. Arreglo mínimo fe432b52837b1fc3e78f69e46431af1ea0c18e00 en el dispatcher vigente: descarga en pilot-handoff y verifica desde raíz. Checksum local PASS y tamper REJECTED; comparaciones SHA/run conservadas.
Actividad concurrente detectada: el trabajo ya añadió workflow_call al piloto y retiró el intake separado. Se reutiliza esta cadena, no se restaura el workflow retirado ni se diseña sustituto.
Un solo reintento del encargo autorizado se solicitó desde este agente en #6066370080; no se requirió botón ni mensaje del propietario. No se presenta esta reparación asistida como autorreparación universal.

## Gates todavía abiertos
Verificación de la ejecución corregida y su informe bot; notificación proactiva mediante tarea DG existente y comprobación de sus límites de entrega. No declarar AUTONOMÍA GENERAL PASS. Después del cierre acotado, reutilizar dispatcher/reducer existente para la próxima conexión autorizada; no desarrollar infraestructura indefinidamente ni bloquear juego.
