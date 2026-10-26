# World4X: comprobación ejecutable de continuidad artística

**Norma del propietario (24-10-2026): no confundir CI técnica con dirección artística efectiva.** Esta herramienta es un control de **proceso y evidencias**, nunca una aprobación automática de la belleza ni autorización para fusionar/publicar.

## Lo que está implementado

- Script: `tools/world4x-artistic-process/verify.mjs`.
- GitHub Actions (Ubuntu, **sin reservar el Windows de Unity/Blender**): `.github/workflows/world4x-artistic-process-gate.yml`.
- Se aplica al PR #100 en cada nueva actualización y puede lanzarse manualmente. Devuelve un **FAIL claro** si faltan evidencias; no elimina/modifica pruebas ni archivos jugables.
- El método existe ya en `AGENTS.md`, `docs/ELDORIA_WORLD4X_GRAPHICS_BREAKTHROUGH_PLAYBOOK_20261024.md` y `docs/ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md`. Este verificador exige **ejecución documentada** y comprueba bytes, no solo promesas.

## Registro requerido antes de dar por cerrada la mejora

El responsable del **mismo workstream y PR #100**, cuando termine una nueva iteración nativa, debe crear o actualizar **en su rama**:

`docs/evidence/world4x-canon-20261022/ARTISTIC_PROCESS_CURRENT.json`

Ejemplo de estructura (es una plantilla, **NO es evidencia ni debe copiarse como si fuera un resultado real**):

```json
{
  "schema_version": 1,
  "workstream_id": "eldoria-world-region-1-canon-20261022",
  "certified_source_commit": "SHA_COMMIT_40_HEX_DEL_ART_CERTIFICADO",
  "blender": {
    "method": "blender-mcp-interactive",
    "mcp_status": "verified",
    "mcp_preflight": "Comprobación real de conexión, sesión y capacidad de observación ...",
    "mcp_reason": "",
    "artistic_decision_log": "docs/evidence/world4x-canon-20261022/REVISION_ARTISTICA_REAL.md",
    "input_blend": "docs/evidence/world4x-canon-20261022/sources/native-.../fuente-original.blend",
    "input_sha256": "HASH_SHA256_REAL_64_HEX",
    "output_blend": "docs/evidence/world4x-canon-20261022/sources/native-.../fuente-editada.blend",
    "output_sha256": "HASH_SHA256_REAL_64_HEX"
  },
  "baseline": {
    "run_id": 40069960813,
    "directory": "docs/evidence/world4x-canon-20261022/sources/native-40069960813/review"
  },
  "candidate": {
    "run_id": 40000000000,
    "directory": "docs/evidence/world4x-canon-20261022/sources/native-40000000000/review"
  },
  "visual_review_path": "docs/evidence/world4x-canon-20261022/REVISION_ARTISTICA_REAL.md",
  "visual_judgment": {
    "outcome": "RETAINED",
    "reviewer": "Identidad real del revisor distinto del autor cuando corresponda",
    "observed_improvement": "Defecto concreto corregido, área del encuadre y por qué merece conservarse",
    "remaining_gaps": "Diferencias importantes que todavía quedan respecto a los originales"
  }
}
```

Si BlenderMCP no se pudo utilizar, registrar `method: blender-bpy-reviewed`, `mcp_status: unavailable` o `not-used` **según lo ocurrido**, describir la comprobación y detallar en `mcp_reason` la razón concreta. No inventar sesiones interactivas. Adjuntar una traza inspección→decisión→corrección→veredicto de la alternativa real. La exigencia artística es **observar y diseñar**, no ejecutar una etiqueta llamada MCP.

Los PNG deben existir realmente dentro de `baseline.directory` y `candidate.directory`, con cada archivo `*-unity.png` y `*-source.json`:

`cercana-horizontal`, `cercana-vertical`, `regional-horizontal`, `regional-vertical`, `panoramica-horizontal`, `panoramica-vertical`.

El script verifica la firma del PNG, dimensiones, hash SHA256 declarado, cámara Unity PlayMode auténtica, pose/zoom/aspecto iguales entre antes/después, cambios de píxeles en al menos una vista y que no haya ediciones artísticas posteriores al commit certificado. Cuando las cámaras han cambiado deliberadamente, **generar un control anterior con las mismas cámaras nuevas**, no comparar perspectivas distintas como mejora de pintura.

El registro debe enlazar las fuentes `.blend` originales y posteriores **existentes y por SHA256**, no limitarse a identificar un archivo remoto inaccesible. El revisor debe describir el resultado real y lo que sigue fallando; `REJECTED` o `PENDING` **no pasan** el cierre.

## Cómo ejecutar

```bash
node --check tools/world4x-artistic-process/verify.mjs
node tools/world4x-artistic-process/verify.mjs .
```

