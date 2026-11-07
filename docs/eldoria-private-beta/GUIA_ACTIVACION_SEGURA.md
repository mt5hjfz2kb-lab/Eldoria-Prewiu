# Eldoria Beta 1 — Activación segura (no iniciada)

## Lo único que exige intervención del propietario

Cloudflare no está conectado a esta sesión: no hay perfil autenticado ni cuenta verificada. Para activar el alojamiento, el propietario debe **iniciar sesión o crear su cuenta Cloudflare en un navegador conectado de forma segura** (sin pegar contraseñas, tokens ni claves en el chat). R2 puede exigir activar facturación; antes de aceptar un coste real, consultar y aprobar sus condiciones. Hasta entonces **no hay URL de juego para testers**.

Fuentes oficiales:
- https://developers.cloudflare.com/r2/reference/wrangler-commands/
- https://developers.cloudflare.com/r2/objects/upload-objects/
- https://developers.cloudflare.com/r2/buckets/public-buckets/
- https://developers.cloudflare.com/r2/pricing/

## Acciones técnicas preparadas para ejecutar tras conectar cuenta

1. Registrar un proyecto `eldoria-beta-privada`, bucket privado R2 del mismo nombre. Deshabilitar acceso público `r2.dev` y comprobar que no haya dominio público asociado al bucket. No crear cuentas para los testers.
2. En un entorno del propietario con Wrangler autenticado, tomar **solo** el ZIP interno congelado con SHA256 `fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771`; extraer sus 19 archivos, no reconstruir Unity ni modificar el juego.
3. Ejecutar el **dry-run predeterminado** `node tools/eldoria-private-beta/cloudflare/deployment-plan.mjs WEBGL_DIR pipeline/eldoria-private-beta/r2-upload-manifest.json`; debe comprobar 19 SHA y 490317080 bytes. Si falta uno, bloqueo.
4. Solo con autorización y credenciales seguras en variables de entorno del equipo, ejecutar modo `--upload`. La herramienta falla cerrada si falta `ELDORIA_BETA_R2_UPLOAD_APPROVED=yes`, `CLOUDFLARE_API_TOKEN`, `CLOUDFLARE_ACCOUNT_ID`. Comprueba cada archivo mediante descarga R2 y SHA. Evitar publicar assets por `r2.dev`.
5. Desplegar Worker `cloudflare/worker.mjs` con su vínculo R2. Mantener `ACTIVE_RELEASE=""`, `OBSERVER_ENABLED="false"`, `QA_MODE="true"`, `CONSENTED_TESTER_IDS=""` y secretos almacenados mediante el sistema seguro de Cloudflare; no commitear `LINK_SECRET` ni `SUPABASE_SECRET_KEY`.
6. Configurar Supabase proyectado en Europa, confirmar RLS y usar clave privada **solo en Worker**. Verificar POST desde un enlace de invitación de QA (antes de testers), ID anónimo, metadata de build, fallo de red y no reenvío indebido. Mantener registros técnicos `is_qa=true` y excluidos de análisis humanos.
7. Publicar en el Worker únicamente la URL protegida por firma HMAC y allowlist. Probar en incógnito: sin link 403, link caducado 403, revocado 403, válido 302 con cookie segura, carga de todos los 19 assets (incluido Brotli/gzip), guardados persistentes y reloj real.
8. Validar en Chrome escritorio, Chrome Android físico y Safari iOS físico. Juego normal con tutorial original; no popup de tester. Comparar resultados con la WebGL original congelada.
9. Si se exige medir los 135 indicadores, construir y validar **separadamente** una variante instrumentada de Unity, sin modificar la experiencia; **no declarar que native325 ya emite los 67 eventos**. El plan exacto de hooks está en `PLAN_PUENTE_TELEMETRIA_UNITY_I_V.md` y requiere cooperación de un runner Unity disponible, nunca interrumpir Forja Oscura.
10. Solo tras verificar el estado `READY` de `verify-readiness.mjs` y la aceptación del propietario, generar enlaces personales para participantes que ya han aceptado las condiciones. Mover de `QA_MODE=true` a `false` solamente al iniciar la ronda real.

## Garantías y riesgos

- Los archivos congelados están en GitHub **Release DRAFT privado**, no en páginas públicas de GitHub. Los testers no deben entrar por GitHub ni por itch.io.
- R2 aloja bytes originales; la capa Worker controla el acceso y puede añadir telemetría **solo a la respuesta HTML**, no a la carga Unity ni a sus guardados.
- Los enlaces anónimos se pueden reenviar. Es seguridad por posesión del enlace, no verificación personal; mitigar mediante expiración, revocación y cohortes pequeñas.
- El navegador móvil puede quedarse sin memoria aunque la carga y los hashes pasen. No prometer Safari iOS sin prueba física.
- Los informes de 135 indicadores deben marcar `NO_INSTRUMENTADO` cuando todavía no exista evento Unity real; evitar ceros falsos.
- Ninguna PR o archivo del kit, por sí solo, sirve como prueba de beta alojada.

**Estado actual:** archivo congelado: SÍ; pruebas de código: SÍ; alojamiento externo conectado: NO; primera partida alojada: NO; telemetría Unity completa: NO; invitaciones: DESACTIVADAS.
