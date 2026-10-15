# Eldoria reference-first — visual assessment of actual existing source images (2026-10-15)

## Verifiable image sources
- Artistic target: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`. Git object (main observed) `6409a860440cdc937818b18939dcba91ab2e6df8`.
- Existing **Unity images** (historical, not captured by the new pilot):
  - `Unity/Assets/Eldoria/ArtTests/OriginalHero/review-establishing.png`
  - `Unity/Assets/Eldoria/ArtTests/OriginalHero/review-approach.png`
  - `Unity/Assets/Eldoria/ArtTests/OriginalHero/review-oblique.png`
- All four images were retrieved as bytes and inspected, not inferred from filenames.

## Visible differences — qualitative, no invented score
| Criterion | Approved reference | OriginalHero historical captures | Consequence |
|---|---|---|---|
| Full scene composition | Wide rocky plateau surrounded by water, bridge/river in foreground, large fortified upper area, monumental road/stairs, dense conifers | Isolated tall turreted building on near-empty grey polygonal stage with sparse round-canopy trees | Main deficit is **environment and composition**, not just an asset's bevels |
| Architecture | Low-wide, connected perimeter walls and stone gates, central fortified top | Narrow tall manor/keep, pitched/slate roof and single small foreground extension | The OriginalHero is **not** a plausible direct replacement for upper defensive ensemble; preserve as a donor or secondary structure |
| Terrain and silhouette | Thick, broken, natural-looking stone escarpment and forests at several depths, water in foreground | Flat/simplified surrounding ground | Need coherent cliff/shore transition, world-scale hierarchy and water silhouette |
| Color and lighting | Warm torch/stone/road areas contrasted against twilight-blue atmosphere and dark pines | Diffuse neutral daylight and low-contrast grey background | Camera, set dressing and directional/ambient light are first-order scene tasks |
| Player-camera legibility | Full 1536x1024 overview including upper and lower gates, main route and HUD | Three smaller views centered on a single building | Must assess wider camera and at least one proximity image in Unity; visual approval pending |

No numerical 0–10 rating can be derived from these images without a complete comparable Unity capture and independent review.

## Existing *candidates* to test next (existence != artistic acceptance or commercial licence)
1. `art-source/valoria/production/bastion-family-v1/BastionFamilyV1.glb` (actual existing Blender-source family plus authored texture maps); stronger semantic match for upper fortress.
2. `art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.glb` (actual existing Blender-source family with associated masonry maps); possible lower gate.
3. `Unity/Assets/Eldoria/Resources/Valoria/Stone_Gate.prefab`, `Stone_Wall.prefab`, `Stone_Tower.prefab`; modular placement candidates, geometry/materials need real import check.
4. `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb` and `RockToWallTransition.glb`; possible wall-to-cliff seam candidates.
5. `Unity/Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab`, `Rock01.prefab`, `Unity/Assets/Eldoria/Resources/Valoria/SM_Cliffs_01.prefab`: environment source candidates.
6. `Unity/Assets/Eldoria/ArtTests/OriginalHero/valoria_hero_fragment.fbx`: use as a technical baseline or secondary manor; do not treat as the target fortress.

## Production decision
**Do not invest in modelling the OriginalHero as the principal fortress.** Test the already available connected wall/gate/fortress families with scene composition, wide camera, and actual source textures first. Do not download new assets until the existing families have received a fair Unity rendering test.

The current capture builder is a first **technical baseline** using OriginalHero, not the desired final artistic composition. The next creator should select a second architectural arrangement without touching `Valoria.unity`, game runtime, world4x, Bastion1Assembly or the other artist's work.

## Execution snapshot and blocker
- Pure-source, Ubuntu-only static workflow: `38794583466` **SUCCESS** at the pilot branch head `bcfcbf21d7d71884211fd8811bf2d2269ddb8d65`; explicitly **not** Unity compile or render proof.
- Another actor concurrently added workflow `.github/workflows/eldoria-reference-first-zero-budget-pilot.yml`, commit `b09f4625a19f244089446d12b9a65233165a113d`. It automatically started native Windows run `38794504051` on the shared runner `DESKTOP-R10PE55`, **FAILURE** during `actions/checkout@v4`: files in the existing shared workspace were 'not uptodate; will not remove from working tree'. The Unity execution step was skipped, so **no new PNGs exist** from this pilot.
- The Windows runner is still claimed by the active Bastion I workstream; do not launch another Windows process or edit a path that would retrigger the concurrent workflow until ownership/checkout separation is resolved. Prefer a dedicated clean worktree/unique job checkout location and a deliberate resource handoff; never forcibly reset another owner's workspace.
- `ART_APPROVED=false`; `UNITY_RENDER_PASS=false`; `REUSE_PROVEN=false`; no scene publication; no licensed-third-party assets downloaded.

## Safe next action
Coordinate with the Bastion I runner owner and the other pilot workflow author. After the Windows resource is released, run a Unity job in an isolated dedicated checkout with the same source SHA and collect genuine capture artifacts; then compare images against the approved reference. Any failure must be diagnosed before a repeat; do not treat a green Ubuntu source gate as aesthetic progress.

## Updated native Unity failure diagnosis and safe source repair

- Isolated Windows run `38794999022` did reach C# compilation in Unity 6000.3.23f1; the capture failed **before executing the scene method**, with two precise `CS0234` errors in `ReferenceFirstPilotCapture.cs`: unqualified `Application.isBatchMode` and `Application.dataPath` were resolved against namespace `Eldoria.Application` rather than `UnityEngine.Application`.
- The builder now uses explicit `UnityEngine.Application.isBatchMode` and `UnityEngine.Application.dataPath`; correction commit `7032d51bfbab1e20ef0f8543744ff9eafc008340`.
- Static rule updated to accept this qualification (commit `aba18c80f3e259a61ca97c3055122f991f72ca4f`). Both commits have `[skip ci]` intentionally: **do not launch another Windows CI on a retained reservation by accident**.
- **VALIDATION LIMIT:** source repair is straightforward and grounded in compiler errors but has NOT been recompiled in Unity since the correction. Actual PNGs, art review and performance remain pending. When Windows runner is explicitly coordinated, rerun the exact fixed commit using an isolated worktree and record the genuine outcome.
