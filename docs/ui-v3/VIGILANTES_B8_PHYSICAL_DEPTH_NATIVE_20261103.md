# Vigilantes B8 — el camino es un sistema, no una fotografía
Fecha: 2026-11-03

## Petición del propietario
Última pasada artística del Camino de los Vigilantes. Debe dejar de leerse como fotografía pegada con indicadores superpuestos, sin mover al Engendro del **bloque de encuentro** ni perder los 25 hitos, lógica, premios, marcha, raycasts, guardado y modelo aprobado.

## Hechos contrastados y aprendizaje
- B5 (SHA `3d4786bc8d3707f7b67679be5234512639cb59cc`, Unity run 41648354560) logró guardianes alfa independientes pero el camino seguía plano. La figura del Engendro fuera de la tarjeta fue rechazada.
- B7 (SHA `111b5661dc5453260907a608e041b259217b2073`, run 41663824030) devuelve Engendro a tarjeta, pone luz en antorchas e hito actual y corrige textos. Unity nativa: SUCCESS.
- La auditoría artística del B7 detecta que una imagen pictórica rica todavía ocupa el 56% de la pantalla como fotografía de base. Iluminación sola es insuficiente: los 25 hitos permanecían casi idénticos a iconos sobre la foto.
- [Unity UI Optimization Tips](https://unity.com/how-to/unity-ui-optimization-tips): evitar dirty/layout/mesh updates innecesarias; no crear otra cámara/Canvas cuando una capa estática en el mismo padre basta.

## Cambio B8 sobre B7
- `RoyalGoldV3VigilantsRoadFoundation` crea el relieve pétreo y las sombras de contacto del camino de 25 puntos; **nunca** genera un segundo camino ni inventa posiciones.
- Para cada segmento real: hendidura proyectada, lateral de piedra, borde dorado de sol y luz de progreso basado en `LastClearedStage`.
- Para los 25 puntos reales: base de roca irregular con elevación, sombra sobre terreno y rebote dorado en el hito próximo. En 5/10/15/20/25 la roca tiene contrafuertes de relieve, sin sustitución del sello/guardian badge ya existente.
- Todas las posiciones provienen de `chapterTrail.StagePosition(n)` reproyectadas al mismo padre de escena; usan el `uvRect` auténtico y respetan la adaptación móvil.
- La malla es ESTÁTICA respecto del progreso, posición y relación de aspecto: solo `SetVerticesDirty` cuando cambia alguno. Los demás reflejos/luces siguen en su capa existente. Sin texturas nuevas, shaders adicionales, nuevos botones, hitos ficticios, recompensas ni sistemas de juego.

## Acceptance gates
- Comparar capturas reales del mismo estado B7 vs B8 en stage 1, guardianes 5/10/15/20/25, victoria/derrota, 1600×900, 900×1600, 390×844 y stage 15 844×390; no presentar montajes como Unity.
- Inspeccionar que el relieve se asiente sobre el terreno, que las 25 bases no parezcan una nueva cadena de iconos flotantes, que la tarjeta permanezca integrada y el Engendro no vuelva al mundo.
- EditMode `RealRoadStoneFoundationFollowsEveryActualStageAndNoTouch` y pantalla nativa `EarlyRetentionNativeCapture` comprueban 25 puntos + progreso real/25, persistencia y ausencia de raycast, foto/Canvas nuevos.
- Probar V4.1, marchas, retención, EditMode/PlayMode en Unity 6000.3.23f1, imagen A/B y perfil de rendimiento. No afirmar ≥8,3 ni FPS medidos hasta pruebas. No hacer merge a main ni publicar.

Estado: fuente B8 candidata en PR canónica #136; pendiente de Unity exact SHA, revisión visual y decisión artística.
