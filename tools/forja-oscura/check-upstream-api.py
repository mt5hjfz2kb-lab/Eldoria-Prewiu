#!/usr/bin/env python3
"""Read-only comparison between Forja pilot and the ACTUAL Eldoria public Unity C# API.
This is a source compatibility preflight, NOT an engine compile or visual acceptance.
"""
from pathlib import Path
import re, json
root=Path(__file__).resolve().parents[2]
requirements={
 "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1/Components/RoyalHUD.cs":[
     r"public\s+string\s+SelectedBuildingId\s*=>",
     r"public\s+.*\s+Navigation\s*;",
     r"public\s+.*\s+Resources\s*;",
     r"public\s+.*\s+Mission\s*;",
     r"public\s+.*\s+Building\s*;",
     r"public\s+void\s+Refresh\s*\(",
     r"public\s+bool\s+NarrativeOpen\s*=>",
     # The disposable first-entry probe expects Construir to use the real
     # low-level sawmill shortcut, not a lingering Bastion selection.
     r'if\s*\(\s*current\.BastionLevel\s*<\s*2\s*\)\s*\{\s*SelectBuilding\s*\(\s*"sawmill"\s*\)\s*;\s*return\s*;'
 ],
 "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1/Components/BottomNavigation.cs":[
     r"class\s+BottomNavigation",r"public\s+Item\[\]\s+Items",
     r"ChronicleName",r"public\s+static\s+int\s+ChronicleOrder"
 ],
 "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV2Hud.cs":[
     r"public\s+RectTransform\s+ChronicleBuildingRect\s*=>",
     r"public\s+RectTransform\s+ChronicleObjectiveRect\s*=>",
     r"void\s+LayoutNavigation\s*\(",
     r"public\s+void\s+SynchronizeNow\s*\("
 ],
 "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3ContextHud.cs":[
     r"public\s+bool\s+ContextVisible\s*=>",
     r"public\s+RoyalButton\s+VisibleActionButton\s*=>",
     r"public\s+RoyalButton\s+VisibleCloseButton\s*=>",
     r"public\s+RectTransform\s+ContextRect\s*=>",
     r"public\s+bool\s+IsConfigured\s*=>",
     r"public\s+void\s+SynchronizeNow\s*\("
 ],
 "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs":[
     r"class\s+ValoriaIntegratedWorld",
     r"public\s+RoyalHUD\s+Hud\s*\{",
     r"public\s+bool\s+Ready\s*\{",
     r"public\s+ValoriaReferenceCleanGameplay\s+City\s*=>",
     r"public\s+bool\s+Select\s*\(\s*string\s+id\s*\)"
 ],
 "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs":[
     r"class\s+ValoriaIntegrationBuild",
     r"public\s+static\s+void\s+CompleteProbePlayerName\s*\(",
     r"public\s+static\s+void\s+Capture\s*\(",
     r"static\s+void\s+CapturePrepared\s*\(",
     r"Action\s+prepareView",
     r"RenderTexture"
 ],
 "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/ValoriaReferenceCleanGameplay.cs":[
     r"public\s+bool\s+Probe\s*=>"
 ]
}
checked=0
for path,pats in requirements.items():
    f=root/path
    if not f.is_file():raise SystemExit("FORJA_UPSTREAM_SOURCE_MISSING: "+path)
    data=f.read_text(encoding="utf-8-sig")
    for pat in pats:
        if not re.search(pat,data,re.S):
            raise SystemExit("FORJA_UPSTREAM_API_DRIFT: "+path+" pattern="+pat)
        checked+=1
skin=root/"Unity/Assets/Eldoria/VisualKit/Runtime/ForjaOscuraLiveHudSkin.cs"
capture=root/"Unity/Assets/Eldoria/VisualKit/Editor/ForjaOscuraNativeEvidence.cs"
assert skin.is_file() and capture.is_file(),"FORJA_SOURCE_NOT_IN_PROJECT"
assert "TrySkinV3Context" in skin.read_text() and "TrySkinResources" in skin.read_text()
assert "CapturePrepared" in capture.read_text() and "VALORIA_GAMEPLAY_PROBE" in capture.read_text()
assert 'check.destinationVisible = hud.SelectedBuildingId == "sawmill";' in capture.read_text(), "NATIVE_BUILD_ROUTE_MUST_REQUIRE_EXPECTED_DESTINATION"
assert "OnDestroy" in skin.read_text() and "originalButtonStyles" in skin.read_text()
assert "EnsureReadable" in skin.read_text() and "originalFontSizes" in skin.read_text(), "MOBILE_FONT_FLOOR_OR_RESTORE_MISSING"
assert "nativeReadability" in capture.read_text() and "minNavigationIconPx >= 32f" in capture.read_text(), "NATIVE_ICON_READABILITY_GATE_MISSING"
assert "minResourceGlyphPx >= 13f" in capture.read_text() and "navigationLabelsFit" in capture.read_text(), "NATIVE_RESOURCE_OR_CLIPPING_GATE_MISSING"
assert "Owner forbids stacked navigation rows" in capture.read_text(), "NATIVE_SINGLE_ROW_GATE_MISSING"
print(json.dumps({"status":"FORJA_UPSTREAM_CSHARP_API_CONTRACT_PASS",
 "assertions":checked+5,"sources":len(requirements),
 "unity_compiled":False,"unity_native_captured":False,"art_approved":False}))
