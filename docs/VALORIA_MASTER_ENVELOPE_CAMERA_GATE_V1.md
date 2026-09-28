# Valoria Master Envelope + Camera Gate v1

Date: 2026-09-28  
Status: **CERTIFIED — STRUCTURAL / INTERACTION SCOPE**  
Certification HEAD: `c8135a8c162e7849d852472d67e2492cd8a88af4`  
Unity Actions run: **36432979557**  
Valoria capture artifact: **10974741738**

## Objective

Prove, before broad production-art dressing, that the certified Playable District Skeleton can grow into a larger-than-one-mobile-viewport Valoria while preserving:
- the original kernel;
- authored isometric orientation;
- bounded pan;
- progression-aware camera bounds;
- real building interaction;
- zoom 9 / 12 / 19;
- mobile framing.

## Implemented master-envelope graybox

The certified kernel remains untouched in role:
- lower entry;
- Planta 0;
- main street;
- independent staircase;
- Planta 1;
- Bastion / Aserradero / Cuartel.

The following structural reservations now exist around it:
- west growth district;
- west route;
- east growth district;
- east route;
- upper civic/government reserve;
- upper civic link;
- late-game/future-system reserve;
- west/east terrain aprons so camera travel does not immediately expose empty void.

These are **graybox reservations, not final art**.

## Camera implementation

The city camera is now:
- fixed authored isometric orientation;
- orthographic zoom clamped to 9..19;
- bounded world translation/pan;
- progression-aware pan range;
- recenterable to the certified home pose.

Current progression-bound prototype:
- Bastion <=10: compact pan envelope;
- 11–15: wider first expansion;
- 16–20: both lateral wings;
- 21–25: capital-scale envelope;
- >25: full planned prototype envelope.

Exact metre bounds remain adjustable after art and mobile playtesting; the **multi-viewport bounded-pan model is now technically proven**.

## Tap vs drag

Runtime input now distinguishes:
- clean tap → building/world interaction;
- drag beyond threshold → camera pan;
- drag release → no accidental building selection.

Aserradero, Cuartel and Bastion remain canonical `WorldHotspot` targets.

## Automated proof

Run **36432979557** completed successfully:
- source preflight: PASS;
- EditMode: PASS;
- PlayMode: PASS;
- Windows desktop build: PASS;
- Valoria benchmark capture: PASS;
- required expansion/mobile capture files: PASS.

PlayMode verifies:
- master reservation objects exist;
- small movement remains below pan threshold;
- larger movement becomes a pan gesture;
- early-game bounds clamp tightly;
- late-game bounds open materially farther;
- camera rotation remains unchanged;
- Bastion resolves after camera translation;
- recenter returns to the exact home position.

## Visual evidence reviewed

Artifact **10974741738** contains:
- `valoria-establishing.png`
- `valoria-gate.png`
- `valoria-districts.png`
- `valoria-master-west.png`
- `valoria-master-east.png`
- `valoria-master-future.png`
- `valoria-mobile.png`

Manual structural review:
- the original kernel remains clearly recognizable and centered as the city anchor;
- west and east reservations provide meaningful lateral growth capacity;
- the future/upper reservation exists without requiring displacement of the core;
- panned views remain inside authored terrain rather than falling immediately into void;
- portrait/mobile framing demonstrates why Valoria must be navigated rather than compressed into one screen;
- the blockout remains intentionally crude/flat and is **not** an art-quality approval.

## Verdicts

**TECH PASS**

**CAMERA / INTERACTION PASS — PROTOTYPE SCOPE**

**MASTER ENVELOPE PASS — STRUCTURAL SCOPE**

**VISUAL PASS — GRAYBOX READABILITY ONLY**

This gate does not certify:
- final city dimensions;
- final Bastion cap;
- final district art;
- production materials;
- final mobile UX feel/inertia;
- final LOD/performance;
- any specific late-game building list.

## Production consequence

Broad production-art dressing may now begin **provided it respects the certified master-plan constraints**:

1. preserve the kernel circulation;
2. preserve reserved growth capacity;
3. author art to plots/districts instead of letting assets dictate topology;
4. keep building maximum envelopes in mind;
5. validate all major art from home + pan positions;
6. keep the city larger than one mobile viewport;
7. do not collapse the west/east/future reservations merely to make an early screenshot look fuller.

The next art block should refine the approved structure, not reopen topology unless a real gameplay requirement proves the structure insufficient.
