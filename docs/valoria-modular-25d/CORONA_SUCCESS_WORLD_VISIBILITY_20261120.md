# Corona — el Bastión debe permanecer visible al comenzar una obra

**Evidencia auténtica**: el navegador Unity de `44811887444` (SHA `16e463c7`) muestra que, tras iniciar la construcción, aparece `Acción iniciada` en una placa de unos 324 puntos sobre la silueta principal del Bastión. Es un resultado técnicamente válido pero artísticamente malo para un juego cuya escena debe dominar la pantalla.

**Corrección:** `Feedback` descarta exclusivamente el aviso genérico `Acción iniciada`, ya que la construcción satisfactoria ahora cierra el panel, muestra el estado `OBRA 0:NN` y una barra de progreso real sobre el Aserradero. Los errores de guardado, órdenes rechazadas o falta de recursos siguen visibles; no se suprime ninguna notificación de error. No se cambian las respuestas de LocalGateway, la economía, escena ni mecánicas.

**Verificación:** Unity PlayMode comprueba que después de Build el panel desaparece y no se superpone el aviso de éxito. La estética completa requiere nueva captura genuina del SHA final a tamaño móvil.
