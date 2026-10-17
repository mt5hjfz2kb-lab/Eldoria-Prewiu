# Valoria Reference Clean — escena real y fuentes editables

Abrir el proyecto Unity existente desde la rama `construction/valoria-master-plan-layers-20261016` y la escena `Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity`. Unity6000.3.23f1, URP17.3.0, glTFast6.14.1. Las escenas anteriores se conservan; esta escena es independiente y contiene una sola representación activa de cada estructura.

## Contenido editable

- TerrainData real y cauce continuo, río con profundidad visual, transparencia somera y espuma de contacto; puente con arcos y apoyos cerrados.
- Bastión compacto, puerta de acceso diferente, camino continuo, cuatro parcelas vacías y fragmentos de muralla; vegetación, rocas, banderas y antorchas modeladas.
- Prefabs en `Prefabs/`, mallas/materiales/TerrainLayers/pipeline URP en `GeneratedArt/`, shaders y componentes de cámara/selección dentro de la misma carpeta de escena.
- GLB en `Sources/Valoria_Reference_Clean.glb`; fuente Blender empaquetada en `docs/evidence/valoria-reference-clean/Valoria_Reference_Clean.blend`; paquete de intercambio `Valoria_Reference_Clean.unitypackage` junto a este documento. Preferir el proyecto completo: el paquete requiere las dependencias del proyecto y Terrain Physics habilitado.
- Selección del Bastión y cuatro parcelas, arrastre/zoom/encuadre de cámara con soporte táctil y límites de terreno. Preparación para futuras construcciones; no se ha sustituido el sistema económico ni el guardado canónico.

## Última validación visual y jugable

Fuente ejecutada `f9cc6a6999626d5e0067960a7fa2c80675e07dd4`; salida nativa `2c3d6174fe78ea5cf4031a4611dfb228d60019ac`; [ejecución Unity/Blender46](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39079638661).

Las capturas `A-blockout.png`, `B-architecture.png`, `C-art-scene.png`, `D-final.png`, `detail-bastion.png`, `detail-gate-bridge.png` y `playmode-home/pan/zoom.png` proceden del render real de Unity. `water-before/after-main/detail.png` y `water-comparison.json` demuestran el último cambio manteniendo cámara, iluminación y malla. Las pruebas de piedra, terreno, orilla y vegetación permanecen en el historial Git; los candidatos rechazados no se promocionaron automáticamente.

`playmode.json`:96/96 recorridos y gálibos,6 escalones,5 selecciones,una cámara,cero excepciones,zoom/pan/home reales y guardado canónico intacto;12/12 rayos de las esquinas de cámara sobre Terrain en formato panorámico móvil. Se conservan las protecciones contra geometría corrupta en Play: sin static batching sobre las mallas glTF y comparación estricta Editor/Play.

La escena contiene635048 triángulos de malla,35 renderers y163 slots de material. Texturas referenciadas:121822558 bytes de memoria nativa en Windows. Render de cámara más lectura GPU a960×540:25.76ms. Estas medidas no son FPS ni memoria de un teléfono; los ceros de UnityStats en batch no significan cero draw calls.

## Herramientas y procedencias verificadas

Blender5.2.2 LTS realmente ejecutado. En46 BlenderMCP ejecutó una acción tipada que modeló22 cantos cerrados junto al agua, usando el complemento ya instalado y su hash verificado `eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5`. Se reutilizó el transporte histórico de38612645467/38766521237, con servidor temporal local y cierre al terminar, sin instalar ni habilitar servicios de pago. Evidencia: `blendermcp-art-operation.json` y `native-tool-check.json`. La ausencia de un servicio permanente en9876 no describe esta vía temporal.

El cerebro existente Qwen3-VL8B revisó cuatro imágenes auténticas en43; `qwen-review.json` conserva imágenes/hashes/modelo/inferencia y crítica con incertidumbre. Es una ayuda, no aprobación artística. Las decisiones de esta escena y recetas de modelado se atribuyen a Codex; no se declara una autoría Qwen inexistente ni entrenamiento de pesos.

Coste de recursos externos0€. Quaternius CC0 recuperado y transformado; cuatro materiales Poly Haven CC0 verificados, originales conservados y derivados adaptados. Procedencias, licencias y rechazos en `resource-licenses.json`, manifiestos de MaterialInputs y `source-art.json`. No hay imágenes generadas presentadas como Unity.

## Validación móvil y reproducción

La compilación aislada ARM64/IL2CPP se solicita en47 con el adaptador Android existente; el resultado definitivo quedará en `android-build.json` y el APK en el artefacto de esa ejecución, sin versionar el binario en Git. Hasta leer ese resultado, no declarar APK compilado. ADB está disponible pero no hay dispositivos Android autorizados; tampoco se ha perfilado un dispositivo iOS. No se certifica rendimiento físico, objetivo8/10 ni `ART_APPROVED`.

Reproducción: receta `tools/valoria-reference-clean/author_scene.py` y workflow `.github/workflows/valoria-reference-clean.yml`, solo con claim exclusivo real y herramientas del PC libres. El workstream conserva la reserva durante la compilación47. No se modifica el arte de main ni se reanudan trabajos pausados.
