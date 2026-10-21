# Eldoria — Memoria operativa, fase 1: inventario contrastado

**Fecha:** 21-10-2026. **Tipo:** índice documental de evidencia, no regla de producción. **Ámbito:** proyecto Eldoria en `main`; compilaciones/arte/entregas anteriores solo como experiencia contextual. **Coste añadido:** 0 € y 0 créditos. **Sin cambios:** Unity, GLB/.blend, escenas, gameplay, guardados, canales publicados, workflows, runners, tareas y registro de propietarios.

## Entregable

**Catálogo estructurado de consulta:** [`pipeline/art-knowledge-catalog-v1.json`](../pipeline/art-knowledge-catalog-v1.json). Contiene **23 fichas** identificadas, con familia, condición que las hace relevantes, método, observación contrastada, limitaciones, documentos de evidencia y ejecuciones citadas. El catálogo **no es una base de aprobaciones**, no alimenta todavía automáticamente a Qwen ni proporciona autorización para ejecutar acciones.

El inventario se reconstruyó leyendo documentos y contratos/código de producción que estaban disponibles en `main`, con instantánea de partida `f8cd9ac757ae2702bd4660b44381f849a9b757c9`. **No se recalcularon en esta fase todos los hashes de artefactos antiguos, no se descargaron todos los PNG/GLB, no se ejecutó Unity ni se midió la calidad de imágenes nuevas.** Los estados se refieren estrictamente a la evidencia que los documentos atribuyen a cada experimento; deben confrontarse con el último `main`, su propietario y los resultados reales antes de reutilizar un método.

## Conclusiones para producción futura

| Familia | Conocimiento recuperado | Límite fundamental |
| --- | --- | --- |
| Reautoría de entorno | Terreno/ribera integrada y trabajos de bosque/cantera con fuentes y capturas Unity (ART-003, ART-004) | Reutilizar el **método**, no formas antiguas que contradigan el canon de Valoria |
| Edificios | Mejoras focalizadas sobre aserradero y lona del cuartel, conservando originales (ART-005, ART-006) | Retenidas visualmente no significa `ART_APPROVED` formal |
| Rechazos con valor | Caminos planos, repintados casi invisibles y barrio de prefabs desconectados (ART-007 a ART-009) | NO repetir ni reintegrar las variantes rechazadas |
| Tripo | Tres modelos con fuentes originales, Blender editable y QA aislada; transporte de albedo 2K reparado (ART-010, ART-011, ART-016, ART-020) | El coste fue histórico y acotado. Integración, rigging, mérito artístico y FPS móvil son evaluaciones distintas |
| Animación Engendro | Rig V4 técnica y geométricamente reproducible sin volver a pagar (ART-012) | El ensayo V4 documentado fue rechazado para promoción artística; consultar el trabajo activo por resultados posteriores |
| Qwen | Hay fallos de esculpido global/invisible, latencia CPU y empobrecimiento del repertorio de acciones (ART-013 a ART-015) | Un resultado de inferencia o modificación de vértices no es una mejora visible |
| Memoria/decisión | Herramientas de memoria, router y planificador ya existen (ART-017, ART-018) | La integración **obligatoria y universal** no está demostrada. No crear otro cerebro paralelo |
| Fiabilidad técnica | QA Unity tras reapertura, protocolo de relevo, actualización Android y transporte itch (ART-019, OPS-001 a OPS-003) | Prueba técnica, subida y activación remota no equivalen a certificación física o visual |

### Condiciones de calidad y confianza

- **`VISUAL_RETAINED_NOT_FORMALLY_APPROVED`**: prueba y retención local documentadas, sin manifestar certificación formal `ART_APPROVED`.
- **`TECH_*` / `SOURCE_*`**: importación, deformación, identidad de fuente o funcionamiento técnico demostrados bajo su alcance, nunca garantía estética.
- **`ART_REJECTED`**: conservar la causa y el ejemplo *para evitarlo*, no como receta positiva.
- **`HISTORICAL_METHOD_TO_EVALUATE` / `TOOL_AVAILABLE_INTEGRATION_NOT_UNIVERSAL`**: recursos recuperables o preparados, sujetos a comprobación de vigencia.
- **`EXISTING_PROTOCOL` / `TECH_PROCEDURE_DOCUMENTED`**: continuidad o distribución de software; siempre prevalece su contrato operativo y manifiesto actual.

