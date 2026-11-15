# La Senda de la Brecha — prueba jugable aislada

**Estado:** prototipo HTML5 determinista, no integrado en Unity ni publicado en la beta I–V. 15/11/2026.

El propietario quiere explorar un evento *opcional* inspirado por lo adictivo de elegir carriles y ver consecuencias inmediatas. No sustituye el combate principal de Eldoria, no reproduce nombres/recursos del juego comercial aportado y no implica monetización.

## Jugar

- Abrir el artefacto `senda-brecha-standalone.html` construido con `node build-standalone.js`. Funciona como un único archivo sin bibliotecas ni peticiones externas. La fuente es `index.html`, `style.css`, `engine.js`, `app.js`.
- Control: pulsar las cartas de puerta o izquierda/derecha, deslizar sobre el escenario, o usar flechas/A/D en teclado. Durante los combates el control de carril está desactivado porque la decisión ya se ha aplicado.
- Tres tramos, dos decisiones por tramo, dos encuentros y jefe. El jugador comienza con 16 infantes + 6 arqueros. T1–T5 y la economía real **no** se modifican. Los premios mostrados son ficticios y no se guardan.

## Pruebas reproducibles

```bash
node tools/senda-brecha/test-engine.js
node tools/senda-brecha/build-standalone.js tools/senda-brecha/senda-brecha-standalone.html
python tools/senda-brecha/test-browser.py
```

La prueba visual requiere `playwright` y Chromium instalado. Usa `page.set_content` para ejecutar el HTML aislado porque algunas máquinas bloquean `file:` en navegadores automatizados. No usa cuentas ni WebGL de Eldoria. Las capturas de la prueba son locales a `qa/` y no constituyen captura nativa de Unity. La API `window.__sendaTest` existe **solo** en este prototipo sin recompensas reales, para prueba automatizada.

Ver `docs/events/senda-brecha/IMPLEMENTATION_PLAN.md` para la adaptación futura a Unity, decisión artística y criterios de entrega. No introducir esta prueba en el flujo principal ni en iPhone WebGL antes de demostrar su presupuesto real de memoria y estabilidad.
