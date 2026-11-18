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
