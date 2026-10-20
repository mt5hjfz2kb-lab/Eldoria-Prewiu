# Eldoria — Tripo API / Engendro de la Fisura: evaluación técnica y artística

Fecha: 21-10-2026. **Fuente producida y preservada; NO integrada ni aprobada como criatura animada del juego**.

## Identidad y alcance

- Contenido existente: `spawnling`, «Engendro de la Fisura», enemigo común de Bastión II en `v0220/js/gameplay.js`.
- Arte encargado: criatura pequeña violeta, parcialmente deforme, entre humanoide y animal. No cambiar reglas, economía, targeting ni los guardados.
- Se hizo **UNA sola generación de Tripo API** H3.1 `v3.1-20260211` y textura estándar. Task `351d1034-25ec-4300-a2e0-d03a0e68175a`, gasto **20 créditos**. Origen: [GitHub Actions 39511077721](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39511077721). Los pasos posteriores nunca repitieron esa generación.

## Evidencia verificable

| Etapa | Resultado |
|---|---|
| Tripo raw | 80.268 triángulos, GLB SHA-256 `5a6fa0ba4440269815ad9b7f4c2429663bf26d7ecd3227077ef6be63633f674f` |
| Blender | 49.800 triángulos, UV0 y normales; GLB optimizado SHA `2701e6f6a37920cbe6b434b903aa7b14283514f8276399d14bfffaece0edcc4a` |
| Fuente editable | .blend SHA `7aa6521ea27b5996851aea7d78f0d30caee15e3ec77797df5ac09d0ff10f7edf` |
| Texturas | Color 2048², normales 2048², ORM 1024² tras optimización |
| QA Unity aislada | Unity 6000.3.23f1; 49.800 triángulos, 29.779 vértices importados, 3 texturas, BaseColor 2048², UV/normales, raycast y 4 capturas PASS; run [39512910985](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39512910985) |
| Archivo permanente | [`ArtSource/World4X/Spawnling_v1/`](../ArtSource/World4X/Spawnling_v1/) con original, GLB optimizado, Blender y `UnityQA/` |

## Evaluación visual observada

**SOURCE ART: candidato aprovechable, aprobación artística final PENDIENTE.** El cuerpo encorvado y asimétrico, manos largas, garras, piernas animalizadas y material púrpura funcionan para una amenaza temprana. La lectura desde zoom medio es singular.

**Defectos que impiden declarar ART_APPROVED sin más trabajo:** rostro estrecho y muy integrado en el tórax; desde determinados ángulos la cola domina y el conjunto se percibe algo reptiliano. El albedo aparece relativamente claro bajo la luz neutra de Unity. La anatomía tiene rasgos difíciles de articular. **No hay rig, skinning ni animaciones**. Desde cámara estratégica, los detalles muy pequeños no se distinguirán.

Se recomienda conservar este primer modelo y, cuando toque integrar Bastión II, revisar primero la silueta, reforzar el rostro y probar de verdad la escala desde el mapa 4X. No gastar otra generación para resolver un defecto que puede corregirse mediante Blender si técnicamente es viable.

## Aprendizaje nuevo de automatización

El primer flujo falló en `Read canonical request` por un archivo ausente en la copia del runner, antes de la API. El segundo llegó a completar Tripo, pero el script `tools/tripo_module_blender.py` faltaba tras checkout: **NO fue fallo del GLB**. El script se recuperó por URL de GitHub en el **commit exacto**, Blender corrió sin cargo y se subieron los tres activos al artifact 12306282456. Se añadió recuperación por SHA al paso canónico antes del futuro Foco; nunca repetir una task pagada por un error posterior.

- `source_run=39511077721` → pagado una vez.
- `recovery_run=39512676401` → Blender sin Tripo.
- `unity_qa_run=39512910985` → importación real 2K sin Tripo.
- `archive_run=39513398494` → GLB/BLEND e imágenes preservados en `main`.

Los cuatro estados que se deben comunicar por separado: SOURCE_GENERATED **sí**, UNITY_TECH_PASS **sí**, ART_APPROVED **pendiente**, INTEGRADO EN PARTIDA **no**.

Relacionados: [lecciones del Jabalí](TRIPO_API_JABALI_LECCIONES_20261021.md), [norma de producción](TRIPO_API_NORMA_PRODUCCION_V1.md), [briefs](TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md).
