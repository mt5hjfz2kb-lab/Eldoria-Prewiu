# Eldoria — auditoría semántica de los 20 documentos prioritarios

**Fecha:** 6 de noviembre de 2026  
**Workstream-ID:** `eldoria-project-entry-organization-20261101` (revisión 9)  
**Estado:** 20 archivos leídos y confrontados con las instrucciones actuales, manifiestos vivos y PR fusionadas. Revisión del catálogo implementada en una rama de documentación; certificación CI y fusión se consignan aparte al finalizar. No se ha ejecutado Unity ni cambiado el juego.

## Qué significa cada estado

- **`current`:** contrato operativo comprobado contra una implementación o política vigente; no significa que toda la cadena, arte o calidad del juego estén certificados.
- **`reference`:** diseño, criterio de calidad o evidencia útil que hay que consultar con contexto; **no se ejecutan sus instrucciones antiguas sin verificar** la autoridad actual.
- **`historical`:** experimento, incidente, guía de una versión superada o método rechazado. Se conserva íntegro para trazabilidad, pero no guía el próximo desarrollo.

**Resultado:** 20/20 decisiones documentadas, **2 `current`, 13 `reference`, 5 `historical`**. Del catálogo inicial de 228 entradas quedan 151 por revisar (frente a 171). No se han reescrito los 20 documentos originales ni se han desactivado herramientas.

## Matriz de las 20 decisiones

