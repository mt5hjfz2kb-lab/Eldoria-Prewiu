# Eldoria Región 1 — revisión artística A/B real, 24/10/2026

**Estado:** mejora incremental conservada en PR #100 DRAFT. NO es ART_APPROVED, NO MERGE, NO publicación. Rama `feature/world4x-region1-canon-20261022`.

## Observación: imágenes auténticas, cámara equivalente

Se inspeccionaron las seis imágenes `*-unity.png` generadas en PlayMode y sus correspondientes `*-source.json` de las ejecuciones **40116606424** (base) y **40127016633** (candidato materializado). Se recalcularon SHA256 de los PNG y se comprobaron exactamente `zoom`, `aspect`, `cameraPosition`, `cameraRotation` y `worldTarget`. No se usaron imágenes recreadas ni comparativas desde diferentes cámaras.

| Cámara Unity | MAE RGB sobre 255 | Píxeles con diferencia superior a 5 en algún canal |
|---|---:|---:|
| Cercana horizontal | 0,414 | 1,17 % |
| Cercana vertical | 0,275 | 0,80 % |
| Regional horizontal | 5,640 | 27,76 % |
| Regional vertical | 3,590 | 20,68 % |
| Panorámica horizontal | 1,498 | 11,25 % |
| Panorámica vertical | 1,398 | 11,47 % |

Estos valores **miden cambio de píxeles, nunca puntuación artística**. Frente a la ejecución 40125446692, la versión 40127016633 reproduce los mismos PNG de regional y panorámica (SHA256 idénticos) y solo diferencias imperceptibles en cercana; no se presenta la materialización como una mejora nueva.

## Decisión y corrección: qué se ha conservado

**Regional, RETENER como mejora parcial:** masas coníferas más continuas en el área central y pradera más trabajada, con caminos y claros separados. Aun así, las manchas arbóreas mantienen geometrías demasiado previsibles frente a `02_vista_regional.png`; el terreno es de color y relieve demasiado uniforme.

**Cercana, RETENER solamente sin atribuir salto de calidad:** se han integrado elementos de sotobosque y textura puntual, pero los PNG muestran un incremento perceptivo pequeño. La composición actual sigue lejos de `01_vista_cercana.png`: el castillo, la ribera y los caminos tienen un entorno mucho menos rico y persiste una superficie visual del puente muy sencilla. No se justifica retomar un ciclo largo de microparches del puente.

**Brecha, RETENER como reparación incremental:** menos placas minerales grandes y rocas sueltas con volumen, pero el borde violeta continúa excesivamente uniforme y sus laterales todavía no alcanzan un aspecto geológico convincente.

**Panorámica, SIN sobreinvertir:** continuidad de costa y una Brecha reconocible; quedan bosques agrupados y terreno poco variado. No priorizar detalles exclusivos de zoom extremo.

## Veredicto

**RETAINED (mejora parcial de la candidata), ART_APPROVED = false.** No corresponde declarar 7,5–8/10 ni cierre del encargo: la regional ha mejorado, pero la cercana y la Brecha conservan brechas de calidad visibles. Los materiales originales, mallas, UV y árboles originales AF/AG siguen bajo protección.

## Certificación técnica y protección

- `40127016633` PlayMode GPU **57/57**, EditMode **26/26**, comprobaciones I–II **44**, capturas generales **34**; `general-proof.json` de la misma ejecución.
- `retained-core-integrity.json`: **50.688 caras históricas, 0 pérdidas/cambios**. Fuente AI fijada por SHA256.
- `Eldoria_Region1_Production.blend`, `Eldoria_Resource_Library.blend`, GLB, consultas y capturas verificadas y conservadas con hashes en `sources/native-40127016633/manifest.json`.
- No se ha medido el rendimiento en un dispositivo Android/iOS físico.
- No se han modificado estado jugable, nodos, navegación, UI ni `main`. El PR sigue DRAFT.

## Método real: observación → decisión → corrección → veredicto

**Observación:** PNG Unity reales del antes/después y revisión directa de la composición en cercana, regional y panorámica.

**Decisión:** descartar la multiplicación de trazos de bosque estrechos y placas de roca; priorizar cuerpos forestales regionales, pigmento conectado y derrubios de piedra.

**Corrección:** materializar los scripts de autoría revisados mediante **Blender/bpy batch** en `40127016633` y comprobar exactamente el arte exportado en Unity; las seis capturas se conservan.

**Veredicto:** se conserva una ganancia incremental, **no se otorga aprobación artística final**. BlenderMCP interactivo no se invocó en esta iteración: el workflow `blender-interactive` quedó en skipped al ejecutar el flujo de producción; no se simula una sesión de MCP. Un futuro pase de alto impacto debería usar inspección-escultura-revisión realmente interactiva y concentrarse en relieve/ribera cerca del castillo y la lectura natural de la Brecha, evitando modificar AF/AG y reservando el runner solo sin conflictos.

