# Eldoria — referencias aprobadas y caducidad de evidencias

Fecha: 2026-11-04. Decisión del propietario: conservar las imágenes **de referencia**, no un archivo histórico de A/B y capturas de pruebas.

## Fuentes que deben preservarse

- `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` es la referencia visual canónica de Valoria, declarada en `pipeline/visual-authority-routing-v1.json`.
- `references/VALORIA_SUPERSEDED_VERTICAL_REFERENCE_20261006.jpg` es una referencia anterior; no confundirla con la vigente ni eliminarla sin revisar sus dependencias.
- Otras referencias aprobadas pueden residir en ramas, documentación o materiales de autoría: **localizarlas y verificarlas antes de borrar cualquier imagen**. La palabra «referencia» debe estar respaldada por una aprobación o contrato, no inferida por el nombre del archivo.
- No reescribir el historial Git ni eliminar referencias, assets de Unity, ilustraciones fuente, modelos ni materiales de terceros como parte de esta política.

## Regla de trabajo

1. Generar capturas Unity auténticas y A/B cuando sean necesarias para revisar o certificar un cambio. La evaluación artística continúa siendo humana; una diferencia de píxeles no es aprobación.
2. Tras la evaluación y cierre de todos los consumidores técnicos de esa ejecución, las imágenes A/B, previsualizaciones y capturas intermedias **no tienen conservación permanente**.
3. Mantener resultados mínimos verificables (SHA del código exacto, resultados de pruebas, identidad/digest de artefacto y, cuando proceda, manifiesto de publicación). Los informes técnicos pequeños pueden conservarse por más tiempo que las capturas.
4. No borrar artefactos de compilación APK/WebGL, firmas, paquetes que aún deban distribuirse, evidencias exigidas por certificaciones activas o artefactos que todavía deban descargarse para publicar o verificar versiones.

## Caducidad inicial propuesta y alcance

| Artefacto | Retención de NUEVAS ejecuciones | Justificación |
| --- | ---: | --- |
| `eldoria-visual-qa-*` | 2 días | Se conserva únicamente `report.json` técnico; las imágenes A/B no se suben |
| `eldoria-work-status-*` | 1 día | Estado regenerable; el resumen de Actions también está disponible |
| `eldoria-art-production-plans-*` | 3 días | Planes regenerables por workflow_dispatch |
| `eldoria-ui-progression-cert-*` y paquetes nativos de Unity | **Sin cambio por ahora** | Su workflow y artefactos están bajo trabajo activo; dependen de pruebas y pruebas de identidad |
| APK/WebGL y paquetes de promoción | **Sin cambio** | Dependen de enlaces, firmas, hashes, canal de distribución y controles de publicación |

Los cambios de retención a través de `actions/upload-artifact` afectan a **nuevos archivos**, no a los existentes. El ajuste global de GitHub Actions también afecta a comprobaciones, ejecuciones y estados desde octubre de 2026; no modificarlo sin auditoría específica.

## Protocolo obligatorio antes de la limpieza retrospectiva

- Inventariar artefactos existentes por `id`, `name`, `size_in_bytes`, `created_at`, `expires_at`, `workflow_run.id` y SHA.
- Obtener lista explícita de artefactos todavía referenciados por `pipeline/unity-publish-request.json`, `pipeline/playtest-delivery.json`, certificados nativos, releases y canal Android.
- Confirmar que las PR y workstreams activos no requieren esos artefactos; incluir el histórico de publicaciones todavía en uso y la comparación visual automática (`tools/visual-qa/compare.py` usa `eldoria-v41-native-<run_id>` de dos certificaciones).
- Producir primero un **dry-run** que enumere exactamente qué se eliminaría y la razón. No borrar por antigüedad o prefijo sin comprobar referencias.
- Autorizar la limpieza técnica únicamente cuando no quedan consumidores. La eliminación de GitHub Actions es irreversible. No borrar archivos del repositorio ni ramas con este procedimiento.

## Implementación de esta primera pasada

Rama independiente: `cost/artifact-retention-references-20261104`. Se acortan retenciones de tres salidas regenerables y se actualiza `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`.

No se modifica `main`, ni el runner Windows, ni `.github/workflows/ui-progression-cert.yml` (archivo con titulares activos), ni el canal de APK/WebGL. Ninguna limpieza histórica se considera ejecutada sin verificación de dependencias y resultado real.

## Validación ejecutada en GitHub (rama aislada)

- Ejecución: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41898038881
- Resultado observado: `success`; job `verify`, sin pasos fallidos.
- Comprobaciones reales: sintaxis YAML con PyYAML, `retention-days` de los tres uploads, `visual-report/report.json` como único resultado persistente del QA visual, presencia de las condiciones de política y SHA-256 del JPEG canónico comparado con `pipeline/visual-authority-routing-v1.json`.
- Estas pruebas **no** certifican todavía ejecución del workflow modificado en `main`, consumo real facturado ni limpieza de los artefactos históricos.
- El workflow de comprobación fue temporal y debe retirarse del candidato; ninguna comprobación Unity se canceló ni se publicó ninguna APK/WebGL.
