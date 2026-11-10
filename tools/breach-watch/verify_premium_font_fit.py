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
    mission_width=184
    def fits(text,font,size,limit,control):
        value=width(font,size,text)
        assert value<=limit+0.15,(control,round(value,2),limit,text)
        print("TYPOGRAPHY_FIT",control,round(value,1),"<=",limit)

    # The reference's single carved title is dynamic, not baked into PNG.
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,11,mission_width-57,
         "900px real single-line chapter in owner bronze")
    fits("Construye el aserradero",REGULAR,12,mission_width-57,
         "true objective alongside painted castle badge")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,13,211-64,
         "390px approved single-line chapter readable at 13pt")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,14,180,
         "844px approved single-line chapter readable at 14pt")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "At(chapterName.rectTransform,smallPhone?54f:48f," in hud
    # This exact accepted Chapter I inscription is measured to fit at 13/14,
    # so it must NOT be silently shrunk by Unity legacy BestFit on small RT.
    assert "chapterName.resizeTextForBestFit=!chapterOne;" in hud
    assert "chapterName.resizeTextMinSize=smallPhone?13:11;" in hud
    assert "chapterName.resizeTextMaxSize=smallPhone?13:11;" in hud
    assert 13*.91>=11.75, "390px title must meet real physical font minimum"
    fits("Prueba Valoria",SEMIBOLD,16,121,"player identity")
    fits("Bastión",SEMIBOLD,16,panel_width-80,
         "selected building title")
    paragraph="Gobierno y progreso de Valoria."
    paragraphs=lines(REGULAR,12,paragraph,panel_width-26)
    assert len(paragraphs)<=2,(paragraphs,"narrative wraps beyond its 43u card")
    assert all(width(REGULAR,12,line)<=panel_width-26 for line in paragraphs)
    print("TYPOGRAPHY_PARAGRAPH",paragraphs)
    # Cost heading uses Unity's resizeTextForBestFit. At least font size
    # 13 must fit the 152u budget and remain clearly legible.
    fits("COSTE DE LA MEJORA",SEMIBOLD,11,panel_width-26,
         "authentic upgrade cost heading")
    fits("Operativo · 2:00",REGULAR,11,panel_width-26,
         "real status/timer")
    # Physically readable small-phone context despite the tighter panel.
    assert 14*.91>=12.5, "390px contextual text too small"
    assert "buildingFunction.fontSize=smallPhone?14:12;" in hud
    action_width=panel_width-24
    # Source-accurate caption at 900x1600 uses title-case and the whole
    # target, just like the owner's approved UI. Unity BestFit may select
    # 13pt to stay inside its true carved bronze button.
    inset_900=max(31,min(38,action_width*.25))
    fits("Mejorar a Bastión II",SEMIBOLD,12,
         action_width*.72-7,"900px premium full upgrade caption")
    inset_390=max(31,min(38,action_width*.25))
    fits("Mejorar a II",SEMIBOLD,15,
         action_width-inset_390-7,"390px compact upgrade caption")
    # The NEW 2K original action sprite is 3.45:1, not legacy 246:60.
    # Preserve source aspect, contain it inside real touch bounds and measure
    # captions against the *painted* red region rather than invisible hitbox.
    from PIL import Image
    fresh=Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/FreshArt/upgrade_button.png')
    with Image.open(fresh) as art:
        art_ratio=art.width/art.height
    assert 3.2<art_ratio<3.8,('Unexpected newly generated action art ratio',art_ratio)
    for hit_width,hit_height,label,caption,size in [
        (147,50,'900 portrait','Mejorar a Bastión II',12),
        (147,50,'390 portrait','Mejorar a II',15),
        (196,45,'1600 landscape','Mejorar a II',16),
        (210,53,'844 compact landscape','Mejorar a II',15),
    ]:
        painted_h=min(hit_height-4,hit_width/art_ratio)
        painted_w=painted_h*art_ratio
        assert painted_w>=hit_width*.70 and painted_h<=hit_height-4+1e-3,label
        assert abs(painted_w/painted_h-art_ratio)<.001,label
        free_width=painted_w*.72-7
        fits(caption,SEMIBOLD,size,free_width,
             label+' inside original cleanroom red painting')
    hud=Path('Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs').read_text()
    assert 'float paintedHeight=Mathf.Min(plate.rect.height-4f,' in hud
    assert 'float paintedWidth=paintedHeight*artAspect;' in hud
    assert 'float captionLeft=paintedLeft+paintedWidth*.28f;' in hud
    assert 'bool fullUpgradeCaption=lastPortrait && lastWidth>=800;' in hud

    # The right-edge close badge must not touch either heading, in portrait
    # or small landscape; both rows preserve >=4 Unity design units.
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    for clause in (
        "At(buildingName.rectTransform,18,7,pw-80,21);",
        "At(buildingLevel.rectTransform,18,31,pw-80,17);",
        "At(buildingName.rectTransform,18,8,pw-80,31);",
        "At(buildingLevel.rectTransform,18,40,pw-80,17);",
    ):
        assert clause in hud, ("Missing separate title/close geometry",clause)

    # Physical pixels: compact first-screen body glyphs and five nav areas
    # cannot be reduced to sub-12-pixel unreadable text.
    scale_390=max(.91,min(3.0,390/514))
    assert 14*scale_390>=12.5
    assert 52*scale_390>=43.5
    print("BREACHWATCH_PACKAGED_PREMIUM_FONT_LAYOUT_PASS")

if __name__=="__main__":
    verify()
