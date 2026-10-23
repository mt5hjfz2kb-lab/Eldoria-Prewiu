# Región 1: investigación, diagnóstico y ensayo continental

Fecha: 23-10-2026. Autor: ChatGPT. PR #100, mismo workstream. Sin Qwen, compras, merge ni publicación. Este documento distingue estudio, hipótesis y resultados; no concede aprobación artística.

## Lectura técnica efectuada y aplicación

1. [World Machine: Layout Generator](https://help.world-machine.com/topic/layout-generator/). Leídas las secciones formas, Bézier, falloff, operaciones positivas/negativas, fractal breakup, generación/modificación e importación/exportación. El diseño vectorial establece primero masas y recorridos; el ruido solo rompe bordes subordinados. En Eldoria se conservan curvas en X/Z, máscaras semánticas separadas y un campo de alturas; no se incorpora World Machine. La escala y el cambio de origen vertical al importar SVG deben ser explícitos. SVG no conserva todos los atributos específicos ni alturas por vértice: no se usa como sustituto de un modelo de datos completo. Aplicación del ensayo: costa continental autoral, cauces y grupos forestales; no segmentación de la ilustración por colores.

2. [Unity Learn: Terrain Editor](https://learn.unity.com/course/introduction-to-terrain-editor/tutorial/working-with-the-terrain-editor-1). Leídas escultura, altura objetivo, suavizado, stamps, pintura por capas y vegetación; el curso corresponde a 2019.4, no se supone equivalencia de interfaz con Unity 6. [Terrain tools Unity 6](https://docs.unity.com/en-us/engine/6000.0/manual/creating-environments/script-terrain/terrain-tools), [heightmaps](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Heightmaps.html) y [Terrain Layers](https://docs.unity3d.com/6000.0/Documentation/Manual/class-TerrainLayer.html) consultados. Se trasladan conceptos de altura, transición y pesos a nuestras mallas y shader. No se requiere TerrainData ni cambiar colisiones. Las máscaras son datos lineales; el pigmento es color. El ensayo amplía el dominio y añade hombros bajos fuera del corredor jugable, manteniendo su cota y la Brecha.

3. [Blender 4.5: Distribuir puntos en caras](https://docs.blender.org/manual/es/4.5/modeling/geometry_nodes/point/distribute_points_on_faces.html). Apertura directa falló; el buscador recuperó el contenido técnico indexado completo de entradas, propiedades y salidas de la versión 4.5. Se estudiaron densidad por área, selección, separación mínima Poisson, semilla, normales e IDs estables. No se afirma haber ejecutado Geometry Nodes. Aplicación: masas diseñadas y exclusiones de agua/caminos, conteo según área, posiciones deterministas; el muestreo de la base inspeccionada era espiral. La reconstrucción limpia usa candidatos pseudoaleatorios de semilla fija, aceptación por máscara y distancia mínima, sin afirmar equivalencia con el algoritmo Poisson de Geometry Nodes. Un futuro exportador GN puede consumir los mismos pesos sin convertirse en otra autoridad geográfica.

4. [Felix Leyendecker: Environment Design, Modularity, Color & Composition](https://80.lv/articles/environment-design-modularity-color-composition). Leída entrevista del artista, incluidas proporciones del concepto frente a 3D, composición en vistas elegidas, zonas tranquilas/detalladas y luz. La regla 70/30 es guía compositiva, no cuota automática. Aplicación: resolver primero el espacio observado desde cámara; evitar llenar vacíos con prefabs o tratar cada arboleda como un sello aislado.

5. [Unity: Road to Real-time Look Development](https://unity.com/resources/road-to-real-time-look-development). Accesible la descripción; NO se obtuvo vídeo/transcripción, por tanto no se declara sesión completa estudiada. Complemento técnico: [iluminación de shader URP](https://docs.unity3d.com/cn/6000.0/Manual/urp/use-built-in-shader-methods-lighting.html), que describe Lighting.hlsl, GetMainLight y Lambert. La luz de pendiente del shader actual no es iluminación completa URP ni demuestra sombras de contacto. Acabado queda después de validar masas; no se vende una normal calculada como relieve visible si el shader no la utiliza.

6. [GDC: The Art of Making Vistas](https://www.gdcvault.com/play/1028957/Art-Direction-Summit-The-Art). Sitio redirige a mantenimiento. Recuperada sinopsis oficial; localizada [publicación de Santa Monica Studio](https://sms.playstation.com/stories/gdc-2023-presentations/media/999) con enlace anunciado a PDF, pero página solo devuelve shell JavaScript y no se recuperó el PDF. NO se afirma haber visto la conferencia. Sustituto técnico accesible: entrevista anterior y [Sheila Alessandrini: Lost Mausoleum](https://80.lv/articles/creating-stylized-mausoleum-from-modular-parts), leída sección blockout, overlay y retorno a Blender. Enseña fijar encuadre temprano y contrastar proporciones antes de acabado. En Eldoria overlay solo como herramienta analítica, nunca como fondo del juego ni evidencia Unity.

7. [Camera.orthographicSize Unity 6](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-orthographicSize.html) y [LODGroup Unity 6](https://docs.unity3d.com/6000.0/Documentation/Manual/class-LODGroup.html). Leídas dimensiones del frustum y transiciones según tamaño en pantalla. En ortográfica, altura visible=2s y anchura=2s·aspect; a yaw cero la extensión terrestre Z depende además de la inclinación. No estirar X/Z al girar el móvil. Un esquema híbrido usa la misma geografía y cambia representación; los crossfades pueden duplicar renderizado temporalmente. Rendimiento exige perfil físico; conteo de polígonos no equivale a FPS.

## Diagnóstico específico, por impacto

Base inspeccionada: a3f5f4c1d4439944003131f0773bb7892773e27e; código certificado 038c9f513a415c4dab282f632302a50c8d930241. Artefacto 12468311395 de Unity 39874832548 descargado; inspección real de PNG horizontal/vertical, documentación de ensayos y fuente C#.

1. **Topología visual equivocada.** Shore usa un polígono cerrado de unos 386 m de ancho y transforma todo exterior en océano. La cámara horizontal muestra unos 868 m: sobra mar en ambos lados. El problema no es falta de árboles. El canon escrito requiere mar limitado a un borde; la imagen panorámica deja continuidad occidental bajo niebla. El nuevo ensayo propone continuidad terrestre oeste/norte y costa oriental/sur, manteniendo anclas.
2. **Confusión entre proporción del lienzo y planta geográfica.** El PNG incluye encabezado/pie; la ilustración tiene perspectiva propia. Copiar su relación de aspecto a X/Z, y después comprimir Z mediante ComposeZ, no resuelve la proyección de la cámara. Conservar cámaras del control permite separar mejora de terreno de mero reencuadre. El encuadre final completo de ambas orientaciones sigue siendo una validación pendiente, no se presume solucionado por ampliar tierra.
3. **El interior es casi un plano.** Height devuelve -4 en casi toda tierra, con tres elevaciones de solo cuatro metros. Además el shader aplica luz principalmente al litoral. Más resolución por sí sola no produce lectura volumétrica. El ensayo añade hombros bajos autorales; acabado/iluminación necesitan posterior contraste.
4. **Bosques como islas de detalle.** Grupos pequeños con conteo casi fijo, pese a tamaños distintos, y grandes zonas sin relaciones. Nuevos lóbulos occidentales forman agrupaciones; densidad proporcional al área, exclusiones compartidas y huecos conservados. No se cambian IDs jugables.
5. **Agua y rutas demasiado diagramáticas.** Ancho fluvial casi constante; falta jerarquía y transición de ribera. No se confunde su corrección futura con el objetivo inicial de masas. Nuevas conexiones deben comprobarse en la misma proyección y no volver a mezclar dos geografías.
6. **Acumulación de capas.** P0 coloreó agua sobre bosque, duplicó recorridos y borró la Brecha; el rechazo está documentado. Atlas comprimido distorsionó contenido; camuflaje procedural y microtintes no resolvieron composición. Se reutiliza la separación semántica, NO aquellos resultados.

## Decisión autoral previa al ensayo

Modificar solo World4XCanon, pruebas focalizadas y evidencia. Ampliar dominio de máscaras/malla a 1820×1560 existente en arquitectura; diseñar costa y continuidad oeste/norte; conservar territorio central, origen, nodos y controles; añadir catchment occidental y grupos de bosque. Prohibido alterar originales. Bruma solo panorámica y periferia, no una mancha interior regional.

Primer ensayo es **BLOCKOUT COMPOSITIVO**, no 9/10. Comparar nuevos PNG con los PNG de 39874832548 mediante identidad de cámara, sin equiparar concept y Unity píxel a píxel. Los controles internos «antes-composición» siguen siendo el control histórico; no renombrarlos como el inmediato anterior. Registrar cada comparación con sus dos SHA.

## Criterios de revisión razonada

Composición 30%, geografía/agua 20%, bosques 15%, relieve 10%, estilo/materialidad 15%, encuadre/continuidad 10%. Cada criterio 0–10 con explicación visual. No es métrica automática. Valoración provisional de la panorámica base: ~4/10; coincide con defectos observados, no certifica las otras vistas. Objetivo 9–10 no alcanzado ni garantizado. Propietario conserva aprobación final.

## Estado de ejecución

Investigación y diseño registrados antes del push artístico. Ensayo preparado para Unity; resultados y decisiones se añadirán tras descargar sus PNG auténticos. Sin resultado visual nuevo todavía. Ningún PASS previo certifica este cambio.

## Revisión del ensayo y orden posterior: folio en blanco

El ensayo 745f252c766648048f084bee8dc1d399cb48dd0c se ejecutó en Unity [39914683678](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39914683678), artefacto 12484397421. Sus siete pruebas pasaron. ZIP descargado, CRC correcto; las seis cámaras coinciden exactamente con 39874832548 en resolución, pose, objetivo, aspecto y zoom. La revisión visual detectó tierra dominante, pero cabeceras vacías, ríos uniformes con desembocaduras interiores, escasa lectura de relieve y bosques repetitivos. No es el resultado solicitado ni se acepta como final. La compresión ComposeZ aplicada a las nuevas rutas prueba que conservar el montaje estaba perpetuando decisiones equivocadas.

El propietario ordenó expresamente reconstruir la presentación sobre un folio en blanco, manteniendo lo funcional. La implementación siguiente reemplaza los constructores activos de RegionOneCanonicalLandscape y RegionOneDirectedGeography. No crea primero la geografía antigua para cubrirla: hay una sola malla continua, una máscara semántica y una representación por árbol. La escena integrada conserva sus objetos funcionales y colliders; los renderers importados de fondo se desactivan mediante el adaptador de integración, sin borrar objetos de gameplay. Es la frontera necesaria con la escena compartida, no un segundo paisaje construido bajo el nuevo.

Decisiones previas a la nueva captura:

- Costa oriental/sur y continuidad occidental/norte definidas en metros finales. Se elimina ComposeZ por completo del camino activo.
- Red fluvial nueva con tronco, cabeceras estrechas, confluencias y salidas al mar; lagos conectados. Las rutas usan las mismas coordenadas.
- Hombros bajos independientes, datum central protegido, normales usadas por la luz principal URP en el suelo. El shader anterior calculaba iluminación que apenas aplicaba a la tierra.
- Bosques con lóbulos autorales, rechazo determinista por densidad, separación y exclusiones; no se afirma implementar el algoritmo Poisson de Blender. Se reutilizan las posiciones medidas de los árboles locales una sola vez y el atlas existente como recurso, no como distribución autoral global.
- Una única Brecha anclada, sin fracturas antiguas ocultas y sin crecer al alejar la cámara. Los tres zooms ven la misma geometría.
- Las capturas de blockout muestran el mismo mundo sin follaje ni pigmento de detalle; NO se llaman «antes». El antes auténtico proviene del commit y run archivados. Se preservan las cámaras del primer control para comparación causal.
- Se mantienen selección, colliders, progreso, guardados, UI y escena. Las pruebas estructurales pasan a exigir ausencia de la vieja malla y Brecha única, además de conservar todos los controles funcionales. Se amplía el filtro nativo con las suites existentes de interfaz, gestión y aceptación V3.

Estado: nueva implementación pendiente de ejecución y revisión visual. No ART_APPROVED; no resultado 9/10, ni perfil físico móvil, ni autorización de merge/publicación.

### Primera ejecución de la base limpia: b104df0

Run [39916484244](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39916484244), artefacto 12485161721, ZIP SHA256 `361d4c5d61a808bcad5710adadac697e90ad268bf22bdf6f715ddc60039930ca`: 16/16 PlayMode, fuente Unity y Blender completados. Descarga y CRC verificados; seis cámaras idénticas a 745f252c. Inspección de PNG real, no solo logs.

Rechazo como acabado: hijos de algunos árboles importados reaparecen por la política general de visibilidad; los cauces siguen excesivamente gruesos, lagos demasiado circulares, relieve apenas legible. La Brecha de roca pintada, al conservar tamaño físico entre zooms, invade la cercana. La base única sí funciona, pero todavía no demuestra fidelidad artística suficiente.

Siguiente decisión sobre esta misma base limpia: clasificar también descendientes y suspender únicamente sus LODs; probar ausencia de cada renderer hijo en los tres zooms. Tallar depresiones de ribera en el campo de altura compartido, reducir ancho fluvial, romper el contorno lacustre y vincular roca a pendientes reales. Sustituir el antiguo decal pétreo por una fractura ramificada autoral de geometría superficial: trazos oscuros, interior violeta fino y corazón luminoso pequeño, todo en una sola malla con cota y ancla constantes. Es una sustitución de representación, no otra Brecha superpuesta ni un cambio de nodos jugables.

### Revisión 611a90d y siguiente acabado integrado

Run 39917602042, artefacto 12486490444, ZIP SHA256 `8fd2a62e059ed05379b3ec495a6a5a30a239590945dc1acc66b95dd35158b42c`: XML 16/16 y 12 PNG completos, pero el proceso Unity terminó con acceso inválido nativo `-1073741819` durante `Cleanup mono`, después de anunciar fin de pruebas correcto. El workflow permanece FALLIDO; no se ignora el código de salida ni se declara QA completa. No hay stack nativo suficiente para atribuir la causa. La siguiente ejecución completa comprobará reproducibilidad.

Los PNG sí permiten revisar el ensayo: los árboles importados ya no reaparecen; sin embargo, la fractura de segmentos rectos parece un circuito y no una falla natural. Se rechaza ese acabado. Se mantienen sus ramas autorales, se reconstruye una única cinta continua por rama con subdivisión irregular, sección transversal de color y transparencia decreciente; se elimina la superposición de tres bandas rectangulares. Los hombros se hacen más estrechos y asimétricos para que sus pendientes tengan lectura real, en lugar de aumentar saturación sobre un plano. El suelo recibe una máscara de contacto derivada de las posiciones de TODOS los árboles efectivamente construidos, integrada en la máscara existente, sin nuevas superficies de terreno ni árboles adicionales.

### Validación 64489c6 y acabado del espacio cercano

Run 39918453410: fuente, 16/16 PlayMode, cierre normal del proceso y revisión Blender completos. Artefacto 12486161120, ZIP SHA256 `8dbf76a1ac53170b6ae22150e301ca98b00eb370133af76816eaafe479b33682`. El fallo nativo de cierre anterior no se reprodujo; no se ha desactivado ningún gate. Sigue sin certificación física ni aprobación artística.

Las capturas confirman retirada de duplicados, contacto forestal y falla fina fija. La cercana conserva demasiado suelo vacío respecto a la referencia. Se han medido los nodos de la escena original (respecto a WorldHome): bosque (47,12), cantera (46.5,-62.891), ruina (-21,-64), explorador (-85,-53), engendro (-92,-57). Nueva decisión compositiva: arroyo conectado que atraviesa el primer plano entre z=-17 y -35, fuera de dichos nodos; cruce de madera no interactivo donde coincide con la ruta local (-8,-28). No se mueve ningún objetivo ni se añade recurso.

Las praderas verdes pasan a tener lóbulos autorales propios, independientes de la densidad del dosel. La ruptura pequeña de borde queda subordinada a esas formas, como en Layout Generator; no es camuflaje de ruido global ni una imagen de fondo. Se combinan en la máscara de cubierta terrestre, con exclusión de agua/ruta, y las bandas húmedas se derivan del mismo canal fluvial. Se corrige además la revisión Blender para usar arcilla neutra: la textura RGBA de datos queda empaquetada como Non-Color y nunca conectada como si fuera pigmento final.

### Encuadre completo y lectura del relieve

35587b6: run 39919495742, artefacto 12486112032, SHA256 ZIP `c56b818072a4d941e74a7296217f2b9ed5429d96a3f446fb7000d4b09f44f00c`. Los 16 casos y las capturas se completaron, pero volvió el acceso inválido durante Cleanup mono: workflow rojo conservado. Existe incidencia oficial compatible [UUM-146722](https://issuetracker.unity.com/issues/23323/crash-on-cleanupmono-when-closing-the-editor) sobre cierres del Editor en Windows. Esto es una hipótesis respaldada por síntomas, NO diagnóstico probado de este proceso ni permiso para ignorar el fallo. Se conserva Unity 6000.3.23f1 y el código de salida como gate estricto.

Revisión visual: el arroyo y puente mejoran la composición cercana; las manchas de pradera demasiado uniformes y los hombros redondos no se aceptan como acabado. Se conservan las áreas autorales, modulando su cubierta interna para evitar charcos de color plano. El relieve pasa de montículos gaussianos a ejes de cresta con altura y anchura interpoladas, siguiendo el método de formas/curvas y falloff. Las normales de cara participan en el sombreado para leer las facetas reales.

Corrección de encuadre necesaria: 244 m de semialtura mostraban solo parte del eje norte-sur. La panorámica final usa 500 horizontal / 620 vertical y límites coherentes con la superficie existente 1820×1560. Se prueba que los extremos principales norte/sur entran en ambos formatos. Esto NO altera coordenadas ni geometría al girar el dispositivo. Las nuevas 14 capturas incluyen dos controles panorámicos con las cámaras históricas exactas (244/456), además de ocho vistas del mundo y cuatro blockouts. La herramienta de revisión usa esos controles para A/B y presenta el encuadre final por separado, sin falsificar comparabilidad.


### Revisión estructural de 3c64e5d y lectura ecológica

[Run 39920351223](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39920351223) completo correcto: Unity fuente, 16/16 PlayMode, cierre normal y Blender. ZIP SHA256 `9462a25ef451e1e12f497c93ab20fc0b126ab41d4f59ad102ff0fc16b4d2d60e`, 14 PNG reales. La panorámica horizontal amplía la cobertura del eje norte-sur sin deformarlo; todavía no alcanza el estilo original. Diagnóstico visual: dientes costeros demasiado periódicos, cauces azules de borde uniforme y ausencia de escala intermedia entre árboles y suelo. Valoración provisional ~5.5/10, no aprobación.

La siguiente intervención corrige esas tres relaciones: entrantes costeros de longitud desigual y suavizado del contorno cerrado en el mismo campo geográfico; ribera menos uniforme y agua con profundidad cromática; una única malla agrupada de pequeñas piedras y hierbas distribuida por las máscaras ecológicas autorales, con exclusiones explícitas de todos los objetivos, caminos, agua y Brecha. No hay superficies de suelo nuevas ni antiguos constructores de paisaje. Se conserva el mismo punto de partida y las mismas cámaras. Estas son hipótesis que requieren PNG de Unity, no una mejora declarada antes de verla.


### Evaluación de f5226f7 y corrección de la Brecha

[39921599849](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39921599849): fuente, 16/16, cierre normal y Blender correctos; artefacto 12487462084, SHA256 `aa63100209e7bf52600675ca1d3266e843bb5a2157ff274575d1d23d779c479d`. Las piedras/hierbas ayudan a leer escala cercana y las riberas son menos uniformes. La Brecha todavía parece un dibujo sobre césped. Corrección causal: depresión de 3.6 m localizada en radio 41 m sin alcanzar objetivos jugables, y material de suelo fracturado (juntas celulares irregulares) integrado en el único shader. Las ramas existentes mantienen su ancla; no se añade otra falla ni un portal.

El redondeo cerrado había afectado inadvertidamente la esquina continental remota y hacía aparecer mar en el extremo superior izquierdo. Se corrige el algoritmo para conservar intactos los segmentos de continuidad norte/oeste; solo redondea costa dibujada. Este hallazgo procede del PNG real, no de los tests verdes. Se mantiene pendiente de nueva captura. La panorámica vertical recorta parte del oeste: no se presenta como cobertura completa ni se estira el territorio para simularla.


### Precalcular la autoría y ampliar regresiones

8c97df1: [39922350970](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39922350970), artefacto 12488480501, SHA256 ZIP `92dbddb0a102baf32192953e616fef5af694bb9e31b4c19c7cd1518e4451ddf5`. Los 16 casos y las 14 capturas terminan, pero reaparece el fallo nativo al cerrar en Cleanup mono. Se conserva rojo y no se atribuye todavía a una causa demostrada. Visualmente la depresión y el suelo fracturado mejoran la lectura de la Brecha; se mantiene una única superficie. Todavía no es 9/10.

Se empaquetan la máscara RGBA exacta exportada por Unity y las 90.201 alturas de su malla (gzip sin pérdida); el jugador carga esos recursos en vez de repetir la autoría de millones de muestras. El Editor conserva el rebake explícito mediante ELDORIA_REGION1_REBAKE_CLEAN_MASK=1. La validación de fuente comprueba dimensiones, datos lineales sin compresión con pérdida y muestras contra las funciones autorales actuales. La proveniencia queda en CLEAN_BAKE_PROVENANCE.json. Se exige comprobar las capturas después del cambio; no se presume identidad visual.

El filtro de regresión añade los dos tests existentes de SpawnlingAnimatedV4Tests: llegada real, resolución de combate, victoria persistente tras recarga y ausencia de escrituras desde presentación. El gate exige al menos 18 casos y sigue rechazando fallos, ignorados y salida nativa no cero. No se debilita el control para ocultar el cierre intermitente.
