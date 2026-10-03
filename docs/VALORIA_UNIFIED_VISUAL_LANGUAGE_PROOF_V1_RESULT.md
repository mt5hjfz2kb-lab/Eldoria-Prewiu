# VALORIA UNIFIED VISUAL LANGUAGE PROOF v1 — closure

Date: 2026-10-03. **TECH PASS / VISUAL FAIL / NOT PROMOTED.** The limited proof did not establish a coherent premium visual language ready to extend across Eldoria. Both reversible connector techniques ran in real Unity and failed integrated visual review. The last accepted visual presentation is still VQB `7a564bc5056bb7166b7704c313ae4db7d55d4530`; Flat Citadel composition, gameplay, reservations and source Hero identity are unchanged.

## Exact provenance and execution

- Canonical repo `mt5hjfz2kb-lab/Eldoria-Prewiu`. Entry main `7d384b6120365b34fc93d39288a69d2b57df47a2`, workstream claim `550b479cfa162ddb0a79921647ec2fa450437456`.
- Experimental branch: `visual-proof/valoria-unified-visual-language-proof-v1`, created from VQB accepted `7a564bc5056bb7166b7704c313ae4db7d55d4530`. Source includes previously accepted Flat Citadel proof/uplift/consolidation/asset coherence/VQB layers. Authored Secondary Art Family v1 `TECH PASS / VISUAL FAIL / NOT PROMOTED` is precedent, not part of this accepted scene.
- Variant 1 `24ac0be70f9f5199686c1769303dfc5e510e1420`: [run 37152534859 SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37152534859), artifact `11284955622`, SHA-256 `b47e77bd2a144960dacd9b7662b3f5a3982ef3f1d7cbefead2c71b906b899895`. Exterior crop/road strips rejected after screenshot review.
- Variant 2 `89ca7efc279b9c845aa746f1396755066d211320`: [run 37152713806 SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37152713806), artifact `11284248212`, SHA-256 `3c3f2214fe799a5530750db62cd9166862336f737b53b37628407029667f0574`. Civic stone access + temporary prepared soil proof rejected after screenshot review.
- Reused `.github/workflows/valoria-visual-quality-breakthrough-v1.yml` on the experimental branch and Unity 6000.3.23f1 self-hosted Windows runner; execute `Eldoria.EditorTools.ValoriaUnifiedVisualLanguageGateV1.Capture` after staging the source CC0 maps. Planner route `environment_composition` passed; workflow governance passed. No new parallel pipeline or paid tool.
- Captured source `ValoriaUnifiedVisualLanguageV1.cs` and `ValoriaUnifiedVisualLanguageGateV1.cs` are experimental/reproducible and have **no runtime integration**. The final branch parks the gate manual-only; request disabled. No gameplay/visual promotion or public owner build.

## Real evidence and direct comparison

Run `37152713806` contains original Unity PNGs `before`, `after`, `before-game`, `game` at **19, 12, 9 and portrait mobile 390×844**, plus `gameplay-home.png` at runtime orthographic 9.1 and the metrics/test logs. The paired previews below only resize and place the original live-HUD images side-by-side. [Manifest with PNG SHA-256](evidence/valoria-unified-visual-language-proof-v1/manifest.json).

| Official camera | VQB accepted → proposed variant 2 | Assessment |
| --- | --- | --- |
| [19 strategic](evidence/valoria-unified-visual-language-proof-v1/compare-19.jpg) | Small city amid controlled green surround. | Applicant is near-identical; exterior continuity problem remains. Variant 1 fields were technical strips and were removed. |
| [12 city](evidence/valoria-unified-visual-language-proof-v1/compare-12.jpg) | Hero and source buildings read, reserves open. | Civic paths lack enough visibility; slight dark seams near access. No scene-wide language jump. |
| [9 detail](evidence/valoria-unified-visual-language-proof-v1/compare-9.jpg) | City largely fills width. | Hero and secondary construction vocabularies remain different; ground interventions are local. |
| [Mobile portrait](evidence/valoria-unified-visual-language-proof-v1/compare-mobile.jpg) | Near full-width cropped view, real HUD. | No distinct visual improvement; Hero rock, wall and source mix remain. A full-city fit would make click targets tiny; bounded pan is part of the existing camera contract. |

The approved reference image depicts dense layered inhabited masonry, varied vegetation, coherent roofs/trim, warm lighting, surrounding world continuation and a polished HUD. Current proof has a clear Hero focal point and authentic functional sources but far less environment storytelling, material/architectural unity, life and HUD richness. The reference supplies quality, hierarchy and occupancy direction; its literal terrain/layout is not to be copied onto locked Flat Citadel. No repository-hashed reference bitmap exists in the benchmark doc, so this comparison is qualitative. Independent authenticated external reference bytes were inspected, not presented as canonical embedded bytes.

## Screen-space measurement and decision

[Projection method and values](evidence/valoria-unified-visual-language-proof-v1/camera-metrics.json): project the defensive ring's X `-9.5…+9.5`, Z `-6.4…+9.3` bounds from the fixed capture camera `(18.2,18.4,-26.8)` looking at `(0,1.55,1.55)`. These are approximate projected **ring widths**, not image segmentation or gameplay occupancy claims:

