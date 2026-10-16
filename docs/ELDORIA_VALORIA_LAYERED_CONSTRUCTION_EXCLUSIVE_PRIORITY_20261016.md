# ELDORIA — PRIORIDAD EXCLUSIVA DEL PROPIETARIO: CONSTRUCCIÓN DE VALORIA POR CAPAS (2026-10-16)

## Orden vigente y alcance
El propietario ha ordenado: **prioridad absoluta a la construcción de Valoria conforme al plano maestro y a las capas profesionales; todos los demás trabajos activos de Eldoria quedan PAUSADOS** hasta nueva instrucción expresa.

Esta orden reemplaza las excepciones de actividad simultánea anteriores en cuanto a prioridad/ejecución. No deroga las protecciones de ramas, escenas, presupuesto, calidad ni gobernanza. NO reanuda las dos ramas previamente congeladas (Bastión I modular y mejora de los 63 assets). La ejecución de Valoria debe hacerse **en un alcance aislado autorizado y reclamado por un único chat constructor**, sin alterar ramas ajenas, producción canónica o publicaciones. Este archivo, por sí solo, no reclama runner ni cancela jobs o sesiones remotas.

## Único objetivo de producción autorizado
ID lógico: `valoria-master-plan-layered-construction-20261016`.

Una primera escena real de Valoria en Unity 6000.3.23f1, modular, convincente y preparada para integración jugable, basada en el plano maestro existente y la referencia aprobada. Presupuesto 0 €; no créditos ni compras.

**Orden de ejecución obligatorio**:
1. Leer `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, este documento, registro activo y políticas de concurrencia. Verificar HEADs, ownership, jobs y capacidades reales.
2. **RECUPERAR, NO REINVENTAR, EL PLANO MAESTRO**: `docs/VALORIA_MASTER_PLAN_V1.md`, `docs/VALORIA_PROGRESSION_MAP_V1.md`, `docs/VALORIA_CAMERA_EXPANSION_PLAN_V1.md`, `docs/VALORIA_PLAYABLE_DISTRICT_SKELETON_V1.md`, `docs/VALORIA_MASTER_ENVELOPE_CAMERA_GATE_V1.md`. Preservar núcleo y circulación certificados, parcelas y ampliaciones hasta el horizonte planificado Bastión 25–35. Extraer un plano cenital verificable del estado real. Si una etapa está certificada, reutilizarla.
3. Verificar las cámaras autorizadas para el runtime y la composición de `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` (SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`). No mezclar cámaras de prototipos ortográficos y SHARP sin distinguir sus contratos. Capturas comparables paisaje/retrato.
4. **CAPAS POR DEPENDENCIA** en una escena aislada: (A) terreno y relieve; (B) río, puente, caminos y escaleras/circulación; (C) blockout de volúmenes; (D) ensamblaje modular arquitectónico real; (E) materiales y superficies; (F) vegetación, agua y contexto/vida; (G) iluminación y atmósfera; (H) integración y prueba de la jugabilidad existente.
5. Primero demostrar continuidad visual del terreno/puente/acceso y una puerta inferior completa (dos torres, arco, portón, murallas y bases), en contexto de la escena entera. **No utilizar los 63 assets por obligación.** El kit se subordina a la topología y a los conjuntos arquitectónicos funcionales.
6. En cada capa comprobar alturas, contacto, uniones, escala, circulación y raycasts; guardar captura Unity reproducible y A/B en la cámara del jugador. Rechazar losas flotantes, árboles suspendidos, estructuras incoherentes, piezas dispersas y bloques sólo cosméticos. No avanzar arte final si hay defectos estructurales importantes.
7. Mantener como referencia de producto el vertical slice web `v0220`, pero usar `Unity/` como fuente de verdad del runtime; conservar `PlayerState`, `LocalGateway`, cámara, hotspots, persistencia y macroloop. No recrear economía, progreso o UI falsa. Preservar la presentación visual certificada como baseline hasta una promoción por gates.
8. Seguir `docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md`, `docs/UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md`, `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` y `docs/VALORIA_FULL_FRAME_CONVERGENCE_LOOP_V1.md`. Distinguir TECH PASS, ART PASS local y FULL-FRAME PASS; un build verde no constituye una victoria visual. Optimización/perfilado móvil con medición real cuando proceda.
9. Tras un gate fallido, corregir causas verificadas y reejecutar; no crear ramas paralelas, reruns costosos o informes para sustituir implementación. Tras interrupción de sesión, guardar SHA, artefactos y siguiente acción. No afirmar trabajo en segundo plano sin proceso real.

## Cuatro trabajos vivos al emitir esta orden: pausa expresa
Trasladados a `history` del registro como `handoff` por PAUSA DEL PROPIETARIO (no completados):
- `world-region-1-visual-convergence-v3-20261010`.
- `world4x-zone1-isolated-reference-prototype-20261013`.
- `eldoria-full-blender-artist-studio-20261014`.
- `valoria63-clean-modular-assembly-20261016`.

Otros trabajos ya pausados siguen pausados; archivos, resultados, pruebas, assets y ramas de todos ellos se conservan. Ningún actor puede reanudarlos automáticamente ni usar recursos reservados para ellos antes de la cesión/validación correspondiente.

## Claims, runner, jobs y publicaciones
El nuevo chat constructor **debe reclamar el workstream** mediante el procedimiento de `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md` antes de editar o ejecutar trabajo pesado. No se otorga ownership ficticio a un chat todavía no conectado. La pausa lógica no cancela acciones de GitHub ni detiene otros chats ya abiertos: cada sesión debe releer este mandato y parar. Revisar jobs real-time antes de pedir el runner.

Al registrar la orden, el listado de GitHub Actions devolvió 0 `in_progress`, 0 `waiting`, y dos probes Unity WebGL antiguos `queued`: #37642384303 y #37642364289. Su cola debe comprobarse y cancelarse si corresponde mediante control autorizado en GitHub; no afirmar cancelación por haber editado este registro.

Prohibidas publicaciones, promociones a `main` de arte jugable, gastos, reanudaciones externas y ediciones de escenas canónicas sin autorización separada y verificación de gates. Las modificaciones de este documento y del registro son **gobernanza de la orden actual**, no un despliegue de producción.

## Relación con el chat constructor
El propietario proporcionará al único chat constructor el prompt integral de construcción por plano maestro y capas. Este documento fija la prioridad en el repositorio; **no crea ni inicia automáticamente una sesión de ChatGPT Work, un proceso de Blender o una build Unity**. El constructor empezará desde `main` actual, comprobará si otro chat posee un alcance superpuesto y documentará su propio claim antes de ejecutar.

**Cierre de esta orden de prioridad:** todos los otros trabajos continúan pausados hasta una nueva instrucción inequívoca del propietario.