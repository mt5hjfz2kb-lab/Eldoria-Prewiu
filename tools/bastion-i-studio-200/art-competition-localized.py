"""Second atelier technique: localized crop generation and deterministic material grading.
Reuses 12 paid assets; no re-generation of whole sprites. Preserve original architecture."""
import os,json,hashlib,base64,io,secrets,shutil
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw,ImageFilter,ImageChops,ImageEnhance
from openai import OpenAI
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);src=root/"previous";out=root/"competition";out.mkdir(exist_ok=True)
p=json.loads((src/"parameters.json").read_text());prior=D(p["estimated_total_usd"])
assert prior==D("2.7841178") and prior-D("1.9100013")+D(".14")+D(".15")+D(".40")<=D("2")
results=p["assets"];ledger=json.loads((src/"ledger.json").read_text());assert all(x["status"]=="complete" for x in ledger)
for n,v in results.items():
    assert hashlib.sha256((src/(n+"-final.png")).read_bytes()).hexdigest()==v["original_sha256"]
    assert v["selected"]=="original"
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def call_begin(slot,cap):
    cap=D(cap);assert sum(D(x["estimated_usd"]) for x in ledger)+cap+D(".40")<=D("2")
    row={"slot":slot,"reserved_usd":str(cap),"status":"in_flight"};ledger.append(row);dump("ledger.json",ledger);return row
def finish(row,raw,model):
    c=economics.estimate(raw.get("model") or model,raw["usage"],raw.get("service_tier") or "standard")
    row.update(status="complete",estimated_usd=str(c),usage=raw["usage"],model=raw.get("model") or model);dump("ledger.json",ledger)
    assert c<=D(row["reserved_usd"]);print("REPAIR_CALL",row["slot"],str(c),flush=True)
# Use cropped owned facade as input; local mask is applied after image generation.
n="AceraZapateros"
a=Image.open(src/(n+"-final.png")).convert("RGBA");assert a.size==(1157,1360)
crop=(385,875,760,1250)
ref=a.crop(crop);ref.save(out/"shoemaker-crop-input.png")
row=call_begin("shoemaker-local-crop",".14")
client=OpenAI(max_retries=0,timeout=180)
with (out/"shoemaker-crop-input.png").open("rb") as im:
    resp=client.images.edit(model="gpt-image-2.6-flare",image=im,prompt="A tiny local edit of this exact owned Valoria 2.5D facade crop, not a redesign. Retain camera and the position of every architectural element. In the existing display window below the hanging boot sign, make two pairs of leather boots and a wooden last visually distinct with readable silhouettes and warm muted leather; slightly strengthen the boot icon on the existing sign WITHOUT moving or enlarging its frame. Preserve the door, frame, stone, roof, plaster, thresholds and existing lighting. No text. Return same square crop, no new border or margin, no zoom or translation.",size="1024x1024",quality="high",background="transparent",output_format="png",n=1)
