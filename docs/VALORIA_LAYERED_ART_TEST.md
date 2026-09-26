# Valoria — art test ilustrado 2.5D aislado

Fecha: 2026-09-26. Estado: **prueba experimental pendiente de aceptar visualmente en capturas reales de Unity**. Se preserva `VisualWorld.Create`, `Valoria.unity`, el gameplay y la decisión C de `VALORIA_FINAL_ASSET_DECISION.md`. Este experimento investiga una alternativa de coste de licencias 0 € para la cámara elevada fija de Eldoria, sin afirmar que sustituya el arte 3D final.

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
