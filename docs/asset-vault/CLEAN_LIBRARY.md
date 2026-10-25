# Eldoria — Biblioteca 3D depurada y utilizable

**Catálogo operativo.** Esta página muestra solo las piezas realmente seleccionadas para reutilizar, no el almacén indiscriminado de archivos.

> **Seguro:** no hemos borrado activos que Unity/Work puedan necesitar. La limpieza consiste en separar y bloquear los 536 recursos de packs externos sin licencia revisada, prototipos y versiones rechazadas de la selección que utiliza producción. No existe promoción automática de basura histórica.

## Resultado certificado de la limpieza

| Resultado | Cantidad |
|---|---:|
| Para sus funciones de producción ya documentadas | **5** |
| Módulos arquitectónicos reutilizables, sujetos a composición | **20** |
| Monumento conservado pero excluido de navegación/circulación | **1** |
| Recursos de terceros ocultos de la búsqueda habitual por licencia sin verificar | **536** |
| Glb realmente inspeccionados (estructura, SHA-256, `.meta` Unity) | **26/26** |
| Variantes 3D históricas accesibles en modo investigación | **920 rutas / 923 versiones** |

Para consultar antes de modelar: `python3 tools/asset-vault/find-reuse.py "aserradero"`. Para investigar modelos no aprobados: añadir `--research`. **Las dos búsquedas no crean recursos ni alteran Unity.**

## 1. Producción: reutilizar el mismo modelo en su función existente

### Valoria_HeroBastion_v1

![Geometría de Valoria_HeroBastion_v1](previews/Valoria_HeroBastion_v1.png)

**Función:** Valoria central fortress.  **Geometría:** 49.800 triángulos.

**Reutilizar en Unity:** `Resources.Load("Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1")` (según importador GLB configurado).
**Ruta física:** `Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb`.

**Restricción:** Validated hero anchor; do not replace with older silhouette.; HEAVY_MODEL: single landmark or rare instance only unless platform performance and LOD tested.

### Valoria_Aserradero_AP2_v1

![Geometría de Valoria_Aserradero_AP2_v1](previews/Valoria_Aserradero_AP2_v1.png)

**Función:** Valoria sawmill.  **Geometría:** 49.800 triángulos.

**Reutilizar en Unity:** `Resources.Load("Valoria/Valoria_Aserradero_AP2_v1")` (según importador GLB configurado).
**Ruta física:** `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb`.

**Restricción:** Dedicated production aserradero.; HEAVY_MODEL: single landmark or rare instance only unless platform performance and LOD tested.

### Valoria_Cuartel_AP2_v1

![Geometría de Valoria_Cuartel_AP2_v1](previews/Valoria_Cuartel_AP2_v1.png)

**Función:** Valoria barracks.  **Geometría:** 49.800 triángulos.

**Reutilizar en Unity:** `Resources.Load("Valoria/Valoria_Cuartel_AP2_v1")` (según importador GLB configurado).
**Ruta física:** `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb`.

**Restricción:** Dedicated production cuartel.; HEAVY_MODEL: single landmark or rare instance only unless platform performance and LOD tested.

### Valoria_Granero_BIII_v1

![Geometría de Valoria_Granero_BIII_v1](previews/Valoria_Granero_BIII_v1.png)

**Función:** Valoria granary (Bastion III art source).  **Geometría:** 49.800 triángulos.

**Reutilizar en Unity:** `Resources.Load("Valoria/Valoria_Granero_BIII_v1")` (según importador GLB configurado).
**Ruta física:** `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb`.

**Restricción:** Production asset, not permission to activate Bastion III gameplay.; HEAVY_MODEL: single landmark or rare instance only unless platform performance and LOD tested.

### PlayerCity_v1

![Geometría de PlayerCity_v1](previews/PlayerCity_v1.png)

**Función:** 4X strategic player city.  **Geometría:** 22.797 triángulos.

