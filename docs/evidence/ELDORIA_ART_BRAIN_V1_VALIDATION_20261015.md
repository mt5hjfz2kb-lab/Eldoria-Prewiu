# ELDORIA — Arquitectura de ojos, geometría y cerebro artístico IA (validación V1)

**Fecha:** 15-10-2026. **Presupuesto:** 0 €. **Estatus:** capa de diagnóstico y transferencia segura **IMPLEMENTADA Y PROBADA**; artista profesional que puede editar **NO VALIDADO**. Nada se publica ni se modifica en la malla/Unity. **Autoridad:** último `main/AGENTS.md` mantiene pausa de producción.

## Arquitectura resultante y proveedores realmente operativos

1. **Ojos:** `qwen3-vl:8b-instruct-q4_K_M` en Ollama/Windows, vista por vista, como generador de observaciones **hipotéticas**. No se trata su texto como geometría verificada. Tres PNG auténticos de la misma torre SHA original.
2. **Medición:** Blender del PC produce geometría WORLD XYZ y vectores reales, independiente de Qwen. `tools/art-direction/vision-diagnostic/spatial_grounding.py` y `tools/art-direction/director-control/roof_direction_probe.py`; fuente original inmutable SHA `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
3. **Cerebro:** **nuevo modelo local Qwen3 14B** (texto/razonamiento, NO tiene ojos), descargado gratis de Ollama modelo Apache-2 Q4 para el PC con RTX 3060 de 12 GB. `strong_local_director.py` recibe mediciones verificadas y observaciones con su incertidumbre; `grounded_director_benchmark.py` conserva baseline 4B/8B. No tiene ninguna herramienta de modelado directamente.
4. **Revisor/gate independiente:** `tools/art-direction/director-control/evidence_gate.py` y doce pruebas automáticas: verifican SHA, cámaras, fuente, zonas protegidas, limitaciones de contacto y prohíben órdenes de Blender. La evaluación estética independiente sigue separada. `build_diagnostic_handoff.py` lleva la respuesta validada de 14B a `NO_EDIT` seguro y deja hashes para auditoría.
5. **Ejecución artística:** sigue PAUSADA por instrucción del propietario. `ART_APPROVED = false`; ninguna prueba nueva alteró la torre ni los assets Unity.

## Pruebas exactas ejecutadas

| Evidencia | Ejecutado | Resultado |
|---|---|---|
| [38699407600](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38699407600) | 3 vistas originales + referencia; percepción 4B/8B | Capturas auténticas y descripciones de un asset; juicio profesional insuficiente |
| [38701710879](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38701710879) | Blender original WORLD y prueba adversarial 4B/8B | 7/7 reglas explícitas en ambos; fallos en síntesis artística libre |
| [38703106394](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38703106394) | Gate independiente | 12/12 tests PASS (fail closed); no hay autorización artística |
| [38703423820](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38703423820) | Inventario real de Ollama y GPU | Solo 4B, 8B y variantes pequeñas; RTX 3060 12 GB |
| [38704030602](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38704030602) | Descarga GRATUITA Qwen3 14B + prueba de dirección espacial | Modelo local instalado; 8/8 casos explícitos; en respuesta libre cometió clasificación de tejado no demostrada y dijo NO_EDIT |
| [38705235631](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38705235631) | Nuevos normales geométricos Blender + autocrítica de 14B | Áreas inclinadas en X+, X-, Y+, Y-, corrección explícita del error de certeza sobre «dos aguas»; NO_EDIT |
| [38705666442](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38705666442) | Transferencia completa director 14B → propuesta → gate independiente | Gate PASS; salida `NO_EDIT_INSUFFICIENT_EVIDENCE`, cero órdenes ejecutables, `art_approved=false` |

## Resultados métricos relevantes

- Caja WORLD m: `[2.44774, 2.65, 3.27077]`; 1 malla importada, 11.400 vértices, 5.338 polígonos, 3.029 islas topológicas, UV original intacta.
- Inclinaciones superiores: 893 triángulos en la franja alta, estimadas por geometría real del GLB.
- Superficies inclinadas upper por direcciones WORLD: `X+ 2.68314 m²`, `X− 2.66940 m²`, `Y+ 2.30833 m²`, `Y− 2.30971 m²`. **Estas áreas no certifican por sí solas el número de faldones del tejado**, porque pueden incluir tablas y otras superficies.
- Qwen3 14B reconoció en la última comprobación que **no estaba justificada** su afirmación anterior de tejado a dos aguas, que perspectivas diferentes no cambian el GLB y que no puede autorizar una corrección sin comprobar geometría de contacto.
- Fuente de corrección final `qwen3-14b-roof-independent-correction.json` SHA-256 de respuesta: `32e96e5004d8d56038139c4e735fa6aeae94df14296f666d6fc5ed44734bca12`.
- Handoff final `director-no-edit-handoff.json`: `mode DIAGNOSTIC_ONLY`, `editor_actions=[]`, `decision NO_EDIT_INSUFFICIENT_EVIDENCE`.
- Verificación final `independent-evidence-gate-result.json`: `EVIDENCE_REFERENCES_VALID_DIAGNOSTIC_ONLY`, `can_edit_blender=false`, `art_approved=false`.

## Aprendizaje obtenido y reglas para reutilización

**Lección específica verificada:** NO dar por correcto lo que describen los ojos VL ni la primera síntesis del director. La información de Blender sobre coordenadas/normales de la fuente y la procedencia de cámara tiene prioridad. Un director que asegura haber identificado un tipo de cubierta a partir de dimensiones y conteos de triángulos no es fiable. Pedir crítica contra mediciones, conservar limitaciones y **negar ejecución** ante incertidumbre sí ha producido una autocorrección significativa en el 14B.

**No hay reentrenamiento de pesos:** se ha instalado un modelo más capaz *candidato* y se han conectado fuentes/evaluaciones; no se puede concluir aprendizaje permanente del modelo ni que ya produzca obra nivel 8/10. La memoria reutilizable está en estas evidencias y reglas.

**Fase de ingeniería cerrada:** ojos separados, medición espacial, director mayor, controles y transferencia segura probados a coste 0. **Fase artística pendiente:** autorización del propietario para levantar parcialmente la pausa y validar UNA corrección sobre copia + A/B de Blender + Unity + revisión independiente; después tres mejoras sucesivas según contrato. No recomendar edición concreta ni integrar 14B en el ejecutor productivo sin esa validación.

**Prohibido sustituir por una afirmación de éxito técnico la falta de aprobación artística.**


## Ampliación experimental 15-10-2026 — memoria, contexto amplio y hardware reales

**Autorización:** diagnóstico Qwen/visión de solo lectura ordenado por el propietario en la misma rama. La pausa de modelado y de Bastión I / 4X / Unity sigue activa. No modifica GLB, no usa créditos, no entrena pesos ni autoriza `ART_APPROVED`.

### Ejecución A/B original — respuesta observada, no aceptación artística

- **Run:** [38713558774](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38713558774); **artefacto** [11961059956](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38713558774/artifacts/11961059956), ZIP SHA-256 `31976e68f4b97354f89f9b8dfef1651dcc1f7f35b1a1ed9f670d00d525dd471a`; `paired.json` SHA-256 `c2df4ad769b6b82d7c536931c6e4b00eace80b76bf8649beff91a077237fcb15`.
- El runner físico `DESKTOP-R10PE55` recuperó la fuente original y ejecutó **24/24 inferencias auténticas Qwen3 14B**, dos variantes por doce afirmaciones, orden entremezclado; suma de latencias API **111,14 segundos**. La memoria eran siete lecciones incluidas efectivamente en la variante con memoria; no pesos entrenados.
- Puntuación automática de tres etiquetas: **sin memoria 8/12, con memoria 7/12; cero mejoras y una regresión**, `REGRESSION`, gate cerrado. **No hay evidencia de ganancia por memoria**. El único caso contado regresión es «la luz neutra demuestra materiales finales»: `NO_DEMOSTRADO` sin memoria y `CONTRADICHO` con memoria. Ambas respuestas niegan que el material final esté demostrado; la distinción del evaluador es discutible y NO se puede reinterpretar retroactivamente como una mejora.
- Limitación detectada al leer respuestas originales: dos preguntas sobre procedencia de cámara no recibían en el prompt la propiedad booleana explícita `all_cameras_same_exact_source_sha256`, aunque sí existía en la evidencia; no inventar certeza a partir de un contexto parcial. Se conserva intacto el trial V1 y se registra un **segundo ensayo prospectivo con etiquetas binarias equilibradas 8/8**, en el mismo sistema de memoria, sin suplantar el resultado original.
- Windows físico: **17.106.989.056 bytes RAM (~15,93 GiB)**; **NVIDIA RTX 3060, 12.288 MiB VRAM**, disponibles antes del modelo 11.716 MiB. Logs Ollama del contexto 4K: ~8.423 MiB de pesos CUDA, ~640 MiB KV, **41/41 capas GPU**.

### Comparación de contexto completo autenticado: 4K frente a 12K

- **Run:** [38714184789](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38714184789), `SUCCESS`; **artefacto** [11962281075](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38714184789/artifacts/11962281075), ZIP SHA-256 `e5cd04465530c17c3538474a887a72373fd471f178147f59ee81c5a758a73ab5`; `context-probe.json` SHA-256 `ab97ff7af69f74ce5c5a5a1fd8d894197566ecabccb58bb5ae17eaf5344d8293`.
- Se alimentaron **38.691 caracteres de evidencia auténtica**, incluyendo Blender WORLD de la torre congelada, descripciones de cámaras Qwen8B identificadas como hipótesis y diagnóstico espacial previo. Mismo contexto, pregunta, Qwen3 14B y temperatura cero; solo cambia `num_ctx`. Fuente SHA original validada `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
- **4K:** `2/4` comprobaciones correctas; entrada procesada `2050` tokens; generación `33,38 tokens/s`; pared `40,44 s`; SHA e islas topológicas erróneos (afirmó SHA ajeno y una isla).
- **12K:** `4/4` correctas; entrada procesada `6146` tokens; generación `16,88 tokens/s`; pared `50,23 s`; identifica SHA original, `3029` islas, sin certificar tejado exacto ni autorización para editar.
- En 12K, GPU consumida tras carga `10563 MiB`, libre `1553 MiB`; Ollama informa `context_length=12288` y `size_vram=10480034774` bytes. **39/41 capas** en GPU (2 capas CPU) en lugar de 41/41 en 4K. Por tanto, ampliación viable en la máquina, pero con pérdida de velocidad y poco margen VRAM. El cambio de latencia está afectado por cargas/reconfiguración: no tratar 40,44 vs 50,23 s como benchmarking de solicitudes calientes idénticas.
- Prueba exploratoria de **una ejecución por ventana**. Demuestra este caso, NO superioridad estadística general ni dirección artística profesional. No se han hecho cambios de fuente ni validación visual A/B.

