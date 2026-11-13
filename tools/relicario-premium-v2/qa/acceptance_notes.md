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
