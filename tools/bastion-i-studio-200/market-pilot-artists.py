"""Issue200 market district pilot: art direction, two original modular RGBA sprites, fit and pre-native review.
Uses immutable actual Unity evidence + original own sprites as references. No city plate.
"""
import os,json,base64,hashlib,re
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
from openai import OpenAI
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);out=root/"market-production";out.mkdir(exist_ok=True)
source=root/"market-source";previous=root/"previous";base="f27f4774e32efc5ad046478a07747eead4256c3e"
old=json.loads((previous/"summary.json").read_text());prior=D(old["total_estimated_usd"])
assert prior==D("1.2760408")
CAP=D("5");final_reserve=D(".45")
names=("CasasDelPeso","PuestoDeAbastos")
ledger=[]
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def reserve(name,cap):
    cap=D(cap)
    assert prior+sum(D(r["estimated_usd"]) for r in ledger)+cap+final_reserve<=CAP
    row={"slot":name,"reserved_usd":str(cap),"status":"in_flight"};ledger.append(row);dump("ledger.json",ledger);return row
def finish(row,raw,model):
    usage=raw["usage"];c=economics.estimate(raw.get("model") or model,usage,raw.get("service_tier") or "standard")
    row.update(status="complete",estimated_usd=str(c),usage=usage,model=raw.get("model") or model)
    dump("ledger.json",ledger);assert c<=D(row["reserved_usd"])
    print("ARTIST_COMPLETE",row["slot"],str(c),flush=True)
