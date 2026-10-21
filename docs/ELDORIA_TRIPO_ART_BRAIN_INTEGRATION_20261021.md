# Eldoria — Tripo API + cerebro artístico | Integración de preproducción sin créditos

Fecha: 2026-10-21. Autoridad: `AGENTS.md` y `main`. Estado: **preflight de fuentes y puente para el Qwen existente verificados en GitHub Actions; NO inferencia Qwen nueva ni rigging o ART PASS**.

## Qué se ha incorporado

- **Herramienta reutilizable, no un segundo pipeline:** `tools/tripo-art-brain/preflight.mjs`. Comprueba SHA-256 verdadero de GLB raw/optimizado, estructura GLB y conteo real de triángulos, UV0/normales, métricas Unity verificadas, PNG de las cuatro vistas y fallos de materiales 4×4. En el Engendro comprueba además las mallas de dos LOD y los hashes de la integración existente (se conservan hotspot, colisiones y guardados).
- **Catálogo de fuentes exactas:** `pipeline/tripo-art-brain/catalog.json`, con tres task IDs y 60 créditos históricos, sus originales y defectos documentados. El Engendro ya tiene near/far en el runtime de Bastión II; el Jabalí pertenece al alcance futuro de Bastión III y el Foco es arte experimental sin nodo jugable.
- **Puente al artista Qwen:** comando `prepare` genera `qwen-inventory.txt` con proveniencia, contexto espacial/narrativo, defectos y rutas de mejora; el artista ya existente lo acepta como `--inventory` en `tools/art-direction/ai-authoring/author_brief.py`. **La preparación no ejecuta Ollama/Qwen, Blender ni Unity** y no debe registrarse como inferencia o mejora.
- **Planificador canónico:** genera una solicitud inerte con `allow_tripo=false`, `credit_authorization.approved=false`, `authorized_credit_cost=0`, `enabled=false`, perfiles correctos (`animated_asset` para bestias, `environment_surface` para el Foco). Se prueba con `tools/plan-art-production.mjs` sin cambiarlo.
- **Control ligero:** `.github/workflows/tripo-art-brain-preflight.yml` ejecuta tests y un inventario completo con checkout parcial. No usa Windows/Unity pesados, no accede a credenciales Tripo, no edita escenas ni sube GLB a Tripo.

## Evidencia real

