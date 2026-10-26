# Prueba interactiva BlenderMCP — 23 octubre 2026

La orden del propietario pausa nuevas iteraciones e integración del mundo. El estado anterior permanece en `4abc37a29ea7be07119a91938a35add3845db980`; este ensayo modifica únicamente una copia Blender y el enlace de observación del mismo PR100.

Fuente exacta: artefacto 12491502436, ejecución 39930695744. Blender `.blend` SHA256 `a86c14e78252c84c2182656656ba3848de4182cebd6c59df08d0621adc90a337`. Referencia: PNG canónico 03_vista_panoramica.png sin cambios. Última Unity auténtica disponible: 39926747679; el export posterior no llegó a producir capturas Unity por el fallo del verificador de posiciones. No equiparar ese render Blender con Unity.

## Conexión

Blender 5.2.2 LTS, complemento existente fijado por SHA256 `eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5`. Sesión local persistente y socket de loopback. El runner transporta una orden nueva cada vez, guarda respuesta JSON y devuelve capturas. No conexión directa de escritorio ni manipulación del ratón; se utiliza la API de Blender mediante BlenderMCP. No Qwen, servicios de assets ni gastos.

El arranque no contiene operaciones artísticas: abre una copia, ofrece captura y espera. Cada comando de modelado se redacta después de ver la imagen anterior. Las siguientes decisiones no están preprogramadas. La fuente original y los recursos de Unity no se sustituyen.

Incidencias de enlace: `addon_utils.modules()` aporta metadatos, no la implementación cargada; resuelto cargando explícitamente el archivo verificado. `Get-Content -Raw` serializaba propiedades PowerShell junto con el texto; reemplazado por ReadAllText. Son reparaciones del enlace, no ciclos artísticos.

## Observación inicial

Referencia examinada a resolución original. Comparada con el render anterior: los bosques tienen huellas ovales y algunas masas occidentales quedaron exageradamente alargadas por la deformación de la composición. Los ríos parecen cintas con extremos desconectados; el terreno apenas establece alturas; la Brecha es radial y demasiado fina. Se necesita probar decisiones localizadas y observar sus consecuencias, sin reconstruir otro paisaje.

La sesión inicial respondió pero no sobrevivió al fin de la orden PowerShell. Se prueba arranque independiente mediante Win32_Process, manteniendo el socket exclusivamente en loopback y un límite automático de una hora. Ningún puerto se abre a Internet. Esta incidencia tampoco cuenta como ciclo artístico.

## Ciclo 1

Observación 00 de la sesión persistente PID36124. Se identifican masas occidentales 04,09,14,19,28 excesivamente estiradas. Se usan origin bounds, resize, rotate y translate sobre esos cinco objetos. Captura 01: desaparecen las bandas horizontales, pero permanecen cuerpos ovales y sombras que refuerzan su lectura como piezas separadas. Resultado limitado, no aceptación artística. La segunda decisión se redacta solamente tras ver 01: modificar las mallas mediante selección de vértices y edición proporcional. Terreno y cámaras conservados.

## Ciclo 2

Tras inspeccionar 01, se aplican transformaciones y se editan los contornos con tres selecciones proporcionales nativas por masa. Cambian 811 vértices de cinco mallas; terreno sin cambios. Captura 02: los entrantes existen, pero las superficies continuas se pliegan y parecen hojas. Rechazado por calidad visual, aunque el control interactivo sí funciona. La decisión 03 se toma tras ver esa captura: recuperar las cinco masas desde 01 y probar una intervención de estructura hidráulica, extruyendo la desembocadura del río existente y modelando el terreno local.

## Ciclo 3 y veredicto visual

La captura 02 lleva a rechazar las siluetas plegadas. Se recuperan exactamente las cinco masas desde 01. Se extruye el borde terminal de la malla `BLD_River_00` tres veces y se intenta abrir el cauce mediante edición proporcional del terreno existente. El registro nativo demuestra tres extrusiones y **solo un vértice de terreno desplazado**: la densidad local de la triangulación no soporta la escultura pretendida. Captura 03: aparece una prolongación pequeña, parcialmente tapada por el terreno. No se ha resuelto correctamente la desembocadura.

**Control interactivo verificado; mejora artística sustancial NO demostrada.** No adoptar estos resultados como arte aprobado ni integrarlos en Unity. Las tres decisiones se enviaron secuencialmente, después de ver las capturas anteriores, al mismo proceso Blender PID36124. El complemento recibe comandos Python de modelado por socket; no es control del ratón ni un viewport continuo dentro de ChatGPT. El runner introduce una latencia de aproximadamente un minuto por intervención, además de descargar evidencias. Esa limitación impide tratar esta conexión como una sesión de pincel en tiempo real, aunque sí permite decisión iterativa real.

### Qué falló exactamente

