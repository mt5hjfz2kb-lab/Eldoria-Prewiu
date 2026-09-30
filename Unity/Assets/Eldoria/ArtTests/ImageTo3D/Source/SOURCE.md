# ImageTo3D Source staging

This folder is an **ephemeral staging area** for isolated Eldoria art gates.

Do not treat files here as canonical production assets. Certified identities, hashes and verdicts live in:
- `docs/VALORIA_MODULE_KIT.md`
- `docs/TRIPO_MODULE_PIPELINE.md`
- the relevant gate document for each family.

## Runner behavior

The canonical GitHub Actions pipeline copies or generates temporary GLB files here during a run. Those binaries are intentionally not committed.

## Owner local review

Inside Unity use:

`Eldoria > Art Gate > Micro-Valoria > Rebuild and Open Certified Review`

The helper:
1. reads the certified owner exports from the local Downloads folder;
2. verifies their SHA-256 identities;
3. uses the canonical Blender processor for raw Terrace/Gate sources;
4. stages local review GLBs in this folder;
5. rebuilds and opens the isolated Micro-Valoria scene.

Generated GLBs and the review scene are ignored by Git so local inspection does not pollute the repository.

Certification still comes from the GitHub Actions gates; the Unity menu is an owner-inspection convenience.

Protected surfaces remain untouched:
- `Valoria.unity`
- `VisualWorld`
- gameplay.
