# VALORIA — relevo de Work a chat normal, 17-10-2026

## Autoridad y punto de recuperación

El propietario pidió expresamente este relevo al agotarse sus créditos de Work. Se detiene esta sesión por su orden, no por finalización artística ni por un bloqueo técnico. No hay producción nueva en segundo plano. Mismo repositorio `mt5hjfz2kb-lab/Eldoria-Prewiu`, misma rama `construction/valoria-master-plan-layers-20261016`, mismo workstream `valoria-master-plan-layered-construction-20261016`. No abrir otra rama ni otra producción.

**Último commit de escena/evidencia ejecutada: `60ba24d7feccebb1d775879c9330c5c35e575ee2`.** Los commits posteriores de este relevo son documentación/gobernanza. Verificar HEAD vivo y sus diferencias antes de actuar. El HEAD inicial al asumir Work fue `52b03eeb771ed9afbc131f254a96b7793f106ed7`.

El registro se deja en `history` con estado `handoff` y recurso Windows liberado, conforme a `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`. El siguiente chat debe recuperar **el mismo id** a `active`, registrar su owner y reclamar Windows solo tras comprobar cola/procesos. Esto no autoriza reactivar los demás trabajos pausados.

## Lo que ya está hecho y no debe repetirse sin causa

- CS0234 reparado en `Editor/ValoriaMacroScreenEnvelopeCandidateV1.cs`: dos referencias usan `UnityEngine.Application.dataPath`. Commit `3bf384899f160fd4cef69af0b65f9f8ce72f78f0`; Unity compila realmente.
- Macro SHARP completado: run `39024979262`, 250 ensayos reales. La puerta antigua ocupaba 67,822 % de anchura frente a 17,7734 % de referencia; trasladar toda la ciudad no bastaba. No reiniciar ese estudio.
- Recalibración aislada nativa Y-up validada en `39025768860`: cámara real y controlador canónico conservados, conversión rígida con error máximo 0,000165 píxeles, 22 huellas y 12 escalones originales; 28 contactos específicos de circulación y barrido peatonal.
- Última autoría Blender/Unity V7: run **`39029275704` SUCCESS**, fuente de ejecución `68f1e64c3a8b55ab1131f6671859cf12cfe4fc70`, artefacto **`12100116291`**. Fuente editable y GLB reales guardados; no son imágenes generadas.
- Última integración jugable: run **`39029700049` SUCCESS**, fuente de ejecución `80859c1e6b94ff20aa87bc19d9c0dbcb8bfc28f4`, artefacto **`12099459455`**. Resultado/evidencia en el commit `60ba24d7...`.
- Prueba jugable real acotada: botones existentes de construcción de aserradero/cuartel, ascenso a Bastión II, reclutamiento, confirmación de marcha, objetivo `b2.complete`, salida por el hotspot original de puerta a `Frontier`, regreso por botón a la escena experimental, recarga exacta y reinicio aislado. Revisión antes de reset **18**, aserradero 1, cuartel 1, Bastión 2. Guardado canónico protegido con huella SHA, sin cambios. QA adelanta solo `IClock`, no costes/reglas. No equivale a prueba física Android/iOS ni a todos los gestos/dispositivos.

## Escena y archivos que se deben abrir primero

