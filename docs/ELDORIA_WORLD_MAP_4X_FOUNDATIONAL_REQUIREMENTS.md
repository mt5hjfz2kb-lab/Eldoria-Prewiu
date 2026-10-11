# ELDORIA WORLD MAP 4X — FOUNDATIONAL REQUIREMENTS

Status: PARKED / FUTURE CANONICAL INPUT
Date: 2026-10-05

## Purpose

Preserve the product intent for Eldoria's world map before implementation begins.

The world map is not a disposable frontier strip around Valoria. It must be conceived from the start as the first playable slice of a much larger persistent 4X world supporting many players.

## Core product intent

- Eldoria's world map is a large-scale 4X multiplayer space.
- Many players must be able to coexist visibly in the world.
- A player's Valoria must have a world-space representation / city icon that belongs to this larger system.
- The first implementation may be limited, but its architecture must scale toward the full world rather than be thrown away.

## First starting region

The first authored/playable region should function as a beginner/start province.

Requirements:

- large enough to contain many visible player-city icons;
- new players spawn at randomized valid positions rather than one fixed coordinate;
- spawn spacing must avoid immediate overcrowding while preserving the feeling of an inhabited shared world;
- enough room for meaningful march distances and local exploration;
- level 1 and level 2 gathering nodes for all core resource types intended for the early game;
- hunting animals / huntable world targets;
- early PvE enemies / corrupt threats;
- roads, terrain landmarks, biome cues and readable navigation;
- reserved space for later systems rather than filling every tile immediately.

The exact region dimensions, densities and spawn counts must be validated by prototype rather than guessed.

## 4X scale requirements to plan before art production

Before locking world-map terrain or content density, define and test:

1. world coordinate system and long-range scale;
2. region / sector / chunk partitioning;
3. player-city placement and randomized spawn rules;
4. minimum/target spacing between player cities;
5. node distribution, respawn and density rules;
6. resource-level geography and future progression bands;
7. PvE enemy and hunting-target distribution;
8. camera zoom/pan behavior on mobile;
9. march pathing, travel-time readability and world-space feedback;
10. streaming, culling and performance strategy for a large map;
11. fog/exploration or visibility rules if retained;
12. expansion into higher-level regions;
13. alliance territory and alliance structures;
14. strategic objectives, events and future world systems;
15. relationship between Valoria interior, the player's world city, marches, nodes/enemies and return.

## Architectural rule

The first world-map vertical slice must be a **window into the final 4X architecture**, not a bespoke small map that will later be replaced.

A reduced first slice is acceptable only if it uses the same durable spatial logic expected for the large world.

## Recommended future workstream

Before broad world-map asset production, open:

**ELDORIA WORLD MAP MASTER PLAN v1**

Expected sequence:

Valoria Bastion I visual slice
→ WORLD MAP MASTER PLAN v1
→ Starting Region full-scale greybox
→ camera / density / spawn / march validation
→ first playable 4X region
→ scale outward into the larger world.

## Starting-region content target

The first meaningful 4X region should eventually demonstrate, at minimum:

- multiple player-city icons visible across the map;
- randomized player starts;
- L1/L2 gathering nodes across all early resource classes;
- huntable animals;
- early enemy/PvE targets;
- a readable world route from Valoria into gathering/combat;
- outgoing and returning marches;
- enough empty/reserved geography that the world feels larger than the currently active content;
- an obvious path to additional regions and higher-level content.

## Product principle

Do not optimize the world-map design around the smallest prototype.

Design the scalable 4X system first, then expose only the portion needed for the first playable slice.

The map should eventually feel large, populated, strategic and persistent even though production will expand it incrementally.
