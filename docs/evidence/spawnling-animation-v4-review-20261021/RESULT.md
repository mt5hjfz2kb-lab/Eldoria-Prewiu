# Eldoria — Engendro: revisión independiente de rig y zarpazo V4

**Fecha:** 2026-10-21. **Origen:** [Unity no ejecutado, rig Blender V4 CI PASS #39651422644](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39651422644), archivo original de Actions `spawnling-expressive-rig-v4-zero-credit-proof`, artifact ID `12368972822`, 20.493.621 bytes.

## Revisión REAL de bytes originales del artifact (no generados de nuevo)

El revisor descargó el ZIP original de GitHub Actions, extrajo GLB y los 14 PNG Blender y comparó la pareja V3/V4 de **siete fotogramas idénticos** (1, 10, 19, 27, 34, 46, 65) a 600×480, misma cámara. Se inspeccionó el chunk glTF del GLB V4 y se decodificaron accessors de skin/animación, no solo la afirmación del script productor.

**Fuente V4:** `spawnling-rig-expressive-swipe-v4.glb`, SHA-256 `86ec1c93a222d58b89b9f393d39c8e5e912b14516ff5298500e5aca80048f631`; `.blend` SHA-256 `6dd0bff3e226b443cc5dcf356fac3172b84fe16066a86721c0a75a4d23efe716`; fuente congelada del Engendro, SHA-256 `4b956ec175f7fef198aef51b2b04faceeb772f080d273cdd80155beb4b0d717d`.

**GLB inspeccionado:** 3 primitivas de malla, 1 skin, 12 joints reales (root, spine, head, brazos/antebrazos, piernas/espinillas, cola), 1 `Eldoria_Spawnling_ExpressiveClawSwipe_V4` y 36 canales de animación. Se verificó `JOINTS_0`/`WEIGHTS_0`, UV0, NORMAL en las tres primitivas y error máximo observado de suma de pesos de 0,00000012. El clip tiene canales de rotación muestreados cada 1/24 s desde `t=0,04167` hasta `t=2,70833` segundos. El intervalo de inicio no es cero; es algo a observar en Unity y normalizar solo si el ensayo importador lo requiere.

| Fotograma | Píxeles V3 frente a V4 con diferencia RGB máxima mayor de 30 / 288.000 |
|---|---:|
| 1 (reposo) | 0 |
| 10 (alerta) | 373 |
| 19 (preparación) | 5.838 |
| 27 (zarpazo) | 10.657 |
| 34 (recuperación) | 5.007 |
| 46 (vuelta) | 350 |
| 65 (reposo) | 0 |

**Revisión visual:** el V4 diferencia de forma perceptible la posición y plegado del brazo durante la preparación y el zarpazo con respecto a V3, sin cambiar el reposo; el movimiento es legible en primer plano de Blender, aunque discreto en encuadre amplio. La articulación y los pesos son aproximados; no se certifica impacto, caminar, deformación facial, contacto sobre la geografía real ni ausencia de clipping a todos los ángulos. **Veredicto: `ANIMATION_SOURCE_REVIEW_PASS_FOR_ISOLATED_UNITY_IMPORT`**, no `ART_APPROVED`.

## Siguiente frontera técnica (sin modificar el juego)

Verificar **en el Unity 6000.3.23f1 real**, mediante importación del mismo GLB exacto en un proyecto QA aislado: `SkinnedMeshRenderer`, `bones`/bindposes, clip real importado, `AnimationClip.SampleAnimation` con deformación física distinta entre reposo/ataque/retorno, texturas BaseColor reales, y capturas de esa cámara Unity. Si Unity falla, diagnosticar sin lanzar otra Tripo API. El Windows runner continúa reservado a la interfaz V3; no ocuparlo antes del relevo operativo y de la autorización de recursos en el registro.

Solo después: escena estratégica real, timing del encuentro Bastión II, `EngendroDefeated`, raycasts, recompensas, guardados y perfiles móviles. **En este informe:** costes nuevos 0 créditos, Qwen no utilizado para diseñar el rig procedural, Unity import NO ejecutado, Android NO probado, integración NO realizada. El Engendro V3 estático en la escena principal permanece intacto.
