# Eldoria — QA, Test Mode and deployment

## Development build
Canonical gameplay source remains `v0220/index.html` + `v0220/js/`. `tools/build-preview.mjs` generates `playtest/` and, only for that development build, injects the isolated QA support files. Never edit `playtest/` directly.

Local setup:
```bash
npm install
npm run qa:setup
npm run build
python3 -m http.server 4173
```
Normal development game: `http://127.0.0.1:4173/playtest/`
QA Launcher: `http://127.0.0.1:4173/playtest/?qa=1&launcher=1`

## QA storage isolation
`qa/qa-storage-isolation.js` loads before the game runtime when `?qa=1`. It transparently redirects only the canonical save key `eldoria-v022-consistent-loop` to a QA-only key. Therefore:
- loading presets does not overwrite a normal player save;
- QA fresh save resets only the QA save;
- leaving QA mode returns to the untouched normal save;
- the frozen tester snapshot does not contain this launcher/injection.

Do not replace this with fixture writes against the real save key.

## QA Launcher modes
The launcher is development-only and exposes three human paths:

### Probar solo el cambio
Loads one deterministic preset exactly at the system under test. Current presets:
- Héroes + Tropas + Marcha
- Bastión II — Engendro de la Fisura
- Bastión III — Fisura / Lyra
- Bastión VI — Forja / Devorador
- Bastión VII — Códice / Reliquias / Duelo
- Bastión VIII — Maelis
- Bastión IX — preparación de marcha / ataque
- Jefe del mundo semiautomático
- Misiones y capítulos v0.27
- Aceleradores
- Final Bastión X

### Probar un tramo
Current segment presets:
- Bastión VI → VIII
- Bastión IX → X

A segment starts before the first system in the block and leaves the relevant later dependencies unresolved so the tester genuinely traverses the block.

### Jugar desde cero
`FRESH SAVE` clears the isolated QA save and reloads Bastion I. It does not clear the normal player save.

Direct preset deep links use `?qa=1&preset=<preset-id>` and load the requested isolated state automatically. Example: `playtest/?qa=1&preset=hero-army-base`.

The preset catalog is `v0220/js/qa-fixtures.js`. It is UMD so the same fixture source can be reused by browser QA and Node/Playwright tooling. Presets must be deterministic, coherent with actual progression, and must not expose future player-facing systems early.

## Owner delivery contract
Every implementation delivery must expose the smallest useful manual verification path to the owner after automated QA has passed.

- **🎯 Probar esta mejora**: focused deep link into the development build and the preset/state that exercises the delivered change. Required whenever isolated testing is reasonable.
- **🧩 Probar tramo**: segment deep link when the change crosses related systems or progression phases. Omit only when a segment adds no meaningful coverage beyond the focused case.
- **🎮 Jugar completo**: always provide the normal development build URL for ordinary play/fresh-save review.

The agent selects the preset from the affected system automatically. If the feature is new and no suitable focused preset exists, add/adapt a deterministic preset or equivalent development-only deep-link as part of the implementation when reasonable. Presets must remain coherent with real progression, isolated from normal saves and unavailable from the frozen tester build.

Owner links are not evidence of correctness by themselves. The automated tier selected by risk still has to pass before delivery. Full fresh-save validation remains mandatory internally for milestones, progression/economy/sequencing work and release candidates even though the owner may use a focused link for convenience.

Never send owner-review links to `tester-v0265/`; use only the current development `playtest/` build.

## Automated QA tiers
### Fast focused iteration
Use the smallest test that proves the change. For launcher/fixture integrity:
```bash
npm run qa:focus
```
The relevant dedicated Playwright file is preferred when one exists.

### Segment QA
For changes spanning a progression block:
```bash
npm run qa:segment
```
Combine with system-specific regressions as needed.

### Integral / fresh-save QA
For milestones, progression/economy/sequencing changes and release candidates:
```bash
npm run validate:local
```
This remains the full release-candidate gate: build + contracts + targeted blockers + regression suite + uninterrupted fresh-save Arc I.

`qa/e2e-full-arc1.js` remains the canonical uninterrupted reachability proof and may not use `setQA/loadState` to jump progression. Deterministic presets are not a substitute for this gate.

## Runtime QA API
The runtime still exposes `window.ELDORIA_V023` / compatibility alias `ELDORIA_V022`, including `loadState`, `state`, `setQA`, `reset`, assertions and economy acceleration for automated tests. The development launcher adds `window.ELDORIA_QA` only under `?qa=1`, with `list()`, `loadPreset(id)`, `fresh()`, `normal()` and `state()`.

## Playwright rules
- Reuse launcher fixtures for targeted state setup whenever practical.
- A fixture must preserve dependencies important to the system under test; do not use impossible god states that hide blockers.
- Prefer real `tap/click` interactions over JS DOM clicks.
- Every player-reported regression should become a permanent assertion when practical.
- Focused/segment fixture success is evidence for that surface only; it does not prove uninterrupted progression.

Canonical mobile emulation remains 390×844, `isMobile:true`, `hasTouch:true`.

## Deployment
`.github/workflows/pages.yml` runs on pushes to `main`. It remains final clean-environment certification/deployment and guards the frozen tester snapshot. Important/release candidates still run the integral gate before being called stable. Do not use Actions as the normal iteration debugger when local targeted QA is sufficient.

A build is only called published after Pages deployment succeeds and only called verified after the published Chromium check succeeds.


## Military system focused acceptance
For changes touching Barracks, troop tiers, Hero Hall, hero affinities, march composition or expedition Power:
- use `?qa=1&preset=hero-army-base` for owner/manual focused review;
- run `qa/e2e-v027-hero-army.js` and `qa/e2e-v027-military-circuit.js`;
- retain `qa/e2e-v027-barracks-recruitment.js` for recruitment/tier visibility;
- escalate to full `validate:local` for any change affecting progression, combat calculations or global Power.
