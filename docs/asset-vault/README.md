# ELDORIA — Biblioteca Maestra de Assets 3D

**Auditoría inicial profunda: 25-10-2026. Estado: solo documentación y herramientas de consulta.**

El catálogo [JSON consultable](../../pipeline/asset-vault/master-catalog-v1.json) registra identidad, fuente, versión, rama/SHA, rol jugable, clasificación, riesgos y evidencia. **No promueve automáticamente** ningún modelo ni cambia Unity, Blender, Work o el PR #100.

## Hallazgo resumido

* **26 GLB de la biblioteca canónica**, con SHA-256 y archivos .meta auditados el 02-10-2026; 5 para sus funciones de producción, 20 componentes de reutilización condicionada y 1 monumento NO transitable.
* **3 familias originales de Tripo**: engendro, jabalí y foco, con Blender editable, variantes y trazabilidad. El foco no pasa el estándar visual ni tiene función jugable.
* **6 representaciones activas candidatas del mundo** en PR #100: geografía, recursos, jabalí Near/Far y engendro Near/Far. NO ART_APPROVED.
* **201 ramas históricas con referencias SHA**: 20 investigadas en fuentes originales, 181 cribadas a nivel de rutas. Solo 8 rutas 3D únicas del muestreo histórico se incorporan de entrada al catálogo; NO supone que solo existan ocho ni que se hayan evaluado todos los binarios antiguos.
* **4 grupos antiguos rechazados** se preservan como advertencias, no como órdenes de recuperación.

## Clasificación por recurso

