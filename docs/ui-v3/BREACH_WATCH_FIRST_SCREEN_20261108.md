# Vigilia de la Brecha · pantalla independiente · primera prueba nativa
**Fecha:** 2026-11-08. **PR canónica:** #173, borrador. **Dirección artística elegida por el propietario:** Vigilia de la Brecha. NO Forja, NO Fortaleza.

## Implementado en código nuevo
- \`Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs\`: Canvas, CanvasScaler, GraphicRaycaster, composición, botones, tarjeta de edificio y presentación aislados. **No** se usan \`RoyalHUD\`, \`RoyalGoldV2Hud\`, \`ForjaOscuraLiveHudSkin\` ni sus prefabs de paneles como piezas visuales; la antigua Canvas queda deshabilitada durante la prueba y se restaura al destruirla.
- \`BreachWatchGraphics.cs\`: arte vectorial nuevo con medallones, pictogramas y placas de basalto/cobre. Es **prototipo de geometría**, no activos pintados aprobados.
- La sesión real del juego sí se reutiliza con \`ValoriaIntegratedWorld.Read\`, \`Submit\`, navegación y \`RoyalHUD.SelectBuilding\` únicamente como comando de selección del juego. No se crea otra economía, partida ni gateway; la lógica autorizada existente sigue siendo la fuente de verdad.
- Escena canónica real de Valoria. Nunca sustituir por el paisaje ficticio de una referencia IA.

## Evidencia Unity ejecutada
- **SHA de fuente ejecutado:** \`1cea977b8ec67495bbbcbc7b31d38425e352c10c\`.
- **Run Windows Unity 6000.3.23f1:** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42427015492 — **SUCCESS**.
- **Artefacto original:** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42427015492/artifacts/13596191419.
- **Ocho PNG auténticos**: ciudad/bastión, cada uno en 1600x900, 900x1600, 844x390 y 390x844; SHA-256 por imagen en \`BreachWatchNativeEvidence.json\`.
- **Pruebas reales:** 1 pulsación mediante \`GraphicRaycaster\` sobre REINO para seleccionar Bastión; cinco destinos nuevos en una fila y dentro de pantalla con hitbox de al menos 44 px en las cuatro resoluciones; datos reales; nuevo Canvas sin elementos gráficos del HUD anterior; Canvas anterior oculto; \`saveUnchanged=true\`, \`oldCanvasVisible=0\`, \`oldHudGraphicsReused=false\`.
- **Limitaciones:** únicamente se ha demostrado la pulsación de REINO. UNIDADES aún no tiene pantalla independiente; no afirmar que las cinco rutas funcionan. No se ha probado el CTA de construcción bajo presupuesto suficiente, giro físico en Android, WebGL, entrada completa desde APK ni rendimiento. Esta prueba aislada no sustituye el recorrido jugable completo.

## Juicio artístico de las capturas auténticas
**NO APROBADO.** La primera pantalla existe y funciona como prototipo técnico, pero está visualmente por debajo de la referencia seleccionada:
1. Las placas del basalto son demasiado geométricas, uniformes y planas; falta autoría pictórica, desgaste localizado, espesor creíble y materialidad.
2. Los glifos hechos solo con líneas parecen iconos provisionales y no emblemas tallados profesionales.
3. La ficha contextual tiene espacios vacíos, jerarquía de niveles/costes mejorable; en móvil ocupa una porción elevada de la escena.
4. El color y las formas de la interfaz no terminan de armonizar con la ciudad real, y la identidad de personaje aún no tiene tratamiento premium.
5. Hay entradas secundarias informativas de prueba, no funcionalidades integradas; deben identificarse o eliminarse antes de una entrega a jugadores.

### Decisión de producción
- Mantener como **prueba técnica reproducible**, no como propuesta artística final ni como sustitución de la interfaz publicada.
- Siguiente enfoque obligatorio: producir un sistema de **componentes de arte originales e independientes** (placa, marco, medallón, iconos y estados) de mayor riqueza, antes de seguir retocando los rectángulos actuales. Comparar la siguiente captura Unity con la referencia aprobada en tamaño móvil real. **No continuar con microajustes de color/bisel** que mantengan el mismo aspecto provisional.
- Extender a otras pantallas SOLO tras una pantalla de ciudad aprobada artísticamente; prohibidos merge, APK y WebGL de este trabajo mientras tanto.

**Métrica del hito:** UNITY_NATIVE_CAPTURED=TRUE, 8/8 PNG, pointer=1, save_preserved=TRUE, independent_presentation=TRUE; ART_APPROVED=FALSE, FULL_GAMEPLAY_PASS=FALSE, MOBILE_DEVICE_PASS=FALSE, RELEASED=FALSE.
