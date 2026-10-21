# Eldoria — memoria operativa del arte · Fase 1 (inventario)

**Corte de auditoría:** 21-10-2026. **Estado:** CATALOGO DE CONSULTA; no habilita ni sustituye el pipeline, no ocupa runner, no valida nuevas obras y no cambia escenas.

- **Fuente estructurada:** [`pipeline/art-knowledge/catalog-v1.json`](../../pipeline/art-knowledge/catalog-v1.json).
- **Autoridad viva en cada encargo:** [`AGENTS.md`](../../AGENTS.md), [registro activo](../../pipeline/active-workstreams.json), [índice documental](../DOCUMENTATION_INDEX.md) y [canon 20-10](../VALORIA_VISUAL_REFERENCE_CANON_20261020.md). Este catálogo **no** reemplaza ninguno.
- **Referencia visual:** `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`, SHA-256 `86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d`. **Excepción posterior vinculante:** las cuatro parcelas dibujadas en la foto no pueden aparecer en la escena. No se deben crear barrios, campamentos o grandes acantilados por el hecho de encontrarlos en informes históricos.

## Resultado entregado

- **27 fichas** rastreables por familia, problema, decisión, resultado y fuentes; 7 etiquetadas *retenido con límites*, 5 pruebas técnicas sin certificación artística, 5 rechazos concretos, 1 método histórico y 9 normas de encaminamiento/seguridad. Las categorías son **disjuntas** pero no representan tasas de éxito.
- Evidencias con nombres de documentos y, donde constan, IDs de ejecuciones. No se han vuelto a descargar y rehashear todos los PNG/GLB históricos: **la certeza es la del documento o prueba individual citada, no una nueva inspección binaria**.
- Todas las fichas conservan `avoid` (qué no repetir) y `conditions` (qué verificar). Nunca elevan `CI SUCCESS` a `ART_APPROVED`.
- Esta fase no retoca scripts `author_brief.py`, Qwen, Unity, workflows, imágenes, permisos, créditos, publicaciones ni registro de propietarios.

## Manual rápido: cómo utilizarlo sin alterar el juego

1. **Mirar estado vigente:** `main` exacto, `active` y últimos jobs; no usar cabeceras antiguas ni reclamar trabajos de otro ejecutor.
2. **Definir fallo visible:** cámara/jugador, escala y uno o dos defectos (silueta, superficie, composición, rig, LOD, error técnico).
3. **Buscar las fichas por `family`, `problem` y `recommended_route`** en el JSON. Empezar por normas vigentes y obras retenidas; rescatar técnicas históricas solo tras comprobar que son compatibles con el canon y el producto actual.
4. **Leer obligatoriamente `avoid` y la fuente original.** La coincidencia temática no garantiza idoneidad: un buen método de cantera puede ser incorrecto para un río o una criatura.
5. **Entregar al artista solo contexto comprobado y relevante:** ruta, fuente original, defecto, estrategia, piezas protegidas y error anterior. Esta fase **no** conecta todavía una inyección automática al artista (eso pertenece a fase 2).
6. **Cuando se ejecute una tarea autorizada**, editar copias, validar A/B Unity con misma cámara y revisión independiente, y mantener juego/guardados/escena intactos hasta promoción por su responsable.

## Índice de conocimiento reunido

### Referencia artística y límites

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K04` — Se pretende recuperar el estilo de Valoria Bastión I–II. | Referencia y excepción sobre parcelas ratificadas 20-10-2026. [`VALORIA_VISUAL_REFERENCE_CANON_20261020.md`](../../docs/VALORIA_VISUAL_REFERENCE_CANON_20261020.md); [`AGENTS.md`](../../AGENTS.md) | NORMA ACTUAL |

### Selección de tecnología

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K01` — La escena parece vacía, mal encuadrada o desunida. | Regla vigente del planificador de producción, no ensayo de mejora estética. [`ELDORIA_TOOLCHAIN_AUTOMATION_V2.md`](../../docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md); [`visual-fallback-policy.json`](../../pipeline/visual-fallback-policy.json) | NORMA ACTUAL |
| `K02` — Materiales, superficie o relieve no encajan visualmente. | Jerarquía de herramientas separa defectos de superficies y silueta. [`ELDORIA_TOOLCHAIN_AUTOMATION_V2.md`](../../docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md); [`UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md`](../../docs/UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md) | NORMA ACTUAL |
| `K03` — Falta geometría o la silueta de una pieza es pobre. | Método normativo sobre alcance actual, sin ART PASS implícito. [`ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md`](../../docs/ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md); [`ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md`](../../docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md) | NORMA ACTUAL |

