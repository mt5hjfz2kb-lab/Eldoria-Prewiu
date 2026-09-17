# Eldoria v0.19.5 r7 — Consolidation baseline

Date: 2026-09-17
Branch: `dev/r7-consolidation`

## Purpose

Stabilize the real r7 candidate before adding content. No r8 is created. `main` and the current public root remain untouched while consolidation is performed.

## Verified repository state

- `main` HEAD at consolidation start: `be1a6e2783c21d83a912af46242445014f1d2850`.
- Root `index.html` currently redirects to `v0195-clean`, not r7.
- `v0195-r7/index.html` is a loader that fetches `v0195-r6/index.html` and injects `r7patch.js` by string replacement.
- r6 itself applies r4/r5/r6 text patches over older source material.
- Therefore the distribution path is currently layered and fragile; it is not the desired development architecture.
- r7 contains real functional/polish changes and must be preserved.

## Architecture target

The next editable baseline must have:

1. One consolidated current build source.
2. Directly editable HTML/CSS/JS rather than runtime patch chains.
3. One build identifier and one save key/schema for the active build.
4. No iframe/version chaining.
5. No runtime string replacement as the primary versioning mechanism.
6. Historical directories kept read-only for recovery until the consolidated build passes QA.
7. GitHub Pages publication changed only after structural validation and playtest.

## Canonical Chapter II flow to preserve and validate

First combat → Rift choice → first card → Aldric tutorial → Hero Hall → global-world introduction → Safe Forest → Contested Quarry → Global Rift Zone → short world assessment → return to Valoria → Bastion → second expedition → remainder of Chapter II → mandatory Relic Duel tutorial with Orin → reward/final → Chapter III teaser: Sea of Glass.

## Vertical-slice operating checklist

### DONE / present in current code path
- Chapter I Valoria reconstruction framework.
- Semi-automatic combat framework.
- Rift choice and first-card reward framework.
- Hero progression concepts: XP, level, Heroic Essence, skill rank, Rift Resonance.
- Hero Hall.
- Global 4X world representation.
- Safe Forest / Contested Quarry / Global Rift onboarding concepts.
- Gate preventing Bastion progression before global onboarding in r6/r7 path.
- Relic collection and Relic Duel framework.
- Mandatory first-duel intent in r6/r7 path (skip/close removed during tutorial).
- Chapter III / Sea of Glass teaser markers exist in the validated clean path.

### PARTIAL / must be reachability-tested
- Exact uninterrupted Chapter II ordering.
- Contextual Hero tutorial completion into Hero Hall and onward transition.
- Global onboarding mission guidance on mobile.
- Return from global-world assessment into Bastion progression.
- Second expedition → remaining Chapter II → Duel transition.
- Orin tutorial teaching N/E/S/W, relic selection, center +1, Orin move, opposed values and capture through interaction.
- End-of-slice reward/final and Sea of Glass teaser reachability.
- Mobile overlays, sticky actions, safe-area behavior and scroll containment.
- Save migration/reset behavior across historical build schemas.

### PENDING
- Full executable E2E from a fresh save to Sea of Glass.
- Consolidated directly editable r7 source replacing the loader/patch distribution chain.
- Structural/DOM reachability audit of every required state transition.
- Mobile human playtest after consolidation.
- Publication of consolidated r7 to the root.

### NEW RECENTLY ADDED / now canonical
- Long Chapter II: Heroes, cards, Duel and global world stay inside Chapter II.
- Global world is mandatory progression, not optional side content.
- Onboarding order: Forest → Quarry → Global Rift Zone.
- Bastion and Chapter II closure blocked until global onboarding is complete.
- First Relic Duel cannot be skipped.
- Chapter III is only `Mar de Cristal` teaser/locked content.
- No claim of real multiplayer; other kingdoms are representation of the future shared-world fantasy only.

## Vertical-slice closure criteria

r7 can be considered structurally consolidated only when all of the following are true:

- A fresh save can reach every canonical state in order with no manual state editing.
- No required system exists only as unreachable code/UI.
- First Duel is mandatory and its tutorial is interactive.
- Global onboarding is mandatory and gates Bastion correctly.
- Chapter II cannot close early.
- Sea of Glass appears only after the Chapter II final.
- Mobile viewport has no blocking overlap or unreachable primary action in the critical path.
- Reset produces a genuinely clean test session.
- The active build has one source path and no runtime dependency on r4/r5/r6/r7 patch chaining.

## Publication discipline

Do not repoint the public root during consolidation. First consolidate, run static/DOM/state QA, then perform a real playtest where possible. Only after that should the root be changed deliberately.
