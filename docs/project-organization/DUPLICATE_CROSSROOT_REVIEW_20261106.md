# Eldoria — auditoría de copias entre áreas (06/11/2026)

**Resultado: 42 grupos interárea clasificados; ninguna eliminación autorizada ni realizada.**

Origen: inventario reproducible fijado al commit `95c00907071b79fc8069e671117f8073aa1ccc86` y árbol `2d2c6eeb3773f94f80ad7788f58d2dc31779caaa`. Contiene 1025 grupos de blobs idénticos. **No es un contador vivo de `main`**. La igualdad de hash identifica bytes iguales en aquella revisión, pero no indica que una ruta sea prescindible: faltan referencias dinámicas, licencias y certificación de Unity para una eliminación física.

| Familia | Grupos | Decisión |
| --- | ---: | --- |
| Texturas de superficie entre fuente e importación Unity | 20 | Conservar ambas |
| Criaturas/retratos entre fuente e importación Unity | 4 | Conservar ambas |
| Arte original/referencia y evidencia histórica | 17 | Conservar ambas |
| Manifiesto de herramienta e importación Unity | 1 | Conservar ambas |

## 20 texturas — fuente e importación

Los PNG de `art-source/` son fuentes, los de `Unity/Assets/` son importaciones con rutas y metadatos propios. No borrar los importados sin comprobar materiales, GUID y consumidores reales; tampoco borrar sus fuentes.

