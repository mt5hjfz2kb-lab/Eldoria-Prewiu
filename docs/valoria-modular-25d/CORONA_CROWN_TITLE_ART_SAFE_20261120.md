# Corona de la Brecha — título del reino dentro del marco ilustrado

Observación: captura REAL de Unity WebGL 1170x2532 del run 44817877082 (SHA 7afd1d3) mostraba “VALORIA” sobreimpreso sobre el extremo derecho dorado del escudo. La etiqueta tenía `RectTransform` x=51, width=78 dentro de una insignia de 182 puntos; su borde derecho geométrico era 91+51+39=181, prácticamente en el límite exterior del marco. Esto no es un problema de fuente sino de ubicación.

Corrección implementada sólo en presentación: x=46, width=70, dando borde derecho 91+46+35=172 (10 puntos de margen). Se conserva `resizeTextForBestFit` min16, max19, el texto y los datos canónicos. El subtítulo Bastión se alinea con la misma columna sin cubrir orfebrería.

NUnit PlayMode ahora exige 8 puntos mínimos de margen derecho del texto dentro de la placa. Falta captura exact-HEAD auténtica en Unity/WebGL para confirmar la composición perceptual; no declarar arte aprobado sólo por la geometría.
