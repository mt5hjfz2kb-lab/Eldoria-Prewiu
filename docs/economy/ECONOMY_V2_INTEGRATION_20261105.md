# ELDORIA — Economía V2, integración de producción y guardados

Estado: implementación en PR #141; verificar pruebas antes de fusionar.

- Canon: producción por nivel de madera (aserradero), piedra (cantera) y comida (granero), con precisión de una millonésima de recurso y liquidación persistente.
- Producción: el tiempo ausente se contabiliza con un **máximo de 48 horas acumulables por ausencia**, desde el último instante liquidado; el exceso no genera recursos atrasados. Al completar una construcción durante la ausencia, se divide el periodo entre nivel anterior y nuevo **sin reiniciar el límite de 48 horas**. La liquidación se marca hasta la hora real de regreso para evitar cobros duplicados; cuando vuelve a jugar, la producción normal prosigue. Los recursos ya almacenados, botines, recolecciones y recompensas se conservan sin esta limitación.
- Guardado: se conservan `PlayerState.SchemaVersion = 1` y `BastionThreeState.EconomyUtcTicks`. `EconomyVersion=0` indica partida anterior y `EconomyVersion=2` indica libro de producción nuevo. En la primera reapertura se liquida la deuda anterior respetando el máximo antiguo de una hora y solo entonces se cambia a V2. Nunca se liquida dos veces el mismo periodo.
- Mejoras del Bastión: niveles II=120s, III=240s, IV=420s, V=600s. Los costes, requisitos, desbloqueos y recompensas existentes se mantienen.
- VI–X: la PR incorpora sus tiempos como datos de planificación. `BastionProgressionCatalog.ForLevel(6)` continúa rechazando el nivel: ni VI–X ni XI–XXV se desbloquean.
- Equilibrio: los valores de producción V2 son provisionales; no constituyen aprobación de economía. Validar progresión de recursos, ritmo de mejoras y retención con jugadores.
- Pruebas obligatorias: fuente real de Unity, dominio en Ubuntu, persistencia serializada `JsonUtility`, 8 h / 48 h / más de 48 h de ausencia, mejora completada durante una ausencia larga, recuperación tras el regreso, idempotencia, migración de guardados, recorridos I–V y validación Unity cuando haya runner seguro.
- Alcance: solo mecánica económica y su enlace visual de duración, sin edición artística, activación de nuevos Bastiones ni publicación.