- [Run de integración (PASS)](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39620738184): tests de fail-closed y las tres fuentes reales PASS. Artefacto `tripo-art-brain-zero-credit-39620738184` contiene informes, tres inventarios Qwen y las decisiones del planificador.
- Origen: [resultados de 3 generaciones](TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md), [postmortem jabalí](TRIPO_API_JABALI_LECCIONES_20261021.md), [Engendro](TRIPO_API_ENGENDRO_RESULTADO_20261021.md), [Foco](TRIPO_API_FOCO_RESULTADO_20261021.md).
- [Engendro ya integrado con 12.040/2.900 tris, no animado](evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md) y [PR original #84](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/84). No reiterar esa integración ni declarar su calidad formal como 8/10.

## Comandos para la siguiente intervención artística permitida

Desde un checkout auténtico de `main` con los GLB archivados (sin LFS stubs):

    node --test tools/tripo-art-brain/preflight.test.mjs
    node tools/tripo-art-brain/preflight.mjs audit all
    node tools/tripo-art-brain/preflight.mjs prepare spawnling --out /tmp/tripo-brain-spawnling-unique
    node tools/plan-art-production.mjs /tmp/tripo-brain-spawnling-unique/art-production-request.json

`prepare` requiere directorio de salida **fuera del repositorio y todavía vacío**. El archivo `qwen-inventory.txt` es la entrada `--inventory` para el **mismo** `author_brief.py` canónico; ese artista exige la referencia real, una captura Unity auténtica previa, el objeto/malla identificados y, cuando corresponda, el contexto de topología del objeto. Ver su contrato de comandos; no fabricar imágenes, movimiento o una decisión Qwen a partir del inventario. El director de escena debe autorizar el uso del runner y evaluar el A/B antes de modificar algo que ya está integrado.

## Secuencia de trabajo que NO se puede fingir

1. Elegir **fuente existente** y revisar capturas auténticas de Unity + informe generado. Identificar si el defecto es de geometría, superficie, animación o integración. Sin gastar créditos.
2. Para la anatomía del Engendro: dar el inventario al Qwen existente junto a capturas reales, producir el plan artístico verificable y autorizar *solo una copia* de la malla para pruebas Blender, conservando la versión integrada. Una edición ejecutada necesita .blend/GLB + A/B real y valoración independiente.
3. Para rigging: inspeccionar topología y articulaciones de la **fuente original de Tripo**. La API oficial ofrece `animate_prerigcheck` como **Rig Check gratis**; el tipo de tarea es `animate_prerigcheck` con `original_model_task_id`. No se ha ejecutado en este trabajo: requiere una sesión API válida y un gate de cliente que garantice una sola task gratuita. Que el GLB esté integrado no implica que sea animable.
4. Si `Rig Check` confirma compatibilidad, elegir rig humanoide o bestia según la malla. La documentación de la API describe `animate_rig` (versión `v2.5-20260210`, `rig_type` entre `biped` y `quadruped`, etc.) y `animate_retarget` con animaciones limitadas por versión. **El coste publicado es 25 créditos por rig + 10 por movimiento**, sujeto a consulta vigente y consentimiento específico; ningún job de este workstream puede llamarlos.
5. Después de una autorización futura separada, recuperar el GLB riggeado **de la misma task**, revisar huesos/skin y clips de forma aislada, importar a Unity sin sustituir escena, probar gestos y combate reales, perfiles Android, identidad visual y guardados, y entonces coordinar promoción con el propietario de escena y su QA.
6. Para Jabalí y Foco: se pueden iterar cero créditos en Blender/Unity usando originales; antes de añadir un nuevo Tripo/LOD P2 ejecutar un A/B de coste/fidelidad medido. No crear Bastión III ni hotspots nuevos.

## Garantías, límites y medición

- `ART_APPROVED=false` y `physical_mobile_profiled=false` siguen siendo correctos. Un audit PASS solo comprueba fuente binaria y evidencias técnicas almacenadas, no que la pieza se vea mejor ni que sea eficiente en un teléfono físico.
- El informe no realiza tareas API y evita el fallo recurrente de volver a pagar porque Blender o Unity tuvo un error. Los originales y SHA nunca se reescriben, y solo se producen archivos en una carpeta temporal que no es el juego.
- El cerebro artístico no ha recibido todavía estas entradas en una **inferencia Qwen nueva**. Es una conexión consumible y probada con el contrato de su `--inventory`, no la certificación de un ciclo automático end-to-end. Ese ciclo y el rig del Engendro quedan como siguientes pasos, sujetos a capturas reales, coordinación de runner y presupuesto.
- Tripo API y Studio usan saldos distintos. Documentación oficial: [Billing](https://platform.tripo3d.ai/docs/billing), [esquema oficial de tareas](https://platform.tripo3d.ai/docs/schema), [importación de GLB](https://platform.tripo3d.ai/docs/import-model) y [edición y Smart LowPoly](https://platform.tripo3d.ai/docs/editing).

## Cómo medir la mejora cuando pasemos a producción

Registrar por pieza: coste créditos, tiempo total en segundos, número de intentos/ediciones, calidad A/B ponderada por revisor independiente, tris LOD0/1/2, material/texture bytes, draws, fps/ram/temperatura física en Android, y protección de guardados/combate. Solo marcar producción escalable tras resultados aceptados en distintas familias con ahorro medido. Reutilizar los verificadores existentes; no añadir un segundo sistema de aprobación.
