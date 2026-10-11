# Valoria — StreetLandingTransition isolated module gate

Date: 2026-09-28.

## Scope

Validate the first **StreetLandingTransition** candidate through the canonical Tripo module pipeline without touching production `Valoria.unity`, `VisualWorld` or gameplay.

Functional role:
- street / landing / urban transition;
- visually connect neighboring module families;
- provide a short readable level change;
- retain usable attachment ends;
- avoid becoming a monumental gate, tower, keep or military enclosure.

## Certified source identity

Runner source:
`C:\Users\crist\Downloads\ancient ruin platform 3d model.glb`

The owner's expected export name was `Eldoria_Module_StreetLandingTransition.glb`, but the actual recent Tripo export retained the generic name above. Identity was therefore locked by exact filename + SHA-256 before Blender or Unity were run.

- source bytes: **7,954,612**
- source SHA-256: **3c6ef8f92879f5c8acee827b8cd8d2e4f33bb94fde0b6ad20a228cc04bd44b27**
- source triangles: **331,460**
- source vertices: **165,663**
- source material slots: **1**
- source UV0: absent
- source normals: present

The request explicitly excluded the recent ResidentialTerraceRock, GateStreetRiseRock MV1/V3/V2/V1 and TerraceStairRock hashes.

## Canonical Blender result

- optimized triangles: **49,800**
- optimized vertices (Blender): **24,832**
- optimized material slots: **1**
- UV0: generated / present on all meshes
- normals: present
- optimized bytes: **2,390,048**
- optimized SHA-256: **ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01**

## Unity isolated gate

GitHub Actions run: **36355160385**  
Artifact: **10944255594**  
Artifact name: `eldoria-tripo-module-street-landing-transition-v1-final-da75ec34bdfe5f3e4d8bf08a909f1ac3dd3d1cae`

Unity 6000.3.23f1 metrics:
- meshes: 1
- renderers: 1
- materials: 1
- colliders: 1
- triangles: 49,800
- UV0: present
- normals: present
- positive raycast: PASS
- empty-space miss: PASS
- all eight canonical captures non-empty: PASS
- review bounds: 8.644 × 5.516 × 18.0

Evidence includes strategic 19, city 12, detail 9, oblique, front, rear, left and right diagnostics.

## Visual review

### Strategic 19
The module reads as a compact elevated urban connector rather than a dominant building. Its longitudinal deck/landing is visible and can sit between larger families without becoming a skyline anchor.

### City 12
The usable horizontal surface and parapeted edges read clearly. The composition is more terrace/landing than monumental gate. Both longitudinal ends remain open enough to attach neighboring modules.

### Detail 9
The paved surface, short level change and parapet rhythm remain legible. The geometry is dense but does not turn into a tower, keep or enclosed defensive block.

### Oblique
This is the strongest functional view. The piece shows a genuine low-to-high urban transition across a short span and offers two distinct attachment ends. It can bridge small height differences between larger modules.

### Front diagnostic
The entrance face remains open and narrow. It does not read as a gatehouse. The connector function is preserved.

### Cardinal diagnostics
The side profile confirms a short stepped/graded vertical transition rather than a flat slab. It also shows that the long sides carry relatively heavy parapet/stone language.

## Verdict

**TECH PASS.**

**VISUAL / FUNCTIONAL PASS.**

StreetLandingTransition is certified as a **fifth distinct Valoria production family** because it fills the previously missing small urban connector / landing role rather than duplicating defense, residential mass, elevation mass or monumental gate/circulation.

Visual caution: the parapets and stone detailing lean slightly fortified. In composition this family should remain subordinate, used as a connector/landing between larger pieces rather than repeated as a defensive wall motif.

## Production decision

StreetLandingTransition is eligible for **Micro-Valoria 2 — inhabited district**.

Production `Valoria.unity`, `VisualWorld` and gameplay remain untouched.
