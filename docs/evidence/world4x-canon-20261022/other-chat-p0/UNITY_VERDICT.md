# Ensayo del otro chat: resultado en Unity

**VISUAL_REJECTED para el atlas entregado.** Conservar las coordenadas editables como fuente de autoría; descartar la superposición de máscaras como acabado.

Separar geografía, pigmentos y distribución es una técnica con fundamento. Esta implementación no aporta la mejora artística solicitada. El descarte afecta al bake ejecutado; no demuestra que un sistema semántico coherente sea inviable.

## Prueba concreta

Paquete `eldoria_p0_art_directed.zip`, Library `libfile_31314d308a008191abd793a6a1f60db9`, SHA256 `dc2d43acc26422c746605fc430e68ed61ad954561ce693212d7938243c4eb08b`. Es distinto del ensayo previo de 40 arboledas reconstruido en este chat.

Se ejecutó su C# `RegionOneP0BakeAuthorialAtlas.Bake` con los seis PNG originales sobre la base pintada. Única corrección previa: importar máscaras como datos lineales. Cuatro capturas comparativas mantienen cámara, geometría, árboles y gameplay.

- Commit `8620a2e1a105ddb339bf4fec484d63073da4dfd9`.
- [Unity real, ejecución 39850201753](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39850201753).
- Artifact `12456743320`, `region1-canon-native-39850201753`.
- 7/7 PlayMode, 16 PNG; duración de pruebas 12,6014925 s.
- Las cuatro parejas regional/panorámica H/V tienen idéntica resolución, zoom, aspecto, posición y objetivo.
- PNG `*-p0-mascaras-*-unity.png`: ensayo. Pareja sin `-p0-mascaras`: base.
- El GLB del paquete es una previsualización separada de 21.025 vértices y 41.472 triángulos. No se ha presentado como malla integrada ni como prueba de un sistema completo de relieve y distribución.

## Defectos demostrados

1. Agua y bosque: 20.370 píxeles superan 128/255 en ambas máscaras, el 30,97 % de los 65.768 píxeles de bosque. Unity muestra manchas azules con silueta de copa y grietas coloreadas como agua.
2. Geografías duplicadas: el bake conserva los ríos/caminos originales y añade recorridos incompatibles.
3. La protección radial de 90 unidades elimina el centro real de la Brecha (8,61): rift=0 en ese punto. Los ramales quedan separados.
4. `forest_belts` se escribe en JSON pero no se aplica al generar las máscaras ni los árboles.
5. El tinte no resuelve relieve, encuadre panorámico ni continuidad entre las tres escalas.

Píxeles que cambian >5/255: panorámica H/V 59,57/63,86 %; regional H/V 50,95/56,05 %. Mide visibilidad del ensayo, nunca calidad.

## Decisión aplicada

Fuentes, máscaras, JSON, bake y pruebas conservados. Se retiran las cuatro capturas experimentales y el bake automático del circuito normal tras evaluar. El atlas P0 nunca se seleccionó por defecto.

Se conserva la idea de capas controladas en coordenadas de mundo. Nuestra continuación separa la Brecha de la pintura base para anclarla exactamente y proteger el acceso de Valoria. No se adoptan las máscaras ambiguas ni su mezcla de geografías.

No hay ART_APPROVED, aprobación del propietario, certificación móvil, publicación ni merge.
