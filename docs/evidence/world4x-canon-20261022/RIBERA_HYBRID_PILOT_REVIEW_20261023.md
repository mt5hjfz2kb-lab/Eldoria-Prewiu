# Ribera — estudio acotado híbrido 2,5D/3D

Estado: experimento de calidad; no ART_APPROVED, no ampliación de Región 1.

La prueba BlenderMCP queda cerrada y sus fuentes y capturas se conservan en `INTERACTIVE_BLENDER_REVIEW_20261023.md`. Este estudio no vuelve a generar el mapa completo. No modifica los PNG canónicos ni los sistemas de progresión, guardado, combate o recursos.

## Referencias y alcance

Se observaron directamente los originales `01_vista_cercana.png` y `02_vista_regional.png`, 512×1024. La guía principal es la ribera inferior de la cercana (recorte de consulta x=0..512, y=390..780), contrastada con las agrupaciones de pinos, loma y cauce de la regional (x=8..310, y=104..260). Son recortes del original, no imágenes generadas. Los personajes y edificios del recorte no se reconstruyen en este estudio ambiental.

## Representación

- Fuente editable Blender: dos superficies con relieve, cauce abierto entre ellas, paredes de ribera, caminos, rocas y tres familias de pinos sólidos. Curvas Bézier y colecciones separadas permiten revisar las formas. Ninguna malla procede del antiguo terreno Unity.
- Pigmento de pradera: textura UV empaquetada en el GLB y el `.blend`. Su riqueza debe juzgarse en las capturas, no por existir una textura.
- Árboles cercanos: tronco y ramas en volumen con claros entre niveles. Las masas son grupos de individuos, sin envolventes ovaladas.
- Distancia: atlas RGBA obtenido renderizando los mismos árboles con la cámara, orientación y materiales de Unity. Planos en las mismas posiciones y mezcla por tramado complementario. No se emplea la referencia pegada al terreno.
- Prototipo de optimización: agrupación por material y un lote distante con atlas compartido. El horneado ocurre en el procedimiento de captura; aún no constituye un pipeline de horneado de producción ni una validación móvil.

## Validación y límites del estudio

La captura es optativa mediante `ELDORIA_RIBERA_PILOT=1`, después de las 14 capturas ordinarias y dentro de la prueba PlayMode que entra al mundo por el gateway auténtico con guardado desechable. Se conserva el estado funcional y se restaura la presentación al terminar. Durante el estudio se ocultan los renderers previos: es una evaluación ambiental dentro del motor integrado, NO una afirmación de que el nuevo relieve ya esté conectado a navegación, alturas de nodos y consultas geográficas del mapa completo.

Se guardan cuatro escalas y controles con cámara idéntica. Este encuadre es propio del estudio y no se presenta como idéntico a las capturas históricas de la panorámica. La cámara fija permite comparar el mismo terreno y observar dos puntos interiores de la mezcla. La rotación libre y cambios de inclinación exigirían impostores multivista; este primer atlas no los acredita.

## Fuentes técnicas consultadas en este estudio

- Unity 6.3 `LODFadeMode.CrossFade`: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/LODFadeMode.CrossFade.html
- Unity 6.3 transiciones LOD Group: https://docs.unity.cn/6000.3/Documentation/Manual/lod/lod-transitions-lod-group.html — explica el render de ambos niveles y la necesidad de implementar la mezcla en shaders propios. Aquí se utiliza un peso explícito por tamaño ortográfico y tramado complementario, no distancia euclídea fija.
- Blender 5.2 Film: https://docs.staging.blender.org/manual/en/latest/render/eevee/render_settings/film.html — transparencia de fondo para recursos de composición. El atlas de este estudio se hornea finalmente en Unity para mantener su luz real.

## Incidencias

El primer intento nativo `39938107015`, commit `9e40ad1772e340814b8a2ea99a48fd944037b839`, falló antes de crear recursos: se buscaba un nodo de material por su nombre predeterminado. Se corrigió creando y nombrando explícitamente los nodos. Ese intento no aporta renders ni demuestra calidad artística.

Resultado visual y hashes: pendientes de inspección de la ejecución real; no se asigna puntuación anticipada.

## Segunda revisión observada

