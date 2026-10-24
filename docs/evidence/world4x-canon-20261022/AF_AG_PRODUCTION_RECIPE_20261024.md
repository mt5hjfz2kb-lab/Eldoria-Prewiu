# AF/AG → Región 1: receta de producción y conservación

Estado: producción abierta. No es aprobación artística, certificación móvil ni permiso de fusión. Relevo autorizado del mismo PR #100, revisión 19; se conservó íntegra la ejecución 40038480141 (26 pruebas, 16 capturas) y su commit de binarios 9a3587642176a52d1071d67430b6a4309b95b012.

## Parámetros vigentes de reproducción

Esta tabla describe la receta del script actual; cada modificación necesita su propia ejecución nativa. Los resultados históricos de abajo no certifican automáticamente una modificación posterior.

| Componente | Parámetro reproducible |
|---|---|
| Fuente editable | AI fijado por SHA256, derivado de AG/AF; abrir copia, no sobrescribir originales |
| Herramientas | Blender 5.2.2 LTS, Unity 6000.3.23f1, URP 17.3.0 |
| Territorio | x ±910 m, z ±1800 m; metros reales, núcleo x ±96 / z -78…270 intacto |
| Unión | Soldadura 0,001 m; altura/pigmento nuevos se integran en 110 m; pendiente amortiguada a 48 m |
| Río | Sección original preservada al contacto; cauce curvo único; retirada gradual del perfil abandonado en 24 m |
| Pintura | Atlas nuevo 3072²; máscaras independientes y empaquetadas; UV/material original por cara sin remapeo |
| Detalle AF | Fragmento original u 0,02–0,075 / v 0,90–0,98; normalización de baja frecuencia sigma 7; dos muestras giradas; repetición física 21,12 ×14,72 m con mipmaps |
| Pradera continental | Pinceladas AF dirigidas, semiejes 37–83 /14–36 m, mezcla ≤0,47 y máscara propia; pase pendiente de captura nativa |
| Claros AF | Espinas y bordes del procedimiento aceptado, localizados en claros reales; mezcla máxima 0,43, exclusión de caminos/agua/costa/sotobosque |
| Vegetación | Dos donantes originales compartidos; continente cercano 70 %, lejano 9 %; copas cercanas del núcleo sin simplificación y lejanas 16 % |
| Transición AG | Banda ortográfica espacial 37–47 m; misma geografía y silueta, sin sustitución temprana por placas |
| Luz del mundo | Sol (48,-35,0), sombras suaves 0,9 / 300 m; fade URP por distancia; restauración exacta al terminar la cámara |
| Encuadre | Cámara existente: rumbo 0° cerca/regional; panorámica 0° vertical /90° horizontal, transición continua por zoom y límites proyectados sobre los mismos vértices |
| Periferia | Franja exterior 107 ±60 m; transición entre zoom regional×0,75 y ×1,25; mezcla opaca al fondo, sin niebla interior; cambio pendiente de captura nativa |
| Contacto y rutas | Consulta de triángulos reales; cuadrícula de consulta 4 m, navegación 8 m; pendiente 0,9, escalón 3,2 m, agua y obstáculos conservados |
| Recursos | Autoridad LocalGateway/PlayerState; 48 IDs persistentes, huella 8 m, separación de copas 9 m; sin trasladar colliders para disimular apoyo |
| Evidencia esperada | 57 PlayMode GPU sin omisiones +26 EditMode +44 controles I–II; 126 PNG del mundo (16+6+100+4), 34 PNG generales nuevos; AG archivado aparte |

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


### Cauce único en el empalme: defecto geométrico aislado
La revisión de 40051179268 identifica una segunda marca junto al río. El perfil de borde medido mantiene su incisión en y=21 m a la izquierda y y=-52 m a la derecha, mientras `rivercentre(x)` desplaza el río nuevo. Prolongar todo el perfil a 110 m duplicaba parcialmente esa depresión en línea recta. La tabla nativa de alturas (muestras sobre triángulos reales, interpolación de la cuadrícula de 4 m) mide a x=-174/y=21 una cota 2,948 m frente a hombros 3,795/3,395 m; a x=174/y=-52, 4,891 frente a 5,383/5,638 m. Esta marca es distinta del defecto de sombras ya corregido.

