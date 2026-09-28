# Valoria — Master Plan & Long-Term Progression Requirements

Status: **DIRECTION REQUIREMENT — ACTIVE**  
Date: 2026-09-28

This document records structural requirements that must be considered before Valoria's city layout is treated as production-ready. It is intentionally documentation-only and must not interrupt or overwrite the current isolated Unity construction/test block.

## 1. Long-term progression scope

- Bastion I–X is the current demo/prologue progression, **not the long-term ceiling of the city**.
- Valoria's master plan must support continued Bastion/city progression to at least the current planning range of approximately **level 25–35** without requiring a fundamental rebuild of terrain, circulation or city topology.
- Do not assume one unique full-city mesh/state per Bastion level. Prefer progression bands / visual eras with meaningful milestone changes and lighter intermediate upgrades.
- The exact final maximum Bastion level remains a product-design decision; the city plan must therefore preserve expansion headroom beyond the first ten levels.

## 2. One master city, many progression states

Valoria should be authored as one persistent spatial framework whose occupancy, architecture and presentation evolve over time.

The durable layer should contain, as appropriate:
- terrain and main elevation logic;
- primary circulation and major access routes;
- reserved plots / expansion zones;
- major walls, gates and district boundaries where structurally required;
- camera navigation envelope;
- future-use space for systems not present in Bastion I–X.

Progression layers may then change:
- building presence/absence;
- building visual tier;
- repairs vs ruins;
- wall completion and reinforcement;
- district density;
- decoration, vegetation, lights, props and population cues;
- construction scaffolding / transition states where useful;
- unlocking of additional urban areas.

Do **not** build a finished end-state city and simply hide arbitrary pieces without first proving that the underlying spatial plan supports every intended stage cleanly.

## 3. Suggested visual progression structure

The precise bands are not locked, but production should be able to support a structure similar to:

- Levels 1–5: recovery / damaged stronghold / sparse occupation
- Levels 6–10: stabilized early city / prologue-complete state
- Levels 11–15: first major urban expansion
- Levels 16–20: mature fortified settlement
- Levels 21–25: advanced regional capital
- Levels 26–30: high-status late-game city
- Levels 31–35: monumental / prestige state

These are planning bands, not a final balance commitment. Major visible transformations can happen at band boundaries, with lighter improvements inside each band.

## 4. Mobile city must be larger than one screen

Valoria must **not** be authored as a postcard where every useful building fits in a single mobile viewport.

The production target is a navigable city space:
- the city footprint exceeds one normal gameplay viewport;
- the player can pan/drag across the city on mobile, primarily left/right and with controlled secondary movement as composition requires;
- camera travel is bounded and authored, not free 360-degree exploration;
- the interaction should feel familiar to a player who already drags around the world map, while Valoria remains much smaller and more controlled than that map;
- the Bastion/core landmark should remain a strong orientation anchor from the official gameplay camera family;
- zoom remains controlled so Valoria does not collapse into another world-map view.

## 5. Expansion must be visible spatially

City progression should be readable not only through UI numbers and building skins, but through physical growth.

Examples of acceptable progression signals:
- previously ruined/unused ground becomes inhabited;
- a closed edge becomes a new district;
- walls extend or are rebuilt;
- upper terraces become occupied;
- new civic/military/residential clusters appear;
- visual density and life increase over time.

It is desirable that a player at a high Bastion level must move the camera farther to inspect the whole city than at the beginning, because the city has physically grown.

## 6. District and layout planning

Before production dressing, the master plan should identify at least:
- central Bastion/core zone;
- residential growth zones;
- military zone(s);
- production/economy zone(s);
- government/civic zone(s);
- walls/gates/defensive edges;
- vertical terraces and their connections;
- reserved future expansion zones;
- any future port/naval or other large-system reservation if retained by game design.

Buildings should not be scattered solely to fill composition. Their location should support readability, progression, clickability and functional grouping.

## 7. Camera & interaction constraints

Any master-plan approval must prove the following with the official mobile camera family:
- readable navigation while panning;
- reliable building click/tap targets;
- no essential building permanently hidden behind foreground mass;
- clear visual hierarchy between Bastion, important buildings and secondary structures;
- no progression state creates unreachable or visually confusing dead areas;
- camera bounds leave enough room for the intended long-term city footprint.

## 8. Structural decision gate

Before locking any hard-to-reverse Valoria decision — especially terrain dimensions, elevation bands, major streets, walls, district boundaries, camera limits or large reserved plots — explicitly ask:

> Does this still work when Valoria grows beyond Bastion X toward the planned level 25–35 range?

If the answer is unknown, treat the structural decision as provisional rather than final.

## 9. Relationship to current layered construction direction

This requirement is compatible with the current bottom-up Valoria direction:

**terrain/base → ground circulation → vertical connections → upper levels/plots → buildings → props/detail**.

The current layered/skeleton work can become the durable master framework **only if** it preserves enough area, camera travel and reserved topology for long-term progression.

The next design milestone after the current execution block should therefore include:

**Valoria Master Plan + Progression Map + Camera & Expansion Plan**

before treating building dressing or final urban density as locked production work.

## 10. Non-goals of this document

This document does not:
- change current gameplay balance;
- set the final Bastion level cap;
- require 35 unique Bastion meshes;
- require the current execution block to stop;
- approve final city dimensions before they are tested;
- replace the existing official-camera and layered-composition rules.

It exists to prevent the current demo scope (Bastion I–X) from accidentally hard-coding Valoria into a city that cannot evolve cleanly for the full game.