1. **Mis decisiones fueron demasiado locales para corregir la composición global.** Cambiar cinco bosques no resuelve el encuadre incompleto, la falta de jerarquía de masas ni la separación entre relieve y agua. El cambio de herramienta no sustituye esa dirección artística.
2. **Representación forestal inadecuada.** Una envolvente continua de 316 vértices puede servir como marcador de masa en un blockout. Abrir entrantes sobre esa envolvente produce láminas, no la estructura vertical y los huecos de un bosque. No debe insistirse en esculpir esas mismas tapas para obtener pinos.
3. **Topología de terreno insuficiente donde se necesita editar.** El total de 18.577 vértices engaña: la desembocadura tiene tan pocos controles locales que tres intervenciones terminan afectando a un solo vértice. Se requiere retopología local y bordes de cauce, no mayor fuerza del pincel.
4. **Encuadre y distribución todavía desacoplados.** La panorámica corta grupos al oeste y en el extremo superior. La referencia muestra una jerarquía completa. La deformación continental previa expandió objetos sin reconstruir su composición desde la cámara.

### Alternativa profesional concreta

Conservar BlenderMCP como acceso a herramientas y observación, pero no promocionar el procedimiento probado como solución artística. El siguiente ensayo debe ser **una sola zona piloto**, no otro mundo completo: encuadrar la referencia como imagen de fondo de cámara (nunca pegada al suelo), marcar sobre ese plano costa, claros y masas prioritarias; editar una silueta y su relieve desde esa cámara; retopologizar el cauce con bordes explícitos antes de esculpir; sustituir una única tapa forestal por un grupo pequeño de árboles tridimensionales con huecos y alturas diseñadas. Reutilizar únicamente árboles existentes que superen una revisión visual de estilo. Después comparar esa zona a cámara idéntica en Blender y Unity. La aceptación tiene que ser visual y previa a extender la técnica por el mapa.

### Valoración razonada de este ensayo, no medición automática

| Criterio | Antes | Después | Motivo |
|---|---:|---:|---|
| Composición general frente al canon | 3/10 | 3/10 | No cambia la jerarquía ni resuelve el recorte. |
| Bosques | 2/10 | 2/10 | Menos estirados, pero siguen siendo tapas; ciclo2 rechazado. |
| Relieve y agua | 2/10 | 2/10 | El intento de desembocadura queda incompleto. |
| Brecha | 2/10 | 2/10 | No intervenida; continúa radial y esquemática. |

No se evalúa acabado final ni rendimiento móvil en este ensayo de control. No hay nueva captura Unity y no se afirma validación funcional nueva: sus recursos no se modificaron.

## Trazabilidad

| Estado | Commit de orden | Ejecución | Artefacto |
|---|---|---:|---:|
| Observación inicial conectada | b8f7f148d6ddd609dbf5a38c71bd2b337fe3fcda | 39934291374 | 12492004109 |
| Ciclo1 | b5e2eec1039e359c62afef034903cb454c9c7c69 | 39934561432 | 12492893037 |
| Ciclo2 | b0485aa0c48d42936809b9014a638517d8557da9 | 39934901024 | 12493366792 |
| Ciclo3, reparación de carga de objetos incluida | 4c4f78a180fef46e22551883354203f5f76c2a7c | 39935648574 | 12494150780 |

Fuentes `.blend`: 00 `cec784c73cca9096edc4489ed23cecbc68b6f84c395e9489360aac54dcc40402`; 01 `499f3172fdfd1e3c6cac66728bae0444cba7f1bbe96200a99cbcd5e9e053b65a`; 02 `36b0a102277fe2ce5ea84eada8c161dab60fc0666a20950b6fdc6b74821e01b2`; 03 `2a437ab597948b964b668074a41461752b63ce462e57c8f3f6f2a187419b0efb`.

Todos los estados, capturas H/V, decisiones, solicitudes y respuestas quedan conservados. PR100 sigue siendo el único candidato. No se fusiona, publica, compra ni invoca Qwen. El cambio de prioridad suspende la integración artística y mantiene intacta su versión anterior.

## Cierre verificado

Ejecución final [39936368191](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39936368191), artefacto [12493961483](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39936368191/artifacts/12493961483). Las matrices de ambas cámaras coinciden exactamente con 00 (error 0); PID36124 conservado en las cuatro capturas y cierre confirmado por el sistema operativo. Dos de tres sondas de la prolongación del río quedan bajo la superficie del terreno: confirma el fallo visto en la captura, no una desembocadura terminada.

Fuente final `Eldoria_Interactive_Trial_NOT_APPROVED.blend`, SHA256 `ee2797a0e5e46b5c7e33e3a1d1654adf7b9debb3f940cf11c1e128a043cec47a`. Incluye las referencias originales empaquetadas y los comandos MCP como textos editables. Los estados anteriores no se sustituyen.
