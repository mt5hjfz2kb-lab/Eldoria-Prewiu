# Eldoria — actualizaciones familiares Android desde main

> **ENTREGA ACTUAL — 20-10-2026.** Primera instalación: [APK Android 1.8 code8](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code8/Eldoria-Valoria-code8.apk), release exacta y pública verificada byte a byte en [run 39495008956](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39495008956) (sha256 7570fb7fc08d4a665e264af51b0968cac2bc79bf783e256a1e367c935964b8e6). Android aapt confirma ARM64, paquete/code8, INTERNET, ACCESS_NETWORK_STATE y REQUEST_INSTALL_PACKAGES. [Actualización Android 1.9 code9](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code9/Eldoria-Valoria-code9.apk) publicada y verificada byte a byte en [run 39494077995](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39494077995) (sha256 13f41bd9c93cea07dd36e82322df501e66fa7041afc30b4ee5d8644968c3b2fe). Canal enabled:true/code9. El propietario debe instalar 1.8 UNA VEZ sin borrar la app y comprobar desde su Android real que al abrir con Wi-Fi se descarga 1.9, aparece Instalar, Android pide confirmación y conserva partida tras actualizar. PHONE_PASS **PENDIENTE**. Las Releases de GitHub son públicas y ambos APK son cortes de main, no la última versión gráfica de cada hora.

## Contrato

La escena jugable única es Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity y la rama fuente es main. No volver a usar ValoriaContentUpdate.Channel, que apunta a construction/valoria-master-plan-layers-20261016, ni incluir Valoria_Content_Bootstrap en este APK integrado. El viejo sistema de bundles era de la escena aislada y permanece deshabilitado.

**Alcance implementado:** la APK nueva detecta en segundo plano actualizaciones completas, publicadas y verificadas al iniciar y tras reabrir la aplicación, muestra Actualizar / Ahora no, usa Android DownloadManager para descargar, comprueba tamaño y SHA-256, y abre el instalador nativo con URI de contenido. Android siempre requiere confirmar la instalación; puede exigir autorización de instalar aplicaciones desconocidas la primera vez. No permite instalación silenciosa ni sobrescribe partidas.

**Alcance pendiente:** actualizaciones diferenciales de assets / AssetBundles para la escena integrada. No basta con activar content_updates=true: el constructor anterior aborta explícitamente para proteger el juego. Hasta migrar/probar esa capa, cada cambio del juego entregado a Android requiere una APK completa y firmada. Las revisiones de código no se descargan como scripts remotos.

## Canal oficial y seguridad

https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/main/updates/android/family.json

El archivo inicial tiene enabled=false para impedir avisos engañosos mientras no haya una APK nueva verificada. Solo una promoción explícita tras build y comprobación de bytes lo pone en true.

La APK solo acepta:
- channel=family, packageName=com.eldoria.valoria.v5test, versionCode mayor que el instalado.
- Certificado SHA-256 idéntico al candidato original.
- sourceSha de 40 hex, SHA-256 del APK de 64 hex, tamaño máximo de 450 MiB.
- URL de Release exacta HTTPS bajo github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-codeN/Eldoria-Valoria-codeN.apk.
- SHA-256 de los bytes completos antes de abrir el instalador; Android comprueba además la firma al instalar.

No se introducen claves privadas en el repositorio. GitHub es público y las APK publicadas como Releases también lo son: utilizar solo builds autorizados para pruebas, sin secretos ni información privada.

## Entrega y actualización

1. Una sola vez, la persona probadora instala encima de la APK 1.6 la primera APK con actualizador integrado (versionCode 7 o superior). La versión 1.6 no conoce el nuevo canal y nunca podrá autoactualizarse a 7.
2. pipeline/android-updater-build-request.json: cambiar enabled=true y versionCode monotónico después de comprobar que el runner está libre. El workflow eldoria-family-android-build.yml compila la escena integrada, preserva firma y entrega android-build-identity.json. No se ejecuta con enabled=false.
3. Revisar el artefacto, logs y QA nativa. El workflow NO publica APK ni cambia el canal por un simple build satisfactorio.
4. pipeline/android-updater-release-request.json: aprobar explícitamente enabled=true, buildRunId, versionCode y expectedSourceSha del run validado. eldoria-family-android-publish.yml coteja identidad y bytes, publica Release inmutable, comprueba el APK público y entonces avanza el canal de main.
5. En futuras versiones, el móvil detectará la promoción al abrir Eldoria y ofrecerá actualizar sin buscar enlaces. Android fuera de Play no permite instalación silenciosa. No publicar cada commit: únicamente builds verificados.

## Pruebas obligatorias

- node --test tools/android-updates/check-channel.test.mjs
- node tools/android-updates/check-channel.mjs updates/android/family.json
- Unity 6000.3.23f1: compilación Android ARM64 de la escena integrada, paquete/versionCode, permiso REQUEST_INSTALL_PACKAGES, certificado original, SHA/bytes del artefacto.
- APK exacta en teléfono: actualización sobre APK anterior sin desinstalar, carga y persistencia Bastión I–II, reinicio, estado offline, manifest alterado y descarga corrupta; ninguna ruta borra datos.
- Instalación de código 7 una vez manual; código 8 como prueba real de actualización ofrecida en el propio juego.

