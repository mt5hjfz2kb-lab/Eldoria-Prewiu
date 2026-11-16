# Ashen Citadel — minimalist premium City HUD + structural art V2

Owner instruction (2026-11-16): the previous UI was artistically too basic, covered the whole City and read like large rectangular boxes. Replace it with **minimal on-screen controls of high graphic quality**, and continue improving the architectural finish. Work must remain in the independent Ashen experiment; do not touch production Unity, beta, or combat logic.

## Before: actual evidence, not references

Genuine native Unity run `43905069629`: before, the permanent full-width top status panel, 4-button account ribbon, bottom 4-button strip and opened rectangular sheet left relatively little visual space for the City. It was technically functional (7 EditMode + 4 PlayMode) but **not** of premium artistic quality. Genuine first-pass minimalist run `43910796844` / source `83fc0f7348598416b3fe171695ed31147f8bd63a` confirmed that separate circular icon controls, a compact upper-left status chip and a small upper-right utility cluster improve the visual hierarchy. However contextual sheets still looked a little oversized and 10-pt navigation labels remained too small.

## Implemented UI V2 (source, isolated branch)

- One 247×58 maximum translucent slate status chip (at the **top left**) with the native citadel crest and readable compact wood/stone/food values.
- Four distinct floating 44-px icon-only medallions in a tiny two-by-two utility cluster (mail, bag, heroes, quests) at the **top right**, no second account bar.
- Four separate bottom medallions with semantics + 12-pt compact labels (Bastion, Buildings, Army, World). **No permanent dark footer background.**
- Contextual action sheets appear only upon interaction; the panel is left-anchored at the bottom, max width 342 and height cap 345 logical px. Minimum height 152; calculated from actual description/row content, so empty Bag/Mail is not a huge blank card. Scroll remains for long actions and nickname input.
- Dynamic success/failure text is shown briefly instead of leaving a permanent message overlay. Existing gateway actions and serialization are untouched.
- 14 committed, SHA256-verified, reproducible authored PNG sprites include the refined transparent bronze medallion and separate status chip. Actual Unity import uses native 9-slice, not nonpersistent procedural textures. Marcellus titles and Alegreya Sans remain the verified font family.
- Source tests and genuine Unity PlayMode screenshots now include both **CityIdle** (no modal; request's key requirement) and City, World, Bag, Mail and Welcome contextual states, on 390×844 and 1600×900. Layout assertions ensure no huge top/footer/utility strip survives.

## Implemented architecture V2 (source, isolated branch)

`Editor/AshenArchitecturalFinishing.cs` adds *non-interactive, physically authored geometric features* via one deterministic mesh asset per stage/building:
- Historic tower–great hall: continuous occupied oak connection, defensive catwalk tied to wall, carved gate arch and cloth, and a single final-stage forged finial instead of duplicated roofs.
- Sawmill: integrated load-bearing log-hoist and timber stacks beneath its existing long asymmetric roof.
- Garrison: reinforced structural gable, carved crest, occupied smoke stack and cohesive heraldic banners.
- Granary: continuous silo staves and integrated transfer beam; less isolated cylinder-over-box effect.
- Stoneworks: an actual large winding wheel with braced crane and dressed-stone stockyard, not a collection of unrelated blocks.

All features reuse existing original Ashen stone/timber/brass/slate material assets. No third-party art, new gameplay interactions, new colliders, or paid services. The five original OBJ base stages and authoritative progression remain untouched.

## Verification and quality gates

Previously verified: authentic first-pass minimalist native screenshots `43910796844` (10 I–V camera + 10 contextual UI screenshots, 11 Unity tests). V2 changes and architecture enhancements are *implemented in source* but require a new exact-source native Unity proof. This is a **major structural/ui layout revision**, not a recolour. For V2 require:
1. Genuine C# compile, 7 EditMode + 4 PlayMode, captured real uGUI.
2. All five Bastion stages plus CityIdle / City / World / Mailbox / Bag / Welcome at native iPhone portrait 390×844 and desktop 1600×900.
3. Visually inspect city occlusion, **actual unlocked/locked controls**, 8 distinct icon meanings, popup close/scroll, no clipping, stone/roof geometry joins, no artificial platforms or broken shadow silhouettes.
4. Compare before/after at same camera; reject if UI still overshadows architecture.
5. iPhone physical FPS/memory and WebGL remain unverified, so do not mark `ART_APPROVED` without separate evidence.

The main source is deliberately left unchanged. Preserve this contract if the task moves to a new chat.