Corrección preparada: fuera del núcleo, sustituir únicamente la incisión abandonada por la interpolación de sus dos hombros medidos (semiancho del cauce original +14 m), con entrada gradual en 24 m. Después se excava una sola vez el cauce curvo existente. La veladura exterior deja también de prolongar el pigmento del cauce abandonado. El contacto original conserva exactamente su perfil, material y UV; no se tapa con una segunda superficie ni se cambia el criterio de navegabilidad. La siguiente ejecución debe comprobar las cotas, rutas y PNG auténticos antes de registrar el defecto como corregido.


La atmósfera periférica del siguiente pase mezcla color de forma opaca, como niebla, en lugar de volver transparentes las superficies. Así no depende del orden de dibujo entre suelo, costa y copas superpuestas. Usa el color continental existente convertido explícitamente al espacio lineal mediante un vector; no modifica el fondo de cámara. Continúa restringida al último 15 % del zoom panorámico, con la misma anchura espacial; la vista regional permanece sin niebla. Comprobación visual nativa pendiente.


### Pase AF de claros verificado
Fuente `a7368d0661c1414587f045b208a4335531369cbc`, run 40052558503, materialización `06a9535c94c26dbc6fe052dc167b7617a7d2e5b3`: **27/27 PASS**, cero omitidas, 158,05 s de Editor. Artefacto visual 12544392143, SHA256 `1862587a0e16f0bfa0642bd5aa189622afc9c570f4d6fb94a1b980e702584e53`. Se conserva el tratamiento localizado de claros; el efecto es moderado y no equivale a resolver toda la diferencia con la referencia. Las marcas del cauce original siguen visibles en este pase, por lo que se aplica la reparación geométrica descrita, sin declarar cierre anticipado.

## Verificación independiente de la evidencia descargada

`review_native_evidence.py` no altera imágenes. Comprueba firma PNG, dimensiones, SHA256, escena, versión Unity y procedencia PlayMode declarada en los JSON del workflow nativo; exige 126 identidades actuales y XML con al menos 27 casos satisfactorios, cero fallos y cero omitidos. Contrasta también la capacidad urbana registrada. La autenticidad se apoya además en el workflow/commit nativo y su código de captura, no únicamente en un JSON.

```text
python docs/evidence/world4x-canon-20261022/review_native_evidence.py PAQUETE_DESCOMPRIMIDO --ag-root PAQUETE_DESCOMPRIMIDO/original-ag-review --output verificacion.json
```

La comprobación básica usa Python estándar; la comparación opcional de cámaras AG necesita Pillow y NumPy. Devuelve hashes de cada PNG, hash del XML y diferencias de las cámaras cercanas horizontal/vertical. La comparación mide conservación, no aprobación artística. Nunca convierte duración de Editor en FPS ni certifica un dispositivo físico.


### Cauce reparado y verificación integral del paquete
Fuente `c3fe3925ec6a2d047d0fc235ac286b29547b447f`, run 40053788522, materialización `5f10db8e59ca3d801291d177e03bc933053a8bd7`: **27/27 PASS**, 159,55 s de Editor, 32/32 reservas accesibles y **126 identidades PNG verificadas independientemente**. ZIP 12545340166, SHA256 comprobado `d7a268731ecd1ff9c3eefea844e90fb75181eccfc61916c27df904d24406a8b1`. `NATIVE_REVIEW_40053788522.json` registra el XML y los hashes individuales.

En los dos puntos diagnósticos, el déficit respecto a los hombros pasa de 0,647/0,619 m a -0,016/-0,001 m; desaparecen las dos marcas rectas en el PNG regional. La cámara cercana AG conserva MAE 0,381 horizontal /0,285 vertical sobre RGB 0–255. Estos resultados no son aprobación artística ni benchmark móvil.