## Límites

PASS de CI no equivale a instalación física real, rendimiento Android ni calidad visual. El runtime no borra partidas ni recursos fuera de su carpeta de APK. No reutilizar las URLs ni las escenas del canal histórico.

## 20 de octubre — descarga automática de APK aprobadas (versión de prueba)

La aplicación Android con el nuevo actualizador comprueba al abrir la versión publicada y validada en el canal familiar de main. Si es superior a la instalada:

- Con Wi-Fi/red sin coste medido, descarga automáticamente en segundo plano mientras el juego continúa; se utiliza DownloadManager del sistema y el APK no se carga entero en memoria.
- Sin Wi-Fi, espera sin gastar datos móviles automáticamente. La interfaz permite pulsar «Usar datos móviles» de manera explícita.
- Conserva el identificador de descarga en un pequeño archivo download-id en su propia carpeta de APK. Al cerrar/reabrir, reconecta con la descarga ya solicitada y evita duplicarla.
- Cuando termina, verifica tamaño y SHA-256 fuera del hilo gráfico. Después muestra «Instalar / Ahora no». Nunca abre el instalador sin pulsar el botón. Android comprueba además el certificado y exige confirmación.
- Si falla, mantiene la aplicación y el progreso intactos. No borra partidas ni datos generales; solo elimina su APK local y la descarga fallida.
- El canal no se habilita por cambios de GitHub; solo después de compilar una APK firmada y verificar Release y bytes públicos. Se excluyen las ramas experimentales.

La APK anterior no puede adquirir estas capacidades sin una instalación inicial de la nueva APK. La compilación 1.7 (code7) del 19/10 es histórica y no contiene descarga automática: la primera candidata debe usar versionCode 8 / 1.8 o superior. Las mejoras de Unity pendientes no llegan a Android hasta que su APK sea compilada, validada y publicada.

Todavía se requieren: prueba de compilación Unity ARM64, certificado apksigner, permiso Android, instalación física sobre la anterior sin perder guardados y después prueba 1.8→1.9 con Wi-Fi, datos móviles y sin conexión. Ningún CI sustituye PHONE PASS.

## 2026-10-20 — entrega Android familia: compilación y publicación verificadas

- **Code8 / 1.8:** primer APK con detección y descarga Wi-Fi automática, build `39481727537`, source `1e7c1858a6d6d2a3b613da8763a34965b85c55f7`, 227769276 bytes, SHA-256 `7570fb7fc08d4a665e264af51b0968cac2bc79bf783e256a1e367c935964b8e6`. Firmado con el mismo certificado que el APK anterior; instalar manualmente una sola vez. Disponible en artifact `12291789342` (archivo comprimido de Actions) y como APK extraído en la conversación de entrega.
- **Code9 / 1.9:** segunda APK para actualizar desde 1.8, build `39490305176`, source `575933732d3e96a934695d35abe5cbb502574762`, 228701167 bytes, SHA-256 `13f41bd9c93cea07dd36e82322df501e66fa7041afc30b4ee5d8644968c3b2fe`. Compilación/firma/manifest PASS. El artefacto de build incluía además un APK code8 antiguo debido al workspace reutilizado; **no fue publicado**. Corregido el empaquetado para próximos builds.
- **Publicación OTA code9:** GitHub Release `eldoria-android-code9`, creado con la referencia de metadatos en main y la identidad del source exacto preservada dentro del manifiesto. Publicación y comparación de bytes en ejecución `39494077995` SUCCESS. HTTPS: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code9/Eldoria-Valoria-code9.apk.
- **Canal:** `updates/android/family.json` está `enabled:true`, `versionCode:9` y fija firma, URL, tamaño, SHA del contenido y código fuente. Ya no depende de ninguna rama antigua. `pipeline/android-updater-build-request.json` queda disabled para no compilar duplicados.
- **Siguiente versión:** seleccionar commit estable de main, fijar código Android **10 o posterior**, compilar y revisar, aprobar Release con source+build run exactos. El código instalado8/9 descargará la nueva versión al abrir con Wi-Fi sin buscar enlaces; Android continúa exigiendo confirmación de instalación.

**No afirmar PHONE_PASS todavía:** faltan instalación sobre APK existente, conservación del progreso y prueba física 1.8→1.9, incluyendo Wi-Fi, descarga interrumpida, confirmación Android y recarga. El servidor de APK está en un repositorio público, así que es distribución de pruebas, no privada.


## 2026-10-20 — Android update prompt: visual accessibility hotfix, code10

Owner physical Android observation: the update text appears over world cards with translucent backgrounds. Cause in code8/code9: runtime OnGUI relied on the default Unity GUI.Box skin, which has no guaranteed opaque background. The fix is local to EldoriaAndroidAutoUpdater.cs: dim the visible world, render an **opaque dark card** with gold frame, readable white text, opaque buttons, responsive portrait/landscape layout, and restore GUI color/matrix/depth. The download policy, signing, local saves and Android install confirmation remain unchanged.

