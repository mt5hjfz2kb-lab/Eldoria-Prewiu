# Organización del proyecto y conservación de archivos

Auditoría recuperada y continuada el 26-10-2026; no es una limpieza destructiva.
Inventario completo de metadatos Git en `e34f058c9eb2d73c863269f9debc1cdff532f93e`:
20.349 archivos, **27.331.041.227 bytes** sin comprimir (27,33 GB decimales).
Recorrido completo de árboles por SHA, sin resultados truncados y sin descargar
27 GB. Evidencia: [repository-size-inventory.json](repository-size-inventory.json).

| Conjunto | Bytes del árbol actual | Interpretación |
| --- | ---: | --- |
| `docs` | 15.246.095.959 | Principal acumulación, sobre todo evidencia y fuentes por ejecución |
| `Unity` | 11.171.646.555 | Incluye 9.926.493.035 bytes de `Eldoria/ArtTests`; no equivale a juego activo |
| `art-source` | 792.316.616 | Fuentes originales, no recursos prescindibles |
| `ArtSource` | 86.155.601 | Segundo árbol con distinta capitalización; no fusionarlo sin revisar consumidores |

`docs/evidence/world4x-canon-20261022` representa 10.494.922.458 bytes;
ValoriaReferenceClean añade 2.898.027.620 y construcción por capas 1.616.503.858.
La prioridad de organización es historia de producción y fuentes repetidas,
no borrar scripts pequeños ni recortar los modelos jugables por intuición.

Por identidad exacta de blob hay 19.035.376.339 bytes únicos y 8.295.664.888
bytes repetidos en varias rutas del árbol. Git ya almacena el mismo blob una vez:
esa diferencia NO es una promesa de reducir el historial comprimido en 8,30 GB.
Ejemplo comprobado: `Eldoria_Resource_Library.blend` de 54.869.267 bytes tiene el
mismo blob en 18 ejecuciones (932.777.539 bytes repetidos en el checkout).
La fuente actual `Eldoria_Region1_Production.blend` de 95.503.119 bytes tiene
cuatro rutas idénticas. Los `.asset` idénticos pueden tener `.meta`/GUID distintos;
no se sustituyen automáticamente por una sola ruta.

Estas cifras excluyen todo el historial Git, cachés locales de Unity y builds
externos. No son una medición de `.git`, espacio de disco físico ni APK.

Reproducción desde un clon completo que ya tenga los blobs (en un clon parcial,
`ls-tree -l` puede provocar descargas; esta auditoría usó metadatos de la API):

```sh
git ls-tree -r -l -z e34f058c9eb2d73c863269f9debc1cdff532f93e > /tmp/eldoria-tree.bin
python tools/dependency-audit/summarize_tree.py --inventory /tmp/eldoria-tree.bin --commit e34f058c9eb2d73c863269f9debc1cdff532f93e --output /tmp/eldoria-tree-summary.json
```

El resumidor solo lee ese inventario; no descarga, mueve ni elimina archivos.

Inventario nativo: run40372380113, artefacto12676940416, fuente inmutable
`d9d4e1f9ffb9f7ab29e1c963a80eb08321c35bb4`. Se descargaron y leyeron el resumen
y los 1.558 registros de dependencias. Las cantidades siguientes corresponden
al subconjunto auditado, NO al peso completo del repositorio ni de la APK.

| Clasificación nativa | Archivos candidatos | Bytes | Acción |
| --- | ---: | ---: | --- |
| Dependencia de escena activa | 69 | 315.145.049 | Proteger |
| Referencia de escenas históricas | 1.135 | 5.635.133.552 | Conservar conjunto recuperable; no mover archivos sueltos |
| Sin referencia resuelta por Unity | 354 | 1.909.173.156 | Copia verificada y análisis complementario; no declarar prescindibles |
| Total analizado | 1.558 | 7.859.451.757 | No confundir con activos cargados simultáneamente |

