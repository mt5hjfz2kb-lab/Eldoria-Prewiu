# Eldoria — Visual Bible / Unity Art Target

Status: **canonical visual production guide**  
Scope: Valoria, Bastion progression, environment language, camera, materials, lighting and Unity translation.  
Parent direction: `docs/MVP_ART_DIRECTION.md`.

This document turns the approved art direction into concrete production rules. Concept images are **visual targets**, not literal gameplay screenshots. The final mobile game must preserve the world identity while prioritizing interaction clarity, performance and readable 4X play.

## 1. North-star statement

**Valoria is a living fortress rebuilt inside the corpse of a lost empire.**

The player should read three ideas immediately:
1. this place was once much greater than it is now;
2. people are surviving and rebuilding inside impossible ruins;
3. La Brecha is still present beyond the safety of the city.

Emotional target for Bastion I:

> “This was enormous. It is broken. Now it is mine to rebuild.”

## 2. Style

- dark fantasy;
- stylized semi-realism;
- solemn, mature and premium;
- atmospheric rather than visually black;
- detailed enough to feel rich, simplified enough to remain readable on mobile;
- never cartoon, glossy-casual, photorealistic, or muddy grimdark.

## 3. Valoria composition

Valoria is **vertical, compact, asymmetric and defensible**.

### Vertical hierarchy
- **upper tier:** Bastion / seat of power;
- **middle tier:** military, workshops and repaired imperial structures;
- **lower tier:** food, storage, shelter, population and logistics;
- **background:** mountains, colossal imperial remains and distant Breach pressure.

The Bastion must remain the primary focal point. The city must not read as a flat grid of buildings.

## 4. The Bastion

Canonical concept:

**“A fortress built inside a dead palace.”**

The Bastion combines military mass with the remains of an older seat of power.

### Shape language
- broad defensive base;
- thick stone walls and large gates;
- broken ceremonial architecture reused rather than restored perfectly;
- one dominant upper mass/tower;
- asymmetry from collapse, repairs and scaffolding;
- no fairy-tale castle silhouette.

It must communicate both government and resistance: **this is where Valoria is ruled, but above all where Valoria survives.**

## 5. Signature landmark — the broken cyclopean arch

Approved identity landmark: **a colossal broken imperial arch** crossing the city/valley.

It is not decoration. It is inherited infrastructure.

### Rules
- larger than any current Valorian building;
- visibly older than the rebuilding settlement;
- broken into at least two major surviving masses;
- integrated into walls, roads, stairs, workshops or defensive routes;
- frames or leads the eye toward the Bastion without obscuring it;
- remains visible through Bastion I–X progression;
- should provoke the question: **“Who built this?”**

Valoria lives **inside and around** the remains of the previous civilization.

## 6. Bastion I — canonical visual state

Bastion I is fragile recovery, not an established city.

### Required composition
- Bastion elevated and partially ruined;
- broken cyclopean arch as the principal ancient landmark;
- small settlement at the Bastion’s feet;
- **Sawmill** in a clear work/logging zone;
- **Granary** in the lower inhabited/logistics area;
- **Barracks** as a modest early defensive structure;
- temporary shelters, carts, crates, fires and repair scaffolding;
- substantial empty/ruined space reserved visually for future growth;
- mountains and larger ruins beyond Valoria;
- La Brecha only as a distant, restrained anomaly.

### Life
The scene must feel inhabited:
- workers repairing structures;
- guards;
- carts and supply movement;
- smoke from fires/workshops;
- hanging cloth/banners;
- small ambient conversations/idle groups;
- birds and restrained environmental motion.

Bastion I must not already look like Bastion VIII.

## 7. Materials

### Ancient imperial remains
- enormous weathered grey stone;
- erosion, cracks, moss and age;
- heavier scale and craftsmanship than anything modern Valoria can reproduce.

### Rebuilt Valoria
- more regular but humbler stone;
- dark repaired timber;
- blackened iron;
- worn fabrics;
- visible scaffolding, braces and patched construction.

### Ground
- rock;
- earth;
- worn stone paths;
- mud where appropriate;
- restrained/desaturated vegetation.

Rule: **the ancient world is monumental; the living reconstruction is functional and human.**

## 8. Color script

### Base world
- stone grey;
- cold blue-grey;
- muted earth brown;
- charcoal;
- desaturated green.

