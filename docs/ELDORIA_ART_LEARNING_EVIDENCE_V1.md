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
