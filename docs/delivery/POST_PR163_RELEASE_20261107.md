# Eldoria — APK y WebGL actualizadas tras PR163

Workstream-ID: eldoria-post-pr163-apk-webgl-release-20261107

## Entrega completada — 7 noviembre 2026

- **WebGL native325** publicada en [itch.io](https://chrismad.itch.io/eldoria-pruebas), canal web Restricted conservado. Butler confirmó build2324364/versionnative325-fb1292b3d9ce ACTIVE a las18:57:35UTC. [Publicación42403626248](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42403626248) SUCCESS; una única subida.
- **APK1.22/code22** publicada y canal familiar22 activado. [Descargar APK](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code22/Eldoria-Valoria-code22.apk). [Publicación42404008210](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42404008210) SUCCESS; descarga pública comparada byte por byte antes de actualizar el canal.
- Peticiones de compilación/publicación deshabilitadas; WebGL delivery_complete impide repetir preparación o subida. Versiones anteriores conservadas. Reserva Windows devuelta a Forja (revisión6); trabajo de entrega archivado como completado. Presupuesto0€.

## Alcance y fuentes

Incluye PR163 integrada en9efc0fefd447585f6867ab03ce2c41084b32adf2 y reparaciones PR177 integradas enb686b335072022cc1576b18256e4eab33703bf11. PR178 integra el mecanismo de entrega verificada. Forja Oscura PR173 permanece independiente y no se incluye.

Reparaciones PR177: temporizador de construcción situado según el borde real del edificio; entrenamiento conserva composición horizontal en ventanas casi cuadradas; RELICARIO/PRÓXIMO cabe en Sala de Héroes. Se corrigieron las mediciones de capturas sincronizadas, la escala móvil600 y el consejo de tres líneas, sin sustituir arte.

WebGL: fuente948ffa9837168a0b68dab80be76b66350422b23a, build42391824345. Android: fuente5b0769e4313d8f93b6dea1787e19a9ab3b87a95f, build42395466213. La única diferencia Unity posterior a native325 es la aserción Editor EarlyRetentionNativeCapture: blob64a644923fbe7849043da2f2d997051ca4cbd1d9 →2dcc4813b42a197dc093389285967ea15575d8c2. El gate comprueba ruta y blobs exactos; todo el código de player coincide.

## Pruebas realizadas

- Certificado completo corregido42395390748/sourceacfdad16e4e38a86010e3f5a9977800c0b941be0: **216EditMode y78PlayMode,0fallos**; controles V4.1, buzón persistente, diálogos, tres marchas, retención/descubrimientos, Aldric y escena I–II. No se atribuye compilación Windows a este run: ese paso se omitió según sus condiciones. La validación general postmerge42391771359 sí compiló Windows.
- Integración native325:225checks I–V PASS con tiempo acelerado.
- Chromium Windows42396852832: **I–II completo en930.66s con reloj normal**,10operaciones, marchas/recompensas, movimiento visible, nombre por teclado, guardado exactamente igual tras recarga;0errores runtime y0fallos de red.75capturas directas y vídeo revisado por muestras. SHA256 guardado d175b08141b7c8de781dcf4f34b7101106cd1723ccbaaae8654eeaab44c594a6.
- Carga local29.515s; recarga19.078s. Son medidas locales, no de Internet ni teléfono. Se alcanzó el límite75capturas; recarga comprobada en vídeo y telemetría. WebM837.36s: su línea temporal no equivale al reloj real930.66s.
- APK: apksigner/aapt en build, descarga independiente con hash y CRC1120entradas, manifiesto verificado,32texturas reales ASTC4x4 con dimensiones conservadas y hashes de referencia comprobados mediante UnityPy1.25.4. Peor PSNR38.51dB/error medio1.90sobre255; recorte comparado sin deterioro visible aparente.
- Transporte WebGL: hashes originales y bytes Unity decodificados intactos, tests del lector Unity y de errores PASS.19archivos/490317080bytes extraídos; límites500MB total y200MB/archivo conservados.
- Once tests del gate verifican rechazo de fuentes distintas, QA pendiente, excepción Editor incorrecta y publicación duplicada.

## Identidades verificables

APK470244892bytes SHA256f8fa1d3d0c9f8c6b170a31abcd8513d221e4ddb63586614322afa63b63a70e41. Paquete com.eldoria.valoria.v5test; certificado original f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8. No borrado de datos.

ZIPWebGL SHA256fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771; preparación42397405907/artefacto13583674142 SHA2562e1e34480eabd2f5134f09f2d17e57c98ca8d72a603cfe040549f218b789c80f. QA13585239386 SHA25660395ec004feeffc1f4eb4a854a0f0e8efa40a524dd8f0d43112bab502cc3444. Recibo13586298867 SHA25697780df94a9bf8860432ce8750463461175f8d8ea772ca24de491f100fa08577. Todos descargados y cotejados.

## Límites y aprendizaje

No se ha instalado en teléfono físico ni jugado la versión alojada en itch; su activación remota y el acceso Restricted son distintos de una prueba alojada. No se otorga aprobación artística global: persiste deuda estética existente, como el rótulo Exploradores partido en portrait. La congelación de una versión para testers es otro encargo.

Medir geometría tras sincronizar viewport; exigir pasos nativos realmente ejecutados; no reconstruir paquetes verificados ni repetir una subida que esté procesándose. Las excepciones Editor requieren identidad exacta y certificado completo. Guardar el recibo remoto y deshabilitar peticiones al cerrar la entrega.
