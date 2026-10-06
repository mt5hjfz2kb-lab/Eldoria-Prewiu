# VALORIA VISUAL AUTHORITY ROUTING GUARD v1

## Purpose

Prevent legacy/procedural Valoria evidence from being mistaken for the current player-facing Valoria.

## Canonical Valoria

The current Valoria beauty authority is the SHARP production line derived from:

- `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`
- production workflow `.github/workflows/valoria-first-playable-city-slice-v1.yml`
- generator `Unity/Assets/Eldoria/Scripts/Editor/ValoriaFirstPlayableCitySliceV1.cs`
- runtime presentation `ValoriaParcelPresentation`

Certified authority evidence:
- visual reset run 37474695980 / artifact 11418118004
- real game state run 37488975868
- real game state final Unity run 37490346723

SHARP owns player-visible beauty; parcel/building progression is layered through the production runtime state assets.

## Legacy procedural Valoria

`Unity/Assets/Eldoria/Scripts/Presentation/VisualWorld.cs` is an original procedural study and remains legacy/fallback/QA only.

It is NOT:
- current Valoria beauty,
- visual promotion evidence,
- an owner-facing current screenshot,
- authority for future visual work.

## Generic Unity benchmark bundle

`.github/workflows/unity-slice.yml` runs `ValoriaBenchmarkCapture.Capture` and uploads `eldoria-valoria-captures-*`.

That artifact is a MIXED REGRESSION BUNDLE. It may include:
- legacy/procedural Valoria views,
- historic/fallback presentation surfaces,
- current World Region 1 captures.

Therefore it must never be presented or interpreted as "current Valoria".

For any request such as:
- current Valoria,
- real Valoria,
- canonical Valoria,
- latest city screenshot,
- show me the city,

use only certified SHARP production evidence or a newer explicitly promoted successor.

## Runtime fallback hardening

Current `SlicePresenter` correctly prefers `ValoriaParcelPresentation` and only falls back to `VisualWorld.Create(true,state)` when the production component is absent.

A fail-closed player guard is now installed at `Unity/Assets/Eldoria/Scripts/Presentation/ValoriaCanonicalRuntimeGuard.cs`. Outside `UNITY_EDITOR`, if scene `Valoria` loads without `ValoriaParcelPresentation`, all existing cameras are disabled and an explicit `BUILD INVALID` overlay is shown. This prevents the legacy city from being silently exposed in a player build.

`VisualWorld` remains available for explicit QA/historical regression only. Future cleanup may remove the fallback branch from `SlicePresenter`, but production safety no longer depends on that cleanup.

The authority-routing guard verifies that:
- SHARP production authority remains fingerprinted,
- `ValoriaParcelPresentation` remains preferred before legacy fallback,
- `VisualWorld` retains explicit legacy/provisional classification,
- generic benchmark artifacts remain forbidden as canonical evidence.

## Automation

Machine-readable authority map:
`pipeline/visual-authority-routing-v1.json`

Checker:
`tools/check-valoria-visual-authority-routing.mjs`

CI:
`.github/workflows/valoria-visual-authority-routing.yml`
