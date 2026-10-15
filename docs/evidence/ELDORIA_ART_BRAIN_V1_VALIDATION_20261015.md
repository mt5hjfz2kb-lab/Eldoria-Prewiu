> **ESTADO VIGENTE — 2026-10-15: COMPARACIÓN REAL DE MEMORIA SELECTIVA COMPLETADA.** Run [38726028141](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38726028141), 96 inferencias Qwen3 14B en el PC. Sin memoria 32/32; completa 31/32; selectiva 32/32. Se reproduce y evita la regresión `one_mesh`; cero regresiones de DECISIÓN selectiva en esta muestra. No demuestra superioridad sobre baseline ni generalización. Fuente y selector intactos; producción artística pausada. Esta cabecera y la sección final sustituyen los anteriores estados PENDING/BLOCKED, que se conservan como historia. La solicitud actual de creación en la ruta original fue aceptada por los controles normales; el motivo de los rechazos antiguos sigue desconocido.

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


## Seguimiento — recuperación selectiva de memoria (diagnóstico aislado, 15-10-2026)

- Se implementó **sin duplicar el catálogo** `tools/art-direction/director-control/selective_memory.py` (commit `f26df80666f3cc2b1ce966a7b828bf7fb36d1820`). Consume las siete lecciones originales de `brain_memory_epistemic_v2.MEMORY` mediante índice temático; nunca consulta etiquetas de respuesta, no altera pesos, no edita Blender; máximo dos lecciones y **abstención de recuperar memoria** cuando no hay pistas temáticas.
- La revisión estática independiente de 13 afirmaciones ensayadas, **simulando el selector exacto**, dio 13/13 coincidencias de selección esperadas. Sobre los 16 enunciados del ensayo real anterior, el selector recuperaría una o dos lecciones para ocho y ninguna para ocho. En la pregunta `one_mesh`, que realmente falló con toda la memoria en [#38714648879](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38714648879), devuelve **cero** lecciones. Esto es una comprobación del enrutamiento, **NO una inferencia real de Qwen ni una mejora confirmada**.
- Ejecución independiente de las pruebas generales de seguridad del director en el commit del selector: [#38715972496](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38715972496) `SUCCESS`. Ese workflow NO ejercita los resultados reales de Qwen con memoria selectiva.
- **Pendiente y no validado:** realizar prueba real Qwen3 14B de las tres variantes (sin memoria, siete lecciones, memoria selectiva) sobre la misma evidencia congelada y un conjunto reservado de afirmaciones nuevas. Guardar respuestas crudas, hashes del contexto exacto, selecciones, latencia, aciertos y regresiones; bloquear promoción si existe una sola regresión con respecto a la variante sin memoria. No modificar los resultados V1/V2.
- En esta sesión, la plataforma **bloqueó expresamente** dos operaciones GitHub de creación de nuevos archivos para pruebas y benchmark (`mcp__GitHub__create_file`; una por controles de seguridad y otra por no poder determinar su estado de seguridad). No se reintentaron por vías alternativas para eludir el control. Esto es **un impedimento del lanzamiento de la validación**, no evidencia de fallo de conexión al runner Windows, que continúa históricamente demostrado.
- **Decisión vigente:** `SELECTIVE_MEMORY_ROUTING_IMPLEMENTED`; `SELECTIVE_MEMORY_QWEN_AB_VALIDATED=false`; `ART_APPROVED=false`; `CALIBRATION_VALIDATED=false`. La producción artística sigue pausada; no reclamar mejora de calidad hasta evidencia Qwen y revisión artística independientes.

## Misión prioritaria de control operativo — resultado PARCIAL, 2026-10-15

**Esta sección prevalece sobre cualquier cierre diagnóstico anterior:** comparación selectiva Qwen PENDIENTE por rechazo de seguridad externo. El protocolo de continuidad sí se ha ampliado y comprobado. No se ha ejecutado producción, Blender ni una nueva inferencia en esta misión.

### A. Vías comprobadas en esta sesión

| Vía | Evidencia real / límite |
|---|---|
| GitHub lectura | Lecturas reales de repo, archivos, refs Git, runs, jobs, logs y artefacto. HEAD diagnóstico al inicio `4a6240fd9e3e87155a850479c699dd5aca41cb86`; main inicial `6ba6faef6cf9b18719aeb4b67f804cb713899dab`. |
| GitHub escritura | Claim aditivo de continuidad `3284d912de9f80c88169a4c2249bc4c4a14b447d`, script `6a55fbabf1e3cc162c0ce892332c95a7b9f47fff`, bootstrap `9eaaa927275539c9045799554aa7c7e7f3c8ca09`. Esto no elimina ni autoriza los rechazos previos. |
| Git público desde terminal Linux | `git ls-remote https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu.git HEAD` devolvió main inicial. Proxy `git.chatgpt-team.site` falló: `could not read Username ... No such device or address`. No se buscaron/extrajeron credenciales. Sin prueba de push desde terminal. |
| Actions observación | Jobs y logs reales de #38714648879: PowerShell Windows y 32 inferencias completas; gate falla por regresión. No confundir failure de aceptación con caída del modelo. |
| Actions ejecución ahora | Hay herramienta de rerun expuesta, NO ejecutada. No hay dispatch callable en el inventario de esta sesión; `workflow_dispatch` YAML es configuración. Un rerun ejecutaría el SHA antiguo, sin selector: no sirve para cerrar B. |
| Runner Windows | Histórico verificado `DESKTOP-R10PE55`, id 22, job `116354265361`. Estado ONLINE actual **NO VERIFICADO**. `/actions/runners` es endpoint rechazado por el conector (`INVALID_ARGUMENT`, no permitido). Consulta global: 0 runs en curso; dos runs antiguos en cola #37642384303 / #37642364289, ambos sin jobs asignados. No se cancelaron ni reactivaron. Cola vacía/antigua no certifica salud del PC. |
| PowerShell PC | Verificado históricamente mediante Actions en los logs del job citado. Ninguna terminal Windows directa expuesta en esta sesión. La shell actual es Linux; su localhost no representa el PC. |
| Blender PC | Evidencias históricas conservadas; job `116322906822` del run #38705235631 terminó SUCCESS en `DESKTOP-R10PE55`. No se arrancó Blender ni se produjo un modelo. |
| Ollama / Qwen3 14B PC | Inferencias reales históricas y artefacto V2 recuperado. Windows ejecuta Ollama en el mismo paso que Qwen y puede apagarlo al terminar; no se supone listener permanente. Sin nueva prueba de salud en el PC. |
| GitHub Desktop | Logs reales muestran Git `2.53.0.windows.4` desde `GitHubDesktop/app-3.6.7/resources/app/git/cmd/git.exe`. No es una interfaz remota de escritorio. |
| Integraciones alternativas | Inventario actual y búsqueda de plugins revisados: GitHub ya instalado; no se encontró integración pertinente de PowerShell/escritorio remoto en los resultados. Esta búsqueda no prueba inexistencia universal de otros plugins. |

### B. Bloqueo preciso y alternativas evaluadas

Operaciones recuperadas del diagnóstico/historial anterior, **no nuevas denegaciones de esta sesión**:

1. `mcp__GitHub__create_file`, rama diagnóstica, ruta `tools/art-direction/director-control/test_selective_memory.py`: pruebas automatizadas del selector; rechazo por controles de seguridad de OpenAI.
2. `mcp__GitHub__create_file`, misma rama, ruta `tools/art-direction/director-control/brain_memory_selective_ab.py`: benchmark de tres condiciones; la plataforma no pudo determinar el estado de seguridad.

No se dispone del payload completo, identificador de revisión ni texto literal íntegro de las llamadas originales; no inventarlos. Rutas, operación, propósito y motivos se contrastaron con el informe existente y la recuperación de contexto.

**Inspección del código actual, sin repetir las 13 pruebas:** `brain_memory_epistemic_v2.py` solo construye `without_memory`/`with_memory`; `question(..., memory)` usa un booleano e inserta el catálogo completo. No importa `selective_memory`, no ofrece tercera condición, selector, callback ni parámetro de lecciones. El workflow existente ejecuta ese archivo. Su rerun no cambia el código ni valida recuperación selectiva. El selector existe (blob `0373b91bcd179839859d3d20a244efafaf85ff1d`) pero no recibe ninguna llamada desde ese benchmark.

Reutilizar las 16 preguntas sería científicamente viable **si se incorporase autorizadamente** una tercera condición, con mismo contexto, modelo, parámetros y evaluación. Incorporar su equivalente en el archivo antiguo, un workflow, Python inline, monkeypatch, otra ruta/API o herramienta eludiría el rechazo: **no se hizo**. Tampoco se sustituyó el modelo por otro host/modelo, ni se etiquetó una predicción del selector como respuesta de Qwen.

**Intervención externa indispensable:** solicitar al canal de soporte/revisión de la plataforma la revisión y resolución de las dos operaciones anteriores, desde el chat donde ocurrieron para aportar sus trazas originales. La autorización general del propietario ya está concedida; no es lo que falta. Tras una resolución efectiva habrá que verificar la operación permitida, incorporar y ejecutar la comparación, confirmar disponibilidad de Windows y guardar respuestas completas/payloads/hashes/selecciones. No hay mecanismo expuesto aquí para revisar o revocar esas decisiones. Si además se necesitara dispatch manual, requerirá una capacidad real de ejecución de Actions; es una limitación distinta y no elimina el rechazo del benchmark.

La comparación vigente sigue siendo:

| Condición | Resultado auténtico |
|---|---|
| Sin memoria | 16/16 |
| Memoria completa | 15/16; regresión `one_mesh` |
| Memoria selectiva | **NO EJECUTADA** |

El baseline está saturado: igualarlo con 16/16 demostraría ausencia de regresiones en estas preguntas y mejora frente a memoria completa, **no superioridad frente al baseline ni generalización a casos nuevos**. El selector se diseñó tras conocer el fallo V2; este conjunto es regresión de desarrollo, no holdout independiente. No se promete ganancia antes de medirla.

### C. Continuidad incorporada y comprobada

Se amplió el protocolo existente de main (no un sistema paralelo):
- `tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md`, commit `9eaaa927275539c9045799554aa7c7e7f3c8ca09`, blob `2ee85fecee48827d018674d6ae166a7593a26da6`.
- `tools/eldoria-agent-preflight.mjs`, commit `6a55fbabf1e3cc162c0ce892332c95a7b9f47fff`, blob `2336aea4808153ea6c0d40efcecd1faa8477f667`.

El bootstrap ahora conserva checkpoint estructurado con rama, HEAD histórico, siguiente paso, rechazos, workflow/run/job/artefacto y hashes. El preflight recupera ese checkpoint, compara blobs exactos antes de reutilizar gates y admite inventario de herramientas actuales para detectar interfaces desaparecidas. Inventario solo acredita exposición; exige operación real para autenticar. No inicia procesos, instala, descarga modelos ni ejecuta jobs. Un checkpoint siempre es histórico hasta contrastarlo con lecturas vivas.

Comprobaciones nuevas del mecanismo de continuidad — **NO pruebas repetidas del selector**:

```json
{
  "scope": "existing session preflight recovery only; NOT selector/Qwen tests",
  "passed": 6,
  "checks": [
    "Fresh process and different cwd recover pending step, exact input identities and original run.",
    "Loss of GitHub exposure is detected without losing historical evidence or pending step.",
    "Invalid and absent inventory never claim verification.",
    "Changed and missing input fingerprints block automatic gate reuse; selector is not executed.",
    "Absent and malformed recovery checkpoint fail closed.",
    "Pause, diagnostic limits and unvalidated selective Qwen state are preserved."
  ],
  "fresh_process": true,
  "different_working_directory": true
}
```

Primera corrida detectó correctamente una discrepancia de un salto de línea en la copia local de las fuentes; se recuperaron los bytes exactos y se verificaron ambos blobs antes del PASS. No se cambió el selector ni el benchmark en GitHub. Ejecución local con probes reales identificó Linux, Git presente, Unity/Blender/Ollama no hallados **en ese host**, Ollama local sin listener y Windows no certificado. El chequeo no concluyó ausencia en el PC.

Prueba independiente solicitada por el propietario: agente nuevo sin historial (`fork_turns=none`), solo lectura, recuperó main `9eaaa927275539c9045799554aa7c7e7f3c8ca09`, diagnóstico `4a6240fd9e3e87155a850479c699dd5aca41cb86`, siguiente revisión externa, método PowerShell/Actions y estado Windows NOT_VERIFIED. Verificó lectura real y ausencia de dispatch/direct Windows en su inventario. Esto prueba recuperación desde contexto independiente en la infraestructura actual; **no** es una nueva sesión real del usuario ni garantía de persistencia de conexiones de Work.

Reproducción: desde un checkout con el bootstrap actualizado, ejecutar `node tools/eldoria-agent-preflight.mjs --root RUTA_REPO --tool-inventory RUTA_JSON`. El inventario es un array JSON de nombres actuales, sin secretos. Repetir con `[]`: debe informar interfaces GitHub ausentes, preservar `resolve-selective-benchmark-security-rejection` y no declarar Windows online. Con una copia temporal que cambie/quite un input, debe informar `changed`/`not_present_on_this_checkout`, sin ejecutar el selector. Sin inventario debe marcar `not_supplied`, no afirmar conexión. Verificar la lectura de main y del HEAD vivo por conector antes de cualquier acción.

`AGENTS.md` de main ya enlaza obligatoriamente este bootstrap. `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, claims y fuentes de producción de terceros se preservaron; solo se añadió nuestro claim acotado al registro canónico.

### D. Conservación de evidencia original V2

ZIP #11961931949 descargado y SHA-256 verificado contra `2f485ed8843ea7d5e9ef3b87acd42f2fb8323ee1e3778a552cd05c93b30b8cff`; bytes de `paired.json` verificados contra `ae60e31dcdf7d6cfb96620290665745a6b6c469cee46f766127941b7c6428d4b`. Contiene 32 registros, decisiones, razones, hashes de prompts/respuestas, latencias y tokens. El V2 original **no guardó el texto bruto completo de cada respuesta**, solo razón limitada a 350 caracteres y hash; no puede reconstruirse lo que no conservó. Este límite debe corregirse en la futura prueba autorizada, sin alterar retrospectivamente la evidencia.

A continuación se conserva el contenido original de `paired.json` recuperado, sin recalificar decisiones ni borrar la regresión. No es una ejecución nueva ni memoria selectiva:

```json
{
  "version": 2,
  "method": "balanced 8 proven vs 8 unsupported; predeclared binary gold; paired randomized order",
  "model": "qwen3:14b",
  "source_sha256": "e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869",
  "grounding_sha256": "93f226d8d389f30a745e0b8c8502c79633caa05c19f379214c231889c6ddf317",
  "memory_sha256": "eff98fbf207a128ffc58df4a61e2f749fafc0c55591915a4d5644e103f15ab83",
  "memory_lessons_count": 7,
  "case_count": 16,
  "baseline_correct": 16,
  "memory_correct": 15,
  "gain_count": 0,
  "regression_count": 1,
  "pairs": [
    {
      "id": "three_cameras_same_source",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "source_locked",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "dimensions",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "sloped_faces",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "many_islands",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "one_mesh",
      "baseline_correct": true,
      "memory_correct": false
    },
    {
      "id": "world_z",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "slopes_present",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "roof_one",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "roof_four",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "contact_aabb",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "camera_geometry",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "uv_replace",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "neutral_material",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "style_torches",
      "baseline_correct": true,
      "memory_correct": true
    },
    {
      "id": "green_is_art",
      "baseline_correct": true,
      "memory_correct": true
    }
  ],
  "responses": [
    {
      "case": "contact_aabb",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 35.3,
      "prompt_sha256": "2ac846a34c3bb16e4895826dea83fdb876b43f0bcfa3f10457e96e3d08e0d4ca",
      "raw_response_sha256": "556049cad17c2358abda7375f96fa50ad0b99c4587393b278bc72a5aaf3661c2",
      "prompt_eval_count": 744,
      "eval_count": 38,
      "total_duration_ns": 35286387900,
      "tokens_per_second": 33.41,
      "reason": "El solapamiento de cajas AABB no implica contacto físico de superficies según las limitaciones de evidencia proporcionadas.",
      "correct": true
    },
    {
      "case": "one_mesh",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 2.38,
      "prompt_sha256": "7f7a11a38a7ed0e7b8f0cbc7bb52c126ee5a71577a0fe34f9da9a5be70a31b06",
      "raw_response_sha256": "e83d33ac25fed4cbf9542b401f9efb6cfb6754a555fcfcd9a59b3d321074dba1",
      "prompt_eval_count": 901,
      "eval_count": 73,
      "total_duration_ns": 2341048200,
      "tokens_per_second": 34.69,
      "reason": "No hay evidencia inequívoca de que Blender haya importado un único objeto de malla de la torre. El contexto medido confirma la existencia de un solo objeto de malla, pero no identifica si corresponde a una torre ni si fue importado por Blender.",
      "correct": false
    },
    {
      "case": "uv_replace",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 2.41,
      "prompt_sha256": "e0b185e5b3871bc969411ff6dba24793047bb4da383a8907454d513ec4e620b2",
      "raw_response_sha256": "10d10b2cfbc1b8aace0c60fb2028f6728370c26656fadf743a93b5492a53b5bd",
      "prompt_eval_count": 745,
      "eval_count": 77,
      "total_duration_ns": 2362898200,
      "tokens_per_second": 34.71,
      "reason": "El contexto medido indica que las UVs existentes son protegidas y no pueden ser movidas, eliminadas o rediseñadas sin autorización del nuevo propietario. No hay evidencia de que la misión diagnóstica autorice la sustitución de las UVs de la torre.",
      "correct": true
    },
    {
      "case": "green_is_art",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 2.09,
      "prompt_sha256": "e30de20e0171a7e8e6c60675ed8948150050b35daa7bb16cd24e2e9481c4c1e4",
      "raw_response_sha256": "5dc6cfb30b3ddad47a8aba7697d728bca58b2c40e5c34621bd5f39208971212b",
      "prompt_eval_count": 747,
      "eval_count": 68,
      "total_duration_ns": 2067465400,
      "tokens_per_second": 34.73,
      "reason": "La calidad artística no se puede deducir inequívocamente de los datos técnicos proporcionados. No hay información suficiente sobre aspectos estéticos, creativos o de juicio artístico para determinar una calificación de 8/10.",
      "correct": true
    },
    {
      "case": "slopes_present",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.81,
      "prompt_sha256": "d251cb4b262d9f269dee74b3520a98ad40096c247ac6d27c2d6c6f02f41df880",
      "raw_response_sha256": "f5a7c4602bf3e59f7e32917154820a74974628e443f41857b89f919a307f205a",
      "prompt_eval_count": 908,
      "eval_count": 54,
      "total_duration_ns": 1790679500,
      "tokens_per_second": 34.85,
      "reason": "La geometría indica la existencia de superficies superiores inclinadas con 'upper_inclined_surfaces_exist': true y se especifica que hay triángulos inclinados en la zona superior.",
      "correct": true
    },
    {
      "case": "roof_four",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 2.42,
      "prompt_sha256": "30aaea3c7ad8e574df62717952bd29cd3644f37460bbc77af6c4f55e465ee05a",
      "raw_response_sha256": "ccd4b0745e3be14228fa898e13d49c9e2544c0ec51cf9787c0c4f3789069a5b0",
      "prompt_eval_count": 747,
      "eval_count": 79,
      "total_duration_ns": 2390887000,
      "tokens_per_second": 34.67,
      "reason": "Las normales agrupadas no certifican el número exacto de aguas de la cubierta. La información proporcionada indica la existencia de superficies inclinadas en la parte superior, pero no permite deducir inequívocamente que la cubierta tenga exactamente cuatro aguas.",
      "correct": true
    },
    {
      "case": "source_locked",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.67,
      "prompt_sha256": "4472016b0ef3d1b5f33ded2201be5e005688d255d697a96f3c1d93ddb400bda3",
      "raw_response_sha256": "b1d619e0a09a52aeff37fc289ded08742bb4208a6b3eeea65b25f54507a4dca3",
      "prompt_eval_count": 741,
      "eval_count": 55,
      "total_duration_ns": 1678607500,
      "tokens_per_second": 34.89,
      "reason": "La evidencia indica que la geometría fuente original se declara inmutable, ya que se menciona explícitamente que 'immutable':true y que se trata de un GLB inmutable importado.",
      "correct": true
    },
    {
      "case": "world_z",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.44,
      "prompt_sha256": "b7b9655103bad55bbc2f04de487ff28fec192e0a4dc209b25a9b1f48bf5ad137",
      "raw_response_sha256": "5c40bac58e083952755c399b9817318d905b379da58ce96dfb72533abaeee408",
      "prompt_eval_count": 741,
      "eval_count": 46,
      "total_duration_ns": 1416183400,
      "tokens_per_second": 34.99,
      "reason": "El campo 'world_up' en los hechos indica '+Z', lo que confirma que la vertical del diagnóstico corresponde al eje +Z en WORLD.",
      "correct": true
    },
    {
      "case": "dimensions",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 2.5,
      "prompt_sha256": "6bbad6d4d749e8039a3e7498d80e132d8bcd2629c02a9c1866c16b80641e1a87",
      "raw_response_sha256": "b92464a50e15174ec37aefb8e3de3b03a8379e50c9b7c47bf448e1dca847dab3",
      "prompt_eval_count": 755,
      "eval_count": 82,
      "total_duration_ns": 2498565000,
      "tokens_per_second": 34.63,
      "reason": "Las dimensiones world de la torre coinciden con los valores proporcionados en 'world_dimensions_m': [2.44774, 2.65, 3.27077], lo que se redondea a 2.45, 2.65 y 3.27 metros.",
      "correct": true
    },
    {
      "case": "neutral_material",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.84,
      "prompt_sha256": "379c001d60e127d10db07218aca7d6fb373da60532f44f65297f389efc9a99e3",
      "raw_response_sha256": "a3f80f15088f4beb92edae0934c85c6cd3852714ee495ddea9e6fdb1066b4343",
      "prompt_eval_count": 903,
      "eval_count": 54,
      "total_duration_ns": 1793607600,
      "tokens_per_second": 34.79,
      "reason": "La iluminación neutra en un render no demuestra el material final del juego, ya que las referencias de estilo y luz neutra no son medidas del modelo según las limitaciones de evidencia.",
      "correct": true
    },
    {
      "case": "roof_one",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.91,
      "prompt_sha256": "3b08822e8f57801e6964397c9fdfa89086f1bfc5bdb197f87e6f5da327bf23ad",
      "raw_response_sha256": "3d748b946bd7faac6313c857f8debb29332dc182e50d20204283aaf375dea6d1",
      "prompt_eval_count": 906,
      "eval_count": 62,
      "total_duration_ns": 1893902100,
      "tokens_per_second": 34.75,
      "reason": "La cantidad de triángulos superiores no permite certificar que hay un único faldón, ya que la presencia de superficies inclinadas no segmenta faldones de manera inequívoca según las lecciones confirmadas.",
      "correct": true
    },
    {
      "case": "contact_aabb",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.31,
      "prompt_sha256": "c975e35dd1183d7794476813f17b6316526ba889f86bf3a294ff8d7c53b08c61",
      "raw_response_sha256": "5a61bc61a01fb3ce86dd0518c3041c4dac3c21696c786676bb73fc5d4545e892",
      "prompt_eval_count": 906,
      "eval_count": 42,
      "total_duration_ns": 1312507000,
      "tokens_per_second": 34.97,
      "reason": "El solapamiento de cajas AABB no demuestra contacto físico de superficies, según las limitaciones de evidencia en el contexto medido.",
      "correct": true
    },
    {
      "case": "sloped_faces",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.47,
      "prompt_sha256": "799b4703864f220581204c278d36b73dbfcb3afb6179c555874a550938faa89e",
      "raw_response_sha256": "3a82fed6093ec86caa8b9e8d35af316c8b826c552bb9aa90c78d54a53bd2585a",
      "prompt_eval_count": 745,
      "eval_count": 47,
      "total_duration_ns": 1454980500,
      "tokens_per_second": 34.93,
      "reason": "El contexto medido indica que en la zona superior hay 893 triángulos inclinados, lo cual se alinea directamente con la afirmación evaluada.",
      "correct": true
    },
    {
      "case": "three_cameras_same_source",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.7,
      "prompt_sha256": "7739e9b8a39a76528d418afcc01b0b407cc6e409501579c5aade4100696aa103",
      "raw_response_sha256": "217420a23662b95c24814682bdf3d0e031de125a82f52da9d2db79d2463d5e4a",
      "prompt_eval_count": 741,
      "eval_count": 55,
      "total_duration_ns": 1678500700,
      "tokens_per_second": 34.86,
      "reason": "El contexto medido indica que todos los tres renders proceden del mismo GLB original inalterado, ya que se menciona explícitamente que 'All three images show the SAME immutable imported GLB'.",
      "correct": true
    },
    {
      "case": "three_cameras_same_source",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 2.02,
      "prompt_sha256": "ffdbe58792df92e9f15243960e394eeba83086ce12ef2f526c00f7dacca8174a",
      "raw_response_sha256": "36c007decda4d203f35bdbab5d8b2aa4495680ac2c5030a3532ed6f52ce17fdd",
      "prompt_eval_count": 903,
      "eval_count": 61,
      "total_duration_ns": 1996038900,
      "tokens_per_second": 34.73,
      "reason": "El contexto medido confirma que los tres renders proceden del mismo GLB original inalterado, ya que se indica que 'all_cameras_same_exact_source_sha256' es true y 'original_unchanged' es true.",
      "correct": true
    },
    {
      "case": "roof_one",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.92,
      "prompt_sha256": "31d64694f57597773715feb42d8b18cae12e30824c348af1e0d4fcb4b4a1ee17",
      "raw_response_sha256": "60a7a4646d73294489fc991843f787b311f1bac2c3663089b1dbe065dc499a47",
      "prompt_eval_count": 744,
      "eval_count": 63,
      "total_duration_ns": 1923684800,
      "tokens_per_second": 34.75,
      "reason": "La presencia de triángulos inclinados en la zona superior no permite certificar que haya un único faldón, ya que no se especifica la cantidad ni la configuración exacta de los faldones en el contexto medido.",
      "correct": true
    },
    {
      "case": "many_islands",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.45,
      "prompt_sha256": "c4bd968b650342f13b3840aece22bc6529789ac214ee7610d92cd2417c49c031",
      "raw_response_sha256": "2b858af46a4ef10d075433a79daeddea1702142e8b10997102294c3580516c38",
      "prompt_eval_count": 907,
      "eval_count": 41,
      "total_duration_ns": 1411971000,
      "tokens_per_second": 34.96,
      "reason": "El contexto medido indica explícitamente que la malla original tiene 3029 islas topológicas conectadas distintas.",
      "correct": true
    },
    {
      "case": "source_locked",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.69,
      "prompt_sha256": "a9c6ba145248e9a7977221461de89acf96ccda5d505b43e2eca0d8fdb0cd68dd",
      "raw_response_sha256": "60ccdf85616f48edc4176dadb587bd9cb73529fc906421a89c175b8b95a86a25",
      "prompt_eval_count": 903,
      "eval_count": 54,
      "total_duration_ns": 1653590700,
      "tokens_per_second": 34.81,
      "reason": "La geometría fuente original se declara inmutable en la evidencia, ya que el campo 'immutable' está establecido en true y se menciona explícitamente en los hechos proporcionados.",
      "correct": true
    },
    {
      "case": "camera_geometry",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.2,
      "prompt_sha256": "3130ebd9ffe4a88a28c675265bd843070a6e45eab69066a5171c7184e6db6496",
      "raw_response_sha256": "df20b7b20c0c16ffb92227150c32374189bd72ae4e225a1e71f39b9561cbcd48",
      "prompt_eval_count": 906,
      "eval_count": 37,
      "total_duration_ns": 1164860800,
      "tokens_per_second": 35.08,
      "reason": "Las tres cámaras ven el mismo archivo SHA inmutable; no se deducen modificaciones de vértices.",
      "correct": true
    },
    {
      "case": "world_z",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.41,
      "prompt_sha256": "d810dcc33ef8a753990086cf570298b60523e5c0749b6aaab59ddf1128694c28",
      "raw_response_sha256": "557ce33a480941ab600151e6a17bf2ba5b9f98e1926531e20b89ac87a22eb35c",
      "prompt_eval_count": 903,
      "eval_count": 45,
      "total_duration_ns": 1391800400,
      "tokens_per_second": 34.92,
      "reason": "El contexto medido indica claramente que el eje vertical en WORLD es +Z, como se especifica en el campo 'world_up': '+Z'.",
      "correct": true
    },
    {
      "case": "green_is_art",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.44,
      "prompt_sha256": "310479fda95a336df0e4c7d9dceda5115b3c231431278284e18513c9354d98ad",
      "raw_response_sha256": "6651253bccfefe104acf7c2a63edb8ddcf807388552f3be992ec2901d284a51d",
      "prompt_eval_count": 909,
      "eval_count": 46,
      "total_duration_ns": 1429876800,
      "tokens_per_second": 34.89,
      "reason": "La calidad artística no se deduce de los datos técnicos o de la autocrítica; no hay información suficiente para asignar una calificación estética.",
      "correct": true
    },
    {
      "case": "style_torches",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.38,
      "prompt_sha256": "901d49ceab7a3d9d121c657197d57348b03c3f0b2128c80db8ed2d4c6871c2fa",
      "raw_response_sha256": "5b44f2fbf405405d59ff0a67d3897425bb09733053982fd4d50a2a55c7922880",
      "prompt_eval_count": 910,
      "eval_count": 44,
      "total_duration_ns": 1370703900,
      "tokens_per_second": 34.94,
      "reason": "Las referencias de estilo y luz neutra no son medidas del modelo ni prueban la presencia de elementos como antorchas en el GLB.",
      "correct": true
    },
    {
      "case": "slopes_present",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.5,
      "prompt_sha256": "9d48d429b5d8f4bbb4a5685e28958f9bc281d2a69d02772d65a9818a4fa6dced",
      "raw_response_sha256": "30f80c2bbcdce0456f151f8d3cd25b2ce46b639ab1644eca51d57c092c3b6159",
      "prompt_eval_count": 746,
      "eval_count": 48,
      "total_duration_ns": 1484912900,
      "tokens_per_second": 34.91,
      "reason": "El contexto medido indica que existen superficies superiores inclinadas, ya que se menciona explícitamente que 'upper_inclined_surfaces_exist':true.",
      "correct": true
    },
    {
      "case": "roof_four",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 2.44,
      "prompt_sha256": "e942529e1ae1b11ee059de3949f966716984c994205eeaaef13bf1efbd6b8299",
      "raw_response_sha256": "fb8ac2bbb0347222fc2a88c0e5a7ff0525fc88ae4653c0c6b02596739b3d3313",
      "prompt_eval_count": 909,
      "eval_count": 74,
      "total_duration_ns": 2422370300,
      "tokens_per_second": 34.61,
      "reason": "Las normales agrupadas no certifican el número exacto de aguas de la cubierta. El contexto medido indica la presencia de superficies inclinadas, pero no permite deducir inequívocamente la cantidad de aguas ni su configuración específica.",
      "correct": true
    },
    {
      "case": "dimensions",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 2.38,
      "prompt_sha256": "7608c806a23b393797ff426cba2323f927ae874d3ae703ca5395cdf1b1e508b5",
      "raw_response_sha256": "5910abc93a08f899bf3446b314b30f3a72076528e7d98f6aa02f537958ee8759",
      "prompt_eval_count": 917,
      "eval_count": 78,
      "total_duration_ns": 2366209600,
      "tokens_per_second": 34.63,
      "reason": "Las dimensiones WORLD de la torre se especifican en el contexto medido como 2.44774, 2.65 y 3.27077 metros, lo que se aproxima a 2.45, 2.65 y 3.27 metros.",
      "correct": true
    },
    {
      "case": "sloped_faces",
      "arm": "with_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.84,
      "prompt_sha256": "8dea27a71e45644a859004b598e78133aead218731139bdae71378dec41be35d",
      "raw_response_sha256": "6b3e81cafe9f94db9182ab6f2cecb94b7347c89a711686d1cb7061a85aae7e38",
      "prompt_eval_count": 907,
      "eval_count": 60,
      "total_duration_ns": 1839533000,
      "tokens_per_second": 34.74,
      "reason": "El contexto medido indica que en la zona superior hay 893 triángulos inclinados, lo cual se deduce directamente de los datos proporcionados en 'geometry.zones_world_z.upper.inclined_triangles'.",
      "correct": true
    },
    {
      "case": "uv_replace",
      "arm": "with_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.34,
      "prompt_sha256": "89faceeab86372ccad9dcc68a953e8dc37fab7da95acec203b64d26f0d50178f",
      "raw_response_sha256": "6e81fe511e38eccc98e6641a66d4ed5978ab250afe5aa564ed4cda0293f2665e",
      "prompt_eval_count": 907,
      "eval_count": 43,
      "total_duration_ns": 1342606000,
      "tokens_per_second": 34.93,
      "reason": "Las UV existentes están protegidas según los hechos inmutables; no se autoriza su sustitución en el contexto diagnóstico.",
      "correct": true
    },
    {
      "case": "many_islands",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.17,
      "prompt_sha256": "74fe891f4ad1270fde8002dd8139f34516d99349022b7663942590da78981851",
      "raw_response_sha256": "fdd01c7375deb570de52586615589145520d5ebf809ee2f617471d11bce78261",
      "prompt_eval_count": 745,
      "eval_count": 37,
      "total_duration_ns": 1166094100,
      "tokens_per_second": 35.09,
      "reason": "El contexto medido indica que la malla original tiene 3029 islas topológicas conectadas distintas.",
      "correct": true
    },
    {
      "case": "neutral_material",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.09,
      "prompt_sha256": "4387bbe2cc6b299aba6e797ab44c7bd77f6e4b2a74878a67f409e196c08080c8",
      "raw_response_sha256": "787828306c81bd09746f299a4a3b892cd4043af4ff7458d721e18e64d26b5b14",
      "prompt_eval_count": 741,
      "eval_count": 35,
      "total_duration_ns": 1095917800,
      "tokens_per_second": 35.19,
      "reason": "El contexto medido no proporciona información sobre la iluminación neutra ni sobre el material final del juego.",
      "correct": true
    },
    {
      "case": "camera_geometry",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.42,
      "prompt_sha256": "246ed87fe9597274b2e39f284a0f6c77482ba05d64e2c3d9e9ef6eeb2c889f77",
      "raw_response_sha256": "7b30d133471be7d2f88dd7275b4c68232d862c2a75f550a975b40498181ce47a",
      "prompt_eval_count": 744,
      "eval_count": 45,
      "total_duration_ns": 1397342700,
      "tokens_per_second": 34.94,
      "reason": "No hay evidencia de modificaciones de vértices en las descripciones entre cámaras; se indica que todas provienen del mismo GLB inmutable.",
      "correct": true
    },
    {
      "case": "style_torches",
      "arm": "without_memory",
      "gold": "NO_PROBADO",
      "decision": "NO_PROBADO",
      "seconds": 1.86,
      "prompt_sha256": "68b01ae1f10fce0f56d909bf1a61655b0cb6444456a41a674ee9f414a90728ec",
      "raw_response_sha256": "8771bf2d42e26277d3a605454e302998b3be533139862030be96fce7fb46e561",
      "prompt_eval_count": 748,
      "eval_count": 60,
      "total_duration_ns": 1836733600,
      "tokens_per_second": 34.74,
      "reason": "No hay evidencia en los hechos proporcionados que indique la presencia de antorchas en el GLB. La información disponible no menciona ni prueba elementos gráficos como antorchas en el modelo 3D.",
      "correct": true
    },
    {
      "case": "one_mesh",
      "arm": "without_memory",
      "gold": "PROBADO",
      "decision": "PROBADO",
      "seconds": 1.3,
      "prompt_sha256": "8c4dc1302c7d4ced18a0966704cc590b8ccc7b53439f4a2c41a7d6a59f9261e2",
      "raw_response_sha256": "fbae5d09ebbdb067a456db0db4fa90eccc38a3b4a9cf6e0c15b2d1bb11c284cf",
      "prompt_eval_count": 739,
      "eval_count": 41,
      "total_duration_ns": 1271024700,
      "tokens_per_second": 35.02,
      "reason": "El hecho indica que 'mesh_object_count':1, lo que confirma que Blender importó un único objeto de malla de la torre.",
      "correct": true
    }
  ],
  "verdict": "NOT_VALIDATED",
  "art_approved": false,
  "any_blender_edits": false,
  "calibration_validated": false,
  "blender_edits_executed": false,
  "model_weights_trained": false
}
```

**Criterio final:** conectividad de GitHub verificada; ruta Windows histórica recuperada pero salud actual no comprobada; continuidad implementada/probada en el alcance indicado; B permanece bloqueado externamente; misión completa **NO TERMINADA**. Torre original intacta, cero Blender/Unity/jobs nuevos de producción, cero gasto, `SELECTIVE_MEMORY_QWEN_AB_VALIDATED=false`, `ART_APPROVED=false`, `CALIBRATION_VALIDATED=false`.

### Confirmación final de recuperación independiente y CI

- Contexto independiente sin historial de misión: lectura real de `AGENTS.md` blob `96aed3520a2bade9eebd07a609948b8f4ee3692a`, bootstrap blob `2ee85fecee48827d018674d6ae166a7593a26da6`, selector/benchmark originales y logs/jobs/artefactos. Reconstruyó correctamente el mismo pendiente y mantuvo Windows `NOT_VERIFIED`; no escribió ni ejecutó modelos/jobs.
- El contexto nuevo enumeró 89 herramientas GitHub y confirmó lectura real, pero no ejecución, ni una nueva sesión independiente de credenciales.
- CI del bootstrap exacto `9eaaa927275539c9045799554aa7c7e7f3c8ca09`: [#38721902138](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38721902138), job `116378249079`, `architecture` SUCCESS. Este CI no prueba Qwen ni Windows.
- El endpoint de runs con `?branch=diagnostic%2Fvaloria-ai-eyes-brain-20261015&per_page=30` se comprobó realmente y funciona. Para HEAD se utilizó `/git/ref/heads/diagnostic/valoria-ai-eyes-brain-20261015`; codificar la barra en el path de `/branches/...` produjo `INVALID_ARGUMENT`, resuelto con la ruta Git ref permitida. No es un bloqueo del repositorio.
- El informe íntegro incorpora ahora los 32 registros originales de V2 además de las referencias al ZIP; no depende solo de la caducidad del artefacto para conservar sus resultados. Ningún resultado fue recalificado.

## Comprobación EN VIVO del PC solicitada posteriormente — 15-10-2026

**Esta evidencia sustituye el estado anterior NOT_VERIFIED para la observación de las 12:49 Europe/Madrid; no garantiza disponibilidad permanente.** Se inspeccionó el workflow de inventario ya existente, se añadieron únicamente fecha/runner/versión y disponibilidad 14B al manifiesto, y el push autorizado disparó [#38722927795](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38722927795). SHA fuente `d7ace70f840c986dda61be58924a60e2081ca9da`, job `116381681682`, Windows `DESKTOP-R10PE55`: **SUCCESS**.

PowerShell real ejecutó la consulta de Ollama instalado, iniciando localhost solo si era necesario, y recuperó el inventario de cinco modelos. Se comprobó **Ollama 0.40.2 respondiendo**, `qwen3:14b` instalado (14.8B, Q4_K_M, 8.64 GiB), GPU RTX 3060 12.288 MiB. Cero nuevas inferencias, descargas o entrenamiento; Blender no arrancó; torre/producción intactas. El proceso Ollama que inicia la prueba se cierra al terminar: esto no significa que esté desinstalado o se haya perdido acceso.

Artefacto `11966897453`, ZIP descargado y hash real verificado `c54d53b7ecedbb72224b6fc66c3cd55e5017d7ffa75ec21521ce14fc7f38f04f`. Manifiesto original conservado:

```json
{
  "runner_name": "DESKTOP-R10PE55",
  "models_trained": 0,
  "schema_version": 2,
  "observed_utc": "2026-10-15T10:49:15.2655125Z",
  "previously_installed_models": [
    {
      "name": "qwen3:14b",
      "size_gb": 8.64,
      "parameter_size": "14.8B",
      "quantization_level": "Q4_K_M",
      "family": "qwen3"
    },
    {
      "name": "qwen3-vl:8b-instruct-q4_K_M",
      "size_gb": 5.76,
      "parameter_size": "8.8B",
      "quantization_level": "Q4_K_M",
      "family": "qwen3vl"
    },
    {
      "name": "qwen3-vl:4b-instruct",
      "size_gb": 3.11,
      "parameter_size": "4.4B",
      "quantization_level": "Q4_K_M",
      "family": "qwen3vl"
    },
    {
      "name": "eldoria-artist:latest",
      "size_gb": 3.11,
      "parameter_size": "4.4B",
      "quantization_level": "Q4_K_M",
      "family": "qwen3vl"
    },
    {
      "name": "hf.co/Qwen/Qwen3-VL-2B-Instruct-GGUF:Q4_K_M",
      "size_gb": 1.45,
      "parameter_size": "1.72B",
      "quantization_level": "Q4_K_M",
      "family": "qwen3vl"
    }
  ],
  "gpu_information": "NVIDIA GeForce RTX 3060, 12288",
  "new_models_downloaded": 0,
  "model_comparison_not_assumed": true,
  "art_approved": false,
  "runner_os": "Windows",
  "mode": "READ_ONLY_DISCOVERY",
  "qwen3_14b_installed": true,
  "ollama_version": "0.40.2"
}
```

**Corrección operativa:** había una ruta legítima para verificar la salud actual sin dispatch directo: el push-trigger del inventario read-only existente. La falta de dispatch callable no bloquea esa vía. Se dejó registrada en `main/tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md`, commit `a82fff509180a132a38b7d8cfc782ebcd9e391f8`, junto con el instante/run/job/artefacto. En futuras sesiones, inspeccionar este método antes de afirmar que no se puede comprobar el PC. Esta operación fue independiente del benchmark rechazado y no incorporó código equivalente.

Permisos actuales de la app GitHub consultados: ajuste específico **Allow all actions**. Lecturas, escrituras y ejecución por push han funcionado en esta sesión. No se modificaron permisos. Los rechazos antiguos de seguridad de los dos archivos del benchmark no se explican por falta de esta autorización, no se han revocado y **no se reintentaron**. No existe en las herramientas actuales un mecanismo de revisión/revocación de esas decisiones. La comparación selectiva sigue pendiente de resolución externa, no de reinstalar Qwen ni de reparar el PC.

## Validación real final — memoria selectiva, 2026-10-15

### Ejecución y resultado observado

- Modelo local `qwen3:14b`; Windows `DESKTOP-R10PE55`; run **38726028141**, job **116391981342**, fuente exacta `a9f1ffd83fa6fad6a88ed5e0a9e110aae205d52c`; **SUCCESS**. Completado a las 11:19 UTC.
- **96/96 inferencias reales**: tres condiciones entremezcladas sobre 16 preguntas originales y 16 reformulaciones prospectivas equilibradas (ocho verdaderas / ocho falsas-no probadas por conjunto). Misma evidencia congelada, mismo modelo, temperatura 0, seed 221015, contexto 4096 y límite 200 tokens.
- Los 32 hashes de prompts originales de baseline/control coinciden con el ensayo V2 previo. La fuente original, el catálogo y el selector **no se cambiaron** para esta prueba. Manifiesto de casos y gold fijados antes de inferir, sin pasar gold al modelo/selector.
- Las 16 nuevas frases son un holdout de formulación en dominios conocidos, no una prueba independiente de nuevos tipos de problema. El orden aleatorio cambia respecto a V2 al introducir brazos/casos; las respuestas del mismo prompt no se asumieron deterministas, se volvieron a medir como controles emparejados.

| Conjunto | Sin memoria | Memoria completa | Memoria selectiva |
|---|---:|---:|---:|
| 16 originales | 16/16 | 15/16 | 16/16 |
| 16 reformulaciones prospectivas | 16/16 | 16/16 | 16/16 |
| **Total** | **32/32** | **31/32** | **32/32** |

**Resultado limitado a esta muestra:** la recuperación selectiva evita la regresión que se reproduce al inyectar memoria completa, sin errores adicionales de decisión respecto al baseline. No supera al baseline sin memoria: lo iguala. No hay evidencia suficiente para superioridad estadística general, aprendizaje de pesos ni dirección artística profesional.

### Regresión conservada y revisión independiente

`one_mesh`, gold `PROBADO`:
- Completa: `NO_PROBADO`, vuelve a dudar de que el único objeto sea la torre aunque recibe `mesh_object_count:1`.
- Selectiva: recupera **0 lecciones**, responde `PROBADO` y cita el objeto único.
- Baseline: `PROBADO`; su respuesta bruta es idéntica a la selectiva en este caso.

Se inspeccionaron las 32 explicaciones selectivas, además de recalcular resultados y verificar las 96 respuestas API/modelo originales. No se detectó nueva regresión binaria. Las respuestas sobre UV contienen la expresión ambigua «nuevo propietario», que también aparece en baseline/control al traducir la evidencia inglesa: no es un permiso nuevo ni una regresión introducida por el selector. Las etiquetas correctas no prueban razonamiento perfecto. El caso reformulado `h_roof_count` no recupera lección por sus pistas léxicas y responde correctamente sin ella: el ensayo no certifica recall semántico exhaustivo.

Selección real: 16 preguntas con abstención, 12 con una lección y 4 con dos. Se conserva origen, índices y hashes de cada selección. Latencia API acumulada 206,37 s, incluyendo carga inicial; medianas observadas sin/control/selectiva 1,625 / 1,844 / 1,703 s. No se usa esta ejecución aislada como certificación de rendimiento.

### Evidencia bruta persistente y comprobación de bytes

Artefacto **11967828291**, nombre `eldoria-qwen14b-memory-selective-three-arm-v1`. ZIP SHA-256 `0ad49fd03f07b628ebe084bc32fbb59ab340b128eea26b52f50182b5478a508e`.

El ZIP EXACTO se conserva además en Git, sin depender de la caducidad del artefacto:
- Archivo: [three-arm-evidence.zip](eldoria-memory-selective-20261015/three-arm-evidence.zip).
- Commit de conservación: `a1dc9ae0ba048f39594508513ca80a4fff79049f`.
- Blob Git verificado contra bytes locales originales: `62c572aae74653b92289e8dccf6813c4dc6d2808`.
- `three-arm.json`: **510.351 bytes**, SHA-256 `d5b36db3aab797a959fa42dabacc48b0e69b29c0ba070318a735985865abd8a2`.
- Incluye petición completa por inferencia, respuesta API original y texto del modelo sin truncar, gold separado, decisiones, selecciones, tiempos, tokens, hashes, manifiesto predeclarado y fuente. Los originales V1/V2 permanecen intactos.

Revisión de integridad, realizada fuera del script productor:

```json
{
  "artifact_sha256": "0ad49fd03f07b628ebe084bc32fbb59ab340b128eea26b52f50182b5478a508e",
  "three_arm_json_sha256": "d5b36db3aab797a959fa42dabacc48b0e69b29c0ba070318a735985865abd8a2",
  "original_raw_file_bytes": 510351,
  "recomputed_correct_counts": {
    "selective_memory": 32,
    "without_memory": 32,
    "with_memory": 31
  },
  "cases": 32,
  "responses": 96,
  "by_group": {
    "original": {
      "complete": true,
      "counts": {
        "without_memory": 16,
        "with_memory": 15,
        "selective_memory": 16
      },
      "case_count": 16,
      "selective_regressions_vs_baseline": [],
      "selective_recoveries_vs_full": [
        "one_mesh"
      ],
      "errors": [],
      "verdict": "NO_SELECTIVE_REGRESSION_IN_THIS_SAMPLE"
    },
    "phrasing_holdout": {
      "complete": true,
      "counts": {
        "without_memory": 16,
        "with_memory": 16,
        "selective_memory": 16
      },
      "case_count": 16,
      "selective_regressions_vs_baseline": [],
      "selective_recoveries_vs_full": [],
      "errors": [],
      "verdict": "NO_SELECTIVE_REGRESSION_IN_THIS_SAMPLE"
    }
  },
  "errors": [
    {
      "case": "one_mesh",
      "arm": "with_memory",
      "raw": "{\"decision\":\"NO_PROBADO\",\"reason\":\"No hay evidencia inequívoca de que Blender haya importado un único objeto de malla de la torre. El contexto medido menciona un único objeto de malla, pero no especifica que corresponda a una torre.\"}"
    }
  ],
  "one_mesh": [
    {
      "arm": "with_memory",
      "raw": "{\"decision\":\"NO_PROBADO\",\"reason\":\"No hay evidencia inequívoca de que Blender haya importado un único objeto de malla de la torre. El contexto medido menciona un único objeto de malla, pero no especifica que corresponda a una torre.\"}",
      "recall_count": null
    },
    {
      "arm": "selective_memory",
      "raw": "{\"decision\":\"PROBADO\",\"reason\":\"El hecho indica que 'mesh_object_count':1, lo que confirma que Blender importó un único objeto de malla de la torre.\"}",
      "recall_count": 0
    },
    {
      "arm": "without_memory",
      "raw": "{\"decision\":\"PROBADO\",\"reason\":\"El hecho indica que 'mesh_object_count':1, lo que confirma que Blender importó un único objeto de malla de la torre.\"}",
      "recall_count": null
    }
  ],
  "seconds_total": 206.37,
  "sha_sources": {
    "brain_memory_epistemic_v2.py": "da05cf7faab0186177081f93489fa2c2db74c3cc42ea2d8fcad6da568cf42d09",
    "selective_memory.py": "c85382e9a7691a1acaeb828ab151b6c06ff367141287067be64fdb7977dd6057",
    "brain_memory_selective_ab.py": "a2fc108132bb8989e1f15c6773e0fa88c0a10cb13951bb4de1d2230da3daea5f"
  }
}
```

### Trazabilidad del desbloqueo actual

Tras la petición expresa del propietario de volver a presentar la operación, la **misma herramienta normal `create_file` aceptó la ruta original** `tools/art-direction/director-control/brain_memory_selective_ab.py`, commit `9bd2ae06da7df716de3b3fbc27dc7298d6cd2158`. Respuesta literal guardada en `ELDORIA_WRITE_REVIEW_INCIDENT_20261015.md`. No se cambió configuración de seguridad, se renombró el archivo ni se usó una vía indirecta. Esta aceptación específica permitió continuar. No se ha recuperado la causa de las dos negativas históricas: no atribuirlas a fallo de GitHub, PC, política nueva ni falso positivo sin evidencia.

El archivo de pruebas selector rechazado antiguamente no fue necesario: no se repitieron las 13 comprobaciones estáticas ya acreditadas. Se verificó la sintaxis/integridad del NUEVO harness y la equivalencia de sus prompts, y después se midió el modelo real. No se promueve producción por un CI verde.

### Estado y continuidad

- `SELECTIVE_MEMORY_QWEN_COMPARISON_COMPLETED=true`.
- `SELECTIVE_MEMORY_NO_DECISION_REGRESSION_OBSERVED=true`, **alcance 32 preguntas / 1 seed / 1 ejecución**.
- `SELECTIVE_MEMORY_QWEN_AB_VALIDATED=true` **solo para este ensayo diagnóstico**; no autorización general de edición, despliegue o producción.
- `SELECTIVE_MEMORY_GENERAL_BENEFIT_VALIDATED=false`; no superioridad frente a no usar memoria.
- `SELECTIVE_MEMORY_PRODUCTION_ENABLED=false`, `ART_APPROVED=false`, `CALIBRATION_VALIDATED=false`, `BLENDER_EDITS=0`, `MODEL_WEIGHTS_TRAINED=false`.

La misión de recuperación operativa y comparación diagnóstica puede cerrarse con estas evidencias y límites. El siguiente trabajo artístico exige nueva autorización de alcance que levante su pausa; no se reinicia automáticamente Bastión I, 4X ni la torre. El bootstrap existente de main debe recuperar este resultado y no seguir enviando a soporte como paso obligatorio del benchmark ya ejecutado.

### Recuperación final independiente y conciliación de instrucciones

La prueba sin historial recuperó correctamente run/job/artefacto completados, resultados 32/31/32, ausencia de superioridad frente a baseline y pausa artística. Detectó prosa antigua BLOCKED/RUNNING fuera del checkpoint: se corrigió antes del cierre, no se dejó como instrucción contradictoria.

Versiones operativas finales:
- Bootstrap main commit `afeddcebdc19488a5bdcd4789610f6b8988cce40`, blob `88b88e017bd439724b623cdba72fe35768e7802a`: procedimiento y checkpoint COMPLETED coherentes; causas históricas no se presentan como bloqueo vigente.
- AGENTS main commit `830f1672cd4febee67218d06a2194cc19a78cb98`, blob `fa8bcb93bda48628f9382fce3d9404c1e6f82e86`: distingue comparación diagnóstica de memoria completada y A/B artístico todavía ausente.
- Tras releer solo esos dos archivos, el revisor independiente confirmó que no persiste una obligación de acudir a soporte ni observar un run pendiente para el ensayo.
- Un proceso local nuevo del preflight volvió a recuperar COMPLETED, el siguiente paso de preservar pausa y los cuatro hashes de entrada coincidentes. No se repitieron inferencias ni pruebas de selector.
- Esto demuestra recuperación del estado desde contexto independiente en la infraestructura actual; no promete acceso permanente de Work ni salud Windows fuera del instante registrado.

**Entrega diagnóstica cerrada:** conectividad real y método de ejecución recuperados; comparación selectiva real completada y evaluada; respuestas preservadas en Git; continuidad reconciliada y comprobada. Se conserva íntegra la pausa de producción y el límite de no superioridad sobre el baseline.
