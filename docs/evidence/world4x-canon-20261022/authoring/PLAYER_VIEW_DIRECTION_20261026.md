# Corrección de la cámara del jugador — 26-10-2026

Dirección: codex-world4x-player-art-22db827a. Encargo vigente del propietario: corregir el mundo, priorizar la imagen cercana, nodos al 85% de la ciudad, integrar después de demostrar calidad y jugabilidad. Presupuesto 0 €.

## Evidencia y diagnóstico

Se han visto los tres PNG originales canónicos y las capturas genuinas del candidato Unity 40253785923, en ambas orientaciones. Evaluación inicial aproximada: cercana 6,5; regional 5,8; panorámica 5,2. No es una certificación. La referencia muestra claros verdes y dorados conectados, sombras legibles, amenazas grandes, puentes cortos y rotura geológica irregular. El candidato presenta dominante amarilla, contraste entre parche central y continuación, recursos pequeños, puente excesivamente largo y paredes de Brecha regulares. Un build verde no resuelve estas diferencias.

Reutilización comprobada con tools/asset-vault/find-reuse.py sobre los índices de main: 14 candidatos. Se conserva la fuente real de Región 1 y sus recursos aprobados; no se añaden módulos arquitectónicos de Valoria ni primitivas decorativas ajenas. Fuente inspeccionada mediante BlenderMCP en 40287703090, SHA-256 fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce. La inspección registra límites, vértices y distribución de materiales reales.

## Decisiones de esta iteración

1. Medir anchura horizontal de los vértices importados: ciudad 24, nodos 20,4. Ajustar el volumen seleccionable existente. Criaturas cercanas: jabalí 10,8 y engendro 13,2, conservando el comportamiento regional.
2. Sustituir la amplificación roja del césped por claros verdes/dorados a escala del mundo, manteniendo grano y caminos del pigmento fuente. Unificar el tratamiento a ambos lados de los límites de material.
3. Dar más lectura a las sombras mediante relleno acotado; reducir las tres luces violetas que se leen como focos.
4. Esculpir escalones y fracturas en los vértices existentes de las paredes, conservando núcleo AF/AG, imágenes originales, topología, reservas y máscaras jugables. Reproyectar las superficies geológicas sobre el terreno y regenerar los datos de altura correspondientes. Eliminar las líneas sinusoidales del acabado.

La ejecución de estas operaciones no constituye aceptación visual. La longitud del puente, el borde de cantera, diversidad de copas y continuidad panorámica requieren revisión y corrección posterior según las capturas. No afirmar que estos defectos estén ya resueltos.

## Criterio de cierre

Revisar capturas reales cercanas, regionales y panorámicas H/V y detalles de Brecha, nodos y criaturas. Mantener pruebas de soporte, selección, rutas, población, persistencia e integración I–II. Comparar con referencia original; valorar por composición y legibilidad. No integrar un candidato defectuoso para alcanzar una fecha, no rebajar tests y no declarar rendimiento físico iPhone sin medición.

## Medición del puente

Inspección de triángulos del GLB real: agua entre z=-17 y -24; terreno a cota 2,98 en -8 y 3,19 en -26. El tablero actual ocupa -35,92 a -1,92. Se corrige a 18 m entre -26 y -8 con apoyo medido en las dos orillas; se conserva el eje x=3,3 y los postes, acortando sus posiciones. La aproximación terrestre deja de ser una rampa de madera. Solo los vértices del propio tablero quedan exentos de la preservación del núcleo; los 96 testigos de pradera y caminos siguen intactos.

El primer intento Unity 40289495400 no llegó al editor: el mensaje de commit no activó la recuperación del paquete base requerida por el workflow. Se corrige el disparo usando el prefijo existente `region-imported(world4x):`; ese intento no cuenta como compilación ni prueba.

La comprobación posterior de los dos bordes (además del eje) detectó agua bajo la esquina x=7,3/z=-26. El gate de apoyo rechazó correctamente ese primer intento. Se conserva el extremo norte -8 y se lleva el sur a -29, donde sí hay terreno en todo el ancho: tablero final previsto de 21 m, no 18. No se debilitó la comprobación de apoyo.

Cantera: el GLB tenía coordenadas UV degeneradas, idénticas para los ocho triángulos del suelo. Se corrige el mapeado de cada copia de contacto y se recorta de forma irregular el borde del material para evitar el aspecto de loseta. No se cambia el emplazamiento guardado ni el donante original.

## Cambio de hipótesis para la Brecha

La previsualización de la talla escalonada conserva el defecto principal: una zanja en Y de profundidad uniforme. La referencia tiene una depresión central rodeada por placas fracturadas y ramas que pierden profundidad. Se prepara una alternativa macroscópica: conservar el eje y las exclusiones existentes, reducir gradualmente la profundidad de las ramas periféricas, tallar cortes poco profundos entre placas irregulares en el entorno central y concentrar el hundimiento en el corazón. No sumar losas flotantes. Todas las superficies de detalle se reproyectan sobre la misma malla; la vegetación existente acompaña verticalmente al suelo.

