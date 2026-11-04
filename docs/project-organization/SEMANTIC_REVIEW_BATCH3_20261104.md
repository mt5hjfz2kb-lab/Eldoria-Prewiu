# Eldoria — revisión semántica de importación, licencias y recursos artísticos, lote 3

**Fecha:** 2026-11-04  
**Workstream-ID:** `eldoria-documentation-semantic-review-batch3-20261104`  
**Tipo:** diagnóstico documental, sin alterar archivos de producción ni clasificaciones del catálogo.

## Alcance y criterio

Se han contrastado **diez documentos marcados `review_pending`** del catálogo y dos documentos posteriores que resuelven contradicciones (`VALORIA_FINAL_ASSET_DECISION.md` y `VALORIA_ART_ASSET_GATE.md`). Se han leído las instrucciones y los veredictos de importación y compra, no se han repetido pruebas de Unity ni verificado recibos de licencias actuales. Un ensayo técnico verde no certifica calidad visual, licencia comercial ni rendimiento móvil.

| Documento revisado | Hecho constatado | Diagnóstico / tratamiento propuesto |
| --- | --- | --- |
| `docs/UNITY_IMPORTED_ASSET_AUDIT.md` | Prueba de cinco prefabs del 25/09, importación/build aprobados, pero aspecto móvil y licencia de los paquetes importados pendientes. Reconoce falta de recibo/licencia/versión exacta de Asset Store. | **Evidencia histórica de importación**, no aprobación de arte ni certificación comercial. Conservar datos de origen; pedir inventario/recibo oficial antes de promover esos recursos. |
| `docs/UNITY_FREE_ASSET_SHORTLIST.md` | Lista del 25/09 con orden «Download now — P0» para seis paquetes. La auditoría posterior confirma importaciones selectivas, no el cumplimiento completo de esa lista. | **Lista histórica de candidatos**; no interpretar «Download now» como permiso vigente de descarga o incorporación. Comprobar procedencia, licencia y necesidad antes de cada recurso. |
| `docs/VALORIA_PURCHASE_AUDIT.md` | Auditoría del 26/09 proponía autorización condicional para comprar Skyreach y probar/devolver. `docs/VALORIA_FINAL_ASSET_DECISION.md` del mismo día **revoca expresamente** la estrategia de compra para probar y decide «ninguna compra ahora». | **Contradicción de autoridad confirmada; riesgo crítico de gasto no autorizado.** Proponer `historical` con advertencia de que prevalece la decisión final. Ninguna compra ni solicitud de reembolso. |
| `docs/VALORIA_ART_ASSET_GATE.md` | Evaluación visual de septiembre: piezas importadas se consideran transitorias, blockout no es arte final; exige licencia, cámara real y revisión visual separada de Unity PASS. | **Referencia de criterio histórico útil**, no diagnóstico visual actualizado de noviembre. No sustituir las referencias aprobadas ni los nuevos ensayos del proyecto. |
| `docs/VALORIA_IMAGE_TO_3D_UNITY_GATE.md` | El Bastión optimizado pasó importación y pruebas técnicas, pero las cuatro capturas fallaron calidad artística; texturas importadas de 4×4, causa sin demostrar. | **Ensayo histórico rechazado artísticamente**. No presentar como Bastión aprobado ni corregir a ciegas el importador sin investigar. |
| `docs/VALORIA_TRIPO_MODULE_UNITY_GATE.md` | Módulo crudo torre/muro/roca: 501.432 triángulos, una malla, sin UV ni texturas. Exige optimización, separación semántica y revisión antes de usarlo como kit. | **Prueba de fuente, no módulo de producción**. Conservar mediciones y límites de aquel ensayo; no imponer sus cifras como límite universal de nuevos activos. |
| `docs/VALORIA_IMAGE_TO_3D_REENTRY_CHECKLIST.md` | Lista de ensayo de un fragmento, con GLB/FBX, licencia, escena aislada, raycast y zooms 19/12/9. | **Referencia de método de ensayo**, no orden para reactivar una ruta concreta ni garantía de que su cámara/LOD sea el actual. |
| `docs/TRIPO_API_NORMA_PRODUCCION_V1.md` | Norma del 21/10 exige presupuesto explícito por pieza, sin reintentos pagados, y conservar fuente/GLB/capturas. Coste observado de 20 créditos solo para una configuración del Jabalí. | **Control de seguridad reutilizable**, pero no permiso de gasto ni tarifa actual. Respetar aprobaciones por pieza, saldo real y prohibición de reintento automático. |
| `docs/ELDORIA_GRAPHICS_TOOLCHAIN_AUDIT_2026.md` | El 29/09 dice «active toolchain decision record» y recomienda Tripo como generador primario y evaluar ArmorPaint/Substance. Su situación técnica/costes son instantánea de septiembre. | **Referencia histórica de investigación**; no declarar herramienta/coste/disponibilidad actual ni comprar software basándose en ella. Contrastar con pruebas recientes, necesidades y autorización expresa. |
| `docs/VALORIA_FINAL_ASSET_DECISION.md` | Decisión final de 26/09: ninguna compra autorizada, Skyreach solo pista de investigación; exige demostrar calidad con tres zooms, estados del Bastión, móvil y licencia. | **Decisión que prevalece frente a la propuesta de compra previa**. No confundir una decisión de septiembre con un veto universal futuro: cualquier compra posterior requiere autorización nueva y concreta. |

## Riesgos que se evitan desde ahora

1. **Gastos no autorizados:** nunca seguir la propuesta «comprar para probar» del informe inicial; la decisión final la revocó.
2. **Licencias no acreditadas:** que el paquete figure importado y que Unity compile no acredita derechos de uso comercial ni la versión exacta adquirida.
3. **Falsa calidad artística:** TECH PASS, mallas y raycasts correctos no equivalen a una composición visual aprobada.
4. **Presupuestos obsoletos:** precios, compatibilidad de tienda, modelos y costes Tripo observados en septiembre/octubre no son cotizaciones actuales.
5. **Pruebas históricas mal reutilizadas:** cámaras, límites de polígonos y checklists de un ensayo no sustituyen los criterios actuales por tipo de activo.

## Estado de ejecución y límites

- **Investigado y documentado:** diez documentos `review_pending` contrastados y dos decisiones adicionales; clasificación propuesta, no aplicada.
- **Implementado:** este informe independiente en `docs/project-organization/`.
- **No implementado:** reclasificación de `docs/documentation-catalog.json`, edición de cabeceras de documentos de otros propietarios, limpieza de duplicados, actualización de licencias o compras.
- **No probado en este lote:** importación Unity actual, existencia de recibos Asset Store, precios o costes de API, calidad móvil de activos, inventario de licencias exhaustivo.
- **Titularidad:** el catálogo sigue en scopes compartidos y no se edita. Los 175 documentos etiquetados `review_pending` no disminuyen por esta auditoría.

## Próxima acción segura

Cuando se libere o se autorice un relevo del catálogo, aplicar avisos/reclasificaciones **solo a documentos revisados**, con trazabilidad de fuente, fecha y decisión prevalente. Empezar por `VALORIA_PURCHASE_AUDIT.md`, `ELDORIA_AI_AUTHORING_ENFORCEMENT_V1.md` y `VALORIA_CAMERA_FIRST_PROJECTION_SHELL_V1.md` por riesgo de gasto, dependencia revocada y técnica visual rechazada. Para los paquetes realmente usados, hacer después una auditoría específica de procedencia/licencia sin presuponer que sus archivos importados incluyen el recibo.
