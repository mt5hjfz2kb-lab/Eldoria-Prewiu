# Valoria Playable District Skeleton v1

Date: 2026-09-28

## Scope

This milestone replaces the failed diorama-fusion strategy with a bottom-up, fixed-camera playable district skeleton.

The certified Tripo families remain preserved as reusable art/reference material, but they no longer define city topology.

## Final topology

The playable district is built in functional layers:

1. continuous terrain/base;
2. Planta 0 civic floor;
3. Planta 0 main street;
4. lower west/east plots;
5. a physically continuous 12-step staircase from Planta 0 to Planta 1;
6. Planta 1 landing;
7. Planta 1 west/east plots;
8. retaining/support volumes below the upper floor;
9. buildings placed after circulation is solved.

The intended read is:

**entry -> street -> ascent -> upper level -> buildings**

No circulation segment depends on merging or cutting a certified Tripo diorama.

## Planta 0

- broad lower civic floor;
- central main street;
- lower entry apron;
- west economic plot: Aserradero;
- east military plot: Cuartel;
- enough screen-space separation for independent click targets from the official fixed camera.

## Vertical connection 0 -> 1

The connection is a dedicated staircase made from 12 physical treads.

It is independent of:
- GateStreetRiseRock MV1;
- StreetLandingTransition;
- TerraceStairRock;
- any fused rock/building asset.

This is the first Valoria route in the Unity slice whose topology is authored explicitly as floor + stair + upper floor rather than inferred from a diorama.

## Planta 1

- central landing at the top of the staircase;
- west and east upper plots;
- support/retaining volumes below the usable floor;
- Bastion occupies the principal upper plot;
- two smaller civil dwelling masses demonstrate remaining building capacity without filling every parcel.

## Official camera

Valoria continues to use the fixed orthographic game camera:

- position: (18.2, 14.6, -25.8)
- target: (0, 3.15, 5.8)
- player zoom range: 9..19

The benchmark capture now keeps that camera transform fixed and changes only orthographic zoom:

- strategic: 19
- city: 12
- detail: 9

Capture artifact:
- run: **36423010456**
- artifact: **10971070486**
- HEAD under review: **b971ba374b63bec0c1b65b33ea6e3f92f90ec09f**

Files:
- `valoria-establishing.png` = zoom 19
- `valoria-gate.png` = zoom 12
- `valoria-districts.png` = zoom 9

## Real interaction

The following world elements are real `WorldHotspot` interactions, not capture-only controls:

### Aserradero
Click opens the real building interaction panel.
If not yet built, the panel offers the existing `Build / sawmill` command.
If already active, it reports the building as active.

### Cuartel
Click opens the real building interaction panel.
If not yet built, the panel offers the existing `Build / barracks` command.
If already active, it reports the building as active.

### Bastión
Click opens the real building interaction panel.
When progression rules allow it, the panel uses the existing `AdvanceBastion / bastion` command.

### Exterior
The lower gate remains a real hotspot to load `Frontier`.

No new gameplay system was invented for this milestone.

## Interaction bug found and fixed

During PlayMode validation the building clicks initially failed even though geometry looked correct.

Root cause:
- Unity primitives were created at origin and then positioned/scaled procedurally;
- same-frame `Collider.bounds` and raycast queries still saw stale physics transforms;
- diagnostic ray dumps showed multiple colliders reporting bounds centred at `(0,0,0)`.

Fix:
- `Physics.SyncTransforms()` is executed after procedural world construction, before interaction queries.

After the fix:
- EditMode: PASS
- PlayMode: PASS
- Windows build: PASS
- benchmark capture: PASS

The PlayMode gate verifies:
- terrain/L0/stair/L1 objects exist;
- the official `Isometric camera` is used;
- Aserradero, Cuartel and Bastión each have reliable player click points;
- each click resolves the correct hotspot;
- each click opens the expected real building panel.

## Capture review

### Zoom 19
PASS for skeleton topology.
The whole district reads as one compact settlement rather than several isolated rock islands. The lower street axis and upper Bastion mass are distinct.

### Zoom 12
PASS for skeleton topology.
The lower buildings are separated, the central route is legible, and the staircase remains visible from entry to the upper platform.

### Zoom 9
PASS for skeleton topology.
The staircase is the dominant circulation cue. Planta 0 and Planta 1 remain visually distinct and the upper platform does not completely hide the lower street.

## Verdicts

**TECH PASS**

Evidence:
- source preflight PASS;
- EditMode PASS;
- PlayMode PASS;
- Windows desktop build PASS;
- capture generation PASS.

**INTERACTION PASS**

Evidence:
- Aserradero, Cuartel and Bastión resolve real world clicks;
- expected building panel opens for each;
- existing commands are retained;
- lower gate continues to lead to Frontier.

**VISUAL / URBAN PASS — SKELETON SCOPE ONLY**

The district now reads as a designed circulation hierarchy rather than a collage of fused dioramas:
- entrance;
- street;
- physical ascent;
- upper level;
- buildings.

This verdict does **not** certify final visual quality.

## Art reuse

Used now:
- existing `ValoriaKit.House` language for Aserradero, Cuartel and small upper dwellings;
- existing `ValoriaKit.BastionCore` for the upper hero landmark;
- existing terrain/rock/tree utilities for sparse framing.

Not used to dictate topology:
- TowerWallRock;
- TerraceStairRock;
- GateStreetRiseRock MV1;
- ResidentialTerraceRock;
- StreetLandingTransition;
- RockTerrainSeamFiller.

All six certified families remain preserved and can be harvested later for hero pieces, architecture, rock faces, facade sections or props when they fit the already-approved topology.

## What remains for Valoria Playable District v1 visual finish

Do not redesign topology unless a later gameplay requirement proves it necessary.

Next visual-production work should replace/refine the blockout in this order:

1. production terrain and retaining-rock treatment around the approved floors;
2. authored street/plaza surface over the existing circulation footprint;
3. production staircase/vertical transition preserving the exact readable route;
4. proper parcel edges/supports that do not read as rectangular slabs;
5. production building art for Aserradero/Cuartel/civil structures;
6. refine Bastion silhouette and roof/upper architecture while preserving click footprint;
7. selective reuse of certified Tripo-family fragments only where they fit;
8. materials, props, vegetation, lighting and atmosphere;
9. mobile/performance/LOD pass after visual composition is accepted.

The current major visual weaknesses are intentional blockout qualities:
- large rectangular retaining masses;
- simple flat parcel edges;
- sparse lower-district density;
- provisional roof/silhouette quality;
- terrain still reads as a broad test platform at zoom 19.

These are visual-production tasks, not topology blockers.
