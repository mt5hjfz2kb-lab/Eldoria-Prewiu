#!/usr/bin/env python3
"""Import NEW independently generated premium HUD assets. No cropping of old HUD.
Never executes anything from source PNG files; only bounded image decoding.
Generated artwork is public game content, not copied reference pixels.
"""
from __future__ import annotations
import hashlib, io, json, pathlib, urllib.request, math
from PIL import Image
from datetime import datetime, timezone
import numpy as np
import cv2

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

def remove_second_panel_divider(panel):
    """Fix one reproducible AI-generation defect BEFORE Unity import.

    The new lower card is a continuous single slab, but the generator added
    a horizontal decorative divider at 66.5% height. The reference has its
    ONLY divider about 57.5%, rendered by the existing noninteractive native
    BreachWatchSectionDivider. Leaving both results in duplicated gold lines
    and a cost heading crossing the wrong one. Remove the baked second line
    with a tightly bounded material-preserving inpaint. Do not add another
    UI layer, composite a screenshot or alter the source panel silhouette.
    """
    source=np.asarray(panel.convert("RGBA")).copy()
    h,w=source.shape[:2]
    x0,x1=round(w*.063),round(w*.937)
    y0,y1=round(h*.649),round(h*.681)
    if x1<=x0 or y1<=y0:raise ValueError("panel inpaint bounds invalid")
    original=source[:,:,:3]
    mask=np.zeros((h,w),dtype=np.uint8)
    mask[y0:y1,x0:x1]=255
    cleaned=cv2.inpaint(original,mask,5,cv2.INPAINT_TELEA)
    # Restore subtle actual nearby mineral grain, with feathered edges;
    # avoid leaving a broad flat smear where the AI divider once was.
    donor_start=round(h*.595)
    for j,y in enumerate(range(y0,y1)):
        fy=min(1.0,(j+1)/20.0,(y1-y)/20.0)
        donor_y=donor_start+j
        if not(0<=donor_y<h):continue
        x=np.arange(x0,x1)
        fx=np.minimum(1.0,np.minimum((x-x0+1)/38.0,(x1-x)/38.0))
        weight=(0.35*fy*fx)[:,None]
        old=cleaned[y,x0:x1].astype(np.float32)
        donor=original[donor_y,x0:x1].astype(np.float32)
        cleaned[y,x0:x1]=np.uint8(np.clip(old*(1-weight)+donor*weight,0,255))
    # Keep original source alpha untouched, including chamfered corners.
    source[:,:,:3]=cleaned
    return Image.fromarray(source,"RGBA")


def isolate_standalone_painting(image,name):
    """Keep ONE real illustrated object, not the model's bonus background strip.

    Some source generators render a magenta-and-olive shadow bar beneath
    the requested sprite, separated by 100 transparent rows. If shipped,
    this becomes a giant rectangular artifact behind the UI. Crop using
    alpha-content bands before creating the genuine Unity sprite.
    """
    if name not in ("panel","mission","profile","resource","upgrade"):
        return image
    image=image.convert("RGBA")
    w,h=image.size
    row_coverage=image.getchannel("A").point(
        lambda v: 255 if v>=90 else 0
    ).resize((1,h),Image.Resampling.BOX)
    values=list(row_coverage.getdata())
    bands=[];start=None
    for y,coverage in enumerate(values):
        live=coverage>=15  # more than 6% of the image width
        if live and start is None:start=y
        if start is not None and (not live or y==h-1):
            end=y if not live else y+1
            if end-start>=max(24,int(h*.08)):
                bands.append((start,end))
            start=None
    if not bands:
        raise ValueError("Generated art has no reliable foreground "+name)
    start,end=bands[0]
    if len(bands)>1:
        print("CLEANROOM_REMOVE_BACKGROUND_STRIP",name,
              "retained",start,end,"discarded",bands[1:])
    # The picked band includes full shadows and hand-painted edges, not
    # cropped from the user's finished HUD reference.
    top=max(0,start-12);bottom=min(h,end+12)
    pic=image.crop((0,top,w,bottom))
    bounds=pic.getchannel("A").point(
        lambda v: 255 if v>24 else 0).getbbox()
    if bounds is None:
        raise ValueError("Source became invisible "+name)
    l,t,r,b=bounds
    pad=16
    obj=pic.crop((max(0,l-pad),max(0,t-pad),
                  min(w,r+pad),min(pic.height,b+pad)))
    # Avoid converting natural metal details into stretched blobs.
    ratio=obj.width/obj.height
    expected={"panel":(.38,1.5),"mission":(1.9,6.0),
              "profile":(1.8,6.0),"resource":(2.1,6.5),
              "upgrade":(2.1,6.5)}
    low,high=expected[name]
    if not low<ratio<high:
        raise ValueError(f"Standalone {name} art ratio {ratio:.2f} invalid")
    return obj


