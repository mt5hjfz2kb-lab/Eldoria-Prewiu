# Eldoria — UI Reference Contract

Status: **CANONICAL IMPLEMENTATION CONTRACT**  
Updated: 2026-09-29

## Authority

The owner-approved mobile UI reference is the visual authority for the player-facing HUD.

It is **not** generic inspiration. Unity must preserve its visual grammar while keeping the existing gameplay wiring, safe-area behaviour and interaction flow.

If temporary development convenience conflicts with the approved reference, the reference wins unless an explicit product decision changes it.

## Current Unity translation — partial, not visual parity

The current player HUD implements a partial styling adaptation through:

- dark compact panels rather than large opaque debug blocks;
- bronze/gold double framing;
- restrained ornamental corner details;
- gold primary-action treatment;
- compact top resource/status bar;
- compact objective/quest panel;
- one dominant guided primary action;
- bottom icon navigation for CIUDAD / MUNDO / HÉROES / ARCÓN / CÓDICE;
- building interaction as a compact bottom sheet;
- player-facing controls separated from QA/debug controls.

The physical art skin is implemented in:
- `Unity/Assets/Eldoria/Scripts/Presentation/ReferenceUiArtPass.cs`

The functional HUD remains owned by:
- `Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs`

## Mobile layout budgets currently locked by PlayMode

- reference topbar: ~68 px
- bottom navigation: ~68 px
- quest panel: <= 66.5 px
- world objective dock: <= 96 px
- building interaction panel: <= 210 px
- primary CONTINUAR action: >= 44 px high

These are implementation guardrails, not permission to drift from the approved visual reference.

## Progression rule

UI follows the same progression contract as world art:

- do not expose buttons, panels, labels or navigation actions for gameplay content before its canonical unlock;
- a future system may exist in code, but its player-facing entry point remains hidden/disabled until the vertical-slice progression allows it;
- UI state and world state must agree.

## QA rule

A UI change is not complete because it compiles.

Before promotion it must preserve:
1. approved-reference visual grammar;
2. one clear next action for the current objective;
3. mobile safe-area and size budgets;
4. working player interactions;
5. progression correctness;
6. PlayMode green;
7. visual evidence from the current build when the change materially affects appearance.

## Anti-regression

Do not:
- reintroduce the old QA action wall;
- replace the approved HUD with generic Unity buttons;
- enlarge panels until they obscure the playable city;
- expose future Bastion systems early;
- treat the approved reference as optional styling.

The repository, approved reference and canonical vertical-slice progression are authoritative over chat memory.

## Direct comparison reviewed 2026-09-30

The owner reiterated that the approved Valoria image is the UI acceptance target, not permission to replace it with generic controls. The reviewed image is the native source `467A0B87-2C56-4587-8E74-927FA3863654(4).jpeg` (Library identity `libfile_58a48f5c575c81918cb6c14e6f4a2bf9`). It is a landscape reference; preserve its art language with responsive placement rather than copying its mockup's later-game values/unlocks into Bastion I.

Current Unity is **NOT visually certified against this image**. Structural PlayMode checks and HUD-free city captures do not prove UI equality.

Remaining visible differences:
- reference has a framed hero portrait and kingdom/power identity; Unity uses a chess-symbol crest;
- reference uses illustrated resource icons; Unity uses text symbols and only the currently implemented wood/stone/power values;
- reference shows a chapter checklist with completion state; Unity shows a single objective text panel;
- reference navigation uses illustrated circular buttons and a prominent Bastion entry; Unity uses rectangular text/symbol buttons;
- reference building labels include compact level and upgrade treatment; Unity's bottom-sheet implementation is only a functional adaptation;
- Héroes/Arcón/Códice are disabled placeholders in the I-II Unity HUD, not migrated functional screens.

Do not expose later systems/resources solely to match mockup decoration. Implement available I-II controls in the approved art language and preserve canonical progression. Require actual HUD-inclusive portrait and landscape evidence before claiming visual parity. The current benchmark capturer explicitly excludes the HUD.
