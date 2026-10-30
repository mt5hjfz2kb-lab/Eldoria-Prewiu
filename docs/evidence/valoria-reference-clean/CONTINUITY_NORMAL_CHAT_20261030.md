# RECUPERACIÓN NORMAL CHAT — 31-10-2026 (checkpoint añadido)

> **Estado:** PR #136 sigue DRAFT, branch `fix/player-feedback-context-ui-20261030`, base `main`. Última revisión de código de esta recuperación: `e42dbf9784cd2ce574b95fd1734c2568943fcedc`; corrección del flujo Unity: `e280120f0786a9c1d33cf6979a03f6302ea7ccdd`. Comprobar siempre `git/ref/heads/fix/player-feedback-context-ui-20261030` para HEAD final después de este documento. No merge, publicación, cambio de motor ni reinicio de guardados.

## Evidencia recuperada sin repetir generación

- PR #136 estaba en `7af82cf6a6329a957064ef52588e591f47c43aab` al comenzar. Últimos commits previos: Lyra modelo existente `8925e117`, espera de retrato real `0859aaeb`, corrección de orientación `24de23ca`, relevo `7af82cf6`. No se ha generado, sustituido ni modificado el GLB de Lyra.
- Unity `41138763107` sobre HEAD `7af82cf6` terminó **SUCCESS** y conserva artefactos `13020030049` (certificación completa), `13018792087` (V4.1), `13018867101` (tres marchas), `13018102628` (Retención), `13019795206` y `13018092903` (Aldric). Los 68/68 EditMode y 73/73 PlayMode de ese historial **no certifican los nuevos commits**.
- Paquete de Biblioteca **Eldoria_Heroes_Integracion_Preparada_v14.zip**, localizado y abierto. Contiene 6 referencias offline, 23 recortes pixel-exactos y scripts de ejemplo, pero los 23 recortes tienen texto/figuras de conceptos aplanados; no son fondos de producción libres de interfaz. Conservado íntegro: `/Eldoria_Heroes_Integracion_Preparada_v14.zip` en Biblioteca. Arte de referencia de verdad: `docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png` y `Unity/Assets/Eldoria/Resources/EldoriaV41/HeroesApprovedMaster.png`, SHA256 `48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800`.
- Se inspeccionó captura Unity `eldoria_latest_heroes_31oct.png` (Library). Salón negro, dos tarjetas sin entorno; ficha Lyra y Talentos funcionales pero paneles negros, enormes huecos. Los PNG de comparativa y los certificados antiguos siguen en Biblioteca `Eldoria_comparacion_nativa.html` y `Eldoria_relevo_evidencias.zip`; no duplicar.
- No se localizaron recursos **independientes y definitivos de fondo de Salón** en el árbol completo de PR136 (21.804 entradas) ni un commit posterior al relevo con ese arte integrado. No interpretar una imagen de concepto como fondo limpio.

## Integración real nueva y límites

- `Unity/Assets/Eldoria/HeroesUI/HeroesApprovedDestination.cs`: `DrawApprovedSalonEnvironment` usa un recorte ornamental de la *lámina v14 canónica ya guardada* (fila superior 512×168), con ocultación de texto ficticio, columnas y luces sin raycasts; conserva controles y datos reales. Salón pasa a composición escenario primero y tarjetas responsivas, con contexto visible, sin dobles Canvas/EventSystems. **Se trata de una recuperación estructural visual, no de un fondo definitivo ni de fidelidad 100 %.**
- No se han incorporado ilustraciones de Lyra alternativas. Se rechazó un concepto nuevo que no coincidía con la referencia v14 ni con su GLB aprobado; jamás se registró como asset de producción.
- `.github/workflows/ui-progression-cert.yml`: los dos preflights de lease rechazaban la titularidad legítimamente transferida al usar el nombre de un chat antiguo y rev74. Se cambiaron para exigir una **única reserva viva en `main`**, de workstream canónico Valoria, rama candidata exacta, owner no vacío, revisión ≥75 y estado `claimed`. Se preserva el bloqueo de otros propietarios; no se ha relajado la suite de Unity.
- `pipeline/active-workstreams.json` de `main`: relevo seguro mediante SHA/CAS a `chatgpt-eldoria-normal-handoff-20261031`, revisiones Valoria76, V4.1 11, Héroes8, vault7 e iPhone7, sin cambiar sus recursos/gameplay. Se reconcilió el alcance histórico iPhone porque la PR ya contenía archivos diagnósticos; no es una prueba física.
- Primeras ejecuciones UI `41144568283` y dueño `41144609632` fallaron por esos **bloqueos de registro** antes de compilar el nuevo código. No acreditan defecto de Unity; después se ha creado el commit de reparación `e280120`; comprobar su run correspondiente y sus capturas.

