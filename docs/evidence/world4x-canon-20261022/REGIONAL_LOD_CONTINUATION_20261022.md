# Verified regional woodland continuity — 22 October 2026

The owner requested continuation of the same hybrid World4X mission. PR #100 and all original reference PNGs are retained. No merge, release, paid generation or gameplay changes.

## Native result

- Before: `a0bfe2368c37f3eed9b879995be3a887c3867ecd`, native run `39824847833`, artifact `12445443807`.
- Corrected candidate: `293fff6565fea452660e9d3b53b254e786353087`, native run `39826768500`, artifact `12446208469`. Original NUnit XML downloaded and inspected: **7 passed, 0 failed**, twelve genuine Unity PNGs.
- The first attempt `f6ef8b7c` / `39826254057` deliberately exposed a near-view regression through the new assertion: local zoom is 36, whereas the fade had begun at 34. Corrected to use the actual `navigation.localZoom`; no test weakened.
- Subsequent source-snapshot export at `1132e559` / `39827508494` again produced 7/7 and 12 native PNGs; its later artist step failed because the existing Ollama service was not running. That workflow failure is separate from successful Unity tests.

## Root cause and actual scope

The regional camera has size 113, while a single distant switch at 152 controlled both gameplay marker suppression and hybrid forest visibility. At regional zoom, all batched canopies disappeared, leaving the flat painted mask and a few tiny donor trees. Woodland now has an independent smooth alpha transition from the actual local zoom to size 55. Marker suppression remains unchanged. The same world coordinates and batches are reused; no new colliders, resources or geometry.

Regression checks exercise sizes 55, 90, 145, 153 and 113 and return to local. The original real wood target remains selectable in both orientations; existing collider count is unchanged. These scoped checks are not full I–II, WebGL or physical iPhone acceptance.

## Matched-image inspection

For all six scale/orientation pairs, camera position, target, zoom, aspect and image dimensions match exactly. Near and panoramic images have **0% RGB pixels changed above 5/255**, in both orientations. Regional changes cover 10.361% landscape and 4.243% portrait; these are visibility measurements, NOT quality scores. Visual inspection confirms woodland is present regionally. Stylization, terrain texture and the whole regional composition still fall short of the immutable owner references. **ART_NOT_APPROVED**.

## Main artistic task remains open

The owner P0 macro-composition experiment is not satisfied by this rendering fix. The actual Unity mesh (6,561 vertices, 12,800 triangles) is now exported as a disposable editor-only source and frozen in Blender at 1 Blender unit = 10 Unity metres, preserving vertex indices/UVs. The existing `tools/art-knowledge/run-artist.mjs` preparation/consumption checks and typed `author_brief.py` / `execute_artist_plan.py` are reused for a bounded terrain-copy experiment. No generated geometry enters runtime automatically. Inspect real before/after, protect the existing central play space and reject insufficient results before any integration.
