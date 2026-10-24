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
