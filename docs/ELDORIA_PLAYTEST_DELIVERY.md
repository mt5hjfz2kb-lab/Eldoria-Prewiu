# Eldoria — entrega independiente de Sites

## Estado actual, 19 octubre 2026

Proveedor elegido por delegación del propietario: **itch.io, proyecto Restricted**, coste 0 EUR. Preparación implementada; **NO PUBLICADO**. Falta la configuración de cuenta/proyecto y credencial. El navegador cloud encontró un bucle de verificación Cloudflare y se detuvo tras un único reintento. No intentar eludirlo ni afirmar que una sesión externa se transfiere al chat.

Autoridad de entrega: `pipeline/playtest-delivery.json`. Distingue candidata native152, source y QA exactos, y publicación vigente native143 en Sites. `pipeline/development-baseline.json` conserva la instantánea fundacional del 18 octubre; no representa la última entrega. El trabajo artístico y main mantienen su propietario y reserva Windows.

## Implementado y comprobado

- ZIP original artifact 12213501867, SHA256 da699020ad3cb3699ff08e707b2a5c9ebd01663d7f794e6c8bfe72abed78eaad, QA Windows 39305545377 SUCCESS.
- `tools/delivery/prepare.py`: verifica el ZIP exacto, procedencia y hashes Unity mediante el comprobador existente, rechaza rutas inseguras y crea ZIP HTML5 con index.html en raíz. **18 archivos jugables idénticos**; no modifica WebGL, escena ni guardados. Probado localmente sobre el artefacto real; digest incorrecto rechazado.
- `.github/workflows/eldoria-itch-delivery.yml`: preparación en Ubuntu a partir del artefacto QA; no utiliza Windows ni recompila Unity. Push en los archivos de entrega prepara el paquete. Publicación solo por workflow_dispatch con publish=true, destino configurado, proyecto Restricted confirmado y secreto de entorno; permanece bloqueada por defecto.
- No se activa la antigua ruta Pages ni Android. No se cambia el acceso del Site existente. Un cambio de dominio puede separar el guardado local del navegador; **no prometer migración automática**.

## Configuración inicial requerida (una sola vez)

1. El propietario necesita cuenta itch.io y un proyecto HTML Game con acceso **Restricted**, contraseña de visita o claves autorizadas; no Public ni solo Unlisted. Título sugerido: Eldoria — pruebas privadas. Coste cero, sin activar ventas. Si no existe cuenta, el propietario acepta sus condiciones y establece sus credenciales directamente.
2. Tras crear el proyecto, registrar su identificador real usuario/proyecto en target. No inventarlo. En la primera subida marcar el canal web como HTML5/Playable in browser y comprobar vista móvil.
3. Configurar entorno GitHub **eldoria-playtest** y su secreto **BUTLER_API_KEY**, obtenido mediante el método oficial de Butler. Nunca pegar la clave en chat, commits o logs. Se necesita autorización del propietario para esta credencial persistente; el flujo no intenta obtenerla ni instalarla por vías indirectas.
4. Solo tras verificar el proyecto restringido, establecer restricted_project_confirmed=true y publish_enabled=true. Ejecutar Prepare or publish exact Eldoria playtest con publish=true. Los próximos chats con acceso a GitHub podrán usar el mismo flujo sin Sites; si no tienen dispatch, no inventar una ejecución.
5. Verificar la publicación real: acceso restringido, revisión native152, archivos, carga e interacción/guardado. Solo entonces actualizar published y state. Un upload exitoso por sí solo no acredita hosted_verified ni prueba física móvil. Conservar Sites hasta comprobar sustituto.

## Ciclo de consolidación para cada mejora

Trabajo artístico en main → compilación de un source exacto → prueba del MISMO paquete → candidate con evidencia → promoción sin recompilar → comprobación del alojamiento → published actualizado. Mantener aparte aprobación artística y física móvil. No publicar automáticamente cada commit ni reemplazar una versión estable por una candidata sin pruebas.

Para futuras candidatas actualizar los identificadores y SHA desde evidencia real; el workflow rechaza artefactos caducados o procedencia/digest incorrectos. Los paquetes Actions caducan: conservar el último paquete entregado en el alojamiento y renovar una preparación antes de caducidad si continúa pendiente.

Documentación oficial: https://itch.io/docs/creators/html5 · https://itch.io/docs/creators/access-control · https://itch.io/docs/butler/installing.html · https://itch.io/docs/butler/login.html
