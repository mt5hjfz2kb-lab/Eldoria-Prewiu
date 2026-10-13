# Bastión I — continuidad de Work, trabajo abierto

Objetivo: continuar la integración existente hasta escena jugable cercana a la referencia, con colores correctos, autoría IA real y prueba de jugador. 0 € / ningún crédito de pago. NO declarar ART PASS ni 8/10 por CI verde.

## 2026-10-13 — Chat normal: revisión visual y descarte de montaje ambiental insuficiente

- Owner de integración transferido de Work al chat normal sobre `main` en `d2e1d1c7e7b83798f57432a4f586556eea4b9913`; Región 1 y otros propietarios intactos.
- Imágenes Unity reales antes/después examinadas del ensayo de reutilización de 3 montañas LOD y 6 árboles. Fuente de prueba `28903335a5dc1cfc5a38f6762b2708cac01d4fc3`, run nativo `38431327960` **SUCCESS**, frames `11830603211`, escena+evidencias `11830987172`. Baseline nativo inmutable `38419031491`, frames `11826196119`. Diferencia exacta de imagen 1440x1000: 1,8325 % de píxeles con alguna componente RGB >10 y MAE 0,5649 (métrica de cambio, NO de calidad).
- **ART FAIL:** los nuevos árboles de hoja ancha y las masas poligonales quedan visualmente desconectados o aparentan flotar en el fondo; no mejoran de modo convincente el paisaje. NO integrar ese montaje ni actualizar el candidato WebGL con ese artefacto; se retira exclusivamente la llamada y el helper de dicho montaje. No modificar GLB autoral, Hero V3, Hero Gate ni terrenos aprobados.
- El ajuste exclusivo de HUD compacto permanece para futura QA; el encuadre retrato de presentación se reduce para priorizar el área visible de juego, pero no se certifica sin captura WebGL exacta. Ningún PASS del candidato anterior se transfiere a la versión modificada.
- Siguiente paso: continuar acabado real autoral del entorno existente, empezando por acantilado/agua y fondo sin introducir árboles flotantes, mediante agente IA+Blender y comparativa Unity exacta; en paralelo se puede producir candidato WebGL de interfaz usando **fuente nativa previa aprobada técnicamente** sin rehacer esa fuente. PR #51 sigue DRAFT/no UNITY_PASS, no fusionar, no usar Windows para él antes de liberar Bastión.
- Se conserva la clasificación técnica anterior: jugabilidad completa de `38422576301` PASS solo para WebGL `38420574701`; arte objetivo 8/10 NO aprobado, Android/iOS físico no probado, SHARP público protegido, 0 € y 0 créditos pagados.

## 2026-10-13 — Qwen original recuperado y Blender real: edición verificada, revisión Unity pendiente
- Qwen visual original terminado en run `38433446558`: generó plan SHA `7e2a5478c8b77000db078d3f12793f00061bbd391f150fb4a8e3b98900fe142d` pero Blender NO llegó a editar por falta de NumPy. No repetir inferencia.
- Reproducción estricta del mismo plan en Blender 4.0 con NumPy: `38438668388` SUCCESS, source `5fbd660f85179e67291cc1cffc9f9fa5eb6a2adf`, artifact `11834685560`, editable .blend y GLB SHA `8d85a68cff7b3bf4584afc832741f3a14b01266c27e8f87542fbcf84f3faeb80`. Geometría montaña: 5432 vértices modificados/3300 triángulos, 1280 caras roca y 2020 forestales; otras mallas y geometría jugable sin cambios, plan IA intacto. Agua: modificación de color/material, no de geometría.
- Este resultado es **BLENDER PASS, ART UNITY NOT YET REVIEWED**. Ejecutar la importación nativa, recuperar capturas `before` de `38419031491` y compararlas con `after` de misma cámara; no afirmar calidad 8/10 por compilación. El workflow Unity consume ese artefacto inmóvil sin Qwen/Blender nuevos. No publicar ni cambiar Región 1.
- El backend WebGL se condiciona a nuevo run Unity final y a prueba posterior sobre sus propios bytes; nunca reutilizar la QA de `38422576301` como prueba del nuevo artefacto. PR #51 sigue DRAFT y espera Windows legítimamente libre.

