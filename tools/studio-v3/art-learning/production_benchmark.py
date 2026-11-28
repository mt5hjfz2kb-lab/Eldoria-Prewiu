"""Prospective production experiment: new asset target, same donor/model/settings,
control versus learned workflow. No Unity render or game modification."""
import base64,hashlib,io,json,os,re,secrets
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
from openai import OpenAI
import studio,economics,guard
from decision_gate import arbitrate,PROTECTED
root=Path(os.environ["RUNNER_TEMP"]);out=root/"production-learning";out.mkdir(exist_ok=True)
base="f27f4774e32efc5ad046478a07747eead4256c3e"
prior=D(json.loads((root/"prior/summary.json").read_text())["estimated_total_usd"])
assert prior==D("1.6126543")
caps={"control-plan":D(".20"),"learned-plan":D(".20"),"control-image":D(".85"),"learned-image":D(".85"),"judge":D(".35")}
assert prior+sum(caps.values())<=D("5")
ledger=[]
def dump(n,v):(out/n).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def begin(slot):
    assert slot in caps and not any(x["slot"]==slot for x in ledger) and all(x["status"]=="complete" for x in ledger)
    assert prior+sum(D(x["estimated_usd"]) for x in ledger)+caps[slot]<=D("5")
    r={"slot":slot,"status":"in_flight","reserved_usd":str(caps[slot])};ledger.append(r);dump("ledger.json",ledger);print("START",slot,flush=True);return r
def finish(r,raw,model):
    cost=economics.estimate(raw.get("model") or model,raw["usage"],raw.get("service_tier") or "standard")
    r.update(status="complete",estimated_usd=str(cost),usage=raw["usage"],model=raw.get("model") or model);dump("ledger.json",ledger)
    assert cost<=D(r["reserved_usd"]);print("DONE",r["slot"],str(cost),flush=True)
def jpeg(path,maxsize=(1200,1500)):
    with Image.open(path) as im:
        im=im.convert("RGB");im.thumbnail(maxsize);b=io.BytesIO();im.save(b,format="JPEG",quality=94);return b.getvalue()
