# Eldoria — Mapa Maestro de Producto

Estado: preproducción para futura migración a Unity
Fecha de inicio: 2026-09-24
Última consolidación: 2026-09-24

Este documento consolida la dirección macro de Eldoria antes de la migración a Unity. No sustituye `PROJECT_STATE.md` (estado funcional) ni `AGENTS.md` (protocolo). Su función es separar decisiones cerradas, hipótesis de diseño y decisiones todavía abiertas, evitando que ideas futuras condicionen la arquitectura de forma accidental.

## 1. Norte de producto

Eldoria es un 4X/RPG de fantasía medieval oscura, mobile-first, cuyo bucle protegido es:

**Valoria → necesidad/decisión → mundo → recolectar/luchar/descubrir → recompensa valiosa → regreso → crecimiento visible → nueva ambición.**

El prólogo Bastión I–X debe enseñar este bucle. El producto a largo plazo debe ampliarlo desde la progresión individual hacia un mundo compartido sin convertir el juego en una colección inconexa de sistemas.

## 2. Estado de decisiones

### CERRADO / CANÓNICO
- Valoria/Reino como hogar y centro de crecimiento del jugador.
- Mundo como espacio de exploración, recursos, amenazas, expediciones y descubrimiento.
- Bastión I–X como prólogo/tutorial completo.
- Poder Total separado de Poder de Expedición.
- Héroes, tropas, Marcha, Forja/equipo y PvE por capas como sistemas del núcleo.
- Códice = conocimiento y descubrimiento del mundo.
- Relicario = cartas/Reliquias, colección, uso/conservación, Práctica y futuro Duelo.
- La Brecha es la amenaza/misterio transversal y debe tener consecuencias jugables, no ser solo lore.
- No fingir multijugador: cualquier simulación previa debe identificarse como tal.
- El mundo/servidor es persistente, indefinido y no se fundamenta en reinicios obligatorios.
- Se abrirán nuevos servidores periódicamente para ofrecer mundos jóvenes a nuevos jugadores.
- Todos los servidores comparten una cronología estructural base según su edad; su estado concreto puede divergir por La Brecha y las acciones de sus jugadores.
- Las Alianzas tienen influencia territorial real y pueden entrar en conflicto abierto.
- La Brecha es un sistema global dinámico capaz de transformar el mundo compartido.
- Eldoria no tiene un final definitivo: el mundo continúa expandiéndose mediante nuevas capas, regiones, amenazas y sistemas.
- El Puerto/Muelle será un sistema avanzado posterior al prólogo: inicialmente PvE/económico mediante expediciones abstractas y progresión de flota.
- Eldoria tendrá un sistema modular y ampliable de eventos temporales PvE/PvP, individuales y colectivos.
- Tras el prólogo, Valoria evoluciona de asentamiento reconstruido a comunidad civil: llegan habitantes y las viviendas pasan a formar parte visible del crecimiento del reino.
- El Consejo de Valoria nace como consecuencia narrativa de esa madurez social y, a la vez, funciona como sistema diegético de investigación/progresión PvE.
- Los consejeros principales son personajes canónicos que se incorporan progresivamente; las tres ramas fundacionales son Recursos, Construcción y Militar.

### DEFINIR ANTES DE UNITY
- Reglas macro definitivas de PvP, guerra y protección del jugador.
- Reglas exactas de conquista, pérdida y recuperación territorial.
- Ciclo detallado de La Brecha y sus estados de madurez.
- Balance y reglas concretas del Oráculo.
- Gobierno del Mundo: legitimidad, acceso, duración, poderes y destitución.

### DISEÑO MACRO, IMPLEMENTACIÓN POSTERIOR
- Puerto/Muelle, barcos y expediciones.
- Posible navegación/mapa marítimo real en una fase futura.
- Conflicto territorial avanzado.
- Diplomacia formal entre Alianzas.
- Sistema de eventos recurrentes.
- Endgame entendido como expansión continua, no como final cerrado.
- Expansiones/regiones posteriores.

## 3. Bloque estructural 01 — Mundo compartido + Brecha + Alianzas

