# ELDORIA — INSTRUCCIONES OPERATIVAS CANÓNICAS (2026-10-10)

**Este es el único punto de entrada operativo de agentes para Eldoria, tanto chats normales como Work.** Sustituye las instrucciones de agente anteriores sobre procedimiento, continuidad, auditorías, aprobaciones y rutas visuales históricas. El texto completo previo se conserva como **historia no normativa** en [docs/archive/AGENTS_PRE_CLEANUP_20261010.md](docs/archive/AGENTS_PRE_CLEANUP_20261010.md). Ni el archivo histórico ni entradas antiguas de estado otorgan tareas, reservas o prohibiciones presentes.

## 1. Autoridad y prioridades
- **La solicitud actual del propietario y el estado vivo del repositorio prevalecen sobre decisiones históricas**, dentro de permisos, seguridad, legalidad, presupuesto, propiedad concurrente y gates objetivos de calidad.
- Rama canónica: `main`; juego real: `Unity/`, Unity 6000.3.23f1. Prototipo web `v0220/` y `playtest/` son legado/compatibilidad, no fuente del gameplay Unity. Conservar `tester-v0265/` congelado.
- Una orden expresa para investigar o desarrollar una **alternativa sin SHARP en una rama aislada** prevalece sobre las congelaciones antiguas de investigación y prohibiciones de métodos que solo regían etapas anteriores. **No supone** autorizar su integración en `main`, cambiar la autoridad visual canónica, modificar gameplay o declararla apta sin pasar los gates correspondientes.
- En producción canónica, la presentación de Valoria sigue protegida por `pipeline/visual-authority-routing-v1.json`, `ValoriaCanonicalRuntimeGuard.cs` y `docs/VALORIA_VISUAL_AUTHORITY_ROUTING_GUARD_V1.md`. No sustituirla por `VisualWorld.cs` ni confundir capturas de regresión con la estética publicada. Referencia aprobada: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`. Para un bloque artístico, consultar los contratos visuales **vigentes que afecten a su alcance**, sin reactivar aprobaciones de 2026-10-05/06 ni congelaciones supersedidas.
- Decisiones de topología irreversible, gasto/créditos, adquisición comercial o acciones que requieran acceso exclusivamente humano necesitan autorización explícita. **Presupuesto predeterminado: 0 créditos de pago.**

## 2. Arranque mínimo, fuentes de verdad y concurrencia
1. Leer este archivo y obtener HEAD vivo de la rama relevante; nunca sustituirlo por la memoria del chat.
2. Leer **solo las cabeceras vigentes** de `SESSION_HANDOFF.md` y `PROJECT_STATE.md` que afecten a la tarea; son principalmente registros cronológicos, no instrucciones por sí mismos.
3. Leer exclusivamente `active` de `pipeline/active-workstreams.json` para identificar propietarios y recursos. `history` tiene valor probatorio únicamente; antiguas palabras STOP, HANDOFF, PENDING, COMPLETED o REVIEW no se aplican al encargo actual.
4. Delimitar un único ámbito coherente sin colisionar con titularidades activas. Seguir `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md` para reclamaciones, recursos compartidos, reconciliación y cierre. No alterar archivos de otro propietario ni reclamar runner/Pages por suposición. No crear departamentos, colas paralelas ni nuevos sistemas de coordinación.

## 3. Papel del chat: ejecutar, no auditar por defecto
- Cuando el encargo es de desarrollo, **implementar y comprobar**: editar → prueba focalizada → corregir → compilar cuando corresponda → QA del artefacto exacto → inspección visual/funcional → publicación autorizada → prueba pública → entrega.
- Un commit, script PASS, render, captura, artefacto, build en cola o mensaje de progreso **no cierran el trabajo**. Si un gate falla, investigar, reparar y repetir las acciones permitidas. Calidad técnica y artística son gates separados; no declarar VISUAL PASS mirando solo CI.
- **Mensaje informativo ≠ pausa ni handoff.** Tras un mensaje, continuar usando las herramientas mientras haya pasos disponibles. No pedir «continúa» para acciones reversibles ya autorizadas.
- Un runner reservado no paraliza modelado, importadores, pruebas Linux, Blender, herramientas y otras tareas independientes. Distinguir reserva declarada, job realmente ocupado y conflicto concreto; no invadir recursos ajenos.
- No inventar ejecución ilimitada, herramientas no disponibles ni trabajo persistente después de acabar el turno. Si una operación es denegada, buscar rutas alternativas **autorizadas**, sin eludir controles.

## 4. Cuándo se puede terminar un encargo
**Solo** por (a) resultado pedido entregado y verificado; (b) decisión humana imprescindible, gasto/irreversibilidad; (c) bloqueo concreto de acceso/herramientas/propiedad después de intentar alternativas seguras; o (d) interrupción efectiva de plataforma. No atribuir automáticamente a un supuesto «límite de llamadas»: describir la última operación realmente disponible o fallida y guardar SHA/run/artefacto/paso pendiente. Nunca presentar progreso parcial como entrega ni afirmar que continúa en segundo plano.

## 5. Integridad de Unity, workflows y publicación
- Preservar gameplay, progreso, economía, persistencia, cámara y navegación salvo encargo explícito; respetar `docs/PROGRESSION_VISUAL_CONTRACT.md`, `docs/UI_REFERENCE_CONTRACT.md` y la autoridad visual aplicable.
- Mantener intacta la arquitectura del repositorio. `pipeline/repository-architecture-invariants.json` y `tools/check-repository-architecture.mjs` controlan operaciones estructurales; un fallo rojo **impide promocionar/publicar**, pero es una tarea de diagnóstico y reparación, no una excusa para entregar un informe. Nunca crear un mini-repositorio ni borrar rutas por checkout parcial.
- Mantener activos los controles de workflow, cuarentena, planner/fallback, art-production governance y gates de Unity. Consultar `pipeline/workflow-quarantine-v1.json`, `tools/check-workflow-governance.mjs` y documentación específica al cambiar esas áreas. No gastar créditos Tripo sin aprobación, fuente exacta y coste conocido. Preferir autoría de malla Blender real sobre maquillaje procedimental o capturas falsas.
- Compilaciones Unity reales usan el runner Windows compartido. No duplicar jobs pesados ni publicar mientras el resultado es incierto. `pipeline/unity-publish-request.json` y `.github/workflows/pages.yml` gobiernan publicación; verificar SHA/candidato/artefacto, QA de candidato y QA publicada. Prohibido reutilizar el PASS de otro commit. QA legacy `npm run validate:local` **no certifica Unity**. Seguir `QA_AND_DEPLOY.md`.
- Evidencia de experiencia real: distinguir Chrome emulado, Unity PlayMode, capturas revisadas y prueba física en móvil; no afirmar que una equivale a otra. La revisión manual del propietario es opcional salvo elección subjetiva expresamente reservada; una ausencia de aprobación no es automáticamente bloqueo.

## 6. Documentación
- Actualizar `SESSION_HANDOFF.md` tras una entrega o interrupción importante **solo si el archivo no está reservado por otro workstream**. `PROJECT_STATE.md` cambia al modificar producto, no por informes repetidos.
- Respetar nombres/versiones y changelog si una entrega los cambia, sin alterar el runtime por un cambio documental.
- Para reglas especializadas consultar documentos específicos *solo cuando sean aplicables*; ningún documento histórico puede contradecir las normas operativas actuales. El archivo previo [archivado](docs/archive/AGENTS_PRE_CLEANUP_20261010.md) solo sirve para recuperar evidencias y decisiones de época, no para impartir órdenes.
