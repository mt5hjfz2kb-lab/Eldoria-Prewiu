# Eldoria — Retirada segura de Qwen del flujo artístico (22-10-2026)

## Decisión del propietario
Qwen/Ollama deja de ser requisito para planificar, crear, revisar o aprobar arte en Eldoria. La dirección artística corresponde a ChatGPT y a las referencias canónicas del proyecto. Qwen permanece instalado y opcional para tareas técnicas ajenas al arte. No eliminar Ollama, Blender, BlenderMCP, Unity, Tripo, memoria, modelos ni evidencias históricas.

## Reglas de compatibilidad
- La retirada NO supone un ART_APPROVED ni reduce exigencias de capturas reales A/B Unity, autoría editable, trazabilidad, QA móvil y revisión independiente.
- El gate artístico debe aceptar autoría trazable del responsable designado sin exigir inferencia Qwen ni consumo de memoria por Qwen; conservar controles de procedencia de decisiones, archivos, geometría, capturas y pruebas.
- Los ciclos históricos de Qwen siguen siendo evidencia válida; no reescribirlos ni borrarlos.
- No tocar workstreams activos ni runners ajenos sin relevo autorizado. Las solicitudes en curso deben conservarse y migrarse solo con compatibilidad comprobada.
- El planificador debe describir fases independientes del modelo (diagnóstico visual, plan de autoría, ejecución Blender/Unity y validación).
- No modificar `main` hasta probar gobernanza, pruebas unitarias y un ciclo de autoría sin Qwen de principio a fin.

## Pruebas de aceptación
1. Una solicitud artística nueva sin Qwen planifica y supera gobernanza si aporta evidencia válida.
2. Una solicitud aprobada sin capturas Unity auténticas sigue siendo rechazada.
3. Un ciclo histórico con Qwen sigue siendo legible.
4. No se invoca Qwen en la ruta artística por defecto.
5. La compilación Unity y los workflows no dependen del servicio Ollama.
6. Ningún workstream concurrente pierde su estado o propiedad.

Estado: **especificación de migración; NO ejecutada ni validada**.