### 3.1 Mundo compartido — dirección cerrada

El mundo compartido amplía el bucle individual, no lo sustituye. Cada jugador conserva Valoria como hogar propio y proyecta su poder al mapa mediante marchas, recolección, PvE, cooperación y conflicto.

Principios:
1. **Persistencia:** el servidor es un mundo persistente, indefinido y evolutivo. Valoria, héroes, colección, Alianza y progreso sobreviven al paso del tiempo.
2. **Sin reinicio fundacional:** los resets obligatorios no forman parte del modelo base. En el futuro pueden explorarse modos especiales o estacionales sin condicionar la arquitectura central.
3. **Nuevos servidores:** se abrirán servidores nuevos periódicamente para que los nuevos jugadores puedan comenzar en mundos jóvenes sin obligar a reiniciar los existentes.
4. **Cronología común por edad:** los desbloqueos estructurales principales siguen una cronología equivalente según la edad de cada servidor. Si un hito base corresponde, por ejemplo, al día 30, un servidor posterior alcanza ese mismo hito al cumplir su propio día 30.
5. **Divergencia del estado del mundo:** compartir cronología no implica mundos idénticos. La Brecha y las acciones colectivas pueden alterar regiones, rutas, recursos, amenazas y otros estados persistentes de cada servidor.
6. **Evolución continua:** nuevas regiones, niveles de Bastión, amenazas, héroes, tropas, edificios, eventos y sistemas pueden abrirse con el tiempo.
7. **Edades sin borrado:** grandes etapas o capítulos pueden transformar el mundo sin eliminar el progreso del jugador.
8. **Regiones:** el mundo se organiza en regiones con identidad, recursos, amenazas y objetivos propios.
9. **Escala gradual:** el jugador empieza localmente; el prólogo revela que existen otros reinos y un conflicto mayor.
10. **Actividad útil:** viajar por el mapa responde a una intención: recurso, amenaza, descubrimiento, alianza, territorio o evento.
11. **Sin destrucción irreversible temprana:** el PvP no debe convertir meses de progreso en una pérdida definitiva.
12. **Sin final definitivo:** un veterano no “completa Eldoria”; accede a nuevas capas de un mundo que continúa creciendo.

Estado: **DIRECCIÓN CERRADA; CADENCIA Y REGLAS FINAS DE EVOLUCIÓN POR DEFINIR**.

### 3.2 La Brecha — dirección cerrada

La Brecha es uno de los motores narrativos y jugables principales de Eldoria y funciona en tres escalas conectadas:
- **Personal/narrativa:** origen del misterio de Valoria, corrupción, materiales, enemigos y Reliquias.
- **Regional:** fisuras, zonas corrompidas, enemigos/eventos y transformaciones del mapa.
- **Global/servidor:** presión compartida que evoluciona con el tiempo y con la respuesta colectiva de jugadores y Alianzas.

No es una simple barra global ni una categoría de enemigo. Puede corromper regiones, modificar terreno, recursos y rutas, abrir o cerrar zonas, alterar criaturas, generar fisuras, jefes y eventos, y amenazar territorio de Alianzas. Debe poder crear situaciones en las que enemigos políticos tengan razones para cooperar temporalmente.

#### Madurez del Mundo
Principio cerrado: **las consecuencias de fallar ante La Brecha escalan con la madurez del servidor**.

En las primeras etapas, el fracaso debe enseñar y advertir. Conforme el mundo madura, los fallos pueden producir efectos más profundos y persistentes: corrupción alrededor de instalaciones, enemigos más peligrosos, alteración de rutas o recursos y contribución al estado global de La Brecha.

La Madurez del Mundo convive con la cronología común de servidores: la columna vertebral temporal es compartida, pero el estado concreto de cada mundo puede divergir.

Estado: **DIRECCIÓN CERRADA; CICLO, FASES, RECOMPENSAS Y RECUPERACIÓN POR DEFINIR**.

### 3.3 Alianzas — dirección cerrada

Las Alianzas son una capa estructural de largo plazo. Deben permitir hacer cosas que un jugador solo no puede hacer con la misma eficacia y crear historia emergente del servidor.

