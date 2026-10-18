# ELDORIA / VALORIA — relevo posterior a V12 y prueba jugable V3 (17 oct 2026)

**Fuente de autoridad:** `main/AGENTS.md`, contrato de aceptación artística, protocolo de workstreams y el relevo de Work `NORMAL_CHAT_HANDOFF_20261017.md`. Este documento registra producción REAL posterior, sin sustituirlos.

## Identidad y exclusividad
- Repositorio: `mt5hjfz2kb-lab/Eldoria-Prewiu`.
- Rama exclusiva existente: `construction/valoria-master-plan-layers-20261016`.
- Mismo workstream `valoria-master-plan-layered-construction-20261016`; owner registrado en main y branch `chatgpt-normal-valoria-construction-20261017`.
- Sin reanudar 63 assets ni World4X ni modificar main Unity canónico. Coste 0 €. PC Windows solo mediante workflow existente y lease comprobado.
- HEAD comprobado ANTES de crear este relevo: `333a72d7a4ca6546d6af1e61f5450537446f9bac`. **Volver a leer HEAD vivo antes de editar**. No asumir que sigue siendo el actual.
- Dos WebGL startup probes históricas de 7 oct continúan en cola y no forman parte de producción Valoria; comprobar jobs/runner siempre.

## Resultado artístico recuperado
- V7 (conservar) `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_EntryZoneV7_CANDIDATE.unity`, BLEND original `docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/ValoriaEntryZoneV7.blend`, SHA-256 `24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381`.
- V8, V9, V10: técnicas exitosas pero rechazadas artísticamente (piedras dispersas/caminos pintados); no usar como base.
- V11: Poly Haven CC0 scan reusado sobre 9.248 vértices de orillas; visible desde Home insuficiente, no art aprobado. Run `39034382958`.
- V12: familia histórica `art-source/valoria/production/wall-family-v1/WallFamilyV1.blend` reutilizada y conservada, SHA-256 `0ac5ef48b55946d0820b8b064662f8d077060332044b077f2993d9444595f430`. Fuente de bounds reales: `entry-zone-v11-inspection/wall-source-world-bounds.json`.
  - Fuente editable nueva: `entry-zone-v12/ValoriaEntryZoneV12.blend` y GLB pareado, autoría `tools/valoria-layered-construction/author-entry-zone-v12.blender.py`.
  - Escena aislada `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_EntryZoneV12_CANDIDATE.unity`.
  - Se incorporaron 2 muros laterales y 4 torres reales con una sola reflexión X intencionada para la copia simétrica del WestPartialWall; no se movieron fuentes V7. EastPartialWall diagonal no seleccionado por incompatibilidad de bounds. Se desecharon TODAS las texturas/materiales del donante por licencia sin certificar; la geometría del proyecto usa el PBR/trim ya verificados de V7, sin descargas.
  - 351.374 triángulos de fuente en V12 frente a 342.006 de V7. 57 renderers visibles, 7/7 contactos, 28/28 rutas, 12 escalones, FOV HOME 44.4228096°, error centro puerta 34.196 px; glTFast espejo X tenido en cuenta.
  - Run técnico y render A/B real: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39035461560
  - PNG auténticos `entry-zone-v12/entry-BEFORE-home.png`, `entry-zone-v12/light-AFTER-home.png`, `entry-zone-v12/entry-AFTER-portrait.png` y referencia `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.
  - **Observación visual directa:** extensión perceptible de silueta de las murallas junto a puerta en iguales condiciones; la escena aún tiene puente/rocas amarillentas, agua plana, plataforma superior greybox, caminos incompletos y vegetación dispersa. Qwen 8B emitió `NO_VISIBLE_IMPROVEMENT` e incluso declaró falsamente que geometría V7/V12 era idéntica, contradiciendo la fuente y las capturas. Esta revisión automática NO certifica arte ni invalida por sí sola las diferencias de imagen. Etiqueta prudente: `TECH_PASS; LOCAL_SILHOUETTE_IMPROVEMENT_OBSERVED; ART_REVIEW_REQUIRED`, **NO `ART_APPROVED`**.
  - Conserva V7 y V12: no promover V12 al arte canónico ni usar nota 8/10 inventada.

## Prueba jugable V3 (terminada y conservada)
- Ya existían `ValoriaEntryExperimentalRuntimeV3.cs` y `Editor/ValoriaEntryPlayableV3.cs`; se actualizó SOLO la fuente de este último desde V8 a V12, reutilizando adapter y estado real de Eldoria, sin inventar sistemas jugables. V2 permanece preservada.
- Escena `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_Entry_PlayableV3.unity`, assets `GeneratedPlayableEntryV3/`, reportes `docs/evidence/valoria-layered-construction-20261016/playable-entry-v3/`.
- Run **SUCCESS** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39036053918
- Resultado `gameplay-playmode.json`: estado `EXPERIMENTAL_EXISTING_UI_I_II_OBJECTIVE_NAVIGATION_PERSISTENCE_RESET_PASS_NOT_DEVICE_QA`; todos TRUE: aserradero y cuartel UI, Bastión I→II, reclutamiento, marcha, capítulo II, navegación al mundo Frontier, vuelta a entrada, persistencia/reload, picking canónico, reset temporal y guardado original intacto. Resultado Revision 18, aserradero 1, cuartel 1, Bastión 2. Captura genuina `playable-entry-v3/playmode-built-i-ii-geometry.png`.
- Esto NO equivale a Android/iOS físicos, FPS móvil ni aceptación artística, y NO permite publicación.

## Siguiente acción exacta y estética
1. Preflight HEAD, owner en main y branch, Actions con colas, exclusividad Windows.
2. No seguir con caminos pintados ni variaciones de puerta; analizar la referencia versus `entry-zone-v12/light-AFTER-home.png` y la captura V3. Corrige uniones reales de muros a torres/suelo, materiales PBR del contacto y el agua/orillas con recursos CC0 ya archivados, en copia de V12 si mantiene una mejora claramente visible. La fortaleza superior greybox sigue pendiente, no vestirla con props aislados.
3. Si se decide avanzar en arquitectura superior, inspeccionar/medir de forma real `art-source/valoria/production/bastion-family-v1` y `defensive-family-v1` (ya existen); jamás asumir anclas cero ni deformar fuentes incompatibles.
4. Tener siempre A/B render genuino en cámara Unity, fotos sin trucos de iluminación. Contrastar con testigo independiente fiable; Qwen falla al percibir nueva silueta aunque modelos y PNG la prueban, así que no usar su dictamen como único juez.
5. Para cada candidato aceptado provisionalmente, repetir el flujo jugable de V3 con escena aislada, no tocar V2/canónico, guardados ni archivos de ramas pausadas.
6. Mantener destino artístico 8/10 sin proclamación gratuita. Salvar commit/run/PNG/veredicto; no dejar un trabajo simulado en progreso después de acabar chat.
