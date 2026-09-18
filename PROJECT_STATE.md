# Eldoria — Estado actual del proyecto

> **Leer este archivo primero al abrir un chat o sesión nueva.**  
> Fuente de verdad operativa: repositorio + Documento Maestro + este estado. El chat no es la memoria principal.

## Estado actual

- Proyecto: **Eldoria / Reinos de Eldoria**
- Fase: vertical slice web en ronda de playtest externo.
- Baseline canónica: **v0.21.0 external playtest**
- Fuente canónica: `v0210/index.html`
- URL pública: `https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/`
- La v0.21.0 queda congelada durante la ronda salvo hotfix que bloquee o invalide el test.
- `v0201` y `v0202` son históricos y no son fuentes de despliegue.
- Reglas de recuperación/regresión: `ELDORIA_BASELINE_RULES.md`.

## Objetivo inmediato

Recoger aproximadamente **5–8 playtests externos** de v0.21.0, comparar patrones y decidir qué necesita validación/corrección antes del salto tecnológico.

No añadir sistemas nuevos a la build congelada durante esta ronda. Registrar los resultados en `PLAYTEST_RESULTS.md`.

## Dirección para Unity

La opción de trabajo preferida es un **vertical slice de producción**:

- reconstruir en Unity únicamente la experiencia/sistemas que hayan demostrado valor;
- diseñar los sistemas importantes de forma reutilizable y data-driven;
- evitar tanto una demo desechable como sobrearquitecturar un MMO antes de tiempo;
- permitir ampliar después héroes, cartas, edificios, tropas, mapa, combate y futuros sistemas sin rehacer el núcleo;
- priorizar datos editables frente a valores enterrados en código, para que el propietario del proyecto pueda ajustar contenido sin saber C#.

**Todavía no iniciar una migración irreversible de sistemas que puedan cambiar por los resultados de la ronda v0.21.**

## Restricción económica

**Presupuesto actual: 0 €.** No comprar assets, plugins, música, modelos, servicios, servidores ni contratar trabajo mientras no exista evidencia suficiente de que Eldoria tiene posibilidades reales. Usar herramientas gratuitas, assets provisionales/gratuitos y trabajo propio.

## Filosofía de producto

- Lo validado no se vuelve a fabricar: se conserva y se evoluciona.
- La web sirve para validar diseño y flujo a bajo coste.
- Unity debe convertir lo validado en una base de videojuego, no reiniciar el diseño desde cero.
- Prioridad móvil.
- El jugador debe saber siempre: **qué intenta hacer, por qué y dónde tocar**.
- Aldric = anfitrión/guía narrativo.
- Orin = guía diegética de Códice, cartas, Brecha y Duelo.
- Tutoriales progresivos y contextuales.
- No simular estados, recompensas o multijugador que no existan realmente.

## Loop y contenido actual

Flujo validado a alto nivel:
**Reino → decisión/mejora → mundo → combate semiautomático → recompensa → regreso**, ampliado con héroes, cartas/Códice, Duelo de Reliquias y mapa global.

Capítulo I reconstruye Valoria. Capítulo II introduce expediciones, Brecha, cartas, Orin, héroes, mundo global y Duelo. Capítulo III / **Mar de Cristal** permanece como teaser.

## Riesgos/hipótesis a contrastar en playtest

No convertir estas hipótesis en cambios hasta observar patrones:
- comprensión del sistema de héroes;
- comprensión/valor de usar vs conservar cartas;
- comprensión y atractivo del Duelo;
- ritmo posiblemente demasiado rápido;
- transiciones narrativas y claridad de objetivos;
- calidad/integración final del retrato de Orin;
- game feel del Duelo;
- transición posterior al onboarding del mundo global.

## Verificación técnica actual

La publicación de Pages usa `v0210/index.html` y ejecuta comprobación de sintaxis JavaScript y gates estructurales/regresión. La última v0.21.0 fue desplegada correctamente.

Esto **no equivale a un E2E completo de navegador/móvil**; no afirmar que existe si no se ha ejecutado.

## Cómo continuar desde un chat nuevo

1. Recuperar este repositorio; **no reconstruir el proyecto**.
2. Leer `PROJECT_STATE.md`.
3. Leer `ELDORIA_BASELINE_RULES.md`.
4. Leer `DESIGN_DECISIONS.md`.
5. Leer `PLAYTEST_RESULTS.md`.
6. Consultar el Documento Maestro cuando una decisión requiera lore/diseño de mayor alcance.
7. Continuar desde la baseline/commit actual y actualizar estos documentos cuando cambie una decisión importante.

## Regla de mantenimiento

Cada cambio de fase, baseline, arquitectura, restricción económica, decisión de diseño importante o resultado de playtest debe reflejarse en estos archivos. El objetivo es que cambiar de chat no implique perder el proyecto.
