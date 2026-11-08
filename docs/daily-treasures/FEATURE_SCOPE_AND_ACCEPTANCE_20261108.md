# Tesoros de Valoria · alcance integral del encargo

**Propósito:** aumentar la satisfacción y el retorno voluntario al juego mediante pequeñas recompensas diarias y **momentos memorables en hitos auténticos**, sin añadir una economía paralela ni una mecánica de pago o azar. Calidad visual mínima: la referencia del cofre ilustrado aprobada por el propietario. El comportamiento de Unity aún no se ha certificado.

## Matriz del alcance completo

| Necesidad inicial | Estado en PR #181 | Criterio de cierre |
|---|---|---|
| Siete premios diarios progresivos | Implementado en dominio y gateway | Conceder una vez por día UTC, reiniciar ciclo tras siete reclamaciones válidas |
| Rachas de fidelidad | Implementado; +25 comida en múltiplos de tres, +80 en múltiplos de siete | Perder una entrada rompe solo la racha, no los premios ya acumulados; próximo extra visible |
| Control del calendario y fallo de reclamación | Implementado | UI indica premio de hoy, recogidos, próximo extra, error; cambia de día con pantalla abierta |
| Cofre verdaderamente emocionante | Código con dos Sprite originales, marcos, cuatro iconos, brillo, brasas y sonido temporal | Igualar o superar la previsualización dentro de Unity, sin flashes que oculten el contenido |
| Entrega antes de la animación | Implementado con comando del gateway | Saltar, cerrar, recargar y releer correo no duplican recompensas |
| Buzón tras premios diarios | Implementado: mensaje de categoría `recompensas` | Mensaje informativo y borrable; no debe pagar al abrir ni al borrar |
| Victorias especiales | Ceremonia no pagadora al cerrar un capítulo real de Vigilantes | Una celebración por capítulo, informes exactos, no cada victoria |
| Hallazgos importantes | Primera familia nueva de exploradores celebrada con narración y recursos reales | Máximo una ceremonia por familia descubierta, no cada ruta repetitiva |
| Construcciones importantes | Mensajes de Bastión II/V + cola visual tras el avance confirmado | Nunca celebrar obra pendiente ni repetir por reinicio o lectura del correo |
| Primer héroe importante | Mensaje único «Lyra se une a Valoria» y ceremonia visual en cola, después del reclutamiento real | No generar copias del héroe, botín adicional ni fingir que la obtiene antes de terminar su misión |
| Recompensas para cartas legendarias y Duelo de Reliquias | **Excluido deliberadamente del corte Bastión I–V**: su adjudicación todavía no forma parte del recorrido de testers | Solo conectar cuando existan contratos de juego y arte reales |
| Economía equilibrada | Simulación de presupuesto provisional 7/14 días en perfil OWNER_I_II | Importes sujetos a datos de testers; no confundir QA_FAST con economía de jugadores |
| Rendimiento y experiencia móvil | QA responsiva no-Unity; herramienta de capturas nativas preparada | 16 capturas (4 × calendario, cofre cerrado, apertura central, cofre abierto), 16 clics auténticos, pruebas iOS/Android/WebGL |
| Integración con Vigilia de la Brecha | Integración en RoyalGold v3 bajo PR #181, separación intencional de PR #173 | Conectar a HUD final sin duplicar pantallas ni introducir imágenes de pantalla completa pegadas |

## Reglas de negocio que no se negocian
1. No obligar al jugador a entrar todos los días; los siete premios acumulados continúan donde se quedaron aunque se pierda la racha.
2. El cierre de día se calcula en UTC y se documenta; el reloj es local hasta disponer de una hora de servidor fiable.
3. El comando del `LocalGateway` decide los premios y registra el buzón **en el mismo guardado**. Los componentes gráficos no llaman a `GameCommand` salvo el botón explícito «Reclamar».
4. No regalar cartas, poderes, equipamiento ni heroínas no implementados. Una ceremonia visual no equivale a botín nuevo.
5. No activar ceremonia grande en cada actividad ordinaria. Se reserva para día 4/7, cierre verdadero de capítulo, nuevas familias descubiertas, Bastión II/V y Lyra auténtica. Se pueden omitir en condiciones de prioridad narrativa.
6. Ningún botón importante más pequeño de 44 px en los tamaños aprobados. El HUD debe compartir el `EventSystem`, respetar Safe Area y conservar una opción «Saltar».
7. El QA de código/artefactos PNG y el éxito en Github Actions **no equivalen** a calidad final. Nunca fusionar sin ver fotos nativas y probar la partida.

## Cómo se comprueba antes de entrar en main
- **Código:** ejecutar `daily-treasures-domain.yml`, incluyendo calendario, gateway real, cola de hitos, economía OWNER_I_II, regresión de capacidad de marchas, metadatos y geometrías.
- **Unity:** usar `daily-treasures-native.yml` solo tras liberación/cambio de propietario del recurso Windows, exigencia de SHA idéntica, 16 vistas PNG y 16 eventos de puntero (entrada, reclamación, salto, cierre). Captura de componente ≠ validación de ciudad completa.
- **Juego real:** sesiones Bastión I–V con tutorial normal, construcción, marcha, 3 marchas, exploración, Vigilantes, héroes, guardado/reinicio, buzón; comparar con la beta antes de publicar.
- **Arte:** revisión humana de relieve, contraste, lectura, ritmo, sonido y coherencia con Vigilia. Si el cofre ilustrado no se ve tan bien como la referencia fuera de Unity, rehacer la solución, no maquillar defectos.
- **Medición posterior y voluntaria:** oportunidades reales de reclamar, reclamaciones válidas/duplicadas, tiempo hasta comprensión de premio, tasa de salto de ceremonia, finalización de capítulo, descubrimientos distintos, retención D1/D3/D7 (con consentimiento y sin datos sensibles). Ninguna retención está demostrada todavía.

**Estado en esta rama:** trabajo técnico y recursos artísticos implementados, dominio y pruebas fuente verificables; pendiente validación nativa y ajuste del balance antes de fusionar/publicar.
