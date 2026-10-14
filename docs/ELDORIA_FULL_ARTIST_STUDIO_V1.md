# Eldoria — General-purpose Blender Artist Studio V1

**Estado: implementación aislada. NO certifica calidad visual, integración Unity ni móvil.**

## Propósito

El propietario exige un artista que MODELE de verdad assets completos, no que solamente evalúe capturas ni escoja parámetros para scripts procedurales de arquitectura. La API canónica anterior solo admite siete operaciones. Este backend independiente aumenta de forma sustancial las herramientas de autoría sin invadir los archivos de Bastión I actualmente reservados.

## Autoría real

El artista IA (Ollama/Qwen) observa referencias e imágenes originales, lee lecciones verificadas y decide cada acción del modelo: geometría explícita, primitivas, dimensiones, extrusiones, perfiles, modificadores, material PBR, UV, textura, reutilización, ensamblado, iluminación y cámaras.

Blender ejecuta las operaciones concretas con bpy y bmesh; ninguna torre, muralla o terreno está diseñado de antemano. Puede construir familias complejas con varios subobjetos y hasta 160 operaciones, dentro de un presupuesto geométrico verificable.

Operaciones soportadas: primitive, mesh, duplicate, transform, sculpt, extrude, inset, subdivide, bevel, modifier, boolean, material, uv, join, delete, light, camera, render_settings, texture, asset_info, native_operator, shader_graph y geometry_nodes. Blender ejecuta además operadores nativos seleccionados de modelado/UV y grafos reales de nodos de shader y geometría, cada uno con lista de tipos y parámetros de seguridad. El taller puede funcionar con Blender directo o un addon BlenderMCP local que debe verificarse mediante SHA-256 exacto antes de cargarlo. El transporte BlenderMCP aún requiere una prueba específica; no confundir código disponible con función certificada.

Modificadores incluidos: BEVEL, SUBSURF, SOLIDIFY, DECIMATE, REMESH, MIRROR, ARRAY, WEIGHTED_NORMAL, TRIANGULATE, SIMPLE_DEFORM y DISPLACE.

No significa literalmente toda la interfaz de Blender: rigging, animación, simulaciones, Geometry Nodes avanzados y pintura libre requieren futuras extensiones probadas. No fingir un 100 % inexistente.

## Ciclo autónomo local

1. artist_loop.py consulta el modelo Qwen LOCAL con la referencia visual del propietario y la memoria del artista.
2. Si se trabaja desde un GLB/BLEND anterior, inspect_source.py obtiene primero una captura auténtica, nombres reales de todos los objetos, dimensiones, UV y materiales sin cambiar el archivo fuente. Qwen propone después un plan original de acciones tipadas. Los errores se devuelven a Qwen para que decida su corrección, sin valores inventados por Python.
3. blender_toolbox.py aplica en Blender las operaciones sobre una COPIA nueva, preservando el recurso original.
4. Guarda el GLB real, el BLEND editable, hashes, operaciones efectuadas y capturas antes/después del renderer Blender con cámara fija.
5. Qwen critica esas imágenes reales y puede decidir un nuevo ciclo con otro enfoque.
6. La calidad ART APPROVED requiere revisión independiente y capturas Unity de la misma cámara, además de controles móviles. El modelo jamás se autoaprueba.

## Ejecución

Requisitos: Blender 4.x o 5.x, Python + Pillow, Ollama local con qwen3-vl:4b-instruct y la referencia visual.

Comando de producción:

    python3 tools/art-direction/full-studio/artist_loop.py \
      --brief "Modela una familia reutilizable de torre medieval a escala móvil" \
      --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
      --source path/al/asset-original.glb \
      --out /tmp/eldoria-studio-real \
      --max-cycles 2 --max-actions 28

Sin --source puede crear un asset nuevo desde escena vacía. Con --source edita una copia del archivo. --review permite fijar un JSON con cámara y render para el mismo encuadre.

Pruebas técnicas:

    python3 -m unittest discover -s tools/art-direction/full-studio/tests -p "test_*.py"
    blender -b --python-exit-code 1 --python tools/art-direction/full-studio/blender_toolbox.py -- --plan tools/art-direction/full-studio/tests/smoke_plan.json --out /tmp/eldoria-studio-smoke --texture-root .

El plan smoke_plan.json es una FICHA TÉCNICA FIJA, NO AUTORÍA QWEN. Solo prueba que Blender ejecuta realmente las herramientas.

## Integración y concurrencia

El trabajo vivo de Bastión I controla tools/art-direction/ai-authoring/**, herramientas de escena y runner Windows. No alterar esos archivos ni lanzar duplicados. Cuando se libere o delegue esa titularidad, integrar el nuevo backend en la API canónica vigente, conservar su memoria y pruebas, realizar una ejecución de autoría real Qwen→Blender y validar el GLB en Unity. NO promocionar automáticamente main ni marcar éxito visual por un CI verde.

## Salvaguardas

Sin Python arbitrario producido por IA, eval, comandos generados, descargas externas, créditos de pago ni escritura sobre origen. Texturas relativas restringidas a la raíz autorizada, verificando licencia antes de incorporar archivos ajenos. Limitaciones de polígonos, firmas y hashes. Blender no es Unity y el modelo no modifica sus propios pesos: la memoria es recuperación de experiencias comprobadas.

La ejecución crea un reusable-asset-catalog.json: geometrías independientes, materiales, dimensiones, pivotes, SHA de cada componente y procedencia. Es evidencia de reusabilidad y exportación, no aprobación estética ni de licencias de terceros.
