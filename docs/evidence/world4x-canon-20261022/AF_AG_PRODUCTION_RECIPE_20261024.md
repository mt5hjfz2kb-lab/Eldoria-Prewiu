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
| Reparación del relevo | Conservar asignaciones originales; normalizar solo nombre de capa UV; comparar caras/material/UV después de unión | `retained-core-integrity.json`; caras y hashes de píxeles de imágenes; nueva captura nativa requerida | Resultado pendiente hasta comprobar run y PNG |

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
