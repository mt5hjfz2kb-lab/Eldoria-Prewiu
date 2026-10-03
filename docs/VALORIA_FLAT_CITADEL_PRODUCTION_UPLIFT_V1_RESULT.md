# VALORIA FLAT CITADEL PRODUCTION UPLIFT v1

Status: **CLOSED — TECH PASS / VISUAL FAIL**

Branch: `visual-proof/valoria-flat-citadel-production-uplift-v1`

Accepted compositional source: **VALORIA FLAT CITADEL PROOF v1** — run **37134976736**, artifact **11278791152**, verdict **TECH PASS / VISUAL FAIL / composition direction accepted**.

Final implementation HEAD: `ca018faf089d10e777566974f2bc6fd3e470ede0`

Final implementation validation run: **37137847235**

Final artifact: **11279086781** (`valoria-flat-citadel-production-uplift-v1-37137847235`)

Toolchain planner run: **37135465408 — SUCCESS**

Tripo credits consumed: **0**

## Fixed macro direction

The macrocomposition remained fixed throughout this block:

- Hero Bastion is the only major elevation.
- Approximately 82% of useful city space remains flat / gently buildable.
- Continuous outer wall defines the city.
- Sparse functional buildings and reserved growth space remain.
- Background stays contained; the mountain-rescue family remains closed.

## Block 1 — production wall

The proof wall was replaced with the canonical Valoria stone family:

- `Valoria/Stone_Wall`
- `Valoria/Stone_Tower`
- `Valoria/Stone_Gate`

The implementation uses a strong gatehouse, asymmetric towers, curtain bays, mid-wall towers, corner towers and restrained Valoria heraldry.

**Result:** materially better silhouette and world coherence than the proof wall, but the wall/tower family still reads too heavy and repetitive at strategic zoom. This is an improvement, not final production quality.

## Block 2 — ground / roads / parcels

The main technical defect was found and corrected: several generated horizontal meshes were only visible with one triangle winding, so the intended city-ground / parcel hierarchy was not reliably visible from the official camera.

The uplift now writes stable UVs and double-sided presentation triangles for the flat-city top surfaces and parcel polygons.

Real result:

- buildable city ground is visibly distinct from the natural surround;
- roads/plaza read as a coherent circulation system;
- functional yards / growth areas have separate value families;
- the flat-city hypothesis remains intact.

This was the largest successful visual correction of the uplift.

## Block 3 — Hero Bastion → city integration

The Bastion is seated on one architectural terrace with a clear stair / landing connection to the city.

The Hero Bastion was lowered and slightly reduced inside the production envelope rather than adding new terrain mass.

**Result:** the access hierarchy is clearer and there is still only one major elevation. However, the lower rock mass that is intrinsic to the current Hero Bastion source remains visually dominant. It still reads more like a castle on a rocky pedestal than the cleaner stone terrace language of the approved reference.

## Block 4 — functional buildings

The production path now uses the canonical dedicated resources where available:

- `Valoria/Valoria_Aserradero_AP2_v1`
- `Valoria/Valoria_Cuartel_AP2_v1`
- `Valoria/Valoria_Granero_BIII_v1`

The buildings retain visual-only integration, no gameplay ownership, and receive restrained heraldry / material harmonization.

**Result:** stronger identity than the generic proof buildings, but material response, silhouette strength and relative scale are not yet uniformly at Hero Bastion quality.

## Block 5 — dressing / life

The accepted stack keeps restrained work props, district standards, warm occupancy cues and limited environmental dressing.

A later authored-nature/support-house experiment was capture-reviewed and found net-negative (mismatched vegetation silhouettes and weaker support architecture). It was explicitly reverted rather than accumulated.

## Final evidence

Final artifact **11279086781** contains:

- `before-19.png`
- `before-12.png`
- `before-9.png`
- `before-mobile.png`
- `after-19.png`
- `after-12.png`
- `after-9.png`
- `after-mobile.png`
- `evidence.json`
- Unity log

Final technical evidence remains:

- baseline gameplay signature preserved: **true**
- uplift gameplay signature preserved: **true**
- wall uplift: **enabled**
- ground uplift: **enabled**
- Bastion integration uplift: **enabled**
- functional-building uplift: **enabled**
- dressing uplift: **enabled**
- flat useful-area target: **0.82**
- major elevations: **1**
- Tripo credits: **0**

## TECH verdict

**TECH PASS**

Unity 6000.3.23f1 compiles and renders the complete stack, deterministic 19 / 12 / 9 / mobile evidence exists, gameplay collider/hotspot authority is preserved, and the block uses no paid generation.

## Visual comparison against Flat Citadel Proof v1

Clear improvements:

1. The buildable city surface is finally visible as an authored ground layer instead of reading as one uniform meadow.
2. Roads, central plaza and operational plots read more clearly.
3. The outer wall has a stronger defensive hierarchy than the proof wall.
4. Hero Bastion access is more explicit without reopening mountain terrain.
5. Functional districts have stronger identity and occupation cues.
6. The macro composition still reads as a scalable city-builder footprint with future growth room.

Remaining failures:

1. The wall/tower family still feels too massive and mechanically modular in the full frame.
2. Hero Bastion's intrinsic rock pedestal remains stronger than the desired architectural terrace transition.
3. Functional and support architecture still lacks one fully unified material/detail standard.
4. The natural surround remains visually sparse and lacks production-grade atmosphere/depth.
5. The proof/capture stack does not yet present the final gameplay HUD/UI layer, so the owner-facing frame still reads as an environment-art proof rather than a finished game screen.
6. Lighting, shadows, atmosphere and small-scale life are not yet rich enough to meet `docs/ELDORIA_VISUAL_BENCHMARK.md`.
7. At 19 / 12 / 9 / mobile the composition is much clearer, but the overall image still reads as a technically advanced prototype rather than the approved reference quality.

## VISUAL verdict

**VISUAL FAIL**

The uplift made real, measurable visual progress and validates the production value of the Flat Citadel base, but it does **not** satisfy the success criterion “the whole frame feels like a real production game rather than a technical prototype.”

No part of this verdict reopens the mountain composition.

## Decision

**Keep Flat Citadel as the canonical visual direction. Do not promote this experimental implementation as finished production art.**

The compositional decision is stronger after this block than before it:

`elevated Bastion + mostly-flat city + clear wall + separated functional plots + contained surround`

remains the correct basis for Valoria.

## Recommended next block

Proceed with **VALORIA FLAT CITADEL ART CONSOLIDATION v1**, not another macrocomposition experiment.

The next block should focus on:

1. a lighter, less repetitive canonical wall/tower presentation;
2. replacing / masking the visible Hero Bastion rock pedestal with an architectural retaining / stair interface while preserving the Hero asset identity;
3. one shared production material/value language across Aserradero / Cuartel / Granero / support architecture;
4. restrained natural perimeter + atmosphere that removes the empty-test-field feeling without becoming a panoramic landscape;
5. real final-game HUD/UI capture integration;
6. final lighting / shadow / fog / occupancy polish.

Do **not** return to the giant mountain, district-by-district terraces, scan plates, photographic foreground, voxel/fused terrain, or density used to hide composition defects.
