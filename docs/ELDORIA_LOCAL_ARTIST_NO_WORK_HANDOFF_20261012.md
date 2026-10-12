# ELDORIA — continuidad del artista local sin ChatGPT Work (2026-10-12)

## No reconstruir
Preservar rama `art/local-ai-professional-artist-role-20261012` y commit origen `2bf8e3f6d1f86118750bce0a0fbfba8975f0460d`. `main` intocable. Su rama hermana Unity es `integration/valoria-v3-unity-lookdev-20261012`. Recuperación en ella: `docs/ELDORIA_WORK_EXHAUSTION_RECOVERY_20261012.md`.

## Componentes ya presentes
- SYSTEM artístico: `tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md` (prompt persistente ≠ entrenamiento de pesos).
- Contrato: `docs/VALORIA_LOCAL_AI_ARTIST_PERMANENT_CONTRACT.md` (puede contener estado antiguo; contrastar con CI).
- Generador: `tools/art-direction/build_cliff_talus_transition_v1.py`.
- Pipeline Qwen/Ollama y Blender: `.github/workflows/valoria-local-artist-production-trial.yml`.
- Integración visual real: `.github/workflows/valoria-local-talus-unity-trial.yml`.
- Prueba standalone #38232756248: SUCCESS; artefacto #11739820971 (`eldoria-local-talus-artisan-38232756248`).
- Prueba Unity #38234122670: SUCCESS; artefacto #11739318712 (`eldoria-local-talus-unity-38234122670`).
- Contrato del rol #38232756246: SUCCESS.

## Primera acción después de recuperar
1. Descargar inmediatamente ZIPs de las dos ejecuciones, porque los artefactos de Actions caducan (workflow del artista: 7 días). Archivar junto al repo, con hashes SHA256 y catálogo de imágenes, GLB, .blend, logs y decisión JSON. No creer que tener solo el commit conserva un artefacto temporal.
2. Antes de modificar código o ejecutar nada comprobar `git status`, versión del runtime, SHA de fuente, y contenido auténtico de los artefactos.
3. En el PC, confirmar `ollama list`, disponibilidad de Qwen3-VL local, Blender y BlenderMCP. El pipeline GitHub Ubuntu no valida por sí solo el PC Windows.
4. Utilizar el script `tools/art-direction/verify_local_artist_environment.py` para comprobaciones iniciales sin gastos ni cambios; revisar resultados y resolver únicamente dependencias faltantes.
5. Para cada nuevo asset: entrada imagen objetivo + captura Unity real + .blend de fuente; rol SYSTEM en llamada Ollama; plan JSON de geometría dirigido por el modelo; BlenderMCP/Blender sobre una variante; export GLB; import a Unity; captura A/B auténtica; dictamen independiente y registro histórico de fallos. No modificar GLB aprobado salvo promoción explícita.
6. Prioridad: conservar el talud realmente ejecutado y revisar su calidad A/B. Solo después corregir los otros siete, priorizando defectos grandes visibles desde cámara de juego.
7. Mantener ambos flujos aislados; integración mediante revisión manual/cherry-pick autorizado, nunca merge automático con `main`.

## Limitaciones comprobadas
Hay evidencia técnica de un ciclo Qwen/Blender/Unity, pero no se acredita aún calidad visual comercial, aprendizaje mediante ajuste de pesos, ejecución autónoma probada en PC Windows, evaluación independiente de todos los ocho assets ni optimización física Android/iOS. Reportar cada uno como PENDIENTE hasta prueba.

## Pruebas mínimas repetibles, sin Work
- Ejecutar inspección local pasiva: `python tools/art-direction/verify_local_artist_environment.py`.
- Comparar directamente registros/artifacts CI existentes antes de lanzar cualquier CI nueva.
- Hacer una sola variante pequeña con input congelado solo cuando sea necesario y no haya otro runner ocupando recurso.
- Medir un cambio visible usando captura equivalente en Unity, no solo exit code = 0.
