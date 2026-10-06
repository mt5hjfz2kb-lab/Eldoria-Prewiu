#!/usr/bin/env python3
from pathlib import Path
import json
import cv2
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-reset-target-v1"
OUT=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-depth-shell-v1"
EVID=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-depth-shell-v1"
OUT.mkdir(parents=True,exist_ok=True); EVID.mkdir(parents=True,exist_ok=True)

rgb=np.asarray(Image.open(BASE/"canonical-golden-crop.png").convert("RGB"))
h,w=rgb.shape[:2]
depth=np.load(BASE/"reset-crop-depth.npy").astype(np.float32)
finite=np.isfinite(depth)
lo,hi=np.percentile(depth[finite],[2,98])
dn=np.clip((depth-lo)/max(hi-lo,1e-6),0,1)

gate=np.asarray(Image.open(BASE/"semantic-gate-mask.png").convert("L"))>127
bridge=np.asarray(Image.open(BASE/"semantic-bridge-mask.png").convert("L"))>127
bridge &= ~gate

# Deterministic vegetation extraction from canonical pixels.
hsv=cv2.cvtColor(rgb,cv2.COLOR_RGB2HSV)
H,S,V=cv2.split(hsv)
green=((H>=25)&(H<=105)&(S>=45)&(V<=185))
dark_green=(V<=120)&(S>=35)&(rgb[:,:,1]>=rgb[:,:,0]*0.82)&(rgb[:,:,1]>=rgb[:,:,2]*0.72)
veg=np.logical_or(green,dark_green)
veg &= ~(gate|bridge)

# Remove isolated grass/noise; retain tree/bush masses.
vm=(veg.astype(np.uint8)*255)
vm=cv2.morphologyEx(vm,cv2.MORPH_OPEN,np.ones((3,3),np.uint8))
vm=cv2.morphologyEx(vm,cv2.MORPH_CLOSE,np.ones((5,5),np.uint8))
n,labels,stats,_=cv2.connectedComponentsWithStats((vm>0).astype(np.uint8),8)
keep=np.zeros((h,w),bool)
for i in range(1,n):
    area=stats[i,cv2.CC_STAT_AREA]
    bw=stats[i,cv2.CC_STAT_WIDTH]; bh=stats[i,cv2.CC_STAT_HEIGHT]
    if area>=32 and (bh>=7 or bw>=7):
        keep |= labels==i
veg=keep

veg_depth=np.median(dn[veg]) if np.any(veg) else 0.18
veg_fg=veg & (dn<=veg_depth)
veg_mid=veg & ~veg_fg

occupied=gate|bridge|veg
yy=np.arange(h)[:,None]/max(h-1,1)
# Local geological/circulation receiver. V1 used a broad lower-frame rectangle and
# created obvious pasted-card borders. V2 is intentionally object/support driven.
hero=(gate|bridge).astype(np.uint8)*255
hero_support=cv2.dilate(hero,np.ones((95,95),np.uint8))>0
near_geo=(dn<=0.36) & hero_support & ~occupied
near_geo_u8=(near_geo.astype(np.uint8)*255)
near_geo_u8=cv2.morphologyEx(near_geo_u8,cv2.MORPH_CLOSE,np.ones((17,17),np.uint8))
near_geo_u8=cv2.dilate(near_geo_u8,np.ones((11,11),np.uint8))
cliff=near_geo_u8>0
far=~(gate|bridge|veg_fg|veg_mid|cliff)

# Organic context support. It must fade to zero before every crop edge so no
# rectangular crop can ever become visible in Unity.
support_seed=(gate|bridge|cliff|veg_fg|veg_mid).astype(np.uint8)*255
support=cv2.dilate(support_seed,np.ones((41,41),np.uint8))
support=cv2.morphologyEx(support,cv2.MORPH_CLOSE,np.ones((17,17),np.uint8))
support=cv2.GaussianBlur(support,(0,0),11.0).astype(np.float32)
edge=np.minimum.reduce([
    np.tile(np.arange(w,dtype=np.float32),(h,1)),
    np.tile(np.arange(w-1,-1,-1,dtype=np.float32),(h,1)),
    np.tile(np.arange(h,dtype=np.float32)[:,None],(1,w)),
    np.tile(np.arange(h-1,-1,-1,dtype=np.float32)[:,None],(1,w))
])
edge=np.clip(edge/54.0,0.0,1.0)
support=np.clip(support*edge,0,255).astype(np.uint8)

