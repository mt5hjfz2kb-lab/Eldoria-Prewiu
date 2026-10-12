# Eldoria local artist — empirical learning journal

This journal is consulted before future artistic authoring. A model cannot learn new weights from an Ollama conversation; persist learning as versioned evidence and updated tools.

## Trial 2026-10-12 — cliff_talus_transition_v1

- Branch: `art/local-ai-professional-artist-role-20261012`
- Run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38231642143
- Artifact ID: `11738373984` (temporary CI artifact)
- Qwen: `qwen3-vl:4b-instruct`, direction.json with system-profile SHA `1e7d44e4da4388b24dfe2016f45b774064143426929874d6b05e6cc5e8719697`
- BlenderMCP: actual remote execute_code operation succeeded; 2813 bedrock vertices modified, max displacement 0.89m, 42 existing talus meshes adjusted
- Outputs: `cliff_talus_transition_v1.blend`, `cliff_talus_transition_v1.glb`, before/after Blender captures
- Validity: TECHNICAL BLENDER PASS; ART FAIL; UNITY UNTESTED; STANDALONE MODULE UNPROVEN
- Human inspection: before/after still displays a large rectilinear slab with a unnaturally flat upper cap. Side geometry looks subtly varied but major silhouette, geology and integration did NOT meaningfully improve. The exported GLB includes the parent cliff bedrock and talus: this is a cliff variant, NOT a standalone reusable talus transition asset.
- Root-cause correction: Do not iterate on fault amplitude or facet count alone. Design an **independent blended contact module**, small silhouette-motivated rock clusters/soil wedge with irregular erosion, material and size inherited from target V3, with explicit placement anchor and clear transition. Compare Unity scene under identical camera; prove the new module is standalone and improves contact with a wall/cliff.
- Never label this run ARTISTIC BREAKTHROUGH or ACTIVACIÓN SUPERADA. Quality is below the intended standard.

## Next gate
1. Confirm loaded system prompt is drawn from the versioned artist profile, not an unrelated hardcoded persona.
2. Author a truly separate transition module informed by Qwen and export only that module.
3. Import into Unity in isolated scene and capture paired real before/after.
4. Independently evaluate visual quality and mobile impact; record result here.

## Trial 2026-10-12 — standalone talus apron plus Unity comparison
- Production run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38232756248
- Unity run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38234122670
- Art artifact 11739820971, Unity artifact 11739318712 (both temporary)
- Actual model `qwen3-vl:4b-instruct` generated an artistic direction grounded in reference and real Unity frame; code loaded permanent role text from versioned source (compact excerpt sent as system instruction).
- BlenderMCP verified success, output genuinely *separate* 6-mesh candidate GLB with geological apron plus 5 talus boulders, SHA256 `43ac21f826dcd7b39d2dfa37a33ec448d64378a1c9390a5a5cb1be9b6b31ce4a`. Original approved cliff unchanged.
- Unity 6000.3.23f1 Windows successfully imported GLB, rendered same-camera before/after, verified PNGs and manifest. TECHNICAL END-TO-END PASS.
- **Visual rejection (not commercial art pass):** after image shows apron and boulders at base, but overall cliff is still an obvious rectangular block. Additionally cliff-face material appears green/brown before, bright grey/white after, so geometry-only A/B is confounded by material/occlusion and needs diagnosis. Do NOT promote into main or production.
- Future correction: independent artist direction should author less uniform talus silhouette and explicit ground-edge anchor geometry. Capture Unity comparison with material isolation and same original renderers; inspect shading/material side-effects before evaluating aesthetic benefit. Validate import GLB normal/texture/collision/LOD mobile cost separately.
- Final verdict: professional persona and local Qwen -> BlenderMCP -> Unity **proven in isolated trial**; default artist role across canonical production workflows **NOT YET ACTIVATED**; asset remains a candidate for redesign, not approved.

