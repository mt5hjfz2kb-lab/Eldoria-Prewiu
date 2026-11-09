# EONSCAR — prueba reversible (9 noviembre 2026)

**Estado:** diseño y código de prueba aislados; NO es Unity, APK o WebGL certificado. No hacer merge automático ni publicar la marca.

## Objetivo
Bienvenida opcional con EONSCAR y botón ENTRAR EN VALORIA sin alterar HUD, guardados, escenas, sistema de tutorial, nombre interno ni package ID.
La identidad elegida combina plata envejecida, oro antiguo y energía violeta de la Brecha.

## Probar en Unity 6000.3.23f1
1. Importar el PNG original transparente en Assets/Eldoria/Branding/EonscarTrial/Resources/EonscarTrial/eonscar-logo.png. Tipo de importación: Texture2D (Default, alpha). IMPORTANTE: el PNG fuente todavía NO está versionado en la rama GitHub. Se entrega aparte en el paquete de prueba adjunto al chat.
2. Abrir la escena jugable de Valoria y pulsar Play.
3. Usar menú Eldoria > EONSCAR > Mostrar prueba sobre la partida.
4. Pulsar ENTRAR EN VALORIA, comprobar HUD/tutorial original, persistencia, acciones del juego y ausencia de clicks filtrados.
5. Salir de Play. La pantalla nunca se activa automáticamente en versiones publicadas.

## Evidencias y pendientes
- Chromium navegador: 1920×1080, 844×390 y 390×844, logo visible, sin scroll horizontal, navegar y volver sin errores: PASS.
- Unity Editor, PlayMode real, Android, WebGL, dispositivos físicos y FPS: PENDIENTE.
- El PNG transparente necesita pulir los bordes antes de considerarlo asset definitivo.
- La disponibilidad legal definitiva de EONSCAR está pendiente.

Workstream-ID: eldoria-eonscar-branding-trial-20261109
Este trabajo NO toca los recursos reservados por PR173, PR179, PR181 o la prueba del cronómetro.
