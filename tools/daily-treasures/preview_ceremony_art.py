#!/usr/bin/env python3
"""Pre-Unity visual review of actual, hash-checked Eldoria art layers.

ALWAYS supply genuine screenshots/renders for all four identities. Generated
poster Lyra, guessed geography, fake Bastion, loot and empty mockups are not
acceptable substitutes. These are *previews*, NOT Unity screenshots.
"""
import argparse, hashlib, importlib.util, json
from pathlib import Path
from PIL import Image, ImageDraw

EVENTS=("lyra","explorers","vigilants","bastion")
SIZES=((1600,900),(900,1600),(844,390),(390,844))
# The official artist paintings are already committed to Eldoria.
OFFICIAL_PAINTINGS={
    "lyra":"Unity/Assets/Eldoria/Resources/EldoriaV41/heroes-hall-illustrated-v2.jpg",
    "explorers":"Unity/Assets/Eldoria/Resources/EldoriaRetention/scouts-painterly-v2.png",
    "vigilants":"Unity/Assets/Eldoria/Resources/EldoriaRetention/vigilants-painterly-v2.png",
}


OFFICIAL_SYMBOLS={
    "explorers":"Unity/Assets/Eldoria/Resources/EldoriaRetention/scouts-crest.png",
    "vigilants":"Unity/Assets/Eldoria/Resources/EldoriaRetention/vigilants-compass.png",
}

def require(flag,msg):
    if not flag:raise ValueError(msg)

