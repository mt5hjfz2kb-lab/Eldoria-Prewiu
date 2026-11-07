#!/usr/bin/env python3
"""Source-constrained screen-pixel geometry model, NOT a Unity capture.

This catches regressions in *intent* and the 44px mathematical bound while
Unity native/physical-device verification is separately required.
"""
from __future__ import annotations

import math
from pathlib import Path

root = Path(__file__).resolve().parents[2]
skin = (root / "Unity/Assets/Eldoria/VisualKit/Runtime/ForjaOscuraLiveHudSkin.cs").read_text()
v2 = (root / "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV2Hud.cs").read_text()
capture = (root / "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs").read_text()
native = (root / "Unity/Assets/Eldoria/VisualKit/Editor/ForjaOscuraNativeEvidence.cs").read_text()
source_contract = {
    v2: [
        "float cellHeight = rows > 1 ? 74f : vertical ? 105f : 106f;",
        "float columnGap = vertical ? 4f : 9f;",
        "float availableWidth = Mathf.Max(150f, safe.rect.width - 12f);",
        "AlignBuildingCartoucheToNavigation();",
    ],
    skin: [
        "float dockWidth = Mathf.Min(safe.rect.width-24f/scale, 640f/scale);",
        "float columnGap = 4f/scale;",
        "float cellWidth = (dockWidth-12f/scale-4f*columnGap)/5f;",
        "float buttonHeight = 56f/scale;",
        "dock.sizeDelta = new Vector2(dockWidth, 64f/scale);",
        "if (item.Label) Hide(item.Label);",
        "Mathf.Min(50f, Mathf.CeilToInt(24f / canvas.scaleFactor))",
        "icon.offsetMin = new Vector2(-19f, -10f);",
        "icon.offsetMax = new Vector2(2f, 10f);",
        "RefreshFastNavigationAndResources();",
        "EnsureBuildingCartoucheClearsNavigation();",
        "dock.anchoredPosition.y + dock.rect.height + 8f",
    ],
    native: [
        "Native illustrated contextual card overlaps reflowed navigation",
        "buildingCardClearancePx",
    ],
}
for body, markers in source_contract.items():
    for marker in markers:
        assert marker in body, f"Geometry source changed; inspect Unity behavior: {marker}"

# The PR's strict CI runs on the *merge ref*, not just its feature branch.
# Since the feature was opened, main acquired a phone-specific Canvas scale.
# Both are actual source implementations, so model precisely whichever is
# checked out. Never silently assume the old 800/900 factor if main changed.
legacy_scaler = "canvas.scaleFactor=width<height?width/800f:height/900f;"
phone_scaler = ("bool phone=Mathf.Min(width,height)<600;"
                "canvas.scaleFactor=width<height?width/(phone?600f:800f):"
                "height/(phone?600f:900f);")
if legacy_scaler in capture:
    phone_specific_scaler = False
elif phone_scaler in capture:
    phone_specific_scaler = True
else:
    raise AssertionError("Capture scale source changed; inspect Unity behavior")


def geometry(width: int, height: int) -> dict:
    portrait = height > width
    phone = min(width, height) < 600 and phone_specific_scaler
    scale = width / (600.0 if phone else 800.0) if portrait else (
        height / (600.0 if phone else 900.0))
    canvas_w = width / scale
    columns, rows = 5, 1
    dock_width = min(canvas_w-24/scale,640/scale)
    gap = 4/scale
    cell_width = (dock_width-12/scale-4*gap)/5
    cell_height = 56/scale
    physical_button_width = cell_width * scale
    physical_button_height = cell_height * scale
    physical_graphic_width = max(54, cell_width - 2) * scale
    physical_graphic_height = cell_height * scale

    icon_source = min(25, max(14, .27 * ((canvas_w - 232 - 56 - 17) / 2))) if portrait else 36
    icon_physical = icon_source * scale
    if icon_physical < 24 and portrait:
        icon_physical = min(50, math.ceil(24 / scale)) * scale
    elif icon_physical < 24:
        icon_physical = (icon_source + 19 + 2) * scale
    return {
        "size": f"{width}x{height}",
        "columns": columns, "rows": rows,
        "scale": scale, "touch_px": round(physical_button_height, 2),
        "graphic_px": round(physical_graphic_height, 2),
        "touch_width_px": round(physical_button_width, 2),
        "graphic_width_px": round(physical_graphic_width, 2),
        "resource_icon_px": round(icon_physical, 2),
        # The existing bottom-anchored building panel must sit 8 source
        # units above the RE-FLOWED dock. A source-model value, not a
        # claimed measured native pixel gap.
        "card_to_dock_margin_model_px": round(8 * scale, 2),
    }


proof = [geometry(w, h) for w, h in ((1600, 900), (900, 1600), (844, 390), (390, 844))]
assert all(v["touch_px"] >= 44 for v in proof), proof
assert all(v["graphic_px"] >= 44 for v in proof), proof
assert all(v["graphic_width_px"] >= 44 for v in proof), proof
assert all(v["resource_icon_px"] >= 24 for v in proof), proof
assert all(v["rows"] == 1 and v["columns"] == 5 for v in proof), proof
assert proof[-1]["touch_width_px"] >= 44, proof[-1]
assert proof[-1]["card_to_dock_margin_model_px"] > 0, proof[-1]
for record in proof:
    print(f"FORJA_GEOMETRY_SOURCE_MODEL {record}")
print(f"SOURCE_MODEL_SCALER={'PHONE_600_800_900' if phone_specific_scaler else 'LEGACY_800_900'}")
print("SOURCE_MODEL_PASS — DOES NOT establish native render, font fit, raycast or artwork quality")