#### Núcleo físico
- Cada Alianza puede construir un **Cuartel General/Fortaleza de Alianza** físico en el mundo. Nombre final pendiente.
- No es decorativo: es centro de progreso colectivo y pieza estratégica.
- Para iniciar su construcción se exige un mínimo de miembros.
- La construcción requiere participación real mediante marchas enviadas por miembros y puede continuar mientras estos están desconectados.
- Si no se mantiene la participación mínima requerida, la construcción se detiene. Valores exactos pendientes de balance.
- El Cuartel General puede ser atacado. La severidad máxima de su caída queda abierta.

#### Territorio
- La Alianza ejerce **control territorial real**, con consecuencias económicas, militares, PvE y de La Brecha.
- La expansión parte del Cuartel General y debe ser continua.
- La frontera crece mediante puestos/estructuras de Alianza conectados construidos colectivamente.
- Estos puestos pueden ser objetivos estratégicos en guerra.
- El territorio puede contener recursos, rutas, instalaciones, Oráculos y otros objetivos con valor real.
- Queda abierta la regla para territorio desconectado por guerra o transformaciones de La Brecha.

#### Progresión
- Las Alianzas tienen progresión colectiva propia.
- Existe un tronco común de desarrollo y especializaciones limitadas que aportan identidad sin crear clases rígidas.
- Posibles ámbitos: militar, economía/construcción, territorio y Brecha.
- La capacidad máxima de miembros crece con la progresión de la Alianza hasta un límite final por balance.

#### Jerarquía y liderazgo
Jerarquía conceptual inicial: **Líder → Oficiales → Veteranos → Miembros**.
- El Líder tiene autoridad ordinaria máxima.
- Los Oficiales comparten por ahora los mismos permisos de gestión ordinaria.
- Solo el Líder puede nombrar o retirar Oficiales.
- Existe un mecanismo extraordinario de destitución difícil de activar, con supermayoría muy alta y resolución de sucesión.

#### Entrada, salida y geografía del jugador
- Abandonar o ser expulsado de una Alianza no teletransporta Valoria.
- Entrar en una nueva Alianza tampoco mueve Valoria automáticamente.
- Reagruparse requiere una decisión voluntaria de traslado.
- Se prevén objetos/mecánicas de reubicación, evitando convertir territorio/guerra en pay-to-win.

#### Diplomacia
- La diplomacia formal entre Alianzas no es requisito de lanzamiento.
- Desde el inicio puede existir diplomacia emergente entre jugadores.
- Más adelante pueden añadirse pactos, aliados formales, enemigos declarados, tratados o coaliciones temporales contra La Brecha.

Estado: **MARCO ESTRUCTURAL CERRADO; BALANCE Y REGLAS DE DETALLE ABIERTOS**.

## 4. Bloque estructural 02 — Oráculos

### 4.1 Identidad
Los antiguos **Oráculos del mundo han sido corrompidos por La Brecha**. Son instalaciones físicas disputadas por Alianzas y conectan PvP territorial, cooperación interna y PvE colectivo. Habrá múltiples Oráculos, no uno único.

### 4.2 Ciclo conceptual
Ciclo provisional:

**Conquista → Dominio → Ofrenda → Manifestación → combate colectivo → recompensa.**

- **Conquista:** actividad recurrente PvP entre Alianzas por el control.
- **Dominio:** la vencedora obtiene una bonificación persistente durante el periodo de control.
- **Ofrenda:** los miembros aportan recursos/objetos.
- **Manifestación:** las contribuciones pueden provocar un jefe relacionado con La Brecha.
- **Combate colectivo:** la Alianza combate conjuntamente.
- **Recompensa:** colectiva e individual, por definir.

La conquista debe valorar el **tiempo de control efectivo durante una ventana**, no únicamente el último golpe. La referencia de actividad semanal de aproximadamente 1–1,5 horas sigue siendo provisional.

