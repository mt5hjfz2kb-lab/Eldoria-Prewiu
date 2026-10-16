# Valoria — 63 asset feedback bridge v1 (integration-safe)
Source of truth: [issue #78](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/78), this registry, and native Unity/Blender evidence. **This is a toolchain guard, not a second asset library or a new visual approval.**

Each of the **63 original module IDs** is given one permanent `assets[].correction_ids` record; empty arrays mean **no integration defect recorded yet**, not "perfect". Corrections each carry an originating GLB SHA-256, Git SHA, Unity run, concrete evidence link, rule and classification.

- `ASSET_GLOBAL`: repair in the artist's source only after actual same-source Blender A/B; retain sockets/dimensions unless explicitly tested.
- `ASSEMBLY_CONTRACT`: verify in scene; artist must preserve constructibility of the changed GLB, not assume standalone render passes.
- `SCENE_LOCAL`: retain per-scene placement/scale/material treatment; **never bake** e.g. Bastion I 0.86 tower scale into every exported tower.

Command examples from repository checkout:
```bash
node tools/valoria-asset-feedback/guard.mjs validate
node --test tools/valoria-asset-feedback/guard.test.mjs
node tools/valoria-asset-feedback/guard.mjs check artist /path/to/candidate.json
node tools/valoria-asset-feedback/guard.mjs check unity /path/to/candidate.json
```

Candidate example:
```json
{
  "asset_id": "pine_small",
  "sha256": "YOUR_NEW_GLBSHA256_64_HEX",
  "commit": "YOUR_EXACT_40_HEX_COMMIT",
  "blender_ab_url": "https://github.com/OWNER/REPO/actions/runs/REALRUN",
  "ready_for_unity": false,
  "decisions": []
}
```
For a module with corrections, provide one `decisions` entry per correction: `{"id":"...", "disposition":"scene_only | preserved_assembly | applied | pending_review", "evidence_url":"https://...", "validation_ab_url":"https://... for applied"}`. `pending_review` means artist may keep iterating but **must not mark ready_for_unity**. An approved Unity review additionally records `unity` with `same_camera=true`, exact `run_id`, `artifact_id`, `before_url`, `after_url`, `art_approved=false` or `art_approved=true` with visual_review=PASS; the guard verifies *provenance metadata*, not aesthetic judgment.

**Integration into both working branches (not automatic merely from a GitHub issue):** at the next version selection in `art/valoria-63-individual-quality-uplift-20261015`, artist must import current `main:tools/valoria-asset-feedback` (not stale own branch), generate one candidate JSON per changed GLB, call `check artist` before selecting a new best. At the next Unity selection in `integration/valoria-bastion1-player-scene-20261012`, owner must import same current main registry, call `check unity` per imported candidate with pinned SHA and native A/B evidence before declaring adoption. Missing/unacknowledged corrections must fail the gate. When new defects are confirmed, add unique correction to registry and append ID only to its asset; never rewrite Blender originals based on scene-only feedback. This bridge is independent of the running Windows runner, no paid services, and must honor active owners.
