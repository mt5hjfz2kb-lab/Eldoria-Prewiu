#!/usr/bin/env python3
"""Import NEW independently generated premium HUD assets. No cropping of old HUD.
Never executes anything from source PNG files; only bounded image decoding.
Generated artwork is public game content, not copied reference pixels.
"""
from __future__ import annotations
import hashlib, io, json, pathlib, urllib.request, math
from PIL import Image
from datetime import datetime, timezone

BASE=pathlib.Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/CleanRoom")
INPUT=pathlib.Path("tools/breach-watch/cleanroom-source-links.json")
MAX=18_000_000
MIN_EDGE=700

def sha(b): return hashlib.sha256(b).hexdigest()
def chroma(src):
    rgb=src.convert("RGB")
    width,height=rgb.size
    rgba=Image.new("RGBA",(width,height))
    sp=rgb.load();dp=rgba.load()
    for y in range(height):
        for x in range(width):
            r,g,b=sp[x,y]
            # Remove ONLY the clean saturated magenta isolation backdrop.
            # Antique burgundy, violet mineral details and warm gold remain.
            is_magenta=(r>130 and b>120 and g<150 and
                        r>g*1.52 and b>g*1.48 and
                        abs(r-b)<100)
            if is_magenta:
                dist=math.sqrt((255-r)**2+g*g+(255-b)**2)
                alpha=int(max(0,min(255,(dist-24)*3.5)))
            else: alpha=255
            if alpha<12: dp[x,y]=(0,0,0,0)
            else:
                if alpha<255:
                    a=alpha/255.
                    r=int(max(0,min(255,(r-(1-a)*255)/a)))
                    g=int(max(0,min(255,g/a)))
                    b=int(max(0,min(255,(b-(1-a)*255)/a)))
                dp[x,y]=(r,g,b,alpha)
    bbox=rgba.getchannel("A").point(lambda v:255 if v>35 else 0).getbbox()
    if not bbox: raise ValueError("No isolated foreground; cannot import")
    return rgba.crop(bbox)

def store(name,image,sourcehash,task,source_label):
    if min(image.size)<90:raise ValueError(f"tiny/failed art: {name} {image.size}")
    BASE.mkdir(parents=True,exist_ok=True)
    dest=BASE/(name+".png")
    image.save(dest,optimize=True)
    # Unity 2D sprite importer: complete illustration, with stable GUID;
    # no 9-slicing in this initial candidate to avoid unexpected edge warping.
    guid=hashlib.sha256(("eldoria-clean-v1/"+name).encode()).hexdigest()[:32]
    meta=f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
  isReadable: 0
  textureType: 8
  spriteMode: 1
  spritePixelsToUnits: 100
  alphaIsTransparency: 1
  textureShape: 1
  maxTextureSize: 4096
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 4096
    resizeAlgorithm: 0
    textureCompression: 1
    compressionQuality: 50
  spritePackingTag:
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    pathlib.Path(str(dest)+".meta").write_text(meta)
    return dict(asset=name,width=image.width,height=image.height,
                sha256=sha(dest.read_bytes()),source_sha256=sourcehash,
                source_task=task,origin=source_label,derived_from="new_generation_only")

def fetch(url):
    req=urllib.request.Request(url,headers={"User-Agent":"Eldoria-CleanRoom-ArtImport/1.0"})
    with urllib.request.urlopen(req,timeout=90) as response:
        if int(response.status)!=200:raise ValueError("PNG fetch failed")
        data=response.read(MAX+1)
    if len(data)>MAX or len(data)<80_000 or not data.startswith(bytes((137,80,78,71,13,10,26,10))):
        raise ValueError("Invalid large PNG, refusing asset")
    img=Image.open(io.BytesIO(data))
    img.verify()
    img=Image.open(io.BytesIO(data))
    if min(img.size)<MIN_EDGE or max(img.size)>4096:
        raise ValueError(f"Image outside supported size {img.size}")
    return data,img

def atlas_medals(img):
    img=chroma(img)
    a=img.getchannel("A")
    w,h=img.size
    # Five independent equal-width lanes. Every lane gets its own alpha
    # bbox, never painted atop an existing UI button or chopped from
    # a screenshot of Eldoria.
    output=[]
    for i,name in enumerate(("castle","provisions","swords","raven","helm")):
        l=round(i*w/5);r=round((i+1)*w/5)
        lane=img.crop((l,0,r,h))
        bbox=lane.getchannel("A").point(lambda v:255 if v>45 else 0).getbbox()
        if not bbox:raise ValueError("Medal not present in lane "+str(i))
        # Keep proportional padding, keep pixels/embossing intact.
        obj=lane.crop(bbox)
        output.append((name,obj))
    return output

def main():
    sources=json.loads(INPUT.read_text())
    assert sorted(sources)==sorted(("panel","mission","medals","profile","resource","upgrade"))
    entries=[]
    for key in ("panel","mission","medals","profile","resource","upgrade"):
        item=sources[key];url=item["url"];task=item["task"]
        print(f"IMPORT_NEW_AI_SOURCE {key} task={task}") # never print URL/JWT
        raw,img=fetch(url)
        if key=="medals":
            for name,sprite in atlas_medals(img):
                entries.append(store("nav_"+name,sprite,sha(raw),task,key))
        else:
            pic=chroma(img)
            entries.append(store(key,pic,sha(raw),task,key))
    assert len(entries)==10
    # Source links are temporary; final published manifest contains only
    # generated file hashes and model identifiers, not expiring JWT URLs.
    manifest={"pipeline":"cleanroom-new-asset-generation-v1",
       "created_utc":datetime.now(timezone.utc).isoformat(),
       "reference_pixels_reused":False,"legacy_hud_asset_reused":False,
       "output_count":len(entries),"source_count":6,"assets":entries,
       "art_approved":False,"unity_native_qa":"pending"}
    (BASE/"manifest.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+chr(10))
    preview=Image.new("RGB",(1200,1600),(35,38,36))
    def thumb(name,box):
        asset=next(BASE.glob(name+".png"))
        im=Image.open(asset).convert("RGBA")
        max_w,max_h=box[2]-box[0],box[3]-box[1]
        im.thumbnail((max_w,max_h),Image.Resampling.LANCZOS)
        x=box[0]+(max_w-im.width)//2;y=box[1]+(max_h-im.height)//2
        preview.paste(im,(x,y),im)
    thumb("panel",(620,80,1160,1040))
    thumb("mission",(35,110,575,330))
    thumb("profile",(35,355,575,550))
    thumb("resource",(35,590,575,725))
    thumb("upgrade",(35,755,575,915))
    for i,k in enumerate(("castle","provisions","swords","raven","helm")):
        thumb("nav_"+k,(100+i*205,1120,280+i*205,1340))
    preview.save(BASE/"cleanroom_preview.jpg",quality=90)
    print("CLEANROOM_NEW_ASSETS_READY",len(entries))
if __name__=="__main__": main()
