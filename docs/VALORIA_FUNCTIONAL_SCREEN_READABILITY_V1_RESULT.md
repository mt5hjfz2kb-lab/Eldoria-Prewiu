# VALORIA FUNCTIONAL SCREEN READABILITY v1 — RESULT

Status: CLOSED  
Final verdict: **TECH PASS / VISUAL PARTIAL / NOT PROMOTED**  
Date: 2026-10-04

## Objective

Test whether the already SOURCE-VISUAL-PASS Granero and Cuartel can become unmistakably readable in the real playable Valoria frame at zoom 9 and mobile through one bounded presentation/layout intervention per building, without changing camera, Final Look, terrain, Hero Bastion, accepted defense or gameplay topology.

## Canonical inputs

- Granero source: `Valoria_Granero_GCSUv1.glb` — SOURCE VISUAL PASS from run **37206352843**, artifact **11305265539**.
- Cuartel source: `Valoria_Cuartel_GCSUv1.glb` — SOURCE VISUAL PASS from the same gate.
- Prior Unity source-integration gate: run **37206779989**, artifact **11305635813**, TECH PASS / VISUAL PARTIAL.
- Camera: canonical **ORTHOGRAPHIC**.
- Final Look: RESET-accepted runtime look.
- Tripo/paid generation: not used.

## STOP-GATE A — cause of weak screen-space read

Inspection of the prior matched captures showed that the problem was not missing source detail.

Concrete causes:
- both approved sources entered the lower city at roughly the same terracotta/stone screen scale as surrounding civil architecture;
- the Cuartel sat too far toward the right edge of the played frame and lost much of its guarded facade in the mobile crop;
- the Granero's authored loading/storage mass was present but competed with the foreground defense and adjacent roofs instead of owning a clean functional silhouette;
- at mobile width the Bastion and foreground defensive band dominate the frame, leaving insufficient independent screen-space for both functional buildings to read simultaneously;
- small cues could not solve this without becoming prop-dependent, which was explicitly disallowed.

## STOP-GATE B — one bounded layout hypothesis

No source geometry, materials or small props were changed.

Granero:
- previous placement: approximately `(-3.10, 0.16, -4.45)`, footprint `5.05`, height `4.15`, yaw `7°`;
- candidate placement: `(-2.35, 0.16, -5.00)`, footprint `5.68`, height `4.68`, yaw `0°`;
- intent: move inward/forward, increase apparent mass modestly and expose the source-authored loading/storage face to the canonical camera.

Cuartel:
- previous placement: approximately `(6.90, 0.16, -3.65)`, footprint `5.15`, height `4.35`, yaw `-8°`;
- candidate placement: `(5.55, 0.16, -4.70)`, footprint `5.62`, height `4.72`, yaw `0°`;
- intent: recover the guarded facade inside the mobile/zoom9 screen, enlarge military mass modestly and separate it visually from the civil edge.

This is the only layout/presentation hypothesis executed in this block.

## STOP-GATE C — real Unity gate

Authoritative run: **37208123181 SUCCESS**  
Artifact: **11305333489**

Matched evidence:
- BEFORE zoom 9: `before-9.png`
- AFTER zoom 9: `after-9.png`
- BEFORE mobile: `before-mobile.png`
- AFTER mobile: `after-mobile.png`

Technical evidence:
- gameplay collider/hotspot signature preserved;
- source geometry unchanged;
- exactly 2 visual placements;
- orthographic camera preserved;
- RESET Final Look preserved;
- accepted defense unchanged;
- Hero Bastion unchanged;
- terrain unchanged;
- focused gameplay tests PASS;
- renderers: **850 -> 850**;
- materials: **81 -> 81**;
- triangles: **2,166,395 -> 2,166,395**;
- lights: **31 -> 31**;
- Tripo credits: **0**;
- paid credits: **0**.

## Visual verdict

**VISUAL PARTIAL / NOT PROMOTED.**

Zoom 9:
- the new placement makes both approved sources somewhat easier to notice;
- Cuartel gains useful screen-space and Granero gains a clearer roof/storage mass;
- however, neither reaches a sufficiently dominant functional silhouette to become instantly unmistakable against the lower-city civil/defensive mass.

Mobile:
- the intervention is visibly different from BEFORE;
- but the criterion still fails: Granero and Cuartel do not survive simultaneously as two obvious, independently readable functions;
- one lower building mass remains much more legible than the other and the foreground defense/civil roofs still consume too much of the available screen-space hierarchy.

This means the layout hypothesis improved presence but did **not** solve functional readability.

## Promotion decision

- Functional Screen Readability candidate: **NOT PROMOTED**.
- `ValoriaFunctionalScreenReadabilityV1.Enabled` remains false by default.
- Existing playable runtime remains canonical.
- Granero/Cuartel approved source assets remain retained as SOURCE VISUAL PASS assets.
- Rollout to Forja/Hospital/Cantera remains **HOLD**.

Per the anti-loop rule:
- no second layout pass;
- no v2/v3 placement iteration;
- no material retuning;
- no prop accumulation;
- no Tripo spend.

## Required next technique

The evidence now shows that these sources are good isolated assets but are **not authored strongly enough for the constrained played camera/mobile screen-space**.

The next source-authoring technique must therefore be camera-first: functional identity has to be encoded into large silhouette/massing visible from the canonical southeast orthographic view and mobile crop before the source is accepted. It should not rely on later placement nudges, tiny props, banners or material contrast to explain the function.

## Final answer

**¿LA LECTURA FUNCIONAL DE GRANERO Y CUARTEL ESTÁ RESUELTA EN PANTALLA REAL? — NO.**

The block is closed deliberately here. The next step is not another layout pass; it is a different camera-first source-authoring technique.
