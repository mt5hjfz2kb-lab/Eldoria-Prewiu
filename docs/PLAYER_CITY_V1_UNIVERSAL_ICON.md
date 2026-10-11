# Eldoria — Player City v1 universal world icon

Status: LOCKED FOR v1  
Date: 2026-09-30

## Decision

For the first shared-world implementation, **all player cities use one universal strategic city mesh** on the 4X map.

Canonical production resource:

`Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb`

The selected source is recovered Player City progression **piece 05** from run **36732048587 / artifact 11105793297**.

Exact promoted identity:
- source piece: `piece_05_22797tris.glb`
- triangles: 22,797
- bytes: 11,177,448
- SHA-256: `e97319398ef1294348d8f59fb747628c6f077ce16dfe6f1cfd5189e108ba0ada`
- promotion run: **36736693923 SUCCESS**
- extra Tripo spend for selection/promotion/integration: **0 credits**

## Runtime contract

The world-city mesh is a **visual representation only**.

Player-specific information remains outside the mesh:
- player name;
- alliance;
- ownership / own / allied / hostile state;
- level / power;
- shield/protection state;
- future skins and cosmetic variants.

The mesh does not own gameplay state, hotspots, player identity or collision.

## v1 scope

Do **not** require multiple city tiers for initial shared-world production.

The previous six-stage progression-sheet objective is superseded for v1 by this simpler contract:
- one production city silhouette;
- reused for every player;
- differentiated by UI/banner/state;
- future skins or progression tiers may be added later without changing the world gameplay contract.

## Future expansion

Possible later layers:
1. banner / owner color variation;
2. cosmetic city skins;
3. event/season skins;
4. a small number of development silhouettes if gameplay later justifies them.

These are future art layers, not blockers for the shared world.

## Acceptance

Player City v1 must:
- read immediately as a player settlement at 18/14/10/7 strategic zooms;
- remain legible on mobile;
- replace the old MegaTower/MegaGate/Shed placeholder composition;
- remain visual-only;
- preserve authoritative world gameplay/collision signatures.
