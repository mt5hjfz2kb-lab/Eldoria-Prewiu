# VALORIA SCREEN-SPACE BREAKPOINT v1 — RESULT

Status: CLOSED  
Final verdict: **TECH PASS / A SCREEN-SPACE VISUAL FAIL / B SCREEN-SPACE VISUAL PARTIAL / C SCREEN-SPACE VISUAL PARTIAL / NOT PROMOTED**  
Date: 2026-10-04

## Objective

Test whether the dominant remaining Valoria problem was insufficient screen-space scale rather than insufficient Granero/Cuartel source quality.

This block used the strongest retained source pair from the immediately previous proof:
- Granero: `Valoria_Granero_RCFv1` — SOURCE VISUAL PASS
- Cuartel: `Valoria_Cuartel_CFSv1` — SOURCE VISUAL PASS

No new assets, materials, Final Look changes, perspective, density, Hero Bastion changes or defense changes were allowed.

## Authoritative evidence

Run: **37211095096 SUCCESS**  
Artifact: **11307155373**  
Gameplay-focused tests: **PASS**  
Tripo credits: **0**  
Paid credits: **0**

Captures:
- `A-zoom9.png`
- `A-mobile.png`
- `B-zoom9.png`
- `B-mobile.png`
- `C-zoom9.png`
- `C-mobile.png`

## Variants

### A — current canonical camera/framing baseline for the retained pair
- zoom9 orthographic size: 9.0
- mobile orthographic size: 9.4
- no spatial adjustment

### B — tighter orthographic framing only
- same camera direction
- zoom9 orthographic size: 7.4
- mobile orthographic size: 8.0
- no source/layout/material change

### C — B plus bounded immediate presentation-space cleanup
- same camera direction and B framing
- Granero: +0.45 X / +0.30 Z, -6° yaw
- Cuartel: -0.80 X / +0.55 Z, +8° yaw
- no parcel redesign, macrocomposition move or gameplay change

## Technical result

**TECH PASS**

Preserved:
- orthographic camera model and direction
- RESET Final Look
- Hero Bastion source
- accepted defense
- terrain
- materials
- gameplay collider/hotspot signature
- focused gameplay tests

No new production asset was created and no paid/generative credit was spent.

## Screen-space measurements

The projected city-core width below is intentionally *unclipped*. Values over 100% mean the projected city footprint extends beyond the viewport and therefore diagnose crop pressure rather than visible occupancy.

### A
Zoom9:
- projected city-core width: 134.7% of viewport
- Granero: 242.8 × 142.5 px
- Cuartel: 240.2 × 139.6 px
- Bastion: 662.5 × 612.7 px

Mobile:
- projected city-core width: 496.3% of viewport
- Granero: 272.5 × 159.9 px
- Cuartel: 269.6 × 156.7 px
- Bastion: 743.6 × 687.7 px

### B
Zoom9:
- projected city-core width: 163.9%
- Granero: 295.3 × 173.3 px
- Cuartel: 292.1 × 169.8 px
- Bastion: 805.8 × 745.2 px

Mobile:
- projected city-core width: 583.2%
- Granero: 320.2 × 187.9 px
- Cuartel: 316.8 × 184.1 px
- Bastion: 873.7 × 808.0 px

### C
Zoom9:
- projected city-core width: 163.9%
- Granero: 308.6 × 180.3 px
- Cuartel: 293.4 × 174.9 px
- Bastion: 805.8 × 745.2 px

Mobile:
- projected city-core width: 583.2%
- Granero: 334.7 × 195.5 px
- Cuartel: 318.2 × 189.7 px
- Bastion: 873.7 × 808.0 px

## Visual verdict by variant

### A — SCREEN-SPACE VISUAL FAIL
The wide 16:9 frame still carries too much exterior context and the city retains some maquette-on-terrain reading. Granero and Cuartel are present but do not become two immediate semantic reads.