raw=resp.model_dump(exclude_none=True);finish(row,raw,"gpt-image-2.6-flare")
rawpath=out/"shoemaker-crop-raw.png";rawpath.write_bytes(base64.b64decode(raw["data"][0]["b64_json"],validate=True))
patch=Image.open(rawpath).convert("RGBA").resize(ref.size,Image.Resampling.LANCZOS)
proposal=a.copy();proposal.paste(patch,(crop[0],crop[1]))
# Original window's interior and sign's panel only (inspected on the exact source).
polys=[[(509,1094),(602,1049),(611,1167),(526,1210)],[(503,957),(550,948),(551,1010),(504,1022)]]
mask=Image.new("L",a.size,0);d=ImageDraw.Draw(mask)
for poly in polys:d.polygon(poly,fill=255)
blend=ImageChops.multiply(mask,mask.filter(ImageFilter.GaussianBlur(6)))
blend=ImageChops.multiply(blend,a.getchannel("A"));blend=ImageChops.multiply(blend,proposal.getchannel("A"))
new=Image.composite(proposal,a,blend);new.putalpha(a.getchannel("A"))
assert ImageChops.multiply(ImageChops.difference(a,new).convert("RGB").convert("L"),ImageChops.invert(mask)).getbbox() is None
new.save(out/(n+"-P.png"));mask.save(out/"shoemaker-authorized-mask.png")
# Armory R: isolated selective shield material treatment; no pixel changes outside mask.
n="ArmeriaDelPatio";a=Image.open(src/(n+"-R.png")).convert("RGBA");assert a.size==(509,406)
mask=Image.new("L",a.size,0);ImageDraw.Draw(mask).polygon([(145,275),(161,266),(177,276),(177,310),(163,333),(150,320),(143,296)],fill=255)
blend=ImageChops.multiply(mask,mask.filter(ImageFilter.GaussianBlur(3)))
# Compress the cool highlights and add slight tonal irregularity without changing geometry.
rgb=a.convert("RGB");grade=ImageEnhance.Brightness(ImageEnhance.Contrast(ImageEnhance.Color(rgb,.86).enhance(1.0),.86).enhance(1.0),.94).enhance(1.0) if False else None
# Direct per-pixel material correction, preserve matte base and edge detail.
g=rgb.copy();pix=g.load();original=rgb.load()
for y in range(260,337):
    for x in range(140,181):
        r,gc,b=original[x,y];l=(r*54+gc*183+b*19)/256
        if l>100:
            # Attenuate unusually bright cool highlight; keep wood excluded by the polygon.
            t=min(.19,max(0,(l-100)/155*.19));v=(r*(1-t),gc*(1-t),b*(1-t))
            pix[x,y]=tuple(round(x) for x in v)
new=Image.composite(g.convert("RGBA"),a,blend);new.putalpha(a.getchannel("A"));new.save(out/(n+"-P.png"))
# Quarry R: selective warm-cast neutralization, preserve luminance and texture.
n="CorteCaliza";a=Image.open(src/(n+"-R.png")).convert("RGBA");assert a.size==(640,213)
rgb=a.convert("RGB");g=rgb.copy();px=g.load();original=rgb.load()
for y in range(a.height):
    for x in range(a.width):
        r,gc,b=original[x,y];lum=.2126*r+.7152*gc+.0722*b
        warmth=max(0,(r-b)-13);amount=min(.38,warmth/100*.48)*min(1,lum/100)
        # Keep green moss and shade; neutralize only golden limestone.
        if r>=gc and gc>=b and r-gc<55:
            px[x,y]=tuple(round(v*(1-amount)+lum*amount) for v in (r,gc,b))
g.putalpha(a.getchannel("A"));g.save(out/(n+"-P.png"))
# Actual pixel proof and standardized visual review.
proof={}
for n,v in results.items():
    a=Image.open(src/(n+("-final.png" if n=="AceraZapateros" else "-R.png"))).convert("RGBA")
    b=Image.open(out/(n+"-P.png")).convert("RGBA")
    assert a.size==b.size and a.getchannel("A").tobytes()==b.getchannel("A").tobytes()
    aa,bb=a.tobytes(),b.tobytes();changed=sum(aa[i:i+4]!=bb[i:i+4] for i in range(0,len(aa),4))
    proof[n]={"changed_pixels":changed,"total_pixels":a.width*a.height,"alpha_identical":True,"sha256":sha(out/(n+"-P.png"))}
    assert changed>0