def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def cover(im,w,h):
    k=max(w/im.width,h/im.height)
    nw,nh=round(im.width*k),round(im.height*k)
    im=im.resize((nw,nh),Image.Resampling.LANCZOS)
    return im.crop(((nw-w)//2,(nh-h)//2,(nw+w)//2,(nh+h)//2))

def fit_focal(layer, shape, event):
    """Size the independent prop by its real alpha bounds, NOT the blank
    transparent 1376x768 canvas. On phones a horizontal postcard is forbidden.
    No poster/text is baked into the scene."""
    w,h=shape
    alpha=layer.getchannel("A")
    bbox=alpha.getbbox()
    require(bbox is not None,"Empty isolated prop")
    focal=layer.crop(bbox)
    portrait=h>w
    fraction_w=.89 if portrait else .55
    fraction_h=.55 if portrait else .72
    k=min(w*fraction_w/focal.width,h*fraction_h/focal.height)
    tw,th=max(1,round(focal.width*k)),max(1,round(focal.height*k))
    scaled=focal.resize((tw,th),Image.Resampling.LANCZOS)
    # Keep real game background perceptible around the central object.
    cy=h*(.34 if event=="explorers" and portrait else .43 if portrait else .5)
    x=round((w-tw)/2)
    y=max(6,min(round(cy-th/2),h-th-6))
    return scaled,x,y


def nine_slice_frame(source, shape):
    """Reuse exact original hand-illustrated frame: unchanged corner geometry,
    stretched straight rails, truly clear centre, no fake floating postcard."""
    w,h=shape
    src=source.convert("RGBA")
    sw,sh=src.size
    corner=54
    dest_corner=max(12,min(corner,round(min(w,h)*.08)))
    out=Image.new("RGBA",shape,(0,0,0,0))
    chunks=(
       ((0,0,corner,corner),(0,0,dest_corner,dest_corner)),
       ((sw-corner,0,sw,corner),(w-dest_corner,0,dest_corner,dest_corner)),
       ((0,sh-corner,corner,sh),(0,h-dest_corner,dest_corner,dest_corner)),
       ((sw-corner,sh-corner,sw,sh),(w-dest_corner,h-dest_corner,dest_corner,dest_corner)),
       ((corner,0,sw-corner,corner),(dest_corner,0,w-2*dest_corner,dest_corner)),
       ((corner,sh-corner,sw-corner,sh),(dest_corner,h-dest_corner,w-2*dest_corner,dest_corner)),
       ((0,corner,corner,sh-corner),(0,dest_corner,dest_corner,h-2*dest_corner)),
       ((sw-corner,corner,sw,sh-corner),(w-dest_corner,dest_corner,dest_corner,h-2*dest_corner))
    )
    for box,(x,y,ww,hh) in chunks:
        piece=src.crop(box).resize((ww,hh),Image.Resampling.LANCZOS)
        out.alpha_composite(piece,(x,y))
    require(out.getpixel((w//2,h//2))[3]==0,
            "Original panel frame obscured the authentic central scene")
    return out


def crop_authentic_insignia(badge):
    """Expand only the visible pixels of the REAL owned icon. The Unity PNG
    importer often leaves large transparent canvas margins; directly
    thumbnailing those margins turns a 240px emblem into a tiny dot."""
    icon=badge.convert("RGBA")
    visible=icon.getchannel("A").point(lambda a:255 if a>=24 else 0).getbbox()
    require(visible is not None,"Empty official insignia")
    padding=max(2,round(max(icon.size)*.012))
    x0,y0,x1,y1=visible
    return icon.crop((max(0,x0-padding),max(0,y0-padding),
                      min(icon.width,x1+padding),min(icon.height,y1+padding)))


def compose(game_capture,layers,shape,event="",badge=None,canonical_lyra=None):
    """Canonical identity first; quarantine AI-invented architecture and maps.

    Source studios remain PRE-UNITY, never advertised as real gameplay.
    The old opaque stage covered authentic backgrounds with fictional scenery.
    """
    w,h=shape
    require(game_capture.width>=200 and game_capture.height>=200,
            "Genuine source too small for composition")
    dst=cover(game_capture.convert("RGBA"),w,h)
    dst.alpha_composite(Image.new("RGBA",(w,h),(18,20,25,40)))
    ornaments=[layer for role,layer in layers if role=="ornament"]
    require(len(ornaments)<=1,"Multiple ornate frames overlap")
    require(all(role in ("stage","ornament") for role,_ in layers),
            "Unknown source composition layer")
    # Explicitly fail closed when someone accidentally passes a stage without
    # a verified canonical scene. Overlay compositing is no proof of identity.
    require(event in ("","lyra","explorers","vigilants","bastion"),
            "Unknown game event must not receive a fabricated ceremony")
    # It is dangerous to silently accept a generated ornament after refusing
    # generated scenery: a candidate may still have baked loot, text or fake
    # heraldry at the margins. Until independently reviewed, its only allowed
    # use is PRE-UNITY reference. Final Unity UI must use separately admitted
    # canonical material sprites, not this source compositor.
    # Stage candidates contain fictional coasts, emblems and buildings.
    # Suppress them all rather than making a small retouch that preserves
    # these inaccuracies. Real environment occupies the complete frame.
    if event=="lyra":
        require(canonical_lyra is not None,
                "Lyra must come from authentic GLB, not an invented portrait")
        bounds=canonical_lyra.getchannel("A").getbbox()
        require(bounds is not None,"Empty real Lyra")
        # Real-image A/B at 1600x900 and 390x844 rejected the previous
        # lower-leg fade: it made Lyra float ghostlike above the hall stairs.
        # Keep the complete original portrait inside the frame and let its
        # lower legs extend naturally to the bottom clipping edge instead.
        # Source: genuine, SHA-checked canonical GLB render; no generated face.
        figure=canonical_lyra.crop(bounds)
        portrait=h>w
        target_h=round(h*(1.10 if portrait else 1.06))
        ww=max(1,round(figure.width*target_h/figure.height))
        hh=target_h
        figure=figure.resize((ww,hh),Image.Resampling.LANCZOS)
        from PIL import ImageEnhance
        figure=ImageEnhance.Brightness(figure).enhance(1.12)
        center_x=round(w*(.50 if portrait else .32))
        top=round(h*(.09 if portrait else .085))
        # Cropping at the viewport is intentional. Do not fade the actor into
        # translucent feet, sever limbs at the sill or invent 2D armor.
        dst.alpha_composite(figure,(center_x-ww//2,top))
    if badge is not None and event in ("explorers","vigilants"):
        side=max(26,round(min(w*(.60 if h>w else .36),
                              h*(.42 if h>w else .59),430)))
        icon=crop_authentic_insignia(badge)
        icon.thumbnail((side,side),Image.Resampling.LANCZOS)
        cx,cy=round(w*.5),round(h*(.43 if h>w else .50))
        dst.alpha_composite(icon,(cx-icon.width//2,cy-icon.height//2))
    if ornaments:
        dst.alpha_composite(nine_slice_frame(ornaments[0],shape))
    return dst


def verified_canonical_lyra(manifest):
    """Never treat an AI portrait as the exact source model hero."""
    root=manifest.parent
    path=root/"lyra-canonical-portrait-source.png"
    receipt=root/"lyra-canonical-portrait-source.provenance.json"
    require(path.is_file() and receipt.is_file(),
            "Cannot preview Lyra without the approved original 3D model render")
    data=json.loads(receipt.read_text(encoding="utf-8"))
    require(data.get("source_sha256")==
            "a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725"
            and data.get("source_length")==4770000
            and data.get("generated_person") is False
            and data.get("art_approved") is False,
            "Lyra portrait source identity is not the original model")
    require(data.get("output_sha256")==digest(path),
            "3D Lyra render modified after authentic provenance receipt")
    with Image.open(path) as im:
        require(im.mode=="RGBA" and im.width>=1024 and im.height>=512,
                "Canonical transparent 3D portrait unavailable")
        return im.copy(),data


def source_audit(manifest):
    path=Path(__file__).with_name("preflight_ceremony_art.py")
    require(path.is_file(),"Missing canonical preflight source")
    spec=importlib.util.spec_from_file_location("daily_art_preflight",path)
    module=importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    result=module.audit(manifest)
    require(result["result"]=="SOURCE_ART_PREFLIGHT_PASS_NOT_ART_APPROVAL",
            "Incomplete/invalid layer contract")
    return result

def write_contact_sheet(out):
    """Review four actual-source compositions together; NOT game captures."""
    tile_w,tile_h=960,540
    canvas=Image.new("RGB",(tile_w*2,tile_h*2),(13,15,20))
    paint=ImageDraw.Draw(canvas)
    for i,event in enumerate(EVENTS):
        path=out/f"{event}-1600x900-PREVIEW-ONLY.png"
        require(path.is_file(),"Missing 1600x900 event review: "+event)
        with Image.open(path) as im:
            tile=im.convert("RGB").resize((tile_w,tile_h),Image.Resampling.LANCZOS)
        x=(i%2)*tile_w
        y=(i//2)*tile_h
        canvas.paste(tile,(x,y))
        # The tag is ONLY on the review contact, never on the art source.
        paint.rectangle((x+7,y+7,x+450,y+39),fill=(9,12,16))
        paint.text((x+17,y+17),event.upper()+" / PRE-UNITY - NOT GAMEPLAY",
                   fill=(236,219,182))
    target=out/"four-ceremonies-CONTACT-PREUNITY-NOT-GAMEPLAY.jpg"
    canvas.save(target,format="JPEG",quality=91,subsampling=0)
    return {"path":target.name,"sha256":digest(target),
            "classification":"SOURCE_STUDY_NOT_UNITY_SCREENSHOT"}


def run(manifest,identities,out):
    require(set(identities)==set(EVENTS),"Provide ALL real Lyra/Scouts/Vigilants/Bastion source images")
    source_audit(manifest)
    data=json.loads(manifest.read_text(encoding="utf-8"))
    portrait,hero_receipt=verified_canonical_lyra(manifest)
    out.mkdir(parents=True,exist_ok=True)
    receipts={}
    for event in EVENTS:
        real=identities[event]
        require(real.is_file(),"Missing canonical image: "+str(real))
        if event in OFFICIAL_PAINTINGS:
            expected=Path(OFFICIAL_PAINTINGS[event]).resolve()
            require(real.resolve()==expected,
                    "Use actual checked-in Eldoria painting, not a concept: "+event)
            require(Path(str(expected)+".meta").is_file(),
                    "Missing genuine Unity importer: "+event)
        else:
            # Bastion requires an authentic Unity city screenshot and external
            # ledger review; a filename and hash cannot certify that alone.
            require(real.suffix.lower()==".png",
                    "Bastion requires city-source PNG")
        with Image.open(real) as src:
            base=src.convert("RGBA")
        layers=[]
        for layer in data["ceremonies"][event]["layers"]:
            with Image.open(manifest.parent/layer["path"]) as img:
                layers.append((layer["role"],img.convert("RGBA")))
        badge=None
        if event in OFFICIAL_SYMBOLS:
            icon_path=Path(OFFICIAL_SYMBOLS[event])
            require(icon_path.is_file() and Path(str(icon_path)+".meta").is_file(),
                    "Missing real official insignia: "+event)
            with Image.open(icon_path) as graphic:
                badge=graphic.convert("RGBA")
        shots=[]
        for size in SIZES:
            name=f"{event}-{size[0]}x{size[1]}-PREVIEW-ONLY.png"
            target=out/name
            compose(base,layers,size,event,badge,
                    portrait if event=="lyra" else None).save(target,optimize=True)
            shots.append({"path":name,"sha256":digest(target),"size":size})
        receipts[event]={"authentic_source_sha256":digest(real),
                         "authentic_lyra_portrait_sha256":
                           hero_receipt["output_sha256"] if event=="lyra" else None,
                         "official_identity_source":data["ceremonies"][event]["identity_source"],
                         "original_stage_drawn":False,
                         "art_safety_note":"All fictional stage candidates quarantined; authentic scene only",
                         "previews":shots}
    contact=write_contact_sheet(out)
    result={"classification":"PREUNITY_STUDY_NOT_GAMEPLAY",
            "art_approved":False,"unity_native_verified":False,
            "canonical_identity_required":True,"contact":contact,
            "scenes":receipts}
    (out/"preview-provenance.json").write_text(json.dumps(result,indent=2),encoding="utf-8")
    return result

def selftest():
    sample=Image.new("RGBA",(1024,576),(115,115,90,255))
    ImageDraw.Draw(sample).rectangle((180,140,840,480),fill=(170,125,82,255))
    layer=Image.new("RGBA",(1600,900))
    ImageDraw.Draw(layer).rectangle((20,20,180,760),fill=(185,115,44,210))
    for size in SIZES:
        img=compose(sample,[("stage",layer)],size)
        require(img.size==size,"Bad device layout")
        require(img.getpixel((size[0]//2,size[1]//2))[3]==255,
                "Canonical capture disappeared")
    # No fictional stage may alter real-world pixels, in ANY ceremony.
    fake=Image.new("RGBA",(1024,576),(235,2,5,255))
    for event in ("explorers","vigilants","bastion"):
        baseline=compose(sample,[],(1600,900),event=event)
        candidate=compose(sample,[("stage",fake)],(1600,900),event=event)
        require(baseline.tobytes()==candidate.tobytes(),
                "Fictional stage leaked into "+event)
    actor=Image.new("RGBA",(200,300),(9,242,27,255))
    portrait=compose(sample,[("stage",fake)],(1600,900),
                     event="lyra",canonical_lyra=actor)
    green=sum(1 for r,g,b,a in portrait.getdata()
              if g>=230 and r<=20 and b<=40 and a==255)
    require(green>500,"Canonical actor disappeared")
    require(portrait.getpixel((30,30))[0]!=235,
            "Fictional architecture visible beyond canonical actor")
    # Regression: published Eldoria icons may have broad transparent margins.
    # A 44px actual emblem must be expanded BEFORE measuring its target size.
    inset=Image.new("RGBA",(256,256),(0,0,0,0))
    ImageDraw.Draw(inset).ellipse((106,106,150,150),
                                  fill=(12,235,42,255))
    real=crop_authentic_insignia(inset)
    require(max(real.size)<64 and min(real.size)>=44,
            "Transparent icon padding was not removed")
    print("PASS: four sizes, actor-depth, canonical insignia alpha bounds; pre-Unity only")

if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("--manifest",type=Path)
    p.add_argument("--out",type=Path)
    p.add_argument("--identity",action="append",default=[],
                   help="lyra=/path/to/REAL_game_scene.png (and other three)")
    p.add_argument("--selftest",action="store_true")
    args=p.parse_args()
    if args.selftest:selftest()
    else:
        require(args.manifest and args.out,"Missing manifest or --out")
        entries={}
        for item in args.identity:
            event,sep,path=item.partition("=")
            require(sep and event in EVENTS and event not in entries,
                    "Invalid/duplicate official scene: "+item)
            entries[event]=Path(path)
        print(json.dumps(run(args.manifest,entries,args.out),indent=2))