**Conclusión después de la prueba equilibrada:** el modelo de texto Qwen3 14B funciona realmente en el Windows del propietario y es viable como director asíncrono de diagnóstico, con contexto 12K útil pero más lento. La memoria indiscriminada NO mejora el rendimiento medido; véase el ensayo V2 a continuación. Ninguna prueba produce `ART_APPROVED` ni `CALIBRATION_VALIDATED`.


### Ensayo de memoria equilibrado V2 — réplica del fallo sin ambigüedad terminológica

- **Run:** [38714648879](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38714648879); **artefacto** [11961931949](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38714648879/artifacts/11961931949), ZIP SHA-256 `2f485ed8843ea7d5e9ef3b87acd42f2fb8323ee1e3778a552cd05c93b30b8cff`; `paired.json` SHA-256 `ae60e31dcdf7d6cfb96620290665745a6b6c469cee46f766127941b7c6428d4b`. Código preinscrito `tools/art-direction/director-control/brain_memory_epistemic_v2.py`.
- Antes de ejecutar se fijaron 16 afirmaciones equilibradas: ocho hechos sustentados por el contexto íntegro de Blender y ocho afirmaciones no probadas/falsas, resultado binario `PROBADO` / `NO_PROBADO`; orden entremezclado de los 32 intentos; temperatura 0 y `num_ctx=4096`. Las siete lecciones de memoria son las anteriores, no se incluyó en ellas la etiqueta correcta. Pruebas de integridad: seis para equilibrio/duplicados/modelo además de las cuatro previas y siete del gate, todas PASS.
- **Sin memoria 16/16; con memoria 15/16. Ganancias 0, regresiones 1.** Suma de tiempos API **89,1 s**; mediana aproximada de generación en respuestas calientes **34,86 tokens/s**. Las 32 respuestas se completaron físicamente en Ollama, pero el workflow terminó en `failure` porque el gate de regresiones **falló correctamente en modo cerrado**, no por desconexión, GPU ni error de Qwen.
- **Regresión localizada `one_mesh`:** sin memoria `PROBADO` correctamente al leer `mesh_object_count:1`; con las siete lecciones no relacionadas `NO_PROBADO` y duda de la importación, pese a reconocer un solo objeto en su explicación. Esto sí es una degradación epistémica verificable de la respuesta para el mismo hecho medido, y no la ambigüedad de tres etiquetas del V1.
- `memory_sha256=eff98fbf207a128ffc58df4a61e2f749fafc0c55591915a4d5644e103f15ab83`. **No existe evidencia de aprendizaje del modelo por añadir toda la memoria a cada prompt.** Inyectar indiscriminadamente lecciones aumenta distracción y puede inducir dudas no justificadas. El contrato de memoria debería priorizar **recuperación por relevancia y procedencia verificable**, con abstención cuando no exista lección aplicable; NO habilitar ese comportamiento para editar arte hasta ensayo posterior de no regresión.
- **Decisión posterior al diagnóstico:** `MEMORY_BENEFIT_NOT_VALIDATED`, `CONTEXT_12K_FEASIBLE_AND_HELPFUL_IN_THIS_CASE`, `ART_APPROVED=false`, `CALIBRATION_VALIDATED=false`, `BLENDER_EDITS=0`. Mantener ojos Qwen-VL 8B + Blender WORLD + director Qwen14B como funciones separadas. Para casos breves, usar 4K para conservar VRAM/velocidad; para grandes evidencias reales, usar 12K solo cuando el contenido lo justifique y ejecutar en solitario. No reclamar más velocidad general ni mejoras visuales hasta medirlas.

**Cierre del diagnóstico autorizado:** infraestructura, modelo 14B, 24+2+32 inferencias, puntuaciones, hardware y fallos reproducibles con archivos originales/hashes verificados. La **capacidad de modelado profesional** no se ha validado y sigue expresamente fuera del alcance de esta misión de solo lectura; no se elevó la pausa de producción ni se alteró la torre.
