# La Senda de la Brecha — microevento por carriles (vertical slice)

**Estado:** prototipo independiente **jugable en navegador**, con arte vectorial/procedimental provisional. **NO** es una escena Unity integrada, una nueva beta ni un evento publicado.

**Objetivo:** comprobar si el bucle de elegir carril → recibir una ventaja real → contemplar un choque corto → volver a decidir genera interés suficiente para ampliar el evento sin alterar el combate estratégico de Eldoria.

## Probarlo sin instalar nada

Abrir `dist/senda-brecha.html` con un navegador moderno. Es un único HTML autónomo: sin red, servidores, compras, créditos, analíticas ni dependencia del proyecto Unity. En un móvil compatible, descargarlo y abrirlo en un navegador que permita ejecutar archivos HTML locales; alternativamente usar un servidor web local. **No** se ha probado en Safari de iPhone físico.

- Iniciar expedición. Seleccionar un carril tocando uno de los tres portales, los controles inferiores o el camino. En escritorio también funcionan flechas, A/D y 1/2/3.
- Cada encuentro tiene una elección bloqueada al cruzar el portal, un enfrentamiento automático y consecuencias observables en el HUD.
- El camino consta de 6 encuentros y un Guardián final; se gana o pierde y puede reintentarse inmediatamente.
- Pausa automática al cambiar de pestaña; botón de pausa/reanudar. No existe compra, mochila o entrega de tropas real.

## Archivos

- `src/rules.js`: motor determinista puro, seis oleadas, puertas, fuerza, bajas, escudos, informes y resultados.
- `src/game.js`: controles táctiles, HUD, temporizador, animación Canvas 2D, pantalla de resultado.
- `src/styles.css`, `index.html`: lenguaje visual provisional negro/cobre/violeta inspirado en Eldoria, sin reutilizar recursos de terceros.
- `scripts-build.mjs`: genera un HTML único sin recursos externos.
- `tests/rules.test.js`: tests de dominio, incluidos 729 caminos posibles.
- `tests/ui_smoke.py`: navegador Chromium real en 390×844, 320×700, 844×390 y 1280×800; juego, derrota, pausa, reinicio, interacción de carriles y capturas. Requiere Playwright Python y Chromium local.

## Reproducir

```sh
node --test tests/rules.test.js
node scripts-build.mjs
python tests/ui_smoke.py
```

El test de UI inyecta solo la marca `__sendaQA` en la copia en memoria de HTML. La simulación rápida de pruebas usa **el mismo** bucle de avance de la partida normal y `playWave`, no un resultado pregrabado.

## Restricciones deliberadas

- No se ha conectado a GameState, LocalGateway, correo, economía, PR de interfaz, assets de Unity, partidas existentes ni canal de testers.
- No se han generado modelos 3D, usado créditos, descargado arte externo ni publicado una página web.
- Las pérdidas son exclusivamente ficticias y existen solo dentro del minijuego.
- La adaptación Unity real exigirá escena y componentes propios, modelo C# equivalente, verificación A/B, controles táctiles y pruebas físicas iPhone/Android.

Contrato de diseño: `docs/events/senda-brecha/VERTICAL_SLICE.md`.
