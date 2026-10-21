# Eldoria — Índice documental único

> **Vigencia operativa revisada:** 2026-10-21 · **Rol:** navegación, no autorización de desarrollo. Este índice reemplaza los mapas de navegación dispersos, pero no reemplaza las reglas técnicas ni los criterios de calidad.

## Arranque de un chat o agente (menos de dos minutos)

1. Leer [AGENTS.md](../AGENTS.md) como única norma operativa general; prevalecen las instrucciones actuales del propietario y el estado vivo del repositorio.
2. Confirmar HEAD de `main` y de la rama de trabajo. Consultar **solo `active`** en [pipeline/active-workstreams.json](../pipeline/active-workstreams.json). No inferir actividad real de un chat sin comprobar commits, trabajos y GitHub Actions.
3. Leer únicamente las **cabeceras vigentes** de [SESSION_HANDOFF.md](../SESSION_HANDOFF.md) y [PROJECT_STATE.md](../PROJECT_STATE.md). Son historial acumulado; la palabra «CURRENT» fechada no certifica actualidad.
4. Elegir la documentación especializada de la siguiente tabla y sus evidencias originales. Respetar el [protocolo de concurrencia](ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md) y las reservas de archivos/runner.
5. Ejecutar y verificar el trabajo autorizado. Un CI verde, un commit o una renderización no equivalen a entrega funcional y artística.

## Fuentes de verdad por asunto

| Asunto | Documento prioritario | Complementos / límite de autoridad |
| --- | --- | --- |
| Operación de agentes y conflictos | [AGENTS.md](../AGENTS.md) | [Protocolo de trabajos paralelos](ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md) |
| Estado y titulares actuales | [Registro `active`](../pipeline/active-workstreams.json) y GitHub Actions del SHA exacto | [Handoff](../SESSION_HANDOFF.md), [estado producto](../PROJECT_STATE.md); sus históricos no son órdenes |
| Producto a largo plazo | [Mapa maestro](ELDORIA_MASTER_MAP.md) | [Decisiones](../DESIGN_DECISIONS.md); el encabezado de preproducción del mapa es histórico |
| Código y gameplay vigente | [Unity](../Unity/) y [Arquitectura Unity](../UNITY_ARCHITECTURE.md) | [Contrato Unity](../UNITY_CORE_CONTRACT.md), [QA](../QA_AND_DEPLOY.md) |
| Calidad y composición Valoria | **[JPEG canónico aprobado y verificado](../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg)** y [reglas de composición confirmadas 20-10-2026](VALORIA_VISUAL_REFERENCE_CANON_20261020.md) — SHA-256 `86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d` | [Biblia histórica](VALORIA_VISUAL_BIBLE.md); las imágenes de grandes acantilados y campamentos previas son históricas, no canon. ART PASS sigue requiriendo capturas reales de Unity. |
| Método de producción visual | [Reglas de producción](ELDORIA_VISUAL_PRODUCTION_RULES.md) | [Manual de acabado Unity](UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md); sujetos a AGENTS |
| Planes de contingencia visual (NO activados) | [Alternativas Valoria — 14 rutas y gates de decisión](VALORIA_VISUAL_FALLBACK_DECISION_PLAYBOOK_20261017.md) | **Reserva documental:** no cambia los alcances y las reservas vigentes, no pausa al constructor y exige autorización del propietario para ejecutar un pivote. |
| Assets Tripo ya autorizados | [Norma de producción y trazabilidad](TRIPO_API_NORMA_PRODUCCION_V1.md) | Reutilizar la task pagada; ninguna lectura o integración autoriza nuevos créditos. |
| Autoría IA Qwen/Blender/Unity | [AGENTS §7](../AGENTS.md) y el **código vivo** en `tools/art-direction/ai-authoring/` | [Guía técnica](ELDORIA_AI_AUTHORING_ENFORCEMENT_V1.md) con **cabecera desactualizada**; [Issue #34](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/34) y último ensayo real; no asumir ART APPROVED |
| Memoria de técnicas y fallos recuperados (fase 1, **solo lectura**) | [Catálogo artístico contrastado](ELDORIA_ART_KNOWLEDGE_PHASE1_20261021.md) y [fichas JSON](../pipeline/art-knowledge-catalog-v1.json) | Índice de experiencia, no ejecución automática ni ART_APPROVED. Comprobar main y evidencias originales antes de reutilizar. |
| Publicación, QA y apps | [Entrega WebGL/itch](ELDORIA_PLAYTEST_DELIVERY.md), [manifiesto WebGL](../pipeline/playtest-delivery.json) y [actualizaciones Android](ELDORIA_ANDROID_FAMILY_UPDATES.md) | [QA](../QA_AND_DEPLOY.md), [catálogo de workflows](WORKFLOW_INDEX.md), run y artefacto exactos. Pages histórico no es el canal actual. |
| Evidencias e historial | [docs/evidence](evidence/) y [docs/archive](archive/) | No conceden permisos, reservas, ni hacen revivir tareas cerradas |

## Qué está realmente activo

**No mantener aquí una copia manual de trabajos en marcha.** Leer `active` en el [registro](../pipeline/active-workstreams.json), cotejar titularidad y rama, y verificar el SHA y las ejecuciones de GitHub Actions. `history` es archivo probatorio. Los Issues [#32](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/32), [#33](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/33) y [#34](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/34) son paneles/contratos temáticos, **no sustitutos de comprobaciones en vivo**.

## Estados de la documentación

- **`current`**: norma o contrato confirmado y aplicable dentro de su ámbito; nunca prevalece sobre `AGENTS.md`, la solicitud actual ni el código real.
- **`reference`**: dirección, antecedentes o instrucciones de un ámbito concreto; puede contener logros históricos, no es estado actual.
- **`historical`**: reemplazado o cerrado; conservar para rastrear decisiones, jamás arrancar trabajo por sus encabezados `ACTIVE`.
- **`review_pending`**: inventariado pero aún no verificado o con contradicciones de vigencia. No se usa como autoridad operativa hasta reconciliarlo.

El [catálogo versionado](documentation-catalog.json) inventaría **todos los Markdown directos de `docs/`** y sus estados explícitos; no borra el pasado. El [protocolo documental](DOCUMENTATION_POLICY.md) explica vigencia, mantenimiento, conflictos y criterios de migración. El [catálogo de workflows](WORKFLOW_INDEX.md) distingue rutas comprobadas de otras aún sin clasificación.

## Qué no hacer

No leer 300 KB de handoff/historia como instrucciones nuevas; no seguir el mapa de pipeline artístico de septiembre como vigente; no mezclar los PASS históricos de SHARP con un ART PASS de los nuevos modelos; no renombrar ni mover documentos con referencias antes de auditar enlaces; no inventar un runner libre; no usar una actualización documental para apropiarse del scope de Bastión I o Región 1.
