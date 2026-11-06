# PR162 — integración y entrega cerradas

Cierre: 6 de noviembre de 2026. PR #162 integrada en `main`; APK Android 1.21 publicada y canal de actualización verificado; WebGL native299 activo en el canal restringido existente de itch.io. Presupuesto adicional: 0 €.

## Resultado y procedencia

| Entrega | Fuente exacta | Ejecuciones |
| --- | --- | --- |
| PR162 | `e7e37d236e982553e06168f0ec216e4de80b6b03` (merge) | PR #162 fusionada |
| Certificación ampliada | `cb8a20aaae20bc5c41e042ba34d781359019b737` | [42216689519](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42216689519) |
| Android 1.21 / code21 | `e3d8347c5ea0aecf8c48f216d1599d2ca7704db9` | Build 42217404501; publicación 42225704456 |
| WebGL native299 | `1db8585329fad788fa702dab3e8a7b76a916b4bc` | Build 42213699104; QA 42226584557 |
| Transporte WebGL | `ba371eaed375fe7f5d4f3a15f28b1fb0cc859932` | Preparación/subida 42230106275; activación 42230848280 |

Las fuentes Android y WebGL contienen el mismo código de juego. Sus diferencias son pruebas Editor, compresión específica de Android y metadatos de coordinación. La adaptación de transporte no modifica los datos descomprimidos de Unity.

## Cambios integrados y pruebas

Nombre de jugador Unicode persistente, pictogramas de recursos, niveles legibles de edificios/nodos/caza y regeneración UTC de 300 segundos. Se conserva el límite de 48 horas para producción ausente.

- 216 pruebas EditMode y 75 PlayMode: todas aprobadas, sin fallos ni omisiones. XML descargados y comprobados.
- 225 comprobaciones de integración, 20 encuadres y 16 destinos de gestión: PASS. Progresión continua I–V y recarga de la partida.
- V4.1, diálogos, tres marchas simultáneas, Vigilantes/Exploradores y Aldric: PASS. Capturas reales de bienvenida, niveles y cartelas revisadas.
- Prueba WebGL de 913,73 segundos: teclado/puntero real para el nombre, 844×390 y 390×844, navegación, combate, marchas, recompensas y guardado/recarga exactos. Cero errores de ejecución o red.
- Mejora del Bastión observada durante 120,523 segundos y entrenamiento durante 600,233 segundos, sin acelerar el reloj.
- 75 capturas y vídeo. El límite de capturas impidió cuatro imágenes adicionales; vídeo, telemetría y archivos de guardado permanecen disponibles.
- SHA-256 idéntico antes/después de recargar: `44ed01f9f3089a2b5e3ff293024ec50cd89fe3b6af3504f6a52d64ec750e80c7`.

Evidencia nativa principal: artifact 13504299840, SHA-256 `15aaae3f68bb146c34c994859cbc70d38073cc05413222696f72c083fc6cc185`. Evidencia del navegador: artifact 13506199633, SHA-256 `5a79b2fb45f89cfa762990a785a6d0fd743ea4c99f24ed88e18ac9364889c5c2`.

La compilación Windows general pasó por separado en 42210784774. La certificación ampliada de main no ejecutó de nuevo los bloques exclusivos de rendimiento/compilación Windows de PR136.

## Android publicado

[Descargar APK 1.21](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code21/Eldoria-Valoria-code21.apk).

- Tamaño: 470.229.532 bytes.
- SHA-256: `b90d72811e89bc3e00ff3b88806d8d550794a6e94f96261213228a30cedd7913`.
- Paquete: `com.eldoria.valoria.v5test`.
- Certificado original: `f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8`.
- Descarga pública comparada byte a byte; canal `updates/android/family.json` verificado en code21. Tres pruebas del canal aprobadas.
- Sin borrado de datos; solicitudes de compilación y promoción desactivadas tras la entrega.