## Continuación segura obligatoria

1. Verificar HEAD actual, el propietario y reserva del registro de `main`; **no abrir nueva PR**, no usar otro workstream y no cancelar un job del otro propietario.
2. Leer logs de `UI Progression Certification` y `Canonical PR ownership` correspondientes a `e280120` o sucesores. Reparar fallos reales sin saltar tests. Comprobar 68 EditMode/73 PlayMode, V4.1, Marchas, Retención, ciudad y guardados; capturar Sala en 390×844 y 844×390 y comparar contra v14. Sin PASS propio, la candidata no está certificada.
3. Si el recorte del salón muestra texto aplanado o arte mal estirado, **retirar solo el decorado nuevo** y sustituirlo por fondos limpios independientes del master; no sacrificar controles/guardados. Se requiere revisión estética auténtica. No generar otra Lyra: usar exclusivamente la existente.
4. Completar de verdad materialidad, marcos, filtros/estado y composición de Sala; luego Marchas, Entrenamiento y Ciudad. Repetir capturas Unity y pruebas de eventos reales, safe areas y orientación. Si fallan dos aproximaciones, cambiar método, no degradar la referencia.
5. **V4.1** issue #138 permanece laboratorio (`ProductionApproved=false` y campaña bajo `SliceRules`); no activarlo, no cerrar la incidencia. Android/WebGL sin nueva publicación hasta QA de artefacto y gate de testers; mantener actualizador, firma y saves.
6. Leer las secciones originales de este archivo bajo este checkpoint para detalles de modelos PR127, 203 hashes, Blender/GLB, procedimientos GitData/Actions y referencias de todas las PR.

---

# ELDORIA — relevo operativo Work → chat normal, 30-10-2026

Este documento es un checkpoint verificable, no una declaración de finalización artística. Leerlo antes de los encabezados históricos de SESSION_HANDOFF.md. Los comentarios posteriores de PR136 y el HEAD vivo prevalecen sobre un SHA registrado aquí. Preservar el historial; no empezar de cero.

## 1. Entrada exacta y titularidad

- Repositorio: mt5hjfz2kb-lab/Eldoria-Prewiu.
- Única candidata UI: PR136, rama fix/player-feedback-context-ui-20261030.
- Último cambio de runtime al escribir este relevo: **24de23ca2792325b52a8e16d49fe81322d8ae2de** (orientación frontal de Lyra). El commit documental que contiene este archivo será descendiente suyo; consultar HEAD real.
- Base main real comprobada durante la recuperación: 56a47e9a774e1ccf6e4a54248cf09bc715176e56. El conector de PR llegó a devolver base_sha obsoleto 12da054c15bb5c0afeb608281fb7a78c558efab6: resolver siempre git/ref/heads/main antes de reconciliar.
- Responsable: codex-recovery-ada07b02a31d. Leer **pipeline/active-workstreams.json de main**, no una copia histórica de la rama.
- Reservas comprobadas: valoria-master-plan-layered-construction-20261016 rev74, único windows-runner-heavy; eldoria-v41-combate-buzon-ui-20261030 rev9; eldoria-heroes-approved-ui-20261030 rev6; eldoria-iphone-lifecycle-diagnostics-20261030 rev5; eldoria-unified-art-vault-20261029 rev5. Todas bajo el responsable anterior.
- Protocolo: docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md. Antes de tomar relevo: comprobar reserva, jobs reales activos/pendientes, historial y alcance; transferir con revisión nueva mediante CAS, nunca asumir que el propietario sigue vivo ni cancelar trabajo ajeno.
- Fase: PR136_REFERENCE_DRIVEN_STRUCTURAL_UI_RECONSTRUCTION.
- Unity6000.3.23f1, URP17.3.0. No cambiar versiones, package ID, firma, guardados ni actualizador.

## 2. Todas las PR abiertas comprobadas

| PR | Estado consultado | Rama | HEAD |
|---|---|---|---|
| #127 | open, draft=true, merged=false | `feature/tier5-six-reference-models-20261029` | `3a27851755b9a000ef4b177e3360d6cbfddf3713` |
| #128 | open, draft=true, merged=false | `docs/unified-art-vault-20261029` | `8ad1db01cfece64f69ca5095faf10df5900b7365` |
| #134 | open, draft=true, merged=false | `feature/eldoria-v41-contracts-20261030` | `d8b60c50cef222c65765a9d9aed18515f94743dd` |
| #135 | open, draft=true, merged=false | `feature/heroes-approved-ui-integration-20261030` | `20bfeed5f61d46ca73c5e8aed7a33877c949553c` |
| #136 | open, draft=true, merged=false | `fix/player-feedback-context-ui-20261030` | `24de23ca2792325b52a8e16d49fe81322d8ae2de` |
| #137 | open, draft=true, merged=false | `feature/eldoria-layered-ui-depth-20261030` | `68cc0508a47facd99e79a4e8ac566a05ad7b4717` |

