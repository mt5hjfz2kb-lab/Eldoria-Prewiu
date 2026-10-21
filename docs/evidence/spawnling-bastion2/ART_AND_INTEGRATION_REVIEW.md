# Eldoria — Engendro B2: arte actual, límites y prueba de juego
Fecha: 2026-10-21. Fuente del proyecto: `main` / escena canónica `Valoria_City_World_UI.unity`. Implementación candidata: [PR #84](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/84).

## Evaluación de calidad artística (NO FINAL)
La mejora observada frente al modelo Tripo de base es **moderada**, no una transformación de calidad radical.
- Puntos retenidos: anatomía monstruosa encorvada, garras, silueta reconocible y deformidades; identidad púrpura más oscura, cola trabajada, dos pupilas discretas ancladas al contorno corporal.
- Fallos evitados: la primera pasada colocó polígonos luminosos flotantes y oscureció en exceso la superficie; se recuperaron las capturas reales y se corrigieron ambos defectos. No repetir el «jaw bar» inventado.
- Límites todavía reales: rostro poco expresivo, sabor todavía reptiliano, sin rig, skinning ni animación de ataque, sin prueba física de batería/FPS. La respiración programada por transformación no es animación de esqueleto.
- Valoración visual provisional, basada en las vistas reales y NO en la cámara final: ~7/10 frente al ~6,5/10 original. **No afirmar 8/10 ni ART_APPROVED.**

## Identidad, coste y archivos reales
- Original pagado una vez: `ArtSource/World4X/Spawnling_v1/Optimized_Source.glb`, SHA-256 `2701e6f6a37920cbe6b434b903aa7b14283514f8276399d14bfffaece0edcc4a`. Los 20 créditos originales son gasto histórico; **esta mejora = 0 créditos, 0 €**.
- Fuente editable real: `ArtSource/World4X/Spawnling_v1/Finished/Spawnling_B2_Finished.blend`.
- GLB cercano: `Spawnling_B2_Near.glb` (~12.000 triángulos de cuerpo + dos ojos).
- GLB lejano: `Spawnling_B2_Far.glb` (~2.900 triángulos de cuerpo).
- Evidencia de autoría: `Finished/author-evidence.json` con SHA para los tres ficheros y cuatro PNG Blender. Blender 4.0.2 en GitHub Actions sin ninguna llamada a Tripo.
- Se conserva la textura/albedo original mediante materiales PBR; se reducen texturas máximas a 1024². Se comparten materiales entre LODs tras importar a Unity, sin duplicación por variación de distancia.
- Verificación real de Blender: [run 39565199683](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39565199683) SUCCESS. No mide arte en juego ni rendimiento.

## Contrato obligatorio de integración
1. Solo reemplazar la geometría provisional del destino **`engendro-valoria`**, en el mapa 4X de Bastión II. No inventar nidos, tiendas, campamentos ni otros enemigos.
2. `WorldHotspot` y su BoxCollider permanecen intactos; no añadir otra selección/collider ni modificar marchas, recompensas, objetivos, narrativa, guardados ni economía.
3. Guardar la escena canónica con meshes y materiales **persistentes** tras destruir el importador glTFast. No depender de importación en runtime desde el dispositivo.
4. El modelo debe ocupar una escala táctica legible, apoyar sus pies sobre el suelo autoral del mapa y mirar a la ciudad, manteniendo coordenadas del hotspot. LOD near/far sin duplicaciones.
5. Renderizar el A/B auténtico a 900×1600 y 1600×900 con el mismo `WorldCamera`, pose y luz. **Verificar fracción de píxeles cambiados >= 0,1%**, solo como umbral de visibilidad, no aprobación artística.
6. Ejecutar la prueba real I–II: gestos de navegación, UI, construcción, cuartel, confirmación de marcha, enfrentamiento Engendro, botín, recarga exacta y derrota persistente. El elemento visual sigue `EngendroDefeated` y desaparece tras la victoria.
7. Solo después de QA y coordinación del propietario de la escena integrada: PR -> `main`; las publicaciones APK/WebGL son otra fase y nunca están acreditadas por la mera importación Unity.

## Verificación nativa realizada y evaluación independiente (21-10-2026)
- **Blender y fuentes**: conservación completa del GLB pagado originalmente, `.blend` editable, 12.040 triángulos cercanos (cuerpo y detalles) y 2.900 lejanos; cero créditos adicionales.
- **Unity real**: la ejecución [39578889927](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39578889927) superó la importación al proyecto Unity 6000.3.23f1, persistió materiales/mallas y guardó la escena. La ejecución global acabó en FAIL **después de los dos pasos técnicos aprobados**, por cambios no preparados para un `git rebase` en el entorno compartido; no fue fallo del modelo ni del gameplay.
- **QA I–II**: 40 comprobaciones reales finalizadas con estado `INTEGRATION_PASS_ART_REVIEW_REQUIRED`, error vacío, según `native-full-i-ii-result.json`. Se conservaron ID y coordenadas de 6 hotspots, 6 colliders existentes y **0 colliders nuevos**. `EngendroDefeated` gobierna el componente visual sin otro sistema de combate.
- **Persistencia verificada en rama**: la recuperación de la ejecución [39582719835](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39582719835) subió el commit `3b00c83e5bdadd817cec562e7a1e006dc537aa8c`, incluyendo la escena jugable, 2 LODs, materiales/UVs/PNG reales y evidencia de I–II. Esto **no** implica que se haya fusionado en `main`.
- **Identidad de escena**: antes `4c8bbdc6ccb1273bcf24c0e94fa102d88077faac71869598a0e215b88a1cb2bb`, después `4c39edeb8fa03f6b7235dc7bad0a041b06de9384b8e8bd6f1e3c23e8cf860237`. El GLB y LOD finales conservan los hashes de `native-integration.json`.
- **Inspección visual real**: `after-local-900x1600.png` muestra de forma clara al enemigo violeta y `after-region-1600x900.png` lo sitúa en el lugar correcto entre los árboles. El cambio regional es **0,2609 %** de píxeles, supera el umbral 0,1 % de mera visibilidad. El cambio local **86,59 % NO demuestra por sí mismo mejora artística**, porque el fotograma previo presenta un fondo/suelo mucho más oscuro: el A/B local no está perfectamente estabilizado. La aprobación artística formal sigue **pendiente**.
- **Juicio artístico**: sustitución sustancial frente a la silueta/provisional antigua, apta como criatura temprana en la cámara del mundo, pero no certificada como 8/10. Persiste rostro poco expresivo y ausencia de rig/ataques animados; no se han probado FPS ni termales en Android físico.

## Cierre de la misión — incorporado en main

**COMPLETADO COMO SUSTITUCIÓN VISUAL JUGABLE.** El [PR #84](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/84) se fusionó en la rama canónica `main` por squash SHA `56970cea8a8ae4f45540e708079912e4cb612a4c`. El juego ya contiene el Engendro 3D mejorado en su escena integrada real, con materiales y meshes persistentes, LODs y un único encuentro `engendro-valoria`. No se tocó el PR #83 de interfaz V3.

**Pruebas completadas:** 40 verificaciones genuinas de I–II/guardados/recompensas en Unity 6000.3.23f1 (run 39578889927), evidencia de presencia local y regional en capturas A/B; guardado de escena y verificación de archivo real en `main`. El run 39582719835 recuperó el mismo commit sin rehacer ni pagar modelos. Se verificó que la escena y los assets persistentes existen en `main`.

**Alcance separado:** no se ha publicado de forma explícita APK/WebGL en este trabajo; la disponibilidad para testers dependerá de la próxima compilación/publicación del canal correspondiente. No se ha demostrado rendimiento en Android físico, rig/animaciones de combate ni calidad 8/10. Estas limitaciones no invalidan la integración visual solicitada, pero no se deben presentar como tareas realizadas. Un CI global posterior a la fusión puede seguir validando el repositorio; si detecta una regresión atribuible a este cambio debe abrirse incidencia específica.

**Coste nuevo:** 0 €. **Estado de este workstream:** CERRADO; no dejar un procesador de recuperación en cola ni volver a gastar créditos de Tripo para este mismo modelo.
