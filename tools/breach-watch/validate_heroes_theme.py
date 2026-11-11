#!/usr/bin/env python3
"""Source-only gate for reversible real Heroes destination styling.
Never equate passing source checks with Unity proof or artistic acceptance.
"""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
R=ROOT/"Unity/Assets/Eldoria/BreachWatch/Runtime"
H=(R/"BreachWatchHeroesTheme.cs").read_text()
B=(R/"BreachWatchHeroesBridge.cs").read_text()
HUD=(R/"BreachWatchHud.cs").read_text()
def need(cond,message):
    if not cond: raise AssertionError(message)
def main():
    for name in ("BreachWatchHeroesTheme","BreachWatchHeroesBridge"):
        need((R/(name+".cs.meta")).is_file(),name+" missing Unity GUID")
    need("Heroes / live controls" in H and
         "ResetPresentation()" in H and "originals" in H and
         "IsOldRoyalFrame" in H and "SpriteMeshType.FullRect" in H,
         "Hero frames must replace old art and survive dynamically rebuilt pages")
    need("HeroesApprovedDestination heroes;" in B and
         "heroes.VisibleRoot" in B and
         "heroes.Refresh()" in B and
         "root.SetParent(canvas.transform,false)" in B and
         "root.SetParent(parent,false)" in B,
         "Hero destination must reuse and restore canonical real screen")
    need("originalClosed?.Invoke();" in B and
         "army.TryAdoptAlreadyOpen()" in B and
         "heroes.Closed=originalClosed" in B,
         "Canonical closure and return to march/training must remain live")
    need("heroes.Suspend();" in B,
         "Unstyled hero screen may not be trapped invisibly under old HUD")
    need("heroesBridge.Initialize(host,canvas,destinationBridge)" in HUD,
         "Vigilia must register original Heroes bridge")
    navigation=HUD.split("void Navigate(int index)",1)[1].split("void LateUpdate()",1)[0]
    need("heroesBridge.TryOpen()" not in navigation,
         "No new Heroes icon before certified mobile screenshots")
    print("VIGILIA_HERO_SCREEN_REPLACEMENT_SOURCE_PASS")
    print("NATIVE_HERO_PAGE_ART_REVIEW_AND_CLICK_QA_PENDING")
if __name__=="__main__":main()
