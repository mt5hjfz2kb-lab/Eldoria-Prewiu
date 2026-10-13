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

## Courtyard v3 native ground-material gate

- Run **38469503150 SUCCESS**, native same-camera Unity A/B 1440×1000: **56,503 / 1,440,000 = 3.923819%** changed pixels, artifact **11850723360**.
- Genuine images show **no pasted-on dark paths**, and a more variegated original grass/earth material. The empty yard is still underpopulated; this is an incremental tonal improvement, **not 6/10**. No architectural or terrain-vertex edits.
- Based on direct inspection, retain the native-ground-material strategy as a candidate, but require spatial composition work for further advancement. Score remains ~5/10, art approval pending broader scene quality.
- Next test reuses the actual already-selected `EA03_Nature_Bush_01a_LOD0` geometry, 12 instances along the courtyard perimeter, to soften wall/soil contacts while leaving the central axial route free. No resource nodes. Run [38470564008](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38470564008) pending; do not claim accepted until native frames are inspected.

## Selected vegetation — actual scale gate

- Native Unity run **38470545151 SUCCESS**: 57,037 / 1,440,000 = **3.960903%** changed pixels, artifact **11851368370**.
- 12 instances of the existing `EA03_Nature_Bush_01a_LOD0` mesh were placed at courtyard lateral edges without touching the road, and original materials retained. **Art assessment: too small to be readable at native camera scale**, only ~534 additional pixels compared with the native-soil candidate. Do not claim significant visual advance or 6/10.
- Next camera-scale test multiplies their inherited original source scale by **2.35**, keeping identical positions, original selected meshes, mobile-friendly shared geometry and same Unity A/B. Workflow [38471224337](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38471224337). Reject if bushes appear oversized or overlap structures; never promote automatically.

## Courtyard v5 — completed native Unity and mobile-framing verification

- Native Windows Unity **run 38471224337 SUCCESS**; [run and full evidence](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38471224337); screenshot artifact **11852611189**, complete integration artifact **11851544948**.
- Blender reused **12 actual original selected `EA03_Nature_Bush_01a_LOD0` meshes** (shared source geometry) at 2.35× source scale; original ground albedo gently varied; 20 small contact stones. No new resource nodes, new buildings, geometry edits to source meshes or obstructions of central 7m route. Verified by Blender source identity, output hash and constraints.
- Independent native Unity same-camera A/B 1440×1000: **60,697/1,440,000 = 4.215069%** changed pixels (channel threshold 5). This is a visibility gate only, not art approval.
- Visually inspected the actual before/after engine images and native mobile-framing **780×520** image. Bushes are now readable at the lateral edges and make the courtyard less uniformly empty; the terrain tone has more variation without the rejected dark overlays. Gates, bridge, central stair and main axial stone route remain visibly clear.
- **Art decision: accept this as a modest incremental courtyard improvement** over the validated baseline, but **NOT a 6/10 scene**. Approximate subjective quality **5.1–5.3/10** vs reference 10/10; score is a qualitative estimate, not a computed metric. Most of the courtyard still lacks coherent inhabitation and foreground-to-background atmospheric depth. No release/main promotion.
- Mobile screenshot proves the frame can be rendered in native Unity at mobile-like dimensions, **not physical-device FPS, memory or build certification**.
- **Next executable artistic action toward 6/10:** focus on one large authored, lore-approved courtyard focal group (non-resource, non-building if Bastion I design forbids it), selected from existing reusable architectural props and placed outside protected route/build parcels; test with identical Unity camera and real device budgets. Alternatively improve contact between the upper terrace edge and the lower courtyard using an existing modular stone edge rather than new flat decals. Keep any rejected variant out of the active scene.
- Learning: only material-native variation plus physically existing selected vegetation survives this gate; artificial decal paths fail despite valid technical A/B. Qwen's prior real mountain design is preserved as provenance, and the new deterministic courtyard decisions are **not attributed to Qwen**.

## Architectural landmark + actual local AI participation (2026-10-14)

- The owner requested a single moderately sized medieval guard/supply landmark at the edge of the existing courtyard, preserving future build plots and the accepted ~5.2/10 baseline.
- Recovered and reused the existing Blender source asset families (`defensive_props`, `stone_path_platform`, `wall_straight`, `wall_connections`) and selected shrub mesh, as well as genuine prior Unity frames and accumulated artistic rejections.
- New isolated no-cost GitHub Actions workflow `.github/workflows/valoria-bastion1-ai-architectural-proposal.yml` starts local Ollama with `qwen3-vl:4b-instruct`, supplies real site coordinates, architecture inventory and accumulated art feedback, and requests a concrete **AI-authored composition proposal**. This does **not** grant Qwen authorship of Blender geometry.
- First Qwen run [38474674963](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38474674963) downloaded the model and performed inference but failed to parse its JSON (`Unterminated string` at character 439), probably due `num_predict=480` truncation. **No valid AI decision was produced or accepted.** Artifact 11852179571 records request/engine provenance.
- Retry raises the bounded token budget to 1100 and writes raw model response before JSON parsing for auditable fallback. Run [38475192811](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38475192811). No training, paid inference or additional model dependency.
- Blender execution candidate `compose_bastion_guard_station.py` is explicitly **deterministic implementation**, gated on a real successful Qwen composition. It reuses existing defensive-prop and paving meshes at the left detached fortification (x≈-20.4,y=-12), with three low-poly timber framing pieces; leaves 7m axial corridor, source geometry and resource nodes untouched. **Do not integrate until valid Qwen plan and same-camera Unity proof.**