PR134/135/137 son registros de origen cuyos avances están en136: no fusionarlas otra vez sin comprobar ancestros/diff. No abrir nueva consolidación. PR127 conserva seis entregas Tripo; PR128 conserva el inventario, no equivale a integración de modelos. PR124 y132 ya fusionadas: conservar sus regresiones, no repetir su desarrollo.

Incidencias relevantes: #109 veto del Bastión antiguo, abierta; #131 fuentes/decisiones V4.1; **#138 migración futura del motor**, abierta y explícitamente NO completada. La lista de PR puede cambiar: volver a consultar antes de cerrar/mergear. No se ha cerrado ninguna por esta recuperación.

## 3. Qué cambió realmente

### Entrenamiento
- Se conserva la reconstrucción iniciada en a5ea2aa1d6d47252e8b71fa79089d0e26a64196c, no se revierte.
- Escenario independiente y arquero aislado con transparencia; tarjetas laterales T1/T2 reales. T2 requiere Cuartel IV, ambas se bloquean durante reclutamiento.
- Orden compacta a la derecha; personaje completo apoyado visualmente en el suelo; título y descripción separados de su silueta en las cuatro resoluciones.
- Costes, capacidad, cantidades, timers, tiers y guardados siguen en el gateway.
- Fallo original41111056120: needed60.44444/available54 en etiqueta de botón. Corregidos altura de navegación60→76 y encabezado172→188, sin ocultar texto ni bajar comprobaciones. Otros solapes pequeños de paisaje se corrigieron después.
- No terminado artísticamente: materiales, tipografía, ornamento y relación exacta de controles con la referencia aún difieren.

### Marchas
- Fondo independiente, paneles localizados, tres huecos reales, porcentajes y tropas canónicas.
- En horizontal pequeño se conserva retrato completo junto al nombre, no una franja de cara.
- No se inventan Paladines/Brujos ni el tercer héroe para copiar números del concepto.
- La estructura vertical sigue demasiado dominada por paneles y texto: necesita reconstrucción, no diez retoques de opacidad.
- Lyra obtiene ahora su retrato del modelo real. Durante importación aparece soporte neutro y CARGANDO RETRATO; el callback refresca la pantalla.

### Héroes y Lyra
- Seis pantallas conectadas al estado real; Aldric usa geometría existente con giro.
- Talentos: tres rutas visuales/nueve nodos deshabilitados; no se añadieron precios, estadísticas ni desbloqueos ficticios.
- **Decisión explícita del propietario en esta sesión:** usar el modelo 3D existente de Lyra, exploradora castaña, conservar composición v14. No volver a preguntar por el conflicto con la elfa rubia de la lámina.
- 8925e1177afd41a99714fb6d181010bf92f10bdc incorporó el modelo exacto PR127. c7b3949f5a22846f2375241390cecd5ed58c6f09 resolvió además la divergencia documental con main sin alterar ese runtime.
- 0859aaebdec4baa5c2b83f8f15a2de44539b0d79 espera disponibilidad auténtica antes de capturar Marchas; mantiene las aserciones.
- 24de23ca cambia yaw inicial Lyra180→0 tras ver espalda en captura Unity0859; Aldric permanece90. La vista frontal de24de necesita comprobarse en su artefacto, no inferirse.
- HeroesLyraModel.cs carga Resources TextAsset .bytes, verifica longitud/hash, importa con GLTFast, obtiene MeshRenderer, genera retrato mediante HeroesModelSnapshot.CopyPortrait y emite LYRA_APPROVED_MODEL_READY. Instancia oculta, inactiva y fuera de la escena jugable; sin actor, collider, rig, animación ni cambios de partida.
- HeroesModelSnapshot.cs renderiza geometría real a384×576 y conserva rotación. Retrato copia la región superior del render. HeroesApprovedDestination.cs monta el modelo. ValoriaIntegratedWorld.cs conecta el proveedor Func<Sprite> en RoyalGoldV41SourceArt para evitar dependencia circular asmdef.
- Las pantallas reales de héroes todavía son demasiado negras/vacías y no reproducen la materialidad v14. Funcionalidad y modelo NO significan arte aprobado.

