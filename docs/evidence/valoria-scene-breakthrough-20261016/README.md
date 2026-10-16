# Eldoria — Bastión I: resultado del experimento de escena limpia

**Veredicto: NO VISUAL WIN.** Se han creado y probado las escenas; no se ha demostrado el salto artístico solicitado. La nueva propuesta conserva la estructura general, pero su suelo, geología y coherencia material son peores que la base anterior. No se integra, no se publica y no se declara ART_APPROVED. Este resultado describe esta implementación: no prueba que Unity, URP, el 3D híbrido o un presupuesto de 0 € sean incapaces de alcanzar el objetivo.

[Comparativa real de cuatro columnas](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/docs/evidence/valoria-scene-breakthrough-20261016/comparison-four-columns.jpg) · [3D nativo](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/docs/evidence/valoria-scene-breakthrough-20261016/final3d-main.png) · [3D + fondo 2.5D](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/docs/evidence/valoria-scene-breakthrough-20261016/final25d-main.png).

Las columnas son referencia, Bastión anterior normalizado para la revisión, propuesta 3D y propuesta 3D + 2.5D. Las filas son principal, horizontal, retrato, desplazamiento y zoom. La misma ilustración completa se repite como contexto: no se han inventado vistas móviles o desplazamientos de la referencia. Las imágenes de las otras columnas son capturas Camera.Render de Unity; se conservan los PNG originales y las matrices de cámara. El PNG original del Bastión anterior también permanece junto a su variante normalizada.

| Evaluación visual aproximada, 0–10 | Anterior | 3D | 3D + 2.5D | Peso |
| --- | ---: | ---: | ---: | ---: |
| Composición y proporciones | 5,2 | 5,6 | 5,6 | 20 % |
| Arquitectura | 5,6 | 5,8 | 5,8 | 20 % |
| Paisaje y geología | 4,5 | 3,0 | 3,0 | 20 % |
| Materiales | 4,8 | 3,8 | 3,8 | 15 % |
| Luz, profundidad y atmósfera | 4,5 | 3,6 | 3,5 | 15 % |
| Lectura móvil | 5,5 | 5,5 | 5,4 | 10 % |
| **Global** | **5,0** | **4,5** | **4,5** | **100 %** |

Son estimaciones visuales con incertidumbre aproximada de medio punto, no métricas objetivas de belleza. Esta inspección fue realizada por el agente que montó la escena, por lo que no es una revisión humana ciega. Se obtuvo además una crítica separada de Qwen3-VL 8B local: concluyó NO VISUAL WIN y puntuó las alternativas 5,5/5,0. Su 9/10 para la base anterior y sus inferencias sobre diferencias de iluminación no se sostienen frente a las imágenes y la verificación del primer plano; no se utilizan para certificar calidad. La respuesta íntegra y el contexto realmente suministrado están archivados.

Lo que sí funciona es la organización de dos alturas, dos entradas, puente, escalera, parcelas y cámara manipulable. La puerta y la cabaña reutilizada tienen detalle real. Sin embargo, la explanada continúa demasiado uniforme; los acantilados se leen como caras texturizadas con costuras; hay contactos y colocaciones de vegetación defectuosos. Las piezas no forman todavía un paisaje convincente. No se atribuye este fallo a falta de herramientas: el ensamblaje y el tratamiento del terreno de esta propuesta son insuficientes.

La alternativa 2.5D se **probó**, usando un horneado nativo del paisaje y agua no interactivos de la propia escena, nunca la ilustración de referencia. Toda la arquitectura, parcelas, accesos y suelo jugable siguen siendo geometría real. Funciona para orientación fija, desplazamiento acotado, zoom y formatos horizontal/retrato. Un giro de 10° se conserva como prueba de estrés no soportada; no se ofrece cámara orbital. El fondo no mejora el primer plano. Se descarta como solución artística ganadora de este experimento, no como técnica universal.

| Comprobación realizada | Resultado y alcance |
| --- | --- |
| Compilación y ejecución Unity | PASS nativo, Unity 6000.3.23f1, proyecto URP 17.3, glTFast existente |
| Cámara | Pan, límites de zoom y Home probados por llamadas reales; capturas de 9 estados por alternativa/base |
| Selección | 6 anclas por alternativa: ambas puertas, cabaña, campamento y 2 reservas |
| Accesos | 126 raycasts de soporte por alternativa, con tolerancia de altura; no es certificación NavMesh ni recorrido de personaje |
| A/B | Igualdad exacta de cámara/configuración y huella de geometría/material del primer plano |
| Portabilidad | 4 escenas con GUIDs resueltos en Assets/paquetes y .meta persistidos |
| Progresión I → II | No ejecutada ni certificada en estas escenas de inspección |
| Android/iOS físico | No medido; los retratos emulados no prueban FPS, memoria o experiencia física |

