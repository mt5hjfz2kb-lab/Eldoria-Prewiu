# Eldoria art brain: verified access vs live runtime

Evidence: GitHub Actions run **38149304572** on this isolated ops branch. This is a reproducible audit and not a claimed full AI deployment.

## Exact facts
- Source recovery: 14/14 studio files successfully staged by existing `tools/valoria-art-brain/bridge.py` from `art/studio-reuse-handoff-20261011` at `e137aaf713...`.
- Local Ollama HTTP endpoint: NOT RUNNING on the fresh GitHub-hosted Linux machine. Qwen3-VL source adapter exists but cannot infer without a locally running model.
- Blender command: not in this smoke runner. Dedicated **hero asset workflows separately install pinned Blender 4.2.5**.
- Unity editor: not in Linux smoke runner; independent Windows Unity runner used in prior project integrations, subject to owner/lease coordination.
- BlenderMCP: recorded, actual previous TCP execution in existing `mcp-report.json` (44 crenels seated, 36 quoin relief corrections). It is not a persistent live daemon between GitHub jobs.

## Portable steps for any executing agent

1. Read current repo workstream ownership and check active GitHub runs. **Never claim the Windows runner without verifying ownership.**
2. In a new isolated checkout, fetch studio sources:
   ```bash
   git fetch --no-tags origin refs/heads/art/studio-reuse-handoff-20261011:refs/remotes/origin/art/studio-reuse-handoff-20261011
   python3 tools/valoria-art-brain/bridge.py --stage /tmp/eldoria-art-brain
   python3 tools/valoria-art-brain/runtime_probe.py
   ```
3. For art rendering use **the existing** `.github/workflows/valoria-first-hero-asset.yml`, which installs pinned Blender 4.2.5 and generates the gate, images and GLB; do not reinvent Blender provisioning.
4. For BlenderMCP reuse the known script/addon pinned hashes; start server and client within the **same Blender process / job**, perform only allowlisted, gate-specific operations and preserve transport proof. Rock-specific contacts code may not run against a building.
5. Ollama/Qwen: first check local `/api/tags`. If unavailable, either provision a suitable *owned* Linux machine with enough memory/disk for a local Qwen3-VL model or skip the optional vision review and preserve independent human/image comparison. Do not download huge models into each ephemeral GitHub runner without measuring resources or owner authorization for ongoing cost. Never route reference images to an arbitrary external endpoint.
6. Unity: use an authorized free existing Unity Windows GitHub runner workflow for real scene load, source checks, mobile camera visuals and material persistence. Not present on this Linux proof machine. No main merge without gates.

## Remaining limitation

There is no demonstrated always-on, shared Ollama+Qwen inference service reachable from arbitrary Work or normal chat sessions. Making one available requires an owned accessible machine / per-job model setup and available storage/RAM; **source recovery alone cannot supply that compute**. A reproducible no-inference pipeline already exists for Blender and MCP changes inside job runtimes. Do not imply all tools are simultaneously live.

The current requested first hero asset visual quality work is independent and should continue while runtime tooling is resolved.

## Live vision recovery result (11 October 2026)

- GitHub Actions run **38150397153**: real ephemeral Ollama installed, model **qwen2.5vl:3b** (Qwen2.5-VL) downloaded, vision image transmitted, valid final text received: `White background.` The step **succeeded**. No paid inference API.
- The originally attempted **qwen3-vl:2b** did process image prompts but repeatedly exhausted token budgets in internal reasoning without a usable final answer; runs 38149726904, 38149944785 and 38150160868 failed their real output gate. It must not be considered operational for art review in this configuration.
- For new art jobs, execute `bash tools/valoria-art-brain/start_local_qwen_ci.sh` **inside the same GitHub job** before the actual art-review adapter. Do not expect Ollama to remain alive across jobs. To evaluate real before/after art, add image review inside the job with the model still running; the current smoke only proves simple image inference, not production art judgment.
