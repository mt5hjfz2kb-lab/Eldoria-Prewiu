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
| Rendimiento y experiencia móvil | QA responsiva no-Unity; herramienta de capturas nativas preparada | 52 capturas originales (cuatro tamaños, dos días de premios reales y cuatro acontecimientos), 56 clics auténticos, 8 premios vía gateway, pruebas iOS/Android/WebGL |
| Integración con Vigilia de la Brecha | Integración en RoyalGold v3 bajo PR #181, separación intencional de PR #173 | Conectar a HUD final sin duplicar pantallas ni introducir imágenes de pantalla completa pegadas |

## Últimas coberturas incorporadas para evitar olvidar parte del encargo

- **Exploradores:** el primer informe de cada una de las cuatro familias originales puede activar la ceremonia especial. Los informes repetidos conservan su presentación breve; no se recalcula ni repaga un botín.
- **Bastión:** al completar realmente II o V, `LocalGateway.Advance` registra una carta narrativa única y la cola visual detecta el cambio de nivel si ocurrió durante la sesión. Si ocurrió sin la aplicación abierta, no reproduce una animación fuera de contexto: el mensaje permanece en el buzón.
- **Lyra:** la primera incorporación real en la misión de la fisura crea un mensaje narrativo único. La cola de hitos distingue reclutamiento nuevo de héroe ya existente y no emite otro premio.
- **Racha:** el calendario muestra ahora el próximo bonus y su cuenta atrás; después de un día perdido refleja que la racha terminó, pero no elimina los siete premios ya acumulados. Si el calendario sigue abierto durante un cambio de fecha UTC, actualiza el estado sin tener que cerrar la pantalla.
- **Comprobación visual nativa:** prueba preparada para **52 PNG originales de Unity** (medallón, días 4/7, vuelos de premios y 16 fotogramas de cuatro estilos artísticos), **56 eventos táctiles reales** y **8 concesiones auténticas** con gateway aislado. Son requisitos pendientes; no son capturas ya tomadas.
- **Regresión transversal:** `tools/march-capacity/Smoke.csproj` y el workflow de recompensas compilan juntos el estado guardado añadido, evitando que la incorporación del tributo rompa la capacidad de tres marchas.
- **Balance provisional:** prueba automática del perfil OWNER_I_II, no QA_FAST. Los primeros 7 días seguidos dan 330 madera, 300 piedra, 230 comida y 8 minutos de aceleradores; las recompensas no están aprobadas económicamente hasta medir su impacto en las pruebas reales.

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
- **Unity:** usar `daily-treasures-native.yml` solo tras liberación/cambio de propietario del recurso Windows, exigencia de SHA idéntica, 52 vistas PNG y 56 eventos de puntero (entrada, reclamación, salto, cierre y eventos visuales, incluido Canvas independiente). Captura de componente ≠ validación de ciudad completa.
- **Juego real:** sesiones Bastión I–V con tutorial normal, construcción, marcha, 3 marchas, exploración, Vigilantes, héroes, guardado/reinicio, buzón; comparar con la beta antes de publicar.
- **Arte:** revisión humana de relieve, contraste, lectura, ritmo, sonido y coherencia con Vigilia. Si el cofre ilustrado no se ve tan bien como la referencia fuera de Unity, rehacer la solución, no maquillar defectos.
- **Medición posterior y voluntaria:** oportunidades reales de reclamar, reclamaciones válidas/duplicadas, tiempo hasta comprensión de premio, tasa de salto de ceremonia, finalización de capítulo, descubrimientos distintos, retención D1/D3/D7 (con consentimiento y sin datos sensibles). Ninguna retención está demostrada todavía.

**Estado en esta rama:** trabajo técnico y recursos artísticos implementados, dominio y pruebas fuente verificables; pendiente validación nativa y ajuste del balance antes de fusionar/publicar.

## Escenas de categoría visualmente independientes — ampliación premium

La solución original de «mismo cofre y distinto tinte» ha sido sustituida por una composición de escenario **por acontecimiento**, calculada en `DailyTreasureStageDirection` y conectada al panel:

- **Lyra:** hall pictórico auténtico (sin retrato inventado de Lyra), categoría heráldica propia. Si la interfaz canónica de héroes está mostrando su modelo 3D autorizado, la cola aplaza el cofre hasta que termine esa presentación, para no tapar ni suplantar el personaje real.
- **Exploradores:** paisaje/cartografía original de Eldoria y composición independiente; en horizontal la escena domina la mitad izquierda, y el cofre y su mensaje ocupan la derecha.
- **Vigilantes:** panorama original, luz de capítulo distinta y mismo principio de escenografía real sin alterar el premio de combate.
- **Bastión II/V:** escenografía real de la ciudad, visible a través del marco y fondo semitransparente; no sustituye el Bastión construido por una ilustración ficticia de otro nivel.

En vertical, pintura de escenario y cofre se superponen con jerarquía de profundidad; en horizontal se separan para aprovechar el formato apaisado. Las rutas originales se verifican por API de Unity en la futura prueba nativa y su movimiento parallax usa recorte UV acotado, sin repetir texturas. La legibilidad de los textos narrativos se ajusta en un rango limitado, y la geometría se comprueba con pruebas de C# puro en ocho dispositivos. **No declarar «acabado premium logrado» hasta ver las imágenes reales de Unity y aprobarlas artísticamente.**