- `02bdfa5ba92526aa3810f29c33192aac146fa4c1` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/stone_ao.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/stone_ao.png`
- `0a8cb474e3ee87c59de95cb53d716af5cb66a871` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/stone_albedo.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/stone_albedo.png`
- `0bfb66a28b3fdf3349f788bc124d0808d1810f36` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/shore_smoothness.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/shore_smoothness.png`
- `15bbb116cb5126d357c28b85ebfc04deca654459` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/shore_ao.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/shore_ao.png`
- `1fe26ff4869fa7524777045dd73600596578fe12` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/vegetation_normal.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/vegetation_normal.png`
- `295d29cbda922ae27711f165c4b5872eccbfa537` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/rock_smoothness.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/rock_smoothness.png`
- `2d84f05598a426e030c2e4e057aba6cb6ee8937a` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/ground_albedo.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/ground_albedo.png`
- `4c2af051496ca89bf628e2065c485855676a571f` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/shore_normal.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/shore_normal.png`
- `4e6cf36fbc37867a4846d8c1ceb23f19be653fa1` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/vegetation_albedo.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/vegetation_albedo.png`
- `5350e6f68c7ec947d09b504444e5664678f35063` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/shore_albedo.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/shore_albedo.png`
- `5abd30e5de6bb5a24301641cb1af811bbe8a21c5` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/rock_albedo.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/rock_albedo.png`
- `812d89e4b93de52ac4b8a7abf3956bebc8dc6f72` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/ground_ao.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/ground_ao.png`
- `9aada6a8dcc9a469ec0b799b418fb48256ee8a77` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/stone_smoothness.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/stone_smoothness.png`
- `a1237d16a1b3274cf854bc8a12201526251f8b12` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/rock_ao.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/rock_ao.png`
- `b8a91510102b12815ad46e3fa8f15cfd6d4baad9` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/ground_smoothness.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/ground_smoothness.png`
- `bd0b19e2f01c0de87fe9a4437961567ca1b92029` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/vegetation_ao.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/vegetation_ao.png`
- `cee6f4922ee1caef7ee20c2684cd311aa231d8e0` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/rock_normal.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/rock_normal.png`
- `d760e573d722aff80e03bf2d44b9e21abde558e4` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/stone_normal.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/stone_normal.png`
- `d825b15cc70061d72284ac4f836c47bdfe3dad9c` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/vegetation_smoothness.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/vegetation_smoothness.png`
- `eea5674949da82630ab72bd3b58eeb50200c721e` — `Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/ground_normal.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/surface-v2/ground_normal.png`

## 4 recursos de criaturas y retratos

Incluye modelos del Engendro, Jabalí y un retrato. Los originales artísticos y sus versiones importadas tienen propósitos distintos. Mantener hasta comprobar escenas, prefabs, cargas dinámicas y procedencia.

- `89a4a57230a5126e3a179e20a008c4073c464227` — `ArtSource/World4X/Spawnling_v1/Finished/finished_face.png` ↔ `Unity/Assets/Eldoria/Resources/EldoriaRetention/engendro-portrait.png`
- `91270ae59ca82bdcb84bffea03d68de931d0162a` — `ArtSource/World4X/Spawnling_v1/Finished/Spawnling_B2_Near.glb` ↔ `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/SpawnlingNear.glb`
- `a2e9dc7981240cf93787459b7a7de870e1303049` — `ArtSource/World4X/RockBoar_v1/RockBoar_Optimizado_49800.glb` ↔ `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/RockBoarNear.glb`
- `f6e602e4d9fcb1a83d303bac87dee8433fc3d3be` — `ArtSource/World4X/Spawnling_v1/Finished/Spawnling_B2_Far.glb` ↔ `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/SpawnlingFar.glb`

## 17 grupos fuente–evidencia

Capas de cámara, composiciones e imagen canónica de referencia. Conservar los originales y evidencias por trazabilidad: rutas idénticas en bytes pueden estar enlazadas desde informes distintos.

- `14f0c52fd46fc3da30a043211cf5e681b9c049a0` — `art-source/valoria/production/first-production-district-v1/first-production-district-v1-build-report.json` ↔ `pipeline/evidence/valoria-first-production-district-v1/source-report.json`
- `17e8ab201b91782b69e7bda60ff6cce3d774afdc` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/layer-2.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/layer-2.png`
- `220e326774efa7c6e74f8cc3546822cb82f04a40` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/vegetation-foreground.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/vegetation-foreground.png`
- `4a5a77e970bae4be38c43c48a35cdb8e82d27547` — `docs/evidence/valoria-reference-clean/reference-target.jpeg` ↔ `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- `4b0c89d8c9880a0df8c7e3f82af803fbb8799753` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/semantic-environment-far.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/semantic-environment-far.png`
- `553933d398eb25477f3cdf1f342183db84028f44` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/layer-3.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/layer-3.png`
- `5b4825ebe20f3fbd07a29e2fbff7529f18eaacd6` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/bridge-back.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/bridge-back.png`
- `5f6eb564b6bc476cb8247c5503c0dc102c7c6469` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/cliff-ground-shore.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/cliff-ground-shore.png`
- `7eb5901f540b795a172ace935916fe269260e5e6` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/semantic-environment-near.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/semantic-environment-near.png`
- `8a18894957b7dd94adbd42f4f447369549091086` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/layer-1.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/layer-1.png`
- `8d2e7e352e969047a74525b19e59edf9f2b16a17` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/gate-back.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/gate-back.png`
- `a5c5f0a5b992254b2e9130869595d7d543a81dfd` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/layer-0.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/layer-0.png`
- `ac27cd419c446ec5137fd4b94725df3139629aa8` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/vegetation-mid.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/vegetation-mid.png`
- `c80bfc60b9ebad995f57e27b3a340e0373cb9fc1` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/gate-front.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/semantic-gate.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/gate-front.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/semantic-gate.png`
- `cc541ff69bf789c435e24ec89246dcace2fc397a` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/bridge-front.png` ↔ `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/semantic-bridge.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/bridge-front.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/semantic-bridge.png`
- `d4ec1411eb81f0183913b8fbf15913f30f9499c0` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1/far-environment.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/far-environment.png`
- `da724309ad0d20eb9cb7579c60db3354a3ae9a96` — `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/canonical-golden-crop.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1/partition-roundtrip.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/canonical-golden-crop.png` ↔ `docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-reset-target-v1/semantic-roundtrip.png`

## 1 manifiesto herramienta–Unity

Dos JSON con igual blob pero ubicaciones/consumidores diferentes: material de una herramienta y manifiesto de importación. No confundir coincidencia de bytes con licencia validada.

- `302647c28a1da2ea69d58bdb090e42ffe5136f38` — `Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/license-manifest.json` ↔ `tools/valoria-reference-clean/rock-inputs.json`

## Condiciones para eliminar una copia

1. Regenerar el inventario contra un SHA actual de `main` y demostrar que ambas rutas siguen siendo idénticas y están bajo alcance disponible.
2. En Unity, identificar referencias serializadas, GUID, importadores, materiales, cargas por nombre/Resources y prefabs; ejecutar antes/después una prueba nativa y conservar la fuente editable.
3. En arte y evidencias, comprobar enlaces, autoría, procedencia y función de referencia/aprobación; no eliminar un original ni una evidencia de ejecución.
4. En recursos externos, verificar recibo, versión y licencia de uso comercial: mismo hash no acredita derechos.
5. Proponer una eliminación reversible únicamente para rutas sin consumidores, con validación y titularidad probadas.

**Alcance efectivamente realizado:** clasificación y decisión provisional de conservación para los 42 grupos; sin mover, borrar, republicar ni modificar recursos. No se afirma que los otros grupos de igual blob sean basura, ni que la licencia de los assets esté acreditada.
