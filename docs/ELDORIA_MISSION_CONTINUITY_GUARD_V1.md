# Eldoria — control de continuidad de misiones v1

**Estado:** observador independiente de bajo coste. No sustituye al propietario ni a `pipeline/active-workstreams.json`.

## Qué resuelve

Un chat puede terminar después de lanzar Qwen, Blender o Unity. El proceso externo puede seguir ejecutándose. Un workflow verde puede ser sólo una etapa. Este guardián observa el estado real de los workflows configurados, registra la próxima acción segura y conserva **una issue de recuperación persistente** cuando la misión sigue abierta sin actividad externa reciente.

No puede mantener vivo al chat, tomar decisiones artísticas por sí mismo ni ofrecer tiempo de ejecución ilimitado. **No despacha ni reintenta jobs pesados**, no modifica los archivos de producción de otros propietarios y no promueve código.

## Fuentes de verdad

- Propietario: `pipeline/active-workstreams.json`, únicamente `active`.
- Configuración: `pipeline/mission-continuity-policy-v1.json`: ramas, workflows reconocidos, evidencia y umbral de inactividad.
- Evidencia técnica: GitHub Actions API, URLs de las ejecuciones y HEAD de rama.
- Recuperación: una issue por misión identificada mediante marcador `eldoria-mission-continuity:<id>`. **No es una nueva cola, ni un workstream, ni una certificación artística**.
- Evidencia automática: informe JSON adjunto a cada ejecución de la vigilancia.

Estados: `EXTERNAL_RUNNING` (workflow en curso/cola), `NEEDS_RECOVERY` (fallo/cancelación), `WAITING_EVIDENCE` (éxito técnico, pero trabajo todavía abierto), `IDLE_UNFINISHED` (sin ejecuciones), `OWNER_NOT_ACTIVE` (propietario ausente) y `MONITOR_ERROR` (fallo de vigilancia).

Un workflow SUCCESS **nunca** cierra automáticamente la misión ni equivale a VISUAL PASS. Es necesario contrastar la autoría IA, el ciclo Blender/Unity real, la calidad de la cámara y los criterios de jugabilidad y móvil.

## Ejecución

El workflow `.github/workflows/mission-continuity-guard.yml` corre en Ubuntu alojado en GitHub aproximadamente cada media hora o manualmente. El schedule solo se activa al entrar el workflow en la rama por defecto; GitHub puede retrasar sus ejecuciones.

El observador sólo crea una issue cuando el propietario está activo en la rama esperada, el último trabajo lleva al menos 20 minutos sin ejecutar una nueva fase y no existe una issue previa con su marcador. Las issues abiertas se actualizan **solo si cambia** su texto; las cerradas manualmente no se reabren. Se rechaza la creación cuando no puede descartarse una duplicación. GitHub necesita permiso de escritura de issues; un error de API deja rojo el workflow y se conserva la traza posible.

No se concede capacidad de modificar ramas, registrar aprobación artística, gastar créditos, reservar Windows ni despachar nuevas ejecuciones. Una futura automatización de pasos productivos requeriría contratos idempotentes y coordinación explícita del propietario antes de usar recursos compartidos.

## Validación

```sh
node --test tools/mission-continuity/core.test.mjs
node tools/check-workflow-governance.mjs
```

Las pruebas cubren: propietario ausente, job activo, éxito que no implica cierre, fallo/cancelación, periodo de gracia, último run activo, runs de otras ramas y unicidad del marcador de issue.

Para la activación: fusionar el PR tras checks, disparar una vez el workflow manualmente y comprobar el JSON real, el estado de Bastión I y la issue única (si realmente está estancado). No declarar instalada y activa una vigilancia mientras permanezca sólo en una rama de propuesta.

## Reanudación de otro chat

Leer el puntero de la issue como referencia, volver a verificar el HEAD y el estado actual de GitHub, respetar la propiedad del workstream y ejecutar el siguiente gate permitido. La issue no autoriza modificar la escena de otro chat. Un chat detenido no significa que la misión esté completada; el registro canónico conserva el trabajo abierto.
