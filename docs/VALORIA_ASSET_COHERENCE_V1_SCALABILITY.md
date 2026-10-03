# Valoria coherence v1 — upgrade seams and actual limits

The Flat Citadel layout need not be rebuilt to raise future graphical quality. This is a source-architecture assessment; it does not certify six-month performance or commercial visual quality.

| Surface | Replaceable independently | Evidence / mechanism | Limit |
| --- | --- | --- | --- |
| Hero architecture | Yes, visual source only | Dedicated canonical GLB; presentation instance; gameplay colliders/hotspots separate | Current architecture and rock are fused in one mesh, so internal rebuilding needs DCC segmentation or a replacement hero shell |
| Hero rock appearance | Yes | Localized world-height/triplanar mask in ValoriaCoherence shader; canonical geometry/UV untouched | Height is a proxy, not a semantic rock mask; arbitrary hero geometry changes need reclassification and recapture |
| Hero access | Yes | Separate retaining, stair, apron modules in Art Consolidation | Art can change while certified gameplay circulation remains fixed; currently visible seam is not physically unified |
| Functional buildings | Yes | Resource path per dedicated Aserradero/Cuartel/Granero plus fit envelope | Source silhouette/detail differences cannot be removed completely by color calibration |
| Wall / towers / gate | Yes | Quiet wall base and seven outer hero events plus four retaining modules remain independent | Current tower atlas has no normal map, block detail is baked and repetitive; upgrade needs authored maps or interchangeable geometry |
| Surface library | Yes | One response shader with original per-asset albedo/normal; explicit family calibration | This proof does not preserve every imported metallic/roughness/AO channel; production PBR parity remains a real gate |
| Ground / roads / parcels | Yes | Independent low-relief surround, city skin, road and parcel renderers | Replacing ground alone does not alter gameplay collision heights; semantic UV density should become a shared world-space material contract |
| Natural edge | Yes | Separate low-relief and atmosphere outside kernel; authored foliage with reserved gates | Bound to official camera envelope; wider future camera envelope requires extension and new validation |
| NPC / environmental life | Yes | Separate visual-only inhabitants with bounded motion | Current people are proxies, not production characters; no animation rig or path simulation certified |
| HUD | Yes | Real SlicePresenter controls plus separate native presentation component | Unity gameplay remains I-II; mature art + I-II HUD comparison must never be described as later-game playable parity |
| Future districts | Yes | R4/R5/R6, C0/C1/C2, XW/XE/XU/XS protected | No permanent homes/trees in these envelopes; exact late building identity remains gameplay-owned |

## Portability to other Bastions

- Share stone/roof/timber response and palette, wall dimensions, road surfaces, UI hierarchy and ambient activity components.
- Give each hero its own source, bounds and semantic rock mask instead of assuming identical silhouette/height.
- Change visible building tier through PlayerState, never through screenshot-only dressing.
- Preserve expansion openings and interaction envelopes before adding district-specific art.

## Real remaining technical limits

- No representative phone GPU/frame-time/memory measurement in this block. A mobile-resolution Windows capture is visual evidence, not a mobile performance pass.
- No full player build/publication in this experimental block.
- Hero is a monolithic 49,800-triangle optimized source. High-quality independent castle floors/rock segmentation is not available merely because the shader can recolor a region.
- Dedicated functional assets have 2048-square textures, but their baked surface/shape quality differs. Texture size alone is not material quality.
- Current wall hero atlas is 2048x1024 and normal-map-free, and some color maps are 16x16. They cannot reach Hero microdetail through simple roughness changes.
- Native HUD improvements require focused progression/interaction tests and screenshots; they do not complete Unity I-X gameplay migration.
- Advanced decals, shadow/probe/lightmap changes need actual renderer configuration and device budgeting; availability is not certification.

Conclusion: independent visual upgrades are architecturally possible without discarding Flat Citadel. The fused Hero source and heterogeneous baked asset quality are the concrete source limitations. They are upgradeable modules, not proof that the city layout has reached a technical ceiling.
