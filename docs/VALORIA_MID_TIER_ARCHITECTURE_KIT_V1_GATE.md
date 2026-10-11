# Valoria Mid-Tier Architecture Kit v1 — exact-source generation and review

Owner explicitly approved the attached four-object sheet as-is and its visible 55-credit cost on 2026-10-01. The four objects are distinct references, not four views of one building.

## Source and paid result

Original JPEG: 1448×1086, 605572 bytes, SHA-256 `f2329e4f2a19d68c4fecc13eaf633f4f89687183a5c50c1387923078b970eae7`. Original bytes preserved in `pipeline/exact-inputs/Valoria_MidTierArchitectureKit_v1/`. Live uploaded blob hash and size were verified before the one Generate click.

Generation run **36850489399 SUCCESS**, artifact **11155202054**, H3.1. Task **3b143007-754d-46ad-8acb-8094e56181f3**. Actual screenshot confirms exact input, new model and balance **2490 → 2435**. Total authorized spend **55 credits**, one generation; no further generation authorized or requested.

Export run **36850732821 SUCCESS**, artifact **11155272611**. Exact GLB `Valoria_MidTierArchitectureKit_v1.glb`: **68231740 bytes**, SHA-256 `5e5432b78151a837eea1a77c2ea5783c543da10da747add208adfdc21cbf284a`. Tripo screenshot confirms export success.

## Zero-credit isolated review

Run **36851146406**: Blender PASS, **1848237 → 49800 triangles** combined. Four spatial clusters: **9889, 8703, 11296, 19525 triangles**. Small disconnected islands explain the difference between combined and split totals; four clusters alone do not certify completeness or usability.

Unity failed during package import with **System out of memory** (exit1073741845), before capture/visual certification. No Unity technical PASS and no visual PASS claimed.

Recovery commit **7f1e83cac017cab93e68c790853f41077fca63a0**, run **36852832649**: same exact GLB, same optimization, no refinement/salvage/promotion. Opt-in Unity session flags `-refreshImportMode InProcess -job-worker-count 1` reduce concurrent import work. Official documentation: https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html . Added a captures/logs-only artifact so visual evidence can be retrieved independently of large model archives.

Recovery **36852832649 FAILURE**. Blender again succeeded. Unity review step's PowerShell process exited **-1073741819** after approximately9minutes; downstream Node artifact upload failed with **Committing semi space failed / JavaScript heap out of memory**, and manifest parsing/cleanup also failed. Job cleanup terminated this run's orphan Unity process and children. This demonstrates continuing runner-wide memory exhaustion; it is not a visual rejection of the model. **No artifacts were uploaded by this recovery run; no current Unity captures or technical/visual certification available.** Generic combined gate was skipped in multipiece mode; its successful step cannot be called Unity PASS.

Requests are parked disabled. Total spend stays55credits; no further paid generation and no production promotion. Full recovery job logs preserve the failure. Resolve runner memory availability before another zero-credit review; do not repeatedly launch the same blocked workload or certify from cluster count alone.

## Checks and boundaries

Changed workflow YAML parses. Required full workflow governance check ran and reported one **pre-existing unrelated** violation in `hero-bastion-integrated-v1.yml` (heavy runner workflow self-trigger); the changed canonical workflow does not introduce that violation.

Production promotion remains disabled. No changes to Valoria.unity, VisualWorld, gameplay topology, hotspots, colliders or circulation. Source identities, raw exported GLB and generation evidence are pinned; review is isolated.

## Final recovered visual review — 2026-10-01

The first Unity-attempt artifact **11156331060** preserved the actual Blender-normalized source and all four extracted GLBs despite the later Unity OOM. Those exact files were recovered and rendered independently at multiple azimuths for visual forensics; no regeneration, Tripo call, source substitution or production integration was performed.

Recovered pieces:
- piece_01: **9,889 tris**
- piece_02: **8,703 tris**
- piece_03: **11,296 tris**
- piece_04: **19,525 tris**
- combined canonical Blender target: **49,800 tris**

**FINAL VISUAL VERDICT: FAIL / NOT PROMOTABLE.** All four extracted GLBs are spatially mis-grouped: each contains two vertically separated architectural masses or substantial disconnected fragments rather than one coherent reusable module. The issue is therefore not merely missing Unity captures or runner memory. The generated sheet survived Tripo and Blender technically, but the automatic spatial clustering did not recover the intended four authored objects as clean standalone modules.