## Estado vigente — prevalece sobre la cronología inferior
- Rama única: integration/valoria-bastion1-player-scene-20261012. HEAD exacto de evidencias antes de esta actualización: ec998413d6d6829a0e18676a18a894071e0eb181. Último HEAD de código/QA ejecutado:55b9eddd7c130f68e39d4ccc41504423fc15f9eb. Obtener HEAD vivo antes de editar; los commits posteriores conservan pruebas/documentación.
- Última Unity38419031491 **SUCCESS**, sourcecf1f7bc69ff832f917fa9e41285ba6c809c320ac. Frames11826196119/full11825492257. Perfil cascade1/atlas1024/MSAA1/shadowDistance180. Aserradero a izquierda, camino/escalera unidos, factores RGB originales Qwen. Captura real final mobile-profile-native-initial.png.
- Última WebGL38420574701 **SUCCESS**, source7f6d708d57bd27e04c0375e79221d3fa9c932da3. Artifact11826517753, bastion1-exact-candidate-webgl-38420574701,187136907bytes,ZIPsha256:2a3c0c02cc7391d9b78a75ecf0422e17242f9700c74a06c3db850f94e4ff9758. Escena nativa exacta38419031491. NO repetir build/native/artista sin cambio real.
- Básica exacta38421408687 **PASS**: startup,canvas844x390 completo,pan táctil,selección/construcción aserradero,cuartel bloqueado,mundo/regreso,recarga/reset,0 errores fatales. Artifact11826269472. Cinco capturas y JSON durables en webgl-38421408687/.
- Run38421408687 global FAILURE sólo por full harness posterior: límite2.5s de confirmación de reset. JSON muestra ELDORIA_PLAYABLE_RESET armed=True después del límite; basic superó reset real. Espera cambiada a15s en4352b52d4e5544f4018e77e1559e656121910469 sin modificar gameplay/reloj/economía ni inyectar estado.
- Full exacta38422576301 **SUCCESS**, source55b9eddd7c130f68e39d4ccc41504423fc15f9eb,job115383507296. Artifact11827588127,bastion1-exact-webgl-browser-qa,9535479bytes,ZIPsha256:8feb2e27a327680b8102d1d877f1968f8c5936f16d35d61a55df0c009019bcdb. Se ejecutó sólo gate pendiente sobre MISMO WebGL38420574701. Ambas orientaciones PASS:3 selecciones reales(sawmill,bastion,barracks),progresión I-II,timers/recursos reales,entrenamiento,marchas,persistencia/recarga,reset y reset persistente.
- Final ambas:revision26,wood1030,stone995,gatheredWood720,gatheredStone700,marchidle,sawmill1,bastion2,barracks1,archers56,trained20,configured/scout/engendro=true,objectiveb2.complete. failure=null; comprobación fatalRuntimeLogs sin errores.
- Todas38 PNG originales + full-player-result.json + build-sizes.json conservadas en Git en webgl-38422576301/, commit ec998413d6d6829a0e18676a18a894071e0eb181. Actions grandes caducan; las capturas/reportes de Git son durables. GLB/blend grandes siguen en artifacts listados en cronología; descargar antes de caducidad si hacen falta.
- Inspección real independiente: revisados landscape-fresh-bastion-i,portrait-fresh-bastion-i,landscape-bastion-ii-complete y screenshot inicial básico contra referencia. Modelos/colores/estados presentes. **NO ART PASS ni8/10**: acantilado/fondo gruesos,agua plana,puente sin acabado arquitectónico,contactos/detalle vegetal insuficientes. HUD tapa composición horizontal; vertical muestra demasiado fondo. CI verde no cierra misión artística.
- Emulación Chromium/touch sobre Unity WebGL real; NO humano/manual independiente ni teléfono Android/iOS físico. No certificar FPS/performance física. Full14m37s en software GPU no es medida de teléfono.
- Recursos: Windows job115376480422 y Ubuntu115383507296 TERMINADOS; ningún job propio sigue ejecutándose. Lease del workstream sigue valoria-player-scene/windows-runner-heavy para integración pendiente; diferenciar reserva de ocupación efectiva. Releer active de main antes de usar. No usar runner para biblioteca histórica.
- Archivos ajenos preservados:WorldRegion1Runtime,ValoriaCurrentWebGLBuild,Pages/publish request,SESSION_HANDOFF,PROJECT_STATE. Sin otra rama/créditos de pago/inferencias ni experimentos nuevos durante este cierre. No promoción/publicación.

