# Eldoria / Construction Delight v1 — candidata no aprobada artísticamente

## Qué implementa
Actividad visual en los edificios reales de Valoria: polvo fino, virutas y apertura discreta al comenzar una obra; movimiento ligero durante la obra; al finalizar, polvo, motas de oro envejecido, un aro suave y un realce cálido que dura aproximadamente 1,7 segundos.

El componente ValoriaConstructionDelightV1 se incorpora automáticamente al host jugable de la ciudad. Lee el mismo LocalGateway, PlayerState y modelos/colisionadores existentes. No cambia el gameplay, el HUD, el tiempo de las obras, el esquema de guardado, modelos, materiales del edificio ni las escenas. La primera candidata usaba `ParticleSystem`, pero se sustituyó por siete emisores de sprites en mallas dinámicas agrupadas (`MeshRenderer`), con shader URP propio y texturas procedurales pequeñas; gasto cero. Esto evita añadir el módulo `UnityEngine.ParticleSystemModule`, que no está habilitado en el manifiesto canónico, y no modifica los archivos que controla la PR #136.

La lógica ConstructionDelightState reconoce construcción inicial del aserradero y cuartel, las mejoras de Bastión y las obras/mejoras c3 de aserradero, cuartel, granero y cantera. Solo dispara la celebración cuando aumenta el nivel real. Una partida cargada a mitad de obra reanuda la actividad, pero no muestra inicio ni finalización falsos. Desde mundo 4X el efecto se mantiene pendiente durante un breve plazo y no se dibuja sobre el mapa.

## Estrategia de calidad
Inspiración: reparación medieval y renacimiento de Valoria. Colores ocre, polvo, ámbar y oro envejecido; sin flashes blancos, confeti, hologramas gigantes ni sacudidas de cámara. Los efectos se centran en las posiciones auténticas de los edificios; son de baja emisión y se suspenden al entrar al mundo.

No se ha añadido un sonido sintetizado de baja calidad. Un efecto sonoro original de calidad necesitará escucha y revisión independientes.

## Compatibilidad con trabajo activo
Esta candidata se desarrolla en alcance y PR separados de la PR #136: no modifica SlicePresenter, cartelas, integración, escenas, modelos, referencias, Main ni archivos del otro propietario de Valoria. La integración final requerirá reconciliar el estado canónico y la calidad de ambos trabajos.

## Pruebas obligatorias pendientes
- Ejecutar Unity 6000.3.23f1 real y ConstructionDelightStateTests en la revisión exacta.
- Capturar vídeo auténtico A/B en 390x844 y 844x390 de inicio, bucle y fin en aserradero y cuartel; comprobar el brillo y la geometría en la cámara aprobada.
- Verificar mejoras c3, Bastión, carga de obra, caducidad, retorno de mundo y no alteración de guardados.
- Comprobar que los materiales están soportados en URP WebGL y Android, sin quads rosas, overdraw molesto ni partículas ocultas por Gaussian splats.
- Medir rendimiento de la escena y probar toque y gestos sin interferencias.
- Rechazar y reajustar el arte si no resulta claramente bonito en movimiento; compilar no es ART APPROVED.

## Incidencia y continuación comprobadas — 04/11/2026

**Implementado en PR #152, no certificado ni ART_APPROVED.** La lógica de estados, los siete emisores de mallas, el shader URP y el capturador nativo con 26 vistas/franjas de tiempo están en la rama `feature/eldoria-construction-delight-20261104`.

Las dos primeras pruebas nativas (runs `41746271850` y `41746717881`) llegaron a Unity, pero fallaron en compilación porque el proyecto no tenía activado `UnityEngine.ParticleSystemModule`. Se cambió de técnica sin tocar el `manifest.json` compartido.

La prueba `41793699479` arrancó el Editor 6000.3.23f1 en Windows pero salió con **198 / No valid Unity Editor license / com.unity.editor.headless not found**, antes de compilar los emisores nuevos.

La lectura aislada `41796258446` verificó que `UnityEntitlementLicense.xml` estaba presente y que Unity Hub/Unity Licensing Client no estaban iniciados. `41796661851` arrancó el Unity Hub instalado, minimizado y sin introducir credenciales; lanzó el servicio de licencias, pero GitHub finalizó esos procesos al cerrar el job. En `41797026292` se repitió dentro de **un mismo job**, con Hub y Licensing Client activos y un proyecto temporal vacío; el Editor volvió a terminar con **198 por licencia no válida**. No se leyeron secretos ni se reinició el PC.

**Bloqueo humano demostrado:** activar/actualizar la licencia con la cuenta correcta en Unity Hub; para Unity Personal, el inicio de sesión en Hub es el método oficial. No hay bypass legítimo ni prueba visual sustitutiva. El runner Windows se liberó en `main/pipeline/active-workstreams.json` revisión 6 y se conservaron la rama y PR #152 como borrador.

