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

## Completed second Unity A/B — foreground rock readability

- **Native Unity run 38465720395: SUCCESS**, artifact `bastion1-unity-review-frames-38465720395` id **11848088015**. Exactly same engine capture method/camera for before and after, resolution 1440x1000. Also produced landscape, portrait and mobile-framing frames in both captures.
- Existing Qwen mountain preserved and deterministic Blender finishing applied. The only new material edit is existing foreground schist multiplier **0.25/0.25/0.25 → 0.53/0.51/0.48**, retaining all textures and original meshes.
- Real independent Unity pixel A/B: **470,669 / 1,440,000 = 32.685347%** changed pixels (threshold >5/255), gate `INDEPENDENT_REVIEW_REQUIRED`. The run and before/after native capture completed without Unity compile errors.
- Human visual comparison of both exact engine screenshots: rock escarpment at front and side is more legible, and distant background has changed silhouette. Main composition remains flat and underpopulated. **Accept this as a verified incremental improvement, NOT 6/10 and NOT art-release approval.** Subjective estimate: around **4.8–5.0/10**, reference 10/10. Pixel change magnitude is NOT equivalent to artistic quality.
- Keep these results in the integration branch, not in main. Next visual priorities: lighten and stratify near cliff in a controlled way; foreground-to-background atmospheric separation; intentional world detail outside playable parcels. Validate mobile performance independently.
- Genuine source: [GitHub Actions 38465720395](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38465720395), native frames artifact 11848088015, integration artifact 11849125132.

## Courtyard environmental composition — learning gate (2026-10-13)

- Baseline is the last accepted mountain+cliff native Unity state from run 38465720395. This is **reconstructed and captured BEFORE** the new environmental intervention; no repeated artistic re-profiling is being judged as new.
- Blender script `tools/valoria-bastion1-assembly/compose_bastion_courtyard.py` adds only peripheral surface details, preserving original mesh coordinates and the protected axial corridor; no resource nodes or new buildings.
- **First real Unity A/B run 38467372726 SUCCESS technically but ART REJECTED**. 30,567 / 1,440,000 = **2.1227%** pixels changed. Actual side-by-side showed two dark, uniform curved ribbons and large flat color patches; unnatural contrast makes the environment less coherent. Do **not** accept that candidate or call it a 6/10 result.
- Corrective response: reduced path widths, replaced 6 large ground islands with 14 small ones, lowered and randomized scree placement, and baked portable mottled ground textures into the GLB. Commit `f95a00f7816a1cb1fc55c42f9b1b999bd36cd69d`, validation run [38468519777](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38468519777). **Pending independent image review; not approved.**
- Reusable lesson: pixel fraction only establishes visibility. Uniform untextured surface decals can reduce art quality even when navigation and Unity compile pass. Always inspect native camera frames, especially peripheral soil/grass seams.

## Courtyard v2 art review and v3 corrective strategy

- Native Unity run **38468519777 SUCCESS**: authentic 1440x1000 same-camera A/B, 12,997 changed pixels (**0.902569%**), artifact **11851480786**.
- **ART REJECTED**: narrower textured paths still render as unnaturally dark strips on the grassy terrace; smaller patches are less intrusive but do not solve the root composition problem. Preserve evidence, never promote.
- Root issue is using additional terrain-overlay meshes as color decals; even with valid UVs and baked textures the resulting shapes are not naturally integrated in the engine.
- Third intervention removes *all* those ribbons and islands and instead modifies the **existing original grass albedo image** in Blender with broad, bounded mottled tonal variation. No new terrain overlays, buildings or resource nodes. Sparse contact scree remains. Native A/B workflow checks the original baseline and captures identical camera again.
- Commit `2651f5534bc45902ee59fc9fcb94a7176b46378f`. Artistic status remains **unapproved pending real Unity evidence**.
