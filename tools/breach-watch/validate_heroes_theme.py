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
    shader=ROOT/"Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/BreachWatchHallWarmStone.shader"
    need(shader.is_file() and shader.with_suffix(".shader.meta").is_file(),
         "Warm illustrated hall shader and Unity metadata required")
    sh=shader.read_text()
    need("BreachWatchHallWarmStone" in sh and "SAMPLE_TEXTURE2D" in sh and
         "_Warmth" in sh and "Blend SrcAlpha OneMinusSrcAlpha" in sh,
         "Fast one-sample mobile hall material missing")
    need("GradeActualHall" in H and "originalHallMaterials" in H and
         "originalAccentColors" in H and "warmHall" in H,
         "The real hall must be graded in-place and restorable")
    need("SynchronizeVisual()" in B and "theme.Refresh(root)" in B,
         "Dynamic portrait rebuild must restyle on each layout/capture")
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
