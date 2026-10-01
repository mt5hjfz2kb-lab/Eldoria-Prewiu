# Valoria Modular Architecture Composition Proof v1 — final experimental proof

Closed: 2026-10-01.  
Branch: `visual-proof/modular-architecture-composition-v1`  
Final visual code: `be43ad8b35e9eaffd3b3af98a4a84efad0a91799`  
Final run: **36835900386 — SUCCESS**  
Artifact: **11148933433**

Verdict: **TECH PASS / MODULAR COMPOSITION PASS / NOT PROMOTED TO PRODUCTION**.

## Question answered

Can existing Eldoria/Valoria geometry be treated as a construction vocabulary and combined into a coherent mid-tier architectural mass at the real gameplay camera, without generating a new monolithic building?

**Yes.**

Alternative A proves the technique at the real front Cuartel parcel. Alternatives B and C are useful negative controls: technically valid and coherent enough to assemble, but visually weaker than A.

No Tripo generation occurred and no credits were spent.

## Baseline preserved

This proof starts from the validated Hero District Integration v1 state.

Preserved:
- Hero Bastion optimized SHA-256 `afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`;
- Hero District composition;
- certified physical central stair;
- gameplay routes;
- Cuartel gameplay target/collider;
- all hotspots and gameplay colliders.

The current Cuartel visual shell is hidden only for each experimental alternative. Its gameplay target remains untouched.

## Real inventory audit

The runtime/repository inventory contains substantially more architectural vocabulary than earlier passes were using.

Certified / promoted Valoria families available in this branch:
- `TowerWallRock`;
- `RockTerrainSeamFiller`;
- `ResidentialTerraceRock` (available, but previously demonstrated unreliable material response in the Mid/Lower proof);
- Stone Architecture v1:
  - `HighStraightWall`;
  - `CornerWallL`;
  - `RockToWallTransition`;
- Terrain & Terrace v1:
  - `BroadRockPlatform`;
  - `SteppedRockTerrace`;
- Stone Kit pieces;
- Valoria `Stone_Gate`, `Stone_Tower`, `Stone_Wall`;
- dedicated Aserradero, Cuartel and Granero assets.

Existing project inventory additionally includes:
- `Arch_Gothic`, `Column_Round`, `MegaGate`, `MegaTower`, `MegaWall`, `Wall_Broken`, `Rock01`, `Rock02`;
- Slavic World complete/support architecture;
- raw Slavic wall/front modules;
- roof variants;
- balcony;
- porches;
- village platform and stair;
- gates/fences;
- doors/shutters;
- props/firewood;
- vegetation and rocks;
- Ground Kit and Urban Props.

Historical family names `TerraceStairRock`, `GateStreetRiseRock MV1` and `StreetLandingTransition` were not found as runtime assets in the authoritative branch tree, so they were not assumed to exist or used in this proof.

## Parcel

Final laboratory parcel: **front central Cuartel visual parcel**.

Anchor:
- `(7.20, 0.42, -4.00)`

The current dedicated Cuartel visual is visually prominent at zoom 12/9/mobile and therefore provides a stronger test than the first attempted west-rebuilders parcel.

Only one Cuartel visual renderer family is suppressed. The separate invisible gameplay target/collider is untouched.

## Alternatives

### A — rock-integrated guardhouse — PASS

Construction vocabulary:
- buried `SteppedRockTerrace`;
- Slavic occupied house core;
- `HighStraightWall` masonry spine;
- `RockToWallTransition`;
- Slavic balcony/gallery;
- restrained local warm light.

Result:
- reads as one designed secondary guardhouse rather than a pile of unrelated modules;
- lower masonry, rock seating, inhabited timber/roof volume and gallery form a coherent hierarchy;
- its footprint remains subordinate to the Hero Bastion;
- survives zoom 9 and mobile;
- substantially improves architecture-to-ground integration compared with the isolated current Cuartel shell.

The key discovery is that a stable complete occupied core can supply the roof/body while certified rock/stone modules reshape its base, silhouette and urban connection. Separate raw roof modules are not required.

### B — civic/military terrace — PARTIAL

Construction vocabulary:
- `BroadRockPlatform`;
- two `CornerWallL`;
- Slavic occupied core;
- Slavic rock gate;
- Slavic porch;
- local warmth.

It integrates with the parcel but reads too low and horizontally compressed. Useful vocabulary, not the selected pattern.

### C — fortified secondary guardhouse — FAIL as architecture candidate

Construction vocabulary:
- `BroadRockPlatform`;
- `TowerWallRock`;
- Slavic occupied core;
- `HighStraightWall`;
- `RockToWallTransition`;
- Slavic rock gate;
- local warmth.

The tower/gate language dominates and the result reads closer to a fortified entrance than a secondary inhabited building. Technically valid but wrong hierarchy/function for this parcel.

## Rejected sub-experiment

The first valid Cuartel run tested raw standalone Slavic roof modules on A/B/C. They made the assemblies read as collage/prototype:
- A received a beige slab-like roof mass;
- B became over-roofed;
- C produced the weakest recycled-module read.

Those roof modules were removed in the final visual code rather than hidden with more density. The existing occupied core roof is the better architectural crown for this camera.

## Unity environment-art findings

This proof uses Unity as more than a model viewer:
- controlled module burial and overlap;
- composition from multiple prefabs/meshes;
- runtime bounds fitting to a real parcel;
- URP/Lit material adaptation while retaining source base/normal textures where available;
- shared material cache;
- GPU instancing enabled on adapted materials;
- MaterialPropertyBlock variation;
- visual-only local lighting;
- deterministic editor composition/gate tooling;
- explicit collider/hotspot stripping from every experimental root.

Terrain, decals, baked lightmaps, reflection probes and LODGroups were considered but were not needed to answer this parcel-level architectural hypothesis. Adding them would not have fixed the main composition question.

## Technical evidence

Final run **36835900386 — SUCCESS**.

- `collider_hotspot_signature_equal=true`
- `hero_district_preserved=true`
- alternatives captured: **3**
- parcel visual renderer families hidden: **1**
- Tripo credits: **0**

Hero bounds remain:
- centre `(0, 7.14, 8.75)`
- size `(12.8, 9.239, 10.962)`

Metrics:

| State | Triangles | Renderers | Materials | Lights |
| --- | ---: | ---: | ---: | ---: |
| Current Hero District baseline | 1,604,280 | 1,207 | 628 | 24 |
| Alternative A | 1,590,939 | 1,236 | 652 | 25 |
| Alternative B | 1,598,689 | 1,239 | 656 | 25 |
| Alternative C | 1,646,072 | 1,237 | 656 | 25 |

Alternative A achieves the best visual result while also using **13,341 fewer triangles than the current baseline**. Its gain is therefore not a brute-force triangle-density effect.

## Decision

**TECH PASS**

**MODULAR COMPOSITION PASS**

### Conclusion A

**We can build mid-tier Valoria architecture with the existing kit.**

The missing capability was not proven to be new geometry. The reusable recipe is:

`stable inhabited core -> buried terrain/rock base -> retaining/masonry spine -> rock-to-wall seam -> one functional projection (gallery/porch/gate) -> shared material response -> restrained occupation light -> official-camera validation`.

Do **not** generate a new Mid-Tier Architecture Kit yet.

Next work should turn this proven recipe into a repeatable composition/prefab-variant system and apply it selectively to the lower/middle parcels, with per-parcel silhouette/function changes rather than cloning Alternative A unchanged.

No production promotion is performed by this proof.
