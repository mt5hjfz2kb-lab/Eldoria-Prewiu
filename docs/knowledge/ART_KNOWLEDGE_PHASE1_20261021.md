# Eldoria — memoria artística operativa, fase 1 ampliada

**Versión de conocimiento:** 21-10-2026. **Estado:** `READ_ONLY`, consolidada en la rama canónica `main` tras PR #92; no es una ruta ejecutable. **No es un pipeline nuevo**, no concede autoridad para escribir en el juego y no ejecuta automáticamente Blender/Qwen/Tripo/Unity.

**Memoria de casos:** [`pipeline/art-knowledge/catalog-v1.json`](../../pipeline/art-knowledge/catalog-v1.json) (54 fichas). De ellas, **27** corresponden al inventario inicial y **27** se han recuperado de ramas históricas fijadas a SHA.

**Auditoría histórica:** [recuperación de las 201 ramas archivadas](ELDORIA_GLOBAL_HISTORICAL_RECOVERY_20261021.md), con cribado de todas las referencias y lectura de fuentes primarias en 20 ramas prioritarias. [Inventario de cobertura](../../pipeline/art-knowledge/historical-branch-screening-v1.json).

## Leer antes de cualquier nueva obra

1. Verificar las órdenes actuales, [`AGENTS.md`](../../AGENTS.md), `main`, `active-workstreams`, el runner real y el [canon visual del 20/10](../VALORIA_VISUAL_REFERENCE_CANON_20261020.md). Este índice **nunca** prevalece sobre una decisión posterior del propietario.
2. Diagnosticar qué se ve mal desde la cámara real: silueta, composición, contacto, material, animación, escala, legibilidad o fallo técnico.
3. Consultar las fichas relevantes del JSON, examinar **`avoid` y `conditions`**, y abrir las fuentes exactas (para ramas antiguas la entrada `historical_sources` incluye commit SHA).
4. Reutilizar método y recursos solo si su procedencia, licencias, fuentes editables y compatibilidad siguen demostradas. **No** reactivar un experimento completo por encontrar una técnica aprovechable.
5. Todo trabajo autorizado posterior debe repetir gates reales Unity, A/B en cámara comparable, revisión independiente y protección de guardados. Un `TECH PASS` nunca equivale a `ART_APPROVED`.

**Regla decisiva de la referencia aprobada:** las cuatro parcelas mostradas en el JPEG deben **permanecer invisibles**. No recuperar los acantilados monumentales, campamentos, barrios o versiones de escenario de antiguas ramas por utilizar un método de roca, vegetación o sillería.

## Catálogo consultable por problema

### Referencia y prohibiciones

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K04` — Se pretende recuperar el estilo de Valoria Bastión I–II. | Fijar JPEG canónico y alcance: bastión, puente/río, pradera, ribera; parcelas de imagen NO visibles.; [Documento canónico](../../docs/VALORIA_VISUAL_REFERENCE_CANON_20261020.md) | NORMA VIGENTE |

### Qué herramienta corresponde al defecto

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K01` — La escena parece vacía, mal encuadrada o desunida. | Primero composición/cámara/materiales en Unity y fuentes existentes; no solicitar geometría nueva por defecto.; [Documento canónico](../../docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md) | NORMA VIGENTE |
| `K02` — Materiales, superficie o relieve no encajan visualmente. | Diagnóstico URP/Unity; Blender para UV, bake, malla o soporte si se demuestra necesario.; [Documento canónico](../../docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md) | NORMA VIGENTE |
| `K03` — Falta geometría o la silueta de una pieza es pobre. | Inspeccionar donante/prefab y .blend; si no sirven, autoría Blender profesional y revisión aislada; Tripo solo condicionado.; [Documento canónico](../../docs/ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md) | NORMA VIGENTE |

