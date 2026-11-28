"""Issue200 competition. Three targets, three distinct image-edit directions each,
blind selection against original and one correction round. All costs accounted.
No automatic art approval. No executable output from agents."""
import os,json,base64,hashlib,io,re,secrets,urllib.request,shutil
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw,ImageFilter,ImageChops
from openai import OpenAI
import studio,economics,guard
root=Path(os.environ["RUNNER_TEMP"]);out=root/"competition";out.mkdir(exist_ok=True)
src=root/"donors";native=root/"prior/baseline"
prior=D("1.9100013");maxnew=D("2");native_reserve=D(".20")
targets={"AceraZapateros":"Architecture: preserve the shoe-makers' street-front, every entrance, silhouette, roof, threshold and five registered shadow sills. Clarify the craft using specific shoe/leather tools and racks, not generic market props.","ArmeriaDelPatio":"Materials and light: preserve the armoury, military work identity and actual geometry/foot; improve readable hammered iron, timber, stone, controlled highlights and volumes. Do not turn it into a weapon shop or heroic palace.","CorteCaliza":"Environment: preserve authored limestone extraction front, geology, foot and visible quarry work; strengthen strata, chipped edges and working quarry identity, without adding terrain plate, separate buildings, broad shadows or changing the road."}
base="f27f4774e32efc5ad046478a07747eead4256c3e"
e=json.loads((native/"evidence.json").read_text());assert e["sourceSha"]==base and e["engineVersion"]=="6000.3.23f1"
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def jpeg(p,size=(1100,1500)):
    with Image.open(p) as im:
        im=im.convert("RGBA");im.thumbnail(size,Image.Resampling.LANCZOS);q=Image.new("RGB",im.size,(60,64,59));q.paste(im,(0,0),im.getchannel("A"));b=io.BytesIO();q.save(b,format="JPEG",quality=93);return b.getvalue()
ledger=[]
def begin(slot,cap):
    cap=D(cap)
    assert all(x["status"]=="complete" for x in ledger)
    assert not any(x["slot"]==slot for x in ledger)
    assert sum(D(x["estimated_usd"]) for x in ledger)+cap+native_reserve<=maxnew,"Budget exhausted"
    r={"slot":slot,"reserved_usd":str(cap),"status":"in_flight"};ledger.append(r);dump("ledger.json",ledger);return r
def finish(row,r,model):
    c=economics.estimate(r.get("model") or model,r["usage"],r.get("service_tier") or "standard")
    row.update(status="complete",estimated_usd=str(c),usage=r["usage"],model=r.get("model") or model);dump("ledger.json",ledger)
    assert c<=D(row["reserved_usd"]);print("CONTEST_CALL",row["slot"],str(c),flush=True)
def ask(slot,prompt,imgs,cap=".10",tokens=2200):
    row=begin(slot,cap)
    content=[{"type":"input_text","text":prompt}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(jpeg(p)).decode(),"detail":"high"} for p in imgs]
    model="gpt-6.2-sol";r=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":tokens,"store":False},os.environ["OPENAI_API_KEY"])
    finish(row,r,model);assert r["status"]=="completed"
    t="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    (out/(slot+".txt")).write_text(t,encoding="utf-8")
    if t.startswith("```"):t=t.split("\n",1)[1].rsplit("```",1)[0]
    j=json.loads(t);dump(slot+".json",j);return j
def getframe(n):
    f=next(x for x in e["frames"] if x["name"]==n);p=native/f["file"];assert sha(p)==f["sha256"];return p
