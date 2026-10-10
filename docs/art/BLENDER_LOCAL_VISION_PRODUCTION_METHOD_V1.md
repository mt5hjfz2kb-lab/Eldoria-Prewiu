# Eldoria — Artistic Brain / Blender Local-Vision Production Method (candidate v1)

Status: **documented and preserved in a dedicated integration branch; NOT merged into main or certified as production default**.
Owner: art production. Game source and existing Unity pipelines remain untouched.
Date: 2026-10-10. Cost policy: EUR 0, no paid API or Work credits.

## Purpose and proven scope

Preserve the validated free workflow for converting reusable medieval architecture into Blender-authored, inspectable GLB modules. Avoid rediscovering local BlenderMCP connectivity, Ollama/Qwen3-VL inference and artifact export in each new chat. This is a method and evidence registry, **not a claim that visual quality or mobile runtime was certified**.

Canonical game: Unity 6000.3.23f1, `main`. Active experimental art branch: `prototype/valoria-bastion1-no-sharp-20261010`; do not edit or merge it casually, especially while another art agent owns it.

## Verified evidence on the art branch

- BlenderMCP transport proof: GitHub Actions run `38084633869`, SUCCESS.
- Free local Ollama/Qwen3-VL bootstrap and lookdev: `38085464538` and `38085802464`, SUCCESS. Earlier material-only loop did **not** pass visual acceptance.
- Local model directing a real BlenderMCP geometry change: `38087083180`, SUCCESS; accompanying result explicitly marked `visual_pass=false` and `unity_tested=false`.
- Architecture source inventory and variants: `38085968881`, `38086314486`, SUCCESS.
- Modular fortification Blender export: `38088678550`, SUCCESS on commit `c170f61df442a40345576ce8ef119e6af9b6421b`, artifact `11683770915` (name `valoria-modular-fortification-family`).
- Human/assistant visual comparison: preliminary improvement around 4/10 to approximately 5–5.5/10; **subjective, not commercial quality certification**. Art-agent report: 6 individual modules + one assembled GLB, 7/7 GLBs structurally inspected, assembled model ~32,120 triangles and 15 meshes. Mobile performance remains unverified.

## Reusable code and workflow locations (experimental branch)

- `tools/art-rd/blender_mcp_tcp_proof.py` and related `blender_mcp_*probe*.py` — transport.
- `tools/art-rd/eldoria_blender_vision_loop.py` — rendered image / local vision analysis loop.
- `tools/art-rd/eldoria_mcp_geometry_agent.py` — local vision-to-geometry MCP execution.
- `tools/art-rd/eldoria_curated_architecture.py` and `eldoria_fbx_candidate_probe.py` — examine existing architecture assets.
- `tools/art-rd/valoria_fortification_family.py` — modular generation / GLB authoring.
- GitHub Actions files under `.github/workflows/` matching `valoria-*`, including local Blender vision, MCP geometry, curated architecture and modular fortifications.

Files above belong to the experimental art branch; existence here does not imply they are on main.

## Standard artistic decision loop (candidate)

1. Read latest branch state, job logs and artifact manifest before touching any files. Preserve working results and the active art owner's scope.
2. Pick **one** representative architectural asset (gatehouse first). Establish quality targets with defensible medieval building references and mobile-size camera views. Check legal reuse/license of free PBR materials and donor assets.
3. Use curated existing FBX/GLB assets if they already provide superior geometry. Employ procedural modeling for functional modular connections; use BlenderMCP for controlled, limited geometry/material edits. Do not equate block proliferation with detail quality.
4. Produce reproducible `before` render and then `after` with **the same camera, lighting, exposure and resolution**. Capture isolated hero shot, assembled context, and small on-screen-size view.
5. Use local Qwen3-VL via Ollama to propose specific corrections; treat AI feedback as suggestions requiring human/visual verification. Apply a bounded edit via BlenderMCP, render again and compare against visual criteria.
6. Check: silhouette, construction logic, radial stonework, connectors, door/wood/iron construction, PBR stone/wood/metal, irregular weathering, palette appropriate to dark medieval Valoria, repeated patterns, legibility at phone scale. Reject regressions.
7. Export actual GLB. Verify file contents, materials, normals, pivots, dimensions, joints and assembled contacts. Record mesh and triangle counts as metrics, **not proof of performance**.
8. Save PNG comparisons, scene source and GLBs as CI artifacts. Record exact commit SHA and Action run/artifact IDs.
9. Only consider Unity import after visual acceptance. Confirm shader compatibility, texture memory, material/draw-call counts, LOD, batching and mobile GPU/frame-time on representative hardware. Do not claim mobile-ready from Blender render alone.

## Acceptance gates

- **Technical gate:** Blender Action SUCCESS; all expected GLBs export and parse; matching render artifacts; structural connectors and scene assembly valid.
- **Artistic gate:** a visibly credible improvement in identical before/after views, detail appropriate for actual in-game screen size, historically/plausibly constructed surfaces and high-quality PBR. A metric based only on file size, pixel deltas or mesh consolidation does not pass.
- **Mobile game gate:** Unity integration and hardware performance verified without changing gameplay. Separate approval before merging any art assets or workflows into `main`.
- A code change, successful pipeline execution or model vision confidence is **not** a release gate.

## Operational constraints and known failures

- Runs `38086443300` failed for missing `after.png`; `38087083180` later succeeded. Preserve evidence artifacts.
- Export runs `38088567416` and `38088584605` failed; fixed unavailable OpenImageDenoise in Ubuntu Blender runner via `c170f61df`; `38088678550` SUCCESS.
- Earlier export failures also required NumPy installation and renderer configuration. Do not retest historical failures unless new evidence demands it.
- No parallel edits to art branch without coordination. No paid cloud credits. No unreviewed downloads/assets with unclear licensing.
- Do not build new departments, overwrite main game source, disable required quality controls or publish raw prototypes to the playable build.

## Integration roadmap (not yet executed)

1. Preserve this method and evidence (this branch).
2. Improve one gatehouse reference asset to a convincing visual standard, using local free tools and inspectable before/after renders.
3. Identify **exact** canonical art-brain entrypoint(s), production request schemas and workflow owners on current main; propose the smallest safe changes in a PR (do not impose unvalidated procedural generation as the default).
4. Review and merge method/documentation only when consistent with existing main governance.
5. Adopt new workflows and assets into main **only after** art acceptance, isolated CI, Unity mobile import and ownership approvals.

## End-of-task reporting

Report independently (a) technical success, (b) artistic success, (c) Unity/mobile success, and (d) merge/adoption status. Provide links to commit/run/artifacts and any blocked next steps. Never mark all stages successful merely because Actions returned SUCCESS.
