# VALORIA BASTION I-II — PRESENTATION PARITY CORRECTION v1

Status: **ACTIVE — production validation pending**
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

## Final gate

Do not close until production run, screenshots/artifacts, publication, published probe, gameplay/save-reload and mobile checks pass, including a published fresh-start reset proof and real building-level indicator proof. Any regression is corrected in this same workstream.
