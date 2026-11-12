# Eldoria — countdown attached to its construction site (2026-11-09)

Workstream-ID: eldoria-ui-v3-management-completion-20261021

## Owner-observed defect and diagnosed cause

On the live pretester beta (PR179 mobile10), a paid building timer slides/parks at
the side of the screen while its building stays in the scene. This is confirmed in
source: `RoyalGoldV3ContextHud.RenderTimer` replaced world/camera projection with
`safe.rect.xMax - width / 2 - 12`, a fixed HUD dock X coordinate. This is a
presentation regression, not a construction deadline/save change.

The frozen beta's `tools/eldoria-private-beta/native/ux/RoyalGoldV3ContextHud.cs.txt`
contains the same fixed-dock logic. That overlay is owned by the active PR179
workstream and is **not** altered by this PR. PR173 and PR181 are untouched.

## Implemented in canonical V3 source

- Keep a 220×46 maximum compact countdown and its authentic progress from the
  existing gateway, with no new clock/controller.
- Classify Bastion, Sawmill, Barracks, Granary and Stoneworks construction (including
  `upgrade-barracks`) as world-anchored.
- Each active frame, obtain the existing scene Transform and Camera, project the
  model's roof via the existing collider-corner projection; show the timer over
  that roof, or below its silhouette if the actual HUD overlaps.
- Do not clamp a displaced clock to a distant screen edge. Hide it when the
  building is not projectable or no near-building screen position is available.
- Preserve the prior separation of expedition/gathering operation summaries,
  which can stay in the compact HUD dock.
- Preserve the selected-building rule that avoids a second overlapping timer
  while the existing illustrated operation sheet is visible.

## Verification performed

- Source inspection confirmed the fixed-right formula in the previously
  published beta and main.
- 26 source-contract and representative mathematical layout checks passed on
  the new branch (expected follow behaviour, top/bottom fallback, hide when
  offscreen and no return to side docking).
- The committed regression source/model check is
  `node tools/ui-v3-domain-smoke/verify-construction-anchor.mjs`.
- **Unity 6000.3.23f1 has NOT yet compiled or rendered this candidate.**
  The checks above do not prove real Canvas geometry, frame timing or mobile UI.

## Native acceptance still required before promotion

1. Compile the exact PR HEAD in an isolated Unity checkout.
2. On a disposable real runtime save, start Sawmill/Barracks construction and
   upgrade; capture 390×844 and 844×390 (plus 1600×900 and 900×1600).
3. Pan/zoom the active camera: measure
   `WorldToScreenPoint(building/collider)` and
   `RectTransformUtility.WorldToScreenPoint(timer.Root.position)` at each
   frame; the timer must move with its building, with near-building offset
   maintained (not a constant HUD coordinate).
4. Check boundary cases: building partly/off camera, top mission overlap,
   selected building's illustrated sheet, reload while work is active, and
   multiple unrelated operations. No old fixed side timer.
5. Verify the countdown and operation completion use the pre-existing
   authoritative gateway; run the relevant I–V journey and gameplay gates.

PR179 owns the currently published beta. After its responsible workstream
integrates or ports the candidate into its hash-pinned overlay manifest, it
must update `candidate_sha256`, replace the old fixed-HUD test assertion
with an actual target-relative/camera-pan assertion, run its native and WebGL
regressions, then publish a new site version. **This PR alone does not change
the live beta, APK or WebGL.**