### Retención, ciudad, buzón
- Exploradores tiene mesa, viñetas, pergaminos y acciones reales, tres estrategias normal/sigilo/rápida, UTC/reserva/recompensa única.
- Vigilantes conserva25etapas y guardianes; horizontal aún está dividido entre gran mapa y enorme panel de encuentro. Prioridad estructural pendiente.
- Ciudad contextual, navegación Crónica Real, diálogos y eliminación de trazos de marchas ya forman parte de136; verificar visualmente versión actual, no asumir terminado.
- Se corrigió la desaparición de entradas bloqueadas del pie: cinco destinos visibles con interactabilidad real, no desbloqueo artificial.
- Buzón persistente real, sin superposición entre destinos; acabado artístico aún pendiente.
- Ninguna publicación ni integración de gameplay en main en esta recuperación.

## 4. Pruebas y evidencias exactas

| Fuente | Run | Resultado conocido |
|---|---|---|
| a5ea2aa1d6d47252e8b71fa79089d0e26a64196c |41111056120|Compila; falla etiqueta de entrenamiento60.44444/54. Evidencia inicial conservada.|
| 5a33985521f2d7d1b3a68234bb852b1e777e1549 |41130365064|68/68EditMode,73/73PlayMode, certificación completaPASS.|
| 0f4a4b578d1cd55acde315b0eb2ee0ccaa61701d |41131927783|Última base completamente certificada al redactar:68/68EditMode,73/73PlayMode, V4.1/tresmarchas/retención/Aldric/I–VPASS.|
| c7b3949f5a22846f2375241390cecd5ed58c6f09 |41136520296|Compila; falla Source texture loaded Approved source / aldric-face al capturar Marchas antes de carga Lyra. No fallo de dominio.|
| 0859aaebdec4baa5c2b83f8f15a2de44539b0d79 |41137305168|V4.1PASS,489comprobaciones; modelo importa, retrato auténtico, giro y guardado idéntico en6pantallas. Revisión visual detecta espalda. Marchas/retención/Aldric pasados; regresión completa EN CURSO al redactar.|
| 24de23ca2792325b52a8e16d49fe81322d8ae2de |41137726445|Corrección frontal pendiente de Windows; contratos dominio/arquitectura/gobernanzaPASS. NO atribuirle todavía los PASS nativos del padre.|

Run0f compiló commit merge67e93161319feb442f6b7a0c8e4fc320d0bb9648; comparación con0f mostró SOLO SESSION_HANDOFF.md y WORK_RECOVERY_20261030.md diferentes, sin cambios Unity/workflow. Registrar tanto fuente como merge en próximos certificados.

Artefactos0f:
- V4.1 13014960472 SHA256173adc0588bec06fc71d5bb3944e231dbc115095cad48a4b34e8e99e17e7c032.
- Retención13015540448 SHA256f27ddb12778304dd5ea4eab120e762cce1dfbfceb85e7abd5138b34acaae5131.
- Tresmarchas13015665074 SHA256dd159dfe15ed2d15269bbbb7b02431bac8ec9308bbbb21e9908eb00d2f696c8a.
- Aldric13014606589 SHA256f47ca002babc94b6fc309d389113497fbafe7a935177a4d8948c0c2b30542216.
FalloLyra c7: artefacto13016407684 SHA256fb5d02a35e3f44681bd0f5d55da8e22ae993fbb0d3064840dd7e00f3739de32f.
Lyra0859: artefacto13015964935 SHA256b619c4edae83a8e990fa293feb34299fb172bb5850f594d545fbdb8d36c6158a.
Los artefactos GitHub caducan: descargar/preservar entregas relevantes; no confundir link temporal con respaldo.

Capturas requeridas: training-reference-ratio554×542, training-small-phone390×844, training-small-phone-landscape844×390, training-phone900×1600, training-landscape1600×900; march-reference-ratio941×1672 y móviles; heroes-lyra-resumen-{phone,landscape}, heroes-lyra-rotated-landscape; seis páginas héroes, buzón y19capturas Retención. No usar rendersBlender ni conceptos como pruebaUnity.
En fullartifact: _temp/Eldoria-UIProgression-EditMode.xml y PlayMode.xml; docs/evidence/valoria-reference-clean/integration-result.json y capturasI–V.
NO hay prueba físicaAndroid/iPhone de esta candidata. No hay fidelidad100% ni aceptación final.

## 5. Inventario de assets y fuentes recuperables

