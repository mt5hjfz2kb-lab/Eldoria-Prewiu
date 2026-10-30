# Eldoria — contrato de corrección de la experiencia native281 (22-10-2026)

> **Registro histórico del 22-10-2026; diagnóstico opcional reconciliado el 30-10-2026. QA físico iPhone NO realizado.** Orden del propietario: corregir los reinicios repetidos, la interfaz V3 discordante y un mapa que no parece un mundo 4X, antes de continuar a Bastión V. Trabajo acotado a Bastión I–II, 0 €, sin eliminar ni reiniciar la partida familiar.

## Fuente de verdad y discrepancia de publicación

- En la prueba del **22-10-2026** el propietario **vio "281" en la página jugada**. Esta observación es prioritaria para reproducir los fallos. Es una etiqueta visible, **no** un hash verificado de todo el paquete servido.
- El registro histórico en `pipeline/playtest-delivery.json` indicaba native245 confirmada y native281 subida en procesamiento (build remoto 2180594); puede estar retrasado. Si se comprueba el servidor usar **solo consulta remota de lectura**, nunca segunda subida de 281.
- La 281 pasó Unity native run `39678224055` y Chromium WebGL run `39683431955`. En ese entorno funcionaron I–II, entrenamiento ~600 s, mando táctil simulado/ratón, navegar por mundo y restaurar guardado con hash estable. **No se demostró estabilidad continuada en iPhone Safari, acceso Restricted alojado, aprobación estética, ni 10/10 artístico**.
- Tickets relacionados: [#95](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95) para experiencia visual y [#96](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/96) para los reinicios (P0).

## P0 — reinicios durante la partida: diagnóstico antes de corrección

**Hecho confirmado:** el jugador sufre reinicios intermitentes mientras juega en la página; al regresar conserva aparentemente el progreso y la ubicación. Que el guardado sobreviva una recarga deliberada no demuestra estabilidad de la sesión.

**Clases de fallo que distinguir, sin presuponer causa:**
1. Recarga de **documento/iframe** (nueva navegación `performance.navigation`, `pagehide/pageshow`) frente a reinicio de **Unity sin recarga**.
2. Pérdida de contexto WebGL y posible recuperación frente a cierre del proceso web por presión de memoria.
3. Errores JS/promesas, fallos en transportes/split gzip, retorno de background o bloqueo por la capa de alojamiento. Distinguir iPhone Safari de navegador embebido y Chromium Windows.

**Primer cambio instrumental autorizado para candidata aislada:** diario local de eventos, de activación explícita, **sin captura de guardados ni identificadores personales ni envío de telemetría**. Campos estrictos: revisión, milisegundos desde apertura, tipo de navegación, visibilidad, pagehide/pageshow, contextlost/restored, errores clasificados, orientación, marcador ciudad/mundo si ya existe fuente no invasiva. Registro circular pequeño, sin escritura en cada frame, separado de las claves de `LocalGateway`. Comprobar que la instrumentación no introduce recargas ni degradación significativa. Cualquier corrección de memoria/código/hosting exige evidencia de una clase de fallo concreta.

**Criterio de salida:** sesión **≥30 min** en iPhone Safari real y una segunda pasada en navegador integrado si es accesible; transiciones ciudad ↔ mundo, zoom/pan, entrenamiento, marchas, cambios de orientación y segundo plano, **cero reinicios involuntarios**; recarga controlada conserva estado sin duplicar recursos/tiempos/recompensas. Guardados del propietario intactos. Si no hay acceso a iPhone real, declarar **PENDIENTE QA FÍSICO** y no dar por resuelto.

## P1 — mapa 4X: escala territorial y significado, no «zoom» sin más

**Código canónico confirmado:** `ValoriaIntegratedWorld.cs` configura `regionalZoom=32`, `localZoom=14`, y en primer acceso ejecuta `PanTo(WorldHome)`, `SetZoomLevel(1,true)`, `FitToAuthoredBounds`; `ZoneOneStrategicCamera.cs` limita visión con aspecto, inclinación y límites reales. Existe semántica de celdas/chunks en `ZoneOneWorld.cs`, pero **que haya una estructura de datos NO demuestra que todos esos nodos aparezcan o sean jugables en la escena publicada**.

**Objetivo observable:** al entrar desde la ciudad, lectura inmediata de **territorio estratégico**, no simplemente una ciudad reducida o campo con árboles dispersos. Distancia inicial, posición focal, escala relativa, ocupación útil de pantalla, señalización/jerarquía de POIs reales y variedad de terreno han de funcionar **juntas**. Conservar zoom local voluntario, 4 direcciones, límites válidos, selección táctil de destinos y vuelta a ciudad.

**Prohibiciones:** no agregar por intuición ciudades, campamentos, economías de recolección nuevas o cuatro parcelas eliminadas; no convertir árboles decorativos en nodos jugables falsos. Solo mostrar como interactivos destinos/recursos respaldados por contenido y reglas existentes. La representación del **Engendro** y sus hotspots debe seguir siendo perceptible a la escala estratégica; enlaza #95.

**Evidencia exigida antes de aceptar:** captura **A/B desde el mismo punto** de salida ciudad→mundo y otras 2 vistas de navegación, en **horizontal y vertical**, con medición de proporción visible de territorio, tamaños en píxeles de POIs importantes y número de POIs reales legibles/interactuables; grabación de seleccionarlos; comparación visual independiente. Ni subir `globalZoom` ni renderizar más árboles demuestra por sí solo mejora 4X.

## P1 — interfaz V3: recuperar exactamente la opción del propietario

Referencia vinculante: `docs/ui-v3/OWNER_APPROVED_DIRECTION.md`. En **ciudad y mundo**, propuesta 1: tinta azul/negro y oro envejecido, menos marcos, escena protagonista; diálogo normal propuesta 1 cinematográfica; efectos de Fisura propuesta 3 solo si evento real; Orín excepcional. No modificar la selección.

**Código vigente:** controles contextuales `RoyalGoldV3ContextHud.cs` unidos a anclas proyectadas de los edificios; ajuste de tarjeta hasta **290 unidades** y exclusiones de cabecera diferentes horizontal/vertical. Gestión `RoyalGoldV3ManagementScreens.cs` genera **pantallas opacas** de Entrenamiento y Marchas, con `Layout` dinámico y botones de retorno. Es necesario comparar **lo que se renderiza realmente** y cuánto ocupa, no inferir fidelidad artística por la existencia de esas clases.

**Objetivo observable:**
- Ciudad despejada, recursos/objetivo/navegación compactos, sin ficha gigante ni elementos que tapen paisaje o edificio seleccionado.
- `Construir/Mejorar` y cada contador real legibles, **junto al edificio correspondiente** cuando proceda, sin paneles duplicados ni botones falsos; cerrar deselecciona.
- Entrenamiento y Marchas como **pantallas independientes visualmente**, opacas y legibles, solo al abrir esa gestión; cantidad de Arqueros T1 reales, costes, cronómetros auténticos, marcha parcial de Sir Aldric y regreso correcto, sin alterar Gateway/guardados.
- Contraste, tipografía, áreas táctiles y safe area en horizontal y vertical, con textos largos; transiciones sin navegación táctil heredada.

**Evidencia exigida:** siete pares de imágenes A/B en cámaras/safe areas iguales: ciudad sin selección; ciudad con edificio seleccionable; temporizador activo; mundo abierto; diálogo cinematográfico; entrenamiento; preparación de marcha. Anotar % del frame oculto por HUD, rectángulos de controles y posiciones proyectadas, legibilidad y éxito del toque en dispositivo. **El propietario debe poder aprobar visualmente la versión**, no solamente el CI.

## Instrumentación entregada en rama de revisión (NO publicada)

- Código: `tools/delivery/optional-iphone-lifecycle-diagnostics.js`; integrado **solo para futuros paquetes** en `tools/delivery/prepare.py`.
- Se mantiene **inactivo por defecto**. `?eldoria_diag=1` en la URL del marco real de juego habilita botón local **Diagnóstico** y diario circular de hasta 48 eventos; `?eldoria_diag=0` lo desactiva. El flag queda en almacenamiento local del origen, sin usar el guardado del juego; el botón permite consultar eventos anteriores tras una recarga en el mismo origen. Si itch.io no transmite parámetros al iframe, hay que instrumentar el marco real en una compilación de QA; no asumir que la query externa llega al contenido.
- No recopila contenido del error ni URL, solo categorías `memory`, `graphics`, `network` u `other`. No transfiere datos fuera del navegador.
- Test automatizado reutilizable: `node tools/delivery/test-optional-iphone-lifecycle-diagnostics.mjs`. Comprobación JavaScript aislada en V8: sintaxis, arranque detectado como reload, error clasificado sin texto sensible, contextlost/pagehide, límite de 48 eventos, exclusión explícita: **PASS**. La prueba Node/Unity/Chrome y **una prueba física del teléfono** siguen pendientes. La nueva instrumentación nunca se ha ejecutado sobre la 281 ya publicada; no se realiza otra subida.

## Orden de integración sin interferencias

1. **Congelar la referencia exacta de la 281** (hash fuente/carga/asset si es verificable, etiqueta observada por propietario). Preservar `main` y recursos de los workstreams vigentes; no alterar el hosting para investigar la página.
2. Instrumentar/probar P0 en rama de revisión; reproducir en teléfono y aislar causa. La corrección definitiva de P0 tiene prioridad sobre los cambios artísticos de rendimiento intensivo.
3. Preparar variantes puntuales de cámara/composición del mapa y maquetación V3 sobre la escena canónica, siempre A/B real; **no fusionar prototipos históricos enteros** ni nuevas escenas/guardados.
4. Validar interacción y jugabilidad I–II sin interrupciones, crash, resets involuntarios ni regresión de persistencia. Ejecutar Unity 6000.3.23f1 EditMode/PlayMode + WebGL Chromium usando el **mismo SHA** que las capturas.
5. Confirmar iPhone físico (y navegador embebido por separado), luego publicar **una sola candidata** en canal Restricted usando el circuito y hashes verificados, con confirmación real de activación. No confundir aprobación visual, tests técnicos, build activo y juego alojado certificado.

**No marcar estas tareas como terminadas** hasta documentar pruebas móviles y aceptación estética. No iniciar Bastión V sobre una base inestable.

## Continuación del 30-10-2026

La consolidación final del propietario sustituye la antigua restricción de detenerse en Bastión II. Región 1 y la paridad I–V ya están integradas en main. Esta PR conserva el diagnóstico local, inactivo por defecto, y lo incorpora al preparador WebGL actual sin cambiar los bytes del motor Unity ni las partidas.

Pruebas locales: activación explícita, persistencia tras recarga, límite de 48 eventos, exclusión de mensajes/URL de errores y desactivación aprobadas; preparador Python compila. La prueba de transporte exige un paquete WebGL preparado y se ejecutará al preparar la candidata; no se afirma PASS sin ese paquete. No se ha medido aún una sesión física de Safari de 30 minutos ni se ha identificado con certeza la causa de los reinicios. La incidencia #96 permanece pendiente.
