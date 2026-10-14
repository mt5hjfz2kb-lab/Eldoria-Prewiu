# Eldoria artist — permanent artistic quality gate in the ORIGINAL studio

Authoritative order: `main/AGENTS.md` and `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`. Re-read at the start of every artist session.

This is the **same existing Blender/Qwen full-studio**, not an alternative generator.
The study uses original frozen Valoria tower SHA, actual reviewed Blender
source camera and geometry, artist-selected ops, Qwen's genuine past-memory
context and the unchanged source/BLEND/GLB + PNGs.

## Mandatory progression

1. **TECH_PASS** means a genuine Blender run and source QA, never art acceptance.
2. Each visually relevant new Qwen cycle issues
   `cycle-N/art-review-request.json` with SHA of *exact actual submitted
   messages+images+schema*, original owner reference, input asset, actual
   model-authored plan, original frozen tower, output BLEND/GLB, review
   camera specification and genuine native Blender before/after PNGs.
   This artifact is only `ART_REVIEW_REQUIRED`. The Qwen self-critique
   is explicitly not an independent reviewer.
3. A separate agent/person must inspect the **exact actual PNGs**,
   source composition, roof/stairs/door/atlas continuity and reference.
   Supply a separate JSON review at
   `--independent-reviews-dir RUTA/cycle-N.json` containing:
   `review_kind="INDEPENDENT_BLENDER_VISUAL_REVIEW"`,
   `independent=true`, independent `reviewer_id`, `accepted=true`,
   exact SHA strings copied *from the genuine review request* for
   `original_asset_sha256,input_asset_sha256,output_asset_sha256,
   qwen_plan_sha256,reference_sha256,before_png_sha256,after_png_sha256,
   review_camera_spec_sha256`; 0..10 `before` and `after` scores
   for `geometry,architecture,materials,style,camera`; independent
   `diagnosis,observed_improvement,protected_parts_impact` of >=24
   meaningful characters each; `protected_regions_ok=true`,
   `severe_regressions=[]`.
   A file is **not** visual truth by itself. External human/agent needs to
   view/compare source images. Formal technical verification cannot judge taste.
4. If no reviewer decision exists, the runner **stops after producing the
   pending candidate**; it never reuses pending output as the next Qwen source.
   A genuinely reviewed improvement >=0.25 weighted with no dimension down
   >0.5 and no protected damage can become `BLENDER_VISUAL_ACCEPTED`
   provisional. Store `independently-accepted-chain.json` and the exact
   accepted BLEND; next run uses this BLEND as `--source`, explicitly sets
   `--frozen-original-asset-sha256` to the immutable **first tower** SHA
   and passes `--accepted-chain`. Previous accepted output SHA must equal
   next input SHA; no rejected candidate is allowed to become the next base.
5. Two rejected plans of the same geometric family: switch hypothesis,
   not just bevel width. Five complete failures: produce strategy diagnosis.
   Never assert learned neural-network weights; historical cases are
   independently grounded retrieval memory.
6. The **formal** status remains `CALIBRATION_IN_PROGRESS` even after
   three Blender improvements, until final score >=8 and the exact final
   Unity camera A/B and independent decision exist and
   `pipeline/art-visual-approvals/*.json` passes **the unchanged**
   `tools/art-visual-acceptance/verify.mjs`. The current Unity-world/game
   production pause stays active. Do not manufacture Unity footage, reviewer
   scores or fake accepted manifests.

## Automated gates already added

- Linux Blender studio CI: full Python regressions, real camera-ray hit,
  26 Blender operations; unchanged canonical
  `node --test tools/art-visual-acceptance/verify.test.mjs` and
  `node tools/art-visual-acceptance/verify.mjs --directory pipeline/art-visual-approvals`.
- Native Windows artist workflow: same JS verifier before spending local
  Qwen cycles, real GLB and fixed camera, original source SHA checks,
  and honest nonzero failure when no reviewable artifact exists.
- Each artist invocation records exact SHA of what was actually sent to Qwen.
  `artistic_quality_gate.py` compares all camera/A/B/source/plan hashes to
  an independent review and produces a provisional score-linked source chain.
- A candidate with visible pixels but no independent comparison never
  becomes ART_APPROVED; a Qwen positive self-review never promotes it.

Current baseline: run #38649450100, original source
`e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
Its four Qwen8B extrudes changed 0.193–0.360% of the camera, all
`SCREENSPACE_REJECTED`. **0 accepted consecutive visual interventions**.
The viewer-diagnosed repeated-invisible-extrusion lesson was added to the
existing `learning_cases.json`; stop reusing that hypothesis.