def ask(slot,prompt,images,cap=".35",tokens=2300):
    row=reserve(slot,cap);content=[{"type":"input_text","text":prompt}]
    for p in images:
        from io import BytesIO
        with Image.open(p) as im:
            im=im.convert("RGB");im.thumbnail((1600,1800));b=BytesIO();im.save(b,format="JPEG",quality=94)
        content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(b.getvalue()).decode(),"detail":"high"})
    raw=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":tokens,"store":False},os.environ["OPENAI_API_KEY"])
    finish(row,raw,"gpt-6.2-sol");assert raw["status"]=="completed"
    t="\n".join(c["text"] for i in raw["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    if t.startswith("```"):t=t.split("\n",1)[1].rsplit("```",1)[0]
    j=json.loads(t);dump(slot+".json",j);return j
# Isolate the ORIGINAL left column of the matched stage V comparison. It is
# reference only, never composited into game textures.
for n in ("stage-5-urban-details","stage-5-comparison","stage-5-contact-details"):
    with Image.open(previous/(n+".jpg")) as im:
        im.crop((0,0,im.width//2,im.height)).save(out/(n+"-original.jpg"),quality=94)
donors=[]
for n in names:
    p=source/(n+".png")
    with Image.open(p) as im:
        im=im.convert("RGBA");bg=Image.new("RGB",im.size,(61,65,63));bg.paste(im,(0,0),im.getchannel("A"));bg.thumbnail((1200,1000));q=out/(n+"-donor.jpg");bg.save(q,quality=94);donors.append(q)
brief="""Senior art director for Valoria 2.5D mobile strategy. The attached first three images are original actual Unity stage V crops; last two are the owned original independent weighing house and market stall sprites, respectively. Two earlier attempts had ZERO or negative overall gain; simple moves caused the weighing-house roof to obscure the well. New approach: craft two premium original sprites as a COHERENT working market district around the unchanged granary; not a scene plate, not a new gameplay building. Use reference qualities of Whiteout Survival (clear mobile silhouettes, visual hierarchy) and Call of Dragons (rich stylized architecture and believable working districts); no copied design. The large functional granary, castle, C sawmill, well, all other props, routes and reserves stay. Weighing house currently at MarketRow=(3.37,-3.52), well=(1.58,-1.92), granary=(3.95,-5.65), stall at granary+(.73,-2.12). X increases right and Y increases upward/back, positive Y increases depth. Prior rejected Peso (-.52,+.28) approached well, so do not move left/up. Solve the occlusion mainly with a compact lower, open-front weighing-house silhouette and a legible adjacent stall with coherent materials. Return only JSON: {target:string,peso_brief:string,stall_brief:string,peso_width:number,stall_width:number,peso_dx:number,peso_dy:number,stall_dx:number,stall_dy:number,court_strength:number,validation:[strings]}. Bounds peso_width .72-.92, stall_width .90-1.0, peso_dx 0-.30, peso_dy -.12-.12, stall_dx -.16-.24, stall_dy -.15-.15, court_strength .70-.95. Width is occupied width fraction, not stretching. Specify exact perspective, palette, timber/stone/roof, distinct silhouette, threshold and readable activity WITHOUT people baked into assets, no cast ground shadow or broad slab. No imaginary claims. Describe actual design in Spanish; image prompts may be English."""
director=ask("art-director",brief,[out/(n+"-original.jpg") for n in ("stage-5-urban-details","stage-5-comparison","stage-5-contact-details")]+donors)
bounds={"peso_width":(.72,.92),"stall_width":(.9,1),"peso_dx":(0,.30),"peso_dy":(-.12,.12),"stall_dx":(-.16,.24),"stall_dy":(-.15,.15),"court_strength":(.70,.95)}
for k,(lo,hi) in bounds.items():assert type(director[k]) in (int,float) and lo<=director[k]<=hi
client=OpenAI(max_retries=0,timeout=180)
def box(im):
    a=im.getchannel("A");b=a.point(lambda x:255 if x>128 else 0).getbbox()
    assert b and a.getextrema()[0]==0 and a.getextrema()[1]>=240
    return b
def image_asset(n,revision=1,crit=""):
    row=reserve(n+"-image-"+str(revision),".80")
    ref=source/(n+".png")
    prompt="Re-author a NEW premium independent 2.5D production sprite for our own game Valoria using the attached OWNED original sprite for orientation and role only. "+director["target"]+" "+director["peso_brief" if n==names[0] else "stall_brief"]+" "+crit+" Fixed orthographic parallel projection, same world viewing direction as donor, warm upper-left light, finely hand-painted stylized realistic stone and timber, clear mobile silhouettes, legible open front doors and weighing/market purpose, restrained rich accents. Transparent background, complete visible footprint, safe margins, no flat ground plane, no broad painted base, no cast shadow, no characters, no text, no watermark, no other separate buildings. Render as a finished game asset, not concept art."
    with ref.open("rb") as im:
        rsp=client.images.edit(model="gpt-image-2.6-flare",image=im,prompt=prompt,size="1024x1024",quality="high",background="transparent",output_format="png",n=1)
    raw=rsp.model_dump(exclude_none=True);finish(row,raw,"gpt-image-2.6-flare")
    assert len(raw["data"])==1
    orig=out/(n+"-raw-"+str(revision)+".png");orig.write_bytes(base64.b64decode(raw["data"][0]["b64_json"],validate=True))
    with Image.open(orig) as fresh, Image.open(ref) as donor:
        assert fresh.mode=="RGBA"
        before=donor.convert("RGBA");b=box(fresh);ob=box(before)
        assert b[0]>2 and b[1]>2 and b[2]<fresh.width-2 and b[3]<fresh.height-2
        subject=fresh.crop(b);W,H=before.size;l,t,r,bot=ob
        factor=director["peso_width" if n==names[0] else "stall_width"]
        scale=min((r-l)*factor/subject.width,(bot-2)/subject.height,(bot-t)*1.04/subject.height)
        w,h=max(1,round(subject.width*scale)),max(1,round(subject.height*scale))
        x=round((l+r-w)/2);y=bot-h
        assert x>=0 and y>=0 and x+w<=W and y+h<=H
        fitted=Image.new("RGBA",(W,H));fitted.paste(subject.resize((w,h),Image.Resampling.LANCZOS),(x,y))
        dest=out/(n+".png");fitted.save(dest)
    meta=(source/(n+".png.meta")).read_text()
    ppu=float(re.search(r"spritePixelsToUnits:\s*([\d.]+)",meta).group(1))
    pivot=re.search(r"spritePivot:\s*\{x:\s*([\d.]+),\s*y:\s*([\d.]+)\}",meta)
    return {"name":n,"raw_sha256":sha(orig),"donor_sha256":sha(ref),"fitted_sha256":sha(dest),"canvas":[W,H],"original_bbox":ob,"candidate_bbox":box(fitted),"ppu":ppu,"pivot":[float(pivot.group(1)),float(pivot.group(2))],"width_factor":factor,"native_alpha":True}
def sheet():
    im=Image.new("RGB",(1300,1000),(47,50,47));d=ImageDraw.Draw(im)
    for i,n in enumerate(names):
        for j,p in enumerate((source/(n+".png"),out/(n+".png"))):
            with Image.open(p) as q:
                q=q.convert("RGBA");q.thumbnail((630,470));im.paste(q,(i*650+(650-q.width)//2,j*500+28+(470-q.height)//2),q.getchannel("A"))
            d.text((i*650+8,j*500+8),n+" | "+("ORIGINAL" if j==0 else "NEW"),fill="white")
    p=out/"asset-comparison.jpg";im.save(p,quality=94);return p
manifest={n:image_asset(n) for n in names}
gate_prompt="Independent senior sprite art reviewer. Top original sprites, bottom newly authored independent sprites, each column same building. Real scene reference is attached separately. Decide whether the two candidates merit native integration, NOT whether the game improved. Check silhouette, orientation, palette, quality, coherent market activity, safe alpha and readable entrances; no broad ground patches. A compact weighing house is intentional to reveal the granary and well. Return JSON {merits_native:boolean,defects:[strings],correction_asset:string,correction_prompt:string,reason:string}; correction_asset must be CasasDelPeso or PuestoDeAbastos or empty. Do not reject only because scene is not integrated yet."
gate=ask("sprite-review",gate_prompt,[sheet(),out/"stage-5-urban-details-original.jpg"],".30",1500)
if gate["merits_native"] is not True:
    n=gate["correction_asset"]
    if n in names and prior+sum(D(x["estimated_usd"]) for x in ledger)+D(".80")+D(".30")+final_reserve<=CAP:
        manifest[n]=image_asset(n,2,gate["correction_prompt"])
        gate=ask("sprite-review-after-correction",gate_prompt,[sheet(),out/"stage-5-urban-details-original.jpg"],".30",1500)
dump("asset-manifest.json",manifest)
total=prior+sum(D(x["estimated_usd"]) for x in ledger)
assert total+final_reserve<=CAP
dump("parameters.json",{"source_head":base,"values":director,"estimated_prior_usd":str(prior),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"billing_verified":False,"sprite_gate":gate,"native_unity":False})
print("MARKET_PRODUCTION_COMPLETE",json.dumps({"gate":gate,"total":str(total),"assets":manifest},ensure_ascii=False),flush=True)
assert gate["merits_native"] is True,"Independent sprite gate rejected; do not run Unity"