### Cómo se comprueba la mejora

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K05` — Un workflow termina SUCCESS pero se desconoce si el arte mejora. | A/B PNG genuino Unity a misma cámara y luz, veredicto independiente, QA funcional y registro por separado.; [Documento canónico](../../docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md) | NORMA VIGENTE |

### Terreno y naturaleza

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K06` — Orillas/terreno se ven planas y sin contacto. | Reautoría localizada de orilla con geometría real y materiales compatibles, retención solo tras A/B Unity.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDA CON LIMITACIONES |
| `K07` — Puente/puerta/bastión presentan piezas desconectadas o poca profundidad. | Corregir apoyos pétreos/cubierta real sobre fuentes Blender existentes, siguiendo escala y encuadre canónicos.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDA CON LIMITACIONES |
| `K08` — Bosque y cantera resultan sintéticos o no apoyan bien en el suelo. | Reutilizar coníferas originales, modificar ramificación/atlas/LOD; cantera con estratos y contacto de terreno.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDA CON LIMITACIONES |
| `K09` — La Brecha y los contactos de edificios parecen superpuestos. | Fracturas moderadas, terreno conectado y elementos ya autorizados; QA jugable con cámara original.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDA CON LIMITACIONES |
| `K16` — Foco de corrupción requiere identidad visual. | Reusar fuente Tripo existente; rediseñar silueta/fisuras en Blender antes de plantear integración.; [Documento canónico](../../docs/TRIPO_API_FOCO_RESULTADO_20261021.md) | TECH PASS PARCIAL |
| `K35` — Roca parece un poliedro y escalera es una tira pintada. | Usar malla real/candidatos CC0 adaptados y volumen funcional con conexiones físicas; validar en cámara y topología.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/22db6d0d539b2baa8ca4f4eeeb42b6a25bb5fed4/docs/evidence/valoria-modular-63-individual-quality/RECUPERACION_GPT6_20261016_63_GLBS_CONTINUIDAD.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K43` — Necesitamos recuperar material rico de fuente secundaria sin rehacer toda una geometría Hero. | Reutilizar selección semántica de UV/PBR y zonas de material solo donde la fuente conserva detalle y la revisión individual lo avala.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/64675d692ac45318743b4d34d7aae7aff0917d7f/docs/VALORIA_SEMANTIC_SOURCE_REAUTHORING_PROOF_V1_RESULT.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K50` — Se necesita fuente de roca natural CC0 con retoques profesionales y no otro generador desde cero. | Recuperar el método de extracción de donante CC0 con SHA, revisión en Blender, reparación acotada y QA Unity separado.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/c13c03d90479305b4dfe3adc740c8933fabb1bf6/tools/art-studio-recovery/REPRODUCE.md) | RECETA HISTÓRICA POR REVALIDAR |

