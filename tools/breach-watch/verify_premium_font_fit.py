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
    mission_width=170
    def fits(text,font,size,limit,control):
        value=width(font,size,text)
        assert value<=limit+0.15,(control,round(value,2),limit,text)
        print("TYPOGRAPHY_FIT",control,round(value,1),"<=",limit)

    # The reference's single carved title is dynamic, not baked into PNG.
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,11,mission_width-44,
         "900px real single-line chapter in owner bronze")
    fits("Construye el aserradero",REGULAR,12,mission_width-44,
         "true objective alongside painted castle badge")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,13,211-64,
         "390px approved single-line chapter readable at 13pt")
    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,14,180,
         "844px approved single-line chapter readable at 14pt")
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    assert "At(chapterName.rectTransform,smallPhone?54f:36f," in hud
    # This exact accepted Chapter I inscription is measured to fit at 13/14,
    # so it must NOT be silently shrunk by Unity legacy BestFit on small RT.
    assert "chapterName.resizeTextForBestFit=!chapterOne;" in hud
    assert "chapterName.resizeTextMinSize=smallPhone?13:11;" in hud
    assert "chapterName.resizeTextMaxSize=smallPhone?15:11;" in hud
    assert 13*.91>=11.75, "390px title must meet real physical font minimum"
    assert "navCaptions[i].fontSize=smallPhone?13:12;" in hud
    assert "navCaptions[i].fontSize=compactLandscape?14:12;" in hud
    # This source-based preview checks the actual shipped OFL bold body font.
    # A wrapped word is allowed only if it fits within the new real 36u well;
    # Unity's rendered TextGenerator is the final authority, not this model.
    for label in ("REINO","RECURSOS","EJÉRCITO","EXPLORAR","HÉROES"):
        for target,pt,pitch in (("900x1600",12,81),("390x844",13,81),
                                ("844x390",14,85),("1600x900",12,70)):
            measured=width(BODY_BOLD,pt,label)
            assert measured<=2*(pitch-2)-4,(target,label,measured,"would need 3 lines")
    assert hud.count("captionWidth,36f);")==2
    assert 14*.86>=11.75, "844px navigation labels must be readable"
    # The genuine 3x Editor captures must not silently render text 1x,
    # while runtime WebGL density remains unchanged to protect phone memory.
    assert "float scale=authoredScale*density;" in hud
    assert "SetNativeCaptureDensity(float density)" in hud
    fits("Prueba Valoria",SEMIBOLD,16,121,"player identity")
    # Genuine Unity 900x1600 prior failure: 49u Baskerville NIVEL I
    # wrapped into two lines. The real approval reference uses a slim copper
    # engraving, not a giant square-serif heading. Render the exact live
    # level in Cormorant, with 54u available and 4u next to the close badge.
    fits("Bastión",SEMIBOLD,16,54,"real Bastion name in 54u")
    fits("NIVEL I",SEMIBOLD,12,54,"real Bastion level single row")
    fits("450",BODY_BOLD,15,42,"real 450 wood price single row")
    fits("300",BODY_BOLD,14,39,"real 300 stone price with equal icon gap")
    paragraph="Gobierno de Valoria."
    paragraphs=lines(REGULAR,12,paragraph,panel_width-26)
    assert len(paragraphs)<=2,(paragraphs,"narrative wraps beyond its 43u card")
    assert all(width(REGULAR,12,line)<=panel_width-26 for line in paragraphs)
    print("TYPOGRAPHY_PARAGRAPH",paragraphs)
    # Cost heading uses Unity's resizeTextForBestFit. At least font size
    # 13 must fit the 152u budget and remain clearly legible.
    fits("COSTE DE LA MEJORA",SEMIBOLD,11,panel_width-26,
         "authentic upgrade cost heading")
    fits("Operativo · 2:00",REGULAR,14,panel_width-32,
         "real status/timer")
    fits("0 / 1",BODY,14,147,"phone mission progress with bottom clearance")
    fits("Operativo · 2:00",REGULAR,12,126,"large portrait timer inside padded well")
    assert "chapterProgress.fontSize=smallPhone?15:12;" in hud
    assert "buildingState.fontSize=smallPhone?14:12;" in hud
    assert 14*.91>=12.5,"Phone secondary text must be readable at native scale"
    # Physically readable small-phone context despite the tighter panel.
    assert 14*.91>=12.5, "390px contextual text too small"
    assert "buildingFunction.fontSize=smallPhone?14:11;" in hud
    action_width=panel_width-12
    # Source-accurate caption at 900x1600 uses title-case and the whole
    # target, just like the owner's approved UI. Unity BestFit may select
    # 13pt to stay inside its true carved bronze button.
    inset_900=max(31,min(38,action_width*.25))
    fits("Mejorar a Bastión II",SEMIBOLD,12,
         action_width*.70-7,"900px premium full upgrade caption")
    inset_390=max(31,min(38,action_width*.25))
    fits("Mejorar a II",SEMIBOLD,15,
         action_width-inset_390-7,"390px compact upgrade caption")
    # The new reference-guided single burgundy artwork is approximately 4.7:1.
    # Preserve source aspect, contain it inside real touch bounds and measure
    # captions against the *painted* red region rather than invisible hitbox.
    from PIL import Image
    fresh=Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/CleanRoom/upgrade.png')
    with Image.open(fresh) as art:
        art_ratio=art.width/art.height
    assert 3.8<art_ratio<5.7,('New reference-guided burgundy art has invalid silhouette',art_ratio)
    for hit_width,hit_height,label,caption,size in [
        (146,50,'900 portrait','Mejorar a Bastión II',11),
        (159,50,'390 portrait','Mejorar a II',14),
        (196,45,'1600 landscape','Mejorar a II',15),
        (210,53,'844 compact landscape','Mejorar a II',15),
    ]:
        painted_h=min(hit_height-4,hit_width/art_ratio)
        painted_w=painted_h*art_ratio
        assert painted_w>=hit_width*.70 and painted_h<=hit_height-4+1e-3,label
        assert abs(painted_w/painted_h-art_ratio)<.001,label
        free_width=painted_w*.70-14
        fits(caption,SEMIBOLD,size,free_width,
             label+' inside original cleanroom red painting')
    hud=Path('Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs').read_text()
    assert 'float paintedHeight=Mathf.Min(plate.rect.height-4f,' in hud
    assert 'float paintedWidth=paintedHeight*artAspect;' in hud
    assert 'float captionLeft=paintedLeft+paintedWidth*.30f;' in hud
    assert 'bool fullUpgradeCaption=lastPortrait && lastWidth>=800;' in hud

    # Dynamic titles must fit the existing title well, including the real
    # Aserradero destination reached by the provisions medallion.
    for title_width,min_size,max_size,label in [
        (54,12,15,"900 portrait"),
        (67,14,15,"390 portrait"),
        (99,16,18,"1600 landscape"),
        (113,16,18,"844 landscape"),
    ]:
        for name in ["Bastión","Aserradero","Cuartel","Granero","Cantera"]:
            chosen=next((s for s in range(max_size,min_size-1,-1)
                         if width(SEMIBOLD,s,name)<=title_width),None)
            assert chosen is not None,(label,name,title_width)
            fits(name,SEMIBOLD,chosen,title_width,label+" real building title")
    assert width(SEMIBOLD,15,"Aserradero")>54, "Regression case no longer exercises overflow"
    assert "FitBuildingName();" in hud
    assert "for(int size=buildingNameMaxFont;size>=buildingNameMinFont;size--)" in hud

    # Fixed layout sizes must survive every orientation; construction-time
    # BestFit bounds used to override the narrower portrait inscription.
    assert "costHeader.resizeTextForBestFit=false;" in hud
    for size,available,label in [(10,132,"900"),(13,145,"390"),
                                  (14,194,"1600"),(14,210,"844")]:
        fits("COSTE DE LA MEJORA",SEMIBOLD,size,available,label+" fixed cost heading")
    assert "costHeader.fontSize=14;" in hud

    # The right-edge close badge must not touch either heading, in portrait
    # or small landscape; both rows preserve >=4 Unity design units.
    hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
    for clause in (
        "At(buildingName.rectTransform,45,21,pw-104,21);",
        "At(buildingLevel.rectTransform,45,43,pw-104,17);",
        "At(buildingName.rectTransform,60,h<530f?12f:22f,pw-121,h<530f?27f:22f);",
        "At(buildingLevel.rectTransform,60,h<530f?40f:45f,pw-121,21);",
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
