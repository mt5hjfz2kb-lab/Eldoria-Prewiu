#!/usr/bin/env python3
"""Consolidate real render images; label when a model failed."""
import json,math
from pathlib import Path
from PIL import Image,ImageDraw
out=Path("audit-output")
assets=json.loads((out/"audit-result.json").read_text())["assets"]
for mode in ["pbr","clay"]:
    cellw,cellh=312,325
    for start in range(0,len(assets),18):
        part=assets[start:start+18]
        page=Image.new("RGB",(cellw*3,cellh*6),(233,234,235))
        draw=ImageDraw.Draw(page)
        for k,a in enumerate(part):
            x=(k%3)*cellw;y=(k//3)*cellh
            draw.text((x+8,y+5),a["id"][:40],fill=(20,20,20))
            pic=out/"renders"/a.get(mode,"nonexistent")
            if pic.is_file():
                im=Image.open(pic).convert("RGB")
                im.thumbnail((300,285),Image.Resampling.LANCZOS)
                page.paste(im,(x+(cellw-im.width)//2,y+26))
            else: draw.text((x+20,y+60),"NO RENDER / FAILED",fill=(150,0,0))
        page.save(out/("contact_"+mode+"_"+str(start//18+1)+".jpg"),quality=86)
