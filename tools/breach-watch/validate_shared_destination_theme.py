#!/usr/bin/env python3
"""Source-only guard for the staged native Vigilia destination adapter.

A PASS means source and packaging consistency, NOT an artistic or Unity proof.
Real native screenshots, click/close/resize tests and owner approval are still
mandatory before any Army/Heroes/Mail route becomes visible.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
B = ROOT / "Unity/Assets/Eldoria/BreachWatch"
R = B / "Runtime"
A = B / "Resources/VigiliaArt"


def require(value, explanation):
    if not value:
        raise AssertionError(explanation)


def main():
    theme = (R / "BreachWatchSharedTheme.cs").read_text()
    bridge = (R / "BreachWatchDestinationBridge.cs").read_text()
    hud = (R / "BreachWatchHud.cs").read_text()
    for path in ["PremiumSurfaces/panel_fill.png",
                 "PremiumSurfaces/burgundy_action.png"]:
        require((A / path).is_file(), "Missing reusable original art: " + path)
        require((A / (path + ".meta")).is_file(), "Missing import settings: " + path)
    for source in [R / "BreachWatchSharedTheme.cs",
                   R / "BreachWatchDestinationBridge.cs"]:
        require(source.with_suffix(".cs.meta").is_file(),
                "Missing stable script GUID: " + source.name)
    require("source.border.sqrMagnitude" in theme and
            "SpriteMeshType.FullRect" in theme and "Image.Type.Sliced" in theme,
            "Reusable stone must use sliced borders, never full-frame stretching")
    require("ReplaceLegacyPainting(destination)" in theme and
            "RoyalGoldV3ManagementBackdrop" in theme and
            "Approved source / blue-panel" in theme and
            "ReplacedLegacyGraphics>=4" in theme and
            "originalTextColors" in theme,
            "Legacy blue artwork must be removed, not covered by a new image")
    require("originalButtonTargets" in theme and
            "originalTransitions" in theme and "Restore()" in theme,
            "Theme must preserve original Selectable behavior")
    require("management.Open(destination)" in bridge and
            "management.VisibleRoot" in bridge and
            "root.SetParent(vigilia.transform,false)" in bridge and
            "destinationRoot.SetParent(originalParent,false)" in bridge,
            "Destination must reuse and restore canonical live screen")
    require("host.CanOpen(RoyalSection.Army)" in bridge,
            "Destination must retain story gate")
    require("destinationBridge.Initialize(host,canvas)" in hud,
            "Native proof canvas must initialize destination bridge")
    require("int mask=(1<<0)|(1<<1);" in hud,
            "Unapproved Army/Heroes controls must remain hidden")
    require("case 2:" not in hud.split("void Navigate(int index)",1)[1]
            .split("void LateUpdate()",1)[0],
            "Do not expose incomplete new Army navigation")
    print("VIGILIA_SHARED_DESTINATION_SOURCE_PASS")
    print("VISUAL_APPROVAL_NOT_CLAIMED")
    print("UNITY_DESTINATION_SCREENSHOTS_PENDING")


if __name__ == "__main__":
    main()
