> **ACTUALIZACIÓN DE ENTREGA GRÁFICA — 19 OCT 2026.** El resultado presente es `NATIVE152_WEBGL_QA_PASS` (run Windows `39305545377`), pero la web privada sigue en `native143`. Paquete verificado artefacto `12213501867`; cero fallos runtime/red, guardados exactos; no certificado ART_APPROVED ni móvil físico. [Detalle y pendientes](GRAPHICAL_LEAP_NATIVE152_DELIVERY_20261019.md). La reparación de validaciones tratada más abajo permanece cerrada.

# Estado actual 19-10-2026 — incidencia de validación reparada y entrega web verificada

Continúa el mismo workstream y main. Esta cabecera supera los bloqueos y estados históricos inferiores.

- Recuperación real: run 39274718168 restauró 10.212 archivos Unity desde el commit exacto; el inventario anterior cortaba la salida de Git con Select-Object -First y abortaba la restauración. Cambio efectivo en .github/workflows/unity-slice.yml: inventario completo, -C explícito y --full-tree.
- NO certificar ese run: sus XML dieron 20 EditMode/32 PlayMode PASS y Build Successful, pero se ejecutaron en valoria-layered-source/Unity. El argumento no documentado -projectPath=Unity fue ignorado; Unity abrió el último proyecto usado. La búsqueda del ejecutable en Unity/Builds falló por esa discrepancia. La evidencia de las asambleas permite detectar el falso cierre.
- Reparación adicional ya en main e5eae7119d2635497da55e2f72c6e9321f295dcd: argumento -projectPath separado de la ruta absoluta entre comillas en los nueve lanzamientos; cada XML debe identificar asambleas bajo el Unity/Library/ScriptAssemblies exacto del checkout. Una prueba aprobada en otra copia ahora falla explícitamente. No se relajaron pruebas.
- CONFIRMADO: Unity gate 39277900049 COMPLETED / SUCCESS, source e5eae7119d2635497da55e2f72c6e9321f295dcd. Restauración exacta, EditMode, PlayMode (incluidos controles de identidad de asambleas), Windows player y captura benchmark PASS. Artifact de pruebas/build 12200516729; capturas 12199973254. La confirmación sustituye el estado pendiente anterior.
- Sites ACTUALIZADO: proyecto privado EXISTENTE appgprj_6ad4ce602c808191859268d570c93352, misma URL https://valoria-city-world.kbkvc5c45n.chatgpt.site. Versión 3, deployment appgdep_6ad63a0012448191b64d52721e4479a6 SUCCEEDED, source Sites 3c20f30dccaad6cdad7c1356b57e60464c41e57e. Sustituye native111 por paquete native143 exacto del artifact 12187980116; cinco hashes originales verificados (184860322 bytes) y 18 archivos del archive idénticos. QA browser independiente previa: run 39253871138, artifact 12187705647; no se reconstruyó WebGL. Se mantiene acceso exclusivo propietario. Esta publicación usa esa evidencia independiente, NO el gate general todavía pendiente.
- ENTREGA VERIFICADA: el propietario autorizó explícitamente la lectura del Site a las 18:12 Europe/Madrid. Los 18 archivos publicados coinciden por tamaño y SHA-256 con el paquete probado. index.html redirige a / en el mismo origen, donde coincide exactamente; no se enviaron credenciales a otro origen. El bloqueo previo de revisión automática está resuelto. No se realizó un nuevo recorrido browser en el servidor ni prueba física móvil; se mantiene la evidencia browser independiente del mismo paquete.
- Comprobador de paquetes: reparación 090e8b0c0f56606a53c5c3f56c99fae69eb2b477 acepta BOM UTF-8 de los JSON Windows reales. 16/16 tests node pasan, incluido caso BOM; el paquete real pasa inspectPreparedSite con cinco archivos originales y un flujo fragmentado. Se conservan comprobaciones de procedencia, hashes y rechazo de rutas inseguras.
- Android: el fallo de compilación 1.7 YA fue resuelto antes de esta sesión. Run 39216718830, source 616f8c431fb549e170b0118a4fd405d4b4b1128a, artifact 12169359211, resultado BUILD_AND_SIGNATURE_PASS. No equivale a publicación ni instalación física; updates/android/family.json sigue disabled. No promover ese APK antiguo como equivalente a native143.
- No se ha hecho arte nuevo ni modificado reglas, economía o guardados en esta reparación. El toque directo en puerta y la prueba física móvil siguen sin cobertura nueva. El recorrido browser I-II normal-clock previo continúa siendo el del artifact exacto citado abajo.