### Arquitectura

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K10` — Aserradero incompleto o de baja legibilidad. | Reutilizar fuente STATE2, reparar techo/estructura y componer piedra/madera para cámara real.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md) | RETENIDA CON LIMITACIONES |
| `K11` — Cuartel/tejado de lona muestra normales defectuosas o intersecciones. | Reutilizar y reparar fuente CampFamily: soldadura de normales, pliegues y anclajes, luego A/B Unity.; [Documento canónico](../../docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md) | RETENIDA CON LIMITACIONES |
| `K31` — El plan geométrico produce piezas enterradas, cubiertas desconectadas o volúmenes repetidos. | Usar preflight de salud de composición sobre fuente/Blender antes de pasar a Unity.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/65add927cde3722407906808f93f22035c975d4c/docs/ELDORIA_FULL_ARTIST_STUDIO_V1.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K32` — Crear una familia consistente de arquitectura defensiva con Qwen/Blender en vez de prefabs aislados. | Recuperar decisiones de las siete fuentes artísticas, interfaces y LOD del estudio histórico como ejemplos individuales, no como escena actual.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/4519a2773ab1d7cc5cac21203c420f3a81faa9d9/docs/evidence/valoria-asset-family-v2/QUALITY_TAKEOVER.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K33` — Piedra heroica demasiado regular y coronaciones con aspecto de juguete. | Reutilizar procedimiento de biseles/arrises irregulares, sillería coherente, PBR mate, LOD que no destruya silueta.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/4519a2773ab1d7cc5cac21203c420f3a81faa9d9/docs/evidence/valoria-hero-surface-v2/ART_REVIEW.md) | RECETA HISTÓRICA POR REVALIDAR |

### Criaturas y Tripo

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K12` — Se necesita fuente de fauna/criatura 3D con silueta inicial. | Tripo API H3.1 solo con autorización por pieza; archivar GLB raw/task; Blender; QA Unity glTFast BaseColor real.; [Documento canónico](../../docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md) | TECH PASS PARCIAL |
| `K13` — Engendro debe verse como criatura estática del Bastión II. | Reutilizar GLB pagado, remodelar color/silueta en Blender, dos LOD y preservar WorldHotspot/guardados.; [Documento canónico](../../docs/evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md) | RETENIDA CON LIMITACIONES |
| `K14` — Engendro necesita un gesto animado sin gastar créditos. | Reusar mesh fuente y probar rig/skin de Blender; validar deformación y retorno a reposo en Unity aislado.; [Documento canónico](../../docs/evidence/spawnling-animation-v4-review-20261021/unity-approved.json) | TECH PASS PARCIAL |
| `K15` — Jabalí rocoso para futuro capítulo. | Conservar único GLB original Tripo y someterlo a escala, LOD, animación y prueba Unity al autorizar Bastión III.; [Documento canónico](../../docs/TRIPO_API_JABALI_LECCIONES_20261021.md) | TECH PASS PARCIAL |

### LOD y rendimiento

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K17` — Mallas Tripo con ~50k triángulos pesan demasiado para repetición móvil. | Generar LOD sobre copias de fuente congelada y comparar cámara estratégica/iso con UV y mapas intactos.; [Documento canónico](../../docs/TRIPO_MOBILE_LOD_PROOF_20261021.md) | TECH PASS PARCIAL |
| `K34` — Biblioteca modular parece lista pero tiene errores de UV, pivot o shader URP. | Aplicar preflight glTF de UV0, bounds, pivotes, extensiones PBR y coste real antes de Unity.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/22db6d0d539b2baa8ca4f4eeeb42b6a25bb5fed4/docs/evidence/valoria-modular-63-individual-quality/QA_TECNICO_GLB_63_PIVOTES_UV_20261016.md) | RECETA HISTÓRICA POR REVALIDAR |

### Qwen, visión y aprendizaje

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K18` — Qwen cambia geometría pero no el significado de la pieza. | Comparar intención/imagen/forma con operaciones semánticas existentes, límites espaciales de malla y crítica independiente.; [Documento canónico](../../docs/evidence/ELDORIA_ARTIST_FAILURE_FORENSICS_20261015.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K19` — Qwen criatura mueve toda la malla en vez de corregir anatomía. | Preflight de escala y límites sobre fuente, filtro de traslación dominante y rechazo antes de Blender.; [Documento canónico](../../docs/ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md) | NO REUTILIZAR COMO RESULTADO |
| `K20` — Esculpido Qwen localizado invisible desde la cámara estratégica. | Inspeccionar orientación de rostro, selección de vértices real, visibilidad multiángulo y rechazar cambios microscópicos.; [Documento canónico](../../docs/ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md) | NO REUTILIZAR COMO RESULTADO |
| `K28` — Qwen solo dispone de unas pocas operaciones de Blender y no puede diseñar libremente un modelo complejo. | Consultar catálogo de operaciones tipadas del antiguo full-studio; seleccionar herramienta realmente ejecutable tras validar interfaz, sin incorporar su pipeline entero.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/65add927cde3722407906808f93f22035c975d4c/docs/ELDORIA_FULL_ARTIST_STUDIO_V1.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K30` — Las lecciones de un acantilado contaminan un brief de torre o criatura. | Seleccionar lecciones transferibles por relevancia y quitar coordenadas e IDs específicos del modelo anterior, manteniendo SHA y procedencia.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/65add927cde3722407906808f93f22035c975d4c/tools/art-direction/full-studio/learning_context.py) | RECETA HISTÓRICA POR REVALIDAR |
| `K38` — Qwen ve imágenes pero inventa geometría o interpreta mal tejados multivista. | Atar cada afirmación a fuente XYZ, múltiples cámaras y topología, pedir contradicciones explícitas y veredicto independiente.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/f0fed53e4e58d85dd7b51033a92a2bb28fd2c046/docs/evidence/VALORIA_SPATIAL_DIRECTOR_INDEPENDENT_VERDICT_20261015.md) | NO REUTILIZAR COMO RESULTADO |
| `K39` — Se inyectan recuerdos sin saber si reducen errores. | Usar recuerdo selectivo con procedencia y A/B emparejados, reportar ganancias y regresiones.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/f0fed53e4e58d85dd7b51033a92a2bb28fd2c046/tools/art-direction/director-control/selective_memory.py) | RECETA HISTÓRICA POR REVALIDAR |