dump("protected-pixel-proof.json",proof)
def sheet(n,labels,paths,name):
    with Image.open(paths[0]) as im:W,H=im.size
    scale=min(400/W,500/H);q=Image.new("RGB",(420*len(paths),690),(47,51,48));d=ImageDraw.Draw(q)
    for i,(label,path) in enumerate(zip(labels,paths)):
        with Image.open(path) as im:
            im=im.convert("RGBA");r=im.resize((round(W*scale),round(H*scale)),Image.Resampling.LANCZOS)
            q.paste(r,(i*420+(420-r.width)//2,35+(500-r.height)//2),r.getchannel("A"))
            r=im.copy();r.thumbnail((90,90));q.paste(r,(i*420+(420-r.width)//2,570),r.getchannel("A"))
        d.text((i*420+10,10),label,fill="white")
    p=out/name;q.save(p,quality=94);return p
mapping={};imgs=[]
for n in results:
    paths={"original":src/(n+"-final.png"),"full":src/(n+"-R.png"),"protected":out/(n+"-P.png")}
    order=list(paths);secrets.SystemRandom().shuffle(order);mapping[n]=dict(zip("XYZ",order))
    imgs.append(sheet(n,list("XYZ"),[paths[k] for k in order],n+"-repair-blind.jpg"))
    sheet(n,["Original","Full revision","Localized correction"],list(paths.values()),n+"-repair-comparison.jpg")
dump("repair-mapping.json",mapping)
row=call_begin("localized-independent-review",".15")
prompt="""Independent game art instructor. Three blind sprite triplets X/Y/Z, each at same scale with small thumbnail: first shoemakers' street, second armory, third limestone quarry face. Each contains original, full revision and localized repair in random order. Score each 0..10 for craft quality, visual identity, silhouette, perspective, material distinction, light and mobile legibility. Distinguish MATERIAL regressions (loss of recognizable function, structure, perspective, silhouette, major palette or serious seams) from minor craft imperfections. Do not treat a small highlight as a veto to testing; prefer original when no candidate improves. State concrete visual differences, no claims of native integration. JSON {AceraZapateros:{scores:{X:number,Y:number,Z:number},preferred:'X'|'Y'|'Z'|'tie',material_regressions:{X:[strings],Y:[strings],Z:[strings]},minor_issues:{X:[strings],Y:[strings],Z:[strings]},evidence:string},ArmeriaDelPatio:{same},CorteCaliza:{same}}. Spanish."""
content=[{"type":"input_text","text":prompt}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(f.read_bytes()).decode(),"detail":"high"} for f in imgs]
r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":3000,"store":False},os.environ["OPENAI_API_KEY"])
finish(row,r,"gpt-6.2-sol");assert r["status"]=="completed"
s="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
j=json.loads(s);assert set(j)==set(results);dump("localized-review.json",j)
for n,v in j.items():
    assert set(v["scores"])==set("XYZ") and all(type(x) in (int,float) and 0<=x<=10 for x in v["scores"].values())
    assert set(v["material_regressions"])==set("XYZ") and v["preferred"] in ("X","Y","Z","tie")
    scores={mapping[n][k]:a for k,a in v["scores"].items()};material={mapping[n][k]:a for k,a in v["material_regressions"].items()}
    preferred=mapping[n].get(v["preferred"],"tie")
    # Diagnostic native trial: best valid new sprite even if gain is not yet enough.
    eligible=[k for k in ("full","protected") if not material[k]]
    selected=max(eligible,key=lambda k:scores[k]) if eligible else "original"
    # The facade full revision alters roof silhouette: never use it in this trial.
    if n=="AceraZapateros" and selected=="full":selected="protected" if "protected" in eligible else "original"
    path=src/(n+"-final.png") if selected=="original" else src/(n+"-R.png") if selected=="full" else out/(n+"-P.png")
    shutil.copyfile(path,out/(n+"-final.png"))
    results[n]["selected"]=selected;results[n]["final_sha256"]=sha(out/(n+"-final.png"));results[n]["final"]={"scores":scores,"judge_preferred":preferred,"selected":selected,"material_regressions":material,"evidence":v["evidence"]}
    print("LOCALIZED_RESULT",n,selected,json.dumps(scores),flush=True)
total=D("1.9100013")+sum(D(x["estimated_usd"]) for x in ledger)
assert total-D("1.9100013")+D(".40")<=D("2")
p.update(assets=results,estimated_total_usd=str(total),estimated_new_usd=str(total-D("1.9100013")),localized_repair=True,promoted=False)
dump("parameters.json",p);dump("asset-results.json",results)
print("LOCALIZED_PRODUCTION_FINAL",json.dumps({n:{"selected":v["selected"],"final":v["final"]} for n,v in results.items()},ensure_ascii=False),str(total),flush=True)
