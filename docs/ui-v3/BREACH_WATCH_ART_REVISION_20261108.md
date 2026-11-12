# Vigilia de la Brecha — revisión artística de pantalla nativa

**Estado: IMPLEMENTADO EN PR173, NO CERTIFICADO VISUALMENTE EN EL EDITOR CON EL NUEVO ARTE.** No fusionar ni publicar la nueva interfaz hasta superar el gate artístico y de comportamiento.

## Comparación con primera prueba genuina

- Capturas antiguas (base nativa independiente y plana): Unity Windows run **42427015492**, commit **1cea977b8ec67495bbbcbc7b31d38425e352c10c**, ocho PNG reales, artefacto **13596191419**. Resultado **TECH_PASS, ART_REJECTED**: materiales geométricos planos, iconos esquemáticos, sin miniatura real, composición contextual móvil pobre. El éxito de esta versión no es válido para cambios posteriores.
- La UI de producción sigue intacta y funcional; ninguna modificación de gameplay, ciudad, cámara, guardado o publicación en main por el arte nuevo.

## Segunda estrategia, por qué

Los ciclos de paneles procedurales / overlays previos no llegaron a la calidad escogida. Se abandona la solución plana, no se reescribe otra vez el HUD ni se importa una imagen completa como pantalla jugable. Los componentes del **Canvas independiente BreachWatchHud** se mantienen. Se incorporan materiales más trabajados y una miniatura del modelo REAL, y se reproyecta el panel en cada formato.

## Implementado en la rama PR173, pendiente de captura nueva

1. **BreachWatchMaterialLibrary**: textura original de basalto labrado, hierro ennegrecido y cobre envejecido, generado una vez como sprites con nueve cortes. Reserva de seguridad si falta un PNG externo; la prueba nativa no acepta el fallback para las piezas críticas.
2. **Arte independiente CC0**: cuatro sprites extraídos del PSD original de AnnoMotion Dark Fantasy UI/HUD Kit, fuente y licencia incorporadas en `Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt`. Fuente https://annomotion.studio/public/packs/dark-fantasy-ui-hud-kit/download; SHA256 del ZIP `f089903c2a515c1737344772c538e2bc2d8a153e219096d9118f74db5bf47d76`. La licencia distribuida afirma CC0 1.0. Medallón, marco de hierro, bandera borgoña y CTA de bronce no proceden de RoyalHUD/Forja.
3. **Símbolos independientes en relieve**: mallas de varias capas, con sombra de grabado y metal, más reconocibles que las primitivas líneas iniciales.
4. **Miniatura real del Bastión**: render 512x256 de las auténticas mallas/materiales de Valoria con cámara temporal independiente; ni plano ilustrado ficticio ni reutilización del preview de RoyalGold. Conserva proporciones al insertarse en marcos de distintos tamaños.
5. **Contexto responsive**: pantalla apaisada pequeña usa una distribución de dos columnas para retrato/textos y reserva una fila física exclusiva para CTA, sin coste ni estado debajo del botón. Se ajustó vertical a panel con ilustración y textos independientes, y anchuras de objetivo por formato.
6. **Interacción**: los botones del nuevo Canvas bloquean los clics dirigidos al mundo a través de un adaptador reversible de puntero; restauración al destruir, sin sustituir la lógica de selección ni de guardado. Estado hover/pressed/disabled real de Unity.
7. **Pruebas de fuente y arte**: workflows Ubuntu `source-syntax` con Roslyn y `approved-art-integrity` con hash de cuatro PNG/importador .meta/licencia; no acreditan render de Unity ni aprobación artística.
8. **Gate nativo ampliado**: exige ocho capturas reales del mismo head, ambas escenas (ciudad/Bastión) en 1600x900,900x1600,844x390,390x844; presencia y sprite del arte nuevo; modelo 3D renderizado; texto legible; nav en una fila y sin solapamientos; coste/estado/CTA sin superposición; pointer raycast y bloqueo al mundo; juego/guardado inalterados. Una prueba no ejecutada no debe figurar como PASS.

## Bloqueo coordinado en el momento de redacción

