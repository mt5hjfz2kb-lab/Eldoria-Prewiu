# Eldoria — siguiente paso autónomo, con límites verificables (26/10/2026)

**Estado:** extensión operativa ejecutada en `main`; únicamente diagnóstico de solo lectura. No hay autorización implícita para fusionar, editar el arte de PR #100, gastar créditos, saltar propietarios, reiniciar sesiones Work o enviar instrucciones a un servicio IA inexistente.

## Mejora introducida

Se reutiliza `Eldoria Mission Continuity Guard`, en vez de crear otra cola/sistema. Los dos trabajos observados (Región 1 y Tripo) ahora pueden recibir `diagnostic` de `tools/mission-continuity/triage.mjs`.

El diagnóstico inspecciona **solamente el último run relevante** y una muestra acotada de metadatos de los jobs y artifacts:
- `WAIT_FOR_SAME_RUN`: la ejecución continúa; no duplicar.
- `DIAGNOSE_FAILED_STEPS`: nombre de job/paso fallido para que el propietario investigue; no reintentar a ciegas.
- `EXTERNAL_APPROVAL`: requiere autorización de GitHub.
- `STALE_SOURCE`: las evidencias no corresponden al HEAD actual.
- `REVIEW_VERIFIED_RUN_OUTPUTS`: el run terminó con éxito; falta comprobar el artefacto/capturas/juego y calidad artística.
- `EVIDENCE_PARTIAL`, `EVIDENCE_UNAVAILABLE`, `NO_MATCHING_RUN`: no suponer disponibilidad o corrección.

Se guarda `run_id`, lista acotada de `failed_steps` y `artifacts` en el JSON del propio guardián, que continúa almacenándose en el artifact `mission-continuity-report-RUN_ID`. No lee los logs internos, secretos, assets binarios ni otras ramas innecesariamente; consulta por API los metadatos del run exacto. Si GitHub no ofrece metadatos, falla en modo *desconocido*, no en modo *aprobado*.

## Qué automatización se ha conseguido y cuál sigue pendiente

**Conseguido:** observación periódica de misiones, documentación/hashes, consulta de jobs y artefactos del último run, identificación de estado y **próximo paso seguro**. Las pruebas de GitHub confirman el control de regresión; no se usa el PC Windows para este diagnóstico.

**Pendiente para un ejecutor de IA autónomo real:** entorno persistente que ejecute un modelo/servicio autorizado, compruebe el claim del workstream antes de cada cambio, reciba exactamente `next_safe_action`, edite solo archivos propios en rama aislada, haga pruebas focalizadas, compare el resultado y deje evidencias verificadas. No puede reanudarse un chat cerrado mediante GitHub. Un bot sin modelo real o sin permisos explícitos NO se presenta como ejecución inteligente. No habilitar escritura/dispatch en este guardián para simularlo.

**Pendiente para aprendizaje artístico:** el nuevo constructor de prompts y el verificador de hashes ya existen, pero la conexión a la **inferencia real** de Tripo/Qwen/Blender tiene que hacerla su responsable, conservando prueba del prompt realmente recibido y de la respuesta. El arte de Región 1 y el PR #100 permanecen reservados; un PASS técnico no concede el objetivo visual 7,5/10.

## Seguridad

- Usa las autorizaciones actuales; ningún cambio de titularidad o reserva Windows.
- Metadatos acotados (25 jobs/artifacts máximo); nunca logs completos.
- Ningún intento de arranque automático de Unity, gasto Tripo, publicación, merge o falso ART_APPROVED.
- El vigilante conserva su `report_only` para los dos workstreams activos, sin abrir issues duplicadas.
- Mantener el historial de evidencias; nada se elimina.

## Validación

```bash
node --test tools/mission-continuity/core.test.mjs tools/mission-continuity/nightly-core.test.mjs tools/mission-continuity/triage.test.mjs
node tools/check-workflow-governance.mjs
```

El flujo horario `.github/workflows/mission-continuity-guard.yml` ejecuta estas pruebas y registra las observaciones de GitHub de forma acotada. La mejora se considera aceptada **solo** cuando el workflow exacto y la protección de arquitectura son SUCCESS.


## Extensión: recuperación técnica nocturna verdaderamente automática, estrictamente acotada

El sistema `nightly-recovery.mjs` ya tenía una prueba de reintentos de infraestructura transitoria para trabajos históricos. Se amplió su política para observar el **mismo trabajo canónico** `eldoria-world-region-1-canon-20261022` en su rama de candidato, aunque la titularidad figure en `main`.

Esta es la única actuación productiva autónoma autorizada por este bloque: **reintentar como máximo una vez el mismo job de GitHub Actions fallido**, y solo si simultáneamente:
- Es de noche en Madrid (22:00–08:00), la ejecución ha terminado como `failure`, han transcurrido al menos 20 minutos, es exactamente el SHA vigente de la rama, y el evento fue `push` o `workflow_dispatch`.
- Solo hay un job fallido, con logs que acreditan explícitamente una caída temporal de infraestructura (`HTTP 502/503/504`, pérdida de conexión de runner u otra causa enumerada).
- No hay errores de Unity, código, Python, shader o arte que el reintento pretenda esconder, no se ha usado ya el único reintento y **no hay ninguna otra ejecución de Actions activa en todo el repositorio**.
- El titular sigue `claimed` en `pipeline/active-workstreams.json` con el mismo nombre, rama, revisión, alcance y recurso `windows-runner-heavy`; se relee el `main` vivo **inmediatamente antes** de reintentar.
- Ni el SHA de la rama ni las reservas han cambiado. Si cualquier comprobación falla, se detiene y registra la causa: **no** hay auto-reclamo, cambio de propiedad, force push, aprobación artística, nueva fase creativa, uso de Tripo de pago, publicación ni merge.

**Limitación:** disponer de este mecanismo no significa que ya haya ocurrido un reintento real; los tests y el dry-run comprueban los frenos. GitHub puede no ofrecer un runner libre. Este rescate técnico no es un agente IA que piense y arregle un defecto gráfico.

La solución general para correcciones creativas autónomas sigue pendiente de un ejecutor IA de larga duración realmente conectado al repo con sus permisos. Los actuales workflows y chats no prueban esa capacidad por sí mismos.

### Comprobación

```bash
node --test tools/mission-continuity/nightly-core.test.mjs
```

Consultar la ejecución exacta de `Eldoria Mission Continuity Guard` en GitHub y el resultado `night-recovery-report.json`. El veredicto nocturno aceptable sin fallo de infraestructura es `NOT_FAILED`, `NOT_NIGHT`, `RUNNER_OR_REPO_BUSY` u otro stop verificable; **no** se espera `ONE_TRANSIENT_JOB_RETRIED` salvo incidencia real y todos los criterios cumplidos.