**Reutilizar en Unity:** `Resources.Load("WorldPlayerCity/PlayerCity_v1")` (según importador GLB configurado).
**Ruta física:** `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb`.

**Restricción:** Universal strategic player-city mesh with identity/state separate.; CONDITIONAL: profile instances, materials, LOD and actual mobile frame budget.

## 2. Kit modular: útil, pero necesita integrar y revisar

Estos módulos existen, tienen el GLB y `.meta` verificados y un uso documental concreto. **No están aprobados automáticamente para cualquier nueva escena o bioma**.

| Modelo | Uso | Triángulos | Unity Resources | Estado |
|---|---|---:|---|---|
| [Piece01](previews/Piece01.png) | Valoria modular mid-tier wall/building | 11.997 | `Valoria/MidTierArchitectureKit_v1/Piece01` | CONDICIONAL |
| [Piece02](previews/Piece02.png) | Valoria modular mid-tier wall/building | 13.201 | `Valoria/MidTierArchitectureKit_v1/Piece02` | CONDICIONAL |
| [Piece03](previews/Piece03.png) | Valoria modular mid-tier wall/building | 16.650 | `Valoria/MidTierArchitectureKit_v1/Piece03` | CONDICIONAL |
| [Piece04](previews/Piece04.png) | Valoria modular mid-tier wall/building | 6324 | `Valoria/MidTierArchitectureKit_v1/Piece04` | CONDICIONAL |
| [ResidentialTerraceRock](previews/ResidentialTerraceRock.png) | inhabited/civic support mass inside already-approved production topology | 49.800 | `Valoria/Rescued/ResidentialTerraceRock` | CONDICIONAL |
| [RockTerrainSeamFiller](previews/RockTerrainSeamFiller.png) | terrain seam burial/overlap around production buildings and terraces | 49.800 | `Valoria/Rescued/RockTerrainSeamFiller` | CONDICIONAL |
| [StreetLandingTransition](previews/StreetLandingTransition.png) | visual street/landing dressing over an independent gameplay route | 49.800 | `Valoria/Rescued/StreetLandingTransition` | CONDICIONAL |
| [TerraceStairRock](previews/TerraceStairRock.png) | visual vertical transition/terrace fragment where gameplay route already exists independently | 49.800 | `Valoria/Rescued/TerraceStairRock` | CONDICIONAL |
| [TowerWallRock](previews/TowerWallRock.png) | defensive skyline accent / wall-rock anchor | 50.000 | `Valoria/Rescued/TowerWallRock` | CONDICIONAL |
| [CornerWallL](previews/CornerWallL.png) | Valoria retaining masonry / wall | 7335 | `Valoria/StoneArchitectureKit_v1/CornerWallL` | CONDICIONAL |
| [HighStraightWall](previews/HighStraightWall.png) | Valoria retaining masonry / wall | 6440 | `Valoria/StoneArchitectureKit_v1/HighStraightWall` | CONDICIONAL |
| [RockToWallTransition](previews/RockToWallTransition.png) | Valoria retaining masonry / wall | 7270 | `Valoria/StoneArchitectureKit_v1/RockToWallTransition` | CONDICIONAL |
| [piece_01_10364tris](previews/piece_01_10364tris.png) | stone sheet modular subset | 10.364 | `Valoria/StoneKit/piece_01_10364tris` | CONDICIONAL |
| [piece_02_12602tris](previews/piece_02_12602tris.png) | stone sheet modular subset | 12.602 | `Valoria/StoneKit/piece_02_12602tris` | CONDICIONAL |
| [piece_03_6433tris](previews/piece_03_6433tris.png) | stone sheet modular subset | 6433 | `Valoria/StoneKit/piece_03_6433tris` | CONDICIONAL |
| [piece_04_7824tris](previews/piece_04_7824tris.png) | stone sheet modular subset | 7824 | `Valoria/StoneKit/piece_04_7824tris` | CONDICIONAL |
| [piece_05_4966tris](previews/piece_05_4966tris.png) | stone sheet modular subset | 4966 | `Valoria/StoneKit/piece_05_4966tris` | CONDICIONAL |
| [piece_06_7602tris](previews/piece_06_7602tris.png) | stone sheet modular subset | 7602 | `Valoria/StoneKit/piece_06_7602tris` | CONDICIONAL |
| [BroadRockPlatform](previews/BroadRockPlatform.png) | Valoria buried rock/terrace support | 8650 | `Valoria/TerrainTerraceKit_v1/BroadRockPlatform` | CONDICIONAL |
| [SteppedRockTerrace](previews/SteppedRockTerrace.png) | Valoria buried rock/terrace support | 6379 | `Valoria/TerrainTerraceKit_v1/SteppedRockTerrace` | CONDICIONAL |