### Siguiente acción ejecutable desde chat normal
1. Obtener HEAD vivo de ESTA rama y leer active de pipeline/active-workstreams.json de main. Leer este bloque primero: instrucciones de cronología que dicen recuperar builds ya terminados están supersedidas. No reiniciar infraestructura artística.
2. Abrir webgl-38422576301/landscape-fresh-bastion-i.png y portrait-fresh-bastion-i.png junto a references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg. Revisar el encuadre/HUD concreto y preparar corrección de integración mínima dentro del trabajo existente; no nuevos artistas/estudios de assets. Cámara runtime:Unity/Assets/Eldoria/Scripts/Presentation/ValoriaParcelPresentation.cs(Home/Zoom/Focus);HUD:SlicePresenter.cs. Generator REAL:Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/Editor/ValoriaBastion1MeshPlayerScene.cs, no Scripts/Editor. ValoriaMeshPresentationV1.cs no existe en esta rama: no reconstruirlo por el scope histórico.
3. Para jugar, descargar11826517753 y comprobar run/source/digest arriba. QA ya superada: NO repetir sin cambio de código/escena. Arranque B1wood230/stone150/aserradero0/cuartel0/arqueros36; los finales de pruebas no son el inicio elegido.
4. Para cambio real, conservar GLB autoral9cf0fa48e93df288ed8b86409102c413e9fc22339f1126a79d4ec3f728fd18b1/fuentes congeladas. Validar gates focalizados/build/QA del artefacto NUEVO. No trasladar PASS anterior a un nuevo candidato.
5. Misión artística abierta: acabar paisaje/agua/puente/acantilado y composición antes de afirmar parecido aceptable. Restricciones vigentes:no nuevas ramas/no experimentos/no repetir trabajo/no runner histórico. Promoción/Pages pertenecen al workstream Region1; no sobrescribir su alcance.


## Rama y titularidad
- Rama única: integration/valoria-bastion1-player-scene-20261012.
- Owner actual: work-owner-authorized-bastion1-continuation-20261013.
- Recursos reclamados: valoria-player-scene y windows-runner-heavy, en pipeline/active-workstreams.json.
- No modificar WorldRegion1Runtime, ValoriaCurrentWebGLBuild, Pages, SESSION_HANDOFF ni PROJECT_STATE reservados al otro trabajo.
- Releer HEAD vivo y registro antes de promover. No abrir otra rama ni reconstruir familias.

## Estado inicial elegido
Bastión I en reconstrucción. Aserradero muestra su estado 0 de terreno/cimientos; estados 1 y 2 se activan por la progresión real. Cuartel vacío/bloqueado hasta Bastión II. Cámara y selección en coordenadas de los modelos. Conservar economía y cartas iniciales existentes, sin recompensas de QA ni inyección de estado.