No sustituir inventarios extensos por una lista breve ni eliminar originales. Índices canónicos para TODOS los assets:
- pipeline/asset-vault/master-catalog-v1.json
- pipeline/asset-vault/current-full-index-v1.json
- pipeline/asset-vault/historical-full-index-v1.json
- pipeline/asset-vault/clean-production-shelf-v1.json
- docs/asset-vault/README.md, CLEAN_LIBRARY.md, GALLERY.md, AUDITORIA_PROFUNDA_20261025.md, FULL_TREE_DISCOVERY_20261025.md
- RamaPR128 añade inventario que distingue ausentes, LFS, enlaces, hashes y relacionesGUID; revisar diff antes de incorporar. Un punteroLFS no son los bytes del modelo.
- Obtener árbol Git recursivo al SHA exacto para rutas completas; índices no garantizan por sí mismos que un objeto externo siga descargable. No declarar respaldo total por enumerar nombres.

### Recursos en runtime/editar
- Unity/Assets/Eldoria/Resources/EldoriaV41/training-archer-isolated-v1.png
- Unity/Assets/Eldoria/Resources/EldoriaV41/training-courtyard-layer-v1.png
- RestoEldoriaV41/ contiene fuentes2D de entrenamiento/marchas/buzón; conservar .meta/GUID.
- Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3ManagementScreens.cs
- RoyalGoldV3EarlyRetentionScreens.cs y RoyalGoldV3EarlyRetentionArtwork.cs en el mismo directorio.
- Unity/Assets/Eldoria/HeroesUI/Resources/Approved/ y ApprovedParts/ conservan v14. Son referencias/recortes CON TEXTO HORNEADO, no fondos limpios ni controles finales. No ponerlos como pantalla plana encima del juego.
- Unity/Assets/Eldoria/HeroesUI/{HeroesApprovedDestination,HeroesModelSnapshot,HeroesLyraModel}.cs.
- Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb; AldricCity/ y normal-import-proof.json/documentación docs/aldric-city/ conservan importación y QA.
- Unity/Assets/Eldoria/Resources/UI/AldricDialogue/ALDRIC_BUST_PROVENANCE.json.
- Unity/Assets/StreamingAssets/Eldoria/TierFive/ y Unity/Assets/Eldoria/BastionIVV/ mantienen el runtime ya integrado. No sustituirlo en bloque por PR127.

### Lyra exacta
- OriginalPR127: taskTripo f4670d45-00c9-43eb-a658-902efdebee87, run40858960344, artefacto12890387724.
- Entrega persistente: **Eldoria-lyra-entrega.zip**, Library libfile_4e240aa4feb08191a4da6c3b5276c591. Buscar nombre exacto si cambió ubicación.
- GLBoptimizado4.770.000bytes SHA256a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725.
- GLBoriginal SHA256ec3a2cee0882fab28a7365ffb30bb2711bbe55553c55170d2419e5420a770b52.
- Editablefinished/editable.blend7.415.837bytes SHA2565e3394e3c78bf8e4bb8805b72c2925c25c9e3035a61ee9b25ffe5d3c270fb2ef.
- ReferenciaJPEG SHA256120a8e277cf8e25479fb13db4a41400d38605e50e5a0c4a393685701b93a8ca3.
- Runtime: Unity/Assets/Eldoria/Resources/EldoriaV41/lyra-approved-model-v1.bytes (mismos bytesGLB, extensiónTextAsset); Gitblob290c9803242911741ff810fdbddb7591ea39737a. No regenerar ni reconstruir Base64 a mano.
- Modelo estático1mesh/1material, ~30ktriángulos, UV/PBR/normales; **sin rig ni animaciones**.

### Cinco edificios PR127
Entregas persistentes: Eldoria-bastion-v-entrega.zip, Eldoria-aserradero-v-entrega.zip, Eldoria-cuartel-v-entrega.zip, Eldoria-granero-v-entrega.zip, Eldoria-cantera-v-entrega.zip. Cada una conserva originalGLB, optimizadoGLB, editableBLEND, referencia/configuración/renders/checksum. Fuentecomún ArtSource/Tier5ReferenceProduction/manifest.json en HEADPR1273a27851755b9a000ef4b177e3360d6cbfddf3713.
**203archivos verificados en los6paquetes**, originales y referencias coinciden conPR127. Resultado máquina en ASSET_RECOVERY_VERIFIED_20261030.json junto a este documento. Conservación verificada ≠ integraciónUnity/aprobación final. No crear nuevasgeneraciones a0€.