En CI, la herramienta se ejecuta desde una copia comprobada de `main` y examina el commit exacto del PR #100. No se usa Unity/Blender y no se adquiere `windows-runner-heavy`.

## Qué NO demuestra este check

Un `WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS` certifica **documentación e integridad del flujo de producción**, no la calidad final, la actuación real del modelo como artista, el rendimiento móvil ni `ART_APPROVED`.

La aceptación estética sigue siendo un juicio visual independiente conforme al contrato del propietario. Tampoco se puede garantizar que un check impida pulsar «Merge» si no está configurado como **required status check** en la protección de `main`; la protección de rama es una configuración separada. Hasta tenerla, mantener PR #100 en borrador y no fusionar sin revisión.

Las pruebas del mundo 57 PlayMode / 26 EditMode / 44 de progresión se mantienen independientes e intactas. **No cancelar los trabajos actuales ni reclamar su ámbito artístico para rellenar el informe.** El responsable actual debe producir la prueba durante su siguiente ciclo seguro.

## Dirección artística por prioridades, no por microarreglos (control adicional)

El verificador `tools/world4x-artistic-process/strategy.mjs` examina el **historial de cambios reales en el código artístico** de Región 1. Si detecta **cuatro o más cambios consecutivos sobre el mismo elemento** (puente, bosque, pradera, panorámica), da un error claro y solicita una nueva decisión antes de considerar la intervención preparada para integrar. El detector clasifica los mensajes reales de commits: no pretende medir inteligencia ni calidad estética. Si detecta una causa mal clasificada, documentar y corregir el detector sin falsear commits.

No se escribe otro sistema artístico ni se levanta otro proyecto: el responsable del **PR #100 existente** añade, únicamente cuando el control lo exija, `docs/evidence/world4x-canon-20261022/PRODUCTION_DIRECTION_CURRENT.json` en **su rama**, conservando la reserva de Windows y las fuentes AF/AG.

Ejemplo orientativo de estructura, **no es una evidencia ni una orden de sustituir los defectos reales**:

```json
{
  "schema_version": 1,
  "workstream_id": "eldoria-world-region-1-canon-20261022",
  "repeated_topic": "bridge",
  "responds_to_sha": "SHA_COMPLETO_REAL_DEL_ULTIMO_CAMBIO_DE_PUENTE",
  "decision": "PIVOT_TO_HIGH_IMPACT",
  "visual_evidence": "Qué mostraron exactamente las últimas capturas reales de Unity y qué NO corrigieron las iteraciones anteriores",
  "root_cause": "Por qué llevamos varios ensayos de un mismo detalle y qué enfoque hay que cambiar para salir de ese ciclo",
  "reason_for_priority": "Qué problema de imagen global pesa más para el jugador, comparando cercana, regional y panorámica",
  "competing_problems": [
    {"view": "cercana", "defect": "Defecto del puente observado en Unity y su impacto en el acceso al castillo", "impact": 3},
    {"view": "panoramica", "defect": "Brecha central pequeña y distribución geográfica sin jerarquía visual suficiente", "impact": 5}
  ],
  "preserve_AF_AG": true,
  "heavy_run_rule": "QUICK_VISUAL_PREVIEW_FIRST",
  "shared_artistic_goal": "Recuperar la composición continental aprobada y la legibilidad de la geografía en las vistas regional y panorámica",
  "next_batch": [
    {"view": "panoramica", "visual_change": "Recomponer la posición y silueta legible de la Brecha en el suelo y su fractura"},
    {"view": "regional", "visual_change": "Rediseñar la continuidad de masas forestales y caminos conservando relieve y claros"}
  ]
}
```

El SHA es el del **último commit del tema repetido**, no el de `main`. Los problemas y su impacto deben justificarse por capturas, no por valores inventados. El siguiente lote debe ser **coherente**, no una combinación arbitraria de arreglos, y debe mantener la integridad de AF/AG y juego.

Existe una única excepción `ONE_LAST_CRITICAL_FIX` si un fallo de juego/recurso impide continuar, con `critical_blocker` detallado y `one_retry_limit` que explique la intervención única. El verificador **no** permite renovar indefinidamente esa excepción más allá del umbral: debe revisarse la causa o pivotar. Ningún informe tiene permiso para cancelar runs en curso de otro responsable.

El control se ejecuta junto al gate artístico en `world4x-artistic-process-gate.yml`, alojado en Linux, y se prueba con `node tools/world4x-artistic-process/strategy.test.mjs`. Fallar en una iteración de dirección **no significa que Blender, Unity o el juego estén rotos**: significa que faltan prioridad y evidencia para proseguir con la misma estrategia. Una decisión registrada tampoco sustituye una mejora real de imágenes. La aceptación visual continua requiriendo comparación contra el canon.
