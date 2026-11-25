"""Select explicit authentic phone frames from the frozen evidence archive; never execute external evidence."""
import hashlib,json,pathlib,sys
from PIL import Image
from mission import SOURCE_SHA,SOURCE_RUN
root=pathlib.Path(sys.argv[1]); out=pathlib.Path(sys.argv[2]); out.mkdir(parents=True,exist_ok=True)
images=[]
for source,dest in [("bastion-1.png","reference.jpg"),("bastion-5-completo.png","city.jpg")]:
    hits=list(root.rglob(source))
    if len(hits)!=1: raise SystemExit("Expected exactly one "+source+"; available PNG names: "+str(sorted(set(p.name for p in root.rglob("*.png")))[:80]))
    raw=hits[0].read_bytes()
    with Image.open(hits[0]) as img:
        img=img.convert("RGB"); dims=list(img.size)
        img.thumbnail((1024,1024),Image.Resampling.LANCZOS)
        img.save(out/dest,quality=90)
    images.append({"name":source,"source_sha256":hashlib.sha256(raw).hexdigest(),"source_dimensions":dims,"derived_name":dest,"sha256":hashlib.sha256((out/dest).read_bytes()).hexdigest()})
(out/"manifest.json").write_text(json.dumps({"run_id":SOURCE_RUN,"source_sha":SOURCE_SHA,"artifact":"ciudad-region-recorrido","images":images},indent=2))
print("AUTHENTIC_PHONE_EVIDENCE",[(x["name"],x["source_dimensions"]) for x in images])
