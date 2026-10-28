# Aldric pasea por Valoria — candidato aislado

Objetivo: una instancia **visual** de Sir Aldric, sin crear otro héroe lógico y sin alterar partidas guardadas, combate, la UI o la cámara. Entra en la ciudad por la escena integrada, busca el MeshCollider real `Axial_Paving` del prefab `Paths` y solo camina cuando tres puntos y todos sus segmentos tienen calzada y espacio libre. En ausencia de una ruta validada no se crea el personaje.

Modelo autorizado obligatorio: `Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb`, SHA-256 `c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b`, 10.140.372 bytes, 26 joints, 57.332 triángulos, clips `Aldric_Idle_v1`, `Aldric_Walk_v1`, `Aldric_SwordAttack_v1`, `Aldric_Hurt_v1`, `Aldric_Defeat_v1`. La fuente exacta está recuperada en el paquete de entrega del propietario; **aún no se ha transferido como binario a este commit**, por lo que la funcionalidad permanece protegida (no aparece ningún sustituto falso).

El código usa glTFast 6.14.1 (ya instalado), una sola entidad visual, escala 1,85 m verificada contra bounds, pies a nivel de calzada, pausa y recorrido, 95 m de culling, `SkinnedMeshRenderer.updateWhenOffscreen=false`. **Antes de importar verifica el SHA-256 y tamaño exactos del GLB cargado desde StreamingAssets**, y tras la importación exige 26 huesos y los cinco clips aprobados. Una identidad incorrecta o importación fallida aborta sin actor. Aldric desaparece de la ciudad al viajar a Mundo, salir en una marcha propia o investigar la fisura; reaparece al regresar. No altera `GameState`, `LocalGateway`, ni `ValoriaIntegratedWorld`.

Para promocionar a main: (1) terminar PR120, (2) importar el GLB exacto del paquete al path indicado; no usar el modelo estático ni los PNG de diálogo; (3) ejecutar Unity nativo PlayMode/EditMode en el SHA exacto con capturas reales horizontal/vertical, probar `Axial_Paving`, obstrucciones, pausas y ausencia en misión; (4) inspeccionar escala, piel/capa animada y perfil Android. No declarar aprobado hasta esas pruebas. No publicar APK sin ellas.

## Entrega verificable y procedimientos

- Fuente binaria de la sesión: `Eldoria_Aldric_Modelo_animado_exacto_para_PR122.zip`, con la ruta de `Unity/Assets/StreamingAssets/Eldoria/Aldric/` preservada. Se generó fuera de GitHub; **no está todavía incluida en esta PR**. No marcar integración completa hasta verificar en el repositorio remoto.
- Después de trasladar la fuente al proyecto Unity, ejecutar `python3 tools/aldric-city/verify_approved_glb.py`: fallo (salida 2) si el archivo falta o difiere, éxito únicamente para el modelo original de cuerpo entero (GLB2, SHA, 26 articulaciones, 10 mallas y 5 clips).
- La rutina de calle valida MeshCollider `Axial_Paving`, pendientes y cada segmento a intervalos de 0,35 m; prueba opcionalmente una ruta más extensa de 4 puntos. Si no hay recorrido seguro, **Aldric no aparece** (no fingir que está funcionando).
- Revisar la orientación frontal del personaje: el código aplica una corrección preliminar de -90 grados; la orientación solo se aprueba tras grabar marcha real vista desde la cámara de ciudad.
- Comprobar horizontal/vertical y reposo/andar, ausencia durante viajes de Aldric y la investigación, guardar/cargar sin cambios, tiempo de carga y memoria/FPS reales en Android.
- El trabajo no incluye navegación NPC general ni generación de rutas sobre una ciudad remodelada. Tampoco modifica los edificios, la UI, PR 120, ni los guardados. `ART_APPROVED` y `APK_PUBLISHED` quedan pendientes.

**Estado del corte actual:** código en PR 122 + verificador fuente preparados, fuente auténtica empaquetada, comprobación local del modelo posible. **No hay PlayMode ni Android físico certificado y falta importar el binario en el repositorio.**
