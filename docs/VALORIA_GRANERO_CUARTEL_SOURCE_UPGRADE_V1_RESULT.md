# VALORIA GRANERO + CUARTEL SOURCE UPGRADE v1 — RESULT

Status: CLOSED  
Final verdict: **TECH PASS / SOURCE VISUAL PASS (BOTH) / UNITY VISUAL PARTIAL / NOT PROMOTED**  
Date: 2026-10-04

## Scope

This block changed only the artistic 3D sources for Granero and Cuartel. It did not reopen Production Art System Reset v1 or First Production District v1, and did not alter Forja, Hospital, Cantera, terrain, camera, Final Look or accepted defense.

## Sources chosen

### Granero
Canonical dedicated source:
- `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb`

Reauthored/normalized output:
- `Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/Valoria_Granero_GCSUv1.glb`
- 49,800 triangles
- output SHA-256: `565d282dc84d1244a212b43d310332fc89a2766b6adc742bf92bcccd6695b977`

Why selected:
- dominant storage hall mass
- strong roof presence
- deep loading access
- annexes / secondary volume
- visible agricultural-storage identity that survives neutral isolated review

### Cuartel
Sources:
- dedicated `Valoria_Cuartel_AP2_v1.glb`
- accepted rich defensive wall `Valoria_DefenseWall_FPDv1.glb`
- accepted rich defensive tower `Valoria_DefenseTower_FPDv1.glb`

Reauthored compound output:
- `Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/Valoria_Cuartel_GCSUv1.glb`
- 120,015 triangles
- output SHA-256: `5e25e0258d228e6b424f779cf29b34021c992ae0c0d93f092acf7f8107be35fd`

Why selected:
- dedicated barracks core remains the authored building
- bounded defensive modules create a guarded military compound
- fortified access / robust stone mass / guarded perimeter carry military identity
- banners remain supporting cues rather than the sole identity

## What was discarded

- MidTier donor recomposition from First Production District: rejected because it remained too generic and civil.
- AP2 Cuartel alone: rejected as the final upgraded source because isolated semantic read remained too close to a large civilian building.
- first Cuartel compound assembly: SOURCE VISUAL FAIL because inherited GLB transforms dispersed defensive pieces; fixed as a technical assembly defect, not accepted visually.
- Tripo: not used. No new generation and no credits spent.
- primitive-authored replacement: not used.
- recolor/material-only approach: not used.

## STOP GATE 1 — isolated source

Authoritative run: **37206352843 SUCCESS**  
Artifact: **11305265539**

Previews:
- `docs/evidence/valoria-granero-cuartel-source-upgrade-v1/granero-source-3q.png`
- `docs/evidence/valoria-granero-cuartel-source-upgrade-v1/cuartel-source-3q.png`

Verdicts:
- **Granero — SOURCE VISUAL PASS**
- **Cuartel — SOURCE VISUAL PASS**

The isolated Granero reads as agricultural storage through architecture itself. The corrected Cuartel reads as a guarded military compound through massing, entrance and defensive framing.

## STOP GATE 2 — Unity

Authoritative run: **37206779989 SUCCESS**  
Artifact: **11305635813**

Matched evidence:
- BEFORE zoom 9: `Unity/ValoriaGraneroCuartelSourceUpgradeV1Captures/before-9.png`
- AFTER zoom 9: `Unity/ValoriaGraneroCuartelSourceUpgradeV1Captures/after-9.png`
- BEFORE mobile: `Unity/ValoriaGraneroCuartelSourceUpgradeV1Captures/before-mobile.png`
- AFTER mobile: `Unity/ValoriaGraneroCuartelSourceUpgradeV1Captures/after-mobile.png`

Technical result:
- gameplay collider/hotspot signature preserved
- exactly 2 visual source replacements
- orthographic camera preserved
- accepted RESET Final Look preserved
- defense unchanged
- terrain unchanged
- focused gameplay tests PASS
- renderers: 852 -> 850
- materials: 81 -> 81
- triangles: 2,083,716 -> 2,166,395
- lights: 31 -> 31

## Unity visual verdict

**VISUAL PARTIAL / NOT PROMOTED.**

The source quality problem was genuinely improved: both isolated sources are substantially stronger than the FPD MidTier-derived Granero/Cuartel. However, the played-camera comparison does not satisfy the final promotion criterion.

At zoom 9:
- the replacement is real and visible;
- the richer architecture improves local form;
- but the two functions still do not separate strongly enough from the surrounding dense terracotta/civil family to read immediately and independently.

At mobile:
- the improvement is more diluted by crop, scale and surrounding density;
- one changed mass is visible, but both functions do not survive as unmistakable Granero + Cuartel identities simultaneously.

The remaining blocker is therefore **screen-space functional hierarchy/integration**, not lack of source geometry, Blender capability, materials, Final Look, camera or pipeline capability.

Per the anti-loop rule, this block stops here rather than starting v2/v3 micro-placement or scale iterations.

## Promotion decision

- Granero upgraded source: **certified at SOURCE level, not promoted to runtime**
- Cuartel upgraded source: **certified at SOURCE level, not promoted to runtime**
- First Production District accepted defense: unchanged
- existing playable Granero/Cuartel presentation remains canonical
- rollout remains HOLD for Forja/Hospital/Cantera

The source-upgrade runtime component remains gated/disabled by default. No player-facing promotion was made.

## Cost

- Tripo generations: **0**
- Tripo credits: **0**
- paid external assets: **0**

## Final answer

**¿GRANERO Y CUARTEL HAN DADO YA EL SALTO VISUAL QUE NECESITAMOS? — NO, todavía no en el juego real.**

The important progress is that the project now has two substantially stronger source assets and has proven that source quality was indeed part of the problem. But the required final result is not merely good isolated models: both must remain unmistakably functional and superior at zoom 9 and mobile inside Valoria. That condition is not yet met, so no rollout extension is authorized.