### Mundo y patrimonio original
ArtSource/World4X/RiftFocus_v1/ contiene Tripo_H31_Original.glb, Optimized_Source.glb, Optimized_Editable.blend, PROVENANCE.md y UnityQA/.
ArtSource/World4X/RockBoar_v1/ contiene Tripo_RockBoar_H31_Original.glb, RockBoar_Optimizado_49800.glb/.blend, PROVENANCE.md y UnityQA/.
ArtSource/World4X/Spawnling_v1/ contiene original/optimizado/editable y Finished/Spawnling_B2_Near.glb, Spawnling_B2_Far.glb, Spawnling_B2_Finished.blend, author-evidence.json y vistas.
No reactivar Qwen/Ollama. Conservar sus registros históricos sin tratarlos como herramientas artísticas obligatorias actuales.
#109: GUID del antiguo Bastionfa36c6abf91adc7778caeac12130f567 no aparece directamente en escena0f; eso NO descarta geometría derivada. Incidencia sigue abierta hasta auditoría real de escena/build.

### Referencias aprobadas
- UIglobal Tres estilos de interfaz para Valoria.png, columnaIZQUIERDA Opción1 CrónicaReal, SHA25627565c7b85004dd7b84efdfe7587b9bfd976e376b07242c51ae5241851e60582.
- Diálogo1 normal; Diálogo3 exclusivamente eventoarcano correspondiente.
- entrenamiento_recorte_lamina_APROBADO.png SHA256a110ba0ffd2a08fa4976c488f87e624fa5601fe3f36b223ee82d19f306adbadc.
- marcha_3_heroes_porcentajes_APROBADA.png SHA2569afd178a57783f83bd0ccb5bbfb78c50643eb75ae74d01cbed63375d7dcd091c.
- buzon_final_pestanas_APROBADO.png SHA256ef01ed38d83d0be012561320727098e59eaa1e809d08f2febf1b1dd44886b0dd.
- Kit ELDORIA_V41_KIT_ARTISTICO_DE_FIDELIDAD_20261029.zip SHA25604973ba9c2e36d90566e1df9b0a0c32d0a5eb764bedcc24b51bb9102b8651715; originales dentro01_REFERENCIAS_ORIGINALES de ELDORIA_V41_PREPRODUCCION_ARTISTICA_EXACTA_20261029.
- Héroes ELDORIA/Library Eldoria_Heroes_Integracion_Preparada_v14.zip, libfile_8e8bacfa68108191b23f205a5b62b529; maestra REFERENCIA_MAESTRA_APROBADA.png SHA25648cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800. DecisiónLyra3D solo cambia su identidad, no autoriza otro estilo.
- Vigilantes/Exploradores: originales seleccionados en docs/early-retention/; referencias persistentes eldoria_retencion.png/libfile_ebb0c0a48948819188613accb0908817 y variantes históricas deben resolverse/leer antes de usar, no asumir que la variante(1) es nueva aprobación.
- Biblioteca /Eldoria/Produccion-pendiente/ mantiene paquetes de #131. Usar búsqueda exacta y hashes, no generar sustitutos.

## 6. Motor V4.1 — protegido, NO migrado

Issue138 leído íntegro. Códigoexperimento Unity/Assets/Eldoria/Scripts/Domain/EldoriaV41Combat.cs y EldoriaV41Contracts.cs; testsEldoriaV41ContractsTests.cs; tools/eldoria-v41/; docs/eldoria-v41/IMPLEMENTATION_STATUS.md.
**ProductionApproved=false; RulesVersion=v4.1-laboratory deben conservarse.** Campaña sigue resolviendo con SliceRules. Tener clase compilada o68/73PASS NO prueba migración.
Simulaciones: 02_COMBATE_V41_VERIFICADO/test_combat_v41.py20/20; 03_CONTROLES_PROBADOS/test_march_planner.cjs14/14; 05_BUZON_DOMINIO_AISLADO/test_mailbox.mjs22/22 (comprobar nombreexacto enZIP; issue138 lo registra como AISLADO). Estos precedentes son de laboratorio, no gameplayUnity.
Paquete ELDORIA_V41_REFERENCIAS_APROBADAS_Y_BUZON_VERIFICADO_20261029.zip SHA256b55bb8fff9a762269ded5803bc514cf41b659abad8dd463b6d75050149920440; paqueteejecuciónpendiente SHA2560cbfb989bd74bdbd0ee05cccdb4ea456d99b3e4b693b3b7703599b9d1b7c0aba.
No perder multiplicadores/resultados ni activarlos silenciosamente. #138 defineG0recuperación,G1contratos/balance,G2puente rulesVersion/migraciónaditiva,G3PvEporfases,G4informes,G5certificación. Combateautomáticorápido, profundidadantesdebatalla, sinhabilidadesmanuales; heridosPvE sinmuertos si siguecontratovigente; no inventarPvP.
Conservar3familiasArqueros/Paladines/Brujos,3héroes y3marchas como conceptos distintos; bloqueosnarrativos reales permanecen. No cerrar138 hasta resolver activo autorizado y pruebasregresión/recompensaúnica/offline.