| # | Documento revisado | Estado | Por qué y qué no debe confundirse |
|---:|---|---|---|
| 1 | [`ANDROID_NATIVE_PLAYTEST.md`](../ANDROID_NATIVE_PLAYTEST.md) | `historical` | Preparación Android del 10/10 ya superada: el manifiesto vivo publica APK familiar 1.21/code21. Las pruebas físicas allí descritas siguen siendo criterios, no prueba de dispositivo ya realizada. **Autoridad:** updates/android/family.json; pipeline/playtest-delivery.json. |
| 2 | [`ELDORIA_PUBLISHED_PROBE_INTEGRITY_V1.md`](../ELDORIA_PUBLISHED_PROBE_INTEGRITY_V1.md) | `historical` | Certifica corrección del verificador pero documenta un probe publicado fallido el 08/10; no describe la publicación actual native299 ni certifica el juego actual. **Autoridad:** pipeline/playtest-delivery.json; PR #162. |
| 3 | [`ELDORIA_SHARED_RUNNER_CHECKOUT_COLLISION_20261020.md`](../ELDORIA_SHARED_RUNNER_CHECKOUT_COLLISION_20261020.md) | `reference` | Incidencia y mitigación puntual de checkout 20/10. Conservar como precedente de aislamiento de runners, no como descripción de la cola/reservas actuales. **Autoridad:** AGENTS.md; docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md; pipeline/active-workstreams.json. |
| 4 | [`ELDORIA_TOOLCHAIN_AUTOMATION_V2.md`](../ELDORIA_TOOLCHAIN_AUTOMATION_V2.md) | `current` | Contrato vigente para clasificar necesidades, usar herramientas existentes y exigir autorización específica de créditos; la ruta está implementada en el planificador. Las pruebas de 01/10 son historia, no autorización artística nueva. **Autoridad:** tools/plan-art-production.mjs; docs/ELDORIA_QWEN_ART_DECOUPLING_20261022.md. |
| 5 | [`ELDORIA_TOOLCHAIN_AUTOMATION_V2_RESULT.md`](../ELDORIA_TOOLCHAIN_AUTOMATION_V2_RESULT.md) | `reference` | Evidencia técnica fechada 01/10 de LOD/bake/Unity, no garantía de calidad actual ni de que transformaciones avanzadas de Tripo estén automatizadas. **Autoridad:** docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md. |
| 6 | [`ELDORIA_WEB_PARITY_CAMERA_REMEDIATION_20261029.md`](../ELDORIA_WEB_PARITY_CAMERA_REMEDIATION_20261029.md) | `reference` | Correcciones I–V, T2, aceleradores y cámaras realmente fusionadas en PR132. El documento conserva pruebas y decisiones de esa versión; menciones a PR131/no publicar son históricas, no el estado presente. **Autoridad:** PR #132 (merged 30/10); PR #162 (merged 06/11); pipeline/playtest-delivery.json. |
| 7 | [`ELDORIA_WORLD_MAP_4X_FOUNDATIONAL_REQUIREMENTS.md`](../ELDORIA_WORLD_MAP_4X_FOUNDATIONAL_REQUIREMENTS.md) | `reference` | Requisitos fundacionales aparcados el 05/10 y retomados por Region 1. Valen como intención futura del 4X, no como orden de abrir hoy una primera implementación inexistente. **Autoridad:** docs/ELDORIA_WORLD_REGION_1_V1.md. |
| 8 | [`ELDORIA_WORLD_REGION_1_V1.md`](../ELDORIA_WORLD_REGION_1_V1.md) | `reference` | Informe de cierre y arquitectura de Region 1 v1 sobre main, no aprobación indefinida de su arte ni estado vivo de los workstreams/world actuales. **Autoridad:** docs/ELDORIA_WORLD_REGION_1_V1.md; docs/ELDORIA_WEB_PARITY_CAMERA_REMEDIATION_20261029.md. |
| 9 | [`WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md`](../WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md) | `reference` | Taxonomía de nodos, PvE, ciudades y marchas reutilizable. El texto 'next production wedge' y el modelo Frontier/zoom antiguo no sustituyen al diseño Region 1 integrado. **Autoridad:** docs/ELDORIA_WORLD_REGION_1_V1.md; PR #132. |
| 10 | [`WORLD_MAP_VISUAL_BENCHMARK_V1.md`](../WORLD_MAP_VISUAL_BENCHMARK_V1.md) | `reference` | Benchmark visual orientativo; NO aplicar tamaños ortográficos antiguos 18/14/10/7 como parámetros de la cámara actual: PR132 diferencia escala regional, cercana y panorámica según viewport. **Autoridad:** PR #132; docs/ELDORIA_WEB_PARITY_CAMERA_REMEDIATION_20261029.md. |
| 11 | [`VALORIA_REAL_GAME_STATE_V1.md`](../VALORIA_REAL_GAME_STATE_V1.md) | `historical` | Trabajo SHARP/PLY del 06/10 explícitamente sin cierre Unity/visual. La evolución posterior de Valoria/PR136/PR162 lo convierte en evidencia de experimento, NO en tarea activa de regeneración SHARP. **Autoridad:** PR #136; PR #162; pipeline/active-workstreams.json. |
| 12 | [`VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md`](../VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md) | `reference` | Requisitos de espacio y crecimiento hacia 25–35 niveles son planificación, no niveles implementados ni licencia para modificar topología/cámara de un titular ajeno. **Autoridad:** docs/VALORIA_MASTER_PLAN_V1.md; docs/VALORIA_PROGRESSION_MAP_V1.md; docs/VALORIA_CAMERA_EXPANSION_PLAN_V1.md. |
| 13 | [`VALORIA_FINAL_ASSET_DECISION.md`](../VALORIA_FINAL_ASSET_DECISION.md) | `historical` | Decisión sin compra del 26/09 y evaluación comercial con precios/licencias fechados. Prevaleció frente a VALORIA_PURCHASE_AUDIT, pero no es una cotización ni un veto eterno; gastar requiere nueva aprobación expresa. **Autoridad:** AGENTS.md; docs/project-organization/SEMANTIC_REVIEW_BATCH3_20261104.md. |
| 14 | [`VALORIA_VISUAL_AUTHORITY_RESET_V1.md`](../VALORIA_VISUAL_AUTHORITY_RESET_V1.md) | `reference` | Prueba visual corregida del 06/10 y fuente aprobada de ese momento. Conservar hashes/capturas; no asumir que sus SHARP/cámara HOME describen el juego actual o que el 'STOP' antiguo bloquea producción autorizada posterior. **Autoridad:** docs/DOCUMENTATION_INDEX.md; PR #136; PR #162. |
| 15 | [`VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md`](../VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md) | `historical` | GATE final FAIL: tarjetas semánticas/proyección con huecos y disoclusiones, método rechazado explícitamente. Conservar como evidencia negativa; no reactivar como pipeline aprobado. **Autoridad:** docs/VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md; docs/VALORIA_VISUAL_AUTHORITY_RESET_V1.md. |
| 16 | [`VALORIA_ART_ASSET_GATE.md`](../VALORIA_ART_ASSET_GATE.md) | `reference` | Auditoría visual de septiembre contra geometría primitiva y packs inadecuados; principio TECH != VISUAL sigue útil. La orden 'reemplazo para next build' y paquetes candidatos no describen la ciudad actual. **Autoridad:** docs/project-organization/SEMANTIC_REVIEW_BATCH3_20261104.md; PR #136. |
| 17 | [`ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md`](../ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md) | `current` | Router acotado implementado para fallos clasificados, política read-only sin gasto. El 'no new R&D' se aplica SOLO a ese router; no impide investigar proactivamente fuera del router con autorización y pruebas. **Autoridad:** tools/route-visual-fallback.mjs; pipeline/visual-fallback-policy.json; tools/plan-art-production.mjs. |
| 18 | [`ALDRIC_DIALOGUE_BUST_INTEGRATION_20261027.md`](../ALDRIC_DIALOGUE_BUST_INTEGRATION_20261027.md) | `reference` | PR118 fusionada y capturas nativas del busto verificadas. Los apartados iniciales que dicen 'PR draft'/'pendiente Unity' quedaron superados por la sección de certificación final; no repetir Tripo ni gasto. **Autoridad:** PR #118 (merged 27/10); docs/ALDRIC_DIALOGUE_BUST_INTEGRATION_20261027.md. |
| 19 | [`BASTION_I_II_OWNER_PLAYTEST_CHECKLIST.md`](../BASTION_I_II_OWNER_PLAYTEST_CHECKLIST.md) | `reference` | Checklist de experiencia humana: UX, recompensas y persistencia siguen siendo criterios útiles; QA_FAST y 'Frontier' reflejan una fase histórica. No certifica por sí mismo la APK o WebGL actual. **Autoridad:** PR #132; PR #162; updates/android/family.json. |
| 20 | [`VALORIA_MASTER_PLAN_V1.md`](../VALORIA_MASTER_PLAN_V1.md) | `reference` | Plan espacial de Valoria en bandas hasta 25–35 niveles, condicionado a graybox y cámara. Es insumo de diseño, no prueba de topología/cámara final ni permiso para tocar Unity actualmente reservado. **Autoridad:** docs/VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md; docs/VALORIA_CAMERA_EXPANSION_PLAN_V1.md; pipeline/active-workstreams.json. |

