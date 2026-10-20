# ELDORIA — Tripo API | Lecciones verificadas del Jabalí de roca (v1)

Fecha de registro: 2026-10-21. Base: `main`. Estado: **evidencia técnica real + valoración artística provisional**, no aprobación de producción móvil.

## 1. Caso real: qué se logró

La entidad **`boar` / «Jabalí de roca»** ya existe en `v0220/js/gameplay.js` como presa de nivel 1, capítulo III. Se fabricó **una sola vez** mediante **Tripo API H3.1** (`v3.1-20260211`) a partir de un texto; no se generaron variaciones pagadas. Task exacta `28f57ff9-b978-468c-8c2a-6136e0a65ea1`; factura registrada: **20 créditos API**. [Run de generación](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39495619494).

| Paso | Evidencia y resultado | Limitación |
| --- | --- | --- |
| Tripo → GLB | Original 84.178 triángulos, [fuente SHA 166c32…](../ArtSource/World4X/RockBoar_v1/PROVENANCE.md) | Una sola malla; no hay rig ni animaciones |
| Blender → GLB | 49.800 triángulos, 34.125 vértices; GLB y BLEND editables preservados | Reducción de ~40,8 % en triángulos, no prueba de LOD móvil final |
| Unity 6000.3.23f1 | 1 malla, 1 renderer, 1 material; 3 texturas; UV0 y normales PASS | Ejecución aislada; no es mapa real |
| BaseColor | 2048 × 2048 **en Unity** tras habilitar dependencias | Comprobar por resolución importada, no solo «texturas=3» |
| Selección | Raycast positivo y clic vacío negativo PASS | Collider del ensayo no implica integración jugable |
| Cámaras | Cuatro PNG **auténticos** en `ArtSource/World4X/RockBoar_v1/UnityQA/` | No se ha perfilado FPS, RAM ni draw calls en Android/iOS |

Fuente permanente: [`ArtSource/World4X/RockBoar_v1/`](../ArtSource/World4X/RockBoar_v1/) · [métricas](../ArtSource/World4X/RockBoar_v1/UnityQA/boar-unity-metrics.json) · [Unity con 2K / run 39505056649](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39505056649) · [archivado en main / run 39505996400](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39505996400).

## 2. Valoración artística independiente

**Estimación visual provisional del modelo aislado: 7/10**; **preparación para entrega móvil animada: aproximadamente 4/10** (juicio artístico y técnico, no benchmark ni certificación). La lectura próxima es buena: se reconoce el jabalí, cuerpo robusto, colmillos, pelaje y placas pétreas diferenciales; formas tridimensionales reales. Limitaciones observadas: placas que parecen superpuestas, patas/contacto a suelo discutibles, irregularidad anatómica y pérdida de detalle a cámara estratégica. La silueta se mantiene reconocible, pero las rocas se vuelven manchas a distancia.

**No afirmar** que `TECH PASS` de importación equivale a `ART_APPROVED`, ni que el engendro y el próximo prop tendrán calidad equiparable. No se han medido horas humanas ahorradas, repetibilidad estadística ni rendimiento de móvil. Las capturas del proyecto mínimo no sustituyen las capturas de la escena 4X oficial.

## 3. Fallos reales y soluciones ya demostradas

### 3.1. GLB 2K → texturas 4×4 en Unity

**Síntoma:** el GLB conservaba el albedo 2048² pero glTFast creaba materiales con texturas provisionales 4×4. Un primer cambio de color del shader corrigió tintes, **pero no** la resolución. Causa diagnosticada: proyecto **aislado** creado únicamente con `com.unity.cloud.gltfast: 6.14.1` carecía de módulos Unity obligatorios para imágenes:

- `com.unity.modules.imageconversion: 1.0.0`
- `com.unity.modules.unitywebrequest: 1.0.0`
- `com.unity.modules.unitywebrequesttexture: 1.0.0`

La escena principal Eldoria ya incluía esos módulos. Añadirlos al manifiesto aislado (no modificar la escena jugable) y **asertar la resolución BaseColor importada >=1024**, junto con UV/normales/collider, solucionó la importación. Prueba real: [run 39505056649](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39505056649). Historia previa del mismo defecto: [gate Tripo de Valoria](VALORIA_TRIPO_MODULE_UNITY_GATE.md).

### 3.2. Unity completo bloqueaba una simple prueba de arte

La primera evaluación del jabalí falló en el gate de importación del proyecto Unity completo por timeout, **después** de haberse pagado la generación. El resultado se recuperó **del mismo artifact**, no se repitió el gasto. El QA posterior utilizó un proyecto Unity mínimo, glTFast y módulos de imagen, recuperando exactamente el GLB validado por SHA. [Scripts ya existentes](../tools/rock-boar-unity-qa/) y [run PASS 39505056649](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39505056649).

### 3.3. Preservar archivos antes de seguir

El flujo **debe subir por separado el GLB sin optimizar** antes de Blender, conservar `.blend` y GLB optimizado, registrar `task_id`, créditos, SHA-256 y resultados, y archivar fuentes en `ArtSource/` fuera de `Unity/Assets` para no aumentar builds I–II. El artefacto de GitHub es temporal; la fuente en `main` es duradera. Nunca repetir automáticamente una petición Tripo ante fallo posterior en Blender/Unity.

### 3.4. Perfil móvil y actuación

49.800 tris del jabalí son **referencia artística/LOD0 de review**, **no presupuesto universal**. Para mapa 4X y fauna repetida: evaluar LOD por ocupación real en pantalla, impostor si aporta, compresión 2K/1K por distancia y número de instancias, antes de `Resources`/Addressables. El GLB no trae esqueleto, skinning ni movimiento; no puede certificarse como criatura animada. No convertir un modelo estático en «enemigo integrado» por colocar un prefab.

## 4. Evidencia disponible / no disponible

**Conocido:** una generación real autenticada costó 20 créditos; se produjo GLB, `.blend` y 4 renders verdaderos; importación 2K, UV, normal y raycast probados. **No conocido:** coste real de lotes, tasa de fracaso de otras especies, deformación en animaciones, rendimiento físico Android/iOS, tiempo completo de corrección repetible o aceptación estética desde la cámara canónica 4X.

## 5. Decisión reproducible

Tripo **sirve como fuente de geometría y texturas**, no como ensamblador de Eldoria. Orden: comprobar necesidad real/lore → escoger recurso existente vs. Tripo → brief de silueta/cámara y escala → cuenta API/plan, precio y aprobación de gasto **por activo** → una solicitud, sin retries pagados → preservar original → Blender con edición y reparación explícita → QA Unity mínimo con dependencias de imágenes y 2K efectivos → revisión artística de 4 vistas y vista estratégica → perfil móvil/LOD → **integración separada y aprobada**, si procede.

**NUNCA** reemplazar las escenas de Bastión I–II, alterar progreso, hotspots, guardados o gastar saldo por presencia de una herramienta. Ver normas del [pipeline Tripo canónico](TRIPO_MODULE_PIPELINE.md), [contrato de calidad artística](ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md) y [herramientas de autoría](ELDORIA_TOOLCHAIN_AUTOMATION_V2.md).