**Continuación técnica exacta una vez restablecida la licencia:** reclamar runner en el mismo ID canónico `eldoria-construction-delight-v1-20261104`, sincronizar registro main/PR, actualizar `tools/construction-delight/native-request.json`, ejecutar `.github/workflows/eldoria-construction-delight-native.yml`, revisar las **26 capturas Unity reales** vertical/horizontal y la secuencia de finalización, corregir defectos artísticos y repetir QA de guardados/móvil. Nunca publicar, mezclar PR o declarar calidad >=8 antes de comprobar imágenes y movimiento. Hasta entonces, **compilación nueva pendiente, A/B real inexistente, arte no validado**.


## Investigación acumulativa y pruebas sin Unity — 04/11/2026, posterior al error 198

Se investigó el riesgo de que GitHub Actions lance Hub en una sesión de servicio (recomendación de GameCI). La prueba de Windows `41798743304` **descartó esa hipótesis para este ejecutor**: PowerShell del runner, `explorer.exe`, Unity Hub y Unity Licensing Client estaban todos en **SessionId 2**. No usar arreglos de Session 0 para este caso.

El ejecutable oficial Unity CLI sí está instalado. El chequeo read-only `41799120407` produjo `auth status` **exit 3** y `license status` **exit 4**, sin cuenta activa ni licencia disponible para CLI. Unity-Technologies documenta estos códigos. Un segundo chequeo `41799334303` devolvió `hub_session_adoptable=False`: no había una autenticación existente de Hub que la CLI pudiera adoptar automáticamente. No se leyeron tokens ni se cambió de cuenta. No confundir un XML de licencia presente con una sesión autenticada actual. No solicitar reactivación ciega sin explicar esta evidencia. La última ejecución nativa verdadera, `41793699479`, continúa bloqueada por exit 198 antes de compilar la nueva versión.

**Progreso verificable independiente de la licencia:** la ejecución Ubuntu `41799938783` compiló en .NET el archivo original `ConstructionDelightState.cs` con una simulación estructural mínima de `PlayerState` e hizo 34 comprobaciones, **PASS**. La ejecución conjunta `41800430492` repitió los 34 assertions y analizó la sintaxis C# con Roslyn de los tres archivos principales (lógica, renderizador y captura), **0 errores de sintaxis, PASS**. Esta última prueba usa el SDK .NET 10 para coincidir con la versión de las bibliotecas Roslyn instaladas, evitando los conflictos de versión detectados en una primera candidata. No equivale a compilar los métodos de Unity ni a representar píxeles.

**Prevención de bucles:** el workflow nativo incorpora un preflight de licencia contra un proyecto temporal de Unity antes de descargar el repositorio completo. Ante exit 198 debe fallar explícitamente, sin reinstalar, reiniciar, tocar credenciales, aceptar nuevas condiciones ni repetir builds pesados. El diagnóstico de SessionId no debe volverse a ejecutar sin indicios nuevos.

**Único bloqueo externo actual:** abrir Unity Hub en el escritorio autenticado y restablecer una licencia válida mediante el flujo oficial de Unity. No usar los métodos de activación CLI Pro/Enterprise para Unity Personal; Unity exige una sesión válida y consentimiento a las condiciones. Este paso no lo puede superar el agente legítimamente con las herramientas y autenticación disponibles.

Reanudación exacta: revisar `main/pipeline/active-workstreams.json`, reclamar el mismo ID `eldoria-construction-delight-v1-20261104` para `windows-runner-heavy` mediante CAS solo cuando realmente esté libre; sincronizar snapshot al PR, actualizar `tools/construction-delight/native-request.json`, ejecutar la captura nativa y examinar las 26 imágenes A/B y fotogramas secuenciales antes de decidir si el diseño es bonito. El rendimiento Android/WebGL y la versión instalada siguen sin validar. **No ART_APPROVED, no merge, no APK.**


## QA técnico adicional — 04/11/2026 (verificado, sin aprobación artística)

- La PR #152 contenía un snapshot antiguo del propietario de Valoria (ownership_state=available, rev. 129), aunque main ya indicaba propietario exclusivo de PR #136, rev. 130. Se sincronizó el registro main mediante SHA/CAS, sin editar ni desplazar al propietario. La comprobación *Valoria visual authority routing* pasó en el run [41803412696](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41803412696).
- La validación Ubuntu [41804060440](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41804060440) ejecutó **36 assertions PASS** contra ConstructionDelightState.cs enlazado directamente y Roslyn identificó **0 errores de sintaxis** en los tres archivos C# del módulo/capturador. Esto *no* significa compilar Unity, ejecutar shaders, validar WebGL/Android ni acreditar acabado bonito.
- Una celebración requiere ahora **dos pruebas del mismo trabajo auténtico**: el nivel aumentó y CompletedTaskIds del LocalGateway contiene el ID anterior. Se añadieron tests de falsos positivos (nivel elevado sin comprobante o con ID diferente), además de obras iniciales, c3, Bastión y aceleraciones. No hay nuevo estado persistido.
- El renderer borra **todos** los sprites al salir de la ciudad, incluso con otra obra activa: los destellos finales no pueden filtrarse al mundo 4X. El bucle se reanuda desde el estado real al regresar.
- La captura nativa ahora usa RenderPipeline.StandardRequest y SubmitRenderRequest oficiales de URP, en lugar de confiar en Camera.Render; verifica el aumento de nivel y el comprobante de obra, y mide píxeles modificados tanto en toda la imagen como en la región del Aserradero auténtico. Sigue necesitando **ejecución real**.
- Diagnóstico Windows actualizado de solo lectura [41804158624](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41804158624): ejecutor y explorer.exe en SessionId 2; archivo XML de licencia presente; Hub, Licensing Client y Editor ausentes durante esa prueba; Unity CLI auth status exit 3 y license status exit 4. La sesión del ejecutor no era Session 0. No se accedió a tokens ni hubo cambios en procesos, credenciales o reinicios. Un diagnóstico **no convierte esos códigos de CLI en prueba de que la cuenta Unity del propietario haya caducado**.
- Se corrigió asimismo el falso resultado CI: el script de PowerShell conservaba LASTEXITCODE=4 del subproceso CLI aunque el diagnóstico de solo lectura había terminado. Ahora devuelve éxito del diagnóstico, preservando los códigos observados.

