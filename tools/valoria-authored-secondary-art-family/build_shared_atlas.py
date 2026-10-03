import os, math
import numpy as np
from PIL import Image

ROOT=os.environ.get("ELDORIA_ASF_MATERIAL_OUT","pipeline/candidates/valoria-authored-secondary-art-family-v1/materials")
os.makedirs(ROOT,exist_ok=True)
SIZE=512
rng=np.random.default_rng(7321)

def save_pair(name, rgb, height, strength=3.0):
    rgb=np.clip(rgb,0,1)
    Image.fromarray((rgb*255).astype(np.uint8),"RGB").save(os.path.join(ROOT,name+"_diff.png"))
    gy,gx=np.gradient(height.astype(np.float32))
    nx=-gx*strength; ny=-gy*strength; nz=np.ones_like(height)
    l=np.sqrt(nx*nx+ny*ny+nz*nz)
    n=np.stack([(nx/l)*.5+.5,(ny/l)*.5+.5,(nz/l)*.5+.5],axis=2)
    Image.fromarray((np.clip(n,0,1)*255).astype(np.uint8),"RGB").save(os.path.join(ROOT,name+"_normal.png"))

yy,xx=np.mgrid[0:SIZE,0:SIZE]

# Warm ashlar stone: staggered courses, restrained noise, readable mortar.
noise=rng.normal(0,1,(SIZE,SIZE))
for _ in range(3):
    noise=(noise+np.roll(noise,1,0)+np.roll(noise,-1,0)+np.roll(noise,1,1)+np.roll(noise,-1,1))/5
stone_h=.58+.08*noise
mortar=np.zeros((SIZE,SIZE),dtype=bool)
course=64
for y in range(0,SIZE,course):
    mortar[max(0,y-2):min(SIZE,y+3),:]=True
for row,y in enumerate(range(0,SIZE,course)):
    offset=40 if row%2 else 0
    for x in range(-offset,SIZE,96):
        mortar[y:min(SIZE,y+course),max(0,x-2):min(SIZE,x+3)]=True
stone_h[mortar]=.20
stone_base=np.array([.61,.58,.50])
stone_rgb=stone_base[None,None,:]*(.88+stone_h[...,None]*.22)
stone_rgb[mortar]=np.array([.34,.33,.30])
save_pair("ASF_Stone",stone_rgb,stone_h,4.2)

# Timber: linear grain + fine variation + subtle knots.
grain=.5+.18*np.sin(xx*.11+np.sin(yy*.025)*1.8)+.08*np.sin(xx*.37)
grain+=rng.normal(0,.045,(SIZE,SIZE))
timber_h=.50+.22*grain
for cx,cy in [(120,130),(360,290),(250,420)]:
    r=np.sqrt(((xx-cx)/34)**2+((yy-cy)/20)**2)
    ring=np.exp(-((r-1.0)**2)*10)
    timber_h-=ring*.18
timber_base=np.array([.34,.20,.105])
timber_rgb=timber_base[None,None,:]*(.74+timber_h[...,None]*.52)
save_pair("ASF_Timber",timber_rgb,timber_h,3.6)

# Blue slate: staggered shingles, subtle cool variation.
slate_h=.52+rng.normal(0,.025,(SIZE,SIZE))
rowh=42
for row,y in enumerate(range(0,SIZE,rowh)):
    slate_h[max(0,y-2):min(SIZE,y+2),:]-=.22
    offset=28 if row%2 else 0
    for x in range(-offset,SIZE,56):
        slate_h[y:min(SIZE,y+rowh),max(0,x-1):min(SIZE,x+2)]-=.12
slate_base=np.array([.17,.27,.33])
slate_rgb=slate_base[None,None,:]*(.83+slate_h[...,None]*.34)
save_pair("ASF_Roof",slate_rgb,slate_h,5.0)

print("VALORIA_ASF_SHARED_ATLAS=PASS")
for f in sorted(os.listdir(ROOT)): print(f,os.path.getsize(os.path.join(ROOT,f)))
