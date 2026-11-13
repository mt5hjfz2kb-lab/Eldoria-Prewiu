# Pruebas y revisión: candidato V2 · 13/11/2026

## Verificado (reproducción local)
- Chromium con viewport 1400×1240: ocho cartas renderizadas y mismo tamaño/ratio 5:8.
- Chromium 390×844: sin desplazamiento horizontal; misma dimensión al cambiar rareza o sello indestructible; cambio de la ilustración mediante archivo local.
- Tablero de inspección 3×3: nueve celdas, siete cartas compactas, sin cambiar el ratio.
- Ilustración, valores N6 E4 S3 O2 y efecto de madera de 20 minutos son iguales en todas las variantes (incluidas las de rareza distinta, únicamente comparativa visual).
- Fuentes RGBA por rareza, sello SVG independiente, datos externos `card_data.json` y generador de JS.

## Revisión artística a partir de las capturas
- **V1**: marcos vectoriales independientes pero con gradientes planos, remates poco ricos y jerarquía metálica escasa respecto a la referencia aprobada.
- **V2**: recupera directamente texturas, siluetas y remates de metal que estaban en la lámina aprobada, con cada rareza diferenciada por material y ornamento. El texto y el área ilustrada se han separado; no se ha variado el tamaño según rareza.
- **Límites visuales reales**: el efecto todavía puede exigir adaptación de tipografía al render Unity; el detalle del marco tiene origen raster de la referencia, no una versión vectorial completa. Evaluar contraste y nitidez en dispositivo físico antes de llamar la pieza ART_APPROVED.

## Rechazos / cambio de método
1. **V1**: metodología SVG íntegramente construida con gradientes; correcta geométricamente, insuficiente para el acabado solicitado.
2. **Primera extracción V2**: cuadros de recorte demasiado anchos para rara/épica/legendaria; aparecían restos de números y texto anterior, rechazo visual.
3. **Extracción registrada V2**: nuevo crop correcto por rareza y limpieza de texto/datos con fuente única de verdad; las capturas son la evidencia. No se continuó iterando en microacabados tras el impacto material recuperado.

## Pendiente de verificación
- Aprobación visual del propietario.
- Importador/asset pipeline Unity 6000.3.x, formatos y alpha, prueba real en 390×844 y iPhone Safari/WebGL, integración con el Relicario/Orin sin tocar guardados.
- Balance y contenido del Relicario (fuera del alcance de arte de esta PR).

## V2.1 — revisión de tipografía y solapamientos (13/11/2026)
Se añadió polish.css, una capa reversible de fuentes del sistema (serif medieval para rareza/nombre/cifras; sans para efecto), panel inferior más aireado y etiqueta de rareza sobre fondo protegido. La propiedad indestructible mantiene el texto AL USARLA, RECIBES y deja el icono ∞ por separado. Cifras de dos dígitos y títulos largos disponen de tamaños adaptativos. No se modificaron sprites, reglas, cantidades, proporciones 5:8 ni beta.
Pruebas locales en Chromium: galería 1440/768/390; carta móvil 320/390/430; tablero 390, sin colisiones o texto cortado en el DOM. Ejecutar python src/test_typography_v21.py para repetirlas y producir capturas. Pendientes: Unity, iPhone físico y aprobación artística final.

## Corrección estructural V2.2 — placas y números, no otro reajuste CSS
El problema original no era únicamente la posición CSS: las texturas de referencia retenían letras grabadas y cuadros opacos de limpieza. Los nuevos números eran una capa de texto independiente y parecían flotantes. La solución es `stat-plaques.js`, que dibuja **el borde metálico, la placa interior y cada N/E/S/O con su cifra en un mismo SVG 320×512**, por rareza y con números dinámicos del catálogo; cubre de forma coherente las inscripciones horneadas en la referencia. El marcador Indestructible y los cuatro marcos originales se mantienen por separado.
Se verificó en Chromium de 7 tamaños: galería 1440/768/390, carta móvil 390/320/430, tablero 390; 136 placas comprobadas geométricamente. Cada cifra y su letra quedan dentro de la placa, con centro horizontal desviado menos de 2 unidades SVG; también N/E/S/O=10. La prueba reproducible `python src/test_stat_plaques_v22.py` produce `qa/stat_plaques_v22.json` y capturas locales; no prueba Unity ni iPhone físico. No se ha fusionado esta PR a main.

## V2.3 — Indestructibles sin contorno verde (13/11/2026)
El antiguo SVG tenía dos contornos verdes **continuos** que convertían el estado Indestructible en un quinto marco y producían un recuadro artificial. Se han eliminado ambos y la sombra verde global de `.is-indestructible`. El nuevo `assets/overlay_indestructible.svg` de 320×512 contiene únicamente cuatro pequeñas marcas rúnicas en esquinas y un sello ∞ incrustado (∞ trazado como vector para evitar fallos tipográficos). **No cambia** dimensiones 5:8, rareza, marcos materiales, placas de N/E/S/O, ilustración ni valor/efecto. El sello conserva su posición superior derecha y es un overlay independiente reutilizable.
QA local con rasterizado SVG: el tramo central del perímetro permanece totalmente transparente, no hay píxeles sobre las cuatro placas de valor ni en el texto/efecto; sello visible. La comparación de arte se realizó sobre capturas reales preexistentes de la V2.2, sin fingir una captura de Unity o del juego. `python src/test_indestructible_v23.py` permite reproducir la geometría/transparencia. La integración Unity/WebGL e inspección artística final siguen pendientes.

## Versión V2.4 — Pulido artístico controlado (13/11/2026)
- Placas direccionales: cuatro acabados de metal armonizados con los marcos (hierro cálido, plata azulada, oro oscuro amatista y oro solar). Biseles con luz secundaria y receso de esmalte oscuro. Se conservan las mismas formas, dimensiones, anclajes, cifras y valores N/E/S/O.
- Sello indestructible: reducción proporcional al 88 % en el mismo anclaje, borde metálico oscurecido, dos remaches pequeños y luz esmeralda más suave. Sigue siendo una propiedad adicional sin borde verde continuo.
- Texto del efecto: aumento moderado de tamaño únicamente cuando la carta mide al menos 245 px de ancho; el panel mantiene altura y posición.
- PR de referencia: no modifica Unity, el vertical slice activo ni la beta. No hay una certificación artística automática de 9/10.
- Pruebas reproducibles: QA local Chromium (48 comprobaciones de centrado de números para 4 rarezas y valores 1, 6 y 10; diez anchuras de panel sin recortes), CairoSVG a 320×512 (sin contorno exterior, 4 placas y efecto libres, sello visible). Las regresiones V2.2/V2.3 siguen en src/. Faltan evaluación artística del propietario, Unity y prueba real iPhone.
