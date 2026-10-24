"""World-space macro grassland color on NEW continental atlases only.

Every approved AF/AG/AI source pixel, original UV, mesh face and gameplay
target remains unchanged. This is an artistic experiment requiring native A/B.
"""
import math
import numpy as np

# Deliberately asymmetric, broad landcover swaths positioned in metres.
# Separate warm highland meadows from cooler, greener hollows; no added POIs.
_CANONICAL_MEADOW_SWATHS = (
 (-640,1420,300,390,.12,(1.31,1.19,.78)),
 (240,1460,420,360,1.42,(1.15,1.08,.80)),
 (-590,860,340,330,2.26,(.83,1.13,.86)),
 (340,800,350,440,1.84,(1.25,1.14,.80)),
 (-610,140,240,480,2.90,(1.18,1.12,.85)),
 (450,100,310,410,.92,(.84,1.11,.89)),
 (-420,-620,420,360,1.21,(1.25,1.17,.81)),
 (410,-790,400,360,2.65,(.87,1.12,.87)),
 (-560,-1350,350,390,.63,(1.27,1.14,.78)),
 (340,-1500,390,480,2.03,(1.14,1.13,.82)),
 (-1190,700,240,360,.58,(.86,1.11,.87)),
 (1200,-900,280,370,1.15,(1.21,1.11,.82)),
)

def paint_canonical_macro_pigment(rgba, X, Y, bounds, meadow_mean):
    """In-place color-only treatment, same absolute coordinates in both atlases."""
    left,bottom,right,top=bounds
    rows,cols=X.shape
    assert Y.shape==X.shape and rgba.shape==(rows,cols,4)
    assert right>left and top>bottom and np.isfinite(meadow_mean).all()
    # Restore warm readability to the current uniform dark-olive paint.
    # AF/AG source faces are a DIFFERENT material and are never passed in.
    rgba[:,:,:3]=np.clip(rgba[:,:,:3]*np.array((1.095,1.085,.965)),.012,.97)
    for cx,cy,rx,ry,phase,mult in _CANONICAL_MEADOW_SWATHS:
        reach=max(rx,ry)*1.56
        x0=max(0,int((cx-reach-left)/(right-left)*(cols-1)))
        x1=min(cols,int((cx+reach-left)/(right-left)*(cols-1))+2)
        y0=max(0,int((cy-reach-bottom)/(top-bottom)*(rows-1)))
        y1=min(rows,int((cy+reach-bottom)/(top-bottom)*(rows-1))+2)
        if x1<=x0 or y1<=y0:continue
        area=np.s_[y0:y1,x0:x1]
        dx=X[area]-cx;dy=Y[area]-cy
        ca,sa=math.cos(phase),math.sin(phase)
        u=dx*ca+dy*sa;v=-dx*sa+dy*ca
        # Broken, directed geography, not repetitive round camouflage spots.
        u=u+30*np.sin(v/173+phase)+11*np.sin(dy/53-phase)
        v=v+24*np.sin(u/127-phase)+17*np.sin(dx/79+phase)
        nu=u/rx;nv=v/ry
        angle=np.arctan2(nv,nu)
        rim=1+.16*np.sin(3*angle+phase)+.07*np.cos(5*angle-phase)
        footprint=np.clip((rim-(nu*nu+nv*nv))*.94,0,1)
        weight=(footprint*footprint*(3-2*footprint))*.72
        palette=np.clip(np.asarray(meadow_mean)*np.asarray(mult),.012,.97)
        rgb=rgba[area][:,:,:3]
        rgb*=1-weight[:,:,None]
        rgb+=palette[None,None,:]*weight[:,:,None]
    # The macro elevation is already sculpted: shade those existing five hills.
    # Pigment only; no new terrain, roads, objects, blockers or model topology.
    for cx,cy,rx,ry in ((-490,520,100,160),(380,680,90,145),
                        (-580,-570,100,95),(-390,1470,150,210),
                        (320,-1510,180,160)):
        area=(np.abs(X-cx)<rx*2)&(np.abs(Y-cy)<ry*2)
        if not np.any(area):continue
        dx=(X[area]-cx)/rx;dy=(Y[area]-cy)/ry
        hill=np.exp(-(dx*dx+dy*dy))
        light=np.clip((-.52*dx+.74*dy)*hill,-.26,.26)
        rgb=rgba[:,:,:3][area]
        rgb=np.clip(rgb+light[:,None]*np.array((.24,.18,.11)),.015,.96)
        rgba[:,:,:3][area]=rgb
    return rgba
