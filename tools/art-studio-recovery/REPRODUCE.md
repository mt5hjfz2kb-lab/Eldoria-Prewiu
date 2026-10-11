# Valoria studio rock recovery: isolated executable proof

Use only `art/valoria-studio-recovery-rocks-v1-20261011`. No main merge or publication.

The existing canonical planner and art-production governance run first. The workflow reuses the exact production source from run 38127483365 after Qwen-directed MCP correction if its `.blend` and authoring script match. A changed source requires new production.

Full clean reproduction: download the historically verified Yughues CC0 archive (SHA-256 is checked by the workflow), extract LOD4 using `extract_cc0.py`, then run `produce_rock_study.py` with Blender. This preserves the baseline top terrain and produces real fixed-camera before/after captures and GLBs. Load `ValoriaRockStudy.blend` to inspect the authored instances, source maps, cut crowns, context gates and cameras.

`review_with_local_vision.py` uses the existing local Ollama `/api/chat` mechanism and Qwen3-VL with the exact owner reference. It can reject the candidate or choose a semantic next action. Cloud inference is disabled. A model judgement cannot grant ART PASS. `--post-mcp` records a separate final comparison after correction, preserving the first rejection and raw transport response.

`mcp_repair_contacts.py` uses the pinned existing BlenderMCP addon's real TCP command queue. Only a known geological contact repair can execute; AI text never becomes executable code. All service integrations and telemetry stay disabled. Changed geometry is rendered again with the same cameras and saved as genuine Blender source and GLB. If the model chooses a different donor, baseline or no correction, no forced edit occurs.

Evidence must distinguish successful execution from artistic approval. Original ground, absent foliage/path uplift, cliff repetition, Unity shader/coordinates and hardware performance remain separate issues.

To run the existing-scene Unity comparison, use `ValoriaStudioRockUnityProof.cs` after the owner of the Windows lease releases or hands it over. Do not infer Unity PASS from Blender or GLB audits. The script refuses an ambiguous camera, missing original renderer identity or terrain SHA mismatch. The later integration terrain differs: rebase is mandatory before running this test against it.

Final workstream status is BLOCKED rather than completed: Windows ownership and integration terrain drift prevent Unity certification. To resume, reclaim only this isolated workstream and re-enable its matching request before invoking the existing canonical planner; preserve other active entries. Cached post-correction review requires the pinned blend identity. A fresh source runs donor production, first vision decision, MCP correction and post-correction comparison. Every model review keeps visual_pass=false until independent art inspection and real engine evidence.

Focused single-board comparison (--focused) is cropped from the actual before/after cliff renders and exact owner image. Original full-frame and focused raw responses remain preserved. decision_policy.py rejects contradictory retention/replacement proposals; the real Blender policy test produced mcp-policy-rejection-report.json without opening a TCP correction session. Unavailable vision cannot silently trigger a hardcoded geometry edit; an explicit --director-contact-review is required for an independently inspected director decision. No model output grants ART PASS.
