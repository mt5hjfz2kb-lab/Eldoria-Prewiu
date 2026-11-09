# EONSCAR — prueba reversible (9 noviembre 2026)

**Estado:** diseño, código y PNG de 512×384 de prueba incluidos; NO es Unity, APK o WebGL certificado. No hacer merge automático ni publicar la marca.

## Objetivo
Bienvenida opcional con EONSCAR y botón ENTRAR EN VALORIA sin alterar HUD, guardados, escenas, sistema de tutorial, nombre interno ni package ID.
La identidad elegida combina plata envejecida, oro antiguo y energía violeta de la Brecha.

## Probar en Unity 6000.3.23f1
1. Importar el PNG original transparente en Assets/Eldoria/Branding/EonscarTrial/Resources/EonscarTrial/eonscar-logo.png. Tipo de importación: Texture2D (Default, alpha). El PNG de prueba 512×384 ya está versionado; el maestro de alta resolución sigue adjunto al chat (consulte ART_SOURCE.md).
2. Abrir la escena jugable de Valoria y pulsar Play.
3. Usar menú Eldoria > EONSCAR > Mostrar prueba sobre la partida.
4. Pulsar ENTRAR EN VALORIA, comprobar HUD/tutorial original, persistencia, acciones del juego y ausencia de clicks filtrados.
5. Salir de Play. La pantalla nunca se activa automáticamente en versiones publicadas.

## Evidencias y pendientes
- Chromium navegador: 1920×1080, 844×390 y 390×844, logo visible, sin scroll horizontal, navegar y volver sin errores: PASS.
- Unity Editor, PlayMode real, Android, WebGL, dispositivos físicos y FPS: PENDIENTE.
- El PNG transparente de prueba tiene paleta reducida; **no equivale** al maestro y necesita control artístico antes de ser definitivo.
- La disponibilidad legal definitiva de EONSCAR está pendiente.

Workstream-ID: eldoria-eonscar-branding-trial-20261109
Este trabajo NO toca los recursos reservados por PR173, PR179, PR181 o la prueba del cronómetro.

## Authoría y permisos
- Solicitud AI artística canónica: `pipeline/art-production-requests/eldoria-eonscar-branding-trial-20261109.json` en `main`; estado SOURCE QA, no aprobada para producción.
- Los controles artísticos exigen trazabilidad del PNG reducido SHA256 c5ac3f2688196548daea6c61e7d0654f3b0cff0c613c78b44a12b4d72bbeab72; véase ART_SOURCE.md.
- Workflow Unity native `.github/workflows/eonscar-branding-native.yml`: preparado, requiere lease exclusiva antes de ejecutar. Ningún cambio automático de testers.