La panorámica también necesita atmósfera en el borde de la región visible, no solo en el borde lejano de la continuación que queda fuera de cámara. Se prepara una franja estrecha e irregular de bruma únicamente en el zoom panorámico. La vista regional permanece libre de niebla y se restaura el estado de la cámara al terminar cada render.

## Primera revisión real de Unity: 40291321336

Importación y fuente PASS. PlayMode: 60/62, sin omisiones. Dos defectos concretos: el suelo de Brecha se exportó a 2 mm cuando su contrato exige 15 mm (error medido 13,3 mm); se restablece el desplazamiento de 15 mm sin cambiar tolerancias. La nueva prueba de selección consultó `Collider.bounds` mientras la vista de ciudad desactiva su física; se corrige para medir el volumen local real y se fija explícitamente el zoom cercano.

Las capturas reales confirman nodos mayores y el puente corto. Se rechaza el acabado de pradera de esta iteración: atenúa el grano original en cercana y genera camuflaje en panorámica. El control sin acabado conserva mejor la textura. La siguiente versión preserva el grano del núcleo, comprime la variación cromática con la huella de píxel y calibra la continuación al verde/dorado del núcleo, con caminos separados. Se mantiene la crítica de la zanja en Y; la alternativa de depresión fracturada sigue pendiente de observar en Blender y Unity.

## Cauce y transición geológica

Las capturas reales de criaturas y cantera revelan agua tapada por el terreno fuera del núcleo. La inspección del GLB y de sus alturas detecta terreno por encima de 437/464 centros de triángulo en la extensión oriental del río y 795/1292 en la occidental (muestreo de cuadrícula, diagnóstico, no prueba exacta de contacto). Se excava únicamente la huella de las superficies de agua existentes, conservando los tableros y las máscaras jugables. No se inventan cursos nuevos.

La alternativa de depresión central de Blender 40293974247 cambia la masa, pero su material gris uniforme forma un parche y las normales planas acentúan triángulos alargados. Se conserva la geometría de depresión, se suavizan las normales modificadas y se pinta un pigmento editable de placas y grietas con transición a suelo, con UV comunes en terreno y recubrimientos. La revisión de Unity sigue siendo obligatoria.

## Jerarquía regional y continuidad de partidas

El código ocultaba la ciudad a zoom >=152, antes de la escala regional. Se mantiene únicamente la representación de la ciudad realmente desbloqueada, con tamaño regional estable (aprox. 8–10% de altura de pantalla) y su mismo destino seleccionable. Se oculta en panorámica. No se añaden ciudades a las reservas vacías. Las criaturas ajustan su apoyo visual al terreno vigente sin reescribir coordenadas guardadas.

La ejecución Blender 40295461620 agotó el tiempo durante la modificación. Se identificó un acceso a normales de vértices inmediatamente después de cambiar suavizado de cada cara, susceptible de recalcular toda la malla repetidamente. Se agrupan los cambios y se leen las normales una vez; se añade registro de etapa. La recuperación comprueba PID, ejecutable y ruta exacta de la sesión propia antes de detenerla; se reinicia desde la fuente inmutable, no desde un resultado parcial.


## Connected ground candidate — native review next

BlenderMCP 40297426643 completed after batching terrain normal evaluation. Source preserves 51,828 protected vertices and all 62 original packed images; 11,428 riverbed vertices corrected below existing water. Near bridge remains 21 m. Actual Blender renders inspected: river continuity improves and fractured basin replaces regular Y-wall composition. Basin edges and regional tonal integration still require actual Unity assessment; no 7.5 or art approval claimed. Next native run includes corrected detailed meadow, regional selectable city, 85% resource width, creature grounding and panorama-only peripheral mist. Materialization remains conditional on all 62 PlayMode / 26 EditMode / 44 functional checks and fresh native evidence.


## Genuine Unity review 40299103664: retain close gain, reject remaining seams

Exact source cd24ed90abfb9306bcb08fce5d53ba5ba9a2f63c; materialized df781015159ebc0bbc1b38642b977f698bdc29b8. Downloaded XML verifies 62/62 GPU PlayMode, 26/26 EditMode, 44 probe checks, no skipped cases; 160 actual-camera image hashes verified. Close original detail and paths return; 85% nodes and regional selectable city pass. This is NOT the final 7.5 acceptance.

Actual six-view review exposes three visible faults: extension-only palette replacement erased source paths/grain and draws a rectangular seam; dark pigment covers the whole fractured basin rather than only its heart; peripheral haze varies blur width but retains four straight outer boundaries. Change hypotheses: use one source-preserving pigment operation across all meadow materials; retain meadow on outer geological plates with connected dark cracks and a central mineral wound; use a rounded irregular perimeter for panorama-only atmosphere. No regional fog.

