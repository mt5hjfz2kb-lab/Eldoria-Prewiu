#!/usr/bin/env python3
"""Read-only dimensional guard for real-city camera composition (not Unity proof).

The owner-approved 864x1536 image frames Bastion at ~one third screen width,
with the entire bridge and river below. Earlier native Unity 390x844 shots
measured the Bastion close to 70% width. These mathematical predictions
stop a future edit from inadvertently removing our reversible framing, but
they do NOT replace in-engine camera/terrain and pixel tests.
"""
from pathlib import Path
import math

SRC=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchCityComposition.cs")
HUD=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs")
EVIDENCE=Path("Unity/Assets/Eldoria/BreachWatch/Editor/BreachWatchNativeEvidence.cs")

def run():
    code=SRC.read_text()
    hud=HUD.read_text()
    evidence=EVIDENCE.read_text()
    for mandatory in [
        "1.78f*ratio","1.67f,2.65f",
        "float goalWidth=aspect<.50f?.43f:.40f",
        "float currentWidth=maxX-minX",
        "cityCamera.orthographicSize*",
        "nextZoom,cityCamera.orthographicSize,65f",
        "cityCamera.orthographicSize=Mathf.Clamp(originalZoom*factor,45f,65f)",
        "originalZoom=cityCamera.orthographicSize",
        "originalPosition=cityCamera.transform.position",
        "host.City.Controls.FitToTerrain()",
        "host.InWorld","camera","WorldToViewportPoint",
    ]:
        assert mandatory in code,f"City composition safety contract removed: {mandatory}"
    assert "BreachWatchCityComposition" in hud and "cityComposition.Layout(pixelWidth,pixelHeight)" in hud
    assert "void OnCityReentered(" in code and "lastWorldMode && !host.InWorld" in hud
    assert "cityComposition.OnCityReentered(Screen.width,Screen.height)" in hud
    assert "castleViewportWidth" in evidence and "Approved panoramic portrait city view" in evidence
    assert ".homeZoom=" not in code and ".WorldNavigation" not in code,(
        "Standalone layout must never modify gameplay controller presets or World camera")

    # A predicted camera magnification is not a screenshot measurement.
    # The native GPU test uses the actual Bastion mesh projected bounds.
    for width,height in ((864,1536),(900,1600),(390,844),(1170,2532)):
        aspect=width/height
        zoom_factor=max(1.67,min(2.65,1.78*(.5625/aspect)))
        target_width=.43 if aspect<.50 else .40
        assert 1.67<=zoom_factor<=2.65
        assert .39<=target_width<=.43
        print("VIGILIA_CITY_COMPOSITION_SOURCE_PASS",
              f"{width}x{height}",f"initial_magnification={zoom_factor:.3f}",
              f"true_model_goal_width={target_width:.2f}")
    assert "PortraitCityFramed" in hud
    portrait=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchBuildingPortrait.cs").read_text()
    assert portrait.count("portraitCamera.Render();")==1, (
        "Premium building diorama must not redraw a fake ground image over genuine 3D meshes")
    assert "Vigilia / one-shot earth under real building" not in portrait, (
        "No rectangular synthetic grass stage may reappear behind the real castle")
    assert "portraitCamera.clearFlags=CameraClearFlags.SolidColor;" in portrait
    assert "PortraitGeometryClipped" in portrait and "LastGeometryUvBounds" in portrait
    print("VIGILIA_TRUE_3D_NEUTRAL_DIORAMA_SOURCE_PASS no fake earth, single URP render")
    print("Requires real Unity render: terrain clamps, castle screen bounds and user-visible art")

if __name__=="__main__":
    run()
