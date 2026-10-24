# Eldoria — acabado de la ribera existente

Variante conservada del PR #100. Presupuesto 0 €. No hay fusión, publicación ni aprobación artística.

## Qué se ha conservado y qué se ha cambiado

Se abrió una copia verificable del último prototipo válido, procedente del run 39941757944. No se reconstruyó la ribera ni Región 1. La fuente original, las imágenes canónicas y las pruebas permanecen intactas.

- **Superficies:** máscaras UV independientes para pradera, tierra, humedad, erosión, hojarasca, caminos y piedra. El acabado retenido añade pinceladas de cobertura vegetal y bordes de tierra ligados a caminos, árboles y orillas existentes.
- **Orillas:** edición de vértices del terreno heredado y de sus paredes, con entrantes, salientes, biseles y cambios de altura. El agua comparte los bordes editados.
- **Contacto y luz:** AO real horneada en Blender, conservada como imagen independiente; sombras recibidas y proyectadas en Unity URP. Ajustes de estudio restaurados al terminar la captura.
- **Agrupación:** se reubicaron 18 pinos y 12 rocas existentes. No se añadieron nodos, recursos ni mecánicas.
- **Híbrido:** árboles cercanos 3D y atlas lejano renderizado desde esos mismos árboles. Las mallas mantienen la proyección de sombras al pasar a tarjetas.

La pintura se ejecutó mediante bpy sobre imágenes, máscaras y nodos editables. No se afirma una sesión manual con el ratón en Texture Paint. Las referencias originales se utilizaron para juzgar estilo y composición; no se pegaron al terreno.

## Decisiones basadas en las imágenes

| Intervención | Decisión visual |
|---|---|
| A: escultura y máscaras | Conservar orillas. Rechazar grandes manchas angulosas del suelo. |
| B: contornos curvos, AO, luz y agrupación | Conservar volumen, sombras y agrupaciones. El suelo sigue demasiado liso. |
| C: cobertura pictórica UV y agua/follaje alternativos | Conservar cobertura y bordes del suelo. Rechazar agua demasiado oscura y follaje con contraste duro. |
| D: consolidación | Mantener suelo de C y agua/follaje de B; misma geografía. |

No se ha descartado el prototipo útil por no llegar al objetivo artístico. Tampoco se ha declarado satisfactorio todo cambio por superar las pruebas.

## Archivos

`fuentes/` contiene el BLEND final y los estados A, B y C para recuperar decisiones. `exportacion/` contiene el GLB que consume Unity y todos los PNG editables. `capturas/` contiene originales de Unity y sus cámaras; El manifiesto enlaza los PNG canónicos originales; las comparaciones incluyen el fragmento correspondiente. `zoom/` contiene el vídeo y la secuencia original completa. Las operaciones y el informe están documentados en el repositorio. `pruebas/` contiene XML y logs auténticos. `manifest.json` identifica hashes, commits y ejecuciones.

El BLEND final tiene texturas empaquetadas, curvas guía y colecciones editables. Algunas representaciones descartadas están ocultas y etiquetadas para recuperación. Al exportar, seleccionar las mallas visibles y exportar GLB; las tiras de agua anteriores se recuperaron en la variante final. La hierba triangular anterior queda preservada pero excluida de esta presentación.

## Límites que siguen abiertos

La superficie aún carece de la riqueza pictórica del concepto; las copas y algunas agrupaciones siguen repitiendo formas, las orillas conservan tramos demasiado regulares y el agua necesita más naturalidad. La siguiente mejora debe dirigirse a esas formas y al diseño de cobertura, no ampliar el mundo ni repetir ajustes menores de color.

Esta ribera continúa como variante de estudio optativa dentro de la escena integrada. Las capturas usan Unity real, pero no acreditan que se haya sustituido el mundo jugable completo. Los objetos funcionales y sus colliders siguen vivos y se restauran sus renderers. Las pruebas conservadas comprueban los sistemas existentes; no son una evaluación de belleza.

El vídeo procede de 97 capturas originales de una trayectoria continua 34→52→34, con cámara fija y resolución 960×720. Se reproduce a 16 fps para revisión; no mide el rendimiento real ni la interacción táctil. Las comparaciones estáticas usan 1280×960 y las mismas cámaras del prototipo. No hay perfilado de móvil físico; mantener sombras de las mallas lejanas tiene un coste que habrá que optimizar antes de producción.

## Conocimiento aplicado

