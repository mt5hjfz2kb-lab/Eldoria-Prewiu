# Integración de las correcciones jugables de la beta 10 en `main`

## Procedencia y alcance
- PR canónica: [#179](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/179). No se ha fusionado la rama beta completa ni abierto otra PR.
- Fuente de interfaz: commit **`be49d62e3daf490f41dba0bf8123a8e2eee64480`**, comprobado en Unity con la beta 10. Se incorporan **19 archivos fuente** a su ruta canónica de Unity; los archivos de prueba `.cs.txt` de la rama de congelación siguen siendo históricos.
- En 18 archivos el original de `main` coincidía literalmente con el original congelado. El único archivo evolucionado, `EarlyRetentionRules.cs`, se reconcilia aplicando los dos desbloqueos de beta y conservando **`Math.Max(p.AldricExperience,p.LyraExperience)`** y su comentario (corrección de maestría de héroes, PR180).

## Comportamiento trasladado
Accesos contextuales Mundo/Ciudad y Correo, eliminación de controles prematuros, introducciones antes de Marchas/Vigilantes/Exploradores, fondo estable de Vigilantes, nivel de enemigos/edificios anclado a cámara, cuartel con opciones de entrenar/mejorar, mejoras reales hasta nivel del Bastión, guía de misiones y cierre con icono, temporizadores compactos sin saltos o duplicados.

## Pruebas y límites
- Origen beta: validación funcional y visual en Unity, recorrido I–V de **227 aserciones**, beta 10 WebGL probada. No equivale a certificar de nuevo el resultado combinado de `main`.
- Regresión reproducible en el código canónico: `pwsh -NoProfile -File tools/eldoria-private-beta/native/ux/test_domain.ps1 -Source .` (C# dominio, mejoras/guardados/desbloqueos y el enlace real de la interfaz). Ejecutada por el workflow `eldoria-beta10-main-domain.yml` sin ocupar Windows.
- Se preservan los trabajos independientes **PR173 (Vigilia de la Brecha)** y **PR181 (Tesoros de Valoria)**. PR181 modifica también `LocalGateway.cs` en su rama; al integrarla deberá conciliar sus cambios con esta actualización de `main`, no restaurar el archivo antiguo.
- No se sustituyen automáticamente APK/WebGL publicados ni se declaran testers habilitados por una integración de código. La comprobación de plataforma y la publicación son decisiones/procesos separados.

Estado: candidato de reconciliación desde PR179, con cierre condicionado a pruebas de la rama integrada.