### Estrategias artísticas rechazadas

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K21` — Cambio de pradera/ribera parece grande en métricas, pero no arregla composición. | Descartar mezcla superficial como mejora suficiente; centrarse en formas/escala y contacto verdaderos.; [Documento canónico](../../docs/evidence/valoria-reference-clean/OWNER179_BANK_MEADOW_VISUAL_VERDICT_20261020.md) | NO REUTILIZAR COMO RESULTADO |
| `K22` — Camino claro superpuesto a ciudad parece una tira artificial. | No reutilizar V12; corregir continuidad de plataforma/terreno con diseño y A/B.; [Documento canónico](../../docs/evidence/valoria-reference-clean/NATIVE160_ART_REJECTION_AND_ROLLBACK_20261019.md) | NO REUTILIZAR COMO RESULTADO |
| `K23` — Ciudad se llena de piezas sueltas sin tejido funcional. | Rechazar dispersión de muros/casas/ruinas y respetar escena actual; recuperar solo método bajo nuevo diseño autorizado.; [Documento canónico](../../docs/evidence/valoria-reference-clean/URBAN_V7_ART_REJECTION_20261019.md) | NO REUTILIZAR COMO RESULTADO |
| `K37` — Dovelas, mortero y superficie fotográfica parecen damero o bandas artificiales. | No promover V6/9 defectuosas; mantener geometría de cuña y corregir material/UV con referencia pétrea y A/B macro.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/22db6d0d539b2baa8ca4f4eeeb42b6a25bb5fed4/docs/evidence/valoria-modular-63-individual-quality/REVIEW_7_WEAKEST_V8_V10_20261016.md) | NO REUTILIZAR COMO RESULTADO |
| `K40` — Traslado de una celda Golden a zona de producción pierde salto visual. | No reutilizar el ensamblaje de transferencia como arquitectura premium; conservar validadores de costes y composición solo como técnica.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/5f7f389148186b42d20f3673bc4e3f093557c941/docs/VALORIA_PRODUCTION_CELL_GOLDEN_TRANSFER_V1_RESULT.md) | NO REUTILIZAR COMO RESULTADO |
| `K41` — Camino o conector de materiales busca lenguaje visual único y crea barras oscuras. | No reactivar rutas de parche de acceso ni conectores superficiales rechazados.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/9565ca577eb920abcb76b2b09b69022686685a12/docs/VALORIA_UNIFIED_VISUAL_LANGUAGE_PROOF_V1_RESULT.md) | NO REUTILIZAR COMO RESULTADO |
| `K42` — Geometría Hero recibe tiras brillantes o wrappers vacíos que la empobrecen. | Evitar fundas primitivas y reautoría superficial; diagnosticar interfaces espaciales y formas mayores.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/ee51cfbbb4752b631a099e32da1b361ce568dd1e/docs/VALORIA_SOURCE_LEVEL_ART_DIRECTION_PROOF_V1_RESULT.md) | NO REUTILIZAR COMO RESULTADO |
| `K44` — Montaje híbrido 3D/2.5D parece mejor en objetos sueltos pero empeora suelo/luz. | No escalar mezcla 2.5D ni repetir dispersión de columnas/UV erróneas; hacer primero continuidad del suelo y profundidad real.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/2f4e0a166d2ef18810ac0ef48b5bac5b089fc779/docs/evidence/valoria-scene-breakthrough-20261016/FINAL_VISUAL_REVIEW.json) | NO REUTILIZAR COMO RESULTADO |
| `K48` — Una obra de seis módulos parece una meseta continua por sus nombres, pero los recursos son pequeñas islas. | Clasificar primero el rol espacial y límites reales del donante, y no componer un paisaje masivo con piezas puntuales.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/2959d743e7aab966bafa12a89bd3529c39141959/docs/evidence/reference-first-zero-budget-visual-pilot-20261015/V16_TO_V20_DONOR_EVIDENCE_AND_VISUAL_DECISION_20261016.md) | NO REUTILIZAR COMO RESULTADO |
| `K51` — Se pulen pavimento y pequeños patios sin efectos visibles en la cámara del jugador. | Rechazar intervenciones sin impacto de pantalla significativo aunque PlayMode pase.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/f3f5aac2f2bb6b6f3cd67584ea8823ed1e59f9a5/docs/VALORIA_AUTHORED_GAMEPLAY_CELL_ART_DIRECTION_V1_RESULT.md) | NO REUTILIZAR COMO RESULTADO |
| `K52` — Familia artística secundaria no armoniza con Hero pese a materiales compartidos. | Diseñar relación de escala, volumen y materiales del conjunto antes de producir variantes masivas.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/b52c3e78c3be7f1a2b8027fc73c5350890942cf0/docs/VALORIA_AUTHORED_SECONDARY_ART_FAMILY_V1_RESULT.md) | NO REUTILIZAR COMO RESULTADO |
| `K54` — Abundantes prefabs retocados crean artefactos materiales graves y no coherencia de escena. | Mantener revisión independiente y rollback ante regresión severa de color/iluminación.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/2959d743e7aab966bafa12a89bd3529c39141959/docs/evidence/reference-first-zero-budget-visual-pilot-20261015/SECOND_VISUAL_REVIEW.md) | NO REUTILIZAR COMO RESULTADO |

### Continuidad de fuentes y ejecución

| Ficha y problema | Método / enseñanza | Estado |
|---|---|---|
| `K24` — Un modelo/captura de GitHub Actions desaparece y se pierde el resultado. | Anotar commit/run/hash y preservar copias permanentes de fuente + evidencias críticas, no solo artifacts efímeros.; [Documento canónico](../../docs/ELDORIA_ART_LEARNING_EVIDENCE_V1.md) | NORMA VIGENTE |
| `K25` — La siguiente sesión ignora aprendizajes o recibe instrucciones antiguas. | Leer AGENTS/current main/workstreams y documentación específica; recuperar brief y errores antes de Qwen.; [Documento canónico](../../docs/DOCUMENTATION_INDEX.md) | NORMA VIGENTE |
| `K26` — Un método conocido falla y se inicia nueva investigación de herramientas. | Clasificar fallo, consultar fallback evidenciado y excluir rutas rechazadas; parar si la causa es ambigua.; [Documento canónico](../../docs/ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md) | NORMA VIGENTE |
| `K27` — Un candidato se publica o entra al juego por CI verde. | Separar QA fuente, revisión visual, QA Unity jugable, dispositivo, promoción de release, y verificar destinatario.; [Documento canónico](../../docs/WORKFLOW_INDEX.md) | NORMA VIGENTE |
| `K29` — Reinventamos modelos aunque exista una fuente local adecuada. | Recuperar la receta de catálogo con archivo GLB/GLTF, SHA, licencia, atribución, reutilización autorizada e importación controlada.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/65add927cde3722407906808f93f22035c975d4c/docs/ELDORIA_FULL_ARTIST_STUDIO_V1.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K36` — Se elige automáticamente la última versión de un asset aunque sea peor. | Comparar por identidad SHA y veredicto real cada iteración, no por fecha, status SUCCESS ni numeración V.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/22db6d0d539b2baa8ca4f4eeeb42b6a25bb5fed4/docs/evidence/valoria-modular-63-individual-quality/RECUPERACION_GPT6_20261016_63_GLBS_CONTINUIDAD.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K45` — Se quieren 63 módulos en una escena limpia pero la documentación confunde fuentes recuperadas con Unity ejecutado. | Reutilizar método de reconstrucción/validación de fuentes GLB y GUID de metadatos; tratar builder como candidato aislado hasta Unity.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/47049355befde55a33b2358502f58176cc1ab548/docs/evidence/valoria63-clean-scene-20261016/README.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K46` — En pruebas aisladas Unity se pierden referencias al recargar escena. | Forzar conservación de .meta/GUID y comparar reapertura idéntica antes de certificar.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/2f4e0a166d2ef18810ac0ef48b5bac5b089fc779/docs/evidence/valoria-scene-breakthrough-20261016/BLOCKOUT_REVIEW.md) | RECETA HISTÓRICA POR REVALIDAR |
| `K47` — Se recupera parcialmente una escena World4X y se anuncia como trabajo completo. | Registrar exactamente assets recuperados, hashes y estado parcial antes de reutilizar en Unity.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/f3ccc8d0b7767472ef09b02c3091f6d676f204ee/docs/world4x-zone1/PARTIAL_SOURCE_RECOVERY_20261014.json) | RECETA HISTÓRICA POR REVALIDAR |
| `K49` — El GLB carga pero materiales quedan blancos, magenta o con brillo aberrante. | Verificar slots de glTF, valores por defecto, índices faltantes=-1, roughness y equivalencia PBR al guardar Unity.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/89ded38acf411a503269d618c2469ec5269999b1/docs/evidence/valoria-bastion1-reference-assembly/RESULT.json) | RECETA HISTÓRICA POR REVALIDAR |
| `K53` — Tres propuestas visuales se confunden con tres pruebas de calidad reales. | Conservar fuentes de estilos como prototipos, con licencias, pero no valorar sin capturas Unity verdaderas.; [Fuente histórica fijada por SHA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/b2b87d52fe297152669b7071777acdd3c7aa3115/Unity/Assets/Eldoria/ArtTests/ThreeVisualStyles/README.md) | RECETA HISTÓRICA POR REVALIDAR |