### Human life
- warm amber;
- fire orange;
- restrained old gold;
- warm windows/interiors.

### La Brecha
- violet;
- magenta;
- black-violet;
- rare white-hot/crimson accents.

**Violet is not Eldoria’s general brand color. It belongs to La Brecha and corruption.** Its scarcity gives it meaning.

## 9. Lighting and atmosphere

Preferred baseline:
- cloudy late-afternoon / broken overcast;
- cool directional world light;
- readable shadow detail;
- low atmospheric mist in chasms/valleys;
- fine smoke from habitation and reconstruction;
- warm local fire/window contrast.

Dark fantasy comes from mood and history, not from making the player unable to see.

## 10. Camera and gameplay framing

Target camera: elevated **3/4 view, approximately 35–40°**, adapted through real mobile testing.

### Principles
- Bastion remains prominent;
- façades and vertical depth stay visible;
- interactable buildings remain easy to identify/tap;
- world occupies most of the screen;
- no free 360° rotation in the first production target unless testing proves it necessary.

### Intended scales
1. **Detail:** admire a building/character/upgrade moment.
2. **Management:** normal city play; default target.
3. **Wide:** understand Valoria’s layout and growth.

The concept-art composition is a target for hierarchy and atmosphere, **not a fixed in-game camera screenshot**.

## 11. Progression Bastion I–X

Progression tells the story of reconstruction. It is never only “same building, bigger.”

### I–II — Survival
- exposed ruins;
- timber repairs;
- tents/shelters;
- fires;
- sparse population;
- weak defensive completeness.

### III–IV — Settlement
- first stable districts;
- clearer roads;
- repaired walls;
- more organized logistics.

### V–VI — Fortress
- stone becomes dominant;
- stronger towers/defenses;
- visible military organization;
- city reads as defensible.

### VII–VIII — Realm
- Valoria develops its own architectural identity;
- ceremonial/government detail;
- denser inhabited city;
- stronger banners and civic cohesion.

### IX–X — Renaissance
- monumental strength returns;
- repaired ancient structures are used confidently;
- the city feels like a power again.

**Scars remain at every stage.** Valoria never becomes a pristine fantasy theme park.

## 12. World relationship

Valoria must visually belong to a larger 4X world:
- routes leave the city logically;
- terrain connects to frontier geography;
- distant ruins imply regions beyond the current play space;
- Breach pressure exists in the world, not as a decorative sky effect;
- city scale and world scale must remain compatible with future persistent multiplayer.

## 13. Mobile production rules

When translating the target to Unity:
- strong silhouettes before surface detail;
- modular environment kits;
- LODs and culling from the start;
- reusable stone/timber/ruin material families;
- restrained VFX;
- readable touch targets;
- interaction feedback must survive atmospheric lighting;
- visual density may never hide gameplay.

## 14. Approved visual-target interpretation

The current approved concept direction shows:
- a prominent ruined/rebuilding Bastion;
- a huge broken imperial arch;
- layered cliff/terrace construction;
- warm inhabited pockets inside a cold damaged landscape;
- a distant violet Breach manifestation;
- visible repair activity and early settlement life.

For **Bastion I**, production must reduce the apparent maturity/density of the settlement relative to grand key art: more damage, more scaffolding, fewer completed buildings and more obvious space for growth.

## 15. Acceptance test for a first Unity art pass

A screenshot of the playable city should allow a new viewer to answer, without explanation:

- Where is the Bastion?
- What is ancient and what is being rebuilt?
- Where can I interact?
- Does this look like a living city rather than a matte painting?
- Is the cyclopean arch part of the city’s identity?
- Can I sense a larger ruined world beyond Valoria?
- Is La Brecha present without dominating every normal surface?
- Could this plausibly evolve from Bastion I to X?

If any answer is unclear, the visual pass is not complete.

## 16. Reference-management rule

Approved concept images belong under `docs/visual-reference/` and must be labeled by purpose and status. They are references, not source assets for runtime shipping.

Suggested naming:
- `valoria-bastion-i-target-v01.png`
- `valoria-bastion-i-target-v02.png`
- `valoria-bastion-i-gameplay-framing-v01.png`

When a concept is superseded, keep the newer approved target clearly identified in `docs/visual-reference/README.md`.
