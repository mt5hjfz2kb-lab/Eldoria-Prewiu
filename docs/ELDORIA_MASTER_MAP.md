# Eldoria — Mapa Maestro de Producto

Estado: preproducción para futura migración a Unity
Fecha de inicio: 2026-09-24

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
- El mundo/servidor es persistente e indefinido: no se fundamenta en reinicios obligatorios.
- Las Alianzas tienen influencia territorial real y pueden entrar en conflicto abierto.
- La Brecha es un sistema global dinámico capaz de transformar el mundo compartido.

### DEFINIR ANTES DE UNITY
- Reglas macro definitivas de PvP, guerra y protección del jugador.
- Reglas exactas de conquista, pérdida y recuperación territorial.
- Ciclo detallado de La Brecha y sus estados de madurez.
- Balance y reglas concretas del Oráculo.
- Gobierno del Mundo: legitimidad, acceso, duración, poderes y destitución.
- Consejo de Valoria: estructura de ministros/ramas como progresión PvE interna.

### DISEÑO MACRO, IMPLEMENTACIÓN POSTERIOR
- Puerto/Muelle y navegación/barcos.
- Conflicto territorial avanzado.
- Diplomacia formal entre Alianzas.
- Eventos colectivos recurrentes.
- Endgame y ciclos de larga duración.
- Expansiones/regiones posteriores.

## 3. Bloque estructural 01 — Mundo compartido + Brecha + Alianzas

### 3.1 Mundo compartido — dirección cerrada

El mundo compartido amplía el bucle individual, no lo sustituye. Cada jugador conserva Valoria como hogar propio y proyecta su poder al mapa mediante marchas, recolección, PvE, cooperación y conflicto.

Principios:
1. **Persistencia:** el servidor es un mundo persistente, indefinido y evolutivo. Valoria, héroes, colección, Alianza y progreso sobreviven al paso del tiempo.
2. **Sin reinicio fundacional:** los resets obligatorios no forman parte del modelo base. En el futuro pueden explorarse modos especiales o estacionales sin condicionar la arquitectura central.
3. **Evolución:** nuevas regiones, niveles de Bastión, amenazas, héroes, tropas, edificios, eventos y sistemas pueden abrirse con el tiempo.
4. **Edades sin borrado:** grandes etapas o capítulos pueden transformar el mundo sin eliminar el progreso del jugador.
5. **Regiones:** el mundo se organiza en regiones con identidad, recursos, amenazas y objetivos propios.
6. **Escala gradual:** el jugador empieza localmente; el prólogo revela que existen otros reinos y un conflicto mayor.
7. **Actividad útil:** viajar por el mapa responde a una intención: recurso, amenaza, descubrimiento, alianza, territorio o evento.
8. **Sin destrucción irreversible temprana:** el PvP no debe convertir meses de progreso en una pérdida definitiva.

Estado: **DIRECCIÓN CERRADA; REGLAS DE EVOLUCIÓN POR DEFINIR**.

### 3.2 La Brecha — dirección cerrada

La Brecha es uno de los motores narrativos y jugables principales de Eldoria y funciona en tres escalas conectadas:

- **Personal/narrativa:** origen del misterio de Valoria, corrupción, materiales, enemigos y Reliquias.
- **Regional:** fisuras, zonas corrompidas, enemigos/eventos y transformaciones del mapa.
- **Global/servidor:** presión compartida que evoluciona con el tiempo y con la respuesta colectiva de jugadores y Alianzas.

No es una simple barra global ni una categoría de enemigo. Puede corromper regiones, modificar terreno, recursos y rutas, abrir o cerrar zonas, alterar criaturas, generar fisuras, jefes y eventos, y amenazar territorio de Alianzas. Debe poder crear situaciones en las que enemigos políticos tengan razones para cooperar temporalmente.

#### Madurez del Mundo

Principio cerrado: **las consecuencias de fallar ante La Brecha escalan con la madurez del servidor**.

En las primeras etapas, el fracaso debe enseñar y advertir: la Brecha puede fortalecerse con consecuencias limitadas o principalmente narrativas. Conforme el mundo madura, los fallos pueden producir efectos más profundos y persistentes: corrupción alrededor de instalaciones, enemigos más peligrosos, alteración de rutas o recursos y contribución al estado global de La Brecha.