## 7. Continuar desde chat normal, sin Work

No depende de una sesión shell abierta ni de archivos scratch. GitHub contiene código/commits/docs; Library contiene entregas originales; Actions ejecuta Unity en Windows existente. El acceso del conector debe estar disponible en el nuevochat: no prometer herramientas no expuestas ni copiar credenciales.

1. Descubrir GitHub tools y leer get_pr_info136, este documento, SESSION_HANDOFF y registro vivo de main. Obtener comentarios más recientes, runs porHEAD y jobs. No usar el SHA de este texto como orden de retroceso.
2. Inspeccionar fetch_file alSHA exacto. Para cambio pequeño sincheckout: create_tree(base_tree_sha=HEAD, entradaspath/mode100644/typeblob/content), create_commit(parent_sha=HEAD,tree_sha), update_ref(branch_name,expected_sha=HEAD,sha=nuevo). CAS obligatorio; si falla, releer y reconciliar. Nunca force-push.
3. Los blobsbinarios existentes se reutilizan porSHA en el árbol. No volver a subirGLB si ya existe290c980... Para binario nuevo autorizado: create_blobencodingbase64 y verificar Gitblob/hash real. No imprimirbase64 ni URLsfirmadas.
4. Un push aPR136 dispara .github/workflows/ui-progression-cert.yml porque el diffacumulado contieneUnity. Incluso un commit solo documental puede disparar certificacióncompleta. Agrupar checkpointscoherentes; no encadenar commitsvacíos para sondear.
5. Workflow tiene concurrency que deja terminar certificacióniniciada. Los pendientes pueden ser sustituidos por unHEADmásnuevo: consultar runs reales. No cancelar jobs ajenos ni crear workflowsparalelos. Leer guards depropiedad y revisión74 antes de transferir; adaptar ambos guards solo a reserva legítima.
6. fetch_commit_workflow_runs→fetch_workflow_run_jobs→fetch_workflow_job_logs (logs completos disponibles al finalizar). fetch_workflow_run_artifacts→download_workflow_artifact. DescargaZIP firmadaes temporal. Extraer PNG/XML/logs con entorno disponible o presentar archivos descargables. Unchat sin ejecuciónlocal puede hacer commits/consultarlogs conGitHub y activarCI conpush legítimo; si no tiene visorZIP no puede afirmar inspecciónvisual.
7. La compilaciónusa PRmergecommit; contrastarlo consourceSHA mediantecompare_commits. No extrapolarPASSsiUnity/workflowcambian.
8. Inspeccionar capturasauténticas conreferencia, no soloresult.json. Corregir fallosreales y repetir enSHAexacto. No relajarTests/ocultartexto. EditMode/PlayModeXML, dominios, I–V, guardados, marchas, buzón y móviles.
9. Si se necesita reconstrucción3D/Blender, usarfuenteseditables y workflows/protocolo existentes; conectorGitHub no es acceso interactivoautomático alPC. No asumirBlenderMCP autenticado. No introducir serviciosdepago.
10. Library: buscar nombres exactos originales, conservar identidad/versiones alreemplazar; descargar y verificarSHA. La comparaciónpersistente actual es Eldoria_comparacion_nativa.html libfile_6194fa97dc948191b527e5dcd0bcad15. Su versión0 muestra evidencia5a, no la nuevaLyra; esperar comentario con actualización, no asumir que ya se reemplazó. Los paths /tmp y /workspace deWork son cachéprescindible, no handoffduradero.
11. Mantener checkpointenesta rama y comentarioPR conHEAD/run/resultado/pendiente tras cada bloque. El nuevochat debe poder reanudar aun siWorkdesaparece.
12. No guardar PAT/tokens, cookies, licenciasUnity, clavesfirmaAndroid, connectionstrings ni URLsfirmadas enGitHub. Solo nombresprocedimientos/metadatos públicos.

## 8. Próximas acciones y aceptación

