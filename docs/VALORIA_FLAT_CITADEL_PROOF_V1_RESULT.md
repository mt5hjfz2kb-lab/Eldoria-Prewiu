# VALORIA FLAT CITADEL PROOF v1

Status: **CLOSED — TECH PASS / VISUAL FAIL / COMPOSITION DIRECTION ACCEPTED FOR NEXT BASE**

Branch: `visual-proof/valoria-flat-citadel-proof-v1`

Final proof HEAD: `ace7975cdcf2bdc008d2435cfd221fad575e7dae`

Final workflow run: **37134976736**

Final artifact: **11278791152** (`valoria-flat-citadel-proof-v1-37134976736`)

## Hypothesis tested

Replace the mountain-first composition with a game-first macro layout:

- roughly 75–85% of the useful city footprint is one coherent flat / very-gentle buildable plane;
- Hero Bastion is the only major elevation, placed on a moderate civic/defensive rise;
- clear gate → road → plaza → Bastion axis;
- sparse, separated functional buildings with readable parcels;
- continuous outer wall;
- explicit unused growth space;
- reduced background and no mountain/world-frame dependency.

The owner-approved reference image was used as a compositional target only, not as a literal building-copy target.

## Execution summary

The first successful flat-citadel run (**37134724405**, artifact **11277737407**) proved the macro layout but still read as a raised rectangular/podium-like city because the playable footprint ended at the wall.

A second large-block iteration replaced that presentation with:

- a surrounding meadow/natural floor beyond the wall;
- a near-zero-height city surface rather than a visible terrain podium;
- a lower, thinner outer wall;
- the canonical Hero Bastion visual family restored as the dominant landmark;
- the same sparse four-building functional layout;
- the same single controlled Bastion rise;
- no world-frame mountain shell.

The final real capture set is contained in artifact **11278791152**:

- `after-19.png`
- `after-12.png`
- `after-9.png`
- `after-mobile.png`
- matching BEFORE captures
- `evidence.json`
- Unity gate log

## Technical evidence

Final `evidence.json`:

- collider/hotspot signature equal: **true**
- hidden legacy renderers: **832**
- functional buildings: **4**
- outer wall pieces: **32**
- nature pieces: **10**
- flat useful-area target: **0.82**
- major elevations: **1**
- background strategy: **camera-contained / no mountain world-frame**
- Tripo credits: **0**

### TECH verdict

**TECH PASS**

The proof compiles and renders in Unity 6000.3.23f1, produces deterministic 19 / 12 / 9 / mobile evidence, preserves the canonical gameplay collider/hotspot signature, and uses no paid generation.

## Visual review against the new reference

### What improved decisively

1. **City readability is much stronger.** The frame now reads immediately as a strategy/city-building space rather than a city embedded in a geological object.
2. **The Bastion can dominate without a giant mountain.** A single modest rise is enough to produce a strong hierarchy.
3. **The city floor is coherent.** The lower settlement is predominantly one buildable surface rather than a chain of terraces/platforms.
4. **The wall creates a clear playable footprint.**
5. **There is visible growth headroom.** The scene no longer starts as a fully packed metropolis.
6. **The main composition survives 19 / 12 / 9 / mobile.**
7. **The previous mountain-family failure mode is gone.** No giant island, wedding-cake terrain, scan plates or stacked cliff bands are required.

### Remaining visual failures

1. The outer wall is still prototype-grade and mechanically repetitive.
2. Secondary architecture is not yet unified enough with the Hero Bastion in material/detail quality.
3. The ground treatment is too uniform and lacks authored road-edge / parcel / vegetation transitions.
4. The background is intentionally reduced but currently too empty/neutral; it needs a restrained natural surround, not a return to panoramic scenery.
5. The final frame still lacks the polish, life, material richness and UI integration required by `docs/ELDORIA_VISUAL_BENCHMARK.md`.
6. Some building placement and scale relationships remain blockout-like even though the macro composition is substantially better.
7. The current proof is visually cleaner than the mountain approach, but it is not yet a production-quality city-builder frame comparable to the approved reference.

### VISUAL verdict

**VISUAL FAIL**

This is not a declaration that the flat-citadel hypothesis failed. The **execution is still prototype-grade**, so the proof cannot be promoted as finished visual production.

## Decision

**Adopt the flat-citadel composition as the new Valoria visual base direction. Do not promote the current proof implementation as final art.**

The critical distinction is:

- **composition hypothesis: PASS**
- **technical proof: PASS**
- **current visual execution: FAIL**

Future work should build on the flat-city macro layout and improve architectural coherence, wall quality, parcel/road surfacing, vegetation/life and final framing. It should not reopen the mountain-rescue family unless new evidence invalidates this result.

## Closed methods / stop rules

Do not return to:

- one giant mountain or one central terrain island;
- district-by-district terraces;
- open-sheet terrain;
- concentric/contour wedding-cake landforms;
- scan plates / photographic foreground;
- voxel or fused improvised terrain;
- density added to hide composition problems;
- micro-adjustment loops before a large macro defect is addressed.

## Recommended next block

A production-oriented **FLAT CITADEL BASE v1** should retain this macro composition while replacing proof-grade elements in large coherent blocks:

1. canonical/modular outer wall family;
2. authored flat ground + road/plaza/parcel surface language;
3. Hero Bastion seating and approach;
4. coherent Aserradero / Cuartel / Granero presentation;
5. restrained natural perimeter and atmosphere;
6. final UI/full-frame integration.

Every block must continue to validate 19 / 12 / 9 / mobile and preserve gameplay authority.
