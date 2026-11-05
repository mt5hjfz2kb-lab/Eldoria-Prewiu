# ELDORIA — Economía V2, integración de producción y guardados

Estado: implementación en PR #141; verificar pruebas antes de fusionar.

- Canon: producción por nivel de madera (aserradero), piedra (cantera) y comida (granero), con precisión de una millonésima de recurso y liquidación persistente.
- Producción: se contabiliza el tiempo real transcurrido y al completar una construcción se divide el intervalo en los niveles anterior y nuevo. Reabrir la partida con el mismo reloj no vuelve a pagar recursos.
- Guardado: se conservan `PlayerState.SchemaVersion = 1` y `BastionThreeState.EconomyUtcTicks`. `EconomyVersion=0` indica partida anterior y `EconomyVersion=2` indica libro de producción nuevo. En la primera reapertura se liquida la deuda anterior respetando el máximo antiguo de una hora y solo entonces se cambia a V2. Nunca se liquida dos veces el mismo periodo.
- Mejoras del Bastión: niveles II=120s, III=240s, IV=420s, V=600s. Los costes, requisitos, desbloqueos y recompensas existentes se mantienen.
- VI–X: la PR incorpora sus tiempos como datos de planificación. `BastionProgressionCatalog.ForLevel(6)` continúa rechazando el nivel: ni VI–X ni XI–XXV se desbloquean.
- Equilibrio: los valores de producción V2 son provisionales; no constituyen aprobación de economía. Validar progresión de recursos, ritmo de mejoras y retención con jugadores.
- Pruebas obligatorias: fuente real de Unity, dominio en Ubuntu, persistencia serializada `JsonUtility`, avance tras desconexión, idempotencia, migración de guardados, recorridos I–V y validación Unity cuando haya runner seguro.
- Alcance: solo mecánica económica y su enlace visual de duración, sin edición artística, activación de nuevos Bastiones ni publicación.
