# AF/AG → Región 1: receta de producción y conservación

Estado: producción abierta. No es aprobación artística, certificación móvil ni permiso de fusión. Relevo autorizado del mismo PR #100, revisión 19; se conservó íntegra la ejecución 40038480141 (26 pruebas, 16 capturas) y su commit de binarios 9a3587642176a52d1071d67430b6a4309b95b012.

## Fuentes y decisiones de fabricación

| Etapa | Herramienta y operación verificadas | Parámetros/fuentes recuperables | Decisión |
|---|---|---|---|
| I → K | Blender bpy, edición de islas de follaje y perfiles de orilla; recuperación del pigmento exacto desde GLB | 156 árboles preservados; 22 acentos con dos copas esculpidas; 4 interrupciones de orilla; `ribera_branch_sculpt.py`, `SOURCE_REOPEN_RECOVERY.md` | Retener copas/perfiles y agua I; descartar piscinas J y manchas L |
| K → O | Escultura de margen, atlas conectados a la coronación real y aplicación de familias K | 20 trazos, atlas 2048×512, 134 copas restantes; `ribera_bank_paint_and_firs.py`; O run 39965511083 | Retener O; N transparente resultó turbio/plano |
| Q → AF | Pintura UV estratificada sobre relieve y claros existentes | `finish_connected_sector.py`; AF run 40010956718, commit 6f866fa3e71dcc752cc1529f157ccd17c8553546 | Retener flujos amplios, excluir hierba AE del export |
| AF → AG | Tres lóbulos forestales conectados, exclusión de caminos y LOD híbrido | AG run 40012204581; commit 84ce3ae48b1818b6bee7b4420d32ce5fa3df4169; 429 árboles; transición ortográfica 37–47 | Retener AG; AH introduce placas rocosas rechazadas |
| AG → AI | Extensión soldada de 100 m al norte, UV nuevos solo en caras nuevas | `extend_living_sector.py`; fuente AI SHA256 28f5bc9665145244fb3b3ac400983e184ef664afdd6da0b8c04dac6fae5a23fc, run 40024930128 | Conservar fuente como entrada continental |
| AI → continente, defecto identificado | Se asignó `Region1 broad continental pigment` a caras originales y se reemplazó su UV | `build_region_from_ai.py`, anterior a ac87016f | Regresión: vértices preservados no garantizan materiales preservados |
| Reparación del relevo | Conservar asignaciones originales; normalizar solo nombre de capa UV; comparar caras/material/UV después de unión | `retained-core-integrity.json`; caras y hashes de píxeles de imágenes; nueva captura nativa requerida | 50688 caras y 30 imágenes preservadas; 96 muestras de importación y 26 pruebas pasan en run 40040528055. PNG originales AG y reparación revisados: pigmento recuperado, iluminación aún más brillante antes del siguiente pase. |

## Receta AF que se debe transferir, no sustituir por ruido global

1. Abrir copia del BLEND fijado por SHA; no usar un export continental degradado como origen del pigmento. Recuperar materiales/imágenes realmente enlazados al Principled BSDF.
2. Mantener las tres rutas Bézier; rasterizar la máscara original con ancho 1,65 m. La exclusión del bosque considera margen 3,2–3,7 m. No pintar caminos nuevos desconectados de la geografía.
3. Conservar la máscara de hojarasca derivada de posiciones reales de árboles (radio de influencia 6,7 m; máximo 0,44), separada de los trazos de pendiente y de la máscara de caminos.
4. AF compone tres masas con centros (-55,98), (-6,91), (52,93), semiejes (30,19), (37,17), (31,21), mezclas 0,39/0,43/0,39. Sus ejes ondulan según el relieve. Son parámetros del sector original, no coordenadas para estampar repetidamente en todo el continente.
5. Guardar `AF broad meadow flows` como sRGB, y `AF editable slope glaze mask` / `AF editable connected litter mask` como datos Non-Color. `use_fake_user=True` conserva máscaras sin uso directo.
6. Guardar PNG y ejecutar `image.pack()` para fuentes GENERATED o FILE. Reabrir el BLEND y verificar las imágenes: filtrar por `image.type == IMAGE` perdió el suelo de I en un ensayo anterior.
7. Exportar solo mallas visibles. AE continúa editable pero excluida; no incorporar AH ni fuentes de copas auxiliares ocultas como vegetación jugable.
8. Al unir superficies, mantener valores UV por esquina y asignación de material por cara. Igualar nombres de UV evita capas vacías en UV0. No remapear las caras originales al atlas global para esconder una unión.
9. Diseñar las transiciones en el área nueva con pigmento y relieve derivados del borde original; medir geometría y mirar imágenes. Una coincidencia de alturas no acredita continuidad artística.

