# La Senda de la Brecha · contrato de diseño del evento

**Fecha:** 2026-11-15. **Fase demostrada:** primer prototipo web jugable separado del videojuego. **Fase NO demostrada:** integración Unity, arte final, rendimiento WebGL/iPhone, aprobación artística o interés real de testers.

## Por qué existe

El propietario jugó el modo por carriles del juego comercial mostrado en las capturas y un vídeo adjuntos, y destacó que le enganchaba. La decisión de diseño es tomar prestada **la dinámica abstracta de elecciones inmediatas y refuerzo visible**, sin copiar arte, interfaces, personajes, economía, nombres ni código. Eldoria conservará su combate estratégico T1–T5 para expediciones y enfrentamientos ordinarios.

## Bucle vertical

1. El jugador entra al evento con 22 soldados **virtuales**, sin modificar su ejército principal.
2. Cruza un escenario medieval corrompido por la Brecha y elige entre izquierda, centro y derecha.
3. Cada portal ejecuta **una** de cuatro reglas: incorporar soldados, aumentar permanentemente el ataque, recibir escudos consumibles o reducir enemigos del encuentro.
4. El encuentro se resuelve sin intervención manual: cálculo determinista de fuerza, riesgo, daños absorbidos, bajas y victoria/derrota.
5. El HUD refleja el nuevo estado, la ruta sigue al siguiente tramo y el sexto culmina contra el Guardián.
6. Tras ganar o caer, se muestra resultado, pérdidas, supervivientes y puntuación **solo del prototipo**, con reinicio.

El jugador toma decisiones durante el avance, no habilidades manuales en tiempo real durante la batalla. Duración objetivo con temporización actual de **6 × 9,6 s = 57,6 s** de simulación (sin contar menús ni pausas), ajustable tras ensayos reales.

## Motor y equilibrio inicial (candidato, no aprobado)

Seis tramos: El Umbral, Bosque Marchito, El Puente Roto, Las Ruinas, Antes de la Grieta, Guardián de la Brecha. Cada tramo dispone de 3 portal(es) de diferente tipo y de un enemigo con un número y multiplicador fijos. Un ataque insuficiente puede causar derrota incluso si persisten unidades. Los escudos absorben bajas antes de afectar a soldados. Bonus de ataque limitado a +125%, escudos a 50 y ejército a 96 para evitar desbordes.

**Prueba exhaustiva del candidato:** 729 secuencias de tres carriles en seis tramos, 168 victorias y 561 derrotas (23,0% frente a 77,0% en combinaciones uniformes). Este dato demuestra que hay soluciones y riesgo, **NO** que la dificultad se sienta justa, ni predice porcentajes de victoria entre jugadores. La beta posterior deberá medir frustración, curva de aprendizaje y ganas de repetir. Están cubiertos casos de derrota y victoria, pausa, reinicio y validación de carril.

## Dirección visual propia

- Estética de Valoria: camino de piedra, guardia medieval, ruinas, oro apagado, pizarra, cobre y emanaciones violetas de la Brecha.
- UI modular y legible, no botones turquesa/cartas lilas del juego de referencia. Entrada, progreso, tropas/ataque/escudos, tres elecciones y feedback de bajas.
- El prototipo usa únicamente polígonos y gradientes Canvas 2D. Su sencillez actual **no equivale** a la calidad gráfica deseada para el producto.
- **Fase futura:** personajes y criaturas 3D de la biblioteca ya autorizada, sombreado consistente y animaciones ligeras; fondo original que preserve la claridad de carriles. Antes de crear nuevos modelos, consultar el catálogo `tools/asset-vault/find-reuse.py`, procedencia/licencias y el canon artístico. No copiar referencias comerciales.

## Cómo integrar tras aceptación del concepto

**No automatizar el merge del prototipo en el runtime Unity.** Integración propuesta:

1. Crear escena aislada `SendaBrecha` y controlador propio bajo `Unity/Assets/Eldoria/SendaBrecha/`, sin modificar `RoyalHUD`, `BreachWatch`, `LocalGateway` ni mundo 4X durante la prueba conceptual.
2. Traducir las reglas puras de `src/rules.js` a C# con datos versionados; añadir pruebas de paridad por cada ruta y de conservación de unidades virtuales.
3. Sustituir Canvas 2D provisional por una escena URP de cámara fija, sin duplicar el runtime del mundo ni cargar simultáneamente sus texturas pesadas. Pooling y representación limitada de soldados, sin un GameObject por cada unidad virtual.
4. Asegurar gestos de cambio de carril, estados de pausa, touch targets, Safe Area y HUD tanto vertical como horizontal.
5. Definir desbloqueo de evento y recompensas reales **solo después** de decidir su coste/impacto en la economía; si se conectan al correo, utilizar el ledger idempotente existente. Hasta entonces no conceder recursos ni consumir tropas.
6. Certificar Unity EditMode/PlayMode, capturas A/B reales, WebGL/Android y iPhone Safari físico; no usar únicamente una build verde como aprobación visual o estabilidad.

## Criterios del siguiente gate

- El usuario entiende, en menos de un tramo, que cada portal tiene consecuencias tácticas diferentes.
- Cambio de carril y resolución se perciben sin retrasos molestos en dispositivo físico.
- Un intento completo se siente divertido; el feedback explica por qué se pierde y ofrece una nueva decisión significativa.
- Los cuatro tamaños de pantalla son legibles, sin controles ocultos ni texto de vital importancia bajo un panel.
- No aumenta la tasa de cierres del mundo/ciudad ni modifica partidas, comercio, alianzas, recompensas y combate T1–T5.

## Evidencia reproducible

- `node --test tests/rules.test.js`: 5/5 controles de dominio **PASS**, exploración de 729 rutas.
- `python tests/ui_smoke.py`: Chromium real **PASS** en 390×844, 320×700, 844×390, 1280×800: entrada, elección, pausa/reanudación, combate, victoria, derrota y reinicio; cero errores de página registrados. Capturas en carpeta local `evidence/`.
- `node scripts-build.mjs`: genera el archivo único `dist/senda-brecha.html`; no peticiones externas necesarias.

**Limitaciones:** Las pruebas del navegador se realizaron en el contenedor de desarrollo sobre Chromium de escritorio con emulación de tamaño y táctil; no prueban el rendimiento/compatibilidad de Safari real de iPhone. El arte generado es provisional y no aprobado. La escena Unity no ha sido creada. No se han realizado tests con jugadores reales.

## Historial de consolidación

La rama contenía un piloto anterior de **3 tramos y 2 elecciones** en el commit [`3e2a83d`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/3e2a83dbc95f540e7833d02cc5e1023153712ba9), con su motor, UI y pruebas independientes. Esa variante queda conservada íntegramente en el historial de Git, pero fue **sustituida** para evitar dos entradas jugables diferentes en el mismo evento. La implementación canónica de esta PR es la de **6 tramos, 3 carriles** bajo `tools/senda-brecha/src/` y `dist/senda-brecha.html`. Sus pruebas no se suman ni se confunden con las del piloto anterior.
