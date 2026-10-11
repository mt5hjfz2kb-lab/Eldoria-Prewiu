# Eldoria — Reusable AI Art Studio: safe handoff (2026-10-11)

> STATUS: EXPERIMENTAL REUSABLE ENTRYPOINT — NOT PROMOTED, NOT UNITY-INTEGRATED, NOT ARTISTICALLY APPROVED.

## Scope and source of truth
This handoff freezes a discoverable entrypoint for future art sessions without editing `main`, the active Unity integration branch or the Work-owned source branch. Branch source: `art/valoria-studio-recovery-rocks-v1-20261011` at `88deab05f8116d72ce342e4643002607a91c14b9`. Always re-check the live HEAD and active ownership before reuse.

Read in order:
1. `AGENTS.md` — canonical operational rules; owner instructions and active workstream ownership override this note.
2. `pipeline/active-workstreams.json` — `active` only, no inferred runner entitlement.
3. `docs/ELDORIA_ART_BIBLE.md`, `docs/VALORIA_VISUAL_BIBLE.md`, `docs/VALORIA_REFERENCE_IMAGE_NOTES.md` and `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` — current visual requirements (verify applicability).
4. `tools/art-studio-recovery/REPRODUCE.md` — exact reproduction instructions and known limitations.
5. `pipeline/art-studio-recovery-v1.json` and `.github/workflows/art-studio-recovery-v1.yml` — source configuration and opt-in Linux CI.
6. `tools/art-studio-recovery/` — donor exploration, Blender authoring, MCP-based contact correction, local Qwen visual analysis and advisory decision policy.

## What is actually working, what is not
- Real Blender scene, GLB export, donor selection, matched view renders and versioned artifacts: demonstrated by successful isolated GitHub runs `38127483365`, `38128263672`, `38128846228`, `38130229366`. Confirm each run's attached evidence.
- Two CC0 donor rock routes and real BlenderMCP contact manipulation are implemented in the isolated workflow. License/source checks must remain enabled.
- Qwen3-VL can produce visual opinions, but evaluation confusion has been observed; it is ADVISORY only, not an autonomous visual-pass authority. A CI SUCCESS is a technical gate, never proof of artistic acceptance.
- The isolated art-studio branch is NOT in the canonical `main` Unity production pipeline; there is no verified automatic wiring from every new artist request to the studio, and there is no certified mobile Unity appearance/performance for the new rock candidates.
- Neither a Blender source nor an isolated render should be represented as a final Valoria Unity integration.

## Safe reuse on a new art task
1. Create a NEW task branch based on a consciously selected, verified source SHA; do not write to Work's active branch or `main`.
2. Read the canonical art brief and specify a single asset family plus comparison target, source/target camera and scale.
3. Use the existing `tools/art-studio-recovery/` scripts and the existing opt-in workflow in that task branch; adapt names/parameters rather than copying a whole new subsystem.
4. Audit donor license and file checksums; prefer zero-price/local tools; no cloud Qwen/paid Tripo.
5. Produce exact Blender source + GLB + side-by-side camera-matched previews + manifest and source provenance; run a GLB audit.
6. Review actual before/after imagery. Do not trust AI prose alone. Record ACCEPT / REWORK / REJECT with concrete visible reasons; without review, mark ART PASS UNKNOWN.
7. Deliver a clearly versioned candidate for an isolated Unity proof, preserving original gameplay and visual authority. Validate in the real strategic and mobile-framed cameras; mobile hardware performance remains unverified until measured.
8. Only after visual approval and tests, propose a reviewed merge/integration; never autonomously claim permanent installation or promotion.

## Evidence and current decision
Technical pipeline: PROVEN EXPERIMENTAL. Agent autonomy: PARTIAL. Artistic candidate approval: NOT PROVEN. Always-on future-agent connection: NOT IMPLEMENTED. Unity integration of recovered rocks: NOT VERIFIED.

This document is intentionally a safe, persistent pointer to executable existing infrastructure, not a claim that the whole studio is connected to production or already validated.