### Corrección de encuadre horizontal sin cambiar geografía
La revisión completa de las seis vistas detecta que la ampliación longitudinal ocupa bien la pantalla vertical, pero la panorámica horizontal deja demasiado fondo vacío. No se acepta ese resultado por el solo PASS técnico. Se añade rumbo a la **cámara estratégica existente**, sin otra cámara ni otra geografía: 0° en cerca/regional, 0° panorámica vertical y 90° panorámica horizontal. El rumbo procede continuamente del zoom real entre regional×1,15 y panorámica×0,85; el desplazamiento con teclado sigue los ejes de cámara, arrastre/proyección siguen usando el plano real y el anclaje del zoom se conserva hasta donde permiten los límites.

El ajuste usa los ocho extremos de los límites reales con esa orientación. No recorta, estira ni desplaza terrenos, recursos o guardados. La prueba conserva el requisito de TODOS los vértices dentro del encuadre y añade una ocupación de su caja proyectada superior al 65 % en ambas orientaciones. También verifica el retorno de panorámica a cerca y la proyección del centro sobre el objetivo real, sin mover el recurso. Cálculo previo sobre vértices nativos: 74,68 % horizontal /76,05 % vertical; **no son aún mediciones Unity del nuevo pase**. La siguiente ejecución debe validar la cámara real, capturas y controles completos.


### Encuadre continuo verificado en ambas orientaciones
Fuente `a9635bfce656b4c9ed53aa9cdbb5dfb1a9c4dca4`, run 40056123101, materialización `ad9b4213fc416d0a14556033fdda05bcda3cbaaf`: **27/27 PASS**, cero omisiones, 158,45 s de Editor. Unity mide caja proyectada 74,68 % horizontal /76,05 % vertical; todos los vértices siguen dentro del encuadre. Las seis capturas auténticas confirman el aprovechamiento horizontal y la continuidad del sector original. Se conservan las diferencias AF/AG cercanas del pase anterior (MAE 0,381/0,285).

ZIP visual 12545687903, SHA256 `7fbe2a34d614d29b292b7fc45d7c6199936fb692fd9e758b595352124efc4d85`; 126 PNG y XML comprobados en `NATIVE_REVIEW_40056123101.json`. La representación continental todavía necesita más riqueza de suelo, masas forestales e hidrografía frente a las referencias. No se declara aprobación artística ni autorización de integración. Siguiente comprobación: suite general existente de UI/Valoria/I–II sobre candidato reconciliado, sin modificar ese workflow ni sus pruebas.


### Pase de pradera continental: preparación posterior al encuadre
La inspección de 40056123101 conserva el nuevo encuadre, pero rechaza considerar resuelta la uniformidad del suelo. Se reduce el lavado sinusoidal de amplitudes 0,018/0,014/0,008 a 0,004/0,003/0,002 y se trasladan pinceladas AF dirigidas a los corredores existentes, fuera del camino. Semianchos 37–83 m y 14–36 m, centro desplazado 42–94 m a cada lado, espina exp(-u⁴), ondulación 4,5/2,5 m, borde quebrado 0,86±0,14 y mezcla máxima 0,47. Paletas I/AF existentes, máscara editable independiente. La capa se aplica antes del sotobosque, riberas, costa, caminos y empalme original: cada uno conserva su autoridad. La veladura de hojarasca pasa de 0,56 a 0,69 para conectar visualmente los árboles reales.

Solo cambia el atlas de la ampliación; vértices, materiales/UV originales, árboles, reservas, colliders y ubicaciones jugables permanecen iguales. No se considera mejorado hasta comparar PNG Unity del siguiente pase con 40056123101.

La revisión de fotogramas 006/012/018 del zoom horizontal detecta el borde rectangular físico durante el giro, antes de activarse la atmósfera del último 15 %. Se adelanta únicamente esa misma franja periférica (107 m ±60 m): transición entre zoom regional×0,75 y ×1,25. El interior y el sector AF/AG no reciben niebla, ni se recortan o desplazan vértices; la cámara conserva/restaura su fondo. Comprobar los mismos fotogramas en Unity antes de aceptar esta corrección.


