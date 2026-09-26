# Valoria — art test ilustrado 2.5D aislado

Fecha: 2026-09-26. Estado: **prueba técnica ejecutada; rechazada como pipeline de ciudad 4X, útil como concept art/fondo de cámara fija**. Se preserva `VisualWorld.Create`, `Valoria.unity`, el gameplay y la decisión C de `VALORIA_FINAL_ASSET_DECISION.md`. Este experimento investiga una alternativa de coste de licencias 0 € para la cámara elevada fija de Eldoria.

## Resultado real y decisión de producción

Commit de la prueba `c2e1652f6cfd1cd3587a6e8a1cdfead97e16f5f7`; Unity Actions run **36250258935**: source preflight, EditMode, PlayMode, Windows build y render del experimento **verdes**. Artefacto `eldoria-layered-valoria-captures-c2e1652f6cfd1cd3587a6e8a1cdfead97e16f5f7` (ID **10908514472**) contiene las seis capturas `stage-{1,2}-{strategic,city,detail}.png` a 1280×720. Se revisaron visualmente las seis. El artefacto separado de las tres capturas de Valoria confirma que el workflow siguió capturando la escena de producción; el diff del commit no incluye la escena, `VisualWorld` ni gameplay.

| Pregunta | Resultado |
| --- | --- |
| Representación | Dos PNG RGBA de 1536×1024 para el Bastión se alternan como material de un mismo Quad URP Unlit, delante de un PNG RGB sobre otro Quad orientado hacia la cámara. No hay malla 3D del edificio, proyección sobre geometría ni profundidad interna. El Quad del Bastión tiene collider rectangular. |
| Tres zooms | Los tamaños ortográficos 19, 12 y 9 **renderizan sin error**. El estratégico muestra una composición legible pero el Bastión ocupa una parte pequeña de la pantalla; ciudad prioriza la fachada; detalle recorta la base/edificios bajos. La prueba usa una cámara fija semejante a la de ciudad, pero estos tres tamaños no son una validación de todos los desplazamientos/cámaras de gameplay ni una medición de dispositivo móvil. |
| Progresión | Las dos etapas retienen el cuerpo central, torre principal, acueducto y roca; la segunda añade ala y torre a la derecha. Otros detalles cambian ligeramente al regenerar la imagen completa (andamios, huecos, vegetación, iluminación). La identidad general se reconoce en las capturas, pero el cambio de estado no está demostrado como transición limpia y solo hay dos estados de los cuatro exigidos. |
| Ciudad 4X | **No es un pipeline de producción validado.** El test valida una composición ilustrada aislada. No hay edificios independientes, conexiones caminables, selección precisa, orden dinámico de oclusión, cámara desplazable/rotatoria, animación integrada ni variantes de ciudad. La profundidad es pintada y la roca del Bastión se percibe superpuesta al suelo sin unión física ni acceso coherente al camino. |
| Rendimiento | La prueba carga tres imágenes y dibuja dos planos; esto no extrapola a una ciudad poblada. Cada RGBA 1536×1024 ocupa unos 6 MiB sin comprimir; con dos etapas y el fondo RGB son unos 16,5 MiB en RGB/RGBA base antes de mipmaps y formatos de plataforma. Cuatro etapas y múltiples edificios implican más texturas, transparencia/overdraw, atlas y pruebas de memoria y FPS móvil. No se midieron en esta prueba. |

La interacción del test solo alterna la etapa si el raycast toca el **rectángulo** del Quad, incluidas zonas visualmente transparentes; el job verde y las capturas estáticas no prueban clics precisos ni entrada táctil. Los tamaños de zoom muestran composición; no prueban el flujo jugable. **Veredicto: esta vía no resuelve el bloqueo artístico principal de Valoria como ciudad 4X interactiva.** Conservar las imágenes como dirección de arte, matte painting o fondo controlado es razonable; promocionar los planos a la escena principal no lo es.

## Material original

`Unity/Assets/Eldoria/ArtTests/LayeredValoria/` contiene tres PNG originales generados con la herramienta integrada de imágenes de Work (no son modelos 3D, ni assets externos):

