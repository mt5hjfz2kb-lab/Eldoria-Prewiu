# Issue 186 — macroscopic responsive composition, 12 November 2026

**Stage:** implemented only on PR #187 branch `e2782abc7623d282d3da93a3c8cd680e1e682c21`; source CI [43191687786](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43191687786) SUCCESS. **Not yet proven by a new Unity frame, art A/B, WebGL, Android or iPhone. Do not merge or publish.**

## Diagnosis from original native pixels, not from a renderer mock

The authentic [43189305862](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43189305862) batch passed 52 first-hit interactions and 20 actual saved bag/mail/city images, with Unity overlay restored. Its original screenshots are stored byte-identically in `docs/issue186/remote-native-review/43189305862/`.

The remaining dominant artistic weakness is **composition**, not a damaged sprite or missing texture: on **1600×900** the item picture and amount are near the far left while the one USAR button sits near the far right (more than a thousand pixels of low-information gap); on **900×1600** 96-pixel items and short rows are squeezed into the upper part of a tall, almost empty panel. This cannot be solved with another 2-pixel margin change. Prior row-frame simplification and better badge placement helped local readability, but did not solve this scale mismatch.

## Implemented change and rationale

`Issue186BagOverlay.Layout()` now uses the **same active owner Canvas**, existing `panel_fill`, original packet PNGs, burgundy USAR skin, real EventSystem first-hit policy, resource gateway and persisted saved state. No extra Canvas, duplicate currency owner, generated PNG, paid asset or Unity mesh is introduced.

* For a large wide viewport (width at least 1150, height at least 650 and aspect over 1.35), four packets become a **central, two-column/two-row layout** of at most 1120 units across, with max 240-unit row height and 136-unit icons. This shortens name/amount-to-action travel without moving USAR into the card's transparent Seen surface.
* For tall tablet portraits (height at least 1150, width at least 650), the central content is at most 800 units wide, four rows grow up to 270 units high with max 150-unit icons, and titles/amounts are proportionally larger.
* The original 390×844 phone and 844×390 short landscape still follow their previous one-column/four-row and one-column/two-row logic. The first four packet receipts, total count, page state, skip guide, real hand/halo and exact-once resource mutations are unchanged.
* Row tint is deliberately faint rather than an extra duplicate ornate frame; original separators remain. The body is vertically centered within otherwise unused height on large formats.

Before commit, independent source-level geometry arithmetic at **390×844, 844×390, 900×1600 and 1600×900** rejected rows that clipped the footer, insufficient text width or icon size below 48. These checks do **not** claim Unity rendering approval.

## Acceptance gate for next legitimate Windows turn

Run the **existing** Issue186 combined flow on the new exact PR187 HEAD, only once the exclusive Windows lease belongs to #187 and no conflicting Unity process is active. Demand four fresh GPU/URP city frames and 16 fresh saved bag/mail screenshots; compare at the same resolutions with native [43189305862](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43189305862) images. Review the whole composition, text at actual screen pixels, red NEW badge, the real hand over the actual USAR target and no amount/label truncation. Require actual frontmost modal/first-hit interactions, read≠claim≠credit, USAR exactly once, JSON reload, and two-layer 245-file exact restoration. Refuse stale screenshots and side-effectful cleanup.

**If visual gain is not obvious, do not start another round of microscopic spacing changes.** Reconsider the panel's overall presentation structure using the approved reference, but preserve the gateway, navigation and saved receipts. The owner must see the native before/after images before any art reaches main.