El recurso `windows-runner-heavy` de Unity Windows está actualmente reclamado en main por el trabajo `eldoria-private-beta-i-v-20261107`. El trabajo BreachWatch **no** puede lanzar Unity, cancelar la beta ni adjudicarse el recurso aunque no haya jobs ejecutándose en el instante consultado. Todos los cambios de esta segunda estrategia se han hecho de forma aislada. 

**Siguiente acción segura:** releer registro de main y GitHub Actions. Tras la liberación formal de Windows, reservarlo exclusivamente en el trabajo canónico `eldoria-forja-oscura-visual-proof-20261107`, lanzar el flujo exact-head modificando solo `tools/breach-watch/native-request.txt`, descargar PNG/report, inspeccionarlos al 100%, corregir cualquier falta de calidad/funcionalidad y repetir. El gate final es decisión visual independiente del TECH_PASS. Android/WebGL no están probados; no fusionar ni publicar.



## Revisión ejecutada el 8 de noviembre (continuación hasta gate nativo)

**Implementado después del prototipo HTML, NO CERTIFICADO EN UNITY AÚN:**

- Corrección semántica de los cinco medallones aprobados: REINO/castillo, RECURSOS/provisiones, EJÉRCITO/espadas (inactivo), EXPLORAR/cuervo (gated), HÉROES/casco (inactivo). Antes Construir usaba falsamente una bolsa de provisiones y Crónicas usaba un casco; se han eliminado rutas engañosas.
- Navegación ciudad/mundo idempotente con las APIs reales `TrySetWorld(bool)`: REINO y RECURSOS regresan a la ciudad antes de seleccionar Bastión o Aserradero; EXPLORAR no alterna accidentalmente a ciudad al repetir toque.
- Clase original `BreachWatchAtmosphere` integrada detrás de los controles propios del nuevo Canvas; gradiente de borde muy discreto, no intercepta raycasts, no altera cámara/materiales del mundo. Su mesh usa winding correcto.
- Cartela de marca `VIGILIA DE LA BRECHA / LAS CENIZAS DE VALORIA` adaptativa en zona superior libre SOLO horizontal; se oculta en móvil vertical sin robar la zona jugable. No procede de ningún HUD legado.
- `BreachWatchBuildingPortrait`: cámara URP Base explícita y autónoma; sin postprocesado ni XR, máscara exclusiva para mallas auténticas de edificio con luz aislada. Se reduce riesgo de miniatura vacía por Camera.AddComponent sin datos URP.
- Script de certificación exige medallones con nombres/acciones correctos, destinos sin pantallas nuevas deshabilitados, cabecera según orientación, malla atmosférica sin capturar toques, Bastión real y PNGs en cuatro resoluciones.
- Ejecución del flujo GitHub Actions **42476791045**: integridad de arte = PASS, sintaxis Roslyn = PASS, owner-preflight = PASS. El job Unity = SKIPPED **por reserva viva exclusiva del workstream beta**, no es TECH_PASS ni ART_APPROVED de esta revisión.
- PR 173 sigue sin merge. El único resultado realmente nativo antiguo válido continúa en **42427015492** (otra versión anterior artísticamente rechazada). Las maquetas HTML posteriores nunca se consideran capturas de Unity.

**Bloqueo concreto vivo:** `pipeline/active-workstreams.json` (main, revisado al terminar) sigue asignando `windows-runner-heavy` en exclusivo a `eldoria-private-beta-i-v-20261107`, revisión 8. La cola vacía no es liberación formal. Se solicitó coordinación en PR179 sin invadir su trabajo. No se permite ejecutar Unity en ese runner, forzar un cambio de dueño ni declarar calidad visual aprobada por Roslyn.

**Continuación autónoma segura preparada:** existe una comprobación condicional que reevalúa formalmente la liberación del runner. Cuando su titular lo entregue, se podrá reclamar con CAS y ejecutar el workflow nativo de PR173, inspeccionar los PNG originales, corregir y repetir hasta producir una evidencia real evaluable; no cambiar main, no integrar HUD, no publicar. Las herramientas alternativas Ubuntu fueron evaluadas: Unity 6 Personal exige activación mediante Unity Hub en una máquina nueva; no hay licencia/datos de activación verificados para un nuevo runner. No sustituir una prueba Unity por capturas inventadas, HTML, ni la aprobación de una imagen conceptual.

