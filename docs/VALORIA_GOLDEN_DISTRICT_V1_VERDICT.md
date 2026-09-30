# Valoria Golden District v1 — Reproducibility Verdict

Date: 2026-09-30  
Branch: `visual-proof/valoria-golden-district-v1`  
Base: `main@9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`  
Validated head: `7d09d43de78768a80555bc165cf757a2be5ffd42`  
Run: `36784151237`  
Artifact: `11128768779`

## Verdict

**REPRODUCIBLE VISUAL PROOF PASS**

The Golden Cell visual recipe was ported onto a branch created directly from the real current `main`, then validated with the same deterministic 19 / 12 / 9 / mobile cameras.

After correcting the resource staging paths, the main-derived proof reproduces the successful Golden Cell look.

## Proven

- The improvement does not depend on accumulated experimental-branch state.
- The recipe can be applied on top of the real Valoria codebase without changing gameplay colliders / hotspots.
- The current Bastion silhouette can be retained while its visible material family is brought into the Golden visual language.
- The processional stair, terrace, civic access, support buildings and rock/architecture transition remain readable at gameplay zoom.
- The visual result is materially stronger than the current production view at zoom 9 and mobile.
- Tripo credits: 0.
- Paid assets: 0.
- External proof textures / hero resources are CC0 and staged only in the experimental workflow.

## Important production constraint

The current proof still stages some visual resources at CI time. Therefore this branch is **not yet a production integration**.

Before promotion to `main`, production must use assets that are persistently available to the Unity project/runtime, or replace the staged proof resources with equivalent in-repo material assets.

## Next production step

1. create a runtime-safe Golden District material family using persistent in-repo resources;
2. move the bounded district composition from editor-only proof code into a disabled production integration layer;
3. validate identical gameplay collider/hotspot signature;
4. capture matched 19 / 12 / 9 / mobile before/after;
5. enable only after the production integration visually matches this proof closely enough.

This closes the reproducibility question: **the new artistic direction is viable on the real project base.**
