# VALORIA VISUAL R&D DECISION REGISTER v1

Status: CANONICAL AUDIT MEMORY
Date: 2026-10-06
Rule: repository evidence overrides chat memory. A green CI run is not a visual pass.

## Purpose
Keep a durable record of which visual-production methods were tested, what they actually proved, what remains reusable, and what must not be reopened without materially new evidence.

## Decision classes
- **LEADER**: strongest current route toward the final production pipeline.
- **COMPONENT PASS**: useful as one layer of the pipeline, not sufficient alone.
- **BASE 3D PASS**: usable volumetric donor/blockout; not final art.
- **TECH PASS / VISUAL PARTIAL**: technically functional but below final visual target.
- **BLOCKED**: not evaluable because of infrastructure/access; not a visual rejection.
- **REJECTED**: evidence shows the method should not be used as the main production route.

## Current production hypothesis
**LEADER — Hybrid Valoria pipeline**

Target reference / semantic isolation
→ high-fidelity visual representation where useful
→ real 3D geometry for interaction, depth and occlusion
→ human/professional geometry correction where generation invents forms
→ appearance transfer / projection / PBR
→ photogrammetry or high-quality donors for surface relief
→ Unity 6 URP runtime, camera, interaction, gameplay and optimization.

No single tested generator currently satisfies all requirements alone.

## Methods

### SHARP Gaussian full-frame
**Status:** LEADER / CLEAN 600K UNITY PRODUCTION BASELINE PASS
- Run: 37448896124
- Artifact: 11404383856
- Output: ~64 MB Gaussian PLY + synthesized-view video.
- Proven: strongest full-frame appearance retention observed so far.
- Unity Gate 1 run 37455323684 / artifact 11409152206: bounded 294,912-splat representation renders successfully inside Unity 6 URP.
- Clean production baseline: run 37456541286 / artifact 11409359136, generated from clean approved reference artifact 11408853949 at 589,824 splats. HUD-free HOME/pan/zoom and raster front/behind depth coexistence pass.
- Proven in Unity: HOME render, bounded left/right pan, raster front-probe visibility and behind-probe occlusion.
- Remaining unknowns: clean source without baked HUD/UI, final density/performance envelope, mobile runtime budget, interaction/hotspots and wider camera motion.
- Workstream: `valoria-sharp-gaussian-pipeline-v1`.
- Current governance: execution stalled after scope expansion to Unity; this is not a technical rejection.
- **Next gate:** isolated SHARP PLY → Unity 6 URP proof.
- Do not promote to production before that gate.

### TRELLIS on isolated semantic Lower Gate
**Status:** BASE 3D PASS
- Runs include: 37449196984
- Artifact: 11405445323
- Proven: semantic isolation improves shape coherence versus raw crop; real GLB and multi-angle evidence.
- Limitation: architecture still simplified/deformed; insufficient as final hero asset.
- Reuse: volumetric donor/blockout.

### InstantMesh
**Status:** BASE 3D PASS
- Proven: closed recognizable mesh; ~154k faces in inspected proof.
- Limitation: melts upper silhouette/details and loses architectural identity.
- Reuse: fast volumetric base only.

### Hi3DGen
**Status:** TECH PASS / VISUAL PARTIAL / HIGH SEED VARIANCE
- Canonical proof run: 37449400507, artifact 11406435313.
- Semantic proof run: 37450000430, artifact 11406576490.
- Seed bake-off run: 37450920927, artifact 11406558410.
- Proven: dense watertight geometry and improved architecture with semantic input.
- Seed finding: seed 7 preserves a substantially more coherent gate/tower/arch structure; seed 137 breaks the architecture severely. Quality is therefore highly seed-dependent.
- Limitation: false holes/cuts, invented depth, unwanted attached ground in some variants; reproducibility is not acceptable for automatic production.
- Reuse: volumetric donor or fixed-seed isolated source only. Do not promote as a general production generator without a deterministic quality gate.

### Hunyuan3D 2.1
**Status:** TECH PASS / HIGH VARIANCE / NOT PRODUCTION-SUITABLE
- Semantic success run: 37449805027, artifact 11405242167.
- Second semantic success: 37449955466, artifact 11405177537.
- Octree-512 run: 37450709536, artifact 11406667481.
- Proven: can return real GLB and sometimes preserve gate/tower/arch concept well.
- Critical issue: strong run-to-run variance; inspected repeats degrade heavily.
- Octree 512 does not solve the visual problem: the front view remains melted/soft with invented or collapsed structural regions.
- High-resolution attempt also failed technically.
- Reuse only as occasional volumetric donor. Do not treat a single good run as production proof.