## Unity y comparación reproducible

El piloto AG usa `Eldoria/RiberaFade`, sol (48,-35,0), sombras suaves 0,9 y distancia de sombra 300 m, restaurando el estado al terminar. La producción continental utiliza los materiales importados y su iluminación de juego. Por tanto, recuperar únicamente la textura no prueba equivalencia de acabado: revisar también shader, espacio de color, sombras y LOD. No modificar la iluminación de Valoria globalmente para favorecer una captura.

Cámaras originales del sector: objetivo respecto a WorldHome interpolado de (20,2,95) a (0,2,50), cámara objetivo+(0,130,-145), LookAt objetivo. Horizontal 1280×720 y zoom 24/38/52; vertical 720×1280 y zoom 32/52/74. Se trata de tres escalas **del sector**, no de la panorámica de Región 1.

La captura continental conserva sus 16 PNG y añade `retained-sector/`: seis PNG con las cámaras anteriores y 25 fotogramas por orientación. JSON de cámara, SHA256 y contexto identifican las diferencias de iluminación. No presentar una comparación con luz diferente como identidad píxel a píxel. Los PNG proceden de Camera.Render en PlayMode; su cadencia no mide FPS físicos.

## Juego y condiciones de cierre

Se conservan LocalGateway, PlayerState y rutas sobre triángulos importados. La ejecución 40038480141 validó rutas, recolección, reaparición tras reinicio y demás suites existentes; una nueva modificación geométrica exige revalidarlas. La selección de una ubicación tiene que demostrar acceso real, además de terreno plano y ausencia de exclusiones.

Antes de integración: comprobar restauración artística en el sector; elevar el continente con masas diseñadas, relieve/riberas y máscaras específicas; revisar tres escalas/dos orientaciones y zoom; reconciliar main y ejecutar regresiones; identificar límites físicos y obtener aprobación del propietario. El estado del encargo sigue abierto hasta cumplir esos criterios.

## Pase continental dirigido tras revisión nativa 40040528055

La comparación de los PNG auténticos muestra una mejora real del núcleo pero rechaza el continente anterior: placa rectangular perceptible, praderas sin diseño, 25 agrupaciones forestales aisladas y oclusión del río junto a los bordes del sector. El shader PBR de producción ilumina las copas más que el shader AG. Esta observación precede a las operaciones siguientes; no es una puntuación ni aprobación.

Decisiones ejecutables: mantener el 100 % del núcleo; extender su color de borde mediante raycast + material + UV originales en una franja nueva de 55 m; pintar nueve flujos de pradera y tres glaseados sobre las elevaciones existentes, con máscaras independientes guardadas en Blender; ampliar y romper las siluetas forestales conservando caminos, riberas y reservas de ciudad; usar sus dos fuentes de copa editables con colocación no reticular y variación de escala. Continuar la sección transversal real del río desde el primer vértice, sin el retraso anterior de 60 m; terminar el agua en la costa. Igualar exclusión fluvial al ancho visible. Conservar todos los tests de rutas reales.

Restauración de look development: clonar la receta de shader AG sobre materiales de geografía, conservar textura/factor originales y aplicar el sol histórico solo durante el render de la cámara de mundo. Restaurar estado de luz y distancia de sombras al terminar; no editar ciudad ni UI ni el material fuente. Añadir 25 fotogramas por orientación de la transición cercana → continente, aparte de los 50 del sector. Evaluar coste físico por separado: el inventario de run 40040528055 encontró ADB disponible pero **cero dispositivos Android físicos**; iOS no medido.

Pendiente de revisar el resultado de este pase; no confundir decisiones de autoría con mejora ya demostrada. La forma exterior de la región y la calidad final de Brecha todavía deben evaluarse después de las correcciones anteriores.

### Presupuesto de geometría y continuidad de exclusiones

Inspección directa del GLB verificado `d930f951…` (74.794.436 bytes): los 25 grupos continentales duplicaban aproximadamente 47.400 vértices cada uno; los dos donantes originales tienen 4.246/4.252 vértices exportados. Aumentar los árboles manteniendo esa duplicación es un enfoque que debe descartarse, aunque el render pueda terminar. La siguiente preparación conserva transformaciones individuales y comparte dos mallas LOD0 derivadas al 40 % y dos LOD1 al 16 % de esas derivadas. El núcleo usa sus mallas originales. Los datos editables se guardan comprimidos, sin reducir ni sustituir texturas originales.