- `bastion-stage-1.png`: palacio antiguo habitable parcialmente arruinado sobre roca, fragmento de acueducto, fortificación, andamiaje y taller bajo.
- `bastion-stage-2.png`: edición de la imagen anterior que añade un ala terminada y torre secundaria a la derecha, intentando conservar cámara, base y cuerpo central. Tiene pequeñas diferencias en ventanas, andamios, vegetación y pintura que pueden producir un salto al cambiar de estado. Eso se debe **juzgar** en juego; la continuidad todavía no está aprobada.
- `valley-backdrop.png`: paisaje original sin fortaleza, con camino, valle, bosque, acueducto distante, montañas y una franja contaminada en el horizonte.

Prompts utilizados: la primera imagen pidió un **fragmento original** de palacio y fortaleza de piedra clara en un escarpe, una torre dominante, dos masas secundarias desiguales, tejados de pizarra, arcos, ruina antigua, taller inferior y acentos azules; tomó la referencia del propietario **solo como dirección de calidad, cámara y paleta**. La segunda tomó la primera imagen como objetivo de edición y pidió preservar la estructura izquierda y reconstruir únicamente el ala derecha. El fondo pidió una vista elevada de un valle con un solar vacío en el centro, ruinas lejanas y una amenaza territorial a la derecha, sin edificios centrales ni HUD. En las tres: sin etiquetas, sin texto, sin marcas de agua; las dos imágenes de arquitectura se pidieron con alpha real.

## Ensayo reproducible en Unity

`LayeredValoriaArtTestCapture.Capture` crea una **escena independiente** en Unity 6000.3.23f1/URP, coloca un fondo ilustrado en un plano distante y el Bastión con alfa en un plano más próximo, con la orientación y la posición de la cámara base de `VisualWorld`. Genera **seis capturas reales de Unity** a 1280×720: dos etapas × tres tamaños ortográficos (19, 12, 9). Cambiar la etapa sustituye el material de un mismo elemento y conserva su zona clicable. En el editor, las teclas `1/2` cambian etapa y `Q/W/E` cambian zoom; hay botones y el Bastión se puede pulsar. **No hay build jugable ni promoción a la escena de producción.**

El menú **Eldoria → Art gate → Open isolated layered art test** genera/abre la escena temporal para probarla dentro del editor. El workflow de Windows genera seis capturas; si el runner no llega a procesarlo, **no se declara validado**. La nueva escena se genera desde las imágenes/código de este directorio; no está en la lista de escenas de la build principal.

## Riesgos que deben resolverse antes de aprobar dirección

1. Las ilustraciones tienen **una sola perspectiva**. Las tres distancias de zoom conservan el ángulo, pero la cámara no puede rotar ni rodear el edificio; la profundidad del fondo es pintada. Esto coincide con el alcance del test, **no demuestra** perspectiva libre ni paralaje convincente. Panning y oclusión con edificios vecinos quedan sin probar.
2. Dos imágenes completas del Bastión consumen memoria de textura y crean diferencias pequeñas al conmutar. Para cuatro o más etapas hay que demostrar continuidad, versiones coherentes y presupuestos de memoria/atlas en móvil. Este test solo muestra **dos** estados; cuatro no están probados.
3. La roca del recorte debe integrarse con el solar del fondo, el camino y la vegetación. La ilustración puede parecer un recorte pegado sobre un valle o un diorama; la captura real manda. No resolverlo con una afirmación sobre calidad de una imagen individual.
4. Casa, Cuartel, Granero y una ciudad completa exigirían capas reutilizables, orden de oclusión, interacciones precisas y coste de producción aceptable. La escena aislada **no valida** ninguna de estas cosas, ni rendimiento móvil.
5. Hay que comparar las seis capturas con el [benchmark](ELDORIA_VISUAL_BENCHMARK.md) y con el [checkpoint certificado](VALORIA_ART_ASSET_GATE.md), revisar recorte y continuidad, después probar el ejecutable. Si no se alcanza un salto visual funcional, se rechaza esta vía sin promocionarla a Valoria.

No incorporar este experimento en `VisualWorld` ni actualizar la conclusión de `VALORIA_FINAL_ASSET_DECISION.md` a A hasta superar la prueba visual y de producción correspondiente.
