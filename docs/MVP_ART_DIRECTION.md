# Eldoria — MVP Art Direction / Visual Target

Status: **canonical MVP visual direction**  
Scope: visual identity, readability and future Unity target.  
This document does **not** require the current web prototype to reproduce the target art literally.

## Product visual statement

**Eldoria is a stylized semi-realistic dark-fantasy 4X, built for mobile readability, in which the player rebuilds a monumental bastion over the ruins of Valoria while exploring a fractured continent threatened by La Brecha.**

The visual promise is premium, solemn and epic, but always subordinate to gameplay clarity.

## Core art pillars

1. **Monumental ruin** — Eldoria must communicate that a great civilization existed here before the player. Arches, bridges, walls, towers and imperial remains carry historical weight.
2. **Reconstruction over ashes** — inhabited areas use warm human light, activity and repaired structures against a cold damaged world.
3. **La Brecha as a wound in reality** — not a generic portal. It corrupts terrain, matter, atmosphere and creatures.
4. **True 4X scale** — the world must read as territory: regions, routes, settlements, nodes, threats, marches and strategic space.
5. **Mobile-first legibility** — strong silhouettes, clear interactables, restrained permanent UI and a visible world/playfield.
6. **Stylized semi-realism** — more serious and grounded than bright/casual fantasy, but never photorealistic or visually muddy.

## City / Valoria — canonical target

### Identity
Valoria is a **bastion-realm rebuilt inside the remains of an imperial corpse**. It is compact, vertical and defensible, not a generic spread of small houses.

### Required visual language
- massive stone terraces, walls, bridges and arches;
- broken imperial structures reused by the living city;
- strong vertical depth and fog-filled chasms;
- cold blue/grey ambient world light;
- warm amber windows, furnaces, braziers and inhabited spaces;
- readable routes and walkways between interactable buildings;
- ruined or locked districts that can be recovered over progression.

### Hero buildings / landmarks
These need distinct silhouettes and practical tap readability:
- **Bastión** — dominant progression spine and visual anchor;
- **Cuartel** — visibly military and functional;
- **Granero** — food/production identity;
- **Forja** — heat, metal, smoke and industrial glow;
- **Salón de Héroes** — ceremonial, prestigious and character-focused;
- future districts/buildings may expand the city, but must not destroy hierarchy.

### City gameplay rule
The player should be able to look at the city and immediately understand what can be touched, upgraded or entered. Art must never turn the city into a matte painting with invisible interaction.

## World map — canonical target

### Identity
The world is a **large fractured strategic continent**, not a scenic background with icons pasted on top.

### Required readable elements
- player realm/settlement;
- other strongholds / neutral or future player settlements;
- resource nodes;
- fauna / hunting;
- common and uncommon corrupted threats;
- world bosses / special threats;
- fissures / corrupted locations;
- marches and travel paths;
- named regions / biomes;
- strategic ruins, bridges, passes and coastlines;
- clear territory/route logic at the appropriate zoom.

### Zoom principle
The final 4X may support several information scales:
1. **Strategic / continent** — regions, major conflicts, Breach pressure, alliances/territory.
2. **Regional** — settlements, fortresses, large objectives and routes.
3. **Local gameplay** — cities, marches, enemies, beasts, resources and actionable nodes.

The current web slice only needs to communicate this future direction truthfully; it must not fake a finished shared-world system.

## La Brecha — canonical visual language

La Brecha is a dimensional injury and the strongest visual contrast in Eldoria.

### Palette / material
- deep violet;
- magenta;
- black-violet;
- selective crimson / white-hot accents;
- floating fragments, impossible geometry, glowing cracks, distorted atmosphere.

### Progression of influence
1. subtle anomalous marks;
2. local fissures;
3. corrupted zones / creatures;
4. major world manifestation.

La Brecha must attract attention without making every normal region visually irrelevant.

## UI direction

The interface is **dark, elegant, compact and strategic**.

- charcoal / slate / stone panels;
- restrained old-gold / ivory iconography;
- Breach violet only where semantically relevant;
- red reserved for real danger/error/hostility;
- compact contextual surfaces rather than permanent giant cards;
- deep management screens may be full-screen;
- ordinary city/world actions remain object-local or compact;
- the world remains the protagonist.

