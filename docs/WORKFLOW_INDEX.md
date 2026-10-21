# Eldoria — rutas de trabajo y validación

**Estado:** `current` como índice de localización. **Revisión operativa:** 21-10-2026. El índice no concede permisos ni certifica actividad, calidad o publicación: contrastar registro, rama, SHA y run.

## Desarrollo y entregas

| Uso | Ruta existente | Condición |
| --- | --- | --- |
| Base jugable integrada | [valoria-reference-clean.yml](../.github/workflows/valoria-reference-clean.yml) | Unity real sobre la escena canónica; runner Windows compartido, alcance y reserva vigentes. |
| Certificación UI y progreso | [ui-progression-cert.yml](../.github/workflows/ui-progression-cert.yml) | Tests y capturas nativas cuando corresponda. Las ampliaciones de UI V3 en su PR no se consideran integradas por existir en esa rama. |
| Entrega WebGL actual | [eldoria-itch-delivery.yml](../.github/workflows/eldoria-itch-delivery.yml) | Candidato y promoción en [manifiesto de entrega](../pipeline/playtest-delivery.json); QA y bytes exactos. Subida aceptada no equivale a activación remota ni prueba del alojamiento. |
| Build Android familiar | [eldoria-family-android-build.yml](../.github/workflows/eldoria-family-android-build.yml) | Solicitud Android habilitada, versión monotónica, firma y procedencia. Un build verde no publica. |
| Publicación Android | [eldoria-family-android-publish.yml](../.github/workflows/eldoria-family-android-publish.yml) | Release y canal solo tras verificar APK, firma, tamaño y SHA. |
| Automatización nocturna Android | [eldoria-family-android-nightly.yml](../.github/workflows/eldoria-family-android-nightly.yml) | Su programación y configuración viva determinan cuándo actúa; no es publicación de cada commit. |
| Pages histórico | [pages.yml](../.github/workflows/pages.yml) | Ruta retirada, conservada para trazabilidad. No reactivarla para entregar el WebGL actual. |
| Regresión del prototipo web | [tester-qa.yml](../.github/workflows/tester-qa.yml) | Conserva todas las pruebas. En PR omite la suite cuando solo cambian Unity, arte, documentación o metadatos excluidos; rutas desconocidas, comparación incompleta o error de API ejecutan toda la suite. No certifica Unity. |

## Controles de infraestructura

| Familia | Ruta | Qué comprueba |
| --- | --- | --- |
| Estado breve de los trabajos | [eldoria-work-status.yml](../.github/workflows/eldoria-work-status.yml) | Informe derivado de las fuentes vigentes y jobs observados; permisos de lectura, sin segundo registro ni publicación. El informe es una instantánea con fecha. |
| Recuperación de resultados nativos | [Guía de checkpoint](../tools/workflow-recovery/README.md) | Integrado en el flujo nativo del engendro. Conserva resultados antes de enviar cambios; exige QA e identidad y rechaza conflictos sin sobrescribir el editor. |
| Arquitectura y documentos | [repository-architecture-guard.yml](../.github/workflows/repository-architecture-guard.yml) | Estructura del árbol Git exacto, catálogo, navegación, pruebas de errores y memoria artística. Descarga solo los contenidos leídos; ejecución local por defecto conserva inspección de archivos del árbol de trabajo. |
| Propiedad y autoría artística | [art-production-governance.yml](../.github/workflows/art-production-governance.yml) | Solicitudes, ownership y evidencias obligatorias. No determina belleza. |
| Autoridad visual | [valoria-visual-authority-routing.yml](../.github/workflows/valoria-visual-authority-routing.yml) | Escena, referencia y responsable canónicos; permite otros trabajos autorizados sin crear una segunda autoridad visual. |
| Evidencia de aprobación visual | [eldoria-art-visual-acceptance.yml](../.github/workflows/eldoria-art-visual-acceptance.yml) | Rechaza aprobaciones sin evidencia. Si existen declaraciones, materializa todos los bytes necesarios; no sustituye juicio artístico. |
| Gobierno de workflows | [workflow-governance.yml](../.github/workflows/workflow-governance.yml) | Integridad de automatizaciones. |
| Cuarentena | [workflow-quarantine.yml](../.github/workflows/workflow-quarantine.yml) | Aislamiento de rutas heredadas. |
| Planificación artística | [art-production-plan.yml](../.github/workflows/art-production-plan.yml) | Elección de ruta desde solicitudes vivas; no lanzar por mera existencia del YAML. |

## Cómo continuar sin duplicar trabajo

1. Leer `active` del [registro](../pipeline/active-workstreams.json), HEAD de `main` y rama encargada. El registro declara titularidad; comprobar aparte jobs, reservas y recursos realmente utilizados.
2. Consultar solo el workflow y las evidencias del alcance. Un archivo ausente de `main` puede pertenecer a una rama autorizada; no reconstruirlo ni ejecutarlo desde otro trabajo.
3. Agrupar una corrección coherente y comprobar el defecto concreto antes de otra compilación pesada. Reutilizar una prueba solo si las entradas y superficies que valida siguen siendo idénticas.
4. Distinguir preparación, integración, arte aceptado, QA nativa, APK/WebGL, upload, activación remota y prueba del alojamiento. Ninguna fase demuestra las demás.
5. No cancelar un job ajeno, publicar una candidata ni ocupar Windows para comprobar documentación.

Las ramas históricas y los workflows no incluidos siguen sujetos a su clasificación y configuración reales; no se presumen obsoletos ni autorizados. Para actualizar una rama abierta, reconciliar con `main` conservando los cambios de su propietario y volver a ejecutar únicamente los gates afectados.

Ver [índice documental](DOCUMENTATION_INDEX.md), [política documental](DOCUMENTATION_POLICY.md), [entrega WebGL](ELDORIA_PLAYTEST_DELIVERY.md), [actualizaciones Android](ELDORIA_ANDROID_FAMILY_UPDATES.md) y [AGENTS](../AGENTS.md).
