# Eldoria · Codex + Unity CLI + Pipeline (Issue #147)

**Estado de verificación: 2 de noviembre de 2026.** La infraestructura técnica está PROBADA por etapas, no hay una sesión autenticada de Codex manejando arte de Eldoria.

## Evidencia exacta

| Prueba | Resultado | Evidencia |
|---|---|---|
| CLI Linux + protocolo MCP `initialize` / `tools/list` | PASS | run 41467275565; 7 herramientas, ningún editor |
| Windows 10, Unity 6000.3.23f1, Unity CLI existente | PASS | run 41476625118; preflight real |
| Unity CLI → Unity Pipeline 0.9.1-exp.1 en editor Windows scratch | PASS | run 41478984732 |
| Cambiar escena scratch desde Unity CLI, consultar objeto y producir PNG Unity antes/después | PASS | run 41478984732, artifact 13158597570 |
| Codex CLI 0.166.0 + plugin oficial Unity 0.1.8-beta instalados en Ubuntu aislado | PASS | run 41480046885, artifact 13158613473 |
| Codex autenticado ejecutando una decisión de IA dentro del Editor | **NO PROBADO** | no había sesión de modelo ni credenciales; no atribuir a pruebas anteriores |
| Mejora artística de pantalla de Eldoria por el agente | **NO PROBADA** | PR #136 continúa bajo otro titular; no tocar archivos compartidos |

Los paquetes del proyecto publicado no fueron modificados. No hubo Tripo, coste nuevo, reinicio ni publicación de una versión de juego. Capturas originales de 320×180 son deliberadamente una prueba técnica de un objeto creado, no aprobación artística.

## Qué aporta de verdad al proyecto

1. **Se elimina una incertidumbre técnica:** un agente puede interactuar con Unity vía la CLI/MCP y el Editor puede ejecutar cambios reales sin convertir el proyecto en un conjunto de scripts manuales. El plugin de Codex está disponible y se puede instalar sin añadir una nueva suscripción de herramienta.
2. **Pruebas separadas de producción:** reproducir el canal en un proyecto scratch, con comprobación de procesos y reserva, y destruir solo procesos propios. Los workflows Windows son **manuales**, nunca se ejecutan al cambiar una PR.
3. **Visibilidad sobre resultados:** la QA visual de PR #146 ya puede comparar PNG auténticos y generar reportes before/after. No equipara métricas de píxeles con fidelidad artística.

## Puerta de entrada para probar Codex auténtico

- Recuperar una sesión de Codex con autenticación explícita válida de su propio usuario (no incluir claves en repositorios, logs ni CI). Una instalación del plugin **no es autenticación**.
- Comprobar `codex --version`, `codex plugin list`, `codex login status`, `unity --version`, `unity pipeline editors --format json`. Nunca ejecutar agentes con acceso de escritura a `main`.
- Para ensayos, crear una copia con un solo worktree de origen fijo SHA y `Library` propia. Solo si Windows está libre y el workstream autorizado posee el recurso exclusivo. **Nunca lanzar dos Unity Editor simultáneos en el runner.**
- Ofrecer una única tarea de UI concreta, real y reversible (preferentemente Marchas o Entrenamiento después del relevo de PR136). Exigir trazabilidad del modelo, instrucciones, comandos de Unity ejecutados, captura original y captura nueva en cuatro resoluciones, tiempos, pruebas de UI y guardado idéntico.
- El resultado que no pase calidad artística independiente sigue siendo candidato. No asignar `ART_APPROVED`, no hacer merge del juego ni publicar.
- Si la autenticación de Codex no está disponible, detener solo esta etapa y documentar el bloqueo exacto; **no utilizar el script de CLI como evidencia de que un modelo tomó decisiones**.

## Controles de seguridad

- `.github/workflows/eldoria-unity-cli-editor-proof.yml` y `.github/workflows/eldoria-unity-cli-windows-safe-pilot.yml`: `workflow_dispatch` **únicamente**, runner Windows, puerta de titularidad `windows-runner-heavy` en main, comprobación de procesos y copia scratch bajo `RUNNER_TEMP`.
- `.github/workflows/eldoria-codex-plugin-isolated-qa.yml`: GitHub-hosted Ubuntu, `HOME` y `CODEX_HOME` temporales, sin `OPENAI_API_KEY`, sin llamadas al modelo; comprueba plugin oficial con `codex plugin list`.
- Plugin/CLI experimentales: comprobar cambios de versión antes de reutilizar; no actualizar automáticamente Unity ni el juego. Evitar `unity auth login` automático y no exponer el servidor Pipeline fuera del dispositivo.
- El trabajo en PR #149 es infraestructura de prueba y **no debe alterar la PR #136**.

## Fuentes

- https://github.com/Unity-Technologies/unity-agent-plugin
- https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package
