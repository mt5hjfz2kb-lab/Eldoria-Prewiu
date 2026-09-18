# Eldoria — Reglas de baseline y prevención de regresiones

Este archivo es parte del proceso de desarrollo, no documentación opcional.

## Errores que no deben repetirse

1. **Reconstruir una versión nueva desde una base antigua.** Una versión validada es el único punto de partida permitido para la siguiente iteración.
2. **Congelar como canónica una build solo porque compila o pasa greps.** La validación exige preservar también assets, narrativa, orden, UX y comportamiento ya aprobados.
3. **Perder assets aprobados al consolidar.** Sir Aldric debe conservar su retrato real; el placeholder de espadas está prohibido.
4. **Recuperar un primer boceto de textos o tutoriales.** Las correcciones narrativas posteriores tienen prioridad y deben mantenerse acumulativamente.
5. **Usar texto visible/regex como sustituto del estado real del juego cuando exista un evento o estado fiable.** Los disparadores narrativos deben migrar hacia estado/acciones reales.
6. **Añadir feedback falso.** Recompensas, propiedad de cartas, progreso y estados visuales deben derivar del estado real.
7. **Crear ramas/versiones/patches paralelos por inercia.** Una sola línea de avance; versiones solo por hito o hotfix con motivo.
8. **Declarar una versión validada sin revisión de regresión.** Antes de congelar: comparar contra la baseline anterior y comprobar los elementos ya aprobados.

## Contrato de baseline

- Baseline actual: `v0202/index.html` (v0.20.2).
- Toda mejora parte de esta baseline materializada.
- No se reconstruye desde r7/r6/0.19.x para producir una 0.20.x futura.
- Una corrección se aplica incrementalmente sobre la baseline actual.
- Tras QA y validación, la build completa resultante sustituye a la baseline.
- La publicación debe fallar si desaparece un elemento protegido.

## Elementos protegidos actuales

- Retrato real de Sir Aldric en sus cinemáticas.
- Orin conserva la estructura cinematográfica validada equivalente a Aldric, con paleta púrpura y `assets/orin-portrait.jpg`; no sustituirla por layouts completos ni recreaciones CSS.
- Introducción contextual de Capítulo I y flujo narrativo de Aldric.
- Typewriter estable y botón Continuar manual en diálogos importantes.
- Orden canónico Capítulo I → Capítulo II → mundo global → Héroes/Reliquias/Duelo → Mar de Cristal.
- Bosque Seguro con recompensa real de +180 madera y marcha visible.
- Propiedad visual del Duelo derivada de `piece.owner`, nunca alternada por índice.
- Archivo, Héroes, Resonancias y Duelo con onboarding progresivo.
- Mar de Cristal como teaser del Capítulo III.
- Prioridad móvil.

## Checklist antes de congelar

- Build y sintaxis.
- Partida nueva/reset.
- Comparación contra baseline anterior.
- Assets aprobados presentes.
- Textos y escenas aprobados presentes y en su orden.
- Sin placeholders reintroducidos.
- Sin recompensas/propiedad simuladas.
- Metadatos de versión coherentes.
- Publicación correcta.
- Solo entonces puede marcarse como nueva baseline canónica.


## Punto de recuperación v0.20.2

- Fuente canónica desplegable: `v0202/index.html`.
- URL pública: `/playtest/`, generada siempre desde esa fuente.
- `v0201/index.html` queda como histórico anterior y no debe volver a usarse como fuente de despliegue.
- Todo cambio posterior debe modificar primero la baseline canónica y pasar los gates de regresión antes de publicar.
