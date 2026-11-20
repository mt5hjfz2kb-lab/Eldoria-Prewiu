# Corona — fuente premium panorámica específica para placas y botón Construir

En los materiales activos se midió la fuente RGBA real `world_rune.png` y `action_arcane.png` como 580×260 (2,23:1). Unity estaba estirándolos como `world_rune` de 180×44 (4,09:1) y el CTA de construcción de 270×42 (6,43:1): eso achataba lateralmente la gema central y convertía las piezas volumétricas en franjas, aunque los PNG originales fueran vistosos.

El horneador reproducible `tools/valoria-corona-brecha/bake.py` recompone **solo esas dos fuentes**, conservando las esquinas originales y **el diamante central de ancho 74 px y alto 260 px sin alterar sus proporciones**. Expande las dos bandas interiores de esmalte utilizando filtrado Lanczos de la ilustración que ya existe, sin copiar arte de juegos de terceros ni añadir fotogramas de ciudad. Salidas: placa 1036×260 (~3,98:1) y CTA 1665×260 (~6,40:1). Se conservan GUID Unity y fuente artista original; otros 13 PNG se hornean como antes.

El test nativo de Unity valida las proporciones reales de ambas Images frente a sus RectTransform; se han preparado comparativas PIL A/B, **no** pruebas Unity. La calidad definitiva exige observar nuevas capturas auténticas 390×844, 1170×2532 y 844×390 y revisar no existan nuevas costuras visuales. No se cambia `LocalGateway`, el reloj, la escena o el guardado.
