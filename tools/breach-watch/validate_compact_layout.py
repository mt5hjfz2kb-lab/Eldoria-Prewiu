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
        # Approved lion badge is a COMPLETE one-piece circle, with a real
        # ~15px breathing gap before Prueba Valoria at 900px. A previous
        # source split the lion rim at 25%, causing an extended wing and
        # player name painted ON the gold emblem.
        painted_lion_right=round(2496*.35)*100/2000
        name_left=(45+8) if pw<600 else (37+16)
        assert name_left-painted_lion_right>=8,(size,'player name crosses source lion ring')
        profile_name=R(0,5 if pw<600 else 4,1,22 if pw<600 else 18)
        profile_power=R(0,27 if pw<600 else 22,1,18 if pw<600 else 17)
        assert profile_name.y+profile_name.h<=profile_power.y+.5,size
        # Real ink was touching the lower profile bevel, not a mere layout
        # style issue: both dynamic lines must now have bottom inset and
        # no overlapping nominal rows in the source contract.
        assert profile_power.y+profile_power.h<= (52 if pw<600 else 43)-4,size
        right=w-(m+182+9+m)
        chip=(min(85,max(72,(right-4)/2)) if resources==2 else min(70,max(58,(right-8)/3))) if pw<600 else 62
        resgap=4 if pw<600 else 12
        resw=resources*chip+resgap*(resources-1)
        resource_left=w-m-resw if pw<600 else min(w-m-resw,w*.52)
        for i in range(resources):
            place("resource"+str(i),resource_left+i*(chip+resgap),18 if pw<600 else 17,chip,38 if pw<600 else 28)
            symbolsize=25 if pw<600 else 19
            assert 4+symbolsize+1.5<=symbolsize+8,size
        missionw=min(211 if pw<600 else 170,w*(.49 if pw<600 else .38))
        # The new alpha-trimmed carved source starts at its real painted edge.
        place("mission",m,80 if pw<600 else 82,missionw,80 if pw<600 else 60)
        # The approved castle badge is part of the single real PNG; live
        # information starts after its unscaled nine-slice left border.
        chapter_text=R(54 if pw<600 else 36,10 if pw<600 else 7,missionw-(64 if pw<600 else 44),17 if pw<600 else 15)
        assert chapter_text.x>=36 and chapter_text.x+chapter_text.w<=missionw-8,size
        assert chapter_text.w >= (144.7 if pw<600 else 122),size
        if pw<600:
            assert chapter_text.w>=144.7,(size,"13pt approved title must fit")
        # The genuine native 900x1600 view failed with only ~1 pixel of
        # breathing room underneath '0/1'. Keep ink away from the lower bevel.
        short_objective=R(59 if pw<600 else 36,28 if pw<600 else 22,missionw-(70 if pw<600 else 44),12 if pw<600 else 10)
        progress=R(59 if pw<600 else 36,42 if pw<600 else 34,missionw-(70 if pw<600 else 44),18 if pw<600 else 12)
        assert chapter_text.y+chapter_text.h<=short_objective.y+1,size
        assert short_objective.y+short_objective.h+2<=progress.y,size
        assert progress.y+progress.h<= (80 if pw<600 else 60)-(8 if pw<600 else 5),size
        assert 87+20<=112-3,size
        # Compact source plaque keeps native text within both real bevels.
        assert missionw/(80 if pw<600 else 60)>=2.6,size
        assert short_objective.y+short_objective.h<progress.y,size
        place("breach",w*.50-56,69,112,30)
        # Future utility buttons do not draw or intercept pointer events.
        navsize,gap=(70,10) if pw<600 else (58,5)
        inset,art_scale=(4,.92) if pw<600 else (5,.90)
        painted_physical=(navsize-2*inset)*art_scale*scale
        assert painted_physical>=38,(size,"medallion painted region below 38px",painted_physical)
        assert 5*navsize+4*gap<=w-2*m,(size,"all five unlocked medals would overflow")
        navw=nav_count*navsize+(nav_count-1)*gap
        navtop=h-(111 if pw<600 else 90)
        for i in range(nav_count):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        panelw=min(171,w*.42) if pw<600 else 158
        card_right=m if pw<600 else 39
        panely=navtop-297-(34 if pw<600 else 72)
        context=place("context",w-card_right-panelw,panely,panelw,297)
        touch("close",R(context.x+panelw-55,context.y+14,50,50))
        header=R(45,21,panelw-104,21)
        level=R(45,43,panelw-104,17)
        assert header.y+header.h+.7<=level.y,size
        assert header.x+header.w<=panelw-55-3.5,size
        assert level.x+level.w<=panelw-55-3.5,size
        touch("action",R(context.x+6,context.y+297-53,panelw-12,50))
        # Real CleanRoom one-piece painted well has its gold lower rule at
        # 52.1% of the 280u card = 145.9u. The live selected mesh and
        # two-line text must fit INSIDE, not across the built-in painted line.
        # The original separate uGUI divider is now disabled for portrait.
        real_painted_rule=160.785*297/280
        portraitphoto=R(15,61,panelw-30,74)
        info=R(13,146,panelw-26,22)
        costlabel=R(13,174,panelw-26,17)
        costdata=R(13,194,panelw-26,20)
        # Actual Unity text numbers are fully separate live labels:
        # retain readable 450 and 300 with gap after true 19u icons.
        wood_icon=R(14,194,19,19)
        wood_qty=R(39,194,42,20)
        stone_icon=R(89,194,19,19)
        stone_qty=R(110,194,panelw-115,20)
        assert wood_icon.x+wood_icon.w+3<=wood_qty.x,size
        assert stone_icon.x+stone_icon.w+2<=stone_qty.x,size
        assert stone_qty.x+stone_qty.w<=panelw-4,size
        state=R(13,220,panelw-26,17)
        action=R(6,244,panelw-12,50)
        # A native 512x320 RGBA render of the genuine Bastion occupies
        # y=.12..85 and x=.24..78. The final raw displayed viewport
        # minus its 2u inset MUST accommodate its vertical silhouette
        # without invoking the old aspect-clipping clamp.
        raw_display_w=portraitphoto.w-4
        raw_display_h=portraitphoto.h-4
        visible_uv_height=1/((raw_display_w/raw_display_h)/(512/320))
        assert visible_uv_height>=.80,(size,"True flag/stairs cannot fit in portrait aperture")
        assert portraitphoto.y+portraitphoto.h+3<=info.y,size
        assert info.y+info.h+1.5<=real_painted_rule,size
        assert costlabel.y>=real_painted_rule+2,size
        assert not info.intersects(costlabel),size
        assert not costlabel.intersects(costdata),size
        assert not costdata.intersects(state),size
        assert not state.intersects(action),size
        assert not boxes["mission"].intersects(context),size
        limit=34
    else:
        place("identity",m,11,234,58)
        profile_name=R(0,6,1,30)
        profile_power=R(0,36,1,21)
        assert profile_name.y+profile_name.h<=profile_power.y,size
        chip=(min(110,max(82,(w*.28-7)/2)) if resources==2 else min(100,max(82,(w*.28-12)/3)))
        resw=resources*chip+7*(resources-1)
        for i in range(resources):
            place("resource"+str(i),w-m-resw+i*(chip+7),16,chip,41)
            assert 5+30+1.5<=38,size
        place("mission",m,100,256,92)
        chapter_text=R(66,14,174,20)
        assert chapter_text.x>=65 and chapter_text.x+chapter_text.w<=240,size
        place("breach",w*.49-17,11,34,34)
        compact=h<530
        navsize,gap=(75,10) if compact else (55,15)
        inset,art_scale=(4,.92) if compact else (6,.86)
        painted_physical=(navsize-2*inset)*art_scale*scale
        assert painted_physical>=38,(size,"medallion painted region below 38px",painted_physical)
        assert 5*navsize+4*gap<=w-2*m,(size,"all five unlocked medals would overflow")
        navw=nav_count*navsize+(nav_count-1)*gap
        navtop=h-(88 if compact else 78)
        for i in range(nav_count):
            item=place("nav"+str(i),(w-navw)*.5+i*(navsize+gap),navtop+3,navsize,navsize)
            touch("nav"+str(i),item)
        contextw=234 if h<530 else 220
        contexth=min(330,h-174)
        contexty=75 if h<530 else 118
        context=place("context",w-m-contextw,contexty,contextw,contexth)
        touch("close",R(context.x+contextw-57,context.y+3,52,52))
        header=R(50,8,contextw-111,31)
        level=R(50,40,contextw-111,17)
        assert header.y+header.h+.7<=level.y,size
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
          f"painted medal >=38px, all 5 fit, "
          f"no premature buttons or cost/state/button overlap")

