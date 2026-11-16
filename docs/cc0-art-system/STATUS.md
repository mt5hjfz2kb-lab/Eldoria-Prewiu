# Eldoria CC0 Art System — estado de continuidad y evidencia

**Experimento registrado:** `Eldoria CC0 Art System — Sistema artístico gratuito reproducible`.  
**Workstream ID:** `eldoria-cc0-art-system-20261116`  
**Rama aislada:** `experiment/eldoria-cc0-art-system-20261116`; **no mergear ni modificar main**.  
**Fecha de actualización:** 16/11/2026. Leer también `README.md`, `asset-lock.json`, `ART_REVIEW_AND_PIVOT_20261116.md` y `main/AGENTS.md`.

## Qué se ha investigado, hecho y probado de verdad

| Línea | Estado | Evidencia |
|---|---|---|
| Reglas de arte, recursos comerciales gratuitos y derechos | Documentado; licencias externas aún no acreditadas pieza a pieza | `README.md` y `asset-lock.json` |
| Bastión original I–V y composición inicial | **Modelos y Blender probados**; aspecto insuficiente | [Action 43847137592](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847137592) |
| Importación de Quaternius auténtico | **Probada en Blender**, seis archivos originales SHA-256 | [Action 43844725460](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43844725460); fuente privada, no redistribuir |
| Mezcla directa Quaternius/fortaleza propia | **Rechazada artísticamente**; siluetas y materiales distintos | [Action 43847893229](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847893229) |
| Ciudad cohesionada y edificios diferenciados | **Blender probado**, estéticamente solo parcial | [Action 43851647751](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43851647751) |
| Agrupación estática de mallas I–V | **Blender/GLB probado**; no equivale a medición GPU/iPhone | [Action 43853428203](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43853428203) |
| **Director V3**: funciones propias y bosque | **Blender + 5 GLB + 3 capturas reales**, render con problemas artísticos | [Action 43866662062](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43866662062) |
| **Director V4**: nuevos materiales y fachada | GLB y renders generados; **primer gate global rechazó progresión visual insuficiente** (3,30 de diferencia media a escala de imagen) | [Action 43870376065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43870376065); otros runs V4 revisar al retomar |
| **Director V5**: corrección geométrica, entorno y 5 niveles | **Implementado en código real Blender**, workflow Ubuntu disparado. **No declarar pass hasta terminar run y revisar imágenes** | `tools/cc0-art-system/director_v5_scene_refine.py`; `.github/workflows/cc0-director-v5.yml`; primera ejecución [43873251581](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43873251581) |

## Diagnóstico artístico acumulativo

En los renders auténticos de Director V3/V4 seguían apareciendo cubiertas poco trabajadas, bosques con copas cónicas y claras, un plano de tierra demasiado uniforme y fortaleza V insuficientemente superior a I al examinar todo el encuadre. La referencia conceptual generada con IA **NO ES** modelo, render Unity, ni garantía de producción posible. No volver a presentar ilustraciones como avance del juego.

El nuevo Director V5 modifica físicamente: **sendas sobre terreno (sin plataformas), copas de coníferas irregulares y oscuras, rocas y hierbas ancladas, maquinaria y abastecimiento de cuatro sectores, torres y elementos verticales exclusivos del Bastión V, iluminación cálida legible.** 5 exportaciones glTF se regeneran desde V4 real anclado al run 43870376065; guardan datos reales de geometría, no una imagen pegada.

## Estado de pruebas obligatorio (NO confundir)

- **Blender V5:** pipeline lancé; inspeccionar logs y PNGs **en persona**; no hay aceptación visual automática.
- **Importación/capturas Unity 6000.3.23f1:** pendiente. La escena nativa del proyecto no se ha sustituido ni probado con Director V5.
- **Rendimiento iPhone WebGL/Android:** pendiente. El número de mallas debe reoptimizarse para nueva geometría y probar memoria/draw calls reales; una reducción de mallas en Blender no demuestra FPS.
- **Licencias externas comercializables:** solo paquete original, perfil, edición y destino autorizados. V5 crea geometría propia y **no incorpora** archivos originales de terceros. El repositorio GitHub sigue siendo público.
- **Calidad artística:** V3/V4 no aprobadas. V5 es candidata, **no aprobada**.

