# Eldoria — control de continuidad de misiones v1

**Estado:** observador independiente con recuperación nocturna EXCEPCIONAL de errores transitorios de infraestructura. No sustituye al propietario ni a `pipeline/active-workstreams.json`.

## Qué resuelve

Un chat puede terminar después de lanzar Qwen, Blender o Unity. El proceso externo puede seguir ejecutándose. Un workflow verde puede ser sólo una etapa. Este guardián observa el estado real de los workflows configurados, registra la próxima acción segura y conserva **una issue de recuperación persistente** cuando la misión sigue abierta sin actividad externa reciente.

No puede mantener vivo al chat, tomar decisiones artísticas por sí mismo ni ofrecer tiempo de ejecución ilimitado. **No inicia fases nuevas ni repara código o arte.** La extensión nocturna puede reintentar UNA vez únicamente el mismo job de una ejecución fallida por un fallo transitorio de infraestructura inequívoco, con rama y propietario comprobados, sin otra ejecución activa y con el recurso asignado. No modifica los archivos de producción de otros propietarios ni promueve código.

## Fuentes de verdad

- Propietario: `pipeline/active-workstreams.json`, únicamente `active`.
- Configuración: `pipeline/mission-continuity-policy-v1.json`: ramas, workflows reconocidos, evidencia y umbral de inactividad.
- Evidencia técnica: GitHub Actions API, URLs de las ejecuciones y HEAD de rama.
- Recuperación: una issue por misión identificada mediante marcador `eldoria-mission-continuity:<id>`. **No es una nueva cola, ni un workstream, ni una certificación artística**.
- Evidencia automática: informe JSON adjunto a cada ejecución de la vigilancia.

Estados: `EXTERNAL_RUNNING` (workflow en curso/cola), `NEEDS_RECOVERY` (fallo/cancelación), `WAITING_EVIDENCE` (éxito técnico, pero trabajo todavía abierto), `IDLE_UNFINISHED` (sin ejecuciones), `OWNER_NOT_ACTIVE` (propietario ausente) y `MONITOR_ERROR` (fallo de vigilancia).

Un workflow SUCCESS **nunca** cierra automáticamente la misión ni equivale a VISUAL PASS. Es necesario contrastar la autoría IA, el ciclo Blender/Unity real, la calidad de la cámara y los criterios de jugabilidad y móvil.

## Ejecución

El workflow `.github/workflows/mission-continuity-guard.yml` corre en Ubuntu alojado en GitHub aproximadamente cada hora, tras un cambio del propio guardián en main, o manualmente. La activación tras fusionar el PR usa push; el schedule solo se activa al entrar el workflow en la rama por defecto; GitHub puede retrasar sus ejecuciones.

El observador sólo crea una issue cuando el propietario está activo en la rama esperada, el último trabajo lleva al menos 20 minutos sin ejecutar una nueva fase y no existe una issue previa con su marcador. Las issues abiertas se actualizan **solo si cambia** su texto; las cerradas manualmente no se reabren. Se rechaza la creación cuando no puede descartarse una duplicación. GitHub necesita permiso de escritura de issues; un error de API deja rojo el workflow y se conserva la traza posible.

No se concede capacidad de modificar ramas, registrar aprobación artística, gastar créditos, reservar Windows ni lanzar nuevas fases productivas. El reintento acotado de un job de GitHub no cambia ese límite. Una futura automatización de pasos productivos requeriría contratos idempotentes y coordinación explícita del propietario antes de usar recursos compartidos.

## Validación

```sh
node --test tools/mission-continuity/core.test.mjs
node tools/check-workflow-governance.mjs
```

Las pruebas cubren: propietario ausente, job activo, éxito que no implica cierre, fallo/cancelación, periodo de gracia, último run activo, runs de otras ramas y unicidad del marcador de issue.

Para la activación: fusionar el PR tras checks, disparar una vez el workflow manualmente y comprobar el JSON real, el estado de Bastión I y la issue única (si realmente está estancado). No declarar instalada y activa una vigilancia mientras permanezca sólo en una rama de propuesta.

## Reanudación de otro chat

Leer el puntero de la issue como referencia, volver a verificar el HEAD y el estado actual de GitHub, respetar la propiedad del workstream y ejecutar el siguiente gate permitido. La issue no autoriza modificar la escena de otro chat. Un chat detenido no significa que la misión esté completada; el registro canónico conserva el trabajo abierto.

## Extensión nocturna v1 — autorización del propietario 16-10-2026

- Única programación: el cron horario ya existente en `.github/workflows/mission-continuity-guard.yml` (GitHub puede retrasar ejecuciones). La ventana de reintento es **22:00–08:00 Europe/Madrid** y respeta horario de verano.
- Vigilancia ampliada a `prototype/reference-first-zero-budget-visual-pilot-20261015` y `art/valoria-63-individual-quality-uplift-20261015`, además de Bastión I. Cada ciclo guarda ambos informes JSON como artifacts; el observador mantiene issues de recuperación solo para propietarios que figuran como activos en main.
- Política adicional `pipeline/night-recovery-policy-v1.json`, motor `tools/mission-continuity/nightly-recovery.mjs`, puertas puras `nightly-core.mjs` y regresiones `nightly-core.test.mjs`.
- El único retry potencialmente autorizado inicialmente es el workflow aislado Unity Reference-First. **Solo** un job de una ejecución fallida, con excepción transitoria inequívoca de GitHub/runner/red en logs. Nunca después de errores de C#, Python, geometría, QA visual, materiales ni assets. Mismo SHA de la rama, último run, ventana de gracia 20 minutos, 1 intento máximo, verificación de estado de otros workflows y claim `windows-runner-heavy` del propietario. Se verifica todo de nuevo inmediatamente antes de actuar; ante duda no hay retry.
- La rama de mejora de 63 assets se **observa solamente**: su workstream no consta con identidad/claim activo en main. Se prohíben reintentos hasta que el chat propietario coordine un registro vigente y se apruebe una política específica. El supervisor no se apropia de la rama.
- Un job en estado éxito no inicia el siguiente ciclo creativo. El supervisor no puede abrir, reconectar ni prolongar un chat de ChatGPT; únicamente archiva estado y actúa ante la excepción técnica definida.
- Costo: cero servicios/créditos de pago. Sin deployments, merges de arte, certificación automática 8/10, ni cambios de producción. Para desactivar reintentos sin parar los avisos: `enabled=false` en `pipeline/night-recovery-policy-v1.json`; para parar todo, desactivar el workflow desde GitHub Actions.
- La instalación debe pasar las pruebas del PR y un disparo controlado del workflow desde main; si falla no puede declararse activa.