**Gate pendiente imprescindible:** ocho PNG originales con SHA correspondientes al HEAD de la PR, informe nativo PASS, y evaluación visual humana del contraste, detalle del arte, lectura móvil, consistencia con la referencia y calidad global antes de pedir decisión al dueño.


## Revisión de calidad compacta — 8 de noviembre, posterior a la referencia aprobada

**Código implementado, capturas nativas de esta versión pendientes.** La imagen generada por IA es una referencia de dirección, no una prueba Unity. El diseño independiente contiene:

- Sin estandarte rojo grande ni segundo título arriba; identidad compacta, cinco medallones independientes y ciudad despejada.
- Costes procedentes del gateway real: madera y piedra ilustradas cuando existen ambas, sin ocultar la comida si una operación requiere tres recursos.
- Componentes propios BreachWatchFrameEngraving, BreachWatchNavFiligree, BreachWatchBevelTrim y BreachWatchAtmosphere, con sus metadatos Unity estables. Ninguno usa controles del HUD visual anterior.
- Retrato 3D del modelo auténtico y cámara URP aislada; botón de acción, botón de cierre y accesos con área táctil mínima equivalente a 44 px en las cuatro resoluciones.
- Tipografía Cormorant Garamond Regular/SemiBold embebida y verificada por SHA, licencia SIL OFL 1.1. Arte de iconos licenciado CC0 con manifiestos de origen, hash, sprite y GUID.
- Cuatro pruebas geométricas de código fuente (NO píxeles opacos de una captura): 1600×900 17,8%; 900×1600 29,1%; 844×390 29,3%; 390×844 33,1%. Comparación y metodología: BREACH_WATCH_HUD_OCCUPANCY_BENCHMARK_20261108.md.
- Probada alternativa gratuita GitHub Ubuntu: no existen los tres datos necesarios de activación de Unity como secretos; salió VIGILIA_ALTERNATE_HOSTED_UNITY_NOT_CONFIGURED. No se inventan licencias.

**Todavía no demostrado:** que la nueva revisión compile realmente en Unity 6000.3.23f1 ni que los materiales, UI y retrato logren la calidad visual premium o el rendimiento de teléfono. El resultado antiguo del run 42427015492 pertenece a un HEAD previo y NO certifica estos cambios. El job native se omite por la reserva exclusiva de windows-runner-heavy de la beta privada; un workflow con éxito de Roslyn no acredita ART_APPROVED.

**Cierre exigido:** esperar liberación formal por el titular de beta y reclamar mediante CAS antes de ejecutar; obtener 8 PNG nativos para el HEAD exacto, revisar calidad, legibilidad y clics reales, corregir y repetir; mantener el juego publicado intacto y sin merge hasta la aceptación visual del propietario.


## Revisión de continuidad — 8 de noviembre, último HEAD de PR173

**Dirección que el propietario ha seleccionado definitivamente:** imagen vertical 864×1536 facilitada en el chat, ciudad real siempre visible, escudo dorado con león, Brecha flotante, objetivo compacto de la primera etapa, tres cápsulas de recursos, ficha derecha más estrecha y cinco medallones. El arte debe parecer hecho de basalto/metal trabajado con iluminación cálida, pero TODO el contenido y los clics siguen siendo datos y controles nativos de Unity; prohibido pegar la captura compuesta como una textura de pantalla.

### Problemas concretos detectados en las capturas Unity anteriores

