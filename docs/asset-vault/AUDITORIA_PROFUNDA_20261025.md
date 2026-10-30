# ELDORIA — Auditoría profunda y cierre de biblioteca 3D (25/10/2026)

> **ANULACIÓN EXPRESA POR EL PROPIETARIO, 25/10/2026:** el Bastión antiguo `Valoria_HeroBastion_v1.glb`, creado con un modelo de IA anterior, está **BLOQUEADO PARA USO**, con independencia de cualquier antiguo `TECH PASS` o etiqueta `USE_PRODUCTION`. Debe preservarse solo como evidencia archivada y **no usarse ni promocionarse** sin aprobación explícita posterior del propietario. La biblioteca activa contiene **4 piezas de producción**, 20 módulos condicionales y 1 monumento de uso restringido; ese Bastión está excluido. Comprobar de forma segura por el titular de la ciudad si hay instancias actualmente activas, según [ticket #109](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/109).


## Alcance real

Repositorio `mt5hjfz2kb-lab/Eldoria-Prewiu`; trabajo aislado en PR #106. Ningún archivo de escena Unity, asset original, modelo GLB/.blend, metadatos Unity ni reserva del PR #100 ha sido modificado por esta auditoría. La biblioteca distingue **disponibilidad**, **procedencia**, **reutilización posible** y **calidad visual aprobada**.

## Certificación de inventario

| Fuente | Cobertura real | Evidencia |
|---|---:|---|
| Árbol Git actual de la candidata documental | **775 rutas 3D** | `pipeline/asset-vault/current-full-index-v1.json` |
| Binarios Git distintos en ese árbol | **767** | 4 grupos de duplicados exactos por blob SHA |
| Rutas ya conocidas por la biblioteca curada | **39** de las 775 | El catálogo curado tiene 47 registros; algunos son grupos históricos o fuentes de otra rama |
| Ramas archivadas fijadas a commit exacto | **201/201** | `pipeline/asset-vault/historical-full-index-v1.json` |
| Rutas 3D distintas entre esas ramas | **920** | Archivo histórico completo |
| Versiones de binario distintas (misma ruta, distinto Git blob) | **923** | Se detectaron 3 rutas con dos versiones cada una |
| GLB canónicos de Valoria + metadatos | **26/26 GLB y 26/26 .meta** | `pipeline/asset-library-canonicalization-audit.json` y nuevas lecturas de Git |
| Fuentes y variantes Tripo | **12/12 rutas** | `pipeline/tripo-art-brain/catalog.json` |
| Miniaturas de malla de los 26 GLB | **26/26 imágenes** | `docs/asset-vault/GALLERY.md`, `docs/asset-vault/preview-manifest-v1.json` |
| Ramas inaccesibles entre las 201 | **0** | CI GitHub-hosted `40216800392` y auditoría persistida |

**Interpretación:** 775 y 920 son rutas en dos conjuntos solapados, **NO** 1.695 modelos diferentes. Hay duplicados, bibliotecas de terceros, pruebas, versiones LOD y fuentes editables. Es seguro afirmar que están inventariados los archivos 3D de `main` en la foto examinada y de los **201 árboles de commits archivados**; no que se haya auditado cada commit intermedio, cada PC o cada artifact Actions caducado.

## Evaluación útil / no útil / condicional

| Origen / familia | Archivos 3D actuales | Uso recomendado | Limitación real |
|---|---:|---|---|
| Mega Fantasy Props Pack | **280** | Biblioteca de atrezzo y composición secundaria, solo selección puntual | Licencia y calidad móvil por pieza no verificadas |
| Slavic World Free (EmaceArt) | **213** | Elementos secundarios puntuales si el canon lo admite | Rechazado como lenguaje principal de bosques |
| PolyOne modular terrain | **14** | Estudio aislado de piezas de transición | No reemplazar AF/AG/Región 1 automáticamente |
| URP Tree Pack | **12** | Variantes opcionales de vegetación | Estilo, masa forestal y coste móvil por comprobar |
| Bublik modular castle | **8** | Candidatos de referencia para cantera, molino de madera, puerta y torres | No sustituir los dos nodos ya aprobados ni usar sin licencia |
| Holotna mountain | **5** | Roca/árboles/montaña condicionalmente | No recuperar geografía vieja en lugar de la autorizada |
| Quaternius comparative | **3** | Arco/columna/pared rota secundarios, tras adaptación | Licencia, materiales, escena por revisar |
| NatureStarterKit2 | **1** | Material de prueba; baja prioridad | No confundir disponibilidad con aprobación |
| Producción alternativa de Valoria | **42** | Fuente de geometría autoral para puertas, torres, viviendas, muros | Históricos; comparar contra el canon actual |
| Blender editable Valoria producción | **47** | **Alta prioridad**: rescatar fuentes editables antes de repetir modelado | Incluye iteraciones y variantes no aprobadas |
| Ediciones en `docs/evidence` y `art-source/lookdev` | Múltiples | Conservar para recuperar métodos, UV, geometría y fallos aprendidos | Documentación y prototipos, no biblioteca de producción directamente |

Hay **536 rutas de paquetes de terceros**. Existir en el proyecto no demuestra por sí solo el derecho de redistribuir el modelo fuera del juego ni que podamos recomendarlo como obra propia. La clasificación es conservadora. La presencia del fichero `Bublik/.../Stone_Mine.fbx` y del `Lumber_Mill.fbx` merece una revisión futura, pero Work ya está construyendo los modelos aprobados del propietario: **no interferir ni reemplazar su trabajo por esos prefabs**.

### Duplicados exactos encontrados, con pruebas Git

1. **Seis copias** byte-idénticas de `cliff_rock_column.glb` en los niveles v2–v7 de construcción de Valoria: mantener todas como evidencia de revisiones, pero una sola fuente referenciable es suficiente para nueva producción.
2. `WorldInventory/MegaGate.fbx` = `Mega Fantasy Props Pack/FBX/stone_half_gate.fbx`.
3. `WorldInventory/MegaTower.fbx` = `Mega Fantasy Props Pack/FBX/tower.fbx`.
4. `WorldInventory/MegaWall.fbx` = `Mega Fantasy Props Pack/FBX/stone_wall_detailed.fbx`.

Son **4 grupos / 8 rutas redundantes**, sin necesidad de borrar ningún archivo ahora. El uso de un mismo GLB por varios nodos distintos no es un duplicado indebido: es reutilización correcta.

### Decisiones de reutilización

**Conservar como producción en su ámbito:** Aserradero, Cuartel, Granero BIII (no habilita nueva progresión) y Player City. Componentes canónicos StoneKit, MidTier Piece01–04, RetainingWall y RockToWallTransition son de alto valor para montaje modular según la cámara actual.

**Conservar condicionado:** TerraceStairRock, TowerWallRock, RockTerrainSeamFiller, ResidentialTerraceRock, StreetLandingTransition, fuentes Blender familiares, recursos Tripo de fauna/engendro y el GLB actual de Región 1 con `ART_APPROVED=false`.

**No autorizar como solución vigente:** GateStreetRiseRock_MV1 como paso transitable; viejos StoneArchitecture 03/04/06/07/08; grupos TerrainTerrace 02–06; Slavic foliage como bosque canónico; vivienda periférica y acantilados rechazados; foco Tripo como Brecha terminada.

## Herramientas integradas en PR #106

- **Detección automática de nuevas rutas 3D** en PR y `main`, leyendo solo el Git tree, sin descargar los cuerpos de GLB/FBX/Blend ni interferir con Unity: `.github/workflows/asset-vault-discovery.yml`. La base existente de 775 se separa de las **altas posteriores**.
- **Buzón persistente de candidatos**, un único issue actualizado cuando haya nuevos archivos, sin aprobarlos automáticamente.
- **Buscador cruzado de 47 activos curados, 775 archivos actuales y 920 rutas históricas**: `python3 tools/asset-vault/find-reuse.py cantera`. Devuelve rama y SHA de referencia, con etiquetas de riesgo.
- **Inventario de 201 ramas por Git blob** conservado en el repositorio, con 923 versiones exactas. El workflow manual permite regenerarlo.
- **Galería visual de 26 miniaturas de malla real**, sin texturas ni luces URP. Es evidencia de geometría e identidad, no fotografía del juego ni nota 7,5.
- **Regla de búsqueda antes de modelar en `AGENTS.md`** e índice documental común. Obliga al agente a justificar por qué necesita generar algo en vez de aprovechar una fuente existente.
- **Pruebas aisladas Python y CI GitHub-hosted**, sin reservar el editor de Windows. Los workflows de miniaturas e inventario histórico quedan manuales para no repetir renders largos con cada documentación.

## Qué no puede certificarse como 100 % aún

- No se han abierto individualmente las 920 rutas históricas en Blender ni valorado estéticamente todas sus escenas. No sería eficiente ni seguro importar material rechazado masivamente.
- No se puede demostrar que se conserve un recurso creado **solo en un PC**, un commit histórico inaccesible o un artifact Actions expirado; sí se conservaron los **201 SHA archivados** y el contenido que Git todavía posee.
- Falta validación legal/licencia de 536 archivos de bibliotecas externas y rendimiento Android/iOS por pieza. No asignar falsos permisos de uso.
- Las nuevas piezas del PR #100 todavía pertenecen a Work y deben entrar al catálogo curado con pruebas una vez aprobadas. Una ruta detectada automáticamente empieza como **candidata**, no como aprobada.
- La activación en `main` requiere terminar la revisión/fusión aislada del PR #106, manteniendo indemne PR #100. Mientras #106 sea DRAFT, la automatización está probada en su PR, pero no está desplegada permanentemente al `main` de Eldoria.

## Veredicto

**Inventario estructural y recuperación histórica: completo para la muestra de 201 SHA archivados y el estado actual de archivos Git versionados.**
**Detección automática y miniaturas canónicas: implementadas y probadas en el PR independiente.**
**Certificación artística, licencias de packs, archivos externos al Git y despliegue en main: requieren sus verificaciones propias.**

No afirmar un 100 % universal de arte, porque confundir inventario con aprobación repetiría el mismo error que ha ralentizado Eldoria.
