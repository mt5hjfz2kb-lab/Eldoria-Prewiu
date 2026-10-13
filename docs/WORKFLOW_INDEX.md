# Eldoria — Catálogo de workflows y punto de entrada

**Estado:** `current` como **índice de localización** · **Revisión:** 2026-10-13. No certifica qué workflows están activos ni si un job está en curso. Para eso consultar GitHub Actions y el SHA correspondiente.

El repositorio contiene numerosos workflows de prototipos y campañas anteriores. **No se deben activar simplemente por encontrar su YAML**. La existencia del archivo no demuestra uso actual ni que sea el responsable correcto de la tarea.

| Familia | Ruta | Rol conocido / cautela |
| --- | --- | --- |
| Arquitectura | [repository-architecture-guard.yml](../.github/workflows/repository-architecture-guard.yml) | Comprueba invariantes y, en esta rama, documentación. Ligero; no runner Windows |
| Gobierno artístico | [art-production-governance.yml](../.github/workflows/art-production-governance.yml) | Validación de trazabilidad y reglas; CI verde **no** implica `ART_APPROVED` |
| Gobierno de workflows | [workflow-governance.yml](../.github/workflows/workflow-governance.yml) | Comprobación de integridad de automatizaciones |
| Cuarentena | [workflow-quarantine.yml](../.github/workflows/workflow-quarantine.yml) | Conserva aislamiento de workflows heredados |
| Planificación de arte | [art-production-plan.yml](../.github/workflows/art-production-plan.yml) | Proceso especializado; verificar triggers y peticiones vivas antes de ejecutar |
| Publicación | [pages.yml](../.github/workflows/pages.yml) | Solo con publicación autorizada y QA exacto; respetar propietario de Pages |
| Tester congelado | [tester-qa.yml](../.github/workflows/tester-qa.yml) | Regresión histórica protegida; no modifica tester-v0265 |

## Rutas aisladas / históricas

Las compilaciones, ensayos Qwen, escenas y tests de Bastión I pueden existir únicamente en su rama de integración. **Consultar HEAD de esa rama y el registro**: un workflow ausente de `main` o una ejecución roja antigua no autorizan a reproducirlo desde otro chat. La Región 1, publicación y artista IA conservan sus titulares.

Los workflows no incluidos en esta tabla se consideran **sin clasificación documental**, no automáticamente obsoletos ni seguros de ejecutar. Cuando uno se eleve a producción, añadirlo a este catálogo y relacionarlo con sus gates, SHA, titular y archivo de petición; no crear una segunda ruta solo para documentarlo.

## Cómo averiguar qué está ejecutándose

1. Identificar el workstream en el [registro `active`](../pipeline/active-workstreams.json).
2. Leer el último HEAD de la rama propietaria y sus solicitudes de ejecución.
3. Consultar GitHub Actions filtrando por rama/SHA, verificar `status` y `conclusion` del **run y job**, y sus artefactos/logs.
4. Registrar por separado cambio de código, CI verde, integración Unity, A/B visual, APK/WebGL y publicación. No dar ninguna fase por completada usando el PASS de otra.

Véanse [índice documental](DOCUMENTATION_INDEX.md) y [AGENTS.md](../AGENTS.md).
