# Region 1 canonical 4X — real integrated Unity visual audit (native39780987248)

**Source SHA:** `b0cca024664c06c5c18883382b9e7781b45d0866` · **Unity:** 6000.3.23f1 / URP 17.3.0 · **Workflow:** [39780987248](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39780987248) · **Actual six-camera artifact:** [12426399160](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39780987248/artifacts/12426399160)

**Verdict:** `UNITY_PLAYMODE_INTEGRATED_PASS` (5 NUnit cases, 0 failed), `GEOGRAPHIC_CONTINUITY_PASS` (panorama has no hard old terrain square), `ART_OWNER_REFERENCE_ACCEPTANCE_PENDING`. This audit is explicitly **not** ART_APPROVED, WEBGL_PLAYABLE_VERIFIED or PHYSICAL_IPHONE_VERIFIED.

## Ground-truth views
Native real integrated camera exports contain `cercana`, `regional` and `panoramica`, each landscape **1280×720** and portrait **390×844**, with hash and world target position in matching `-source.json`. These are native Unity PlayMode pixels, not web mockups or AI renders. No UI-layer overlay in camera-only captures; they do not certify UI V3 or hosted playability.

### Verified positives
- Same `WorldRoot` and `ZoneOneStrategicCamera` in the actual `Valoria_City_World_UI` scene; consistent geographical XYZ across zoom.
- Large continuous peripheral ground, sinuous river, partial shore and single fracture near the centre; no second scene or floating island.
- Both previously-visible **240×240** old prairie/macrogeography renderer sheets omitted in global view and restored locally. All original `WorldHotspot` components and colliders remain; no fabricated cities/camps/enemies or resource rewards.
- No weather fog in regional (geometric border haze activated exclusively in overview), and minimal long-range LOD with reused trees and batched tiny ridges.
- Five checks pass including correct third-person source scene binding, no extra hotspots/colliders, zoom consistency, geological ground-only crack, LOD transitions and six actual camera PNGs in disposable gateway session.

### Open visual gaps relative to the exact October 22 original PNGs
1. **Photographic and material fidelity is below target.** The meadow is too smooth/schematic from altitude; biome colors, terrain texture detail and silhouette are not yet as rich as `03_vista_panoramica.png`.
2. Distant forest masses are still identifiable as grouped graphic marks rather than convincingly scattered 3D woodland; comparison must be rechecked after current foliage edits.
3. The Breach in this run still resembles a dark radial star with a bright dot instead of a natural ruptured soil basin; geology/stain work follows on a later SHA.
4. Roads still dominate as four near-symmetrical spokes and should connect natural paths less diagrammatically.
5. Current portrait camera does **not** fit all of Region 1 horizontally; panorama is a pan/zoom view in portrait, not a certified full-region single-frame fit.
6. There is no verified texture/GPU/fps/iOS memory profile, no 30-minute physical iPhone test (#96), and no hosted WebGL build on this exact SHA.
7. Independent UI V3 and Engendro visual acceptance remain owned by their workstreams, not implied by this test.

**No promotion / no release:** PR #100 remains draft, no main merge or itch.io publication until genuine visual A/B, gameplay/save regression, WebGL browser and separate physical iPhone stability gate. Bastions III–V remain paused.

This snapshot is an honest baseline for subsequent version comparisons, not the delivery declaration.
