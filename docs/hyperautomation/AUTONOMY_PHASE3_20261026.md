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
