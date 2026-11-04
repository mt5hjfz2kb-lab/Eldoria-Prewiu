# Eldoria — revisión semántica de documentación, lote 1

**Fecha:** 2026-11-04  
**Workstream-ID:** `eldoria-documentation-semantic-review-20261104`  
**Alcance:** diagnóstico de cinco documentos de instrucciones/decisiones antiguas y sus autoridades actuales. Solo lectura de documentos originales y del catálogo; **ninguna reclasificación aplicada**. No modifica Unity, `AGENTS.md`, catálogo compartido, scripts, automatizaciones ni arte. No autoriza gastos o publicaciones.

## Qué se ha comprobado

Las etiquetas de `docs/documentation-catalog.json` y los contenidos de los documentos se han contrastado con las reglas de prioridad de `docs/DOCUMENTATION_POLICY.md`, el veto expreso de `AGENTS.md` y el contrato de retirada de Qwen `docs/ELDORIA_QWEN_ART_DECOUPLING_20261022.md`. No se ha interpretado un antiguo `PASS` como una certificación vigente del juego.

| Documento | Evidencia contrastada | Diagnóstico y disposición propuesta | Riesgo de utilizarlo como orden vigente |
| --- | --- | --- | --- |
| `docs/ELDORIA_AI_AUTHORING_ENFORCEMENT_V1.md` | Su sección «Proceso obligatorio» exige Qwen/Ollama local para toda autoría artística; `AGENTS.md` y `ELDORIA_QWEN_ART_DECOUPLING_20261022.md` revocan expresamente ese requisito desde 22/10 y documentan integración en PR #104. | **Contradicción directa confirmada.** Conservar como evidencia histórica de la técnica; no seguir sus pasos obligatorios Qwen. Candidato a `historical` con aviso de sustitución. Su estado real en catálogo continúa `review_pending` hasta que el responsable del catálogo lo actualice. | **Alto**: puede forzar dependencias y pruebas que el propietario prohibió exigir. |
| `docs/ELDORIA_PARALLEL_DELIVERY_HANDOFF_20261010.md` | Fija responsables, HEAD, ramas y estado de un runner del 10/10. Su propia sección «Recovery procedure» pide releer registro y GitHub Actions vivos. `AGENTS.md` y `pipeline/active-workstreams.json` son la autoridad actual. | **Instantánea temporal, no mapa vivo.** Proponer `historical` con referencia al registro de titularidad y al protocolo de concurrencia. Catálogo actual: `review_pending`. | **Alto**: usar sus reservas y SHAs como actuales podría invadir trabajo ajeno. |
| `docs/VALORIA_HERO_BASTION_V1_FINAL_GATE.md` | El informe del 01/10 registra `TECH PASS / VISUAL PARTIAL / NO PRODUCTION PROMOTION`; `AGENTS.md` (25/10) prohíbe explícitamente usar el modelo antiguo y exige conservarlo archivado. | **Evidencia histórica válida, no autorización artística.** Proponer `historical` y referencia al veto actual; no borrar GLB ni `.meta`. Catálogo permanece sin editar. | **Alto** si alguien confunde el PASS técnico con permiso de uso. |
| `docs/ELDORIA_CANONICAL_AUTOMATION_SANITATION_2026_10_06.md` | Describe un saneamiento de rutas experimentales y un `PASS / CLOSED` fechado. Coincide en intención con las reglas actuales que impiden reactivar experimentos retirados; **no** se ha comprobado aquí cada disparador de workflow en el HEAD vivo. | **Resultado histórico con comprobación técnica adicional pendiente** antes de declarar que cada workflow sigue solo manual. Propuesta provisional `reference` o `historical`, no `current`. | **Medio**: el estado de los workflows puede cambiar desde la fecha del informe. |
| `docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md` y su `_RESULT.md` | El contrato prioriza Unity para composición, Blender para procesamiento y Tripo solo tras demostrar necesidad/autorización; el resultado del 01/10 documenta pruebas técnicas y aclara que las transformaciones avanzadas Tripo no estaban certificadas. | **Contrato y prueba técnica históricos parcialmente reutilizables**, no garantía de que cada puente esté operativo hoy. Mantener `review_pending` hasta contrastar herramientas actuales y sus tests. | **Medio**: un PASS de octubre no demuestra ejecución actual ni calidad visual. |

## Decisiones operativas seguras ya obtenidas

1. Para **autoría artística**, Qwen/Ollama **no es obligatorio**. La dirección actual y las pruebas de calidad están en `AGENTS.md` y el contrato de desacoplamiento; un agente no debe ejecutar el flujo antiguo solo porque un documento de octubre lo llame obligatorio.
2. Para **titularidad, reservas, Windows y ramas**, no usar nombres, HEAD ni reservas citados en el handoff del 10/10; leer el registro vivo y las ejecuciones de GitHub.
3. Para el **Bastión rechazado**, preservar la evidencia pero **no reutilizar ni publicar** el modelo por un antiguo TECH PASS.
4. Para **automatizaciones y Tripo**, distinguir prueba histórica de disponibilidad presente; no inferir permisos de gasto ni ejecución por un informe antiguo.

## Qué NO se ha certificado

- No se ha revisado el contenido de los 175 documentos pendientes; este lote cubre **seis archivos concretos en cinco grupos temáticos**, con tres contradicciones/riesgos de autoridad prioritarios. La cifra global de `review_pending` **no disminuye** por crear este informe.
- No se han verificado todos los workflows experimentales ni las APIs actuales de Blender/Tripo. Los informes históricos no equivalen a un test reciente.
- No se han editado cabeceras de documentos reservados, ni `docs/documentation-catalog.json` porque está reclamado por otros workstreams; hacerlo aquí sería una invasión de titularidad.
- No se han movido o borrado archivos idénticos. Los 43 grupos entre áreas requieren una revisión independiente de referencias, licencias, uso dinámico y procedencia.

## Próximo lote seguro

Tras un relevo/claim explícito del catálogo, aplicar **primero** los tres avisos de mayor riesgo y reclasificar con evidencias: obligación Qwen obsoleta, handoff de 10/10 y Bastión vetado. Ejecutar el guard documental y sus pruebas en una PR separada. Después revisar los otros 169 documentos aún no inspeccionados por este lote, priorizando los 11 de `project`, los 14 de `engineering` y las normas de `art` que puedan orientar trabajos activos. Ningún cambio de estado se hace por el título del archivo ni por automatismo.