El LOD continental pasa a usar tamaño de pantalla de cada árbol, umbral 0,065, en lugar del tamaño del bosque completo. Los LOD distantes siguen siendo geometría de la misma copa, no tarjetas prematuras. Unity debe verificar que el número de instancias coincide con Blender y que solo existen dos mallas cercanas compartidas. El shader conserva la receta AG y añade instanciación y dither de LOD. Esto describe una reducción de duplicación, **no FPS físicos certificados**.

Los claros de caminos, reservas, riberas y accesos a puentes deben coincidir entre pigmento, árboles y máscara navegable. Se corrige la máscara que todavía podía señalar bosque en riberas donde la colocación ya excluía árboles. El cálculo escalar optimizado de distancia a caminos/fallas se comparó con el vectorial en 600 casos: error máximo 0.

La atmósfera periférica preparada afecta únicamente a los últimos 75 ± 29 m del borde y se activa al alcanzar el 85–100 % del encuadre panorámico según aspecto. No cubre el sector, la Brecha ni la vista regional. No sustituye relieve ni pruebas de unión; debe revisarse en los PNG siguientes y descartarse si disimula defectos interiores o produce un efecto de isla.


### Rutas y composición forestal

El servicio existente toma las cajas de los renderers como obstáculos. Al unir cientos de árboles en una sola malla, esa caja engloba los claros y corta caminos que sí se ven libres: no se resuelve debilitando exclusiones. Las instancias individuales conservan la geometría y proporcionan obstáculos por copa. La separación de árboles nuevos respecto a caminos aumenta a 16 m y los accesos de puentes se despejan considerando el radio real de copa; el sector original mantiene sus árboles salvo las exclusiones históricas ya justificadas.

El índice espacial de obstáculos solo reduce comparaciones: mantiene el mismo predicado de caja ampliada 1 m y las mismas muestras del MeshCollider. La conectividad ortogonal se calcula una vez por conjunto de celdas ocupadas y origen, con invalidación por cualquier cambio. Mantiene comprobaciones de agua/falla en el punto medio, desnivel 3,2 m y aproximación máxima 24 m. La prueba real de rutas añade un cambio de ocupación en una copia aislada del estado para exigir desvío o rechazo, y luego restauración de la ruta determinista; no toca guardados ni elimina casos anteriores.


La transición geométrica nueva prolonga las pendientes medidas en el borde durante los primeros metros y las amortigua antes del relieve continental, además de conservar altura exacta. Se prepara una costa oriental con entrantes de amplitud desigual y pigmento de plataforma marina; los puntos de mar y tierra exigidos por las pruebas permanecen clasificados igual. Estas operaciones afectan la ampliación, no las riberas ni texturas originales del sector.


## Resultado rechazado 40042023684 y corrección preparada

Fuente `0b65fea`: Blender y la importación conservan 50.688 caras, 30 imágenes y 96 muestras; se exportaron 5.283 árboles continentales. PlayMode: **20 PASS, 6 FAIL**, 1.192,63 s. Solo 26/48 recursos colocados y 7 reservas accesibles; no se materializó el candidato. La captura agotó su límite de 180 s y produjo nueve vistas parciales, sin la serie completa. Se conservan el XML y ZIP `9ab7f1da438e423e1a4c0ec5c6430d16429aeb1288556e9d364aa499db17f7b7` en el artefacto 12540329339. No ampliar tiempos ni reducir exigencias para aceptar este pase.

Revisión de esos PNG: el sombreado cercano se aproxima de nuevo a AG y las masas forestales ganan extensión, pero se rechazan las franjas longitudinales del glaseado de borde y el contraste rectangular persistente. Corregir la extrusión de grano: suavizar el color amplio del borde con núcleo gaussiano de 49 muestras, sigma 9, y extinguir el detalle unidimensional en 2,5 m. La franja de 55 m conserva únicamente el color amplio.

Se inspeccionó directamente `AF_broad_meadow_flows.png` extraído del GLB verificado. Se seleccionó un fragmento de hierba libre de caminos/manchas (u 0,02–0,075, v 0,90–0,98). Su media determina la base de la ampliación; el detalle normalizado y reflejado se guarda en `RegionOneGrassDetail.png` como datos Non-Color y se repite físicamente a 21,12 × 14,72 m solo sobre la superficie continental nueva. Peso 0,85, mipmaps y filtrado anisotrópico; el núcleo sigue usando su atlas exacto. Es transferencia de material original, no una textura simplificada que lo sustituya.


