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

## Aislamiento del taller, realismo de cámara y respuesta corta del modelo

La primera inferencia artística con Qwen 2B falló porque copió operaciones históricas sobre el acantilado, no la torre solicitada. La segunda produjo JSON completo pero parámetros de malla incorrectos. Ambos fallos están registrados en learning_cases.json con los hashes y artefactos originales.

La corrección no sustituye creatividad por geometría preestablecida: **el primer ciclo de un asset nuevo está acotado a unas pocas formas y proporciones elegidas por Qwen**, con JSON estrictamente validado, y los ciclos sobre un BLEND ya existente ofrecen el resto de herramientas (edición, texturas, materiales, escultura, Geometry Nodes, etc.). Esto permite construir de abajo arriba sin pedir a un modelo pequeño miles de parámetros en una sola inferencia.

La memoria se recupera como lecciones verificables desde el diario canónico y el registro de fallos del estudio. Se eliminan del contexto los identificadores, geometrías y coordenadas obsoletos; sus hashes quedan preservados. No hay entrenamiento de pesos.

La escena de revisión contiene iluminación, suelo neutro de contacto y cámara orientada realmente al asset. **Estos recursos de presentación quedan fuera del GLB reutilizable**. Los renders A/B de Blender se miden para rechazar propuestas invisibles, pero nunca equivalen a una certificación Unity ni a calidad artística por sí solos.

## Biblioteca artística con reutilización verificable

El artista puede realizar la operación **import_asset** para importar un GLB/GLTF existente de la biblioteca aprobada. Para evitar reconstruir y perder assets de calidad, puede elegir piezas previamente disponibles e integrarlas en un conjunto nuevo. Blender importa los objetos reales preservando materiales y procedencia y aplica únicamente los cambios artísticos elegidos por el modelo.

El archivo --asset-catalog es un JSON con schema_version=1 y assets con relative_path (relativa a --texture-root), sha256, license (OWNED, CC0 o CC-BY-4.0), reuse_approved=true y attribution obligatoria para CC-BY. El ejecutor rechaza rutas fuera de la raíz, fuentes alteradas, licencias no aprobadas o importaciones vacías. El manifiesto del nuevo asset conserva SHA y licencia por cada submalla. No hay descargas automáticas ni créditos de pago.

Una prueba técnica independiente utiliza el GLB generado en la primera prueba de Blender, crea un catálogo OWNED con su SHA exacto, lo vuelve a importar mediante import_asset y comprueba la preservación de geometría, inventario, licencia y origen. Esa prueba NO afirma que Qwen eligiera dicho archivo: el fixture es puramente técnico.

## Autoría visual comprobable y rechazo de composiciones defectuosas

El primer asset diseñado realmente por Qwen quedó archivado en la ejecución 38568103614: pasó el ciclo Qwen→Blender con modelos BLEND/GLB editables y comparación A/B, pero la imagen mostraba volúmenes superpuestos, tejado separado, partes hundidas y materiales de prototipo. Era una demostración de **autoría técnica real, NO de calidad**.

La nueva comprobación composition_health.py rechaza composiciones físicas incoherentes en la etapa inicial de arquitectura: piezas enteras enterradas, tejados desconectados o por debajo del soporte, repetición de volúmenes idénticos y ausencia de alturas diferenciadas. Devuelve los defectos a Qwen para que él mismo rediseñe la escena; no introduce geometría automática.

La revisión Qwen usa ahora un contrato JSON de crítica acotado, mientras la verificación de presencia en pantalla y revisión artística permanecen separadas. La prueba real de reautoría toma un .blend congelado generado por Qwen, comprueba su SHA, pide nuevas operaciones originales sobre objetos inspeccionados y crea un nuevo .blend sin alterar la fuente. El objetivo es probar mejora acumulativa real. Un PASS de Blender no constituye un resultado visual aprobado.

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

## Corrección de aceptación de mallas heredadas (2026-10-14)

La ejecución 38576788822 no satisfizo la misión: una inferencia Qwen propuso y Blender ejecutó transformaciones, pero el hash de datos locales de la malla heredada NO cambió. Un cambio de posición de objeto es una transformación espacial, no una edición de vértices ni topología. Se registra ahora `spatial_transform_changed` por separado y la autoría sobre malla exige `inherited_mesh_data_changed` y `originals_changed` en una operación `sculpt` auténtica.

Para una prueba de autoría no simulada, la fase `--phase mesh_edit` obliga al modelo local a inspeccionar nombres y límites locales reales de los objetos en un GLB existente, elegir él mismo centro/radio/desplazamiento de pincel, y ejecutar la edición en Blender. No se añaden vértices predeterminados ni plantillas arquitectónicas; una operación que no toque vértices reales queda rechazada. Fuente de aceptación: `Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_Tower_CohV1.glb`, sin sobreescribirla. El plan bruto Qwen, los cambios de datos de malla y las capturas A/B deben verificarse en los artefactos de la ejecución; un test PASS sin la etapa Qwen NO equivale a cumplimiento.

Invocación de edición real: `python3 tools/art-direction/full-studio/artist_loop.py --brief 'Retoca la silueta de la torre de Valoria preservando su identidad' --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg --source Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_Tower_CohV1.glb --out /tmp/eldoria-tower-unique --asset-category architecture --phase mesh_edit --max-cycles 1 --max-actions 4`. Exige Ollama/Qwen local, Blender y permisos sobre directorios de salida, nunca créditos de pago.

### Corrección de contrato para Qwen local 2B y nombres nativos (segunda prueba)

La primera prueba sobre la torre real 38580635610 quedó rechazada antes de Blender: tres intentos devolvieron movimientos `delta=[0,0,0]` y algunos IDs de acción con `/`; además, el GLB importado usa un nombre legítimo que contiene `·`. El validador no permitía ese carácter y la fase de edición carecía de anclajes geométricos seguros.

La nueva operación tipada `sculpt_anchor` sigue siendo **decidida por Qwen**, no una corrección predeterminada: el modelo selecciona objeto real, extremo local de la malla (entre seis localizados durante inspección), eje y signo del desplazamiento, amplitud no nula y radio. Blender resuelve el índice exacto del vértice seleccionado y esculpe la malla heredada; las operaciones y los hashes de vértices antes/después prueban la autoría efectiva. El operador no construye otra torre ni decide un diseño por defecto. Se admiten nombres reales con punto medio y IDs de registro `/0` sin exponer código ni rutas del sistema. Una prueba que no edite datos locales de la malla original seguirá fallando.