**Estado contrastado:** implementación y pruebas de dominio/sintaxis actualizadas; **ninguna captura nativa de estos efectos**, ninguna compilación de esta revisión en Unity por el bloqueo del Editor (198), ningún A/B aprobado, ningún merge. La sesión de Unity puede recuperarse de forma legítima al autenticarse en Hub; no usar métodos de activación Pro/Enterprise en Unity Personal ni interferir con el trabajo paralelo de PR #136.


## Descubrimientos de integración real en Unity — 04/11/2026

- Ejecución [41809767387](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41809767387): Unity 6000.3.23f1 compiló la versión con siete emisores de malla y abrió PlayMode. La prueba se detuvo por un nombre antiguo de la raíz de sprites; el nombre se corrigió sin modificar la escena.
- Ejecución [41811262373](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41811262373): Unity compiló y renderizó cuatro capturas reales (city-overview y city-clean, vertical/horizontal). Se inspeccionaron: Aserradero no encuadrado correctamente en vista cercana y ActiveConstructionSprites era 0; por tanto NO es un A/B ni arte aprobado. Artefacto fuente [13313566823](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41811262373/artifacts/13313566823).
- Hallazgo técnico: ValoriaIntegratedWorld.Focus no mueve una parcela urbana totalmente fuera de cámara; ValoriaReferenceCleanGameplay.Focus puede centrar el punto auténtico. QA llama primero a City.Focus y después al encuadre integrado. Los WorldHotspot de la escena integrada pueden no ser hijos del objeto de gameplay. El anclaje usa ahora integrated.FindTarget(id), el Collider del objetivo y, si la obra inicial tiene oculto el edificio futuro, los renderizadores auténticos del andamio o terreno. Nunca se inventan coordenadas.
- La orden de prueba Gateway.Execute no pasaba por el botón que refresca Presenter. Ahora QA llama explícitamente a city.Presenter.RefreshState() y Hud.Refresh() para activar el auténtico andamio. El renderer expone ActiveBuildingId de solo lectura para diagnosticar falta de emplazamiento frente a emisión.
- Verificación posterior [41813983423](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41813983423): 36 assertions PASS, 0 errores sintácticos Roslyn en los tres archivos de producción/prueba. NO sustituye Unity runtime.
- Reserva Windows liberada correctamente después de la prueba anterior; ahora el workstream eldoria-unity-metadata-integrity-20261104 controla windows-runner-heavy para su recuperación prioritaria. No interferir, no editar archivos del otro chat ni disparar Unity hasta su liberación. PR152 sigue aislada.

**Cierre pendiente:** reclamar Windows mediante CAS tras su liberación, sincronizar registro principal, lanzar el workflow nativo y revisar las 26 imágenes reales A/B y seis fotogramas de finalización por orientación; corregir hasta lograr calidad artística. Hasta entonces sin capturas de los efectos nuevos, ART_APPROVED=false, sin merge y sin publicación.


## Control preventivo de geometría — 4/11/2026

La revisión del código aislado detectó un fallo introducido durante la corrección del anclaje: `VisibleBounds(GameObject,out Bounds)` llamaba recursivamente a sí misma sin condición de salida. Podía desbordar la pila si el emplazamiento no tenía collider utilizable y requería consultar los renderizadores del andamio o suelo.

- **Corregido en commit `b3fec12e1bb7cf14cefca15de2f342b7d91203ab`**: agrega los `Renderer.bounds` reales de los objetos visibles y devuelve false cuando no hay geometría.
- **Prevención añadida en commit `fb8abdd62fb1d1a19aa29a0593b6954072492c14`**: Roslyn exige que `VisibleBounds` no se invoque a sí misma y que inspeccione/agregue renderizadores auténticos.
- **Prueba offline [41848714887](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41848714887)**: 36 assertions de dominio correctas; cero errores de sintaxis en tres archivos C#; guardia `CONSTRUCTION_DELIGHT_BOUNDS_GUARD_PASS`.
- El ejecutor Windows permanece reservado por `eldoria-unity-metadata-integrity-20261104`. No se lanzó Unity para verificar esta corrección. Cuando se libere, reclamar el recurso mediante el registro CAS y generar los A/B auténticos antes de valorar calidad, fusionar o publicar.

**Estado:** código corregido y prueba estática verificada; implementación todavía pendiente de ejecución Unity nativa y revisión artística.