frames=[getframe(n) for n in ("stage-5-home","wide","entry","quarry")]
# Official store screenshot evidence if available; no third party assets in production.
refs=[];receipts=[]
for app,name in ((1605558677,"Call of Dragons"),(6443575749,"Whiteout Survival")):
    try:
        req=urllib.request.Request("https://itunes.apple.com/lookup?id="+str(app)+"&country=us",headers={"User-Agent":"Valoria art research/1.0"})
        with urllib.request.urlopen(req,timeout=10) as f:data=json.load(f)
        rec=data["results"][0];urls=rec.get("screenshotUrls",[])[:2]
        for i,url in enumerate(urls):
            if not url.startswith("https://"):continue
            with urllib.request.urlopen(url,timeout=10) as f:raw=f.read(4_000_000)
            q=Image.open(io.BytesIO(raw)).convert("RGB");q.thumbnail((900,1300));p=out/(str(app)+"-"+str(i)+".jpg");q.save(p,quality=90);refs.append(p)
            receipts.append({"name":name,"url":url,"sha256":sha(p),"use":"study only; official promotional screenshot, not necessarily actual gameplay"})
    except Exception as ex:receipts.append({"name":name,"status":"not_retrieved","error":str(ex)[:160]})
dump("reference-receipts.json",receipts)
notes="""Study commercial references only as visual benchmarks: Call of Dragons and Whiteout Survival. Store images if retrieved may be promotional; never copy their assets or claim they show gameplay. Read the Academy role briefs and the environment art workflow notes: separate shape construction, modular assets, consistent texel density and materials, lighting/composition and assembly. Sources: https://80.lv/articles/vertical-slice-building-a-medieval-city (shared modular dimensions and architecture style rules); https://80.lv/articles/medieval-market-a-detailed-guide-on-stylized-environment-art-in-unreal-engine (modeling, texturing, modular assembly, lighting). Use the owned original sprites, not commercial screenshots, for image-generation input."""
prompt="Director of Valoria competitive atelier. Study actual stage V Unity images, then the three original owned sprites in target order, then any retrieved commercial store references. "+notes+"\n"+guard.role_brief("director",academy=True)+"\n"+guard.role_brief("artist",academy=True)+"\nTargets: "+json.dumps(targets)+"\nFor each target create three genuinely distinct, production-ready prompts, A architecture/silhouette faithful, B materials and controlled illumination, C functional storytelling/craft details using the donor as identity. No altering projected viewpoint, foot, original outer size or shadow direction; no new roads, characters or scene plates. Specify three visual strengths to protect, an observed defect, and a clear practical test. JSON {targets:{each target:{strengths:[strings],defect:string,A:string,B:string,C:string}},shared_style:string,reference_lessons:[strings]}. Spanish notes and actionable image prompts. Do not pretend a reference image is available if not visible."
plan=ask("director",prompt,frames+[src/(n+".png") for n in targets]+refs,cap=".18",tokens=4000)
assert set(plan["targets"])==set(targets)
for n,v in plan["targets"].items():
    assert len(v["strengths"])>=3 and all(isinstance(v[k],str) and len(v[k])>40 for k in ("A","B","C"))
client=OpenAI(max_retries=0,timeout=180)
def box(im):
    a=im.getchannel("A");b=a.point(lambda x:255 if x>128 else 0).getbbox()
    if not b or a.getextrema()[0]!=0 or a.getextrema()[1]<240:raise ValueError("Missing usable alpha")
    return b
def fit(raw,donor,dest):
    with Image.open(raw) as im,Image.open(donor) as orig:
        if im.mode!="RGBA":raise ValueError("Not RGBA")
        orig=orig.convert("RGBA");b=box(im);ob=box(orig);W,H=orig.size
        margin=min(b[0],b[1],im.width-b[2],im.height-b[3])
        subject=im.crop(b);l,t,r,bot=ob
        scale=min((r-l)/subject.width,(bot-t)/subject.height)
        w,h=max(1,round(subject.width*scale)),max(1,round(subject.height*scale))
        x=round((l+r-w)/2);y=bot-h
        assert x>=0 and y>=0 and x+w<=W and y+h<=H
        q=Image.new("RGBA",(W,H));q.paste(subject.resize((w,h),Image.Resampling.LANCZOS),(x,y))
        q.save(dest)
        # Exact silhouette and projection are not established by fitting alone.
        a=orig.getchannel("A").point(lambda v:255 if v>128 else 0);bmask=q.getchannel("A").point(lambda v:255 if v>128 else 0)
        intersection=ImageChops.multiply(a,bmask).histogram()[255]
        union=ImageChops.lighter(a,bmask).histogram()[255]
        return {"sha256":sha(dest),"canvas":[W,H],"bbox":box(q),"original_bbox":ob,"raw_margin":margin,"possible_clipping":margin==0,"alpha_iou":round(intersection/max(1,union),3)}
