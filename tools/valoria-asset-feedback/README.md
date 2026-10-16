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

## Deployed workstream gates — 2026-10-16

**Both active workstreams now read the canonical `main` registry at execution time**, not stale branch copies. This closes the previous gap where Bastion I code and Blender asset upgrades could silently ignore each other's recorded failures.

1. Artist branch `art/valoria-63-individual-quality-uplift-20261015`: its original `.github/workflows/valoria-63-individual-quality.yml` now has an **Ubuntu feedback preflight before the Windows/Blender job**. On a successful 63-asset render it has an additional Ubuntu artifact receipt gate verifying **63 GLBs by hash and both PNG views** for each, using the latest `main` registry. A separate light runner workflow `.github/workflows/valoria-63-feedback-preflight.yml` certifies the owner's current acknowledgment file without launching Blender.
2. Bastion I branch `integration/valoria-bastion1-player-scene-20261012`: its existing `.github/workflows/valoria-bastion1-modular63-unity-review.yml` now blocks the Windows/Unity stage unless the **8 selected SHA-256s and all current feedback** agree. The postflight runs again against current `main` and checks actual native A/B image files. `.github/workflows/valoria-bastion1-feedback-preflight.yml` separately tests the owner's acknowledgment on Ubuntu.
3. Owner-specific append-only review files are **not** global corrections, and must be revised deliberately:
   - Artist: `docs/evidence/valoria-63-individual-quality-uplift-20261015/VALORIA63_FEEDBACK_ACKNOWLEDGMENT.json`
   - Unity: `docs/evidence/valoria-bastion1-player-scene-integration-20261012/MODULAR63_FEEDBACK_ACKNOWLEDGMENT.json`
4. Each acknowledgment pins a **Git blob SHA-1 of the actual authoring script** and every version selected for Unity. If a script changes, its document must be reviewed and its source hash refreshed; if an asset version changes, its corresponding SHA-256 in the selection/acknowledgment must match. If new corrections are added to `main:registry.json`, BOTH workstreams must acknowledge them before their next guarded production run.
5. Corrections are typed per piece: `ASSET_GLOBAL` can only become `applied` with authentic A/B; `ASSEMBLY_CONTRACT` must be explicitly preserved; `SCENE_LOCAL` may not become an automatic Blender model-wide change. `pending_review` allows isolated *experiments* but **blocks promotion**, and a technical CI PASS never gives `ART_APPROVED`.

**Evidence of connection**:
- Central guard verified in [main feedback run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38936619284).
- Artist-side preflight [run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38936564156).
- Unity-side preflight [run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38936577642).
- Unit tests include intentional failures for a missing correction, globalized scene-local adjustment, altered GLB selection, unsupported promotion, and changed authoring source.

**Known boundaries:** This is automated **coordination and rejection**, not an AI that diagnoses new model defects on its own. A Bastion owner discovering a new issue must write the verified correction under the appropriate single asset in canonical `registry.json` (and cross-reference [#78](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/78)); the next controlled production preflight will then fail closed until the artist and Unity workstreams explicitly evaluate it. Jobs already running before this change still use their own pinned workflow commit and do not retroactively gain gates. The feedback guard neither certifies aesthetics nor alters a GLB, Unity scene, gameplay, release target or paid resource.

For an explicit recheck in a branch checkout, use the dedicated Ubuntu preflight workflow or:
```bash
node path/to/current/main/tools/valoria-asset-feedback/workstream-sync.mjs artist \
  docs/evidence/valoria-63-individual-quality-uplift-20261015/VALORIA63_FEEDBACK_ACKNOWLEDGMENT.json '' \
  path/to/current/main/tools/valoria-asset-feedback/registry.json \
  path/to/current/main/tools/valoria-asset-feedback/asset-ids.json \
  tools/valoria-modular-fortress/individual_quality_uplift.py
```
