# Corona — eliminar doble medallón y recuperar escala de iconos

**Fecha:** 20/11/2026. **Contexto:** UI premium original en `CoronaBrecha/icon_hammer.png`, `icon_city.png` e `icon_task.png`.

## Problema artístico probado en el origen de los sprites
Los iconos premium de OBRAS/CIUDAD/ESTADO ya son medallones originales completos con aro metálico, símbolo, gemas y volumen. La antigua función `DockButton` dibujaba adicionalmente `nav_medallion` y colocaba el icono completo dentro a **42 × 42** puntos. Resultado: doble aro muy cargado y la ilustración esencial se leía con apenas ~25 puntos. Este defecto no se detecta con pruebas de números/clics.

## Cambio de producción
`DockButton` usa ahora cada `icon_*` directamente como sprite de la imagen del botón, a **86 × 80** puntos de interacción, `Image.Type.Simple`, `preserveAspect=true`. El nombre funcional queda en tamaño 11 con sombra propia. Mantiene exactamente las acciones actuales `OpenSawmill`, `OpenStatus`, `CameraInput.Home`, sin simular héroes ni un nuevo mundo. `nav_medallion` permanece para cerrar las ventanas de detalles donde sí corresponde.

## Pruebas
- NUnit PlayMode exige `image.sprite.name == icon_hammer / icon_city`, `preserveAspect`, un solo sprite premium por botón, botones operativos y guardado canónico.
- La modificación del archivo C# es **implementación**, no prueba visual de calidad comercial. Necesita captura exact-SHA real 390×844, 1170×2532 y 844×390, evaluación de legibilidad y pulsaciones; Safari físico pendiente.
