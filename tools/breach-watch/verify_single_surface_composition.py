#!/usr/bin/env python3
"""A reproducible audit of why Vigilia's painted UI felt stacked.

The owner-approved source sprites already contain opaque ink/stone and
finished contact shadows. Their double uGUI Shadow and a fully painted
secondary portrait backplate caused an additional 'card on card' silhouette.
Do not infer ART_APPROVED from this source audit.
"""
from pathlib import Path
from PIL import Image

ROOT=Path("Unity/Assets/Eldoria/BreachWatch")
HUD=(ROOT/"Runtime/BreachWatchHud.cs").read_text(encoding="utf-8")
QA=(ROOT/"Editor/BreachWatchNativeEvidence.cs").read_text(encoding="utf-8")
ART=ROOT/"Resources/VigiliaArt"

for relative in (
    "OwnerControls/profile_bar.png",
    "OwnerControls/resource_capsule.png",
    "PremiumSurfaces/chiseled_quest.png",
    "PremiumSurfaces/panel_fill.png",
    "PremiumSurfaces/brass_portrait.png",
):
    with Image.open(ART/relative) as source:
        source=source.convert("RGBA")
        opacity=source.getpixel((source.width//2,source.height//2))[3]
        assert opacity==255,(relative,"center pixel already opaque",opacity)

# The one piece of imported brass_portrait already has an opaque middle.
# It must not be blended as a second 3D picture card under transparent URP
# actual building geometry.
assert "brassFrame.enabled=false;" in HUD
assert "brassFrame.raycastTarget=false;" in HUD
assert "!brassWells[0].enabled" in QA

# Every main surface owns its own original metal shading. Extra Unity
# shadows added to those same Graphics duplicate their silhouettes.
assert HUD.count("AddContactShadow(")==1, "Extra painted shadow calls remain"
# Regression found after shadows were removed: HasSinglePremiumIdentity
# still required a Shadow, so real Unity would fail even though Roslyn passed.
assert "!identity.GetComponent<Shadow>() &&" in HUD, (
    "Single-profile assertion silently depends on a deleted overlay")
assert "identity.GetComponent<Shadow>() &&" not in HUD.replace(
    "!identity.GetComponent<Shadow>() &&", ""), (
    "A duplicate contact shadow was reintroduced as a pass criterion")
assert "Need(depthLayers.Length==0" in QA
# Dusk A/B must darken only the real world backdrop, not add an
# unclickable modal or tint the source UI art. The atmospheric mesh was
# already the FIRST canvas child before the profile and card.
atmosphere=(ROOT/"Runtime/BreachWatchAtmosphere.cs").read_text(encoding="utf-8")
assert HUD.index('Rect("Soft volcanic atmosphere / no world replacement"') < HUD.index('identity=Rect("Identity / basalt crest"')
assert "raycastTarget = false;" in atmosphere
assert "float alpha=.008f+.070f*ring*ring+" in atmosphere
assert ".019f*Mathf.Pow(dy,3f);" in atmosphere
assert "new Color(.025f,.025f,.027f," in atmosphere
assert "Mathf.Clamp(alpha,0f,.105f)" in atmosphere
# Source formula keeps screen center at 8%, peripheral tint <=28% and
# *cannot* replace the actual terrain, building meshes or cameras.
center_alpha=.008
corner_alpha=.008+.070+.019
assert 0<=center_alpha<=.012 and corner_alpha<=.105
for name in ("identitySurface","resourceSurface","chapterStone","singleSlate","surface"):
    assert name+".color=new Color(.97f,.95f,.90f,1f);" in HUD, (
        name, "an inconsistent outer material palette returned")
assert "float crestSize=smallPhone?45f:37f;" in HUD
assert "At(identityCrest,4,5,48,48);" in HUD
assert "profileLion.rect.width<=profilePlate.rect.height-4f" in QA
assert "owner_gold_border" in HUD and "panel_fill" in HUD
assert "exactOwnerGoldBorder.type=Image.Type.Simple;" in HUD

# A separate screen-space Canvas can still paint a legacy panel even when
# the RoyalHUD canvas has been disabled. Inspect the COMPLETE actual Unity
# hierarchy on every native run, not merely the canvas we know about.
evidence=(ROOT/"Editor/BreachWatchNativeEvidence.cs").read_text(encoding="utf-8")
assert "AuditActualCanvases" in evidence
assert "activeForeignScreenSpaceCanvases" in evidence
assert 'c.renderMode=="ScreenSpaceOverlay"' in evidence
assert 'c.renderMode=="ScreenSpaceCamera"' in evidence
assert "canvasDiagnostics=canvasDiagnostics" in evidence
assert "oldCanvasVisible = canvasDiagnostics.Count" in evidence
assert "oldHudGraphicsReused = canvasDiagnostics.Any" in evidence
assert "foreignScreenSpaceCanvasPaths=foreignScreenSpace.Select(c=>c.path).ToArray()" in evidence
assert "Math.Max(shots.Max(x=>x.activeForeignScreenSpaceCanvases)" in evidence
# The original etched surfaces are their OWN visual components. Two cheap
# vector castle badges had been pasted ON TOP of their already-sculpted
# metal. They contribute no new gameplay information beyond the live titles.
assert 'Glyph(objective,"Chapter stone rune"' not in HUD
assert 'Glyph(panel,"Building crest"' not in HUD
assert "chapterSigil==null" in evidence
assert "engravedBastion==null" in evidence
# Press/hover must tint the ONE existing owner sprite, never the
# transparent hit mask and never another floating image.
assert HUD.count("UseSourceArtInteraction(")==4, (
    "One helper plus close/action/nav art binding must exist")
assert "button.targetGraphic=paint;" in HUD
for clause in (
    "UseSourceArtInteraction(closeButton,closeIcon);",
    "UseSourceArtInteraction(actionButton,actionPlate);",
    "UseSourceArtInteraction(button,surface);",
):
    assert clause in HUD,("Missing real painted interaction response",clause)
assert "button.transition=Selectable.Transition.ColorTint;" in HUD
assert "ui.BuildingAction.targetGraphic==actionArt" in QA
assert "closeRoot.targetGraphic==sourceX" in QA


print("VIGILIA_SINGLE_SURFACE_SOURCE_PASS (Unity external canvas census pending native, not ART_APPROVED)")
