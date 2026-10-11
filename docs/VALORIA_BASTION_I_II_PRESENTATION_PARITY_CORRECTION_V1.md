# VALORIA BASTION I-II — PRESENTATION PARITY CORRECTION v1

Status: **PASS / CLOSED — current published WebGL passed the declared presentation-parity, interaction, progression, persistence and reset gates**
Workstream: `valoria-bastion-i-ii-presentation-parity-correction-v1`

## Authority and scope

Canonical comparison is the repository's current web vertical slice (`v0220/index.html`) versus the current Unity production path published at `/unity-owner/`. Gameplay/state/persistence/economy, SHARP authority and World Region 1 are protected and are not redesigned by this block. Bastion III remains closed.

## Direct confrontation

| Surface | Web vertical slice authority | Unity before correction | Verdict | Correction |
|---|---|---|---|---|
| HOME/camera | City-first, large/local, not map-like | Published owner review: too distant | PARITY_FAIL | Production HOME uses 0.88 of source FOV, bounded pan expanded slightly |
| HUD hierarchy | Compact top resources + quest + bottom nav | Migrated compact hierarchy present | PARITY_PASS | Preserved |
| Building CTA | Building interaction is spatial/contextual | Bottom-wide detached panel | PARITY_FAIL | Compact contextual panel follows selected building and sits below it |
| Construction timing | Visible action/feedback cadence | Global message had seconds, building panel only said “en construcción” | PARITY_PARTIAL | Building CTA/body now show live remaining seconds |
| Feedback | Toast/particles/objective emphasis | Real feedback exists but activity is visually quiet | PARITY_PARTIAL | Preserve real messages; add bounded ambient/construct activity layer |
| Ambient life | Web has mist drift, ember rise, sheen, particles and animated objective cues | SHARP city reads as static image between interactions | PARITY_FAIL | Low-cost presentation-only mist/embers plus construction pulse; no gameplay invented |
| Bastion I | Real state-driven I and SHARP authority | Correct | PARITY_PASS | Preserved |
| Bastion II | Real state-driven II cues/unlocks | Correct technical migration | PARITY_PASS | Preserved |
| I→II transition | Real power/unlock feedback | Present | PARITY_PASS | Preserved |
| Mobile legibility | Compact safe-area hierarchy | Certified landscape/portrait | PARITY_PASS | Context panel reduced to 276×166 and clamped on-screen |
| World Region 1 | Separate real 4X screen | Certified | PARITY_PASS | Untouched |

## Additional parity findings

The canonical web slice contains continuous atmosphere (mist, ember motion, resource sheen) and explicit objective emphasis. Unity had functional feedback but almost no persistent motion over the SHARP city, amplifying the “photo” impression. The correction adds only presentation motion and construction activity feedback. It does not create NPC simulation, resource simulation, fake controls or new gameplay.

## Implementation

- `SlicePresenter`: building-associated contextual panel, live construction countdown, city ambient presentation layer, construction activity pulse.
- `ValoriaParcelPresentation`: tighter HOME framing via `PresentationHomeFov`, preserving local bounded pan and restrained zoom.
- Tests lock the contextual panel footprint, ambient layer presence and corrected HOME FOV.
- Paid credits: **0**.


## Owner additions — mandatory before closure

The owner has identified two additional requirements on the published owner build. They are part of this same workstream and must not be deferred or moved to another block.

### Repeatable owner reset to the real start

The current published link can reopen from persisted Bastion II/progress, which prevents reliable owner validation of Bastion I and the I→II loop.

Add a clear, explicit reset/new-start action suitable for the owner build that:
- returns to the canonical initial game state so Bastion I can be tested again from zero;
- clears/reinitializes only the canonical persisted state required for a true fresh start;
- does not introduce a second state system, fake progression or parallel persistence;
- is deliberate enough to avoid accidental reset;
- is validated in the published owner build, including that a subsequent reload remains at the reset canonical state.

### Art-integrated building level indicator

Relevant real buildings must display a small but clearly legible level number beside/near the building.
- The value must come from real canonical building state, never hardcoded mock data.
- It must visually belong to the city art/UI language rather than look like debug text.
- It must remain readable on mobile without cluttering the scene or competing with the contextual CTA.
- Validate at least the relevant Bastion I/Bastion II building states and capture evidence.

These requirements are additive. They do not replace the existing HOME framing, contextual construction CTA, live countdown, ambient-life and full vertical-slice parity obligations.

## Final gate — PASS / CLOSED

Final production run **37681793700** passed with artifact **11509591694**; UI certification run **37681793717** and source/editor gate **37681794138** also passed. The certified owner WebGL was published by run **37684790661**. Published mobile probe **37687592179** passed with artifact **11511827528** and `playablePass=true`: HOME, horizontal touch pan both directions, vertical pan, MUNDO, Region 1 gather/reward, REINO/Valoria return, reload persistence, and landscape/portrait checks all passed without a white screen. Persisted reload state remained revision 4 / wood 590 / stone 150 / gatheredWood 360 / gatheredStone 0 / sawmill 0 / Bastion I.

The presentation correction remains bounded to Bastion I-II. Bastion III was not opened; World Region 1 gameplay/state authority, economy, persistence and SHARP authority were not redesigned. Paid credits: **0**.

Published owner build: `https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/unity-owner/`.

## 2026-10-08 autonomous closure policy

Owner/manual physical-device confirmation is no longer a mandatory closure condition for this workstream. The owning execution must continue automatically from current `main` through current-head production, matched visual evidence, gameplay/UI/persistence gates, WebGL publication and published device-equivalent probe. If those declared gates pass, close and release the workstream autonomously; do not leave it active merely awaiting owner review. Concrete later owner feedback remains authoritative evidence and may revoke/reopen the PASS.

Intermediate commits, queued/in-progress runs, capture availability, publication, probe completion, or chat/context boundaries are not stopping points. Continue diagnosis → correction → rerun → validation → publish → probe → close until completion or a genuine human/platform blocker.


## 2026-10-08 final autonomous recertification

The reopened correction is now **CLOSED**.

Final certified chain:
- runtime reset/touch repair: `6add911217465d2f4ab264815af8a8eb2d0cf454`;
- published release request: `fc33c67521a9376b66fc38f83d1b8b7fdab8f215`;
- probe control-geometry correction: `0e32b3504302349907a7ad93eadfddb6ca5d91fe`;
- production run **37767338367** — SUCCESS, artifact **11546715716**;
- UI progression certification **37767338225** — SUCCESS;
- Unity source/editor gate **37767338361** — SUCCESS;
- publish run **37768050958** — SUCCESS;
- published WebGL probe **37770987758** — SUCCESS, evidence artifact **11548212275**.

The published probe completed the full fresh-save Bastion I→II loop in both landscape and portrait, including real visible touch-selection of Sawmill/Bastion/Barracks, construction, gathering, recruitment, configured march, Engendro combat/reward, exact final-state reload persistence, deliberate reset and reset-survives-reload. No runtime errors remained in the accepted probe.

Matched screenshots were inspected for fresh Bastion I, building selections and Bastion II completion. The controls/context panel remain visible inside the mobile frame and the corrected camera/pan/recenter behavior is evidenced.

**Important quality boundary:** this is a PASS for the declared presentation-parity correction scope. It is **not** a claim that overall Eldoria artistic quality is final or that independent human playability/artistic feel has been fully certified. Later concrete owner/device feedback remains authoritative and can reopen the block.

Detailed closure evidence: `docs/evidence/valoria-bastion-i-ii-presentation-parity-correction-v1/FINAL_CLOSURE_2026-10-08.md`.
