# Eldoria Valoria — TerrainV2 A/B art gate (2026-10-17)

## Preserved source and native proof
Repository branch: `construction/valoria-master-plan-layers-20261016`. This is ONLY the isolated test scene; no canonical scene, frozen library workstream or gameplay authority was changed.

Native Unity run [39019735279](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39019735279) SUCCESS technical. Scene artifact: `Valoria_SHARP_RiverCliffV2_CANDIDATE.unity`, Blender edit `terrain-v2/ValoriaTerrainRiverCliffV2.blend` (SHA-256 `6cf867a57bdb801693a20c03558d4eaffca204d5f46e49046d7cf9edb3b65de0`), derived GLB SHA-256 `c4a3c7d2082e93e89006b6cd4810525828a4f3671426be778babb69514b36ce0`. 9 genuine Blender mesh groups / 19,020 source triangles. No source originals overwritten.

A/B is from the **same real player camera component and identical HOME FOV**, not historical orthographic QA:

- BEFORE (best earlier native scene): `terrain-v2/terrain-v2-BEFORE-home.png`.
- AFTER V2 (candidate): `terrain-v2/terrain-v2-AFTER-home.png`, portrait `terrain-v2/terrain-v2-AFTER-portrait.png`. Pan/zoom captures retained.
- Actual Unity native result `terrain-v2-native.json`: 7 of 7 semantic source collider contacts, 12 historic stair colliders retained, scene and camera authority untouched.
- The on-screen 3D gravity/walkability in tilted diagnostic staging frame, actual PlayerState/LocalGateway progression, device touch hit areas and mobile performance **remain untested**. Collider rays are not a gameplay certification.

## Independent visual decision

**TECH PASS; TERRAIN-V2 LOCAL ART FAIL; FULL-SCENE ART FAIL. DO NOT PROMOTE.**

After screenshot: a dense, repetitive, high-contrast sawtooth limestone strip replaces the broad flat retaining face behind the gate. The rhythm is clearly synthetic; repeated zigzags look like a texture artefact or serrated curtain rather than naturally layered cliff geology. Foreground riverbed/shore detail is still not convincing from HOME, as the oversized gate and bridge occupy too much of the frame. Despite an organic topography source mesh and valid geometry, there is no sufficiently convincing improvement in reference similarity. The upper Bastion remains plain GREYBOX. Portrait view cuts through the gate/tower and loses the city. A CI green/19,020 triangles is NOT a visual improvement score.

**Revert by selection, not destructive edits**: the retained best scene is `Valoria_SHARP_SemanticContact_IsolatedV1.unity`; earlier `Valoria_MasterPlan_Layers_ABCD_BridgeV1.unity` and TerrainV1 are untouched. V2 remains optional editable art source for revising geology but is explicitly rejected as final visible terrain.

## Key professional dependency still unresolved

Reference art reset [target-spec.json](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/main/docs/evidence/valoria-art-production-reset-v1/target-spec.json), exact authorized source SHA-256, gives lower-gate visual bounding box 273 pixels wide on a 1536-pixel reference (~17.8% width) with uncertainties; in current real-player HOME native screenshot the assembled gate-plus-wings almost completely spans the 1230-pixel view, with structures cut off. Geometry is *not* in approved reference proportion even though 7/7 click collider semantics project onto the correct targets.

**Do not repeat** blind Blender noise sculpt -> Unity import -> same disproportional first-person crop. The next geometry action must be camera-first macro scale/composition: recover the already-existing professionally referenced **85-mesh preproduction blockout** at `docs/evidence/valoria-art-production-reset-v1/unity-blockout-input.json`, particularly its non-rectangular main platform, 7-to-11 terrace step, bridge/gate and stair bounding geometry. Compare its full envelope and target size against current source plan in a **separate reversible layout proposal**. No early architecture expansion, materials/trees hiding bad geometry, fake SHARP point hits, asset nonuniform scale or published canonical edits.

Stage status: approved reference and historical topology = SOURCE PASS; native real player camera FOV/pan/zoom = TECH PASS; targeted gate selection = 7/7 HOME and 39/39 visible points on camera states; gameplay integration = NOT CERTIFIED; macro composition = FAIL; terrain continuity = source physics pass/visual fail; scene artistic rating 6/10 = NOT ACHIEVED.

## Why a rigid whole-city rotation cannot certify game physics

The current diagnostic camera rigid fit quaternion `(-.2131735888,.1196094847,-.0232835002,.9693856151)` pitches local source up approximately **24.77° relative to Unity global vertical**. A seven-viewport-collider match does not establish global gravity-aligned movement, walkable surface normals or mobile game-state integration. Before advancing the visual experiment beyond macro stage, either justify the game's actual movement physics in the rotated local frame with a native test or construct a physically upright spatial layout candidate that preserves the connectivity graph and revalidates 12 stairs/bridge/plots. Do **not** blindly promote the tilted scene as a playable certified Valoria.

No money or credits authorized or spent.