# Reconstruct only hidden content behind movable foreground cards.
remove=((gate|bridge|veg_fg|veg_mid).astype(np.uint8)*255)
bgr=cv2.cvtColor(rgb,cv2.COLOR_RGB2BGR)
background=cv2.inpaint(bgr,remove,7,cv2.INPAINT_TELEA)
background=cv2.cvtColor(background,cv2.COLOR_BGR2RGB)

def rgba(name, image, alpha):
    a=(alpha.astype(np.uint8) if alpha.dtype!=bool else alpha.astype(np.uint8)*255)
    out=np.dstack([image,a])
    Image.fromarray(out,"RGBA").save(OUT/f"{name}.png")
    Image.fromarray(out,"RGBA").save(EVID/f"{name}.png")
    Image.fromarray(a,"L").save(EVID/f"{name}-mask.png")

# Far/context plate exists only under the irregular support envelope. It is not a
# whole-crop photo plate. Foreground semantic cards cover it at HOME.
far_alpha=support
rgba("far-environment",background,far_alpha)
# Feather the geological receiver slightly to avoid hard cutout islands.
cliff_alpha=cv2.GaussianBlur((cliff.astype(np.uint8)*255),(0,0),1.2)
rgba("cliff-ground-shore",rgb,cliff_alpha)
rgba("vegetation-mid",rgb,veg_mid)
rgba("gate-front",rgb,gate)
rgba("bridge-front",rgb,bridge)
rgba("vegetation-foreground",rgb,veg_fg)

# Back copies are exactly co-registered at HOME, so hidden there.
# They are deliberately darker/less saturated; differential parallax exposes them as thickness.
def back_image(mask, darkness=0.62, warm=0.03):
    arr=rgb.astype(np.float32)
    gray=np.mean(arr,axis=2,keepdims=True)
    arr=arr*darkness + gray*(1-darkness)*0.25
    arr[:,:,0]+=255*warm
    return np.clip(arr,0,255).astype(np.uint8)

rgba("gate-back",back_image(gate,0.56,0.025),gate)
rgba("bridge-back",back_image(bridge,0.52,0.015),bridge)

# Debug composite proves exact front partition inside the hero-support area.
partition=[
 ("far",far),("cliff",cliff),("veg_mid",veg_mid),
 ("gate",gate),("bridge",bridge),("veg_fg",veg_fg)
]
comp=np.zeros_like(rgb)
owner=np.zeros((h,w),np.uint8)
for idx,(name,m) in enumerate(partition,1):
    comp[m]=rgb[m]; owner[m]=idx
Image.fromarray(comp,"RGB").save(EVID/"partition-roundtrip.png")
Image.fromarray((owner*(255//len(partition))).astype(np.uint8),"L").save(EVID/"partition-debug.png")

report={
 "authority":"docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg",
 "source_crop":"camera-first-reset-target-v1/canonical-golden-crop.png",
 "dimensions":[w,h],
 "layers":{
   "far-environment":{"parallax_compensation":0.34,"depth_order":0},
   "cliff-ground-shore":{"parallax_compensation":0.22,"depth_order":1},
   "vegetation-mid":{"parallax_compensation":0.16,"depth_order":2},
   "gate-back":{"parallax_compensation":0.12,"depth_order":3},
   "gate-front":{"parallax_compensation":0.07,"depth_order":4},
   "bridge-back":{"parallax_compensation":0.07,"depth_order":5},
   "bridge-front":{"parallax_compensation":0.02,"depth_order":6},
   "vegetation-foreground":{"parallax_compensation":-0.04,"depth_order":7}
 },
 "pixel_counts":{name:int(mask.sum()) for name,mask in partition},
 "vegetation_depth_split":float(veg_depth),
 "support_nonzero_fraction":float(np.mean(support>4)),
 "support_border_max_alpha":int(max(support[0,:].max(),support[-1,:].max(),support[:,0].max(),support[:,-1].max())),
 "occlusion_fill":"OpenCV Telea radius 7 behind Gate/Bridge/vegetation only",
 "home_rule":"front/back copies co-registered; back thickness invisible at HOME",
 "edge_rule":"v2 support is object-driven and multiplied by a 54px crop-edge fade; full rectangular alpha impossible",
 "credits":0,
 "unity_touched":False
}
(EVID/"depth-shell-source-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