## Trial 2026-10-12 — full local Windows artist authoring without Work
- GitHub Actions run #38240877892: **SUCCESS** across local persistent Ollama `eldoria-artist` → reference-guided inference → BlenderMCP edit → Blender 5.2 → standalone GLB + editable .blend + before/after PNG.
- Candidate GLB SHA256 `f8a2412937e5ce05cb2688a4a71a9c75a8357684dfa4a2a422034cabdda2bca5`; source approved and unchanged. Artifact #11742524534, temporary and must be archived.
- Evidence states 2813 bedrock vertices modified; separate GLB exports six new objects (talus apron + five rocks). This is a **Windows technical authoring PASS**, not an artistic PASS.
- Direct Blender capture still shows a rectangular parent cliff and flat turf cap. The standalone exported module is not a full cliff replacement. Before/after Unity visual review launched by `.github/workflows/eldoria-windows-authored-talus-unity-review.yml`; do not approve for game until inspected.
- Correct lesson: local autonomy of technical pipeline is established; further progress needs measurable high-quality geometric silhouette changes, grounded terrain contacts, materially controlled paired renders, and mobile performance checks. Do not claim trained/fine-tuned model weights or substitute output count for visual quality.

## Windows local artist Unity visual review — 2026-10-12
- Real Windows Unity import run [#38246640258](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38246640258): technical SUCCESS; artifact #11746602965. Candidate GLB from Windows production #38240877892 and frozen cliff import verified.
- Real Unity matched-camera before/after screenshots: added geometry forms a broad, flat grey apron and isolated bright white boulders; the parent cliff remains rectangular. Material/light differences confound perceived improvement. Manifest explicitly reports `visual_pass: false`, `mobile_pass: false`. **ART FAIL: do not promote to game.**
- Corrective quality gate added in `ValoriaLocalTalusUnityCapture.cs`: neutral-clay before/after renders under identical lighting/camera for *both* assets, independent from their differing glTF textures. Await results before diagnosing geometry alone. Do not repeat fault-amplitude-only cycles.

## Controlled neutral-material Unity verdict — 2026-10-12
- Run [#38247754055](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38247754055) SUCCESS for **technical** Unity capture; four distinct paired PNGs and talus-only. Artifact #11746444144 (temporary).
- With identical neutral URP Lit clay on original cliff and added talus, **before/after difference is confined to thin apron and several separated boulders at the foot**. Flat rectangular grass-top silhouette remains geometrically unchanged. The after model does **not** achieve meaningful improvement of dominant cliff form.
- Original imported textured comparison also changes green/brown cliff material to bright grey/white rock/talus appearance, making uncontrolled A/B unsuitable for artistic acceptance.
- Independent visual verdict: **ART FAIL / do not promote**. Genuine technical capability of local Windows artist proven; art direction needs a *different modeling approach*, not higher fault amplitudes, more small boulders, re-colored stone, or lighting-only improvement.
- Next controlled brief: reconstruct the visible terrain/cliff silhouette itself as coherent, modular eroded geology with non-rectilinear upper profile and integrated terrain continuity. Start from approved reference, preserve measured join anchors and engine/mobile budget. Judge matched-camera clay and textured renders before any adoption into the persisted first zone. No automatic modification of `main`.

## Coherent cliff V5 — measured Unity review (2026-10-12)
- Authored without Work: [production #38260070066](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38260070066) SUCCESS; [matched Unity review #38260643740](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38260643740) SUCCESS; artifact #11752208221.
- A/B visually inspected in four genuine Unity images. Relative to the frozen cliff, V5 replaces the block's regular skyline with broad eroded undulations and distinct continuous rock/grass regions; rock strata are more readable. This is a **meaningful geometric improvement** over prior talus-only approaches, not a purely material change.
- Remaining blockers: extrusion still looks like a roughly constant-thickness 12m module; the grass cap has parallel striations and flat depth, side ends are conspicuously planar and the rock is not yet joined convincingly to the first-zone environment. Mobile budget and gameplay collision **not validated**.
- Decision: **candidate retained, technical pass, art approval pending**. Do not silently promote into `main`. Next goal is a scene-level first-zone contact/scale review and a publishable preview viewer, with the V5 and the frozen original kept as separate comparison alternatives.
