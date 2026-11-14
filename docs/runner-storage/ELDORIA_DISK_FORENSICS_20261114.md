# Eldoria — recuperación y prevención del llenado de C: (14/11/2026)

Estado: **recuperación real verificada, PR #192 fusionada en `main` y primera activación automática `workflow_run` probada físicamente el 14/11/2026; aún quedan fuentes históricas sin clasificar o preservar.**

## Causa y evidencia
El equipo Windows autoalojado `DESKTOP-R10PE55` mantuvo copias de trabajo y cachés de Unity de sucesivas pruebas. No era la cuota de GitHub: se trataba de `C:\` del ordenador.

- Inventario [43601135345](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43601135345): `C:\` 930,77 GiB totales, 920,66 usados, 10,11 libres. Directorio del ejecutor `C:\actions-runner-eldoria\_work`: 424,38 GiB, incluyendo `Unity/Library` 126,42; `Unity/Assets` 132,66; Git 72,37; outputs 22,47 y otros 70,45 GiB.
- [43602195188](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602195188): se clasificaron ocho cachés aisladas (~70,531 GiB), y cinco de proyectos compartidos (~50,144 GiB) protegidas.
- [43602596197](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602596197): eliminación real SOLO de seis `Unity/Library`, con fuente/HEAD/run/permisos/junctions comprobados. Espacio libre 9,673 → 56,686 GiB, **+47,013 GiB**. Ningún `Assets`, `.meta`, `ProjectSettings`, `.git` o captura eliminado. Dos directorios con HEAD divergente quedaron intactos.
- [43603053226](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603053226): otros grupos medidos; `Program Files (x86)` 258,674 GiB, perfiles 98,30 GiB, `Program Files` 42,794 GiB, `C:\` archivos raíz 39,858 GiB. Son categorías actuales, no prueba de cuándo se crearon.
- [43603542678](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603542678) y [43605103207](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43605103207): plataforma de videojuegos 123,091 GiB y otro juego instalado 105,588 GiB, independientes de cachés del ejecutor. La raíz C: contiene Windows pagefile 33,469, hibernación 6,373 y swap 0,016 GiB; no son archivos temporales de Eldoria ni se deben borrar indiscriminadamente.
- [43606616837](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43606616837): inventario posterior a una nueva prueba Unity: C: **46,62 GiB** libres y directorio del ejecutor **387,131 GiB**. Reaparecieron aproximadamente 10 GiB de ocupación tras la recuperación anterior; esto verifica que sin limpieza recurrente el disco vuelve a llenarse.
- [43603542306](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603542306): prueba Vigilia posterior llegó a Unity real con ~52,26 GiB libres después del checkout y falló por un aserto de filtrado del sprite de recursos, NO por falta de espacio.

## Prevención integrada en main mediante PR #192

`.github/workflows/eldoria-postrun-unity-cache.yml`:
- Se activa **solo al completar** los flujos nativos admitidos y cuyos trabajos proceden del mismo repositorio.
- No consume la máquina para ramas o nombres de trabajos desconocidos. Usa la cola de concurrencia de Vigilia; no cancela procesos ni tests.
- Comprueba el ID exacto de ejecución finalizada, SHA, nombre del flujo, artefacto subido y vigente, origen Git, HEAD local exacto, `Library` no versionada, ausencia de uniones simbólicas y de Unity/Blender activos.
- Elimina **únicamente** `Unity/Library` de una copia aislada de ejecución específica con el mismo mecanismo `robocopy /MIR /XJ` que funcionó en [43602596197](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602596197). Nunca borra fuentes, modelos, archivos originales, `.meta`, `.git` ni evidencia. En cualquier discrepancia se detiene o omite esa copia.
- Aun integrada, reduce la acumulación **de cachés**; las copias de `Unity/Assets` y Git exigen una política posterior de archivo y deduplicación antes de poder eliminarse. No afirmar que se ha resuelto la ocupación de la máquina para siempre.

PR [#192](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/192) fusionada el 14/11/2026. Comprobación estática y parseo PowerShell en [43606178339](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43606178339) SUCCESS. **Primera limpieza automática física comprobada:** al finalizar la prueba Unity de Vigilia [43615782618](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43615782618) el 14/11/2026, el `workflow_run` [43617042919](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43617042919) ejecutó `reclaim` con `success` en `DESKTOP-R10PE55`. El log registra `POSTRUN_CACHE_RECLAIMED run=43615782618 GiB=5,621 assetsDeleted=false` a las **16:42:46 UTC**. Esto prueba la eliminación acotada de esa `Unity/Library`, pero NO prueba por sí solo la retención permanente ni una nueva medición de `C:` total. La ejecución Unity previa terminó con un error visual de recorte real del texto `RECURSOS` (29,7 px solicitados, 16,0 disponibles), no con error de almacenamiento. El checkout emitió avisos sobre una ruta Git `alternates` ausente hacia `breachwatch-isolated-43532823031-1`: antes de retirar esta fuente o sus dependientes hay que verificar toda la cadena de alternates.

Documentación de terceros:
- Unity, caché `Library`: https://docs.unity3d.com/Manual/AssetWorkflow.html — Unity puede regenerarla con el proyecto cerrado.
- GitHub, directorio temporal del runner: https://docs.github.com/en/actions/reference/workflows-and-actions/variables — el workspace de un ejecutor autoalojado no debe tratarse como archivo histórico permanente.


## Retiradas históricas realizadas con preservación de originales únicos

Las copias listadas abajo se retiraron **solo después** de generar un ZIP de archivos locales modificados, no versionados e ignorados con hash SHA256 por archivo y manifiesto; conservar una copia exacta de `.git` y comparar sus hashes; comprobar que no existían referencias Git `alternates` desde otros worktrees; certificar origen/HEAD exactos; comprobar que Unity, Blender y Git no estaban activos; y subir una copia de la lista de originales a GitHub Actions. Las copias completas se podían reconstruir desde su commit Git + el respaldo único.

| Directorio aislado retirado | Run final con prueba física | Recuperado (GiB) | Espacio libre tras operación (GiB) |
|---|---|---:|---:|
| `breachwatch-native-source-43465479546-1` | [43609888294](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43609888294) | 6,260 | 74,971 |
| `breachwatch-native-source-43483165131-1` | [43610788815](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43610788815) | 6,230 | 77,857 |
| `breachwatch-native-source-43481923633-1` | [43610788815](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43610788815) | 6,229 | 84,085 |
| `breachwatch-native-source-43469529824-1` | [43612929494](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43612929494) | 6,378 | 88,754 |
| `forja-thin-native-source-42404744384` | [43613446447](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43613446447) | 6,055 | 93,203 |
| `beta-structural-source-42505611257-1` | [43613930941](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43613930941) | 12,442 | 103,783 |
| `issue186-fresh-source-43503266427` | [43614706067](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43614706067) | 5,484 | **107,704** |

Última medida física absoluta verificada el 14/11/2026 antes de la nueva prueba nativa: **107,704 GiB libres** en `C:\`. Después hubo una prueba Unity con consumo temporal y recuperación post-run de 5,621 GiB, pero NO existe una nueva medición total de espacio libre. No sumar estas variaciones para inventar un valor absoluto.

Los respaldos externos más recientes por los que se puede verificar la restauración son:
- `b695`: fuente exclusiva artefacto GitHub `14120946457` (661 ficheros), manifiesto Git `14121260373` (39 entradas), fuente `ec4d664e8765a4ee3d97f71f075820b10f04f449`.
- `f047`: fuente `14121341145` (126), Git `14121246594` (39), fuente `4d838a69f7db40232c9c9516e9a0ec109f009e2f`.
- `b425`: fuente `14121008070` (119), Git `14121461912` (32), fuente `948ffa9837168a0b68dab80be76b66350422b23a`.
- `i032`: fuente `14120719795` (182), Git `14121920431` (40), fuente `b63b94743efc84ec156d9ae5239143c6048fe069`.

## Preservación preventiva verificada de otro checkout del tutorial (14/11/2026)

**SOURCE_ARCHIVE_I111: original aún intacto; NO se ha retirado.** Se identificó `issue186-fresh-source-43511148168` (~5,475 GiB en el inventario del 14/11). Corresponde a [la ejecución histórica 43511148168](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43511148168), terminada `failure`, SHA remoto `eb2fbffffc957d8b1e41334b0cd3b8b4e68983e6`; el fallo antiguo no autoriza su eliminación por sí solo.

- Copia diferencial de **186 archivos de trabajo locales**, incluida selección de modificados tracked (índice y árbol), untracked e ignored: [Actions 43617490592](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43617490592), `SOURCE_DELTA_ARCHIVE_VERIFIED`, ZIP `4,63 MiB`, SHA256 `7ddf014978e533fd42f0aadead6afc0efba884b807fdc6f34c9e82663d4424d8`, artefacto externo **14122299376** (`eldoria-unique-i111-43617490592`, retención 90 días). Original sin eliminar.
- Copia exacta de **40 archivos Git originales**, **1,560 GiB** preservados separadamente en el runner con verificación SHA256 archivo por archivo: [Actions 43617632234](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43617632234). SHA256 del manifiesto `05057631a0cd17f6f2da996184538a2a294be55556fa10ea9bcc3aed3f267366`, artefacto externo del manifiesto **14122842061** (`eldoria-git-manifest-i111-43617632234`, 90 días). **Los objetos Git completos aún están locales, no consta copia externa de esos 1,560 GiB.**
- Última lectura física de la operación de respaldo: `C:` **101,916 GiB libres**, 14/11/2026 a las **16:49:22 UTC**, anterior a un nuevo trabajo Unity. No atribuir a este archivo ninguna liberación de espacio: archivó, no eliminó; el espacio disminuyó por la duplicación protegida.
- Antes de retirar esta fuente debe verificarse que el nuevo trabajo Unity ha concluido y que `windows-runner-heavy` ha sido liberado en el registro vivo; respaldar externamente los objetos Git si su singularidad lo requiere; comprobar los alternates entrantes y salientes; verificar el ZIP, SHA, manifiestos y todas las fuentes únicas inmediatamente antes de retirar la copia. Durante esta comprobación [Issue186 43617649660](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43617649660) había reclamado el recurso Windows; no se encoló ninguna retirada.

Las siete retiradas históricas anteriores mantienen los ZIP de deltas y manifiestos externos con retención de **90 días**, y los objetos Git originales preservados localmente bajo `C:\actions-runner-eldoria\_preserved_snapshots`. Que un archivo manifiesto esté fuera del ordenador no prueba que todo su objeto Git también lo esté.

## Alcance pendiente y restricciones comprobadas

La medición anterior de todo el entorno Eldoria fue 385,991 GiB cuando quedaban 46,62 GiB libres; **el tamaño actual del entorno no se ha vuelto a medir de forma completa** después de estas retiradas. No estimarlo como si fuese una nueva medición.

La última inspección [43612099033](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43612099033) identificó fuentes que NO se pueden borrar sin más:
- `valoria-layered-source` 66,34 GiB, fuente reciente, caché de Unity y errores de acceso a rutas largas durante el inventario; se considera activa/protegida.
- `timer-source` 22,48 GiB, `delight-source` 18,07, `daily-source` 11,82, `issue186-source` 11,32: directorios compartidos y con cambios locales.
- `forja-source` 17,46 GiB sin `.git`: material posiblemente único; no eliminar.
- `beta-structural-source-42427811334-1` 17,76 GiB con HEAD inválido: no eliminar; otras copias beta antiguas tienen miles de cambios `git status`.
- `breachwatch-isolated-43532823031-1` tiene al menos dos dependientes por `alternates`; no tocar mientras existan dependencias.
- `breachwatch-native-source-43489752868-1` conserva evidencia de una beta integrada exitosa y debe protegerse como referencia.
- Tres fuentes históricas más del tutorial de unas 5,5 GiB cada una, con cambios locales y sin `Library`, son candidatas a la misma técnica de archivo y retirada una por una si se confirma que ya no son fuentes calientes de otra ejecución.

La finalidad final es conservar un proyecto de trabajo estable con sus activos únicos y eliminar copias efímeras verificadas, no prometer un límite artificial ni borrar datos para alcanzar 100–150 GiB.

## Reglas de protección
No eliminar juegos instalados, `pagefile.sys`, `hiberfil.sys`, directorios compartidos, objetos `.git`, `Assets`, versiones modificadas ni fuentes de usuario. No eliminar carpetas completas basándose solo en el nombre del run. No reiniciar ni cancelar trabajos concurrentes. Guardar evidencias fuera del runner antes de recuperar cachés. Informar del espacio exacto recuperado y de las reservas de disco antes de iniciar cada importación.
