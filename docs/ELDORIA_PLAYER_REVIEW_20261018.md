# Revisión de experiencia de jugador — native111

18 octubre 2026. Base exacta: fuente48837605705fed198ec485f2c2726af2ffa5eea9, run39138751241. Esta revisión NO es una partida manual completada.

## Qué se ha comprobado

- Se abrió el enlace privado en navegador remoto. El acceso redirigió a auth.openai.com/log-in; no existe una sesión autenticada disponible. No se accedió al canvas ni se modificó el acceso privado. Se eligió Apple y posteriormente se declinó la solicitud segura de acceso. Se respeta la cancelación: la partida manual queda pendiente.
- Se revisaron directamente capturas originales Unity de ciudad inicial, ciudad limpia construida, mundo horizontal/vertical, entrenamiento y diálogos.
- El recorrido automatizado native111 pulsa botones uGUI reales y completa I–II con recarga y hashes de guardados preservados; usa TestClock acelerado. Confirma funcionamiento, no diversión, tiempo de espera real, entrada táctil física o rendimiento de Safari.
- Se inspeccionó el código de navegación/gating. Los hallazgos inferidos se distinguen de lo visible. No se fabrican FPS, puntuaciones de arte ni sesiones de usuario.

## Prioridades

| Orden | Hallazgo y evidencia | Próxima mejora | Criterio de aceptación |
|---|---|---|---|
| P1 | Mundo: Valoria aparece como castillo redondo de otra familia visual; el nodo etiquetado Bosque muestra geometría rectangular provisional. Captura integration-world-16x9.png. | Unificar el icono de ciudad con el Bastión real y reemplazar los puntos provisionales por bosque/cantera/enemigo reconocibles. | Identificar tipo y ciudad sin leer etiquetas; mismas familias de color/material; tres niveles de zoom legibles. |
| P1 | UI: dos flechas parecidas en la misión; etiquetas enormes tapan edificios y recursos; el panel de cuartel tapa el edificio seleccionado. Capturas ciudad/entrenamiento/vertical. | Un único botón de objetivo, etiquetas compactas y cámara que encuadre dentro del espacio libre de paneles. | Objetivo y edificio seleccionado visibles y tocables en horizontal y vertical, sin superposición. |
| P1 | Incoherencia de desbloqueo: CanOpen(World) restringe botón, pero Select(gate) llama SetWorld(true) sin la misma condición. Confirmado por código, no probado manualmente. | Aplicar una única regla de acceso a botón, puerta y navegación contextual, con explicación cuando esté cerrado. | Todas las entradas al mundo respetan el mismo progreso; ninguna ruta permite saltarlo accidentalmente. |
| P1 | Entrenamiento muestra Operación en curso dos veces y mantiene coste/acción de entrenar durante la operación. Hay contador, pero la información compite. | Un estado claro Entrenando 20 arqueros + tiempo + resultado al terminar; distinguir coste pendiente de coste pagado. | El jugador entiende qué se pagó, qué sucede y cuándo termina sin otra ventana. |
| P1 | Ciudad: aserradero y cuartel quedan en césped sin caminos de servicio; amplias superficies vacías, bordes del río repetitivos y contacto edificio/terreno pendiente de prueba. Captura integration-city-art.png. | Caminos de tierra integrados, áreas de trabajo orgánicas sin volver a losas, transición suelo/cimientos, vegetación y orilla menos repetitiva. | Misma cámara antes/después, acceso visual continuo a cada edificio, sin flotación ni nuevos obstáculos. |
| P2 | Exploración: HUD concentra estadísticas de varios destinos incluso sin seleccionar uno; faltan señales visuales claras de marcha y actividad en las capturas. La ausencia total de animación no se certifica con imágenes estáticas. | Información por selección, destino/ruta/tiempo/recompensa visibles; revisar en partida real si existen marchas y cómo se perciben. | Comprender salida, recolección/combate y regreso mirando el mundo; no depender del párrafo numérico. |
| P2 | Narrativa: Aldric dice Las murallas ya se sostienen mientras la escena mantiene fragmentos derruidos; recurso Comida aparece a0 sin utilidad clara en I–II. | Alinear texto con reconstrucción parcial y ocultar/explicar recursos sin mecánica activa. | Ninguna frase o control promete un sistema ausente. |
| P2 | 179MB aprox. de salida WebGL; primera carga y memoria de iPhone no medidas. | Medir carga fría/caliente, memoria, estabilidad, gestos y orientación en Safari; optimizar texturas/recursos conforme a datos. | Evidencia de dispositivo y ausencia de recargas por memoria; fijar presupuesto con medidas, no suposiciones. |
| P3 | Juego funcional pero falta comprobar ritmo y decisiones en tiempo real. | Partida nueva sin acelerar reloj: observar esperas, claridad, repetición, retorno de recompensas y cierre I–II; después decidir variedad PvE/recompensas. | Recorrido manual registrado con puntos de confusión y tiempos. No abrir BastiónIII antes de estabilizar esta base. |

## Orden recomendado

1. Claridad y fallos de interacción: gating único, objetivo, cámara/panel y estados de operación.
2. Coherencia del mundo con Valoria y eliminación de representaciones provisionales.
3. Integración artística de la ciudad en el terreno, caminos de trabajo, materiales y vegetación.
4. Marchas/actividad perceptible y ajuste narrativo.
5. Rendimiento y ritmo real en móvil; después nueva progresión.

La orden actual solicita consolidar y proponer mejoras. Estos hallazgos se registran como trabajo siguiente; no se aplican cambios artísticos/jugables adicionales durante la consolidación para conservar la versión probada.