Independent GLB triangle-centre sampling (64 nearest triangle candidates) detects 0/438 and 0/448 remaining tributary occlusions, but 73/464 east and 18/1292 west river-extension occlusions. Some triangles bridge the ribbon although their vertices lie outside it. Include the immediate 4.5 m bank skirt in physical excavation and refit existing rooted scenery, with the protected core and bridge decks unchanged. Unify BOTH existing basalt material slots so the four old branch tips stop reading as unrelated pale fins. Source files and rejected images remain retained.


Blender 40302507916: river continuity is visibly restored and outer plates retain meadow. The four old canyon tips still have a physical silhouette; recolouring alone is insufficient. Before Unity, remove the residual 16% outer trench depth entirely with a continuous radial taper reaching zero at 112 m from the wound; retain the central physical depth and surrounding shallow cracks. This is a geometry correction of the diagnosed tips, not another colour-only pass.


40303337549 flattens outer trench relief but the triangular pigment boundaries remain visible: constant replacement green cannot match the authored surrounding image. Final source transition samples the actual continental pigment bilinearly into the new basin image, preserving all originals, and uses the same grass-grain/finish treatment on outer rock. Geological darkening tapers away beyond the central wound. Independent GLB UV/image sampling at 237 boundary pairs finds original core/extension median RGB ratios 0.985–1.022: original pigments are already close; divergent runtime finish was the rectangular-seam cause.


BlenderMCP 40304157232 inspected at close and regional scales: pale triangular fins no longer appear; outer fractures fade into the actual continental pigment; river is continuous through the reviewed settlement region. Retain for full Unity verification with unified meadow finishing and rounded panorama-only atmosphere. No final art score assigned from Blender.


## Unity 40304622115 and painted-ground refinement

Actual 61/62 PlayMode, no omissions; one floor-contact error 11.3815 mm against the unchanged 5 mm maximum. Runtime terrain/query/collider consistency tests pass. Fit a private decorative floor mesh once to the actual imported contact surface, preserving x/z and all terrain vertices; refuse corrections above 25 mm, retain +15 mm design offset and keep the 5 mm acceptance threshold. This corrects imported-overlay contact, not terrain or game data.

Actual Unity images now show the continuous river, removed pale fins, coherent near detail and rounded peripheral atmosphere. The remaining broad grass lacks the illustrated tuft/earth texture. A new original bitmap brush was directed with built-in imagegen (no paid API path): seamless flat top-down stylized medieval grass/ochre earth albedo, 80/20 grass/soil, tiny pebbles and grass strokes, no scene objects, text, perspective, shadows or repeated polygon cells. Original saved PNG is preserved unchanged as RegionOnePaintedMeadow.png. It is applied as a mean-normalised world-space detail over existing grass, protecting actual paths, fading at panorama footprint and excluding the mineral core. This is a new visible material hypothesis, not another broad-colour tint. Keep only after actual Unity A/B; source references and 62 packed images remain unchanged.


## Regional forest hierarchy: measured cause and bounded correction

Current genuine Unity40304622115: direct visual estimate close7.2, regional6.4, panorama6.2; overall approximately6.8, not7.5. Near source fidelity/river/resource hierarchy retained. A simple illustrative dark-green RGB pixel proxy (G>1.18R,G>1.12B,maxRGB<.65) covers28.22% of regional portrait versus9.07% of the reference artwork content; near is8.55% versus12.63%, so thinning the close source would target the wrong camera. This is colour coverage, not an exact vegetation-area measurement or beauty score.

Root cause is actual production LOD enlargement: continental crowns20-26m, heights34-41m; retained far crowns20-24m, heights34-40m. Reduce BOTH matched far families to12-16m crowns /18-23m heights. Small ecotone crown14m and20m height, with coverage probability recalibrated to its new footprint. Preserve every near canopy, existing anchor, shared source mesh, resource/creature and gameplay mask. This is one regional/panorama composition correction rather than deleting actual woodland or making the close camera emptier. Keep all original near/far imports and the existing matching-envelope tests; actual Unity comparison required.


40310659122 genuine Unity:61/62, floor-contact assertion now passes; same test then exposes a2.798532m mismatch at the regional/continuation boundary. Excavating the regional river bed must update BOTH mesh copies at that edge. Actual Blender correction now excavates the same existing water footprint on the continuation and copies exact shared-edge heights; masks and terrain vertex count stay fixed.

Painted close detail retained provisionally. First distance-map road enlargement rejected: colour-segmented source earth includes broad field stains, and65% opacity /5.5m growth becomes flat, thick ribbons in real panorama. Replace the segmentation with the existing authored Catmull-Rom route control points from build_region_from_ai.py, a softer edge and bounded opacity; no gameplay route or target changes. Regional far-canopy correction remains grouped with this composition pass.