Evidencia de identidad HTTP de los 18 archivos (SHA-256; lectura autorizada del Site privado):

```json
{
  "url": "https://valoria-city-world.kbkvc5c45n.chatgpt.site/",
  "revision": "native143",
  "published_files_identical": true,
  "files": [
    {
      "file": "Build/ValoriaWebPlaytest.framework.js.unityweb",
      "bytes": 84816,
      "sha256": "8a853c5e9f9baa8b4a3986f187e70fd0a991949b668c95995664c2ba85b095cb"
    },
    {
      "file": "Build/ValoriaWebPlaytest.loader.js",
      "bytes": 47867,
      "sha256": "6cffce9c86356c56bf2ea4672874efa76edc3e95d858dc5b0818dcc65c134ce8"
    },
    {
      "file": "Build/ValoriaWebPlaytest.wasm.unityweb",
      "bytes": 10000353,
      "sha256": "474bdf50dcaf613abe52f1d0ad4c2db03434d13713ae5c950bd8e8f9c953f73d"
    },
    {
      "file": "TemplateData/webmemd-icon.png",
      "bytes": 1670,
      "sha256": "c56f5494640cedda273aad38d58e6ab9da625d5b3f4d875c0ff92c6192a569c2"
    },
    {
      "file": "build-manifest.json",
      "bytes": 1007,
      "sha256": "a44187421c3fb1fa21af726fb8b9e0ff5aada4be0dc72da22ed57a28e1b76f2a"
    },
    {
      "file": "city.png",
      "bytes": 2658815,
      "sha256": "388464e38c27ea164e8838367c8a8dd01570407d867a18d8ae7685d26deef8f1"
    },
    {
      "file": "index.html",
      "bytes": 1912,
      "sha256": "cb126f498956243188f9c074b8504e66cfcab186f77ac0c17070487ac64d8cc6"
    },
    {
      "file": "payload/b820fb7908629667092c/000.bin",
      "bytes": 20971520,
      "sha256": "18964fd2d0473d3cfa8b28d6d2bbb70beb989c2e483950e85431150f0a3e78e2"
    },
    {
      "file": "payload/b820fb7908629667092c/001.bin",
      "bytes": 20971520,
      "sha256": "491cf9f4117f1a996726076f253c1af847190659e4c5cc90194ff0f85f99f510"
    },
    {
      "file": "payload/b820fb7908629667092c/002.bin",
      "bytes": 20971520,
      "sha256": "d6528c3455b95ee1fd13c872153db65d8b9aebd0cd3b4b356dcee31103252355"
    },
    {
      "file": "payload/b820fb7908629667092c/003.bin",
      "bytes": 20971520,
      "sha256": "281f3ba8436ed256bdad924972070037248f6848e8fdc5a7ddb91c9176a28e88"
    },
    {
      "file": "payload/b820fb7908629667092c/004.bin",
      "bytes": 20971520,
      "sha256": "c1126bd7ea797e1b729482e70b3efa09764ab7d205f72b3c6859716dc5bac72f"
    },
    {
      "file": "payload/b820fb7908629667092c/005.bin",
      "bytes": 20971520,
      "sha256": "422efb1ecbb3070dd8d1df691f245f557b06efccc2bed07b13202afb6c553997"
    },
    {
      "file": "payload/b820fb7908629667092c/006.bin",
      "bytes": 20971520,
      "sha256": "87605d6f9e6aebf198b36d333c8ae7ae6d2fe36f3c006baffce3b5de156d654c"
    },
    {
      "file": "payload/b820fb7908629667092c/007.bin",
      "bytes": 20971520,
      "sha256": "c007b5f18f51f9a9aaa1e3baa4ddf0f74318c4f5909668fcc72e851cb0278ca3"
    },
    {
      "file": "payload/b820fb7908629667092c/008.bin",
      "bytes": 6953456,
      "sha256": "dcb9b980dd8bea92add533911ea153a897a400fb6fd2a0d30b79956099ffc702"
    },
    {
      "file": "play.js",
      "bytes": 4477,
      "sha256": "ea99b509d3d6483d65483906db359bfc42e96c93cd4d25562514fa1126ea4ba4"
    },
    {
      "file": "style.css",
      "bytes": 2924,
      "sha256": "1de9bf732e5ff9179625eb5aef90abac80064ebc77cb9469aa5d67966df4b53d"
    }
  ],
  "live_browser_playthrough": false,
  "physical_mobile_tested": false
}
```

