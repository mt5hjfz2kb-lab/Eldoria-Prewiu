# VALORIA SOURCE-LEVEL ART DIRECTION PROOF v1 — RESULT

Date: 2026-10-03  
**TECH PASS / VISUAL FAIL / NOT PROMOTED**

Accepted base: VQB `7a564bc5056bb7166b7704c313ae4db7d55d4530`. Experimental branch: `visual-proof/valoria-source-level-art-direction-proof-v1`. Valid capture implementation: `471dd82eb1acc9c3db9ceb47b84b40eb65276c85`. Run **37155891705 SUCCESS**; visual/source artifact **11286036653**; Blender-source artifact **11285676565**. Planner, Unity gate and focused HUD/progression tests passed, real HUD was captured, gameplay signature stayed intact and Tripo cost was 0.

## Source evidence

Deterministic Blender script: `tools/valoria-source-level-art-direction/build_source_family_v1.py`.

- `Valoria_SLAD_Source_v1.blend` — SHA-256 `b52f63daeba2eb20ba0a6c74c6eeb31d9b9ef064c1844c6b651279de71b80ad3`
- `SLAD_HeroRockInterface_v1.glb` — 1,404 tris — `8b33af961b254fe04c73f95ad7de6e060225268a96f5d8bc7118104afcc20448`
- `SLAD_AserraderoKit_v1.glb` — 1,188 tris — `7b74170470ea574cb5eee457254a9efba096b85ee377b9b47939d2fc05e761c4`
- `SLAD_WallSegment_v1.glb` — 1,620 tris — `bf98b94c33be92ff223f7c3851842c3b631dc817838885d1380a70db2b153f75`

## Stop-gate

Real-HUD zoom 9 and portrait mobile were compared first, as required.

**Zoom 9 FAIL:** the new Aserradero/wall kit reads as a broad pale low-detail mass beside richer canonical sources. The Hero interface reads as a bright added strip, not an integrated rock-to-masonry transition.

**Mobile FAIL:** the Hero strip remains visibly pasted-on/over-bright; the Aserradero sample is mostly outside the crop; hierarchy and premium coherence do not improve.

Therefore no 12/19 extension and no micro-tuning pass were performed.

## Learning and scalability

Shared palette is not shared authorship. Low-detail wrappers amplify mismatch. The fused Hero rock requires semantic source editing or a richer integrated transition, and the canonical Aserradero must preserve its source detail.

The **pipeline is scalable**: Blender source → GLB → Unity → real-HUD capture → gameplay gate works. The **tested art language is not scalable** and must not be propagated to Granero, Cuartel, Forja, Hospital or future Bastions.

A materially different high-fidelity proof is estimated at **6–10 art/technical-art days** for Hero semantic rock/masonry + Aserradero + wall, plus **2–4 engineering/QA days**. After a successful family is frozen, normal secondary pieces are roughly **1.5–4 art days + 0.5–1 integration/QA day** each; Hero/Bastion-class pieces roughly **4–8 art days before QA**.

## Next technique

Do not retry primitive wrappers, ground strips, global tint or the failed shared-atlas approach. A future block should edit the rich sources themselves in Blender: semantic Hero rock ↔ masonry separation without destroying silhouette, source-preserving Aserradero stone/timber/roof re-authoring, and a wall derived from that same high-detail grammar. Validate 9/mobile first.

Accepted visual baseline remains VQB `7a564bc5056bb7166b7704c313ae4db7d55d4530`.
