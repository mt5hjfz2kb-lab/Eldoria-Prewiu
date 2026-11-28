# Evidencia donante de Región 1 y salas Corona

Revisión congelada `6057c2604de84ae27f47ede44eafa294c96bcd24`. La receta original está en `.github/workflows/valoria-region-modular-native.yml`, blob `374cef8bd49d5f78ddc17121dfd9611fbfd14748`. Los blobs de las dos escenas, pruebas, empaquetador y trazas constan en `run.py`. El workflow histórico de regresión del navegador referencia un jugador de otra revisión.

Dentro de la reserva existente de la PR, usar el checkout completo y desechable de Unity con finales de línea Git originales. Materializar también `/tools/valoria-modular25d/` y `/tools/bastion-iii/author-frontier.py`. Esta fase de región requiere Blender y su receta de autoría, Unity 6000.3.23f1, D3D11 y WebGL. No volver a ejecutar la fixture de ciudad ya solicitada. No despachar los workflows históricos. La navegación de la prueba del donante se usa únicamente para llegar al arte de referencia.

```powershell
python $kit/run.py native --checkout $donante --unity $unity --blender $blender --output $nativo
python $kit/run.py browser --checkout $donante --native $nativo --chrome $chrome --dpr 1 --output $movil
```

La segunda fase lee el jugador privado del mismo paquete y sirve los bytes en loopback. El navegador requiere Chromium, Playwright y su ffmpeg disponibles. La receta original usa Playwright 1.56.0 y también admite repetir el recorrido a DPR 3. El informe registra las versiones observadas. Conservar `private-player/` en el espacio de trabajo privado. Para el relevo exportar únicamente manifiestos, NUnit, trazas, PNG, vídeo y hashes de revisión. La prueba navegador modifica el reloj UTC de su sesión para recorrer los tiempos de espera.

| Vista que debe revisar el equipo artístico | Evidencia real esperada | Qué evaluar frente a Main y la integración |
|---|---|---|
| Ciudad en región y escalas cercana, regional y estratégica | home, region, panorama, ciudad desplazada de prueba | Pivote, encuadre, escala, silueta e identidad. La ciudad desplazada es una comprobación técnica excepcional. |
| Bosque, madera, piedra y río | woodland, wood, stone, river, stone-retina | Lectura del recurso, vegetación, cauce, cruces, sombras y profundidad. |
| Brecha | breach, breach-near, breach-panorama | Continuidad del terreno, pigmento, legibilidad del accidente geográfico por escala. |
| Criaturas y movimiento | 32 frames idle-Boar, 32 idle-Spawn, 60 camera | Contacto con el suelo, oclusión, estabilidad y continuidad del movimiento. |
| Identidad, HUD, objetivos y Consejo | nombre-inicial, bastion-1 a bastion-5, activities, council | Tipografía, jerarquía, marco, recursos, narrativa contextual y límites de placa. |
| Construcción y tutorial | guia-iniciar-obra, temporizador-en-parcela | Cartela, mano, estado de obra, legibilidad y anclaje. |
| Salón de héroes | sistema-heroes | Retratos, placas de selección y nombre. Las fichas y pestañas individuales requieren muestras aparte. |
| Preparación de marchas | sistema-march | Ranuras de héroe, composición y jerarquía de acciones. |
| Entrenamiento | sistema-training | Controles, coste, tiempos y contraste móvil. |
| Vigilantes y Exploradores | sistema-vigilants, sistema-scouts | Recompensas, órdenes, estados disponibles y lectura del mapa. |
| Tesoros | sistema-daily; tesoro-antes, seis fotogramas y resultado si hay reclamo | Escala de calendario, ceremonia, contrapeso de texto y animación. Se registra si el reclamo estaba disponible. |
| Correo y mochila | sistema-mail, sistema-bag | Contraste y lectura de filas, iconos y marcos. Un listado visible no constituye captura de un informe de combate o de uso de objeto. |
| Viaje ciudad/región | ciudad-a-region-5, regreso-a-valoria-5, region-1-pinch y pan | Legibilidad al cambiar de escala y conservación de composición. |

Por cada sala se guardan las dos orientaciones a 390×844 y 844×390 CSS, sus controles expuestos por la traza de botones real y sus puntos táctiles normalizados. La traza identifica centros de controles accesibles para la prueba. La geometría y el comportamiento de la futura integración se comparan con el inventario de Main. La fase nativa exige un NUnit pasado por fase regional e igualdad byte a byte de cuadrícula y rutas del **propio donante**. La comparación determinista de Main con el candidato sigue siendo independiente.

El kit deja sin calificar la rúbrica 20/20/15/15/10/10/10 hasta que existan imágenes reales de las tres revisiones en estados homologables y se inspeccionen. Las pantallas capturables en una sola orientación, las pestañas de héroes, el informe detallado, la mochila en uso, otros estados de recompensa, diálogos narrativos y confirmaciones sin muestra coincidente permanecen abiertas. Chromium móvil y vídeo WebGL no acreditan iPhone físico.
