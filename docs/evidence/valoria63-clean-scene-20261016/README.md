# Valoria Bastión I — isolated clean scene with 63 existing GLBs

Status: source recovery VALIDATED; native Unity scene PENDING. No art approval or gameplay certification.

## Isolation
- Unity 6000.3.23f1, URP17.3, glTFast; same existing Unity project.
- Own branch: experiment/valoria-63-clean-composition-20261016.
- Only new scene output: Unity/Assets/Eldoria/ArtTests/Valoria63CleanScene/Valoria_BastionI_Modular63_Clean.unity.
- Never modify the existing ProductionSlice/Runtime/Valoria.unity, paused Bastion I integration branch, or paused Valoria 63 art branch.
- Existing approved visual reference SHA-256: 8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689.

## Inputs confirmed
- 63 GLB modules from the archived real Blender pack, Actions run 38813966671.
- Six documented visually preferred replacements (existing oak gate, two portcullises, limestone wall, medium pine, natural cliff column), with exact SHA-256 pins.
- Successfully re-staged, checked and archived inputs: run 38953883707, artifact 12070320708, 43.6 MB.
- Source recovery is not native Unity artistic quality evidence.

## New scene builder
- Unity/Assets/Eldoria/ArtTests/Valoria63CleanScene/Editor/Valoria63CleanSceneBuilder.cs
- Builds a truly empty Unity scene, imports all 63 glTF meshes, preserves render assets, and assembles cliff/river, fortified bridge and outer gate, access road, stairs, upper keep and construction reserves.
- Requires all 63 named assets physically instantiated in the composition and more than 120 placed module instances; fails rather than pretending success.
- Requests real Unity PNGs at 1536x1024, 1280x720 and 390x844, saves the new scene and validates reopening in a second editor process.

## Outstanding
- No compiled/validated native Unity scene exists as of this handoff, and no honest screenshot/visual score can be given yet.
- Native job is present but workflow_dispatch-only; the other clean scene experiment retains windows-runner-heavy in main/pipeline/active-workstreams.json.
- Before dispatch, hand over that exclusive resource in main registry, verify the old job has stopped, and then run the own manual Unity workflow. The job checks the LIVE main lease and blocks overlap.
- A green technical build cannot certify visual quality, full gameplay preservation or Android/iOS performance; independently inspect native images and perform appropriate tests before promoting.

See workflow: .github/workflows/valoria63-clean-scene.yml.
