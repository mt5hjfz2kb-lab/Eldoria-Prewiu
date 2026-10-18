# Eldoria — actualizaciones familiares Android desde main

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
