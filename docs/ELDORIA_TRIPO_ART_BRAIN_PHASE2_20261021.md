# ELDORIA — Tripo API + cerebro artístico | Fase 2 de ejecución aislada

Fecha: 21-10-2026. Rama canónica: main. Workstream activo: `eldoria-tripo-art-brain-phase2-20261021`. Gasto permitido: **0 créditos de pago y 0 €**. No sustituir Qwen, Blender ni Unity actuales, ni alterar el juego.

## Objetivo y fronteras

Continuar el trabajo cerrado en [preflight Tripo art-brain](ELDORIA_TRIPO_ART_BRAIN_INTEGRATION_20261021.md), llevando el Engendro existente de fuente autenticada a un **plan de mejora 3D realmente decidido por Qwen** sobre una copia Blender. La escena actual ya contiene el Engendro de Bastión II, con LOD cercano (~12.040 tris) y lejano (2.900); los encuentros, estado `EngendroDefeated`, guardados, colisiones, marchas y UI no se tocan.

El workstream Valoria actual conserva `windows-runner-heavy` y la escena de juego; esta línea solo usa Linux GitHub-hosted y material archivado. No declara rigging, ataques animados ni calidad aprobada por una prueba estructural.

## Trabajo real demostrado

- `author_brief.py` del **mismo** Qwen/Ollama añade `--subject-kind creature|mineral_prop`; `terrain` sigue siendo el comportamiento por defecto sin alterar los prompts históricos. El modo criatura habla de anatomía, cabeza, extremidades, contacto y legibilidad, en lugar de escribir instrucciones de acantilado.
- La lógica `--cpu-compact` no impone a criaturas el diámetro mínimo de 1 metro ni la deformación de 0,2 metros que se diseñaron exclusivamente para acantilados de Valoria.
- [Contrato de imágenes auténticas y entrada Qwen probado](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39625619119): las fuentes GLB originales, los SHA, las fotos Unity reales y el inventario alcanzan la interfaz `--inventory` del autor. Esta prueba usa un servidor **Ollama simulado** para validar el transporte: no es inferencia verdadera ni obra mejorada.
- [Inspección verdadera de fuente Blender y Qwen con coordenadas auténticas](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39626965531): mismo `execute_artist_plan.py --inspect` que el artista, objeto `Spawnling_Near_Authored`, SHA de `Spawnling_B2_Finished.blend`, datos de topología y puntos reales, y prueba de que el contexto puede llegar al artista (respuesta simulada). No hay modificación de Blender en este gate.
- [Rig Check gratuito, intento de conexión](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39626248664): fuente y tests PASS; el secreto `TRIPO_API_KEY` **no existe en GitHub Actions hospedado**. Resultado exacto `BLOCKED_GITHUB_ACTIONS_API_KEY_ABSENT`. **0 llamadas API, 0 créditos; riggable DESCONOCIDO.** La credencial usada en generaciones anteriores estaba en la sesión Tripo CLI del runner Windows, que está reservado para otra línea. No extraer ni trasladar credenciales a texto o Git por comodidad.
- La documentación oficial de Tripo confirma [Rig Check gratis](https://developers.tripo3d.ai/en) y [endpoint v3 de riggabilidad](https://developers.tripo3d.ai/en/docs/animations-rig-check). Autorig a partir de 25 créditos y retarget de animaciones aparte: **no permitidos en este trabajo**.

## Ensayo de Qwen y edición Blender de verdad

Workflow exclusivo, sin modificar el juego: [`.github/workflows/tripo-art-brain-real-creature-pilot.yml`](../.github/workflows/tripo-art-brain-real-creature-pilot.yml).

- Usa el `.blend` ya acabado en Bastión II con hash original, y no genera otro modelo con Tripo.
- Recupera fotografías realmente capturadas de Unity; las copias de 640 px derivadas para CPU guardan SHA de origen y destino.
- Inspecciona `Spawnling_Near_Authored` mediante la herramienta existente, pasando a Qwen coordenadas/valores reales, lecciones del diario artístico y el inventario del activo, sin repetir estrategias ficticias.
- Levanta Ollama local con Qwen3-VL-2B en un runner Linux aislado, realiza **inferencia verdadera** y exige un plan de Blender tipado. Su ejecución solo modifica un archivo nuevo temporal, protegido por SHA de entrada.
- Intenta producir un A/B auténtico de Blender clay a misma cámara. Se exige diferencia real para comprobar visibilidad, pero **no es ART_APPROVED ni un A/B Unity**.
- Todo resultado se conserva en artifacts con el run ID; no se fusiona geometría automáticamente, no hay cambios jugables.

### Primer ciclo Qwen real: ejecutado, pero ART REJECTED

[Run 39628068735](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39628068735) completó REAL Qwen 2B (346,211 s, 312 tokens), plan tipado, edición Blender, GLB/BLEND temporales y auténticas imágenes comparables. El éxito técnico fue **insuficiente**: el plan desplazó los 9.753 vértices con `sculpt_region center=[0,0,0], radius=10.24, delta=[0.5,0.5,0.5]`. La fuente mide solo 1,469 m de diagonal. El A/B cambió 2,8024 % de píxeles, pero el centro de la malla se movió 0,8607 m, y sus tres dimensiones cambiaron menos de 0,3 %. **Veredicto independiente: REJECT_NOT_MEANINGFUL_CREATURE_SHAPE; no mejora anatómica real, no art-approved ni integración.**

Recibo reproducible: [pipeline/tripo-art-brain/verified-failures/spawnling-qwen2b-global-move-20261021.json](../pipeline/tripo-art-brain/verified-failures/spawnling-qwen2b-global-move-20261021.json). Los originales y el Engendro de producción siguen intactos.

### Corrección permanente y reensayo

- `author_brief.py` ahora lee las dimensiones reales del Engendro y rechaza radios/movimientos globales desproporcionados **antes de ejecutar Blender**; devuelve a Qwen una corrección apropiada para criatura, sin recomendaciones de acantilado.
- `tools/tripo-art-brain/shape-change-gate.mjs` detecta desplazamiento dominante de figura con dimensiones casi invariantes aun si los píxeles cambian. El gate nunca concede `ART_APPROVED`.
- `preflight.mjs` alimenta la siguiente solicitud Qwen con el error verdadero del run anterior como memoria específica de esta fuente, y verifica su relación con el `.blend` original.
- [Prueba real de fuente + simulación controlada](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39631782575) cerró satisfactoriamente sin llamar al Qwen real: protege la ruta de entrada y los límites, preserva el modo terreno anterior.
- **Segundo ensayo Qwen real — RECHAZADO:** [run 39631639948](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39631639948) ejecutó dos inferencias verdaderas (361 y 404 tokens). La segunda propuso un `sculpt_region` con desplazamiento nulo o inferior al mínimo de 0,005 metros; el validador lo rechazó ANTES de Blender. No hubo edición de malla, captura A/B nueva, integración ni gasto. Se preservó el artifact `12360077106`, y el hecho queda documentado en `pipeline/tripo-art-brain/verified-failures/spawnling-qwen2b-zero-delta-20261021.json`.
- **Tercera hipótesis, no repetición ciega:** se modificó la dirección específica para criatura: cabeza orientada a +X según fuente previa, escala real desde la malla comprobada, radio y movimiento de una región anatómica localizada, y exigencia inequívoca de delta NO NULO. El inventario Qwen antepone ambos rechazos y cabe en 3.200 caracteres; no oculta el segundo error por truncamiento. La prueba se ejecuta solo contra la copia aislada, y no concede aprobación artística por sí sola.

## Limitaciones reales y siguiente gate

Una inferencia de Qwen solo demuestra que el modelo participó si existen registros genuinos de imágenes, prompt original, token/inferencia, plan y edición realizada. Un A/B Blender con variación visible tampoco certifica mejora global, estética, rig o rendimiento móvil. Valoria posee el renderer/escena: para integrar cualquier cambio habría que comprobar el mismo marco del mapa, la firma de jugabilidad y un móvil físico cuando corresponda, sin sobrescribir la versión actual.

**La evaluación de si el Engendro puede animarse no puede declararse positiva hasta obtener respuesta real de Rig Check** con credencial segura y 0 créditos verificados. Nunca se debe iniciar `/animations/rig` ni `/animations/retarget` para salvar un bloqueo; estas operaciones necesitan autorización de coste concreta.

## Medición y continuidad

Registrar la fuente SHA, identidad de Qwen, tiempo de inferencia, acciones permitidas realmente ejecutadas, bytes y conteo de malla, diferencia real de imágenes, observaciones del revisor independiente, créditos, móvil y estado de integración. Etiquetas separadas: SOURCE/TECH, QWEN_AUTHORED, BLENDER_REVIEW, UNITY_ART_APPROVED, MOBILE_PASS, RUNTIME_INTEGRATED.

## Revisión independiente del tercer Qwen real: descartado, no reinferido

- [Run real 39636058045](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39636058045): Qwen3-VL-2B ejecutó un plan tipado y Blender modificó 206/9753 vértices; centro [0,0,0], radio 0,088 m, desplazamiento Z 0,015 m. El A/B estratégico apenas cambió 2/307200 píxeles. No se tocó el juego.
- [Replay real 39639377365 — SUCCESS técnico](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39639377365): se recuperó por SHA el mismo candidato sin inferencia ni escultor nuevos. En cinco pares Blender verdaderos: cámara isométrica general 2 píxeles, acercamiento isométrico 24, cara cercana 485, lateral 158, posterior 90, sobre 307200 por vista. Solo la cara llega al 0,1579 %, sin mejora anatómica significativa. Veredicto independiente ART_REJECTED_NO_MEANINGFUL_VISUAL_UPLIFT, NO ART_APPROVED. Artifact 12363503143, ver aprendizaje source-locked en pipeline/tripo-art-brain/verified-failures/spawnling-qwen2b-invisible-local-sculpt-20261021.json.
- **Cambio real de hipótesis:** tools/tripo-art-brain/creature-anatomy-context.py deriva vértices faciales candidatos de la fuente Blender, basados en la orientación facial verificada +X. El mismo Qwen recibe esta región mediante --mesh-context; author_brief.py --anatomy-focus face impide aceptar otro origen incorrecto o desplazamientos diminutos. El modo terreno no cambia.
- [Qwen3-VL-4B real 39641404559](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39641404559): la inspección nueva fue auténtica y halló **2350 vértices en una región frontal +X** (centro aproximado [0.328358,-0.002131,0.198458] m) con SHA exacto de la fuente. El modelo 4B en CPU GitHub-hosted **agotó 540 segundos sin devolver plan**, así que no hubo edición ni gasto. Evidencia conservada en artifact **12366431017**. Es un fallo de capacidad/latencia de ese entorno, NO un defecto de Blender ni prueba de que 4B diseñe peor. Se conserva el mismo contexto y se ensaya la alternativa disponible de Qwen 2B con filtro facial nuevo en [run 39643146556](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39643146556). La nueva ejecución está pendiente de evaluación independiente; no repetir el 4B hosteado a ciegas.

El objetivo es una corrección anatómica visible. Si este enfoque falla, revisar estrategia y herramientas del artista, no repetir infinitamente prompts ni gastar generaciones Tripo. Rig Check real sigue pendiente por credencial no disponible en Actions, y rigging/animación de pago NO están autorizados.
