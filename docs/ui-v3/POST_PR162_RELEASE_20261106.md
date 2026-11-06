# PR162 — integración y entrega Android/WebGL

## Cambios comprobados

PR162 integrada en main: e7e37d236e982553e06168f0ec216e4de80b6b03. Nombre de jugador y persistencia, pictogramas de recursos, niveles legibles y población renovable con demora UTC de 300 segundos. Se conserva el límite de 48 horas de producción ausente.

Correcciones de validación posteriores: c788c942b09c095e2ddf7fd636371301babbd357 completa el nombre mediante el formulario real en partidas de prueba avanzadas; conserva sin nombre la prueba específica de bienvenida; mantiene la firma de captura pública usada por reflexión y actualiza etiquetas tras preparar la cámara de captura. No cambia el runtime.

Pruebas nativas de c788c942: run42210784774, 216 EditMode y75 PlayMode, cero fallos/skips; compilación Windows PASS. La validación visual ampliada debe quedar respaldada por su ejecución final antes de cerrar esta entrega.

La ruta de certificación UI acepta ahora el responsable canónico de main para la fase PR162, manteniendo reserva exclusiva y rechazo de Unity/Blender ocupados. Se conserva la política que prohíbe lanzar trabajo pesado por edición del propio workflow. Evidencia de diagnóstico: run42210784700 rechazó la reserva por reconocer únicamente ramas históricas; run42213518514 detectó el disparador propio y se corrigió antes de publicar.

## Candidatos exactos

Android21: build42212292004, source79dca31d72abeb57e71db59f08943713ffabfcc5.
WebGL299: build42213699104, source1db8585329fad788fa702dab3e8a7b76a916b4bc.
Comparación entre fuentes: únicamente workflow de certificación, tres líneas de comentario en un Editor de pruebas, registro y solicitud de compilación. El código ejecutado del juego y sus assets son idénticos.

## Pendiente de completar con evidencia

Identidad y tamaño real APK, firma original, descarga pública íntegra, canal actualizado; certificado UI completo; QA Chromium normal clock con nombre por teclado/puntero, móvil vertical/horizontal, progresión, marchas y guardado/recarga; publicación Butler y versión remota activa.

No se atribuye a emulación o Windows una prueba física de teléfono. No se concede aprobación artística global por pasar compilaciones.

## Diagnóstico y correcciones posteriores

Android42212292004 produjo497493588bytes: rechazado correctamente por el límite450MiB del actualizador. No se elevó ese límite ni se publicó la APK rechazada. ZIP9 probado sobre las entradas mayores de code20 ahorra solo1116392bytes; librerías nativas ya están comprimidas. LZ4HC descartado porque la documentación de Unity indica que ZIP predeterminado comprime mejor en Android.

La captura V41 fallaba porque cambiaba a escala height/900 DESPUÉS de capturar. ValoriaIntegratedWorld.LateUpdate usa width/800 en vertical y height/900 en horizontal. cb8a20aaae20bc5c41e042ba34d781359019b737 elimina esa contradicción de fixtures y conserva los umbrales de geometría, legibilidad y espacio de escena.

Android retry42217404501/sourcee3d8347c5ea0aecf8c48f216d1599d2ca7704db9 prepara ASTC4x4 calidad100 solamente para ilustraciones grandes de EldoriaV41/EldoriaRetention. Mantiene resolución, originales y resto de plataformas, restaura byte a byte sus metadatos tras la compilación y exporta pruebas para comparar imágenes extraídas de la APK. Pendiente de resultado y revisión visual real.

WebGL42213699104/source1db8585329fad788fa702dab3e8a7b76a916b4bc SUCCESS, artifact13501518766 SHA2564adb472e14e384fafd5c55ba007457d82c8fddd81414ebc2b979ffd3774025b6. QA solicitada desdefd33bb831e3c01b53ba7e7677c9c3ca554406f7e; aún no es publicación.


## Resultado Android21 y revisión de empaquetado

Build42217404501 SUCCESS:470229532bytes (<450MiB), SHA256 b90d72811e89bc3e00ff3b88806d8d550794a6e94f96261213228a30cedd7913. Firma original f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8, paquete com.eldoria.valoria.v5test, versión1.21/code21, permiso instalador y sin reset de datos verificados. Solicitud de compilación desactivada08cf6fc para no duplicar.

Proof artifact13504605007; APK artifact13503044417. Verificación independiente de APK real con tools/android-updates/verify-apk-textures.py:32texturasASTC4x4,32dimensiones previas a compilación conservadas,32SHAfuentes verificados, metadatos restaurados. PSNR mínimo38.51dB; se inspeccionaron seis recortes de las diferencias mayores, sin corrupción o deterioro visible apreciable. La compresión es con pérdida, no identidad de píxeles. scout-action-skin-v4 ya estaba importada a2048x725 desde2107x746: referencia de comparación reescalada Lanczos, aproximación documentada; no se confunde con comparación exacta del filtro de Unity. No es aprobación artística global ni prueba física de móvil.

## Contrato de retorno al mundo

Browser42218825091 valida nombre realPruebaWeb_162,8direcciones de pan, zoom, cero errores runtime/red, pero falla la expectativa antigua de conservar el pan. TrySetWorld restablece explícitamente WorldHome + NearZoom en cada visita y la integración nativa exige esa conducta.0cc38475 corrige solo el test: dos retornos reales deben producir el mismo encuadre de Valoria y zoom64vertical/36horizontal. Nueva QA42222753516/sourceef6ccaecb76d00a046aa9f8012b3996c9e4a734e pendiente; usa los mismos bytesWebGL299 y conserva controles/progresión/guardado.
