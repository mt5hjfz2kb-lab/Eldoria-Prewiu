#!/usr/bin/env python3
"""Fail-closed source-only guard for the ONE real canonical story/modal bridge."""
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
R=ROOT/"Unity/Assets/Eldoria/BreachWatch/Runtime"
A=ROOT/"Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt"
b=(R/"BreachWatchModalBridge.cs").read_text()
hud=(R/"BreachWatchHud.cs").read_text()

def check(test,why):
    if not test: raise AssertionError(why)

def main():
    check((R/"BreachWatchModalBridge.cs.meta").exists(),
          "A Unity C# component must have stable metadata")
    for asset in ("PremiumSurfaces/panel_fill.png",
                  "PremiumSurfaces/burgundy_action.png",
                  "OwnerControls/close_medallion.png"):
        check((A/asset).exists() and (A/(asset+".meta")).exists(),
              "Missing approved canonical modal art "+asset)
    check("ModalWindow realModal;" in b and
          "realModal=host.Hud.Modal" in b and
          "root.SetParent(visualCanvas.transform,false)" in b and
          "root.SetParent(originalParent,false)" in b,
          "Must migrate and restore one existing ModalWindow")
    check("originalWindowScale" in b and
          "window.localScale=new Vector3(fit,fit,originalWindowScale.z)" in b and
          "var actualSafeArea=window.parent as RectTransform;" in b and
          "window.anchorMin=window.anchorMax=new Vector2(.5f,0f);" in b and
          "window.anchoredPosition=new Vector2(0f,12f);" in b and
          "window.anchorMin=originalAnchorMin;window.anchorMax=originalAnchorMax;" in b,
          "Phone layout must be responsive without changing gameplay")
    check("if(old)Hide(old)" in b and
          "oldTargets" in b and "oldTransitions" in b and
          'rect.Find("Symbol")' in b,
          "Disable legacy artwork and duplicate X, preserve live buttons")
    check("modalBridge.Initialize(host,canvas)" in hud and
          "CanonicalModalVisibleOnIndependentCanvas" in hud,
          "New independent Canvas must host actual story and confirmation UI")
    check("new ModalWindow" not in b and
          "new RoyalButton" not in b,
          "No duplicate narrative or fake confirmation commands")
    print("VIGILIA_REAL_MODAL_MIGRATION_SOURCE_PASS")
    print("UNITY_MODAL_POINTER_PROOF_PENDING")

if __name__=="__main__":
    main()
