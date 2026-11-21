# Corona: resolución de edificios, reposo y evidencia temporal
Base validada: 4b20945049c1f5328d58f08a02208f34336d40aa, ejecución 44854060028.
Candidato aislado: experiment/corona-retina-building-pass-20261121.
Estado de este documento al commit: implementado; pendiente del run del propio SHA.

## Diagnóstico y decisiones
Los originales del Bastión (480×314) y Aserradero (362×273) pierden definición al ampliarse en Retina. Se conservan intactos y se añaden restauraciones de 1440×942 y 1086×819. PPU 288 frente a 96; mismo pivote, tamaño de Sprite.bounds y posición. No cambian cámara, edificios canónicos ni reglas.

Las restauraciones son ediciones con la herramienta integrada imagegen tomando exclusivamente los edificios existentes como referencia. Conservan arquitectura y composición, pero reconstruyen detalles; no son una recuperación matemática de píxeles perdidos. Ajuste técnico posterior: encaje uniforme de su contorno alfa en la caja visible original, canvas exactamente 3× y ancla inferior común. No se introduce fondo de ciudad. Los hashes/dimensiones están en el manifiesto adjunto.

Texturas sin compresión destructiva y con mipmaps para alejar cámara sin aliasing. Coste teórico adicional de los dos edificios alrededor de 12 MB RGBA con mipmaps; esto NO es una medida del consumo total del jugador ni una certificación de memoria de Safari.

El ciudadano se quedaba en una fase de marcha durante la pausa. Dos poses propias de reposo comparten altura de 192 píxeles, PPU 200 y pivote con el atlas actual. Rutas, velocidad, pausas y orden por profundidad se conservan. La fase de marcha avanza por distancia efectivamente recorrida.

## Corrección de evidencia
El vídeo anterior avanzaba .56 s por fotograma y exportaba 10 fps: mostraba movimiento 5,6 veces acelerado. Era render auténtico, pero no representaba la velocidad del juego. Nuevo clip: 120 fotogramas con paso 1/15 s y exportación a 15 fps = 8 s. El manifiesto declara paso, cantidad y duración; el ensamblador valida estos valores y los hashes de los PNG. No implica 15 FPS medidos del jugador ni prueba en dispositivo.

Las transiciones de arte sincronizan inmediatamente la textura de sombra: Camera.Render del Editor puede ejecutarse sin un LateUpdate intermedio. La captura comprueba que la sombra corresponde al sprite canónico actual antes de cada estado, evitando diferencias entre la evidencia del Editor y WebGL.

## Validación exigida
Prueba de PlayMode: originales disponibles, restauraciones importadas, fábrica realmente devuelve sprites HD, dimensiones/pivotes de mundo invariantes y reposo delantero/trasero distintos. Mantener pruebas de costes, cierre, guardado, cámara y HUD. Comparar capturas auténticas, incluidas 390×844 DPR3, horizontal y tres estados del Aserradero. La aceptación artística y Safari físico siguen siendo independientes.

## Especificaciones de generación
- Bastión: restauración de alta definición de la misma arquitectura, silueta, tejados, torres, andamios, ventanas, puerta y banderas; juntas de piedra, pizarra y madera legibles, luz cálida superior izquierda; alfa real sin terreno ni sombra exterior.
- Aserradero: misma chimenea, buhardilla, bandera, sierra, banco, troncos, barriles y toldo; eliminar halo exterior marrón conservando oscuridad interior; alfa real.
- Reposo: el mismo mensajero de las dos primeras filas del atlas, dos vistas tres cuartos frontal/trasera mirando a la derecha, ambos pies plantados y brazos relajados; sin suelo ni sombras pintadas.
Estos archivos son recursos de producción, no capturas del motor.