### Mobile readability rule
Visual ambition must never reduce tap clarity, text readability, safe-area compliance, or the visible gameplay share. Prefer fewer stronger visual signals over decorative density.

## Color script

### World base
- stone grey;
- cold blue-grey;
- charcoal;
- muted earth;
- desaturated vegetation;
- ash beige.

### Human / reconstruction accent
- warm amber;
- fire orange;
- soft gold.

### Breach accent
- violet;
- magenta;
- black-violet;
- occasional crimson.

The restrained base exists so human warmth and Breach corruption remain meaningful.

## Heroes and troops

### Heroes
Heroes should have strong silhouettes and narrative presence without superhero/cartoon excess. Their equipment and visual identity should communicate role before reading numbers.

**Aldric** establishes the tone: guardian, authority, burden, worn nobility and resilience.

### Troops
Troop families must be readable by silhouette and battlefield role:
- **Arqueros** — lighter offensive/ranged identity;
- **Paladines** — future heavy defensive identity;
- **Brujos** — future arcane/rupture identity tied more closely to Breach knowledge.

Player-facing unlock visibility continues to follow the narrative rule **encontrar → comprender → desbloquear**.

## Creatures and threats

- Hunting fauna should belong to the ecosystem and remain readable at map scale.
- Breach enemies must look altered/corrupted rather than like unrelated fantasy monsters.
- World bosses need unique silhouettes and spectacle, but gameplay telegraphs/readability win over ornament.

## What Eldoria is NOT

- not cartoon fantasy;
- not bright casual-mobile fantasy;
- not photorealism;
- not muddy grimdark where interactables disappear;
- not a static key-art composition pretending to be gameplay;
- not a UI-heavy screen where the actual world becomes background;
- not a direct visual copy of another 4X.

## Realism / engagement decision

Eldoria targets **stylized semi-realism**, not maximum realism.

The objective is to feel more solemn, mature and premium than bright/casual references while keeping the strengths that make successful mobile strategy readable:
- immediate hierarchy;
- obvious interactions;
- visible progression;
- clear rewards;
- strong silhouettes;
- fast visual comprehension.

**Rule:** never increase realism or decorative detail if it reduces readability, performance, interaction clarity or progression feedback.

## Canonical production detail

The concrete Valoria/Unity production rules are maintained in [`docs/VISUAL_BIBLE.md`](VISUAL_BIBLE.md). That document owns the approved Bastion I composition, broken cyclopean arch landmark, camera target, materials, lighting and Bastion I–X visual progression. This file remains the higher-level MVP art-direction statement.

## Unity handoff rule

When the project migrates to Unity, this document becomes a visual production target, not a demand for literal 1:1 reproduction.

Unity production should translate this direction into:
- playable camera and world scale;
- modular city/district growth;
- readable 3D/2.5D assets;
- mobile LOD/performance budgets;
- shaders/VFX for Breach corruption;
- strong UI hierarchy;
- reusable environment kits;
- animation and interaction feedback.

The web prototype remains a gameplay/design laboratory. Do not spend web-prototype time recreating final Unity art if doing so does not answer a product question.

## MVP acceptance implication

The MVP does not need final-production art. It **does** need to prove that its gameplay, UX and content are compatible with this visual target.

Before Unity migration, the slice should validate:
- kingdom → need/choice → world → gather/fight/discover → reward → return;
- readable city/world navigation;
- combat and hero systems;
- Breach as a meaningful gameplay/narrative promise;
- a coherent first-session experience;
- human playtest evidence that players understand the loop and want to continue.

## Visual reference status

Two gameplay-oriented visual targets were approved in the design session:
- **City target:** playable isometric Valoria, compact building labels, strong vertical fortress structure, warm inhabited lights, distant Breach pressure.
- **World target:** readable 4X map with player realm, routes/marches, resources, threats, regions and an integrated Breach zone.

These are **directional targets, not pixel-perfect UI contracts**. Camera, spacing, density and controls must continue to evolve through real mobile testing.