### CraftsMan3D
**Status:** BASE 3D PASS / VISUAL BELOW TARGET
- Proven: dense watertight mesh (~424k faces in inspected proof).
- Limitation: rendered architecture remains soft/melted; density did not translate to fidelity.
- Reuse: coarse volume only.

### Human-Mesh Projection Bake
**Status:** COMPONENT PASS / VISUAL PARTIAL
- Run: 37448457610
- Artifact: 11404606995
- Proven: human-authored mesh preserves real 3D parallax and can receive target-conditioned appearance.
- Limitation: current transfer too dark/superficial for final identity.
- Reuse: strong hybrid component.

### Geometry-Anchored Projection Bake
**Status:** COMPONENT PASS WITH CONDITION
- Proven: tight-crop projection improves target appearance on real geometry.
- Limitation: duplicate/overlapping architecture appears if base geometry is not well aligned.
- Rule: appearance transfer comes after geometry alignment, never as a substitute for it.

### PBR derived from target
**Status:** COMPONENT PASS
- Proven: generated BaseColor, Normal, Height, Metallic and Roughness maps.
- Limitation: isolated material result does not reproduce Valoria architecture.
- Reuse: material/lookdev layer on a correct mesh.

### Photogrammetry donor
**Status:** COMPONENT PASS / SOURCE SELECTION PENDING
- Proven: real surface relief, joints, chips and wear can be transferred from donor geometry.
- Limitation: tested donor was visually wrong for Valoria (modern/red brick).
- Reuse: medieval stone/rock donor layer only after better source selection.

### CC0 / human-authored modular geometry
**Status:** COMPONENT PASS / GENERIC
- Proven: robust editable professional geometry pipeline.
- Limitation: tested free sources were too generic for Valoria identity.
- Reuse: structural donor/correction pieces only.

### KINGDOM commercial modular castle
**Status:** BLOCKED — OWNER ACQUISITION REQUIRED
- Observed price: USD 74.50 + tax.
- No purchase authorized.
- Do not buy while zero-spend routes remain informative unless owner gives fresh explicit authorization.

### TripoSR
**Status:** IN TECHNICAL PROBE
- MIT/open-source route chosen as last reasonable single-image reconstruction check.
- First attempts failed before generation due torchmcubes build dependencies.
- Failures so far are infrastructure/install failures, not visual evidence.
- Rule: allow only a bounded final technical correction; if geometry remains below hero threshold, close the single-image reconstruction family.

### Unique3D
**Status:** BLOCKED
- Run: 37448430078
- Artifact: 11404088202
- ZeroGPU requested ~600s GPU and did not return usable mesh.
- Infrastructure blocker, not visual pass.
- Do not reopen unless execution conditions materially change.

### Hunyuan initial ZeroGPU path
**Status:** BLOCKED / SUPERSEDED
- Earlier public route required ~270s GPU and was blocked.
- Superseded by later executable Hunyuan3D-2.1 experiments above.

### SF3D
**Status:** REJECTED FOR CURRENT FLOW
- Repeated upstream Gradio failures before usable model.
- With other generators producing real meshes, further retries have poor expected value unless upstream behavior changes.

### CityBuilder CC0 gate
**Status:** REJECTED VISUALLY
- Artifact rendered with broken/magenta materials and did not provide useful evidence above existing human-authored donors.

### Semantic 2.5D Camera-First / projection cards
**Status:** REJECTED AS PRODUCTION METHOD
- Final verdict already established: TECH PASS / VISUAL FAIL / CAMERA ENVELOPE FAIL / OCCLUSION-DEPTH FAIL / PRODUCTION SCALABILITY FAIL.
- Final gate: `docs/VALORIA_SEMANTIC_2_5D_CAMERA_FIRST_FINAL_GATE.md`.
- Do not reopen mask/inpainting/parallax/card-shell iterations without materially new evidence.

### Scripted Blender procedural reconstruction as primary authoring
**Status:** REJECTED AS PRIMARY ART METHOD
- Proven ceiling: geometry/silhouette and premium perception remained below target.
- Reuse only for utilities, automation, evidence rendering or support geometry.