Los umbrales temporales exactos y las fases de madurez quedan abiertos. La intención es que un mundo persistente envejezca y se transforme sin necesitar resets para recuperar tensión.

Estado: **DIRECCIÓN CERRADA; CICLO, FASES, RECOMPENSAS Y RECUPERACIÓN POR DEFINIR**.

### 3.3 Alianzas — dirección cerrada

Las Alianzas son una capa estructural de largo plazo. Deben permitir hacer cosas que un jugador solo no puede hacer con la misma eficacia y crear historia emergente del servidor.

#### Núcleo físico
- Cada Alianza puede construir un **Cuartel General/Fortaleza de Alianza** físico en el mundo. Nombre final pendiente.
- No es decorativo: es centro de progreso colectivo y pieza estratégica.
- Para iniciar su construcción se exige un mínimo de miembros, evitando fortalezas fantasma y saturación del mapa.
- La construcción requiere participación real mediante marchas enviadas por miembros. Una marcha sigue trabajando aunque el jugador se desconecte, igual que una recolección.
- Si no se mantiene la participación mínima requerida, la construcción se detiene. Los valores exactos quedan para balance.
- El Cuartel General puede ser atacado. La severidad máxima de su caída queda abierta: daño, inutilización temporal, pérdida territorial o reconstrucción son posibilidades. No se asume que su destrucción disuelva automáticamente la Alianza.

#### Territorio
- La Alianza ejerce **control territorial real**, con consecuencias económicas, militares, PvE y de La Brecha.
- La expansión parte del Cuartel General y debe ser **continua**: no se puede controlar una zona aislada sin conexión territorial previa.
- La frontera crece mediante **puestos/estructuras de Alianza conectados** construidos colectivamente.
- Estos puestos pueden ser objetivos estratégicos en guerra.
- El territorio puede contener recursos, rutas, instalaciones, Oráculos y otros objetivos con valor real.
- Queda abierta la regla para territorio desconectado por guerra o por transformaciones de La Brecha.

#### Progresión
- Las Alianzas tienen progresión colectiva propia.
- Existe un **tronco común** de desarrollo para todas, que desbloquea capacidades fundamentales.
- Sobre ese tronco existen **especializaciones limitadas** que aportan identidad sin crear clases rígidas ni permitir que una mala decisión inutilice una Alianza.
- Posibles ámbitos: militar, economía/construcción, territorio y Brecha. Las ramas exactas quedan abiertas.
- La capacidad máxima de miembros **crece con la progresión de la Alianza** hasta un límite final por balance. Las cifras mencionadas durante diseño son ejemplos, no valores cerrados.

#### Jerarquía y liderazgo
Jerarquía conceptual inicial: **Líder → Oficiales → Veteranos → Miembros**. Los nombres finales pueden adaptarse al lore.

- El Líder tiene autoridad ordinaria máxima.
- Todos los Oficiales comparten por ahora los mismos permisos y pueden realizar prácticamente toda la gestión ordinaria.
- Solo el Líder puede nombrar o retirar Oficiales.
- Otras facultades irreversibles exclusivas del Líder se decidirán cuando existan las mecánicas correspondientes.
- Los cargos funcionales especializados no son necesarios de inicio; pueden añadirse después si la complejidad lo exige.

El liderazgo es fuerte, pero existe un mecanismo extraordinario de destitución:
- una moción difícil de activar;
- supermayoría muy alta como principio (el 80 % se conserva solo como referencia, no cifra cerrada);
- protecciones contra abuso, rotación hostil o votaciones constantes;
- la moción debe resolver también la sucesión para que la Alianza no quede descabezada.

#### Entrada, salida y geografía del jugador
- Abandonar o ser expulsado de una Alianza **no teletransporta Valoria**.
- La ciudad permanece físicamente donde estaba y pierde las bonificaciones, protecciones y derechos de la antigua Alianza.
- Entrar en una nueva Alianza tampoco mueve Valoria automáticamente.
- Reagruparse requiere una decisión voluntaria de traslado.
- Se prevén objetos/mecánicas de reubicación de ciudad. Pueden formar parte de recompensas y eventualmente de monetización, siempre evitando que comprar traslados convierta guerra/territorio en pay-to-win.

