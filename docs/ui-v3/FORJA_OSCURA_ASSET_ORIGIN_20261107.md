# Eldoria — Forja Oscura | procedencia de los recursos v0.4

Estado: **declaración de procedencia del equipo, NO certificación jurídica de derechos ni aprobación artística**.

## Inventario incorporado a PR #173

- `Unity/Assets/EldoriaVisualKit/Resources/ForjaOscura/Surfaces/`: **21 sprites PNG**, creados mediante un procedimiento gráfico propio de esta conversación (Python/Pillow/NumPy, materiales y relieves procedurales) a partir de la dirección visual escogida para Eldoria.
- `Unity/Assets/EldoriaVisualKit/Resources/ForjaOscura/Icons/`: **20 sprites PNG**, aislados de un atlas de 4×5 iconos generado específicamente para el kit de Eldoria mediante generación de imágenes de OpenAI; posteriormente recortados, limpiados y preparados como archivos de alfa transparente. No son imágenes extraídas de un paquete de Unity Asset Store ni de una captura de un videojuego ajeno.
- `Unity/Assets/EldoriaVisualKit/Runtime/` y `Editor/`: adaptador y herramientas C# escritos específicamente para conectar esta piel con el RoyalHUD que ya tiene Eldoria.

**No se importó ningún paquete externo de interfaz de pago** para estos 41 PNG. La referencia artística elegida por el propietario fue una inspiración de estilo y distribución, **no se usa una captura completa como textura funcional**. Los elementos del HUD deben mantenerse separados del fondo, textos y valores reales.

El kit de procedencia v0.1 entregado en la conversación contiene generadores y fuentes intermedias. La rama de la PR conserva una copia binaria reconstruible e íntegra del paquete de producción bajo `tools/forja-oscura/parts/`; ejecuta `python3 tools/forja-oscura/reconstruct.py` para verificar su SHA-256. La reproducibilidad de la creación artística desde las fuentes originales es **un trabajo diferente** y no se ha demostrado por este simple empaquetado.

## Derechos y publicación

No se afirma que los activos sean exclusivos, estén libres de similitudes accidentales, tengan marca registrada limpia ni que una generación de IA equivalga a una licencia de terceros. El repositorio es público; los derechos de los paquetes antiguos del proyecto no quedan resueltos por este documento. No se concede autorización sobre imágenes de referencia o recursos externos que puedan aparecer en otras carpetas.

El control `tools/license-compliance/public-source-gate.mjs` clasifica `Unity/Assets/EldoriaVisualKit/` como raíz desconocida porque **su lista de carpetas propias no incluye esa ruta**; esta observación no demuestra que la carpeta sea un paquete comercial, ni constituye por sí sola autorización para omitir el control. Antes de marcarlo verde debe revisarse de manera independiente la procedencia anterior y adoptarse una resolución compatible con la política de licencias, preservando el nivel de protección actual.

## Validaciones pendientes

- Importación de PNG + `.meta` en Unity 6000.3.23f1, compilación de C# y ensamblados.
- Capturas **genuinas** antes/después en la **misma sesión** a 1600×900, 900×1600, 844×390 y 390×844; acciones, raycasts y estado persistente.
- Medición visual frente a la última referencia elegida: riqueza gráfica, no deformación, legibilidad móvil, composición sobre la ciudad **sin cambiar el mundo**.
- Conformidad de procedencia/licencias separada del resultado técnico, antes de fusionar en `main` o publicar.

**Esta evidencia registra lo realmente creado y lo pendiente; no declara el juego probado, el arte aprobado ni la PR lista para merge.**
