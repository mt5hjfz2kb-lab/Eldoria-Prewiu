# Aldric pasea por Valoria — candidato aislado

Objetivo: una instancia **visual** de Sir Aldric, sin crear otro héroe lógico y sin alterar partidas guardadas, combate, la UI o la cámara. Entra en la ciudad por la escena integrada, busca el MeshCollider real `Axial_Paving` del prefab `Paths` y solo camina cuando tres puntos y todos sus segmentos tienen calzada y espacio libre. En ausencia de una ruta validada no se crea el personaje.

Modelo autorizado obligatorio: `Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb`, SHA-256 `c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b`, 10.140.372 bytes, 26 joints, 57.332 triángulos, clips `Aldric_Idle_v1`, `Aldric_Walk_v1`, `Aldric_SwordAttack_v1`, `Aldric_Hurt_v1`, `Aldric_Defeat_v1`. La fuente exacta está recuperada en el paquete de entrega del propietario; **ya incorporado directamente a esta rama** tras reconstrucción exacta del artefacto Tripo y las recetas Blender/rig. El Git blob se ha validado como `5173328f4fa66cf1f050dba5a9000b44256ff086`. Si la carga o ruta falla, el actor no aparece: no se usa ningún sustituto.

El código usa glTFast 6.14.1 (ya instalado), una sola entidad visual, escala 1,85 m verificada contra bounds, pies a nivel de calzada, pausa y recorrido, ocultación según encuadre ortográfico (no distancia fija de 95 m), `SkinnedMeshRenderer.updateWhenOffscreen=false`. **Antes de importar verifica el SHA-256 y tamaño exactos del GLB cargado desde StreamingAssets**, y tras la importación exige 26 huesos y los cinco clips aprobados. Una identidad incorrecta o importación fallida aborta sin actor. Aldric desaparece de la ciudad al viajar a Mundo, salir en una marcha propia o investigar la fisura; reaparece al regresar. No altera `GameState`, `LocalGateway`, ni `ValoriaIntegratedWorld`.

Para promocionar a main: (1) terminar PR120, (2) el GLB exacto ya está versionado; no usar el modelo estático ni los PNG de diálogo; (3) ejecutar Unity nativo PlayMode/EditMode en el SHA exacto con capturas reales horizontal/vertical, probar `Axial_Paving`, obstrucciones, pausas y ausencia en misión; (4) inspeccionar escala, piel/capa animada y perfil Android. No declarar aprobado hasta esas pruebas. No publicar APK sin ellas.

## Entrega verificable y procedimientos

- Fuente binaria de la sesión: `Eldoria_Aldric_Modelo_animado_exacto_para_PR122.zip`, con la ruta de `Unity/Assets/StreamingAssets/Eldoria/Aldric/` preservada. Se generó fuera de GitHub; **incluida como GLB en esta PR**, reconstruida en la ejecución 40711186631. No marcar integración completa hasta verificar en el repositorio remoto.
- Después de trasladar la fuente al proyecto Unity, ejecutar `python3 tools/aldric-city/verify_approved_glb.py`: fallo (salida 2) si el archivo falta o difiere, éxito únicamente para el modelo original de cuerpo entero (GLB2, SHA, 26 articulaciones, 10 mallas y 5 clips).
- La rutina de calle valida MeshCollider `Axial_Paving`, pendientes y cada segmento a intervalos de 0,35 m; prueba opcionalmente una ruta más extensa de 4 puntos. Si no hay recorrido seguro, **Aldric no aparece** (no fingir que está funcionando).
- Revisar la orientación frontal del personaje: el código aplica una corrección preliminar de -90 grados; la orientación solo se aprueba tras grabar marcha real vista desde la cámara de ciudad.
- Comprobar horizontal/vertical y reposo/andar, ausencia durante viajes de Aldric y la investigación, guardar/cargar sin cambios, tiempo de carga y memoria/FPS reales en Android.
- El trabajo no incluye navegación NPC general ni generación de rutas sobre una ciudad remodelada. Tampoco modifica los edificios, la UI, PR 120, ni los guardados. `ART_APPROVED` y `APK_PUBLISHED` quedan pendientes.

**Estado del corte actual:** código en PR 122 + verificador fuente preparados, fuente auténtica empaquetada, comprobación local del modelo posible. **No hay PlayMode ni Android físico certificado y el binario está versionado en esta PR. Falta certificar la escena real en Unity.**


## Actualización verificada — 28/10/2026

**Transferencia del modelo resuelta.** GitHub Actions [40711186631](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40711186631) reconstruyó la geometría aprobada y los cinco clips desde el [artefacto original Tripo](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40527678020), la receta del art pass azul y los cuatro fragmentos SHA-fijados del rig. Coste añadido: 0 €. Comprobaciones SHA-256 exactas PASS.