- **Escena jugable actual:** `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_Entry_PlayableV2.unity`.
- Escena artística V7 conservada: `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_EntryZoneV7_CANDIDATE.unity`.
- Fuente editable: `docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/ValoriaEntryZoneV7.blend`.
- Exportación: misma carpeta, `ValoriaEntryZoneV7.glb`.
- SHA BLEND V7: `24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381`.
- SHA GLB V7: `bc079e187055a8c6fd818a19fd41c8cdbb0aa3e07ba37c501412e13d1e3a39be`.
- Autoría reproducible: `tools/valoria-layered-construction/author-entry-zone-v7.blender.py`.
- Integración: `Editor/ValoriaEntryZoneV7Integration.cs`.
- Adaptador aislado: `ValoriaEntryExperimentalRuntimeV2.cs`; constructor/prueba `Editor/ValoriaEntryPlayableV2.cs`.
- Evidencias jugables: `docs/evidence/valoria-layered-construction-20261016/playable-entry-v2/` (`gameplay-playmode.json`, `complete-playerstate-before-reset.json`, logs y `playmode-built-i-ii-geometry.png`). Esa captura Camera.Render muestra geometría PlayMode; **no incluye HUD Overlay**.
- Recursos persistidos: `GeneratedEntryV7/` y `GeneratedPlayableEntryV2/` dentro del ArtTest.

Capturas V7 que hay que inspeccionar, no solo enlazar:

1. `entry-zone-v7/entry-BEST-PREVIOUS-V5-home.png`: mejor anterior conservada; tiene defectos físicos conocidos.
2. `entry-zone-v7/entry-BEFORE-home.png`: V6 oscurecida por normales invertidas; no usarla sola para exagerar mejora.
3. `entry-zone-v7/entry-AFTER-home.png`: geometría V7 con luz anterior comparable.
4. `entry-zone-v7/light-BEFORE-home.png` y `light-AFTER-home.png`: A/B de dirección de luz; la escena guardada usa AFTER.
5. `entry-zone-v7/light-AFTER-portrait.png`, resto de pan/zoom/portrait y captura jugable V2.
6. Referencia original aprobada `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`, SHA `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`.

## Medición actual y límites honestos

V7 Unity: puerta **19,09087 % ancho / 22,52178 % alto**, referencia **17,7734 % / 20,6055 %**; centro a 34,196 px del objetivo, dentro de la tolerancia macro usada. Envolvente de puerta con murallas fuente: 36,51245 % de anchura; esto **no aprueba** toda la distribución lateral de murallas. Cámara `ValoriaParcelPresentation`, perspectiva HOME **44,4228096°**. Siete contactos de fuente en HOME horizontal, 40 aciertos acumulados en estados de cámara (portrait muestra cinco objetivos), 12 escalones, **28/28** contactos de circulación y barrido libre. 342.006 triángulos de fuente y 54 renderers visibles: cifras técnicas, no calificación artística ni rendimiento móvil.

**TECH_PASS. GAMEPLAY_PASS acotado al flujo descrito. FULL_FRAME_ART_PASS no superado. MOBILE_PERFORMANCE_PASS no probado.** Hay mejoras visibles de roca, pavimento, eliminación de cubos sueltos y luz de fachada, pero la escena sigue lejos de la referencia. El Bastión superior es un cubo provisional. No declarar entrega artística aprobada.

Qwen 8B revisó imágenes reales en `entry-zone-v7/independent-review-*.json`: verdict bruto **ART_REJECTED**. Conserva ese dictamen y revisa sus razones contra imágenes y geometría: hizo afirmaciones no demostradas sobre colisión y describió el pavimento como carente de detalle pese a existir piedras modeladas. En V6 había dado LOCAL_ART_IMPROVEMENT sin detectar normales invertidas. No tomar sus respuestas como autoridad única, no borrar críticas y no inventar puntuaciones. Autor geométrico actual: GPT de Work; no atribuir estos scripts a Qwen. La primera inferencia auxiliar 4B de autoría sí ocurrió y fue rechazada por formato, sin ejecutar su plan.

## Próxima intervención concreta — no ejecutada todavía

Se estaba inspeccionando la familia original `art-source/valoria/production/wall-family-v1/WallFamilyV1.blend/.glb` para recuperar murallas laterales más completas. **No existe todavía una V8 ni una nueva composición de murallas o fondo.** No confundir esta inspección con trabajo hecho.

Orden inmediato:

