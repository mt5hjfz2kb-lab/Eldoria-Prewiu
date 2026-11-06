# Eldoria — revisión semántica de importación, licencias y arte (lote de 25)

**Fecha:** 06/11/2026  
**Workstream-ID:** `eldoria-project-entry-organization-20261101` (revisión 11, mismo encargo canónico)  
**Alcance de esta pasada:** documentación y catálogo. No se ha solicitado ninguna generación, descarga de Asset Store, ejecución de Unity, gasto, edición de prefabs, limpieza destructiva ni nueva publicación.

## Alcance verificado

- 25 entradas que estaban como `review_pending` examinadas individualmente contra su contenido, resultados posteriores y reglas operativas de mayor autoridad. Esta auditoría cambia **etiquetas y advertencias del catálogo**, nunca el contenido histórico de esos 25 archivos.
- Clasificación: **13 `historical`, 12 `reference`, cero `current`**. Así no se concede vigencia operativa a una prueba técnica ni a una compra obsoleta.
- Conteo del catálogo: **228 documentos**; **28 `current`, 51 `reference`, 23 `historical` y 126 `review_pending`**. El estado previo tenía 151 pendientes; la disminución de 25 debe ser exacta.
- Baseline de catálogo antes del cambio (blob): `a4ef56b5744b16dac17bcd5d5051be42ddd494f0`. Las fuentes se recuperaron de `main` y sus blobs originales figuran en cada fila para no confundir la versión revisada con un cambio futuro.

## Matriz de decisiones, fuentes y límites

