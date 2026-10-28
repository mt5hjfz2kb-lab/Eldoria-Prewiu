# Eldoria — Primera evolución motivadora de los dos destinos

**Aprobación del propietario:** guardianes diferenciados, informes útiles, relatos de exploración y cambios visibles en el mapa. Sin modelos 3D adicionales, otras monedas, sistemas de cartas o balance de premios final.

## Vigilantes — superación visible

Cada quinto hito presenta un guardián REAL de la progresión (5/10/15/20/25), no una variante cosmética de las mismas estadísticas:

| Hito | Guardián | Fortalezas reales | Pista presentada |
| --- | --- | --- | --- |
| 5 | Guardián de la Coraza | Defensa reforzada | Más ataque y ruptura |
| 10 | Acechador del Umbral | Más ataque, menos defensa | Resistencia/Vida |
| 15 | Engendro de la Brecha | Ruptura y Vida reforzadas | Defensa y Vida |
| 20 | Centinela Inquebrantable | Mucha Vida y defensa | Daño sostenido |
| 25 | Vigía de las Cenizas | Ataque y defensa reforzados | Preparación equilibrada y Poder |

No se altera el combate del mundo 4X ni se inventan criaturas. La función `Stage()` devuelve estadísticas efectivas diferentes, y `SliceRules.FightCreature` resuelve con las estadísticas del héroe/marcha elegidos. Un informe breve pero útil muestra victoria/derrota, rondas, Vida restante en victoria, Poder real y una sugerencia de mejora cuando el guardián resiste. No se recompensa repetir etapas.

**Motivación:** medallones luminosos de tramos superados ya existentes, estandarte del guardián, nuevo aviso táctico corto y presentación de victoria/derrota con el informe auténtico. Pantalla medieval aprobada y enemigo del lore sin cambios de arte.

## Exploradores — un mapa propio, no un temporizador genérico

Cada tipo real de encargo genera uno de **3 relatos diferentes** (12 en total) mediante una selección determinista por periodo UTC y ranura; cambiar de pantalla, cerrar el juego o recargar no cambia la historia. Al recoger un encargo:

- La información concreta aparece en el mismo aviso de éxito (sin nueva pantalla modal) y se conserva el título y el relato asociados al identificador único de ese encargo.
- El mapa muestra un sello dorado por cada una de las **cuatro familias** verdaderamente encontradas. No es un recurso canjeable: es un recuerdo de exploración.
- Aparece la leyenda **HALLAZGOS DEL REINO** y el último informe narrativo, acortado solamente para el espacio pequeño del mapa; el informe completo se presenta al reclamar.
- El motor autoritativo `LocalGateway` sigue otorgando exactamente la misma madera, piedra, comida y XP. Nunca se duplican recompensas por repetir una solicitud de red.

No se inventa información que modifique el PvP ni se desvela contenido de capítulos que aún no estén desbloqueados. Son microhistorias relacionadas con la Brecha, Valoria, patrullas, rescates y materiales recuperados.

## Comprobación de cierre obligatoria

1. CI de dominio: cinco perfiles y estadísticas distintas, pista en cada guardián, doce títulos y relatos independientes, identidad de informe estable tras recarga; tres marchas siguen compartiendo cupo.
2. NUnit real: etapa, combate, informes y recompensas originales sin relajar aserciones; pruebas de nuevos perfiles e informes deterministas.
3. **Unidad Unity nativa:** usar el original `EarlyRetentionNativeCapture.Capture` ampliado. Tras enviar tres encargos y cobrarlos, assert de los sellos reales, resumen visible y dos capturas nuevas `scout-discovery-claimed-horizontal.png` y `scout-discovery-claimed-phone.png`; junto con las 19 capturas originales = 21.
4. Revisión de pantalla real 1600×900, 900×1600, 390×844 y 844×390 sin títulos cortados, sin tapar el mapa ni interceptar botones. El texto táctico debe permitir ajustar la composición sin manipular el cálculo de combate.
5. PR126 está apilada sobre PR124 (capacidad global) mientras ésta no se fusione. No fusionar ni publicar APK antes de QA nativa y reconciliación, ni invadir el editor exclusivo reservado a PR122.

Arte reutilizado: `vigilants-painterly-v2`, `scouts-painterly-v2`, cinco medallones ya presentes, pergaminos y sellos originales del HUD. Los cuatro nuevos sellos se dibujan con triángulos uGUI (sin texturas ni coste adicional).
