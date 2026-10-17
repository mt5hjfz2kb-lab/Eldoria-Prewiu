# Valoria — relevo Work, 17-10-2026

Misma rama `construction/valoria-master-plan-layers-20261016`, mismo workstream. HEAD inicial comprobado: `52b03eeb771ed9afbc131f254a96b7793f106ed7`. Relevo explícito del propietario; claim `chatgpt-work-valoria-takeover-20261017` registrado en main y rama. Otros trabajos pausados preservados. Cero gasto, ninguna publicación ni cambio de escenas canónicas.

## Evidencia ejecutada

| Hito | Ejecución real | Resultado limitado |
| --- | --- | --- |
| CS0234 y macro SHARP | 39024979262 | TECH_PASS: Unity compila tras cualificar dos `UnityEngine.Application.dataPath`; 250 ensayos conservados. |
| Recalibración física aislada | 39025768860 | 22 huellas, 12 escalones originales; 28 contactos de circulación y barrido peatonal. Siete contactos visibles. |
| Blender original y fuentes de entrada | 39026138982 | Blender 5.2.2 LTS real. V1 rechazado: doble origen enterraba puerta. |
| Puerta normalizada y recursos CC0 recuperados | 39026741735 | V2: mejora local de puerta; terreno y repeticiones rechazados. |
| Superficie geográfica continua | 39027055209 | V3: elimina lunares del césped; acantilados planos y fragmentos incompatibles, ART_REJECTED. |
| Taludes esculpidos | 39027544457 | V4: importación y contactos pasan; facetas de cuadrícula y roca incompatible, sin aprobación artística. |
| Interfaz y progreso existentes | 39027773298 | Prueba PlayMode acotada: selección real `SlicePresenter`, aserradero y cuartel por botón, Bastión I→II y recarga exacta `FileStateStore`, revisión 13. |

Errores nuevos del adaptador experimental (revisión long y módulo ScreenCapture ausente) corregidos en `ceb10b4001643c3a95db1e6e0507cf3112b98853`; segundo run jugable satisfactorio. No se modifican reglas, costes, core ni guardado canónico. QA adelanta únicamente el reloj mediante `IClock` existente. La captura PlayMode muestra geometría real; no incluye el HUD Overlay.

## Cámara y proporciones

Se conserva `ValoriaParcelPresentation`, perspectiva, HOME 44.4228096°. El antiguo gate ocupaba 67,822 % de anchura frente a 17,7734 % en referencia. La escena espacial V1 alcanza 19,334 % anchura / 16,312 % altura, antes de recuperar la familia de puerta alta en V2. No declarar ajuste final del gate V5 sin medirlo. Conversión rígida a mundo Y-up con error de proyección máximo 0,000165 píxeles; no modifica el FOV ni el controlador canónico. El puente de cámara verifica proyección y rayos de la propiedad realmente consumida por `SlicePresenter`. HOME, pan, zoom y orientación vertical tienen capturas y pruebas. Cinco objetivos visibles en vertical; no confundir con siete visibles en horizontal.

## Fuentes recuperables

Cada `entry-zone-vN/` contiene BLEND editable, GLB, SHA de fuentes/resultados, versión Blender, captura BEFORE/AFTER y registro Unity. Las escenas y recursos importados están en `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/`. La escena jugable guardada es `Valoria_SHARP_Entry_PlayableV1.unity`. Fuentes históricas y candidatos anteriores permanecen intactos. Los assets 63 se inspeccionan/copían desde evidencias sin reactivar ni editar su rama. No se usa el pavimento con licencia pendiente.

Ollama respondió y se inventariaron los modelos instalados; Qwen 4B produjo una inferencia real de autoría auxiliar, rechazada por formato de acciones. No se ejecutó ese plan ni se fingió autoría Qwen. Las decisiones geométricas posteriores son de GPT de esta sesión. Presupuesto 0 €; no se descargaron pesos nuevos.

## Estado y continuación exacta

V5 está ejecutándose en `39028046271`: suavizado correcto de malla, mezcla de roca por pendiente, PBR CC0 y desplazamiento real, pavimento y doce pieles de peldaño modelados con bisel. Se conserva la cota física. Tras Unity se solicita una crítica visual local independiente de la geometría GPT; su salida tampoco constituye aprobación automática.

Siguiente acción: inspeccionar los PNG V5 auténticos, el informe fuente/Unity y la crítica; medir nuevamente envolvente de puerta/murallas y validar circulación sobre la geometría conservada. Integrar únicamente mejoras visibles en otra copia jugable, repetir prueba existente. La parte superior sigue siendo blockout; no presentarla como Bastión terminado.

**FULL_FRAME_ART_PASS=false. MOBILE_PERFORMANCE_PASS=no probado en dispositivos físicos. GAMEPLAY_PASS solo para el alcance acotado descrito; entrenamiento, viaje/retorno, misión completa, reset/reload de escena y QA de dispositivo siguen pendientes.**
