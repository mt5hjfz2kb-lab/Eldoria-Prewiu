# Valoria — Spatial Coherence 2.5D: isolated A/B pilot

**18 November 2026 · status: SOURCE IMPLEMENTED, NATIVE QA NOT EXECUTED, ART NOT APPROVED.**

**Source branch:** `art/valoria-spatial-coherence-20261118`, based on `10dbc197d48ea8517c6a3dd6c22f1a2abf75d508` from the City2D V6 branch.
**Owner-protected baseline:** `art/ciudad-2d-hitos-1-5-20261116`. The registered City2D workstream is *claimed by a separate chat* and has the `windows-runner-heavy` resource reserved on main at HEAD `904ff183ffb9053f68b9c0f359b2e9d3d40ce805`; DO NOT overwrite its folder, run native Windows jobs on that reservation, create a competing PR, or alter main.

## Precise scene evidence, not impressions

The **immutable control A** is the pre-existing archived native scene:
`Unity/Assets/Experimentos/Ciudad2D/QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity`.
Its existing Git blob SHA-1 at branch creation is `4785b9bdbd77a5f700a2fd4d835ac3ac39c3e362`.

Earlier authentic Unity Editor V6 images are preserved in `QA/NativeV6Scale/` and their manifest; `docs/ciudad2d/CIERRE_VALORIA_PREMIUM_V6_20261117.md` records [native run 44133747143](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44133747143) and 1 successful PlayMode test. **That does not validate this new branch B.**

\n### Metadatos de escena A archivada (defecto de procedencia detectado)\n\nEl fichero original `ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity.meta` contiene los caracteres literales `\\n` en lugar de saltos de línea reales; su importación en un checkout nuevo requiere verificación Unity. **No se corrige ese archivo reclamado por el otro chat.** Para no depender de ese `.meta`, este piloto incorpora `Scenes/Valoria_Spatial_A_Control.unity` como copia de los mismos 228583 bytes de YAML y un `.meta` nuevo con saltos válidos. La utilidad de captura abre esa copia A; el verificador estático comprueba igualdad binaria con el archivo archivado. No se trata de una nueva ilustración ni cambia los recursos compartidos.\n
New **scene B** (verifiable separate Unity scene asset):
`Unity/Assets/Experimentos/ValoriaSpatial25D/Scenes/Valoria_Spatial_B_Pilot.unity`.

It reproduces A's Unity scene content with ONE new presentation-only bootstrap. It never alters the archived A asset, production scenes, ProjectSettings, economic mechanics, saved data, building rules, HUD or World4X. An optional synthetic C hybrid is **not created**, because superiority of B is not yet proven.

## Causal audit from the source files (investigated, not a visual pass)

1. Ground: one rich, hand-painted `Terreno_Organico.png` sprite. **Its painted walls, skyline, major trees, roads and paths cannot independently occlude people or change depth**. Most art richness is retained in a single source, but spatial interaction is limited.
2. Buildings: six separately positioned `CityBuilding` instances; four Premium V6 candidates with independent RGBA shadows and read-only plot-state gating. The whole building sprite sorts as a single object, not by facade/wall sections.
3. Props: 23 standalone vegetation/architecture sprites from one JSON layout. The original builder ALREADY derives their render order from vertical position; the problem is not that Y sorting is completely absent. Pivot `BottomCenter`, PPU 100 and original `OrderFor` are in the source, not a hypothesis.
4. Camera: one orthographic camera with pan/pinch; source objects share the same image-space plane. *Parallax between them would be physically wrong.* Distant skyline requires separate treatment **only if a genuinely distinct distance layer is authored**.
5. Atmosphere: 7 pulsing halo overlays plus smoke and embers, not pedestrians. Glows use `sortingOrder=9000` and need local light occlusion review. An animated smoke overlay isn't equivalent to a living city.
6. Existing shadows and source lighting are baked RGBA, not a unified re-lightable material. A universal warm LUT cannot change conflicting shadow directions or erase rectangular bases.
7. Known static aesthetic limitations in the archived review: Cuartel close to portrait left safe edge, Granero shape reads as an inn, possible Cantera platform silhouette. **These cannot be certified corrected by this technical pilot.**

## Implemented source-only B (awaiting Unity confirmation)

