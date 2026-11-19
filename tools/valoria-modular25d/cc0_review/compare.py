#!/usr/bin/env python3
"""A/B donor research: never inject source art into Eldoria until visual review.

Sources: FeudalWars (2015), OpenGameArt, CC0:
- https://opengameart.org/content/western-european-castle-isometric-25d
- https://opengameart.org/content/medieval-blacksmith-isometric-25d

No copying from commercial reference games. This source A/B is intentionally
NOT the gameplay render, nor an original Eldoria sprite or approved production.
"""
from pathlib import Path
from urllib.request import Request,urlopen
from PIL import Image,ImageDraw,ImageEnhance
import hashlib,json,io

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/"tools/valoria-modular25d/cc0_review/evidence"
OUT.mkdir(parents=True,exist_ok=True)
ROOTGAME=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D"
urls={
 "castle":"https://opengameart.org/sites/default/files/castle_7.png",
 "blacksmith":"https://opengameart.org/sites/default/files/blacksmith_0.png",
}
sources={}
for name,url in urls.items():
    req=Request(url,headers={"User-Agent":"Mozilla/5.0 (compatible; Eldoria-cc0-art-review/1.0)"})
    with urlopen(req,timeout=30) as response:
        raw=response.read(3*1024*1024)
    im=Image.open(io.BytesIO(raw)).convert("RGBA")
    if im.width<100 or im.height<100:raise ValueError("Not expected CC0 source dimensions "+name)
    sourcehash=hashlib.sha256(raw).hexdigest()
    sources[name]={"url":url,"sha256":sourcehash,"dimensions":im.size,"license":"CC0-1.0"}
    im.thumbnail((400,370),Image.Resampling.LANCZOS)
    im.save(OUT/(name+"_donor_cc0.png"))
    print("CC0_DOWNLOADED",name,sourcehash,im.size)

sheet=Image.new("RGBA",(1200,550),(40,46,53,255))
draw=ImageDraw.Draw(sheet)
pairs=[("bastion_i","castle"),("sawmill_i","blacksmith")]
for index,(ours,donor) in enumerate(pairs):
    x=index*600
    src=Image.open(ROOTGAME/(ours+".png")).convert("RGBA")
    src.thumbnail((230,400),Image.Resampling.LANCZOS)
    other=Image.open(OUT/(donor+"_donor_cc0.png")).convert("RGBA")
    sheet.alpha_composite(src,(x+30+(250-src.width)//2,70+(370-src.height)))
    sheet.alpha_composite(other,(x+285+(250-other.width)//2,70+(370-other.height)))
    draw.text((x+26,26),ours+" - current procedural Blender",fill=(235,235,222,255))
    draw.text((x+290,26),donor+" - original CC0 donor, not Eldoria art",fill=(235,235,222,255))
draw.text((40,495),"AUTHORING STUDY ONLY | NOT UNITY | NO ART APPROVAL OR GAMEPLAY PROOF",fill=(232,195,140,255))
sheet.convert("RGB").save(OUT/"authored-vs-cc0-source-review.png")
(OUT/"source-license-and-sha.json").write_text(json.dumps(sources,indent=2))
