# Hero Fortress Replacement Proof v1 — Final Result

Date: 2026-10-01
Base main HEAD: `9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`
Experimental branch: `visual-proof/hero-fortress-replacement-v1`

## Objective
Keep the already-proven production cell frozen and replace only the Bastion/access hero architecture.

Frozen:
- official camera / zooms;
- processional stair;
- plaza;
- civic terrace;
- rock ↔ architecture transition;
- base PBR system;
- base lighting / atmosphere;
- gameplay logic, colliders, hotspots and authoritative topology.

## Candidate A — existing local authored castle family
Source:
`Unity/Assets/Mega Fantasy Props Pack/` already present in the repository.

Used:
- tower.007;
- tower_small_window.003 / .004;
- stone_wall_detailed;
- stone_wall_detailed_corner;
- stone_half_gate.001;
- tower_destroyed.

Validation:
- run **36788009062 — SUCCESS**
- artifact **11130422329**
- source HEAD **f6dc2c342fe0e34248c4e26109fd5ce602c9ff6f**

Metrics:
- BEFORE: 1,574,646 triangles / 1,199 renderers / 629 materials / 22 lights
- AFTER: 1,172,028 triangles / 1,044 renderers / 576 materials / 25 lights
- collider/hotspot signature equal: **true**
- Tripo credits: **0**
- paid assets in this block: **0**

Visual result:
**FAIL.**
The authored pieces reduce renderer load and are technically cleaner than the helper-heavy Golden Transfer, but the resulting fortress reads flatter and more generic. It loses vertical hierarchy and the richer focal read of Golden Transfer. It does not create a premium-category frame.

## Candidate B — zero-cost external CC0 complete fortress
Asset:
**Blue Banner Castle Courtyard** from 3DAssets.dev / Medieval Castle Construction.

Source page:
https://3dassets.dev/assets/medieval-castle-construction-castle-cour-480ed575-starter-scene

Model:
https://cdn.3dassets.dev/assets/23508/v1/model.glb

License:
**CC0 1.0 Universal**.

Cost:
**0**.

Validation:
- run **36788310853 — SUCCESS**
- artifact **11129889533**
- source HEAD **ae32bf29eef8249c4992788566466323b0b742ff**

Metrics:
- BEFORE: 1,574,646 triangles / 1,199 renderers / 629 materials / 22 lights
- AFTER: 1,178,414 triangles / 1,038 renderers / 576 materials / 25 lights
- collider/hotspot signature equal: **true**
- Tripo credits: **0**
- paid assets: **0**

Visual result:
**FAIL.**
The complete fortified courtyard is coherent and cheap in renderer/triangle terms, but it is visibly low-detail and horizontally dominant. At zoom 12 / 9 / mobile it reads below the Golden Transfer and below the requested premium target.

## Shared material provenance
Experimental PBR family is staged from Poly Haven CC0 assets, matching the proven Golden/Transfer surface system:
- castle_wall_slates;
- wood_planks;
- roof_slates_03;
- cobblestone_floor_001;
- mossy_rock;
- rusty_metal_sheet;
- medieval_wall_01.

No PBR assets are promoted to main in this block.

## Comparison result
Three-way review:
1. current production;
2. Golden Transfer;
3. new hero candidates.

Neither Candidate A nor Candidate B exceeds Golden Transfer.
Candidate A improves technical efficiency but weakens architectural hierarchy.
Candidate B improves single-mass coherence but lowers detail/silhouette quality.

Therefore **the free/local architecture inventory is not sufficient to answer the premium hero requirement**.

## Gameplay safety
Both successful candidates preserve:
- same-scene collider/hotspot signature;
- authoritative processional stair and access;
- gameplay topology;
- production camera family.

No gameplay object is replaced.

## Unity systems actually used
- URP/Lit;
- shared PBR material family;
- mipmapped/normal/AO texture imports;
- visual-only renderer replacement;
- directional light + restrained local warm lights;
- fog/atmospheric separation;
- mesh combining for helper material groups;
- renderer reduction relative to the original transfer.

Not added merely for checklist compliance:
- Shader Graph;
- decals;
- baked lightmaps;
- Light Probes;
- Reflection Probes;
- LODGroup;
- explicit MaterialPropertyBlock;
- occlusion culling.

## Final verdict
**VISUAL FAIL / LOCAL-FREE ROUTE EXHAUSTED FOR THIS PROOF.**

The experiment does not prove that hero architecture is the *only* remaining ceiling, but it does prove that:
- stronger hero architecture matters materially;
- the current production hero is below target;
- replacing it with merely "better pack architecture" is insufficient;
- the target now requires a fortress-specific hero asset with the correct silhouette, surface richness and Eldoria identity.

## Exact missing piece before Tripo
One **fortress-specific Bastion/access hero assembly**, not a generic castle kit:
- monumental central keep;
- deep readable gate aligned to the existing stair;
- two asymmetric flanking towers;
- short connected hero wall returns;
- integrated rock/terrace foot;
- light-stone / slate / timber / restrained blue-gold material language;
- visible large-scale architectural detail that survives zoom 9 and mobile;
- one coherent authored mesh/family, avoiding helper-object proliferation;
- compatible with the frozen gameplay footprint.

This is the only geometry question the next generation should answer.

## Tripo escalation
No Tripo generation was started and no credits were spent.

If the owner authorizes the next block:
1. create one exact isolated reference image for the Bastion/access assembly above;
2. persist exact bytes + SHA in the canonical repo input;
3. stage it in Tripo and show the exact input plus visible Generate cost;
4. stop before Generate for explicit authorization.

Expected cost: **approximately 55 Tripo credits** if the current image-to-3D Studio route presents the same cost as the recently validated production generations. The visible Studio cost must be re-confirmed before Generate; this is an estimate, not authorization.

## Main disposition
Nothing from this branch is merged.
`main` remains unchanged.
