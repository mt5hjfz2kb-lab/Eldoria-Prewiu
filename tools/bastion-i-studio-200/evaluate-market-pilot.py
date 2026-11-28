"""Independent blind native district audit, verified source bytes and frame hashes."""
import os,json,hashlib,base64,io
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);src=root/"native";prod=root/"production";out=root/"market-delivery";out.mkdir(exist_ok=True)
p=json.loads((prod/"parameters.json").read_text());prior=D(p["estimated_total_usd"])
assert prior+D(".45")<=D("5") and p["sprite_gate"]["merits_native"] is True
a=json.loads((src/"baseline/evidence.json").read_text());b=json.loads((src/"candidate/evidence.json").read_text())
assert a["sourceSha"]==b["sourceSha"]==p["source_head"] and a["engineVersion"]==b["engineVersion"]=="6000.3.23f1"
assert all(e["nativeWorldRender"] and e["gatewayBuild"] and e["savedAndReloaded"] and e["progressionStagesCaptured"]==5 for e in (a,b))
assert len(a["frames"])==len(b["frames"])==74
frames={}
for x,y in zip(a["frames"],b["frames"]):
    assert all(x[k]==y[k] for k in ("name","width","height","orthographicSize","cameraX","cameraY"))
    pair=[src/d/z["file"] for d,z in (("baseline",x),("candidate",y))]
    for f,z in zip(pair,(x,y)):assert hashlib.sha256(f.read_bytes()).hexdigest()==z["sha256"],str(f)
    frames[x["name"]]=pair
def make(names,file,reverse=False,w=620,h=660):
    canvas=Image.new("RGB",(w*2,(h+32)*len(names)),(27,28,30));d=ImageDraw.Draw(canvas)
    for j,n in enumerate(names):
        for i,f in enumerate(frames[n][::-1] if reverse else frames[n]):
            with Image.open(f) as im:q=im.convert("RGB");q.thumbnail((w-8,h-8))
            x=i*w;y=j*(h+32);canvas.paste(q,(x+(w-q.width)//2,y+32+(h-q.height)//2))
            d.text((x+8,y+8),n+" | "+("A" if i==0 else "B"),fill="white")
    buf=io.BytesIO();canvas.save(buf,format="JPEG",quality=95);data=buf.getvalue();assert len(data)>10000
    (out/file).write_bytes(data);return data
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False),encoding="utf-8")
make(["market-mobile","market-overview"],"market-before-after.jpg",w=650,h=800)
make(["peso","granary","stall"],"market-details.jpg",w=720,h=680)
make(["junction","entry","wide"],"city-context.jpg",w=620,h=710)
make(["stage-1-home","stage-4-home","stage-5-home"],"progression-control.jpg",w=420,h=690)
# Pair B/A for the blind evaluation, A=novel, B=original; labels not exposed.
general=make(["market-mobile","market-overview","wide"],"blind-general.jpg",True,w=620,h=680)
details=make(["peso","granary","stall","junction"],"blind-details.jpg",True,w=510,h=510)
manifest={"source_sha":p["source_head"],"matched_frames":len(frames),"unity_editor":True,"stage_1_4_unchanged_resources":True,"playmode":False,"device":False}
dump("evidence-summary.json",manifest)
brief="""Independent senior environment art director. Compare two actual matched Unity scenes of the SAME Valoria Bastion V market district, A and B, blind. Identical camera angles; main castle, granary and sawmill C unchanged. Judge the district as a cohesive commercial-quality game environment, NOT whether a commit exists or art is new. Priorities: architecture quality and identity, clear silhouette of weighing house and market stall, independence of sprites without collage, physical ground contact, connected streets, clean readable well and granary entrance, palette/light, hierarchy at mobile size, detail that communicates market work. A strong change may be worse; don't assume new is better. Score BOTH variants on seven 0..10 criteria resources,integration,coherence,composition,materials,readability,polish; weighted 20/20/15/15/10/10/10. Also score whole city (0..10, based only on provided wide/entry), distinct from district. Return JSON {A:{scores:{seven keys},city:number},B:{scores:{seven keys},city:number},observed_differences:[strings],regressions:[strings],remaining_defects:[strings],recommendation:string,preferred:"A"|"B"|"tie"}. Spanish, honest and concrete. No claims of PlayMode or device tests; compare the actual images."""
payload={"model":"gpt-6.2-sol","input":[{"role":"user","content":[{"type":"input_text","text":brief}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(q).decode(),"detail":"high"} for q in (general,details)]}],"reasoning":{"effort":"low"},"max_output_tokens":2400,"store":False}
dump("audit-ledger.json",{"status":"in_flight","reserved_usd":".45"})
r=studio.request_api(payload,os.environ["OPENAI_API_KEY"])
cost=economics.estimate(r.get("model") or payload["model"],r["usage"],r.get("service_tier") or "standard")
dump("audit-ledger.json",{"status":"complete","estimated_usd":str(cost),"usage":r["usage"]})
assert cost<=D(".45") and r["status"]=="completed"
s="\n".join(c["text"] for j in r["output"] if j.get("type")=="message" for c in j["content"] if c.get("type")=="output_text").strip()
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
j=json.loads(s);w={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
for k in ("A","B"):assert set(j[k]["scores"])==set(w) and all(type(v) in (int,float) and 0<=v<=10 for v in j[k]["scores"].values()) and 0<=j[k]["city"]<=10
def score(k):return round(sum(j[k]["scores"][m]*n for m,n in w.items())/100,2)
before,after=score("B"),score("A")
summary=dict(manifest,baseline=before,candidate=after,delta=round(after-before,2),city_before=j["B"]["city"],city_after=j["A"]["city"],audit=j,target_met=after>=9 and after-before>=.3 and j["preferred"]=="A",estimated_total_usd=str(prior+cost),estimated_audit_usd=str(cost),billing_verified=False,promoted=False)
dump("summary.json",summary)
print("MARKET_NATIVE_FINAL "+json.dumps(summary,ensure_ascii=False),flush=True)
