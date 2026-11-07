# Protocolo de medición de beta Bastión I–V

- **Inicio de ronda:** congelar SHA del juego, build WebGL exacta y perfil de reloj real. Separar `is_qa` de humanos y comprobar consentimiento; un enlace no prueba identidad personal.
- **P0 primero:** primer objetivo, aserradero, mundo, vuelta con recursos, cuartel, arqueros, primer Engendro, informe, granero, cacerías, Fisura 20min, Lyra, decisión IV, cantera V, 4/4. Recoger fallos e interfaz en cualquier punto.
- **Fines de sesión:** usar salida voluntaria solo cuando el jugador lo declare. No interpretar cierre de navegador como abandono: puede ser una espera programada. Ofrecer continuar en otra sesión y medir vuelta con ID seudónimo.
- **Vigilantes/Exploradores:** instrumentar apertura/desbloqueo, intentos, resultado, tropas reservadas, premios y progresión de etapas/órdenes; no exigir completar las 100 etapas posibles durante el piloto.
- **Ritmo:** 10 minutos de entrenamiento y 20 de investigación son esperas reales importantes; medir lo que hacen durante esas esperas y si vuelven, no acelerar para causar una cifra artificial.
- **Calidad de informe:** cada métrica se reporta con numerador/denominador, condición de elegibilidad, versión exacta, estado de instrumentación y número de sesiones. `NO_ALCANZADO`, `NO_INSTRUMENTADO`, `NO_CONSENTIMIENTO`, `NO_DISPONIBLE` no equivalen a cero.
- **Encuesta final (máximo 10 preguntas, 2–3 min):** claridad inicial, facilidad de controles, comprensión de recursos, marchas y combate, interés por Lyra/Fisura y Brecha, calidad visual, sensación de progreso, ganas de continuar. Incluir `No llegué a verlo`. Opcionales: sistema favorito y mayor frustración en categorías predefinidas; texto libre separado de la telemetría.
- **Privacidad:** datos técnicos seudónimos y consentimiento válido obtenido antes de emitir la invitación. Nunca nombre real/nick, correo, IP, contenido de diálogos, entradas de texto, vídeos ni capturas sin permiso específico. Revocar consentimiento vacía la cola; secretos únicamente del lado servidor.
- **Validación crítica:** test con aceptación previa verificable / denegación, 403 sin invitación, 422 con datos desconocidos, 202 + fila real en Supabase, deduplicación, fallo y reintento, guarda/restaura, FPS sin impacto apreciable, juego completo I→V en tiempo normal o sesiones continuadas. Ninguna prueba de mocks equivale a este gate.
- **No interferencia:** no tocar Unity, PR de publicación, APK/WebGL actuales; solamente preparación del canal de medición hasta que la nueva compilación sea estable.

## Decisión posterior del propietario — experiencia exactamente igual a Eldoria

El consentimiento a participar y a la recogida definida de estadísticas se solicita antes de enviar el enlace. Quien no acepte no recibe enlace de tester; se permite retirar consentimiento y revocar el acceso. El enlace abre **directamente** el juego sin registro, correo, PIN, contraseña, intro de tester o popup de consentimiento. No se añade tutorial específico ni indicaciones a las personas durante la partida. El tutorial original, mensajes de Aldric/Lyra, objetivos, reloj real, recursos, combates, guardados, desbloqueos y limitación Bastión V **permanecen idénticos**. Los observadores solo intervienen ante bloqueo técnico o seguridad y documentan cualquier ayuda. Las encuestas se recogen al final de la sesión, voluntariamente, sin tapar el juego. No grabar voz/pantalla sin autorización separada.

## Estado de instrumentación

Un nombre aceptado por Supabase no equivale a un evento emitido por Unity. Los 135 indicadores solo se publicarán cuando el generador de eventos real del juego esté verificado; hasta entonces cada uno debe aparecer como NO_INSTRUMENTADO. El alojamiento, cookie de invitación, backend privado y recepción simulada no acreditan la experiencia completa. La tabla se mantiene sin filas humanas antes de la beta.
