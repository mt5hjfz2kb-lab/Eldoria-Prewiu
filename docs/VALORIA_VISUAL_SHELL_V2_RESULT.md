# Valoria Visual Shell v2 — Result

Date: 2026-10-03

## Scope

Isolated complete-frame proof on branch `visual-proof/valoria-visual-shell-v2`.
Gameplay, progression, routes, camera authority, colliders and hotspots were preserved.
Tripo spend: 0 credits.

## Baseline

The canonical handoff came from the completed full-frame convergence workstream after v41.
The strongest visually useful pre-shell frame remained the clean hero-first / dedicated-building composition around v38; v39-v41 world-frame geometry experiments were technically valid but visually rejected.

## Method 1 — projected annulus

- Branch head: `8952f6836dfb6726e7f02a38af904e071f12639a`
- Run: `37119744934` — SUCCESS
- Artifact: `11271839600`
- Technique: 2.5D annulus sampling the exact Kiara backplate in screen space.
- Verdict: **VISUAL FAIL**.
- Reason: shell sampled the raw image while the canonical backplate applied its own grading, producing a visible dark annulus.

## Method 2 — grading-matched annulus

- Final branch head: `d46757f0270871792572a7434ed51845005fea37`
- Run: `37120085692` — SUCCESS
- Artifact: `11273037225`
- Technique: exact backplate tint match, reduced contact grade, tighter/raised inner ring.
- Verdict: **TECH PASS / VISUAL INSUFFICIENT**.
- Result: the shell became effectively invisible, proving screen-projected matching works, but it did not materially improve the floating lower-cliff silhouette.

## Method 3 — localized projected matte

- Branch head: `8e357a8d3b1118dada6f8950735dd2bbdc5e8919`
- Run: `37120292764` — SUCCESS
- Artifact: `11273257204`
- Technique: camera-locked local matte intended to erase only the lower-right hanging cliff residue while sampling the canonical backplate.
- Verdict: **TECH PASS / VISUAL INSUFFICIENT**.
- Result: no material full-frame improvement over method 2 / clean baseline.

## Final verdict

**VISUAL FAIL / CATEGORY CEILING PROVEN.**

The 2.5D projected-shell approach can match the backplate cleanly, but it cannot solve the remaining quality blocker because the blocker is real foreground geometry/silhouette, not merely a background seam.

Per the workstream contract, stop after three methods and reclassify.

## Reclassification

Next route: **zero-credit native terrain geometry**.

Recommended next proof:
- Blender-generated terrain/cliff shell specific to Valoria;
- no Tripo and no paid assets;
- compact footprint;
- irregular high-density rock skirt under/around the hero island;
- separate top/cliff material regions using existing Unity PBR materials;
- no broad foreground platform;
- preserve gameplay/colliders/hotspots/routes;
- full-frame capture required before any promotion.

Do not promote the Visual Shell v2 experimental code to production.
