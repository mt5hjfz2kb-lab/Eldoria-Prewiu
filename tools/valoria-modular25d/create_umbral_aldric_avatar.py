#!/usr/bin/env python3
"""Reproducible *original Eldoria* Sir Aldric avatar for the isolated Umbral II HUD.

The artist-approved source bust remains owned by Eldoria/main.
This derivative only crops, masks and frames the ORIGINAL pixels. No
third-party portrait, no new hero and no fabricated character/lore.
"""
import base64,hashlib,io,json,os,urllib.request
from pathlib import Path
from PIL import Image,ImageDraw,ImageEnhance,ImageFilter

ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/UmbralII"
PROOF=ROOT/"docs/valoria-modular-25d/UMBRAL_II_ALDRIC_PORTRAIT_SOURCE.json"
REPO=os.environ["GITHUB_REPOSITORY"]
SOURCE="Unity/Assets/Eldoria/Resources/UI/AldricDialogue/aldric_neutral.png"
ORIGINAL_SHA="156791ca04f17c45fdc85662a134352902f76229337d02e6210f6dea10c1c685"
GUID=hashlib.md5(b"Eldoria-UMBRAL-II-owned-Sir-Aldric-headshot-20261120").hexdigest()

def fetch():
    url="https://api.github.com/repos/"+REPO+"/contents/"+SOURCE+"?ref=main"
    req=urllib.request.Request(url,headers={
      "Authorization":"Bearer "+os.environ["GH_TOKEN"],
      "Accept":"application/vnd.github+json"})
    with urllib.request.urlopen(req,timeout=40) as f:data=json.load(f)
    assert data.get("encoding")=="base64",data.get("encoding")
    original=base64.b64decode(data["content"])
    assert hashlib.sha256(original).hexdigest()==ORIGINAL_SHA,"Aldric source changed"
    return original

def main():
    original=fetch()
    src=Image.open(io.BytesIO(original)).convert("RGBA")
    assert src.size==(512,512),src.size
    # Preserve a temporary visual inspection contact sheet in the job
    # artifact; these files are never staged into the experiment branch.
    inspection=ROOT/"tools/valoria-modular25d/inspection_aldr ic".replace(" ","")
    inspection.mkdir(parents=True,exist_ok=True)
    src.save(inspection/"aldric_original_source.png",optimize=True)
    candidates=[
        (138,16,374,252),(130,95,374,339),(128,140,384,396),
        (128,170,384,426),(100,130,410,440),(100,200,412,512),
        (0,0,512,512),(160,210,400,450)
    ]
    montage=Image.new("RGB",(4*160,2*184),(22,36,45))
    drawer=ImageDraw.Draw(montage)
    for i,box in enumerate(candidates):
        thumb=src.crop(box).resize((144,144),Image.Resampling.LANCZOS)
        dest=((i%4)*160+8,(i//4)*184+8)
        montage.paste(thumb,dest,thumb)
        drawer.text((dest[0],dest[1]+150),str(i)+" "+str(box[1]),fill=(235,219,185))
    montage.save(inspection/"aldric_crop_candidates.jpg",quality=92)
    # The 512px original is APPROVED, but its historical crop box belongs
    # to an earlier upstream portrait. Authentic source inspection found the
    # face at y=130..440; crop 100..410 frames hair, eyes and shoulders.
    # Not a stylized imitation or a WOS portrait.
    crop=src.crop((100,130,410,440))
    crop=crop.resize((112,112),Image.Resampling.LANCZOS)
    # Slight warm midnight background is visible only where source was
    # transparent, providing contrast against the brass medallion.
    bg=Image.new("RGBA",(128,128),(20,42,52,0))
    d=ImageDraw.Draw(bg)
    d.ellipse((5,5,123,123),fill=(26,57,69,255))
    d.ellipse((9,9,119,119),outline=(194,161,99,255),width=2)
    base=Image.new("RGBA",(128,128),(0,0,0,0))
    base.alpha_composite(bg)
    # Important: use source RGBA hair/face, not a re-drawn face.
    layer=Image.new("RGBA",(128,128),(0,0,0,0))
    layer.alpha_composite(crop,(8,8))
    mask=Image.new("L",(128,128),0)
    ImageDraw.Draw(mask).ellipse((11,11,117,117),fill=255)
    mask=mask.filter(ImageFilter.GaussianBlur(.45))
    combined=Image.new("RGBA",(128,128))
    combined.paste(layer,(0,0),mask)
    base=Image.alpha_composite(base,combined)
    d=ImageDraw.Draw(base)
    d.arc((6,6,122,122),175,350,fill=(237,210,142,230),width=2)
    DEST.mkdir(parents=True,exist_ok=True)
    out=DEST/"aldric_avatar.png"
    base.save(out,optimize=True)
    with Image.open(out) as read:
        read.load()
        assert read.mode=="RGBA" and read.size==(128,128)
        assert read.getpixel((0,0))[3]==0
    # Derive importer from an existing project-owned sprite: preserve
    # Unity texture type, no mip maps, and give the NEW derivative its own GUID.
    template=(DEST/"dock_medallion.png.meta").read_text(encoding="utf8")
    import re
    meta=re.sub(r"(?m)^guid: [0-9a-f]{32}$","guid: "+GUID,template)
    meta=re.sub(r"(?m)^spriteBorder: .+$",
                "spriteBorder: {x: 0, y: 0, z: 0, w: 0}",meta)
    (DEST/"aldric_avatar.png.meta").write_text(meta,encoding="utf8")
    data=out.read_bytes()
    PROOF.parent.mkdir(parents=True,exist_ok=True)
    PROOF.write_text(json.dumps({
      "schema":1,"identity":"Sir Aldric",
      "source_path":SOURCE,"source_sha256":ORIGINAL_SHA,
      "source_provenance":"ALDRIC_BUST_PROVENANCE.json / already-approved Aldric head and shoulders",
      "original_cropped_pixels_preserved":True,
      "crop_box_in_512":[100,130,410,440],
      "output_path":out.relative_to(ROOT).as_posix(),
      "output_sha256":hashlib.sha256(data).hexdigest(),
      "size_bytes":len(data),"size":[128,128],
      "unity_guid":GUID,"not_gameplay":True,"visual_approval":False,
      "physical_iphone_tested":False
    },ensure_ascii=False,indent=2)+"\n",encoding="utf8")
    print("UMBRAL_II_APPROVED_ALDRIC_SOURCE_TO_MEDALLION_PASS",
          ORIGINAL_SHA,hashlib.sha256(data).hexdigest())

if __name__=="__main__":main()
