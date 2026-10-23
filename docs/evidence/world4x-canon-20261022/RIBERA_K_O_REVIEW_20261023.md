# Eldoria — Ribera K → O, revisión artística del 23-10-2026

## Resultado y alcance

Se conserva **O como candidata mejorada de esta misma ribera**, sin sustituir la aprobación del propietario sobre K. I y K permanecen recuperables; N se rechaza artísticamente aunque sus 18 pruebas pasaron. No se amplía Región 1, no se fusiona main y no se publica. La escena sigue siendo el estudio opt-in `ELDORIA_RIBERA_PILOT=1`, no una sustitución permanente del mundo jugable.

**Valoración razonada global: aproximadamente 7/10, todavía por debajo del hito 7,5–8.** No es una medición automática, ni ART_APPROVED. La mejora más clara está en la silueta del bosque y la diversidad de perfiles de ribera. El prado sigue demasiado continuo, los caminos uniformes, el agua conserva trazos longitudinales largos y el fundido de impostores muestra tramado intermedio.

## Autoridad y comparación

Base del propietario: K, commit `a911bd153713e3aadf7eb7422faa07d91bbf28d8`, ejecución [39957867110](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39957867110).

Candidata O: commit [`feabc4d514b9ce4eed66f777d70d46b33cf167c8`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/feabc4d514b9ce4eed66f777d70d46b33cf167c8), ejecución [39965511083](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39965511083), artefacto `12506079287`.

Las capturas K/O usan posición, rotación, zoom y resolución idénticos: 1280×960; tamaños ortográficos 34, 40, 46 y 52. Se verificaron sus JSON y dimensiones. La secuencia nueva contiene 97 capturas auténticas de Camera.Render, a 960×720, recorriendo 34→52→34; primer y último PNG idénticos. El vídeo se codifica a 16 fps para revisión: **no acredita 16 fps de rendimiento móvil**.

Referencia visual: originales intactos `docs/visual-reference/world-map/01_vista_cercana.png`, `02_vista_regional.png`, `03_vista_panoramica.png`. El fragmento cercano utilizado es x=0..512, y=390..780 de 01. Se juzga estilo y relaciones geográficas, no equivalencia píxel a píxel; no se inventan edificios, enemigos, puentes o nodos para rellenar la ilustración.

## Decisiones observadas y ejecutadas

### M — cambiar el perfil físico, no solo la pintura

Entrada: `.blend` K exacto, SHA256 `1089e26bcb66f874d41a3579a1027916d5b7619ccc2892f8ac9c6c00b4234566`.

El canal parecía encajonado porque casi todo el borde mantenía una pared estrecha. Se esculpieron **cuatro tramos asimétricos** en las mallas existentes del terreno y las dos orillas, extendiendo coronación, hombro, plataforma aluvial, pie húmedo y pie sumergido. Los grupos de vértices conservan las selecciones. No se calculó una nueva geografía ni se desplazó el curso del río. Se retiró la pintura de estratos horizontales y se mapeó tierra sobre las coordenadas reales del margen.

La captura de Unity muestra plataformas bajas visibles junto a las rocas y alternancia con taludes altos. Se conserva esta intervención, aunque las plataformas todavía son angulosas y algunos tramos conservan un borde superior continuo.

Commit `ec99f0a85d0645e6f9736d034571026bd11e698d`; ejecución [39962626761](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39962626761); 18/18 pruebas.

### N — fondo físico y transparencia: prueba rechazada

Se abrió la fuente M exacta y se modeló un fondo editable de 193×17 vértices a partir de los límites existentes del agua. Una curva Bézier nativa y un atributo de profundidad describen el canal profundo alternante. Se exportó pigmento derivado de profundidad y una superficie alfa; un shader URP exclusivo del estudio permitía ver el fondo, sin activar globalmente OpaqueTexture ni refracción de pantalla.

**La técnica funcionó, pero el resultado no mejoró el arte.** En Unity el río quedó turbio y excesivamente liso: desaparecieron las líneas largas, pero también la claridad y la corriente estilizada de K. La profundidad geométrica bajo una superficie transparente no basta para producir una lectura pictórica convincente. No se continuó ajustando opacidad o color en muchas ejecuciones: se rechazó N, conservando fuente, shader y capturas como evidencia.

Commit `1cd47ca126f6beb6bdcadcfab9211e196a32b031`; ejecución [39964456990](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39964456990); 18/18 pruebas. Su shader permanece sin selección en el piloto O; el convertidor de materiales se restauró a su comportamiento anterior.

### O — acabado pictórico conectado a la ribera y siluetas forestales

O vuelve a la fuente M, conservando el agua K/I. Se pintaron dos atlas de margen de 2048×512 con máscaras independientes, conectadas a la coronación real del terreno: **20 trazos dirigidos** dejan huecos de erosión, roca y pie húmedo, en vez de islas de pigmento desligadas de la geografía. Las curvas guía son editables dentro de Blender. La tierra y la pradera se muestrean en las coordenadas reales del banco; el pie húmedo se oscurece dentro del atlas. El pigmento compuesto es el mapa activo y puede editarse mediante Texture Paint; las máscaras originales se conservan separadamente como fuentes, no se presentan como un nodo Mix en vivo.

