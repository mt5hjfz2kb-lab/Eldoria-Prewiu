# Zona 1 — suelo, continuidad verificable

Rama: `prototype/world4x-zone1-zoom-reference-20261013`. No integrar en main ni modificar Bastión I. El mundo no está terminado; el terreno no está aprobado y no se permite multiplicar sectores.

## Producción real conservada

- Run `38558966835`: Qwen3-VL 4B recibió referencia, imágenes reales, topología y memoria de descartes. Su curva exacta de tres puntos se ejecutó en Blender: 2295 vértices modificados, parcela de ciudad, bordes y otras mallas conservados. No se reescribieron sus parámetros. Fuente y A/B: `evidence/relief-revision-38558966835/`.
- Familia exportada e instalada bajo Unity: `Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/Ground/ZoneOnePrairieCandidate.glb`, SHA256 `6751fbd60e8fce5122d5bd5aa1486aa68d2e8bf471cbab9c949f72a500a6d75a`. Una malla, 14641 vértices, 28800 triángulos. Estos datos no certifican FPS móvil ni navegación.
- Materiales gris-beige (run `38551351550`, ejecución preservada `38553648773`) y camuflaje verde (inferencia `38554125622`) rechazados tras renders efectivos; conservar la textura previa. Evidencia y decisiones en `GROUND_ART_REVIEW_20261014.json`.
- Diagnóstico adicional desde tres encuadres del mismo suelo: `evidence/strategic-ground-staging-20261014/`. Son imágenes **Blender**, no Unity ni equivalencia certificada. Persiste pradera uniforme, caminos pintados y poca lectura del relieve. No alcanza el objetivo artístico.
- El ejecutor compartido registra erróneamente `target: 1.0` por reutilización de variable interna en sculpt_path. Se conserva el registro bruto; plan, nombres y hashes independientes acreditan la malla realmente editada. No tocar el ejecutor de otro propietario para corregir esta incidencia.
- La memoria contiene experiencias verificadas recuperadas; no hubo entrenamiento de pesos.

## Paso nativo preparado, aún no ejecutado

`NATIVE_CAPTURE_REQUEST.json` fija ambos GLB exactos. Workflow `world4x-zone1-ground-native-capture.yml`: comprueba main vivo **antes de encolar Windows**, vuelve a comprobar la reserva inmediatamente antes de Unity y usa checkout separado por run. `CaptureGroundPair` prepara ocho PNG reales A/B global, regional, local y móvil; `BuildGroundAndCapture` guarda la escena y assets nativos aislados. El código C# aún no ha compilado en Unity.

Última comprobación: main `85c2dd9cf1f9cef504b6f47514135085539b3545` conserva `windows-runner-heavy` para `chat-normal-bastion1-fullframe-convergence-20261013`; Zona 1 no posee esa reserva. No tomarla aunque no haya jobs. No se encontró instalación/activación independiente de Unity local. Requiere una ventana nativa coordinada por el titular conforme a `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`, sección Shared runner rule. La solicitud guardada no otorga esa ventana.

Tras una cesión legítima: ejecutar workflow, inspeccionar el suelo desde URP real, corregir detalle, mezcla de materiales y lectura de pendientes; comparar desde las mismas cámaras. No añadir ciudad ni decoración para esconder el fallo del suelo. Solo tras calidad demostrada continuar familias/composición. No certificar 8/10, jugabilidad, móvil físico ni mundo completo sin pruebas.

Intento concreto `38563776671`, commit `f721031175536a9cacfd203cc8067ebc3f44f55e`: el guard consultó main vivo y devolvió `ZONE1_NATIVE_RESOURCE_BLOCKED`; job Windows `unity-ground` omitido, Unity no iniciado. Prueba de material posterior, independiente: `evidence/tiled-detail-rejected-20261014/`. Se reutilizó exclusivamente la escala 4m propuesta por Qwen, manteniendo textura donante RGB y UVs repetidas en lugar de atlas de región. Renders regional y retrato efectivos aún muestran una alfombra de césped; **VISUAL_REJECTED**, GLB aislado sin sustituir el candidato de Unity. No fue una nueva inferencia ni una aprobación nativa.

## Blender independiente

`tools/world4x-zone1/production/run_local_blender.sh` utiliza un Blender Ubuntu extraído en un directorio propio, sin instalarlo en el sistema. Indicar `ELDORIA_LOCAL_BLENDER_ROOT`; scripts usan `--work-dir` porque `--cycle` colisiona con argumentos de Cycles. El transporte utilizado es CLI directo, no BlenderMCP. Los workflows Linux conservan fuentes, inferencias y GLB sin créditos de pago.

## Continuación real: geometría estable y 4 encuadres — 2026-10-14

- Artista Qwen3-VL 4B real, fuente Blender con tres acciones elegidas por el modelo: +6 m colina, −2.5 m valle y +2 m conexión tipo collado. Ejecución `38580519176` SUCCESS; evidencia `evidence/general-ground-38580519176/`.
- La malla de origen fue protegida; 1957 vértices con variación vertical, 28.800 triángulos y la verificación independiente confirmó borde exterior, parcela edificable y XY intactos. Export GLB independiente `6c8331bd5118f0f14529580f5ac2be4ef87f1b969e1c840e5f401b04e9e460f8`. NO reemplazar la fuente/Unity original sin aprobación.
- A/B Blender a cámara de autoría invariable: 31.752 píxeles modificados / 800.000, 3,969 %. En el trabajo anterior eran 14.349 / 800.000. Esta medida acredita presencia en pantalla y magnitud relativa de edición, **no naturalidad ni calidad aprobada**.
- Captura multizoom posterior `38583442987` SUCCESS, `evidence/real-multizoom-38583442987/`: pares BEFORE/AFTER del MISMO 3D y la MISMA cámara en encuadres global, regional, local y vertical móvil (8 renders Blender), cámara y SHA por imagen en `zoom-review.json`. No son capturas Unity ni pruebas en dispositivo.
- Qwen evaluador afirmó genéricamente «no defects» sin señalar elementos de la referencia; **esa autocrítica no da un ART PASS**. La malla es candidato sujeto a inspección artística independiente. Los defectos anteriores de pradera uniforme y caminos pintados no se dan por subsanados solamente por los cambios geométricos.
- Intento previo de prueba de zoom `38583098270`: faltaba `numpy` en Blender Ubuntu; corregido en workflow y repetido con éxito, conservando fallo para trazabilidad.
- La reserva `windows-runner-heavy` sigue bajo Bastión I en main; no se inició Unity ni se cambió la escena de Región 1. No hay aprobación visual, integración en Unity, FPS móvil ni publicación. Cero servicios/créditos de pago.
