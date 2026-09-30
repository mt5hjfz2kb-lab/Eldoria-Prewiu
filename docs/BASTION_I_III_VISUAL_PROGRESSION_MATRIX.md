# Eldoria — Bastion I–III Visual & Interaction Progression Matrix

Status: **CANONICAL IMPLEMENTATION CHECKLIST**  
Updated: 2026-09-29

This matrix turns the canonical vertical-slice chronology into explicit Unity visibility and interaction rules.

The fact that an asset already exists, is integrated for composition, or has passed an art gate does **not** mean the player may see or use it.

## Bastion I

### Aserradero
- plot/location may exist from the beginning;
- interaction is available when the chapter objective requires reconstruction;
- completed production architecture is hidden while `SawmillLevel == 0`;
- completed architecture appears only when reconstruction completes;
- its independent gameplay hotspot remains authoritative.

### Cuartel
- completed architecture is hidden for all of Bastion I;
- gameplay target may exist internally for stable topology, but its collider must be disabled;
- no player-facing UI action may expose Cuartel gameplay during Bastion I;
- no single frame may flash the completed Cuartel during scene creation/reload.

### Granero
- completed Granero architecture is absent/hidden;
- no Granero gameplay target or UI action is exposed;
- reserved city space may exist only as normal early-city/support occupation, not as a recognizable completed Granero.

## Bastion II

### Aserradero
- reconstructed architecture remains visible and usable.

### Cuartel
- Cuartel becomes an active chapter objective;
- interaction/plot may become available;
- before construction completes, the completed production building remains hidden;
- scaffolding / construction-state art may be shown if authored;
- after `BarracksLevel > 0`, the production Cuartel becomes visible;
- after unlock, its independent target is interactive.

### Granero
- completed Granero remains absent/hidden;
- no Granero interaction or player-facing UI entry point is exposed.

## Bastion III

### Granero
- Granero may enter the visible/player-facing chronology according to the canonical Bastion III chapter flow;
- the dedicated production asset may be loaded/integrated technically earlier, but visibility must remain gated until Bastion III;
- future Granero interaction must use independent gameplay geometry, not imported visual colliders.

## Whole-scene rules

For every progression state:

1. **Visual state, gameplay interaction and UI exposure are separate gates but must agree.**
2. Future content may be present in project resources or reserved topology without becoming visible/interactive.
3. Scene creation must apply the authoritative state before the first rendered frame.
4. Persistent refresh must preserve that state after construction completion, reload and return from Frontier.
5. Imported meshes never own gameplay click geometry.
6. A player should be able to infer city growth from visible changes: ruined/empty → construction/need → completed building.
7. The canonical vertical slice and repository contracts override temporary art-pass convenience.

## Current implementation mapping

- `VisualWorld.cs`
  - creates stable topology and dedicated art;
  - applies `ValoriaProgressionVisualGuard.Apply(state)` before the first rendered frame;
  - Cuartel target collider is created already gated by Bastion level;
  - Granero production architecture is only instantiated from Bastion III in the current master-envelope implementation.

- `ValoriaProgressionVisualGuard.cs`
  - completed Aserradero visible only when `SawmillLevel > 0`;
  - completed Cuartel visible only when `BarracksLevel > 0`;
  - Cuartel target interactive only from Bastion II;
  - keeps visual state synchronized with persisted `PlayerState`.

- `SliceSceneSmokeTests.cs`
  - verifies Cuartel hidden/non-interactive in Bastion I;
  - verifies Cuartel visible/interactive once completed in Bastion II.

## QA checklist for every new building

Before promotion of a new building/feature, answer all of these explicitly:

- Which Bastion level first reveals the location/need?
- What does the plot look like before unlock?
- When does interaction become available?
- Is there a construction/transition state?
- What exact state makes the completed asset appear?
- Is the UI entry point gated to the same moment?
- Does a reload preserve the same state?
- Can any imported visual collider accidentally make it clickable early?
- Does the prior Bastion state remain visually truthful?
- Is the progression covered by an automated regression where practical?

No future dedicated building is considered fully integrated until this checklist has an answer.
