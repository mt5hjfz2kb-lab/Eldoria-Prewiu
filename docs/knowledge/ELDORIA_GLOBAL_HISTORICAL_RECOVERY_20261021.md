# Eldoria — recuperación global de conocimiento artístico (21-10-2026)

**Estado:** recuperación documental y de métodos, consolidada como inventario de solo lectura. **No se han reactivado ramas ni ejecutado escenas experimentales.** GitHub sigue teniendo una sola rama canónica de desarrollo: `main`.

## Alcance comprobable

| Dimensión | Resultado |
|---|---:|
| Ramas históricas inventariadas en el registro 18/10 | 201 |
| Ramas contrastadas contra `main` mediante comparación de commits/rutas | 201/201 |
| Ramas con rutas artísticas/técnicas en comparación | 154 |
| Ramas sin coincidencias de rutas bajo el filtro usado | 47 |
| Comparaciones limitadas a 300 ficheros que recibieron comprobación de árbol completo | 8/8 |
| Ramas prioritarias con lectura de una o varias fuentes técnicas/artísticas originales | 20 |
| Ramas con cribado de ficheros, sin revisión exhaustiva de sus resultados | 181 |
| Errores de comparación archivados | 0 |
| Fichas previas de fase 1 | 27 |
| Fichas nuevas con fuentes históricas fijadas por SHA | 27 |
| Fichas ahora en memoria común | 54 |

**Importante:** estos números **NO** equivalen a 201 ramas revisadas archivo por archivo ni a 54 técnicas aprobadas. Las 201 se cribaron automáticamente a nivel de rutas de archivo y las 20 priorizadas se estudiaron mediante sus informes/código. Las otras 181 tienen cobertura **de inventario**, no certificación del contenido individual. Algunos ficheros comparados fueron modificados o heredados y su presencia no demuestra que un experimento sea útil.

### Evidencia y trazabilidad

- [Índice único actualizado (54 fichas)](../../pipeline/art-knowledge/catalog-v1.json): problema, técnica, resultado, advertencias, vínculo al archivo exacto y SHA de rama histórica.
- [Cobertura completa de las 201 ramas](../../pipeline/art-knowledge/historical-branch-screening-v1.json): cada nombre, commit congelado, profundidad de auditoría, limitación de comparación y rutas representativas.
- [Comparación adicional de los ocho árboles completos](../../pipeline/art-knowledge/historical-audit/truncated-tree-crosscheck.json): evita depender únicamente del límite de 300 ficheros de GitHub.
- [Detalle de comparaciones por tramos](../../pipeline/art-knowledge/historical-audit/): trece ficheros que conservan los resultados de las consultas originales.

**Precisión:** las comparaciones tomaron como base el `main` vigente en cada consulta; como había otros trabajos avanzando en `main`, no son un diff inmóvil contra un único commit base. Todas las referencias históricas recuperadas están fijadas a SHA. Los ocho árboles completos sí se contrastaron además contra una misma fotografía de `main`, definida en su informe. Para reutilizar una técnica deberá revalidarse contra `main` actual.

## Descubrimientos principales

### A. Tenemos un artista Blender profesional más amplio que el usado habitualmente

La rama congelada [`art/eldoria-full-blender-artist-studio-20261014`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/tree/65add927cde3722407906808f93f22035c975d4c) conserva la caja de herramientas `full-studio`: esquemas tipados para Qwen, inspección de fuentes, extrusión, biseles, malla, UV, booleanos, comprobaciones de coherencia, operaciones semánticas y recuperación de lecciones transferibles.

**Valor rescatable:** capacidad de actuar sobre geometría real y rechazar cubiertas desconectadas, piezas enterradas, reutilización sin licencia, y planes espaciales que no afectan a la imagen.

**Límite:** la rama era una implementación aislada. Un catálogo grande de herramientas no demuestra autoría autónoma de calidad ni compatibilidad directa con el pipeline de producción actual. No fusionar todos sus scripts.

Fichas: `K28–K31`.

### B. Disponemos de siete fuentes arquitectónicas estudiadas y 63 GLB modulares

La familia histórica V2 de siete fuentes se revisó en su momento frente a una puerta V3 y contiene Blender, GLB/LOD y texturas editables, con aprobación **en el ámbito de fuente**, no del juego actual. El antiguo trabajo de 63 GLB contiene ensayos, ganadores por SHA, errores de UV, pivote y material, recursos CC0 y negativos precisos.