### Certificación completa y reparación de interacción inmediata
La suite general detectó un defecto que los pases anteriores no cubrían: el zoom estratégico se aplicaba antes que la profundidad de cámara, y un raycast de arrastre podía comenzar bajo el plano del terreno en ese primer fotograma. `ApplyPose` sincroniza ahora distancia=max(140,zoom×2,5) y farClip=max(900,distancia×2,5). La entrada/Home usa rumbo regional y 82 % del límite realmente navegable. Se preparan cuatro rayos en ambas orientaciones antes de LateUpdate; ninguna prueba de rutas, contacto o guardado se elimina. El preset cercano se verifica por igualdad estricta con el preset real, no por el antiguo número 17.

Run 40061480616 confirma 44 controles nativos I–II, 26/26 EditMode y 56 PlayMode sin fallos; queda una captura omitida por el modo sin gráficos, y el workflow general resulta rojo. Se amplía el workflow propiedad de World4X a TODOS los 57 PlayMode con GPU y captura activa, más EditMode y el probe I–II existente con sus mismas comparaciones de encuadre. Exige 34 PNG generales de esta ejecución, fuente exacta y hashes. No se modifica el workflow reservado de UI ni se presenta una omisión como prueba superada. `GENERAL_CERTIFICATION_REPAIR_20261024.md` conserva causas, datos y descartes.

El verificador usa ahora un mínimo de 57 pruebas y exige también el manifiesto general. Para revisar paquetes históricos de 27 pruebas se debe indicar explícitamente `--expected-tests 27`. Los 126 PNG de mundo mantienen su identidad individual, aparte de los 34 del probe general y del archivo AG. Todo el nuevo pase sigue pendiente de ejecución y revisión visual.


## Full native certificate and visible grain seam — 2026-10-24

Run [40062946172](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40062946172) succeeded from source `a5a17842a6e73b36a5907c2f4385612e0bf28cce`, materialized as `6c9855a76016b735b392656dd8813ddf47d27856`. Independent artifact verification: 57/57 GPU PlayMode, 26/26 EditMode, 44 real I–II checks, 126 world PNG plus 34 fresh general PNG; zero failed or omitted tests. XML duration 184.1396056 s is Editor-suite duration, never mobile FPS. Original AF/AG matched-close conservation remains 0.380859/255 horizontal and 0.285011/255 vertical MAE. These values do not grant artistic approval.

The six real views show a bounded improvement from the directed prairie strokes. The real `general/integration-art-engendro-prebattle-close.png` still reveals a rectangular texture-density transition beside the original I/AF terrain. Source recovery identified its cause: the AF pigment patch is encoded sRGB (mean approximately 0.535/0.543/0.229), but its relative detail was being multiplied into linear-light Unity albedo. In addition, averaging two detail samples and weighting them by .85 reduced variance again. Measured original linear relative RGB grain standard deviation is approximately .158/.161/.140; the old runtime prediction was only .044/.045/.044. See `LINEAR_GRAIN_SOURCE_ANALYSIS_20261024.json`; this is source-texture analysis, not simulated Unity evidence.

Next bounded candidate (native result pending): convert the *derived patch* to linear light with the exact sRGB transfer, remove the same Gaussian low-frequency illumination (sigma 7 pixels), normalize against its linear mean, encode .5 + .5 relative grain in the existing linear data texture, clip only to [.1,.9], and use sqrt(2) strength for two decorrelated half-amplitude samples. World scale stays 21.12×14.72 m. Original albedo, material slots, UVs and source image pixels remain untouched. Compare the same genuine Engendro screenshot before accepting.

The continental forest retains the same roots, source meshes, heights, random sequence and shared near/far topology. Broaden only new crowns up to 1.35× in Blender XY, smoothly ramping from 1× at the retained core to full width at 220 m. This addresses sparse dotted masses without adding billboard substitutes or changing geometry by camera distance. Existing real mesh/renderer clearance, route, city and resource tests remain mandatory and unchanged; a failure requires actual geometry/placement repair. Outer rectangle and less-rich hydrography remain explicit visual differences, not approved outcomes.
