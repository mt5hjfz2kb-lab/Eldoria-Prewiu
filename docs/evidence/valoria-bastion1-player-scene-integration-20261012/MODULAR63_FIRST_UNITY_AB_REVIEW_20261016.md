# Bastión I — primera prueba nativa de reutilización modular: revisión visual

**Fecha:** 2026-10-16. **Veredicto artístico:** `ART_REJECTED` (no promoción).

- **Rama existente:** `integration/valoria-bastion1-player-scene-20261012`.
- **Unity real:** [workflow #38899136195](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38899136195), `SUCCESS` técnico, artifact `12045579490`, `bastion1-modular63-unity-review-38899136195`.
- **Fuente de escena jugable original:** run `38419031491`; elementos del gameplay, parcelas, caminos y barreras no editados ni publicados.
- **Fuentes seleccionadas con SHA real:** seis GLB (puerta roble V1, dos rejas hierro oscuro, muralla V6, pino medio y roca de columna neutral). Los primeros cuatro solo se prepararon, no se instanciaron. Pino y roca se usaron en esta prueba; `MODULAR63_SELECTED_VERSIONS_20261016.json` contiene los SHA y procedencia.
- **Composición del AFTER:** dos instancias del pino `pine_medium` y una de `cliff_rock_column`, ancladas al terreno real, cero modelos 3D nuevos. Se conserva el `before` con la misma cámara, iluminación y resolución para comparación; capturas de paisaje `1440x1000`, móvil `780x520` y retrato `390x844`.
- **Métrica independiente del motor, calculada directamente de los PNG del artefacto:** MAE RGB `0.11333` en 1440×1000; `4.973/1.440.000` píxeles (`0,3453 %`) con diferencia en algún canal mayor de 10; ninguna diferencia en el tercio central de la imagen (columnas 480–959). Esta métrica mide visibilidad, **no** calidad.
- **Juicio visual:** no mejora la jerarquía de la escena. Los pinos y la roca quedan en extremos laterales; siguen presentes el bloque rectangular de terreno, geometría de fondo artificial, acantilados demasiado oscuros y falta de detalle de terreno. No se justifica su promoción, ni ART_APPROVED, ni un 8/10. El PASS nativo no es un PASS artístico.
- **Rendimiento:** no se han medido FPS en iPhone real. Los módulos seleccionados son superiores al antiguo chip escaneado de 66.122 triángulos por roca pequeña, que queda expresamente descartado como repetición ambiental a falta de optimización.
- **Siguiente hipótesis distinta:** ensayar sobre **una copia de la escena** un lookdev nativo Unity de iluminación, material, color ambiente y niebla, manteniendo cámara y fuentes; comparar fuente original → añadido ambiental → lookdev con PNG genuinos. No crear mallas ni nuevos assets. Rechazar si el resultado empeora la escena o diluye la legibilidad. No publicar.

**Restricciones vigentes:** presupuesto 0 €, arte original inmutable, la rama de 63 assets continúa separada, runner Windows exclusivo, sin modificación de economía, misiones, guardado, HUD ni navegación; no subir un nuevo candidato a main sin verificación funcional y artística.
