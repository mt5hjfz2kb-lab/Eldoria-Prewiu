# Eldoria — Plan Maestro de Migración a Unity

**Documento rector de la migración propuesta · 2026-09-24.** Repositorio canónico `mt5hjfz2kb-lab/Eldoria-Prewiu`; `main` observado `cbcad3ca1f9a8d8c4b0c82bc6012b56c3a0be4b1`; fuente jugable `v0220/index.html` + `v0220/js/`; build de desarrollo generada `playtest/`; testers congelados `tester-v0265/`. Versión web v0.32.0. Este bloque es **auditoría y planificación**: no crea proyecto Unity, no sustituye el web, no actualiza versión de producto ni modifica testers. Revalidar HEAD y `SESSION_HANDOFF.md` en la siguiente sesión.

## Leer en este orden

`AGENTS.md` → `SESSION_HANDOFF.md` → `PROJECT_STATE.md` → este plan → [`UNITY_READINESS_REPORT.md`](UNITY_READINESS_REPORT.md) (estado/evidencia) → [`UNITY_ARCHITECTURE.md`](UNITY_ARCHITECTURE.md) (contratos) → [`UNITY_VERTICAL_SLICE.md`](UNITY_VERTICAL_SLICE.md) (primer build) → [`UNITY_ASSET_AUDIT.md`](UNITY_ASSET_AUDIT.md) (arte). Dirección de producto: `docs/ELDORIA_MASTER_MAP.md`, `docs/ELDORIA_MASTER_MAP_V1_CLOSURE.md` y `docs/MVP_ART_DIRECTION.md`. `DESIGN_DECISIONS.md` contiene historia además de decisiones vigentes: resolver conflictos según jerarquía del repo y código actual. `CHANGELOG.md` es historia, no HEAD.

## Mandato y límites

**Etapa 1:** consolidar en Unity el núcleo jugable validable y elevar escala/cámara/arte/animación/UX móviles. **Etapa 2:** mundo multijugador persistente con estado y autoridad de servidor. Construir la etapa 1 con ID de jugador/reino/mundo, comandos validados, reloj, guardado/versiones, regiones, entidades y marchas listos para sustituir autoridad local, sin fabricar jugadores/alianzas/servidores falsos y sin elegir backend aún. Mantener web como especificación observable hasta obtener paridad y validación de un corte Unity. Cierre de Mapa Maestro V1 permite planificar y arrancar el corte mínimo; detalles PvP/Oráculos/Gobierno/Brecha global expresamente aplazados.

## Clasificación de migración

| MIGRAR — reglas/contratos verificables | REDISEÑAR — intención sí, implementación no | POSPONER | NO ARRASTRAR |
| --- | --- | --- | --- |
| Bucle Valoria→mundo→recompensa→retorno; costes/producción/misiones I–X; tareas UTC/offline; roster por familia/tier; héroes y afinidades; IDs/enemigos/rasgos; inventario/equipo; cartas (descubierto/poseído/consumido/Indestructible/cooldown); Hospital y heridos PvE. | `s.power` acumulativo y desglose, reconciliación `marchSetup`/`combatMarch`, estado guardado, UI/cámara, finales simplificados, economía y pacing tras observación; diálogo ES/EN por IDs; reportes/telemetría útiles. | Práctica/Relicario completos después de primera slice; resto de I–X por capas; PvP/Duelo online, Alianzas, servidor global, consejo, Oráculos, Puerto, eventos y monetización hasta etapa adecuada. | DOM/CSS y overlays, `localStorage` como autoridad, `Date.now()` global sin abstracción, reparaciones por flags y compatibilidad v022/v023 como arquitectura nueva, `runtime-hotfix.js`, `qa-fixtures.js` en builds jugador, emoji como arte final, rankings simulados presentados como mundo real. |

## Roadmap con puertas de salida