Code10/1.10 is requested from the current integrated main; do **not** announce this UI improvement on the physical phone until signed APK build and test are complete. Keep the stable public update channel code9 until the independently verified code10 APK is promoted. Full device readability and tap behavior still need physical QA.

## 2026-10-20 — code10 / 1.10: corrección visual Android PUBLICADA

**Estado técnico FINALIZADO.** El propietario observó en su teléfono un cuadro semitransparente mezclado con el HUD del mundo. Se sustituyó el predeterminado GUI.Box de Unity por un panel **opaco oscuro con marco dorado, textos blancos de alto contraste, botón principal y «Ahora no»**, reescalado para vertical y horizontal. El fondo del juego se atenúa durante el aviso; la descarga automática por Wi-Fi, el consentimiento de instalación del sistema Android y la persistencia de guardados no cambian.

- Código y pruebas: commit 3a1cc3f17dc7adc543126c46ec53f402bb39aa94; **9/9 pruebas de política y UI PASS**. Unity editor source gate 39502034766 SUCCESS.
- **APK compilada y firmada:** code10 / versionName 1.10, build 39502034675, SUCCESS (verificó paquete original, clave/certificado, versionCode, ARM64 y REQUEST_INSTALL_PACKAGES); artifact eldoria-family-updater-code10-39502034675.
- **Publicación:** run 39504548347 COMPLETED SUCCESS, validó el SHA original, creó Release code10, descargó el APK público y comparó bytes exactos (ELDORIA_FAMILY_RELEASE_BYTE_MATCH_PASS). Manifest main canal familia actualizado y habilitado para versionCode 10, **247491508 bytes**, SHA-256 **24299a64a3dd8a205b2058df42cc7bf6191609ac2fe607ba5a0da73a52ec9511**, source exacto 3a1cc3f17dc7adc543126c46ec53f402bb39aa94.
- Descarga Android: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code10/Eldoria-Valoria-code10.apk
- Se deshabilitó la petición de publicación tras éxito; no hay duplicados ni recompilaciones en bucle.

**Límite: aceptación móvil PENDIENTE.** El aviso que ya está compilado en las APK 1.8/1.9 seguirá siendo semitransparente durante el paso a 1.10. La nueva ventana solo existe tras instalar 1.10. El propietario puede instalarla encima del paquete existente desde el canal o enlace, **sin desinstalar**. Para certificar el aspecto de la nueva ventana en el móvil es necesario que haya una futura actualización posterior a code10 (o prueba instrumentada en Android real) y comprobar la captura, los botones y la continuidad del guardado. No declarar PHONE_PASS ni afirmar que la UI física ya se ha visto.

## 2026-10-21 — Publicación nocturna automática en GitHub

**AUTOMATIZACIÓN CONFIGURADA (pendiente de primera ejecución programada):** el workflow .github/workflows/eldoria-family-android-nightly.yml corre una vez cada noche con cron UTC 17 2 * * *. Equivale aproximadamente a **04:17 en horario de verano / 03:17 en horario de invierno en Madrid**; GitHub puede demorar su inicio. No usa servicios pagados y reutiliza el mismo compilador Android firmado en Windows que ya produjo code8, code9 y code10.

**Solo se prepara UNA APK si se cumplen todas las condiciones:**

1. Canal familiar publicado válido y firmado; peticiones manuales Android desactivadas; propietario canónico de main sin reserva de otro trabajo.
2. El árbol completo Unity/ ha cambiado desde el APK publicado. Un cambio exclusivo de documentos, workflows o registros no provoca una nueva APK.
3. Al menos **45 minutos** sin cambios en Unity/, sin trabajo nativo incompatible ya en ejecución.
4. Unity cuenta con un trabajo Valoria nativo o prueba completa del editor Unity **SUCCESS para el mismo árbol Unity/**. Un preflight de código sin compilación/jugabilidad real no basta. Si falta QA, se conserva la APK anterior.
5. versionCode aumenta exactamente **+1** respecto al canal vigente; build ARM64, APK firmada con la clave original y verificaciones de integridad/permisos/bytes pasan. Después se publica una Release inmutable y se comparan de nuevo sus bytes HTTPS.
6. El canal updates/android/family.json avanza solo después de publicar; hay control de conflictos sobre main, sin force-push. En Android la descarga es automática por Wi-Fi y la instalación requiere confirmación; no se borra deliberadamente la partida.

**Gestión:** ejecutar manualmente el workflow con dry_run=true (opción por defecto) comprueba los gates sin compilar ni publicar. La variable del repositorio ELDORIA_NIGHTLY_ANDROID_ENABLED=false permite pausar futuras noches, pero no hace falta configurarla para activar el plan. Los logs especifican cuándo se salta la noche por falta de cambios, QA o recurso.

**Límites:** no publica cada commit ni certifica calidad gráfica absoluta. No sustituye el test de un Android físico. Las APK completas pesan cientos de MB. Si el PC de Unity no está operativo, la compilación no finalizará. Las Releases son públicas y destinadas a pruebas, no a distribución comercial.
