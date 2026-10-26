# Auditoría forestal de la Región 1 integrada

Base: main tras PR112. Evidencia de observación: artefacto 12674022008, run
40370249261, capturas genuinas y `world-wide-renderer-survey.tsv`. Comparado
directamente con el tríptico canónico de 22-10. Este inventario no mide FPS móvil.

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

Conservar fuentes, pinos protegidos y ecotonos útiles como primera intervención.
Priorizar retirada visual de copas genéricas recientes. PR113 propone ocultar
1.664 de las 4.988 (33,36%), con identidad determinista compartida entre near/far.
Esto es reversible y no regenera el terreno, pero una selección por hash NO
garantiza claros coherentes ni mejora artística: exigir comparación visual real.
Si queda el efecto de confeti, pasar a retirar subgrupos completos según composición
y accesos, manteniendo manchas de bosque útiles. No acumular porcentajes globales.

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

Pendiente de cerrar con el resultado exacto de PR113: seis pares de cámaras
cercana/regional/panorámica horizontal/vertical, matrices y dimensiones iguales,
capturas sin retoque, estabilidad de foco/selección/guardados y suite integrada.
Registrar si realmente mejoran claros, recursos, amenazas y coherencia, además
del censo. Menos renderizadores no acredita FPS, memoria liberada ni ART_APPROVED:
los meshes y fuentes permanecen cargados. No publicar APK por esta corrección.
