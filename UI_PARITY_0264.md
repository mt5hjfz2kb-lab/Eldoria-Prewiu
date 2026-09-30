# Eldoria v0.26.4 — Matriz de paridad funcional UI/UX

Baseline: v0.26.3 certificada, commit `b9fe018f41c99adf8b447dafbca3c9178b31b7a7`.
Regla: el rediseño puede mover, redimensionar y reagrupar controles, pero no puede retirar acciones, información ni gates de progresión.

| Función / control | Antes v0.26.3 | Después v0.26.4 | Verificación |
|---|---|---|---|
| Ciudad / Valoria | ✓ | ✓ | `nav-kingdom` + fresh-save |
| Mundo | ✓ | ✓ | `nav-world` + nodos QA |
| Héroes | ✓ | ✓ | full-screen + `nav-heroes` |
| Arcón | ✓ | ✓ | full-screen + ficha de objeto |
| Códice | ✓ | ✓ | full-screen + reliquias/Duelo |
| Forja | ✓ | ✓ | pasa a pantalla completa; forjado conservado |
| Ranking | ✓ | ✓ | movido de navegación primaria al HUD |
| Poder total | ✓ | ✓ | HUD + panel de Poder |
| Madera / Piedra / Comida | ✓ | ✓ | HUD compacto, mismos valores |
| Menú de jugador | ✓ | ✓ | mismo acceso `player-menu` |
| Objetivo/tutorial contextual | ✓ | ✓ | mismo sistema, menor ruido |
| Bastión / mejora principal | ✓ | ✓ | panel inferior contextual |
| Aserradero | ✓ | ✓ | construir/mejorar/timer |
| Cuartel | ✓ | ✓ | construir/reclutar |
| Granero | ✓ | ✓ | construir/mejorar/timer |
| Cantera de Valoria | ✓ | ✓ | construir/mejorar/timer |
| Salón de Héroes | ✓ | ✓ | pantalla completa |
| Equipar héroes | ✓ | ✓ | mismo inventario/equipo |
| Configurar marcha | ✓ | ✓ | mismo compañero/reglas |
| Prueba de Marcha | ✓ | ✓ | CTA móvil + ataque |
| Recolectar recursos | ✓ | ✓ | panel inferior contextual |
| Cazar fauna | ✓ | ✓ | panel inferior contextual |
| Atacar Corruptos/Fisura | ✓ | ✓ | panel inferior contextual |
| Devorador de Éter | ✓ | ✓ | élite diferenciada visualmente |
| Nareth / Maelis | ✓ | ✓ | ubicación/acción conservadas |
| Asalto final | ✓ | ✓ | acción y progresión conservadas |
| Timers de tareas | ✓ | ✓ | visibles, sin cambio de lógica |
| Recolección offline | ✓ | ✓ | QA de persistencia existente |
| Respawn/relocación de nodos | ✓ | ✓ | lógica sin cambios |
| Arcón: equipo/material/lore | ✓ | ✓ | cuadrícula + ficha |
| Códice: usar/conservar | ✓ | ✓ | sin cambios de reglas |
| Indestructible/enfriamiento | ✓ | ✓ | sin cambios |
| Duelo de Reliquias | ✓ | ✓ | tablero 3×3 conservado |
| Diálogos/cinemáticas | ✓ | ✓ | contenido sin cambios |
| Bloqueos/desbloqueos | ✓ | ✓ | prueba locked/unlocked |
| Barra inferior | dinámica | Ciudad · Mundo · Héroes · Arcón · Códice | gates conservados |
| Mapa táctil | arrastre directo | seguimiento directo + inercia ligera | QA de pointer drag |
| Paneles rápidos | locales/variables | bottom sheet consistente | no solapa nav |
| Zonas táctiles | variables | mínimo objetivo 44 px | QA móvil |

## Inventario funcional auditado

### HUD y navegación
Menú de jugador; recursos; Poder; Ranking; Arcón; Ciudad; Mundo; Héroes; Códice; gates de desbloqueo.

### Ciudad
Bastión, Aserradero, Cuartel, Granero, Cantera, Forja y Salón de Héroes; construir, mejorar, reclutar, abrir gestión y temporizadores.

### Mundo
Bosques, canteras, carne/provisiones, fauna, Corruptos, Fisura, élite, Nareth, Prueba de Marcha, Brecha final y retorno a Valoria; recolectar, cazar, inspeccionar y atacar.

### Gestión
Héroes: selección, ATQ/DEF/APO, equipo y composición. Arcón: inventario. Códice: reliquias, usar/conservar, entrenamiento y Duelo. Forja: estado, material y creación de Hoja de Éter.

### Flujo y soporte
Tutorial contextual, diálogos, cinematográficas, feedback, tareas offline, respawn de nodos, progreso Bastión I→X, resumen/encuesta/reset de playtest.

La regresión v0.26.4 falla si un control crítico desaparece, un panel tapa la navegación, un gate se abre antes de tiempo, una pantalla profunda vuelve a modal o el mapa deja de responder al arrastre.
