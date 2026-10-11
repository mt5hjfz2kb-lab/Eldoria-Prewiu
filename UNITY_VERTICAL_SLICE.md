# Primera vertical slice Unity — «Una ruta desde Valoria»

**Finalidad:** ver y tocar Eldoria en móvil cuanto antes y validar identidad/legibilidad del bucle; no completar Bastión I–X de golpe. Es un corte propuesto del producto web v0.32.0, no un nuevo capítulo ni una promesa de online.

## Recorrido jugable mínimo

1. **Valoria al amanecer después de La Brecha:** cámara isométrica controlable; Bastión, Aserradero dañado, puerta, huella violeta distante. Un intercambio breve Aldric explica una necesidad concreta: madera para levantar el Aserradero y asegurar la ruta.
2. **Reconstrucción visible:** seleccionar Aserradero, ver coste disponible/requisito, iniciar una mejora corta; recurso descontado una vez, estado de obra y silueta reparada perceptibles. Evitar bloquear el primer viaje si la economía aún no alcanza: recolectar permite pagarla.
3. **Salida al mundo:** abrir la puerta, encuadre de frontera con camino, bosque agotable, cantera y la marca de corrupción; el jugador elige el bosque y envía una Marcha real de Aldric + arqueros. El movimiento tiene origen, destino y retorno visualizados.
4. **Recolección y amenaza representativa:** primer viaje recoge una carga y muestra reloj/recompensa; a continuación, una amenaza de la ruta (Corruptos/Engendro inicial del prototipo, según coste de producción) ofrece una previsión comprensible y resolución PvE breve. El encuentro debe mostrar composición, daños/escudo, resultado y **por qué** venció/perdió. Para no inventar una dificultad nueva, usar stats/reglas observables de `v0220/js/gameplay.js` como referencia inicial con fixtures; si el campamento de Bastión I usa la resolución simplificada, documentar la elección de una amenaza de Bastión II como prueba de combate modelado, sin adelantar la progresión narrativa de la build completa.
5. **Regreso y cambio visible:** madera/material obtenido se acredita una sola vez, regreso de la Marcha, completar/elevar el Aserradero, producción visible y una iluminación/actividad de habitantes en el edificio. Mostrar «Valoria creció» junto con el siguiente indicio de La Brecha; guardar/reabrir para confirmar persistencia. Cerrar la slice explícitamente, sin fingir completar Arco I.

Esto cruza **Valoria → necesidad → mundo → recolección/combate → recompensa → regreso → crecimiento** con una sola zona de ciudad, una franja de frontera, 1 edificio mejorable, 1 recurso, 1 encuentro y un héroe. La cantera puede estar presente como contexto/segunda opción de recolección si la calidad del corte lo permite; no crea una red de nodos ni el sistema completo de misiones. Hospital se representa como contrato/test de dominio posterior; no se presenta antes del Heraldo/Bastión X. Cartas, Forja, Maelis, Duelo, rankings y escena de jefe se migran en fases siguientes.

## Identidad visual a producir en esta slice

| Disciplina | Resultado jugable, no solo concepto |
| --- | --- |
| Escala y cámara | Comparar dos encuadres isométricos móviles sobre **la misma escena**; zoom/pan limitado, Bastión y puerta ubicables, bosque legible como acción. Captura en retrato de iPhone real y viewport Android. |
| Entorno/edificios | Kit modular de ruina imperial de piedra, torre/portón, Aserradero antes/después, camino, bosque y roca. Distinción funcional entre objeto accionable y decorado mediante silueta, luz y feedback. |
| Personaje/marcha | Aldric con silueta de guardián, pequeño grupo de arqueros coherente con el roster; animaciones idle, caminar, ataque/impacto y retirada/retorno; no generar héroes adicionales. |
| Brecha/VFX | Grieta distante violeta/magenta, partículas sutiles, alteración local de tierra cerca de amenaza, pulso controlado en combate; no convertir toda Valoria en neón. |
| Luz/audio | Ciudad fría/ruinosa con puntos cálidos de reconstrucción; mundo más expuesto; signo de Brecha frío anómalo. Capa ambiental y 3–5 señales audibles puntuales originales/licenciadas para mejora, marcha, impacto, recompensa. |
| UI/UX | HUD compacto de recursos y Poder, botón Marcha/ir al Mundo, tarjeta de elección en objeto, reporte de combate corto y un panel contextual de coste/resultado. Safe area, targets táctiles, texto ES/EN y señal «siguiente paso» sin tapar juego. |

Dos metas distintas: **calidad visual** (coherencia estilizada semirrealista, ruina vertical habitada, Brecha reconocible) y **claridad jugable** (descubrir, comprender, actuar, feedback, siguiente paso). Aceptar assets de bloqueo solo hasta el primer *art spike*; no cerrar cámara/escala basándose en una ilustración estática.

## Dependencias y orden de entrega

**VS0 — base técnica:** versión/paquetes bloqueados, proyecto abre sin errores, Domain/Application y tests de wallet/tarea/marcha/save; escoger dos dispositivos de prueba. **VS1 — greybox navegable:** Valoria ↔ frontera, cámara/tap/puerta, ruta y vuelta; validación móvil. **VS2 — verticalidad jugable:** reconstrucción, coste, timer, recolección, combate modelado y reward único; carga de save. **VS3 — identidad:** kit de Valoria y bosque, Aldric/arquero, Brecha, VFX, animación, UI y audio; prueba de comprensión con jugador sin ayuda. Se trabaja por iteraciones pequeñas de cámara+arte sobre el mismo recorrido, no por decenas de escenas.

### Criterios de aceptación

- Partida limpia comienza en Valoria, jugador detecta necesidad y logra viaje, amenaza/recompensa, regreso y mejora **mediante UI** sin consola ni presets.
- Débito y recompensa ocurren exactamente una vez; cerrar/abrir durante tarea y tras la recompensa conserva estado, no duplica unidad ni recurso.
- Combate y reporte dicen qué estadística/rasgo/decisión influyó; un fallo deja una ruta de recuperación; vista Marcha se corresponde con composición efectiva y heridos nunca participan.
- Una build móvil instalada presenta escena, objetivos y botones dentro de safe areas, sin panel bloqueante, input perdido ni pérdida de fotogramas que impida jugar; establecer objetivos cuantitativos después del primer perfilado en dispositivos objetivo.
- Una persona sin instrucción externa describe qué hizo, por qué mejoró Valoria y qué representa La Brecha. Comparar con referencia web sin exigir pixel parity ni inventar online.

**Fuentes concretas:** `v0220/js/gameplay.js` capítulo I, costes/enemigo común; `v0220/js/state-economy.js`, `tasks-offline.js`, `hero-army.js`; `v0220/index.html` `kingdom`, `world`, `gather`, `openMarchPrep0266`, `resolveLayeredCombat0266` y Aldric; arte en `docs/MVP_ART_DIRECTION.md`. Usar IDs de misión y nombre como referencia, reconciliar la divergencia Marcha/combate indicada en `UNITY_READINESS_REPORT.md` antes de certificar combate Unity.
