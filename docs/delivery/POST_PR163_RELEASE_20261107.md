# Eldoria — actualización APK y WebGL posterior a PR #163

Workstream-ID: eldoria-post-pr163-apk-webgl-release-20261107

## Encargo y alcance

El propietario autorizó actualizar el canal APK firmado familiar y la WebGL ya existente tras las mejoras de experiencia de PR #163. Esto **no es la distribución prevista para los futuros testers**. Se conservan guardados, certificado Android original, acceso Restricted de itch.io, costes cero y versiones previas inmutables.

## Evidencia disponible

- Código de PR163 incorporado en main: merge 9efc0fefd447585f6867ab03ce2c41084b32adf2.
- Unity/WebGL native324: run 42367983297 completado con éxito y 225 verificaciones de integración del recorrido I–V (reloj acelerado para prueba).
- Unity UI: run 42367986501, 216 EditMode y 78 PlayMode aprobados.
- Chromium Windows normal-clock: run 42373214530, 62 acciones visibles, 10 operaciones, 5 marchas, guardado idéntico tras recarga y sin errores de ejecución o red. No supone prueba física Android/iPhone ni una segunda partida manual I–V.
- El canal Android publicado sigue en 1.21/code21, firma SHA256 f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8; siguiente versión 1.22/code22.
- El canal WebGL Restricted chrismad/eldoria-pruebas sigue en native299.

## Preparación sin sustituir la prueba real

El flujo nuevo en Ubuntu comprueba la procedencia exacta del artefacto nativo, el resultado de Chromium, la configuración de la rama QA y el árbol Unity integrado en main. Reutiliza los mismos bytes originales WebGL y comprueba la identidad de los datos Unity tras adaptar el transporte a itch con Brotli. Aplica los límites de 500 MB totales y 200 MB por archivo; la prueba es negativa hasta que el propio workflow termine PASS. No accede a las credenciales de itch ni publica nada mientras publish_enabled=false.

## Condiciones previas a la publicación

1. PR177: la discrepancia en el test de capturas V4.1 debe quedar certificada en Unity; un fallo por recurso Windows ocupado no es un PASS.
2. WebGL: paquete preparado, hashes y transporte comprobados; publicación Restricted al canal ya autorizado y verificación remota de la versión realmente activa. No dar por probado el alojamiento únicamente porque Butler acepte subir archivos.
3. Android: compilar y firmar code22 usando la clave original del runner Windows después del relevo legítimo de PR173. Comprobar certificado, nombre de paquete, integridad, límite de tamaño y que el actualizador conserve la partida. Publicar APK inmutable y comprobar el archivo remoto antes de cambiar family.json.
4. La prueba física de Android y el rendimiento en iPhone siguen siendo comprobaciones separadas. No afirmar que se han realizado.

## Coordinación

Workstream canónico independiente: eldoria-post-pr163-apk-webgl-release-20261107. Scope de entrega y pipeline exclusivamente. PR177 mantiene la corrección nativa; PR173 Forja Oscura mantiene windows-runner-heavy; este encargo no cancela sus jobs, no cambia sus archivos y no consume el runner hasta un relevo autorizado.

## Relevo y paquete comprobado por el ejecutor actual

El propietario reasignó expresamente esta misma entrega a `codex-release-a56bdf7714d0`, revisión 4 del registro. PR178 continúa siendo la única PR de entrega.

Preparación `42383793241` SUCCESS, artefacto `13578463741`, SHA256 del archivo Actions `ef025d6c265e0c165b163cfec542ecbe5d4081c8b726f2d32ea598e4988f5eab`. Descargado y cotejado localmente; ZIP HTML5 `1484d43c4f3bfa23e4325add79be02826fbd989f3b3a46290a5dd3b4a24b3936`, 490378952 bytes extraídos.

El workflow ahora implementa una promoción explícita, deshabilitada hasta contar con el run nativo exacto y PR177 fusionada. Verifica todos los archivos, el transporte, dueño/revisión vivos y evita subir de nuevo una versión presente o en procesamiento. La única diferencia Unity tolerada respecto al paquete jugado es el blob exacto de la corrección de aserción Editor de PR177; cualquier cambio de juego, recurso o escena requiere una nueva candidata. Siete pruebas locales incluyen rechazo de cambio runtime, revisión obsoleta, certificado incompleto y artefacto incorrecto.

**Pendiente:** certificación PR177, publicación remota WebGL y APK22 firmada. No se ha probado teléfono físico ni se ha publicado este paquete.

## Prioridad temporal autorizada por el propietario

El propietario respondió «Sí autorizo» a la cesión temporal de Windows para terminar APK/WebGL sin cancelar procesos activos. Registro main actualizado: Forja conserva dueño/archivos/PR173, cede únicamente el recurso (revisión5); Valoria revisión160, ejecutor `codex-release-a56bdf7714d0`, certifica PR177. Todos los procesos nativos observados habían terminado antes de transferir. La reserva se devolverá a Forja tras la APK.

PR177 reconciliada con main; fuente `7b07fc49ae507b3686ad2acb9564e9671435c595`, misma reparación Editor y guardas adaptadas a la reserva vigente. La publicación exige certificado de esta fuente exacta. No se afirma éxito Unity antes de terminar el run.
