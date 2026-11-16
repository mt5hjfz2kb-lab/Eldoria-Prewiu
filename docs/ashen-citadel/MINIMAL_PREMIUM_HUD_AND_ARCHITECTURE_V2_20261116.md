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

## Final verified Unity V2 — 16 November 2026

- Actual native Unity job **43915637621 — SUCCESS**, exact source `e69d858a3efe7e760cabb0aa6f10ddde62fc080c`; artifact `14249927459`; archived artifact SHA256 `5671267c44a22c5b20f6cf9365c8f695c6de849d9b4cf55056c94c37cc127d9c`.
- Native `Ashen-EditMode-Results.xml` **7/7 passed** and `Ashen-PlayMode-Results.xml` **4/4 passed**, zero skips/failures. The original proof file records 10 authentic Unity I–V screenshots (5 stages × horizontal/portrait), all ten PNG SHA256 verified; additionally, 12 genuine composited uGUI PlayMode screenshots are present for **CityIdle, City, Welcome, World, Bag and Mailbox**, both 390×844 and 1600×900.
- **Actual visual inspection:** `Ashen_UI_CityIdle_390x844.png` shows the small Bastion I courtyard and keep occupying far more of the display, unimpeded by the old solid top and bottom bands. Top-left dark material chip, four independent upper-right utility medallions and four lower-left/right game navigation medallions are coherent; the 12 source PNG assets and Marcellus/Alegreya font family are no longer transient white textures. The original huge dark bars are demonstrably gone.
- The contextual `Ashen_UI_City_390x844.png` opens a **temporary** top-left inspector rather than a permanent panel, with the true gameplay actions and source data. It still overlaps part of the historical tower roof while open, and its descriptive text is small. **That remains an artistic/usability limitation, not a technical defect or final design approval.**
- `Ashen_Bastion_5_1600x900.png` confirms the new connected wooden catwalk, guard gallery, forge/quarry crane and distinctive secondary economic structures. Previous orphan floating finial, roof-crossing gable poles and silo staves were removed after inspecting the first native geometry candidate.
- **Decision:** This is a **substantial improvement in screen visibility** and architectural character, with genuine engine proof and passing game regressions. It still uses economical stylized modular geometry and has unresolved typography density/context inspector occlusion; the workstream remains `ART_APPROVED=false`. **Physical iPhone, WebGL, memory and thermal profiling not tested.** Main game and beta unchanged.

### Accumulated QA lessons

- Art quality gates must inspect **CityIdle** as well as City with an opened panel: the previous screenshot suite only tested contextual states and could incorrectly pass a blocked City view.
- Generated/sliced Unity textures must exist as reproducible imported sprites in the isolated Resources folder; temporary `Sprite.Create` produced solid white UI despite passing functionality.
- Hiding full-width HUD backgrounds and offering smaller metal medallions raises actual **3D screen exposure** more than another ornate full-screen UI reference.
- Architecture micro-props can create floating sticks; inspect their world intersections in real Unity, remove crossing/detached elements rather than merely changing materials. This work is art source backed, not a concept render.
- All evidence is tied to exact source SHA, not a later documentation commit. Native UI screenshots are Editor-camera composited PlayMode captures, **not** GameView on an actual iPhone.
