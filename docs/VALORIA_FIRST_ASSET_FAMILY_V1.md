# Valoria first modular asset family V1 — executable handoff

## Authority and isolation

Owner order 2026-10-11: produce modular wall, compatible watchtower, corner, architectural sockets/remates, hand-laid stone pathway/platform, credible cliff/terrain, and essential defensive props. Reject all earlier prototype-looking candidates until reviewed side-by-side against the **unchanged** Hero V3 gate.

Development branch: `art/valoria-first-asset-family-v1-20261011`. Frozen reference branch: `art/valoria-first-hero-asset-v1-20261011`. Neither `main` nor the original V3 mesh/materials are to be changed. No paid products.

## Reproduction using the actual existing tools

1. Work with the existing GitHub branch; inspect the live SHA and active workstream ownership before edits. The existing workflow `.github/workflows/valoria-first-asset-family-v1.yml` runs on an isolated Linux hosted runner, NOT the Windows Unity runner. The workflow triggers automatically on edits to its explicit source paths; see its exact list for current configuration. Avoid redundant workflow triggers and race-condition pushes.
2. Existing `tools/art-rd/valoria_fortification_family.py` provides historical geometry helpers, not a quality gate. Current family construction is `tools/art-rd/valoria_first_family_v1.py`, which preserves the gate and extracts a genuine right-hand V3 watchtower instead of creating another gate or using a primitive tower.
3. The cliff's sculpted continuous rock is supplemented from the earlier verified Eldoria art brain's **CC0** photogrammetry donor. The workflow downloads its historic OpenGameArt archive, validates the pinned SHA-256, obtains the existing studio's `extract_cc0.py`, and makes original albedo and normals available to `valoria_family_cc0_scanned_cliff.py`.
4. Generate actual Blender renders at close and strategic cameras in the `before` stage. `valoria_family_qwen_decisions.py` uses locally started Ollama/Qwen on three genuine images: **frozen V3 reference**, candidate close and candidate strategic. It requires an allowlisted semantic corrective art action per asset, exact structured JSON and descriptive image observation. Invalid answers are retried; raw traces are preserved instead of manufactured.
5. Render `after` sources, save **editable** `.blend` and `.glb` for each asset. The existing, SHA-verified BlenderMCP add-on (pinned upstream commit `7a0373ec9199183cb460068c4f96aed9c579fb4f`) is driven through its real localhost TCP `execute_code` transport by `valoria_family_mcp_cliff.py` and `valoria_family_mcp_secondary.py`. It executes safe, selected geometry authoring operations. AI text is never executed as arbitrary code.
6. Each changed asset must have Qwen observation/action, `before-hero.png`, `before-strategic.png`, `mcp-after-hero.png`, `mcp-after-strategic.png`, modified editable `.blend`, exported `.glb`, and `mcp-authoring-evidence.json`. The workflow uploads these in `valoria-first-asset-family-v1-production`. The combined assembly includes real Hero V3; inspect actual contact lines from close and phone-scale cameras.
7. Review against the V3 reference. No workflow `success`, GLB count, image score, simple geometry perturbation or model approval establishes artistic parity. If cliff silhouette, masonry authenticity, material variation, clear sockets, non-grid paving or mobile readability is weak, mark **REWORK**, make an authored correction, and render/compare again. The previous CC0 art studio's ground-contact failures should not be repeated.
8. Only after actual visual parity: arrange an isolated Unity 6000.3.23f1 import/render test on the authorized Windows owner runner, respecting its current lease. Test GLB roundtrip, material/texture integrity, physically consistent modular contacts, mobile camera visibility and rendering costs. Save exact Unity images and run SHA. Until done, mark Unity NOT VERIFIED, ART PASS false.

## Source-of-truth evidence

- Art baseline `art-source/valoria/production/first-hero-gate-v1/quality-v3/`.
- New temporary Actions output `art-source/valoria/production/first-asset-family-v1/`.
- Agent raw responses `qwen-inference-trace.json`; executable decisions `agent-decisions.json`.
- Per-piece genuine BlenderMCP evidence `<asset>/mcp-authoring-evidence.json`.
- `manifest-before.json` and `manifest-after.json` describe rendered/exported generations, not approval.
- Original studio source on `art/studio-reuse-handoff-20261011`: `tools/art-studio-recovery/mcp_repair_contacts.py`, `produce_rock_study.py`, `extract_cc0.py`, `decision_policy.py`.

## Current decision

**REWORK — NOT ART PASS — NOT UNITY PASS.** This file is a procedural handoff, never evidence of acceptance. Inspect current workflow runs and latest exact artifact before altering the art again. Older green runs with purely geometric prototypes are rejected by the owner.