| Coste estático de escena, no rendimiento de dispositivo | 3D | 2.5D |
| --- | ---: | ---: |
| Renderers activos | 287 | 260 |
| Triángulos de instancias | 660.914 | 630.511 |
| Materiales compartidos | 178 | 169 |
| Texturas compartidas | 285 | 286 |

La reducción de 30.403 triángulos, alrededor del 4,6 %, no demuestra una mejora de FPS. La textura adicional del fondo tiene un coste. No se declara optimización móvil aprobada.

Las escenas están dentro del mismo proyecto:

- [BastionI_Blockout.unity](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/BastionI_Blockout.unity).
- [BastionI_Hybrid3D_Final.unity](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/BastionI_Hybrid3D_Final.unity).
- [BastionI_Hybrid25D_Final.unity](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/BastionI_Hybrid25D_Final.unity).
- [BastionI_FrozenBaseline_MatchedReview.unity](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/43f0db08a3e9b597f0943be6e3503f1e90dc5894/Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/BastionI_FrozenBaseline_MatchedReview.unity).

Para inspeccionarlas, seleccionar esta rama en el proyecto existente y abrir la escena en Unity. En Play, arrastrar desplaza, rueda/pinza cambia zoom, Home restaura la cámara y un clic/toque corto selecciona. Es una escena de inspección interactiva: no contiene el macroloop completo ni sustituye el juego.

El origen es main consultado en 81b1f762 y el claim 3b233cb5. Fuente exacta de la comparación final: **bda7305f5674b09505ac52de800f3e22943cdadb**. Ejecución **38952589240**, evidencia persistida **43f0db08a3e9b597f0943be6e3503f1e90dc5894**, artefacto **12068214908** (SHA-256 bd95fec9787435c041b4b61a4aa484946df76d154fc3dc68f9367036ab32fdc4). El manifiesto exacto contiene los SHA-256 de las cuatro escenas y 27 PNG. La normalización del baseline usa tamaño original 26, tamaño de revisión 24, escala uniforme 24/26, giro Y de 180° y anclaje en el pie de LowerGate; se conserva la imagen nativa sin normalizar. Una corrección posterior arregla la coma decimal de su JSON, sin cambiar capturas ni geometría.

La cadena conservada incluye el blockout 38946722924, el primer híbrido 38948240084 y el fallo nativo 38949465765 al hornear el fondo. La escena 3D guardada se recuperó sin reconstruirla; el horneado se resolvió con proceso separado/capa de culling en 38950499990. No se ocultó el fallo ni se sustituyeron renders por imágenes generadas. La última corrección retiró 56 instancias anteriores de roca; aun así quedaron defectos de costura y materialidad, visibles en la entrega.

Se reutilizaron fuentes con SHA: puerta V3 y módulos del commit 0abf1387; familias anteriores; cabaña Valoria_Aserradero_AP2_v1 ya existente; pino y roca de artefactos previos; y recursos ya importados Nature Starter Kit 2/Holotna. No se compró ni generó un recurso de pago. Las fichas oficiales identifican Nature Starter Kit 2 y Mountain de Holotna como gratuitos bajo Standard Unity Asset Store EULA; no son CC0. Quaternius identifica sus modelos como CC0 y compatibles con uso comercial. Véanse source-manifest.json y RESEARCH_AND_LICENSES.md para procedencia y límites. No hubo nueva autoría Qwen en Blender: las mallas de soporte son prototipo de Unity y el modelo local se utilizó para crítica, no para certificar un artista profesional o aprendizaje de pesos.

**Decisión de integración:** conservar el Bastión jugable anterior y ambas ramas pausadas. No portar este resultado a main. Se pueden conservar como herramientas de revisión la escena aislada, controles, cámaras y verificador; la cabaña es candidata a una comparación individual posterior, no una promoción automática.

Para un futuro candidato que supere el gate artístico, la integración concreta sería copiar únicamente su raíz visual a una copia del Bastión I, enlazar Ground/Construction/Built de ValoriaParcelPresentation con los mismos IDs de gameplay, mantener los proxies de selección y las dos reservas de 9×9 m, y ejecutar de nuevo progresión I→II, persistencia, capturas y perfilado físico Android/iOS. La fase de ampliación a Bastión II permanece sin implementar en este experimento. Un resultado cercano a 6 experimental tampoco satisface por sí solo el contrato formal de 8/10.

No está demostrado un método ganador. El siguiente cambio útil tendría que resolver el conjunto suelo–roca–puente–arquitectura mediante arte de entorno coherente y editable, con una prueba de imagen completa antes de escalar. No se propone otra biblioteca, otro cerebro IA ni continuar microvariaciones de este candidato rechazado. El banco de pruebas queda conservado y el recurso Windows se libera al cerrar.
