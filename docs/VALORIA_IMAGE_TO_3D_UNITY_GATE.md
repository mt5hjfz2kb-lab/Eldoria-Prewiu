# Bastión optimizado — prueba aislada de Unity

Fecha: 2026-09-26. Base de trabajo: `main` en `f5cd8c3469819e80d531255556c462ecd313ce0e`. Esta prueba usa **solo** el archivo `Bastion_Optimized_v1.glb` del paquete entregado, conservando intactos `Valoria.unity`, `VisualWorld` y gameplay. El original de 501.472 triángulos no se volvió a procesar.

## Evidencia comprobada fuera de Unity

El GLB entregado tiene 12.794.888 bytes (SHA-256 `1ba4ef4daa352265116e672dfb84d5bbe26a0bd22262a2955d94cd4682862521`), una malla, 39.251 vértices, **81.506 triángulos**, UV por vértice y material PBR con mapa base, normal y metal/rugosidad, cada uno de **2048×2048**. Su eje vertical es Y y sus límites son aproximadamente (-0,295, 0,002, -0,495) a (0,295, 0,635, 0,499) en unidades del archivo: **sin escala arquitectónica expresada en metros**. Se conserva la silueta geométrica como volumen real, no una imagen sobre un plano. Tres texturas RGBA 2K equivaldrían a unos **48 MiB** sin compresión y sin contar mipmaps; el consumo real depende de la importación/compresión Unity y figura en el reporte del Editor cuando corra.

Frente al source anteriormente inspeccionado (~501.472 triángulos, tres 4096², una sola malla), esta versión reduce las caras alrededor del **83,7 %** y cada dimensión de textura a la mitad (cuatro veces menos píxeles). **La fidelidad visual no se puede certificar solo con conteos:** hay que revisar las capturas de Unity frente a la referencia. Sigue siendo una **malla única**; ni los muros ni las torres son módulos independientes. La simplificación y el reempaquetado 2K los realizó el propietario fuera de Work. No se ha hecho retopología aquí.

## Implementación de la puerta Unity

El proyecto declara [Unity glTFast 6.14.1](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.14/manual/ImportEditor.html) para importar el GLB en Editor. `BastionImageTo3DReview.Capture` (menú *Eldoria → Art gate → Open isolated image-to-3D Bastion review*) crea `Unity/Assets/Eldoria/ArtTests/ImageTo3D/BastionImageTo3DReview.unity` a partir **del archivo exacto** `Source/Bastion_Optimized_v1.glb`. Centra su pivote y normaliza el mayor lado X/Z a 22 unidades de revisión, reportando ese factor; esto **no prueba escala real**, que debe fijarse en un DCC para producción. El terreno de revisión es un plano sin reemplazar la malla.

La cámara guarda `strategic.png`, `city.png`, `detail.png` y `oblique.png`: ortográfica 19/12/9 y oblicua 12, siempre 1280×720. El capturador mide vértices, triángulos, renderers, materiales, texturas y memoria estimada por el Profiler del Editor, además del tiempo CPU de capturar cada fotograma (**no equivale a FPS móvil**). Exige normales/UV y una prueba positiva y negativa con `MeshCollider` estático + `Physics.Raycast`. Un componente aislado acepta clic de ratón en Play Mode y registra selección; no toca `VisualWorld`.

El workflow `unity-slice.yml` ejecutará esa puerta y publicará escena/capturas/`metrics.json` **solo cuando el GLB exista en el checkout o cuando el runner Windows encuentre el ZIP exacto en `Downloads` y verifique el SHA-256 antes de extraer únicamente el optimizado**. El binario de 12,8 MB todavía no está en `main` y no cabe en la conexión textual de publicación disponible sin fragmentarlo, operación expresamente descartada por el propietario. Hasta revisar el runner, **no se afirma importación Unity, capturas, raycast ni rendimiento medidos**. Los datos del GLB son inspección externa al Editor.

## Reentrada sin nueva investigación

1. Copiar **solo** `Bastion_Optimized_v1.glb` en `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Source/` en el repositorio clonado; comprobar SHA-256 y tamaño contra el recibo del paquete, y publicarlo en `main` con GitHub Desktop. No subir el ZIP ni el source pesado. El workflow se dispara automáticamente.
2. Verificar resultado real del runner Windows y bajar su artifact `eldoria-image-to-3d-<sha>`. Confirmar las cuatro imágenes, `metrics.json` y la escena. Si glTFast o material URP falla, corregir únicamente la puerta aislada y repetir.
3. Comparar silueta, techos, huecos, textura y orientación con la referencia original en las tres distancias y vista oblicua. Medir la escena sola; una ciudad con edificios repetidos y móvil objetivo necesita una prueba adicional de instancias/LOD/compresión.

**Veredicto provisional: B — viable con límites aún sin certificar en Unity.** La generación produjo geometría PBR y la reducción a ~81k/2K demuestra una parte automatizable fuera de Work. La identidad, escala arquitectónica, unión roca/camino, clic preciso en superficies finales, separación modular, etapas del Bastión y presupuesto para una ciudad 4X siguen pendientes de evidencia visual y trabajo manual. Si las cuatro vistas conservan la identidad pero la malla no puede separarse, su función probable es **pieza hero singular**, no kit modular automático. No promover a Valoria hasta completar esa puerta.