---

# Recuperación 2026-10-19 — I–II Windows PASS; despliegue privado y gate Editor pendientes

**Estado verificado: OPEN — NO DECLARAR PUBLICACIÓN NI CIERRE ARTÍSTICO.** Recuperación del mismo workstream `valoria-master-plan-layered-construction-20261016`, sin crear ramas ni proyectos. Fuente del juego WebGL conservada: `b8127112890ca64c808e767e74c2a536747f6bf9`; HEAD del repositorio antes de la reparación CI: `f1f257cbd42012e5daa8608572312c60e53ff418`. La compilación exacta procede de run `39251384413` (job `native` PASS, artefacto `12185029720`), aunque el mismo run tuvo un job de QA Linux fallido. **No volver a compilar el WebGL por ese fallo de test.**

**QA funcional Windows normal-clock realmente aprobada:** run `39253871138` / job `118064149702` SUCCESS, WebGL de la misma compilación, Playwright Chromium/Edge con GPU, horizontal y vertical. PASS: partida nueva, bloqueo temprano de botón Mundo, zoom real y retención de cámara, desplazamiento mundial en cuatro direcciones, escenas de ciudad/mundo, selección/encuadre, todas las acciones primarias del recorrido Bastión I–II, narración, operaciones con duración real, marchas visibles de ida/regreso, recompensa, recarga y guardado byte-identical SHA-256 `0b9228bc4ccb01f92ec7ef99b2597ae26324a9c9b8a48adb785b82c350f92a31`. Cero errores de runtime/red; vídeo y capturas artefacto `12187705647`; paquete Site validado `12187980116` (`valoria-verified-private-site-windows-39253871138`), SHA-256 del ZIP de upload `1f2b02f8c010ed9183ba677832682388c93d75f7c781b137e41e2a54a86ca1a9`. **Cobertura explícitamente no realizada:** clic visible directo en collider de la puerta desde el encuadre inicial; centro de puerta fuera del viewport en el intento temprano. La regla de acceso compartida y las rutas normales figuran PASS en `integration-result.json`, pero no equivalen a ese toque específico.

**Fallo de infraestructura separado:** run `39251384434` (Unity slice source/editor gate), `EditMode` abortó antes de ejecutar tests porque Unity recibió `-projectPath` absoluto con doble prefijo. Reparado en `main` commit `5548bd93c110f8fa223892335d876f0de4a7e6c3`: los nueve lanzamientos del workflow `.github/workflows/unity-slice.yml` usan `-projectPath Unity` y `-WorkingDirectory $env:GITHUB_WORKSPACE`. **La reparación todavía requiere ejecución de confirmación**, no rerun del SHA antiguo ni declaración de EditMode PASS. Modificación de infraestructura solamente; código/escena/bytes del WebGL validado intactos.

