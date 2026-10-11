# VALORIA PLAYABLE DISTRICT v1 — ART PASS 1

Status: **CERTIFIED**

## Certified production checkpoint

- Playable/art code checkpoint: `26cec966b2495c0dc1d0f925ffd5aa6e7793f9a3`
- Unity gate run: `36441851841`
- Full Unity checks artifact: `10978669163`
- Official Valoria capture artifact: `10979357857`
- Official capture digest: `sha256:2211d652dbd3e76b0afc565c39028a4f9650a232f9300e33f4a43b34614c7721`
- Baseline skeleton code: `b971ba374b63bec0c1b65b33ea6e3f92f90ec09f`
- Baseline skeleton capture artifact: `10971070486`

The certified Playable District Skeleton topology, gameplay coordinates, official camera/orientation, official zoom envelope and Master Envelope reservations remain authoritative. This pass changes visual presentation around those fixed coordinates; it does not redesign the district topology.

## What changed

### Terrain and support

- The visible giant rectangular world-ground slab is no longer the rendered city support.
- The world collision/support volume remains present, while the authored view uses a continuous terrain sheet whose external edge is outside the official camera framing.
- The rectangular `VPD · continuous terrain`, L0 civic floor, plots and L1 floor/support volumes remain in place for topology/collision but no longer dictate the visible silhouette.
- Planta 0 and Planta 1 receive irregular terrain skins at their certified elevations.
- The former dark monolithic L1 support/plinth is visually replaced by rock clusters, low retaining masonry and terrain overlap.
- Master Envelope reserve objects remain at the same coordinates and retain their structural role, but their visible graybox rectangles are suppressed/integrated into low-contrast terrain skins.

### Street and stair

- The certified main-street corridor is unchanged.
- Its visible surface is rebuilt as overlapping worn-stone slabs rather than a single clean rectangle.
- The twelve certified stair treads remain individually visible and continuous.
- Small intermittent rock shoulders integrate the stair into the mountain without covering the route.

### Parcel integration and differentiation

- Aserradero and Cuartel remain on their certified plots and retain their original hotspot targets.
- Aserradero gains visible timber/workshop cues: timber stack and crane/scaffold language.
- Cuartel gains military cues: standard/banner and training markers.
- Bastion remains the Planta 1 landmark and retains its certified functional position/hotspot.

No Tripo credits were spent. No new external asset was generated. The final certified polish explicitly reuses existing project assets **SlavicCobbleRoad**, **SlavicFlatRock**, **SlavicBoulder** and **SlavicStoneFence** as visual-only dressing over the frozen functional surfaces. Their colliders are disabled; the certified gameplay floor/hotspot volumes remain authoritative. Procedural ValoriaKit terrain skins, pines, masonry, timber, banners and scaffold elements remain in use where appropriate.

## Plot envelopes recorded for the next architectural pass

### Aserradero

- Certified plot centre: `(-7.0, 0.31, -2.8)`
- Certified plot footprint: **5.1 wide × 5.2 deep**
- Current primary building footprint: approximately **3.4 × 2.8**
- Current click target: centre `(-6.55, 1.68, -3.75)`, size **3.75 × 2.25 × 1.15**
- Principal readable facade from the official camera: south/front side, biased toward the central street.
- Desired next-pass visual height: roughly **3.2–3.8 world units above L0**, while remaining subordinate to the Bastion.
- Keep the east/central-street edge of the plot visually open enough that the route and click target remain legible.
- Dedicated asset brief if replacement is required: isolated medieval mountain sawmill/workshop, asymmetric gable + timber lean-to/canopy, large readable timber storage/work frame, stone footing, no fused terrain, no road, no stairs, footprint capped to the certified plot.

### Cuartel

- Certified plot centre: `(7.0, 0.31, -4.0)`
- Certified plot footprint: **5.1 wide × 5.2 deep**
- Current primary building footprint: approximately **3.5 × 2.9**
- Current click target: centre `(7.55, 1.68, -5.05)`, size **3.85 × 2.30 × 1.15**
- Principal readable facade from the official camera: south/front side, biased toward the central street.
- Desired next-pass visual height: roughly **3.4–4.0 world units above L0**.
- Keep the west/central-street edge free of mass that could hide the route.
- Dedicated asset brief if replacement is required: isolated compact barracks/guardhouse, stone lower mass, asymmetric timber/slate upper silhouette, one small watch element or banner anchor, visible training/military language, no fused courtyard/terrain/road.

### Bastion

