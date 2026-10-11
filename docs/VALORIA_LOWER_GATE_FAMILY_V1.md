# VALORIA LOWER GATE FAMILY v1

Date 2026-10-05. Independent workstream: `valoria-lower-gate-family-v1`. Initial state **PRODUCTION PROOF**. Base main `5dedb34fe72fa8a120d0fcb3336b8d4713b56794`; claim `c18064df858562881777a0cf4c370413a1e778af`. The owner's current instruction explicitly identifies the blockout as approved and requests this one-family production proof; this authorizes the next step without reopening preproduction. The prior workstream and evidence remain closed and immutable.

## Family spec — closed before source execution

Authority: unchanged `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, SHA256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`; approved `blockout-scene.json`, `unity-blockout-input.json`, target spec and Unity run37275659322/artifact11329019907. Current scope replaces five masses only: WestTower, EastTower, Lintel, WestWing, EastWing. No bridge, road, threshold, cliff, platform, other wall, Bastion, plot or camera changes.

### A. Screen-space role

Source camera exactly orthographic yaw20/pitch35/span48, center0, distance110, 1536×1024. Source target gate bbox [565,526,838,737], annotated uncertainty ±12px. Actual approved tower+lintel union [566.249,520.653,826.761,748.140] — width260.512px/height227.487px, 16.96%/22.22% of source frame. Family wings extend this architectural connection [437.672,547.084,973.720,731.989]; do not confuse wing extent with the primary gate bbox.

Preserve primary external bbox center within12px and extents within5%; source authoring may remove internal placeholder fullness to form the target arch/crenellations. No movement/scale to cosmetically fix an import. The shared bridge/threshold/main-road bytes remain unchanged. Gate sits exactly at the main-platform cliff edge and bridge receiver. Short wing endpoints remain the approved placeholders' endpoints; this is not Walls Family production. Exact projected bounds and all non-family mesh identities will be compared after Unity import.

### B. Silhouette

Two square stone towers, west3.3m wide/east3.7m, both3.2m deep, top15.4m. Intentionally modest width asymmetry follows blockout. Open rounded central arch with radiating voussoirs, recessed raised grille, thick2.5m vault and visible rear returns. Broad stone merlons, lower connecting curtain skyline15.0m, hollow usable-looking tower crowns, restrained coarse wear. No roof, no gigantic lintel ornament, no generic faceted cylindrical tower. Blue/gold front banners are present in the exact target and belong to identity, not added decorative props.

### C. Metric scale / interfaces

Blender X east/Y north/Z up, one unit one metre, inherited world placement. West tower center(1.55576,-25.79179), east(9.77828,-25.49508); foundations7.1m, tower heights8.3m. Clear central aperture width4.724m; spring10.75m, arch apex13.112m; raised grille lower edge10.9m. The narrowest height across the full passage is3.65m above base. Relative to a hypothetical1.8m person: tower4.61×, passage width2.62×; this is an art scale hypothesis, not gameplay rescaling. Bridge end7.03m, existing threshold7.031–7.08m remain continuous. Core tower thickness3.2m; wing thickness and endpoints inherited. No new gameplay collider.

### D. Materials

Shared structural warm limestone: metric4m tile/8 masonry courses, color+normal, roughness. Secondary cut-stone trim: coherent warmer/lighter grain, coarse voussoir/plinth/bevel relief. Restrained dark iron raised grille. Blue/gold textile heraldry, front cloth bow and pointed hem. Contact darkening and edge roughness belong to source maps/geometry; no lights, fires, vegetation or extra props. Wear must not dissolve the architectural silhouette. Normal/color transport must be inspected in Unity, not assumed from Blender.

### E. Modularity

Six semantic groups: WestTower, EastTower, Arch, WestWing, EastWing, Base. Tower body/crown and wall interface vocabulary is reusable; asymmetric tower envelopes and arch receiver remain family-specific. Repeated merlons and quoin course segments are trim modules. Masonry joints/stone grain are shared materials, not individually modelled blocks. Banners use one shared atlas. No decal dependency. Export uses one GLB with semantic nodes and four material roles; no monolithic cliff/bridge diorama.

### F. Mobile readability

At1280×720/span48 one metre≈15px vertically; atportrait-entry390×844/span36 one metre≈23.4px before foreshortening. Tower widths, arch void, .6–.7m merlons, .38m arch ring and1m banners must survive. .035m bevels/joints mostly disappear and belong to normal/surface filtering; do not add micro carvings, handles, tiny fixtures or loose stones. Gate source3:2≈260×227px; actual imported matched captures decide readability. Portrait HOME may partially frame the gate as already approved; entry is its focused view.

## Legacy review — closed

