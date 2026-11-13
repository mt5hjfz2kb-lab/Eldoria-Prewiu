# Eldoria — decisión definitiva de entrega de testers (9 noviembre 2026)

**Decisión vinculante del propietario 7/11, reafirmada 9/11: usar GitHub Pages; no Cloudflare.**

## Experiencia prometida
- Una sola URL de Eldoria WebGL por WhatsApp, jugar directamente en Safari/Chrome/ordenador
- Cero APK, descargas obligatorias, itch.io, cuentas, correos, contraseñas o códigos
- El enlace **puede reenviarse**; GitHub Pages es público: nunca describirlo como acceso autenticado ni prometer revocación individual o 403
- El alcance jugable y medible de la primera ronda es **Bastión I–V**, no Bastión VI ni funcionalidades futuras
- Mantener congelados Unity WASM, materiales y partidas durante cada ronda, salvo fallo crítico confirmado

## Canal real y medición
- El hospedaje jugable aprobado es GitHub Pages: `https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/`
- Versión jugable conservada: `native325-mobile10-pretest-ux`, fuente `be49d62e3daf490f41dba0bf8123a8e2eee64480`. El propietario confirmó entrada y juego tras recuperar beta10; la duración de 30 minutos sigue sin acreditarse. Pruebas exactas: nativa `42595057735`, paquete WebKit `42596176998`, alojada `42600894847`.
- Cronómetro anclado: candidata aislada `/beta-iphone-clock-ab/`, fuente Unity `004d3ef9c26e9070762e9e38ce84d69d525efd4f`. Su primera entrega falló físicamente en iPhone a los 2–3 segundos. La publicación de un cargador corregido no equivale a reparar esa incidencia.
- **Supabase «Eldoria Playtest Feedback»** conectado: `public.eldoria_unity_beta_events` tiene RLS y el endpoint QA histórico `eldoria-beta-events-qa` no certifica flujo real
- Instrumentación prevista para Pages: script browser aislado, consentimiento explícito, identificadores aleatorios locales, biblioteca cerrada de eventos, cola con reintentos y deduplicación. No enviar nombres, email, URL de invitación, contenido de diálogos, chat, guardados ni `VALORIA_PLAYTEST_CHUNK`
- Transporte: HTTPS a una **Supabase Edge Function**, con clave publishable en el cliente (nunca service_role ni secret en GitHub Pages). La función valida y envía al Postgres bajo credenciales de servidor. No se necesita Cloudflare
- Un enlace público sin autenticación no identifica personas; las «sesiones» son señales pseudónimas y pueden corresponder a diferentes navegadores de la misma persona. Las respuestas manuales se interpretan por separado

## Estado y puertas de salida
- **Probado con alcance separado:** beta10 tiene recorrido nativo I–V y entrada/recarga WebKit; la candidata tiene prueba nativa de anclaje. Ningún PASS de escritorio certifica iPhone. El endpoint de estadísticas es QA y no existe prueba vigente de hitos de jugadores desde beta10.
- **En desarrollo (NO confundir con prueba real):** conexión desde el Pages que verán los testers, recepción en Supabase bajo nueva versión, consentimiento UI, hitos reales B1–B5 por canal de Unity sin datos personales
- **Pendiente de certificar:** iPhone/Android físicos con reproducción sostenida y reanudación, datos reales de hitos, QA de la página instrumento vs versión estable, licencias, revisión visual final
- `pipeline/tester-readiness/release-request.json` **debe seguir desactivado** hasta superar gates; la publicación técnica actual no es una convocatoria de testers
- Mantener intacta beta10 y la redirección vigente de `/unity-owner/` mientras se certifica el canal de medición. No sustituir el sitio principal sin rollback y WebKit del nuevo artefacto

La preparación histórica en `tools/eldoria-private-beta/cloudflare/` permanece **archivada como alternativa rechazada**; sus tests no certifican la ruta GitHub Pages + Supabase. No activar Workers/R2 ni generar gastos o cuentas.

## Continuación del 10 de noviembre: resultado verificable

La optimización del transporte está implementada en `de7095ec2b4b4553d9662a3cce67eb44822cc64e`: lectura secuencial sin buffers completos de origen/destino, entregas de hasta 64 KiB, cancelación y nombre de script con hash. CI [42787786118](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42787786118) verifica por HTTP los 31 bloques y 503.831.429 bytes; 36 pruebas locales pasan. La integridad criptográfica completa se verifica en CI, no se recalcula en el teléfono. No está publicada y no demuestra corregir el reinicio.

Las comparaciones 42786071982, 42787998561 y 42789387455 tenían un control que redirigía a Sites, frente a una candidata local: no permiten inferir causalidad. Se corrigió el control y se añadieron comprobaciones de origen e identidad tras recarga. La comparación equivalente [42789786979](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42789786979) completó entrada/recarga en los tres brazos, pero falló el umbral de memoria: beta10 2.653.776 KiB, reloj directo 3.042.464 KiB y reloj con transporte nuevo 2.931.856 KiB. Son medidas RSS de escritorio, sensibles a compresión y paginación; no miden la memoria del iPhone ni establecen por sí solas una regresión causal reproducible.

El perfil nativo [42792903970](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42792903970), artefacto 13752687573, capturó actividad de OMG/B3 y huella máxima de 3,4 GiB. Se aisló esa hipótesis con exactamente los mismos archivos en [42793879891](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42793879891), artefacto 13752933860: ambos brazos pasan entrada y recarga; desactivar OMG (opción confirmada en logs) NO reduce el pico. RSS 2.662.592 frente a 2.724.704 KiB; huella máxima 3,3 frente a 3,4 GiB. No aplicar parches de padding WASM ni atribuir el cierre al compilador.

**Estado:** el reinicio físico no se ha reproducido aquí y su causa sigue sin confirmar. No se ha accedido remotamente al iPhone. Hace falta el registro de terminación coincidente con el fallo (WebContent/Safari o JetsamEvent), junto con la URL y si se abre en Safari o dentro de otra aplicación, para distinguir cierre por memoria, fallo nativo o navegación. No basta un documento sin señal de salida.

Beta10, sus guardados y el enlace publicado siguen intactos. Invitaciones desactivadas; también pendientes certificación física sostenida, hitos reales y consentimiento en la versión final, y derechos de recursos. No afirmar “listo para testers” ni que hay trabajo ejecutándose después de cerrar esta sesión.
