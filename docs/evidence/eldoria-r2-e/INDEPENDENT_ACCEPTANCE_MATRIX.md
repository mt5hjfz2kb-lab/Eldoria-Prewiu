# R2-E — Matriz independiente de aceptación y publicación de la Ronda 2

Fecha 2026-10-08. D13 QA, D14 publicación, D02 arquitectura. Es una preparación de criterios sobre producto real, NO cierre de R2 ni repetición de QA móvil R1.

## Candidato R2-B en curso
Fuente gameplay y panel: `6e0e12396a98b04333b595fff704afa3e849c30a`. UI Progression Certification run `37838898438`: SUCCESS, job `113523346859`, EditMode + PlayMode + resumen + artifact, pero verificar además la evidencia específica de nuevos botones en build publicable. Source/editor run `37838898336`: SUCCESS, job pesado SKIPPED por evaluación de impacto; no sustituye a Unity UI. Producción Unity run `37838898425` con request/parcel-source y capture en ejecución al corte.

## Matriz de evidencias obligatorias R2-B
1. Código de dominio: NUnit para 2 elecciones, recursos, stock, idempotencia, stale revision, guardado preexistente y reload. La existencia de tests no constituye su veredicto hasta leer results.
2. UI: ambas opciones realmente clicables con panel Bosque de Valoria, descripción clara en dos orientaciones y sin solapar botones existentes. El texto de recompensa no puede confundirse con la recolección normal de 360 madera.
3. Juego publicado: recorrer HOME→MUNDO→Bosque→opción elegida→REINO→reload→MUNDO, comprobar decisión persistida y no repetible; repetir desde save fresco para la otra rama. Verificar return/reset/cámara.
4. Regresión protegida: Bastión I–II, Cuartel, ejército, marcha, Scout/Engendro y persistencia, sin errores runtime fatales.
5. Publicación: no sustituir `/unity-owner/` estable por candidato no certificado o no autorizado; separar URL estable y candidato y conservar SHA del candidato con artefactos.
6. QA independiente M03 y seguimiento de fallos M04 con owner y reintentos acotados; sin auto-merge ni gastos.

## R2-A y C
R2-A usa planificador canónico con cero créditos y exige capturas oficiales World en estados comparables, señalización y selección de POI conservando SHARP Valoria. R2-C exige capture/playthrough portrait/landscape para nuevo panel y navegación real antes de llamar mejora UX terminada.

## Restricciones
La emulación móvil no sustituye iPhone físico; sin mediciones reales no informar FPS, memoria o consumo. M07 conserva su propiedad visual y M11 depende de su aceptación. Informe de QA no se autocertifica por haber creado este fichero. Si un run termina durante una conversación cerrada, su resultado permanece en Actions/artifacts; no afirmar que un agente reanudará automáticamente cambios de código sin workflow explícito de consumo/QA.

## PLAYER-REPORTED P1 EXPERIENCE GATES — DO NOT CLOSE BY AUTOMATED MACROLOOP ALONE

- EXP-CAM-01: Valoria long vertical touch drags in both directions visibly traverse required areas, with measured before/after frame positions and bounded limits, portrait and landscape. Technical camera values alone do NOT pass.
- EXP-BLD-02: Aserradero, Bastión and Cuartel hotspot selection, target-size checks, action CTA visibility, construction/action outcome, close/reopen, save/reload, orientation-specific screenshots, including overlap or missed touches. A PASS on game state alone does NOT pass.
- Compare against `docs/VALORIA_BASTION_I_II_VERTICAL_SLICE_MIGRATION_V1.md` and approved UX. If mobile player observation conflicts with simulated PASS, keep P1 bug open and escalate independent visual review.
- Weekly Monday real published Chromium probe runs via existing `unity-webgl-startup.yml`, with automated DG issue intake; the weekly probe is a regression screen, not a physical user-experience sign-off.
- No promotion of R2-B WebGL candidate or replacement of stable Unity owner link until these two issues have been specifically retested or explicitly documented as unresolved and excluded from acceptance by a separate owner decision.