- Blender 5.1, **Render Baking**: UV, nodo activo, margen y bake de emisión/AO. Aplicado al contacto del terreno, manteniendo pigmento y AO separados. https://docs.blender.org/manual/en/5.1/render/cycles/baking.html
- Unity 6.3, **Use shadows in a custom URP shader**: `ShadowCaster`, variantes de sombra principal, coordenadas de sombra y atenuación. Aplicado al shader del estudio. https://docs.unity3d.com/6000.3/Documentation/Manual/urp/use-built-in-shader-methods-shadows.html

El acceso directo a las páginas `latest` de Blender y a Texture Paint falló; no se afirma haberlas leído. El procedimiento conserva máscaras e imágenes que pueden editarse en Blender sin repetir toda la construcción.

## Verificación final y evaluación artística

- Commit nativo: `7d04b80f752ec4d12c1d69640a9c5300441cea41`.
- Ejecución: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39947565310 — completada con éxito.
- Artefacto: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39947565310/artifacts/12498976720 . ZIP SHA256 `73f33d78c29d2a217c3aca09ee7aee960b1b9e02ca152fe4b329df66764cf96d`.
- BLEND final SHA256: `51152e69cb3a06ac6401351cf3ca6ca7beab82889ee2f45411740d510078ef3d`.
- GLB final SHA256: `046bfc47a43e883f591b9930e8e2d97860d04f991277ffdd82d96271128d5bc4`.
- 18/18 casos PlayMode aprobados; salida nativa satisfactoria. No se eliminaron ni debilitaron pruebas. Se mantiene la cobertura existente de progresión I–II, recursos, combate/Engendro y UI V3; no sustituye una nueva sesión manual exhaustiva ni una prueba física de móvil.
- Cámaras 34, 40, 46 y 52: posición, rotación y zoom idénticos al prototipo original; PNG 1280×960. Referencias canónicas intactas verificadas por el workflow.
- Zoom: 97 PNG de Unity, vista fija, 34→52→34, extremos idénticos. Vídeo de revisión a 16 fps; no benchmark de rendimiento.
- No se observan errores de compilación C# o shader en los logs finales. También se corrigió el aviso de liberar una RenderTexture que seguía activa.

| Criterio | Valoración final | Razón y límite |
|---|---:|---|
| Superficies | 5/10 | Bordes y cobertura más intencionados, pero todavía grandes zonas lisas y algunas huellas de bosque demasiado redondeadas. |
| Relieve y orillas | 5,5/10 | Biseles, irregularidad y estratos visibles; aún hay tramos que parecen una banda continua. |
| Bosques y agrupación | 5/10 | Árboles reconocibles y grupos junto al río; copas y triadas demasiado repetidas. |
| Iluminación y contacto | 6,5/10 | Sombras y AO visibles, árboles mejor asentados; falta ajuste artístico más completo. |
| Agua y caminos | 5/10 | Caminos con bordes trabajados, agua legible; río todavía esquemático. |
| Continuidad visual del zoom | 7/10 | Mismos elementos y sombras entre escalas en la secuencia; faltan dispositivo físico y revisión táctil. |
| Cercanía global al estilo aprobado | **5,5/10** | Mejora útil frente al prototipo, todavía lejos de la riqueza y naturalidad del concepto. |

Estas puntuaciones son un juicio artístico razonado a partir de capturas, no una medición automática ni una aprobación del propietario. **Se retiene la mejora; ART_APPROVED continúa falso.** No se amplía Región 1 sobre la base de esta valoración.

## Recuperación y procedencia

| Estado | Commit | Ejecución |
|---|---|---|
| Prototipo válido anterior | `865ddaaa71ce5ff105ea71c0426d973849158556` | 39941757944 |
| A | `60a3da68ade0dc88cf67f90e558ddf5317773307` | 39945404410 |
| B | `01522dee75356343869c02f8a12d9b7e96a2322e` | 39946078962 |
| C | `f75ef5cce9b941e602fbf64eea924ed516187c7c` | 39946968622 |
| D conservado | `7d04b80f752ec4d12c1d69640a9c5300441cea41` | 39947565310 |

Los artefactos de GitHub tienen caducidad; el paquete entregado conserva los BLEND A/B/C/final, mapas, GLB, capturas y secuencia completa para reabrir la autoría sin reconstruir el paisaje. La reproducción de CI descarga el estado C fijado por ejecución y comprueba su hash antes de consolidar; pasado el plazo del artefacto deberá restaurarse esa fuente desde el paquete conservado. No se ha introducido una dependencia de compra o servicio de generación.
