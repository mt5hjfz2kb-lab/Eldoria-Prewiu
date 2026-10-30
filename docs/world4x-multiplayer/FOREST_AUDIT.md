# Auditoría forestal de la Región 1 integrada

Base visual final: main tras PR112, run40387268720 (artefacto12686696359).
Candidata revisada: run40394742870 (artefacto12687737377). Seis pares auténticos
con cámaras equivalentes, contrastados con el tríptico canónico de22-10.
El censo inicial se contrastó con `world-wide-renderer-survey.tsv`. No mide FPS móvil.

## Qué está activo y qué significan las cifras

`RegionOneCanonicalLandscape.Build` instala `RegionOneBlenderLandscape`, que
importa el GLB y aplica `RegionOneProductionLod`. No instala
`RegionOneHybridForestImpostors` en ese recorrido. `RegionOneAuthoredComposition`
es utilizado por el terreno híbrido histórico; no debe sumarse como otro bosque
simultáneo de la presentación canónica. El código antiguo se conserva como fuente.

El manifiesto registra **5.047 árboles continentales**. El censo nativo confirma
5.047 renderizadores cercanos `BLD_ForestMass_`, de los que 59 son ecotonos y
4.988 copas genéricas; hay otros 5.047 renderizadores `BLD_ForestFar_` emparejados.
Esas parejas son representaciones LOD del mismo elemento, no 10.094 árboles físicos.
El censo cuenta además 462 `Pilot_Pine` y 462 `Far_Pilot_Pine`. El estado de un
renderer en el inventario no equivale a visibilidad efectiva en un fotograma.

Las **471 instancias trasplantadas** son un dato de operaciones de producción en
`living_geography`, no un recuento de pinos actualmente visibles. La fuente
`authoring/sculpt_living_geography.py` lo resuelve: recorre los mismos
`BLD_ForestMass_` y aumenta `transplanted` al elegir un emplazamiento hacia la ribera,
sin crear instancias. Incluso puede volver al punto previo por ocupación después
de incrementar el contador: no debe leerse como censo final de anclajes desplazados.
Son operaciones sobre el conjunto continental, NO otros 471 árboles adicionales
ni los 462 pinos. El LOD ya selecciona una fracción
de pinos lejanos y de ecotonos pequeños; mantener esa distinción al medir la poda.

## Juicio visual y decisión

La captura regional muestra copas claras redondeadas repetidas, mezcladas con
coníferas pequeñas, distribuidas como una textura de puntos. Algunas masas
conservan contornos reconocibles; otras llenan accesos y compiten con criaturas y
nodos. El canon propone grupos menores, claros amplios y jerarquía del contenido.
La vista cercana necesita conservar árboles de mejor silueta como encuadre, no
copiar todas las masas regionales a escala de primer plano.

La primera selección por hash (1.664 de 4.988 copas genéricas) se rechazó tras
examinar seis pares auténticos: perforaba las mismas masas sin retirar la silueta
artificial ni mejorar suficientemente su composición. Se cambió de enfoque.

La corrección retenida oculta el modelo compartido de copa redondeada y las cohortes
de relleno `Context`, `Belt`, `Regional` y `Castle`. El GLB contiene 2.482 copas
redondeadas; 1.544 elementos pertenecen a esas cohortes, con solapamiento. La unión
retirada son **3.251 de 5.047 árboles continentales (64,4%)**: permanecen **1.796**,
incluidos31ecotonos de abeto, y los **462 pinos originales**. No sumar categorías
solapadas ni representaciones LOD. Las decisiones near/far son idénticas.

Las fuentes de producción explican la selección: `build_region_from_ai.py` añadió
esas cohortes para rellenar espacios y `interactive/command.py` transformó un
modelo compartido en la copa redondeada repetitiva. Se conservan las fuentes Blender,
mallas, objetos, transformaciones y metadatos Unity. La intervención de visibilidad
se revierte aisladamente sin reconstruir la geografía.

La revisión independiente observa una mejora clara en cercana horizontal y las dos
regionales; moderada en ambas panorámicas; cercana vertical sin cambio significativo
ni regresión. Se conservan bosquetes, ribera y pinos, con claros más continuos junto
a Valoria. Persisten coníferas repetidas y puntos lejanos. Véase
[revisión de las seis vistas](FOREST_VISUAL_REVIEW.md); no es aprobación artística
global ni prueba de transiciones animadas.

## Límite funcional importante

`RegionOneTerrainRoutes` construye obstáculos desde las envolventes de los árboles,
incluso aunque sus renderizadores estén ocultos. `forceRenderingOff` no altera
ese grafo. Por tanto esta poda es visual, no abre nuevas rutas ni plazas edificables.
Conservar ese comportamiento durante la corrección protege marchas y guardados;
no prometer expansión de suelo utilizable por borrar una copa de la imagen.
Una futura revisión de navegación debe ser explícita, certificada y separada.

Los nodos de madera/piedra pertenecen a `LocalGateway`/`RegionOneResourcePlacement`
y `RegionOneResourceModels`, con identidad persistida y regeneración. No son los
árboles decorativos. Esta intervención no debe tocar sus cantidades, posiciones,
IDs, colliders, regeneración ni rutas guardadas.

## Validación de la candidata

Los seis pares cercana/regional/panorámica horizontal/vertical tienen dimensiones,
pose y proyección equivalentes y hashes PNG originales verificados mediante
`verify_capture_pairs.py`. El verificador artístico existente ha emitido
`WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS (NOT ART_APPROVED)` usando las fuentes
BLEND históricas exactas y sin afirmar una nueva sesión Blender/MCP.

El resultado técnico final se registra por separado en FOREST_VISUAL_REVIEW.md.
Menos renderizadores no acredita FPS ni memoria liberada: mallas y fuentes siguen
residentes. No se publica una APK por esta corrección.