1. Leer este relevo, verificar HEAD y recuperar claim del mismo workstream. Confirmar que las ejecuciones anteriores terminaron antes de usar Windows.
2. Abrir las capturas reales V7/V2 y referencia. Conservar V5/V7/V2; la V7 sigue siendo candidata, no aprobación. No reconstruir cámara ni progresión.
3. Corregir los restos rectangulares beige identificados: `GREYBOX VPD · master east route` y `GREYBOX VPD · master west route`, centros nativos `(11.1,.27,-1)` y `(-11.1,.27,-1.2)`, tamaño `(7,.14,2.4)`. Sustituir su visual por conexiones de camino coherentes con parcelas recalibradas; preservar circulación y validar la superficie real. Existe también `GREYBOX VPD · lower entry apron` `(0,.3,-8.35)`, tamaño `(6.6,.14,2)`, que debe integrarse visualmente.
4. Comprobar apoyo inferior del puente: en V7 se corrigió el origen de ARCH_FOOTINGS, pero el pie llega aproximadamente a -4,35 frente al lecho cercano a -4,5 con variación. Medir cada contacto real antes de ajustar/extender la malla; no ocultar un hueco con el agua ni mover colliders.
5. Afinar ambiente local: el agua sigue demasiado plana/cian; la luz frontal mejora banderas pero deja roca muy amarilla; pinos/arbustos se leen pálidos, dispersos y poco integrados. Suelo y terreno aún tienen aspecto de tablero. Diseñar una corrección perceptible, reutilizando recursos, y conservar A/B equivalente. No lanzar otra biblioteca ni decorar para ocultar geometría deficiente.
6. Evaluar reutilizar solo las murallas originales que mejoren la entrada. Familias actuales: `WestPartialWall`, `EastPartialWall`, `WestWallTowerA/B`, `EastWallTowerA/B`. Sus mallas tienen posiciones históricas ya incorporadas; inspeccionar WORLD/bounds/anclas y normalizar la copia antes de colocar. No asumir origen cero, no estirar modelos.
7. Comparar la zona puente + puerta de dos torres + inicio de escalera + roca/riberas con la mejor anterior y referencia, en cámara real. Solo después integrar mejoras en otra copia y repetir el flujo jugable existente. La fortaleza superior completa sigue subordinada a esta primera zona.

## Lecciones técnicas que evitan repetir fallos

- glTFast refleja X al convertir glTF RH a Unity. La autoría de terreno/caminos en coordenadas nativas debe aplicar conversión una vez; las fuentes históricas ya importadas pueden tener orientación incorporada. No reflejar todo indiscriminadamente.
- Un heightfield abierto no tiene un fuera inequívoco: después de recalcular normales verificar que apuntan arriba. V6 tenía media -0,933; V7 la corrige a **+0,9386** con assert.
- Al anexar objetos Blender desde bibliotecas, enlazar y llamar `bpy.context.view_layer.update()` **antes** de leer `matrix_world` y hornear vértices. Omitirlo dejó los pies del puente como cubos desplazados. V7 comprueba bounds Z nativos -18,88 a -10,12.
- Cachear normales antes de desplazar vértices; no leerlas mientras se modifica toda la malla.
- No confundir centro geométrico, ancla visual y contacto de collider.
- `ValoriaUprightViewBridge.cs` conserva controlador/lente canónicos y transforma solo el marco. También copia matriz/aspect de la cámara render a la propiedad de cámara que usa `SlicePresenter` para proyectar/seleccionar. Eliminarlo rompe la integración.
- El adaptador jugable usa guardado separado, protege navegación/reset y evita duplicar el host al regresar desde Frontier. No sustituirlo por `SliceBoot` canónico que escribiría el guardado real.

## Recursos efectivamente recuperados y ejecución

