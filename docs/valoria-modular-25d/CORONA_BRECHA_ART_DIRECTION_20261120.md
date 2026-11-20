# Corona de la Brecha — dirección de arte operativa, Bastión I

**Rama aislada:** `experiment/valoria-corona-de-la-brecha-ui-20261120`. **Origen:** Umbral II `f66cdae8a240e0918af156ef6da457415ed92e4e`. Propietario elige dirección C. `main` y beta intactos.

**Identidad:** fantasía adulta, reino que resurge junto a una fisura arcana. Diferenciación fuerte respecto a Umbral II: contornos angulares de recursos, medallones circulares separados para acciones, emblema de Aldric, panel de obsidiana con gemas, pendón de objetivo y botón turquesa para la única acción principal.

**Paleta:** obsidiana #1A1428, violeta #30223F, amatista #A85BEB, oro envejecido #C99C54, marfil #F1E9D4, turquesa de acción #63D9CF, alerta #B55B80.

**Fuente gráfica:** 15 SVG originales, uno por pieza, en `tools/valoria-corona-brecha/source/`; `bake.py` genera PNG RGBA 2× con .meta/GUID Unity y 9-slice, hash en `CORONA_BRECHA_SPRITE_PROVENANCE.json`. Ninguna imagen extraída de juegos de terceros, ninguna pantalla de ciudad como único sprite. Los iconos son ilustraciones independientes y la escena existente sigue en 2.5D espacial.

**Integración:** `Modular25DInterface.cs` únicamente se rediseña visualmente. No se cambian `LocalGateway`, reglas de costes, construcción, guardado, movimiento, cámara, personajes ni la escena. Los únicos botones son Obras, Ciudad, Estado, Bastión/Aserradero y Construir, todos ya funcionales.

**Criterio de verificación:** comprobar 15 importaciones, 6 iconos visibles, 8 sprites 9-slice, Canvas y estado sin cambios, auténticas capturas Unity y navegador retrato/horizontal, Build real y saldo persistido. Fuente y pruebas estáticas != calidad comercial aprobada. iPhone físico y métricas siguen pendientes.

**Riesgos visuales:** el marco de identidad no debe tapar recursos; iconos deben distinguirse a 44 puntos, panel necesita suficiente contraste, sellos de mundo no deben tapar accesos del Bastión/Aserradero. Las capturas de diseño locales no son evidencia de ejecución del motor.

## QA de 9-slice: bordes normalizados al tamaño real
El 20/11 se identificó que los bordes iniciales `28` lógicos en resource/world/CTA sobrepasaban la altura efectiva de controles de 34–43 puntos y forzaban `Image.GetAdjustedBorders` a comprimir ambos extremos. El anillo de retrato también se reutiliza en chips de 27 puntos, por lo que requiere bordes específicos menores. Se normalizan los ocho marcos: identity_crest=20, resource_siglet=14, nav_medallion=19, portrait_ring=10, world_rune=10, objective_rune=17, action_arcane=14, panel_obsidian=56. El manifiesto documenta las alturas mínimas y NUnit valida cada importación con un centro 9-slice no degenerado. No se cambian las imágenes pintadas ni ningún evento o guardado del juego. **Pendiente:** inspeccionar capturas genuinas Unity/WebGL posteriores al rebake y verificar apariencia a tamaño de móvil.

## Segunda pasada ilustrada: 14 activos premium

La generación SVG básica pasó su gate de 15 importaciones, **pero no alcanzó calidad visual objetivo**. Se sustituye por 14 PNG originales ilustrados de alta definición extraídos como piezas independientes de una fuente original de generación de imagen (no copias de juegos comerciales). Están versionados en `tools/valoria-corona-brecha/premium` con hashes de los bytes fuente y se importan como sprites de Unity con GUID estable. **La técnica de SVG sigue siendo el fallback reproducible para `icon_scroll` y rescate, no una certificación artística.** El pipeline debe fallar si el PNG premium existe pero está dañado; jamás cambiarlo por un sustituto silencioso. Las pruebas visuales reales de Unity aún deciden si estos activos se leen bien a tamaño móvil, con especial atención a la deformación de los sellos en 9-slice y a la legibilidad de números/etiquetas.

## Corrección causal 20/11 — ilustraciones completas NO se deben dividir en nueve cortes

La simulación de sprites a tamaño móvil (fuentes pintadas 580×260 para controles 117×43) reveló una regresión visual: los paneles sólo mostraban una línea fina, pese a tener PNG correctos y `spriteBorder` válido. **Causa:** márgenes transparentes grandes en las fuentes de ilustración + estiramiento de su zona central mediante `Image.Type.Sliced`. Las formas completas de joyas/medallones no son texturas de bordes repetibles. La corrección que preserva la imagen es normalizar el recorte visible de los marcos pintados al producir los PNG de Unity y dibujarlos con `Image.Type.Simple`. Las fuentes PNG originales siguen intactas en `premium/`; los PNG de Resources son transformaciones deterministas con hash y trazabilidad. Los SVG y sus contratos 9-slice se conservan sólo como fuentes alternativas, no como mandato de render para las ilustraciones finales. Botones/acciones, eventos, costes y guardado no se tocan. **Pendiente de verificar:** resultado real en Unity/WebGL, iconos y texto en iPhone.

## Protección del área tipográfica en las fuentes ilustradas

Además de evitar 9-slice en ilustraciones completas, el área central de un sello tiene gemas y filetes que compiten con números y letras pequeños. Se conserva el arte premium en `premium/` sin alterar y se produce un **inserto de esmalte oscuro con borde violeta** en la zona de texto durante el horneado reproducible de `resource_siglet`, `identity_crest`, `objective_rune` y `world_rune`. Se desplaza el título a la derecha del cristal central. La simulación previa en 390×844 mostró la mejora de contraste; sólo el juego real puede aprobarla. Ningún coste, comando o guardado ha cambiado.
