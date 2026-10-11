# Valoria — recibo técnico del Bastión image-to-3D

Fecha: 2026-09-26

Este documento registra el primer GLB real recibido para reabrir la puerta de producción image-to-3D sin modificar Valoria.

## Fuente
- Archivo original aportado por el propietario: `fantasy castle 3d model(1).glb`.
- Inspección previa registrada: ~501k triángulos, una sola malla y tres texturas 4096×4096.
- El original se conserva como **source/high-poly**. No debe someterse a más procesamiento pesado dentro de Work.

## Candidato optimizado
- Nombre: `Bastion_Optimized_v1.glb`
- Estado: generado fuera del repositorio y validado estructuralmente.
- Mallas: **1**
- Vértices: **39,251**
- Triángulos: **81,506**
- Tamaño: **12,794,888 bytes (~12.2 MiB)**
- Objetivo de texturas registrado para esta pasada: 2048×2048.
- Propósito: **prueba técnica/artística aislada**, no asset de producción final.

## Ubicación prevista
Cuando el binario esté disponible para el repositorio/runner, colocarlo en:

`Unity/Assets/Eldoria/ArtTests/ImageTo3D/Source/Bastion_Optimized_v1.glb`

No reemplazar archivos de producción ni editar `Valoria.unity`.

## Validación Unity obligatoria
1. Importación real del GLB mediante una vía compatible con Unity 6000.3.23f1.
2. Confirmar malla, normales, UV, materiales y texturas.
3. Ajustar únicamente orientación/escala/pivote en la escena aislada.
4. Capturar los tres zooms de revisión: ortográfica 19 / 12 / 9, 1280×720.
5. Añadir una vista oblicua/orbitada para demostrar volumen real.
6. Probar collider/raycast de selección sin integrar gameplay.
7. Comparar pérdida visible frente al source/reference.
8. Registrar triángulos, materiales, resolución de texturas y coste de memoria/runtime observado.

## Criterio
- **A**: conserva identidad y detalle suficiente, importa de forma reproducible y el coste es razonable para seguir construyendo el pipeline.
- **B**: viable, pero requiere una segunda optimización/retopología o reparación manual repetible.
- **C**: la reducción destruye calidad, materiales o geometría de forma que hace inviable la vía como pipeline principal.

Hasta que el binario esté presente en el proyecto/runner y Unity produzca evidencia, no se debe afirmar que el pipeline está validado de extremo a extremo.
