# ELDORIA — ESTABILIZACIÓN TÉCNICA / ENTREGA Y PUERTA DE REANUDACIÓN (14-10-2026)

**ESTADO: PAUSA GLOBAL DE PRODUCCIÓN ORDENADA POR EL PROPIETARIO.**
La orden de pausa está en `AGENTS.md` de `main`, Bastión I, mundo 4X y artista Blender, y en `pipeline/active-workstreams.json`. Tiene precedencia sobre las instrucciones de trabajo ininterrumpido. **No reanudar la producción, fusionar PRs ni iniciar secuencias de arte/Unity por éxito técnico hasta orden expresa del propietario.** Se permite solamente estabilización técnica aislada a 0 €, sin interrumpir las fuentes del juego ni usar el runner Windows compartido.

Este documento es un handoff de pruebas, no una autorización de promoción ni un diagnóstico de calidad visual.

## Correcciones preparadas y preservadas

1. **Artista general IA / Blender — [PR #58](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/58), draft**, rama `stabilization/eldoria-toolchain-20261014`, base `art/eldoria-full-blender-artist-studio-20261014`. Corrige automáticamente SOLO identificadores de seguimiento inválidos o duplicados de Qwen, preserva respuesta original y SHA256; no cambia operaciones, geometría, posiciones ni aprobaciones. Pruebas reales de Blender y GLB **SUCCESS #38588900599**. Dos ediciones independientes consecutivas de una malla heredada con Qwen 2B y Blender real, **SUCCESS #38592242486**, evidencia artefacto 11907584871. No se ha alcanzado ART_APPROVED.

2. **Mundo 4X / zona 1 — [PR #59](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/59), draft**, rama `stabilization/eldoria-world4x-contracts-20261014`, base `prototype/world4x-zone1-zoom-reference-20261013`. Qwen elige X/Y, curvatura, radio y desplazamiento vertical original; el valor Z inicial se mide mediante inspección de geometría física de la fuente `.blend` inmutable. No inventar, fijar, clamplear ni sustituir el diseño artístico. Validación tipada independiente, protección de ciudad y bordes, origen y evidencia. Instalar y comprobar `numpy` **dentro del Python embebido en Blender** para exportar GLB. Concurrencia de Git: preservación con rebase acotado, sin force push, reteniendo artifact si hay conflicto. Tests de contrato, inspección de fuente y concurrencia **SUCCESS #38595577879**. Qwen 4B generó dos caminos válidos al primer intento en **#38595477284**, pero la ejecución falló posteriormente por `numpy` ausente; el plan original conservado fue reproducido íntegro sin nueva inferencia y con `numpy` corregido en **SUCCESS #38596962021**: 2 operaciones originales, 1684 vértices cambiados, GLB + .blend, topología de fuente y límites/pad protegidos verificados, sin aprobación artística; artefacto 11910731042. Segunda reproducción independiente del mismo plan Qwen sin nueva inferencia **SUCCESS #38597220492**. Nueva inferencia integral **#38596649111** estaba en curso en la última inspección; consultar su conclusión en GitHub antes de utilizarla.

3. **Bastión I — [PR #60](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/60), draft**, rama `stabilization/eldoria-bastion-guard-20261014`, base `integration/valoria-bastion1-player-scene-20261012`. Causa real del fallo #38585780283: Blender autoriza y produce máximo 1.28 m en el terreno A19, pero el guard de Unity rechazaba >0.65 m. Se armoniza el máximo contractual a 1.30 m (margen 0.02 m) **sin alterar la generación**, manteniendo el máximo de movimiento protegido en carretera/parcelas de 0.0005 m, verificación SHA, prohibición de colliders y nodos nuevos y NO promoción automática. Pruebas aisladas **SUCCESS #38596686197**. No se ha ejecutado nueva Unity integral porque la producción está pausada.

## Gobernanza documental validada

La primera publicación de este documento provocó el fallo del guard de arquitectura **#38597799713** porque faltaba en `docs/documentation-catalog.json`. El catálogo se corrigió en `0b42e379c7fae5036aa445ada955ad876b2bb7ed` y el guard completo terminó **SUCCESS #38598223536**. Los siguientes documentos directos de `docs/` deben registrarse también en el catálogo.

## Requisitos para reanudar, una vez el propietario lo ordene

- Revisar cada PR contra el HEAD **actual** de su rama de origen, resolver cambios concurrentes preservando el contenido; nunca sobrescribir ni force push. Nunca transferir al mundo 4X decisiones geométricas fijas como si hubieran sido creadas por Qwen.
- Aplicar cambios técnicos y ejecutar las pruebas específicas en cada rama; las ejecuciones de referencia citadas demuestran aisladamente que el método funciona, no que las ramas originales hayan sido reparadas ya.
- Recuperar imágenes con cámara idéntica y someter mejoras gráficas a revisión independiente: art_approved sigue `false` en los tres procesos. Para Bastión, volver a correr Unity nativo solo después de reanudación autorizada y comprobar las mismas invariantes, gameplay, cámara y Android/iOS.
- Antes de dejar operar a agentes autónomos de producción, mantener guardas contra una segunda misión con recurso ocupado y registro de procedencia `source_SHA -> original_AI_plan_SHA -> candidate_SHA -> independent_gate`. Evitar lanzar varias ejecuciones artísticas por cada commit documental.
- Mantener presupuesto 0 € y los modelos Blender/Unity existentes. Toda reanudación requiere instrucción expresa del propietario y una actualización coherente de `AGENTS.md` y `pipeline/active-workstreams.json`.

**NO CONCLUIR:** que la referencia artística 9,5/10 está alcanzada; que el juego móvil está certificado; que Work queda automáticamente cancelado o reanudado por este archivo; o que un `SUCCESS` de CI equivale a `ART_APPROVED`.