| Fase | Construir | Depende de | Criterio de salida |
| --- | --- | --- | --- |
| **0. Congelar referencia web** | Confirmar HEAD publicado y certificado v0.32.0, registrar CI/run, capturar fixtures de estado y casos I/III/VI/VII/IX/X sin tocar tester; revisar sesiones humanas existentes y lagunas. Especificar discrepancias Poder/Marcha en `UNITY_READINESS_REPORT.md`. | Acceso a `main`/CI y producto; no depende de Unity | Baseline identificable por SHA, reporte reproducible y decisiones semánticas anotadas; si falta evidencia humana, marcada como riesgo, no inventada. |
| **1. Fundaciones Unity** | Crear proyecto 6.3 LTS en `Unity/`, paquete lock, escenas Bootstrap/Valoria/Frontier, assemblies, CI EditMode/build, save v1, comandos/reloj/RNG, localización ES/EN. Mantener web y flujo de Pages intactos. | Fase 0 para paridad; aprobar hipótesis reversible de cámara/formatos | Proyecto abre en otra máquina/CI con patch fijado, tests puros verdes, build escritorio instalable, fuente web y tester sin cambios. |
| **2. Primera slice visible** | Recorrido exacto [`UNITY_VERTICAL_SLICE.md`](UNITY_VERTICAL_SLICE.md); kits Valoria/frontera, Aldric/arqueros, Brecha, mejora, marcha, recolección y un encuentro, recompensa/regreso, audio/VFX/UI. | Fase 1, arte de bloqueo y dispositivos de prueba | Build jugable móvil, progreso persistente, claridad novato, combate explicable y crecimiento visible; perfilado en al menos iPhone/Android objetivo. No exigir I–X aún. |
| **3. Consolidación núcleo I–X** | Migrar por tramos I–III, IV–VI, VII–VIII, IX–X: misiones, economía, héroes/Forja, Reliquias/Práctica, Hospital/final; revalidar diseño de combate/ritmo. | Slice y reglas reconciliadas | Suite de paridad por escenarios, saves/offline, I–X en Unity sin ayudas, evaluación humana del deseo de continuar, arte/UX coherentes; v0.32 web permanece como referencia hasta aprobar reemplazo. |
| **4. Preparar etapa 2** | Diseñar opciones de backend y threat model con requisitos medidos; prototipo técnico de gateway autoritativo aislado; identidad y mundo compartido por hitos, luego Alianzas/PvP según producto. | Núcleo estable, escala/reglas/operación de eventos definidas | Pruebas de autoridad, idempotencia, reconciliación, privacidad y coste; especificación y decisión de proveedor aparte. No es un hito de la primera slice. |

**No convertir fases en versiones web**: el web conserva v0.32.0 salvo hotfix real; la futura build Unity tendrá su propio identificador de proyecto/commit y reglas de versión aprobadas. Ninguna nueva rama paralela es necesaria por defecto. Commits coherentes en `main` con CI selectivo; proteger arte fuente y archivos Unity binarios grandes con Git LFS si el hosting/presupuesto lo permite, sin subir builds a git.

## Puertas técnicas críticas

1. **Poder y Marcha:** especificar un roster canónico de propiedad y despliegue, fórmula pura Total (desglose), Expedition y combate, reglas de heridas/curación; si cambian números, test de comparación y migración justificada. Lo actual usa `s.power` más equipo y un combate que selecciona arqueros T1/1 héroe fuera de `marchSetup` (`UNITY_READINESS_REPORT.md`).
2. **Save/economía:** migradores versionados; coste al inicio y premio idempotente; reloj inyectado y tareas serializables; no confiar en client clock cuando haya servidor. Una partida web existente no se convertirá automáticamente a Unity sin un importador explícito y testado.
3. **Mundo futuro:** IDs de región/entidad y ciclo vida con revisiones, propiedad, resultados de marcha/recolección, estado de Brecha por servidor **separado** de madurez/edad estructural. Dos servidores de misma edad pueden tener estado divergente; un servidor más joven puede adelantar una edad de Brecha. Evitar un singleton global compartido de `WorldState`.
4. **Producto/arte:** cámara y escala validadas en dispositivo, licencias de assets demostrables, comparación de ruta sin explicación externa; medir costes de contenido/producción antes de ampliar I–X.

## Automatización para nuevas sesiones de Work + GitHub