### 4.3 Fracaso y Madurez del Mundo
En servidores jóvenes, fracasar ante la Manifestación/Jefe debe tener consecuencias leves o narrativas. Conforme el mundo madura, el fracaso puede fortalecer La Brecha y producir consecuencias regionales o globales más tangibles.

Estado: **CONCEPTO CERRADO; NÚMEROS, CALENDARIO, PARTICIPACIÓN, JEFE Y RECOMPENSAS ABIERTOS**.

## 5. Bloque estructural 03 — Gobierno

Eldoria contempla dos sistemas distintos que no deben confundirse.

### 5.1 Consejo de Valoria — Gobierno interno PvE

El Consejo de Valoria nace de la propia reconstrucción del reino. Tras Bastión IX–X comienza a hacerse visible una nueva etapa: Valoria deja de ser únicamente un núcleo militar y de supervivientes, empiezan a llegar habitantes y la construcción de viviendas introduce una población civil estable.

La referencia provisional para el nacimiento formal del Consejo es **Bastión XIV–XV**, cuando esa población ya se ha asentado y la comunidad reclama representación en la toma de decisiones. El número exacto de Bastión no queda bloqueado todavía; importa la secuencia narrativa: **reconstrucción → llegada de habitantes → viviendas/comunidad → representación → Consejo**.

El Consejo cumple dos funciones inseparables:
- **Narrativa/simbólica:** representa que Valoria vuelve a ser una sociedad y no solo un asentamiento en reconstrucción.
- **Mecánica:** sustituye el árbol tecnológico abstracto por una progresión PvE diegética basada en consejeros y sus ramas de investigación.

#### Consejeros
- Los consejeros principales son **personajes canónicos**, no cargos elegibles entre múltiples candidatos.
- Cada consejero tiene identidad, historia y un ámbito propio de responsabilidad.
- No son héroes de combate ni forman parte de la colección de héroes; pertenecen a la vida civil e institucional de Valoria.
- Se incorporan **progresivamente**, con poca separación entre las primeras incorporaciones, para que cada llegada tenga peso narrativo y enseñe su rama sin presentar todo el sistema de golpe.
- La necesidad narrativa del reino debe justificar la llegada de nuevas ramas futuras.

#### Ramas fundacionales
Las tres áreas imprescindibles identificadas para el primer Consejo son:
1. **Recursos:** producción, recolección, almacenamiento y eficiencia económica.
2. **Construcción:** desarrollo urbano, capacidad de crecimiento y evolución física de Valoria.
3. **Militar:** entrenamiento, ejército y desarrollo de capacidades militares.

No se fija todavía cuál de las tres aparece primero. Tampoco se añaden ramas por completar artificialmente el sistema: comercio, conocimiento/Brecha, exploración u otras podrán incorporarse cuando la evolución del juego las justifique.

#### Modelo de progresión
- Dirección inicial: **lineal y acumulativa**. El jugador puede avanzar sin miedo a una elección irreversible temprana.
- La arquitectura debe permitir introducir en el futuro bifurcaciones o especializaciones si el sistema demuestra necesitarlas.
- Talentos concretos, costes, tiempos, requisitos y balance quedan deliberadamente fuera de esta fase de preproducción.

#### Alcance político inicial
En el primer corte, el Consejo **no es un simulador político interno**. Los consejeros no necesitan todavía discrepar, competir por influencia ni plantear decisiones políticas sistémicas. Esa posibilidad queda abierta para una evolución futura, pero no condiciona el diseño inicial.

Estado: **DIRECCIÓN MACRO CERRADA; PERSONAJES, ORDEN EXACTO, ÁRBOLES Y BALANCE POR DISEÑAR MÁS ADELANTE**.

### 5.2 Gobierno del Mundo — autoridad política del servidor
El servidor tendrá una **autoridad política real ejercida por un jugador**. Nombre provisional: **Soberano del Mundo**.

Principios:
- no debe ser simplemente el jugador número 1 de un ranking;
- debe poseer poderes y responsabilidades reales;
- puede existir una corte/consejo con cargos ocupados por otros jugadores;
- puede afectar políticas/beneficios del servidor y asumir responsabilidades relacionadas con La Brecha;
- el acceso al cargo puede combinar poder militar, territorio y legitimidad política.