## Evidencias nativas de la pasada de cubierta y fachada — 04/11/2026

**Prueba real pasada:** [41872572157](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41872572157), commit exacto `b1b17576a50b24ca12b31f74080bb29e632008c2`, Unity 6000.3.23f1 / URP. `NATIVE_PASS`; archivo [13345725500](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41872572157/artifacts/13345725500), **26 capturas auténticas** de escena integrada (844×390, 390×844); comprobantes de tarea construida y guardado inalterado tras recarga. Siete capas de materiales de edificio con resplandor temporal no invasivo. Nunca se fusionó con main.

Evolución A/B comprobada:
- Primera pasada sin pulso arquitectónico [41868962109](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41868962109): diferencia de finalización 0,447% paisaje y 2,069% retrato. Oro concentrado en el suelo; **rechazo artístico**.
- Segunda pasada con luz cálida mediante MaterialPropertyBlock [41871246928](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41871246928): finalización 1,391% paisaje y 6,451% retrato, 7 superficies reales; mejor, pero tejado aún oscuro. **Rechazo del objetivo 8,2**.
- Tercera pasada con glints y gleam sobre cubierta/cornisa [41872572157](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41872572157): finalización 1,601% paisaje y 7,448% retrato. Realzado de materiales sin alterar texturas compartidas, glints en 4 ubicaciones de cubierta; éxito técnico y guardado correcto. No se puede deducir belleza ni calidad de un porcentaje de píxeles.

**Calidad visual:** candidato entregado para evaluación humana, **ART_APPROVED=false**. Efectos más integrados, pero no acreditados >=8,2/10; algunas tomas siguen pareciendo sobrias. No se añadió sonido de finalización aprobado ni se ejecutó QA de rendimiento móvil físico/WebGL de esta revisión. Evitar prometer 8,2 sin opinión del usuario sobre la captura auténtica.

**Próxima decisión:** el usuario revisa A/B y secuencia. Si pide más realce, priorizar mejoras de iluminación/arquitectura con comparación real, no agregar humo/confeti. Ningún cambio en gameplay, save, timers, modelos, escenas, PR #136 o main.

El registro `pipeline/active-workstreams.json` refleja fuente y evidencia exacta y reserva temporal de Windows debe liberarse tras el workflow. Mantener PR #152 DRAFT hasta la aprobación explícita.


## Rediseño artístico estructural — 05/11/2026

El propietario rechazó expresamente la calidad del A/B V3. No añadir más confeti, humo o aros sobre la hierba. Diagnóstico causal: la escena solo recibía motas suspendidas y un tinte general a _BaseColor; no existía una progresión de luz que recorriera la arquitectura misma. Los porcentajes de píxeles cambiados no miden belleza.

### Cambios realmente implementados en la rama PR #152 (sin certificación Unity de esta nueva revisión)

1. Nuevo ConstructionDelightArchitecturalReveal.cs con renderizadores temporales sobre MeshFilter.sharedMesh auténticos del andamio durante obra y del edificio terminado al completar. No hay reemplazo de modelos, clon de materiales originales ni cambios en escena/guardado. Clímax 1,65 s, avance vertical de 0,03 a 0,87 s, disipación elegante.
2. Shader URP Resources/ConstructionDelightArchitecturalReveal.shader con una línea luminosa en maderas/piedras, normal de superficie y acento en bordes; ZTest LEqual, sin escritura de profundidad ni sombras. Brillo de cimentación a cubierta SOBRE los triángulos originales, no un halo superpuesto.
3. Una única luz cálida transitoria, sin sombras, sobre la fachada real. Bloom URP global solo si cámara ya habilita postprocesado y su capa lo permite. No se cambia configuración de cámara compartida. Se deshabilita todo al pasar a Mundo4X y destruye al acabar.
4. En obra, polvo existente como apoyo y luz intermitente sobre el andamio auténtico: solo 35% de cada ciclo de 3 s para reducir coste GPU en temporizadores largos. Sin progreso falso.
5. Se corrigió posible fuga de GameObjects: los nuevos MeshRenderer hijos se registran y desactivan/destruyen en Clear(), también en cambios de mundo/Gateway y OnDestroy().
6. A/B suprime emisores y materiales, mallas, luz y Bloom transitorio en A; los restaura en B. Prueba de 34 PNG (26 anteriores + 8 de movimiento durante la obra), comprobante de tarea y save idéntico tras recarga. Verifica inexistencia de overlays tras recarga.
7. SyntaxProbe.cs comprueba cuatro archivos C# y exige shader con GUID. Sin alterar Unity/Packages/manifest.json, donde NO está habilitado com.unity.modules.particlesystem, ajeno al alcance de escritura. Se conservan partículas batched de apoyo. VFX Graph no se adopta como base por sus requisitos compute/SSBO y compatibilidad móvil/WebGL.

### Investigado / implementado / probado / pendiente

