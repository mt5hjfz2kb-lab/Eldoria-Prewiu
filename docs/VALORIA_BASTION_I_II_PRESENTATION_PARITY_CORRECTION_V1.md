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

## Final gate

Do not close until production run, screenshots/artifacts, publication, published probe, gameplay/save-reload and mobile checks pass. Any regression is corrected in this same workstream.
