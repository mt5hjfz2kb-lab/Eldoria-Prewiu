# Eldoria — Política de vigencia y mantenimiento documental

**Estado:** `current` · **Versión:** 2 · **Fecha:** 2026-10-21 · **Alcance:** únicamente documentación y navegación; NO cambia dirección artística, presupuesto, juego ni autorización de publicación.

## 1. Jerarquía y resolución de conflictos

1. Instrucción actual del propietario, seguridad, restricciones de permisos/gasto y HEAD real del repositorio.
2. [AGENTS.md](../AGENTS.md) para el modo de operar y propiedad de trabajos; [registro de titularidades](../pipeline/active-workstreams.json) para concurrencia.
3. El contrato especializado vigente que corresponda al área (Unity, gameplay, arte, QA). Cuando difieran, el contrato más específico **y actualizado** del alcance prevalece; no se inventa precedencia por nombres parecidos.
4. `PROJECT_STATE.md` y `SESSION_HANDOFF.md` proporcionan contexto cronológico, **no** nuevas órdenes. Usar cabeceras relevantes, no recorrerlos completos por defecto.
5. Evidencias, experimentos, resultados, documentación archivada, decisiones históricas: útiles para investigar, sin autoridad automática.

## 2. Ciclo documental

| Estado | Se puede usar para | Requisito antes de afirmar vigencia |
| --- | --- | --- |
| `current` | Norma/contrato de su ámbito | Estar verificado contra implementación actual, propietario y otras autoridades |
| `reference` | Diseño, contexto, hipótesis, trabajo acotado | Verificar fecha/estado; no trasladar un antiguo PASS al presente |
| `historical` | Reconstruir decisiones y pruebas | No reactivar frases `ACTIVE`, `GO` o `PENDING` |
| `review_pending` | Encontrar contexto sin perderlo | Conciliar cabezera, SHA, última prueba, supersesión y scope |

El fichero [documentation-catalog.json](documentation-catalog.json) conserva el inventario completo de Markdown directamente bajo `docs/`. Las rutas sin revisión explícita quedan `review_pending`, **no** `historical` por su nombre ni `current` por aparecer en `docs/`. La clasificación se hará por lotes sin interrumpir la producción. Las subcarpetas `evidence/` y `archive/` son almacenes históricos/probatorios por regla de directorio.

## 3. Al producir un cambio

- Confirmar HEAD y el registro `active`; no editar archivos reservados por otros workstreams aunque estén desactualizados. Un índice puede **advertir** un desfase sin modificar el documento bajo titularidad ajena.
- Si cambia una norma, actualizar **un** documento prioritario y su fila de catálogo. En los subordinados, añadir un aviso de vigencia en vez de copiar repetidamente la norma.
- Si cambia el producto, actualizar el estado productivo solo por el responsable que tenga acceso al archivo. Si cambia un resultado de ejecución, guardar prueba y SHA; no reescribir manualmente la historia.
- Si una guía queda obsoleta, marcar `historical`, anotar su sustituto en el catálogo y, cuando no rompa enlaces, colocar un aviso al principio de la guía. Mover físicamente a `archive/` solo tras auditar enlaces entrantes y con permiso sobre ese scope.
- Aceptar `review_pending` cuando haya incertidumbre. No afirmar que documentación obsoleta está consolidada sin comprobarla.

## 4. Verificaciones automáticas

El guard de arquitectura ejecuta `node tools/check-documentation-governance.mjs --tracked-tree` y sus pruebas. En CI, los archivos y enlaces se contrastan con el árbol exacto de Git del candidato; solo se descargan los contenidos que las pruebas leen. La ejecución local sin esa opción conserva la inspección del árbol de trabajo, incluidas eliminaciones sin commit. Valida catálogo JSON, rutas existentes, estados controlados, exhaustividad de los Markdown directos en `docs/` y los enlaces locales de las páginas de entrada. No necesita Unity, Windows ni servicios externos.

**Límites:** el checker NO determina la verdad semántica de todos los documentos, no comprueba datos en vivo de GitHub ni declara calidad artística. Los documentos `review_pending` requieren intervención sustantiva. Cualquier fallo documental no justifica borrar archivos o reiniciar los trabajos del juego.

## 5. Responsabilidades vigentes

Esta política es permanente y no impone la rama o las reservas de la tarea documental de 2026-10-13. El ejecutor de mantenimiento debe registrar un alcance concreto en el registro vivo, respetar archivos reservados y conservar las decisiones y evidencias históricas. Los trabajos del juego mantienen su titularidad y sus gates.

Al cerrar mantenimiento, distinguir explícitamente:
- **Integridad documental:** catálogo, existencia de rutas, enlaces y pruebas automáticas.
- **Coherencia operativa:** reglas de arranque, concurrencia, herramientas y rutas de entrega revisadas contra código y configuración actuales.
- **Archivo histórico o pendiente de revisión:** no se declara semánticamente actualizado por un CI verde, ni se usa como una nueva orden.

No copiar estados de publicación en todos los índices. Para WebGL, consultar `pipeline/playtest-delivery.json` y la guía de entrega; para Android, el canal y solicitudes específicos. Una build verde, un upload aceptado y una versión activada son estados distintos.
