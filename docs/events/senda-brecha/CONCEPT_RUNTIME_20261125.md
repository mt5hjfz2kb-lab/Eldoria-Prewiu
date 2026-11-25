# Senda — concept to runtime, 25 November 2026

Owner approved the concept as the target, not the actual game art. PR #195 remains unmerged and isolated. No production scenes, economy or saves modified; no additional paid API calls.

## Verified runtime
- WebGL commit `6668a00adef5c27a27c0775f0fd237e1729e1afb`: [CI run 45704914735](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45704914735), PASS, capture artifact 15065570616.
- 390×844, 320×700, 844×390, 1280×800. Victory, defeat, pause/replay, correct screen lane direction and gate reset verified. No reported browser errors.
- Original Aldric GLB from pinned source 5ef2aee67cf8fde57ba2fc4b3d929b517ee67944, SHA256 c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b; 10 skin meshes and actual animations. 22 derived guard rigs with 5,999 triangles in armor/cape geometry per base guard (accessories excluded), distinct helmets, shields and socket weapons. Original Spawnling. Three original architecture sprites, two original terrain textures and 36 foliage sprites; 3D ground and gates, 2.5D distant scenery.
- Mobile test: 223 draw calls, 200,876 triangles, 78 batches. Not physical iPhone FPS or memory evidence.
- Earlier run 45702878552 passed but timber GLBs and primitive auxiliaries failed visual review; replaced with actual Valoria painted Bastion/ruins and skinned guard derivatives. Run 45704116311 passed the initial guard version; final run additionally corrects poses, gate sizing, weapons, ground contact and lane inlays.
- Exact web source: https://raw.githack.com/mt5hjfz2kb-lab/Eldoria-Prewiu/6668a00adef5c27a27c0775f0fd237e1729e1afb/tools/senda-brecha/visual-3d/index.html (third-party hosting not independently tested).
- Actual captures are visibly more coherent than the earlier prototype but below concept: repetitive environment, simple lighting, portal treatment, composition and animation polish remain. Do not declare art approval.

## Native proof failed
Unity commit 66be104b243eb2b47c60787c2b21c19e4eb0c5cc, [run 45703272784](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45703272784). Editor compiled and imported the isolated project but glTFast rejected the original Aldric GLB; proof stopped with "Original imported assets / clips missing". No native screenshots. Earlier native version does not contain the later WebGL guard derivatives. Editor exited and runner completed; cleanup of the disposable Library also failed on a deep package-cache path. No production project files were deleted. Keep this failure explicit. Next: diagnose importer-specific compatibility, preserve original source bytes, validate derived import and port final scene before physical iPhone and visual approval.