Estado: **DIRECCIÓN CERRADA; MECÁNICA POLÍTICA ABIERTA**.

## 6. Bloque estructural 04 — PvP, guerra y protección

### 6.1 PvP abierto
Dirección cerrada: **el PvP abierto y las guerras entre Alianzas están permitidos**. La política del servidor debe emerger de sus jugadores.

Los ataques directos a ciudades pueden existir, pero deben incorporar protecciones para jugadores nuevos o claramente más débiles. Valoria no debe ser destruida de forma permanente ni borrar meses de progreso.

### 6.2 Guerra formal de Alianzas — dirección provisional aprobada
Se adopta un **modelo híbrido**:
- el PvP ordinario puede existir sin guerra formal;
- para alterar el mapa —conquistar territorio o atacar infraestructura estratégica de Alianza— debe existir una guerra formal;
- una Alianza puede declarar la guerra unilateralmente;
- existe un periodo de movilización/preparación antes de habilitar la guerra territorial;
- una vez iniciada, la guerra no tiene por qué ser un evento cerrado con marcador y duración prefijada;
- su resolución puede surgir de rendición, acuerdos o condiciones estratégicas por definir;
- la conquista respeta la geografía continua desde fronteras hacia posiciones interiores y, eventualmente, el Cuartel General.

Principio: **la guerra formal es el permiso para alterar el mapa, no un sustituto de la guerra emergente entre jugadores**.

Estado: **DIRECCIÓN PROVISIONAL APROBADA; MECÁNICAS DE GUERRA POR DISEÑAR MÁS ADELANTE**.

## 7. Bloque estructural 05 — Puerto/Muelle y expediciones

### 7.1 Dirección inicial
El Puerto/Muelle es un sistema de progresión avanzada y **no forma parte del prólogo Bastión I–X**. Puede existir visual o narrativamente antes de estar disponible para reforzar la sensación de un mundo mayor.

Su primera implementación conceptual es principalmente **PvE y económica**:
- construir y mejorar barcos/flota;
- enviar expediciones abstractas que parten y regresan;
- obtener recursos, objetos y descubrimientos mediante esas expediciones;
- generar una nueva capa de progresión y planificación sin exigir un segundo mapa navegable.

### 7.2 Evolución futura
El sistema debe dejar abierta, sin exigirla para Eldoria inicial, una evolución hacia:
- navegación/exploración marítima real;
- un segundo espacio o mapa marítimo;
- PvP naval;
- zonas u objetivos disputados;
- eventos marítimos periódicos PvE, PvP o mixtos.

Principio: **primero expediciones; navegación real solo si la evolución del producto la justifica**.

Estado: **DIRECCIÓN MACRO CERRADA; MECÁNICAS, DESBLOQUEO, FLOTA Y RECOMPENSAS ABIERTOS**.

## 8. Bloque estructural 06 — Sistema de Eventos

Eldoria tendrá un **sistema modular de eventos temporales y rotatorios**, diseñado para crecer con el juego y evitar depender de un único formato recurrente.

Puede albergar:
- eventos PvE individuales;
- eventos PvE colectivos;
- eventos PvP individuales;
- eventos PvP de Alianzas;
- competiciones directas o indirectas por objetivos/puntuación;
- formatos PvPvE donde La Brecha u otras amenazas interfieran en la competición;
- eventos vinculados a la Madurez del Mundo o a etapas concretas del servidor.

Los eventos PvP individuales serán **aislados y acotados en el tiempo**, permitiendo competir y destacar fuera del poder territorial de la Alianza. No sustituyen al PvP abierto ni a las guerras de Alianzas.

El Oráculo puede convivir dentro del ecosistema general de actividades periódicas, pero no define por sí solo el Sistema de Eventos.

Estado: **DIRECCIÓN MACRO CERRADA; FORMATOS, CALENDARIO, MATCHMAKING, PUNTUACIÓN Y RECOMPENSAS POR DISEÑAR MÁS ADELANTE**.