- La foto aislada del Bastión exhibía un cuadrado verde de terreno debajo y un edificio demasiado pequeño. La cámara original del juego no se altera; la cámara de miniatura exclusiva de Vigilia usa un recorte aproximadamente 8% más cercano y un tapete de tierra con textura en repetición que sobresale del encuadre. La prueba GPU ahora rechaza también un castillo cortado por los márgenes.
- El rectángulo de misión empezaba demasiado abajo y ocultaba parte del tejado en retrato. Se ha adelantado hacia la zona superior: la superficie compacta mide 182×80 unidades lógicas antes de escala, y su número 0/1 queda por encima del bisel inferior. Se muestra «BASTIÓN I / LA BRECHA» únicamente para el capítulo I real, sin falsificar el estado.
- El panel contextual ocupaba una anchura mayor que la referencia: ahora **178×307** unidades lógicas antes de escala en retrato; mantiene el Bastión real, el coste auténtico y el botón táctil ≥44 px. El espacio mayor no debe comprarse sacrificando textos o acciones.
- El encabezado viejo tenía un castillo en vez del **león** del propietario, y un bloque rojo grande. La nueva identidad utiliza únicamente el león pequeño y cinco medallones originales separados; el banner y la segunda cartela se han suprimido.
- Las tres cápsulas superiores dibujaban bordes dobles y una X. Ahora se utiliza solo el original `PremiumSurfaces/slate_resource`, rediseñado con bordes 14/11/14/11 apropiados para una cápsula de 38 unidades, sin bordes apilados.
- El CTA rojo mostraba un X genérico y texto que invadía el dibujo. Ahora utiliza dos martillos de forja con cabeza grabada, dejando el texto real a su derecha en un RectTransform separado, con ajuste al ancho del botón y sin mentir sobre la disponibilidad de la mejora.
- El botón de mejora bloqueada no daba explicación. Ahora tocarlo muestra **`RoyalAction.Reason` real** en una discreta cartela temporal de pizarra y no puede gastar recursos ni ejecutar comandos; el texto desaparece solo o al cambiar de edificio. Hay una nueva prueba de puntero real para esto, además de reino, cerrar, recursos y abrir/cerrar misión.
- En móviles Retina (1170×2532), el antiguo límite del factor de escala 1,75 hacía los controles demasiado pequeños. El nuevo factor proporcional a 514px de anchura mantiene la referencia de 900px, con suelo 0,91 para que 390px conserve zonas táctiles y máximo 3,0 para no sobredimensionar tabletas. Se añadió un quinto formato de prueba geométrica, sin afirmar que equivalga a una compilación iPhone.

### Validación y siguiente iteración

Las pruebas Ubuntu **`source-syntax` (Roslyn), `approved-art-integrity` (SHA, licencia, metadatos), `validate_compact_layout.py` y auditoría de propiedad** han seguido pasando después de las correcciones. Las capturas anteriores en Unity son reales y verificadas, pero corresponden a un SHA **anterior** a estos ajustes: siguen siendo evidencia de defectos antiguos, NO la aceptación de la nueva versión. Los cálculos geométricos de pantalla son estimaciones, no una nota artística.

En el último registro canónico consultado, el ejecutor `windows-runner-heavy` continúa bajo PR179 `eldoria-private-beta-i-v-20261107` revisión 13, por una comprobación nativa de paridad y estabilidad iPhone. No ocupar, interrumpir o quitar esa reserva. En cuanto su propietario libere formalmente Windows, reclamar mediante CAS para `eldoria-forja-oscura-visual-proof-20261107`, ejecutar `.github/workflows/breach-watch-native.yml` al SHA exacto de PR173, inspeccionar las ocho capturas en las cuatro orientaciones, comparar visualmente con la referencia 864×1536, corregir y repetir hasta lograr el acabado premium. **No fusionar ni publicar, ni declarar ART_APPROVED hasta esa validación real.**


## Revisión final de legibilidad de tipografía real (8 noviembre 2026)

**Investigado:** en las capturas anteriores el texto del botón rojo y la descripción del Bastión se reducían demasiado o se solapaban con la decoración. Los archivos de tipografía embebidos en la rama son Cormorant Garamond (SIL OFL) y Libre Baskerville. No puede inferirse su ancho usando Arial o capturas conceptuales.

**Implementado:** el texto breve de la ficha y del botón utiliza Cormorant embebida; el botón reserva solamente la anchura que ocupan los martillos grabados. Se mantiene `RoyalAction` real, sin cambiar el comando ni el estado de disponibilidad. Se añadió `tools/breach-watch/verify_premium_font_fit.py`, que mide textos en el archivo TTF real y comprueba la ficha 178×307, el encabezado de misión 182×80, el motivo en dos líneas y la acción «MEJORAR A II». No añade mockups ni materiales al HUD antiguo.

