# Eldoria Blender Professional Authoring Pipeline v1

Status: **CANONICAL**
Effective: 2026-10-04
Scope: player-visible Valoria environment geometry authored or materially reauthored in Blender.

## Purpose

Blender is not merely a geometry generator or cleanup step. For player-visible PRIMARY, SECONDARY and HERO environment art, Blender must be used as an **artist-authoring environment**.

The production target is not “technically valid geometry”. The target is geometry that reads as if an experienced environment artist designed it specifically for Valoria and for the official gameplay camera.

## Core rule

A Blender source does **not** become production art because it exports correctly, has many objects or triangles, uses bevels, contains PBR material slots, was created by Python, or passes CI.

Visible production art must demonstrate deliberate authorship in silhouette, massing, depth, hierarchy, material response and integration.

## Required authoring sequence

1. **Screen-space brief** — identify the exact frame defect and camera views; define intended silhouette/hierarchy against the canonical reference; define interfaces to terrain, circulation and adjacent assets.
2. **Primary forms** — establish massing, silhouette, proportions and controlled asymmetry before detail. Prefer direct mesh editing, extrude/inset, controlled booleans, curves, deformation and purpose-built modular pieces over primitive stacks.
3. **Secondary architecture** — add real depth: recessed openings, wall thickness, cornices, buttresses, stairs, arches, parapets, plinths, roof/eave thickness, believable returns and transitions. Break visible repetition deliberately.
4. **Organic/damaged interfaces** — use sculpting, displaced geometry, controlled noise or authored rock breakup only where it materially improves rock/stone transitions, wear or silhouette.
5. **Material-ready source** — UVs and texel density appropriate to the official camera; canonical Eldoria material families; high-to-low bake of normal/AO/curvature/height when useful; check value relationships against adjacent promoted art before Unity.
6. **Isolated art review before integration** — silhouette/clay preview, lit 3/4 preview, official-camera proxy preview when practical, wireframe/UV diagnostics when relevant. Reject before Unity if it still reads as procedural, primitive, overly symmetrical or generic.
7. **Unity integration** — deterministic export, gameplay authority preserved, official-view capture and integrated visual verdict.

## Professional-authoring toolbox

Use when appropriate:
- direct mesh edit;
- extrude / inset;
- selective bevel;
- controlled Boolean;
- Solidify;
- curves and profiles;
- lattice/deform;
- Array only for genuinely repetitive construction;
- Mirror only where symmetry is architecturally intentional;
- Geometry Nodes for reusable artist tools, trims, controlled scatter/breakup, masks and secondary systems;
- sculpt / voxel-remesh / multires for rock, damage and organic transitions;
- weighted/custom normals;
- UV unwrap and texel-density normalization;
- high/low baking;
- trim/tileable/unique-mask workflows;
- LOD generation after visual approval.

No tool is mandatory merely because it exists. Use the minimum set that produces the intended art.

## Rejection patterns

For visible PRIMARY/SECONDARY/HERO work, these are presumptively non-production until visual evidence proves otherwise:
- cube/cylinder/cone stacks as the main silhouette;
- mathematically repeated bays with no authored variation;
- equidistant windows/buttresses/merlons used as the whole design;
- flat color Principled materials presented as final surface work;
- perfect symmetry without architectural reason;
- uniform beveling everywhere;
- procedural noise used to simulate authored damage;
- “more polygons/modules” as the justification for quality;
- scripts that only assemble primitives and then label the result authored.

Primitive operations remain valid for blockout, hidden SUPPORT geometry and as intermediate construction steps inside a genuinely authored final piece.

## Art-review questions

Before Unity integration:
1. Does the silhouette look intentionally designed rather than generated?
2. Are primary and secondary forms readable from the actual gameplay camera?
3. Is there meaningful depth rather than surface decoration?
4. Is repetition controlled and broken where visible?
5. Does the piece have a clear relationship to neighboring Valoria architecture?
6. Do material values and surface scale sit inside the canonical family?
7. Does the piece improve the target frame without relying on props or explanation?

If 1, 2 or 7 is no, do not integrate it as a production candidate.

## Automation role

Automation should construct repeatable artist tools, save reproducible .blend sources, generate previews, record source/export identities and metrics, run diagnostics, bake/export, and produce Unity evidence.

Automation must **not** treat object count, triangle count, modifier count, successful export or workflow success as proof of art quality.

## Geometry Nodes policy

Geometry Nodes are encouraged as **artist tooling**, not as an excuse to generate every visible building from one generic grammar.

Good uses include controlled stone breakup, trim placement, authored scatter masks, roof/detail variation, repeated structural sub-elements with manual overrides, and reusable damage/aging systems.

## Sculpt / high-poly policy

Use sculpt/high-poly only when it improves visible form or baked surface information, especially rock-to-architecture transitions, broken stone edges, erosion/damage, hero relief and irregular foundations. Do not sculpt detail that disappears at the official camera.

## Surface and bake policy

Where high-frequency detail matters, preserve a high source, make a controlled low/production mesh, unwrap intentionally, bake normal/AO/curvature/height as justified, and validate the bake visually in the real game frame. A technically successful bake is not automatically visually equivalent.

## Source report minimum

