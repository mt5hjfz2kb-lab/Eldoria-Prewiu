# Eldoria — WORLD REGION 1 v1

Status: **ACTIVE / 4X WORLD SCREEN CONTRACT LOCKED / HEAVY UNITY VALIDATION DEFERRED WHILE MOBILE WEBGL OWNS THE RUNNER**

Canonical workstream: `eldoria-world-region-1-v1`.

## Source-of-truth decision

This block extends the certified Valoria production slice; it does not replace it. The canonical Valoria scene, visual authority, camera, parcel/build progression, save/reload semantics and Apple SHARP production line remain protected.

The existing Unity architecture already contains the core semantics required for the first world loop: `PlayerState` carries stable world/player IDs, region resources and march state; `LocalGateway` owns idempotent `Gather`/`Fight` commands, outbound/gathering/returning phases, one-time reward crediting and persistence; the earlier `Frontier` presentation proves a separate world scene route existed. Therefore Region 1 must **reuse those semantics and reauthor the world presentation as a dedicated 4X screen**, not fork a second world/gameplay model.

## Reuse / replace matrix

| Surface | Decision |
|---|---|
| `LocalGateway`, `PlayerState`, `SliceRules`, march timing/reward/save | REUSE unchanged unless a demonstrated missing invariant requires a bounded extension |
| IDs `forest-valoria`, `quarry-valoria`, `corrupt-scout` | REUSE as first Region 1 gameplay entities |
| City ↔ World transition | NEW explicit UI transition: `Mundo` button from Valoria; `Reino`/Valoria selection to return. No physical walk through the city gate. |
| Historical `Frontier` procedural visuals | REPLACE as player-facing art; retain only useful topology/interaction knowledge |
| Dedicated regional 4X map | BUILD as the player-facing World screen; Region 1 is the first bounded sector, not a continent/global map |
| World camera | NEW bounded Region 1 controller, derived from Valoria's controlled-camera philosophy |
| Region data | NEW versioned `pipeline/world-region-1.json`, scalable to future regions |
| Narrative ruin | NEW non-combat POI `old-watch-ruin`; it teaches orientation/lore without inventing a second progression system |

## Minimal validated sector

Before expanding density, the first sector is only:

**Valoria icon/home position → road network → forest resource node → old watch ruin → corrupt scout → return to Valoria via `Reino`/city selection.**

The quarry may exist as secondary context, but it is not required to prove the first loop. Natural boundaries (rock shelves, forest walls, river/terrain falloff) constrain the camera and hide unbuilt territory. The World screen deliberately changes scale from the city: Valoria is represented as a compact, recognizable map-city landmark rather than the full city scene.

## Region pattern

A region is data + presentation + interaction adapters. Region data owns stable POI IDs, logical positions, route graph, visual salience, unlock rules and persistence bindings. Presentation owns terrain/vegetation/ruins/atmosphere. Interaction adapters translate taps into existing gateway commands. Save/gameplay remain outside presentation.

Future Region 2/3 must be addable by adding another region definition and scene/presentation package without rewriting Valoria or `LocalGateway` semantics.

## Camera/mobile contract

No unrestricted free camera. Pan is clamped to the region envelope; zoom is clamped to a narrow useful range; tap targets remain independent colliders/proxies; POIs use silhouette/spacing/feedback rather than HUD overlays to stay readable. Performance density concentrates around Valoria's map representation, routes, ruin, resource nodes and threats.

## Gameplay proof required

A valid proof must demonstrate through real UI/input:

1. Start in certified Valoria.
2. Tap the explicit `Mundo` button.
3. Transition to the dedicated Region 1 4X World screen.
4. Select `forest-valoria` or the route threat.
5. Start a real march through `LocalGateway`.
6. Complete interaction.
7. Return and credit the reward exactly once.
8. Reload during/after the march without duplication.
9. Tap `Reino` or select Valoria on the map, return to the city screen and observe the changed wallet/progression.
10. Repeat command IDs remain idempotent.

The existing `corrupt-scout` is explicitly the minimum encounter; this block does **not** claim a new full combat system.

## Visual direction

Region 1 uses the same dark adult medieval art direction as Valoria but at deliberate 4X-map scale: recognizable Valoria silhouette, authored roads, forests, relief, ruins, resource landmarks, sparse warm human traces, localized corruption and atmospheric depth. Premium density is reserved for Valoria's map landmark, route junctions, ruin, resource nodes and threats. Lower-density terrain and vegetation carry the spaces between them. The visual gate fails if the screen looks like a generic prototype or like a different game, but it does not require city-level asset density across the whole map.

No broad visual R&D, no Tripo, no paid credits, no continent generation.

## Gates

Closure requires independent evidence for TECH, VISUAL, WORLD/VALORIA CONTINUITY, NAVIGATION, INTERACTION, GAMEPLAY LOOP, SAVE/RELOAD, BOUNDED CAMERA and MOBILE READABILITY. `WORLD/VALORIA CONTINUITY` means coherent Eldoria art direction, UI language and persistent state across the deliberate scene/scale transition—not a seamless physical landscape. Green CI alone is insufficient.

## Concurrency note

At claim time `valoria-mobile-web-playtest-v1` owns the Windows Unity runner and GitHub Pages surfaces. Region 1 may prepare data/docs/code that does not dispatch or monopolize that resource. Heavy Unity generation/capture/build must wait until that workstream releases the runner, then the Region 1 claim may explicitly add the runner resource after re-reading the registry.