- Certified functional origin: around `(0, 3.0, 7.25)` on Planta 1.
- Certified central landing/support envelope: landing **7.0 × 4.2**, centre support **7.2 × 4.4**, with adjacent L1 parcels.
- Current click target: centre `(0.75, 5.0, 5.95)`, size **5.4 × 4.7 × 1.35**
- Principal facade: south/front face toward the official camera and stair.
- Desired next-pass skyline: approximately **6–7 world units above L1**, with the tallest mass kept central/rear so the stair mouth remains readable.
- Dedicated asset brief if replacement becomes necessary: isolated monumental fortress/keep for a mountain city, strong vertical central keep, asymmetric tower heights, coherent slate roofs with controlled overhang, stone integrated into a rock plinth but **no fused mountain/city/street/stair**, front gate/readable facade aligned to the certified stair axis.

## Interaction and technical gate

Run `36441851841` completed successfully.

- Source preflight: PASS
- EditMode: PASS
- PlayMode: PASS
- Windows desktop build: PASS
- Official Valoria benchmark render: PASS
- Aserradero hotspot/panel regression: PASS
- Cuartel hotspot/panel regression: PASS
- Bastion hotspot/panel regression: PASS
- Gate `gate` -> `Frontier`: explicitly covered by `ValoriaArtPassPreservesGateTravelAndOfficialZoomEnvelope`: PASS
- Official zoom clamps 9 / 19 and canonical 12 value: explicitly covered: PASS
- Master Envelope + camera pan + Bastion hotspot after pan: PASS

## Visual comparison against certified Skeleton v1

### Zoom 19 / establishing

Skeleton v1 exposed the edge and thickness of a very large rectangular ground board. The final Art Pass 1 capture removes the giant rectangular-board read and suppresses the Master Envelope proof slabs as visible plates while preserving their exact reserved objects underneath. The kernel now reads as an inhabited shelf within a wider valley/mountain field. The outer terrain is intentionally low-detail at this pass; it is no longer a topology-defining board.

The city remains intentionally sparse outside the kernel; this pass does not pretend the future Master Envelope is already built.

### Zoom 12 / gate

Skeleton v1 showed Planta 1 as a dark rectangular block supporting the Bastion. Art Pass 1 removes that dominant zócalo read. Rock, earth, low retaining fragments and authored Slavic rock pieces now carry the elevation transition. The foreground edge gains a broken rock band instead of one clean platform edge. The main street and twelve-step stair remain the strongest circulation line, with authored cobble overlays adding surface variation without changing the corridor.

Aserradero and Cuartel are more distinguishable by function without growing into the street.

### Zoom 9 / districts

At close gameplay distance, the greatest improvement is the support transition under the Bastion: the former continuous dark wall/plinth is gone and the upper level reads as a rock/earth terrace with authored rock/stone-face dressing. Plot borders are less like clean rectangles; the road gets real cobble dressing; and the stair stays unobscured. Aserradero and Cuartel retain distinct functional silhouettes without consuming the route.

The architecture itself is **not final production art**. The Bastion roof silhouette and both lower buildings still need a dedicated architecture pass; Art Pass 1 certifies terrain/support/circulation integration, not final building art.

## Gate verdict

- **TECH PASS**
- **INTERACTION PASS**
- **VISUAL PASS — ART PASS 1**

VISUAL PASS here means the stated Art Pass 1 criteria are met relative to the certified Skeleton v1: the rectangular platform read is substantially reduced, Planta 0 and Planta 1 read as one mountain-city support system, street/stair remain clear, vertical separation is preserved without a monolithic zócalo, and no new fused diorama is introduced.

It does **not** mean Valoria has reached the final benchmark.

## Next Art Pass

Priority for Art Pass 2:

1. dedicated architecture/silhouette pass for Aserradero and Cuartel inside the recorded plot envelopes;
2. Bastion silhouette pass: roof scale/logic, asymmetry, tower hierarchy, stronger rock/architecture junction;
3. increase inhabited urban density with small structures/retaining details that respect the frozen routes;
4. improve mountain depth with stronger controlled vertical rock masses and cliff faces; Art Pass 1 deliberately stops before turning the broad low-detail valley skins into final geology;
5. only after those forms work: materials, secondary vegetation, decals, VFX, lighting polish and optimization.

If a dedicated new asset is needed, use the parcel brief above, prepare the source image and dimensions first, and take the Tripo automation only to the pre-spend point until the owner explicitly authorizes credit use.
