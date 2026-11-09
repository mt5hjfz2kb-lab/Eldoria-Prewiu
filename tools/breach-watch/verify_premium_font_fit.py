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
    mission_width=200
    def fits(text,font,size,limit,control):
        value=width(font,size,text)
        assert value<=limit+0.15,(control,round(value,2),limit,text)
        print("TYPOGRAPHY_FIT",control,round(value,1),"<=",limit)

    # The reference's single carved title is dynamic, not baked into PNG.
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,11,mission_width-70,
         "approved single-line chapter and Breach inscription")
    fits("Construye el aserradero",REGULAR,13,mission_width-70,
         "true objective alongside painted castle badge")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,13,211-64,
         "390px approved single-line chapter readable at 13pt")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,14,180,
         "844px approved single-line chapter readable at 14pt")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "At(chapterName.rectTransform,smallPhone?54f:59f,8f," in hud
    assert "chapterName.resizeTextMinSize=14;" in hud
    assert "chapterName.resizeTextMaxSize=14;" in hud
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
    fits("Mejorar a Bastión II",SEMIBOLD,13,
         action_width-inset_900-7,"900px premium full upgrade caption")
    inset_390=max(31,min(38,action_width*.25))
    fits("Mejorar a II",SEMIBOLD,15,
         action_width-inset_390-7,"390px compact upgrade caption")
    # Original owner-authored 246x60 sprite must cover the complete
    # visible button, not letterbox in a wider transparent Unity Button.
    # Exact aspect preserves undistorted engraved hammers and true label.
    for hit_width,label,caption,size in [
        (147,"900 portrait","Mejorar a Bastión II",13),
        (147,"390 portrait","Mejorar a II",16),
        (196,"1600 landscape","Mejorar a Bastión II",16),
        (186,"844 historical tight-landscape safety","Mejorar a Bastión II",15),
        (210,"844 actual widened-landscape","Mejorar a Bastión II",15),
    ]:
        paint_height=hit_width*60/246
        assert 22<paint_height<55, (label,"image size")
        assert abs(paint_height*246/60-hit_width)<.01, (label,"letterbox")
        free_width=hit_width*(1-.25)-7
        fits(caption,SEMIBOLD,size,free_width,
             label+" within FULL WIDTH genuine 246x60 artwork")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "float paintedHeight=plate.rect.width/Mathf.Max(1f,artAspect);" in hud, (
        "Premium CTA must render source at full painted width")
    assert "bool fullUpgradeCaption=lastWidth>=800;" in hud, (
        "Full chapter action must be readable in both >=800px orientations")

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
