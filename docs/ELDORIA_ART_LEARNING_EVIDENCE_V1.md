# Eldoria — memoria artística empírica y continuidad (V1)

**Estado:** herramienta de conservación y comprobación probada en rama aislada. **Fecha:** 2026-10-13. **No es una ruta alternativa de autoría:** respeta AGENTS.md y Issue #34.

## Qué implementa

- Inventario JSON versionado de ensayos del Bastión I extraídos de logs reales de GitHub Actions, con run ID, commit completo y hash SHA-256 de la referencia visual aprobada.
- Memoria acumulativa: el comando brief recupera lecciones de todo el inventario, no solo las cuatro últimas. Puede alimentar el encargo Qwen de la ruta de autoría ya existente.
- Cada ensayo documenta defecto, lección, siguiente corrección y resultado medido, además del vínculo a la ejecución.
- Comparador estricto: solo agrupa cambios de píxeles cuando son iguales objetivo, cámara, resolución, render, referencia y umbral. La diferencia de píxeles **no** representa por sí sola una mejora de calidad.
- Guard sin ART PASS falso: ART_APPROVED siempre queda false en los casos automáticos; una aprobación real requiere revisión independiente, integración Unity, QA y gobernanza del proyecto.
- Ingestión append-only: cualquier nuevo registro entra pending_verification y solo se escribe en un fichero de salida distinto; exige verificación del log, imágenes originales y hashes antes de incorporarlo a la historia canónica.
- Tests positivos/negativos en CI existente (Repository architecture guard), sin utilizar Windows, Qwen ni créditos de pago.

## Comandos verificables

    node tools/art-direction/learning-evidence/learning.mjs audit
    node tools/art-direction/learning-evidence/learning.mjs brief --target bastion1
    node tools/art-direction/learning-evidence/learning.mjs report
    node --test tools/art-direction/learning-evidence/learning.test.mjs

Los artefactos originales referenciados en los logs **pueden caducar**. La base inicial está clasificada run_log_reviewed porque hemos comprobado esos registros de GitHub, pero **NO** afirma haber vuelto a descargar y rehashear todas las imágenes. Si el operador no puede verificar PNG/GLB originales, debe conservar pending_verification, no inventar un verdict.

## Proceso a ejecutar después de CADA ensayo artístico

1. Observar defecto en la escena real con referencia fija y anotación del encuadre.
2. Generar plan genuino de Qwen antes de modificar geometría, con hashes e identidad de modelo.
3. Aplicar el plan en Blender/BlenderMCP sobre copia editable y conservar los originales.
4. Validar geometría, malla, materiales y parcelas protegidas.
5. Capturar before/after en Unity con misma cámara, URP, materiales y resolución, y conservar imágenes+SHA.
6. Registrar diagnóstico, hipótesis, corrección, métricas y resultado de una evaluación visual independiente. Un control de visibilidad superado sigue siendo REVIEW_REQUIRED.
7. Añadir observación pending_verification, verificar sus fuentes originales y promover solo la evidencia realmente comprobada.
8. Generar el siguiente brief desde el catálogo histórico y diario previo, sin perder errores anteriores. Cuando haya rúbrica común independiente (composición, silueta, roca/vegetación, contacto, color, claridad, coste móvil), medir su evolución separadamente del número de píxeles modificados.

## Frontera de propiedad y limitación real

El Bastión I activo posee tools/art-direction/ai-authoring/**, el archivo ARTIST_LEARNING_JOURNAL.md y los workflows artísticos. **No se modifican.** Para conectar el nuevo módulo a la ejecución obligatoria tras cada ensayo, el **propietario del Bastión** debe invocar brief antes de la inferencia y registrar evidencia al terminar, conservando su propio código y runner. Este trabajo entrega el módulo de memoria y las comprobaciones, **no una automatización aún integrada al 100 % en el artista en ejecución**.

Esta solución no cambia los pesos del modelo, no certifica un 8/10 y no autoriza publicar arte sin revisión. Tampoco equipara un resultado de Blender a la vista del jugador en Unity.

## Adaptador de memoria antes/después — 2026-10-13

El adaptador `tools/art-direction/learning-evidence/cycle-hooks.mjs` se ha diseñado **sin modificar la escena, el artista ni los workflows reservados a Bastión I**. Se consume por el propietario del trabajo en el siguiente ciclo de autoría; no debe proclamarse activación automática del runner mientras no se haya conectado al workflow vivo.

**ANTES** de cada inferencia Qwen en Bastión I, tras preparar el inventario y sin cambiar la versión del modelo:

```bash
node tools/art-direction/learning-evidence/cycle-hooks.mjs prepare --target bastion1 --out "$ART/historical-memory.txt"
cat "$ART/historical-memory.txt" >> "$ART/inventory.txt"
```

El archivo de procedencia `historical-memory.txt.provenance.json` conserva el hash del catálogo de seis casos, el hash del resumen, la referencia y número de lecciones. **Conservarlo como evidencia junto al plan original Qwen.** Verificar en el nuevo `artist-inference.json` que el contenido del inventario realmente llegó a la inferencia; un fichero preparado pero no transmitido no equivale a aprendizaje utilizado.

**DESPUÉS** de un ciclo con auténticas capturas PNG A/B del mismo ángulo de Unity, y con diagnóstico, lección y siguiente cambio expresos del revisor, ejecutar el paso `stage` con el SHA, run ID y hash de cámara auténticos:

```bash
node tools/art-direction/learning-evidence/cycle-hooks.mjs stage \\
  --target bastion1 --out "$ART/next-memory-candidate.json" \\
  --run-id "$GITHUB_RUN_ID" --git-sha "$GITHUB_SHA" \\
  --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \\
  --before "$ART/unity-before-original.png" --after "$ART/unity-after-original.png" \\
  --camera-sha "$ACTUAL_MATCHED_CAMERA_SHA256" \\
  --view-id unity-player-pbr --render-pass unity_pbr \\
  --diagnosis "$VERIFIED_VISUAL_DIAGNOSIS" \\
  --lesson "$VERIFIED_LESSON" --next-action "$NEXT_ARTIST_CHANGE"
```

El adaptador decodifica bytes originales con el comparador existente, comprueba SHA de referencia y produce un candidato `pending_verification` y un archivo `.event.json`. Nunca modifica `observations.json` ni promueve automáticamente `ART_APPROVED`. Verificar origen de las capturas, coincidencia real de cámara y autoría Qwen/Blender antes de incorporar el candidato al catálogo permanente. No inventar diagnóstico si todavía no hay revisión independiente. En un rechazo artístico por Blender previo a Unity, continuar con correcciones ligeras; no lanzar compilaciones completas solo para registrar una lección.

La conexión del comando `prepare` al workflow vivo y la ingestión tras cada captura requieren el consentimiento operativo del propietario actualmente reservado en `pipeline/active-workstreams.json`. Hasta entonces, la integración completa es **PENDIENTE**; las herramientas y sus pruebas son independientes. Comprobar con `node --test tools/art-direction/learning-evidence/learning.test.mjs`.
