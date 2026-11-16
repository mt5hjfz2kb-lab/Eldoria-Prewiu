# Ashen Citadel — Vigilia interface V1 (functional, reproducible)

**Status:** implemented in the isolated experiment; awaiting actual Unity UI screenshot review. Not merged or adopted as Eldoria's production design.

## Rationale

The original city UI used repeated, heavy gold ornamental frames on every button, high-contrast uppercase labels, a large 100 px HUD and an unrelated account ribbon. On 390×844 the header, eight navigation controls, 190–400 px action sheet and bottom navigation competed for space with the 3D city. Several labels appeared blurred or tiny in the native proof.

Ashen's architectural language (slate roofs, ash-white stone, bronze details and warm interior light) calls for one **restrained, dark-slate and brushed-brass UI system**, not copied game screenshots or a rectangular blue popup from another Eldoria experiment.

## Implementation

- `Runtime/AshenInterfaceIdentity.cs` programmatically generates one coherent 9-sliced panel, primary and secondary buttons and a readable Citadel seal at native source resolution. Source is C# and entirely editable; no external fonts, new paid art, or image-generation placeholders.
- `Runtime/AshenInterfaceGlyphs.cs` supplies eight unique role-specific glyphs for Bastion, buildings, troops, world, mail, bag, heroes and challenges. All remain attached to **existing functional button actions**.
- `Runtime/AshenCitadelExperience.cs` now uses this kit: compact 82 px title bar; 39 px account ribbon; 64 px bottom navigation; full-colour emblem, a thin metallic divider rather than corner clutter; a slim action sheet with a clear title/description hierarchy; calmer responsive text and consistent active/pressed/disabled button states.
- Existing `AshenCitadelExtendedUI`, `LocalGateway`, state/save, unlock rules, combat and mailbox/Bag/Heroes handlers are left intact.
- The original art sprites remain in `Resources` for rollback. One runtime-created kit is shared across all screens; no duplicated prefab assets.

## Visual quality and acceptance

Before approval, inspect real Unity GUI screenshots at 390×844 and 1600×900 for **City, World, Bag, Mailbox and Welcome**. Confirm all eight actions resolve to their correct features and the popup fits inside safe area without occlusion. Source parsing alone is not UI quality. The current code is merely *implemented + Roslyn source QA*, not full UI/art acceptance or physical iPhone certification.

The four navigational labels are still textual to preserve clarity; icons only supplement their meaning, never change navigation. This design should later adopt the same source imagery for building-specific panels and dynamic power/time cues, but do not claim those screens have been redesigned yet.

### Locked art language
- Background: charcoal navy with low-gradient limestone/ash variation.
- Primary material: brushed brass at a narrow border, *not* heavy gold filigree.
- Accent: warmed ivory text; grey-green secondary copy, bronze/tarnished amber icon linework.
- Density: contextual action sheet near the lower interaction zone; no permanent full-screen overlay.
- Touch targets: actual uGUI Buttons with the original action handlers and states.
- Layout invariant: screens with a 390×844 portrait frame preserve the full visual field between header and footer; wide camera frames retain readable HUD widths via center anchoring.

## Gates remaining

Unity native capture, UI click and overlap review, 7 EditMode + 4 PlayMode, plus the existing additional UI photo flows and iPhone physical smoothness. Only a genuine screenshot and interaction test can establish whether this is substantially better in use.