**Valor rescatable:** operaciones en sillería, estructuras físicas de escaleras, roca natural no primitiva, texturizado con criterios de escala y elección del mejor candidato por resultado (no por fecha).

**Límites:** material `TEXCOORD_0` ausente en nueve piezas históricas, cuatro pivotes de dovelas por revisar, agua con extensiones glTF potencialmente incompatibles, licencias de origen que requieren comprobación, 0 certificaciones de calidad final de aquella biblioteca. Las referencias artísticas antiguas han sido sustituidas.

Fichas: `K32–K37`.

### C. El problema de Qwen no era solo potencia de hardware

Los estudios de Qwen 4B y 8B resolvieron reglas geométricas expresas, pero fallaron en la interpretación arquitectónica libre de varias vistas. Además, el estudio profesional documenta que el historial de un acantilado introducía coordenadas equivocadas en un brief de torre.

**Valor rescatable:** inspección XYZ real, contraste semántico de imágenes, selección de memoria por relevancia, guardias de escala/centro, pruebas pareadas de regresión y rechazo previo a Blender.

**Límite:** no se puede tratar la memoria como reentrenamiento ni concluir que Qwen razona profesionalmente por acertar 7/7 preguntas con reglas explícitas.

Fichas: `K30`, `K38`, `K39`.

### D. Los rechazos de composición son posiblemente tan valiosos como los éxitos

Se han recuperado resultados auténticos de Golden Transfer, lenguaje visual unificado, Source-Level Art Direction, reautoría semántica y montaje híbrido 3D/2,5D. En varios, **Unity PASS y un conjunto de materiales correctos convivieron con VISUAL FAIL**. Esto explica por qué la estética no avanzó al ritmo de los experimentos.

**Reglas negativas conservadas:** no reintroducir capas planas o bordillos para arreglar macrocomposición; no convertir una roca en arquitectura con PBR si la forma no funciona; no multiplicar prefabs sin contacto; no cubrir fuentes ricas con tiras de bajo detalle. Para World4X se documentaron regresiones de materiales y problemas de `.meta`/GUID.

Fichas: `K40–K54`.

## Qué sí pasa al sistema actual

**Conocimiento y referencias, NO código operativo sin validar.** El catálogo incluye las rutas de fuentes históricas con SHA y por tanto un ejecutor autorizado puede recuperar el método, sus pruebas negativas y el recurso exacto. La prioridad de selección debe basarse en: relevancia del fallo visible, estado del método, calidad de evidencia, compatibilidad con el canon y conservación de jugabilidad.

La biblioteca histórica de roca, la puerta, los siete modelos y los módulos individuales son candidatos a **reutilización selectiva**, nunca una orden de incorporarlos a Valoria. El canon 20-10 manda: no parcelas visibles, no campamentos/barrio no desbloqueado y no acantilados enormes en Bastión I–II.

## Qué sigue sin demostrarse

- No se han inspeccionado manualmente todos los miles de PNG/GLB de las 201 ramas ni vuelto a calcular todos sus SHA binarios.
- Los 181 archivos de rama clasificados como cribado siguen disponibles para inspección dirigida cuando un problema real necesite una fuente concreta. Su estado no se eleva a `ART_APPROVED`.
- Ninguna mejora adicional de arte ni QA Android/iPhone se ha ejecutado en esta recuperación.
- La conexión automática entre este catálogo y Qwen, la escritura de aprendizaje al finalizar cada iteración y el benchmark de ahorro de tiempo son **fase 2/3**, todavía no realizados.
- Los artifacts de GitHub que caducaron deben tratarse como no verificables hasta recuperar sus bytes; el enlace/manifest histórico no sustituye los originales.

## Integridad del trabajo

Se incorporan únicamente ficheros nuevos de análisis/documentación y la actualización del JSON/guía de memoria. No se cambian imágenes canónicas, `AGENTS.md`, `active-workstreams`, escenas, shaders, Blender existente, scripts ejecutables, workflow, pubicaciones ni Android. Sin Tripo, sin coste de generación, sin uso del runner Windows.

**Cierre:** fase 1 ampliada = **inventario global de 201 ramas + recuperación selectiva de conocimiento de 20 ramas prioritarias + 54 fichas en memoria**, con incertidumbres explícitas. No es una certificación universal de los experimentos ni el inicio de fase 2.