1. Obtenerrun final delHEAD documental/runtime24de: verificarLyrafrontal, retratodeMarchas, giro,6pantallas/savebyteequal y queno hayaexcepciones/importfallo. Si sigue orientaciónincorrecta, comparar lasvistasgeométricas; no cambiarartistaidentidad. Completar68Edit/73Play yI–V exactos.
2. Actualizarcomparación referencia/anterior/actual en Library y preservarZIPevidencias/hashmanifiesto. Mostrar imágenes reales, declarar diferencias.
3. Reconstrucciónestructural Héroes/Marchas/Entrenamiento/ciudad: preservarjerarquía/materiales/composición aprobadas. Héroesnecesitaentorno ypanelesauténticos; Marchasverticaldemasiadopanel; Trainingyaestáporcapas peroacabadoincompleto. No utilizar maestraaplanada comoruntime.
4. Vigilantesmapayencuentrointegrados; Exploradorespergaminosfisicamenteintegrados manteniendo3estrategias; buzónpestañasoriginales. Validar ambasorientaciones y390×844/844×390/900×1600/1600×900.
5. ReconciliarPR127/128 conarchivosverificados203, sin cerrar porinventario; completar galeríacurada. Auditar109 yregresiónBastionesI–V.
6. #138migraciónmotor bajo suprotocolo ydecisionespendientes; nomezclaractivaciónsilenciosa conUI.
7. Publicación: Androidregistrado1.20/code20, packagecom.eldoria.valoria.v5test, source d2ed96d5570f54ce0ad34864efe799f2974d5889; WebGLnative298 sourcec8580934201ae1e17543b8683749d40751fb1e12. Verificar registrosvivos antesdeactuar. No se actualizaron aquí. Mantenerfirma, partidas yautoactualizador.
8. No merge/main/release/testersexternos hasta calidad/certificación/aceptación requeridas. No declarar100% porPASS técnico. Presupuesto0€, no créditosnuevos. Dosintentosfallidos deunatécnica→cambiar método, no bucle cosmético.

## 9. Regla de lectura histórica

SESSION_HANDOFF.md conserva casi200KB dehistorial útil con responsables/SHAsantiguos: sus encabezados inferiores no cambian laprioridaddeestecheckpointrapido. WORK_RECOVERY_20261030.md conservainvestigacióninicial. Los originalesnunca se borran. Si un resultado figura pendiente aquí y un comentario posteriorloresuelve, citarSHA/runexactosdelcomentario yverificarlosantesdeusarlos.


## Continuación chat normal — 31/10/2026 (auditoría de fondos)

- HEAD observado antes de cambios: 7af82cf6a6329a957064ef52588e591f47c43aab. PR136 sigue DRAFT, sin publicación ni merge.
- CI exacta run 41138763107: SUCCESS (Unity UI Progression). Artefactos 13020030049 (certificación completa), 13018867101 (marchas), 13018792087 (V4.1), 13018102628 (retención). No equivale a aceptación artística.
- Árbol Git completo (21.804 entradas) comprobado: referencia aprobada docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png y Unity/Assets/Eldoria/Resources/EldoriaV41/HeroesApprovedMaster.png; HeroesApprovedDestination.cs y modelo Lyra .bytes 4.770.000 bytes. NO se encontró fondo nuevo independiente de Salón/Héroes integrado en Resources ni ArtSource. No regenerar Lyra.
- Biblioteca: Eldoria_Heroes_Integracion_Preparada_v14.zip (10.789.610 bytes), incluye seis previews, HeroesRebuiltSalon.cs, HeroesRebuiltProfile.cs, HeroesGoldFrameGraphic.cs, HeroesProductionOverlay.cs, ApprovedParts, contratos y verificaciones offline. Es material reutilizable no fusionado: requiere revisión de compatibilidad Unity y separación de elementos aplanados, no integración ciega.
- Últimas capturas Biblioteca: eldoria_latest_heroes_31oct.png, eldoria_latest_military_31oct.png, eldoria_latest_city_31oct.png; comparador Eldoria_comparacion_nativa.html. No afirmar que estas capturas prueban fondos definitivos.
- Prioridad: reconstruir Salón con arquitectura pictórica sin valores/botones incrustados; integrar piezas del paquete v14 únicamente tras verificar transparencia, dimensiones, raycasts y bindings; capturar Unity 390x844 y 844x390; después Marchas, Entrenamiento, Ciudad. Mantener seis pantallas, gameplay, guardados, #138 V4.1 laboratorio y gates tester BLOCKED.
- Nota de coordinación: main pipeline/active-workstreams.json revisión75 ya registra owner codex-recovery-8545f387ddc2; no cambiar titularidad ni ejecutar Unity en runner compartido sin comprobar su reserva viva. No abrir PR nueva.
- Una imagen conceptual generada en esta sesión no es la referencia aprobada y no debe sustituirla ni integrarse automáticamente.