## Resultados ya realizados — NO repetir
1. Fuente ciudad completa original: 38356460099, artifact 11792269250, GLB SHA256 3052ed2cc91e0dde177ec7bcca94254c3d3d2aacecb0e60101daa8f97e09f9bb.
2. Ensayo CC0 empaquetado original: 38388922583, artifact 11808120370. Blend a06c85de85a25ae3e5212d752af445f4b59296c2b0c1efa510bbddec6e02ffd6. Se reutilizan mapas Poly Haven licenciados, no se regeneran.
3. Ciudad PBR marrón + WebGL: 38395469618 SUCCESS, source c53108d7160160d55743b460526a03a2443031bc, artifact 11811847205. Visualmente RECHAZADA por marrón/ruido/contactos, no 8/10.
4. QA básica de ese WebGL: 38396278910 PASS en su ensayo básico, aunque run global rojo por fallo posterior del harness.
5. QA I-II completa exacta del mismo WebGL marrón: 38399110721 SUCCESS, source 3ca4c52e53bbb32e1d5065530b9e600dbfcfcd7b, artifact 11814743471. Ambas orientaciones, 3 selecciones reales (aserradero, bastión, cuartel), timers/recursos reales, persistencia y reset, 0 errores. Final: revision26, wood1030, stone995, gatheredWood720, gatheredStone700, sawmill1, bastion2, barracks1, archers56, trained20, configured/scout/engendro=true, objective b2.complete. 38 capturas. NO atribuir este PASS al nuevo candidato aún no compilado.
6. Césped y factores Gamma: 38398380999 SUCCESS, source 5428741afc02e8be8bf081910784363a6ce78376, artifact11814852361. Material todavía amarillo; no aprobación.
7. Inferencia Qwen3-VL8B genuina: 38399110675, artifact11815340532. Plan sculpt_path original de 5 nudos, 1758 vértices; no ampliar parámetros ni fingir autoría.
8. Error real de frustum corregido en render_artist_ab.py: AUTO interpretaba ortho_scale52 como anchura, recortando acantilado. Ahora 720x500, sensor_fit VERTICAL, altura52 y anchura74.88, equivalente a Unity1440x1000/orthoSize26. Replay 38401480696 PASS de VISIBILIDAD, no calidad.
9. Merge original 38402469730 rechazó 18 celdas protegidas. Executor tiene máscara explícita de ownership: conserva posiciones exactas abs(x)<22,-34<y<36,z>-1 y ejecuta parámetros Qwen intactos en su scope restante. También se corrigió variable t que sobrescribía nombre del target, sin cambio geométrico.
10. Merge con máscara 38403329338 SUCCESS, artifact11816670641. 1637 vértices modificados, 0 celdas protegidas. GLB4a69ad6cbc12cb5391ce4f290a80a5641a3ed65c4adf86bf88c2ba89f347654f. Blender frustum correcto:0.341111% píxeles de clay cambiados, NO ART PASS.
11. Mipmaps glTFast, filtro Trilinear/aniso2, KHR_texture_transform y conversión Gamma ya implementados y comprobados en Unity. Pérdida de referencia URP en segundo proceso corregida resolviendo GUID original48e3a09b57f028a4bb2e693d044ea6ca.
12. Unity A/B real 38405267853 SUCCESS, source f27b7b94755acfa62511c260c0dac30eb54e5667, artifact11819226738. Recuperado sin rerender en 38406612333 / artifact11819148039. Cámara JSON idéntica;86330/1440000 píxeles RGB>10 (5.9951%), MAE1.3854. Inspección independiente: cambio visible pero escena global aún no aprueba objetivo artístico.
13. Nuevo Qwen8B genuino de acabado: 38404956232. Respondió antes de editar, plan SHA714f34adc57ee8f7d1705dc59d7b8dd16a3ed97a16317e1f7b3c8af44e97fc0a. Fractura1.2m, paso4.5m, capas6m, strength.35, fase1.57, flat=false, grassRGB[.35,.55,.25], cliffRGB[.25,.25,.25], UVgrass.75/UVcliff.5. Blender falló ANTES de editar por numpy ausente; NO se repitió inferencia.
14. Replay exacto 38406612333 SUCCESS, artifact11818663305. Se instaló numpy y ejecutaron parámetros originales:2718 vértices, 0 cambios de meseta, UV/topología/otros meshes conservados. GLB731799f563ce1cf731d2226767585eaaebd1c094712f26d45a1c95653f08bbc9.
15. Exportador Blender perdió MixRGB multiplicadores. Detectado en Unity y JSON de GLB (baseColorFactor ausente). Reparación explícita 38409277470 SUCCESS: sólo dos factores del JSON cambiados; BIN/meshes/texturas byte-idénticos y plan original intacto. GLB9cf0fa48e93df288ed8b86409102c413e9fc22339f1126a79d4ec3f728fd18b1. No es nueva decisión artística.
16. Camino existente RoadFamilyV1 SHA683310d65fd83de40115b310b35ceae8b35b935baaf8305a068d8e1203709f67 reutilizado. GLB contiene MainRoad JOINED, 1 renderer/3 primitivas, no 4 nodos. No cambiar el archivo congelado. Instancia a anchura4.1 y z[-5.5,18.5], desactiva CentralRoad antiguos.
17. Corrección de contacto SOLO en copia de terreno de candidato: Bastion1RoadClearance.cs baja soporte bajo pavimento y escalera. Originales de carretera, escalera, modelos, UV y anclajes de parcelas intactos. Esta corrección de integración NO se atribuye a Qwen ni se confunde con su máscara protegida.
18. Unity con RGB transferidos/camino real:38409530725 SUCCESS, source136ec0fcd70de9ed5a8d9bf6a630803ea3b518b4, artifact11820981755(frames) y11820782242(full). Logs confirman valores lineales originales y shader Gamma.
19. Última escena nativa:38412903213 SUCCESS, source7bcc33921e122d7abe94f865268e67c82b803ec4, artifact11822493278(frames) y11822978137(full). Captura real independiente inspeccionada: escalera completa visible, camino continuo existente, césped verde/oliva. Queda trabajo de paisaje/agua/puente/acabado para objetivo global; no 8/10 certificado.

