# Eldoria — Decisiones de diseño y producción

Este archivo contiene decisiones vigentes que no deben depender de la memoria de un chat.

## D-001 — Una sola línea canónica
**Vigente.** No crear ramas/versiones paralelas por inercia. Cada baseline validada pasa a ser la base real de la siguiente.

## D-002 — Vertical slice web como laboratorio
**Vigente.** La versión web valida loop, tutorial, narrativa, orden de sistemas y comprensión a bajo coste. No es el objetivo tecnológico final.

## D-003 — Combate
**Vigente.** Combate semiautomático como dirección principal, con intervención significativa del jugador.

## D-004 — Tutorial y narrativa
**Vigente.** Tutorial contextual y progresivo. El jugador debe saber qué hacer, por qué y dónde tocar. Aldric guía narrativa/general; Orin enseña Códice, cartas, Brecha y Duelo.

## D-005 — Cartas
**Vigente / en validación.** Las cartas se descubren durante el recorrido; rareza, usar/conservar e Indestructible forman parte del sistema. La comprensión y el valor real de usar vs conservar se están midiendo en v0.21.

## D-006 — Mundo global
**Vigente.** El mapa global forma parte de la fantasía de crecimiento/4X. No fingir backend o PvP real que todavía no exista.

## D-007 — Salto a Unity
**Dirección preferida, pendiente de cerrar tras playtests.** Construir un **vertical slice de producción**: alcance limitado al contenido validado, pero sistemas reutilizables/data-driven que puedan crecer. No hacer una demo técnicamente desechable ni construir prematuramente toda la infraestructura de un MMO.

## D-008 — Accesibilidad técnica para el propietario
**Vigente para Unity.** El propietario del proyecto no necesita saber programación. Estadísticas, costes, héroes, cartas, enemigos, edificios y recompensas deben ser editables mediante datos/Inspector/herramientas siempre que sea razonable, no enterrados en código.

## D-009 — Presupuesto
**Vigente. Presupuesto: 0 €.** No realizar gasto económico hasta que los playtests y la evolución del producto den evidencia suficiente para justificar inversión. Antes de cualquier gasto futuro, identificar qué problema resuelve y qué salto de calidad/validación aporta.

## D-010 — Fuente de verdad
**Vigente.** El chat es espacio de trabajo, no archivo maestro. Código, estado, decisiones, baseline y resultados de pruebas deben quedar persistidos en el repositorio/documentación.
