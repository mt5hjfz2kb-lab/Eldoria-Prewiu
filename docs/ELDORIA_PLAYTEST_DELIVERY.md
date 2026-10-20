# Eldoria — entrega independiente de Sites

## Estado actual, 19 octubre 2026

Proveedor elegido por delegación del propietario: **itch.io, proyecto Restricted**, coste 0 EUR. Automatización implementada; primera subida aceptada, pero **JUGABILIDAD EXTERNA AÚN NO VERIFICADA**. Proyecto chrismad/eldoria-pruebas creado por el propietario con Restricted seleccionado. Propietario confirmó secreto BUTLER_API_KEY guardado en entorno eldoria-playtest; credencial aceptada por Butler. El navegador cloud encontró un bucle de verificación Cloudflare y se detuvo tras un único reintento. No intentar eludirlo ni afirmar que una sesión externa se transfiere al chat.

Autoridad de entrega: `pipeline/playtest-delivery.json`. Distingue candidata native152, source y QA exactos, y publicación vigente native143 en Sites. `pipeline/development-baseline.json` conserva la instantánea fundacional del 18 octubre; no representa la última entrega. El trabajo artístico y main mantienen su propietario y reserva Windows.

## Evidencia de preparación en GitHub

Run **39310549617 COMPLETED / SUCCESS**, source **9564104118872fc78a36894153eb7e626e4c85b9**. Paquete preparado: artifact **12215243251**, `eldoria-external-playtest-39310549617`. Incluye ZIP listo para itch.io e identity.json, sin recompilación. Gobernanza de workflows y cuarentena: PASS. Esto NO implica publicación.

[Descargar preparación](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39310549617/artifacts/12215243251). Descomprimir el contenedor de Actions y subir el ZIP interior Eldoria-native152-itch.zip; no subir el contenedor exterior.

## Implementado y comprobado

- ZIP original artifact 12213501867, SHA256 da699020ad3cb3699ff08e707b2a5c9ebd01663d7f794e6c8bfe72abed78eaad, QA Windows 39305545377 SUCCESS.
- `tools/delivery/prepare.py`: verifica el ZIP exacto, procedencia y hashes Unity mediante el comprobador existente, rechaza rutas inseguras y crea ZIP HTML5 con index.html en raíz. El empaquetado inicial conservaba 18 archivos, pero era incompatible con la detección gzip de itch. La corrección `itch-complete-gzip-v1` reconstruye el archivo Unity original (mismo SHA256), elimina los nueve fragmentos de transporte y adapta el manifiesto; 10 archivos. `play.js` muestra el detalle real de fallos. No cambia el motor, escena ni datos del juego. Probado localmente sobre el artefacto real; digest incorrecto rechazado.
- `.github/workflows/eldoria-itch-delivery.yml`: preparación en Ubuntu a partir del artefacto QA; no utiliza Windows ni recompila Unity. Push en los archivos de entrega prepara el paquete. Publicación automática al cambiar promotion_request en el manifiesto de main: id explícito, destino, revisión y SHA256 deben corresponder a candidate. Compara con el manifiesto del commit anterior; cambios de documentación, workflow o estado sin nueva solicitud NO publican. Mantiene QA y hashes originales, entorno y secreto. workflow_dispatch queda como alternativa.
- No se activa la antigua ruta Pages ni Android. No se cambia el acceso del Site existente. Un cambio de dominio puede separar el guardado local del navegador; **no prometer migración automática**.

## Configuración inicial requerida (una sola vez)

1. El propietario necesita cuenta itch.io y un proyecto HTML Game con acceso **Restricted**, contraseña de visita o claves autorizadas; no Public ni solo Unlisted. Título sugerido: Eldoria — pruebas privadas. Coste cero, sin activar ventas. Si no existe cuenta, el propietario acepta sus condiciones y establece sus credenciales directamente.
2. Tras crear el proyecto, registrar su identificador real usuario/proyecto en target. No inventarlo. En la primera subida marcar el canal web como HTML5/Playable in browser y comprobar vista móvil.
3. Configurar entorno GitHub **eldoria-playtest** y su secreto **BUTLER_API_KEY**, obtenido mediante el método oficial de Butler. Nunca pegar la clave en chat, commits o logs. Se necesita autorización del propietario para esta credencial persistente; el flujo no intenta obtenerla ni instalarla por vías indirectas.
4. Solo tras verificar el proyecto restringido, establecer restricted_project_confirmed=true y publish_enabled=true. Actualizar promotion_request con id único, target, revision y artifact_sha256 coincidentes con candidate en un commit a main. El push inicia preparación y publicación sin sesión de navegador ni botón del propietario. Nunca cambiar esa solicitud por un simple informe; comprobar ejecución y logs antes de afirmar subida.
5. Verificar la publicación real: acceso restringido, revisión native152, archivos, carga e interacción/guardado. Solo entonces actualizar published y state. Un upload exitoso por sí solo no acredita hosted_verified ni prueba física móvil. Conservar Sites hasta comprobar sustituto.

## Automatización autorizada el 19 octubre, 21:54 Europe/Madrid

El propietario solicita eliminar la intervención manual recurrente. La promoción explícita registrada en main activa la subida; NO se publica cada cambio del juego. Primera solicitud: itch-native152-first-20261019. Si falla, corregir la causa y cambiar id para un reintento deliberado. Las protecciones existentes del entorno GitHub se conservan. La primera configuración HTML5 en itch puede requerir marcar el archivo como playable in browser una sola vez. No afirmar que está jugable solo por haber subido.

## Ciclo de consolidación para cada mejora

