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


def compose(game_capture,layers,shape,event="",badge=None):
    w,h=shape
    require(game_capture.width>=200 and game_capture.height>=200,
            "Genuine source too small for composition")
    dst=cover(game_capture.convert("RGBA"),w,h)
    dst.alpha_composite(Image.new("RGBA",(w,h),(18,20,25,55)))
    ornament=None
    for role,layer in layers:
        require(layer.mode=="RGBA","Art layer requires alpha, not a poster")
        if role=="ornament":
            ornament=layer
            continue
        require(role=="stage","Unknown source composition layer")
        if event=="bastion":
            # The generated stone gate is NOT the user's actual building.
            # Never mistake it for the real Bastion. Original city is hero.
            continue
        focal,x,y=fit_focal(layer,shape,event)
        dst.alpha_composite(focal,(x,y))
        if badge is not None and event=="vigilants":
            icon=badge.convert("RGBA")
            side=max(26,min(round(focal.width*.22),round(focal.height*.22),200))
            icon.thumbnail((side,side),Image.Resampling.LANCZOS)
            dst.alpha_composite(icon,
                 (x+(focal.width-icon.width)//2,
                  y+round(focal.height*.41)-icon.height//2))
        if badge is not None and event=="explorers":
            # Authentic Scouts sigil only; no fabricated cartography.
            icon=badge.convert("RGBA")
            side=max(24,min(round(focal.width*.15),110))
            icon.thumbnail((side,side),Image.Resampling.LANCZOS)
            dst.alpha_composite(icon,
                 (x+round(focal.width*.70),y+round(focal.height*.30)))
    if ornament is not None:
        dst.alpha_composite(nine_slice_frame(ornament,shape))
    return dst


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

def run(manifest,identities,out):
    require(set(identities)==set(EVENTS),"Provide ALL real Lyra/Scouts/Vigilants/Bastion source images")
    source_audit(manifest)
    data=json.loads(manifest.read_text(encoding="utf-8"))
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
            compose(base,layers,size,event,badge).save(target,optimize=True)
            shots.append({"path":name,"sha256":digest(target),"size":size})
        receipts[event]={"authentic_source_sha256":digest(real),
                         "official_identity_source":data["ceremonies"][event]["identity_source"],
                         "original_stage_drawn":event!="bastion",
                         "art_safety_note":("Generated gate omitted: not real game architecture"
                                            if event=="bastion" else "Stage is unapproved compositional candidate"),
                         "previews":shots}
    result={"classification":"PREUNITY_STUDY_NOT_GAMEPLAY",
            "art_approved":False,"unity_native_verified":False,
            "canonical_identity_required":True,"scenes":receipts}
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
    print("PASS: four sizes retain real source; pre-Unity only")

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