**Probado:** ejecución de fuente `source-syntax` de GitHub Actions [42511527667](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42511527667): Roslyn, geometría responsive, cinco tamaños (incluido 1170×2532) y tipografía con Pillow superaron sus comprobaciones. `approved-art-integrity` también pasó.

**Pendiente de verificar:** la **calidad final real** y los clics/sombras/renderizado de esta revisión en Unity 6000.3.23f1; el job `native` sigue `skipped` por exclusiva del Windows de PR179. Los checks de Ubuntu no constituyen aprobación visual y no autorizan merge/publicación. El run nativo previo [42501713450](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42501713450) se refiere a un SHA anterior y sus defectos no representan fielmente este nuevo resultado. Antes de entregar al propietario, capturar 8 PNG nativos del HEAD vigente, comparar contra la imagen aprobada (móvil 390×844 y 900×1600 de especial prioridad) y corregir si no se alcanza la calidad.


## Revisión de composición real contra la referencia aprobada — 8 noviembre, tarde

**Problema observado en evidencia, no supuesto:** las capturas genuinas anteriores de Unity para 390×844 y 900×1600 mostraban el Bastión ocupando aproximadamente 65–70 % de la anchura del móvil, el puente cortado y muy poco paisaje. El modelo en la imagen vertical seleccionada por el propietario ocupa aproximadamente un tercio. El defecto principal no era solo el HUD, **también era el encuadre de la cámara nativa**.

**Implementado en PR173, sin tocar main:** clase independiente `BreachWatchCityComposition` con zoom ortográfico de entrada adaptado al aspecto del móvil y encuadre del modelo REAL hacia un punto próximo al tercio superior. Una única aplicación al abrir/reorientar la ciudad, reentrada diferenciada tras cambiar desde Mundo, y restitución del encuadre anterior al desmontar la pantalla. No se altera `homeZoom`, archivos de guardado, cámara regional, terreno, foco ni datos de juego. En pantallas apaisadas se conserva la composición previa. El sistema deja libres los gestos de jugador después del encuadre inicial, aunque **el primer ajuste de zoom manual desde el nuevo punto de partida necesitará un test nativo específico** por el límite de zoom del controlador canónico.

**Implementado artísticamente:** la cartela rectangular de objetivo se sustituyó por un sprite original `chiseled_quest` en una estructura completamente independiente, con silueta oblicua, cantos trabajados, latón tenue y centro libre para objetivo/progreso REALES. Ensanchada solo lo necesario para mantener en una línea `BASTIÓN I / LA BRECHA` usando el archivo Cormorant verdadero (medición aprox. 145 píxeles a 13 unidades). Los cinco medallones se dibujan al 80 % dentro de áreas de toque inalteradas: así la navegación deja respirar la ciudad sin reducir su accesibilidad. La capa de selección se centra en la escala visual, y cuenta, misión y ficha reciben sombras de contacto discretas.

**Pruebas implementadas y verificación:** fuente Python `verify_city_framing.py` explicita hipótesis de magnificación y ausencia de cambios de recursos/controles del juego. El Editor nativo `BreachWatchNativeEvidence` comprobará realmente ancho y centro del Bastión en viewport, el zoom resultante, ocho PNG auténticos, texto completo en tarjeta oblicua, escalas táctiles y las tres sombras independientes. Esto **no sustituye** la revisión visual de esas imágenes. La fuente y los sprites generados pasaron los gates Ubuntu en runs anteriores; las modificaciones posteriores requieren un nuevo gate exact-head antes de ejecutarse en Unity.

**Pendiente genuino y condición de continuidad:** `windows-runner-heavy` sigue registrado en main como exclusivo de la beta PR179 (revisión 13 en la última lectura). No debe reclamarse antes de que su titular lo libere. Cuando esté libre, adquirir con CAS, renderizar y estudiar calidad al tamaño real, corregir lo que falle y devolver runner. No declarar `ART_APPROVED` por los tests de código, ni merge ni publicación sin decisión del propietario.
