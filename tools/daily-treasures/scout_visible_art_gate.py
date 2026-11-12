#!/usr/bin/env python3
"""Pixel-based source-art gate for the genuine Scout parchment on eight devices.

This reproduces source RectTransform geometry and measures authentic PNG alpha
occlusion. It guards an actual previous Unity artistic failure (oversized scroll).
It cannot certify the current Unity render or grant ART_APPROVED.
"""
from __future__ import annotations
import hashlib
import json
import math
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/"Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies"
STAGE=ROOT/"Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs"
PANEL=ROOT/"Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs"
PARCHMENT=ART/"explorers_stage.png"
SCREENS=((390,844),(844,390),(900,1600),(1600,900),
         (1024,768),(768,1024),(375,667),(667,375))

def source_guard():
    manifest=json.loads((ART/"source-manifest.json").read_text(encoding="utf-8"))
    authentic=manifest["assets"]["explorers"]
    assert authentic["file"]==PARCHMENT.name
    assert hashlib.sha256(PARCHMENT.read_bytes()).hexdigest()==authentic["sha256"]
    source="".join((STAGE.read_text(encoding="utf-8")+
                    PANEL.read_text(encoding="utf-8")).split())
    # Fail closed if someone changes the actual Unity layout without
    # remeasuring the changed geometry and reviewing fresh native evidence.
    for expression in (
        "if(portrait&&width>=650f&&height>=900f)",
        "Math.Min(height-20f,1060f)",
        "if(!portrait&&width>=950f&&height>=620f)",
        "Math.Min(height-26f,Math.Min(640f,height*.76f))",
        "0,.15f*h,w-24,.56f*h",
        "-.18f*w,.018f*h,.62f*w,h-42f",
        "portrait?.72f:.78f",
        "portrait?.72f:.84f",
        "stage.SceneW*(portrait?.13f:.11f)",
        "stage.SceneH*(portrait?.10f:.07f)",
        "ContainOriginalArtwork(",
        "FindActualCommittedScoutKind(eventId)",
    ):
        assert "".join(expression.split()) in source, (
            "Unity Scout composition changed; remeasure before approving: "+expression)

def theatre(w,h):
    p=h>w
    if p and w>=650 and h>=900:
        return min(w-24.,620.),min(h-20.,1060.)
    if not p and w>=950 and h>=620:
        return min(w-32.,1120.,w*.84),min(h-26.,640.,h*.76)
    return min(w-24.,440. if p else 740.),min(h-20.,700. if p else 370.)

def area_mask(size,center,quad):
    x,y,w,h=quad
    cx,cy=center
    output=Image.new("1",size)
    ImageDraw.Draw(output).rectangle(
        (round(cx+x-w/2),round(cy-y-h/2),round(cx+x+w/2),round(cy-y+h/2)),
        fill=1)
    return output

def occupied_ratio(painted,region):
    intersection=ImageChops.logical_and(painted,region)
    return sum(intersection.histogram()[1:])/max(1,sum(region.histogram()[1:]))

def measure(original,screen_w,screen_h,oversized_legacy=False):
    width,height=theatre(screen_w,screen_h)
    portrait=screen_h>screen_w
    if portrait:
        sx,sy,sw,sh=0.,.15*height,width-24.,.56*height
        aw,ah=sw*.72,sh*.72
        ax,ay=sx+sw*.13,sy-sh*.10
        rx,ry,rw,rh=0.,-.27*height,width-48.,.16*height
    else:
        sx,sy,sw,sh=-.18*width,.018*height,.62*width,height-42.
        aw,ah=sw*.78,sh*.84
        ax,ay=sx+sw*.11,sy-sh*.07
        rx,ry,rw,rh=.29*width,.07*height,.34*width,.34*height
    if oversized_legacy:
        aw,ah,ax,ay=sw*1.72,sh*1.72,sx,sy
    # Unity Image.preserveAspect DOES NOT change its child RectTransforms.
    # New production code contains the PARENT to these exact source ratios.
    scale=min(aw/original.width,ah/original.height)
    actual_w=max(1,round(original.width*scale))
    actual_h=max(1,round(original.height*scale))
    alpha=original.getchannel("A").resize(
        (actual_w,actual_h),Image.Resampling.BILINEAR)
    pixels=alpha.point(lambda a:255 if a>128 else 0).convert("1")
    size=(math.ceil(width)+64,math.ceil(height)+64)
    center=(size[0]/2,size[1]/2)
    painted=Image.new("1",size)
    painted.paste(pixels,(round(center[0]+ax-actual_w/2),
                          round(center[1]-ay-actual_h/2)))
    scene=area_mask(size,center,(sx,sy,sw,sh))
    s=min(width/(440. if portrait else 740.),
          height/(700. if portrait else 370.))
    s=max(1.,min(1.5,s))
    heading_w=min(width-116.,280.) if portrait else min(width*.58,330.)
    heading_x=-min(26.,(width-heading_w)*.2) if portrait else sx
    heading_y=height*(.29 if portrait else .405)
    heading=area_mask(size,center,(heading_x,heading_y,
                                   heading_w,min(56.,40.*s)))
    receipt=area_mask(size,center,(rx,ry,rw,rh))
    button=area_mask(size,center,(
        0. if portrait else rx,-height/2+40.,
        min(width-30.,325.) if portrait else min(rw,320.),54.))
    return {name:occupied_ratio(painted,mask) for name,mask in (
        ("real_scenery_covered",scene),
        ("title_covered",heading),
        ("confirmed_report_covered",receipt),
        ("continue_button_covered",button))}

def main():
    source_guard()
    with Image.open(PARCHMENT) as opened:
        original=opened.convert("RGBA")
        assert original.size==(1376,768),"Original authored parchment dimensions changed"
        results=[]
        for w,h in SCREENS:
            m=measure(original,w,h)
            assert .02<m["real_scenery_covered"]<.24, (
                f"Authentic Scout geography obscured or parchment missing {w}x{h}: {m}")
            for key in ("title_covered","confirmed_report_covered",
                        "continue_button_covered"):
                assert m[key]<.03, f"Scout parchment blocks {key} {w}x{h}: {m}"
            results.append(m["real_scenery_covered"])
            print(f"SOURCE ART GATE {w}x{h}: real geography covered "
                  f"{m['real_scenery_covered']:.1%}; key text and action unobscured")
        old=measure(original,390,844,oversized_legacy=True)
        assert old["real_scenery_covered"]>.30, (
            "This regression gate failed to catch the earlier native art defect")
        assert old["title_covered"]>.10
        print(f"DEFECT REPLAY: old huge scroll covered "
              f"{old['real_scenery_covered']:.1%} and title "
              f"{old['title_covered']:.1%} at 390x844; "
              f"new worst-case {max(results):.1%}. NOT A UNITY ART PASS.")

if __name__=="__main__":
    main()