On portrait mobile, however, the important finding is the opposite of the original hypothesis: the city is not too small. The Bastion already dominates most of the frame and the lower functional city is heavily constrained by portrait crop.

### B — SCREEN-SPACE VISUAL PARTIAL
At zoom9 there is a **clear frame-level improvement**:
- less empty exterior;
- stronger city scale;
- less maquette feeling;
- architecture is materially larger in useful pixels;
- the frame moves closer to the premium 4X target.

Granero and Cuartel gain roughly 20–22% linear pixel size versus A at zoom9.

But this does **not** convert them into two unmistakable simultaneous functions. More importantly, mobile becomes worse: the tighter crop makes Hero Bastion almost fill the portrait frame and reduces useful access to the lower functional city.

Therefore B is not promotable as one global canonical framing.

### C — SCREEN-SPACE VISUAL PARTIAL
C preserves the B frame-level improvement and gives only a small additional local gain from orientation/placement.

It still does not solve the semantic Granero/Cuartel read and it does not solve mobile. The mobile crop remains the decisive failure.

Per the anti-loop rule, no D/E/F is permitted.

## Answers to the required questions

### 1. ¿El problema principal era que Valoria ocupaba demasiado poco espacio en pantalla?
**EN PARTE, pero no como causa principal global.**

For 16:9 zoom9, Valoria was indeed being shown too loosely and the tighter frame is visibly better.

For portrait mobile, the problem is not insufficient scale. The projected city already exceeds the viewport by a very large margin and the Bastion already dominates the frame. The real mobile issue is **aspect-ratio/crop allocation and home-pose composition**.

### 2. ¿B o C hacen que Granero y Cuartel se lean claramente mejor sin crear nuevos assets?
**MEJORAN SU TAMAÑO APARENTE, PERO NO LO SUFICIENTE PARA UN PASS FUNCIONAL.**

They are larger and somewhat easier to inspect at zoom9, but still do not become two immediate independent semantic reads. On mobile the tighter framing is counterproductive because the lower functional city is cropped harder.

### 3. ¿La ciudad empieza a parecer más cercana a la referencia?
**SÍ en zoom9 16:9.**

B/C reduce exterior dead space and strengthen capital-scale presence. That is a real visual improvement.

**NO como solución completa**, because mobile composition deteriorates and the functional-building readability criterion remains unresolved.

### 4. ¿Debemos cambiar el framing canónico?
**NO a este framing más cerrado como regla global.**

Retain the canonical orthographic camera and current global framing for now.

The next valid structural change is not another tighter zoom and not another building variant. It is an **aspect-ratio-aware framing/home-pose policy**, especially for portrait mobile:
- independent mobile orthographic size and target/home pose;
- explicit allocation of screen space to Hero Bastion *and* the functional lower city;
- no assumption that one 16:9 framing parameter can serve portrait mobile;
- re-use the existing SOURCE VISUAL PASS Granero/Cuartel pair for that proof.

## Canonical decision

- No B/C global framing promotion.
- Orthographic camera remains canonical.
- Existing global framing remains canonical pending a dedicated aspect-ratio-aware/mobile framing proof.
- Granero/Cuartel are **not** to be rebuilt again.
- Forja/Hospital/Cantera rollout remains HOLD.
- No D/E/F screen-space variant.
- Next structural recommendation: **responsive/aspect-aware home framing and crop policy**, not more source authoring.

## Final answer

**¿EL GRAN PROBLEMA DE VALORIA ERA EN PARTE QUE LA ESTÁBAMOS MOSTRANDO DEMASIADO PEQUEÑA EN PANTALLA? — SÍ, EN PARTE EN 16:9; PERO NO ERA EL BLOQUEO PRINCIPAL EN MOBILE.**

The tighter 16:9 frame is visibly better and proves that screen-space scale contributes to the maquette feeling. The decisive mobile failure is different: portrait framing allocates too much of the useful viewport to Hero Bastion and crops the lower functional city. Solving that now requires an aspect-ratio-aware/mobile composition policy, not another asset pass.