def generate(n,variant,brief,donor,revision=None):
    slot=n+"-"+variant;r=begin(slot,".14")
    fixed="Create a production-ready independent original 2.5D sprite for Valoria, not concept art. Use the attached OWNED donor as source. Same parallel orthographic view, silhouette footprint, ground foot, proportions, warm upper-left lighting, coherent painted wood/stone/metal. No broad ground plate, no extra buildings, no people, no text, no cast shadows, transparent PNG. Entire subject visible, 8% margin on all sides. Preserve distinctive role and material palette. "
    with donor.open("rb") as im:
        res=client.images.edit(model="gpt-image-2.6-flare",image=im,prompt=fixed+targets[n]+" "+plan["shared_style"]+" "+brief,size="1024x1024",quality="high",background="transparent",output_format="png",n=1)
    raw=res.model_dump(exclude_none=True);finish(r,raw,"gpt-image-2.6-flare")
    p=out/(slot+"-raw.png");p.write_bytes(base64.b64decode(raw["data"][0]["b64_json"],validate=True))
    dest=out/(slot+".png")
    try:return fit(p,src/(n+".png"),dest)
    except (ValueError,AssertionError) as ex:
        return {"invalid":str(ex),"raw_sha256":sha(p)}
def sheet(n,choices,filename):
    # All candidates retain original pixel scale; display each at common scale.
    W,H=Image.open(src/(n+".png")).size
    panelw=360;cols=len(choices);q=Image.new("RGB",(panelw*cols,740),(48,51,49));d=ImageDraw.Draw(q)
    for i,(label,p) in enumerate(choices):
        with Image.open(p) as im:
            im=im.convert("RGBA");scale=min(340/W,500/H);size=(round(W*scale),round(H*scale));r=im.resize(size,Image.Resampling.LANCZOS)
            q.paste(r,(i*panelw+(panelw-r.width)//2,35+(500-r.height)//2),r.getchannel("A"))
            # Use actual home camera scale from manifest and original PPU metadata.
            meta=(src/(n+".png.meta")).read_text();ppu=float(re.search(r"spritePixelsToUnits:\s*([\d.]+)",meta).group(1))
            scale_world={"AceraZapateros":1,"ArmeriaDelPatio":.88,"CorteCaliza":1}[n]
            f=next(x for x in e["frames"] if x["name"]=="stage-5-home")
            k=844/(2*f["orthographicSize"])*scale_world/ppu
            sm=im.resize((max(1,round(W*k)),max(1,round(H*k))),Image.Resampling.LANCZOS)
            q.paste(sm,(i*panelw+(panelw-sm.width)//2,580),sm.getchannel("A"))
        d.text((i*panelw+10,10),label,fill="white");d.text((i*panelw+10,550),"Approx. mobile pixel scale",fill="white")
    p=out/filename;q.save(p,quality=94);return p
def compare(n,variants,slot):
    valid=[k for k,m in variants.items() if not m.get("invalid") and not m.get("possible_clipping") and m.get("alpha_iou",0)>=.60]
    if not valid:return {"selected":"original","scores":{"original":None},"critique":"No technically eligible asset","correction":""}
    labels=["X","Y","Z","W"][:len(valid)];secrets.SystemRandom().shuffle(valid);mapping=dict(zip(labels,valid))
    choices=[("Original",src/(n+".png"))]+[(k,out/(n+"-"+v+".png")) for k,v in mapping.items()]
    p=sheet(n,choices,n+"-"+slot+"-blind.jpg")
    q=ask(n+"-"+slot,"Independent visual director. Compare original and anonymous candidates "+", ".join(labels)+" of "+n+". Target: "+targets[n]+". Same canvas and scale; first image is actual Unity wide, second target comparison. Judge asset quality, function, distinctive design, silhouette, perspective, materials, lighting and mobile legibility. Require materially visible improvement, not novelty. A sprite sheet cannot prove in-scene integration. Rate 0..10, explicitly flag any new material regression. Return JSON {scores:{Original:number,"+",".join(k+":number" for k in labels)+"},preferred:'Original' or candidate label,regressions:{"+",".join(k+":[strings]" for k in labels)+"},critique:string,correction:string}. Correction is one precise practical instruction for the preferred candidate, even if original wins. Spanish.",[frames[1],p],cap=".10",tokens=2000)
    assert set(q["scores"])==set(["Original"]+labels) and all(type(x) in (float,int) and 0<=x<=10 for x in q["scores"].values())
    assert q["preferred"] in ["Original"]+labels and set(q["regressions"])==set(labels)
    scores={"original":q["scores"]["Original"],**{v:q["scores"][k] for k,v in mapping.items()}}
    # Select strongest non-regressive alternative for native testing even if not better than original.
    allowed=[v for k,v in mapping.items() if not q["regressions"][k]]
    selected=mapping.get(q["preferred"],"original")
    if selected not in allowed:selected=max(allowed,key=lambda k:scores[k]) if allowed else "original"
    r={"selected":selected,"scores":scores,"mapping":mapping,"critique":q["critique"],"correction":q["correction"],"judge_preferred":mapping.get(q["preferred"],"original"),"regressions":q["regressions"]}
    dump(n+"-"+slot+"-result.json",r);return r
all_results={}
for n in targets:
    donor=src/(n+".png");vs={}
    for k in ("A","B","C"):vs[k]=generate(n,k,plan["targets"][n][k],donor)
    first=compare(n,vs,"selection")
    # Correct the strongest eligible variant; if no eligible variant, repair donor from critique.
    chosen=first["selected"];anchor=donor if chosen=="original" else out/(n+"-"+chosen+".png")
    if sum(D(x["estimated_usd"]) for x in ledger)+D(".14")+D(".10")+native_reserve<=maxnew:
        vs["R"]=generate(n,"R","One targeted correction only. Preserve remaining features of this donor. "+first["correction"]+" "+plan["targets"][n]["B"],anchor)
    final=compare(n,vs,"final")
    selected=final["selected"]
    chosenpath=donor if selected=="original" else out/(n+"-"+selected+".png")
    # original fallback still delivered; no false claim of new asset.
    shutil.copyfile(chosenpath,out/(n+"-final.png"))
    sheet(n,[("Original",donor)]+[(k,out/(n+"-"+k+".png")) for k,v in vs.items() if not v.get("invalid")],n+"-all-options.jpg")
    sheet(n,[("Original",donor),("Finalist "+selected,chosenpath)],n+"-final-comparison.jpg")
    all_results[n]={"variants":vs,"first":first,"final":final,"final_sha256":sha(out/(n+"-final.png")),"original_sha256":sha(donor),"selected":selected}
    dump("asset-results.json",all_results)
total=prior+sum(D(x["estimated_usd"]) for x in ledger)
assert total-prior+native_reserve<=maxnew
dump("parameters.json",{"source_head":base,"estimated_prior_usd":str(prior),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"assets":all_results,"billing_verified":False,"promoted":False})
print("COMPETITION_PRODUCTION_FINAL",json.dumps({n:{"selected":v["selected"],"scores":v["final"]["scores"]} for n,v in all_results.items()},ensure_ascii=False),str(total),flush=True)