| View | Projected ring width / viewport | Interpretation |
| --- | ---: | --- |
| 19 / 1280×720 | 36.2% (bounding area ~10%) | Intentional strategic overview; not the everyday main gameplay scale. |
| 12 / 1280×720 | 57.4% (bounding area ~25%) | Everyday camera leaves substantial exterior and reserved open space in view. |
| 9 / 1280×720 | 76.5% (bounding area ~45%) | Detailed city presentation occupies most horizontal frame. |
| 9.4 / 390×844 | 281.7% | Portrait crops city by design; bounded pan and touch access must serve offscreen plots. |

The **real runtime city home zoom is 9.1**, established in `VisualWorld.Create`, with fixed orientation and pan/zoom governed by `SlicePresenter`. The official capture at 19 should not be mistaken for its initial playable view. Screen-space scale matters and is not fixed by scaling all geometry or forcing the full city into a narrow portrait. The major remaining difference from the approved reference is not only apparent width: at home scale the field inside and beyond walls reads unoccupied, while Hero, wall and secondary assets still have different design grammar. We preserve 9–19 until a representative device interaction test supports revising it. The new runtime-home capture is evidence of frame, not a new playable build.

## Visual and technical verdict

| Criterion | Result |
| --- | --- |
| Same-world Hero/secondary/wall/ground | **FAIL** — inherited rich sources and generic/support geometry remain visibly heterogeneous. |
| Hero rock integrated with city | **FAIL** — accepted VQB wrap retains a prominent fused base; local access patch was visually worse. |
| City dominates at main scale | **PARTIAL** — runtime 9.1 covers ~76% projected width; mobile is a city crop, not a grass miniature, but the played frame is sparse. |
| Contained believable exterior | **FAIL** — first connected road/fields appeared as boardlike dark bars; rollback restored VQB. |
| Real HUD and mobile | **TECH PASS / VISUAL PARTIAL** — real UI, interactive tests; reference richness not reached. |
| Reproducible pipeline and protected growth | **PASS** — matched captures and gate; 3 future Arc-I plots, C0/C1/C2 and XW/XE/XU/XS retained. No presentation colliders or hotspots. |
| Significant visual step, scalable unified language | **FAIL / UNPROVEN** — no source-family production system emerged from these candidate screenshots. |

Both capture workflows reached SUCCESS. Run 2 has **5 total / 5 passed / 0 failed** focused PlayMode tests; capture evidence preserves gameplay collider/hotspot signature and macro composition. `language-metrics.json`: 5 connector meshes, 3 temporary ground sections, zero visual colliders. This proves technical reversibility, **not** production art quality or frame-rate. The mobile PNG is Windows at mobile resolution; no representative phone GPU/CPU/memory/overdraw profiling or full player build was run.

## Applied methods, stop rule and scalability

Full [research, tool decisions, rejected variants](VALORIA_UNIFIED_VISUAL_LANGUAGE_PROOF_V1_RESEARCH.md). Existing Unity URP shader/PBR source, authored functional GLBs, VQB wall rhythm and deterministic capturer were used. No new Blender file was produced; prior authored-secondary Blender source proved semantic segmentation viable but its image failed. No Tripo credits, purchase, new package or plugin was consumed. The visual-only code is kept as an experimental record, while final accepted scene remains VQB. After two distinct external versus civic geometry attempts yielded net-negative or marginal full-frame output, the family is closed under the anti-loop rule. Do not multiply strips, recolor the Aserradero again, or deploy the failed atlas.

**Six-month answer:** yes for *architecture and tooling seams*, not yet yes for this *specific art language*. Game state, layouts, camera, parcels, source GLBs, wall caps, Hero approach modules, shaders and HUD skin can be replaced independently without reconstructing Valoria. The fused Hero rock cannot be edited as an independent source piece until separated/re-authored; texture and roughness maps do not solve that silhouette. The bottleneck is a jointly authored source-rich architecture/ground/environment vocabulary judged in the played camera, with a deliberately art-directed density policy that honors future plots. No evidence supports a Unity engine ceiling or mandatory mountain/global layout rebuild.

A rough planning estimate to *test*, not promise, a serious next source-authored one-cell system: **8–15 artist/technical-artist working days** for style sheet, semantic Hero interface and Aserradero/wall/ground trim/mask kit; **3–6 engineering/QA days** for source import, runtime camera envelope, mobile profiling and 19/12/9/mobile gates; add iteration contingency if the first full-frame review fails. This estimate assumes existing Blender/Unity skills, canonical source access and no paid asset generation. Scaling to all named future buildings or other Bastions cannot be priced from this failed proof and is explicitly withheld.

Precise next block: **VALORIA AUTHORED GAMEPLAY CELL ART DIRECTION v1**. First commit a screen-space art sheet grounded in the approved quality reference and fixed Valoria parcels, then author one truly shared stone/roof/timber/retaining/ground source family **at preserved Hero-level visual density**, including semantic Hero rock interface; validate its gate/plaza/Bastion/Aserradero/wall cell with real HUD at main desktop and portrait camera plus pan extremes. Only if that significant screenshot wins should Cuartel/Granero and other Bastions be budgeted. Do not assume that this candidate has been implemented or approved.

Final branch is experimental; **NOT PROMOTED**. The best current real visual remains VQB; no new production runtime/public playtest contains this failed proof.
