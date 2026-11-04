# Eldoria — revisión semántica de ingeniería y coordinación, lote 2

**Fecha:** 2026-11-04  
**Workstream-ID:** `eldoria-documentation-semantic-review-batch2-20261104`  
**Alcance:** lectura y contraste de ocho documentos de ingeniería y coordinación marcados `review_pending`. Este informe no reescribe ni cambia el estado del catálogo compartido. No modifica Unity, scripts, workflows, arte, ni reservas ajenas.

## Evidencias contrastadas y decisiones

| Documento | Hallazgo comprobado | Riesgo | Tratamiento seguro |
| --- | --- | --- | --- |
| `docs/UNITY_MIGRATION_SCOREBOARD.md` | La cabecera dice `active operational scoreboard`, pero está fechada el 28/09. Presenta héroes adicionales, hospital, equipamiento y Bastión I–X como `REFERENCE_ONLY`. El trabajo Unity posterior ya incluye contratos y pruebas de varias de esas áreas; una tabla congelada no demuestra el estado de `main` de noviembre. | **Alto**: puede inducir a reconstruir trabajo realizado o degradar la autoridad Unity. | Proponer `historical` para el **estado de progreso de septiembre**, sin perder sus definiciones de etapas. Para progreso actual, comprobar Unity, pruebas, PR y `pipeline/active-workstreams.json`. No se ha calculado un scoreboard actual en este lote. |
| `docs/UNITY_MIGRATION_ACCELERATION_PLAN.md` | Encabezado `active migration execution policy` del 28/09. Su estrategia por capacidades verticales, persistencia y prueba real es reutilizable, pero el orden de migración refleja un punto anterior del proyecto. | **Medio/alto**: confundir estrategia útil con prioridades operativas vigentes. | Mantener como **referencia de método**; no ejecutar automáticamente su lista de prioridades ni declarar el estado del juego a partir de ella. Proponer `reference` con advertencia temporal. |
| `docs/VALORIA_CAMERA_FIRST_PROJECTION_SHELL_V1.md` | Dice `ACTIVE — ZERO-SPEND TECHNOLOGY PROOF` el 06/10 y propone continuar hacia capas/proyección si funciona. El `docs/VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md` del mismo día registra `FINAL FAIL / REJECTED`; `docs/ELDORIA_CANONICAL_AUTOMATION_SANITATION_2026_10_06.md` retira disparadores automáticos experimentales. | **Crítico**: reactivar una técnica descartada, gastar horas o desplazar el mundo 3D aprobado. | Proponer `historical` con aviso de **ruta rechazada**. No ejecutar su secuencia ni tratarla como tarea abierta. No se han modificado workflows. |
| `docs/VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md` | Documenta fallos visuales repetidos pese a TECH PASS, con pruebas Unity y decisión explícita de rechazo. | **Bajo como evidencia; alto si se ignora**. | Mantener como **referencia de decisión histórica vinculante para esa técnica**. No equiparar TECH PASS con calidad. No afirmar que prohíbe toda técnica 2.5D futura: rechaza específicamente el shell/proyección y sus variantes equivalentes. |
| `docs/ELDORIA_PUBLISHED_PROBE_INTEGRITY_V1.md` | El cierre del 08/10 certifica corrección de las comprobaciones, pero informa **FAIL** del juego publicado por errores `level0`. No certifica reparación del juego ni publicación posterior. | **Alto**: confundir una prueba que detecta fallos con una partida jugable aceptada. | Proponer `historical` para el resultado publicado de octubre. Antes de afirmar estado actual, verificar el artefacto distribuido, el run exacto y pruebas posteriores. |
| `docs/VALORIA_CAMERA_FIRST_FUNCTIONAL_SOURCE_V1_RESULT.md` | Cierre del 04/10: Granero rechazado, fuente Cuartel aceptada solo como candidata, integración Unity `VISUAL FAIL / NOT PROMOTED`. | **Alto** si se importa el resultado como arte aprobado. | Proponer `historical` y conservar la distinción fuente candidata frente a integración aceptada. |
| `docs/ELDORIA_MASTER_MAP_RESUME.md` | El 24/09 indica retomar Puerto/Muelle y cerrar el mapa conceptual; `docs/ELDORIA_MASTER_MAP_V1_CLOSURE.md` registra después el **cierre conceptual** y la transición a planificación Unity. | **Medio**: reiniciar una sesión de preproducción ya cerrada. | Proponer `historical`; no seguir sus instrucciones de «próxima sesión». |
| `docs/ELDORIA_MASTER_MAP_V1_CLOSURE.md` | Declara **cerrado a nivel conceptual**, no diseñado al detalle ni implementado. Diferencia núcleo Unity y futuro mundo persistente. | **Medio** si se confunde diseño conceptual con implementación. | Mantener como **referencia conceptual**, sin tratarla como estado técnico actual. |

## Reglas operativas que ya pueden aplicarse sin editar archivos ajenos

1. **No usar el scoreboard del 28/09 como estado vivo.** Una función solo se considera integrada o jugable tras comprobar el código actual, su prueba y el recorrido real.
2. **No reactivar la proyección-shell rechazada.** El `FINAL FAIL` prevalece sobre su antigua cabecera `ACTIVE`; no se reabre una técnica descartada por el nombre de una tarea.
3. **No declarar una versión publicada aceptada** por el PASS de su verificador: en octubre el verificador funcionó precisamente porque rechazó errores reales.
4. **No reiniciar el mapa conceptual**: está cerrado en su ámbito; sus reglas finas siguen abiertas cuando corresponde.
5. **No confundir candidata visual con aprobación de producción**: Granero/Cuartel quedaron sin promoción en ese ensayo.

## Alcance y límites de la comprobación

- Se han leído **ocho documentos** de la lista `review_pending`, más los contratos y decisiones que sirven para contrastarlos. Son ocho revisiones semánticas documentadas, **no ocho reclasificaciones**.
- La existencia de trabajos posteriores de Unity se ha constatado por el historial de PR y el estado del proyecto, pero **no** se ha recalculado el estado de cada capacidad del scoreboard. Sus valores actuales siguen pendientes de prueba.
- No se ha verificado aquí cada workflow de octubre contra el YAML de `main`, ni un APK o WebGL actual. La auditoría evita afirmar que un resultado de octubre sigue siendo el último.
- `docs/documentation-catalog.json` está en el alcance de otros workstreams. Se conserva intacto y sus 175 `review_pending` no se reducen.
- Ninguna fuente artística, recurso, `.meta`, escena o archivo duplicado se ha borrado o movido.

## Siguiente paso controlado

Revisar el siguiente lote de documentos de ingeniería relacionados con importación, cámara, licencias y pruebas, y **separadamente** obtener la titularidad del catálogo para aplicar solo clasificaciones justificadas. Las actualizaciones de cabeceras deben coordinarse con sus propietarios y pasar el guard documental. No se debe hacer una reclasificación masiva por palabras como `FINAL`, `PASS` o `ACTIVE`.
