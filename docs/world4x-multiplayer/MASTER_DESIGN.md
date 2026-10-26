# Eldoria — arquitectura territorial multijugador

Propuesta de diseño, 26-10-2026. No instala servidores, genera ciudades ficticias,
modifica Región 1 ni cambia partidas. Base inspeccionada: main después de PR112.
La geografía actual se conserva. El proyecto no tiene todavía un mundo online
certificado; el experimento adjunto es una herramienta independiente.

## Decisión recomendada

Un mundo continuo compuesto por regiones geográficas conectadas, subdivididas
internamente en sectores de carga y autoridad. Las fronteras técnicas no aparecen
como cuadrículas, pantallas de carga o saltos de coordenadas. Una región no es un
monopolio de madera, piedra o enemigos iniciales. Cada grupo de posiciones de
aparición debe poder sostener simultáneamente a su población admitida.

Región 1 será el primer territorio artesanal y el banco de pruebas. No declararla
automáticamente una plantilla repetible: su población actual está calculada desde
una sola Valoria. Mantener el relieve, ríos, Brecha y recursos de partidas actuales.
Los cambios multijugador se ensayarán en un mundo y esquema de guardado nuevos.

## Geografía y aparición

Los candidatos a ciudad proceden de terreno edificable real: pendiente, huella,
accesos, riberas, fracturas, distancia entre ciudades y espacio de expansión. No
basta con estar en tierra ni con estar conectado a Valoria. Cada candidato necesita
rutas propias hacia madera, piedra, fauna adecuada y expansión, sin atravesar
amenazas superiores. Un puente único no cuenta como dos salidas independientes.

Antes de abrir plazas, asignar capacidad disponible para TODOS los candidatos del
lote. No prometer el mismo stock a varios jugadores. Si una zona no admite más
población, no colocar allí otra ciudad hasta reparar oferta o habilitar otra zona.
La admisión es por capacidad, no por llenar visualmente el mapa. Las incorporaciones
tardías deben volver a comprobar ocupación, agotamiento y control territorial.

La provisión inicial puede garantizarse mediante reservas temporales de suministro
o reposición vinculada a demanda de la cohorte. Es una propuesta futura que requiere
diseño de autoridad; no se añade ahora a LocalGateway. No ocultar una desigualdad
dando un regalo inicial que deje al jugador permanentemente aislado.

## Equidad basada en la economía existente

OWNER_I_II exige recoger 600 madera y 500 piedra. La carga actual es 360 y 700:
dos expediciones de madera y una de piedra. Para garantizar viajes completos,
reservar conservadoramente 720 madera y 700 piedra por aparición. Un nodo actual
de 1250 madera no sostiene dos reservas completas; uno de 5200 piedra sostiene
siete. Son capacidades iniciales, no una predicción de producción sostenible.

El capítulo completo añade aserradero 80 madera; Bastión II 450/300; cuartel
180/120; 20 arqueros 400/240. Hay cartera inicial y recompensas de objetivos:
no sumar costes brutos como si esas entradas no existieran. El simulador adjunto
comprueba acceso/capacidad del primer tramo y reproduce el balance económico de
la ruta ganadora I–II: termina con 1.030 madera y 995 piedra, coincidiendo con la
evidencia nativa existente. No simula resultados de combate ni toda la máquina de
estados. Antes de economía online, añadir agotamiento, reposición, colas y concurrencia.

Actualmente LocalGateway usa 2 segundos por trayecto, independientemente de la
longitud de la ruta. El ciclo de las tres expediciones suma 27 segundos de fases
de viaje/recolección, sin interacción humana. No usarlo como velocidad física.
El futuro modelo por distancia se expresa como `2*longitud/velocidad + recolección`;
el experimento barre velocidades hipotéticas 4/8/16 m/s, sin fijarlas en el juego.

Evaluar dispersión del tiempo a primer recurso, objetivo y primer ejército; reportar
peor caso y percentiles además de media. Una desigualdad equivalente a entrenar un
arquero (30 s reales actuales) es una unidad interpretable de ventaja, NO un umbral
aprobado automáticamente. Elegir la tolerancia mediante pruebas de progresión y
retención; no fabricar un «equilibrio 95%» sin significado económico.

## Recursos, fauna y competición

Madera y piedra presentes en toda zona inicial, con presentación adaptada al bioma.
Los árboles decorativos no proporcionan stock. Los nodos jugables conservan IDs,
capacidad, generación y autoridad. La oferta se calcula por rutas y jugadores activos,
incluyendo agotamiento simultáneo, no solo por distancia al nodo más cercano.

