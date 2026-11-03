# Camino de los Vigilantes — B11 candidata de profundidad estructural

Fuente canónica: PR #136, base `76af72ff9aba44315f4a3d41a7edff4813839945`.
Propietario canónico registrado en main revisión 122 (B11); sin merge a main, sin publicación y sin gasto.

## Cambio material

B10 superponía seis recortes transparentes sobre una pintura que seguía conteniendo los mismos castillos, puente y acantilados. B11 conserva la pintura aprobada, original e inalterada, pero dibuja **un fondo alternativo limpio** (generado con máscaras originales y reconstrucción sin costes), sobre el cual viven las seis capas ya separadas. La ilustración aprobada sigue asignada a `RawImage.texture` y en recursos; el render usa `RoyalGoldV3VigilantsLivingLandscape.mainTexture` solamente durante Vigilantes. Exploradores usa la textura original de siempre.

Fuente del fondo: rama `art/vigilants-structural-b11-20261103`, `DepthB11/clean_backdrop.png`, SHA256 `e027d4257008ec52cf2488740a838f83ee06801d56544676dad00755ad4316da`, [CI source 41693861507](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41693861507) SUCCESS.

El seis-planos actual se conserva con tintes según distancia (lejos frío, cerca cálido) y movimiento controlado. No hay cámaras, Canvas, raycasts, trozos de UI horneados, ni modificaciones del combat engine, marcha, recompensa, guardado o número de etapas.

## Validación y criterio de rechazo

En EditMode: B11 fuente real 1536×1024, seis planos con alfa, canvas 0 extra, conservación de original, no propagación a Exploradores. En la captura nativa se exige que el `mainTexture` REAL sea el fondo B11 mientras `texture` sigue siendo la pintura fuente y hay seis cutouts bajo el mismo Canvas.

**Estado: CANDIDATA / ART_REVIEW_REQUIRED.** No igualar pipeline source PASS a Unity PASS. Exigir después capturas auténticas de B9/B10/B11 etapa 5, 15 y móvil, EditMode/PlayMode y visual inspeccionado. Si el nuevo fondo deja manchas, grietas, halo o resultado aún plano, RECHAZAR esa parte y reconstruir la forma dominante a través de malla de terreno y repintado profesional. La desaparición de duplicado es condición necesaria, no objetivo artístico ≥8,3 demostrado.
