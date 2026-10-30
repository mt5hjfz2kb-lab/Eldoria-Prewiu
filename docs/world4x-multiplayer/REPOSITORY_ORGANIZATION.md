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
Productor concreto identificado en la instantánea auditada: `authoring/materialize_verified_candidate.ps1`
creaba `sources/native-$GITHUB_RUN_ID` y copiaba de nuevo las tres fuentes BLEND;
`world4x-canon-native.yml` consume fuentes guardadas y verifica hashes de ellas.
La primera etapa posterior adapta ese productor y su manifiesto, manteniendo el
contrato de rutas y hashes de los consumidores y todas las rutas históricas. Los
insumos temporales del workflow nativo permanecen iguales. Borrar las copias
sin revisar sus consumidores rompería un flujo de recuperación que sigue siendo real.
Las capturas A/B necesarias se conservan como evidencia original, no se reemplazan
por miniaturas. No añadir otro sistema de catálogo paralelo al Asset Vault existente.

Orden de reorganización y avance de la primera etapa, con pruebas en cada paso:

1. **Implementado en PR114:** reutilizar fuentes BLEND idénticas en el materializador,
   conservando las distintas; recuperación y resolución por ruta/hash probadas.
2. Delimitar checkouts de CI por función: Unity necesita el proyecto y sus fixtures;
   la autoría necesita las fuentes editables indicadas por el manifiesto; la auditoría
   puede leer Git sin materializar decenas deGB. La auditoría ya usa checkout reducido
   y lectura de blobs. PR114 acota otros dos controles de metadatos. No trasladar
   esa configuración a Unity sin listar sus fixtures.
3. Ensayar restauración/importación de un conjunto histórico completo en un entorno
   desechable, incluyendo escenas consumidoras y `.meta`; después separar su copia
   de trabajo de `Assets`, manteniendo la fuente y evidencia recuperables.
4. Revisar por separado una migración del historial si sigue siendo necesaria.
   No confundir los pasos anteriores con reducir automáticamente el pack Git.

La espera observada en los checkouts completos del runner confirma el coste operativo
de materializar el árbol grande. No es una medición del rendimiento del juego móvil.


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

El análisis de referencias literales final lee blobs Git de main725bccd0, no
archivos de un checkout parcial. La procedencia Unity del inventario sigue siendo
d9d4e1f9: ambas fuentes se registran por separado. La lectura de blobs se probó
con un archivo deliberadamente ausente del checkout y referencias a un GUID y a
un directorio generado: siguió detectándolas. No se declara cobertura completa
sobre archivos de texto mayores de6MiB ni sobre referencias construidas dinámicamente.

El análisis de referencias literales es complementario: un resultado sin coincidencias
no demuestra ausencia de referencias dinámicas. La prueba nativa tampoco identifica
por sí sola todos los consumidores externos. No se elimina ni mueve ninguno de los
354 assets por un falso «unused».

## Cierre verificable

Copia creada y verificada por run40403787985 SUCCESS, artefacto12690382554.
Ubicación: `C:\Users\crist\Documents\EldoriaArchives\eldoria-354-assets-20261026.zip`.
264.433.994 bytes comprimidos;354 assets (1.909.173.156 bytes originales),
742 entradas incluyendo `.meta` de archivos y carpetas.
SHA256 `9bfb12c60b1f84218705e9f9a7ee32278df3f1f71811c6c05248b24b422507bb`.
[Comprobante descargado y contrastado](archive-backup-verification.json).
El exportador comprobó cada blob Git, GUID, SHA256 y CRC, además del ZIP completo.
La rama `archive/eldoria-assets-before-cleanup-20261026` conserva el snapshotd9d4e1f9.
No se añadió el ZIP de264MB al historial ni se eliminaron assets.

El análisis literal de esa ejecución antigua examinó solo250archivos y omitió5.850
por su checkout parcial: NO se usa su resultado de cero coincidencias para declarar
prescindibles los354candidatos. La versión corregida lee blobs exactos de main;
[Resultado final de referencias](external-references.json), ejecutado localmente
sobre Git725bccd0:6.105 archivos,185.676.323bytes leídos,8textos mayores de6MiB
omitidos y ningún archivo omitido por ausencia del checkout.108candidatos tienen
menciones en código/configuración/serialización,269en documentación,61coincidencias
literales de GUID y276menciones de directorio;64sin coincidencias. Los grupos se
solapan y una mención de carpeta NO demuestra uso directo.29archivos construyen
rutas de producción dinámicas. Se conservan todos los354candidatos.

Los JSON/TXT de informes bajo docs se clasifican como documentación: por ejemplo,
el propio inventario de tamaño no debe hacerse pasar por un consumidor operativo.
La corrección de clasificación se probó con un GUID en código y otro solo en un
informe JSON, ambos ausentes del checkout pero presentes en Git. Los569hallazgos
caben íntegros en el reporte. No se usa como evidencia final el conteo anterior290
«código/config», que mezclaba informes documentales con configuración.

Segunda comprobación nativa: run40404173727 SUCCESS, artefacto12693362102,
estado `EXISTING_REVERIFIED`. Relee las742entradas contra blobs Git/manifiesto,
CRC y SHA256; el ZIP y su fichero SHA256 siguen exactamente iguales y no fueron
sobrescritos. Ese run confirma también la cobertura de6.105textos/185.676.323bytes,
pero su clasificación preliminar290/38 se sustituye por el resultado local108/269
corregido para separar documentación de configuración. La copia y el análisis
literal son verificaciones distintas.

