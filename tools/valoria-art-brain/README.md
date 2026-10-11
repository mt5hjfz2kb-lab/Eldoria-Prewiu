# Valoria — existing art brain access (not a new brain)

This branch previously carried only the gatehouse Blender authoring script. The studio's proven-but-experimental sources live on `art/studio-reuse-handoff-20261011`; neither Qwen nor BlenderMCP runs automatically simply because their names occur in a prompt.

## Verified bridge
```bash
git fetch --no-tags origin refs/heads/art/studio-reuse-handoff-20261011:refs/remotes/origin/art/studio-reuse-handoff-20261011
python3 tools/valoria-art-brain/bridge.py --check
python3 tools/valoria-art-brain/bridge.py --stage /tmp/eldoria-art-brain
```

Sources are staged **outside the repository**, to avoid overwriting this branch's art or `main`. The stage JSON records the pinned source commit. A free Linux CI smoke test verifies this operation. This is the smallest reversible integration; no duplicate studio architecture.

## What is usable, and what remains to be adapted
- `review_with_local_vision.py`: existing local Qwen3-VL/Ollama evaluation adapter, originally rock-specific. Adapt gate images, exact reference, prompt and output schema before running. Requires a reachable local Ollama inference service; GitHub hosted Linux runners do not supply that service by default.
- `mcp_repair_contacts.py`: demonstrated BlenderMCP TCP control for rock-contact repair, **not** a general gatehouse editing tool. Do not execute rock repairs on the gate. Adapt a constrained, tested gatehouse command handler to use MCP meaningfully and safely.
- `extract_cc0.py` and `produce_rock_study.py`: donor proof for the rock scene. Only reuse compatible licensing/asset-prep patterns, never pretend rock geometry is a gatehouse finish.
- `decision_policy.py`: conservative approval/rejection policy; avoid unconditional PASS from Qwen.
- `prepare-task.mjs`: manifests and traceability, not 3D production itself.

## Current gatehouse artistic task
Keep the selected V5 silhouette fixed. Improve stone surface character, roughness, normal, texture-scale, edge treatment and light response without adding towers, roofs or battlements. Use matched-camera unmodified before/after and an independent visual review. Only run external vision/MCP if runtime and task-specific adaptors are actually available, log the invocation and exact output, otherwise report capability as available-source/NOT EXECUTED. Do not assert `AI art director` worked just because a script rendered.

Never use paid credits, modify `main`, preempt shared Windows, or classify a successful smoke test as art acceptance.