- El GLB original de 10.140.372 bytes ya está en `Unity/Assets/StreamingAssets/Eldoria/Aldric/`, con GUID estables.
- El QA de fuente [40712097411](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40712097411) pasó; **no prueba Unity render ni rendimiento móvil**.
- Test nativo listo: `Unity/Assets/Eldoria/Tests/PlayMode/AldricCityRoamingNativeTests.cs`; workflow `.github/workflows/aldric-city-native.yml`: prueba real de escalado, paseo y 4 PNG de cámara real (paisaje/retrato, andando/parado).
- Dependencia vigente: PR #120 ocupa `windows-runner-heavy`. No iniciar Unity ni fusionar hasta que termine y se libere esa reserva.
- Estado: **MODEL_IN_GITHUB=PASS / SOURCE_QA=PASS / REAL_UNITY_PLAYMODE=PENDING / MOBILE_QA=PENDING / MAIN_NOT_MERGED**.

## Corrección de integración posterior al QA de fuente

- Se comprobó el `PlayerCamera` auténtico en `Valoria_Reference_Clean.unity`: origen aproximado (46, 77, -104). La distancia a la plaza de Valoria supera 95 m, **aunque la plaza está en su encuadre**. Un culling basado en distancia habría ocultado a Aldric. Reparado con `Camera.WorldToViewportPoint`, comprobando profundidad y un margen de 5 % de pantalla, sin alterar la cámara real.
- El componente se adhiere al inicio y al volver a cargar la escena integrada (`SceneManager.sceneLoaded`) para que las pruebas y el reinicio no eliminen al caminante. Se evita duplicar el componente.
- La consulta de guardado para saber si Aldric está en marcha o investigando se limita a una cada 0,2 s; el componente solo consulta el mismo gateway sin escribirlo.
- El intento histórico de recuperación del modelo usando la máquina Windows no encontró el archivo. **La reconstrucción posterior en GitHub sí funcionó**, por lo que se retiró ese workflow obsoleto.
- Los tests de fuente en Ubuntu siguen siendo **no-Unity**; la prueba nativa de paseo y las cuatro capturas están preparadas, no ejecutadas. No marcar `PLAYMODE_PASS` ni `ART_APPROVED` por esos tests.

## Continuación de fuente — 28/10/2026

El mismo encargo canónico se reclama en main como `eldoria-aldric-city-roaming-20261028`, sin reserva de Unity mientras PR120 siga validándose. Revisión independiente encontró una colisión C# real: `Application.streamingAssetsPath` se resuelve al namespace hermano `Eldoria.Application`. Un ensayo mínimo con el compilador Roslyn confirmó CS0234. Se utiliza explícitamente `UnityEngine.Application.streamingAssetsPath`. Las esperas de inicio/importación/prueba pasan de contar fotogramas a plazos de tiempo real: un equipo que renderiza rápidamente no debe agotar la espera antes de cargar la escena o el GLB.

El binario aprobado se ha vuelto a leer desde el objeto Git y coincide en tamaño10.140.372, SHA256c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b,26articulaciones y los5clips originales. No hay nueva generación ni gasto. Esta verificación de fuente no es una prueba de Unity ni permite declarar aceptación visual. Las pruebas nativas siguen pendientes hasta acabar PR120.


## Fuente preparada mientras termina PR120

La reserva Windows sigue siendo exclusivamente de PR120; no se ha iniciado Unity122.
El caminante ahora comprueba el volumen completo con OverlapCapsuleNonAlloc contra
los colliders actuales, en lugar de tres puntos de una lista tomada al arrancar.
Un búfer lleno bloquea el paso. La prueba nativa añade una obstrucción física
estrecha después de validar la ruta, verifica que el paseo no modifica el estado,
y ejecuta navegación a Mundo, marcha real de recolección y la investigación real,
con ocultación/reaparición y recarga persistida. Solo se modifica una partida
VALORIA_GAMEPLAY_PROBE comprobada. Ocho capturas auténticas previstas, sin contar
ninguna como existente antes de ejecutar Unity. El workflow prepara los mismos
recursos canónicos III y restaura Search.index de forma reversible.
Referencia de API: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.OverlapCapsuleNonAlloc.html
Estas son correcciones de fuente, no aceptación visual ni certificación móvil.

La prueba preparada también exige deformación efectiva de los26huesos entre
fotogramas durante Walk y materiales soportados sin shader de error. Cuatro
capturas diagnósticas desde ángulos opuestos para cara/capa se suman a las ocho de cámara
jugable/ausencia/retorno. La cámara de producción se conserva intacta; las
diagnósticas son exclusivamente del probe y no sustituyen las vistas jugables.
