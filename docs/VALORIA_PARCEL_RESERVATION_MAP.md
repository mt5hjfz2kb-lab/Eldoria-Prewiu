# VALORIA PARCEL RESERVATION MAP

Status: **ACTIVE — ART CONSOLIDATION v1 spatial contract**  
Date: 2026-10-03  
Macro direction: **Flat Citadel locked**

## Why this exists

Valoria must not be art-directed only for the current visible buildings. The current Unity slice exposes Bastion / Aserradero / Cuartel, while the canonical web progression already defines additional Arc-I world-space structures. Long-range planning explicitly targets roughly Bastion 25–35.

The reservation map therefore protects maximum building envelopes and expansion corridors before final dressing.

This document does **not** invent unlock levels beyond the canonical game contract. Confirmed Bastion I–X unlocks come from `v0220/js/gameplay.js`; levels 11–35 use the district bands from `docs/VALORIA_PROGRESSION_MAP_V1.md` and `docs/VALORIA_MASTER_PLAN_V1.md`.

## Confirmed Arc-I physical building progression

| Progression | World-space result | Reservation rule |
| --- | --- | --- |
| Bastion 1 | **Aserradero** becomes the first functional reconstruction | Active from early state; west-lower economic plot reserved to maximum envelope |
| Bastion 2 | **Cuartel** is constructed | Active east-lower military plot; protected training-yard frontage |
| Bastion 3 | **Granero** is constructed | Active south/central utility plot; protected yard and road access |
| Bastion 4 | Existing Aserradero / Granero / Cuartel advance to level 2; no new physical building | Existing plot envelopes absorb art-tier growth; roads do not move |
| Bastion 5 | **Cantera de Valoria** is constructed | Reserved west-mid economic plot |
| Bastion 6 | **Forja** is reconstructed | Reserved east-mid production plot |
| Bastion 7 | Códice + Relicario systems unlock | **No physical parcel by default**; both are UI/meta systems unless later gameplay explicitly creates a city building |
| Bastion 8 | Hero / expedition progression; no confirmed new city structure | No new permanent plot consumed |
| Bastion 9 | War-preparation progression; no confirmed new city structure | No new permanent plot consumed |
| Bastion 10 | **Hospital** participates in the Arc-I finale and must exist as a city POI | Reserved south-east / lower civic-support plot |

## Coordinate frame

Coordinates are visual-plan coordinates used by the current Flat Citadel proof family. They are not gameplay colliders and do not move canonical interactions.

Current Arc-I defensive ring is approximately:
- X: -9.5 to +9.5
- Z: -6.4 to +9.3
- Hero Bastion anchor: around X 0 / Z 7.25
- gate / lower approach: negative Z

## Parcel map

| ID | Current state | Centre (X,Z) | Protected max envelope | Intended role |
| --- | --- | ---: | ---: | --- |
| H0 | active | 0, 7.25 | 8.2 × 7.6 | Hero Bastion; only primary elevation |
| F1 | active | -5.95, -1.55 | 5.0 × 4.4 | Aserradero + work yard |
| F2 | active | +5.95, -1.75 | 5.2 × 4.5 | Cuartel + training yard |
| F3 | active | -2.75, -4.38 | 4.4 × 3.9 | Granero + storage yard |
| R4 | reserved Arc I | -6.25, +3.20 | 5.2 × 4.4 | Cantera / heavy economy |
| R5 | reserved Arc I | +6.25, +3.15 | 5.0 × 4.3 | Forja / production |
| R6 | reserved Arc I | +2.70, -4.35 | 4.4 × 3.8 | Hospital / recovery |
| C0 | protected | 0, -4.8 → 0, +4.6 | 2.4–3.2 wide | Gate → plaza → Bastion processional road |
| C1 | protected | -0.5,-0.9 → -8.4,-1.2 | ≥1.6 wide | West service/economy route |
| C2 | protected | +0.5,-0.9 → +8.4,-1.2 | ≥1.6 wide | East military/production route |
| XW | reserved 11–35 | west wall seam near -9.5,+3.8 | ≥3.2 opening interface | West Growth District expansion corridor |
| XE | reserved 11–35 | east wall seam near +9.5,+3.8 | ≥3.2 opening interface | East Military/Production expansion corridor |
| XU | reserved 16–35 | rear/upper edge behind Bastion | hero-scale plot(s), exact metres provisional | Upper Civic / Government District |
| XS | reserved 26–35 | outer future edge, side chosen after envelope graybox | large special plot | Special / prestige / alliance/civic system if gameplay requires it |

## Early-state appearance of reserved parcels

R4 / R5 / R6 must remain visually intentional while empty. Allowed early-state treatments are:
- compacted earth;
- low temporary fences;
- sparse construction stakes;
- one or two material stacks that are removable;
- controlled weeds/grass at edges;
- broken low masonry that can become foundations.

Forbidden on these parcels:
- permanent houses;
- mature trees in the building envelope;
- large boulders;
- decorative monuments;
- irreversible VFX anchors;
- wall towers that block future access;
- any dressing that forces a future road relocation.

The parcel should read as **prepared / recoverable city ground**, not as an empty developer slot.

## Wall growth interfaces

The Arc-I wall is not allowed to become a permanent obstacle to Bastion 11–35 expansion.

Art Consolidation v1 therefore treats west/east mid-wall seams as **expansion interfaces**:
- today: defensible closed/repairable wall condition;
- future: can become a gate/breach/extended street without moving the Arc-I kernel;
- no hero tower is placed directly across the future corridor;
- nearby wall modules remain replaceable.

This also helps the present visual problem: the wall can vary by function rather than repeating identical tower/curtain rhythm.

## Long-range district reservation

The canonical long-range progression remains:
- 1–5: kernel;
- 6–10: kernel densifies + adjacent reservations become legible;
- 11–15: first lateral district;
- 16–20: opposite lateral district + upper civic;
- 21–25: capital defensive/civic growth;
- 26–30: advanced / special plots;
- 31–35: prestige completion.

Exact late-game building identities are intentionally **not invented here**. The master envelope is protected by district class and maximum plot class until gameplay assigns a system.

## Dressing mask for Art Consolidation v1

Permanent dressing may occupy:
- narrow road edges outside clearance;
- wall shoulders away from XW/XE;
- non-reserved natural surround;
- building-owned yards after maximum envelope is respected.

Permanent dressing must stay out of:
- H0 maximum Bastion envelope;
- R4 / R5 / R6;
- C0 / C1 / C2;
- XW / XE;
- XU approach;
- XS long-range reserve.

## Acceptance gate

The Art Consolidation capture must prove all of the following without changing gameplay authority:
1. R4/R5/R6 remain visibly available.
2. C0/C1/C2 remain readable.
3. west/east future expansion seams are not blocked by permanent wall hero pieces.
4. Bastion remains the main landmark.
5. current city still feels intentionally composed rather than under-filled.
6. dressing does not consume future system territory.
7. 19/12/9/mobile preserve the same reservation logic.