- Y-ordering at the *world foot* of independently movable props, buildings, independently sorted tree and three workers; linked existing V6 shadow order synchronized behind its building.
- An actual independent tree near the Aserradero, intended to occlude the woodcutter during movement. It reuses the existing `Props/ArbolAntiguo.png`; **no imported 3D asset, no new illustrative cutout**.
- Three tiny visual-only proxy characters: woodcutter at Aserradero, recruit at Cuartel, porter at Granero. Procedural poses and contact ellipses are deliberately a spatial QA proxy, **NOT art acceptable for the game**. Paths run around each building's outside footprint; no new economy, collision, navigation mesh or save schema. The building ownership and construction state control their visibility. They are not a general pathfinding solution.
- Deterministic `SampleAt(seconds)` controls snapshots; normal PlayMode movement uses real `Time.deltaTime`. A moving character can have a different sort order from an independent tree. **Correct on-screen masks and overlap still require native verification.**
- Native Editor evidence code: `SpatialPilotNativeEvidence.Capture` loads A and B independently; common camera, viewports, level fixtures and timestamps; records 8 static views per variant plus **36 real Unity RenderTexture camera-motion frames** per variant (44 A + 44 B = 88 PNG total if executed), SHA-256 and provenance `evidence.json` out of `Assets`. This tool has NOT been executed. A video/GIF would still need assembly from those frames; no fake photo-scrolling is accepted.
- A/B source-integrity script `QA/validate_spatial_sources.py` compares archived A exactly against B after removing the one added bootstrap GameObject and SceneRoots entry. It also validates the script GUID, preserved plot IDs and lack of game-state access. This is a **static check**, not a substitute for Unity.

## Native QA execution once the existing resource lease is released

Menu: **Eldoria → Experimentos → Valoria Spatial → Captura nativa A-B (sin tocar main)**.

For headless Unity, after obtaining a verified free runner and isolated checkout:
```text
Unity -batchmode -nographics -projectPath <checkout>/Unity -executeMethod Eldoria.Experimentos.ValoriaSpatial25D.Editor.SpatialPilotNativeEvidence.Capture -quit -logFile <log>
```

Open `Valoria_Spatial_B_Pilot.unity` in Unity 6000.3.23f1 and click Play to examine actual gestures and time-based worker paths; do not add it to production Build Settings. If any Editor compile errors occur, **fix and rerun before saying the feature works**. The frame-capture runner must not preempt another job. Before running, ensure source checkout uses this experiment's exact HEAD. Keep A/B PNGs, native logs, SHA manifest and any NUnit results. Derive GIF/MP4 from **actual Unity frames** if capture succeeds.

Reproducible static check, not Unity:
```text
python Unity/Assets/Experimentos/ValoriaSpatial25D/QA/validate_spatial_sources.py
```

## Required artistic/technical gates (NOT PASSED)

- A/B inspection of 88 REAL captured frames across 390×844 and 1170×2532, plus 844×390; look specifically for the worker passing in front and behind the tree, building/ground contacts, visibility at I/III/V, and continuity during camera pan/zoom.
- Prove `A == B` when bootstrap is disabled. Compare pixels with identical settings; any differences would indicate hidden side effects.
- Repeated mobile PlayMode tap/selection test: all six IDs and plot states unaffected; no new GameObject steals click.
- **Hard remaining problem**: major foreground wall, trees, roads and paths baked into terrain are not modular. Need a measured, source-aligned alpha-foreground extraction **from this exact painted ground** or newly painted seamless modular patches, not guessed rectangular masks and not simplified green ground. This source change cannot be promoted without true Unity A/B and original art inspection. No unsupported claim that B fixes the collage.
- Confirm sprite/pivot-to-ground measurement for Cuartel and Granero with actual foot contact points. Worker figure height ~0.6 Unity units is a provisional QA scale, not an approved proportional system. Calibrate one adult, door, cart, pathway and wall at normal iPhone size.
- Lighting: measure painted light direction against Bastion V and shadow gates. Don't add indiscriminate bloom, glow, extra dynamic lights.
- GPU texture memory, sprite count, draw calls, transparencies/overdraw, WebGL load and device FPS via Unity Profiler/Frame Debugger on the eventual target; none measured by this branch. Physical iPhone/Android check pending. WebGL not published.

## Reference reasoning (no game-engine attribution)

Visual references observed in public screenshots:
- *Whiteout Survival*: focused furnace, compact readable structures, small figures and unified snow/ground cues. Screenshot does **not** prove a particular rendering technology: https://www.suara.com/tekno/2025/02/28/131809/review-whiteout-survival-game-bangun-kota-menarik-di-dunia-es
- *Top Heroes*: paths/ground, visibly separate building footprints, many readable small inhabitants and point-of-interaction gestures. Screenshot does **not** prove implementation details: https://topheroes.blog.fc2.com/blog-entry-24.html
- Unity's documented **Transparent Queue Sorting priority**: Sorting Layer and Order in Layer are evaluated before custom-axis distance; explicitly setting order by position is valid but requires contact pivots. https://docs.unity.cn/6000.0/Documentation/Manual/sprites-sort.html and https://docs.unity.cn/Documentation/Manual/2d-renderer-sorting.html

