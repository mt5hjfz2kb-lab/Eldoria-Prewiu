# Unidad cerrada a nivel de fuente: identidad espacial 4X

## Commits
- Punto de recuperación y limitaciones: `d139e2104ac0a75bd7463550ba81cae87ae63bae`.
- Contrato de identidad espacial: `28e1bdc81934e596073434bf0ae8110d05ccc1eb`.
- Prueba de contrato: `ace60361b653c6f7078cece5cc4e42050bd3e1c6`.

## Qué se ha implementado
`tools/world4x-zone1/world-spatial-contract.mjs` aporta una representación inmutable de coordenadas de celda [0,127] y entidades con identificadores estables, divisiones de chunks 16x16 y acceso a los mismos objetos al consultar zoom global, regional o local. Rechaza identificadores inválidos, posiciones fuera del mundo, IDs repetidos y ocupación duplicada de una celda. No importa el gameplay ni altera partidas.

`tools/world4x-zone1/world-spatial-contract.test.mjs` contiene pruebas de consistencia de tres zooms, celdas/chunks, entradas inválidas y ausencia de funciones de escritura. Coordenadas de ejemplo son datos de prueba, no posiciones canónicas de ciudades.

## Estado de verificación
**Archivos y commits confirmados mediante conector GitHub.** La fuente y las pruebas están escritas y guardadas, pero **NO se ejecutaron estas pruebas en GitHub Actions ni Unity en esta sesión**. El contenedor de este turno tiene Node 22.16.0, pero no puede resolver github.com para recuperar el checkout remoto; `git ls-remote` devolvió `Could not resolve host: github.com`. El checkout local anterior con modelos 3D tampoco está montado en este entorno. Las evidencias Blender locales previas no están por tanto guardadas en estos commits.

Prueba exacta a ejecutar en el checkout de esa rama: `node tools/world4x-zone1/world-spatial-contract.test.mjs`.

## Siguiente acción exacta
Recuperar y conservar los archivos .blend/GLB/PNG auténticos del checkout anterior (sin inventarlos), ejecutar el test Node en el mismo checkout y realizar nueva comparación visual regional/global/local. Tras revisión A/B visual independiente, coordinar una captura nativa Unity únicamente cuando el runner Windows sea cedido por Valoria. No fusionar ni publicar automáticamente.
