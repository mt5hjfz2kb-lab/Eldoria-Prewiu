# Eldoria — arranque técnico vigente

Actualizado el 21-10-2026 por encargo de reducir esperas sin alterar el juego.

## Arranque mínimo

1. Leer `main/AGENTS.md`, HEAD de la rama encargada, solo cabeceras pertinentes de `SESSION_HANDOFF.md` y `PROJECT_STATE.md`, y solo `active` de `pipeline/active-workstreams.json`.
2. La base de integración es `main`. Las pausas diagnósticas de 14–16 de octubre son históricas, no una nueva prohibición global. Respetar las pausas y reservas actuales por alcance. UI V3 y engendro no se consideran terminados por este mantenimiento.
3. Comprobar herramientas disponibles en esta sesión y consultar únicamente los workflows/runs del trabajo pertinente. No descargar ni releer toda la historia por defecto.
4. Con checkout y Node: `node tools/eldoria-agent-preflight.mjs` ofrece diagnóstico local de solo lectura. Con conector GitHub, leer rutas y jobs; no afirmar que el diagnóstico se ejecutó en Windows.
5. Antes de una operación pesada, contrastar reserva y job real. No cancelar procesos ajenos ni asumir disponibilidad por una cola aparentemente vacía.

## Consulta breve del estado y recuperación de resultados

- Con checkout: `node tools/work-status/report.mjs` (o `--json`) genera una vista pequeña desde `active`, el manifiesto WebGL y el canal Android. No carga el histórico en su salida ni modifica las autoridades.
- En GitHub: workflow `eldoria-work-status.yml`, pestaña Summary y artefacto `eldoria-work-status-RUN_ID`. Añade ramas y jobs Windows observados. Una reserva declarada, un job observado y un proceso del PC son hechos distintos; un fallo de API se muestra como información parcial, nunca como PC libre.
- El flujo nativo del engendro conserva una copia de los archivos generados antes de publicarlos, con SHA fuente, identidad del run, hashes y resultado de QA. Si falla Git, recuperar su artefacto `spawnling-native-recovery-RUN_ID-ATTEMPT` antes de repetir Unity.
- La publicación de esa copia requiere su configuración versionada, QA correcta y el workstream activo original. Rechaza cambios concurrentes de Unity y nunca fuerza el remoto ni limpia, cambia o rebasa el directorio del editor. Un artefacto retenido tampoco equivale a integración ni a aprobación artística.
- Guía de uso: `tools/workflow-recovery/README.md`. La escena y el workflow de Valoria y los archivos de UI conservan sus propietarios; no conectar otros productores sin reconciliar su alcance.

## Capacidades existentes

- Unity 6000.3.23f1: runner Windows, etiqueta `unity-6000-3-23f1`, variable `UNITY_EDITOR_PATH`; ver `.github/workflows/unity-slice.yml` y workflow específico de la tarea.
- Blender 5.2.2, Ollama/Qwen y BlenderMCP: históricamente probados en el PC. Evidencias Blender/MCP: runs 38607499677, 38612645467 y 38766521237. No instalar duplicados ni asumir que el servicio permanece iniciado.
- GitHub permite lectura y las escrituras autorizadas que exponga la sesión. GitHub Desktop instalado no equivale a control remoto del PC. Un localhost inaccesible desde ChatGPT no prueba ausencia del servicio en Windows.
- Context7, si está disponible, sirve para dudas concretas de versión/API. Si falta, continuar por documentación oficial sin convertir el acceso en otra misión.

Distinguir siempre: declarado, probado históricamente, accesible ahora en el chat, probado en el job actual y bloqueado. No son intercambiables.

## Ejecución y cierre

- Coste 0 por defecto; ningún descubrimiento autoriza compras, créditos, descargas de modelos o producción ajena.
- Reutilizar evidencia válida cuando sus entradas y superficies no hayan cambiado. Agrupar correcciones coherentes antes del siguiente ensayo pesado; no repetir builds completos por cambios exclusivamente documentales.
- Para arte, consultar los contratos vigentes y fuentes existentes; conservar autoría, pruebas A/B y revisión independiente. Un PASS técnico no es ART_APPROVED.
- Un fallo requiere diagnóstico concreto; una denegación de seguridad no se elude por otra API. Continuar trabajo independiente permitido.
- Mensajes de progreso no son pausas. No pedir «continúa» para pasos ya autorizados. No afirmar ejecución de fondo al terminar un turno.
- Cerrar solo con resultado solicitado comprobado o bloqueo real; registrar rama/SHA, evidencia y pendiente exacto. No modificar los estados de entrega de otros propietarios.

## Historia bajo demanda

[Bootstrap completo anterior, con hashes, modelos y ensayos diagnósticos](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3dcbea127105bbd765c7c24dcd5da7084ea1fe75/tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md). Es evidencia histórica, no una orden para reiniciar pruebas ni reactivar ramas.
