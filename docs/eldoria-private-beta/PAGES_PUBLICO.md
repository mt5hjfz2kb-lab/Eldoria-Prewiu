# Beta pública en GitHub Pages — decisión de 7/11/2026

El propietario autoriza un enlace público compartible, sin Cloudflare, login, contraseña ni APK. Esta decisión sustituye el bloqueo anterior por falta de cuenta Cloudflare. El contenido sigue congelado en native325; no cambia Unity ni el tutorial.

## Preparación implementada
- `prepare-pages.py` verifica el ZIP original por SHA256, transforma la compresión Brotli en gzip por fragmentos y verifica los bytes descomprimidos.
- Los binarios Unity y archivos de juego conservan sus bytes; cambia únicamente el transporte y `build-manifest.json`.
- La web anterior en `unity-owner/` se copia antes de desplegar, junto con su índice. La beta tiene ruta propia `beta-native325/`.
- El workflow de Pages vive en main por las restricciones del entorno github-pages y usa el código de empaquetado de la rama aislada mediante SHA fijado. No requiere Unity ni Windows.

## Limitaciones que permanecen
- `native325` todavía no emite los 67 eventos de gameplay. La web pública no debe usar directamente el Worker privado ni insertar claves privadas en HTML.
- El enlace público permite compartirlo; no hay protección de invitaciones.
- No enviar invitaciones de medición hasta verificar la conexión de los eventos y la experiencia de los dispositivos elegidos.
- Un hash y una descarga HTTP correctos no acreditan partida completa ni rendimiento de iPhone.

## Conservación
La copia original permanece en la release draft `eldoria-beta-native325-frozen-20261107`. No se cambia el archivo congelado al adaptar su transporte para Pages. La próxima ronda debe usar otra ruta/identidad.