A: none. B in preproduction: StoneDefensiveV2 MainGate/Tower/Wall. Their real source report and isolated gate preview were inspected: MainGate38,640tris/6 donor panels, Tower51,520tris/8; flattened decorative wall-derived slabs and large ornamental lintel do not reproduce the new square tower/arch silhouette. For this family downgrade these complete assemblies to **C — REFERENCE ONLY**. The StoneArchitecture donor wall/corner grammar is **B — REUSE WITH MODIFICATION** in principle, but source shape/material salvage would require separation/re-UV/re-proportion with no demonstrated advantage; no donor imported. Coherent/Defensive/Skin/FirstDistrict gates and TowerWallRock/GateStreetRiseRock stay **C**; complete fused dioramas **D** as replacements because they include out-of-scope bridge/road/cliff. Starter v1/v2/v3 failed temporary gates remain **D**. No A, no legacy geometry reused, no sunk-cost reuse.

## Method — closed

**Blender direct + editable authored mesh spec + procedural surface helpers**, zero Tripo. The explicit vertices/profile sections/arch shape are designed from the locked family spec; the generic DCC engine performs boolean slit cuts, selective bevels, face normals, metric UVs, shared maps, semantic consolidation and deterministic export/capture. Automation does not issue an art verdict. Primary architecture is custom mesh/loft/curved vault, not imported primitive stacks. The saved .blend is independently editable.

Pipeline exception is limited and justified: existing starter source workflow previously hard-coded five legacy assets; extend that existing engine with request-driven `authored_mesh_family`, generic mesh input/output, no new per-family workflow. Existing editor-only preproduction capturer requires a generic replacement-asset mode after ART SOURCE PASS, because its old contract prohibits every final GLB. Existing Tripo-module pipeline is unnecessary for zero-credit directly authored source and does not support the approved whole-scene blockout comparator; it remains parked. No alternate authentication, Tripo generation or asset-specific capture pipeline.

## Review and production criteria

Isolated clay/3quarter/front/side/approved-camera proxy/wireframe plus bounds/source hash; eight owner questions and professional hard questions reviewed explicitly before Unity. On fail correct source in this same family. Then existing Unity blockout capturer substitutes only the five approved masses with the GLB, fixed pivot, no normalization, same camera/light/background; matched BEFORE/AFTER generated in the same run. No canonical scene saved. Five focused gameplay tests plus protected runtime blob comparison.

Planning budget≤25k LOD0 tris; four material roles, shared512px stone maps/256px heraldry. Estimated14 submesh draws after semantic consolidation. LOD1 removes quoin relief/bevel segments while preserving arch/merlons, LOD2 removes metal grid and surface relief only after screen-space error review. Device FPS is not certified by this isolated proof. No automatic runtime rollout or next family.

## Result

**PRODUCTION FAMILY PASS.** The source gate remains **ART SOURCE PASS** from run **37281447235** / artifact **11332112538**. The previously queued Unity integration subsequently completed successfully on **DESKTOP-R10PE55**: run **37282357911**, job **111673150034**, artifact **11337479259** (`valoria-lower-gate-family-v1-unity`).

The integrated proof was reviewed at the exact approved cameras against the canonical target and approved blockout. Reconstructed `tools/review-family-replacement.py` metrics pass in all seven official views: protected projection delta **0 px**, no protected mesh missing, replacement width ratio about **0.9986**, height ratio about **1.0192**, and maximum blockout-center delta **2.895 px**. Against the source target annotation, the integrated tower/arch bbox is [566.431, 521.101, 826.578, 747.376] versus target [565, 526, 838, 737], with center error **5.697 px / 0.309% of frame diagonal**, width ratio **0.9529** and height ratio **1.0724**. The asset therefore preserves the approved screen-space role without camera, scale or composition cheating.

Direct visual review is **VISUAL PASS** for this family: the paired square towers, real rounded opening/vault, crenellated crown and blue/gold heraldry retain the target Lower Gate read; the bridge → gate → road axis remains continuous; the base and wing endpoints meet the approved receiver without a visible transform break; and the family remains legible in mobile landscape and portrait-entry. Portrait HOME retains its already-approved partial framing and shows no new crop regression. The proof scene intentionally leaves cliff dressing, vegetation, fires, final lighting and adjacent wall families as blockout; those out-of-scope differences are not counted as Lower Gate completion and do not justify reopening this source.

Unity import evidence is real, not estimated: **15,830 triangles, 42,112 vertices, 6 renderers, 4 materials, 5 textures, 14 submesh draws, 4,432,816 mesh bytes, 11,888,328 texture bytes**. UVs, normals and tangents are present. Colliders remain **0**; production scene opened/saved are both false. Focused PlayMode tests are **5/5 PASS**. All non-Lower-Gate projected bounds are unchanged at **0 px** in every official view. No runtime/gameplay/camera/composition change and **0 Tripo credits**.

Formal evidence: `docs/evidence/valoria-lower-gate-family-v1/matched-camera-metrics.json` and `integrated-visual-review.json`. Device FPS and authored LOD1/LOD2 remain future optimization gates; neither is required to reopen the accepted family. **Do not begin Bridge without owner review.**
