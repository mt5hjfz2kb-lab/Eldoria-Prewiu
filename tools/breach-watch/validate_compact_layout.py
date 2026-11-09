#!/usr/bin/env python3
"""Source-level safe-area geometry guard for Vigilia. NOT a Unity render proof.

This model uses exact numeric anchors from the native uGUI Reflow contract.
Screenshots/camera/sprites are separately certified by BreachWatchNativeEvidence.
Every art/layout change must be checked in Unity once Windows is released.
"""
from __future__ import annotations
from dataclasses import dataclass
from pathlib import Path
import math

SOURCE=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs")

@dataclass(frozen=True)
class R:
    x:float
    y:float
    w:float
    h:float
    def intersects(self,other:"R", tolerance:float=0)->bool:
        return (min(self.x+self.w,other.x+other.w)-max(self.x,other.x)>tolerance and
                min(self.y+self.h,other.y+other.h)-max(self.y,other.y)>tolerance)
    def visible(self,w:float,h:float,tol=0)->bool:
        return self.x>=-tol and self.y>=-tol and self.x+self.w<=w+tol and self.y+self.h<=h+tol

def coverage(rects:list[R],w:float,h:float)->float:
    nx=250;ny=250
    mask=bytearray(nx*ny)
    for r in rects:
        x0=max(0,int(math.floor(r.x/w*nx)))
        x1=min(nx,int(math.ceil((r.x+r.w)/w*nx)))
        y0=max(0,int(math.floor(r.y/h*ny)))
        y1=min(ny,int(math.ceil((r.y+r.h)/h*ny)))
        for y in range(y0,y1):
            for x in range(x0,x1):
                mask[y*nx+x]=1
    return 100*sum(mask)/len(mask)

def check(size:tuple[int,int],food_unlocked:bool=False,world_unlocked:bool=False):
    pw,ph=size
    portrait=ph>pw
    scale=(max(.91,min(3.00,pw/514)) if portrait else
        max(.86,min(1.30,min(pw/1250,ph/680))))
    w=pw/scale
    h=ph/scale
    m=13 if portrait else 18
    boxes={}
    panels=[]
    resources=3 if food_unlocked else 2
    nav_count=3 if world_unlocked else 2
    def place(name,x,y,ww,hh,solid=True):
        r=R(x,y,ww,hh)
        assert r.visible(w,h,1.01),f"{size}: offscreen {name} = {r} inside {w:.1f}x{h:.1f}"
        boxes[name]=r
        if solid:panels.append(r)
        return r
    def touch(name,r):
        assert r.w*scale>=43.5 and r.h*scale>=43.5,(
            f"{size}: {name} physical touch area {r.w*scale:.1f}x{r.h*scale:.1f}")
    if portrait:
        place("identity",m,11 if pw<600 else 14,182,52 if pw<600 else 43)
        right=w-(m+182+9+m)
        chip=(min(85,max(72,(right-4)/2)) if resources==2 else min(70,max(58,(right-8)/3)))
        resw=resources*chip+4*(resources-1)
        for i in range(resources):
            place("resource"+str(i),w-m-resw+i*(chip+4),18 if pw<600 else 17,chip,38 if pw<600 else 28)
        missionw=min(185,w*.46)
        place("mission",m,96 if pw<600 else 82,missionw,78)
        # Gold chapter icon is x=9..29 inside the single authored mission
        # tablet; the actual chapter text must NEVER paint through its rim.
        chapter_glyph=R(9,9,20,20)
        chapter_text=R(31,5,missionw-45,20)
        assert chapter_text.x>=chapter_glyph.x+chapter_glyph.w+1.5,size
        place("breach",w*.52-56,74,112,30)
        # Future utility buttons do not draw or intercept pointer events.
        navsize,gap=49,14
        navw=nav_count*navsize+(nav_count-1)*gap
        navtop=h-100
        for i in range(nav_count):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        panelw=min(171,w*.42)
        panely=navtop-303-45
        context=place("context",w-m-panelw,panely,panelw,303)
        touch("close",R(context.x+panelw-55,context.y+3,50,50))
        header=R(51,7,panelw-110,28)
        level=R(51,34,panelw-110,19)
        assert header.x+header.w<=panelw-55-3.5,size
        assert level.x+level.w<=panelw-55-3.5,size
        touch("action",R(context.x+12,context.y+303-53,panelw-24,50))
        info=R(13,140,panelw-26,42)
        costlabel=R(13,191,panelw-26,15)
        costdata=R(13,208,panelw-26,24)
        state=R(13,232,panelw-26,18)
        action=R(12,250,panelw-24,50)
        assert not info.intersects(costlabel),size
        assert not costlabel.intersects(costdata),size
        assert not costdata.intersects(state),size
        assert not state.intersects(action),size
        assert not boxes["mission"].intersects(context),size
        limit=34
    else:
        place("identity",m,11,234,58)
        chip=(min(110,max(82,(w*.28-7)/2)) if resources==2 else min(100,max(82,(w*.28-12)/3)))
        resw=resources*chip+7*(resources-1)
        for i in range(resources):
            place("resource"+str(i),w-m-resw+i*(chip+7),16,chip,41)
        place("mission",m,102,225,83)
        chapter_glyph=R(9,9,20,20)
        chapter_text=R(37,8,178,20)
        assert chapter_text.x>=chapter_glyph.x+chapter_glyph.w+1.5,size
        place("breach",w*.49-17,11,34,34)
        navsize,gap=55,15
        navw=nav_count*navsize+(nav_count-1)*gap
        navtop=h-78
        for i in range(nav_count):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        contextw=234 if h<530 else 220
        contexth=min(330,h-174)
        contexty=75 if h<530 else 118
        context=place("context",w-m-contextw,contexty,contextw,contexth)
        touch("close",R(context.x+contextw-57,context.y+3,52,52))
        header=R(58,8,contextw-119,31)
        level=R(58,37,contextw-119,19)
        assert header.x+header.w<=contextw-57-3.5,size
        assert level.x+level.w<=contextw-57-3.5,size
        if contexth<305:
            action=R(12,contexth-56,contextw-24,53)
            costs=R(12,166,contextw-24,24)
            state=R(12,197,contextw-24,20)
            func=R(114,62,contextw-124,76)
            heading=R(12,141,contextw-24,20)
        else:
            action=R(12,contexth-48,contextw-24,45)
            costs=R(13,227,contextw-26,30)
            state=R(13,261,contextw-26,19)
            func=R(13,157,contextw-26,40)
            heading=R(13,201,contextw-26,21)
        touch("action",action)
        assert not func.intersects(heading),size
        assert not heading.intersects(costs),size
        assert not costs.intersects(state),size
        assert not state.intersects(action),size
        assert context.y+context.h<=navtop-2,size
        limit=32 if ph<500 else 22
    assert not boxes["identity"].intersects(boxes["resource0"]),size
    compact=coverage(panels,w,h)
    assert compact<=limit,(size,compact,limit)
    print(f"VIGILIA_SOURCE_GEOMETRY_PASS {pw}x{ph} {compact:.1f}% upper-mask "
          f"max {limit}% nav={nav_count} resources={resources}, >=44px touch, "
          f"no premature buttons or cost/state/button overlap")