**Publicación privada pendiente:** la última publicación registrada del proyecto Sites `appgprj_6ad4ce602c808191859268d570c93352` / URL `https://valoria-city-world.kbkvc5c45n.chatgpt.site` sigue siendo native111, deployment `appgdep_6ad4e341a4488191a25befd593bdfc27`. NO hay evidencia de despliegue del nuevo paquete `12187980116`. La sesión de recuperación no dispone de operación Sites para subir al proyecto privado, por tanto no se ha realizado la publicación. NO usar Pages pública ni anunciar que el nuevo enlace ya está actualizado.

**Siguiente operación verificable:** obtener una sesión/conexión que pueda publicar en el proyecto privado Sites EXISTENTE; promover los bytes validados del artefacto `12187980116` sin recompilar Unity; comprobar identidad de versión/archivos y recorrido en la URL privada desplegada en las dos orientaciones; ejecutar gate Editor corregido en el próximo job compatible sin duplicar jobs o recompilar innecesariamente; resolver por separado la cobertura del clic físico sobre puerta. Conservar la versión anterior hasta superar la verificación de publicación. Solo al cerrar esas obligaciones, avanzar al trabajo artístico del mismo juego en `main`.

**Arte actual:** muestras reales Unity `environment-city-wide-before/after.png` y `environment-world-wide-before/after.png` y `environment-finish-unity.json` verifican modificaciones históricas de caminos de servicio y sustitución visual del castillo del mundo por una familia más cercana a Valoria, pero con ganancia gráfica todavía insuficiente para proclamar 7/10 o `ART_APPROVED`. Mantener las mejores fuentes preexistentes native61, native70–74, cuartel y aserradero; no se ha producido ni integrado arte nuevo en esta recuperación. Rendimiento Android/iOS físico no medido, presupuesto 0 €.

---

# Estado vigente — actualización privada native111 publicada

18 octubre 2026. Continúa la MISMA misión: elevar la calidad visual de Valoria con su artista IA existente, Royal Gold y Zona1 4X. No reiniciar ni usar Tripo.

- Enlace: https://valoria-city-world.kbkvc5c45n.chatgpt.site (mismo proyecto privado appgprj_6ad4ce602c808191859268d570c93352).
- Publicación confirmada appgdep_6ad4e341a4488191a25befd593bdfc27, fuente Sites 8500154eeb25e3bdc955cc47d3d9ef7c33e0a68f. Unity native111 39138751241 SUCCESS; fuente 48837605705fed198ec485f2c2726af2ffa5eea9, salida 663acb07f3a2693a70f46b3e94a40340a38ad84f. Web artifact12141221363; revisión artifact12141475371.
- Nuevo aserradero de Qwen + BlenderMCP instalados, inferencia real105 y reparación107 bajo el mismo plan. GLB6544ce7c993ff8f55b7c0420ebcfa9a93615c47c67e625cbde0e4382f20c4cbd. Tejado azul cerrado, piedra y madera coherentes; frente abierto orientado a cámara. Fuentes originales intactas. A/B nativo con misma cámara/luz; mejora observada, NO aprobación artística global.
- Parcelas rectangulares ocultas; colisiones/reglas conservadas. Etiquetas de construcción y flecha del objetivo llevan al destino. Mundo se desplaza horizontal y verticalmente. Menús ocultos hasta que existe su mecánica/progreso. Marco UI simplificado.
- Textos originales de tester-v0265: Narrador/Aldric en inicio, aserradero, hallazgo del bosque y cuartel. Continuar por pulsación real; crónica consultable; marcas de lectura fuera del estado económico. No se anuncian mecánicas futuras ausentes. Ver WEB_DIALOGUE_RESTORATION.md.
- Native111: recorrido completo I–II, acciones uGUI reales, narrativa resistente a refrescos, diálogo largo cabe en vertical, mismo estado ciudad/mundo, recarga y hashes de guardados canónicos PASS. Captura integration-story-mark.png inspeccionada completa y legible.
- WebGL exacto empaquetado con CRC ZIP y hashes de todos los archivos, transporte por fragmentos y prueba de bytes/orden/URL/longitud PASS. Publicación succeeded. Navegador físico/iPhone y rendimiento NO verificados.
- Calidad visual global sigue pendiente: terreno y caminos hacia edificios, integración de apoyos con terreno, vegetación/materiales y mundo prototipo. La prueba minY del aserradero conserva la base anterior; NO demuestra por sí sola contacto físico con el terreno. No certificar 8/10 ni ART_APPROVED.
- Android97/1.5 intacto. UI/mundo ramas independientes intactas. Próximo trabajo: mejora visual sostenida de ciudad con capturas reales, sin otra reinvención del modelo ni del pipeline. Registro mantiene ownership; runner libre al terminar111.

