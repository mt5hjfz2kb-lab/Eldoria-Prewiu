# Simulación independiente de acceso y oferta inicial

Ejecutar desde la raíz:

```sh
python -m unittest discover -s tools/world4x-balance -v
python tools/world4x-balance/simulate.py --seeds 24 --output docs/world4x-multiplayer/balance-results.json
```

No depende de paquetes externos. Lee OWNER_I_II de la fuente real y registra su
SHA-256. No escribe Unity, partidas, población ni geografía. Los resultados son
reproducibles; las pruebas incluyen desvío alrededor de río, corte de puentes,
stock compartido insuficiente y reasignación que evita el fallo de selección voraz.

24 semillas × 3 densidades (12/24/36 ciudades) × 2 estados de puentes × 2 radios
medidos sobre caminos × 2 políticas = 576 configuraciones, 13.824 evaluaciones.
Una cuadrícula hipotética irregular de 40×40 contiene río, cruces, obstáculos y
una zona peligrosa. BFS usa cuatro vecinos, como la ruta actual, sin atajos diagonales.
La amenaza se trata como terreno prohibido para el acceso inicial; no se finge una
simulación de combate. Cada aparición requiere recurso, enemigo accesible y espacio.

| Política | Evaluaciones | Admitidas | Rechazadas | Ruta total mediana / p95 / máxima (aristas) |
| --- | ---: | ---: | ---: | --- |
| Distribución sin garantía local | 6.912 | 2.404 | 4.508 | 44 / 80 / 96 |
| Provisión local y asignación con menor recorrido máximo | 6.912 | 6.912 | 0 | 12 / 12 / 12 |

La segunda política añade oportunidades cercanas sobre suelo seguro y sin ocupar
ciudades ni otros sitios. Mantiene los obstáculos y puentes cerrados. Es una prueba
de mecanismo, NO una instrucción para multiplicar esos nodos en Región 1 ni una
afirmación de igualdad perfecta. Una primera versión solo asignaba capacidad y
dejó un máximo de 44 aristas: la reasignación podía alejar a un jugador pese a
existir oferta local. Se corrigió buscando la menor distancia máxima que permite
asignar stock a todos. La nueva ejecución reduce ese máximo a 12. Aún deben
comprobarse agotamiento y equidad entre cohortes, no solo una oleada inicial.

Las unidades de recorrido son aristas de 8 m hipotéticos; cada caso informa el
tiempo futuro máximo bajo 4/8/16 m/s. Son escenarios, no velocidades implementadas.
El juego actual usa viaje fijo, por lo que no se presenta el resultado como latencia
real de Eldoria. Hay datos concretos de economía (600/500 objetivos; 360/700 cargas;
1250/5200 capacidad). El ledger adicional aplica costes y recompensas de la ruta
ganadora I–II, sin saldo negativo en ningún paso, y termina en 1.030 madera y
995 piedra, el mismo balance de la evidencia nativa. Las seis pruebas del simulador
incluyen esa comprobación y los pagos de combate de la fuente. No se simulan los
resultados de combate ni todas las transiciones temporales del motor.

**Desigualdades detectadas:** stock de madera comprometido para demasiadas ciudades,
recursos al otro lado de un río sin ruta, candidatos cuyo enemigo accesible requiere
atravesar peligro y mayor carga de recorridos según emplazamiento. El radio euclídeo
no detecta el corte de puentes. Contar recursos sin asignar stock tampoco detecta
la competencia de varias ciudades por la misma reserva.

**Reglas propuestas:** validar rutas por ciudad; asignar stock concurrente antes de
admitir; reparar oferta solo en suelo válido; rechazar emplazamientos no sostenibles;
comprobar rutas alternativas y expansión; repetir con agotamiento y nuevas cohortes.
No concentrar madera o piedra exclusivamente en un bioma.

**Límite explícito:** esto no certifica las posiciones reales de Región 1 ni balance
online. Falta importar el grafo físico de Unity para esa certificación y validar la
cadencia elegida. La muestra cubre configuraciones adversas reproducibles, no prueba
matemáticamente todos los mapas posibles. No hay servidores, PvP ni medición móvil.
