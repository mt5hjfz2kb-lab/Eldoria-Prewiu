# Corona de la Brecha — ajuste real de HUD tras capturas WebGL (20-11-2026)

Fuentes: [captura y pruebas Chromium 44802943006](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44802943006), `sourceSha=70444c18217d3a519f313c840e9fb3b13f2fa033`, capturas 390×844 de pantalla de ciudad, panel Aserradero y comando de construcción. Es una revisión anterior a las últimas ilustraciones premium: estos hallazgos son de **geometría/interacción**, no una aceptación final del arte más reciente.

**Problemas reales observados:** la placa dinámica `CONSTRUIR ASERRADERO` usaba una imagen turquesa extremadamente estirada que competía con el botón principal; el título VALORIA invadía la joyería del sello; abrir el panel podía dejar controles de navegación por debajo en horizontal; el feedback breve se situaba sobre la segunda fila de objetivos. El problema del test que ignoraba un gasto real 230→150 se documenta por separado en `CORONA_BROWSER_ROI_CAUSAL_FIX_20261120.md`.

**Correcciones implementadas solo en `Modular25DInterface.cs`:**
- Placas sobre los edificios con ilustración de obsidiana `world_rune` y sus propios iconos originales (`city`, `hammer`); etiqueta de Aserradero reducida a `ASERRADERO` en vez de instrucciones extensas. Altura pulsable del Aserradero aumentada a 44 pt.
- La imagen turquesa `action_arcane` queda reservada para la acción real `INICIAR OBRA` del panel, respetando estados del dominio.
- `OpenSawmill`, `OpenStatus` y `ClosePanels` ocultan/restablecen dock y objetivo mientras hay un panel contextual, previniendo pulsaciones debajo del panel y solapamiento en 844×390.
- Emblema superior reajustado; avisos temporales trasladados bajo los recursos y objetivo, sin alterar la cámara ni la escena.
- Tests PlayMode comprueban dibujo del distintivo, 44 pt, ocultación y recuperación de botones, y construcción canónica/guardado.

**Sin tocar:** `LocalGateway`, `GameCommand`, coste, reloj, guardado, unidades, edificios, rutas y `main`.

**Aún no validado:** importación Unity y captura WebGL del nuevo SHA, arte final en tamaño móvil, iPhone real, FPS/GC, render 390×844 y horizontal. No certificar arte comercial por tests verdes.