#### Diplomacia
- La diplomacia formal entre Alianzas no es requisito de lanzamiento.
- Desde el inicio puede existir diplomacia emergente entre jugadores: cooperación, conflicto y acuerdos sociales sin contrato sistémico.
- En fases posteriores pueden añadirse pactos de no agresión, aliados formales, enemigos declarados, tratados o coaliciones temporales contra La Brecha.
- Esta sofisticación puede acompañar la Madurez del Mundo.

Estado: **MARCO ESTRUCTURAL CERRADO; BALANCE Y REGLAS DE DETALLE ABIERTOS**.

## 4. Bloque estructural 02 — Oráculos

### 4.1 Identidad

Los antiguos **Oráculos del mundo han sido corrompidos por La Brecha**. Son instalaciones físicas disputadas por Alianzas y conectan PvP territorial, cooperación interna y PvE colectivo.

Habrá múltiples Oráculos, no uno único, para que el sistema escale con el número de Alianzas y pueda ofrecer objetivos de distinta importancia o localización.

### 4.2 Ciclo conceptual

Ciclo provisional:

**Conquista → Dominio → Ofrenda → Manifestación → combate colectivo → recompensa.**

- **Conquista:** actividad recurrente PvP entre Alianzas por el control de un Oráculo.
- **Dominio:** la Alianza vencedora obtiene durante el periodo de control una bonificación persistente para todos sus miembros.
- **Ofrenda:** durante el dominio, los miembros aportan recursos/objetos al Oráculo.
- **Manifestación:** las contribuciones pueden provocar la aparición de un jefe relacionado con La Brecha.
- **Combate colectivo:** la Alianza combate conjuntamente contra ese jefe.
- **Recompensa:** existen recompensas colectivas e individuales por definir.

Como dirección de diseño, la conquista debe valorar el **tiempo de control efectivo durante una ventana**, no únicamente el último golpe. La referencia inicial de una actividad semanal de aproximadamente 1–1,5 horas es provisional y no constituye balance cerrado.

Los Oráculos pueden diferenciarse por región, importancia, bonificación o relación con sistemas como guerra, producción, exploración o Brecha. No está cerrado.

### 4.3 Fracaso y Madurez del Mundo

En servidores jóvenes, fracasar ante la Manifestación/Jefe debe tener consecuencias leves o narrativas. Conforme el mundo madura, el fracaso puede fortalecer La Brecha y producir consecuencias regionales o globales más tangibles.

Estado: **CONCEPTO CERRADO; NÚMEROS, CALENDARIO, PARTICIPACIÓN, JEFE Y RECOMPENSAS ABIERTOS**.

## 5. Bloque estructural 03 — Gobierno

Eldoria contempla dos sistemas distintos que no deben confundirse.

### 5.1 Consejo de Valoria — Gobierno interno PvE

Sistema interno del reino del jugador. El concepto original es un consejo con ministros/asesores que funcionan diegéticamente como ramas de investigación o tecnología.

Posibles ámbitos: economía/producción, militar/defensa, conocimiento/exploración/Brecha y otras ramas futuras.

Nombre **Consejo de Valoria** provisional. Se diseñará en profundidad más adelante.

Estado: **CONCEPTO REGISTRADO; DISEÑO ABIERTO**.

### 5.2 Gobierno del Mundo — autoridad política del servidor

El servidor tendrá una **autoridad política real ejercida por un jugador**. Nombre provisional: **Soberano del Mundo**; título final pendiente de lore.

Principios:
- no debe ser simplemente el jugador número 1 de un ranking;
- debe poseer poderes y responsabilidades reales;
- puede existir una corte/consejo con cargos ocupados por otros jugadores, incluso potencialmente de distintas Alianzas;
- puede afectar políticas/beneficios del servidor y asumir responsabilidades relacionadas con La Brecha;
- el acceso al cargo puede combinar poder militar, territorio y legitimidad política.

Quedan abiertos el método exacto de acceso, duración, poderes, sucesión y destitución.

Estado: **DIRECCIÓN CERRADA; MECÁNICA POLÍTICA ABIERTA**.

## 6. Bloque estructural 04 — PvP, guerra y protección

### 6.1 PvP abierto

Dirección cerrada: **el PvP abierto y las guerras entre Alianzas están permitidos**. La política del servidor debe emerger de sus jugadores: un servidor puede ser relativamente pacífico, dividirse en bloques o vivir conflictos frecuentes.

