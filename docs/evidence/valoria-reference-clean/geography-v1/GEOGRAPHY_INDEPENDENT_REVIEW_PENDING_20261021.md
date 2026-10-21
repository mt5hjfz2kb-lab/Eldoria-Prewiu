# Geography V1 — visual evidence review, 21 October 2026

**Status: NOT ART_APPROVED; current canonical scene unchanged.** This note describes genuine captured candidate images and is **not** an independent reviewer approval or authorization to integrate/publish.

## Source and tested captures

- Canonical Unity scene before trials: SHA-256 `4c39edeb8fa03f6b7235dc7bad0a041b06de9384b8e8bd6f1e3c23e8cf860237`; six POI identities and locations protected.
- Native260: [GitHub Actions run 39640945963](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39640945963), genuine same-scene A/B (review artifact `12364538958`). In the forest frame the measured 21-25 mature-tree candidates increase visible canopy; visual difference is real, yet the whole 4X sector still contains large uniform meadows.
- Native261: [run 39641901228](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39641901228), evidence artifact `12365323917`, 25 added visual tree meshes, unchanged 28,800 source-world ground triangles and intact original UV/roads. Quarry contact varies more than native260, but still reads as an isolated excavation with dark shading against homogeneous grass.
- Native262: [run 39643122106](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39643122106) **did not execute the Unity stage**: live Windows lease was reassigned to ongoing UI V3 management certification. This is a resource guard, not a failed artistic build; the low meadow height sculpt in current source remains untested.
- Native260 and native261 **trial** evidence only. The images and `trial.json` do not certify game performance or user-device behavior, and no corresponding `ART_APPROVED` record has been established.

## Direct visual observations

- **Valoria:** castle, bridge and river remain attractive anchor pieces; green prairie and extremely geometric path edges dominate the image. Soil-color mixing on its own does not resolve the tabletop impression. Current candidate's low raised meadow geometry has **not** been rendered on an authorized Windows runner.
- **Region 1:** adult crowns give a more legible forest mass; timber access remains recognizable. Region 1 as a whole still reads as landmarks on a large grassy rectangle. Quarry soil/rock transition and healthy-to-corrupt biome gradients remain conspicuous, and the landscape lacks natural large-scale continuity.
- **No certified numeric score improvement:** baseline ~6.6/10 city and ~5.3/10 world are owner-provided visual estimates, not calibration measurements. Do **not** infer 8.0/7.5 from CI or changed pixels. A genuinely independent reviewer must assess complete matched 4X + city images before integration.
- **Camera metadata defect corrected:** native260 camera `*-camera.json` files contained unescaped carriage returns from Unity matrix formatting. Source commit `6845204bd70930e66bee957cc29365080b10ed46` switches subsequent capture metadata to Unity `JsonUtility`; this correction still requires a future exact native run for materialized proof.

## Required next gate

1. Wait for the **existing** UI V3 owner to release `windows-runner-heavy` in the canonical registry; never steal/cancel their certification. Re-read `main`, scope and latest committed request first.
2. Restore/reserve Windows under the same art workstream, then run the current untested meadow/forest/quarry candidate in native Unity with landscape, portrait and entire real Region 1, respecting the live ownership revision.
3. Review architecture and geography from identical camera matrices and fixed progression. Reject disconnected mesh bases, flat dark contamination halos, altered road atlas, floating/hidden forest copies and any gameplay changes.
4. Only after real A/B + independent visual acceptance perform reversible integration and native I–II, save/reload, UI V3, Engendro, camera/picking and performance checks. Publishing Android/itch is separate and not authorized by this work.

**No asset or gameplay file is changed by this review.**

## Continuación source-safe y corrección de contacto — 21-10-2026

- Se retomó el mismo workstream, sin crear una rama ni reclamar el Windows ocupado por UI V3.
- [`bc42c984`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/bc42c984248f013a4c383aac4fd8431c5a49f0ee): las coníferas copiadas consultan la **malla visual candidata** para medir su apoyo; antes se apoyaban incorrectamente sobre el mapa de alturas antiguo.
- [`15ba6535`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/15ba653578e2e49bca373a56bc6d6006e0fe644e): el relieve regional modificado se atenúa alrededor de los troncos adultos existentes para evitar enterrarlos, preservando el atlas y las coordenadas/ID de POI.
- La prueba ligera de código/arquitectura sobre esa modificación concluyó sin fallos. Esto no certifica captura ni arte. Ninguno de estos cambios ha modificado la escena canónica `Valoria_City_World_UI.unity`.
- El `request.json` vigente permanece en `geography_trial`, revisión **262**. El intento anterior native262 no abrió Unity por la reserva vigente de `windows-runner-heavy` en UI V3. El siguiente ensayo debe incrementar revisión tras reservar y comprobar en vivo el mismo propietario/escena/artefacto.
- Las capturas native261 de la Región 1 completa y de Valoria se conservan como A/B histórico. Materiales/caminos originales están presentes, pero **no hay ART_APPROVED, mejora 8/10 o 7,5/10, prueba física Android, ni entrega del nuevo relieve**. La revisión Qwen existente también requiere Windows/Ollama y no ha sido ejecutada para la candidata nueva.

**Puerta siguiente:** liberar la reserva del trabajo UI V3 por su propietario; recuperar la reserva bajo el mismo workstream de Valoria; ejecutar nueva prueba Unity con capturas A/B desde cámaras idénticas y métricas de contacto árbol/terreno original y candidato, luego valoración independiente y QA I–II; solo entonces considerar la integración en main. Coste 0 €, no publicar.