def remove_chroma_edge_halo(image,name):
    """Remove visible magenta fringes, WITHOUT altering geometry or alpha.

    The AI's #FF00FF backdrop leaks into partially transparent pixels.
    Our actual cleanroom alpha pass leaves between 1,031 and 4,976 pink
    pixels per sprite. On green Unity terrain these show as pasted-on
    purple rims. Inpaint only those contaminated edge RGB samples from
    adjacent genuine artwork, while preserving the original alpha channel.
    Benchmarked on the full 10-source sprite kit before use.
    """
    rgba=np.asarray(image.convert("RGBA")).copy()
    alpha=rgba[:,:,3]
    r=rgba[:,:,0].astype(np.int16)
    g=rgba[:,:,1].astype(np.int16)
    b=rgba[:,:,2].astype(np.int16)
    magenta=(r>145)&(b>145)&(g<125)&(r>g*1.45)&(b>g*1.45)
    # Some generator pixels are FULLY opaque neon magenta despite being
    # adjacent to transparent background. Detect the outer three-pixel ring
    # as well, without touching legitimate purple mineral details inside.
    distance=cv2.distanceTransform((alpha>12).astype(np.uint8),
                                    cv2.DIST_L2,3)
    contaminated=(alpha>12)&magenta&((alpha<244)|(distance<3.5))
    changed=int(contaminated.sum())
    if changed:
        repaired=cv2.inpaint(rgba[:,:,:3],
                             contaminated.astype(np.uint8)*255,
                             2,cv2.INPAINT_TELEA)
        rgba[contaminated,:3]=repaired[contaminated]
        # Transparent edges stay transparent. No extra sprite/glow is drawn.
        assert np.array_equal(rgba[:,:,3],alpha)
    nr=rgba[:,:,0].astype(np.int16)
    ng=rgba[:,:,1].astype(np.int16)
    nb=rgba[:,:,2].astype(np.int16)
    residual=((alpha>12)&((alpha<244)|(distance<3.5))&
              (nr>145)&(nb>145)&(ng<125)&
              (nr>ng*1.45)&(nb>ng*1.45))
    if int(residual.sum())>max(20,int(changed*.01)):
        raise ValueError(f"Chroma removal insufficient: {name}")
    print("CLEANROOM_MAGENTA_FRINGE_FIXED",name,
          "removed",changed,"remaining",int(residual.sum()))
    return Image.fromarray(rgba,"RGBA")


def grade_owner_mineral(image,name):
    """Reference-directed material grade, NOT a new overlay.

    The approved UI plate is warm graphite/black mineral (typical interior
    RGB ~31,25,19). AI generation made every 2K blank slate visibly violet
    (interior ~49,45,57). Shift ONLY the low-luminance dark mineral RGB,
    preserving the sculpted gold/bronze edging, the tiny original emblems,
    entire original alpha mask and all source geometry. No fake text or HUD.
    Tested against the actual owner-approved and source-art pixel samples.
    """
    if name not in ("panel","profile","mission","resource"):
        return image
    rgba=np.asarray(image.convert("RGBA")).copy()
    rgb=rgba[:,:,:3].astype(np.float32)
    red,green,blue=rgb[:,:,0],rgb[:,:,1],rgb[:,:,2]
    alpha=rgba[:,:,3]
    luminance=.2126*red+.7152*green+.0722*blue
    metallic=(red>green*1.22)&(green>blue*1.09)&(red>77)
    dark=(alpha>30)&(~metallic)&(luminance<120)
    fade=(np.clip((120-luminance)/45,0,1)*
          np.clip((alpha.astype(np.float32)-30)/90,0,1)*dark)
    # Preserve entire baked icons/crest in their original generated colors;
    # on-screen they live INSIDE the artwork, never in a pasted second disk.
    h,w=alpha.shape
    if name=="profile":fade[:,:round(w*.35)]=0
    if name=="mission":fade[:,:round(w*.23)]=0
    if name=="resource":fade[:,:round(w*.23)]=0
    if name=="panel":fade[:round(h*.16),:round(w*.30)]=0
    darken=(.79,.76,.56) # warm obsidian instead of violet
    for channel,factor in enumerate(darken):
        rgb[:,:,channel]*=(1-fade*(1-factor))
    rgba[:,:,:3]=np.uint8(np.clip(rgb,0,255))
    assert np.array_equal(rgba[:,:,3],alpha)
    print("VIGILIA_WARM_GRAPHITE_ART",name,
          "changed_pixels",int((fade>.2).sum()))
    return Image.fromarray(rgba,"RGBA")


