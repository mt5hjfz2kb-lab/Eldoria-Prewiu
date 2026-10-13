# Valoria — Selección e integración aislada de assets V1

**Tipo de entrega:** catálogo y adaptación reutilizable para Unity. **No** sustituye ni modifica la escena jugable de Bastión I. **No** es una aprobación de calidad gráfica 10/10 ni certificación móvil.

## Fuentes y autoridad
- Auditoría de 36 GLB: `audit/valoria-legacy-glb-20261013/docs/evidence/valoria-legacy-glb-audit-20261013/RESULT.md`, Actions #38393822753.
- Inventario SHA-256 de 26 GLB: `pipeline/asset-library-canonicalization-audit.json`. Ningún GLB original ha sido reescrito.
- Familia de ocho Hero V3: fuentes inmutables del commit `0abf13870d21725c30acf3f3e3a8c86385470073`. **Se referencian** y se dejan bajo su actual propietario de integración, sin duplicarlas ni desplazar su trabajo.
- Puente/escaleras: referencias externas, no seleccionadas para integración automática.

## Selección de diez piezas inicialmente reutilizables
`hero_bastion_legacy`, `stone_corner_l`, `stone_wall_high`, `stone_rock_transition`, `stonekit_02`, `rock_platform`, `stepped_rock_terrace`, `sawmill_ap2`, `barracks_ap2`, `granary_biii`.

Selección prudente: la valoración visual 6.5–7.5/10 indica **candidatos**, no un nuevo ART_PASS; `hero_bastion_legacy` no pretende reemplazar Hero V3. Se mantienen nueve piezas secundarias/StoneKit como **CONDITIONAL**, cuatro con material problemático como **MATERIAL_DIAG**, dos como **SCENERY_ONLY** y una como **WORLD_ONLY**. La lista exacta, justificaciones, SHAs y roles figuran en el JSON del catálogo.

## Integración opcional en Unity
1. `Unity/Assets/Eldoria/AssetLibrarySelectionV1/Resources/EldoriaSelectedAssetCatalogV1.json`: catálogo estable de 36 entradas con puntuación, clasificación, recurso Unity, ruta de origen y hash.
2. `EldoriaSelectedAssetLibraryV1.cs`: API `Lookup(id)` / `SpawnSelectedVisual(id,parent,position,rotation,scale)`. Solo admite `SELECTED`; instancia el prefab ya generado o el GLB importado por glTFast. Desactiva colisiones y comportamientos en el clon. **No se conecta a ninguna escena ni se ejecuta automáticamente.**
3. `Editor/EldoriaSelectedAssetLibraryBuilderV1.cs`: herramienta explícita `Eldoria/Art Library/Build Selected Prefabs V1 (isolated)`. Verifica el SHA-256 de las diez fuentes originales, instancia el modelo real importado, elimina de la copia sus colliders/MonoBehaviours y escribe prefabs únicamente bajo `AssetLibrarySelectionV1/Resources/EldoriaSelectedAssetsV1/`.
4. `Eldoria/Art Library/Capture Selected Prefab Gallery (isolated)` genera una escena temporal **no guardada** y captura de Unity con las diez piezas, sin tocar Valoria.unity o el gameplay. No da ART_PASS automático.

**No hay auto-promoción, render de referencia falso ni copia de la familia congelada Hero V3 a la rama.** No hay nuevos créditos ni coste de licencia.

## Pruebas
- CI independiente en `ubuntu-latest`: 26 fuentes originales y `.meta` encontrados, tamaño, SHA-256, unicidad, categorías válidas, 10 seleccionados, origen exacto de Hero V3, alcance Unity sin conexiones implícitas.
- Prueba con Unity 6000.3.23f1 en Windows **pendiente** hasta que el propietario de Bastión I libere `windows-runner-heavy`. Se deben ejecutar `Build` y `Capture` en ese orden, revisar `SelectedAssetLibraryV1Captures/import-evidence.json` y `selected-prefabs-unity.png`, confirmar shaders/texturas y presupuesto móvil antes de considerar `UNITY_PASS`.
- La validación estática no puede certificar los prefabs generados ni sustituye Unity. Los archivos originales conservan su propietario y los nuevos prefabs están estrictamente fuera de la escena de juego.

## Qué falta para cerrar del todo la integración
1. Ejecutar editor Unity real en un runner permitido, generar los diez prefabs e inspeccionar su galería.
2. Guardar evidencias verificadas y decidir promoción individual. Nunca alterar automáticamente Bastión I ni `main` por un PASS estático.
3. Si se detectan materiales negros o shaders rotos, corregir importador/compresión por pieza, sin rehacer las mallas.
