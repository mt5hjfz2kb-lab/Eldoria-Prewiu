# Valoria PremiumV6 · prueba jugable de rendimiento WebGL

Este paquete es una compilación **auténtica de Unity 6000.3**, preparada para comparar comportamiento en un iPhone. Solo contiene la escena aislada Ciudad2D V6. **No** usa partidas reales, economía, combate ni sincronización. No sustituye la beta y no se publica automáticamente.

## Abrir en el ordenador y en el iPhone

1. Descarga el artefacto de GitHub Actions **ciudad2d-v6-webperf-playable-...**, descomprime la carpeta completa manteniendo `index.html`, `Build/` y `TemplateData/` juntos.
2. Instala Python 3 si el PC no lo tiene; abre una terminal/PowerShell en la carpeta donde aparece `index.html`.
3. Ejecuta `python serve.py` (o `py -3 serve.py` en Windows). El servidor escucha en el puerto **8765** de tu red local.
4. Abre en el PC `http://localhost:8765/`. Para abrir en iPhone, conecta ambos dispositivos a la **misma red Wi-Fi** y escribe `http://IP_DE_TU_PC:8765/` en Safari. Puedes obtener la IP local del PC con `ipconfig`. Si el cortafuegos lo pregunta, limita el permiso a **redes privadas** y desactívalo al terminar.
5. Cuando termines, detén el servidor con Ctrl+C. No redirijas el puerto 8765 a internet ni publiques este archivo como beta sin autorización.

**No abras `index.html` como un fichero local** (`file://`): WebAssembly necesita servirse mediante HTTP(S).

## Interacción y diagnóstico

- Toca **I, II, III, IV, V** en la interfaz para ver los estados ilustrados. Son escenarios simulados **solo en memoria**; no hay compras ni niveles reales.
- Selecciona edificios con un toque; desplaza con un dedo y haz pellizco con dos dedos. En PC, zoom con rueda y desplaza con botón derecho/central.
- La demo abre en **Bastión V**; la Cantera aparecerá como parcela disponible según las reglas reales. El botón **«Cantera QA: verla construida»** muestra visualmente su estado terminado **solo en esta prueba**, sin modificar guardados.
- Un pequeño panel indica FPS medios de ventanas de 5 segundos, el fotograma más lento y cuántos fotogramas superan 50 o 100 ms.
- Si aparece el icono **Diagnostics** de Unity, úsalo para consultar sus métricas adicionales. Safari puede no ofrecer la memoria JavaScript; eso **no implica** que el consumo sea cero.

## Qué registrar al probar en iPhone físico

Anota modelo/iOS, navegador, primera carga, FPS sin tocar nada, FPS desplazando la cámara, FPS haciendo zoom, FPS con Cantera QA construida, tiempo antes de una posible recarga y si hubo calentamiento/cierre inesperado. Repite con retrato y apaisado, y realiza una prueba sin tocar durante 2 minutos.

### Interpretación estricta

Esta compilación usa **recursos WebGL sin compresión** para que el servidor local no necesite ajustes especiales. La descarga será mayor que en una distribución optimizada, por lo que el tiempo de carga no representa el objetivo comercial. Los FPS y cuelgues observados en **tu iPhone real** sí ofrecen indicios útiles de su rendimiento en Safari, pero **no equivalen** a los de una app iOS nativa. Una prueba de Unity en Windows o un navegador de CI tampoco acredita el rendimiento físico del iPhone.

La ejecución y el ZIP son artefactos temporales; conserva una copia si deseas repetir la misma prueba. No usar para testers externos ni promocionar a `main` sin aprobación.
