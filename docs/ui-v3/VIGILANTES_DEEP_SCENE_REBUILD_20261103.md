# Camino de los Vigilantes — reconstrucción profunda B3 (2026-11-03)

## Orden del propietario
B2 (SHA 877d7fe4) es TÉCNICAMENTE verde, pero ARTÍSTICAMENTE rechazada: no volver a superponer una fotografía dentro de una tarjeta. Rehacer la composición, no acumular efectos que disimulen el error. Objetivo subjetivo ≥8,3/10; no afirmar éxito artístico sin comparar capturas Unity reales. Sin gasto ni publicación.

## Diagnóstico exacto
B2 presenta 47% de paisaje ilustrado + 50% de panel oscuro con imagen insertada en su interior. El Engendro es auténtico y de fondo transparente, pero está ubicado dentro del card de texto, no en el mundo. Los otros encuentros reutilizan el cuadrante de patrullas del atlas y parecen fotografías circulares recortadas. La perspectiva del camino apenas cambia y las líneas violetas del Engendro parecen gráficos UI artificiales.

## Cambio estructural B3
- Mover el ÚNICO sprite original de enemigo bajo `Scenic illustration` (mismo Canvas y sin segundo EventSystem), POR DEBAJO de la ruta real de 25 etapas. Retener exactamente `engendro-reference-v3`, `scout-mission-vignettes-v4`, `guardian-banner-v4`, UV originales, nombres y puntos de progreso; ningún modelo o lore ficticio.
- `RoyalGoldV3VigilantsTerrainOcclusion`: roca y vegetación del MISMO paisaje, re-proyectados con su cover UV real encima de la criatura; oclusión natural sin foto separada.
- `RoyalGoldV3VigilantsPortraitGraphic`: abandonar ventana circular B2; el arte de patrullas se disuelve por máscara pictórica irregular cuando está dentro del terreno. La criatura real de stage15 mantiene alfa original.
- `RoyalGoldV3VigilantsParallax`: parallax amplificado en cuatro planos (fortaleza, acantilado, primer plano a ambos lados), actualización limitata a 5 Hz y sin texturas duplicadas.
- `RoyalGoldV3EncounterGrounding`: retirar las rectas geométricas violetas; en su lugar usar contacto difuso orgánico con el terreno junto a la imagen alfa auténtica.
- Composición horizontal: escenario ~60% ancho y panel táctico ~36%; en vertical, escenario continuo sobre tarjeta de información con texto a toda la anchura. Sin foto dentro del panel; la escena es el protagonista.
- Retener `RoyalGoldV3StageTrailGraphic`, 25 posiciones calculadas desde `ArtworkUv`, sellos de guardianes 5/10/15/20/25, recompensa/gateway/composición de marcha/XP y persistencia byte por byte.

## Pruebas innegociables
1. Verificar run Unity B3 del SHA exacto con EditMode/PlayMode completos, los eventos reales y 26+ capturas nativas; ninguna excepción.
2. Comparar A y B3 en mismo estado: capítulos, guardianes 5/10/15/20/25, Engendro, victoria, derrota. Móvil 390×844, panorámico 1600×900, retrato 900×1600 y donde haya 844×390.
3. Inspeccionar si el retrato sigue leyendo como un fragmento pegado o se percibe auténtica integración; si no mejora inequívocamente, NO APROBAR.
4. Raycasts: foto, oclusión, parallax, hitos no pueden capturar los toques de `Combatir`, experiencia, `Formar marcha` y `Volver`. El escenario no puede contener paneles de lógica propios.
5. `EarlyRetentionNativeCapture` exige que la criatura cuelgue de `Scenic illustration`, fondo superpuesto sobre sus pies, los 25 hitos fuera de su caja, el atlas original en encuentros normales y el alfa original en Engendro 15.
6. Mantener PR137 divergida sin merge ciego; trabajar en PR136, no fusionar `main`, no publicar, no afirmar 8,3/10 ni rendimiento móvil sin medidas.

## Referencia A/B
A anterior a B2: SHA `178d12ca`, Unity run `41621181708`, artefacto retención `13224061203`. B2 rechazada: SHA `877d7fe4`, Unity run `41627656196`, artefacto retención `13226723271`. La siguiente B3 debe citar su propio HEAD y run. Este documento no es una prueba nativa por sí solo.
