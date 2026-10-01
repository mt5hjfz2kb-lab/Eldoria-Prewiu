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

Recovery currently queued behind another owner's remote toolchain proof. Do not cancel unrelated work or launch another paid Tripo task. Review actual official19/12/9 and per-piece diagnostics before certifying any reusable pieces.

## Checks and boundaries

Changed workflow YAML parses. Required full workflow governance check ran and reported one **pre-existing unrelated** violation in `hero-bastion-integrated-v1.yml` (heavy runner workflow self-trigger); the changed canonical workflow does not introduce that violation.

Production promotion remains disabled. No changes to Valoria.unity, VisualWorld, gameplay topology, hotspots, colliders or circulation. Source identities, raw exported GLB and generation evidence are pinned; review is isolated.
