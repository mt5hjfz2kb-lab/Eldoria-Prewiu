# M04 — integración limitada y condiciones de activación
Fecha: 2026-10-08. Propietario: chat-dg-v3-20261008. Ronda 1 intacta.

## Hechos comprobados
- PR #29: `tools/m04/incidents.mjs` incluye `findingFromWorkflowFailure` (mapeo estructurado sin presumir autenticidad) y `retryDecision` (autorización, ownership, budget máximo dos, bloqueo de SHA defectuoso).
- GitHub Actions 37828360149: 22/22 pruebas PASS, negativa incluida.
- PR #27: workflow_run intake falla solo con jobs conclusion=failure y registra comentario en Issue #23 o #25; sigue DRAFT/no fusionado. No hace corrección, reintento ni QA. No editar ni desplegar sin reconciliar propiedad.
- Por tanto, M04 está TESTED ISOLATED; el circuito real NO está OPERATIVO.

## Paso de integración mínimo
1. Mantener PR #27 como único listener `workflow_run`: NO crear otro trigger equivalente.
2. Invocar M04 desde el mismo pipeline ya existente, con payload real autenticado vía `github.rest.actions.listJobsForWorkflowRun`; mapear únicamente jobs fallidos mediante `findingFromWorkflowFailure`. Datos de evento son insumos, no autoridad de cierre.
3. Guardar registro durable con deduplicación run/job y control de escritura concurrente antes de permitir transiciones o reintentos; comentario en issue no sustituye almacén seguro.
4. Usar `retryDecision` solo como puerta lógica; ninguna llamada a rerun/dispatch sin comprobar token autorizado, claim vigente, concurrencia, presupuesto y fuente corregida.
5. Emitir QA M03 independiente del SHA y artifact exactos; ningún JSON autoproporcionado basta para certificar.
6. Ensayo positivo y negativo controlado en GitHub tras integración autorizada y despliegue; verificar Issue DG y no confundir con recepción push.
7. No invadir M07 ni el runner Windows; no fusionar automáticamente PR #27 ni #29.

## Bloqueo honesto
La prueba E2E del listener propuesto depende de que el workflow llegue al branch predeterminado, cosa que una PR draft aún no hace. No llamar PASS a un ensayo aislado. Hasta el despliegue/revisión del único workflow existente, no hay trigger durable de fallos activo para esa PR; y hasta disponer de almacenamiento/autorización autenticados no se habilita reparación automática.
