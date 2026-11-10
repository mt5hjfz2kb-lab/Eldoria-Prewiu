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

## Continuación del 10 de noviembre: una prueba fallida no se promociona

La comparación `42786071982` rechazó el cargador con caché: pico WebContent de 2.542.880 KiB (control) frente a 2.930.304 KiB (candidata). Incluía trazado de cada dibujo, por lo que no permite atribuir causalmente toda la diferencia al transporte ni extrapolarla al iPhone.

PR179 `de7095ec2b4b4553d9662a3cce67eb44822cc64e` reemplaza esa reconstrucción por lectura secuencial: sin buffers completos de origen/destino, máximo 64 KiB por entrega y cancelación de lectores. SHA256 de los 31 bloques (503.831.429 bytes) comprobado por HTTP local y por CI `42787786118`; 36 pruebas locales pasan. La validación criptográfica completa se hace en CI; el navegador valida origen HTTPS, programa monótono y longitudes. El nombre del cargador incorpora su hash para evitar caché antigua (Pages indica `max-age=600`).

Prueba Mac del nuevo transporte `42787998561`, con trazado por dibujo desactivado en ambos brazos. Consultar su resultado vivo y `main/pipeline/tester-readiness/candidate.json`; no presentar este documento como certificación física. La publicación de esa candidata exige que dicha prueba pase. Beta10, guardados y convocatoria externa permanecen protegidos.
