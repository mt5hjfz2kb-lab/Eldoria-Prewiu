## Canonical pipeline enforcement — owner reinstatement 2026-10-09

The original Eldoria production pipeline is the **single route** for all player-facing changes, including work produced by local Ollama, ChatGPT Work, DG and M16. This operational requirement complements the independent playable-build acceptance gate below and does not create a second pipeline, workflow, department or claim.

**Dispatch and ownership:** M16 consults `pipeline/active-workstreams.json` and allocates only a bounded already-authorized order within the owning department's scope. The source SHA, owner, runner reservation and authorized next transition are preserved. R2-B owns `windows-runner-heavy` except during explicitly coordinated, completed loans.

**Production route:** specialist implementation → exact-source integration into real Unity project → source/editor verification → existing Unity candidate build (or authenticated exact-tree reuse) → independent D13/D14/D02 technical and functional QA → D09/D10 mobile/UX review and M05 visual review where applicable → genuine candidate playable-path and reload/regression checks → M03/M16 acceptance → approved delivery. QA scope exclusions must be justified, never silently skipped. No stable Pages publication or ownership release on partial PASS.

**Failure route:** return the exact defect, failing job/artifact, source revision and permitted files to the existing department via M04/M16; bounded repair → new independent QA against the corrected revision. Prevent retries on unchanged failing SHA. If a runner is busy, defer/requeue the authorized order and resume via the existing durable trigger; never start a competing runner job. Keep workstream OPEN/BLOCKED with real evidence until acceptance or genuine owner/platform intervention.

**Single source of truth:** each authorized workstream phase must distinguish produced / Unity-integrated / built / independently-playtested / accepted / delivered; reconcile stale registry phases against actual runs before dispatch. Historical QA SUCCESS on an earlier source must never be attached to a newer candidate. Support-only work may close independently but must not be represented as player-facing delivery.

**Application to active work:** owners of R2-B and `eldoria-local-agent-preflight-v1` must continue their currently authorized claims through this same route and update their owned workflow code only as necessary. This policy does not itself grant access to occupied scopes or runners, trigger a build, create a new workstream or certify existing incomplete work.

# Eldoria — QA, Test Mode and deployment

## Independent playable-build acceptance gate

**Applies to every significant player-facing change.** This is an acceptance gate within the existing departments, not a new team or authorization to launch jobs. Follow `AGENTS.md` and current workstream ownership before dispatch.

| Existing owner/reviewer | Required independent assessment |
| --- | --- |
| Implementing department | Deliver exact candidate source SHA, runnable URL, affected flow, expected outcomes and changed-surface list; do not self-certify acceptance |
| D13/D14/D02 (QA) | Build/runtime errors, real gameplay interactions, state/economy/progression, reload persistence, affected regressions and full fresh-save loop where risk warrants |
| D10/D09 (UX) | Real mobile/touch navigation, camera/pan, accessibility of controls, discoverability, comprehension, feedback and clear next action; landscape/portrait as applicable |
| M05 / existing visual reviewer | Direct inspection of genuine candidate captures/video at canonical cameras, before/after comparison, framing, visual consistency and regression; never infer visual PASS from a green CI |
| M16/M03 (independent coordinator/verifier) | Confirm exact SHA/run/artifact linkage, actual non-skipped QA execution, separation of reviewer from implementer and all applicable gate verdicts; route defects to existing M04/owner |

**Minimum playable scenario:** Launch the actual candidate/published build in a fresh isolated save; reach the affected feature by ordinary controls (not a test-only state injection as the sole proof); perform the player action; observe visible response and resource/state change; navigate away and back; reload to check persistence when applicable; inspect landscape and portrait mobile captures, and execute the complete progression loop for milestone/release or economy/progression changes. Include an unaffected adjacent flow to detect regressions. For art-only changes, direct before/after visual inspection is mandatory; for audio, inspect actual playback/trigger evidence where tooling supports it.

**Evidence record (per delivery):**
- Workstream, implementing owner, independent reviewer(s), source SHA, exact candidate URL/build and version.
- Real run ID, job/step conclusions (including skipped vs executed), artifact IDs, screenshots/video, browser/viewport/touch configuration and tested steps.
- `TECH`, `FUNCTIONAL`, `VISUAL`, `EXPERIENCE`, `REGRESSION`, `PUBLISHED` (when applicable): each `PASS`, `FAIL`, `BLOCKED/NOT_VERIFIED`, or `N/A` with justification.
- Defect IDs and before/after retest evidence; objective UX findings, plus explicit limits (automated mobile emulation is not a physical-device/human enjoyment test).
- Final coordinator verdict and links to canonical GitHub issue/workstream record.

**Acceptance:** A green workflow, skipped job, successful Unity build, static screenshot or implementation-only assertion is not independent gameplay/experience acceptance. Missing applicable evidence is `NOT_VERIFIED`, never PASS. A failed visual or experience gate overrides functional PASS for final acceptance. Route failures through existing M03/M04 and the original owner for bounded correction; rerun an independent test against the corrected SHA and update the evidence. Only after all applicable gates PASS may the coordinator record `ACCEPTED`, close the workstream, release its resources and trigger an already-authorized compatible handoff. No mandatory owner playtest, new department, paid services, or automatic dispatch is implied.


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
- Bastión VII — Códice / Relicario
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

## Discoverability / comprehension pass
Every player-facing focused, segment or integral regression must evaluate more than technical success. For new or modified systems, explicitly check:

1. **Discoverability** — does the player notice that the control/system exists?
2. **Comprehension** — is its purpose understandable without external explanation?
3. **Interaction** — is the expected action obvious and reachable, especially on mobile?
4. **Feedback** — does the UI communicate what happened in readable, proportional feedback?
5. **Next step** — does the player understand what to do next?

Also audit responsibility boundaries: Barracks, troops, heroes, march preparation, Codex, Reliquary, Chest and mission systems must not mix unrelated management information. Contrast, text size, truncation, hidden affordances and tutorial timing are release blockers when they prevent understanding.

A green functional test does not override a failed novice-player UX pass. The build remains unverified until both are green.

## Deployment
`.github/workflows/pages.yml` runs on pushes to `main`. It remains final clean-environment certification/deployment and guards the frozen tester snapshot. Important/release candidates still run the integral gate before being called stable. Do not use Actions as the normal iteration debugger when local targeted QA is sufficient.

A build is only called published after Pages deployment succeeds and only called verified after the published Chromium check succeeds.


## Military system focused acceptance
For changes touching Barracks, troop tiers, Hero Hall, hero affinities, march composition or expedition Power:
- use `?qa=1&preset=hero-army-base` for owner/manual focused review;
- run `qa/e2e-v027-hero-army.js` and `qa/e2e-v027-military-circuit.js`;
- retain `qa/e2e-v027-barracks-recruitment.js` for recruitment/tier visibility;
- escalate to full `validate:local` for any change affecting progression, combat calculations or global Power.
