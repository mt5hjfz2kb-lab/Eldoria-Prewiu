# Valoria Premium V6 — piloto espacial en movimiento (18/11/2026)

## Estado verificable
- **INSPECCIONADO:** fuente original de escena Unity, constructor, cámara ortográfica, coordenadas, sorting, puertas de estado y las pruebas de Premium V6.
- **IMPLEMENTADO EN FUENTE:** rama aislada `art/valoria-25d-profundidad-piloto` derivada del HEAD exacto `10dbc197d48ea8517c6a3dd6c22f1a2abf75d508`; componentes de escenario B, shader, captura A/B con poses idénticas y comprobación de SHA original.
- **NO VERIFICADO AÚN:** compilación de los nuevos componentes, RenderTexture A/B, vídeo nativo, resultado artístico en movimiento, FPS, GPU/memoria, UI/gestos WebGL y dispositivo físico. Un archivo de código no equivale a la escena B ejecutada.

La integración funcional paralela está en `integration/valoria-25d-playable-20261117` y **no se modifica**. No crear PR ni fusionar a `main` sin el control de propiedad del repositorio. No tocar economía, guardados, combate ni comandos.

## Control A inmutable

- Archivo original: `Unity/Assets/Experimentos/Ciudad2D/QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity`.
- SHA-256 exigido antes y después: `aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06`.
- Evidencia anterior de Unity: 11 PNG y PlayMode del run `44133747143`; revisión final: `docs/ciudad2d/CIERRE_VALORIA_PREMIUM_V6_20261117.md`.
- B se crea **exclusivamente** como `QA/DepthPilot/Experimento_Ciudad2D_V6_Depth_B.unity` mediante «Guardar como copia» y no sobreescribe A ni `Experimento_Ciudad2D.unity`.
- El original de Granero/Cuartel/Aserradero/Cantera, sus escalas Premium V6, sus sombras independientes, su progresión y sus zonas de clic permanecen intactos.

## Causas documentadas a partir de la estructura

1. `Terreno_Organico.png` es una única lámina pictórica con cielo, montañas, muros, caminos y vegetación horneados. No hay geometría intermedia que revele volumen con pan/zoom.
2. Edificios y props se dibujan como sprites con `OrderFor(y)`, no como objetos que penetren realmente en un terreno con profundidad. El pie ilustrado puede diferir del pie físico.
3. Glows originales tienen orden fijo `9000`, que no conoce oclusiones físicas. Los efectos de humo están ordenados por la posición del sprite, no por una geometría de tejado.
4. Las sombras V6 existen y están condicionadas por `Built`, pero no impiden que el fondo y el sprite parezcan capas independientes. Duplicar sombras sin probar el apoyo sería otro parche.
5. Una cámara ortográfica frontal solo cambia escala y desplazamiento: **no puede revelar lados nuevos de un PNG plano**. Paralaje del cielo puede aportar una pista, pero no resuelve muros/edificios/plataformas.

Estos son hallazgos estructurales; el juicio visual sobre su intensidad durante un recorrido requiere grabación A/B. No declarar el diagnóstico plenamente confirmado solo con un fotograma.

## B — hipótesis mínima y reversible

**Separación de decorado lejano sin desplazar edificios:** `City2DDepthSkyMotion.cs` añade en escena B una segunda lectura de la textura de terreno, exclusivamente para el segmento alto del paisaje (shader `City2DDepthSkyMask.shader`; alfa progresivo por UV y≈0.80–0.90), con respuesta de pan 0,085 y recorrido máximo 0,095 unidades. Todo lo anclado —Bastión, secundarios, caminos, muros, sombras, puntos de toque— queda en su posición original. Un parpadeo o un contorno duplicado en el horizonte supone **rechazar la técnica**. La duplicación de dibujado exige medición de sobrecoste GPU.

**Actividad local con arte existente, sin crear PNG de menor calidad:** animación determinista muy suave de un carro original `Atrezzo_decor_04` ligado visualmente al Aserradero y de un árbol original `Atrezzo_decor_00` ligado al Bastión. No duplica vegetación, vehículos ni modifica el gateway. Ningún «trabajador premium» está diseñado: **la prueba de trabajador humano está pendiente** y no debe sustituirse por una silueta de baja calidad.

