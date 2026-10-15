# Eldoria — evaluación independiente real: «ojos» frente a «cerebro» (V1)

**Fecha:** 2026-10-15. **Ejecución de diagnóstico:** [GitHub Actions 38699407600](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38699407600), **artefacto 11956145394** (imágenes y respuestas originales). **Revisor:** GPT-6, distinto de los modelos locales Qwen que generaron las respuestas; lectura de PNG originales y resultados brutos del artefacto. **Ámbito:** diagnóstico no destructivo, SIN modelado, Unity ni aprobación estética.

## Identidad de las evidencias

- Torre original congelada `Valoria_Tower_CohV1.glb`: SHA-256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`. Inspección nativa: **una malla GLB importada**, WORLD Z arriba, límites XYZ aprox. X[-1.224,1.224], Y[-1.325,1.325], Z[0,3.271]. Una malla de Blender no garantiza que cada isla de geometría sea contigua.
- Tres vistas **reales** del mismo original (`front_three_quarter.png`, `reverse_three_quarter.png`, `elevated_roof.png`), más `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`. Sin cambios en el original.
- Prompt/contrato de este ensayo SHA-256 `92a6c8a2bfc5914d44f8dca404a1bf7fe142bcf26325ac099140072a38fc9f41`. El examen divide visión por imagen y razonamiento posterior con las notas generadas por el modelo, sin que este escriba código ni manipule Blender.
- **Qwen4B** `qwen3-vl:4b-instruct`: respuesta de razonamiento original SHA-256 `e70a4b67a0db53d1e894014f8cb8a7ff5cf87d73d38ebaf7110234d3d358e251`, duración del examen reportada **52,69 segundos**.
- **Qwen8B** `qwen3-vl:8b-instruct-q4_K_M`: respuesta de razonamiento original SHA-256 `c1d032c3ce5cfa749fbd63c941f98382cbe6f886151201291d5326742c20b236`, duración **83,22 segundos**.
- El workflow acabó en **SUCCESS técnico**; ese estado significa que produjo respuestas suficientes para evaluar, no que su contenido fuera artísticamente correcto. **No hay `ART_APPROVED` ni `CALIBRATION_VALIDATED`.**

## Evidencias de ojos: ambos modelos pueden reconocer lo que ven

1. Ambos identificaron **fondo azul y rectángulo rojo** en una imagen de control cuya respuesta no estaba indicada por texto. Ollama confirmó capacidad multimodal `vision` y decodificación real de imágenes.
2. Ambos describieron razonablemente la imagen principal de una construcción de madera, su cubierta inclinada, escalera exterior y barandillas. Las tres vistas de la torre, presentadas por separado, generaron observaciones no vacías, al igual que la referencia de estilo.
3. La prueba previa con cuatro imágenes y un encargo largo en una única petición provocó que ambos declarasen que no podían ver imágenes, aunque la inferencia multimodal estaba funcionando. Dividir **observación por vista → síntesis posterior** eliminó ese bloqueo. Por tanto: **el problema no era ausencia absoluta de «ojos».**
4. Las descripciones por separado **no son perfectas**: ambos modelos confundieron en alguna vista la cubierta piramidal/a cuatro vertientes con un tejado a dos aguas, inventaron o contaron de forma dudosa peldaños y confundieron elementos de alero/cumbrera. Deben distinguir observación de conjetura.

## Evaluación independiente del cerebro: fallo de criterio espacial/artístico

**Qwen4B — RECHAZADO como diagnóstico profesional.**
- Consideró que el tejado de la **misma torre** tenía geometría cambiante entre cámaras porque una descripción decía «piramidal» y otra «a dos aguas». **No es una demostración de desalineación real:** la perspectiva puede hacer parecer distintas las mismas superficies, y las propias observaciones pueden ser inexactas.
- Extendió la falsa contradicción a la escalera y las barandillas: que unos peldaños se describan como rectangulares y desde otra cámara aparezcan pocos no implica que existan modelos diferentes por vista. Propuso rehacer elementos que están protegidos.
- Identificó la necesidad abstracta de estabilidad y conexiones, pero no dio evidencia física fiable de qué superficie concreta era defectuosa. **No demuestra tres reparaciones justificadas.**

**Qwen8B — RECHAZADO como diagnóstico profesional.**
- Afirmó que las dimensiones o el cuerpo cúbico de la torre obligaban a un tejado plano / cumbrera horizontal. **Es falso:** una planta aproximadamente cuadrada es compatible con cubiertas a cuatro vertientes y ápices; la caja de límites no define por sí sola la topología del techo.
- Propuso reemplazar la cubierta por una solución a dos aguas sin una causa observada suficiente. Esta es una **hipótesis destructiva**, no una corrección dirigida.
- Propuso trasladar la **escalera exterior al interior**, precisamente un elemento protegido por contrato y visible en varias cámaras. Infringe el encargo del propietario.
- Su mayor tamaño y respuesta detallada no produjeron mejor criterio espacial en esta muestra concreta.

| Criterio evaluado | Qwen4B | Qwen8B |
| --- | --- | --- |
| Canal multimodal y control de color | Demostrado | Demostrado |
| Reconocimiento de torre en una imagen | Demostrado con imprecisiones | Demostrado con imprecisiones |
| Descripción independiente de tres cámaras | Suficiente para revisión, no libre de errores | Suficiente para revisión, no libre de errores |
| Conciliación coherente de perspectivas | **FAIL**: diagnostica falsos cambios de geometría | **FAIL**: interpreta mal cubierta y límites XYZ |
| Respeto de puerta, escalera, barandilla, malla original | **FAIL**: reconstrucciones no justificadas | **FAIL**: propone alterar escalera protegida |
| Tres intervenciones profesionales sustentadas | **FAIL** | **FAIL** |
| Preparación para autoría autónoma en Blender | **NO** | **NO** |

**Veredicto:** `VISION_PERCEPTION_PARTIALLY_PROVEN / SPATIAL_ART_REASONING_REJECTED_BOTH`. Ninguno supera hoy el examen de criterio autónomo de arquitectura/arte. No hay ganador autorizado entre 4B/8B; la comparación es una muestra concreta, no demuestra incapacidad permanente. No sería correcto usar estos planes como instrucciones de edición ni declarar aprendizaje de pesos.

## Hipótesis siguiente y cambios necesarios antes de modelar

1. **Geometría fiable antes de juicio:** incorporar al razonamiento las superficies reales, alturas, normales, contactos de malla/BVH y la topología de la cubierta; la caja XYZ NO justifica afirmaciones sobre formas. Los tres renders muestran una **sola fuente**, no tres alternativas geométricas.
2. **Separar observación, conciliación y crítica:** obligar a detectar desacuerdos entre descripciones como **incertidumbre de cámara**, no a decidir que la malla cambia según ángulo. No proponer reparaciones hasta verificar desde un render adicional o dato geométrico que el defecto existe.
3. **Contrato anti-destrucción:** rechazar todo plan que cambie puerta, escaleras, barandillas, identidad, UV/atlas, u otra zona correcta. Penalizar hipótesis físicamente falsas, no solo JSON inválido o componentes sin AABB.
4. **Reevaluación verdaderamente independiente:** repetir la prueba sin crear activos y comprobar si las tres reparaciones sobreviven revisión de imágenes/estructura. Solo después de evidencia favorable puede proponerse reautorizar una **única** edición Blender A/B, nunca un ciclo de producción entero por CI verde.

**Presupuesto:** 0 €, usando Qwen/Blender existentes. **Estatus de producción:** pausada por orden del propietario; original intacto; Unity intacto. **No se trasladan ni borran memorias, ramas o pruebas anteriores.**

Esta revisión reemplaza la mera etiqueta `INDEPENDENT_VISUAL_REVIEW_REQUIRED` del workflow por un juicio humano/IA independiente, **sin convertir un archivo de diagnóstico en manifest `ART_APPROVED`**.
