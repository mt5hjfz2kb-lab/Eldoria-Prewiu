#!/usr/bin/env python3
"""Postprocess renders into actual Unity sprites with exact baked-ground pivots.

This is authoring, not Unity screenshot capture. Uses only PNG from the original
Blender procedural source; resampling/cropping never fabricates a game scene.
"""
from pathlib import Path
import hashlib, json, math, random, sys
from PIL import Image, ImageDraw, ImageFilter

PROJECT = Path(__file__).resolve().parents[3]
# This repository root is parent[3] from tools/valoria-modular25d/blender/.
ROOT = PROJECT / "Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D"
ORIGIN = Path(sys.argv[1]) if len(sys.argv)>1 else Path("/tmp/eldoria-modular-bake")
ROOT.mkdir(parents=True, exist_ok=True)

def meta(guid, ppu, px, py):
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: {ppu}
  spriteAlignment: 9
  spritePivot: {{x: {px:.6f}, y: {py:.6f}}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureCompression: 2
    compressionQuality: 100
"""

def folder_meta(folder):
    g=hashlib.md5(("eldoria-mod25d-folder-"+folder.name).encode()).hexdigest()
    return f"""fileFormatVersion: 2
guid: {g}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

for folder in (ROOT.parent,ROOT):
    Path(str(folder)+".meta").write_text(folder_meta(folder),encoding="utf-8")

out=[]
for source in sorted(ORIGIN.glob("*_uncropped.png")):
    key=source.name.removesuffix("_uncropped.png")
    record=json.loads((ORIGIN/(key+".json")).read_text(encoding="utf-8"))
    image=Image.open(source).convert("RGBA")
    alpha=image.getchannel("A")
    bbox=alpha.point(lambda value:255 if value>10 else 0).getbbox()
    if bbox is None:raise ValueError("Empty sprite from Blender: "+key)
    fx,fy=record["foot_xy_pixel"]
    left=max(0,min(bbox[0]-18,math.floor(fx)-25))
    top=max(0,min(bbox[1]-18,math.floor(fy)-25))
    right=min(image.width,max(bbox[2]+18,math.ceil(fx)+25))
    bottom=min(image.height,max(bbox[3]+18,math.ceil(fy)+25))
    if left>=right or top>=bottom:raise ValueError("Invalid crop: "+key)
    cropped=image.crop((left,top,right,bottom))
    # A tiny one-pass sharpen offsets bilinear softness at the small iPhone size.
    cropped=cropped.filter(ImageFilter.UnsharpMask(radius=.9,percent=55,threshold=3))
    pivot_x=(fx-left)/cropped.width
    pivot_y=1-(fy-top)/cropped.height
    if not 0<=pivot_x<=1 or not 0<=pivot_y<=1:
        raise ValueError("Foot origin outside cropped sprite: "+key)
    path=ROOT/(key+".png")
    cropped.save(path,optimize=True)
    uid=hashlib.md5(("eldoria-mod25d-sprite-"+key).encode()).hexdigest()
    Path(str(path)+".meta").write_text(meta(uid,96,pivot_x,pivot_y),encoding="utf-8")
    out.append(dict(name=key,size=list(cropped.size),
       foot_pivot=[round(pivot_x,6),round(pivot_y,6)],
       sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
       bytes=path.stat().st_size,source="Blender-generated original - not Unity"))
    print("ART_ASSET",key,cropped.size,tuple(round(v,3) for v in (pivot_x,pivot_y)),path.stat().st_size)

# Do not save a decorative JPG/PNG into the main scene. The authored resources
# remain separate files. A contact sheet is strictly for artist-side review.
manifest={"schema":1,"authoring":"original Blender bake 2.5D","engine_proof":False,
          "generated_from":"tools/valoria-modular25d/blender/bake_valoria.py",
          "palette":"baked common materials + shared camera and lights",
          "sprites":out}
(ROOT/"sprite_manifest.json").write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
images=[entry for entry in out if entry["name"] in
        {"bastion_i","sawmill_i","sawmill_available","sawmill_under_construction",
        "pine_0","villager_walk_0"}]
board=Image.new("RGBA",(1200,820),(24,32,38,255))
d=ImageDraw.Draw(board)
for i,entry in enumerate(images):
    asset=Image.open(ROOT/(entry["name"]+".png")).convert("RGBA")
    asset.thumbnail((350,320),Image.Resampling.LANCZOS)
    col=i%3;row=i//3
    x=40+col*390+(350-asset.width)//2
    y=40+row*390+(330-asset.height)
    board.alpha_composite(asset,(x,y))
    d.text((x,y+asset.height+5),entry["name"],fill=(225,221,210,255))
review=ORIGIN/"artist-only-review-NOT-UNITY.png"
board.convert("RGB").save(review)
print("ART_REVIEW_ONLY",review)
print("TOTAL_RGBA_RAM_ESTIMATE_MIB",
      round(sum(e["size"][0]*e["size"][1]*4 for e in out)/1024**2,2))
