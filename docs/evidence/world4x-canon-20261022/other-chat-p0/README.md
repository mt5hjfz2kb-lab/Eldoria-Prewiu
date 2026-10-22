# ELDORIA — P0 region-geography authoring experiment

**This is an independent editable art-direction deliverable, NOT an integrated Unity screenshot or approved game build.** The active PR #100 remains owned by a different chat (ownership revision 11); this pack does not overwrite it, change gameplay, consume runner or purchase credits.

- `region1_layout.json`: editable roads, waterways, rift splines, forest belts and relief anchors. Coordinates are actual World4X local X/Z with the existing Breach at (8,61).
- `masks/*.png`: six **semantic masks** at 1024×1024 (land, forest, water, roads, relief, rift), aligned to `[-910,910] x [-780,780]` world units. All are derived/curated from existing owner reference only for placement; **not** the concept PNG pasted as a ground texture.
- `geographic_layout_preview_NOT_UNITY.png`: structural diagnostic drawing; **not** a game render and must never be shown as such.
- `build_eldoria_p0.py`: reproducible generating script expecting owner reference at `/mnt/data/eldoria_world_canon/03_vista_panoramica.png`. Requires Pillow, cv2, numpy.

To integrate: sample the independent masks at shared world coordinates into the existing RegionOneHybridTerrainArt, RegionalForestImpostors, `SampleHeight`, and sea/road/breach masks as a reversible EXPERIMENT variant. Use the same XYZ and existing hotspots; compare against a genuine unity real-world-camera baseline. Do NOT merge/release without authentic PlayMode captures and owner approval.

## Unity-ready, non-destructive atlas trial

`Unity-ready/Assets/Eldoria/World4XCanon/P0ArtMasks/` contains the six masks and `Unity-ready/Assets/Eldoria/World4XCanon/Editor/RegionOneP0BakeAuthorialAtlas.cs` contains an Editor menu item to bake a **new** `RegionOneP0Terrain.png` next to the old painted atlas. It does not overwrite that atlas or change the actual game; it needs the current owner of PR #100 to import it and run an A/B via the real integrated WorldCamera. The C# script is supplied as **uncompiled** candidate because the active runner is leased to another chat. Source masks are grayscale geographic labels, not fake 2D full-screen renders.

## Actual 3D geometry (sandbox experiment)

`region1_p0_preview_mesh_NOT_INTEGRATED.glb` is a real editable triangle mesh built from the geographic relief mask in the same World4X X/Y/Z space. It has **21,025 vertices, 41,472 triangles**, is below the existing near-field gameplay objects (height -7.4 to -0.91), and has no colliders or game actions. It carries an original diagnostic semantic atlas, not the reference image. This asset is deliberately not yet in Unity; see `validate_layout.py` (structural test only). Run `build_glb.py` after editing masks.

**Owner conflict**: canonical `eldoria-world-region-1-canon-20261022` in `main` was independently reclaimed as `ownership_revision=11` by `chatgpt-world4x-hybrid-resume-82901015` after the owner-directed rev10 claim from this chat. To avoid concurrent destructive writes and duplicate Unity runs, this experiment remains packaged outside the active production PR until the current claimed workstream integrates it or transfers ownership. It is not ART_APPROVED and is not certified on iPhone/WebGL.