---

# Valoria continuation: city, Royal Gold UI and Zone 1

Owner explicitly transferred the saturated Valoria chat on 18 October 2026 and requested interface ownership and Zone 1 integration. Same production branch, no restart. Main registry claim 5bb6040587610ddb6f34b30a6a14ceec8775aca0.

Retained city/Android output c0c498b816c1e13be7edb77f8cf5c5428ed7d39a, native97 39128057990 success. Public content report verifies HTTPS bytes for Android97, not a physical phone test.

Pinned UI 236a81aac894ea0845b85c90310b92dddb32d10f and World4X f3ccc8d0b7767472ef09b02c3091f6d676f204ee are consumed only through their component/source directories. Their independent branches remain intact. World sector is retained prototype art, not ART_APPROVED; no new procedural world is substituted.

Candidate: Integration/Valoria_City_World_UI.unity. Original city scene stays unchanged until native/visual review. RoyalHUD accepts the existing session; original presenter continues the sole Advance loop and input gesture ownership, with external selection/hit-test hooks. World route swaps active camera without another save or wallet. Editor probe imports actual GLB and persists cloned resources, checks city/world/city state identity, real Royal build/gather buttons and reload; genuine screenshots are required.

Native98 requested by request.json. Next: inspect exact run and original images; repair compile/runtime/layout failures; complete I-II with the new UI before promotion. No APK/publication for this unreviewed integration candidate. Primary mission remains visual/playable Valoria, Bastion I-II, RoyalGold and Zone1.

Native98 39131782550 / source31ffb22d4cde85d21c4e61c3b3dea1642d1a3703: native compilation passed; runtime builder stopped with `Original authored city missing`. The source city is a parent transform, not the child renderer name. Native99 fixes exact original hierarchy anchors, exercises real uGUI pointer events and full I-II progression. Original city and stable Android are unchanged.

Native99 39132144937 / source500534e7db18f290f78797ecca20ace7889cb4d0: actual imported scene + RoyalGold render in city and world; single-camera and navigation state checks PASS. Actual pointer action FAIL (raycast blocked), so no playable certification. Genuine original screenshots inspected: city retained and UI visible; world prototype still materially below art target. Also detected non-readable texture clone warnings; native100 fixes persistence via GPU readback and separates panel activation from click frame. Artifact12138311731 preserves all failure/capture evidence.

Native100 39132392803 / source9893da1eb1a36ae2950ef75e6c56d539f83100ed / output02b5632e15de5328a299d017bd1d9bf24ff4c45f: INTEGRATION_PASS_ART_REVIEW_REQUIRED. Real Royal uGUI pointer actions completed sawmill, world gathering/scout, Bastion II, barracks, recruitment, march confirmation and Engendro; reload and both canonical save hashes PASS. Artifact12138417802. Inspected genuine city/world landscape and portrait images. World retains sparse prototype art and some provisional props: NOT ART_APPROVED. Candidate is integrated, not yet promoted to original scene or APK.