| # | Fuente y blob SHA (40 caracteres) | Clasificación | Diagnóstico y autoridad prevalente |
| ---: | --- | --- | --- |
| 1 | [`ELDORIA_GRAPHICS_TOOLCHAIN_AUDIT_2026.md`](../ELDORIA_GRAPHICS_TOOLCHAIN_AUDIT_2026.md)<br> `50c246adce5b132b13a6eaee0a05176d8aa32bef` | `historical` | Auditoría ACTIVE 29/09: ruta Tripo primaria, precios y pilotos ya no son autoridad operativa. Autoridad: tools/plan-art-production.mjs; docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md. |
| 2 | [`TRIPO_API_NORMA_PRODUCCION_V1.md`](../TRIPO_API_NORMA_PRODUCCION_V1.md)<br> `3fd9a570e7bc241daffd37436d16ab1686b71993` | `reference` | Control de gasto/QA conservable; ensayo Engendro/Foco ya ejecutado, 20 créditos no es tarifa actual ni nueva autorización. Autoridad: docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md; AGENTS.md. |
| 3 | [`UNITY_FREE_ASSET_SHORTLIST.md`](../UNITY_FREE_ASSET_SHORTLIST.md)<br> `2f9b5c5c966d06a4d663cc3609c1cc7c44d86c29` | `historical` | Lista del 25/09 de importación inmediata: no autoriza adquisiciones ni acredita licencias y QA actuales. Autoridad: docs/UNITY_IMPORTED_ASSET_AUDIT.md; docs/VALORIA_FINAL_ASSET_DECISION.md. |
| 4 | [`UNITY_IMPORTED_ASSET_AUDIT.md`](../UNITY_IMPORTED_ASSET_AUDIT.md)<br> `85153ee8f793bffd43eb857d652dfccf7f7ec732` | `reference` | Ensayo técnico de cinco prefabs; recibos/versiones de paquetes incompletos, sin prueba de uso comercial o calidad móvil. Autoridad: docs/VALORIA_ART_ASSET_GATE.md; AGENTS.md. |
| 5 | [`VALORIA_IMAGE_TO_3D_UNITY_GATE.md`](../VALORIA_IMAGE_TO_3D_UNITY_GATE.md)<br> `8f8319a10afdfa1e6cfa4a81509e4b0303fab50b` | `historical` | El Bastión fusionado falló artísticamente en tres zooms; mapas 4×4 y sin modularidad. No promover. Autoridad: docs/VALORIA_IMAGE_TO_3D_PIPELINE_GATE.md. |
| 6 | [`VALORIA_TRIPO_MODULE_UNITY_GATE.md`](../VALORIA_TRIPO_MODULE_UNITY_GATE.md)<br> `4d67c1ef47cd7238d1c366c89734cd419543b4a5` | `reference` | Importación y resolución de dependencias glTFast demostradas, pero 50K/19-12-9 y fuente modular no son garantía final de producción. Autoridad: docs/TRIPO_MODULE_PIPELINE.md. |
| 7 | [`VALORIA_IMAGE_TO_3D_REENTRY_CHECKLIST.md`](../VALORIA_IMAGE_TO_3D_REENTRY_CHECKLIST.md)<br> `a81dfb3c57a9b0119281f20e4149d50b8974afe2` | `reference` | Método aislado de UV, materiales, escala y collider; no reabrir la ruta Bastión fallida ni imponer cámara antigua. Autoridad: docs/VALORIA_IMAGE_TO_3D_UNITY_GATE.md. |
| 8 | [`VALORIA_IMAGE_TO_3D_ASSET_RECEIPT.md`](../VALORIA_IMAGE_TO_3D_ASSET_RECEIPT.md)<br> `e1f233a99fbc4cc7d2244d7214cad92035ae4403` | `historical` | Recibo técnico de candidato previo al rechazo artístico del gate posterior. Autoridad: docs/VALORIA_IMAGE_TO_3D_UNITY_GATE.md. |
| 9 | [`VALORIA_IMAGE_TO_3D_PIPELINE_GATE.md`](../VALORIA_IMAGE_TO_3D_PIPELINE_GATE.md)<br> `8395ccc168e09b3ba4bd78db89f7c209e20ab0ca` | `historical` | El propio índice declara cancelado el procedimiento de reentrada y reemplazada su ruta. Autoridad: docs/VALORIA_TRIPO_MODULE_UNITY_GATE.md. |
| 10 | [`TRIPO_MODULE_PIPELINE.md`](../TRIPO_MODULE_PIPELINE.md)<br> `775cdc583b76e9a644e452bb9a507fe13208da3e` | `reference` | Workflow modular especializado existe, pero no sustituye planificador, autorización de gasto, titularidad ni QA actuales. Autoridad: .github/workflows/tripo-module-pipeline.yml; tools/plan-art-production.mjs. |
| 11 | [`TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md`](../TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md)<br> `72d9c2ce7aa79aec6a9bc9610eec1b0361f94185` | `historical` | Briefs y tope de 40 créditos de dos piezas ya generadas; prohibido reutilizar como permiso para nuevas llamadas. Autoridad: docs/TRIPO_API_ENGENDRO_RESULTADO_20261021.md; docs/TRIPO_API_FOCO_RESULTADO_20261021.md. |
| 12 | [`TRIPO_API_ENGENDRO_RESULTADO_20261021.md`](../TRIPO_API_ENGENDRO_RESULTADO_20261021.md)<br> `6354c5b517951dc73c4175774626eeee7c6e503b` | `reference` | Registro de generación/QA aislada; no certificar rig, animación, art approval ni estado jugable presente. Autoridad: docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md. |
| 13 | [`TRIPO_API_FOCO_RESULTADO_20261021.md`](../TRIPO_API_FOCO_RESULTADO_20261021.md)<br> `212b869f84f527cb3e0859ce4ffb438f24dac406` | `reference` | Ensayo de fuente de corrupción, no hotspot ni misión aprobada; sólo valor técnico hasta validación visual. Autoridad: docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md. |
| 14 | [`TRIPO_API_JABALI_LECCIONES_20261021.md`](../TRIPO_API_JABALI_LECCIONES_20261021.md)<br> `ca3b152d57971a79a63e1691c92a4a0e1bd17a1f` | `reference` | Lecciones de recuperación por SHA y texturas 2K; coste de 20 créditos fue histórico y no fija tarifa. Autoridad: docs/TRIPO_API_NORMA_PRODUCCION_V1.md. |
| 15 | [`TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md`](../TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md)<br> `d006aaa6915e57c228bb3200cfc62fe29ef8c275` | `reference` | Tres fuentes y 60 créditos históricos; las cifras de integración del 21/10 no representan el juego de hoy. Autoridad: pipeline/active-workstreams.json; AGENTS.md. |
| 16 | [`TRIPO_MOBILE_LOD_PROOF_20261021.md`](../TRIPO_MOBILE_LOD_PROOF_20261021.md)<br> `64f2fd6f4238c09f440d4c8c52d3a19ed5f8f63a` | `reference` | Prueba técnica LOD sin FPS móvil ni ART PASS. Ratios experimentales; mención a dirección Qwen superada por orden del propietario. Autoridad: docs/ELDORIA_QWEN_ART_DECOUPLING_20261022.md. |
| 17 | [`VALORIA_TRIPO_MODULAR_GATE.md`](../VALORIA_TRIPO_MODULAR_GATE.md)<br> `6731b2754398635cc3acf45cdc322527f6785cbd` | `historical` | Preflight anterior al gate 50K posterior; no reabrir su experimento como pendiente. Autoridad: docs/VALORIA_TRIPO_MODULE_UNITY_GATE.md. |
| 18 | [`VALORIA_ZERO_EURO_ASSET_GATE.md`](../VALORIA_ZERO_EURO_ASSET_GATE.md)<br> `d7affe99f5a49a94dabcdcb05925632292fa5f7b` | `historical` | Combinación de paquetes rechazada artísticamente aunque pruebas Unity fueran verdes. Autoridad: docs/VALORIA_ART_ASSET_GATE.md. |
| 19 | [`VALORIA_ART_SOURCE_CAPABILITY_BAKEOFF_V1.md`](../VALORIA_ART_SOURCE_CAPABILITY_BAKEOFF_V1.md)<br> `af007f7bb687ddde92989fa38e7cada7923301d3` | `historical` | Plan de source bake-off 06/10 rotulado ACTIVE con candidatos superados; no justificar gasto ni método de producción vigente. Autoridad: docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md. |
| 20 | [`ASSET_SHORTLIST_ZERO_EURO.md`](../ASSET_SHORTLIST_ZERO_EURO.md)<br> `7d5eb29c26a3e32a5a7213d6b61958c75ab17910` | `historical` | Lista de descargas/precios de septiembre sin licencias/acreditación ni necesidad actuales. Autoridad: docs/UNITY_IMPORTED_ASSET_AUDIT.md. |
| 21 | [`ASSET_LIBRARY_CANONICALIZATION_PASS_V1_RESULT.md`](../ASSET_LIBRARY_CANONICALIZATION_PASS_V1_RESULT.md)<br> `ac331b479f0b3270dc293c9e7342686e1e77a180` | `historical` | Antiguo PASS llamó productivo a un Bastión vetado posteriormente; metadatos y archivo se corrigieron en PR153. Autoridad: AGENTS.md; docs/evidence/unity-metadata-20261104/RESULT.md. |
| 22 | [`ASSET_LIBRARY_REPROCESSING_PASS_V1_RESULT.md`](../ASSET_LIBRARY_REPROCESSING_PASS_V1_RESULT.md)<br> `ddbfa3d654a49d89ba37867c1419b8c57c1f518e` | `historical` | Promoción antigua del Hero Bastion explícitamente revocada: su TECH/VISUAL PASS no autoriza reutilizarlo. Autoridad: AGENTS.md. |
| 23 | [`ASSET_DEEP_UPLIFT_PASS_V1_RESULT.md`](../ASSET_DEEP_UPLIFT_PASS_V1_RESULT.md)<br> `a6cded66bb34c1608907fa279309ab4e4b39a932` | `reference` | Diagnóstico de materiales con dos PBR candidatos aparcados; sin promoción en la composición vigente. Autoridad: pipeline/asset-vault/clean-production-shelf-v1.json. |
| 24 | [`ASSET_VISUAL_UPLIFT_PASS_V1_RESULT.md`](../ASSET_VISUAL_UPLIFT_PASS_V1_RESULT.md)<br> `47f3c73c1186041301e174778189c39da24c8b2b` | `historical` | Uplift visual antiguo incluye Hero Bastion vetado; PASS circunscrito a aquella composición. Autoridad: AGENTS.md. |
| 25 | [`ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md`](../ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md)<br> `9ff4c0551850cd981334e3c7c7e6eaff9f6489b1` | `reference` | Criterios de autoría y QA útiles, pero su secuencia aún propone Qwen/Ollama para arte, superado por la orden posterior. No invocarlo para dirección artística. Autoridad: docs/ELDORIA_QWEN_ART_DECOUPLING_20261022.md; AGENTS.md. |