Run 39940965250, commit eadb71fd4f6eb0c5b14e3c4b09322ecd2d0ec5c5: PNG reales cercana/regional y 2 transiciones; XML 18/18, salida nativa -1073741819 tras `Cleanup mono`. No se declara ejecución nativa completamente correcta. ZIP SHA256 89508965689582107d24f83a952b729c0f0e9bcca2d7dec31fba68ed4c3ae629.

Se observa mejora frente a la primera ribera: caminos curvos, cauce diagonal y borde con altura. Sigue rechazada como objetivo: copas apiladas, sobreiluminación verdosa, suelo poco convincente y pobre distribución del detalle. No se amplía el mundo.

Cambio de método para la siguiente prueba: extraer una copa ya esculpida del recurso existente PineCanopy77.glb (sin modificarlo), soldar y reducir su malla en Blender, distribuir copias en las posiciones del estudio y hornearlas en Unity para distancia. Se deja de generar la copa con anillos. Luz estilizada acotada para impedir que la intensidad de la escena blanquee la paleta.

## Resultado verificado del cambio de representación

Commit `865ddaaa71ce5ff105ea71c0426d973849158556`, [ejecución nativa 39941757944](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39941757944), artefacto `12496576481`. ZIP SHA256 `adf6f43f22b370971151cc85cec23c7980e2603225e21003c93da961fc138405`, CRC íntegro. XML: **18/18**, ejecución completa correcta y salida de Unity 0. Se observaron los PNG reales de zoom 34 y 52, además de los intermedios 40/46. Los cuatro controles anterior/candidato tienen idénticas posición, rotación y escala ortográfica, a 1280×960.

Fuente editable `.blend`: `259932e1ac071417ba579edde228890c74ee97d0e0d6c3fee92ace894df113a4`. Exportación GLB: `3e764b4b17d20f6f050bdb429f8c1f477c24733fbb1abdfc605301f127112d82`. El script reproduce esos recursos en el runner y la evidencia contiene los binarios; el piloto no se activa en el inicio normal del juego.

La tercera prueba utiliza una copa existente esculpida, extraída de `PineCanopy77.glb`, soldada y reducida en Blender. Sustituye las tres familias de anillos del primer intento. No se invoca Qwen ni ningún generador o servicio de pago. El terreno, curvas y distribución siguen procediendo de la fuente Blender de este estudio.

**Veredicto: NO supera la puerta artística.** Hay una mejora respecto de los óvalos y de los anillos, pero no una reproducción convincente del objetivo. Valoración razonada del estudio, no una medición automática:

| Criterio | Valoración | Evidencia visual |
|---|---:|---|
| Composición del fragmento | 4/10 | Río diagonal y bosque reconocible; claros excesivamente vacíos y relación entre caminos y loma poco convincente. |
| Relieve y riberas | 3/10 | Altura real de la orilla, pero perfil demasiado uniforme y pendiente amplia con lectura artificial. |
| Bosques | 5/10 | Copa volumétrica conservada en el atlas; repetición todavía visible y detalle parecido a escamas. |
| Agua y caminos | 4/10 | Trazados coherentes, pero agua como bandas lisas y caminos demasiado homogéneos. |
| Materiales y luz | 3/10 | Color corregido, sin riqueza pictórica suficiente; manchas circulares de suelo y falta de profundidad en el acabado. |
| Continuidad de zoom | 6/10 provisional | Mismas posiciones y siluetas en los cuatro puntos; no certifica animación completa, giro libre ni móvil. |

Valoración global aproximada: **4/10** frente al objetivo. No hay fundamento para afirmar 9–10/10. No ampliar este resultado a Región 1 ni pedir aprobación artística de un candidato que falla esta puerta.

La siguiente decisión de producción debe centrarse en un acabado pictórico autoral de un fragmento aún menor y en la lectura del relieve desde cámara: pintar bordes, estratos, claros y transiciones con intención, y comprobar una sola agrupación de vegetación junto a la ribera antes de distribuirla. Repetir más semillas, anillos, ruido, círculos o variaciones menores de color no resuelve la carencia observada.

**Límites pendientes:** pilotaje de navegación/alturas de objetivos sobre este relieve, orientación vertical, interpolación animada y perfilado físico móvil. Las pruebas existentes protegen el juego, pero no convierten la presentación aislada de captura en una sustitución jugable terminada. El encargo artístico permanece abierto.
