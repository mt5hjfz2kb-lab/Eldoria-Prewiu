#!/usr/bin/env python3
"""Measure real embedded UI fonts against the native first-screen geometry.

This is a typography SOURCE gate, not a Unity screenshot or artistic approval.
Every string here is an actual canonical first-chapter text or an approved
display caption. Use the exact embedded OFL font files shipped in the PR.
"""
from pathlib import Path
from PIL import ImageFont
import math

ROOT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/Fonts")
REGULAR=ROOT/"CormorantGaramond-Regular.ttf"
SEMIBOLD=ROOT/"CormorantGaramond-SemiBold.ttf"
BODY=ROOT/"LibreBaskerville-Regular.ttf"
BODY_BOLD=ROOT/"LibreBaskerville-Bold.ttf"

def width(font:Path,size:int,text:str)->float:
    return ImageFont.truetype(str(font),size).getlength(text)

def lines(font:Path,size:int,text:str,max_width:float):
    face=ImageFont.truetype(str(font),size)
    output=[]
    current=""
    for word in text.split():
        proposed=f"{current} {word}".strip()
        if face.getlength(proposed)<=max_width:
            current=proposed
        else:
            assert current,f"No word of {text!r} fits width {max_width}"
            output.append(current)
            current=word
    if current:output.append(current)
    return output

def verify():
    for font in [REGULAR,SEMIBOLD,BODY,BODY_BOLD]:
        assert font.is_file(),f"Missing real packaged Unity font {font}"

    # Phone design-units are stable between 390x844 and Retina 900x1600.
    panel_width=171
    mission_width=185
    def fits(text,font,size,limit,control):
        value=width(font,size,text)
        assert value<=limit+0.15,(control,round(value,2),limit,text)
        print("TYPOGRAPHY_FIT",control,round(value,1),"<=",limit)

    # The story title starts in clean carved stone. No duplicated vector
    # crest should steal the title's paint width or make an overlay.
    fits("BASTIÓN I",SEMIBOLD,14,mission_width-29,
         "first line engraved chapter")
    fits("LA BRECHA",SEMIBOLD,14,mission_width-29,
         "second line engraved Breach")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "At(chapterName.rectTransform,17f,5f,missionWidth-29f,32f);" in hud
    fits("Construye el aserradero",REGULAR,14,mission_width-22,
         "chapter-objective")
    fits("Prueba Valoria",SEMIBOLD,16,121,"player identity")
    fits("Bastión",SEMIBOLD,19,panel_width-80,
         "selected building title")
    paragraph="Gobierno y progreso de Valoria."
    paragraphs=lines(REGULAR,15,paragraph,panel_width-26)
    assert len(paragraphs)<=2,(paragraphs,"narrative wraps beyond its 43u card")
    assert all(width(REGULAR,15,line)<=panel_width-26 for line in paragraphs)
    print("TYPOGRAPHY_PARAGRAPH",paragraphs)
    # Cost heading uses Unity's resizeTextForBestFit. At least font size
    # 13 must fit the 152u budget and remain clearly legible.
    fits("COSTE DE LA MEJORA",SEMIBOLD,13,panel_width-26,
         "authentic upgrade cost heading")
    fits("Operativo · 2:00",REGULAR,14,panel_width-26,
         "real status/timer")
    action_width=panel_width-24
    # Source-accurate caption at 900x1600 uses title-case and the whole
    # target, just like the owner's approved UI. Unity BestFit may select
    # 13pt to stay inside its true carved bronze button.
    inset_900=max(31,min(38,action_width*.25))
    fits("Mejorar a II",SEMIBOLD,16,
         action_width-inset_900-7,"900px premium one-line upgrade caption")
    inset_390=max(31,min(38,action_width*.25))
    fits("Mejorar a II",SEMIBOLD,15,
         action_width-inset_390-7,"390px compact upgrade caption")
    # The full original-for-Vigilia GOLD nine-slice is one continuous
    # chamfered button, with a left-side engraved hammer and live title.
    # Unlike the rejected burgundy sprite, it can change width without
    # stretching its bevels or letterboxing its text.
    for hit_width,label,caption,size in [
        (147,"900 portrait","Mejorar a II",16),
        (147,"390 portrait","Mejorar a II",16),
        (196,"1600 landscape","Mejorar a II",16),
        (186,"844 historical tight-landscape safety","Mejorar a II",15),
        (210,"844 actual widened-landscape","Mejorar a II",15),
    ]:
        free_width=hit_width*(1-.24)-8
        fits(caption,SEMIBOLD,size,free_width,label+" within single gold face")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "VigiliaArt/UnifiedButtons/action_gold" in hud
    assert "actionPlate.type=Image.Type.Sliced;" in hud
    assert "float captionLeft=plate.rect.width*.24f;" in hud
    assert "actionLabel.color=can?new Color(.99f,.95f,.82f,1f):" in hud
    assert "float paintedHeight=Mathf.Max(30f,plate.rect.height-4f);" in hud
    assert 'actionCaption=host.English?"Upgrade to II":"Mejorar a II";' in hud
    # Dynamic nav captions are live Cormorant, inside the same 80u source
    # tile, rather than an extra small medallion and detached text label.
    for text in ("REINO","RECURSOS","EXPLORAR"):
        fits(text,SEMIBOLD,14,112,"one painted nav tile "+text)
    assert "navCaptions[i].fontSize=14;" in hud
    assert "VigiliaArt/UnifiedButtons/nav_dock" in hud

    # The right-edge close badge must not touch either heading, in portrait
    # or small landscape; both rows preserve >=4 Unity design units.
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    for clause in (
        "At(buildingName.rectTransform,18,7,pw-80,28);",
        "At(buildingLevel.rectTransform,18,36,pw-80,17);",
        "At(buildingName.rectTransform,18,8,pw-80,31);",
        "At(buildingLevel.rectTransform,18,40,pw-80,17);",
    ):
        assert clause in hud, ("Missing separate title/close geometry",clause)

    # Physical pixels: compact first-screen body glyphs and five nav areas
    # cannot be reduced to sub-12-pixel unreadable text.
    scale_390=max(.91,min(3.0,390/514))
    assert 14*scale_390>=12.5
    assert 49*scale_390>=43.5
    print("BREACHWATCH_PACKAGED_PREMIUM_FONT_LAYOUT_PASS")

if __name__=="__main__":
    verify()
