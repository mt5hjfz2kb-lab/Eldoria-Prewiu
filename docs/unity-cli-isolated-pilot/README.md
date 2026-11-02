# Eldoria · Issue #147 · Prueba aislada de Unity CLI

La **meta real** es que Codex actúe sobre Unity (observe escena, ajuste un componente reversible, capture la imagen y verifique controles). Unity no adquiere una IA autónoma: necesita un agente externo y la conexión Pipeline.

## Etapa A (esta PR)
- Instalar la CLI oficial en un **Ubuntu efímero de GitHub**, nunca en el PC Windows de Eldoria.
- Verificar versión, ayuda, lista de editores disponibles y resultado de `unity pipeline list`.
- Comprobar existencia de los metadatos del plugin oficial de Unity para Codex.
- Conservar `proof.json` y logs de instalador, sin publicar secretos.
- Evitar cualquier `unity auth login`, editor `install`, `unity eval`, cambios en paquetes del proyecto, gastos, reinicios o conexiones a PC.

## Etapa B (no se realizará con Windows reservado)
1. Confirmar el relevo real del titular de `windows-runner-heavy`, cola y procesos activos.
2. Preparar copia aislada de Unity 6000.3.23f1 conservando `Assets` y `Packages`, con una nueva `Library`. Respetar la caché original y no mover activos del proyecto canónico.
3. Comprobar `unity --version`, `codex --version` y capacidades/credenciales ya disponibles, sin instaladores inesperados.
4. Instalar `com.unity.pipeline` únicamente en la copia y comprobar mediante `unity pipeline list` que el editor aparece.
5. Ejecución read-only del agente para inspeccionar jerarquía y errores; luego UNA edición reversible de presentación sin modificar gameplay.
6. Capturas nativas Unity antes y después, test funcional exacto, medición de tiempo/errores y análisis del supervisor visual de PR #146.
7. Ninguna aprobación artística automática ni publicación/merge de juego.

**Límite esencial**: aprobar la CLI en Linux es solamente compatibilidad de herramientas; **no demuestra todavía conexión Codex ↔ editor Unity**.

Documentación oficial:
- https://docs.unity.com/en-us/unity-cli/use-unity-cli
- https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package
- https://github.com/Unity-Technologies/unity-agent-plugin

Relación: Issue #147, sin duplicación ni cambios a PR #136.