## Riesgos de proyecto detectados

1. **Modelo antiguo de Bastión:** varias pruebas de octubre lo llamaban `VISUAL PASS` o elemento de producción. La prohibición posterior del propietario tiene prioridad y el archivo permanece excluido del uso normal. Que un informe técnico pase no lo rehabilita.
2. **Gastos y licencias:** las listas de septiembre que ordenan `Download now` o `PRIORIDAD ALTA` no representan compras autorizadas hoy; los propios ensayos de importación admiten que falta acreditar recibo, versión exacta y términos de ciertos paquetes. No se declara aquí ninguna licencia comercial verificada.
3. **Qwen y autoría:** en la sección final de `ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md` hay todavía una sugerencia de usar Qwen/Ollama como director/crítico artístico. La decisión vinculante del 22/10 y `AGENTS.md` prevalecen: Qwen **no es requisito ni dirección del arte de Eldoria**. El documento se mantiene solo como referencia técnica y artística de métodos compatibles, hasta una futura revisión de su texto por titular autorizado.
4. **Tripo:** los briefs del Engendro/Foco contenían autorizaciones y presupuestos acotados a generaciones *ya realizadas*. No deben reutilizarse para lanzar otra llamada ni inferir precios actuales. Conservar `task_id`, originales, `.blend`, datos de textura y las lecciones de recuperación por SHA.
5. **QA real frente a etiquetas:** los informes de Jabalí, Foco, Engendro, módulos y LOD aíslan importación/UV/colisión/silueta, pero por sí solos no acreditan una criatura animada, aceptación estética de la escena real, licencia verificable o rendimiento físico móvil. Un 49.800/50.000 triángulos antiguo no es presupuesto universal.
6. **Rutas antiguas:** el ensayado Bastión image-to-3D falló la legibilidad y texturas útiles; un `C`/`FAIL` en su prueba no debe reabrirse como fase de producción. La lista y el informe Tripo modular de 2026 sí conservan métodos útiles, pero la autoridad es el planificador y el arte efectivamente aprobado, no las indicaciones 'ACTIVE' fechadas.