## Continuación en cualquier chat

1. Leer `main/AGENTS.md`, `main/pipeline/active-workstreams.json` y PRs abiertas. No cancelar, ocupar ni tomar Windows de otros workstreams.
2. Comprobar HEAD **real de esta rama** y jobs de `cc0-director-v5.yml`. En particular recuperar el run `43873251581` y su artefacto `eldoria-cc0-director-v5-real-3d`.
3. Si falla: leer el primer paso fallido y **corregir la causa**, no declarar éxito porque existan renders/GLB. Si pasa: abrir capturas reales I, V, vertical; comparar con V3, defectos de composición, iluminación, utilidad de industrias y progresión.
4. Si la calidad sigue siendo baja: reestructurar edificios/terreno, no encadenar retoques cosméticos. Registrar defectos persistentes y hipótesis de solución.
5. Solo tras inspección artística positiva: instalar modelos originales/optimizados en sandbox Unity aislado, probar los botones y escenas, capturar desde el Editor y validar dispositivo físico cuando sea posible. Sin fusionar ni publicar sin aprobación del propietario.

**Principio rector:** código ejecutado ≠ geometría válida ≠ calidad visual ≠ juego estable. Distinguir siempre estas cuatro cosas.

## Director V6 — reconstrucción estructural de presentación (16/11/2026)

