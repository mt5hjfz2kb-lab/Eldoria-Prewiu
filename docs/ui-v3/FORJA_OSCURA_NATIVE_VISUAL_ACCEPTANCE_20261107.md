# Eldoria — Forja Oscura · contrato de aceptación visual y jugable

**Estado: contrato de control de calidad, NO aprobación artística.** Workstream: `eldoria-forja-oscura-visual-proof-20261107`. PR canónica: #173.

## Referencia que eligió el propietario

Se toma como objetivo la **imagen concreta que proporcionó** (vista horizontal 1536 × 864, SHA-256 `c9085f49ade49ff0836e9547f402afe8ad85e87032db8d84659b237f777a65ae`). Presenta una interfaz **rica, adulta y oscura**, muy trabajada, de metal negro y bronce envejecido, iconografía ilustrada y profundidad gráfica. Su estructura distingue:

- Encabezado: identidad del jugador, nivel, poder y recursos.
- Accesos secundarios con medallones visualmente trabajados.
- Objetivo de capítulo compacto y legible.
- Navegación inferior coherente en cinco destinos.
- Ficha de edificio contextual, con ilustración genuina, nivel, costes, efectos y botón principal.
- Ciudad real visible como protagonista, sin rediseñar su atmósfera ni reemplazarla por una imagen ficticia.

La imagen original es una **propuesta conceptual**, no una composición de sprites jugables ni una licencia para cambiar los modelos de ciudad. No se exige similitud píxel a píxel de un fondo que no existe en el juego. Sí debe conservarse la calidad visual, estructura y sensación premium de **los controles**.

## Fallos que invalidan la aceptación incluso si C# y los gates están verdes

1. La interfaz se parece a una app plana o provisional: **paneles lisos genéricos, botones rectangulares sin profundidad, iconos que parecen placeholders**.
2. La estética se vuelve clásica/barroca con un exceso de ribetes, pergaminos o dorados que compiten por la atención.
3. Botones, iconos, recursos o textos recortados, demasiado pequeños o sin coherencia de materiales; texturas nine-slice deformadas en horizontal/vertical.
4. Cualquier panel azul antiguo que aparezca inesperadamente, modal duplicada o pantalla que tape una parte importante de la ciudad.
5. Los botones abren destinos incorrectos, pierden su estado original, dejan de responder o sus hit-targets son menores de 44 px equivalentes en el móvil.
6. Los recursos, costes, niveles, progreso, tiempo de construcción o nombre de jugador dejan de proceder de `RoyalSnapshot` y los gateways autorizados.
7. Se sustituye la ciudad original por una imagen inventada o se modifican cámara, iluminación y terreno para favorecer una captura.
8. Una captura ilustrada, una composición Python o un PASS del render estático se presenta como evidencia de integración nativa.

## Prueba obligatoria del HUD real

- Desde la **misma sesión Unity 6000.3.23f1**, producir ocho PNG `native-before-` / `native-after-` correspondientes a 1600 × 900, 900 × 1600, 844 × 390 y 390 × 844.
- Registrar SHA-256 de todas las capturas, `dataBefore == dataAfter`, y las medidas de botones y regiones con el capturador del juego; ningún test debe modificar una partida real.
- Revisar **cada par visualmente** mediante `tools/forja-oscura/build-review.py`; ver además el aspecto sin panel y con el edificio seleccionado, la apertura y cierre, las acciones habilitadas/bloqueadas, y el regreso a la ciudad.
- En Android y WebGL, comprobar interacción real, escalado, legibilidad, safe area, rendimiento y guardado. Cifras de FPS/memoria solo cuando estén medidas.
- El informe `ForjaNativeEvidence.json` solo acredita la captura: su estado `UNITY_NATIVE_CAPTURED_VISUAL_REVIEW_PENDING` **nunca implica `ART_APPROVED`**.

## Diagnóstico artístico actual del kit

**Parcial, no aprobado.** Los 20 iconos ilustrados de 256 px tienen volumen y una dirección reconocible, pero los PNG de las superficies actuales muestran un acabado más plano y menor riqueza material que la referencia escogida. El sistema gráfico está diseñado para poder sustituir cada sprite sin tocar las reglas del juego; **la equivalencia visual todavía no se ha demostrado**. No aprobar por comparación sobre fondo ficticio.

## Condición de cierre

La fase se cerrará únicamente cuando la **captura real** confirme calidad y fidelidad aceptables, las interacciones funcionen y el diseño haya pasado controles en el proyecto y dispositivos pertinentes. Hasta entonces, mantener PR #173 como borrador y no actualizar `main`, APK o WebGL por este cambio.

No se requieren veinte variantes visuales; una sola candidata puede evolucionar a partir de **defectos concretos observados en los PNG nativos**. Las correcciones son proporcionales a defectos observados, no vueltas cosméticas.