## Autoridades y comprobaciones usadas

- [Norma de agentes](../../AGENTS.md) y [retirada de Qwen artístico](../ELDORIA_QWEN_ART_DECOUPLING_20261022.md).
- [Orquestación de producción](../ELDORIA_TOOLCHAIN_AUTOMATION_V2.md), [planificador ejecutable](../../tools/plan-art-production.mjs) y [política documental](../DOCUMENTATION_POLICY.md).
- [Biblioteca operativa curada](../../pipeline/asset-vault/clean-production-shelf-v1.json), que mantiene el veto del Bastión antiguo.
- Workflows especializados existentes: `tripo-module-pipeline.yml` y `tripo-mobile-lod-proof.yml`, con trabajos delimitados; su existencia no prueba que sea seguro reutilizar una orden de generación antigua.
- [Revisión de licencias/importación del lote anterior](SEMANTIC_REVIEW_BATCH3_20261104.md), usada como contraste, sin presentarla como prueba nueva de licencias actuales.

## Qué está hecho y qué sigue pendiente

**Implementado en candidata:** 25 etiquetas y notas del catálogo; este informe de evidencias. La guía principal incorpora el nuevo recuento. No se edita ningún documento original, modelo, textura, escena, paquete de terceros, script productivo ni workflow.

**Probado documentalmente:** listado exhaustivo de 25 rutas, los cambios `review_pending → historical/reference`, conteo de las 228 entradas y referencias de autoridad. La validación CI y fusión se consignarán con sus IDs al cerrar la PR.

**Fuera de alcance y sin verificar:** recibos/licencias actuales de todas las dependencias, compatibilidad comercial de material externo, nivel artístico del juego, FPS y temperatura móvil, utilidad de los otros 126 documentos y seguridad para eliminar duplicados. Esas cuestiones necesitan sus respectivas pruebas y titulares; no convertir la incertidumbre en un PASS.

**Siguiente paso tras este lote:** revisar documentos pendientes de implementación/entrega y arte visual, priorizando contradicciones que puedan iniciar gastos o modificar `main`. La limpieza física de duplicados continúa separada hasta comprobar consumidores y licencias.
