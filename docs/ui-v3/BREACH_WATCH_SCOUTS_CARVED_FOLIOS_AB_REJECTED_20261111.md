# Vigilia — Experimento visual de contratos de Exploradores (11/11/2026)

Estado: **RECHAZADO por calidad visual**, aun cuando las 8 capturas nativas y las pruebas funcionales pasaron. La PR #173 conserva la composición anterior de los contratos. No publicar ni presentar la variante oscura como aprobada.

## Evidencia comparativa exacta

- Baseline Unity: SHA `0edc9d7d0a1073b7f407cde9f4abad848bb20176`, [Actions 43085172334](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43085172334).
- Variante probada: SHA `4952fda47099d798ecf2af60ad0b1bc27dee1679`, [Actions 43087468302](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43087468302).
- En ambos casos: imágenes de `real-unity-scouts-{390x844,900x1600,844x390,1600x900}.png` dentro de `BreachWatchDestinationEvidence`. La prueba usa la misma progresión simulada sin alterar el guardado.
- El candidato SÍ sustituyó las **tres imágenes reales de pergamino** por el sprite premium de piedra oscura; desactivó el relieve del papel y adaptó la tinta manteniendo miniaturas, lacres, textos de recompensa y botones reales. No fue un simple tinte superpuesto.

## Decisión de dirección artística

**La variante nueva quedó peor.** Los 3 contratos se convirtieron en placas carbón muy uniformes. La reducción de luminosidad eliminó profundidad y jerarquía, redujo mucho la legibilidad de las cantidades y dejó demasiados materiales oscuros seguidos. El estilo original con pergamino cálido, marco físico, ilustraciones vivas y botones burdeos se veía más completo y de mejor fabricación. Los resultados técnicos no justifican adoptar un diseño visual inferior.

La sustitución completa de la interfaz exige **consistencia material, no que cada superficie sea idéntica**: piedra para navegación y contenedores generales, pergamino ilustrado para contratos/documentos y metales/cera para acciones y recompensas. La primera pantalla aprobada sigue siendo el estándar de calidad; no se presupone que oscurecer los contratos lo eleve.

## Acción aplicada

Se devolvieron a fuente `BreachWatchRetentionTheme`, `BreachWatchRetentionBridge` y los controles nativos, se retiraron completamente `BreachWatchScoutStoneContracts.cs` y su `.meta`, y se añadió un guard automático que falla si vuelven los pergaminos reemplazados por la variante rechazada. Se mantienen los materiales y las misiones originales y se conserva el grado crepuscular del paisaje como experimento independiente.

## Siguiente técnica (cambio sustancial, sin microcorrecciones)

El elemento más alejado del acabado pictórico del escenario es el **adversario geométrico plano de Vigilantes** dentro del panel táctico. No intentar convertirlo en otro pergamino oscuro ni seguir retocando 2 puntos de sombra o margen. Investigar **una ilustración específica, con procedencia y licencia verificadas, del enemigo canónico de cada familia**, preferentemente a partir de modelos ya autorizados y con alfa limpia, y reemplazar la fuente plana IN PLACE; conservar el nombre, etapa, estadísticas, iconografía viva y control de batalla. Comparar la imagen completa de Unity y rechazar la técnica si parece otra fotografía pegada sobre la ilustración.

**Estado de aprobación:** la baseline de Exploradores es técnicamente verificable y visualmente preferible a esta variante; el usuario aún no ha aprobado todas las pantallas, ni se ha probado en móvil físico. Ni merge ni publicación concedidos.