**Motivo:** las capturas reales de V5 en [Action #43874433432](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43874433432) muestran caminos superpuestos claros, terreno plano con poco interés, árboles repetitivos y arquitectura secundaria poco diferencial. El modelo V5 sirve como base geométrica, **no está aprobado**.

**Código nuevo:**
- `tools/cc0-art-system/director_v6_structural_redraw.py`: importa cada GLB físico de V5; elimina mallas problemáticas, **reconstruye una superficie continua con senderos pintados por vértice**, añade coníferas irregulares reales, vegetación, texturas originales retocadas físicamente, refuerzos del Bastión V y siluetas de los cuatro edificios funcionales; genera 5 GLB y una fuente editable Blender.
- `.github/workflows/cc0-director-v6.yml`: Ubuntu alojado, sin Windows compartido ni gasto; input **exacto** del artifact V5 run `43874433432`; comprueba glTF/COLOR_0/texturas, 3 render PNG, salto visual I/V y límites de licencias. Artefacto esperado `eldoria-cc0-director-v6-real-3d`.
- Primera ejecución `43877803268`: Blender arrancó e importó V5; **FALLÓ** por uso de materiales borrados en el reinicio de escena `read_factory_settings`. Se corrigió regenerando los materiales tras cada reinicio; [segunda ejecución #43878136136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43878136136) es la referencia para comprobar el estado real. **No declarar PASS sin verificar conclusión y revisar PNG.**

**Prueba de aceptación del trabajo, por orden:** Blender fuente/modelos reales y capturas, inspección artística que demuestre corrección de suelo/vegetación/edificios, agrupación 3D actualizada por materiales/sector, integración Unity aislada mediante reserva Windows legítima y test móvil. El primer PASS es técnico, jamás ART_APPROVED.

**Aislamiento:** esta versión solo edita su rama; las mallas externas de Quaternius NO se incorporan al código. Ni main, ni publicación, ni fuentes del Bastión vetado, ni saves, ni gameplay.

## V6 confirmado; V7 material/arquitectura en comprobación (16/11/2026)

- **V6, probado:** [Actions #43878136136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43878136136) completó SUCCESS. Materializó 5 GLB, un Blender editable y 3 PNG reales. La malla de terreno tiene 11.865 vértices y está exportada con atributo `COLOR_0`; en estadio V la exportación contenía 1.589 mallas y 10 imágenes incrustadas. **Nada de ello demuestra FPS ni calidad premium**.
- **Resultado de inspección visual V6:** ya no hay losas blancas a modo de escalerilla; sin embargo las coníferas se leen muy claras y geométricas, los elementos altos del Bastión parecen añadidos y faltan riqueza de piedra y contraste. El JSON V6 reveló `images_repainted: 0`: la ruta anterior no había modificado ninguna textura física.
- **V7 implementado en `tools/cc0-art-system/director_v7_material_architecture.py`:** corrige esos defectos reemplazando pirámides de tejados por dos aguas conectadas, árboles de ramaje irregular y **texturas originales modificadas realmente**. Su fuente es exclusivamente el GLB V6, de autoría propia.
- **V7 primer run `43880135476` FALLÓ de forma informativa:** Blender reconocía texturas empaquetadas del GLB sin ruta de archivo ni datos de píxeles accesibles, por lo que 0 imágenes cambiaron. No se rebajó el control de aceptación; se reimplementó la técnica leyendo el **BIN bufferView del GLB 2.0**, procesando el PNG verdadero y sustituyendo los nodos de textura por PNG nuevos propios con SHA antes/después. Nuevo run exacto: [#43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054); comprobar su conclusión y logs.
- **Pendientes genuinos:** inspeccionar visualmente V7 A/B, repetir optimización estática sobre los modelos V7 (no asumir resultados de V6), ejecutar integración Unity aislada solo cuando Windows esté correctamente reservado y probar el dispositivo real. **No fusionar ni publicar**.

**Evidencia / principio de honestidad:** Blender render success ≠ unidad artística aceptable; GLB texturas rebake success ≠ iPhone performance. Mantener referencias y defectos al cambiar de conversación.

## Director V8 — reconstrucción material y arquitectónica (16/11/2026)

**Punto de partida verificable:** [Director V7 run 43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054) = 5 GLB de autoría Eldoria + fuente BLEND + 3 capturas reales Blender; sin prueba Unity o móvil. Revisión visual independiente: aspecto aún demasiado plano, árboles geométricos y pálidos, textura de tierra débil y torre superior aislada.

**Nuevo trabajo real en la misma rama:**
- `tools/cc0-art-system/director_v8_atlas_and_citadel.py` importa V7 real del run exacto; reemplaza suelo por altura y textura de 1024×1024 con caminos coloreados en terreno; introduce tres atlas físicos 1024×1024 (piedra, pizarra y madera), retoma materiales compartidos, sustituye coníferas por copas orgánicas con geometría propia, construye nave gótica conectada y tejados de capilla, mejora cuartel/granero/cantera/aserradero y mantiene cinco exportaciones físicas.
- `.github/workflows/cc0-director-v8.yml` ejecuta Blender 4 sobre Ubuntu alojado sin Windows compartido, con input inmóvil [V7 #43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054). Espera tres capturas auténticas 1600×900 (I, V) y 720×1200 (V), cinco GLB, BLEND editable y evidencia técnica en `eldoria-cc0-director-v8-real-3d`.
- **Run inicial V8 [#43882248142](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43882248142):** lanzado; su éxito, calidad de capturas y rendimiento NO deben darse por supuestos mientras no terminen la ejecución y la revisión.

**Límites:** no modifica `main`; no incorpora fuentes privadas de Quaternius; no es conversión de imagen conceptual a 3D; no certifica rendimiento ni calidad artística Unity/iPhone. Siguiente obligación: inspeccionar fotos reales V8, medir fallos geométricos y corregir antes de declarar mejora sustancial.

## Director V8 y V9 — evidencia y corrección causal (16/11/2026)

- **V8 completada y verificada en Blender:** [run #43882248142](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43882248142), artifact `eldoria-cc0-director-v8-real-3d`. Cinco GLB físicos, fuente `.blend`, 3 PNG auténticas; V8 Bastión V exportado con 1.885 meshes, 77 materiales, 11 imágenes. **No Unity, no móvil, no aprobación artística.**
- **V8 juicio visual:** mejora real de caminos/tejados/materiales; quedan coníferas paleadas, entorno oscuro y nave principal poco orgánica. No confundir el render conceptual aprobado por IA con una captura jugable.
- **V9 alternativa bosque mixto:** `tools/cc0-art-system/director_v9_natural_forest_and_integrated_keep.py`, rama idéntica. Primera prueba `43884700954`: Blender generó cinco GLB y PNG; QA de workflow falló por comparar erróneamente su `source_run` con el ID V7 en lugar del V8. Corregido en `.github/workflows/cc0-director-v9.yml`. **Pero la captura real reveló el defecto más grave: nave principal casi negra por geometría V8+V9 duplicada y z-fighting.** Nueva corrección `ba5259f683e20eeebb2ef0a28d3c9997830293e8` elimina mallas de V8 antes de reconstruir V9, rebaja la cota del tejado, añade guard de una única nave para nivel V y cero para I–IV. Run de comprobación [#43886355573](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43886355573) debe completarse e inspeccionarse.
- **V9 alternativa estructural:** código separado `tools/cc0-art-system/director_v9_architectural_lift.py`, salida `DirectorV9Structural`. Su [run #43884858236](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43884858236) pasó Blender, glTF y PNG, pero el arte no alcanzó el estándar: cubiertas y gabletes demasiado blancos/planos, fuerte modularidad. Una revisión independiente de esa línea no implica aprobación para Eldoria.
- **Decisión pendiente:** comparar capturas reales **corregidas** V9 con alternativa estructural y V8. Si no corrigen macrodefectos, pivotar nuevamente la técnica; no sumar decoraciones sobre geometría incompatible.
- **Riesgo de continuidad:** GitHub Actions conserva los binarios solo 30 días; los scripts y reglas permanecen en la rama, pero una cadena de builds anclada a artifacts viejos puede caducar. Debe documentarse/automatizarse un rebuild completo por scripts originales antes de considerar la fuente reproducible a largo plazo. No publicar modelos de terceros sin prueba de licencias.

**Estado honesto:** investigado sí; implementado y renderizado V8 sí; V9 nuevas pruebas y correcciones en curso; aprobado artísticamente NO; Unity NO; iPhone/WebGL NO; `main` intacta.


## Continuación V10 — arquitectura integrada y comparación real (16/11/2026)

**Encargo vigente:** sustituir retoques conceptuales por correcciones de geometría física y capturas nativas. Rama/workstream sin cambios.

- **Base V9 comprobada:** [Actions #43886721399](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43886721399) SUCCESS. Capturas reales I/V en 1600×900 y V en 720×1200 inspeccionadas. Evolución visible, pero nave alta rectangular, tejados planos y suelo simplificado. La alternativa V9 de bosque mixto #43886355573 aparece más clara y rompe paleta; ninguna variante tiene aprobación artística.
- **Causa comprobada en GLB, no supuesto visual:** el nivel V incluye a la vez las mallas nombradas "Valoria imperial restored upper hall", "BASTION V / actually roofed royal nave" y "upper Valoria great hall cast masonry" junto con antiguas cubiertas. La superposición es motivo plausible de las estructuras pegadas.
- **Corrección geométrica V10:** fuente en tools/cc0-art-system/director_v10_integrated_citadel.py. Elimina primero arquitectura alta V8 obsoleta, reconstruye un solo cuerpo de nave con coro superior, tejados inclinados cruzados, transeptos físicamente conectados, contrafuertes, marcos profundos y portón. Etapas I-IV conservadas; la fuente es V8 real fijada a run #43882248142, sin paquete externo ni ilustración IA usada como textura.
- **Prueba nueva:** [Action #43889934794](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43889934794). Consultar conclusión y capturas antes de afirmar que V10 está terminada.
- **Otra variante sin mezclar:** director_v10_physical_materials.py [Action #43889437704](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43889437704). Atlas físicos, una hipótesis independiente; comparar imágenes A/B, no fusionar sin comprobar.
- **Límites:** Unity V10 NO ejecutada, iPhone y Android NO probados, ART NO aprobado, main sin cambios y Windows reservado para otro workstream.

**Siguiente acción autónoma:** verificar ambos runs, descargar PNGs reales y comparar V9/V10, diagnosticar macrocambios y corregir si sigue fallando. Luego optimizar mallas del V10 escogido, y solo con reserva legítima comprobar Unity con capturas reales. Un CI exitoso no equivale a calidad aceptable.


## Pruebas V10 corregidas y V11 de atlas propios (16-11-2026)

- **V10 geometría integrada real:** [Action #43889934794](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43889934794) SUCCESS: cinco GLB y editable .blend, tres renders genuinos Blender; usuario puede comparar Bastión I/V. Se eliminan volúmenes de cubiertas antiguas que coexistían en el GLB V8. **Revisión visual:** mayor unión de alas, pero cuerpo superior sigue demasiado liso/blanco; no ART_APPROVED.
- **V10 materiales primer intento rechazado:** [#43889437704](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43889437704) generó archivos pero falló QA visual con *99,996 %* de píxeles casi negros en región de torre superior. **Causa probada en código:** el ciclo de Blender reinicia escena con read_factory_settings, importa GLB sin luces y recrea una cámara, pero no reconstruía sol/ambiente/antorchas; no era falta de calidad del GLB en sí.
- **V10 iluminación recuperada:** [#43891297913](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43891297913) SUCCESS tras añadir restore_v9_physical_lighting en tools/cc0-art-system/director_v10_physical_materials.py. Restauradas 3 fuentes de luz determinísticas, color y exposición originales. PNG real ahora legible con nuevos atlas de ladrillo, pizarra y roble. **Técnico probado, calidad comercial NO.**
- **V9 batching verificado:** [#43888770748](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43888770748) SUCCESS. Bastión V pasó de 2.442 mallas importadas a 202 mallas agrupadas (reducción 91,73 %), con exactamente 130.643 caras preservadas. No medir como draw calls GPU ni FPS.
- **V11 combinación física:** tools/cc0-art-system/director_v11_textured_citadel.py reusa **materiales realmente texturizados V8_master_ashlar, V8_master_roof, V8_master_timber** sobre los volúmenes reconstruidos de V10 y genera mapeado UV físico para polígonos de cubierta/fachada. No tiene paquetes externos ni imágenes AI como textura. [Prueba #43891952674](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43891952674) se debe verificar y descargar; **no declarar finalizada ni aprobada sin inspeccionar PNG real**.
- Todo lo anterior es exclusivamente **Blender aislado**; NO se ha importado/probado esta línea en Unity/Editor/iPhone. No fusionar en main ni ocupar Windows reservado para art/visual-reboot.

### Prioridad de continuación

1. Verificar V11 exacta, abrir PNG I/V y retrato y comparar V9/V10 a cámara equivalente.
2. Rechazar visualmente si gran nave y árboles siguen siendo simples; buscar solución estructural, no añadir decoraciones arbitrarias.
3. Optimizar para móvil solo la variante visual elegida; reservar Windows según main/active-workstreams antes de Unity. Las pruebas Blender son evidencia de activos, no de un juego jugable.

## Relevo — correcciones V11 y V12 comprobadas (16-11-2026)

- V11: run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43891952674 SUCCESS; Blender real, cinco GLB, .blend y capturas I/V + vertical. 11 imágenes embebidas, 1.887 a 2.370 mallas. Problemas visuales: cuerpo central rígido y suelo muy uniforme. No Unity ni iPhone.
- V12 alternativa paisaje: run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43894052922 SUCCESS, fuente tools/cc0-art-system/director_v12_living_valley.py. Aporta especies de árboles pero mantiene Bastión alto más rectangular. No aprobar automáticamente.
- V12 arquitectura: run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43894465023 SUCCESS, fuente tools/cc0-art-system/director_v12_architecture_terrain.py. Elimina físicamente nave alta anterior y reconstruye volumen y cubierta; exporta cinco GLB y cuatro capturas Blender auténticas con cámaras comparables. Problema detectado por revisión real: discos verdes de suelo demasiado planos.
- Corrección causal de discos: se reemplazaron parches poligonales por plantas submétricas 3D batched en script. Commit 49b6f2f79080ae6574c192f1cc3fa36dd46e90d9; run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43896082396. NO declarar PASS hasta comprobar status y capturas reales.
- V12 estructura sigue con 1.932 a 2.456 mallas sin optimizar; batching antes de Unity móvil. Un glTF y render Blender no certifican FPS, guardados, UX, beta ni iPhone.
- Derecho/alcance: solo fuente geométrica de autoría Eldoria; imágenes conceptuales IA NO forman parte de modelos. Rama aislada, main intacta, no merge ni publicación.
- Próximas acciones: inspeccionar run 43896082396 y defectos visibles, elegir una sola base; luego optimizar y probar importación Unity con propiedad legítima del equipo Windows. No ART_APPROVED mientras no exista evaluación real.

## Cierre de prueba V12 con terreno corregido — 16-11-2026

- **Comprobado en Blender:** [GitHub Action #43896519558](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43896519558) terminó SUCCESS. Fuente en esta rama: `tools/cc0-art-system/director_v12_architecture_terrain.py`, commit de corrección `ccaee08896857cae0139f012468023f1e9c33395`.
- **Salida física verificada:** cinco GLB (niveles I–V, con 1.932/1.942/1.958/2.082/2.456 mallas), un BLEND editable, cuatro PNG reales (I/V horizontal, V detalle cercano, V vertical) y metadatos del pipeline. El workflow verifica origen V11, progresión I/V, sustitución de la antigua nave y la ausencia de licencias externas. Artifact `eldoria-cc0-v12-actual-improved-architecture`, ID `14238504601`, retención 30 días.
- **Revisión visual:** los discos verdes artificiales de terreno de #43894465023 ya no aparecen en la imagen de #43896519558. El nuevo volumen del Bastión es distinto y más conectado; siguen pendientes rigidez en planos principales, riqueza de fachadas/terreno, profundidad atmosférica e identidad artística premium. NO ART_APPROVED.
- **Límite legal:** arte de geometría original de Eldoria; los modelos Quaternius se ensayaron por separado y no se han incorporado a esta salida V12.
- **Límite técnico:** el tamaño del GLB y la prueba Blender no certifican FPS, draw calls, memoria, Unity, WebGL, iPhone, Android ni funcionamiento de gameplay. Antes de presentar una beta: batching/LODs de las 2.456 mallas, conservar GUIDs/puntos de acción y comprobar con captura real de Unity bajo reserva legítima del Windows compartido.
- **Reproducibilidad pendiente:** el script está versionado y la fuente V11 está fijada a #43891952674, pero GitHub Actions conserva artifacts solo 30 días. Debe consolidarse una fuente maestra propia duradera y un rebuild completo antes de depender de ella a largo plazo.
- **Próxima decisión basada en evidencia:** escoger entre la geometría V12 corregida y la variante de paisaje V12 #43894052922 con capturas de la misma cámara. No fusionar automáticamente variantes ni usar ilustraciones IA como supuestos assets.

## V13: reconstrucción física de la fachada y prueba de Unity separada (16/11/2026)

**Propósito:** responder al encargo explícito de realizar correcciones directamente en la geometría del juego. Prohibido sustituir modelado por imágenes conceptuales.

**Trabajo implementado en esta rama, no en main:**
- `tools/cc0-art-system/director_v13_gothic_facade.py`: importa los cinco GLB originales V12 de [Action 43896519558](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43896519558). En nivel V exige eliminar físicamente el coro anterior (mínimo 30 piezas) antes de construir una fachada gótica de piedra, cubierta inclinada con vertientes y coursing, ventanas ojivales, contrafuertes conectados y herrería real de puerta. No inventa materiales desde una imagen conceptual ni introduce vendor meshes.
- El script también diferencia cantera (grúa), aserradero (rueda y madera), granero (provisiones y silo) y cuartel (dianas); añade textura de variación geográfica de 512 × 512 al **suelo existente**, preservando el trazado de caminos y sin planos decorativos flotantes.
- `.github/workflows/cc0-director-v13.yml`: ejecución Linux hospedada [#43902021480](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43902021480) sin ocupar Windows; exige exportar cinco GLB, una fuente BLEND y cuatro capturas **reales de Blender** (I, V, V cerca, V vertical), verificando ausencia de geometría vieja superpuesta y textura física integrada.
- `Unity/Assets/Art_Reboot/CC0System/Editor/CC0V13SceneBuilder.cs`: generador de cinco escenas editor independientes y capturas reales Unity I y V; utiliza `com.unity.cloud.gltfast 6.14.1` instalado en `Unity/Packages/manifest.json`. **Código registrado, todavía no ejecutado/compilado en Unity:** exige copiar los GLB reales del artifact V13 a `Unity/Assets/Art_Reboot/CC0System/DirectorV13/` sin tocar escenas de producción.

**Estado real de V13 al registrar esta línea:** workflow lanzado; no declarar técnica ni estética aprobada hasta conclusión de Action y examen visual de PNG. No hay aún prueba física de iPhone, Android ni WebGL. No se ha modificado `main`.

**Criterios de cierre:** inspeccionar capturas reales V13 y anotar defectos; comprobar que nueva nave supera V12 y que no hay dobles cubiertas; medir mallas y agrupar estáticas; ejecutar SceneBuilder en Unity con permiso/recurso real, comprobar materiales, dirección de cámara y el fallo de reinicio del mundo en la beta de forma independiente. No fusionar ni declarar arte premium por CI verde.

## Actualización V13-V14 — ejecución sobre modelos reales, 16/11/2026

**V13 VERIFICADA TÉCNICAMENTE:** [Action #43902021480](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43902021480) concluyó SUCCESS. Artefacto `eldoria-cc0-v13-authentic-architecture` con GLB I–V, BLEND editable, atlas físico de terreno y cuatro capturas reales de Blender. El proceso elimina 89 piezas de coro anterior antes de reconstruir la zona gótica de Bastión V; el QA comprueba que el modelo exportado contiene nueva geometría y ya no contiene `single coherent top choir stone mass`.

**Crítica artística real V13:** [captura V13 V y acercamiento en artifact del run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43902021480) muestran puerta mejor articulada y nueva fachada con ventanas góticas, pero la escena sigue muy uniforme y oscura, con poca textura en el suelo y poca riqueza visual de ciudad. NO se aprueba premium. **No usar imágenes conceptuales creadas con IA como resultados del proyecto.**

**V14 IMPLEMENTADA Y EN COMPROBACIÓN:**
- `tools/cc0-art-system/director_v14_env_arch_finish.py` importa los cinco GLB V13 físicos y cambia materiales de terreno con una nueva textura propia 768×768, pinta sendas orgánicas directamente en textura físicamente exportable, genera vegetación 3D batched, piedras con geometría irregular y enriquece detalles imperiales, cantera, aserradero, granero y cuartel.
- `.github/workflows/cc0-director-v14.yml` importa el artefacto exacto V13, verifica propiedad de fuentes, genera cinco GLB físicos, BLEND editable y cuatro PNG auténticas con misma cámara. [Run #43904480456](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43904480456). **No declarar pass hasta obtener conclusión y examinar imágenes.**
- `Unity/Assets/Art_Reboot/CC0System/Editor/CC0V13SceneBuilder.cs` ya registrado; se confirmó documentalmente que el proyecto tiene el editor importer `com.unity.cloud.gltfast 6.14.1` y que glTFast crea prefabs importados desde GLB. NO se ha ejecutado Unity ni capturado la escena en el Editor, y el código no demuestra compilación.

**Siguiente fase obligatoria:** abrir y comparar V14 real contra V13, identificar si logra mejora cualitativa y, solo después, llevar la geometría a Unity, optimizar mallas por materiales, capturar realmente en Editor, probar WebGL/iPhone. Nunca tocar `main` ni otras ramas gráficas sin autorización.

## V14 ejecutada / V15 PBR comercial CC0 en prueba (16/11/2026)

- **V14 implementada y probada en Blender:** [Action #43904480456](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43904480456) = SUCCESS. El artifact `eldoria-cc0-v14-owned-physical-3d` contiene 5 GLB originales, fuente BLEND, albedo procedural `V14_Valoria_AuthorOwnedRoadsAndMeadow.png` y 4 PNG reales. **Inspección visual:** caminos y terreno mejor integrados; fachada y volúmenes legibles, pero superficies de piedra/cubiertas demasiado planas y la calidad aún no es premium. No aprobar arte. No se ha ejecutado Unity.
- **V15, investigación aplicada a materiales reales:** fuentes verificadas Poly Haven CC0: `stone_brick_wall_001` y `roof_slates_02` (cada una diffuse, normal GL y roughness JPG 1K). Los seis archivos tienen URL oficial, tamaño, MD5, SHA256 y prueba comercial registrados por `tools/cc0-art-system/audit_polyhaven_cc0.py`. **No hay packs de modelos de terceros en V15.** El sistema no utiliza la API Poly Haven dentro del videojuego; sólo obtiene los archivos gratuitos de proveedor durante compilación con fuentes congeladas.
- **Código V15:** `tools/cc0-art-system/director_v15_real_pbr.py` carga la geometría real V14 y asigna UV físicos por cara, texturas coloreadas previamente mezcladas en PNG, normal maps y roughness a las familias de piedra y pizarra; exporta cinco GLB/BLEND y cuatro captures Blender. Se reemplazó expresamente un intento de shader MixRGB no garantizado por **albedo realmente horneado** para que Unity glTFast pueda importar el efecto físico.
- **Pipeline V15:** `.github/workflows/cc0-director-v15-pbr.yml`, run [#43907312243](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43907312243). Verificar conclusión y examinar PNG antes de atribuir mejora artística. Los modelos y sus fuentes siguen en artifact de GitHub Actions; no fusionar ni alterar main.
- **Unity:** `CC0V13SceneBuilder.cs` y `CC0V14SceneBuilder.cs` registrados para usar glTFast 6.14.1. Son scripts candidatos, **aún sin ejecución real**. No deducir FPS o estabilidad iPhone del éxito en Blender.

**Mandato siguiente:** abrir el render real V15, comprobar continuidad estilizada y ausencia de fototextura pegada; si da peor resultado que V14, no promover. Preparar una prueba aislada en Unity con escena real y verificarla en Editor/iPhone sólo cuando runner disponible con propiedad clara.

## V16 — separación física de cuatro industrias y revisión de identidad (16-11-2026)

**Objetivo aprobado:** distanciar aserradero, cantera, cuartel y granero del Bastión, moviendo sus piezas reales, no una representación 2D; mejorar sus diferencias de silueta y sus accesos. **Nombre canónico del experimento:** Eldoria CC0 Art System — Sistema artístico gratuito reproducible. **Rama:** `experiment/eldoria-cc0-art-system-20261116`. Esta rama no sustituye `main`.

**Autoría implementada:** `tools/cc0-art-system/director_v16_separate_industries.py`, tomando **cinco GLB físicos de V15** del Action [43907312243](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43907312243). Reubica completos y por nombre de origen los cuatro grupos, registra cada objeto trasladado, desplaza aserradero (-22,-7.5)→(-32,-12.5), cantera (-23.5,11.8)→(-33.2,18.4), cuartel (23.8,11.4)→(34.5,18.4), granero (22,-9.2)→(32,-15.5), con incremento de distancia al Bastión >7 m. Conserva sistemas, progresión Bastión I–V y archivos fuente.

**Calidad geométrica real:** nueva textura física de 896² con caminos curvos hasta cada nuevo emplazamiento; limpieza de los antiguos segmentos de carretera y vegetación que obstaculiza las nuevas ubicaciones. Aserradero con rueda y canal de agua modelados, cantera con terrazas y gran puente grúa, cuartel con patio militar, parapetos, torre y dianas, granero con silos independientes, provisiones y cosecha en malla agrupada. El objetivo es evitar cuatro edificios idénticos pegados al núcleo. Sin imágenes conceptuales ni geometría no autorizada de terceros. Los PBR CC0 Poly Haven de V15 permanecen.

**Ejecución QA:** `.github/workflows/cc0-director-v16-spread.yml`, [Action #43911033294](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43911033294). Comprueba cinco GLB, BLEND editable, ocho PNG *verdaderas* (I, V, cercano, vertical y un detalle por industria), traslados de geometría mediante posiciones reales de nodos y un informe de evidencias JSON. **En ejecución al escribir esto; NO declarar éxito sin verificar conclusión y abrir capturas.**

**Pendiente:** análisis visual auténtico y iteración; batching de nueva geometría, importación/captura Unity 6000.3 real, rendimiento WebGL/iPhone, confirmación de correspondencia de accesos/botones. No modificar main ni interferir con Windows compartido. Estar separado y superar CI de Blender NO significa resultado premium ni juego jugable.
