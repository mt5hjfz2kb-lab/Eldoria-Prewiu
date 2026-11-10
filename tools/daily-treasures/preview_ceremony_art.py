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

def require(flag,msg):
    if not flag:raise ValueError(msg)

def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def cover(im,w,h):
    k=max(w/im.width,h/im.height)
    nw,nh=round(im.width*k),round(im.height*k)
    im=im.resize((nw,nh),Image.Resampling.LANCZOS)
    return im.crop(((nw-w)//2,(nh-h)//2,(nw+w)//2,(nh+h)//2))

def compose(game_capture,layers,shape):
    w,h=shape
    require(game_capture.width>=200 and game_capture.height>=200,
            "Genuine source too small for composition")
    dst=cover(game_capture.convert("RGBA"),w,h)
    dst.alpha_composite(Image.new("RGBA",(w,h),(18,20,25,67)))
    for layer in layers:
        require(layer.mode=="RGBA","Art layer requires alpha, not a poster")
        k=min(w/layer.width,h/layer.height)
        tw,th=max(1,round(layer.width*k)),max(1,round(layer.height*k))
        ink=layer.resize((tw,th),Image.Resampling.LANCZOS)
        dst.alpha_composite(ink,((w-tw)//2,(h-th)//2))
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
        with Image.open(real) as src:
            base=src.convert("RGBA")
        layers=[]
        for layer in data["ceremonies"][event]["layers"]:
            with Image.open(manifest.parent/layer["path"]) as img:
                layers.append(img.convert("RGBA"))
        shots=[]
        for size in SIZES:
            name=f"{event}-{size[0]}x{size[1]}-PREVIEW-ONLY.png"
            target=out/name
            compose(base,layers,size).save(target,optimize=True)
            shots.append({"path":name,"sha256":digest(target),"size":size})
        receipts[event]={"authentic_source_sha256":digest(real),
                         "official_identity_source":data["ceremonies"][event]["identity_source"],
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
        img=compose(sample,[layer],size)
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
