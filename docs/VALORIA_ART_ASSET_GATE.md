# Valoria — asset gate for the visual benchmark

Reviewed: 2026-09-26. Reference: owner-provided epic Valoria concept; acceptance contract: `docs/ELDORIA_VISUAL_BENCHMARK.md`.

## Decision

The current runtime composition must **not** be iterated into the benchmark by adding more procedural modules, trees or material tints. The three deterministic captures of the last certified pass (`0442f880`, run `36224356226`) show a small independent fortress on a flat field, simple repeated roofs and conical trees, no convincing terrain separation, no visible distant territorial Breach. A subsequent terrain/ridge trial (`d201ea6e`) passed Unity tests/build but made the scene visibly worse (brick-pattern valley, primitive mountain silhouettes and sphere foliage); it was rejected and its complete tree was restored on `main` in `8ec709e`. Technical green was not visual acceptance.

## Existing families

| Family | Visual decision for benchmark | Evidence / viable use |
| --- | --- | --- |
| Mega Fantasy Props Pack castle walls/towers/half gate | Transition only | Authored stone surface helps at middle distance, but connected modules still produce a short, repetitive citadel; its destroyed towers were rejected in captures because their yellow material/style clashes. Small masonry/props may remain in lower town. |
| Bublik Simple Modular Castle Assets | Incompatible as capital core | Minimal low-poly modular silhouette; cannot provide the generations-old monumental keep and ruined palace in the reference. |
| EmaceArt Slavic World Free | Transition only | URP-compatible source pack; fences, rocks, roads and low-town details can support a coherent district. Its architecture is village-scale; visually unsuitable as the Bastion core. Several foliage variants/materials previously blew out in actual URP captures. |
| PolyOne Free Modular Terrain | Transition only | Cliffs/mountains can contribute very distant silhouettes, but previous large close placements hid the city, and small repeated rocks do not create a believable sculpted landscape. |
| Tree_Packs/URP_Tree_Pack | Candidate for a controlled foliage test | Three URP tree prefab families are present but not visually certified in this scene. They are preferable to further procedurally generated cone pines if their actual rendered foliage fits; three variants alone cannot carry an entire biome. |
| NatureStarterKit2 | Incompatible until demonstrated otherwise | Old mixed shader/script package; Unity 6 compile issue was repaired but its rendering, LOD/performance and style remain unapproved. |
| Procedural ValoriaKit roofs, pines, terraces and ground | Incompatible as final art | Explicit blockout, visibly repeated geometric forms in three camera angles. More placement/tint iterations cannot produce the reference's architectural detail or geological variation. |

## Minimum art replacement for the next build

1. **Core fortress:** one compatible *architectural family* with substantial, cohesive keeps, asymmetric towers and roofs, arches, deep windows, balconies, damaged variants and masonry trim. Avoid a kit containing only wall/tower segments. The old palace ruin must be built from the same material/scale language. This is the hard gate for a meaningful visual leap.
2. **Landscape:** sculptable Unity terrain or cohesive terrain meshes plus matching cliffs, strata, paths, rock and ground materials/decals. The city must occupy a defensible height with roads and terraces cut into it.
3. **Vegetation:** multiple interoperable URP tree species and ages, canopy/bush/groundcover variants with alpha and sensible LOD; review the already imported three URP tree prefabs first.
4. **Far world:** mountains, valley forest and monumental ruins sharing the same lighting/palette; a distinct contaminated terrain and dead-vegetation set for the distant Breach.

Budget remains **0 €**. Free status, license and Unity 6 URP appearance must be checked for each selected package before promotion. The owner would need to import a chosen Asset Store package through their Unity account on the Windows PC if no distributable, licensed zero-cost source can be obtained directly. Do not buy anything or confuse a store preview with a validated in-game asset.

## Gate to resume implementation

Start by substituting the Bastion core and its immediate landscape as one composition, keeping `WorldHotspot` interactions independent. Make one establishing view read as a different art phase before expanding to districts; then take all three deterministic captures, inspect them, correct the dominant 2–3 defects, repeat and certify EditMode + PlayMode + Windows build. Do not record a green pipeline as art approval.