## Jerarquía comprobada

- **Operativa:** [AGENTS](../../AGENTS.md), [índice documental](../DOCUMENTATION_INDEX.md), [registro de titulares](../../pipeline/active-workstreams.json) y código implementado. Una PR abierta (p. ej. #163) **no equivale** a código integrado en main.
- **Publicación:** manifiesto [APK familiar 1.21/code21](../../updates/android/family.json) y [WebGL native299](../../pipeline/playtest-delivery.json). Una entrega publicada no equivale a prueba real en teléfono ni aceptación visual.
- **Arte:** el [planificador efectivo](../../tools/plan-art-production.mjs) y el [router de fallos conocidos](../../tools/route-visual-fallback.mjs) con su [política](../../pipeline/visual-fallback-policy.json). La prohibición de buscar R&D automáticamente es una característica acotada del router, **no una prohibición de investigación responsable para el agente**.
- **Mundo:** los contratos fundacionales ayudan a decidir qué debe ser un mapa 4X, pero los valores antiguos de cámara `18/14/10/7` pertenecen a una prueba Frontier; no deben sobreescribir los límites de cámara reconciliados por [PR #132](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/132).
- **Valoria:** los documentos de expansión hacia niveles 25–35 son planificación de producto, no evidencia de edificios implementados hasta ese nivel; cualquier cambio de código/escenas pertenece al titular del workstream activo.
- **Aldric:** [PR #118](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/118) está fusionada con capturas nativas de los bustos; el principio del informe dice “pendiente”, pero su sección posterior confirma la validación. No es motivo para volver a gastar créditos.

## Riesgos operativos eliminados a nivel de catálogo

1. Seguir instrucciones de Android que presuponen que no existe APK, cuando hay un manifiesto de entrega vigente.
2. Tratar como procedimiento actual un ensayo SHARP que nunca obtuvo el cierre declarado, o un método de proyección 2,5D rechazado con `VISUAL FAIL`.
3. Repetir la evaluación de compra con precios y permisos de septiembre como si fueran vigentes, en lugar de exigir autorización y verificar licencias actuales.
4. Usar cotas de cámara Frontier en la Region 1 actual, confundir TECH PASS con aprobación artística o interpretar pruebas antiguas como una publicación reciente.
5. Reiniciar PR ya fusionadas o tocar scopes reservados basándose en cabeceras de “pendiente” fechadas.

## Qué está verificado y qué no

**Verificado:** existencia y lectura de las 20 fuentes; PR #118, #132, #136 y #162 fusionadas; manifiestos actuales de Android/WebGL; planificador y router existentes; catalogación exacta de 20 rutas y recuentos comprobados; reserva del mismo workstream con scope exclusivamente documental.

**No verificado en esta revisión:** ejecución del juego, impresión de calidad visual, funcionamiento de APK en dispositivo físico, completitud del mundo 4X, licencias comerciales de terceros, eliminación segura de duplicados, 151 documentos restantes. Las páginas originales y evidencia se conservan.

**Próxima pasada:** revisar por familias los 151 documentos restantes con el mismo criterio. Priorizar contratos de importación/licencias y seguridad de builds, después pruebas artísticas muy fragmentadas; evitar clasificar como `current` una prueba técnica o un diseño todavía no materializado.
