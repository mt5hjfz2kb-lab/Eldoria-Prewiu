# ELDORIA · Relicario · Plantilla Maestra V2 Premium (candidata, no desplegada)

**Objetivo:** recuperar la riqueza visual de la referencia **aprobada como dirección artística** sin perder la plantilla reutilizable V1. PR exclusivamente de producción artística futura: **NO altera** escenas Unity, partidas, sistemas de combate, el Relicario v0.32 o la beta privada I–V.

## Decisión técnica y por qué

Se descartó mejorar en bucle los gradientes y filigranas SVG de V1: eran editables, pero visualmente planos comparados con la referencia. Se eligió **híbrido raster premium + estructura dinámica**:

1. **Fototextura real de los cuatro marcos** a partir del tablero visual aprobado: materiales y ornamentos auténticos de cada rareza, sin redibujar a mano formas genéricas.
2. Se abre y enmascara la **ventana interior** y se limpian cifras, nombre y efecto previamente baked; todos los textos pasan a DOM/campo UI, editables y locales. El método y los cortes de fuente figuran en `src/build_v2.py`.
3. El **sello indestructible** es `assets/overlay_indestructible.svg` independiente y no altera las dimensiones de la carta (5:8).
4. Los cuatro materiales se muestran con **la misma ilustración**, el mismo `id`, valores y efecto; `card-data.js` se genera desde `card_data.json` (el futuro juego siempre manda).
5. Exportaciones WebP RGBA minimizan tamaño de la **PR y demo**. `python src/export_unity_png.py` devuelve PNG de 640×1024 para importación posterior en Unity.
6. **Para una carta fija 5:8** usar `Image.Type.Simple` con `preserveAspect` (o equivalente) para el marco; **reservar 9-slicing para paneles redimensionables**, donde de verdad debe conservar las esquinas. Considerar `SpriteAtlas` para un uso simultáneo de varios sprites y medir memoria/drawcalls. Esto coincide con la documentación oficial de Unity:
   - https://docs.unity3d.com/Manual/9SliceSprites.html
   - https://docs.unity3d.com/Manual/class-SpriteAtlas.html

## Archivos

- `assets/frame_{common,rare,epic,legendary}.webp`: **4 marcos raster con alfa**, mismo lienzo 640×1024, cada uno con ornamentos/materiales de su rareza, extraídos de la referencia aprobada.
- `assets/overlay_indestructible.svg`: sello de infinito y energía protectora; aplica sobre las cuatro rarezas sin tocar estadísticas.
- `assets/bosque_valoria_art.jpg`: ilustración separada reutilizada de la V1.
- `assets/icon_wood.svg`: icono desacoplado de datos y efectos.
- `card_data.json`, `card-data.js`: datos reales, sin inventar valores ni bonificaciones por rareza. No confundir **comparar** con conceder nuevas cartas.
- `app.js`, `styles.css`, `index.html`: galería / vista móvil interactiva / tablero de escala 3×3. No incluye IA ni duelos PvP.
- `src/{build_v2.py,export_unity_png.py,sync_card_data.py,test_v2.py}`: regeneración local, exportación Unity futura y QA Chromium. **`build_v2.py` requiere la fuente de referencia artística**, cuyo original se conserva en el ZIP de producción asociado. Los WebP finales ya están incluidos en la PR, por lo que **no hace falta regenerar** para integrar.
- `qa/`: PNG A/B V1→V2 (generados en laboratorio local), reportes y capturas móvil.

## Ejecución y aceptación

`python src/sync_card_data.py && python src/export_unity_png.py` prepara los datos y las texturas de Unity. Desde este directorio, abrir `index.html?view=gallery` en un navegador con acceso local a recursos o servir con `python -m http.server`.

Para QA, `python src/test_v2.py` usa Chromium y recursos autocontenidos de **entrada**; no requiere conexión externa ni Unity. Comprueba 8 variantes, misma proporción 5:8, mismo efecto y valores, ausencia de desbordamiento en 390×844, sello reversible sin cambiar medidas y 9 celdas del tablero. **No** sustituye el criterio visual del propietario ni prueba el resultado real de Unity.

## Limitaciones artísticas y de producción

- Es una **V2 candidata**: los materiales y siluetas son más ricos respecto a V1, pero la aprobación final del acabado la determina una comparación visual del propietario. Un test verde no equivale a calidad.
- Se conserva el panel corto de efecto: `Recibes madera equivalente a 20 min de producción.`; el efecto auténtico completo sigue en `card_data.json` y en la etiqueta accesible.
- Los marcos son fototexturas con elementos de origen referenciado, no geometría vectorial completamente editable; el sello sí es SVG editable. Para alterar profundamente ornamentos debe reautorarse la textura de esa rareza.
- Al importar a Unity: revisar compresión, altas/bajas densidades, alfa, atlas, bordes transparentes, tipografía localizada, safe area y capturas reales iPhone/WebGL, sin activar esta pieza hasta terminar sus gates y su revisión.
- Sin publicación, sin CI pesada, sin consumo de créditos de pago ni cambios en `main` fuera del registro de titularidades.

**Estado al 13/11/2026:** `IMPLEMENTADO` como prototipo artístico aislado; `PROBADO` en Chromium 390×844; `PENDIENTE` de revisión final del propietario e importación Unity y QA dispositivo. No afirmar `ART_APPROVED` ni `GAME_READY`.

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

## V2.5 — revisión de las cuatro rarezas a escala móvil (13/11/2026)

- Común, Rara, Épica y Legendaria verificadas en formato normal e Indestructible mediante Chromium a 390×844. Comprobaciones adicionales a 320×720 y 430×932: **24 renders**, 5:8 constante, cero desbordes o cifras fuera de sus placas en la reconstrucción local del diseño.
- **Correcciones justificadas por la lectura móvil:** texto del efecto ampliado un 5,6% (de 4.5cqw a 4.75cqw para cartas >=245 px), alcanzando unos 12,6 px a 265 px de ancho; etiqueta Legendaria más aireada (3.65cqw e interletrado menor; aproximadamente 8 px de margen por lado en esa escala).
- Sin retocar la imagen central, las cuatro texturas premium, geometría de placas N/E/S/O, sello Indestructible, efecto real de madera ni proporción 5:8.
- Prueba automatizada añadida: `python src/test_mobile_four_rarities_v25.py`. Crea ocho capturas móviles con **los recursos versionados** y una lámina comparativa, además del reporte JSON. La prueba local realizada usó una reconstrucción equivalente desde la referencia de arte; el nuevo script de la PR aún deberá ejecutarse sobre un checkout del repositorio antes de tratar sus capturas como evidencia exacta del commit.
- **Calidad:** evaluación artística provisional, no una certificación de 9/10 por CI. Pendientes: capturas y rendimiento Unity/WebGL en dispositivo físico; aprobación del propietario en el contexto de la interfaz del juego.
