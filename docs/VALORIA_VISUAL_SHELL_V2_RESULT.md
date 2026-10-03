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

## Evidence correction and continuous landform proof (same day)

The method 1 artifact reports `visual_shell_v2_built: true`. The method 2 and method 3 artifacts each report `visual_shell_v2_built: false`, despite successful workflow status. Their images are essentially the baseline island. Those two runs therefore **do not establish** that a grading-matched annulus or localized matte can solve or cannot solve the problem. The earlier “category ceiling proven” conclusion for 2.5D projection was overstated. Method 1 alone shows a visible dark annulus and fails visual review.

A second isolated branch, `visual-proof/valoria-shell-v2-landform`, tested a genuinely instantiated continuous PBR terrain mesh with the certified Hero Bastion, existing functional sawmill/barracks, a compact granary/service family, shared surface maps, fog/light changes, and 19/12/9/mobile plus camera-offset captures. It changed presentation only. The workflow staged CC0 Poly Haven maps and the backplate without paid assets or Tripo.

| Proof | Source | Run / artifact | Gameplay signature | Full-frame verdict |
| --- | --- | --- | --- | --- |
| Broad ridge and unified lower/upper height field | `658bc99c5b5b424d3f77b17181e3b0d7c4842600` | [37120495376](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37120495376) / `11272259962` | Equal | **Visual fail.** Rear brown ridge hides the horizon, arch reads as a brick slab, route as a dark plank. |
| Open valley, widened terrain blend, restrained architecture, continuous stone route | `7b913fa8c8ce72ee1ccf310c242797059309cdc7` | [37120699376](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37120699376) / `11272793710` | Equal | **Visual fail.** Horizon returns, but the bare terrain reads as a mesa, route clips into two pale strips, buildings remain detached. West/east camera offsets expose the same seams. |

Both runs completed their Unity capture gate and emitted real images. Technical success is not visual acceptance. Neither code branch is promoted to `main`.

### Reference comparison, ranked by visual impact

The approved 1536×1024 benchmark in `docs/ELDORIA_VISUAL_BENCHMARK.md` shows a populated fortress city embedded in irregular rock, layered terraces, monumental ruins, lush valley depth, a coherent stone/timber/roof palette, and a legible 4X hierarchy. The best current real full frame is still hero-first, with a substantially more detailed Bastion than its support environment.

1. **Silhouette and mass:** the Bastion still presents a distinct rounded rocky plinth against unrelated lower terrain; the benchmark connects cliff, walls, circulation, districts and valley.
2. **Lower district / hero district:** two isolated functional buildings and tiny supports cannot read as a city or lived-in production scene.
3. **Terrain transition:** the height-field can form one collision-free visual mass, but its smooth mesa and bare slopes expose a hard quality gap against the authored hero rock.
4. **Architecture family:** the tested large ruin is a rectangular brick slab beside a finely authored Gothic Bastion. Removing it improves focus but leaves the monumental frame missing.
5. **Circulation:** the new stone strip clips through changing elevation; paths/stairs need authored stone transitions tied to terrain and actual hero landing.
6. **Material/texel coherence:** hero limestone, supporting masonry, wood and PBR ground differ strongly in detail scale and response; shared maps alone do not unify geometry.
7. **Background/atmosphere:** the naturalistic Kiara photograph is more detailed than the 3D foreground; hard terrain edges reveal the compositing boundary. The broad rear ridge hid this issue rather than solving it.
8. **Scale, activity and 4X/mobile readability:** support structures appear either tiny or oversized relative to the Bastion, and the mobile crop becomes a castle over a blank earth wall. Human activity and functional civic spaces are absent.

### Decision

**B, with a concrete bottleneck; Visual Shell v2 itself is a visual fail.** Unity 6/URP and the controlled camera can instantiate a terrain/material shell without changing gameplay, so these runs do not establish a renderer or pipeline ceiling. The missing production ingredient is an **authored, camera-checked rock/terrace/retaining-wall/circulation connector and a coherent mid/lower architecture family**. A sampled height field, prefabs on top, and photo-matched patches do not provide that shape language. The current asset library has high-quality hero art and some supporting modules, but the tested combination does not bridge the gap. Tripo has not been justified by a uniquely specified geometry gap, and no credit was spent.

Stop this procedural-height-field route after two full-frame captures. The next block should begin with a 19/12/9/mobile image-space paintover against the approved benchmark, author a compact Blender mesh/terrain transition for the exact Bastion-to-civic silhouette (with rock/stone masks and buried seams), then re-evaluate the selected existing architecture at a common texel scale. Keep all gameplay authority and reject the block unless the complete frame visibly improves. This is a change of method, not an invitation to continue moving or scaling prefabs.
