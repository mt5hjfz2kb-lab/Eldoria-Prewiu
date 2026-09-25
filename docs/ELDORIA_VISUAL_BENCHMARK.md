# Eldoria — Visual Benchmark

Updated: 2026-09-25

## Status

This document defines the **official initial visual benchmark for Eldoria**. It is the art-direction target for the current Unity migration until a later Visual Bible explicitly replaces it.

The benchmark is based on the owner-approved reference image supplied on 2026-09-25. The image itself remains an external visual reference; this file preserves the durable art-direction contract in the repository so future work does not depend on chat history.

## Core direction

Eldoria must read as a **bright epic-fantasy 4X world with a visible dark/corrupted threat**, not as grimdark, generic low-poly fantasy, or a Unity blockout.

The target feeling is:
- monumental but inhabited;
- strategically readable but visually rich;
- bright and inviting in Valoria, with danger visible beyond it;
- dense enough to feel alive without obscuring gameplay;
- clearly part of a much larger world.

## Composition and camera

- Use an elevated strategic/isometric camera, but keep it close enough that buildings, people, props and activity remain legible.
- Build strong depth in layers: foreground settlement, primary city/bastion, surrounding landscape, distant world/horizon.
- Valoria/Bastion must be the primary focal point.
- Secondary composition should pull the eye toward the outer world and the corruption/Breach.
- Avoid flat board-like staging, exposed map edges, empty planes or isolated prefab islands.
- Use elevation changes, terraces, cliffs, bridges, stairs, retaining walls and natural terrain transitions to create scale.

## Environment density

- No large dead or white placeholder zones.
- Use vegetation, rocks, paths, props, smoke/fire, scaffolding, work areas, fences, carts, tents, resource piles and other signs of life.
- Density should feel authored, not randomly scattered.
- Roads and paths must connect gameplay spaces naturally and help guide the eye.

## Architecture

- Valoria should feel ancient, monumental and partly rebuilt after catastrophe.
- Architecture must appear habitable and functional, not just decorative ruins.
- Bastion, Sawmill, Barracks, Granary and future core buildings should have distinct silhouettes and be readable in-world.
- Prefer coherent stone/wood construction with restrained blue heraldic accents.
- Reuse of asset packs is acceptable only when the result feels like one world rather than a collage of unrelated packs.

## Palette and lighting

- Base palette: natural stone, warm wood, earth, vegetation and restrained metal.
- **Valoria identity:** blue accents/banners/heraldry.
- **Corruption/Breach identity:** violet/magenta used selectively as a contrast, never as arbitrary neon geometry.
- Lighting should be readable and luminous, with warm settlement lights and clear atmospheric separation.
- Use shadows, fog/haze, ambient depth and distance lighting to create scale.
- Avoid blown-out whites, flat lighting and uniformly dark presentation.

## The Breach / corruption

- Corruption must feel territorial and environmental.
- Prefer scars, altered land, distant glow, fissures, corrupted vegetation, fog and landscape changes.
- It should read as a powerful world threat in the distance and gradually contaminate nearby areas.
- Do not represent it with floating bars, simple purple cylinders, placeholder crystals or unrelated neon primitives unless they are temporary development-only markers hidden from the player build.

## Life and activity

Valoria should look inhabited:
- workers/builders;
- soldiers/training;
- smoke from active structures;
- fires/torches;
- construction/reconstruction;
- camps, tents, tools and materials;
- subtle movement and environmental animation where practical.

The player should feel that Valoria is rebuilding, not that the camera is looking at a static diorama.

## HUD relationship to the world

- The world is the protagonist of the screen.
- HUD must frame the world rather than cover it.
- Building labels, chapter/mission information and navigation should be compact, readable and visually integrated with the fantasy setting.
- Avoid developer-style debug text or generic Unity buttons in owner/player review builds.

## Asset-selection rule

Imported assets are raw material, not the art direction.

For every asset:
1. Does its silhouette fit Eldoria?
2. Does its material/palette fit the scene?
3. Does its scale fit surrounding assets?
4. Can it be integrated without looking like a foreign pack?
5. Does it improve the full-frame composition?

If not, do not use it simply because it is available or technically compatible.

## Acceptance criteria for the Valoria vertical slice

A Valoria/Bastion art pass is not accepted merely because:
- Unity compiles;
- the build is green;
- better models were imported;
- terrain is no longer white;
- more vegetation exists.

It is accepted only when the **whole frame** begins to communicate:
- epic fantasy;
- a dense, living kingdom;
- monumental scale;
- strong depth;
- coherent visual identity;
- readable 4X gameplay;
- a larger world beyond the city;
- corruption as a distant/encroaching threat.

If a capture still reads primarily as a blockout, test map, prefab showcase, flat board or Unity prototype, continue iterating before expanding the art direction to more of Eldoria.

## Review protocol

For each benchmark iteration:
1. Capture at least three deterministic views of Valoria/Bastion.
2. Compare against the approved reference direction on:
   - composition;
   - density;
   - depth;
   - lighting;
   - scale;
   - architecture;
   - terrain;
   - environmental storytelling;
   - corruption treatment;
   - HUD/world balance.
3. Record remaining gaps honestly.
4. Do not expand the visual pass to the rest of the game until Valoria is convincing enough to act as the repeatable standard.

## Relationship to the web vertical slice

The web build remains the gameplay/system reference until Unity parity is explicitly approved. This visual benchmark governs **how Unity should present Eldoria**, while the web slice continues to preserve established gameplay, progression, UX and system contracts during migration.
