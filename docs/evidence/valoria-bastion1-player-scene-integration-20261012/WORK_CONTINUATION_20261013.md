# Bastión I — continuidad de Work, trabajo abierto

Objetivo: continuar la integración existente hasta escena jugable cercana a la referencia, con colores correctos, autoría IA real y prueba de jugador. 0 € / ningún crédito de pago. NO declarar ART PASS ni 8/10 por CI verde.

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