## Compilación jugable actual — recuperar antes de repetir
Se ha creado .github/workflows/valoria-bastion1-exact-candidate-build.yml con request explícito pipeline/valoria-bastion1-candidate-build-request.json. Restaura EXACTOS Valoria.unity y Generated/GeneratedNormals de 38412903213, preserva World, compila WebGL sin repetir artistas/assembly/capturas.
- Primer intento38415030579 falló ANTES de compilar al localizar prueba del artista. Su artifact contiene un WebGL viejo del runner: NO USAR.
- Corregido: descarga frames exactos aparte, verifica su qwen-integration-proof.json, elimina output viejo.
- También se detectó que URP canonical móvil tiene shadowDistance50/cascade1/MSAA1, distinto al preview180/cascade4/MSAA4. El candidato ahora liga el StudyURP EXACTO capturado en todas las Quality/Graphics temporales. Sin esta corrección, el juego perdería sombras de la captura. Rendimiento físico sigue sin medir.
- Request build_attempt2 dispara nuevo run. Recuperar el run de HEAD vivo y logs; reparar antes de asumir éxito. Arte/publicación NO autorizados por verde técnico.
- Workflow governance217 workflows PASS.

## Siguiente acción concreta
1. Recuperar compilación exacta actual; resolver fallo de transporte/compilación si existe.
2. Actualizar .github/workflows/valoria-bastion1-browser-qa.yml para consumir SÓLO ese nuevo run/sha/artifact exitoso, no c531 histórico. Ejecutar QA básica + I-II reales en ambas orientaciones, con identidad móvil/canvas visible, sin inyectar estado ni tocar reloj.
3. Revisar capturas WebGL contra nueva Unity/reference. No confundir native_before con player screenshot ni clay con Unity. No publicar por test verde.
4. Continuar acabado pendiente (paisaje/puente/agua/material de roca y contactos), conservar mejoras actuales. Fuente Hero V3/familias congeladas intactas. No reiniciar infraestructura artística.

## Entorno de interacción
Work local recuperó Chromium oficial pero rechaza sockets POSIX; Playwright local no abre. Los workflows Ubuntu disponibles ejecutan Chromium/touch sobre WebGL real. Es emulación automatizada con controles reales, NO móvil físico ni humano/manual independiente. Las desconexiones de cliente no tienen diagnóstico confirmado; archivos/commits y runs externos permiten recuperación, pero no prometer ejecución indefinida del agente tras acabar turno.

