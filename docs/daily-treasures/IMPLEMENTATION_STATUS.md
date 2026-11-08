# Tesoros de Valoria — entrega de producción, PR #181
Fecha: 8 de noviembre de 2026. Alcance: sistema de recompensas y primer cofre ceremonial.

## Investigación y decisiones aplicadas
- El juego ya dispone de PlayerState, IClock (ticks UTC), LocalGateway, CompletedCommandIds y el buzón EldoriaV41Mailbox. Reutilizarlos reduce el riesgo de recompensas duplicadas y evita crear otro guardado/servicio.
- La ruta de integración vigente parte de RoyalGoldV3EarlyRetentionScreens. Vigilia de la Brecha (PR #173) mantiene otro Canvas independiente; su interfaz **no se modifica** en esta PR y exigirá una integración deliberada después de terminarse.
- La beta native325 (PR #179) permanece congelada y esta PR no cambia su WebGL, APK ni canales de test.
- El reloj actual es local y no certifica protección contra cambios fraudulentos de fecha. No afirmar autenticidad de tiempo de servidor.
- Las cantidades son candidatas para Bastión I–V; requieren balance económico antes de integrar/publicar.

## Código implementado en la rama
- Recompensas diarias por ciclo de siete reclamaciones válidas en fechas UTC diferentes: D1 60 madera, D2 40 piedra, D3 1 acelerador de 1m, D4 90 madera + 60 piedra, D5 2 aceleradores 1m, D6 100 comida + 80 piedra, D7 180 madera + 120 piedra + acelerador de 5m.
- Bonificación ligera y explícita: +25 comida al completar cada tercer día consecutivo y +80 comida en cada séptimo día consecutivo (en ese caso sustituye la de +25).
- Si falta un día la racha consecutiva vuelve a 1, pero **no** se pierden los días de recompensa acumulados del calendario.
- Una reclamación se guarda en el PlayerState; el mismo comando nunca paga dos veces, incluso tras recargar. El buzón recibe un evento de "recompensas" como recibo: leer/borrar el mensaje no aumenta recursos. Entrega y recibo son un solo Commit.
- Panel pequeño de entrada y calendario de siete premios como UI uGUI aislada, un único Canvas heredado, un único gateway. En D4 y D7 se activa cofre 2D procedural de madera/metal con apertura real de tapa, luz, brasas y sonidos sintéticos, duración ≈2,25s. Se puede saltar la ceremonia y los premios ya están guardados.
- Sin premios aleatorios, contenido ficticio, recompensas de héroes ni cartas inexistentes. Las ceremonias de logros especiales reutilizando este módulo son una segunda fase que debe validarse sin interferir con la experiencia de Vigilantes.

## Pruebas realmente realizadas
- `tools/daily-treasures/Program.cs`: 100 afirmaciones del calendario, rachas, día 7 y vuelta al primero, reloj UTC, ausencia y previews.
- `tools/daily-treasures/GatewayChecks.cs`: 27 afirmaciones del **LocalGateway real**: pagar recursos/aceleradores, idempotencia de comandos y del día, correo, borrado/lectura, recarga del mismo estado y retroceso de reloj.
- `tools/daily-treasures/check-integration.mjs`: guardas de fuente y registro, no verifica interacciones del usuario.
- **CI Ubuntu completada** sobre SHA `8a9aee03030c36d4f2c3469299b36b08ada13aee` (PR181, run 42489571353): PASS en compilación de ambas suites y validación de fuente. Verificar nuevamente el SHA final si se realizan más ediciones.
- Compatibilidad de dominio V4.1 y EarlyRetention: workflows verdes en SHA `70432b5095670a156f2226e14073fc59e4968261`. El smoke V3 falló primero por omisión del nuevo archivo en su checkout; se ha corregido la configuración pero el nuevo SHA todavía necesita comprobarse.
- Las comprobaciones de arte/governance y de propiedad canónica del workstream han pasado en commits posteriores a los cambios iniciales.
- Una prueba de Roslyn de análisis sintáctico C# acompaña al módulo. **No equivale a compilación Unity.**

## Pendientes reales
- [ ] Obtener una compilación nativa exacta con Unity 6000.3.23f1 cuando el recurso Windows esté formalmente libre; por ahora pertenece a PR173. No ejecutar job pesado ni desplazar al titular.
- [ ] Capturas reales de ciudad/panel/cofre abierto/cerrado en 1600×900, 900×1600, 844×390, 390×844, desde la misma revisión; revisar diseño, letra, solapes, tamaños táctiles y calidad artística.
- [ ] Comprobar selección/recepción, reinicio de partida, visibilidad durante otras ventanas, persistencia WebGL y partidas I–V. Probar también abandono durante la animación.
- [ ] Validar el arte y mejorar lo que no alcance la referencia Vigilia; el cofre procedural actual **no** está artísticamente aprobado ni tiene nota de calidad.
- [ ] Balance del tributo sobre progreso Bastión I–V y prueba con jugadores reales, monitorizando retención sin tratarla como resultado garantizado.
- [ ] Integración UX en la nueva Vigilia de la Brecha cuando PR173 lo permita. No duplicar dos HUDs.
- [ ] Diseñar/activar ceremonias de grandes victorias o héroes mediante solo eventos ya acreditados y sin doble premio.
- [ ] QA móviles físicos iOS/Android; no publicar ni modificar native325 hasta completar gates.

## Estado de la misión
**IMPLEMENTADO EN PR181 / TESTS DE DOMINIO REALES PASS / ART NO APROBADO / UNITY NATIVO PENDIENTE / NO FUSIONADO NI PUBLICADO.**