### Cómo se acepta una mejora

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K05` — Un workflow termina SUCCESS pero se desconoce si el arte mejora. | Contrato formal distingue TECH PASS, ART PASS, MOBILE PASS e integración. [`ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`](../../docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md); [`README.md`](../../pipeline/art-visual-approvals/README.md) | NORMA ACTUAL |

### Terreno, naturaleza y mundo

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K06` — Orillas/terreno se ven planas y sin contacto. | Fase terreno/ribera retenida en Valoria; QA nativo y fuentes editables archivadas, sin calidad global certificada. [`PRODUCTION_DELIVERY_20261020.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md); [`PRODUCTION_STATE.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md) | RETENIDO CON LIMITES |
| `K07` — Puente/puerta/bastión presentan piezas desconectadas o poca profundidad. | Fase puente/puerta/Bastión retenida, con A/B real y persistencia nativa; sin 8/10 formal. [`PRODUCTION_DELIVERY_20261020.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDO CON LIMITES |
| `K08` — Bosque y cantera resultan sintéticos o no apoyan bien en el suelo. | Bosque/cantera retenidos en entrega native229, con comparaciones y 40 checks; no calidad final móvil. [`PRODUCTION_DELIVERY_20261020.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDO CON LIMITES |
| `K09` — La Brecha y los contactos de edificios parecen superpuestos. | Tratamiento Brecha/ciudad funcional integrado en las cuatro fases; revisión visual global pendiente. [`PRODUCTION_DELIVERY_20261020.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_DELIVERY_20261020.md) | RETENIDO CON LIMITES |
| `K16` — Foco de corrupción requiere identidad visual. | QA Unity aislado real PASS; juicio artístico provisional ~5/10 y no integrado. [`TRIPO_API_FOCO_RESULTADO_20261021.md`](../../docs/TRIPO_API_FOCO_RESULTADO_20261021.md) | PRUEBA TÉCNICA |

### Arquitectura

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K10` — Aserradero incompleto o de baja legibilidad. | Aserradero retenido tras Qwen+BlenderMCP y A/B nativo; mejora localizada, no ART_APPROVED global. [`PRODUCTION_STATE.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md) | RETENIDO CON LIMITES |
| `K11` — Cuartel/tejado de lona muestra normales defectuosas o intersecciones. | Fuente reparada en native87–91 y retención jugable; sin aprobación artística formal. [`PRODUCTION_STATE.md`](../../docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md) | RETENIDO CON LIMITES |

### Criaturas y generación 3D

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K12` — Se necesita fuente de fauna/criatura 3D con silueta inicial. | Tres fuentes Tripo H3.1 generadas y verificadas en Unity aislado; no escalabilidad ni móvil probados. [`TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md`](../../docs/TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md); [`TRIPO_API_NORMA_PRODUCCION_V1.md`](../../docs/TRIPO_API_NORMA_PRODUCCION_V1.md) | PRUEBA TÉCNICA |
| `K13` — Engendro debe verse como criatura estática del Bastión II. | Reemplazo 3D estático incorporado en juego con QA I–II; rostro/animación y perfil físico pendientes. [`ART_AND_INTEGRATION_REVIEW.md`](../../docs/evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md); [`ELDORIA_TRIPO_ART_BRAIN_INTEGRATION_20261021.md`](../../docs/ELDORIA_TRIPO_ART_BRAIN_INTEGRATION_20261021.md) | RETENIDO CON LIMITES |
| `K14` — Engendro necesita un gesto animado sin gastar créditos. | V4.2 reparó influencias de pies/brazos y pasó revisión aislada Unity rest/swipe/return; no integrado en canon ni móvil. [`unity-approved.json`](../../docs/evidence/spawnling-animation-v4-review-20261021/unity-approved.json); [`RESULT.md`](../../docs/evidence/spawnling-animation-v4-review-20261021/RESULT.md) | PRUEBA TÉCNICA |
| `K15` — Jabalí rocoso para futuro capítulo. | Fuente aislada legible, QA material 2K/raycast PASS; no integrado ni perfilado. [`TRIPO_API_JABALI_LECCIONES_20261021.md`](../../docs/TRIPO_API_JABALI_LECCIONES_20261021.md); [`TRIPO_MOBILE_LOD_PROOF_20261021.md`](../../docs/TRIPO_MOBILE_LOD_PROOF_20261021.md) | PRUEBA TÉCNICA |

### Optimización móvil

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K17` — Mallas Tripo con ~50k triángulos pesan demasiado para repetición móvil. | Jabalí y Foco: LOD0/1/2 y mapas 1K probados en Blender; faltan Unity integrado, FPS/batería físicos y ART PASS. [`TRIPO_MOBILE_LOD_PROOF_20261021.md`](../../docs/TRIPO_MOBILE_LOD_PROOF_20261021.md); [`tripo-mobile-lod-profiles.json`](../../pipeline/tripo-mobile-lod-profiles.json) | PRUEBA TÉCNICA |

### Aprendizaje del artista Qwen

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K18` — Qwen cambia geometría pero no el significado de la pieza. | La revisión forense identificó habilidades arquitectónicas históricas útiles, no validadas como ruta presente para todo asset. [`ELDORIA_ARTIST_FAILURE_FORENSICS_20261015.md`](../../docs/evidence/ELDORIA_ARTIST_FAILURE_FORENSICS_20261015.md) | MÉTODO HISTÓRICO |
| `K19` — Qwen criatura mueve toda la malla en vez de corregir anatomía. | Ensayo Qwen2B desplazó casi todos los vértices: ART_REJECTED; regla preventiva añadida. [`ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md`](../../docs/ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md); [`shape-change-gate.mjs`](../../tools/tripo-art-brain/shape-change-gate.mjs) | RECHAZADO |
| `K20` — Esculpido Qwen localizado invisible desde la cámara estratégica. | Tercer Qwen sobre Engendro modificó 206 vértices pero solo 2 píxeles estratégicos; no mejora visible. [`ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md`](../../docs/ELDORIA_TRIPO_ART_BRAIN_PHASE2_20261021.md) | RECHAZADO |

### Errores artísticos que NO repetir

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K21` — Cambio de pradera/ribera parece grande en métricas, pero no arregla composición. | OWNER179 alteró ~10,8% píxeles, pero mejora insuficiente y no integrado. [`OWNER179_BANK_MEADOW_VISUAL_VERDICT_20261020.md`](../../docs/evidence/valoria-reference-clean/OWNER179_BANK_MEADOW_VISUAL_VERDICT_20261020.md) | RECHAZADO |
| `K22` — Camino claro superpuesto a ciudad parece una tira artificial. | Native160 rechazado artísticamente pese a 32 pruebas I–II; revertido. [`NATIVE160_ART_REJECTION_AND_ROLLBACK_20261019.md`](../../docs/evidence/valoria-reference-clean/NATIVE160_ART_REJECTION_AND_ROLLBACK_20261019.md) | RECHAZADO |
| `K23` — Ciudad se llena de piezas sueltas sin tejido funcional. | Urban V7 y compact V15 rechazados y revertidos; no autorizan añadir barrios posteriormente. [`URBAN_V7_ART_REJECTION_20261019.md`](../../docs/evidence/valoria-reference-clean/URBAN_V7_ART_REJECTION_20261019.md); [`NATIVE165_COMPACT_QUARTER_ART_REJECTION_20261019.md`](../../docs/evidence/valoria-reference-clean/NATIVE165_COMPACT_QUARTER_ART_REJECTION_20261019.md) | RECHAZADO |

### Continuidad y seguridad operativa

| ID / caso | Decisión y evidencia real | Estado |
|---|---|---|
| `K24` — Un modelo/captura de GitHub Actions desaparece y se pierde el resultado. | Fuentes detallan expiración de artifacts y procedencia SHA; medida de continuidad. [`ELDORIA_ART_LEARNING_EVIDENCE_V1.md`](../../docs/ELDORIA_ART_LEARNING_EVIDENCE_V1.md); [`ARTIST_LEARNING_JOURNAL.md`](../../tools/art-direction/ARTIST_LEARNING_JOURNAL.md) | NORMA ACTUAL |
| `K25` — La siguiente sesión ignora aprendizajes o recibe instrucciones antiguas. | Protocolo de arranque y memoria existente; conexión automática del hook de memoria general no acreditada. [`DOCUMENTATION_INDEX.md`](../../docs/DOCUMENTATION_INDEX.md); [`ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`](../../docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md) | NORMA ACTUAL |
| `K26` — Un método conocido falla y se inicia nueva investigación de herramientas. | Router de fallback probado en capa independiente; integración end-to-end con flujo visual central no acreditada. [`ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md`](../../docs/ELDORIA_VISUAL_FALLBACK_ORCHESTRATOR_V1.md); [`visual-fallback-policy.json`](../../pipeline/visual-fallback-policy.json) | NORMA ACTUAL |
| `K27` — Un candidato se publica o entra al juego por CI verde. | Contratos vigentes separan aprobación artística y entrega funcional. [`WORKFLOW_INDEX.md`](../../docs/WORKFLOW_INDEX.md); [`ELDORIA_ANDROID_FAMILY_UPDATES.md`](../../docs/ELDORIA_ANDROID_FAMILY_UPDATES.md) | NORMA ACTUAL |

## Límites de la evidencia y peligros de interpretación

**Norma no es una prueba:** `canonical_policy` representa cómo debemos trabajar; no certifica que una nueva escena salga mejor. **Integrado no equivale a ART_APPROVED:** la mayoría de las obras retenidas pasaron gates funcionales y muestran ganancias localizadas, pero no cumplen automáticamente el registro formal de aprobaciones [`art-visual-approvals`](../../pipeline/art-visual-approvals/README.md). **Histórico no es destino artístico:** la técnica de roca/arquitectura puede reaprovecharse cuando el canon vigente lo permita, no sus grandes acantilados ni edificios retirados. **Coste 0:** la existencia de Tripo/API o scripts con créditos históricos no autoriza ni solicita gasto nuevo.

Los informes del 18–20 de octubre contienen cronologías con estados obsoletos; se utilizan para extraer *lecciones* y nunca para tomar control de una escena o reiniciar trabajos. Además, varias capturas y artifacts de GitHub caducan; las pruebas que falten deben marcarse pendientes de verificación, sin afirmar aprobación retrospectiva.

## Brechas identificadas para fase 2 (NO ejecutadas aquí)

- El módulo general `tools/art-direction/learning-evidence/` conserva seis casos de Bastión I, pero su adaptador `prepare/stage` no está demostrado como obligatorio en **todas** las inferencias Qwen del juego.
- `author_brief.py` recibe un diario acotado y opcionalmente un inventario de activos; no hay un mecanismo genérico que seleccione por relevancia de estas **27 fichas** antes de cada producción.
- Faltan más evaluaciones artísticas definitivas a nivel pieza y evidencias permanentes para algunas comparaciones antiguas. Los casos del Engendro animado V4.2 han pasado revisión aislada Unity, pero requieren promoción y QA de juego/dispositivo aparte.
- El planificador y el router de contingencia existen; falta demostrar que todas las rutas canónicas los consumen automáticamente sin reiniciar hipótesis obsoletas.
- La fase siguiente debe **conectar al sistema actual** un selector de memoria de solo lectura, con registro de qué fichas/bytes recibió Qwen y pruebas negativas. No crear un nuevo cerebro/pipeline, no activar créditos y no reentrenar pesos del modelo.

## Definición de fase 1 completada

Catálogo JSON de fuentes + estados + restricciones, guía humana de navegación, preservación de negativos y limitaciones explícitas. **Fase 2 y fase 3 siguen pendientes y no han sido habilitadas por crear estos archivos.** Ésta es una fotografía versionada; antes de actuar deberá contrastarse con el `main` actual.

