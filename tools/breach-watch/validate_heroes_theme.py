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
    hero_source=(ROOT/"Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV41SourceArt.cs").read_text()
    need("var realModelPortrait=LyraPortraitProvider();" in hero_source and
         "if(realModelPortrait)return realModelPortrait;" in hero_source and
         'const string key="approved-lyra-portrait"' in hero_source,
         "Lyra must display approved illustration while live model portrait loads")
    genuine_profile=(ROOT/"Unity/Assets/Eldoria/HeroesUI/HeroesApprovedDestination.cs").read_text()
    need("FinishApprovedFallback(img,hero.Portrait)" in genuine_profile and
         'SetFloat("_PortraitFeather",.20f)' in genuine_profile and
         'SetVector("_PortraitUVRect"' in genuine_profile and
         "hallProfileFallbackMaterial" in genuine_profile,
         "Lyra's genuine approved photo fallback must be feathered in the 2.5D hall")
    need('navCell=(navWidth-navGap*2f)/3f' in genuine_profile and
         'Hall / nav reliquary' not in genuine_profile and
         'Hall / nav marches' in genuine_profile and
         'Hall / nav return' in genuine_profile,
         "Hero Salon must show exactly three genuine, wider destinations; no inert Relicario")
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
    need("heroesBridge.TryOpen()" in navigation and
         "case 4:" in navigation and
         "hero.CanOpenDestination()" in HUD,
         "Heroes nav must route only to real source-bound portrait screen after Lyra unlock")
    print("VIGILIA_HERO_SCREEN_REPLACEMENT_SOURCE_PASS")
    print("NATIVE_HERO_PAGE_ART_REVIEW_AND_CLICK_QA_PENDING")
if __name__=="__main__":main()
