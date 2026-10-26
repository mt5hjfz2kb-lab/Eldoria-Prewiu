# Simulación independiente de acceso, niveles y ocupación

Actualizada con la aclaración del propietario del 26-10-2026: los recursos tienen
niveles, solo un jugador recolecta en cada nodo y el más fuerte permanece si se disputa.
Los resultados anteriores de asignación por stock quedan superados por esta versión.

Ejecutar desde la raíz:

```sh
python -m unittest discover -s tools/world4x-balance -v
python tools/world4x-balance/simulate.py --seeds 24 --output docs/world4x-multiplayer/balance-results.json
```

Solo biblioteca estándar. Lee la economía OWNER_I_II real y registra su SHA-256.
No escribe Unity, partidas, población ni geografía. Nueve pruebas comprueban
rutas/puentes, asignación alternativa, stock, economía I–II, ocupación única de una
cantera, niveles de comparación y reemplazo del recolector débil por el fuerte.

## Muestra y reglas

24 semillas × 3 densidades (12/24/36 ciudades) × 2 estados de puentes × 2 radios
medidos sobre caminos × 4 escenarios = **1.152 configuraciones y 27.648 evaluaciones**.
La cuadrícula hipotética irregular de 40×40 contiene río, cruces, obstáculos y
zona peligrosa. BFS usa cuatro vecinos y no cruza amenazas superiores para el
acceso inicial. Se evalúan también enemigo apropiado y espacio de expansión.

Los nodos de ejemplo se reparten entre niveles 1/2/3; nivel 1 es la oferta inicial
que se compara. Esto no fija el máximo del juego ni bloquea la entrada a niveles
superiores. Sus rendimientos, requisitos y cantidades futuros no están definidos:
no se inventa una tabla económica. Se conserva el stock actual 1250/5200 como
referencia del nivel inicial. **Stock para siete cargas no significa siete jugadores
simultáneos:** cada nodo dispone de una sola plaza de ocupación en la asignación.

La reparación ofrece dos opciones locales distintas de cada recurso, en suelo
válido y sin ocupar ciudades u otros sitios. El escenario disputado coloca un
rival más fuerte en la opción inicial más próxima de cada ciudad y recurso. Cada
nodo ocupado tiene un rival; no se supone que un mismo jugador tenga marchas
ilimitadas. El principiante debe encontrar otra opción, no compartir el nodo ni
expulsar mágicamente al rival. Son pruebas adversas, no una distribución del mapa activo.

| Escenario | Evaluaciones | Admitidas | Rechazadas | Ruta mediana / p95 / máxima (aristas) |
| --- | ---: | ---: | ---: | --- |
| Distribución sin garantía, nodos libres | 6.912 | 279 | 6.633 | 54 / 88 / 96 |
| Dos alternativas locales, nodos libres | 6.912 | 6.912 | 0 | 12 / 12 / 12 |
| Sin garantía, opción próxima disputada | 6.912 | 2 | 6.910 | 76 / 76 / 76 |
| Dos alternativas, opción próxima disputada | 6.912 | 6.912 | 0 | 12 / 16 / 32 |

La fila de dos admitidas es un caso extremo de oferta insuficiente; sus percentiles
no describen una población representativa. P95 usa rango más próximo. La columna
ruta se calcula únicamente entre posiciones admitidas, nunca esconde rechazos como
rutas de longitud cero. Los adversarios no ceden su ocupación en el experimento.

La reparación sigue sosteniendo las posiciones de esta muestra, pero la disputa
incrementa el peor recorrido de 12 a 32 aristas: aparecen costes de desvío reales
que el recuento de stock anterior ignoraba. No se garantiza igualdad perfecta ni
resistencia a un bloqueo ilimitado. Si un rival ocupa todas las alternativas, la
posición se rechaza para nueva admisión; no se añade protección PvP no solicitada.

## Economía, tiempos y alcance

Se necesitan 600 madera y 500 piedra; las cargas son 360/700, por lo que se reservan
conservadoramente 720/700 y se computan dos expediciones de madera y una de piedra.
El ledger de la ruta ganadora I–II aplica costes/recompensas reales, sin saldo negativo,
y termina en 1.030 madera y 995 piedra, igual que la evidencia nativa existente.
No simula resultados de combate ni toda la máquina de estados del motor.

Las aristas representan 8 m hipotéticos. Cada caso presenta sensibilidad a velocidades
futuras 4/8/16 m/s. Actualmente LocalGateway usa viaje fijo de 2 s y recolección de
5 s: tres ciclos suman 27 s de fases, sin interacción humana. Las velocidades del
experimento no se instalan en el juego y no representan latencia medida.

La función de disputa mantiene un único ocupante y utiliza solo orden de fuerza
abstracto (rangos 1 y 2), no la fórmula real de combate. En empate conserva al ocupante
como convención de simulación; no es una decisión final de gameplay. No decide bajas,
botín ya recogido, tiempos de batalla, retirada ni compensaciones al expulsado.

**Desigualdades detectadas:** plazas insuficientes pese a stock abundante; nivel
inadecuado para la comparación inicial; monopolio de la opción cercana por un rival
más fuerte; ríos sin ruta, peligro obligatorio y distinta carga de desplazamiento.

**Reglas propuestas:** validar rutas y niveles; asignar una plaza por nodo; ofrecer
alternativas comparables; revalidar ocupación antes de admitir nuevas ciudades;
medir desvíos y tiempo a objetivos; repetir con agotamiento, nuevas cohortes y
bloqueos sostenidos. Todos los nodos siguen siendo disputables.

**Límite explícito:** no certifica posiciones reales de Región 1 ni equilibrio online.
Falta importar su grafo físico y acordar economía por nivel y cadencia de combate.
No hay servidor, concurrencia online implementada ni medición móvil. La geografía
y los nodos del juego activo permanecen intactos.