PC runner `DESKTOP-R10PE55`, etiquetas `[self-hosted, windows, unity-6000-3-23f1]`. Unity **6000.3.23f1**, URP **17.3.0**, glTFast **6.14.1**. Blender **5.2.2 LTS** real. Ollama **0.40.2**, modelos comprobados: `qwen3:14b`, `qwen3-vl:8b-instruct-q4_K_M`, `qwen3-vl:4b-instruct`, `eldoria-artist:latest`, `hf.co/Qwen/Qwen3-VL-2B-Instruct-GGUF:Q4_K_M`. No descargar pesos. BlenderMCP histórico no se ha requerido: Blender nativo funciona. Recuperar herramientas existentes de `tools/art-direction/`, no fabricar otro agente.

Fuentes congeladas aprovechadas: terreno V3 SHA `768a08535f066a40c707fd52901a0c16efad91ec06ce91f35ca31cf625b5792f`, puerta LowerGateFamilyV1 SHA `3febc6df897ee4b3eec57fa9c717973194b0125afa8c471aca5af6ad86ead1cf`, puente BridgeLevelAccessV1 SHA `44423997651b05c839da833cdef1914a433058f903db37a9427d66d173b889d0`. Todas preservadas.

Rama 63 pausada HEAD inspeccionado `22db6d0d539b2baa8ca4f4eeeb42b6a25bb5fed4`. Checkpoint de versiones en `docs/evidence/valoria-modular-63-individual-quality/CHECKPOINT_20261016.md` de esa rama. Copias derivadas únicamente en construcción:

- Roca scan Rock Face 02, artefacto `12061076273`, GLB `5a3fa128f87a51851aaf98b7269461db23536d31302e79b6ba7d2beb7bc761ae`. Sus caras aisladas resultaron inadecuadas como fachadas repetidas; se retiraron en V5.
- Prado CC0, artefacto `12044316857`, PNG `d528215ccc37271a505c99773c454c224c024f12a619b03c47c154f733081d32`, reutilizado en bake geográfico 2k.
- Pino mediano, artefacto `12035633500` / run `38877895047`, GLB `003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e`.
- Arbusto 1, artefacto `12012128470` / run `38819524063`, GLB `5da94bd40f1afc7b03dc1fb052352ca97b986b35aafb49a69249cebb876f14f0`. Pino/arbusto Quaternius CC0; que sean gratuitos o provisionales no los aprueba visualmente en Valoria.
- Roca PBR adicional **Aerial Rocks 02, Poly Haven CC0**, mapas originales/URL/hash y crédito en `cc0-rock-provenance.json`, bytes y BLEND empaquetados. La API dio 403: funcionó descarga directa oficial. No pagar ni depender de la API para recuperar archivos ya guardados.
- Aserradero `sawmill-family-v1/state0` y `state1`; campamento `camp-family-v1`, importados desde originales y ajustados uniformemente por bounds/contacto.
- No se usó el pavimento histórico con licencia pendiente. No extraer ni redistribuir texturas de Tree_Packs: su aviso menciona Textures.com. Inspeccionar licencias antes de reutilizar nuevos paquetes.

Las demás familias .blend/.glb y paquetes del encargo inicial siguen disponibles para inspección selectiva, no constituyen aprobación artística ni obligación de usarlos todos.

**Workflow existente reutilizado:** `.github/workflows/valoria-sharp-macro-envelope-candidate-v1.yml`. Su nombre de archivo es histórico. Al relevo ejecuta **ValoriaEntryPlayableV2.Build y PlayProbe**, no macro ni Blender V7. Para nuevo arte ajustar el workflow existente de forma secuencial o recuperar su versión V7 del commit `68f1e64c...`, manteniendo guard de lease, referencia, procesos libres, un solo runner y push normal. No crear otra cadena de producción por comodidad.

No cambiar main ni escenas canónicas; excepción limitada: registro de coordinación del relevo. No force-push, publicación, builds públicos, gastos ni reactivación de workstreams pausados. La referencia y la misión original del propietario prevalecen. Continuar autónomamente desde este punto sin repetir diagnósticos ya superados.
