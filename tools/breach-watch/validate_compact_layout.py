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

def check(size:tuple[int,int]):
    pw,ph=size
    portrait=ph>pw
    scale=(max(.86,min(1.75,pw/430)) if portrait else
        max(.86,min(1.30,min(pw/1250,ph/680))))
    w=pw/scale
    h=ph/scale
    m=13 if portrait else 18
    boxes={}
    panels=[]
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
        place("identity",m,11,182,55)
        right=w-(m+182+9+m)
        chip=max(58,(right-8)/3)
        for i in range(3):
            place("resource"+str(i),w-m-(3*chip+8)+i*(chip+4),18,chip,38)
        place("mission",m,125,min(250,w*.62),89)
        place("breach",w*.52-17,84,34,34)
        for i in range(3):
            place("disabled seal"+str(i),m,h-338+i*53,49,49)
        navsize,gap=56,11
        navw=5*navsize+4*gap
        navtop=h-100
        for i in range(5):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        panelw=min(198,w*.46)
        panely=navtop-324-45
        context=place("context",w-m-panelw,panely,panelw,324)
        touch("close",R(context.x+panelw-57,context.y+3,52,52))
        touch("action",R(context.x+12,context.y+324-55,panelw-24,52))
        info=R(13,149,panelw-26,44)
        costlabel=R(13,196,panelw-26,21)
        costdata=R(15,220,panelw-28,28)
        state=R(13,249,panelw-26,19)
        action=R(12,269,panelw-24,52)
        assert not info.intersects(costlabel),size
        assert not costlabel.intersects(costdata),size
        assert not costdata.intersects(state),size
        assert not state.intersects(action),size
        assert not boxes["mission"].intersects(context),size
        limit=36
    else:
        place("identity",m,11,234,58)
        chip=min(100,max(82,(w*.28-12)/3))
        resw=3*chip+14
        for i in range(3):
            place("resource"+str(i),w-m-resw+i*(chip+7),16,chip,41)
        place("mission",m,102,225,83)
        place("breach",w*.49-17,11,34,34)
        for i in range(3):
            place("disabled seal"+str(i),m,h-179+i*48,43,43)
        navsize,gap=55,15
        navw=5*navsize+4*gap
        navtop=h-78
        for i in range(5):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        contextw=210 if h<530 else 220
        contexth=min(365,h-174)
        contexty=75 if h<530 else 118
        context=place("context",w-m-contextw,contexty,contextw,contexth)
        touch("close",R(context.x+contextw-57,context.y+3,52,52))
        if contexth<305:
            action=R(12,contexth-56,contextw-24,53)
            costs=R(12,166,contextw-24,24)
            state=R(12,197,contextw-24,20)
            func=R(114,62,contextw-124,76)
            heading=R(12,141,contextw-24,20)
        else:
            action=R(12,contexth-55,contextw-24,49)
            costs=R(13,239,contextw-26,30)
            state=R(13,272,contextw-26,23)
            func=R(13,163,contextw-26,46)
            heading=R(13,212,contextw-26,22)
        touch("action",action)
        assert not func.intersects(heading),size
        assert not heading.intersects(costs),size
        assert not costs.intersects(state),size
        assert not state.intersects(action),size
        assert context.y+context.h<=navtop-2,size
        limit=32 if ph<500 else 24
    assert not boxes["identity"].intersects(boxes["resource0"]),size
    compact=coverage(panels,w,h)
    assert compact<=limit,(size,compact,limit)
    print(f"VIGILIA_SOURCE_GEOMETRY_PASS {pw}x{ph} {compact:.1f}% upper-mask "
          f"max {limit}% 5x single-row nav, >=44px touch, no cost/state/button overlap")

def main():
    source=SOURCE.read_text(encoding="utf-8")
    expected=[
        'Mathf.Clamp(pixelWidth/430f,.86f,1.75f)',
        'At(identity,margin,11,182,55)',
        'float navSize=56,navGap=11',
        'float navTop=h-100f',
        'float ph=324f',
        'At(((RectTransform)actionButton.transform),12,ph-55,pw-24,52)',
        'float navSize=55,navGap=15',
        'float navTop=h-78',
        'float pw=h<530f?210f:220f',
        'float ph=Mathf.Min(365,h-174f)',
        'At(((RectTransform)actionButton.transform),12,ph-56,pw-24,53)',
        'At(((RectTransform)closeButton.transform),pw-57,3,52,52)'
    ]
    for pattern in expected:
        assert pattern in source, f"Runtime contract changed: {pattern}"
    for size in [(1600,900),(900,1600),(844,390),(390,844)]:
        check(size)
    print("BOUNDARY: source-level layout estimate, requires independent Unity render and actual pixels")

if __name__=="__main__":
    main()