- INVESTIGADO: APIs oficiales Unity/URP de shaders, Volumes, Bloom y restricciones móviles, contraste con 26 capturas verdaderas V3.
- IMPLEMENTADO: shader, componente de revelado estructural, conexión con LocalGateway y captura nativa de 34 imágenes; cambios aislados en PR #152.
- PROBADO SOLO FUENTE: [Run 41936083331](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41936083331): 36 assertions PASS, cuatro archivos con sintaxis Roslyn PASS, recurso shader presente. Ejecutado ANTES de la última optimización/timing: repetir a HEAD.
- PENDIENTE: compilación Unity de esta revisión, ejecución de HDR shader/light/Bloom en escena, 34 capturas A/B auténticas, inspección artística, draw calls/tiempo en móvil, Android/WebGL, cuartel/Bastión/c3. NO hay evidencia >=8,2/10 y el propietario no ha aprobado arte. PR DRAFT, sin fusión/publicación.

### Reanudar de forma segura

Consultar main/pipeline/active-workstreams.json. Reclamar windows-runner-heavy con CAS cuando titular PR #136 lo libere, sincronizar snapshot al PR, activar tools/construction-delight/native-request.json con [delight-native], ejecutar Unity 6000.3.23f1 con escena real y evaluar todos los fotogramas. Liberar runner al concluir. No confundir verde CI con calidad; no alterar main, Packages/manifest.json, escenas ni modelos canónicos.


## Animación estructural por etapas y ampliación C3 — 05/11/2026

Además del shader, el candidato incorpora ConstructionDelightStageAssembly.cs: duplicados visuales temporales de mallas 3D auténticas, con las mismas geometrías y materiales existentes, sin clonar colisionadores, scripts ni estados. Durante obras nuevas, las piezas se van colocando en orden de altura conforme al temporizador real, y en los 1,68 s posteriores a la finalización se apoyan sobre su posición definitiva. Los renderizadores originales se restauran incluso al abandonar la ciudad/recargar; nunca se alteran archivos de modelos/escenas. No equivale todavía a arte aprobado.

Se verificó directamente en main/Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs que las duraciones C3 empleadas por WorkDuration coinciden con las reglas auténticas de Granero, Cantera y mejoras (9 / 12 / 8+2*n segundos), por lo que el escalonado visual no se basa en una cuenta arbitraria. Para los modelos C3 se añadieron rutas de selección desde ValoriaParcelPresentation.Bindings usando BuildingId, Built, Construction y Ground, que son referencias reales de la escena, sin coordenadas simuladas.

QA A/B: ya registra y exige presencia de piezas 3D durante obra y finalización, shader de barrido sobre mallas visibles, comprobantes LocalGateway, guardado inalterado al recargar y desaparición de duplicados temporales. Se requieren 34 capturas auténticas en horizontal/vertical, incluidas cuatro etapas de movimiento de obra y seis de terminación por orientación.

**Última prueba offline de esta arquitectura:** [41937883348](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41937883348): 36 tests de dominio PASS, cinco archivos C# sin errores de sintaxis Roslyn, shader presente, guardia contra recursión PASS. Las correcciones finales de emplazamiento C3 se registraron después; repetir esta prueba en el HEAD final. Ninguna de estas comprobaciones equivale a Unity compilado ni calidad visual.

**Pendiente obligatorio:** Unity Editor real exacto, 34 capturas A/B, revisión de movimiento de piezas y su integración con el brillo, rendimiento Android/WebGL y aprobación artística. Windows runner-heavy continúa reservado al cierre post-merge de PR #136 en main, y esta rama no lo reclama ni inicia Unity mientras esté ocupado. Preservar PR #152 DRAFT; no fusionar/publicar.


## V4 — construcción física del edificio real (05/11/2026)

**Diagnóstico artístico:** las tomas auténticas V3 (run 41872572157) muestran brillo y polvo discretos, pero el modelo completo aparece instantáneamente al acabar. Eso no ofrece una verdadera construcción ni recompensa satisfactoria; la V3 no estaba aprobada.

**V4 implementada como candidata en PR #152:**
- Nuevo ConstructionDelightStageAssembly.cs: clona hasta 28 componentes de malla auténticos del edificio final reutilizando EXACTAMENTE sus mallas, materiales, sombras, lightmaps y capas. No clona scripts ni colliders.
- Durante una primera obra real, revela los componentes desde cimientos y vigas hacia el techo. La duración y el progreso proceden exclusivamente de Clock y el deadline persistido de LocalGateway, no de una segunda economía o un segundo temporizador.
- Al completar una tarea con su comprobante auténtico, monta físicamente los componentes del edificio por orden durante 1,68 segundos. Temporalmente oculta solo sus renderizadores originales, restaurando el estado anterior incluso ante recarga, salida a mundo 4X y destrucción de escena.
- Las partículas artesanales, barrido URP y breve luz de fachada quedan subordinados a ese montaje. Polvo y destellos finales se sincronizan con el aterrizaje de las vigas y la cubierta.
- La captura A/B restaura el edificio auténtico durante «A» y activa las copias en movimiento durante «B», sin alteraciones de guardado ni gameplay.
- La prueba estructural Roslyn exige copia de malla/material original, ausencia de mutaciones del Gateway y restauración de Renderer.enabled.

**Verificado:** run 41938038416 y pasada posterior de fuente: 36 assertions de dominio, cinco archivos C# sin errores sintácticos, guarda de montaje original PASS. Se reconcilió la rama con el main actual mediante commit de fusión de dos padres, sin modificar producción ni sobrescribir PR #136.