No existe una autorización de aprobación artística por editar este índice. El único registro de `ART_APPROVED` verificable está bajo [`pipeline/art-visual-approvals/`](../pipeline/art-visual-approvals/). En el momento de la revisión la carpeta no albergaba fichas formalmente aprobadas; los logros locales pueden seguir siendo válidos, pero esa aprobación no se presume.

## Cómo consultar antes de una nueva intervención

1. Identificar el **problema visible real** y el recurso/sector afectado; leer `AGENTS.md`, `pipeline/active-workstreams.json`, el `main` vigente y [el canon actual de Valoria](VALORIA_VISUAL_REFERENCE_CANON_20261020.md) si procede.
2. Abrir el JSON y buscar `trigger`, `domain` o `id` relacionados. Recuperar **primero** las fichas con evidencia y método reutilizable, después revisar fichas `ART_REJECTED` para no repetir el error. No seleccionar automáticamente por coincidencia de palabras.
3. Abrir las fuentes originales en `evidence[]` y comprobar el SHA/runs originales, permisos y ejecución/autoría actuales; confirmar que la técnica resuelve un defecto similar en cámara de juego. Si la fuente ha cambiado, el caso queda como referencia, no solución confirmada.
4. Documentar en el encargo qué caso motiva el procedimiento y qué contradicciones/canon lo limitan. Solo un trabajo **expresamente autorizado** puede producir o integrar arte y debe seguir los gates independientes del [contrato de calidad](ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md).
5. Después de un experimento con evidencia genuina, proponer su incorporación al catálogo mediante revisión verificable, sin sobrescribir ni degradar datos existentes. **En fase 1 no está conectado el hook de escritura a ningún workflow.**

Ejemplo de consulta **manual, de solo lectura**, desde un checkout de `main`:

```bash
node -e "const c=require('./pipeline/art-knowledge-catalog-v1.json'); for (const x of c.cases.filter(x=>/forest|terrain|ground|material/i.test(x.trigger+' '+x.domain))) console.log(x.id, x.verdict, x.evidence[0]);"
```

## Fuentes y límites de cobertura

- `docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md` y `PRODUCTION_STATE.md`: mejoras retenidas en ciudad/mundo y regresiones de materiales; avances sobre escenas reales, sin asumir una nota global.
- `docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md` y `docs/ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md`: tres fuentes y experimentos de anatomía/rig; la fase 2 del Engendro **sigue activa** en el registro y puede superar estas instantáneas.
- `docs/evidence/ELDORIA_ARTIST_FAILURE_FORENSICS_20261015.md`: problemas de contratos semánticos, análisis visual y Qwen.
- `docs/ELDORIA_ART_LEARNING_EVIDENCE_V1.md`, `tools/art-direction/learning-evidence/observations.json`, `tools/art-direction/ai-authoring/author_brief.py`: memoria implementada, acotada a los ámbitos descritos, no aprendizaje de pesos.
- `docs/ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md`, `docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md`: selección de rutas y continuidad, no despacho ilimitado.
- `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`, `docs/ELDORIA_ANDROID_FAMILY_UPDATES.md`, `docs/ELDORIA_PLAYTEST_DELIVERY.md`: patrones de riesgo y continuidad que otros ámbitos pueden reaprovechar.

**Cobertura intencionadamente curada, no enciclopédica:** 23 casos relevantes, representativos y enlazados. No se etiquetaron automáticamente como actuales los 172 documentos `review_pending` del catálogo general de documentación; su vigencia es un asunto independiente de la utilidad histórica de sus experimentos. Se conservaron las técnicas rechazadas con su etiqueta explícita. No se han borrado archivos ni resultados.

## Qué queda para la fase 2 — NO ejecutado aquí

Preparar una integración *única y controlada* con el planificador/artista existente: elegir fichas por defecto e identidad de fuente, adjuntar la memoria relevante a la inferencia genuina y verificar que fue recibida, guardar diagnósticos posteriores como pendientes de revisión, añadir tests positivos/negativos y medir latencia/tasa de intentos sin tomar Windows ni editar Unity. Respetar reservas vigentes. Esa integración **no se afirma completada** por la mera existencia de este catálogo.

### Riesgo de esta entrega

Bajo para el producto: se agregan fichas documentales, sin modificar código ejecutable ni el contenido jugable. Riesgo residual de **interpretación equivocada** de evidencia histórica; se mitiga con estados conservadores, enlaces originales, referencia al canon y prohibición explícita de tomarlo como autoridad o disparador automático. El coste de GitHub CI documental no es equivalente a gasto en Tripo o publicación.