## Native recovery 40045435899 — verified, artistic production remains open
Source `3d09adfbc4715229850015989691e54e7891edd5`, materialized source/binaries `bf5798929b6227f5172689b62dec0f3ece276357`. Unity XML: 26 passed, zero failed/skipped; 139.72 s editor suite, not a mobile benchmark. Source gate: 50,688 unchanged original faces, 30 source image hashes, 96 imported material/UV samples; 5,134 continental near trees sharing exactly two meshes. Main capture count remains 16; retained sector 6 plus 100 zoom frames = 122 current genuine PNGs.

Visual artifact 12541268503 SHA256 `138f411c63c7869c4cc62c17d1672b8af28ed0454a96bfbaa5a91893e888af8e`. The original sector pigment and restored shader are retained. Continental meadow repetition, isolated forest shapes, simple shore profiles and visible retained-core composition still require improvement; this is explicitly NOT final artistic acceptance.

Next controlled experiment preserves the source pixels and geometry, but high-pass normalizes only the derived meadow detail (Gaussian sigma 7 source pixels), blends two rotated detail samples and removes subpixel detail at regional distance. Unequal woodland belts use the same geographic exclusions and shared crowns, with young/old scale variation 1.3–3.9. Real O cutbank endpoint profiles and original clay/wet-toe materials continue into the new river. Core LOD switches through a deterministic spatial 37–47 m orthographic band, including shadow fade; no historical core near mesh is simplified. Four additional close views identify actual continental wood/stone resources in two orientations. These changes require another native validation and image review, not an inherited PASS.


## Comprobación cuantitativa de conservación y corrección macroscópica
En cámaras auténticas coincidentes AG frente a 40045435899, diferencia media absoluta RGB (0–255): horizontal cercana 0,712; vertical cercana 0,284. Fracción de píxeles con alguna diferencia >8/255: 2,14 % y 0,31 %. Las vistas más abiertas incluyen ampliación real y diferencias LOD; no se presentan como identidad ni como puntuación artística. Ambos PNG se inspeccionaron directamente.

La panorámica vertical del territorio casi cuadrado desperdicia gran parte del encuadre. El siguiente pase amplía **terreno real** desde z ±1000 hasta ±1800, conservando x ±910 y todas las coordenadas del núcleo y territorio previo. Añade caminos conectados, ocho masas forestales, reservas urbanas adicionales y relieves norte/sur; no estira la malla, los guardados ni la cámara. La consulta cambia su profundidad exacta a 3600 m, mantiene muestra cada 4 m y obtiene alturas de triángulos. La navegación amplía su cobertura conservando celda 8 m, pendientes, agua y obstáculos. Nuevas aserciones exigen población real alcanzable al norte y al sur de ±1000 y contacto exacto en las nuevas zonas.

El índice de copas conserva la separación de 9 m de los recursos y cubre también los árboles originales AF/AG. Se contrasta contra las cajas reales de TODOS los renderers cercanos, como oráculo independiente; no se reduce la exclusión. Las posiciones persistidas siguen siendo coordenadas absolutas y no se migran ni reinterpretan.

Este pase macroscópico y su coste nativo están pendientes de medir. No ampliar límites de pruebas para absorber una regresión.


## Reproducción operativa y archivos editables
Herramientas nativas verificadas: Blender 5.2.2 LTS (`d13f752e3b9c`), Unity 6000.3.23f1, URP 17.3.0. El workflow `.github/workflows/world4x-canon-native.yml` recupera AI por identidad, comprueba la reserva viva y ausencia de procesos incompatibles, y ejecuta secuencialmente:

```text
blender -b --python authoring/build_region_from_ai.py -- OUTPUT_DIR PINNED_AI_BLEND
blender -b --python authoring/build_resource_library.py -- OUTPUT_DIR PINNED_AI_BLEND
Unity: RegionOneCanonicalSourceGate, después suites PlayMode integradas
```

Consultar los argumentos completos del workflow para evitar inferir rutas de una máquina. Los dos BLEND quedan en `sources/native-RUN/`, con `manifest.json` que enlaza commit fuente, run, SHA256 y bytes de cada archivo. El mundo guarda las mallas originales, materiales/UV, máscaras de pintura empaquetadas, trazos editables de caminos y copas donantes. Las fuentes originales certificadas no se sobrescriben. El GLB, consultas de triángulos y biblioteca de recursos se copian únicamente después de verificar su identidad y pasar todas las pruebas.