**Pendiente:** compilar la V4 en Unity 6000.3.23f1 cuando el titular de windows-runner-heavy libere su ejecución; evaluar los 34 fotogramas A/B reales en retrato y paisaje y seis momentos de finalización; comprobar contacto, profundidad con splats, popping, rendimiento móvil, WebGL y recarga. Corregir si la animación de piezas se ve artificial. Nada de ART_APPROVED ni merge/publicación hasta evidencia y aceptación.


## V4: ensamblaje tridimensional sin reinicio visual — 05/11/2026

**Decisión artística:** el propietario rechazó la calidad del polvo, las partículas doradas y los pulsos de cubierta V3. La V4 candidata muestra el montaje de piezas de la **geometría auténtica** del Aserradero durante el temporizador, seguido por la colocación de cubierta y resplandor URP localizado tras la confirmación del trabajo real. V3 queda conservado en los commits y en sus A/B previos. No se declara aprobado el nuevo montaje.

**Regresión crítica detectada y corregida antes de la certificación:** la primera versión de ConstructionDelightStageAssembly.Complete eliminaba las piezas ya colocadas y recreaba todo desde cero cuando LocalGateway confirmaba el edificio. Eso produciría desapariciones y saltos de geometría. Ahora el runtime preserva el montaje mediante StopWork(preserveStage) exclusivamente si ConstructionDelightState.JustCompleted verifica el comprobante real; la transición conserva las posiciones y escalas vivas de cada copia y anima las piezas faltantes hasta sus posiciones finales. Si no existía preview, usa un fallback temporal. Todos los Renderer.enabled originales quedan restaurados al terminar, recargar o abandonar la ciudad.

**Fidelidad material:** cada copia reutiliza MeshFilter.sharedMesh, Renderer.sharedMaterials y los MaterialPropertyBlock específicos que existan. ConstructionDelightArchitecturalReveal y el pulso de color registran incluso partes de cubierta todavía ocultas, pero solo emiten sobre ellas cuando esa pieza se vuelve visible. No se clonan colliders, scripts de juego ni guardados.

**Pruebas realizadas:** [42013182372](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42013182372) — 36 assertions de dominio y cinco archivos C# sin errores de sintaxis; guardias del handoff de tarea y continuidad de obra superadas. **No equivale a compilar V4 en Unity.** El primer intento [42012756978](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42012756978) falló porque la antigua aserción exigía mantener un corte visual artificial de 70%; se sustituyó por una verificación más estricta de poses conservadas.

**Gates pendientes:** compilar Unity 6000.3.23f1, generar A/B auténticos y secuencias temporales vertical/horizontal, verificar ausencia de saltos y pérdida de tejado, persistencia al recargar, inspeccionar materialidad y rendimiento móvil. El HEAD con la mejora final de cubierta/materiales requiere nuevo test. ART_APPROVED=false; sin merge ni APK. PR #152 mantiene intacta la versión jugable de main.


## Experimento controlado A/B de técnica visual — 05/11/2026

El usuario pidió comparar dos técnicas sin suposiciones sobre cuál queda mejor:

- **A — Arquitectura**: efectos atmosféricos discretos, shader URP de barrido vertical sobre geometría auténtica, MaterialPropertyBlock que calienta piedra/madera, luz local y Bloom corto; **sin colocar mallas progresivamente**.
- **B — Montaje 3D**: idénticas partículas, shader, pulso de materiales, luz y Bloom, más el ensamblaje progresivo de las mallas auténticas durante el plazo real y al completar.

**Misma escena, cámara, resolución, tarea LocalGateway, progreso, semilla y momento exacto**. Se implementó `SetTechniqueForCapture(bool)` exclusivamente en Editor. Durante construcción A oculta solamente los duplicados de malla, conservando el barrido sobre el andamio. Al finalizar A restaura temporalmente las mallas originales y muestra una segunda instancia del mismo shader/Bloom iniciada en el mismo instante de completado; B devuelve el montaje de copias al mismo estado. El comparador restaura siempre B al terminar cada par. Sin duplicar partidas ni fingir capturas.

La evidencia nativa requerida pasa de **40 a 64 PNG**: conserva las 40 capturas de V4 (incluida persistencia, primeras poses y secuencias) y añade 24 capturas comparativas de técnica A/B en vertical y horizontal durante obra, finalización y dos momentos adicionales de cada movimiento. Ambas utilizan exactamente el mismo método URP `RenderPipeline.StandardRequest`. El QA debe registrar diferencias en píxeles auténticos, presencia de ambas instancias de Bloom y continuidad de las mallas, y rechazar falta de muestras o materialización.

**Alcance y seguridad:** la segunda instancia de realce arquitectónico solo se instancia en `city.Probe` y se excluye de las builds con `#if UNITY_EDITOR`. Al construir la referencia A se restauran temporalmente los `Renderer.enabled` reales; un `finally` garantiza volver al estado original. `StageAssembly` nunca modifica lógica, colisionadores, objetos persistidos ni materiales compartidos. Todas las capturas son estáticas del mismo marco de Unity y sin posprocesado externo.

