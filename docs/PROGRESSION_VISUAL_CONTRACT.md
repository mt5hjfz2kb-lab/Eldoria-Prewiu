# Eldoria — Progression Visual Contract

This file is a hard implementation rule for Unity. The canonical vertical slice defines when gameplay content exists for the player; art production must not reveal or activate content earlier than that contract.

## Core rule

A gameplay element may be authored in the scene or reserved in layout before unlock, but it must not appear as a completed/usable element and must not expose an active gameplay hotspot before its progression gate.

## Current I–III contract

- Bastion I
  - Bastion is visible and interactive.
  - Sawmill plot/state may exist as required by the chapter flow.
  - Completed Sawmill architecture appears only after reconstruction completes.
  - Barracks completed architecture is hidden.
  - Barracks gameplay hotspot is disabled/absent.
  - Granary completed architecture is hidden.

- Bastion II
  - Barracks plot/interaction becomes available according to the chapter objective.
  - Completed Barracks architecture appears only after construction completes.
  - Granary remains hidden.

- Bastion III
  - Granary may become visible/available according to the canonical chapter flow.

## Implementation requirements

1. Visual state and interaction state must be gated independently but consistently.
2. Reserved terrain, collision supports, camera bounds, and future district envelopes may remain present if invisible/non-interactive.
3. Imported art must never bypass progression just because the asset exists in Resources or has already passed visual certification.
4. Any scene reload/state refresh must reconstruct the same progression-correct visual state from PlayerState.
5. Future buildings, units, districts, world nodes, narrative props, and UI entry points must follow the same rule.

## QA acceptance

For each milestone, QA must verify at minimum:

- content that should not exist yet is not visibly present as completed art;
- content that should not be usable yet cannot be selected/tapped;
- newly unlocked content appears at the correct milestone;
- completed construction replaces its pre-build state only after the authoritative state changes;
- returning to the scene preserves the correct visual/interactable state.

The repository and canonical vertical slice are authoritative over chat memory or temporary art-pass convenience.