Los conjuntos `Integration/Generated-39132392803`, `39132807199`, `39137079921`,
`39138751241`, `39138143327`, `39136444844` y `39133202399` rondan cada uno
251 MB en este inventario. Es una señal concreta de acumulación de versiones de
producción. Tamaños parecidos no prueban identidad de contenido: no se deduplican
por nombre, fecha o tamaño. Las fuentes y GUID forman parte del conjunto.
El inventario completo posterior sí prueba duplicación por blob de varias
texturas de esos conjuntos; la equivalencia de contenido no prueba equivalencia
de referencias Unity ni autoriza eliminar sus `.meta`.

## Organización propuesta

Mantener un único juego activo en Unity y un índice pequeño que indique:
versión actual, fuentes reutilizables aprobadas, experimentos rechazados y evidencia.
El archivo histórico debe quedar fuera de la ruta habitual de importación cuando
su conjunto esté restaurable y las herramientas que lo consumen estén adaptadas.
No renombrar ahora directorios de Unity: rompe referencias literales aunque el GUID
permanezca, y hay scripts de autoría y workflows que consumen rutas históricas.

Conservar para cada conjunto: commit, manifiesto de archivos, SHA de blobs y
SHA-256, assets y `.meta`, fuentes Blender, licencias, referencias y prueba de
restauración. Los assets rechazados permanecen identificados como rechazados;
archivarlos no los autoriza para producción. El Bastión IA vetado sigue excluido.

Reducir acumulación futura: una evidencia vigente por candidato y referencias al
run para logs redundantes; no volver a copiar todos los binarios en cada informe.
Para las fuentes idénticas, mantener una ruta canónica y que los manifiestos de
ejecución apunten a ella y a su hash; conservar en el archivo recuperable las rutas
históricas hasta adaptar sus scripts de restauración. Empezar por la biblioteca
de recursos repetida 18 veces, fuera de Assets, después de mapear sus consumidores.
Productor concreto identificado: `authoring/materialize_verified_candidate.ps1`
crea `sources/native-$GITHUB_RUN_ID` y copia de nuevo las tres fuentes BLEND;
`world4x-canon-native.yml` consume fuentes guardadas y verifica hashes de ellas.
La deduplicación debe modificar conjuntamente ese productor y los consumidores,
con resolución por manifiesto y restauración de rutas históricas. Borrar las copias
sin esa adaptación rompería un flujo de recuperación que sigue siendo real.
Las capturas A/B necesarias se conservan como evidencia original, no se reemplazan
por miniaturas. No añadir otro sistema de catálogo paralelo al Asset Vault existente.

## Qué puede reducirse y qué no

Un archivo ZIP de respaldo por sí solo no reduce el checkout. Mover archivos fuera
de `Assets` puede reducir importación y copia activa, pero no borra sus blobs del
historial Git. Reducir el historial requeriría una migración/repositorio de archivos
planificada y permisos específicos; no se hace `filter-repo`, force push ni borrado
de ramas como parte de esta auditoría. Tampoco confundir menor tamaño del repositorio
con menor tamaño de APK: son conjuntos diferentes.

El paso seguro inmediato es proteger los 354 candidatos antes de cualquier decisión.
La ejecución original de archivo falló por el alias WindowsApps de Python. Se ha
reparado la misma rama `audit/asset-recovery-package-20261026` reutilizando el Python
instalado con Blender, sin instalar software ni gastar. Además se corrigió el parseo
de tamaños rellenados con espacios de `git ls-tree -l`, se exige el commit fuente
exacto y no se sobrescribe un archivo parcial previo.

El análisis de referencias literales es complementario: un resultado sin coincidencias
no demuestra ausencia de referencias dinámicas. La prueba nativa tampoco identifica
por sí sola todos los consumidores externos. No se elimina ni mueve ninguno de los
354 assets por un falso «unused».

## Cierre verificable

La copia debe verificarse mediante blobs Git, GUID, SHA-256 por archivo y CRC del
ZIP, y conservarse en `Documents/EldoriaArchives` del PC. La ejecución publica solo
el comprobante ligero y el análisis de referencias, no otros 2 GB en Actions.
El resultado y la ubicación exactos se incorporan al cierre del encargo después
de que termine el job; no se considera creada una copia por haber lanzado el flujo.
