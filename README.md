# Eldoria — desarrollo canónico Unity

> **Entrada operativa:** leer [AGENTS.md](AGENTS.md), el estado actual en [SESSION_HANDOFF.md](SESSION_HANDOFF.md), [PROJECT_STATE.md](PROJECT_STATE.md) y la propiedad activa en [pipeline/active-workstreams.json](pipeline/active-workstreams.json). El repositorio `main` y los resultados verificables prevalecen sobre la memoria del chat y los registros históricos.

## Juego principal

- **Fuente canónica de gameplay y presentación:** `Unity/` — Unity Editor **6000.3.23f1**.
- **Rama de integración:** `main`.
- **Entrega jugable Unity WebGL:** https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/unity-owner/
- **Identidad de una entrega Unity:** SHA del código, ejecución de compilación, artefacto, QA de candidato, publicación y QA del juego publicado; no se identifica por el número de versión del prototipo web.
- **Flujo para una tarea autorizada:** implementar en Unity → probar → compilar en runner Windows → probar el candidato exacto → corregir si procede → publicar de forma controlada → verificar la versión pública → entregar. No convertir una tarea de ejecución en una auditoría, aprobación departamental o informe intermedio. Respetar recursos compartidos, gates, gastos y límites reales de sesión.

## Compatibilidad y legado web (NO fuente Unity)

- `v0220/index.html` y `v0220/js/`: código editable del **prototipo web heredado**, que puede servir como referencia de paridad, no como autoridad del runtime Unity.
- La versión **v0.32.0** se refiere a los metadatos del prototipo web, **no** a la versión publicada de Unity. `v0220` es un nombre de directorio heredado.
- `playtest/`: resultado generado del prototipo web; no editar directamente. Playtest heredado: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- `tester-v0265/`: instantánea congelada **Eldoria Closed Playtest T1 / 0.26.5-test.2**; no modificar ni utilizar como base de desarrollo.
- `v019*`, `v020*`, `v0210` y los planes de migración antiguos son historia/referencia, no instrucciones para reiniciar la migración. Los baselines de recuperación se preservan sin reactivación automática.

## Validación

- **Unity:** seguir [QA_AND_DEPLOY.md](QA_AND_DEPLOY.md), el proceso canónico de compilación, la prueba del artefacto exacto, la publicación controlada y las pruebas WebGL publicadas, con pruebas visuales e interacción reales cuando correspondan.
- **Solo prototipo web heredado:** `npm install`, `npm run qa:setup`, `npm run validate:local` y sus presets de QA. Estos comandos **no compilan ni certifican Unity**. Una prueba Playwright del prototipo no sustituye una prueba de la build Unity.

Conservar evidencia de trabajos cerrados. Consultar documentos especializados solo si la tarea los requiere; las instrucciones antiguas no se convierten en trabajo pendiente por aparecer en un archivo.
