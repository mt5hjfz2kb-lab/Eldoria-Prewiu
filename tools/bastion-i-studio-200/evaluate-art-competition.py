"""Native competition evaluation; blinded per-asset and bundle, exact matched render bytes."""
import os,json,base64,hashlib,secrets
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);native=root/"native";prod=root/"production";out=root/"competition-delivery";out.mkdir(exist_ok=True)
p=json.loads((prod/"parameters.json").read_text());prior=D(p["estimated_total_usd"]);assert prior-D("1.9100013")+D(".20")<=D("2")
names=["AceraZapateros","ArmeriaDelPatio","CorteCaliza"]
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
e={}
for folder in ("baseline","asset-1","asset-2","asset-4","bundle"):
    q=json.loads((native/folder/"evidence.json").read_text())
    assert q["sourceSha"]==p["source_head"] and q["engineVersion"]=="6000.3.23f1"
    assert q["nativeWorldRender"] and q["gatewayBuild"] and q["savedAndReloaded"] and q["progressionStagesCaptured"]==5
    assert len(q["frames"])==(48 if folder in ("baseline","bundle") else 12)
    e[folder]={x["name"]:x for x in q["frames"]}
    for f in q["frames"]:
        assert sha(native/folder/f["file"])==f["sha256"]
        b=e["baseline"][f["name"]]
        assert all(f[k]==b[k] for k in ("width","height","orthographicSize","cameraX","cameraY"))
def make(folder,frames,name,reverse=False):
    cols=["baseline",folder][::-1] if reverse else ["baseline",folder]
    w,h=620,630;canv=Image.new("RGB",(w*2,(h+32)*len(frames)),(34,36,37));d=ImageDraw.Draw(canv)
    for j,f in enumerate(frames):
        for i,col in enumerate(cols):
            with Image.open(native/col/e[col][f]["file"]) as im:
                q=im.convert("RGB");q.thumbnail((w-8,h-8))
                canv.paste(q,(i*w+(w-q.width)//2,j*(h+32)+32+(h-q.height)//2))
            d.text((i*w+10,j*(h+32)+8),f+" | "+(["X","Y"][i] if name.startswith("blind") else ["Original","Candidate"][i]),fill="white")
    canv.save(out/name,quality=94)
    return out/name
mapping={};imgs=[];jobs=[]
for i,n in enumerate(names):
    folder="asset-"+str(1<<i)
    fs=[n+"-detail",n+"-mobile","wide"]
    make(folder,fs,n+"-native.jpg")
    reverse=bool(secrets.randbelow(2))
    mapping[n]={"X":"candidate" if reverse else "baseline","Y":"baseline" if reverse else "candidate"}
    imgs.append(make(folder,fs,"blind-"+n+".jpg",reverse));jobs.append(n)
reverse=bool(secrets.randbelow(2));mapping["bundle"]={"X":"candidate" if reverse else "baseline","Y":"baseline" if reverse else "candidate"}
make("bundle",["stage-5-home","wide"]+ [n+"-detail" for n in names],"bastion-v-comparison.jpg")
imgs.append(make("bundle",["stage-5-home","wide"],"blind-bundle.jpg",reverse));jobs.append("bundle")
dump("mapping.json",mapping)
prompt="""Independent senior environment art director, blind pairs X/Y of ACTUAL Unity Editor scene. First three sheets each compare exactly one original vs proposed sprite in unchanged Bastion V context: AceraZapateros (shoemakers' street), ArmeriaDelPatio (military workshop), CorteCaliza (quarry face). Fourth compares full city with all three. Camera, footprint, routes and materials outside candidates held fixed; inspect scene rather than novelty. For each pair rate both variants 0..10 considering resource quality, integration, identity, composition, materials/light, mobile readability, polish. Explicitly note concrete material regressions for BOTH X and Y, and choose X, Y or tie. Require clearly better craft/function and context, not just different. Do not claim device QA or PlayMode. Return strict JSON {AceraZapateros:{X:number,Y:number,preferred:'X'|'Y'|'tie',regressions:{X:[strings],Y:[strings]},evidence:string},ArmeriaDelPatio:{same},CorteCaliza:{same},bundle:{same}}. Spanish."""
content=[{"type":"input_text","text":prompt}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(f.read_bytes()).decode(),"detail":"high"} for f in imgs]
dump("audit-ledger.json",{"status":"in_flight","reserved_usd":".20"})
r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":3200,"store":False},os.environ["OPENAI_API_KEY"])
cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard")
dump("audit-ledger.json",{"status":"complete","estimated_usd":str(cost),"usage":r["usage"]})
assert cost<=D(".20") and r["status"]=="completed"
s="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
(out/"audit.txt").write_text(s,encoding="utf-8")
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
q=json.loads(s);assert set(q)==set(jobs)
results={}
for k,v in q.items():
    assert all(type(v[x]) in (int,float) and 0<=v[x]<=10 for x in ("X","Y")) and v["preferred"] in ("X","Y","tie") and set(v["regressions"])=={"X","Y"}
    scores={mapping[k][x]:v[x] for x in ("X","Y")}
    regressions={mapping[k][x]:v["regressions"][x] for x in ("X","Y")}
    delta=round(scores["candidate"]-scores["baseline"],2);preferred=mapping[k].get(v["preferred"],"tie")
    results[k]={**scores,"delta":delta,"preferred":preferred,"regressions":regressions,"evidence":v["evidence"],"eligible_for_owner_review":delta>=.3 and scores["candidate"]>=8.5 and preferred=="candidate" and not regressions["candidate"]}
summary={"source":p["source_head"],"native_frames":sum(len(x) for x in e.values()),"native":results,"asset_selection":{k:{"selected":v["selected"],"scores":v["final"]["scores"],"preferred":v["final"].get("judge_preferred")} for k,v in p["assets"].items()},"three_winners":all(results[k]["eligible_for_owner_review"] and p["assets"][k]["selected"]!="original" for k in names),"estimated_total_usd":str(prior+cost),"estimated_additional_usd":str(prior+cost-D("1.9100013")),"playmode":False,"device":False,"promoted":False,"billing_verified":False}
dump("summary.json",summary)
print("ART_COMPETITION_NATIVE_FINAL",json.dumps(summary,ensure_ascii=False),flush=True)
