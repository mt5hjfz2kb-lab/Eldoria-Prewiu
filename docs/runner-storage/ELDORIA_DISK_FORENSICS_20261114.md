# Eldoria — recuperación y prevención del llenado de C: (14/11/2026)

Estado: **recuperación real verificada; prevención automática comprobada estáticamente pero pendiente de integración y primera ejecución real**.

## Causa y evidencia
El equipo Windows autoalojado `DESKTOP-R10PE55` mantuvo copias de trabajo y cachés de Unity de sucesivas pruebas. No era la cuota de GitHub: se trataba de `C:\` del ordenador.

- Inventario [43601135345](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43601135345): `C:\` 930,77 GiB totales, 920,66 usados, 10,11 libres. Directorio del ejecutor `C:\actions-runner-eldoria\_work`: 424,38 GiB, incluyendo `Unity/Library` 126,42; `Unity/Assets` 132,66; Git 72,37; outputs 22,47 y otros 70,45 GiB.
- [43602195188](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602195188): se clasificaron ocho cachés aisladas (~70,531 GiB), y cinco de proyectos compartidos (~50,144 GiB) protegidas.
- [43602596197](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602596197): eliminación real SOLO de seis `Unity/Library`, con fuente/HEAD/run/permisos/junctions comprobados. Espacio libre 9,673 → 56,686 GiB, **+47,013 GiB**. Ningún `Assets`, `.meta`, `ProjectSettings`, `.git` o captura eliminado. Dos directorios con HEAD divergente quedaron intactos.
- [43603053226](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603053226): otros grupos medidos; `Program Files (x86)` 258,674 GiB, perfiles 98,30 GiB, `Program Files` 42,794 GiB, `C:\` archivos raíz 39,858 GiB. Son categorías actuales, no prueba de cuándo se crearon.
- [43603542678](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603542678) y [43605103207](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43605103207): plataforma de videojuegos 123,091 GiB y otro juego instalado 105,588 GiB, independientes de cachés del ejecutor. La raíz C: contiene Windows pagefile 33,469, hibernación 6,373 y swap 0,016 GiB; no son archivos temporales de Eldoria ni se deben borrar indiscriminadamente.
- [43606616837](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43606616837): inventario posterior a una nueva prueba Unity: C: **46,62 GiB** libres y directorio del ejecutor **387,131 GiB**. Reaparecieron aproximadamente 10 GiB de ocupación tras la recuperación anterior; esto verifica que sin limpieza recurrente el disco vuelve a llenarse.
- [43603542306](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43603542306): prueba Vigilia posterior llegó a Unity real con ~52,26 GiB libres después del checkout y falló por un aserto de filtrado del sprite de recursos, NO por falta de espacio.

## Acción preventiva propuesta para main

`.github/workflows/eldoria-postrun-unity-cache.yml`:
- Se activa **solo al completar** los flujos nativos admitidos y cuyos trabajos proceden del mismo repositorio.
- No consume la máquina para ramas o nombres de trabajos desconocidos. Usa la cola de concurrencia de Vigilia; no cancela procesos ni tests.
- Comprueba el ID exacto de ejecución finalizada, SHA, nombre del flujo, artefacto subido y vigente, origen Git, HEAD local exacto, `Library` no versionada, ausencia de uniones simbólicas y de Unity/Blender activos.
- Elimina **únicamente** `Unity/Library` de una copia aislada de ejecución específica con el mismo mecanismo `robocopy /MIR /XJ` que funcionó en [43602596197](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43602596197). Nunca borra fuentes, modelos, archivos originales, `.meta`, `.git` ni evidencia. En cualquier discrepancia se detiene o omite esa copia.
- Aun integrada, reduce la acumulación **de cachés**; las copias de `Unity/Assets` y Git exigen una política posterior de archivo y deduplicación antes de poder eliminarse. No afirmar que se ha resuelto la ocupación de la máquina para siempre.

Comprobación estática hospedada: [43605893451](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43605893451) SUCCESS. No equivale a prueba de la eliminación automática en el equipo físico. Primera ejecución real **pendiente** y debe verificarse en GitHub tras integrar el flujo.

Documentación de terceros:
- Unity, caché `Library`: https://docs.unity3d.com/Manual/AssetWorkflow.html — Unity puede regenerarla con el proyecto cerrado.
- GitHub, directorio temporal del runner: https://docs.github.com/en/actions/reference/workflows-and-actions/variables — el workspace de un ejecutor autoalojado no debe tratarse como archivo histórico permanente.

## Reglas de protección
No eliminar juegos instalados, `pagefile.sys`, `hiberfil.sys`, directorios compartidos, objetos `.git`, `Assets`, versiones modificadas ni fuentes de usuario. No eliminar carpetas completas basándose solo en el nombre del run. No reiniciar ni cancelar trabajos concurrentes. Guardar evidencias fuera del runner antes de recuperar cachés. Informar del espacio exacto recuperado y de las reservas de disco antes de iniciar cada importación.