def main():
    source=SOURCE.read_text(encoding="utf-8")
    expected=[
        'Mathf.Clamp(pixelWidth/514f,.91f,3.00f)',
        'At(identity,margin,smallPhone?11f:14f,182,identityHeight)',
        'float navSize=49,navGap=14',
        'float navTop=h-100f',
        'float ph=303f',
        'At(((RectTransform)actionButton.transform),12,ph-53,pw-24,50)',
        'float navSize=55,navGap=15',
        'float navTop=h-78',
        'float pw=h<530f?234f:220f',
        'float ph=Mathf.Min(330,h-174f)',
        'At(((RectTransform)actionButton.transform),12,ph-56,pw-24,53)',
        'At(((RectTransform)closeButton.transform),pw-57,3,52,52)',
        'if(host && host.CanOpen(RoyalSection.World))mask|=1<<3;',
        'navButtons[i].gameObject.SetActive(visible);',
        'resourceChips[i].gameObject.SetActive(i<visibleResources);',
        'utility.gameObject.SetActive(false);',
        'lastNavigationMask=navigationMask;',
        'At(chapterName.rectTransform,31f,5f,missionWidth-45f,20f);',
        'At(buildingName.rectTransform,51,7,pw-110,28);',
        'At(buildingName.rectTransform,58,8,pw-119,31);'
    ]
    for pattern in expected:
        assert pattern in source, f"Runtime contract changed: {pattern}"
    for size in [(1600,900),(900,1600),(844,390),(390,844),(1170,2532)]:
        for food in (False,True):
            for world in (False,True):
                check(size,food,world)
    print("BOUNDARY: 20 source-only unlocking/layout combinations, needs real Unity pixels")

if __name__=="__main__":
    main()
