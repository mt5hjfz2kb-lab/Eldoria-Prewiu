# Corona: no sobreimprimir recursos futuros sobre iconos premium

Origen: nueva barra superior compacta de seis slots de navegación. En paisaje con recortes laterales de iPhone, los tres contadores de recursos pueden reducir su ancho por debajo de 117 puntos. Mantener un icono de 42 puntos y una caja de texto de 62 puntos produciría superposición cuando la economía crezca.

Implementación en el HUD: el icono pintado se dimensiona con el ancho real de cada cápsula (28-42 puntos), y el campo numérico se ajusta a la zona restante, siempre separados. CompactResource(long) utiliza K/M/B para el saldo mostrado en el HUD cuando alcance 1000 o más; la hoja Estado y todos los comandos siguen leyendo los números exactos del LocalGateway. No cambia recursos, costes, tiempos ni guardado.

NUnit PlayMode valida origen de icono, ausencia de colisión geométrica, cifras iniciales exactas y formato 1K/1M para cantidades futuras. Falta evidencia nueva Unity/WebGL en ambos formatos e inspección humana de legibilidad física.