## 3. No incorporar como infraestructura transitable

Estos archivos permanecen rescatables en Git, pero no se ofrecerán como piezas normales de producción.

| Modelo | Uso | Triángulos | Unity Resources | Estado |
|---|---|---:|---|---|
| [GateStreetRiseRock_MV1](previews/GateStreetRiseRock_MV1.png) | Monumental entrance fragment | 49.799 | `Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1` | SOLO MONUMENTO |

## 4. Lo que hemos quitado de la selección normal

Se deja **fuera de la biblioteca activa** todo el material que no pueda reutilizarse con razonable seguridad: packs externos pendientes de licencia individual, diseños rechazados, prototipos de ramas antiguas, modelos duplicados, objetos de prueba y versiones caducadas. Los archivos siguen conservados en sus rutas Git para no romper referencias, licencias, escenas ni pruebas antiguas.

Duplicados exactos detectados: seis copias de una roca de pruebas en distintas etapas de Valoria, y las copias `MegaGate`, `MegaWall`, `MegaTower` que ya existen en Mega Fantasy Props Pack. La duplicación está documentada; **ninguna se ha borrado sin verificar referencias de prefab, .meta, GUID, escenas y código**.

## 5. Evaluación técnica real

**CI [40220473133](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40220473133):** 26/26 GLB válidos, con mallas dibujables, SHA-256 coincidente con inventario anterior y metadatos Unity. Suman aproximadamente 655.033 triángulos entre todos. Hay diez piezas de 40.000 triángulos o más; son candidatos pesados si se repiten masivamente en móvil. **No equivale a ensayo de rendimiento Android/iPhone ni aprobación estética de la escena.**

## 6. Fuentes existentes fuera del estante

El engendro, el jabalí y sus versiones Tripo se conservan en el [catálogo de fuentes](../../pipeline/tripo-art-brain/catalog.json), y la nueva Región 1 sigue en PR #100, sin ART_APPROVED. En la búsqueda aparecen como **fuentes disponibles con restricciones**, no como permiso de regenerar, duplicar enemigos o publicar contenido de Bastiones futuros.

## 7. Contrato de mantenimiento

1. Todo `.glb`, `.blend`, `.fbx`, `.obj`, `.gltf` o `.stl` nuevo o modificado en Git se detecta como candidato. No se incorpora automáticamente a producción.
2. Solo se amplía esta estantería tras validar fuente, licencia, geometría, LOD, ubicación de uso, captura Unity y criterio artístico actual.
3. Al construir un elemento nuevo, hay que demostrar que se ha consultado esta selección y explicar por qué no sirve ningún modelo existente.
4. Work conserva el control exclusivo sobre los assets actuales de Región 1. Los dos nodos nuevos se incorporarán cuando estén realmente terminados y aprobados.
5. Nadie elimina activos de dependencias desconocidas. Retirar de la estantería no significa borrar el archivo, su historia ni su licencia.

**Metadatos legibles por máquina:** [`clean-production-shelf-v1.json`](../../pipeline/asset-vault/clean-production-shelf-v1.json) · [Galería de las 26 piezas](GALLERY.md) · [Inventario histórico completo](../../pipeline/asset-vault/historical-full-index-v1.json).
