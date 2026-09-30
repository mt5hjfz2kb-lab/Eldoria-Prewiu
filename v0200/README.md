# Eldoria v0.20.0 — Narrative Flow

v0.20.0 nace directamente de v0.19.5 r7 consolidada. No es una reconstrucción ni una rama alternativa del diseño: conserva el vertical slice y cambia cómo se presenta, guía y conecta.

## Objetivo de la versión

Convertir la sucesión funcional de sistemas de r7 en una aventura comprensible para un jugador que no conoce Eldoria.

## Contrato de experiencia

El jugador debe saber siempre: qué está intentando conseguir, por qué importa y cuál es su siguiente acción.

Sir Aldric se presenta al comienzo del Capítulo I y actúa como anfitrión narrativo. Los hitos importantes usan diálogo cinematográfico bloqueante con texto progresivo y botón Continuar solo al terminar. Las ayudas mecánicas pequeñas permanecen contextuales para no interrumpir constantemente.

## Arranque canónico v0.20

1. Capítulo I — Las Cenizas de Valoria.
2. Presentación de Sir Aldric.
3. Aldric explica el estado del reino y por qué hay que reconstruir antes de investigar la Brecha.
4. Granero resaltado como primera acción.
5. Reconstrucción: Granero → Aserradero → Cantera → Forja → Cuartel.
6. Cierre ceremonial de la reconstrucción.
7. Mejora final del Granero y apertura de las puertas.
8. Capítulo II continúa con el flujo canónico de r7.

## Diálogo inicial

«Mi señor, Valoria sobrevivió… pero apenas.»

«Nuestros almacenes están vacíos, los talleres destruidos y los soldados dispersos.»

«Antes de buscar respuestas sobre la Brecha, debemos conseguir que el reino vuelva a sostenerse.»

«Empecemos por el Granero. Sin alimento, Valoria no puede levantarse.»

## Reglas congeladas

- No dividir el Capítulo II.
- Héroes, mundo global, Bastión, segunda expedición, cartas/reliquias y Duelo permanecen dentro del Capítulo II.
- Mundo global obligatorio antes del Bastión/cierre.
- Primer Duelo obligatorio.
- Capítulo III es únicamente el teaser de Mar de Cristal.
- No fingir multijugador real.
- Prioridad móvil.

## Trabajo técnico de v0.20

- Materializar una fuente editable limpia desde la build consolidada; dejar de añadir capas de runtime históricas.
- Integrar retrato real de Sir Aldric en lugar del icono genérico de espadas.
- Corregir el disparador del diálogo inicial: debe ejecutarse por estado del juego al entrar en Capítulo I, no por coincidencia textual accidental.
- Mantener el layout del texto estable durante el efecto máquina de escribir.
- Auditar cada transición narrativa del vertical slice y añadir/quitar/reordenar diálogos según necesidad.
- Validar partida nueva completa y móvil antes de considerar v0.20 estable.

## Estado

EN DESARROLLO. v0.19.5 r7 queda como referencia funcional anterior. Esta carpeta define la nueva línea v0.20.0; no debe publicarse como estable hasta superar QA.