La comprobación de recuperación verifica contenido y rutas, no una reimportación
Unity del ZIP en un proyecto nuevo. Ningún asset se retira antes de comprobar sus
consumidores y esa restauración funcional. El script permite volver a verificar
un ZIP existente contra Git sin sobrescribirlo; una identidad de blob incorrecta
fue rechazada en la prueba de protección.

El flujo de recuperación queda en main como `workflow_dispatch` manual: no se
dispara por editar su propio workflow ni por cambios rutinarios de scripts. La
configuración de cierre superó el verificador de gobernanza existente, sin cambiar
sus reglas. Los resultados nativos anteriores conservan su procedencia exacta.

## Primera etapa de eficiencia — 26-10-2026

Encargo posterior del propietario: frenar nuevas copias idénticas y reducir
descargas de comprobaciones, conservando juego, automatización, pipeline e historia.
Implementación aislada en PR114; no cambia escenas, recursos jugables, GUID,
guardados, paquetes Unity, entregas Android ni los workflows nativos de Unity/UI.

### Descarga acotada con las mismas validaciones

Se conservan todos los comandos de validación de estos dos controles:

| Workflow | Entradas materializadas | Bytes de esas entradas |
| --- | ---: | ---: |
| `valoria-full-frame-convergence-control.yml` | 2 archivos | 3.573 |
| `visual-fallback-route.yml` | 2 archivos | 5.600 |

Las cifras son los cuerpos de sus archivos de entrada, no una medición de tráfico
total: Git, Actions y Node tienen costes adicionales. El checkout usa patrones
exactos sin materializar Unity ni los archivos binarios históricos. El ensayo
`python3 tools/repository-efficiency/verify_sparse.py` ejecuta los controles
originales en directorios temporales con únicamente esas entradas y comprueba
que retirar una entrada obligatoria provoca un rechazo.

Se dejó intacto `eldoria-asset-feedback-sync-test.yml`. Una prueba de lectura
sobre sus ramas históricas detectó que el código Unity de la rama actual ya no
coincide con el blob exigido por su plan de revisión; el artista sí pasó la
comparación exacta. Optimizar ese workflow lo habría reactivado por su propio
trigger. No se cambia el plan, la fuente ni el criterio para ocultar esa diferencia,
ni se reabre producción histórica como parte de esta etapa.

### Nuevas fuentes editables sin copias idénticas

El materializador conservado de Región1 consulta el árbol Git del candidato.
Para sus tres fuentes BLEND, reutiliza una ruta ya versionada del archivo
histórico existente solo si coinciden nombre, blob Git, tamaño y SHA256 real.
Si la fuente cambia, conserva su nueva copia en la carpeta del nuevo run.
Las copias históricas actuales no se borran.

El manifiesto mantiene `files[].path/sha256/bytes` y añade
`editable_sources` con la decisión y bytes de nueva copia evitados.
El consumidor sigue una ruta exacta verificable; no se crea otro catálogo
ni se sustituyen fuentes por enlaces simbólicos. Las capturas, pruebas y
metadatos propios de cada ejecución siguen conservándose individualmente.

Una fuente versionada ausente del checkout se recupera de HEAD, solo esa ruta.
Un archivo local modificado o un destino diferente se conserva y provoca rechazo;
no se limpia el workspace ni se sobrescribe. No se ha reactivado la antigua rama
productora, cambiado su titularidad ni lanzado una nueva producción Blender/Unity.

### Evidencia y límites

El workflow ligero `repository-efficiency.yml` ejecuta los controles aislados,
las pruebas PowerShell de reutilización exacta, fuente nueva, reintento, recuperación,
conflictos, límites de ruta y resolución del manifiesto; también analiza la
sintaxis del materializador completo. Usa fuentes binarias sintéticas en un
repositorio temporal, sin tocar archivos artísticos reales. Esto acredita
conservación de bytes y comportamiento del guardado, no una nueva importación
Blender ni certificación artística.

La prevención afecta a nuevas copias idénticas de esas tres fuentes; no pretende
deduplicar automáticamente todo el repositorio. No disminuye el histórico Git ni
el tamaño de APK. El cambio es reversible por el PR, sin migración de datos.

Validación de la versión definitiva `cd0a904ad29235e835498723b976fd970f4e5a70`:
regresión específica [40414522310](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40414522310),
arquitectura40414522015, cuarentena40414522170 y gobernanza40414522294: SUCCESS.
La regresión web [40414522225](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40414522225)
terminó con `FULL FRESH-SAVE ARC I PASS`. El gate artístico se omite correctamente
porque no se modifica arte, sin declararlo aprobado.

[PR114](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/114) integrado como
`46dd9e892fd46c0405d62316251d8e3f51331aef`; su árbol
`d00279d286acac8cd8ac449e0021edcf052e8dcf` coincide exactamente con el árbol
del merge de prueba. En main: control de convergencia40416558995,
router40416559094, regresión específica40416559041, arquitectura40416559004,
cuarentena40416559008, planner40416559003 y gobernanza40416559348: SUCCESS.
El árbol Unity sigue siendo `4d1782c2eeb18892395bdc0d6701dde45ce8c573`.
Las identidades de ArtSource, art-source, updates, tester-v0265 y v0220 también
permanecen iguales respecto al inicio de esta fase. No nueva APK ni publicación.

Reversión si se necesitara: revisar en rama aislada
`git revert -m 1 46dd9e892fd46c0405d62316251d8e3f51331aef`.
No hay archivos movidos o borrados que restaurar. Los certificados técnicos de
Unity anteriores conservan su procedencia; esta etapa no los presenta como una
nueva ejecución Unity ni como medición de rendimiento móvil.
