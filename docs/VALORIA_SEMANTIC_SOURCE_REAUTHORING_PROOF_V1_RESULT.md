# VALORIA SEMANTIC SOURCE REAUTHORING PROOF v1 — RESULT

Date: 2026-10-04  
**TECH PASS / VISUAL FAIL / NOT PROMOTED**

Accepted visual baseline remains VQB `7a564bc5056bb7166b7704c313ae4db7d55d4530`. No production/gameplay/macrocomposition change is promoted.

## Final evidence

- Experimental branch: `visual-proof/valoria-semantic-source-reauthoring-proof-v1`
- Valid captured implementation: `0631016c7ef2ade1d04d77628364db86bdc20029`
- Valid run: **37161308188 SUCCESS**
- Integrated visual/source artifact: **11287623524**, digest `sha256:7fce8cdec138c0d61a1a69fa31ed2690ee69438c4bb7225c6bd847524261c341`
- Blender-source artifact: **11287494742**, digest `sha256:6f620c39b456ac86926ccddbb0f2937956714a1ed96e6f611c81244119f36ee0`
- Blender source: `pipeline/candidates/valoria-semantic-source-reauthoring-v1/Valoria_SSRA_Source_v1.blend`
- Exported GLBs:
  - `SSRA_HeroBastion_v1.glb` — 13,106,964 bytes
  - `SSRA_Aserradero_v1.glb` — 11,137,896 bytes
  - `SSRA_WallSegment_v1.glb` — 12,407,248 bytes
- Unity replacement implementation:
  - `Unity/Assets/Eldoria/Scripts/Presentation/ValoriaSemanticSourceReauthoringV1.cs`
  - `Unity/Assets/Eldoria/Scripts/Editor/ValoriaSemanticSourceReauthoringGateV1.cs`

The successful gate preserved the gameplay collider/hotspot signature and completed the focused gameplay checks. No Tripo generation, paid add-on or paid art service was used.

## What was actually changed

### Hero Bastion

The canonical rich Hero was edited directly, not merely covered in Unity.

- source: 49,800 triangles;
- low-band semantic surgery: 840 disconnected components removed;
- removed: 5,125 triangles = 10.29% of source;
- output: 45,755 triangles including authored transition geometry;
- source basecolor 2048², normal 2048², RM 1024² retained on surviving source geometry.

The low 14% removal itself is technically reproducible and preserves the recognizable upper silhouette. The failure is the replacement transition: architectural retaining pieces reuse source texture response on newly authored geometry without an authored UV/material treatment appropriate to those shapes. At game scale this creates a giant horizontally stretched, visually false band that overwhelms the Hero.

### Aserradero

The canonical 49,800-triangle source mesh was preserved in full, including UVs and embedded PBR textures.

Semantic component zoning:
- stone: 22,358 tris;
- timber: 7,674 tris;
- roof: 3,698 tris;
- untouched/source: 16,070 tris.

This is the strongest positive finding of the proof. The Aserradero remains rich and its functional parts read with greater separation/warmth instead of becoming a simplified block. It demonstrates that **component-level semantic zoning of rich single-material generated assets is viable**.

### Representative wall

A source-derived rich wall output was produced at 6,440 triangles with stone/foundation/cap material zones and 2K/1K PBR maps. It is technically usable, but the proof cannot claim a coherent high-fidelity wall family because the integrated frame is already rejected by the Hero stop-gate.

## Required zoom 9 / mobile verdict

The proof deliberately stops at zoom 9 + mobile as specified.

### Zoom 9 — FAIL

BEFORE is the accepted VQB frame. AFTER is an obvious regression:
- the Hero grows into a huge horizontal textured retaining mass;
- architecture no longer feels fused naturally to rock;
- monumental hierarchy becomes visually top-heavy and artificial;
- the transition attracts more attention than the Bastion itself;
- scene coherence is worse at first glance.

The Aserradero is locally acceptable/better separated, but that local gain cannot offset the Hero regression.

### Mobile home — FAIL

The same defect is stronger in portrait:
- oversized horizontal Hero transition dominates the available city crop;
- rock→masonry does not read as a believable structural transition;
- the player-facing frame is materially worse than VQB.

### Mobile Aserradero pan — PARTIAL POSITIVE

The Aserradero preserves source richness and gains warmer semantic separation. This supports reusing the zoning technique on comparable secondary buildings, but it does **not** satisfy the proof criterion that Hero + Aserradero + wall look clearly like one premium family.

Per the stop rule, no 12/19 extension was run.

## Sharpness / pixelation assessment

Source and Unity evidence show no primary resolution failure:

- Hero: basecolor 2048² / normal 2048² / RM 1024²;
- Aserradero: basecolor 2048² / normal 1024² / RM 1024²;
- wall: basecolor 2048² / normal 2048² / RM 1024²;
- Unity import reports mip chains present;
- filtering: Trilinear;
- measured in-scene source density is ample for the tested camera scale.

Therefore the major AFTER defect is **not** low texture resolution or ordinary mip pixelation. It is incorrect texture/UV/material use on newly authored Hero transition geometry plus composition/form scale. Increasing texture resolution would not fix it. Anisotropy is only 1 in the audit; that could matter for oblique surfaces later, but it is not the cause of this rejection.

## Cost of the technique

Direct external spend in this proof:
- Tripo credits: **0**
- paid add-ons: **0**
- paid asset purchases: **0**