This means the current four-piece extraction must not be integrated into Valoria or used to extend the Modular Assembly System. The source generation remains preserved for possible future zero-credit manual re-separation/reconstruction, but no further paid generation is authorized or needed to establish this verdict. Production promotion remains disabled; gameplay topology, hotspots, colliders, circulation and Valoria.unity remain untouched.

A rerun of recovery workflow **36852832649** was started once at zero credit after runner recovery. Its result may provide additional Unity evidence if it completes, but it cannot overturn the already-observed disconnected geometry without a different separation/reconstruction method.

## Authoritative Unity rerun — final closure

The zero-credit rerun of recovery workflow **36852832649** completed **SUCCESS** after runner memory became available. Final evidence artifact: **11159128095**; full module artifact: **11158673298**. Source commit for the rerun: `7f1e83cac017cab93e68c790853f41077fca63a0`. The exact raw source identity remained **68,231,740 bytes / SHA-256 5e5432b78151a837eea1a77c2ea5783c543da10da747add208adfdc21cbf284a**; optimized combined GLB **49,800 tris / SHA-256 24193e411efefc98e0d49d1db9a74f71ae02c0435647bb5f2a7832f26ac16b0a**.

Unity multipiece technical gate passed: **4 pieces / 49,413 tris total** with UVs, normals, one renderer/material/collider per piece, raycast hit and empty-space miss. Official captures now exist: kit 19/12/9, overview, front diagnostic, close oblique, plus front/rear/side/oblique for every piece.

The official Unity captures confirm the independent forensic review: **all four extracted pieces are visually invalid as standalone reusable modules** because each piece contains two disconnected architectural masses stacked/separated in space or major floating/severed geometry. Therefore the final status is:

- **TRIPO GENERATION: PASS**
- **BLENDER REDUCTION: PASS**
- **UNITY TECHNICAL MULTIPIECE GATE: PASS**
- **VISUAL KIT: FAIL**
- **PRODUCTION PROMOTION: NO**

No additional Tripo credits were spent in the rerun. No production scene, Valoria.unity, gameplay topology, hotspots, colliders or circulation were changed. The correct next move is not to integrate these four extracted GLBs. If this source is revisited, use a zero-credit manual/reconstruction-based separation strategy; otherwise generate future modular references as one clean object per Tripo task instead of relying on sheet clustering for critical architecture.

## Zero-credit YZ reconstruction — recovered four intended objects

A new reconstruction pass changed only the multipiece spatial clustering from `xy` to **`yz`**, using the exact same already-generated raw GLB and exact pinned source SHA. No Tripo generation, no paid retry and no production promotion occurred.

Canonical recovery run **36857266003 SUCCESS** (source commit `83116384dde97d25af73045f67e4fd3661fb07aa`). Evidence artifact **11160571815**; full module artifact **11160251379**. Blender again reduced the exact raw source to **49,800 tris** and Unity multipiece review passed technically with **4 pieces / 49,413 tris total**, UVs, normals, material, renderer, collider and raycast checks present.

The corrected YZ grouping visually recovers the four intended sheet objects:
- **piece 01 — arched entry / porch mass:** correct standalone architecture recovered, but one small detached fragment remains below the main mass. **VISUAL PARTIAL; cleanup required before production use.**
- **piece 02 — two-storey residential mass:** coherent standalone building recovered. **ISOLATED VISUAL PASS CANDIDATE.**
- **piece 03 — workshop / facade mass:** coherent standalone building recovered, including the intended side/hanging-sign structure. **ISOLATED VISUAL PASS CANDIDATE.**
- **piece 04 — roof / upper-structure module with tower/chimney:** coherent partial-height reusable roof module recovered. **ISOLATED VISUAL PASS CANDIDATE.**

This supersedes the earlier conclusion that the paid Tripo generation itself was unusable. The failure was primarily the original **wrong clustering axes**, not the source generation. The sheet can therefore be salvaged at zero additional credit cost. It is **not yet production-integrated**: piece 01 needs a bounded cleanup, and the recovered candidates still require a real Valoria composition/integration proof before promotion.

Request closed disabled after review; no automatic integration.
