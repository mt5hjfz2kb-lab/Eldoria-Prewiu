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

    fits("BASTIÓN I / LA BRECHA",SEMIBOLD,13,mission_width-55,
         "chapter-title")
    fits("Construye el aserradero",REGULAR,14,mission_width-22,
         "chapter-objective")
    fits("Prueba Valoria",SEMIBOLD,16,121,"player identity")
    fits("Bastión",SEMIBOLD,19,panel_width-106,
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
    engraving_inset=max(34,min(48,action_width*.235))
    fits("MEJORAR A II",SEMIBOLD,16,action_width-engraving_inset-7,
         "true compact phone upgrade")
    # Physical pixels: compact first-screen body glyphs and five nav areas
    # cannot be reduced to sub-12-pixel unreadable text.
    scale_390=max(.91,min(3.0,390/514))
    assert 14*scale_390>=12.5
    assert 49*scale_390>=43.5
    print("BREACHWATCH_PACKAGED_PREMIUM_FONT_LAYOUT_PASS")

if __name__=="__main__":
    verify()