Los ataques directos a ciudades pueden existir, pero deben incorporar protecciones para jugadores nuevos o claramente más débiles: escudos/protecciones, recuperación, límites antiabuso, zonas seguras u otros mecanismos todavía por definir.

Valoria no debe ser destruida de forma permanente ni borrar meses de progreso. Una derrota puede producir bajas, pérdida de recursos, daño reparable, retirada u otras consecuencias significativas.

### 6.2 Guerra formal de Alianzas — dirección provisional aprobada

Se adopta como dirección de diseño un **modelo híbrido**:
- el PvP ordinario puede existir sin guerra formal;
- para alterar el mapa —conquistar territorio o atacar infraestructura estratégica de Alianza— debe existir una guerra formal;
- una Alianza puede declarar la guerra unilateralmente; el rival no necesita aceptarla;
- existe un periodo de movilización/preparación antes de habilitar la guerra territorial, evitando que la estrategia dominante sea desmontar infraestructura durante horarios de baja actividad;
- una vez iniciada, la guerra no debe convertirse necesariamente en un evento cerrado con marcador y duración prefijada;
- su resolución puede surgir de rendición, acuerdos o condiciones estratégicas todavía por definir;
- la conquista debe respetar la geografía continua: la presión avanza desde fronteras/puestos exteriores hacia posiciones interiores y, eventualmente, el Cuartel General.

Principio: **la guerra formal es el permiso para alterar el mapa, no un sustituto de la guerra emergente entre jugadores**.

Quedan abiertos: duración del periodo de movilización, condiciones de final, conquista exacta de puestos, intervención de terceros, treguas, rendición, consecuencias de derrota y reglas del Cuartel General.

Estado: **DIRECCIÓN PROVISIONAL APROBADA; MECÁNICAS DE GUERRA POR DISEÑAR**.

## 7. Dependencias detectadas

**Mundo compartido** condiciona → regiones, territorio, PvP, viajes, eventos y Puerto.

**La Brecha global** condiciona → eventos de servidor, PvE colectivo, Madurez del Mundo y parte del endgame.

**Alianzas** condicionan → Oráculos, territorio, cooperación, Gobierno del Mundo y PvP organizado.

**Territorio continuo** condiciona → puestos, fronteras, logística, guerras y acceso a instalaciones.

**Madurez del Mundo** condiciona → severidad de La Brecha, complejidad política, diplomacia y presión de largo plazo.

## 8. Decisiones abiertas prioritarias

1. **Guerra territorial:** ¿cómo se conquista, defiende y pierde exactamente un puesto de Alianza?
2. **Cuartel General:** ¿qué consecuencias máximas puede tener su caída y puede existir alguna circunstancia excepcional de disolución?
3. **Territorio partido:** ¿qué ocurre con zonas que pierden conexión con el Cuartel General?
4. **Brecha:** fases globales, consecuencias, recuperación, recompensas y relación exacta con la Madurez del Mundo.
5. **Gobierno del Mundo:** acceso, poderes, duración, corte/consejo y destitución.
6. **Protección PvP:** escudos, protección de nuevos jugadores, diferencias extremas de poder y recuperación.
7. **Oráculos:** número, distribución, ventanas, control, bonificaciones, ofrendas, jefe y recompensas.
8. **Diplomacia formal:** reglas cuando se incorpore como capa madura del servidor.

## 9. Corte provisional de Unity

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
- regiones/mapa persistente;
- entidades/eventos mundiales;
- pertenencia a Alianza, jerarquía y roles;
- estado global de La Brecha y Madurez del Mundo;
- territorio continuo y estructuras de Alianza;
- estados de PvP/guerra;
- eventos colectivos y Gobierno como extensiones.

### Futuro
- Oráculo completo;
- Gobierno completo;
- PvP territorial avanzado;
- diplomacia formal;
- Puerto/barcos;
- ciclos/endgame definitivos.

## 10. Regla de cierre del prototipo web

El prototipo web no necesita implementar los sistemas anteriores. Debe demostrar de forma estable el núcleo jugable y servir como especificación funcional. Una vez certificado el bucle completo y consolidado este mapa macro, las mejoras puramente cosméticas que busquen imitar el acabado final dejan de ser requisito para iniciar Unity.