## Niveles de confianza y notas

- `NORMA VIGENTE`: política sobre cómo trabajar; no acredita resultados visuales.
- `TECH PASS PARCIAL`: fuente/técnica demostrada, sin calidad final ni móvil certificado.
- `RETENIDA CON LIMITACIONES`: elemento integrado o reutilizado en un alcance concreto con QA, no aprobación artística universal.
- `RECETA HISTÓRICA POR REVALIDAR`: código, método o fuente recuperado de un experimento archivado. Aunque tuviera revisión favorable entonces, **no** se presume aprobado hoy.
- `NO REUTILIZAR COMO RESULTADO`: veredicto de rechazo documentado. Su diagnóstico sí es conocimiento útil; sus assets rechazados no deben promoverse.

**La fase 2 sigue sin activarse:** el artista todavía no consulta este índice por obligación antes de cada inferencia ni devuelve al catálogo todos sus nuevos resultados. La carpeta `pipeline/art-visual-approvals/` no gana aprobaciones por este inventario.

**Limitación de profundidad:** 201 ramas fueron inventariadas/cribadas, 20 recibieron lectura selectiva de código/informes y ocho comparaciones limitadas por GitHub se completaron con árboles recursivos. No se afirma haber revisado todas las imágenes y archivos de todas las ramas. Los archivos de la base histórica permanecen localizables para revisiones concretas futuras.
