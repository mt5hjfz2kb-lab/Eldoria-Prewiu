# Valoria — puerta image-to-3D: índice histórico

Estado actualizado: 2026-09-26. Este documento deja de ser un procedimiento de reentrada. La investigación inicial sobre herramientas de generación precede al primer GLB real y contenía pasos ya superados.

- [Prueba real en Unity del Bastión completo optimizado](VALORIA_IMAGE_TO_3D_UNITY_GATE.md): una malla 3D, tres zooms y vista oblicua capturados; falló la legibilidad arquitectónica y los mapas enlazados llegaron a 4×4. Veredicto C para **ese asset/importado**.
- [Ruta vigente: Tripo por módulos pequeños](VALORIA_TRIPO_MODULAR_GATE.md): generación y segmentación como base provisional; retopo, UV y materiales se validarán fuera de Tripo antes de importar una sola pieza modular en Unity.

No volver a procesar el Bastión original de ~501k triángulos, construir castillos completos ni probar retopología agresiva dentro de Tripo. La siguiente entrada es un GLB pequeño con partes separadas y limpieza externa documentada. Unity ya tiene glTFast 6.14.1. Mantener `Valoria.unity`, `VisualWorld` y gameplay intactos.
