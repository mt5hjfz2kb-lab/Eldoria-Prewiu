# Forja Oscura — contrato de aceptación visual y jugable

**Estado: candidato bajo PR #173, NO aprobado.** Referencia visual del propietario: la maqueta «Eldoria — Ciudadela entre cenizas», con panel derecho **Cuartel**, navegación inferior de cinco funciones y estética metal ennegrecido / bronce. La referencia es **artística**; la ciudad de dicha imagen es ficticia y **no** se debe importar ni usar como fondo del juego.

## El criterio de éxito no es que el código compile

La interfaz debe mostrar el acabado oscuro, cálido y con detalle pictórico de la referencia y al mismo tiempo utilizar los controles y cifras auténticos de Eldoria. **No** sustituir la experiencia por rectángulos planos ni reutilizar toda la maqueta como una sola textura.

| Componente de la referencia | Implementación candidata real | Evidencia aún necesaria |
|---|---|---|
| Barra superior de metal oscuro y datos claros | `Forja / existing header skin` sobre el encabezado existente | Cuatro fotos nativas, sin tapar nombre, poder ni recursos |
| Recursos identificables e ilustrados | PNG `res_madera`, `res_piedra`, `res_comida` sobre los iconos de `ResourceBar` | Valores del `RoyalSnapshot` intactos, glyphs visibles de 844×390 y 390×844 |
| Botones de navegación ilustrados | Cinco `nav_*.png`, respaldo nine-slice `nav_tile_*` y panel continuo | Las cinco acciones reales con raycasts, tamaño táctil, estados bloqueados, sin categorías inventadas |
| Misión narrativa compacta | `panel_quest`, texto y botones existentes de objetivo/Crónicas | Misión, progreso y dos acciones legibles sin tapar el mundo |
| Ficha contextual del Cuartel | Panel V3 `RoyalGoldV3ContextHud.ContextRect` y, si procede, cartela V2 | Título, nivel/coste, CTA auténtica, estado/temporizador y cierre sin solape |
| Transiciones elegantes de botones | Sprites separados normal, resaltado, pulsado y deshabilitado | Cambiar estado sin perder ni inventar `onClick` |
| Mundo como protagonista | Cámara y escena integradas de Eldoria sin cambios | Evidencia de que geometría, luz y cámara son las mismas entre A/B |

### No se pretende reproducir a ciegas

1. La foto propuesta no garantiza los píxeles exactos dentro de Unity; Nine-Slice adapta materiales y marcos a proporciones reales, y el texto auténtico requiere su espacio.
2. La maqueta muestra ilustraciones de edificio y retrato que **no** se van a copiar automáticamente si no existe el recurso auténtico correspondiente. No asignar un héroe como retrato del jugador sin un diseño aprobado.
3. Los iconos y paneles ya son sprites independientes; su calidad **dentro de la ejecución** debe verse en las capturas originales de Unity, no sólo en una hoja de estilo o proyección Python.
4. Cuando no haya correspondencia visual entre la referencia y el resultado nativo, documentar qué componente falla y corregir ese componente reutilizable. No encadenar cambios a toda la UI ni modificar la ciudad para aparentar una mejora.

## Pruebas que deben pasar antes de fusionar

- Compilación de Unity 6000.3.23f1 sin errores, misma rama/SHA y escena canónica; verificar importación de 41 sprites y bordes Nine-Slice.
- A/B **en la misma partida**: ocho archivos `native-before-*` / `native-after-*` más `ForjaNativeEvidence.json`, cuatro resoluciones (1600×900, 900×1600, 844×390, 390×844).
- El capturador debe probar que cambian regiones **de navegación y encabezado**, no aceptar como prueba únicamente los cambios de píxeles del mundo.
- Gameplay/clicks: misiones, construir, mundo, unidades y crónicas; ayudas, cerrar, costes, restricciones, orientación, temporizadores y persistencia intactos. Invalidar PASS si falla un destino real.
- Inspección de calidad: bordes nítidos sin cortes, iconos reconocibles a tamaño real de móvil, jerarquía tipográfica, ausencia de paneles enormes y ausencia de colores o materiales que parezcan una maqueta plana.
- Rendimiento Android/WebGL: medir antes de activar en compilaciones, no inferir FPS/VRAM de pruebas estáticas; revisar licencias/procedencia de PNG.
- **Evaluación estética de usuario**: comparar la captura nativa directamente con la referencia elegida, distinguir fidelidad artística de corrección técnica. No marcar `ART_APPROVED` por tener CI verde.

## Estado de evidencias

- **Probado:** comprobaciones automáticas de integridad de PNG/Unity .meta, licencia de origen declarado, Roslyn sintáctico y contratos API de los controles publicados.
- **Implementado candidato:** `Unity/Assets/Eldoria/VisualKit/` y capturador nativo que evita crear otro HUD.
- **Por verificar:** compilación real Unity, capturas antes/después del skin real, raycasts y experiencia final, rendimiento y aprobación artística.
- **No tocar:** mundo, iluminación, reloj, guardado, balance, PR #163, ni la build pública.

**Decisión de producción:** si el candidato no consigue la riqueza visual exigida en Unity, seguir desarrollando el kit/modulares por componentes y no sustituir el HUD estable. La PR permanece en borrador hasta demostrar calidad real.