## Actualización posterior — estado vivo a recuperar
- 38415715022 restauró fuente/prueba correctamente pero Actions propagó robocopy exit1 (es éxito de copia). Normalizado sólo tras comprobar códigos0..7; no se ocultó fallo.
- 38416315485 SUCCESS, source dbfce12c50b2e95b249057a3fe8eee95ada82c65, artifact11823649081 (187137867bytes). Primer WebGL real con escultura/colores/camino/escalera, perfil HQ4 cascades/MSAA4 y aserradero todavía screen-right.
- QA básica exacta38417838180: startup real PASS (revision0, wood230,stone150,sawmill0,bastion1,barracks0,archers36), canvas844x390 completo. Falló ANTES del pan/selección por screenshot30s timeout en Chromium/software. No clasificarlo como prueba completa ni inventar fallo de economía.
- Coordenadas de aserradero/cuarter/proxies corregidas: cámara desde X negativo, screen-left equivale a UnityX positivo. Ahora sawmill X+16, barracks X-16, proxies coincidentes. Native38417295167 SUCCESS, source0118295e4b1b832004b737a468d02b43219a43ce, artifacts11824843619(frames),11824718787(full). No repetir esa corrección ni volver a flippear sin evidencia.
- Perfil más ligero para resolver captura/software: conservar shadowDistance180 y todos los modelos, cascade1, atlas1024, MSAA1 y RTcapturaAA1. Native38419031491 SUCCESS, sourcecf1f7bc69ff832f917fa9e41285ba6c809c320ac, artifact11826196119(frames),11825492257(full). Captura independiente inspeccionada: aserradero screen-left, escalera/camino visibles, sombras conservadas y colores autorales correctos. Mejora técnica, NO ART PASS ni rendimiento físico.
- Request build_attempt4 ahora consume EXACTAMENTE38419031491; commit7f6d708d57bd27e04c0375e79221d3fa9c932da3. Recuperar nuevo run de exact-candidate-build antes de repetir. No consumir artifact HQ anterior como candidato final.
- .github/workflows/valoria-bastion1-browser-qa.yml ejecuta ahora QA básica real, no reutiliza PASS marrón antiguo; test basic exige run/SHA por env. Al aprobar nuevo build, actualizar run/SHA/name a artefacto NUEVO y ejecutar básica más full-player-qa.cjs I-II ambas orientaciones sobre ese mismo build.
- Native workflow cancel-in-progress=false evita cancelar el build al corregir escena. Misma lease, sin otro workstream.
- Faltan QA final del perfil ligero, inspección real WebGL y acabado global de paisaje/agua/puente/roca hacia referencia. No declarar8/10 ni publicar/main por compilación.

## Relevo incremental 2026-10-13 — perfil ligero compilado, QA exacta en curso
- HEAD de implementación/QA antes de este registro: b88a87922cbbb85372c50baa19411bfb9f453cd2 (misma rama; sin nuevos experimentos).
- WebGL38420574701 **SUCCESS**, source7f6d708d57bd27e04c0375e79221d3fa9c932da3, artifact11826517753, name bastion1-exact-candidate-webgl-38420574701,187136907bytes,digest sha256:2a3c0c02cc7391d9b78a75ecf0422e17242f9700c74a06c3db850f94e4ff9758.
- Fuente nativa EXACTA38419031491/sourcecf1f7bc69ff832f917fa9e41285ba6c809c320ac. No repetir Qwen/Blender ni captura Unity.
- QA38421408687/sourceb88a87922cbbb85372c50baa19411bfb9f453cd2 está ejecutándose en Ubuntu, job115379390718. Básica y full-player I-II con ambas orientaciones sobre este mismo WebGL; no reutilizar resultados históricos.
- Captura nativa real final conservada en Git: mobile-profile-native-initial.png en esta carpeta, blob8eef149f60f002239dc60cf31136fc8554da8e39. Fuente frames11826196119; no es screenshot WebGL.
- Siguiente acción ejecutable: recuperar38421408687/jobs y resultado; si falla revisar logs115379390718 y artifact; resolver el fallo concreto. Si aprueba, inspeccionar PNG WebGL y conservar JSON/capturas finales en esta misma rama, actualizar este documento y estado vivo. Todavía no hay ART PASS/8/10.
- Instrucción vigente del propietario: terminar acciones de integración actuales, sin ramas ni experimentos nuevos ni biblioteca histórica de assets; conservar avances progresivamente. No lanzar otra inferencia o runner por continuidad.
