# Reparación del probe general I–II — 24-10-2026

Run 40057377057, fuente 1d74a842aed946f54ab06877c7d46cf7962a4180, merge probado 80efafcd764a0f6f5670557a213be30a23dcc865: **FAIL**, no certificación general. El probe termina por timeout de 30 minutos; EditMode/PlayMode generales no se ejecutaron.

Artefacto 12546409322, ZIP SHA256 c1558135ffdb95dc1fc2fe30822606a339e3e63f60adf56d4b22e9799b789b96, descargado y verificado. No reutilizar capturas antiguas que el upload pueda recoger del checkout como resultado de esta ejecución. El log nativo confirma:
1. Primer fallo: “First touch must reveal, not skip a speaker”, línea 171 de ValoriaIntegrationBuild.
2. El probe continúa tras ese fallo, omite comandos dentro de Check y alcanza un Need de Engendro fuera de Check (línea 515). La excepción sale de la coroutine, no cierra el Editor y termina en timeout.
3. El proyecto había terminado de cargar en 90,966 segundos; no fue una importación de 30 minutos.

El presentador usa tiempo real a 46 caracteres/s. El probe espera un segundo y presupone que la página aún se revela, sin comprobar esa precondición. La preparación corregida reinicia la MISMA página con SetEnglish(true)/SetEnglish(false), API pública que conserva índice y callback y no escribe el estado del jugador. Exige índice inicial 0, texto idéntico, mismo índice y revelado activo ANTES del toque. Mantiene raycast y eventos de puntero reales; después exige misma página y revelado terminado. No modifica UI, textos, velocidad ni progresión.

GuardedProbe avanza la coroutine y captura cualquier excepción que escape de ella; escribe la excepción y sale con código 1, sin esperar 30 minutos. No suprime Need, Check ni ninguna prueba. Los pasos posteriores todavía deben pasar nativamente; esta reparación es pendiente de validación, no un PASS anticipado.

El pase artístico posterior está preparado aparte, aún sin enviar. No se cambia el workflow de UI reservado por el otro workstream ni sus archivos de producto.

## Segunda ejecución y navegación
Run 40059877404 sobre d3a5e0fd41add8e99252f8aa1caf26ff1457efb3 (merge 956f7e01a12172596c41c28e7effb0a1c149e5a3) falla en ~84 s de probe, sin timeout. La apertura narrativa ya pasa. Nuevo primer fallo: Vertical map pan blocked. Artefacto 12547261820, ZIP SHA256 0453840dbf79ec632258e75ff30e403e381b7751328d0d52d5d1d50b84ed49b7 según workflow (no descarga independiente aún).

Causa de producto: StrategicEntryZoom usa 82 % del zoom global cuando AllowWholeRegionOverview está activo. La panorámica completa tiene ajuste propio de aspecto/rumbo y puede exceder el rectángulo navegable; FitToAuthoredBounds centra el eje sin margen. Corrección: entrada al 82 % del mínimo entre zoom global y MaximumFittingZoom real, manteniendo la panorámica completa explícita. Las cuatro pruebas de pan, retorno, amplitud estratégica y no escritura de save permanecen intactas.

La segunda aserción mantenía el umbral histórico <=17 tras cambiar el preset local del continente a 36/64. Se exige ahora igualdad al preset local con tolerancia 0,02 y zoom local menor que un cuarto del regional. No se sustituye por un límite arbitrario más alto ni se elimina la comprobación de zoom; se prueba el contrato vigente. Verificación nativa pendiente.

## Tercera ejecución
Run 40060419399, fuente d72b481cdbaa85fed8d160377bd7dfe8502a734c, merge 9515f707b889463a946813df92c1dba3e7323b38: FAIL. El preset local ya pasa; persiste Vertical map pan blocked. La entrada puede heredar el rumbo panorámico de 90 grados, por lo que un arrastre vertical no equivale al eje Z hasta actualizar la cámara. Se restablece el rumbo regional antes de calcular el encuadre de Enter/Home y se registran tamaño, aspecto, rumbo, inclinación, límites y desplazamiento real para contrastarlo en la próxima ejecución. No se elimina ninguna aserción de pan.

## Cuarta ejecución: diagnóstico numérico
Run 40060939669, fuente 145f3664e9b09fc61ffe52845d1bd94d1988c59c, merge 700a17f602340903dcca2374695a5e2be2668ec4: FAIL. Log real: size=552,2701, aspect=1,333333, heading=0, tilt=42,81111, target=(1018,0,30), min=(108,-1770), max=(1928,1830); tras pan vertical el objetivo no cambia. Esto descarta rumbo y margen como causa restante; se conserva el margen de entrada y se descarta atribuirles por sí solos el fallo observado.

ApplyPose cambiaba el tamaño ortográfico inmediatamente, pero cameraDistance permanecía en el valor cercano hasta el próximo LateUpdate del paisaje. GroundPoint usa rayos ortográficos: con zoom 552 y profundidad cercana 140, algunos rayos inferiores nacen debajo del plano y Plane.Raycast no encuentra intersección frontal. Corrección: aplicar inmediatamente la misma receta de profundidad existente (max 140, zoom×2,5) y far clip (max 900, distancia×2,5) en ApplyPose cuando está activa la panorámica de región completa. No cambia cámara de ciudad, geografía, límites ni criterios de pruebas. Añadida comprobación nativa pendiente de cuatro rayos de gesto en el mismo frame para ambas orientaciones, antes de cualquier Refresh/LateUpdate. El siguiente run general debe confirmar el movimiento real.
