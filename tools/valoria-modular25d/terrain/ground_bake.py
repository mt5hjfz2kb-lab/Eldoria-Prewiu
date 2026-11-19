#!/usr/bin/env python3
"""Eldoria contiguous isometric ground: small reusable painted tiles.

No fullscreen ground illustration, no raised platforms. Edges share a strict
one-palette grass ring and every tile has original deterministic low-frequency
surface painting plus separate organic road/dirt variations. Output is Unity
Sprite RGBA 132x68 at PPU64, center pivot, approx 2x1 world units.
"""
import hashlib,math,random
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D"
OUT.mkdir(parents=True,exist_ok=True)
SEED=715625
W,H=132,68
GRASS=(94,124,87)
EARTH=(133,113,85)

def noise(seed,x,y):
    k=math.sin(x*12.9898+y*78.233+seed*21.72)*43758.545
    return (k-math.floor(k))*2-1

def field(seed,x,y):
    # coordinate-stable octave blend, with no high-frequency white-noise carpet.
    result=0.;weight=0.
    for scale,amp in [(46,.49),(24,.27),(13,.16),(6,.08)]:
        ix=math.floor(x/scale);iy=math.floor(y/scale)
        tx=(x/scale-ix);ty=y/scale-iy
        tx=tx*tx*(3-2*tx);ty=ty*ty*(3-2*ty)
        n=lambda a,b:noise(seed,ix+a,iy+b)
        lo=n(0,0)*(1-tx)+n(1,0)*tx
        hi=n(0,1)*(1-tx)+n(1,1)*tx
        result+=amp*(lo*(1-ty)+hi*ty)
        weight+=amp
    return result/weight

def inside(x,y,pad=0):
    # True 2:1 diamond. Border has identical solid material on all variants.
    return abs(x-66)/65+abs(y-34)/33 < 1-pad

def paint(kind,variant):
    rng=random.Random(SEED+kind*1009+variant*67)
    im=Image.new("RGBA",(W,H),(0,0,0,0))
    px=im.load()
    # Low-scale colour harmonics deliberately restricted: no glitter/noise carpet.
    for y in range(H):
        for x in range(W):
            if not inside(x,y):continue
            bound=abs(x-66)/65+abs(y-34)/33
            factor=max(0.,min(1.,(0.95-bound)*13))
            n=field(80+variant*5,x+variant*71,y+kind*39)
            delta=round(n*15*factor)
            # The same edge neutral ring masks cross-tile seams under bilinear.
            shade=(GRASS[0]+delta,GRASS[1]+delta+2,GRASS[2]+delta)
            px[x,y]=tuple(max(0,min(255,v)) for v in shade)+(255,)
    d=ImageDraw.Draw(im,"RGBA")
    if kind in (1,2):
        # Soil patch defined as irregular hand-laid diamond INSIDE tile.
        # The global road is continuous in world-space, not built from rectangles.
        points=[]
        for i in range(18):
            a=2*math.pi*i/18
            rx=(35 if kind==1 else 42)*(1+rng.uniform(-.17,.17))
            ry=(14 if kind==1 else 16)*(1+rng.uniform(-.14,.14))
            points.append((66+rx*math.cos(a),34+ry*math.sin(a)))
        d.polygon(points,fill=(120,101,78,222))
        for _ in range(65):
            x=rng.uniform(31,102); y=rng.uniform(22,48)
            if ((x-66)/38)**2+((y-34)/15)**2>1:continue
            c=rng.choice([(170,146,103,95),(69,69,60,70),(168,164,140,95)])
            d.ellipse((x-1,y-.5,x+2,y+1),fill=c)
    if kind==3:
        for _ in range(4):
            x=rng.uniform(31,100);y=rng.uniform(23,45)
            r=rng.uniform(2,4)
            d.ellipse((x-r*1.3,y-r*.54,x+r*1.3,y+r*.54),fill=(82,104,88,112))
            d.arc((x-r*1.3,y-r*.54,x+r*1.3,y+r*.54),180,350,
                  fill=(154,148,123,145),width=1)

    for _ in range(69):
        x=rng.randint(14,116);y=rng.randint(12,56)
        if not inside(x,y,.12):continue
        if kind in (1,2) and ((x-66)/39)**2+((y-34)/17)**2<1:continue
        h=rng.uniform(1,3.8)
        c=rng.choice([(70,104,76,150),(149,169,114,125),(183,156,109,90)])
        d.line((x,y,x+rng.uniform(-1.5,1.5),y+h),fill=c,width=1)
    for _ in range(8):
        x=rng.randint(25,110); y=rng.randint(15,53)
        if not inside(x,y,.14):continue
        d.ellipse((x,y,x+1.4,y+1.4),fill=(185,174,119,150))
    path=OUT/f"ground_{kind}_{variant}.png"
    im.save(path,optimize=True)
    uid=hashlib.md5(("eldoria.modular25d.ground."+str(kind)+"."+str(variant)).encode()).hexdigest()
    meta=f"""fileFormatVersion: 2
guid: {uid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 512
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
  nPOTScale: 0
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spritePixelsToUnits: 64
  spriteAlignment: 9
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 512
    resizeAlgorithm: 0
    textureCompression: 1
    compressionQuality: 90
"""
    Path(str(path)+".meta").write_text(meta)
    return path

for kind in range(4):
    for variation in range(4):
        p=paint(kind,variation)
        print("ELDORIA_GROUND_TILE",p.name,p.stat().st_size)
print("REUSABLE_ISOMETRIC_GROUND_PASS_16")