**Lesson:** Commercial impression depends on coherent layout, contact, overlap and motion, not isolated sprite resolution. Preserving Premium V6 texture quality is a hard constraint. Current pilot improves the *testability of occlusion* but does not prove commercial art quality.

## Owner-safe integration if B earns approval

Keep B's `ValoriaSpatial25D` code and scene segregated. Selectively reuse only components shown better in the A/B proof. Bind visual actors to the existing `CityBuilding`/gateway read-only snapshots on the authorized functional integration branch, not an independent state cache. Respect player-built flags, touchscreen selection and state when reopening saves. Never cherry-pick an entire YAML scene over someone else's functional modifications. Never merge or publish before explicit approval.

## Status matrix

| Item | State |
|---|---|
| Source A and provenance discovered | **INVESTIGATED / VERIFIED IN GIT** |
| Isolated branch and B scene/source committed | **IMPLEMENTED IN GIT** |
| Three technical worker proxies and Y-order tree | **IMPLEMENTED IN GIT**, runtime **UNVERIFIED** |
| Native frame-capture procedure | **IMPLEMENTED IN GIT**, **NOT EXECUTED** |
| Unity Editor PlayMode B, touch, animation | **PENDING** |
| Modularity of major terrain/wall/road layers | **PENDING / CAUSAL LIMITATION** |
| Real A/B comparisons and video | **PENDING** |
| Commercial visual quality 8–8.5/10 | **NOT ESTABLISHED** |
| Performance, WebGL, physical devices | **PENDING** |
| Main and functional branch changes | **NONE** |

### Corrección preventiva de compilación tras comparar el piloto paralelo (18/11)

El piloto de profundidad `art/valoria-25d-profundidad-piloto` encontró en una prueba Unity auténtica errores `CS0234` porque `Application` fue interpretado como el namespace `Eldoria.Application`, no como la clase Unity. Se han cualificado todos los accesos del sandbox espacial mediante `UnityEngine.Application` y añadido guardia en la prueba estática. **La prueba nativa de esta rama sigue pendiente.** La rotación registrada en main pertenece a Ciudad2D y al piloto de profundidad canónico, no habilita por sí sola otro acceso paralelo desde esta rama.

### Prueba B1 — segmentos de muralla desde los píxeles originales (sin repintar)

IMPLEMENTADO EN FUENTE; **NO APROBADO EN UNITY**. Cinco regiones convexas extraen visualmente fragmentos de la muralla frontal de `Terreno_Organico` en el plano original; su malla reproduce los mismos UV de la misma textura del terreno. Esto no crea un atlas nuevo, no sustituye terreno por superficies simples, no mueve los edificios con paralaje artificial y no difumina las bases de ningún edificio.

El piloto usa un `MeshRenderer` transparente por fragmento y orden de render determinado por la coordenada de apoyo de esa muralla. Permite que un actor detrás del tramo quede realmente oculto sin repintar el original. Tiene un riesgo real: si el polígono abarca suelo, puede tapar al actor donde no corresponde, y si las muestras del shader no coinciden con el sprite original, puede producir bordes visibles. **Rechazar y redibujar los contornos tras capturas Unity antes de extenderlo.** Fuente: `SpatialTerrainForeground.cs`, `Shaders/SpatialForegroundCrop.shader`, `Data/foreground_contours_v1.json`. No hay perfiles GPU o pruebas móviles.

### Corrección de contornos v2 tras inspeccionar la textura original

La prueba automática `44280576195` produjo una evidencia **offline auténtica** desde `Terreno_Organico.png` (941×1672). La reproducción de la misma fuente bajo siete máscaras tiene diferencia de píxeles cero en el escenario estático. Al revisar la previsualización se comprobó que las cinco regiones iniciales atravesaban la abertura del portón y sobrepasaban parte de la muralla: se **rechazaron esas coordenadas**. Se han sustituido por siete regiones más precisas: ala oeste, torre oeste, pilar oeste, arco superior, pilar este, torre este y ala este, manteniendo abierto el vano central. Las siete siguen siendo una prueba de composición, no una máscara oclusora aprobada sin imágenes en movimiento de Unity.

### Personaje que atraviesa el acceso (prueba de oclusión, no de arte de personaje)

Se añade un cuarto figurante exclusivamente al sandbox B, vinculado al Bastión, que sigue la calzada central desde el patio hasta el exterior por el vano real de la puerta. No atraviesa las alas de piedra. Cruza la posición de profundidad del arco, y el capturador exige que su orden de render quede detrás y delante del muro en instantes distintos. Las seis tomas finales de la secuencia se enfocan en esa puerta; también se añaden vistas móviles y retina de detalle. El resultado sigue pendiente de Unity y una revisión de que el *dibujo*, no solo el número de orden, oculte correctamente al visitante.