| Pieza o familia | Estado de reutilización | Limitación decisiva |
| --- | --- | --- |
| Valoria_HeroBastion_v1 (`main`) | `USE_PRODUCTION` | Validated hero anchor; do not replace with older silhouette. · `Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb` |
| Valoria_Aserradero_AP2_v1 (`main`) | `USE_PRODUCTION` | Dedicated production aserradero. · `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb` |
| Valoria_Cuartel_AP2_v1 (`main`) | `USE_PRODUCTION` | Dedicated production cuartel. · `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb` |
| Valoria_Granero_BIII_v1 (`main`) | `USE_PRODUCTION` | Production asset, not permission to activate Bastion III gameplay. · `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb` |
| PlayerCity_v1 (`main`) | `USE_PRODUCTION` | Universal strategic player-city mesh with identity/state separate. · `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb` |
| Piece01 (`main`) | `REUSE_CONDITIONAL` | Four selected good source modules; support compact approved topology only. · `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece01.glb` |
| Piece02 (`main`) | `REUSE_CONDITIONAL` | Four selected good source modules; support compact approved topology only. · `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece02.glb` |
| Piece03 (`main`) | `REUSE_CONDITIONAL` | Four selected good source modules; support compact approved topology only. · `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece03.glb` |
| Piece04 (`main`) | `REUSE_CONDITIONAL` | Four selected good source modules; support compact approved topology only. · `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece04.glb` |
| ResidentialTerraceRock (`main`) | `REUSE_CONDITIONAL` | not a certified through-street module; do not force topology to fit it · `Unity/Assets/Eldoria/Resources/Valoria/Rescued/ResidentialTerraceRock.glb` |
| RockTerrainSeamFiller (`main`) | `REUSE_CONDITIONAL` | not a standalone landmark or path · `Unity/Assets/Eldoria/Resources/Valoria/Rescued/RockTerrainSeamFiller.glb` |
| StreetLandingTransition (`main`) | `REUSE_CONDITIONAL` | heavy parapets and no interface certification; never trust as sole traversable connector · `Unity/Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb` |
| TerraceStairRock (`main`) | `REUSE_CONDITIONAL` | interface not certified; do not use mesh to define traversal · `Unity/Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb` |
| TowerWallRock (`main`) | `REUSE_CONDITIONAL` | overuse makes Valoria fortress-only; no circulation role · `Unity/Assets/Eldoria/Resources/Valoria/Rescued/TowerWallRock.glb` |
| CornerWallL (`main`) | `REUSE_CONDITIONAL` | Certified three-piece subset; rejected siblings must not be restored. · `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb` |
| HighStraightWall (`main`) | `REUSE_CONDITIONAL` | Certified three-piece subset; rejected siblings must not be restored. · `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb` |
| RockToWallTransition (`main`) | `REUSE_CONDITIONAL` | Certified three-piece subset; rejected siblings must not be restored. · `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb` |
| piece_01_10364tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_01_10364tris.glb` |
| piece_02_12602tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_02_12602tris.glb` |
| piece_03_6433tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_03_6433tris.glb` |
| piece_04_7824tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_04_7824tris.glb` |
| piece_05_4966tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_05_4966tris.glb` |
| piece_06_7602tris (`main`) | `REUSE_CONDITIONAL` | Six preserved separated modules, not proof of final artistic approval for arbitrary compositions. · `Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_06_7602tris.glb` |
| BroadRockPlatform (`main`) | `REUSE_CONDITIONAL` | Use only buried or top-aligned; do not turn into huge rejected cliffs. · `Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/BroadRockPlatform.glb` |
| SteppedRockTerrace (`main`) | `REUSE_CONDITIONAL` | Use only buried or top-aligned; do not turn into huge rejected cliffs. · `Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/SteppedRockTerrace.glb` |
| GateStreetRiseRock_MV1 (`main`) | `LANDMARK_ONLY` | Documented traversal/interface failure; no use as stand-alone passable route. · `Unity/Assets/Eldoria/Resources/Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1.glb` |
| Engendro de la Fisura — Bastion II (`main`) | `INTEGRATED_REUSE_WITH_LIMITS` | El rostro aún es poco expresivo desde cámara de juego; comprobar frontal, ojos y línea mandíbula antes de modificar. Silueta encorvada útil pero cola/anatomía a · `ArtSource/World4X/Spawnling_v1/Optimized_Source.glb` |
| Jabalí de roca — World4X (`main`) | `WORLD_CANDIDATE_ONLY` | Placas rocosas parecen superpuestas; revisar transiciones orgánicas y cuerpo desde zoom táctico. Contacto pies/suelo dudoso e irregularidad anatómica; diagnosti · `ArtSource/World4X/RockBoar_v1/RockBoar_Optimizado_49800.glb` |
| Foco de corrupción — prop experimental (`main`) | `REJECT_CURRENT_VISUAL` | La corrupción púrpura no se distingue a zoom estratégico; parece una roca convencional. La silueta acumula piedras redondeadas y el centro violeta parece un cri · `ArtSource/World4X/RiftFocus_v1/Optimized_Source.glb` |
| terrain (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RegionOneBlenderWorld.glb` |
| nodes (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RegionOneResources.glb` |
| boar-near (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/RockBoarNear.glb` |
| boar-far (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/RockBoarFar.glb` |
| spawnling-near (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/SpawnlingNear.glb` |
| spawnling-far (`feature/world4x-region1-canon-20261022`) | `TECH_CANDIDATE_NOT_ART_APPROVED` | Never merge, publish, or substitute game scene automatically. Region1 ART_APPROVED=false; validate close/regional/panorama and physical mobile separately. · `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/SpawnlingFar.glb` |
| valoria_gate_reference.glb (`e137aaf713c682509cacc62cd1b3279c140a811c`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion. · `Unity/Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_gate_reference.glb` |
| valoria_visual_slice_environment_v2.glb (`e137aaf713c682509cacc62cd1b3279c140a811c`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion. · `Unity/Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_visual_slice_environment_v2.glb` |
| ValoriaEnvironmentCandidate.glb (`e137aaf713c682509cacc62cd1b3279c140a811c`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion. · `art-source/valoria/production/studio-rocks-v1/ValoriaEnvironmentCandidate.glb` |
| ValoriaRockStudy.blend (`e137aaf713c682509cacc62cd1b3279c140a811c`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion. · `art-source/valoria/production/studio-rocks-v1/ValoriaRockStudy.blend` |
| ValoriaScannedCliffCandidate.glb (`e137aaf713c682509cacc62cd1b3279c140a811c`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion.  · `art-source/valoria/production/studio-rocks-v1/ValoriaScannedCliffCandidate.glb` |
| ZoneOneSectorCandidate.glb (`711716b0a91eb7a809fe272b55215bd8ea7f1862`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion.  · `Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb` |
| cliff_rocks-unity-v1.blend (`2efd346034bdb89ae063406558f0343d753e489a`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion.  · `art-source/valoria/variants/cliff-unity-v1/cliff_rocks-unity-v1.blend` |
| cliff_rocks-unity-v1.glb (`2efd346034bdb89ae063406558f0343d753e489a`) | `HISTORICAL_HOLD_FOR_DIRECT_REVIEW` | Historical path selected by 201-ref screening. Not a current-approved model; inspect geometry, licence, material, camera, import and gameplay before promotion.  · `art-source/valoria/variants/cliff-unity-v1/cliff_rocks-unity-v1.glb` |
| Stone Architecture 03/04/06/07/08 (`main`) | `REJECT_FOR_CURRENT_CANON` | Fused, disconnected or protruding stone shapes; historical REPLACE verdict. · `grupo histórico sin ruta única` |
| Terrain & Terrace 02–06 (`main`) | `REJECT_FOR_CURRENT_CANON` | Rejected groups; do not promote from historical branches. · `grupo histórico sin ruta única` |
| Slavic foliage as primary forest (`main`) | `REJECT_FOR_CURRENT_CANON` | Explicitly rejected as main forestry visual language for World4X. · `grupo histórico sin ruta única` |
| Old generic lateral housing (`main`) | `REJECT_FOR_CURRENT_CANON` | Hidden/replaced by compact Valoria design, conflicts with canon. · `grupo histórico sin ruta única` |

## Qué merece realmente la pena

**Conservar y reutilizar sin modelar de cero:** Hero Bastion, Aserradero, Cuartel, Granero (no activar todavía la progresión Bastión III), Player City. Para apoyo estructural, los módulos StoneKit y MidTier, los tres StoneArchitecture aprobados y TerrainTerrace son fuentes útiles si el entorno exige esas formas. Para transiciones visuales, priorizar RockTerrainSeamFiller, ResidentialTerraceRock, CornerWallL y RockToWallTransition.

**Utilizar con restricciones:** TowerWallRock para acentos de silueta, TerraceStairRock y StreetLandingTransition como presentación sobre rutas certificadas por separado. GateStreetRiseRock_MV1 sirve como monumento, nunca como autoridad del recorrido debido al fallo histórico. Ni los GLB de apoyo ni las texturas sustituyen el diseño artístico actual.

**No reactivar por estar disponibles:** Stone Architecture 03/04/06/07/08, Terrain & Terrace grupos 02–06, foliage eslavo como bosque principal, viviendas periféricas obsoletas, grandes acantilados de la antigua Valoria; el foco de corrupción Tripo no representa automáticamente la Brecha actual.

**Revisar antes de rescatar del historial:** las ocho rutas históricas apuntan a GLB/.blend que figuran en commits fijados (rocas, puerta, antiguos terrenos, acantilados). En muestras se ha verificado que las rutas son accesibles; ninguna es un asset aprobado para la escena vigente por ese hecho.

**Distinguir ciudad y mundo:** la cantera como recurso del mapa no satisface el futuro edificio funcional Cantera de Valoria; los modelos del jabalí de Región 1 siguen en una rama DRAFT y no acreditan integración al capítulo III de la campaña. No adelantar Bastiones III–X.

## Nivel real de seguridad de la biblioteca

**Comprobado:** existe el inventario de 26 GLB certificados anteriormente y su procedencia; existen fuentes originales editables del engendro, jabalí y foco y rutas concretas de ramas históricas; las candidatas del mundo conservan .blend y GLB en el PR activo.

**No comprobado universalmente:** rehash hoy de los 26 binarios, licencia individual de todos los históricos, previews para cada asset, optimización en móvil físico, inventario recursivo de todos los árboles Git de las 201 ramas. Las 181 ramas de cribado no equivalen a una revisión profunda archivo por archivo.

**Riesgos:** artefactos GitHub Actions expiran; ref de rama puede moverse, por eso las recuperaciones históricas usan SHA; un modelo útil puede tener geometría rechazada, pivotes/UV incorrectos o licencia insuficiente; copiar todas las variantes a main inflaría el repositorio innecesariamente.

## Herramientas incorporadas (solo lectura)

Desde un checkout de Eldoria:

    python3 tools/asset-vault/verify.py --output /tmp/asset-audit.json
    python3 tools/asset-vault/discover-history.py --output /tmp/archived-assets.json

`verify.py` verifica por SHA y .meta los 26 GLB canónicos frente al checkout **actual**, y detecta otros .blend/.glb/.fbx/.obj rastreados por Git aún no registrados en el índice curado. No modifica modelos.

`discover-history.py` recorre los árboles completos de los 201 commits archivados **solo cuando los objetos Git estén en el clon local**. Si es shallow/incompleto, declara cuántas ramas no pudo leer: sin fetch, checkout ni merge. Su resultado debe revisarse visualmente antes de promoción.

## Actualización segura y siguiente fase

1. Completar inventario histórico **de árboles**, mediante discover-history en un clon que disponga de todos los objetos, sin ejecutar código antiguo.
2. Generar miniatura aislada + captura real de Unity para los candidatos de mayor valor, comenzando por los 26 canónicos que carezcan de preview identificable.
3. Incorporar los dos nuevos nodos **solo tras la entrega y validación de Work PR #100** con fuente Blender, GLB, hash, referencia visual aprobada, LOD y captura real. No interrumpir la producción.
4. Validar licencias y presupuesto móvil por activo. Solo entonces promover selectivamente al depósito canónico. Los rechazados siguen consultables, nunca autocargables.
5. Automatizar posteriormente el registro de nuevos modelos como **candidatos**, no como aprobados, con revisión humana/artística antes de producción.

## Automatización de entrada y búsqueda (fase 2 implementada en este PR)

**Al crear o modificar modelos 3D en cualquier PR hacia main** (GLB, GLTF, Blender, FBX, OBJ o STL), el workflow `.github/workflows/asset-vault-discovery.yml` analiza el árbol Git completo sin descargar todos los modelos ni usar Unity/Windows. Produce un inventario JSON de **candidatos sin catalogar** y lo adjunta a la ejecución. No edita la rama de Work ni impide el trabajo por motivos artísticos. En los pushes a `main`, actualiza una **única incidencia persistente** titulada `[ASSET VAULT] Candidatos 3D detectados automáticamente`, para que ningún nuevo archivo Git rastreado dependa de que el propietario se acuerde de él. El workflow solo se activará después de integrar esta propuesta documental en main; no debe interpretarse como automatización ya operativa mientras PR #106 permanezca DRAFT.

**Antes de autorizar un modelo nuevo**, ejecutar:

    python3 tools/asset-vault/find-reuse.py cantera
    python3 tools/asset-vault/find-reuse.py engendro
    python3 tools/asset-vault/find-reuse.py "muralla piedra"

El resultado ofrece rutas, rama, rol y restricciones, con rechazo explícito de fuentes no aprobadas. Después revisar el buzón de candidatos automáticos y el inventario histórico; si no existe un recurso apropiado, crear una nueva pieza en la rama autorizada. **Descubrimiento automático no significa aprobación automática:** fuentes, licencia, escala, cámara real, materiales, LOD y criterio artístico siguen siendo requisitos para promoverlo a la biblioteca de producción.

El workflow `.github/workflows/asset-vault-historical-audit.yml` permite una auditoría profunda **bajo demanda** de los 201 árboles históricos en un runner GitHub-hosted, con `--filter=blob:none`. Informa qué SHA no se pudo leer y publica un JSON; no extrae binarios ni toca la máquina Unity. Está preparado, pero su ejecución real y revisión de todos los candidatos sigue pendiente.

**Limitación importante:** modelos generados exclusivamente en el PC y nunca guardados en Git/almacenamiento duradero no pueden detectarse desde GitHub; los artefactos temporales de Actions caducan. El productor debe guardar las fuentes editables en el repositorio o depósito duradero autorizado. No se puede garantizar el 100 % de activos de todos los tiempos hasta auditar también esos orígenes.

## Enlaces de evidencia

* [Canon GLB 26](../../pipeline/asset-library-canonicalization-audit.json)
* [Resultado de canonización](../../docs/ASSET_LIBRARY_CANONICALIZATION_PASS_V1_RESULT.md)
* [Reprocesado y criterios](../../docs/ASSET_LIBRARY_REPROCESSING_PASS_V1_RESULT.md)
* [Tripo y originales](../../pipeline/tripo-art-brain/catalog.json)
* [Ramas 201](../../pipeline/historical-branches-20261018.json)
* [Cribado de ramas](../../pipeline/art-knowledge/historical-branch-screening-v1.json)
* [Análisis histórico y limitaciones](../../docs/knowledge/ELDORIA_GLOBAL_HISTORICAL_RECOVERY_20261021.md)
* [Contrato de trabajo y arte](../../AGENTS.md)

**Regla final:** este documento es navegable y descriptivo, no un cambio de canon. La autoridad artística del propietario y los responsables vigentes siguen prevaleciendo.
