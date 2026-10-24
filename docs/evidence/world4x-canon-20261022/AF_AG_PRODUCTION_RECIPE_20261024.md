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
