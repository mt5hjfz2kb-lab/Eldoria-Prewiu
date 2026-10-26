# Eldoria — prueba móvil y reanudación de interfaz V3, 26-10-2026

**Origen:** incidencias comunicadas por el propietario tras probar físicamente Android 1.14/code14 del mundo 4X PR #100 ya fusionado en main.

**Autoridad:** el informe de un dispositivo real prevalece sobre las pruebas automatizadas que declararon la integración funcional. **No se aprueba el arte** por la autoevaluación 7,5 de Unity anterior ni por compilar.

## Hallazgos de jugador y pruebas de corrección

1. Al seleccionar «Mundo» desde Valoria, el jugador debe llegar **siempre en vista cercana**, centrada en Valoria y sin cambiar partida, incluso después de haber abandonado el mundo en panorámica o regional. `ValoriaIntegratedWorld.TrySetWorld` reutiliza cámara/HUD/gateway originales, pero forzaba `EnterStrategicView` (zoom distante) solo la primera vez. Se repara en PR #111; probar **reentradas repetidas** y giro de móvil.
2. Los nodos no deben saltar visualmente al cambiar de zoom. `RegionOneCanonicalLandscape.UpdateLevelOfDetail` recentraba el panorama bruscamente en `regionBounds.center`, aunque el jugador estuviera en otro sector. Se retira ese `PanTo`. Su `FitToAuthoredBounds` conserva el límite físico. Los recursos cercanos y los impostores regionales usan la misma coordenada lógica del `LocalGateway`, con compensación inicial de pivote de malla; las criaturas Tripo cercana/regional se alinean al mismo punto guardado calculando la envolvente real de sus renderizadores. No tocar coordenadas del guardado ni duplicar entidades.
3. El objetivo son tres escalas **sobre una geografía única con zoom continuo**, anclaje real en el suelo, mismo ID seleccionable y sin desplazamientos espurios de cámara. Probar seguimiento del mismo nodo cercana → regional → panorámica → regional → cercana, orientaciones 1280x720 y 390x844, giros y transiciones lentas y rápidas. Los recursos/amenazas se ocultan en panorámica sin perder datos ni selección legítima al regresar.

## Comparación artística con evidencias reales anteriores

Fuentes de comparación: los tres PNG aprobados en `docs/visual-reference/world-map/` y las capturas Unity auténticas de `docs/evidence/world4x-canon-20261022/sources/native-40337702256/review/`. Esas capturas **preceden** a esta corrección, por lo que no certifican el nuevo código.

- **Cercana:** castillo y madera/roca existentes son reconocibles; pradera y caminos aún se aprecian uniformes y menos orgánicos que el canon, contactos de ribera y siluetas sencillos. Priorizar el paisaje jugable, no llenarlo de props.
- **Regional:** el terreno queda muy plano y se repiten intensamente asentamientos/nodos/criaturas. Conservando la población real, diferenciar escala y masas mediante tratamiento gráfico coherente, sin mover ni regenerar la ubicación persistida y sin crear ciudades falsas.
- **Panorámica:** pierde mucho detalle y relieve, el perímetro presenta una costa oscura y franja uniforme, y la Brecha se distingue menos que en la referencia de tierra fracturada. Priorizar lectura geológica central, diversidad de masas, transición de color y periferia, sin mostrar recursos ni añadir niebla regional.
- **Corrección artística más útil:** intervenir por **familias visuales que comparten una causa** (acabado del suelo/transiciones de pigmento, Brecha/costa y jerarquía de vegetación), con captura real de las tres cámaras en ambas orientaciones antes/después. Si fracasa dos veces, cambiar el método en lugar de repetir retoques de color. Sin gastar créditos ni utilizar Qwen para autoría o evaluación artística.

## Interfaz V3 reanudada

Base: diseño **Interfaz 1 / Diálogo 1** integrado en main, pantalla real de **Entrenamiento y Marchas** del PR #90. No crear segundo HUD, Canvas, gateway ni repetir lo que ya funciona. Próximo tramo tras estabilizar la cámara:

- Verificar que el HUD **no tapa el mundo** ni los objetivos en horizontal/vertical, y que el botón Construir/Mejorar, costes, requisitos y temporizadores están asociados a la selección real.
- Auditar pantallas de héroes y navegación de gestión pendiente respecto al flujo real del vertical slice, sin inventar héroes o funcionalidades. Diálogo 3 de Orín/cartas solo al disparador narrativo canónico.
- Comparar las capturas auténticas y completar I–II, entrenar, marchar, luchar, regresar, apagar/cargar y guardar.

**Estado a 26-10:** PR #111 DRAFT con cambios en fuente y pruebas; **todavía sin Unity QA sobre el HEAD final**, sin aprobación artística externa, sin build Android corregida ni actualización distribuida. PR #99 se conserva DRAFT; no fusionar por coincidencia temática. Presupuesto 0 €. Mantener la partida del propietario intacta.