**Verificación hasta ahora:** prueba offline [42032899847](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42032899847) con 36 comprobaciones de dominio y cinco archivos C# sin errores de sintaxis; guardia especial del A/B satisfactoria. Los cambios posteriores de restauración y prueba de disponibilidad Bloom necesitan nueva pasada de fuentes.

**Pendiente e imprescindible:** ejecución nativa de Unity 6000.3.23f1, obtención de los 64 PNG, revisión real de cada secuencia temporal, inspección de defectos/popping/materialidad, cálculo de coste en Android/WebGL y decisión artística. **No existe todavía un A/B auténtico de estas dos técnicas y no hay ART_APPROVED ni merge/publicación.**

**Recurso Windows:** el workstream de combate T1-T5 `eldoria-v41-combate-buzon-ui-20261030` es su propietario en `main/pipeline/active-workstreams.json`. No reclamar ni iniciar Unity hasta su liberación formal; mantener esta PR como DRAFT y no modificar PR136/V41.

## 64 capturas Unity auténticas y A/B para revisión — 05/11/2026

**Entrega verificable:** run [42077629878](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42077629878) sobre commit `c6b23953231d2675bdb397b01b4873f880da4aea`, Unity **6000.3.23f1**. Artefacto Actions **13438897479**, 64 archivos PNG realmente renderizados con `RenderPipeline.StandardRequest`, de los cuales 24 forman pares A/B de técnica en 844×390 y 390×844. El `result.json` declara `NATIVE_PASS`, tarea auténtica `LocalGateway.Execute(Build)` + `TestClock / LocalGateway.Advance`, ID de tarea completada, recarga con estado idéntico y eliminación de mallas temporales. Ambos perfiles de Bloom estaban listos. **No** equivale a prueba Android/WebGL ni a ART_APPROVED.

**Defecto detectado durante la revisión real (y corregido, no ocultado):** la primera corrida [42075455491](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42075455491) tenía 64 PNG y CI verde, pero figuras magenta enormes. El log registró `Shader error in 'Eldoria/ConstructionDelight/ArchitecturalReveal': syntax error: unexpected token 'h'` en línea 80; `half alpha=0h;` era un literal HLSL inválido. Corregido a `0.0h` (commit `d53aedff`). Se añadió al capturador un gate de píxeles rosa intenso de error Unity (commit `db798a44`) para impedir futuros falsos PASS. El run corregido registró **0 píxeles de material magenta** en los 12 pares de técnica; prueba de fuente [42077227281](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42077227281) PASS.

**Observación artística, sin aprobación:** A presenta una culminación luminosa muy fuerte sobre la cubierta, con aspecto momentáneamente metálico/blanquecino; B ofrece piezas físicas auténticas durante el montaje y mantiene continuidad de mallas (6 disponibles; 1 visible durante obra, 4 al empezar terminación), pero la diferencia entre técnicas se diluye al acabar. En el run, B vs A afecta 1.371/6.467 píxeles durante obra y 18.353/71.961 al completar (horizontal/vertical), decreciendo a cero en `finish-motion-3` cuando se terminan las mallas temporales. Las imágenes ya permiten comparar, pero **la calidad final y el ritmo de montaje aún requieren la decisión artística del propietario**; no se ha observado en Android físico, ni certificado FPS móvil. No merge ni publicación. Tras la prueba se liberó Windows mediante CAS rev20 y se sincronizó el registro en la rama.


## V5 — martillos reales y polvo de obra localizado (05/11/2026)

**Objetivo artístico confirmado por el propietario:** la obra debe parecer artesanal, con **martillos visibles y polvo denso**, no únicamente polvo dorado tenue ni halos decorativos. Se conserva el montaje 3D V4 como base y el barrido cálido/Bloom breve únicamente en la finalización.

**Código implementado (sin alterar partidas ni otras ramas):**

- Nuevo `ConstructionDelightHammerWork.cs` con **dos martillos de madera y hierro forjado**, mallas cubo low-poly con normales y material URP/Lit. Sin colliders, física, personajes artificiales, prefabs, scripts de juego ni coste de herramientas. Se orientan hacia la cámara para resultar legibles y golpean dos puntos sobre los límites reales del andamio. Cada cabeza de hierro queda alineada geométricamente con su punto de contacto al terminar el movimiento. Ciclos de martilleo asíncronos (1,46 y 1,79 s) con subida, descenso, impacto y recuperación, sin un segundo temporizador de economía.
- Dos campos de sprites ya batched adicionales: `denseWorkDust` (115 quads reutilizados) y `hammerImpactDust` (90 quads reutilizados), con textura radial suave existente. `EmitAtWorld` conserva el punto de impacto en coordenadas locales estables, de manera que una nube anterior **no salta** cuando golpea el segundo martillo. Las ráfagas densas se generan tras impactos de hierro y al sentarse auténticas piezas del edificio, con disipación corta. Leves astillas con el campo de motas anterior.
- En `ValoriaConstructionDelightV1.cs`: inicialización solo durante una tarea real en la ciudad; reconstrucción visual al regresar de 4X, apagado al finalizar, destruir escena o recargar. No altera `LocalGateway`, `GameCommand`, estado de jugador, materiales compartidos ni modelos originales. Mantiene la A/B arquitectónica frente a montaje 3D y las **64 capturas** certificadas del protocolo previo.
- QA nativa ampliada: comprueba que los **cuatro renderizadores del conjunto de martillos** aparezcan en la obra, que haya nube de polvo con al menos doce sprites, que se detecte mínimo un impacto durante los fotogramas temporales, y que martillos y polvo denso desaparezcan al finalizar y después de recargar. La captura A sin efectos incluye también ocultar los martillos; la A/B entre técnicas conserva martillos/polvo idénticos en ambas variantes.
- `SyntaxProbe.cs` incluye el sexto archivo C#, verifica las normales y las mallas físicas, la posición auténtica del golpe, la ausencia de mutaciones y la reversibilidad, y documenta expresamente que sintaxis y pruebas estructurales no equivalen a compilar o evaluar imágenes dentro de Unity.

