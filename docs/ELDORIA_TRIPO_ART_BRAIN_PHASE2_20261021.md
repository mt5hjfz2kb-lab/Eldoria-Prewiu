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

**Ejecución lanzada:** [run 39628068735](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39628068735). Estado en esta edición documental: EN CURSO; inspeccionar conclusión y evidencias antes de declarar verdadera dirección Qwen o progreso artístico.

## Limitaciones reales y siguiente gate

Una inferencia de Qwen solo demuestra que el modelo participó si existen registros genuinos de imágenes, prompt original, token/inferencia, plan y edición realizada. Un A/B Blender con variación visible tampoco certifica mejora global, estética, rig o rendimiento móvil. Valoria posee el renderer/escena: para integrar cualquier cambio habría que comprobar el mismo marco del mapa, la firma de jugabilidad y un móvil físico cuando corresponda, sin sobrescribir la versión actual.

**La evaluación de si el Engendro puede animarse no puede declararse positiva hasta obtener respuesta real de Rig Check** con credencial segura y 0 créditos verificados. Nunca se debe iniciar `/animations/rig` ni `/animations/retarget` para salvar un bloqueo; estas operaciones necesitan autorización de coste concreta.

## Medición y continuidad

Registrar la fuente SHA, identidad de Qwen, tiempo de inferencia, acciones permitidas realmente ejecutadas, bytes y conteo de malla, diferencia real de imágenes, observaciones del revisor independiente, créditos, móvil y estado de integración. Etiquetas separadas: SOURCE/TECH, QWEN_AUTHORED, BLENDER_REVIEW, UNITY_ART_APPROVED, MOBILE_PASS, RUNTIME_INTEGRATED.