def store(name,image,sourcehash,task,source_label):
    if min(image.size)<90:raise ValueError(f"tiny/failed art: {name} {image.size}")
    image=remove_chroma_edge_halo(image,name)
    image=grade_owner_mineral(image,name)
    BASE.mkdir(parents=True,exist_ok=True)
    dest=BASE/(name+".png")
    image.save(dest,optimize=True)
    # Unity 2D sprite importer: complete illustration, with stable GUID;
    # no 9-slicing in this initial candidate to avoid unexpected edge warping.
    guid=hashlib.sha256(("eldoria-clean-v1/"+name).encode()).hexdigest()[:32]
    # Protect sculpted source corners and the integrated lion/castle sockets
    # when the runtime screen differs from the 2K generator's proportions.
    # Preserve real metal art; only widen or shorten the EMPTY central well.
    w,h=image.size
    if name=="profile":
        # The full round lion relief extends through x=~34% of the source.
        # A 25%-left nine-slice border BISects its outer copper rim, making
        # the right side stretch into the name area. Preserve the ENTIRE
        # heraldic ring inside the non-stretching left-side source region.
        border=(round(w*.35),round(h*.18),round(w*.10),round(h*.18))
    elif name=="mission":border=(round(w*.21),round(h*.18),round(w*.10),round(h*.18))
    elif name=="resource":border=(round(w*.20),round(h*.18),round(w*.10),round(h*.18))
    else:border=(0,0,0,0)
    left,bottom,right,top=border
    # UnityEngine.UI.Image.pixelsPerUnit == Sprite.pixelsPerUnit /
    # Canvas.referencePixelsPerUnit (100 on the independent HUD). The
    # nine-slice border in UI UNITS is spriteBorder * 100 / spritePPU.
    # Earlier source-only QA divided border by spritePPU directly, missing
    # Canvas.referencePixelsPerUnit; PPU=16 enlarged a 624px lion crest
    # to 3900 UI units and Unity squeezed the center of the 182u plate to 0.
    # Author these 2K ornaments to their real on-screen border widths:
    # profile 39u, mission 26.2u and resource 19.96u respectively.
    pixels_per_unit={"profile":2000,"mission":2000,"resource":2600}.get(name,100)
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
  spriteBorder: {{x: {left}, y: {bottom}, z: {right}, w: {top}}}
  spritePixelsToUnits: {pixels_per_unit}
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
    # The image model may wrap five independent medallions into two rows,
    # even when asked for one row. Fixed five vertical strips are NOT valid:
    # they cut circular game icons into half-discs. Identify actual objects.
    img=chroma(img)
    w,h=img.size
    reduction=5
    sw,sh=max(1,w//reduction),max(1,h//reduction)
    alpha=img.getchannel("A").resize((sw,sh),Image.Resampling.NEAREST)
    binary=alpha.point(lambda v: 255 if v>=155 else 0)
    pixels=binary.load()
    visited=bytearray(sw*sh)
    comps=[]
    from collections import deque
    for sy in range(sh):
        for sx in range(sw):
            ix=sy*sw+sx
            if visited[ix] or pixels[sx,sy]==0: continue
            queue=deque([(sx,sy)])
            visited[ix]=1
            count=0
            x0=x1=sx;y0=y1=sy
            while queue:
                x,y=queue.popleft()
                count+=1
                x0=min(x0,x);x1=max(x1,x)
                y0=min(y0,y);y1=max(y1,y)
                for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                    if not(0<=nx<sw and 0<=ny<sh):continue
                    key=ny*sw+nx
                    if not visited[key] and pixels[nx,ny]:
                        visited[key]=1
                        queue.append((nx,ny))
            if count>240:
                comps.append((count,x0,y0,x1+1,y1+1))
    comps.sort(key=lambda item:item[0],reverse=True)
    if len(comps)<5:
        raise ValueError("Model produced fewer than five independent medallions: "+str(comps))
    candidates=comps[:5]
    centers=[((x0+x1)/2,(y0+y1)/2,i) for i,(_,x0,y0,x1,y1) in enumerate(candidates)]
    centers.sort(key=lambda t:t[1])
    # 3+2 grid = common model layout; 5-in-row is also accepted.
    cy_range=centers[-1][1]-centers[0][1]
    if cy_range>sh*.22:
        top=sorted(centers[:3],key=lambda t:t[0])
        bottom=sorted(centers[3:],key=lambda t:t[0])
        order=[v[2] for v in top+bottom]
    else:
        order=[v[2] for v in sorted(centers,key=lambda t:t[0])]
    output=[]
    for name,idx in zip(("castle","provisions","swords","raven","helm"),order):
        _,x0,y0,x1,y1=candidates[idx]
        pad=7
        left=max(0,x0*reduction-pad);top=max(0,y0*reduction-pad)
        right=min(w,x1*reduction+pad);bottom=min(h,y1*reduction+pad)
        crop=img.crop((left,top,right,bottom))
        # Strong acceptance: nothing may touch its crop edge, and the shape
        # must be roughly round in BOTH dimensions (no sliced icon halves).
        ratio=crop.width/crop.height
        if not .75<ratio<1.28:
            raise ValueError(f"Cut-up medallion {name}: {crop.size}; abort")
        output.append((name,crop))
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
            pic=isolate_standalone_painting(chroma(img),key)
            if key=="panel" and task=="a1a6753c-0df4-4624-87fb-a0ee977413a0":
                pic=remove_second_panel_divider(pic)
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