Jabalíes de nivel inicial accesibles desde cada cohorte; enemigos superiores con
rutas alternativas. La proximidad a la Brecha puede aumentar peligro, sin imponerlo
a una ciudad nueva. El catálogo actual tiene dos familias y cinco niveles: reutilizarlo,
sin crear modelos o enemigos ficticios. Corrupto y Engendro de campaña mantienen
su papel narrativo; no convertir sus recompensas únicas en un recurso compartido.

La expansión debe ofrecer rutas alternativas, información y objetivos valiosos. Las
alianzas competirían por recursos especiales y posiciones estratégicas opcionales,
no por bloquear el único aserradero o puente necesario para iniciar la partida.
No implementar torres, guerras ni ventajas de alianza en esta fase. Primero definir
qué objetivo tiene función, qué recompensa ofrece y cómo evita una ventaja permanente.

## Valoria y la Brecha

Valoria es hogar y referencia visual persistente. En multijugador distinguir la ciudad
personal y la Valoria narrativa: no duplicar la capital del lore por todo el mundo
sin resolver explícitamente esa representación. Propuesta conservadora: cada jugador
tiene su reino; la campaña de Valoria conserva su contexto narrativo y su save local.

La Brecha de Región 1 permanece donde está. Su influencia organiza exploración y
riesgo, no monopoliza recursos básicos ni exige apariciones radiales simétricas.
Otras regiones pueden relacionarse narrativamente mediante huellas de corrupción;
no se decide aquí multiplicar la Brecha ni añadir portales. Hogar, exploración,
peligro y expansión deben leerse a través de contenido que realmente existe.

## Continuidad de cámara y arquitectura móvil

Coordenadas globales estables y una identidad por entidad. Los sectores son índices
de contenido, no mapas diferentes por zoom. Cerca: modelos reales; regional:
representaciones simplificadas ancladas al mismo punto; panorama: terreno y objetivos
estratégicos existentes, sin una alfombra de nodos ordinarios. Preservar el punto
bajo el foco durante zoom y rebase de origen. Cargar sectores vecinos antes del borde
de cámara; usar histéresis para evitar cargas repetidas al oscilar el zoom.

El cliente representa una ventana del mundo; no carga todos los modelos, colliders
y GameObjects del planeta. Terreno modular, atlas e instancias reutilizables; los
presupuestos de memoria, draw calls y sectores se fijan con perfilado físico Android
e iOS. No afirmar FPS o capacidad de jugadores por estimaciones de escritorio.

Autoridad futura de servidor sobre stock, marchas, combate y reloj; comandos
idempotentes, snapshot/versionado y suscripciones espaciales. Mantener la separación
actual ICommandGateway/IWorldResourcePlacement/IWorldMarchRoutes como punto de
adaptación. LocalGateway rechaza expresamente autoridad compartida: no sortear esa
protección ni reutilizar client-side respawn como servidor. Ninguna infraestructura
online ni coste de alojamiento se contrata mediante este documento.

## Reutilización y adaptación

| Elemento actual | Decisión |
| --- | --- |
| Geografía, Brecha, modelos aprobados y fuentes Blender | Conservar; dividir carga futura sin rediseñar su aspecto |
| Cámara continua y anclajes | Conservar; extender límites y probar sectores/origen global |
| HUD V3, selección y destinos reales | Reutilizar; adaptar datos a autoridad remota |
| IDs y comandos idempotentes | Conservar como patrón; añadir identidad global y propiedad |
| Guardado local I–II | Conservar intacto; migración online explícita, no reinterpretarlo |
| Selección de sitios desde Valoria | Adaptar: evaluar desde cada ciudad y demanda concurrente |
| Viaje fijo de 2 segundos | Adaptar tras decisión de cadencia; hoy no es velocidad por distancia |
| Recursos con regeneración local | Adaptar a stock compartido y reposición autorizada |
| Decoración forestal y LOD | Depurar ahora; no convertirla en economía ni capacidad edificable |

## Secuencia de incorporación

1. Terminar correcciones visuales/funcionales actuales y mantener evidencias exactas.
2. Probar reglas offline y casos adversos con semillas, rutas y demanda simultánea.
3. Exportar la red transitable real de Unity, incluidos cruces, alturas y huellas;
   alimentar el mismo análisis sin sustituirla por una aproximación sintética.
4. Simular ciclo económico I–II completo y varias oleadas de entrada/agotamiento.
5. Solo con diseño validado, prototipo privado de autoridad remota y sectores.
6. Perfil físico móvil, continuidad de save, concurrencia y pruebas de abuso antes
   de un mundo público. No expandir terreno para esconder un fallo de distribución.

Resultados y límites del prototipo: [SIMULATION.md](SIMULATION.md).
