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
        "1.78f*ratio","1.67f,2.15f",
        "cityCamera.orthographicSize=Mathf.Clamp(originalZoom*factor,45f,105f)",
        "originalZoom=cityCamera.orthographicSize",
        "originalPosition=cityCamera.transform.position",
        "host.City.Controls.FitToTerrain()",
        "host.InWorld","camera","WorldToViewportPoint",
    ]:
        assert mandatory in code,f"City composition safety contract removed: {mandatory}"
    assert "BreachWatchCityComposition" in hud and "cityComposition.Layout(pixelWidth,pixelHeight)" in hud
    assert "castleViewportWidth" in evidence and "Approved panoramic portrait city view" in evidence
    assert ".homeZoom=" not in code and ".WorldNavigation" not in code,(
        "Standalone layout must never modify gameplay controller presets or World camera")

    # Approximates scale ratio for a 70%-width castle when the camera zooms
    # out, assuming bounds remain in-frame and no terrain clamping. Unity's
    # actual render and bounds proof must still pass separately.
    old_normalized_castle_width=.70
    for width,height in ((864,1536),(900,1600),(390,844),(1170,2532)):
        aspect=width/height
        zoom_factor=max(1.67,min(2.15,1.78*(.5625/aspect)))
        predicted=old_normalized_castle_width/zoom_factor
        assert .30<=predicted<=.43,(width,height,predicted)
        assert zoom_factor>1.65
        print("VIGILIA_CITY_COMPOSITION_SOURCE_PASS",
              f"{width}x{height}",f"zoom_factor={zoom_factor:.3f}",
              f"predicted_relative_castle_width={predicted:.3f}")
    assert "PortraitCityFramed" in hud
    print("Requires real Unity render: terrain clamps, castle screen bounds and user-visible art")

if __name__=="__main__":
    run()