The successful end-to-end CI run took roughly ten minutes wall-clock after several short technical authoring retries. The real production cost is artist/technical-artist time, not compute.

Observed cost structure:
- secondary building like Aserradero: semantic zoning can be automated partly, but classification still needs per-asset review and corrections;
- Hero/Bastion-class asset: requires asset-specific source surgery, transition design, dedicated UVs/materials, likely rebake and camera validation;
- wall/modular family: reusable once a successful stone/trim grammar is authored.

Planning estimate remains consistent with prior evidence:
- ordinary secondary piece after a material/trim grammar exists: ~1.5–4 art days + 0.5–1 integration/QA day;
- Hero/Bastion-class source reauthoring: ~4–8 art days before QA;
- first coherent Hero + secondary + wall family: roughly 6–10 art/technical-art days plus 2–4 engineering/QA days.

These are planning estimates, not money already spent.

## Scalability answer

**Can this technique later be applied to Granero, Cuartel, Forja, Hospital and future Bastions without rehacer cada asset desde cero?**

**Partially, not universally.**

Reusable:
- Blender import/audit/export pipeline;
- connected-component analysis;
- semantic material zoning framework;
- source UV/PBR preservation;
- shared stone/timber/roof material response;
- Unity replacement/gate and 9/mobile stop-gate;
- future shared trim/material/bake recipes once visually approved.

Per-asset work still required:
- semantic classification corrections;
- proportion/form decisions;
- UV treatment for newly created geometry;
- local masks and wear;
- building-specific roof/support logic.

Hero/Bastion-class work is **not** a cheap automatic application. Rock/architecture topology and transitions are asset-specific. Future Bastions could reuse the workflow and material grammar, but each major Hero transition needs deliberate authoring.

Primary bottlenecks:
1. generated single-mesh/single-material sources with thousands of disconnected components but no semantic labels;
2. deciding which loose pieces are architectural vs rock by geometry alone;
3. UV/material authoring on genuinely new geometry;
4. Hero transition design at real screen scale;
5. human visual judgment remains necessary after automation.

## Central answer

> “¿Podemos conservar nuestros assets ricos y convertirlos, mediante edición profunda de sus fuentes, en una familia visual coherente sin perder su calidad?”

**We can preserve and semantically reauthor rich secondary assets; this proof demonstrates that with Aserradero. We have not demonstrated that the same approach automatically turns the Hero + secondary + wall into a coherent family. The Hero transition failed visibly. Therefore the answer for the full family is currently NO / NOT YET PROVEN.**

This is not evidence that Blender source editing is useless. It is evidence that **semantic zoning is scalable for rich secondary assets, while Hero-class rock→architecture surgery needs genuine authored UV/material/form work rather than automated component cutting plus reused source textures.**

## Recommendation

Do not promote this candidate and do not expand it to Granero/Cuartel/Forja/Hospital.

Keep VQB as accepted visual source. Preserve the useful technique as a future secondary-asset tool, but do not open another automated Hero micro-iteration. The next Hero attempt, if authorized as a new block, should be a **manual/high-fidelity Hero transition wedge**: one small camera-facing section authored with dedicated masonry UVs/materials/bake and no reused Hero texture on new architecture. Only after that isolated wedge looks correct at game scale should it replace more of the fused rock.

No production migration is performed by this proof.


## Final variant 2 validation — definitive closure

After the first zoom9/mobile rejection, one materially different Hero technique already present in the same proof was validated once, then stopped under the anti-loop rule.

Variant 2:
- captured implementation: `0556cf8e6b6a2b54a3bee75ad83067facacd72e5`;
- run: **37162147598 SUCCESS**;
- integrated artifact: **11288451036**, digest `sha256:524ee77cfb25345d10939ecf2e2467dd152e3dac4832505f7fbb3f7e0dd1cf59`;
- Blender-source artifact: **11288171793**, digest `sha256:a31edc7ebc32ba52c578952f48f1ca59dc99237191c22e1b4091e1f9a23d4325`.

This variant removed only 421 low-front Hero components / 3,383 triangles (6.79%), then embedded the certified rich `RockToWallTransition` geometry inside the Hero export. The transition itself contributes 7,270 triangles and preserves its own 2K basecolor / 2K normal / 1K RM maps. Hero output becomes 53,687 triangles. Aserradero remains the same full 49,800-triangle source-preserving semantic zoning candidate.

**Variant 2 VISUAL FAIL:** the rectangular/prism band from variant 1 disappears, but the rich transition scales/orients into an oversized vertical rock mass that occludes the Hero, destroys the focal hierarchy and dominates zoom 9 and mobile even more severely. It does not read as controlled rock→masonry integration.

Therefore two materially different Hero approaches have now failed:
1. new retaining architecture using reused Hero material/texture response → horizontal stretched artificial band;
2. rich RockToWallTransition embedded in the Hero export → oversized vertical rock mass / occlusion.

No third microvariant is justified. No 12/19 extension is run. Final proof verdict remains **TECH PASS / VISUAL FAIL / NOT PROMOTED**.

The positive result remains narrow but useful: source-preserving semantic component zoning works for Aserradero-class secondary assets. The negative result is equally important: Hero-class source surgery cannot be made scalable by automatic component cutting plus automatic fitting of existing transition geometry. A future Hero attempt must use deliberate camera-authored modeling with controlled local proportions, dedicated UV/material treatment and likely a manual bake; it should be treated as real Hero art production, not another parameterized repair pass.
