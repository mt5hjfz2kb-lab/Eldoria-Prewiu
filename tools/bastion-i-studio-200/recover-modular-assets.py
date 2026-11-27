import os,json,base64,hashlib,sys
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter,ImageChops
import studio,generate_candidate as gen,economics
root=Path(os.environ["RUNNER_TEMP"]); out=root/"production"; out.mkdir(exist_ok=True)
source=root/"concepts"; key=os.environ["OPENAI_API_KEY"]
assert key
prior=json.loads((source/"summary.json").read_text())
assert prior["calls"]==8 and D(prior["estimated_usd"])==D("0.3408978")
assert all(x["status"]=="complete" for x in json.loads((source/"ledger.json").read_text()))
ledger=json.loads((out/'ledger.json').read_text())
assert [x['slot'] for x in ledger]==['production-director','bastion','sawmill'] and all(x['status']=='complete' for x in ledger)
assert sum(D(x['estimated_usd']) for x in ledger)==D('0.130566')
def dump(name,obj): (out/name).write_text(json.dumps(obj,indent=2,ensure_ascii=False))
def reserve(slot,cap):
    assert len(ledger)<5 and all(r["status"]=="complete" for r in ledger)
    assert D(prior["estimated_usd"])+sum(D(r["reserved"]) for r in ledger)+D(cap)<=D("5")
    r={"slot":slot,"reserved":cap,"status":"in_flight"};ledger.append(r);dump("ledger.json",ledger);print("START "+slot,flush=True);return r
def finish(r,raw,model):
    cost=economics.estimate(model,raw["usage"],raw.get("service_tier") or "standard")
    r.update(status="complete",estimated_usd=str(cost),usage=raw["usage"],model=model);dump("ledger.json",ledger)
    assert cost<=D(r["reserved"]);print("DONE "+r["slot"]+" "+str(cost),flush=True)
def ask(slot,brief,imgs,cap="0.45"):
    r=reserve(slot,cap)
    content=[{"type":"input_text","text":brief}]
    for p in imgs:
        content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(p.read_bytes()).decode(),"detail":"high"})
    model="gpt-6.2-sol"
    raw=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"max_output_tokens":3000,"reasoning":{"effort":"low"},"store":False},key)
    finish(r,raw,raw.get("model") or model);assert raw.get("status")=="completed"
    s="\n".join(c["text"] for i in raw.get("output",[]) if i.get("type")=="message" for c in i.get("content",[]) if c.get("type")=="output_text")
    (out/(slot+".txt")).write_text(s);return s
brief="You are the original Eldoria environment artist, now producing real modular assets, not another city illustration. Image 1 is approved direction B (castle); image 2 approved C (sawmill); preserve Valoria's orthographic fixed 2.5D projection, upper-left sunlight, independent foot pivots, no ground/platform baked into sprites. Produce exactly one JSON object with keys bastion, sawmill, meadow, each a detailed English image generation prompt. Castle: faithfully reproduce silhouette and weathered warm masonry, slate roofs, gateway orientation, scale and restrained ornament from B, only one building. Sawmill: faithfully reproduce open-frame saw workshop, timber stacks and tools of C, only one building. Each building isolated on perfectly uniform saturated magenta RGB 255,0,255 background, no magenta in subject, no ground, no cast shadow, full silhouette with safe margins, all supports visible, no perspective convergence. Meadow: overhead seamless square diffuse grass texture matching B, subtle broad irregular green ochre variation, tiny natural detail, no path, props, building or shadows, seamless edges. No text. Do not invent additional buildings. Existing artist B analysis: "+(source/"artist-B.md").read_text()[:6000]+"\nExisting C analysis: "+(source/"artist-C.md").read_text()[:5000]
prompts=json.loads((out/"prompts.json").read_text())
assert set(prompts)=={"bastion","sawmill","meadow"}
def remove_key(im):
    import numpy as np
    a=np.array(im.convert("RGB"),dtype=np.float32);r,g,b=a[:,:,0],a[:,:,1],a[:,:,2]
    # Smooth alpha based on magenta excess, not simple RGB equality. The building palette excludes magenta.
    chroma=np.minimum(r,b)-g
    alpha=np.clip((95-chroma)/65,0,1)
    # eliminate magenta spill only near keyed edges
    edge=(alpha<.995)&(alpha>0);a[:,:,0][edge]=np.minimum(r[edge],g[edge]+35);a[:,:,2][edge]=np.minimum(b[edge],g[edge]+35)
    rgba=np.dstack((np.clip(a,0,255),alpha*255)).astype("uint8")
    result=Image.fromarray(rgba,"RGBA")
    mask=result.getchannel("A");bbox=mask.point(lambda p:255 if p>128 else 0).getbbox()
    assert bbox and bbox[0]>4 and bbox[1]>4 and bbox[2]<im.width-4 and bbox[3]<im.height-4,"Subject touches frame or key failed"
    fraction=float((alpha>.5).mean());assert .13<fraction<.78,"Unusable alpha coverage"
    return result.crop(bbox),{"alpha_coverage":fraction,"bbox":bbox}
