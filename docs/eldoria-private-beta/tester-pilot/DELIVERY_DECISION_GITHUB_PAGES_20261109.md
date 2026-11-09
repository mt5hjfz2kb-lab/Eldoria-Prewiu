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
- Versión actual publicada: `native325-mobile11-building-anchor`, fuente `8dc875430241a49b4d417782dfeab2d4c7a4f315`; Unity I–V y WebKit alojado certificados en los runs `42645633286`, `42645753309` y `42656897252`
- **Supabase «Eldoria Playtest Feedback»** conectado: `public.eldoria_unity_beta_events` tiene RLS y el endpoint QA histórico `eldoria-beta-events-qa` no certifica flujo real
- Instrumentación prevista para Pages: script browser aislado, consentimiento explícito, identificadores aleatorios locales, biblioteca cerrada de eventos, cola con reintentos y deduplicación. No enviar nombres, email, URL de invitación, contenido de diálogos, chat, guardados ni `VALORIA_PLAYTEST_CHUNK`
- Transporte: HTTPS a una **Supabase Edge Function**, con clave publishable en el cliente (nunca service_role ni secret en GitHub Pages). La función valida y envía al Postgres bajo credenciales de servidor. No se necesita Cloudflare
- Un enlace público sin autenticación no identifica personas; las «sesiones» son señales pseudónimas y pueden corresponder a diferentes navegadores de la misma persona. Las respuestas manuales se interpretan por separado

## Estado y puertas de salida
- **Implementado y probado:** WebGL I–V, temporizador adherido al edificio, publicación Pages, apertura WebKit/macOS, tablas Supabase, antigua librería de eventos
- **En desarrollo (NO confundir con prueba real):** conexión desde el Pages que verán los testers, recepción en Supabase bajo nueva versión, consentimiento UI, hitos reales B1–B5 por canal de Unity sin datos personales
- **Pendiente de certificar:** iPhone/Android físicos con reproducción sostenida y reanudación, datos reales de hitos, QA de la página instrumento vs versión estable, licencias, revisión visual final
- `pipeline/tester-readiness/release-request.json` **debe seguir desactivado** hasta superar gates; la publicación técnica actual no es una convocatoria de testers
- Mantener intacta la versión actual y `/unity-owner/` mientras se certifica el canal de medición. No sustituir el sitio principal sin rollback y WebKit del nuevo artefacto

La preparación histórica en `tools/eldoria-private-beta/cloudflare/` permanece **archivada como alternativa rechazada**; sus tests no certifican la ruta GitHub Pages + Supabase. No activar Workers/R2 ni generar gastos o cuentas.
