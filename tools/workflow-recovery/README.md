# Recuperación de resultados nativos sin repetir el editor

Esta herramienta conserva los resultados de una ejecución, incluso si falla su incorporación a Git. Está conectada a `.github/workflows/eldoria-spawnling-native.yml`; no modifica el juego ni autoriza otra ejecución del engendro ya cerrado.

## Qué se conserva

Antes de publicar, el workflow copia únicamente archivos nuevos/modificados y eliminaciones dentro de las rutas explícitas de `spawnling.json`. El artefacto `spawnling-native-recovery-RUN_ID-ATTEMPT` contiene los bytes originales, sus hashes, el SHA fuente, el run y si pasaron las dos fases nativas. Se sube antes del envío a Git y se retiene 30 días. Los artefactos incompletos no se pueden publicar como resultados validados.

El publicador construye el commit con un índice temporal. No limpia, cambia, guarda en stash ni rebasa el directorio del editor; sus cambios ajenos permanecen intactos. Conserva commits documentales concurrentes. Si cambian Unity, las fuentes declaradas, el mismo archivo de salida o la titularidad, se detiene conservando la copia. Nunca fuerza un push. La captura no incluye claves, configuración del usuario ni todo el disco: sus rutas están enumeradas y acotadas.

## Recuperación autorizada

1. Descargar el artefacto de la ejecución original y conservar una copia íntegra. Usar el checkout del repositorio y su autenticación Git ya configurada, con la configuración versionada que coincide con el checkpoint.
2. Comprobar `checkpoint.json`: fuente, run, rutas y `validated`. El artefacto es una copia recuperable, no una aprobación artística ni autorización de publicar el juego.
3. Si solo falló la incorporación a Git y siguen siendo válidas las entradas y la titularidad, ejecutar:

```sh
node tools/workflow-recovery/checkpoint.mjs publish --root RUTA_CHECKOUT --config tools/workflow-recovery/spawnling.json --checkpoint RUTA_ARTEFACTO
```

4. La herramienta verifica hashes e identidad y vuelve a consultar la rama. Ante conflicto de archivos o entradas protegidas, inspeccionar y reconciliar el cambio concreto; no repetir Unity por un mero error de push ni aplicar ciegamente una salida sobre una escena posterior.

`ALREADY_PRESENT` solo indica que los mismos archivos ya están en la rama; no certifica otros cambios remotos posteriores. Ningún paso sube a itch.io o Android, cambia solicitudes, reabre workstreams o gasta créditos.

## Límites y mantenimiento

- La copia debe completarse y subirse. No garantiza supervivencia ante apagado del PC, pérdida de disco o terminación forzada anterior a esa subida. Un checkpoint parcial nunca obtiene permiso de publicación.
- Las copias locales no se eliminan automáticamente; no limpiar un workspace que contenga resultados sin recuperar.
- La retención de artefactos es finita. Los resultados aprobados deben integrarse y sus fuentes conservarse en Git por el propietario correspondiente.
- Para otro productor, definir sus salidas y sus entradas exactas, acordar su alcance vigente y conectar captura → subida → publicación. No añadir una segunda escena o cola de producción.
- Las pruebas usan repositorios Git temporales y verifican conflictos, archivos binarios, eliminaciones, QA fallida, hashes alterados y conservación de los cambios locales. No ejecutan Unity ni servicios de pago.

## Estado breve

`node tools/work-status/report.mjs --json` ofrece el estado del checkout sin histórico ni escrituras. El workflow `eldoria-work-status.yml` añade observaciones de Actions y genera Summary/artefacto, con fecha y SHA. Sus permisos son de lectura y no activa trabajos ni publicaciones. Los jobs observados no sustituyen reservas ni detectan procesos abiertos fuera de Actions.