**Pruebas y pendientes:** se necesitan nueva ejecución de Roslyn/domain y posterior Unity Editor 6000.3.23f1 con artefacto de 64 PNG, inspección de legibilidad de martillos a distancia real, densidad de polvo sin ocultar el edificio, relevancia artística del remate y medida Android/WebGL. El ejecutor Windows se usa mediante reserva exclusiva y se respeta la ejecución nativa de `main` que pudiera estar activa. **Sin ART_APPROVED ni merge** hasta verificar el resultado visual con el usuario.

**V5 — captura temporal auténtica de martilleo**: la QA se amplió a **72 capturas Unity**: 64 originales (24 A/B controladas, escenas completas, movimiento de obra y finalización) y ocho nuevas tomadas en dos impactos reales de martillo (fotograma de contacto y polvo expandido 0,12 s después, ambos en horizontal y vertical). Un contador del runtime inicia la fotografía, no un fotograma sintético ni una animación reconstruida. Se exige exactamente dos impactos temporizados y polvo visible tras ellos. La comparación A/B de técnica conserva exactamente la misma cámara, particulado, shader/Bloom y datos del Gateway; los martillos/polvo son comunes a ambas variantes. `native-request.json` y el workflow exigen 72 PNG para una certificación válida. Ninguna captura nueva se considera existente hasta completar esta prueba nativa.


## V6 — prueba nativa del martilleo y polvo de obra (05/11/2026)

El usuario pidió que los martillos se distingan y que el polvo de obra sea denso. Primero se implementaron dos martillos low-poly de madera/hierro con cabeza animada y dos campos batched (V5), después se verificaron **72 capturas reales de Unity** en [run 42121001211](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42121001211), SHA `ffe34e43fb0fa7b38bed494ea438a01db8d43e76`. PASS: Unity 6000.3.23f1, 4 renderizadores de martillos, 6 impactos reales, 58 sprites de polvo visibles en el punto de muestreo, tarea LocalGateway y guardado intactos, 24 imágenes de técnica A/B. **La inspección de los PNG reales rechazó la calidad:** los martillos parecían demasiado pequeños y oscuros; el polvo se veía como manchas pequeñas casi transparentes en la cámara habitual. No se aprobó el arte.

**V6** corrigió esos problemas con un nuevo tamaño físico de martillo (`clamp(0.19 × ancho de obra, 0.52, 1.05)` unidades), contraste del hierro y madera, y una textura procedural de nube de 64×64 con bordes suaves distorsionados por Perlin (se genera una sola vez). Los campos reutilizables de obra ahora contienen 150 y 110 quads, con impactos más poblados, mayor radio y colas ligeramente más largas, manteniendo la emisión localizada, presupuesto controlado y salida limpia al terminar; sin modificar scripts de gameplay, el reloj ni el estado guardado. La prueba offline [42122260481](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42122260481) PASS: 36 asserts y seis fuentes C# sin errores sintácticos; guardia de golpe/ensamblaje PASS.

**Unity V6 verificado**: [run 42122481739](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42122481739), SHA certificado `96bd7b02a612b2e924411617edbae3287bfe1fd8`, [artefacto 13458423128](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42122481739/artifacts/13458423128): **72 PNG reales, 24 A/B de técnica y 8 capturas temporizadas de martillos**, `NATIVE_PASS`, 6 contactos de martillo, no pérdida de guardado al recargar, sin herramientas/polvo restante tras completar, mallas originales restauradas. El antiguo V5 y el nuevo V6 se compararon en la misma cámara de Valoria: en `hammer-dust-1-landscape`, un 12,97% de los píxeles de la región local de referencia cambiaron más de 12 niveles RGB; la nueva nube y herramienta se distinguen mejor. Esa medida **no es una puntuación artística**. La revisión visual de las capturas confirma mejoras de lectura, pero detecta aún aspecto de nube redondeada y geometría de martillos básica, sin trabajadores ni gesto humano. No declarar calidad ≥8,2/10 sin aprobación; no fusionar ni publicar.

**Gobernanza:** runner Windows liberado en rev22 del registro principal y la rama; PR #152 queda DRAFT. Sigue pendiente comprobar Android/WebGL físico, FPS/draw calls reales y aprobación visual del usuario. Las pruebas estáticas o el éxito de Unity **no justifican** por sí solos sustituir la versión jugable.