New professional Blender sources should record:
- standard: BLENDER_PROFESSIONAL_V1;
- source .blend path + SHA;
- authoring method summary;
- primary operations/tool families used;
- whether primitive operations were only intermediate;
- triangle/material/UV/normal/tangent/bounds metrics;
- material families and maps;
- preview evidence paths;
- isolated art-review verdict;
- export GLB path + SHA;
- intended official-camera role;
- Unity visual verdict after integration.

## Relationship to Tripo

Tripo remains optional geometry sourcing. If used:
approved reference -> generation/parts -> **Blender professional reauthoring/cleanup** -> art review -> Unity.

A generated GLB is never exempt from this authoring standard when it becomes a high-salience production asset.

## Canonical lesson from Structural Frame v1

VALORIA STRUCTURAL FRAME / URBAN SUPPORT INTEGRATION v1 proved that changing from certified-wall recomposition to new Blender geometry is directionally useful, but it also exposed the weakness of formulaic primitive-based source construction: the candidate gained architectural vocabulary yet still read as a separate inserted system.

Future visible Blender work must design the complete screen-space form + material relationship first, then author the 3D source deliberately.

Do not open endless v2/v3 procedural variants of a failed source technique.

## Promotion rule

TECH PASS and ART SOURCE PASS are separate.

A production candidate needs:
- TECH PASS;
- isolated ART SOURCE PASS;
- integrated VISUAL PASS.

If any one is missing, do not promote.

## Regla permanente de continuidad y reutilización de capacidad artística

**Mandato del propietario (18-10-2026). Vigencia: todas las futuras necesidades de arte de Eldoria sobre la base canónica integrada.** Esta norma complementa los criterios de autoría anteriores; no abre una tarea nueva, no desplaza la ejecución actual y no autoriza ramas, reinicios, gasto ni sustitución masiva de assets.

**Principio:** Cada vez que el juego necesite crear, corregir, ampliar o sustituir un modelo 3D, **partir de la capacidad profesional y los recursos ya demostrados**, no volver por defecto a construcciones básicas ni reinventar desde cero el pipeline. Reutilizar herramientas, fuentes editables y procedimientos *que produjeron mejora artística visible*, no simplemente los que exportaron con éxito.

### Secuencia obligatoria de decisión y ejecución

1. **Recuperar el estado vivo:** leer `main`, el workstream vigente, el último asset integrado, sus interfaces jugables y sus evidencias. Respetar el alcance y las tareas activas. Las ramas antiguas sirven exclusivamente como referencia puntual; no reactivarlas ni fusionarlas completas.
2. **Reutilizar antes de recrear:** consultar bibliotecas, prefabs, fuentes `.blend`, scripts, materiales PBR y familias modulares actuales. Decidir con justificación visual si conservar, reparar, remodelar parcialmente o crear. No producir una variante arquitectónica si lo que falta es acabado.
3. **Recuperar la técnica que dio resultado:** localizar ejemplos reales con **fuente/commit, operaciones de Blender, exportación, capturas de Unity y veredicto independiente**. Revisar tanto los éxitos como los fallos. Ejemplos verificables en `docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md`: intervención controlada de orillas native61; arquitectura Bastión native70 y entorno native73, integrados y validados en native74; fallos de materiales blancos en native69 y pinos sobredimensionados rechazados en native77. Cada ejemplo demuestra *un alcance concreto*, no excelencia universal ni garantía para el siguiente asset.
4. **Autoría profesional donde aporta valor:** construir sobre la geometría existente y emplear en Blender la combinación pertinente de malla directa, inset/extrude, curvas, booleans, biseles selectivos, sculpt, Geometry Nodes, UV, normales, materiales y bake. Usar Qwen/Ollama como dirección o crítica auxiliar con contexto geométrico e imágenes reales; BlenderMCP con acciones tipadas y validadas cuando esté disponible; `bpy` probado como alternativa sin detener la producción. No ejecutar código arbitrario generado por el modelo ni instalar un agente duplicado.
5. **Mejorar la imagen, no solo las métricas:** revisar silueta, profundidad, detalle y relaciones con su entorno en la **cámara real del juego**. Comprobar textura, UV, materiales y color dentro de Unity URP (incluida persistencia de mapas). Exigir antes/después de un cambio significativo con encuadre e iluminación comparables, y evaluación visual independiente. TECH PASS o aprobación verbal de Qwen no equivalen a ART PASS.
6. **Promover sin romper el juego:** conservar fuentes editables, identificadores de objetos/prefabs y los sistemas de selección, construcción, colisiones, navegación, guardado y evolución de Bastión I–II. Integrar selectivamente solo mejoras verificadas; repetir QA funcional y comprobar orientación/rendimiento móvil conforme al alcance. No aumentar polígonos, efectos o atlas sin beneficio perceptible.
7. **Conservar el conocimiento práctico:** registrar brevemente en la evidencia del asset qué fuente y operaciones se reutilizaron, qué defectos se corrigieron, el enlace a capturas comparativas y el veredicto `ACEPTADO / RECHAZADO`. Esto es **reutilización de técnicas documentadas**, no aprendizaje o reentrenamiento de los pesos de Ollama/Qwen.

**Límites permanentes:** 0 €, ningún Tripo/Tripo AI ni proveedores de pago, no proyectos o bibliotecas paralelas, no cambios de prioridad por el mero hecho de leer esta norma. Aplicarla cuando lo necesite el desarrollo jugable y visual ya autorizado, siempre desde el último `main` verificado. Ningún compromiso de calidad 7–8/10 se declara cumplido sin evidencia en la escena integrada.
