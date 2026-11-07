# Eldoria — actualización APK y WebGL posterior a PR163

Workstream-ID: eldoria-post-pr163-apk-webgl-release-20261107

## Alcance y estado

Actualizar la APK familiar y el canal WebGL existente con las mejoras integradas en main. Conservar guardados, firma Android original, acceso Restricted de itch.io y versiones anteriores. Presupuesto: 0 €. Forja Oscura PR173 no forma parte de esta entrega.

Estado: WebGL native325 compilada correctamente y225 comprobaciones de integración I–V PASS (tiempo acelerado); prueba Chromium42396852832 y APK42395466213 pendientes. Publicación nueva todavía no verificada. La distribución y congelación específica para testers es un encargo separado.

## Correcciones incorporadas y probadas

PR163 está integrada en `9efc0fefd447585f6867ab03ce2c41084b32adf2`. La revisión de entrega encontró defectos adicionales corregidos en PR177, integrada en `b686b335072022cc1576b18256e4eab33703bf11`:

- El temporizador de construcción usa el borde inferior real del edificio cuando no cabe encima.
- Entrenamiento conserva la composición horizontal en ventanas casi cuadradas.
- Sala de Héroes muestra RELICARIO / PRÓXIMO completo, conservando icono y fuente mínima.
- Las pruebas de captura miden el viewport sincronizado y respetan la referencia móvil de600 unidades y el consejo de tres líneas aprobado.

Fuente PR177 `c3282a6e7a5dce04c0937dc149e09944120ab0ae`: certificado `42389995761` SUCCESS, 216 EditMode y78 PlayMode, controles V4.1, integración y tres marchas simultáneas. Artefacto V4.1 `13580809061`, SHA256 `57234133a256ff859a9698db796140db077c1b96c0c5d21a7edb466e1090acc5`:143 archivos descargados y cotejados. Se inspeccionaron las capturas reales de los tres arreglos. Functional PASS; no aprobación artística global ni prueba física.

La validación general postmerge `42391771359` terminó SUCCESS, incluidos EditMode, PlayMode y compilación Windows.

## Candidatas actuales

- WebGL native325: fuente `948ffa9837168a0b68dab80be76b66350422b23a`, run `42391824345`.
- APK1.22/code22: firma y paquete originales; fuente solicitada `5b0769e4313d8f93b6dea1787e19a9ab3b87a95f`.
- Prueba de navegador preparada en la rama canónica QA: fuente `a9550aaf5332a2ac3810880c37ab2d8dbdcf5adb`; debe ejecutarse contra los bytes de native325 tras completar la compilación.
- `native324` y su ZIP `1484d43c4f3bfa23e4325add79be02826fbd989f3b3a46290a5dd3b4a24b3936` quedan descartados: preceden los arreglos runtime de PR177. Su transporte verificado no los convierte en una entrega válida.

## Comprobación adicional de retención

El certificado main `42391771211` pasó V4.1, diálogos y tres marchas, pero falló en `EarlyRetentionNativeCapture`: esperaba todavía800/900 unidades en teléfonos, mientras el juego integrado usa600. Se corrigió únicamente la aserción dentro del callback de captura. No cambia arte, gameplay, escenas ni código de player.

El gate de entrega admite exclusivamente ese cambio Editor con identidad exacta: blob `64a644923fbe7849043da2f2d997051ca4cbd1d9` → `2dcc4813b42a197dc093389285967ea15575d8c2`, ruta `Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/EarlyRetentionNativeCapture.cs`. Rechaza cualquier otro cambio Unity. Antes de publicar exige el certificado completo corregido de main `acfdad16e4e38a86010e3f5a9977800c0b941be0`, incluidas retención, diálogos, marchas y ambas suites; no basta con un run verde que omita esos pasos.

La gobernanza rechazó un primer disparador del workflow sobre sí mismo. Se eliminó en `acfdad16`; el disparador legítimo es la solicitud nativa existente. Actions sustituyó automáticamente la solicitud Android pendiente42391824417 antes de iniciar ningún job; la petición code22 se repuso con fuente5b0769e. No se interrumpió ningún proceso nativo.

## Condiciones de publicación

1. WebGL: QA real en Chromium con controles visibles, reloj normal y recarga de guardado; archivo fuente y hashes exactos. Empaquetado con identidad Unity decodificada intacta, máximo500MB extraídos y200MB por archivo. Publicar una sola vez en `chrismad/eldoria-pruebas:web` y comprobar que la versión remota está activa.
2. Android: comprobar paquete `com.eldoria.valoria.v5test`, certificado SHA256 `f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8`, integridad y tamaño≤450MiB. Crear release inmutable; cotejar los bytes descargados antes de avanzar `updates/android/family.json`.
3. Deshabilitar peticiones completadas y devolver a Forja la reserva Windows tras terminar todos los jobs nativos.

## Límites y aprendizajes

El alojamiento Restricted no permite afirmar una partida en itch por acceso anónimo. La prueba Chromium sobre el paquete y la activación remota son evidencias distintas. No se ha realizado instalación física Android, rendimiento iPhone ni aprobación artística global.

Medir geometría dentro del viewport sincronizado; comprobar que las capturas se ejecutan realmente; revisar expectativas históricas antes de cambiar producto; regenerar binarios tras cualquier arreglo runtime. Una excepción Editor exige ruta y blobs exactos, comprobación completa corregida y ninguna diferencia en código de player. Respetar la gobernanza de disparadores y la cola compartida entre builds.

La preparación del ZIP no publica nada y puede solaparse con Windows QA. La promoción exige todos los certificados, el artefacto QA real y el ZIP exacto preparado. Diez pruebas locales verifican también que un recibo QA ausente permite únicamente preparación y siempre bloquea publicación.
