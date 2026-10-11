# AI artist reuse — operator quickstart

This is a **working isolated entrypoint**, not an automatic hook installed on `main`. No paid service or Windows runner is used by the smoke test.

## For a new conversation/agent

1. Check live HEAD, read root `AGENTS.md` and `pipeline/active-workstreams.json` (`active` only). Do not claim a runner or overwrite another owner's branch.
2. Use `art/studio-reuse-handoff-20261011` as a **known reproducible experimental starting point**. Create a distinct task branch from an explicitly verified SHA.
3. Validate prerequisite scripts and policy:

   ```bash
   node tools/art-studio-reuse/prepare-task.mjs --check
   ```

4. Prepare a task with an actual checked-in reference image, for example:

   ```bash
   node tools/art-studio-reuse/prepare-task.mjs \
     --family rock \
     --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
     --camera "strategic angle matching the Valoria baseline" \
     --out artifacts/art-requests/new-rock-family.json
   ```

   The manifest records the SHA256 of exact reference bytes, intended camera, gates and zero-cost requirements. It **does not** automatically execute Blender, change `main`, install new agents or certify art.

5. Read `tools/art-studio-recovery/REPRODUCE.md` and run the **existing** dedicated rock proof pipeline on an owned isolated branch. On any other asset family, first adapt the authoring recipe; the command will report `reuse_proven_pipeline: false`.
6. Inspect actual before/after Blender images, maintain `ART PASS UNKNOWN` pending a reliable review; never use Qwen's prose or workflow SUCCESS as sole proof.
7. Prepare Unity integration via separate owner-authorized handoff, ensure matching source hashes and scene/camera. Do not merge without separate visual/technical approval.

## Explicit acceptance hierarchy

- **Source gate**: verified licenses, original files, hashes, Blender source, GLB audit.
- **Visual gate**: real matched-camera evidence, artist/director acceptance of selected candidate. Qwen is advisory.
- **Unity gate**: real scene reload, strategic/mobile screenshots and material/shader checks.
- **Performance gate**: representative mobile measurements, not desktop/WebGL guesses.
- **Promotion gate**: reviewed, explicitly authorized integration into target production branch.

Each gate is separate. Never claim that this smoke test proves the asset meets reference quality.

## Current boundaries

The studio asset pipeline is verified on Linux Blender, and the isolated Unity proof path exists, but the recovered rock scene is not visually approved or integrated into the canonical Unity game. The smoke workflow only checks the reusable task entrypoint. `main` is unchanged.
