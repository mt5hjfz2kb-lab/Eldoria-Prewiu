# Valoria 2.5D — veredicto de la primera comparación real en movimiento
**Fecha:** 18/11/2026. **Estado:** prueba nativa ejecutada y verificada; **B no aprobada artísticamente; no extender**.

## Fuente y procedencia
- GitHub Actions [#44285505085](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44285505085), sobre fuente `7e8ba303ee0f`, Unity Editor 6000.3.23f1 / Direct3D 11.
- Archivo original [artefacto #14419596041](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44285505085/artifacts/14419596041), `valoria-depth-frames-44285505085.zip`, 122657106 bytes comprimidos. Contiene 114 PNG, `evidence.json` y `PlayMode.xml` (y logs).
- Al descargarlo, los ficheros no están directamente en la raíz del ZIP: se conservan bajo `valoria-depth-44285505085/Unity/Ciudad2DDepthPilotEvidence/`. El empaquetador Linux automático buscó erróneamente `native/evidence.json` y falló por **ruta**, aunque los PNG ya existían. No requiere nuevo render nativo para reparar los vídeos.
- El control A conserva el SHA-256 de escena `aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06`.

## Validación independiente posterior, leyendo el ZIP original
- 114/114 SHA-256 de PNG coinciden con el manifiesto auténtico.
- 57/57 parejas de fotogramas de A/B coinciden en trayectoria, tiempo, resolución, zoom, foco y **posición/tamaño ortográfico realmente aplicados** de cámara.
- Seis recorridos fuente muestran diferencia entre primer y último fotograma, de modo que no son una imagen estática repetida.
- Tres movimientos (18 fotogramas por variante, 12 fps, 390×844): pan horizontal/vertical; aproximación; retirada. Además, seis fotogramas originales a 1170×2532.
- PlayMode `1 passed / 0 failed / 0 skipped` según NUnit XML original. **No es prueba física iPhone ni WebGL**.
- Tras extraer solo los PNG originales se ha generado externamente un MP4 A/B de 4,5 s, con cada fotograma compuesto por sus imágenes A+B Unity (sin generación IA ni interpolación). El MP4 es un derivado de visualización del chat; **no está publicado todavía en GitHub**.

## Medida de magnitud del cambio A/B (no puntuación artística)
Diferencia absoluta media RGB en imágenes de 390×844, escala de píxel 0–255 y porcentaje de píxeles con diferencia máxima por canal ≥12:

| Recorrido | MAE RGB / 255 | % píxeles con diferencia ≥12 |
| --- | ---: | ---: |
| Pan XY | 0,0457 | 0,07 % |
| Acercamiento | 0,0498 | 0,08 % |
| Alejamiento | 0,0442 | 0,08 % |

Esto **no significa** que el algoritmo sea incorrecto ni que el 99,9 % del juego sea «bueno»: solo demuestra que el piloto visual altera una porción diminuta de la imagen final.

## Revisión artística
**B-contacto/animación no demuestra mejora sustancial de profundidad** en un control emparejado. El Bastión, las murallas y la mayor parte del suelo mantienen la misma relación plana con el fondo y la cámara. Las sombras/actividades locales son demasiado tenues o pequeñas para resolver el síntoma de «fotografías superpuestas». El movimiento de cámara sigue siendo principalmente una transformación de una composición pictórica.

**Decisión:** mantener A íntegra y conservar B como evidencia, **rechazar la extensión masiva de B actual**. No atribuir 8/10 ni solución al collage por pasar PlayMode o por haberse generado 114 PNG.

## Próxima hipótesis que realmente ataca la causa
Pilotar **segmentación de oclusiones basada en siluetas** en un sector corto de Bastión–Aserradero–camino: descomponer únicamente una muralla/piedra/vegetación que actualmente está horneada en el terreno y crear máscaras de paso delante/detrás, con pivotes de pie calibrados y sin cambiar sprites premium. Comparar movimiento A/B exacto en el mismo encuadre y rechazar si el salto no es inequívoco o genera bordes artificiales. No añadir de nuevo más hierba, blur, parallax de edificios anclados ni sobrescribir la capa de gameplay. Si las máscaras no bastan, ensayar una sola superficie de suelo 3D mínima conservando las fachadas premium prerenderizadas.

## Ámbito y limitaciones
Rama aislada `art/valoria-25d-profundidad-piloto`; no se ha tocado integración, economía, guardados, combate ni `main` mediante trabajo gráfico. **iPhone físico, WebGL específico del piloto, rendimiento GPU/memoria, y valoración humana en movimiento continúan pendientes.**
