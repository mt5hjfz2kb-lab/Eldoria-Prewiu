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

## Estado a esta revisión
- **Blender + GLB fuente 3D real**: HECHO, revisado visualmente, con defectos iniciales corregidos.
- **Código Unity para integración exclusiva de la criatura**: PREPARADO en PR #84; fuente y materiales ajustados.
- **Prueba nativa de Unity**: proceso original [run 39568710360](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39568710360) iniciado, resultado **PENDIENTE**. Se añadió un parche aislado contra el conocido fallo de `UnityEditor.Search` a la siguiente ejecución; no reescribir el Unity del trabajo de interfaz V3.
- **Main / versión jugable pública**: NO integrado ni publicado al redactar este documento. No existe evidencia de funcionamiento móvil físico; jamás fingirlo.

Esta declaración es un registro de producción, no una aprobación de producto. Debe actualizarse con ejecución, fuente SHA, capturas y veredictos reales cuando termine la integración.
