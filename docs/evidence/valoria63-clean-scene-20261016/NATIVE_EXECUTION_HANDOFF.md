# Valoria clean scene — native Unity handoff

The 63 GLB provenance job passed (runs 38953883707, 38954593696 and 38954736752). The real clean-scene Unity builder is in `Unity/Assets/Eldoria/ArtTests/Valoria63CleanScene/Editor/Valoria63CleanSceneBuilder.cs`.

## Production gate
- Preserve existing Bastion I and SceneBreakthrough scenes. No Unity native run until `windows-runner-heavy` is explicitly released by the SceneBreakthrough owner.
- Restore the validated artifact `valoria63-clean-sources-38954736752` to the branch working tree (folder `Unity/Assets/Eldoria/ArtTests/Valoria63CleanScene/Sources/`).
- Use Unity 6000.3.23f1 with the project's existing glTFast package and execute `Eldoria.Valoria63.Editor.Valoria63CleanSceneBuilder.Build` in batch mode.
- If import/compiler errors arise, fix the isolated builder only, re-run and keep logs. Do not claim completion from successful static staging.
- Reopen the saved `Valoria_BastionI_Modular63_Clean.unity` with `Eldoria.Valoria63.Editor.Valoria63CleanSceneBuilder.Validate`.
- Preserve three real screenshots, Unity logs, SHA manifest and scene; compare screenshots against the approved reference before art approval.
- The native scene is NOT gameplay-certified, mobile-certified or ART_APPROVED. Existing code reserves expansion parcels and route but needs native QA.

## Important implementation check
Before Unity execution verify imported template materials and geometry persistence, and that `BuildLandform` produces a physically connected bridge-to-plateau route. Repair within the isolated branch if necessary. Keep source GLBs immutable and the user-approved reference unaltered.
