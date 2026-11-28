"""Adaptive follow-up using already-paid control asset. It is not a new independent holdout."""
import os,json,hashlib,base64,io,secrets
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
import studio,economics
from protected_region import repair
root=Path(os.environ["RUNNER_TEMP"]);src=root/"previous";out=root/"local-repair";out.mkdir(exist_ok=True)
p=json.loads((src/"summary.json").read_text());m=json.loads((src/"asset-manifest.json").read_text())
prior=D(p["estimated_total_usd"]);assert prior==D("1.7924378") and prior+D(".25")<=D("5")
assert all(x["status"]=="complete" for x in json.loads((src/"ledger.json").read_text()))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2))
donor=root/"donor.png";assert sha(donor)==m["original_sha256"] and sha(src/"control.png")==m["assets"]["control"]["sha256"]
a=Image.open(donor).convert("RGBA");b=Image.open(src/"control.png").convert("RGBA")
assert a.size==b.size==(653,478)
# Pixel-space region author inspected against the source and proposal:
# central cask/workpiece, excluding structural post, sign, roof, external barrels.
polygon=[(230,312),(243,305),(285,305),(306,316),(316,349),(312,386),(301,401),(246,404),(228,387),(226,347)]
c,mask,metrics=repair(a,b,polygon,4)
c.save(out/"protected-repair.png");mask.save(out/"authorised-region.png")
metrics.update(original_sha256=sha(donor),candidate_sha256=sha(out/"protected-repair.png"),proposal_sha256=sha(src/"control.png"),polygon=polygon,feather=4)
dump("pixel-proof.json",metrics)
def panel(im,w,h):
    q=im.copy();q.thumbnail((w,h),Image.Resampling.LANCZOS);p=Image.new("RGB",(w,h),(66,69,66));p.paste(q,((w-q.width)//2,(h-q.height)//2),q.getchannel("A"));return p
def sheet(path,labels,ims):
    q=Image.new("RGB",(1320,690),(32,34,33));d=ImageDraw.Draw(q)
    for i,(label,im) in enumerate(zip(labels,ims)):
        x=i*440;d.text((x+8,10),label,fill="white")
        q.paste(panel(im,420,340),(x+10,32))
        detail=im.crop((205,290,340,420));q.paste(panel(detail.resize((270,260),Image.Resampling.LANCZOS),420,260),(x+10,390))
        d.text((x+8,655),"Detail crop enlarged identically",fill="white")
    q.save(path,quality=95)
mapping={"X":"full","Y":"protected"} if secrets.randbelow(2)==0 else {"X":"protected","Y":"full"}
v={"full":b,"protected":c}
sheet(out/"blind.jpg",["Original","X","Y"],[a,v[mapping["X"]],v[mapping["Y"]]])
sheet(out/"comparison.jpg",["Original","Full control","Protected local repair"],[a,b,c])
dump("blind-mapping.json",mapping)
prompt="Independent art reviewer: three isolated sprites with the same full view and detail crop. Original, anonymous X and Y. Assess whether the unfinished central cask is clearer as a work in progress (not finished storage), whether the local transition has seams or double edges, and whether surrounding architecture, function, palette and perspective are preserved. Prefer original if neither change improves it. Rate each 0..10; no Unity integration or mobile claim. Strict JSON {scores:{Original:number,X:number,Y:number},preferred:'Original'|'X'|'Y'|'tie',observations:{Original:string,X:string,Y:string},seam_or_alignment_defects:{X:[strings],Y:[strings]},recommendation:string}. Spanish."
dump("ledger.json",{"status":"in_flight","reserved_usd":".25"})
r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":[{"type":"input_text","text":prompt},{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode((out/"blind.jpg").read_bytes()).decode(),"detail":"high"}]}],"reasoning":{"effort":"low"},"max_output_tokens":1600,"store":False},os.environ["OPENAI_API_KEY"])
cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard");dump("ledger.json",{"status":"complete","estimated_usd":str(cost),"usage":r["usage"]})
assert cost<=D(".25") and r["status"]=="completed"
s="\n".join(x["text"] for y in r["output"] if y.get("type")=="message" for x in y["content"] if x.get("type")=="output_text").strip()
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
j=json.loads(s);assert set(j["scores"])=={"Original","X","Y"} and all(type(x) in (int,float) and 0<=x<=10 for x in j["scores"].values())
assert j["preferred"] in ("Original","X","Y","tie")
scores={mapping[k]:j["scores"][k] for k in mapping};scores["original"]=j["scores"]["Original"]
summary={"scores":scores,"preferred":mapping.get(j["preferred"],j["preferred"]),"audit":j,"mapping":mapping,"pixel_proof":metrics,"estimated_new_usd":str(cost),"estimated_total_usd":str(prior+cost),"adaptive_same_case":True,"native_integration":False,"game_modified":False,"billing_verified":False}
dump("summary.json",summary);print("PROTECTED_REPAIR_FINAL",json.dumps(summary,ensure_ascii=False),flush=True)