La primera candidata excedía 450 MiB. Se optimizaron únicamente 32 ilustraciones grandes con ASTC 4×4 de alta calidad, conservando sus dimensiones importadas y restaurando los metadatos originales. El verificador `tools/android-updates/verify-apk-textures.py` decodificó las texturas reales de la APK, verificó las fuentes y comparó los píxeles. PSNR mínimo: 38,51 dB; error medio máximo: 1,90/255. Se inspeccionaron los seis recortes con mayor diferencia sin observar corrupción o deterioro apreciable. La compresión tiene pérdida; no se afirma identidad de píxeles.

Una fuente ya se importaba a 2048×725 desde 2107×746: su comparación usa una referencia Lanczos aproximada, expresamente identificada. Artifact de prueba: 13504605007; APK firmada: 13503044417.

## WebGL publicado

[Abrir Eldoria](https://chrismad.itch.io/eldoria-pruebas), canal `chrismad/eldoria-pruebas:web`, privacidad restringida conservada.

- Versión activa: `native299-0f7d6c8fb6d3`.
- Build remoto: **2313504**.
- Subida aceptada una sola vez a las 14:03:56 UTC.
- Activación confirmada a las 14:05:43 UTC por Butler, run 42230848280 / job 127936023155.
- ZIP publicado: SHA-256 `0f7d6c8fb6d34aa64de455eae2881820d5e930e1ebf285daf0bab6c093321d69`.
- Paquete: 19 archivos, 490.426.238 bytes extraídos; se conserva el límite estricto de 500 MB.
- Artifact de entrega: 13508660753, SHA-256 `ef1464c6cc2323d5f968fda374eabb04f6bbd11e9d2789ef8445b4d5dbd51d41`.

La primera preparación se detuvo antes de subir por tamaño. Gzip 9 produjo 518.717.830 bytes y también fue rechazado. Se adoptaron streams Brotli completos, compatibles con la [regla oficial de itch.io para archivos .br](https://itch.io/docs/creators/html5#compression). Los hashes de los datos descomprimidos se conservan.

El lector real de Unity pasó las pruebas HTTP de framework, código y datos con una única reserva de memoria por archivo. También se comprobaron gzip sin cabecera, errores HTTP, tamaños incorrectos y rechazo de Brotli sin cabecera CDN. Datos Unity: 585.372.590 bytes, SHA-256 `d153ac481b134970a5f29ba2d26edb0793dd77a4ab91c51cdfe19c8cebcb81f5`.

La subida terminó correctamente; el job de publicación marcó fallo porque su espera corta venció durante el procesamiento de itch.io. La comprobación independiente posterior confirmó la activación exacta. No se repitió la subida.

## Correcciones de validación y aprendizaje

- Completar el formulario real de nombre en partidas de prueba avanzadas; mantener sin nombre la prueba específica de bienvenida.
- Conservar la firma pública de captura utilizada mediante reflexión y reproyectar las etiquetas después de encuadrar.
- Usar la escala real: ancho/800 en vertical y alto/900 en horizontal, sin modificarla después de capturar.
- Ajustar la ruta de reserva al propietario canónico de main, conservando exclusividad y prohibición de iniciar trabajos pesados al editar su propio workflow.
- Verificar el retorno diseñado a Valoria en vista cercana, no la expectativa obsoleta de conservar el pan.
- Respetar los 120 segundos actuales del Bastión y la cartela de construcción sobre la ciudad; entrenamiento/marcha siguen siendo pantallas independientes.
- Separar el límite de entrada del archivo QA del límite estricto del paquete final publicado.

## Cierre y límites

Solicitudes de publicación desactivadas y seguimiento `6aed0cc2f0308191b29a13b542c886aa` desactivado. Recursos Windows y titularidad canónica liberados tras terminar los trabajos propios.

No se ha probado esta versión instalada en un teléfono físico ni se ha certificado una partida dentro del alojamiento restringido. La prueba completa se realizó sobre los bytes WebGL originales y la adaptación final pasó las comprobaciones del lector real de Unity. La integración/publicación queda cerrada; esto no concede una nueva aprobación artística global ni declara terminadas todas las mejoras visuales del proyecto.
