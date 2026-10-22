# Layered ground trial — actual world anchors

Retained forest source e5d073278f0fc0e40f231d7672ed8c0d4a9fcb37, native39859007296, artifact12461027085:7/7passed,11.9726083sec. Actual near and wood-focus PNGs now show coherent facet silhouettes throughout the measured326existing trees/visual copies. Earlier98-only and prefix corrections did not fully cover the scene; full runtime renderer inventory resolved this. No source-tree additions or gameplay changes. Trees remain2.5D candidates, not ART_APPROVED.

Next dominant defect: featureless yellow local field, no readable forest floor, and a single low-resolution geographic atlas stretched across1820x1560world units. Repeating detail alone did not solve composition.

Consulted installed Context7, official Unity manual and Unity Graphics source22Oct2026:
- https://docs.unity3d.com/Manual/class-TerrainLayer.html (layer textures, tiling, normal information; URP four-layer pass guidance)
- https://docs.unity3d.com/Manual/urp/shader-terrain-lit.html
- https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl

Applied principle: separate geographic pigment from locally scaled surface layers and actual geometry lighting. This is a custom three-surface shader on the EXISTING mesh, not a conversion to Unity TerrainData and not implementation of its height-blend algorithm. One192x192linearRGBA local weight map, generated once from retained tree bounds and genuine city/wood/quarry/ruin transforms. Red: narrow curved footpaths between real destinations, protected city core. Green: tree-root ground cover. Blue: broad meadow patches. Alpha: fade at384m domain boundary. Water mask excludes every surface overlay. No inferred image masks, new buildings, camps, resources or collision edits.

One additional texture sample, three palette surfaces using retained painted detail. Detail fades at large pixel footprints; main-light direction shades the actual mesh normal relative to a flat baseline, preserving diffuse values on flat terrain. Same material on original near prairie and continuous far terrain prevents a new geographic split. Original meshes and original material arrays remain restorable. No new physical displacement or performance certification claimed.

Acceptance: actual near/regional/panoramic PNGs, same camera as retained326-tree source; reject excessive patterning, paths through water/city core, abrupt mask edges, or loss of existing target readability. The whole canonical reference target remains unmet.

First layered native39859637398, source98560a0e4b7d4d71e01fb1610e0589c39f114b4c, artifact12460987872:7/7passed,16.2096158sec. Same-camera PNG review: retain near forest-floor cohesion and actual-destination path idea, but reject full-scale application because panorama shows orphan dark tree-root stains and too much tiled detail. Correct by fading ecology from zoom125 to152 (same gameplay visibility boundary); reduce texture-detail persistence using world pixel footprint .16..75m. Raise footpath ochre lightness for readable actual paths. No geometry/camera changes. Await final native evidence.