La materialización siguiente conserva también los seis PNG principales, sus JSON de cámara/hash, seis vistas históricas del núcleo, cuatro vistas de recursos continentales y XML junto al BLEND. Las series completas de zoom permanecen en el artefacto de ejecución. Esto evita que la caducidad de un ZIP elimine las imágenes esenciales de revisión. Ni esta operación ni las comprobaciones automáticas conceden aprobación artística o autorización de fusión.


### Resultado nativo del pase de orillas y detalle
`b8adc5e25534a28c621dd74ee5f3c823b5d6181b` / ejecución 40047161591: **26/26 PASS**, 145,56 s de suite de Editor; materialización `4d8e707874f419b0983bcd662e5d5f5b93846fd0`. 126 identidades PNG verificadas. Artefacto 12541729864, SHA256 `efa5eb4f4d0c2ba4d5580298804ce310824e97b378e91d3b3ec59490063dc724`. Se conservan 50.688 caras y 30 imágenes; 5.215 árboles continentales con dos mallas cercanas compartidas.

Revisión real: desaparece la repetición extensa de la hierba y mejora la continuidad de las orillas; las masas forestales ya tienen extensión desigual. La composición panorámica sigue siendo insuficiente, por lo que se ejecuta la ampliación longitudinal descrita. Las cuatro vistas adicionales revelan recursos pequeños y piedra excesivamente clara frente al entorno: el siguiente pase reutiliza el mismo shader/material cache AG en sus representaciones, sin tocar el hotspot, y eleva las copas del nodo de madera dentro de su huella existente (XZ ×1,15, altura ×2 sobre el donante original). El atlas distante vuelve a renderizarse desde esas mismas mallas. Los colliders y guardados conservan su autoridad y posición.


### Sombreado multiescala y conservación de copas
El shader AG original estaba diseñado para cámaras a unos 180 m y sombras a 300 m. El continente aleja de verdad la cámara; muestrear sombras sin su fade de distancia puede dejar trazos fuera de la cobertura. El siguiente pase aplica `GetMainLightShadowFade(positionWS)`, conserva coordenadas de sombras de pantalla cuando URP las activa y usa W en el recorte del ShadowCaster. Método oficial: https://docs.unity.com/en-us/engine/6000.5/manual/materials-and-shaders/shaders/writing-custom-shaders-urp/use-built-in-shader-methods/shadows ; la compatibilidad efectiva se certifica compilando en el Unity 6000.3.23f1 fijado. La hipótesis sobre las líneas regionales necesita comparación visual del nuevo run; no se registra como corregida antes de verla.

Las copas próximas continentales conservan el 70 % de los dos donantes en lugar del 40 % anterior; se ensanchan de forma desigual siguiendo la composición AG (X ×1,15–1,5, Y ×1,25, altura ×0,78–1,05 sobre escala de instancia). El LOD distante deriva al 9 % para mantener aproximadamente el presupuesto previo de triángulos lejanos. Las copas originales no se modifican. No confundir compartir dos buffers con tener dos árboles: las instancias y su coste se miden por separado.

La Brecha recibe únicamente una mancha mineral violeta localizada en su anclaje geológico real (-260,150), semiejes 55/65 m y mezcla máxima 0,65; no se añade portal, hotspot, collider ni amenaza ficticia.

La regresión adicional carga JSON de esquema 1 con ausencia real de `WorldResourceLocations`, exige igualdad completa del snapshot serializado después de cargar y verifica posteriormente población, cantidades y victorias conservadas al reiniciar. Se mantienen las 26 pruebas existentes y se añade este caso: siguiente suite esperada, al menos 27 casos y ninguno omitido.


### Ampliación longitudinal verificada
Fuente `c563d46f9fd7f41e3e377b360181fea1df3225a3`, run 40048488067, materialización `03f969dd599eba530cf71bf7ce609c006474244e`: **26/26 PASS**, 167,25 s de Editor. **32/32 reservas urbanas accesibles**, población real en ambas ampliaciones y oráculo independiente de separación de copas satisfactorios. ZIP visual 12543091351, SHA256 `610ceb7665388149fdbd8c99f497037653d0188afd7083c887a2eff3100e53f6`. Las 16 vistas esenciales y sus metadatos ya se conservan junto al BLEND y XML en Git.

