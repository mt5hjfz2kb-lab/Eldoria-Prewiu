import json,sys,hashlib
from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
root,out=map(Path,sys.argv[1:]);out.mkdir(exist_ok=False)
read=lambda name:json.loads((root/name).read_text(encoding="utf-8-sig"))
m=read("native-evidence.json");contract=read("postigo-contract.json");cleanup=read("cleanup.json")
assert m["sourceFrames"]==len(m["frames"])==45 and m["engineVersion"]=="6000.3.23f1" and m["nativeWorldRender"]
assert m["sourceSha"]=="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
assert all(contract[k] is True for k in ("road_centerlines_preserved","original_material_preserved","eight_exact_reversals","b1_boroughs_hidden"))
for f in m["frames"]:assert hashlib.sha256((root/f["file"]).read_bytes()).hexdigest()==f["sha256"]
rows=[]
for view in ("postigo","close","calle",*(f"travel-{i}" for i in range(5))):
    a=Image.open(root/f"A-{view}.png").convert("RGB")
    assert (root/f"A-{view}.png").read_bytes()==(root/f"R-{view}.png").read_bytes()
    for v in ("B","C","S"):
        b=Image.open(root/f"{v}-{view}.png").convert("RGB");assert a.size==b.size
        d=ImageChops.difference(a,b);mask=d.convert("L").point(lambda x:255 if x>2 else 0)
        rows.append({"view":view,"variant":v,"changed_fraction":mask.histogram()[255]/(a.width*a.height),"bounds":d.getbbox()})
    if view in ("postigo","close","calle"):
        # Keep full native evidence; create unscaled pair for visual inspection.
        c=Image.open(root/f"C-{view}.png").convert("RGB")
        pair=Image.new("RGB",(a.width*2,a.height+36),(22,22,22));pair.paste(a,(0,36));pair.paste(c,(a.width,36))
        draw=ImageDraw.Draw(pair);draw.text((12,10),"Original A",fill="white");draw.text((a.width+12,10),"Candidate C",fill="white")
        pair.save(out/f"{view}-AC.png")
        if view=="close":
            grid=Image.new("RGB",(a.width*2,a.height*2+72),(22,22,22))
            for i,v in enumerate(("A","B","C","S")):
                x=(i%2)*a.width;y=(i//2)*(a.height+36)
                grid.paste(Image.open(root/f"{v}-{view}.png").convert("RGB"),(x,y+36));ImageDraw.Draw(grid).text((x+12,y+10),{"A":"Original","B":"Join only","C":"Join + frontage","S":"Shadow disabled (diagnostic only)"}[v],fill="white")
            grid.save(out/"variants.png")
result={"source_run":cleanup["run_id"],"frames":45,"exact_reversals":8,"native_pass":cleanup["native_pass"],"scratch_removed":cleanup["scratch_removed"],"contract":contract,"rows":rows,"art_approved":False}
(out/"postigo-pixel-proof.json").write_text(json.dumps(result,indent=2))
print("POSTIGO_NATIVE_PIXEL_PASS",json.dumps(result))