## 9. Dependencias detectadas

**Mundo compartido** condiciona → regiones, territorio, PvP, viajes, eventos y Puerto.

**Cronología común por edad** condiciona → desbloqueos, incorporación de nuevos servidores y Madurez del Mundo.

**La Brecha global** condiciona → eventos de servidor, PvE colectivo, divergencia entre servidores, Madurez del Mundo y parte del endgame.

**Alianzas** condicionan → Oráculos, territorio, cooperación, Gobierno del Mundo y PvP organizado.

**Territorio continuo** condiciona → puestos, fronteras, logística, guerras y acceso a instalaciones.

**Madurez del Mundo** condiciona → severidad de La Brecha, complejidad política, diplomacia, eventos y presión de largo plazo.

**Población civil de Valoria** condiciona → viviendas, crecimiento urbano y aparición narrativa del Consejo de Valoria.

**Consejo de Valoria** condiciona → progresión/investigación PvE interna y futuras ramas de desarrollo del reino.

**Puerto** condiciona → expediciones avanzadas y, solo en una fase posterior, navegación/PvP marítimo.

## 10. Decisiones abiertas prioritarias

### Macro todavía por consolidar
1. **Gobierno del Mundo:** acceso/legitimidad, poderes, duración, corte/consejo y destitución.
2. **Protección PvP:** principios macro para jugadores nuevos, diferencias extremas de poder y recuperación.
3. **Territorio de Alianza:** consecuencias macro de pérdida, desconexión y caída del Cuartel General.
4. **Brecha:** ciclo global, consecuencias, recuperación y relación exacta con Madurez del Mundo.

### Detalle deliberadamente aplazado
5. **Consejo de Valoria:** identidad/nombres de consejeros, orden exacto de incorporación, árboles, costes, tiempos y posibles especializaciones futuras.
6. **Guerra territorial:** conquista, defensa y pérdida exacta de puestos.
7. **Cuartel General:** consecuencias máximas de su caída.
8. **Territorio partido:** comportamiento exacto de zonas desconectadas.
9. **Oráculos:** número, distribución, ventanas, control, bonificaciones, ofrendas, jefe y recompensas.
10. **Diplomacia formal:** reglas cuando se incorpore como capa madura.
11. **Puerto:** desbloqueo, barcos, expediciones, costes, tiempos y recompensas.
12. **Eventos:** formatos concretos, calendario, matchmaking, puntuación y recompensas.

## 11. Corte provisional de Unity

### Primera vertical slice Unity
- Valoria visualmente convincente.
- Mundo explorable.
- Marcha/expedición.
- Recolección y un encuentro PvE representativo.
- Recompensa y regreso.
- Crecimiento visible del reino/héroe.
- Señales narrativas/visuales de La Brecha y del mundo mayor.

### Arquitectura preparada, no implementación inicial
- identidad de jugador/reino;
- progresión física de Valoria preparada para población, viviendas y crecimiento urbano posterior;
- modelo extensible de progresión/investigación capaz de alojar el Consejo de Valoria y sus consejeros canónicos;
- regiones/mapa persistente;
- edad/cronología del servidor y Madurez del Mundo;
- entidades/eventos mundiales;
- pertenencia a Alianza, jerarquía y roles;
- estado global de La Brecha;
- territorio continuo y estructuras de Alianza;
- estados de PvP/guerra;
- extensibilidad para eventos, Gobierno y sistemas avanzados.

### Futuro
- Oráculo completo;
- Gobierno del Mundo completo;
- Consejo de Valoria y población civil desarrollados;
- PvP territorial avanzado;
- diplomacia formal;
- Puerto/barcos;
- Sistema de Eventos ampliado;
- nuevas regiones y capas de progresión indefinida.

## 12. Regla de cierre del prototipo web

El prototipo web no necesita implementar los sistemas anteriores. Debe demostrar de forma estable el núcleo jugable y servir como especificación funcional. Una vez certificado el bucle completo y consolidado este mapa macro, las mejoras puramente cosméticas que busquen imitar el acabado final dejan de ser requisito para iniciar Unity.