# Eldoria — Relicario premium V2 · contrato de presentación (13/11/2026)

**Autoridad de intención:** referencia visual aprobada por el propietario en la conversación, en particular la lámina de ocho cartas esbeltas 5:8 de Bosque de Valoria, material y tratamiento indestructible. La ejecución V1 modular fue explícitamente valorada como inferior en riqueza. **Esta V2 aún NO cuenta con aprobación visual final** ni integración Unity.

## Reglas no negociables

1. Proporción única 5:8 (fuente 640×1024 px): ninguna rareza ni propiedad indestructible modifica geometría o tamaño.
2. Zona de ilustración protagonista. Panel inferior comprimido con efecto redactado como resultado de **usar** la carta.
3. Valores direccionales siempre en las posiciones N/E/S/O: deben venir del catálogo real, nunca dibujados por un generador de imágenes.
4. Materiales por rareza: **común** hierro pulido envejecido, **rara** plata/cristal azul, **épica** detalles ornamentales oro+amatista, **legendaria** oro ámbar con remates propios. No son solo tintes distintos.
5. Indestructible es **propiedad transversal**: escudo ∞, filamentos verde arcano, brillo contenido. Su rareza original, valores, efecto y dimensión **no cambian**.
6. `Códice` = conocimiento, `Relicario` = cartas, `Arcón` = materiales/equipo. Este candidato no mezcla responsabilidades ni agrega edificios.
7. La prueba de rarezas de **Bosque de Valoria** es una demostración de acabados, NO inventario real: su rareza canónica continúa siendo **común** y su efecto en el juego **20 minutos de madera**.

## Diseño de capas y escalabilidad

- `CardRecord` (fuente de verdad de rareza, id, texto largo, efecto, nombre y N/E/S/O).
- `Illustration` (propiedad gráfica intercambiable, sin texto baked).
- `FrameRarity` (alfa RGBA raster editable por rareza; proporción idéntica).
- `Typography and stats` (capa de texto UI real, apta para localización y tamaños de fuente).
- `IndestructibleOverlay` (SVG+sprite independiente, sin alterar contenido).

Unity: importar marcos como sprites 2D/UI RGBA y exportación PNG, preservar `aspect-ratio=5/8`; **no usar 9-slice indiscriminado en el marco fijo**, sí en paneles o componentes que se escalen de manera no uniforme. Empaquetar sprites que coincidan en una pantalla mediante SpriteAtlas si el perfilado confirma beneficio. No se justifica gasto ni trabajos nativos mientras la beta I–V mantiene su calendario.

## Criterios de aceptación futuros

- A/B frente a la **misma referencia** a escala de carta y vista móvil: material, relieve, remates, nitidez, legibilidad (no solo diferencia de píxeles).
- 8 variantes y ratio exacto, mismas posiciones de arte y texto, sello con/sin tamaño inalterado.
- Iconos de efecto, textos y números independientes; al sustituir la ilustración no aparece dibujo/texto anterior alrededor.
- Uno o más assets en UI Unity real a 390×844 y tablet; comprobar no clipping, safe-area, compresión de bordes y rendimiento en iPhone Safari/WebGL sin reinicios.
- Validar integración con catálogo existente, guardados, rarezas, consumos e indestructibles. No duplicar el motor de cartas web ni activar PvP ficticio.

## Técnica investigada

Preferencia elegida: **material raster con textura + composición en capas**. La versión previa SVG permitió geometría y colores consistentes, pero degradó el detalle en bordes/metales y no reprodujo el material aprobado. Se rescataron desde la lámina aprobada los detalles de cada rareza y se limpiaron los textos/áreas variables para dejarlas fuera de la textura.

Fuentes técnicas oficiales de Unity que fundamentan la importación posterior:
- https://docs.unity3d.com/Manual/9SliceSprites.html
- https://docs.unity3d.com/Manual/class-SpriteAtlas.html

No se sostiene que la extracción fotográfica por sí sola complete la producción: necesita aprobación visual e integración comprobada.

## V2.3 — Indestructibles sin contorno verde (13/11/2026)
El antiguo SVG tenía dos contornos verdes **continuos** que convertían el estado Indestructible en un quinto marco y producían un recuadro artificial. Se han eliminado ambos y la sombra verde global de `.is-indestructible`. El nuevo `assets/overlay_indestructible.svg` de 320×512 contiene únicamente cuatro pequeñas marcas rúnicas en esquinas y un sello ∞ incrustado (∞ trazado como vector para evitar fallos tipográficos). **No cambia** dimensiones 5:8, rareza, marcos materiales, placas de N/E/S/O, ilustración ni valor/efecto. El sello conserva su posición superior derecha y es un overlay independiente reutilizable.
QA local con rasterizado SVG: el tramo central del perímetro permanece totalmente transparente, no hay píxeles sobre las cuatro placas de valor ni en el texto/efecto; sello visible. La comparación de arte se realizó sobre capturas reales preexistentes de la V2.2, sin fingir una captura de Unity o del juego. `python src/test_indestructible_v23.py` permite reproducir la geometría/transparencia. La integración Unity/WebGL e inspección artística final siguen pendientes.

## V2.5 — revisión de las cuatro rarezas a escala móvil (13/11/2026)

- Común, Rara, Épica y Legendaria verificadas en formato normal e Indestructible mediante Chromium a 390×844. Comprobaciones adicionales a 320×720 y 430×932: **24 renders**, 5:8 constante, cero desbordes o cifras fuera de sus placas en la reconstrucción local del diseño.
- **Correcciones justificadas por la lectura móvil:** texto del efecto ampliado un 5,6% (de 4.5cqw a 4.75cqw para cartas >=245 px), alcanzando unos 12,6 px a 265 px de ancho; etiqueta Legendaria más aireada (3.65cqw e interletrado menor; aproximadamente 8 px de margen por lado en esa escala).
- Sin retocar la imagen central, las cuatro texturas premium, geometría de placas N/E/S/O, sello Indestructible, efecto real de madera ni proporción 5:8.
- Prueba automatizada añadida: `python src/test_mobile_four_rarities_v25.py`. Crea ocho capturas móviles con **los recursos versionados** y una lámina comparativa, además del reporte JSON. La prueba local realizada usó una reconstrucción equivalente desde la referencia de arte; el nuevo script de la PR aún deberá ejecutarse sobre un checkout del repositorio antes de tratar sus capturas como evidencia exacta del commit.
- **Calidad:** evaluación artística provisional, no una certificación de 9/10 por CI. Pendientes: capturas y rendimiento Unity/WebGL en dispositivo físico; aprobación del propietario en el contexto de la interfaz del juego.