Las dos familias de abetos esculpidas en K ya habían mejorado 22 árboles de acento. O asigna esos recursos a las **134 copas restantes**: hábito más estrecho en interiores densos, ramas extendidas en bordes abiertos. No aumenta el número de árboles, ni mueve sus posiciones horizontales. El volumen de ramas es el de los recursos editables K, no óvalos planos ni una regeneración de bosque aleatorio. Los impostores se vuelven a capturar a partir de esas mismas mallas reales.

La captura cercana mejora la definición de las ramas y la lectura de grupos; la regional conserva esas familias. Se conservan O y M. Esto no resuelve por sí solo la escasa diversidad de sotobosque, la homogeneidad del prado o el dibujo del agua.

## Evaluación artística por criterio

Escala razonada: 5 = prototipo legible pero esquemático; 7 = base útil con mejoras visibles y defectos evidentes; 8 = acabado convincente próximo a la referencia; 9–10 = fidelidad y coherencia de producción final. Las cifras son juicios, no puntuaciones calculadas por píxeles o tests.

| Criterio | K | O | Evidencia y límite |
|---|---:|---:|---|
| Perfil y transición de orillas | 6 | 7 | Alterna plataformas y taludes; todavía hay facetas duras y uniones demasiado regulares. |
| Bosques y siluetas | 6,5 | 7,5 | Ramas más legibles, desaparecen las copas globulares dominantes; solo dos familias principales, falta riqueza de estratos. |
| Terreno y caminos | 6,5 | 6,5 | Se preserva la textura I/K; pradera extensa demasiado homogénea y caminos de anchura uniforme. |
| Agua | 6,5 | 6,5 | Se conserva K porque N era peor; siguen las líneas continuas y falta articulación de corrientes/remansos. |
| Coherencia del estilo | 6,5 | 7 | Bosque más próximo a las formas de la referencia; material del terreno aún excesivamente granular frente a su pintura clara. |
| Continuidad 3D/2,5D | 6,5 | 6,5 | Geografía y posiciones constantes; se aprecia tramado y variación aparente de densidad durante el fundido. |

No se ha demostrado el nivel 7,5–8 global. No se amplía esta solución a Región 1. El siguiente problema artístico debe tratar la organización del pigmento del prado y la corriente del agua, manteniendo las mejoras de perfil y silueta; no reintroducir manchas aisladas de L, piscinas pintadas de J, copas remalladas globulares ni la transparencia lisa de N.

## Validación y límites

- Blender nativo 5.2.2 LTS; Unity 6000.3.23f1 y URP 17.3.0 en el runner Windows existente. Sin servicios de generación de pago, Qwen o compra de assets.
- O: source gate y ejecución PlayMode correctos; **18/18** casos existentes, sin eliminar ni debilitar pruebas. Incluyen Bastiones I–II, recogida real, cámara, UI V3, entrenamiento/marchas, Engendro, combate/guardado y conservación de nodos auténticos.
- Las pruebas del mundo integrado no convierten este estudio visual en una integración definitiva. El piloto oculta renderers previos durante la captura y los restaura; conserva objetos, IDs y colisionadores jugables. El export artístico no introduce colisionadores.
- 156 árboles conservan posición horizontal, rotación y escala respecto de K. **16 ajustan exclusivamente su altura** para apoyarse en el terreno esculpido M. Se documentan sus nombres en `verification-O.json`.
- Se verificaron las cuatro cámaras K/O y las 97 muestras de zoom. Revisión visual de capturas cercanas, regionales y muestras intermedias. No se afirma que el fundido sea imperceptible.
- Esta entrega no incluye perfilado de dispositivo móvil ni nuevas capturas verticales del piloto. Los tests de aspecto móvil del juego no sustituyen esa revisión artística pendiente.
- No hay fusión, publicación ni aprobación artística final. La aprobación pertenece al propietario.

## Fuentes técnicas específicas consultadas para N

Se leyeron las explicaciones accesibles de Unity 6 sobre [superficies transparentes de Simple Lit](https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/built-in/shaders-in-universalrp/reference/simple-lit-shader), [modos de mezcla URP](https://docs.unity.cn/6000.0/Documentation/Manual/urp/blending-modes.html) y [configuración de URP Asset](https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/universal-render-pipeline/urp-reference/universalrp-asset). Aplicación: mezcla alfa después de opacos, fondo real y ausencia de dependencia global de OpaqueTexture. La prueba visual desaconseja este acabado concreto para Eldoria, aunque la técnica sea válida. No se afirma una nueva lectura de todos los manuales históricos.

## Fuentes editables y recuperación

La entrega incluye `.blend` O con imágenes empaquetadas, GLB, texturas activas extraídas del propio GLB, máscaras y mapas de autoría, scripts M/N/O, materiales/shader del piloto, fuente K y M exactas para reproducir la cadena, fuente N rechazada, renders Blender identificados, PNG reales de Unity, zoom y originales canónicos intactos. `SHA256_MANIFEST.json` verifica cada entrada.

SHA256 O `.blend`: `a59b20127942ee986c74ff255102e127bb1e1ae8cc373f396046622890f27387`.

SHA256 O GLB: `129ad3bcd71c64481d1e405aa5fadb7b9df9ab053b5b51e0bc0fd8a96639729b`.

SHA256 artefacto nativo O: `55d10262b8227e3eb928ed2a1948cdd30f2b5902e6e3232452d2f173f7963d23`.

La reapertura nativa de M se verificó como entrada tanto de N como de O. O se guardó con los mapas empaquetados; no se afirma una segunda reapertura independiente de O que no se haya ejecutado.
