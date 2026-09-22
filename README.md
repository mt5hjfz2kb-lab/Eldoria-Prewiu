# Eldoria — Web Vertical Slice

Start with **`AGENTS.md`**. It defines the repository hierarchy and permanent workflow.

- Active development: `main`.
- Canonical editable runtime: `v0220/index.html` + `v0220/js/`.
- Active runtime milestone: **v0.27**.
- `v0220` is a compatibility directory name; it does not mean the active version is v0.22.
- `playtest/` is generated deployment output; never edit it as source.
- Last certified stable recovery baseline: `baseline/v0.24-certified` at `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435`.
- `v019*`, `v020*`, `v0210`, r7 tooling and old docs are historical/recovery only and do not participate in normal work.

After `AGENTS.md`, a new session reads `SESSION_HANDOFF.md` and `PROJECT_STATE.md`. Specialized documentation is consulted on demand.

Frozen external tester snapshot: **Eldoria Closed Playtest T1 / 0.26.5-test.2** at `/tester-v0265/`. It is isolated from normal development and must not be used as a development source.

Public playtest: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/

Local setup for any new working session:
```bash
npm install
npm run qa:setup
npm run validate:local
```

`npm run validate:local` is the single release-candidate gate: it builds the generated preview, checks contracts, runs real mobile interactions and completes Arc I from a fresh save. A coherent green push to `main` then certifies and publishes the same candidate through GitHub Pages.