## Current priority order
1. **Resume SHARP → Unity 6 URP integration proof.**
2. Finish reproducibility tests already running (Hi3DGen seeds / bounded Hunyuan variant).
3. Finish one bounded TripoSR technical attempt; then either visually qualify it or close single-image reconstruction.
4. Consolidate best real-3D donor + human correction + appearance/PBR path.
5. Do not open additional unrelated visual-generator lanes until the SHARP Unity gate is evaluated.

## Promotion rule
No method becomes production-authoritative from:
- a successful workflow alone,
- polygon count,
- a single good seed,
- a single frontal render.

Promotion requires evidence for the actual use case: target visual fidelity, multi-angle stability, editability, camera envelope, occlusion/depth, Unity/runtime compatibility, and scalability.

### Step1X-3D
**Status:** BLOCKED — PUBLIC SPACE PAUSED
- Run: 37451628769
- Artifact: 11406708574
- The public `stepfun-ai/Step1X-3D` Space reported state `PAUSED` before returning any usable mesh.
- This is infrastructure/access failure, not visual evidence.
- Do not spend time on retries unless the Space becomes available again.

### TripoSR
**Status:** TECH PASS / VISUAL FAIL FOR HERO ART
- Runs include 37450734133, 37450956954, 37451399945 and current pinned-NumPy rerun 37451859597.
- Installation blockers were progressively removed (torchmcubes build isolation, scikit-build-core, pybind11).
- Run 37451399945 reached actual mesh extraction successfully.
- Current remaining failure was GLB export compatibility: `trimesh` called removed NumPy 2.x `ndarray.ptp`.
- Current rerun pins compatible NumPy.
- Final evidence run: 37452367362, artifact 11407410847.
- Proven: complete CPU reconstruction → GLB → Blender multi-angle evidence works end-to-end.
- Visual verdict: FAIL for Valoria hero art. Front evidence is highly noisy and structurally broken; the gate/tower architecture is not preserved at a useful level.
- Reuse: none for hero production. Keep only as a technical reference that the open CPU pipeline works.
- Do not spend further cycles improving TripoSR for this target unless materially new model evidence appears.

### Marigold continuous depth
**Status:** COMPONENT PROBE / INTEGRATION UNPROVEN
- First run: 37451688012, artifact 11406893621.
- Public Marigold v2 Space and APIs were reachable, but the first wrapper returned no output.
- Rerun 37451930411 uses the correct Imageslider first-process API.
- Purpose: continuous depth representation/component, not final art.
- Do not promote until a valid canonical depth artifact is produced and its value to Unity/occlusion is demonstrated.

### SCoPE camera-controlled view synthesis
**Status:** VISUAL REPRESENTATION PROBE / IN PROGRESS
- Initial run 37451541486 failed because the wrong public Space identifier was used.
- Rerun 37451749211 uses the corrected Space and is currently generating camera-controlled video.
- Purpose: measure whether target identity survives controlled camera motion.
- Even a visual pass is not a gameplay/runtime pass; Unity interaction/depth still require separate proof.

### SHARP Unity integration gate
**Status:** TECH PASS / VISUAL PASS AT 300K / SCALE TEST REQUIRED
- Canonical SHARP source: run 37448896124, artifact 11404383856 (~1.18M splats).
- Unity convergence proof: run 37455174198, artifact 11409146782.
- Proven:
  - source PLY sanitation succeeds;
  - RDF (OpenCV/COLMAP) coordinate conversion is correct;
  - UnitySplats D3D11 CPU-sort lifecycle can be driven deterministically in batchmode;
  - bounded ~300k-splat working copy renders the canonical Valoria scene inside Unity 6 URP;
  - HOME and bounded left/right pan preserve scene identity;
  - front 3D probe renders in front; behind probe is hidden by nearer scene content.
- Visual caveat: 300k uniform sampling introduces visible sparse/punctate artifacts versus the full SHARP representation.
- This is the first proof in this R&D cycle that preserves near-target full-frame appearance inside the actual Unity runtime/editor pipeline.
- **Next gate:** repeat at ~600k splats with the same RDF + sanitation + explicit CPU-sort lifecycle. Compare fidelity, stability and capture cost. If stable, evaluate full density and runtime/mobile constraints.
- Do not reopen broad method discovery while this scale path remains viable.
