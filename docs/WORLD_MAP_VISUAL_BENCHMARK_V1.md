# Eldoria World Map — Visual Benchmark v1

Status: ACTIVE / acceptance gate
Updated: 2026-09-29

## Target

Eldoria's world map is a mobile 4X map with a fixed isometric/top-down camera direction and bounded zoom. It is not a third-person environment and it is not an asset-showcase scene.

The right-hand world-map panel in the owner's 2026-09-29 reference is a **readability/composition benchmark**, not a literal layout to copy.

## What must read at gameplay camera

At normal zoom the player must distinguish, without labels doing all the work:

1. major biome masses / territorial mood;
2. mountain and cliff barriers;
3. forest masses and clearings;
4. roads / march corridors;
5. coast / river / water when present;
6. resource nodes;
7. corruption/Breach territory;
8. settlements, forts and hero POIs;
9. armies / interactable nodes layered above the environment.

The map must read as one authored world, not as isolated prefabs on a board.

## Camera gate

Every world-map family must be judged in the real Frontier projection at:

- **far strategic**: orthographic size 18;
- **normal gameplay**: orthographic size 14;
- **near gameplay**: orthographic size 10;
- **maximum-detail review**: orthographic size 7;
- **mobile portrait**: 390x844 at normal gameplay zoom.

Camera direction remains fixed. Assets do not pass because they look good from a bespoke beauty angle.

## Acceptance rules

A family is PASS only if:

- silhouette survives at size 18 and 14;
- repeated instances do not expose obvious copy/paste rhythm;
- terrain and vegetation form masses, not scattered ornaments;
- routes remain readable but do not look like board-game strips;
- POIs remain visually dominant over dressing;
- environmental detail does not compete with armies/resource icons;
- material language can be brought into Eldoria's dark epic-fantasy palette;
- visual-only dressing owns no gameplay hotspot/collider;
- mobile frame remains legible;
- the family has a viable repetition/LOD strategy.

Reject if any of these are true:

- foliage becomes neon, plastic or obviously from another asset pack;
- geometry only looks acceptable at close range;
- a pack requires dense high-cost placement to form a convincing biome;
- modular seams become visible at the normal 4X camera;
- the result resembles a low-poly diorama rather than a continuous territory;
- the environment overwhelms interactable gameplay information.

## Asset-family strategy

### Tier A — geographic base
Terrain, mountains, cliffs, coasts, rivers, large rock masses.
These define the world silhouette and get the strictest coherence gate.

### Tier B — repeatable dressing
Trees, forest clusters, shrubs, boulders, stumps, fences, road edges, debris.
These must be cheap to repeat and visually subordinate.

### Tier C — hero POI
Cities, forts, Breach structures, shrines, alliance structures, elite camps, bosses.
These carry Eldoria identity and may justify bespoke/Tripo geometry.

## Current evidence

- Slavic foliage: **REJECTED** as a primary forest language. Gate 36590669809 / artifact 11044780827.
- Slavic hard-surface subset: **conditional only** for low-salience rocks, roads, fences and props after camera validation.
- NatureStarterKit2: already present in repo; must be tested under this 4X gate before replacement.
- External free candidates should first enter an isolated comparison against NatureStarterKit2 and current Frontier. Do not promote from store screenshots alone.

## Immediate comparison set

1. current Frontier baseline;
2. NatureStarterKit2 already in repo;
3. Holotna Mountain — Stylized Fantasy Environment;
4. Jermesa Hill Rock Mountain Terrain.

Quaternius ruins may be tested later for Tier C secondary POIs; its nature packs are not the first-choice world-language candidate because their Ghibli/low-poly stylization may remain too visible at Eldoria's target look.

No paid asset purchase is authorized by this benchmark.