- Mantener `AGENTS.md` como inicio y añadir punteros a estos cinco docs (sin sustituir protocolo web); nueva sesión verifica `main`, SHA, último workflow, patch Unity y versión/release vigente; no reconstruir reglas desde conversaciones.
- CI separado por rutas: cambios `Unity/**` ejecutan compilación Unity batchmode, tests EditMode/PlayMode y empaquetado de build de escritorio como artefacto con SHA/reglas; cambios web siguen Pages/QA actual. Proteger `tester-v0265/` con la comparación ya existente. No publicar automáticamente una candidata Unity como «estable» solo por compilar.
- Matriz de QA de reglas con fixtures deterministas: ahorro/gasto, construcción/reclutamiento/curación offline, Bastión/orden de misiones, forecaster≡combate, drop/Indestructible, tropas propias/heridas/marcha, guardar/reanudar, ES/EN. Escena smoke automatizada hace clicks reales hasta recompensa/regreso; pruebas visuales capturan Valoria, Mundo, combate y mobile safe areas. Además, playtest humano de claridad/ritmo.
- Build iPhone: primero editor/desktop para iterar; cuando el dueño solicite móvil, proyecto con iOS Build Support y **Mac + Xcode** o runner macOS/Unity Build Automation; firma con certificado/perfil y cuenta Apple Developer cuando corresponda. Publicar IPA por App Store Connect y grupo interno **TestFlight**; el usuario abre enlace/invitación y prueba sin compilar localmente. La distribución externa puede requerir revisión beta de Apple. Documentación: [Unity iOS signing](https://docs.unity.com/en-us/build-automation/sign-build-artifacts/sign-an-ios-application), [Unity Build Automation](https://docs.unity.com/en-us/build-automation), [Apple TestFlight](https://developer.apple.com/help/app-store-connect/test-a-beta-version/testflight-overview/). Credenciales y certificados solo en secretos del servicio, nunca en git/Work chat. Costs/quotas de CI, Apple y distribución se verificarán antes de habilitarlos; no prometer coste cero en iPhone.

## Riesgos de etapa 2 y mitigación desde etapa 1

| Riesgo | Mitigación ahora |
| --- | --- |
| Trampa/local clock; doble gasto/doble loot/reintento de red | Comandos con actor, ID, revisión e idempotencia; reloj autorizado por puerto; transacciones y ledger. |
| Cliente decide combate/botín/heridos | Simulador determinista sobre snapshot y reglas versionadas; resultado verificable, entrada limitada a intención y composición. |
| Marcha doble reservando mismas tropas | Ownership + reserva de roster por `marchId`/task, validación de disponibilidad atómica. |
| Desfase de mundo/servidor y content updates | `serverId`, revisión por entidad/región, edad estructural vs Brecha separadas, catálogo versionado y migrations. |
| Casa/gobierno/Alianza mal delimitados | `realmId`≠`worldId`≠`allianceId`, permisos en gateway, aportes por propietario; no implementar reglas políticas prematuras. |
| Cliente móvil sin conexión y varios dispositivos | Save local etapa 1 declara autoridad local; diseñar resolución de conflictos/reconexión antes de activar cuenta y sincronización, no asumir merges mágicos. |
| Coste operativo y licencias | Benchmarks/perfilado temprano, volumen de eventos estimado, comparativa backend diferida; registro de derechos de assets. |

## Siguiente encargo exacto al agente de desarrollo

> **«Inicia la Fase 0/1 de `UNITY_MIGRATION_PLAN.md` en el repositorio canónico `mt5hjfz2kb-lab/Eldoria-Prewiu`. Verifica `main`, CI y la referencia v0.32.0; reconcilia por escrito las reglas mínimas de Poder, Marcha y combate relevantes para la primera slice con fixtures del prototipo, sin reescribir el web. Crea en `Unity/` un proyecto Unity 6.3 LTS (patch fijado) con URP, Bootstrap/Valoria/Frontier, assemblies Domain/Application/Infrastructure/Presentation, catálogos mínimos, reloj/RNG/gateway local, save versionado y tests EditMode de economía/tareas/marcha. Construye enseguida el greybox jugable Valoria→bosque→marcha→recompensa→Aserradero, y un art spike de cámara/ruinas/Brecha que pueda revisarse visualmente. No implementes online, no borres el web ni toques `tester-v0265/`. Ejecuta tests/build en el entorno disponible, registra limitaciones de editor/iOS/arte, deja CI para la slice y entrégame un build verificable y capturas de móvil/desktop. Si necesitas mi elección sobre estética/cámara o dispositivo objetivo, presenta dos opciones visuales concretas antes de fijarlas.»**

El inicio físico de Unity puede ejecutarse sin cerrar PvP/Gobierno/Oráculos; las decisiones del Director que sí afectan el *art spike* están en `UNITY_READINESS_REPORT.md` y deben resolverse con imágenes jugables, no suposiciones.
