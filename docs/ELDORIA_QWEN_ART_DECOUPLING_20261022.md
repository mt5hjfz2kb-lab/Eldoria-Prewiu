# Eldoria — Retirada segura de Qwen del flujo artístico (22-10-2026)

## Decisión del propietario
Qwen/Ollama deja de ser requisito para planificar, crear, revisar o aprobar arte en Eldoria. La dirección artística corresponde a ChatGPT y a las referencias canónicas del proyecto. Qwen permanece instalado y opcional para tareas técnicas ajenas al arte. No eliminar Ollama, Blender, BlenderMCP, Unity, Tripo, memoria, modelos ni evidencias históricas.

## Reglas de compatibilidad
- La retirada NO supone un ART_APPROVED ni reduce exigencias de capturas reales A/B Unity, autoría editable, trazabilidad, QA móvil y revisión independiente.
- El gate artístico debe aceptar autoría trazable del responsable designado sin exigir inferencia Qwen ni consumo de memoria por Qwen; conservar controles de procedencia de decisiones, archivos, geometría, capturas y pruebas.
- Los ciclos históricos de Qwen siguen siendo evidencia válida; no reescribirlos ni borrarlos.
- No tocar workstreams activos ni runners ajenos sin relevo autorizado. Las solicitudes en curso deben conservarse y migrarse solo con compatibilidad comprobada.
- El planificador debe describir fases independientes del modelo (diagnóstico visual, plan de autoría, ejecución Blender/Unity y validación).
- La consolidación final autorizada el 30-10-2026 integra los controles tras verificar gobernanza y compatibilidad. Una prueba de contrato con fixtures no certifica un ciclo artístico real ni sustituye la revisión visual de sus recursos.

## Pruebas de aceptación
1. Una solicitud artística nueva sin Qwen planifica y supera gobernanza si aporta evidencia válida.
2. Una solicitud aprobada sin capturas Unity auténticas sigue siendo rechazada.
3. Un ciclo histórico con Qwen sigue siendo legible.
4. No se invoca Qwen en la ruta artística por defecto.
5. La compilación Unity y los workflows no dependen del servicio Ollama.
6. Ningún workstream concurrente pierde su estado o propiedad.

## Consolidación del 30-10-2026
PR104 incorpora la implementación de PR101 y conserva sus commits. El planificador y la memoria usan dirección artística independiente del modelo. `DIRECT_ARTIST_BLENDER` admite un ciclo trazable sin rol, prompt ni registro de inferencia Qwen; conserva plan ejecutado, archivos y hashes, geometría modificada, capturas Unity A/B y revisión visual independiente. El verificador sigue leyendo ciclos históricos `AI_DIRECTED_BLENDER`.

Validación local: 77 pruebas de autoría, memoria y continuidad aprobadas; gobernanza de producción aprobada. Las pruebas nuevas verifican la admisión del contrato directo y el rechazo cuando faltan capturas, revisión independiente o correspondencia entre plan y ejecución. Son fixtures sintéticos identificados como tales: **no se ha producido ni aprobado arte nuevo mediante estas pruebas**. La revisión artística de cada recurso conserva sus propios requisitos.

El adaptador histórico `run-artist.mjs` sigue disponible exclusivamente para invocaciones explícitas. Ni el planificador ni la gobernanza lo ejecutan como requisito. No se instala, elimina ni invoca Ollama, Tripo o Unity en esta consolidación.
