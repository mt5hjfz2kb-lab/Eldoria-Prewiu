# Vigilia Fase 1 — kit visual único (evidencia y límites)

Estado registrado: **CANDIDATA DE FUENTE / VISUAL RECHAZADO POR FALTA DE NUEVA CAPTURA UNITY**.
La corrección NO está publicada ni fusionada en `main`; la fase 2 queda fuera.

## Identidad, compatibilidad y referencia

- Workstream canónico: `eldoria-forja-oscura-visual-proof-20261107` (PR histórica #173 fusionada, sin abrir otra PR).
- Rama de trabajo: `feature/forja-oscura-native-proof-20261107`.
- SHA de la fuente corregida: `1332a95f1a3ad36a6c0b425fef84014269edc685`.
- Referencia aprobada original: `75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg`, **864×1536**, SHA256 `f57f1250951c906777e52d5a2e3524d9a6dfea3b64f283da82ad44a4a50e6e13`. Su recuperación del archivo aportado por el propietario es solo referencia visual; no se ha incorporado como sprite de juego.
- Antes real: Unity run [43294080321](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43294080321) para ciudad/Bastión/Buzón y run [43189305862](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43189305862) para Bolsa. Doce PNG de antes cotejados por SHA256, no escalados fraudulentamente a Retina.
- ZIP verificado de antes: SHA256 `87d422a47c800e693139cea4d9f614dbfe7d93caa518bbdb3a9b231d498a1b2b` (el archivo permanece en el contexto del proyecto del chat; no forma parte del código).

## Cambios implementados en fuente

1. Kit de roles visuales `BreachWatchControlKit`: primary, secondary, active/inactive tab, icon. Aplicado a temas de destinos, Buzón, modal y botón Volver de la Bolsa sin sustituir sus controladores.
2. Navegación de ciudad: etiquetas visibles y estado activo a partir del destino real, sin mostrar acciones futuras antes de desbloquearlas. Objetivo de la ciudad ampliado en 390px; se mantiene el estado/progreso real.
3. Buzón: pestaña “Premios” coherente, pintura según sprite del controlador real (no su nombre), indicador ligado a la posición del scroll real.
4. Bolsa: presentación del Volver con el kit y conservación de la cuadrícula propia de Issue186; la prueba pendiente utiliza doce paquetes temporales en memoria (tres recursos × 100/1K/10K/100K), sin comandos de entrega reales.
5. Nitidez: navegación sin mipmaps, filtro bilineal, tope 1024 y compresión desactivada; botón de mejora sin compresión; paneles grandes conservan compresión para no aumentar drásticamente memoria móvil, pero con calidad de importación 100 en vez de 50; sobre original RGBA 1536×1024 importa con máximo 1024 (antes 512). Se actualizaron el generador y los validadores, conservando GUID/PPU/contornos aprobados.
6. Evidencia real a 3×: Capturador Editor exclusivo `BreachWatchRetinaEvidence` y contratos de los workflows preparados para 15 PNG auténticos de ciudad/Bastión/Buzón/informe/Bolsa en 390×844, 844×390 y 900×1600; no cambia el devicePixelRatio de WebGL.

## Qué está realmente verificado

- [Source QA 43525055165 — PASS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43525055165): incluye todos los archivos de captura Retina del Editor, que antes faltaban al sparse checkout.
- [Source QA 43526553331 — PASS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43526553331): incluye hash de sobre original, límite estricto 1024 en cinco plataformas, geometría y política de imports. Se corrigió el validador antiguo que exigía máximo 512; **no** se relajó su control de hash.
- Run 43526325225 — FAIL histórico coherente con ese validador obsoleto, corregido posteriormente. Run 43526325184 de importación de arte — FAIL seguro por guard de titular histórico/revisión 30; no ejecutó importación ni subió nuevos recursos. No se debe confundir con fallo de la imagen.
- La auditoría de antes demuestra capturas auténticas y permite describir diferencias; **no** es una captura posterior ni demuestra mejoras visuales.
- Los controles Ubuntu solo comprueban sintaxis/fuente; no hay Unity runtime de este SHA, compilación nativa, GPU 3×, iPhone físico ni WebGL DPR verificado.
- La nueva resolución del sobre es una hipótesis de mejora nítida con impacto de memoria por medir; no implica aceptación.

## Bloqueo y criterio visual

La titularidad del recurso `windows-runner-heavy` permanece en el trabajo `eldoria-issue186-reward-attention-guidance-20261111`. No se cancela, ocupa ni suplanta esta reserva. Aun con su último run nativo terminado, solo el relevo/liberación CAS formal autoriza la ejecución de Vigilia.

Siguiente prueba tras liberación legítima del runner: ejecutar **una única** campaña native con el SHA fijo, guardar las 15 imágenes 3× y sus manifiestos SHA256, comparar la MISMA cámara a 1× antes/después y referencia aprobada, recortar botones a 300% desde píxeles 3× originales (sin inventar escala); comprobar proporciones, jerarquía, textos, 100/1K/10K, puntos/avisos solo si hay estado real, pluma de USAR y sello de Buzón. Informar fallos antes de una única corrección coherente.

**Decisión al cierre de esta sesión:** `ART_APPROVED=false`, `VISUAL_ACCEPTED=false`, `NATIVE_3X_VERIFIED=false`. No merge, no despliegue, no fase 2.
