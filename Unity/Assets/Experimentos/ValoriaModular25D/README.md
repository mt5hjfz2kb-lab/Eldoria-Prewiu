# Valoria · Bastión I modular 2.5D (NUEVO, aislado)

**Estado técnico:** cuatro componentes runtime en C# escritos, escena Unity YAML versionada, helper Editor WebGL/captura versionado. **SIN ejecución actual en Unity, PlayMode, compilación, imagen nativa, vídeo ni aprobación artística.** No declarar este candidato jugable hasta el gate nativo.

## Abrir

1. Abrir el proyecto Unity 6000.3.23f1 de ESTA RAMA, no la escena de producción.
2. Abrir `Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity` (o menú **Eldoria → Experimentos → Valoria modular 2.5D**).
3. PlayMode: tocar la parcela del aserradero o el botón inferior ASERRADERO; pulsar CONSTRUIR si es posible.
4. Observar costes/madera descontada por `LocalGateway.Execute(new GameCommand(..., "Build", "sawmill", ...))`, temporizador UTC y guardado persistente. Cerrar/reabrir PlayMode para comprobar recuperación. No se modifica la partida beta, se guarda en `eldoria-experiment-modular-25d-bastion1-v1.json`.
5. Mantener separadas las pruebas del editor y el dispositivo físico. Abrir el menú solo cuando el runner Windows esté legítimamente liberado y reclamado.

## Qué es realmente modular

- **Suelo:** 441 sprites diamantados independientes; célula 2:1, 132×68 raster (128×64 + 2 px de solape), PPU 64. No hay una imagen gigante, plataformas ni geometría 3D.
- **Edificios:** raster original 2.5D generado desde `Modular25DArt.cs` (un único método/paleta/perspectiva), dos sprites independientes con pivote de pie.
- **Ambiente:** pinos/arbustos/piedras independientes, sombras de contacto sprite y la misma función de ordenación vertical para objetos y actores.
- **Habitantes:** dos figuras con dos frames de ciclo de marcha y rutas dinámicas; una ruta atraviesa ambos lados de la profundidad de un árbol. No hay física de colisión, navegación avanzada ni animación definitiva.
- **Cámara:** ortográfica fija; traslación y zoom touch/puntero, sin rotación.
- **Interfaz:** status de recursos y botón funcional de construcción, panel con cierre y botón para centrar.

**IMPORTANTE** La escena es un **primer candidato funcional**: arte programático estilizado, no calidad 8/10 verificada. El estado disponible/en construcción se distingue ahora por tinte de un mismo edificio (provisional); necesita sprites propios y revisión visual antes de aprobar. No prometer producción de madera pasiva en Bastión I: la economía de producción real está gobernada por las reglas canónicas y su desbloqueo posterior.

## Entradas y fronteras

- Aserradero: `Eldoria.Application.LocalGateway` + `Eldoria.Domain.ParcelBuildingStates`, perfil existente `OWNER_I_II`.
- El guardado usa `Eldoria.Infrastructure.FileStateStore` con nombre aislado; no abre ni modifica guardados publicados.
- `SliceBoot` (runtime global) reconoce la presencia del presentador legacy desactivado y debe omitir su HUD (hipótesis **pendiente de test PlayMode**; si no lo hace, corregir el bootstrap SOLO en esta rama sin tocar el código de producción).
- `main`, Premium V6, scripts de dominio, economía, arte anterior y ramas de otros responsables no se modifican.
- No se añade la escena a Build Settings del juego. El comando Editor compila con una lista de escenas aislada.

## QA / evidencia

- Menú Editor: **Capturar Unity real (PlayMode)**: PNG world-only 390×844, 1170×2532, 844×390, con fichero de procedencia. No incluye HUD ScreenSpaceOverlay.
- Menú Editor: **Grabar movimiento Unity**: 90 fotogramas de `Camera.Render` en 12 FPS, no frames falsificados. Solo evidencia auténtica al ejecutarlo realmente.
- Menú Editor: **Compilar WebGL aislado**: salida `Unity/Builds/ValoriaModular25D_WebGL`; no se publica ni cambia beta.
- Ver además `docs/valoria-modular-25d/QA_CHECKLIST.md`.

**No hay evidencia nativa todavía.** Leer `DECISIONS.md` para límites y puntos de decisión visual.
