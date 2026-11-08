# Vigilia de la Brecha — auditoría de ocupación visual

**Alcance:** contexto de diseño de la primera pantalla real. No es una certificación artística. Se mide la **suma de rectángulos visibles de controles/paneles**, no una segmentación exacta de píxeles opacos. Un marco troquelado que tiene esquinas transparentes aún consume toda su caja para esta medición conservadora.

## Referencia de juegos comparables (recuperadas en la revisión)

- **Rise of Kingdoms (ciudad normal)**: captura de BlueStacks, 512×512; https://cdn-www.bluestacks.com/bs-images/Rise-of-Kingdoms-Gamota-UI-Guide-18.jpg. Delimitación **manual y conservadora**: HUD superior, avisos/objetivos, accesos laterales, chat y navegación, incluidos iconos superpuestos a edificios. **≈42% de caja total**, con margen de error de 5–10 puntos por cajas grandes parcialmente transparentes. Se excluye la barra de emulación (≈21 px).
- **Call of Dragons (ciudad normal)**: captura de BlueStacks, 512×512; https://cdn-www.bluestacks.com/bs-images/cdg-farming-guide-ptbr-img3.jpg. HUD de recursos, eventos, iconos laterales y etiquetas de interacción. **≈25% de caja total**, margen ±5 puntos; se excluye barra derecha y 21 px superiores del emulador.
- **Lords Mobile (ciudad normal)**: captura 512×512 de LDPlayer, https://encdn.ldmnq.com/faq/images/pt/e456701f-d932-44e2-938a-9e8625be2d18.jpg. Cabecera, eventos, accesos inferiores y laterales. **≈35% de caja total**, margen ±7 puntos; no se cuentan edificios/esculturas como UI.
- **Rise of Kingdoms (edificio seleccionado)**: captura de 9Game, https://dl.bbs.9game.cn/attachments/forum/202009/29/182350rtzzee6868l99z0z.png. Controles de edificio + HUD normal, **≈38% de cajas visibles**, margen ±7 puntos.

**Límite comparativo:** son capturas de versiones, emuladores, estados, resoluciones y recortes distintos. No es válido inferir un «estándar» universal ni una preferencia real de jugadores a partir de esas cifras. El porcentaje de ocupación no mide calidad de arte, legibilidad ni claridad de navegación. Las estimaciones arriba son *superficies de rectángulos de contenido*, no superficie exacta de píxeles no transparentes; es preferible dar rangos a fingir exactitud.

## Nuevo Canvas nativo de Vigilia: geometría actual

A fecha de esta revisión, la prueba **source-level** (Ubuntu, `tools/breach-watch/validate_compact_layout.py`) confirma la huella en los mismos cuatro formatos que certificará Unity:

| Resolución | Estado comparado | Unión conservadora de las cajas del HUD | Límite fuente |
|---|---|---:|---:|
| 1600×900 | Ciudad, Bastión seleccionado | **17,8%** | 24% |
| 900×1600 | Ciudad, Bastión seleccionado | **29,1%** | 36% |
| 844×390 | Ciudad, Bastión seleccionado | **29,3%** | 32% |
| 390×844 | Ciudad, Bastión seleccionado | **33,1%** | 36% |

**No confundir esta simulación con medición de captura nativa.** La prueba de Unity `BreachWatchNativeEvidence.ConservativeUIFootprint` vuelve a calcular desde los rectángulos *reales* en pantalla y rechaza la pantalla si se pasa del límite. Los formatos móvil no pueden mantener literalmente el porcentaje de 1600×900, porque el texto y las áreas táctiles ≥44 px han de seguir siendo legibles.

## Decisión artística confirmada

El propietario prefiere el prototipo compacto de 8 de noviembre, con ciudad protagonista, fondo poco oscurecido, cabecera del jugador sin gran estandarte rojo, objetivo lateral mínimo, 3 recursos ilustrados y 5 medallones inferiores individuales. La ficha flotante del Bastión solo aparece al seleccionarlo y usa materiales detallados, grabados en cobre y el modelo 3D **real**. Se conserva esa composición y **no se reintroduce la barra roja**, un dock inferior de app, ni un modal enorme.

**Prueba final obligatoria:** extraer ocho PNG de Unity al SHA exacto de la PR #173, comparar a tamaño de dispositivo con la referencia del propietario, comprobar centro del mundo libre, lectura de edificio/estado/costes, transparencia real y coste de render. ART_APPROVED solo tras lograr visualmente lo acordado, no por aprobar este informe.