def tile(im):
    # Wrap-safe low-frequency seam correction; retain independent authored texture.
    import numpy as np
    a=np.array(im.convert("RGB").resize((1024,1024),Image.Resampling.LANCZOS),dtype=np.float32)
    for ax in (0,1):
        if ax==1:a=a.transpose(1,0,2)
        n=64; avg=(a[:n]+a[-n:][::-1])/2
        for i in range(n):
            w=(1-i/(n-1))**2;a[i]=a[i]*(1-w)+avg[i]*w;a[-1-i]=a[-1-i]*(1-w)+avg[i]*w
        if ax==1:a=a.transpose(1,0,2)
    a[0]=a[-1]=(a[0]+a[-1])/2;a[:,0]=a[:,-1]=(a[:,0]+a[:,-1])/2
    return Image.fromarray(np.clip(a,0,255).astype("uint8"))
manifest=json.loads((out/"asset-manifest.json").read_text())
assert set(manifest)=={"bastion"}
assert hashlib.sha256((out/"bastion.png").read_bytes()).hexdigest()==manifest["bastion"]["sha256"]
for name in ("bastion","sawmill","meadow"):
    if name=="bastion":continue
    if name=="sawmill":
        f=out/"sawmill-original.png";bits=f.read_bytes()
        with Image.open(f) as im:
            assert im.mode=="RGBA" and im.getchannel("A").getextrema()[0]==0 and im.getchannel("A").getextrema()[1]>=250
            a=im.getchannel("A");bbox=a.point(lambda x:255 if x>128 else 0).getbbox()
            assert bbox and bbox[0]>4 and bbox[1]>4 and bbox[2]<im.width-4 and bbox[3]<im.height-4
            result=im.crop(bbox);info={"native_alpha_preserved":True,"bbox":bbox}
            result.save(out/(name+".png"))
        meta={"model":gen.MODEL,"recovered_from_previous_run":46031689984,"image_sha256":hashlib.sha256(bits).hexdigest()}
        p={"prompt":prompts[name]}
    else:
        r=reserve(name,"0.95")
        p=gen.make_payload(prompts[name]);p["quality"]="high"
        raw=gen.generate(p,key);finish(r,raw,gen.MODEL);bits,meta=gen.unpack(raw)
        f=out/(name+"-original.png");f.write_bytes(bits)
        with Image.open(f) as im:
            result=tile(im);info={"wrap_safe_edges":True};result.save(out/(name+".png"))
    manifest[name]={**info,"sha256":hashlib.sha256((out/(name+".png")).read_bytes()).hexdigest(),"width":result.width,"height":result.height,"original_sha256":hashlib.sha256(bits).hexdigest(),"asset_role":"independent sprite" if name!="meadow" else "continuous material","art_approved":False}
    dump("asset-manifest.json",manifest)
    dump(name+"-receipt.json",{**meta,"quality":"high","prompt":p["prompt"]})
# Compare sprites with concepts; no rating of game or claim of Unity.
sheet=Image.new("RGB",(1050,600),(101,115,66));draw=ImageDraw.Draw(sheet)
for i,name in enumerate(("bastion","sawmill","meadow")):
    with Image.open(out/(name+".png")) as im:
        im=im.convert("RGBA");im.thumbnail((340,540));sheet.paste(im,(i*350+(350-im.width)//2,45),im)
    draw.text((i*350+10,10),name,fill="white")
sheet.save(out/"asset-review.jpg",quality=94)
audit=ask("asset-auditor","Independent technical art review. Judge the three independent assets against the selected B castle and C sawmill concepts described in the brief. Reject defects: magenta fringe, ground baked into buildings, changed orientation, texture seams or wrong perspective. Judge only assets; not a Unity scene or commercial grade. Explain changes for the integrator and whether they merit native A/B. Spanish. "+brief[:4000],[out/"asset-review.jpg",source/"concept-B.jpg",source/"concept-C.jpg"],"0.35")
dump("summary.json",{"previous_estimate_usd":prior["estimated_usd"],"new_estimate_usd":str(sum(D(x["estimated_usd"]) for x in ledger)),"total_estimated_usd":str(D(prior["estimated_usd"])+sum(D(x["estimated_usd"]) for x in ledger)),"calls":len(ledger),"native_unity":False,"art_approved":False,"billing_verified":False})
