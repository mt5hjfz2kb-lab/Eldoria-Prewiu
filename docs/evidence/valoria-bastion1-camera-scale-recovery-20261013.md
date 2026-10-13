# Bastion I — camera-scale art intervention and exact next gate (2026-10-13)

Status: **ART REVIEW REQUIRED, not ART APPROVED**. Keep original player scene and Qwen-authored GLB intact; no promotion to main.

## Why previous strategy stopped

- Qwen8B CPU inference timed out; Qwen4B produced several typed invalid plans and tiny player-camera changes.
- Run 38460437486 rejected by `CAMERA_FOOTPRINT_TOO_SMALL` after two real model attempts. Do not repeat equivalent cliff prompt tweaks.
- Existing verified mountain source from Blender replay 38438668388 is immutable; use it as the baseline for any deterministic finishing, with provenance preserved.

## Completed real alternative

- Non-destructive Blender intervention `tools/valoria-bastion1-assembly/reprofile_distant_mountain.py`, imported the original verified 60MB GLB and altered only its existing distant mountain mesh.
- Native Windows Unity run **38464678831 SUCCESS**, with exact same camera before/after and independent PNG pixel diff. Blender changed **8,741 of 9,896** mountain vertices, maximum displacement **20.9381 m**, while preserving other mesh coordinates.
- Native camera 1440x1000: **89,802/1,440,000 = 6.23625%** changed pixels. Change concentrates in distant backdrop: ~20.11% top third, ~0.51% middle, 0% bottom. Independent visibility gate passed; **artistic quality gate not passed** because foreground remains dark, background composition still lacks depth, and main yard remains sparse.
- Real frames and proof: [GitHub Actions 38464678831](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38464678831) / artifact `bastion1-unity-review-frames-38464678831` id 11848831681. These are genuine engine screenshots, not mockups.

## Follow-on correction already committed and executing

- The original front cliff material `Valoria schist dark fractured cliff.001` has an overly dark **(0.25,0.25,0.25)** PBR base-color multiplier. The largest foreground surface reads almost black.
- Next pass preserves original textures/UVs/normals and replaces only that factor with **(0.53,0.51,0.48)** while keeping the verified broad mountain reprofile.
- Commit `d1d33cbba9628e08c97cf4ecd2ff8eac47f76544` and [native Unity run 38465720395](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38465720395).
- The workflow runs the same camera twice, measures real PNG pixels using pinned independent reviewer, and retains before/after artifacts even on failure. **Do not claim this new pass succeeded before retrieving its completed logs and frames.**
- **Next executable action**: retrieve run 38465720395 result and `bastion1-unity-review-frames-38465720395`, compare actual before/after visually (especially the front cliff), verify color/materials, roads and mobile frames. Accept only if composition genuinely improves. If rejected, keep baseline, target dark near-field cliff/terrain continuity and foreground composition instead of another distant-only or tiny Qwen sculpt. No paid credits.

Visual score remains approximately **4.5/10 pending independent artistic acceptance**, target 6/10. No artificial score inflation.
