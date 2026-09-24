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

### DEFINIR ANTES DE UNITY
- Modelo de mundo compartido/servidor.
- Evolución de La Brecha a escala global.
- Alianzas: propósito, progresión, territorio y cooperación.
- Relación entre Alianzas, La Brecha y actividades colectivas.
- Oráculo: función, control, ciclo y recompensas.
- Gobierno: alcance, legitimidad, poderes y relación con el servidor/alianzas.
- Reglas macro de PvP y protección del jugador.

### DISEÑO MACRO, IMPLEMENTACIÓN POSTERIOR
- Puerto/Muelle y navegación/barcos.
- Conflicto territorial avanzado.
- Eventos colectivos recurrentes.
- Endgame y ciclos de larga duración.
- Expansiones/regiones posteriores.

## 3. Bloque estructural 01 — Mundo compartido + Brecha + Alianzas

### 3.1 Mundo compartido — propuesta base

El mundo compartido debe ampliar el bucle individual, no sustituirlo. Cada jugador conserva Valoria como hogar propio y proyecta su poder al mapa mediante marchas, recolección, PvE, cooperación y conflicto.

Principios propuestos:
1. **Persistencia:** el mapa tiene estado compartido y cambia con acciones/eventos.
2. **Regiones:** el mundo se organiza en regiones con identidad, recursos, amenazas y objetivos propios.
3. **Escala gradual:** el jugador empieza localmente; el prólogo revela que existen otros reinos y un conflicto mayor.
4. **Actividad útil:** viajar por el mapa debe responder a una intención: recurso, amenaza, descubrimiento, alianza, territorio o evento.
5. **Sin castigo destructivo temprano:** el PvP futuro no debe invalidar el progreso inicial ni convertir la reconstrucción de Valoria en pérdida constante.

Estado: **PROPUESTA A CONSOLIDAR**.

### 3.2 La Brecha — propuesta base

La Brecha debe funcionar en tres escalas conectadas:

- **Personal/narrativa:** origen del misterio de Valoria, corrupción, materiales, enemigos y Reliquias.
- **Regional:** fisuras, zonas corrompidas, enemigos/eventos y cambios temporales en partes del mapa.
- **Global/servidor:** presión compartida que evoluciona con el tiempo y con la respuesta colectiva de jugadores/alianzas.

La Brecha no debe ser una simple barra global. Debe modificar qué ocurre en el mundo: aparición de amenazas, acceso a zonas, recursos/materiales, eventos y objetivos colectivos. El estado global puede estructurar temporadas/edades del servidor, pero la forma exacta del ciclo queda abierta.

Estado: **DIRECCIÓN PROPUESTA; CICLO GLOBAL ABIERTO**.

### 3.3 Alianzas — propuesta base

Las Alianzas deben existir porque permiten hacer cosas que un jugador solo no puede hacer con la misma eficacia, no únicamente por chat o bonificaciones pasivas.

Funciones estructurales propuestas:
- cooperación contra amenazas de La Brecha;
- proyectos/objetivos colectivos;
- presencia y coordinación territorial;
- acceso a actividades compartidas como el futuro Oráculo;
- contribuciones/donaciones con propósito visible;
- identidad, roles y coordinación social;
- posible participación en Gobierno según el modelo que se cierre después.

Las Alianzas no deben ser obligatorias durante el prólogo. El juego puede anticipar el mundo compartido y desbloquear la capa social después de que el jugador domine su propio reino.

Estado: **PROPUESTA A CONSOLIDAR**.

## 4. Dependencias detectadas

**Mundo compartido** condiciona → regiones, territorio, PvP, viajes, eventos y Puerto.

**La Brecha global** condiciona → eventos de servidor, PvE colectivo, temporadas/edades y parte del endgame.

**Alianzas** condicionan → Oráculo, territorio, cooperación, Gobierno y PvP organizado.

Por ello, Oráculo y Gobierno no deben cerrarse antes de fijar estas tres bases.

## 5. Decisiones que requieren dirección del propietario

No son bloqueos para documentar el resto, pero sí decisiones de producto antes de cerrar este bloque:

1. **Servidor:** ¿queremos mundos/servidores con ciclo de vida y evolución propia, o un mundo persistente sin reinicios/ciclos fuertes?
2. **Territorio:** ¿las Alianzas deben poseer/controlar territorio físicamente en el mapa o influir en regiones sin propiedad rígida?
3. **PvP:** ¿el conflicto principal debe centrarse en objetivos/territorio o permitir ataques directos frecuentes a ciudades de jugadores?
4. **Brecha:** ¿su evolución global debe poder cambiar de forma duradera el mapa/servidor o funcionar principalmente mediante eventos temporales?
5. **Gobierno:** ¿debe surgir del control/éxito de Alianzas, de un sistema electoral/social, o de una combinación?

## 6. Corte provisional de Unity

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
- pertenencia a alianza y roles;
- estado global de La Brecha;
- propiedad/influencia territorial si finalmente se aprueba;
- eventos colectivos y Gobierno como extensiones.

### Futuro
- Oráculo completo;
- Gobierno completo;
- PvP territorial avanzado;
- Puerto/barcos;
- ciclos/endgame definitivos.

## 7. Regla de cierre del prototipo web

El prototipo web no necesita implementar los sistemas anteriores. Debe demostrar de forma estable el núcleo jugable y servir como especificación funcional. Una vez certificado el bucle completo y consolidado este mapa macro, las mejoras puramente cosméticas que busquen imitar el acabado final dejan de ser requisito para iniciar Unity.