**Oclusión y contacto físico:** la fuente existente permite orden por pie con los sprites independientes, pero el muro horneado y las bases no están segmentados. No sería honesto declarar reparación del muro por añadir partículas, máscaras toscas o más arbustos. Un segundo piloto deberá crear máscaras geométricas verificadas o una superficie parcial híbrida si este B no mejora de forma sustancial.

## Captura nativa, misma trayectoria

Método: `Eldoria.Experimentos.Ciudad2D.Editor.City2DDepthPilotEvidence.Capture`. Solo genera imágenes de `Camera.Render()` a `RenderTexture`, nunca usa vídeos generados ni una captura animada artificialmente.

Para A y B:
- Mismos 6 edificios y snapshot de estudio de Bastión V (Cantera **Available**, no construida sin obra).
- Resolución completa 390×844: **24 fotogramas por variante**, 8 por movimiento, muestra temporal de 8 fps.
- Resolución 1170×2532: 9 imágenes auténticas por variante, tres tiempos muestreados de cada uno de los movimientos.
- Trayectoria: 1) pan horizontal/vertical (zoom fijo 1,38); 2) zoom de 1,38 a 1,82 hacia la zona Bastión-Aserradero; 3) zoom hasta 1,0 regresando a vista general.
- El método compara `Camera.transform.position` y `orthographicSize` entre A y B con tolerancia de 0,0001; si alguna pose difiere, **aborta**.
- Manifiesto `Unity/Ciudad2DDepthPilotEvidence/evidence.json`: 66 PNG esperados, SHA256, dimensiones, tiempos y poses. El lector debe comparar en movimiento y después inspeccionar 390×844 directamente en móvil.
- Transformación determinista de humo/props con `EvaluateAtTime`, sin usar el reloj real de distintas ejecuciones.
- Crear películas con FFmpeg exclusivamente a partir de los fotogramas PNG originales de Unity, manteniendo el orden/tasa y señalando la procedencia. No interpolar ni modificar posiciones.

## Árbol de decisión artístico y técnico

1. **Criterio de rechazo inmediato:** edificios premium más borrosos, contornos dobles en cielo, vibración falsa de tierra/murallas, trabajador o elementos artificiosos, sombra duplicada.
2. **Si no hay mejora clara en vídeo:** revertir la capa de cielo y probar recorte estructural real de fachada/muro con máscaras bien delimitadas o pequeña geometría de apoyo; no añadir filtros/arbustos por defecto.
3. **Si mejora:** medir segundos adicionales de captura, draw calls, memoria texturas, frame time y estabilidad WebGL/Android/iPhone; después expandir a un sector adicional, no toda la ciudad simultáneamente.
4. **Cierre artístico:** valoración humana del vídeo en movimiento con idéntico encuadre, estado y resolución. Ningún CI verde implica 8/10.

## Limitación actual de ejecución

El PC Windows compartido aparece reservado en `main/pipeline/active-workstreams.json` por `eldoria-city2d-illustrated-proof-20261116` en la rama artística original. El flujo `integration/valoria-25d-playable-20261117` también posee ejecuciones en curso. **Este piloto no ocupa el runner sin una reserva explícita transferida para la nueva rama**. No cancelar, reservar ni usurpar trabajo concurrente; la captura de Unity permanece pendiente mientras esa reserva no cambie. Tampoco se afirma QA físico en iPhone.

## Referencias previas para evitar repetición
- `docs/ciudad2d/DIRECTRIZ_CORRECCION_RUMBO_2_5D_20261117.md`
- `docs/ciudad2d/CIERRE_VALORIA_PREMIUM_V6_20261117.md`
- `Unity/Assets/Experimentos/Ciudad2D/ArtBible/BIBLIA_VISUAL_VALORIA_v1.md`
- `Unity/Assets/Experimentos/Ciudad2D/ArtBible/CAPA_VIDA_AMBIENTAL_20261117.md`

**Nunca declarar «profundidad conseguida» sin vídeos de Unity del mismo movimiento; la técnica de cielo es solo un candidato por validar.**