Native101 only corrects clipped world header and explicitly labels return Ciudad/City with native assertions. Continue: named world targets/visual readability, existing sawmill-state2 comparison, then candidate promotion and exact bundled-scene proof before compatible Android1.6/code7. New runtime code requires APK update; do not publish this as content for code6. Primary mission remains the existing Valoria visual/playable work, not pipeline work.

Native101 39132807199 PASS; outputc4055d2cc23f87640c0b788903e20553626c04e5. Native102 39132971029 sourcef2ee9b1e9af8d3f2f5060df011f8d71ba81f6181/output44ec9273df61c387f87e9eaac7b8de39872e1f3f: genuine same-camera sawmill before/after reviewed. Completed STATE2 has full roof, enclosure and timber stock; retain as built model in integrated candidate, not formal whole-scene art approval. Native103 web_playtest first reruns full integrated I-II with this variant, then compiles exact candidate WebGL. Owner18Oct15:43 Europe/Madrid explicitly requests iPhone browser link and reiterates CITY VISUAL QUALITY priority. Private Sites playtest authorized; older site/production branches remain intact. Web save is existing browser PlayerPrefs path, separate from Android.

# Estado actual — Valoria integrada y prueba privada para iPhone

Actualizado 18 de octubre de 2026. La prioridad del propietario sigue siendo MEJORAR LA CALIDAD VISUAL DE LA CIUDAD, junto con su jugabilidad. No reiniciar el proyecto ni sustituirlo por una maqueta web.

- Prueba privada publicada: https://valoria-city-world.kbkvc5c45n.chatgpt.site
- Proyecto Sites: appgprj_6ad4ce602c808191859268d570c93352. Reutilizar este ID y mantener acceso privado en futuras actualizaciones.
- Publicación confirmada: appgdep_6ad4d23be5b08191aebfa4ce91b07c63; fuente del sitio 8aa0c1a90f839e8ae09c47db4a4135a24e3de571.
- Unity native103: 39133202399 SUCCESS; fuente ac7654d9d778a41c74157e1504e2c852de5e1b49; salida 3347e29b91ba94427648b0250e898ef10d125a05.
- Escena jugable: Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity.
- Royal Gold + Zona1 4X usan la misma sesión y una cámara activa. Ciclo real I–II completo con botones uGUI: construir, recolectar, despejar ruta, mejorar Bastión, cuartel, reclutar, confirmar marcha, Engendro y recargar. Guardados canónicos intactos.
- Mejora visual retenida: aserradero terminado STATE2, 4876 triángulos, tejado completo/cerramiento/madera almacenada, comparación nativa102 con misma cámara/luz. Fuente editable existente conservada. La escena original independiente queda intacta.
- WebGL compila la escena actual integrada, no la antigua escena SHARP/Region1. Artefacto12138697606; revisión103. Archivos web177785751bytes. Transporte en fragmentos con hashes verificados, carga con progreso y guardado local del navegador independiente de Android.
- Publicación verificada; ejecución en navegador y rendimiento físico de iPhone NO verificados en esta sesión. Sin control-browser disponible, no se sustituyó por otra vía de automatización. El usuario dispone del enlace para probar Safari.
- No ART_APPROVED: materiales, vegetación, integración edificio-terreno y acabado global de ciudad pendientes. Mundo conserva arte prototipo. Próximo trabajo: legibilidad de objetivos del mapa y mejora visual sostenida de la ciudad con comparativas nativas.
- Android97/1.5 conservado. Esta web no actualiza el APK. Nuevos cambios de código requieren APK compatible antes de publicar contenido Android.
- UI fuente236a81aac894ea0845b85c90310b92dddb32d10f y mundo fuentef3ccc8d0b7767472ef09b02c3091f6d676f204ee consumidos en esta rama. No sobrescribir sus ramas independientes ni reactivar proyectos pausados.