Trabajo artístico en main → compilación de un source exacto → prueba del MISMO paquete → candidate con evidencia → promoción sin recompilar → comprobación del alojamiento → published actualizado. Mantener aparte aprobación artística y física móvil. No publicar automáticamente cada commit ni reemplazar una versión estable por una candidata sin pruebas.

Para futuras candidatas actualizar los identificadores y SHA desde evidencia real; el workflow rechaza artefactos caducados o procedencia/digest incorrectos. Los paquetes Actions caducan: conservar el último paquete entregado en el alojamiento y renovar una preparación antes de caducidad si continúa pendiente.

Documentación oficial: https://itch.io/docs/creators/html5 · https://itch.io/docs/creators/access-control · https://itch.io/docs/butler/installing.html · https://itch.io/docs/butler/login.html

## Corrección de carga, 19 octubre, después de 23:10 Europe/Madrid

El propietario confirma fallo tanto en navegador integrado de ChatGPT como en Safari. La portada visible no demuestra arranque Unity. Se detectó una incompatibilidad verificable: itch detecta gzip por contenido y añade Content-Encoding; el primer fragmento Sites comienza con gzip pero termina antes del final del flujo. En prueba HTTP local con esa cabecera, el primer bloque de 20.971.520 bytes se entrega descomprimido como 24.532.611 bytes: ya no puede concatenarse con los ocho fragmentos comprimidos restantes y viola el tamaño esperado. La descompresión gzip estricta del fragmento falla por EOF.

Corrección: reconstruir 174.615.244 bytes comprimidos con SHA256 57712f5c116bd5da46388cee661e26628ffe9350b77f754855c8652d6d5b7a4f. CRC gzip completo PASS; entrega HTTP con Content-Encoding gzip produce correctamente 215.390.889 bytes. El archivo almacenado en ZIP está por debajo de 200 MB. Motor y contenido originales conservados; manifiesto de transporte y aviso de error cambiados explícitamente. Se verificaron sintaxis JS, manifiesto JSON y hashes de los demás archivos. Estas pruebas NO certifican arranque en iPhone.

Promoción correctiva: commit a1c1c55c3fb57a7f052f7b43fc2adbc1396725ef, run 39325971944. Consultar su resultado y `pipeline/playtest-delivery.json` antes de declarar publicado. No reutilizar los ZIP iniciales de 18 archivos para nuevas instalaciones.

## Reinicio físico al 71 %, 20 octubre, 00:10 Europe/Madrid

El propietario comunica reinicios repetidos al 71 %. No se ha capturado un log de iOS: presión de memoria es una hipótesis sustentada, no un diagnóstico confirmado de terminación del proceso. El cargador original estima el tamaño descomprimido como 1,6 veces Content-Length gzip: para data, 279.384.390 bytes; al completar 215.390.889 bytes ejecuta slice y crea otra copia. Su UnityCache también usa Response.clone antes de consumir el flujo, que puede retener el cuerpo adicional mientras la escritura de caché aún no ha empezado.

Adaptador itch-low-memory-v2: mantiene el flujo HTTP, pero indica al cargador el tamaño exacto calculado por descompresión validada durante el empaquetado. Solo normaliza respuestas gzip conocidas, 200 y con cuerpo. Desactiva la caché de DESCARGAS de Unity mediante cacheControl=no-store (no cambia el almacenamiento de partidas ni la caché HTTP normal). No modifica ninguno de los cuatro archivos del motor/datos; SHA256 idénticos. No baja la calidad artística ni cambia escenas.

Prueba focalizada con el lector real de native152, HTTP gzip y hashes: framework 432.603, wasm 40.040.598 y data 215.390.889 bytes, una única reserva Uint8Array por archivo, todos los hashes decodificados correctos. El mismo test se ejecuta ahora antes de publicar. Esto verifica la reducción de copias del cargador, no el pico total de RAM ni el arranque físico.

Promoción: 6a556f06cf5a49c4c57cf0c70f521c90bf3394ac. Comprobar resultado y build servido antes de pedir nueva prueba. Mantener fallo físico abierto hasta arranque real; si persiste, perfilar dispositivo/ajustar build móvil con el propietario Unity, sin apropiarse del runner Windows ni degradar arte a ciegas.

## PC privado — preparación real, 20 octubre 2026, 16:01 Europe/Madrid

Orden del propietario 15:55: configurar desde este chat. Run 39435262688, job 118670752781 SUCCESS en el PC Windows: Tailscale 1.104.1 instalado desde MSI oficial con comprobación SHA256 y Authenticode; juego native152 verificado y copia estática decodificada en C:\\ProgramData\\EldoriaPlaytest\\versions, current es junction gestionada. No Unity/Blender lanzado, sin recompilación ni modificación artística. Preparación de PC solicita un pc_request.id nuevo y candidato exacto; el cambio NO activa publicación itch. Checkout aislado eldoria-delivery-setup para conservar las copias de otros workflows.

Estado: PC_READY_OWNER_TAILSCALE_LOGIN_REQUIRED. No login, claves ni enlaces de autenticación publicados en logs; no Funnel ni puertos públicos, no Tailscale Serve activo. Falta propietario: abrir Tailscale en el PC e iniciar sesión; instalar Tailscale en iPhone con la MISMA cuenta. Después continuar configuración privada Serve/HTTPS, persistencia y prueba de carga desde el iPhone. No afirmar que existe enlace utilizable ni actualizaciones del PC totalmente conectadas antes de ese paso. Itch permanece disponible. No modificar suspensión, firewall ni perfil Tailscale ajeno.
