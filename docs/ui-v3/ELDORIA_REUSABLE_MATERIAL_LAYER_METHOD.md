# ELDORIA · Procedimiento artístico reutilizable — materialidad por capas

**Fuente:** aprobación provisional del Camino de los Vigilantes B11 (PR #136, fuente \`b462def53010da4493185ea3b394bc25620690ca\`, Unity run \`41703564464\`). **Estado:** MÉTODO CONSERVADO para futuras sesiones, NO certificado como calidad artística universal. Primera aplicación posterior: contratos de la Mesa de Exploradores, PR #136. Presupuesto: 0 €.

## Descubrimiento verificable

Los filtros y el parallax superpuestos sobre una única ilustración dejan la sensación de foto pegada. En Vigilantes se obtuvo un avance observable separando **fondo limpio, planos pictóricos no duplicados, 24 terrazas dependientes del recorrido real, iluminación y ficha funcional**; en móvil el cambio artístico fue menor. Solo el A/B genuino de Unity permitió reconocer el límite y aprobar provisionalmente.

La técnica reutilizable no es «poner más efectos», sino hacer una **jerarquía de materiales y oclusión**:
1. Conservar las ilustraciones auténticas y la autoridad de juego, y definir la superficie física que soporta los elementos.
2. Separar soporte → sombra de contacto → espesor → material frontal → ilustración en ventana rebajada → borde/oclusión → medallón → texto/control interactivo.
3. Reutilizar nine-slice para perímetros escalables; usar mallas uGUI pasivas para contacto, irregularidad, cantos y sombras, con raycast OFF y sin un Canvas/EventSystem o cámara extra.
4. Evitar dibujar la misma pintura debajo y encima de sus recortes. Evitar fotos horneadas con texto, estados o recompensas.
5. Colocar ilustraciones de contratos por tipo real y conservar los recortes UV originales sin deformarlos. Decoración ligada a estado de una orden existente (sin generar órdenes).
6. Adaptar la composición al tamaño real: horizontal, vertical y teléfono en ambas orientaciones. No compensar con tipografía ilegible.
7. Ejecutar Unity native sobre SHA exacto, capturar A/B mismos estados y revisar contacto/sombras, fidelidad a referencia, legibilidad, interactividad, progresión, guardados y ausencia de duplicaciones.
8. Un PASS técnico no equivale a ART_APPROVED ni a objetivo 8,3. Tras dos microciclos sin salto material, cambiar técnica de soporte/geométrica, no repetir gradientes o halos.

## Exploradores — implementación candidata

Recurso nativo: \`RoyalGoldV3ScoutPaperDepth.cs\`.
Tres módulos visuales independientes *por cada contrato real*:
- \`DeskContact\`: sombras de apoyo y canto de papel debajo del pergamino nine-slice existente.
- \`InsetUnderlay\`: lecho oscuro y marco de cuero/piedra detrás de la miniatura ilustrada.
- \`InsetLip\`: reborde diferenciado e irregularidad de esquinas que oculta sutilmente la unión con la miniatura.
  
Los tres modos se alimentan únicamente de \`ScoutOrder.Kind\`, \`StartedUtcTicks\` y \`EndsUtcTicks\`; estados listo, en marcha y cobrable existentes. El \`RoyalGoldV3ScoutContractGraphic\` conserva el medallón y la textura de sello real; \`scout-parchment-v3\` sigue siendo la hoja original nine-slice; \`scout-mission-vignettes-v4\` conserva UV y escala de imagen por tipo. Botones, gateway y economía no se tocan.

## Investigación #137 — NO promover por accidente

La PR #137 (\`68cc0508a47facd99e79a4e8ac566a05ad7b4717\`) propuso tres rutas: NORMAL (12 tropas y duración base), SIGILO (8 tropas y +50 % de duración), RÁPIDA (20 tropas y −25 % de duración), con StrategyMode persistido. El mismo contrato de selección se encuentra ya en la candidata de PR #136 (UI, \`EarlyRetentionRules\`, \`ScoutOrder\`). **No cambiar rutas, duración, cantidades, estado ni cobro por un encargo meramente artístico**. Las modalidades permanecen separadas como candidatas de diseño salvo aprobación específica del propietario.

## Regla de cierre del bloque

Tres contratos distintos visualmente, miniaturas reales y medallones propios; estados de preparado/enviado/retornado y cobro; sin raycasts ocultos; sin recortes de texto; 1600×900, 900×1600, 390×844, 844×390; tres envíos/cobros UTC idempotentes, reload real, misma partida y QA del mismo SHA. Entregar capturas A/B legibles *en el chat*, no solo ZIP. PR #136 DRAFT, sin publicación ni merge; si el arte sigue por debajo del objetivo, conservar evidencia y reabrir mejora, no afirmar nota.

**Protocolo de relevo:** leer este archivo junto a \`pipeline/active-workstreams.json\`, \`docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md\`, \`SESSION_HANDOFF.md\`, PR #136 y #137 antes de editar. No reconstruir el método desde cero.
