# Corona de la Brecha — dirección de arte operativa, Bastión I

**Rama aislada:** `experiment/valoria-corona-de-la-brecha-ui-20261120`. **Origen:** Umbral II `f66cdae8a240e0918af156ef6da457415ed92e4e`. Propietario elige dirección C. `main` y beta intactos.

**Identidad:** fantasía adulta, reino que resurge junto a una fisura arcana. Diferenciación fuerte respecto a Umbral II: contornos angulares de recursos, medallones circulares separados para acciones, emblema de Aldric, panel de obsidiana con gemas, pendón de objetivo y botón turquesa para la única acción principal.

**Paleta:** obsidiana #1A1428, violeta #30223F, amatista #A85BEB, oro envejecido #C99C54, marfil #F1E9D4, turquesa de acción #63D9CF, alerta #B55B80.

**Fuente gráfica:** 15 SVG originales, uno por pieza, en `tools/valoria-corona-brecha/source/`; `bake.py` genera PNG RGBA 2× con .meta/GUID Unity y 9-slice, hash en `CORONA_BRECHA_SPRITE_PROVENANCE.json`. Ninguna imagen extraída de juegos de terceros, ninguna pantalla de ciudad como único sprite. Los iconos son ilustraciones independientes y la escena existente sigue en 2.5D espacial.

**Integración:** `Modular25DInterface.cs` únicamente se rediseña visualmente. No se cambian `LocalGateway`, reglas de costes, construcción, guardado, movimiento, cámara, personajes ni la escena. Los únicos botones son Obras, Ciudad, Estado, Bastión/Aserradero y Construir, todos ya funcionales.

**Criterio de verificación:** comprobar 15 importaciones, 6 iconos visibles, 8 sprites 9-slice, Canvas y estado sin cambios, auténticas capturas Unity y navegador retrato/horizontal, Build real y saldo persistido. Fuente y pruebas estáticas != calidad comercial aprobada. iPhone físico y métricas siguen pendientes.

**Riesgos visuales:** el marco de identidad no debe tapar recursos; iconos deben distinguirse a 44 puntos, panel necesita suficiente contraste, sellos de mundo no deben tapar accesos del Bastión/Aserradero. Las capturas de diseño locales no son evidencia de ejecución del motor.
