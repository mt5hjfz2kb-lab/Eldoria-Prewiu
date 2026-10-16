# Valoria — Visual quality gate after native Unity V9→V15 (2026-10-16)

**Status: ART_FAIL / ART_APPROVED=false / not publishable.**
Canonical owner image: **1513×1039**, SHA256 **de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f**.
All frames here are actual Unity 6000.3.23f1 D3D11 native output at 1536×1024 and are on this isolated branch. No paid sources were purchased.

## Authenticated source and screenshots

| Variant | Unity Actions run | Original overview | Original close | Main finding |
|---|---|---|---|---|
| V9 | [38823012065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38823012065) | [17-real-mountains-overview.png](17-real-mountains-overview.png) | [18-real-mountains-close.png](18-real-mountains-close.png) | Real mountain prefabs were grossly overscaled and obscured scenic depth |
| V10 | [38858416758](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38858416758) | [19-natural-valley-overview.png](19-natural-valley-overview.png) | [20-natural-valley-close.png](20-natural-valley-close.png) | Continuous valley 3D mesh became a grey wall behind upper gate |
| V11 | [38858830128](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38858830128) | [21-misty-valley-overview.png](21-misty-valley-overview.png) | [22-misty-valley-close.png](22-misty-valley-close.png) | Lower landscape still covered the sky from inclined orthographic camera |
| V12 | [38859169267](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38859169267) | [23-open-horizon-overview.png](23-open-horizon-overview.png) | [24-open-horizon-close.png](24-open-horizon-close.png) | Lateral landscape split yielded a sharp triangular terrain cut in top-left |
| V13 | [38859552245](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38859552245) | [25-actual-sky-overview.png](25-actual-sky-overview.png) | [26-actual-sky-close.png](26-actual-sky-close.png) | Camera-aware sky panel removes most terrain wedge but skyline still flat/empty |
| V14 | [38860019892](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38860019892) | [27-enhanced-lower-gate-overview.png](27-enhanced-lower-gate-overview.png) | [28-enhanced-lower-gate-close.png](28-enhanced-lower-gate-close.png) | Enlarged lower gate makes entry read better; many extra cliff rocks floated outside platform; reject boulder placements |
| V15 | [38860369047](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38860369047) | [29-grounded-gate-overview.png](29-grounded-gate-overview.png) | [30-grounded-gate-close.png](30-grounded-gate-close.png) | Corrected rocks are on the existing plateau; gate width is more legible; complete artistic quality still far below reference |

## Subjective, conservative scorecard for V15 (canonical reference = 10/10)

This is a **diagnostic estimate**, not objective numerical certification; owner must review real screenshots.

| Axis | Approx. V15 /10 | Evidence |
|---|---:|---|
| Canonical two-gate/topological structure | 5.5 | Correct broad axis; plaza and intermediate stair still simplified |
| Camera and silhouette | 4.5 | Upper/lower readable; substantial proportions/framing gap |
| Architectural finish, materials and grounded contact | 3.0 | Existing source models genuine; masonry repetitive/weakly shaded |
| Landscape/forest/background coherence | 1.5 | Isolated plateau in empty flat backdrop, remaining triangular artifact |
| Lighting, shadow and color | 2.5 | Warm torches exist; no full atmospheric/mood reconstruction |
| Overall visual acceptance | **FAIL (roughly 3/10)** | Large gap from owner's 8/10 requirement |

## Scientific decision: stop blind iteration of the same background generator

V9–V13 attempted 5 background code changes **without visually converging** on the canonical valley. A CI PASS demonstrates compilation, not art quality. Do not continue optimizing procedural far hills/sky quads as if these guaranteed professional art. The correct next experiment is to source/validate a **single coherent real terrain/forest/background kit** and show a like-for-like Unity A/B. Keep the strongest existing V15 composition as an **unapproved blockout**, not a game scene.

Further work should prioritize:
1. Replace flat foreground and disconnected cliff silhouettes with a connected professionally surfaced terrain family, with source rights documented.
2. One coherent background + lighting study from fixed reference camera; reject if no objectively visible improvement over V15 and never hide missing 3D geometry behind a screenshot.
3. Replace generic upper-wall and lower-gate geometry only with **individually accepted** components. The active 63-asset track's current 7.3/7.4 provisional donors are **NOT ART_APPROVED** and MUST NOT be promoted or imported as validated production art.
4. Only then improve stone texturing, vegetation and details with fixed-camera comparison.
5. Never touch production scenes/gameplay or merge without owner's artistic approval and rights verification.

**Technical outcome:** 7 native Unity variants V9–V15 captured and committed, each with true 1536×1024 PNG and versioned source. No asset passed the 8/10 graphical quality gate. The zero-cost path remains open, but this particular procedural landscape approach **did not work**.

## Containment and reproducibility

- Active branch: `prototype/reference-first-zero-budget-visual-pilot-20261015`.
- Original reference is user-uploaded and identified by SHA, not substituted.
- Last capture: `Unity/Assets/Eldoria/ArtTests/ReferenceFirstPilot/Editor/ReferenceFirstCompositionV15Capture.cs`.
- Last workflow: `.github/workflows/eldoria-reference-first-zero-budget-pilot.yml`.
- Last manifest: `render-manifest-v15.json` with artApproved=false, productionModified=false.
- All artistic gate failures are independent of CI workflow conclusions.
