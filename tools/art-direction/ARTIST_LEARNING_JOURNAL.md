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

## Trial 2026-10-12 — coherent geology V4/V5 in the actual Unity renderer
- V4 Windows Blender authoring run [#38256707677](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38256707677): SUCCESS. Unity isolated identical-camera comparison [#38257700135](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38257700135): SUCCESS technically.
- V4 fixed the disconnected bright-white soil face from V3, preserving a measured zero-gap rock/grass contact. The Unity image nevertheless showed conspicuous repetitive limestone stripes and rectangular module endings. ART FAIL.
- V5 source pipeline [#38260070066](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38260070066) PASS, artifact #11752307439, GLB + .blend + render + proof; independent Unity check [#38260643740](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38260643740) PASS; full-zone Unity integration [#38261014419](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38261014419) PASS, artifact #11752686342.
- V5 noticeably reduces repeated stripes using multi-scale noise and UV distortion, but still has a squared-off modular end, low-interest top cap, and flat surrounding stage. Its technical seam, import and scene integration are proven; **quality gate NOT approved for final Valoria, mobile profiling NOT done**.
- The editor first-zone has its own interaction anchors and camera preview controls, kept isolated from Bastion I-II domain progression. Their script .meta GUIDs must match the specific serialized scene artifact; mismatches silently lose components when read in a later CI checkout. See the dedicated WebGL candidate build/serialization recovery.
- Next meaningful art advance: build connected adjacent rock modules into continuous bank, deliberately hide terminal cut faces at landscape joins, build proper vegetation/erosion contact and revised playable camera framing. Do not spend more cycles microadjusting noise without scene-level A/B.

## Technical learning — typed terrain editing and live local Qwen, 2026-10-12

- [#38317914432](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38317914432): real Linux Blender 4.0 **TECH PASS** with an isolated synthetic mesh: inspected two source boundary rings, used an explicitly authored JSON plan to bridge 4 quads, spatially sculpt 8 vertices, export valid .blend and GLB, verify source remains frozen and the output geometry hash changes. **NOT Qwen authorship, NOT game art, NOT Unity visual approval**. Export initially failed because system Python lacked NumPy for glTF; installing `python3-numpy` fixed it.
- Frozen V5 source artifact [#38260070066](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38260070066) includes a Blender 5.2 Zstd-compressed `.blend` that Linux Blender 4.0 cannot open directly. Safe recovery: preserve both frozen archives, import the matching source V5 `.glb` into a **derived .blend**, hash the origin, then inspect the actual mesh. [#38318821345](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38318821345) verified this conversion and a real Unity screenshot was available.
- Real Ollama Qwen3-VL 4B **not yet an authorship success**: [#38319134244](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38319134244) rejected the uncompressed two-image briefing: **9,915 prompt tokens > 8,192 context**, HTTP 400 `exceed_context_size_error`. Source/images/model were found and Ollama was reachable. Corrective implementation: provide actual AABB and vertex anchors in mesh context; raise request context to `num_ctx=16384`, limit expected output to 2,000 tokens, record bounded local HTTP diagnostic. Follow-up [#38319450217](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38319450217) underway at time of note; await genuine `artist-inference.json` / executed geometries before declaring authoring PASS.
- Artist gate remains **ART PENDING / UNITY BASTION I NOT REVIEWED / MOBILE NOT VALIDATED**. Do not confuse a geometry contract smoke test, old V5 Unity captures or Qwen API connectivity with a new visible artistic result. Prior V5 block-like silhouette is still below the reference.

## Qwen 2B authentic decisions and typed repair — Issue #34 (2026-10-13)

- Run [#38323053630](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38323053630), artifact **11779298589**: actual Qwen3-VL 2B analyzing two real-image derivatives and real frozen V5 cliff; response in **96.518 seconds**, **365 output tokens**, 5,125 prompt tokens, syntactically valid JSON with `asset_decision=REUSE`, `sculpt_region` selected autonomously at object-local coordinate `[-5.8125,-2.888855934,-1.998239398]` and radius 0.5, but `delta=[0,0,0]`. **Preflight failed as required; no changed mesh, no artistic approval.** Other issue: rationale claims original boundary loop although candidate does not choose any certified loop. A complete JSON or nonzero number of tokens does not demonstrate artistic edit.
- Distinct from prior 1,500-token invalid face-index flood at [#38321583269](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38321583269) (artifact **11779531956**, model erroneously chose NEW for frozen cliff): permanent response schema now restricts EXISTING cliff to existing-mesh sculpt actions and prevents fictitious mesh reconstruction.
- Implemented typed geometric safety in `artist_plan_validation.py`, rejecting zero vectors, single identical knots, absent loops, centers beyond authentic mesh local AABB, and unauthorized NEW source changes. `author_brief.py` now preserves every real model response SHA and provides **one explicit rejection/revision round to Qwen itself**; scripts never invent replacement displacement, design or approval. Unit tests model real zero-delta failure. Governance [#38323696703](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38323696703) SUCCESS for the new contract and future art asset guard.
- Bastión I separate-owner consumer [#38323763756](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38323763756) was active at report time. Only its owner may change scene or use Windows; import plus matched Unity A/B still required, and any apparent Blender screenshot PASS is not independent art approval.

## 2026-10-13 — Qwen-authored Bastión I terrain: structural TECH PASS / camera VISUAL FAIL

Evidence: real dual-image Qwen3-VL 2B inference and Blender edit in [run #38323534537](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38323534537), artifact `11779922676`. AI authored `sculpt_region` on real hilltop terrain: center [-40,57,-21.6243], radius 10.24 m, nonzero delta [0.5,0.5,0.5]. Executed on 181 original vertices; mesh source/result SHA256 differ; .blend and .glb valid. No scripted geometry parameters were substituted. **Reject artistic result**: same-camera 640×480 clay A/B changes just **29/307200 raw pixels** and **7/307200 pixels with channel difference >2/255**. This fails player-camera visibility; terrain remains visually unchanged. Blender technical authoring does NOT mean aesthetic improvement and never ART_APPROVED.

**Corrective artist lesson for NEXT inference (important):** choose a camera-visible **terrain silhouette, slope-crest, rock mass or join**, NOT an extreme mesh AABB corner or tiny lower skirt; validate the proposed center and displacement against actual camera-screen projection before editing. Prefer designing a recognizable, asymmetric rocky mass/stratified profile or a meaningful visible terrain transition, while leaving usable construction parcels/bridge/gate/path untouched. If a trial fails the visibility gate, request NEW Qwen-chosen coordinates and form, not a canned synthetic displacement. Re-export/inspect in authentic Unity matched camera; Blender clay A/B alone is not final visual acceptance.