El PNG vertical confirma mayor ocupación útil del encuadre y una región longitudinal continua. Aún difiere de las referencias en complejidad hidrográfica, detalle costero y riqueza de transiciones. No se afirma equivalencia completa. La revisión cercana detecta copas de recursos demasiado esbeltas: el siguiente pase corrige XZ a ×1,5 y altura a ×1,7, manteniendo la huella de colocación validada y volviendo a medir apoyo. Se transfiere también el desgaste quebrado de caminos y pincelada intermedia AF a máscaras independientes de la ampliación; mezcla de hierba máxima 0,12, sin alterar píxeles originales.


### Continuidad del sector conservado: experimento controlado
La vista regional real todavía permite reconocer el collar del sector histórico. Se amplía únicamente el empalme de la superficie NUEVA de 42 a 110 m, prolongando las pendientes medidas con amortiguación de 48 m. No se desplazan las caras originales. La veladura derivada de los píxeles originales pasa de 55 a 110 m; el grano fino solo se conserva a 2,5 m del contacto y la paleta amplia se filtra con 193 muestras, sigma 32, para evitar rayas radiales. La máscara de caminos nuevos conserva su autoridad a partir de 3 m del contacto.

Dos arboledas pequeñas enlazan la composición exterior; su tamaño crece gradualmente desde 1,05 hasta la escala continental a 160 m del sector, siempre con las mismas exclusiones geográficas y de navegación. La atmósfera periférica solo actúa en el último 15 % del zoom global y restaura el color de fondo de la cámara al terminar el render; la vista regional continúa sin niebla. Su ancho nominal de 107 m fluctúa por posición, con variación espacial de hasta ±60 m, sin borrar objetos ni cambiar su posición. Una aserción independiente exige restauración exacta del color de cámara. Parámetros sujetos a revisión de PNG nativo, no aprobación anticipada.


### Resultado de sombras y guardados antiguos
Fuente `0901ac3cd9df17d1b29f4c218477faa317d80444`, run 40049855982, materialización `adb788ab0d2ae99e48d5e9ce485ba43b3626758a`: **27/27 PASS**, cero omitidos, 167,74 s de Editor. ZIP visual 12543690827, SHA256 `213d0c9722735c4f4d62edcd910d90c2a44852ac38e4a66338134a62eb988246`. El PNG regional horizontal confirma la desaparición de las líneas negras tras respetar el fade de sombras URP. Las copas cercanas y la proporción del recurso de madera mejoran; la panorámica aún presenta un contorno demasiado regular y el empalme del núcleo continúa reconocible. El siguiente experimento descrito arriba sigue siendo necesario; no se declara aprobación artística.


### Transferencia AF de claros y bordes de bosque
Preparado para el pase siguiente: se reutiliza la construcción exacta de trazos AF de `finish_connected_sector.py` (espina exp(-x⁴), centros ondulados 4,5/2,5 m, borde quebrado 0,86±0,14 y paletas aceptadas I/AF). Dos trazos dirigidos por arboleda se sitúan en su borde real, con anchuras desiguales de 38–71 m y 19–37 m y mezcla máxima 0,43. Conservan caminos, agua, costa y sotobosque mediante máscaras independientes; no son círculos repetidos ni una capa global de ruido. Se evalúan solo sus rectángulos locales y se guarda `AF directed woodland clearing strokes` en el BLEND.

Su propósito es recuperar variedad pictórica a escala de claros en la ampliación, donde los lavados continentales todavía resultan demasiado uniformes en las capturas cercanas. No cambia UV/textura/malla originales, terreno físico, colliders, recursos o navegación. Necesita nueva construcción e inspección nativa antes de considerarse aceptado.


### Revisión nativa del empalme
Fuente `9b0d58ff14e128f2e39d60a5ee0e794f4aee0d71`, run 40051179268, materialización `9bd55a0b9f737ecd2076da7661f97454c69fdd90`: 27 pruebas satisfactorias, ninguna omitida. Artefacto visual 12543708162, SHA256 `ff164798799466194a29069b57c625b57f0e4a2c9df6c8cccd05acc56df8746c`. Se conserva el empalme ampliado y la vegetación de transición. Se rechaza el cambio de fondo a gris: la imagen panorámica auténtica muestra que aumenta el contraste del contorno rectangular. El siguiente pase conserva el color continental previo y la restauración comprobada de cámara, junto con los trazos locales AF descritos arriba. La geografía no se recorta ni se oculta para alterar las pruebas de cobertura.
