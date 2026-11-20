# Valoria Modular 2.5D — Bastión I (escena aislada)

**Estado revisado: 20/11/2026.** Leer también docs/valoria-modular-25d/HANDOFF_20261120.md. El README del 18/11 describía un prototipo superado (441 tiles de diamantes y ausencia de pruebas), no el sistema actual.

**Rama:** experiment/valoria-modular-25d-bastion1-from-zero-20261118.

**Implementado:** escena independiente Unity 6000.3.23f1; terreno modular continuo con textura pintada, caminos spline y empedrado alpha, huellas de transición entre edificios y suelo; sprites originales Bastión I y Aserradero; tres estados de construcción; árboles, ruinas, arbustos, piedras, habitantes con movimiento/sorting, cámara y zoom; interfaz mínima enlazada al LocalGateway canónico con guardado experimental separado.

**Verificado:** [run 44612966834](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44612966834), SHA 6ce1c97c4967e0908e364fc97622631924885f0a: compilación Unity 6.3, 45 fotogramas reales Camera.Render, 1/1 test NUnit (plataforma EditMode que entra en PlayMode: interfaz, construcción, guardado, cámara y movimiento), vídeo real desde fotogramas con hashes. El mundo renderizado mediante Camera.Render excluye el HUD ScreenSpaceOverlay. Coste y finalización canónicos verificados. No es aprobación artística.

**No verificado:** WebGL jugable (falló BuildPipeline por precondición del frozen terrain query tape), iPhone físico, métricas móviles memoria/GC/FPS, captura GameView de HUD real y aprobación artística final. No congelar técnica ni avanzar II–V sin esas puertas.

## Abrir en Unity

Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity, o Eldoria → Experimentos → Valoria modular 2.5D. Mantener aislados el guardado y la escena de la beta. No tocar main ni Premium V6.

## Reproducibilidad

- Escena: Unity/Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity.
- Código: Scripts/Modular25DGround.cs, Modular25DLandscape.cs, Modular25DWorld.cs, Modular25DArt.cs, Modular25DWalker.cs, Modular25DCameraInput.cs, Modular25DGameplay.cs y Modular25DInterface.cs.
- Arte: Resources/ValoriaModular25D con PNG originales y sus .meta, puntos de apoyo y manifiesto de procedencia.
- Ejecución: tools/valoria-modular25d/, Editor/Modular25DNativeCapture.cs, workflows .github/workflows/valoria-modular25d-*.
- Contratos: docs/valoria-modular-25d/ART_GROUNDING_RULES.md y HANDOFF_20261120.md.

**Prioridad:** corregir el halo oscuro de la base del Aserradero, juntas visuales de empedrado/parcelas y proporciones/armonía de props; verificar capturas A/B genuinas Unity, HUD, WebGL y móvil. No iniciar otro reinicio artístico.
