# Eldoria — Equipamiento, reconstrucción de arte (02-11-2026)

## Objetivo y autoridad
PR canónica #136. Nota inicial del propietario: Aldric 4,6/10 y Lyra 4,6/10. Meta: ≥8,0/10 **solo tras comparar capturas reales de Unity** con `docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png`. No certificar art_approved, ni publicar, ni introducir mecánicas inexistentes.

## Implementación fuente
- `Unity/Assets/Eldoria/HeroesUI/HeroesEquipmentArt.cs`: cuatro ranuras visuales bloqueadas (CABEZA, TORSO, ARMA, PIES) a ambos lados del mismo modelo 3D original del héroe, iconos de categoría dibujados como sprites independientes, filigrana/metal, candados y marcos reales nine-slice. Los nombres de categoría son **guías visuales, no contratos de inventario**.
- `HeroesApprovedDestination.cs`: enlaza las ranuras al escenario 3D existente y convierte el antiguo gran bloque informativo de Equipamiento en contexto por capas sin panel opaco. Conserva Aldric, Lyra, rotación, navegación, selector de retratos y su procedencia real.
- Contexto: título, cuatro posiciones bloqueadas y mensaje de indisponibilidad. En móvil corto se condensa la leyenda a cuatro sellos; en zonas amplias aparecen cuatro filas adicionales. Todos los textos son elementos nativos de Unity, no números o textos incrustados en imágenes.
- **No hay** objetos equipables, inventario nuevo, cambios de estadísticas, costes, materiales, mejoras, comandos, guardado ni eventos. Las ranuras carecen de `Button` y todas sus imágenes tienen `raycastTarget=false`.

## Certificación pendiente
La capturadora existente `EldoriaV41NativeCapture.Capture` ya recorre Aldric y Lyra en `1600×900`, `900×1600`, `390×844` y `844×390` por la pestaña Equipamiento. Se añadieron controles para verificar modelo verdadero, cuatro ranuras individuales, no interacción, candados y marcos nine-slice, límites del escenario y estado contextual. El mismo test comprueba guardado byte por byte antes/después de navegar todas las pantallas.

Debe ejecutarse Unity 6000.3.23f1 **con el HEAD exacto candidato** y reserva segura de Windows. Una vez exista PASS: examinar las ocho imágenes nuevas, compararlas contra el master aprobado, comprobar legibilidad, recortes y peso del personaje; corregir defectos y repetir regresión. Hasta entonces: **CÓDIGO INTEGRADO, COMPILACIÓN/ARTE/REGRESIÓN DEL NUEVO HEAD NO CERTIFICADOS**. No se modifica main ni se publica APK/WebGL.

## A/B auténtico — 02-11-2026, capturas inspeccionadas
- A: `389580f29d1a1433e9d382d24e96addd0280fe22`, Unity run `41507427853`, artifact `13171008209`; ocho PNG `heroes-{aldric|lyra}-Equipamiento-{landscape|phone|small-phone|small-phone-landscape}.png`.
- B **vigente**: `f56f426bc08e13d27133dd29580158ad43c8b5dd`, Unity run `41516706630`, artifact `13177680259`, ocho PNG de las mismas resoluciones. Captura y verificaciones V4.1 **PASS**; `result.json` declara literalmente `functional:PASS`, `visual_review:REQUIRED`, `art_approved:false`.
- Primer B desechado: `8fbf0a4d1194214be0572df8d0f926a3bda624ce`, run `41513143663`, artifact `13174997734`. Sus iconos estaban indebidamente sólidos por uso de `Mathf.SmoothStep` con distancia no normalizada. Corregido con interpolación normalizada `Fade` en `HeroesEquipmentArt.cs`; no usar ese B para solicitar aprobación.
- Inspección del B vigente: iconos transparentes auténticos en las cuatro posiciones, candados visibles, personaje 3D real y panel lateral con cuatro filas y cabecera legible; en retrato móvil el contexto se mantiene dentro de su zona. Falta aprobación artística del propietario.
- **Deuda visible**: vista 844×390 conserva el modelo y la tipografía general demasiado pequeños/borrosos respecto a escritorio, problema compartido con el perfil base; la corrección de Equipamiento mejora iconografía pero **no resuelve todo el perfil responsive**. Debe someterse a aceptación independiente.
- Regresión full PlayMode del run anterior `41513143663`: FAIL en `RoyalV3ManagementIntegrationTests.FullScreenRoyalTrainingAndMarchesPreserveCameraAndDoNotWriteOwnerSave`, comprobación de Entrenamiento/Marchas que esperaba la cifra “7” y recibió “COMPOSICIÓN Y FUERZA REAL”; los pasos previos V4.1, escenas, EditMode pasaron. No declarar `FULL_REGRESSION_PASS` del run de Equipamiento mientras el job exacto no termine satisfactoriamente; no modificar Entrenamiento/Marchas en este alcance.
- No se modifica gameplay, guardado, main, publicación o aprobación global. Autoridad artística: el propietario, después del A/B auténtico.
