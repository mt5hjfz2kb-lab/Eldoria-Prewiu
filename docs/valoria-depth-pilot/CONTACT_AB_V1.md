# Valoria Premium V6 — prueba complementaria de profundidad por contacto

Fecha: 18/11/2026. Rama: art/valoria-25d-profundidad-piloto.

## Estado
- INSPECCIONADO: archivo real Unity Premium V6 congelado, lógica de ordenación Y, sprites/sombras, capturas anteriores, cámara ortográfica y contratos de construcción.
- IMPLEMENTADO EN FUENTE: alternativa B-contacto en carpeta independiente Ciudad2DDepthPilot; sombra interior tenue anclada al soporte alfa medido en Bastión/Aserradero/Granero; corrección de luces ordenadas a 9000 y humo por profundidad de propietario; actividad local del trabajador en Aserradero y almacén en Granero, desactivadas antes de la construcción. Los trabajadores son marcadores visuales procedimentales para medir integración, NO arte comercial ni sustitutos aprobados.
- COMPROBADO EN FUENTE: parser C# en GitHub Actions, inmutabilidad byte a byte de escena A. NO equivale a compilación Unity, capturas B ni calidad final.
- PENDIENTE: compilación Unity, PlayMode, 114 fotogramas auténticos, seis clips, evaluación artística, WebGL/Android/iPhone físico y métricas FPS/VRAM.

## Contrastes independientes
- Control A: Unity/Assets/Experimentos/Ciudad2D/QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity, SHA256 aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06.
- Piloto B-contacto: Assets/Experimentos/Ciudad2DDepthPilot/Valoria_Depth_B.unity, generado únicamente como copia de V6 al invocar DepthPilotNativeCapture.Capture. No comitear ni pisar V6.
- Otra prueba B-cielo en la misma rama usa City2DDepthPilotEvidence y un shader de cielo lejano. No mezclar resultados ni llamar a ninguna «mejor» antes de verlas en movimiento.

## Evidencia auténtica de Unity
En checkout aislado de rama, Unity Editor 6000.3.23f1:
1. Importar el proyecto sin alterar escenas ajenas.
2. Ejecutar método Eldoria.Experimentos.Ciudad2DDepthPilot.Editor.DepthPilotNativeCapture.Capture con -executeMethod del Unity Editor.
3. Se crea B como copia; captura A y B por pan XY, zoom hacia Bastión/Aserradero y zoom de salida; 18 fotogramas por ruta y variante a 390×844, más uno a 1170×2532; 114 imágenes auténticas, mismos parámetros por pose.
4. Verificar evidencia Unity/Ciudad2DDepthPilotEvidence/evidence.json con python tools/valoria-depth-pilot/verify.py --frames Unity/Ciudad2DDepthPilotEvidence --movies (Pillow y FFmpeg). Produce seis MP4 solo a partir de fotogramas verdaderos de Unity, SHA y diagnóstico de diferencias; no certifica calidad visual.
5. En otra invocación Unity ejecutar Test Framework -runTests -testPlatform EditMode -testFilter Eldoria.Experimentos.Ciudad2DDepthPilot.Tests.DepthPilotPlayModeTests.PilotB_RealPlayMode_StatesOcclusionSelectionAndZoom. Valida estados de obra, colliders, selección, cámara, humo y anclaje.

## Integración y recursos
No tocar main ni integration/valoria-25d-playable-20261117. No ocupar el Windows self-hosted actualmente reservado por eldoria-city2d-illustrated-proof-20261116 sin relevo explícito. No declarar captura Unity o aprobación artística solo porque el preflight de Ubuntu sea verde.

## Límite de técnica
El fondo pintado no aporta geometría que ocluya los edificios; orden Y solo decide qué sprite va delante, no intersecciones internas. Añado únicamente contacto anclado, efectos de profundidad local y actividades pequeñas. Si el resultado no mejora el vídeo o los personajes rebajan el acabado premium, descartar esta B y explorar segmentación verificada del terreno o geometría selectiva. Nunca expandir sin QA visual y técnica positiva.

## Diagnostico tras los dos intentos reales (18/11/2026)

- Unity Windows #44275072091: arrancó, no llegó a compilar por falta de Git.exe en PATH heredado por el Package Manager para `com.arloopa.unitysplats`. El workflow ahora persiste el directorio Git mediante `GITHUB_PATH` y verifica Git antes de ejecutar Unity.
- Unity Windows #44275806196: superó la resolución del paquete Git, alcanzó la compilación C# y encontró cinco `CS0234` por confusión `Application.isPlaying` con el namespace del proyecto `Eldoria.Application`. Corregidos en fuente todos los usos nuevos con `UnityEngine.Application` (runtime, Editor y pruebas, incluyendo la otra prueba aislada B-cielo).
- Ambos intentos **NO generaron imágenes A/B válidas** y no acreditan profundidad, calidad, PlayMode ni rendimiento. Los errores y correcciones están versionados; la próxima ejecución nativa es imprescindible para comprobarlos.
- PC vuelve al titular City2D después de este turno; el piloto no ha ocupado la rama de integración funcional.

## Corrección de una incoherencia de evidencia detectada en revisión de fuente

La actualización `City2DStateBridge.SetDemoBastionLevel(5)` activa los edificios
ya construidos, pero `DepthPilotContact.LateUpdate()` se ejecuta solo en PlayMode.
En una captura `Editor_RenderTexture`, las sombras de apoyo del candidato B
podían seguir invisibles. El método `PrepareStage` ahora invoca
`DepthPilotContact.Refresh()` explícitamente **después** del cambio de etapa.
Se añade un control de fuente que impide repetir esta omisión. Hasta obtener
PNG originales de Unity, el efecto visual continúa SIN VERIFICAR.

## Copia A binariamente idéntica con metadatos Unity válidos

Detectado en el repositorio: el fichero heredado
`QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity.meta`
contiene caracteres literales `\\n` en lugar de saltos de línea y no es
metadato YAML válido. El original **no se edita**. Se incorpora a esta rama
experimental la escena espejo `Scenes/Valoria_Depth_A_ExactV6.unity` con
exactamente el mismo blob de Git
`4785b9bdbd77a5f700a2fd4d835ac3ac39c3e362` y un `.meta`
correctamente formado. Antes y después del render se verifican los SHA-256
de las DOS escenas, que deben ser
`aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06`.
A se captura desde la copia exacta; B se crea como variante de esa copia.
El control A original y su fichero .meta permanecen inalterados en la rama
de origen, sin cambio alguno de contenido ni pérdida de recursos premium.
Esta técnica evita depender de un importador tolerante con metadatos corruptos.
