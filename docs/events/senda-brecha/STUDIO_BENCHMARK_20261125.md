# Studio benchmark — 25 November 2026

## Scope
Canonical draft PR #195 only; no merge, integration, production economy, saves, or beta changes. Baseline native commit cb0193132fc6ed67c19f616f7fa9facf1155b79e, proof [43732245691](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43732245691). Web baseline proof [43731262934](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43731262934). They are distinct implementations and must not be conflated.

## Implemented
- Five-role Studio mission: artistic director, original crest generation, implementation recommendations, auditor and coordinator. The generated crest is an original raster UI element, not a production 3D mesh. Coding, testing and integration were performed by ChatGPT, not claimed as autonomous outputs of the five roles.
- Web: road lane separation, physical portal legibility, animated soldier geometry, corrected screen-left/right controls, gate label reset, reduced violet lighting, original crest. Rules retained.
- Native: isolated procedural low-poly scene restyling with textured road, gate design, articulated soldiers, restrained violet lighting and camera. Native proof is Built-in pipeline, not production URP.
- Web optimization: instancing shared geometry, individual frustum culling; proof of a maximum of 350 draw calls per tested viewport required before claiming success.

## Evidence
- Studio runs [45693242088](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45693242088) (output limit, failed) and [45693467813](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45693467813) (5 successful roles). Receipt archive [45694172796](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45694172796).
- Web functional pass [45694564134](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45694564134), exact commit 0f9895a3cc857a224557d43942c52561d4085898. Four viewport sizes; victory, defeat, pause, replay, direction and gate label reset. Not physical iPhone.
- Optimized attempt [45695143327](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45695143327) failed in clip capture after gameplay finished; deterministic clip fix submitted. Final proof [45695680165](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45695680165) **PASS**, source `1ebfadaf2a6f86335d44130712a40c4c7751384e`, artifact 15060296973. All four viewport functional gates passed, errors empty, victory/defeat/pause/replay true. Mobile draw calls 1060→118 (88.87% reduction); small 978→118; landscape 1029→121; desktop 1104→121. This is a draw-call metric, not measured physical-device FPS. Isolated playable URL: https://raw.githack.com/mt5hjfz2kb-lab/Eldoria-Prewiu/1ebfadaf2a6f86335d44130712a40c4c7751384e/tools/senda-brecha/visual-3d/index.html (source verified locally by Chromium CI; external host not separately tested).
- Native recheck [45694055440](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45694055440) on exact source 130e73e58dc1a0d5f2d5f53cd3732484cfc3b33b.

## Cost and limits
Additional owner-authorized ceiling $10; $5 tranche, $2.90 reserved, **$0.09172975 estimated actual API usage** (failed first call $0.0092055 plus successful mission $0.08252425). Six real calls. No more automatic billable triggers. Does not include ChatGPT subscription, local hardware electricity or runner costs; invoice not verified. Cumulative Studio estimate $0.320286975 for 25 calls. The initial output-limit failure was corrected with concise prompts and larger output budget.

The real before/after WebGL images show visible improvement but still low-poly, not commercial-grade. Do not claim artist approval, validated iPhone, production URP integration, or measured productivity gains from this one uncontrolled benchmark. The next quality gap is purpose-built animated medieval character assets and combat/environment art. 