def main():
    source=SOURCE.read_text(encoding="utf-8")
    expected=[
        'Mathf.Clamp(pixelWidth/514f,.91f,3.00f)',
        'At(identity,margin,smallPhone?11f:14f,182,identityHeight)',
        'float lionTextInset=crestSize+(smallPhone?8f:16f);',
        'float navSize=smallPhone?70f:58f,navGap=smallPhone?10f:5f;',
        'float navTop=h-(smallPhone?111f:90f)',
        'float ph=297f',
        'float pw=smallPhone?Mathf.Min(171f,w*.42f):158f;',
        'float cardRightInset=smallPhone?margin:39f;',
        'float chipGap=smallPhone?4f:12f;',
        'float top=navTop-ph-(smallPhone?34f:72f);',
        'At(((RectTransform)actionButton.transform),6,ph-53,pw-12,50)',
        'float navSize=compactLandscape?75f:55f,',
        'float navTop=h-(compactLandscape?88f:78f);',
        'float pw=h<530f?234f:220f',
        'float ph=Mathf.Min(330,h-174f)',
        'At(((RectTransform)actionButton.transform),12,ph-56,pw-24,53)',
        'At(((RectTransform)closeButton.transform),pw-57,3,52,52)',
        'if(host && host.CanOpen(RoyalSection.World))mask|=1<<3;',
        'navButtons[i].gameObject.SetActive(visible);',
        'resourceChips[i].gameObject.SetActive(i<visibleResources);',
        'utility.gameObject.SetActive(false);',
        'lastNavigationMask=navigationMask;',
        'At(chapterName.rectTransform,smallPhone?54f:36f,',
        'chapterName.fontSize=smallPhone?13:11;',
        'chapterName.resizeTextMinSize=smallPhone?13:11;',
        'chapterName.fontSize=14;',
        'At(buildingName.rectTransform,45,21,pw-104,21);',
        'At(buildingName.rectTransform,50,8,pw-111,31);',
        'At(identityPower.rectTransform,59,36,165,21);',
        'At(buildingLevel.rectTransform,45,43,pw-104,17);',
        'At(buildingLevel.rectTransform,50,40,pw-111,17);'
    ]
    for pattern in expected:
        assert pattern in source, f"Runtime contract changed: {pattern}"
    # Real native evidence rejected 10.92px phone and 11.18px landscape.
    # Preserve >=11.75 physical px rather than lower the native quality gate.
    assert 13*.91 >= 11.75, 'phone portrait title below minimum'
    assert 14*.86 >= 11.75, 'short landscape title below minimum'
    # 13 grounded screen resolutions × four actual Chapter-I unlock states
    # = 52 source-level layout checks. They complement, never replace, the
    # eight exact native GPU captures on the real Windows/Unity runner.
    devices=[
        (1600,900),(900,1600),(844,390),(390,844),(1170,2532),
        (1290,2796),(1080,2400),(768,1366),(1284,2778),
        (2340,1080),(720,1280),(1920,1080),(1179,2556),
    ]
    validated=0
    for size in devices:
        for food in (False,True):
            for world in (False,True):
                check(size,food,world)
                validated+=1
    assert validated==52
    print(f"BOUNDARY: {validated} source-only unlocked/layout variants; actual Unity GPU still required")

if __name__=="__main__":
    main()