## Actual Unity40313214146 — all technical checks pass; primary albedo pivot

62/62 PlayMode,26/26 EditMode,44 progression checks passed; materialized0eaafe49cf1502be105c0beca1b2e33a94b07d1e. Six actual views inspected. Far crowns no longer form oversized walls; tracks are softer and source-continuation gap is repaired. Retain this as the first complete safe checkpoint of this direction. Direct visual estimate remains around7.0 overall, not7.5: close detail improves, but the broad meadow still carries the old muddy pigment and the protected-rectangle contrast remains noticeable.

Decision: stop treating the new authored bitmap as a neutral multiplicative detail over the old look. Use that real painted albedo as85% of the grass surface, preserving original exposed paths and the mineral core; derive only a bounded value variation from the previous pigment. One palette for both core and continent, same world-space brush/texture filtering at every zoom. This is the primary-surface material alternative, not another low-frequency sine tint. Keep all original textures/UVs/geometry and the now-certified scaled forest/resources. Re-render actual Unity before assigning the target score.


## Macro style pivot after Unity40316305875

Primary painted grass is a retained surface improvement but not a sufficient jump to declare7.5. All62/26/44 pass; direct inspection of five real views still shows one conifer silhouette family, highly repetitive woodland and a dark polygon fracture with only three isolated purple mineral spots. Do not award the target for another tint.

Decision: author a genuine rounded broadleaf derivative for one of the two continental canopy families, preserving both original pine datablocks, all original Pilot pines, all anchors/masks and exactly two shared continental near donor meshes. Observe actual donor bounds and materials before construction; produce an asymmetric five-lobe low-poly crown with a forked trunk, four warm-green leaf shades, and a separate reduced far mesh. This changes real silhouette and material family with no paid assets. Keep the original source and share the new geometry across the existing instances. Quick real Blender forest/scene previews first.

The same coherent style pass reinforces actual geological vein pixels using their existing violet pigment, within75m of the wound; the central mineral is more emissive and the existing three real lights remain. No portal, targets or new fracture mesh; physical terrain is the already-certified shared surface. Keep only after native Unity review.


Blender40319616586 inspected: near-forest, near-town and regional previews show a real mixed rounded/pine silhouette; retain for Unity. Actual derivative reuses2482 existing anchors,274near vertices/440faces and60far vertices/88faces, original donor meshes preserved. No terrain/UV/mask/core-image edits beyond the already verified source direction. Native source/test diagnostic text updated to describe two shared source-authored families; all assertions and tolerances remain identical.


## Close hierarchy after actual Unity40320274388

Mixed crowns and violet veins are real improvements in the regional image, all62/26/44 passed. Detailed wood/boar portrait images expose a remaining hierarchy fault: some crowns exceed the20.4m gather-node footprint. Existing LOD transition37-47 also selects strategic forest geometry before the portrait close zoom64. Correct the relevant presentation layer rather than enlarge nodes again: cap close canopy width at38% of the24m city reference (9.12m), preserve each base and source mesh, and crossfade close-to-far at80-110 so BOTH close cameras show the close family. Retain12-16m strategic crowns and all source anchors/masks. An independent renderer-bounds assertion now requires gather nodes to exceed twice the maximum near-crown width; no previous threshold or test removed.


## Near contact and bridge reading, after Unity40324796014

Observation: all62/26/44 pass,160camera manifests and34general-image hashes independently verified. Close forest silhouettes improve (7.856% materially changed landscape pixels,0.063% portrait); panoramas are pixel-identical. Do not inflate this small final hierarchy correction into a global art acceptance. Actual Breach close imagery reveals hard coating boundaries; nearest actual GLB terrain vertices coincide exactly in XZ, while independent normal comparison finds29degrees difference at the99th percentile and93degrees maximum. The coating must inherit the physical ground's lighting normal, not merely its height.

Decision/correction: interpolate imported terrain normals while performing the existing private15mm coating contact fit. Preserve every source mesh, world position, UV, pigment and gameplay surface. Add an independent imported-mesh witness requiring the coating normal to agree with a coincident actual terrain normal, retaining the original5mm height assertion.

Bridge inspection: the actual renderer survey confirms all three BLD_CrossingRails objects enabled/active; the near rail geometry is9.2m wide,2.08m high,18.68m long. Posts are already authored. Correct their washed-out imported oak material with a darker warm finish, not duplicate geometry or alter the measured21m crossing. This is a scoped Unity material correction.

Verdict: retain source/canopy checkpoint, review both contact and bridge corrections in actual matched Unity cameras before the integration judgment. No new paid generation; no ART_APPROVED or physical-mobile claim.