def ask(slot,prompt,imgs,tokens=2200):
    r=begin(slot)
    content=[{"type":"input_text","text":prompt}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(jpeg(p)).decode(),"detail":"high"} for p in imgs]
    model="gpt-6.2-sol"
    raw=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":tokens,"store":False},os.environ["OPENAI_API_KEY"])
    finish(r,raw,model);assert raw["status"]=="completed"
    s="\n".join(c["text"] for i in raw["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    (out/(slot+".txt")).write_text(s,encoding="utf-8")
    if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
    obj=json.loads(s);dump(slot+".json",obj);return obj
native=root/"native/baseline"
e=json.loads((native/"evidence.json").read_text());assert e["sourceSha"]==base and e["engineVersion"]=="6000.3.23f1" and e["nativeWorldRender"]
def frame(name):
    f=next(x for x in e["frames"] if x["name"]==name);p=native/f["file"];assert sha(p)==f["sha256"];return p,f
scene,f=frame("stage-5-postigo");home,h=frame("stage-5-home")
donor=root/"donor.png";meta=(root/"donor.png.meta").read_text()
def bbox(im):
    a=im.getchannel("A");b=a.point(lambda v:255 if v>128 else 0).getbbox()
    assert b and a.getextrema()[0]==0 and a.getextrema()[1]>=240
    return b
with Image.open(donor) as im:
    original=im.convert("RGBA");oldbox=bbox(original);W,H=original.size
ppu=float(re.search(r"spritePixelsToUnits:\s*([\d.]+)",meta).group(1));assert ppu==326.5
def panel(im,size,background=(62,67,63)):
    a=im.copy();a.thumbnail(size,Image.Resampling.LANCZOS)
    b=Image.new("RGB",size,background);b.paste(a,((size[0]-a.width)//2,(size[1]-a.height)//2),a.getchannel("A"));return b
panel(original,(620,620)).save(out/"donor.jpg",quality=95)
# Both arms receive exactly the same target evidence. Learned arm receives
# additional historical lessons and a visual positive/negative case contrast.
positive=root/"positive/mill-close-comparison.png"
negative=root/"negative/market-details.jpg"
assert positive.is_file() and negative.is_file()
brief="""Production exercise, NOT an instruction to alter the game. Target the existing ToneleriaDelPostigo cooperage sprite in Valoria Bastion V. Inputs: original independent sprite, original Unity detail of its street, original mobile city. Improve the visible craft identity of the cooperage and premium artistic quality without enlarging its occupied silhouette or blocking its neighbors. Preserve architectural language, roof/material distinction, orthographic viewing direction, open readable threshold, projected foot anchor and original import dimensions. No ground plate, baked people or cast shadows. Return strict JSON {hypothesis:string,strengths:[three concrete observed strengths],defects:[strings],alternatives:[two genuinely different approaches],adjacency:[strings],native_checks:[strings],image_prompt:string}. The image prompt should be a production-ready brief for editing the existing sprite, not a district illustration. Choose one approach and explain why; do not invent current visual defects not supported by images."""
common=[out/"donor.jpg",scene,home]
plans={}
for arm in ("control","learned"):
    extra=""
    imgs=common[:]
    if arm=="learned":
        extra="\n"+guard.role_brief("director")+"\n"+guard.role_brief("artist")+"\nVisual historical teaching cases appended: first, sawmill comparison (right selected locally by owner, left original); second, market comparison (A original, B rejected candidate). The latter shows why simplification can lose identity. Apply the lessons to THIS distinct cooperage target, not copy the sawmill."
        imgs += [positive,negative]
    plan=ask(arm+"-plan",brief+extra,imgs)
    assert all(k in plan for k in ("hypothesis","strengths","defects","alternatives","adjacency","native_checks","image_prompt"))
    assert isinstance(plan["image_prompt"],str) and 80<len(plan["image_prompt"])<6000
    if arm=="learned":
        gate=guard.plan_gate({**plan,"scope":"isolated Toneleria production exercise","source_sha":base,"baseline_frame_hashes":[f["sha256"],h["sha256"]],"mobile_preview":True,"reversible":True,"same_failed_mechanism":False,"owner_budget_verified":True})
        dump("learned-plan-gate.json",gate)
    plans[arm]=plan
client=OpenAI(max_retries=0,timeout=180)
fixed="Re-author a high-quality independent original game sprite using this owned original sprite as visual input. Preserve existing view angle, full visible building silhouette, its role and architecture. Do not add terrain, shadows, people, labels, or other buildings. Transparent PNG, safe margins, realistic painterly stylized Valoria materials, parallel 2.5D projection, warm upper-left light. "
assets={}
for arm in ("control","learned"):
    r=begin(arm+"-image")
    with donor.open("rb") as im:
        res=client.images.edit(model="gpt-image-2.6-flare",image=im,prompt=fixed+plans[arm]["image_prompt"],size="1024x1024",quality="high",background="transparent",output_format="png",n=1)
    raw=res.model_dump(exclude_none=True);finish(r,raw,"gpt-image-2.6-flare")
    p=out/(arm+"-raw.png");p.write_bytes(base64.b64decode(raw["data"][0]["b64_json"],validate=True))
    with Image.open(p) as im:
        assert im.mode=="RGBA";b=bbox(im);assert b[0]>2 and b[1]>2 and b[2]<im.width-2 and b[3]<im.height-2
        subject=im.crop(b);l,t,rr,bot=oldbox
        scale=min((rr-l)/subject.width,(bot-t)/subject.height)
        w,hh=max(1,round(subject.width*scale)),max(1,round(subject.height*scale))
        fitted=Image.new("RGBA",(W,H));fitted.paste(subject.resize((w,hh),Image.Resampling.LANCZOS),(round((l+rr-w)/2),bot-hh))
        dest=out/(arm+".png");fitted.save(dest)
    assets[arm]={"sha256":sha(dest),"raw_sha256":sha(p),"canvas":[W,H],"bbox":bbox(fitted),"ppu":ppu,"pivot":[.5,.075]}
dump("asset-manifest.json",{"source_sha":base,"original_sha256":sha(donor),"assets":assets,"scene_frame_sha256":f["sha256"],"home_sha256":h["sha256"]})
# Blinded identical scale swatches, not a composited/claimed Unity scene.
mapping={"X":"control","Y":"learned"} if secrets.randbelow(2)==0 else {"X":"learned","Y":"control"}
mobile_width=max(1,round(W/ppu*844/(2*h["orthographicSize"])))
def sheet(path,labels,items):
    canvas=Image.new("RGB",(1320,825),(41,43,42));d=ImageDraw.Draw(canvas)
    for i,(label,im) in enumerate(zip(labels,items)):
        x=i*440;d.text((x+12,10),label,fill="white")
        canvas.paste(panel(im,(420,575)),(x+10,30))
        small=im.resize((mobile_width,max(1,round(H*mobile_width/W))),Image.Resampling.LANCZOS)
        sw=Image.new("RGB",(420,182),(88,101,74));sw.paste(small,((420-small.width)//2,(182-small.height)//2),small.getchannel("A"));canvas.paste(sw,(x+10,625))
        d.text((x+10,608),"Approx. native home pixel scale (isolated)",fill="white")
    canvas.save(path,quality=95)
loaded={k:Image.open(out/(k+".png")).convert("RGBA") for k in ("control","learned")}
sheet(out/"blind.jpg",["Original","X","Y"],[original,loaded[mapping["X"]],loaded[mapping["Y"]]])
sheet(out/"comparison.jpg",["Original","Control","Learned"],[original,loaded["control"],loaded["learned"]])
dump("blind-mapping.json",mapping)
prompt="""Independent game environment art reviewer. First image shows original Unity street context, second original mobile city, third three isolated sprites: original, anonymous X and Y (large and same estimated mobile pixel scale). The sprites are not integrated in Unity. Judge each candidate against the original and each other only for asset-level craft quality, identity/function, readable silhouette at mobile scale, material richness, palette and projection fidelity to the scene. Evaluate whether the chosen design preserves the donor's strengths and improves something concrete. Do NOT claim actual in-scene integration, access or landmarks are validated; those require native render. Return strict JSON {scores:{Original:number,X:number,Y:number},preferred:'X'|'Y'|'Original'|'tie',observations:{Original:string,X:string,Y:string},protected:{X:{identity:string,function:string,mobile_readability:string},Y:{identity:string,function:string,mobile_readability:string}},native_checks:[strings],recommendation:string}. Each protected rating is good, minor_defect, material_defect, or uncertain, relative to required craft role. Scores 0..10. Spanish."""
judge=ask("judge",prompt,[scene,home,out/"blind.jpg"],2000)
assert set(judge["scores"])=={"Original","X","Y"} and all(type(v) in (int,float) and 0<=v<=10 for v in judge["scores"].values())
assert judge["preferred"] in ("X","Y","Original","tie")
scores={arm:judge["scores"][k] for k,arm in mapping.items()}
scores["original"]=judge["scores"]["Original"]
preferred=mapping.get(judge["preferred"],judge["preferred"])
total=prior+sum(D(x["estimated_usd"]) for x in ledger)
dump("summary.json",{"target":"ToneleriaDelPostigo","scores":scores,"preferred":preferred,"judge":judge,"mapping":mapping,"delta_learned_vs_control":round(scores["learned"]-scores["control"],2),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"single_prospective_case":True,"weights_trained":False,"native_integration":False,"game_modified":False,"billing_verified":False,"promoted":False})
print("PRODUCTION_LEARNING_FINAL",json.dumps(json.loads((out/"summary.json").read_text()),ensure_ascii=False),flush=True)
