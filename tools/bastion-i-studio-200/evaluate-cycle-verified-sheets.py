"""Verify and independently score matched actual native frames for five stages."""
import os,json,hashlib,base64,concurrent.futures,threading
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);src=root/"native";out=root/"cycle-delivery";out.mkdir(exist_ok=True)
p=json.loads((root/"production/parameters.json").read_text());prior=D(p["estimated_total_usd"])
assert prior+D("1.75")<=D("5")
assert all(x["status"]=="complete" for x in json.loads((root/"production/ledger.json").read_text()))
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False))
a=json.loads((src/"baseline/evidence.json").read_text());b=json.loads((src/"candidate/evidence.json").read_text())
assert a["sourceSha"]==b["sourceSha"]==p["source_head"]=="f27f4774e32efc5ad046478a07747eead4256c3e"
assert a["engine"]==b["engine"]=="Unity" and a["engineVersion"]==b["engineVersion"]=="6000.3.23f1"
for e in (a,b):assert e["nativeWorldRender"] and e["gatewayBuild"] and e["savedAndReloaded"] and e["progressionStagesCaptured"]==5
assert len(a["frames"])==len(b["frames"])>=100
frames={}
for x,y in zip(a["frames"],b["frames"]):
    assert all(x[k]==y[k] for k in ("name","width","height","orthographicSize","cameraX","cameraY"))
    paths=[src/d/f["file"] for d,f in [("baseline",x),("candidate",y)]]
    for path,f in zip(paths,[x,y]):
        actual=hashlib.sha256(path.read_bytes()).hexdigest()
        assert actual==f["sha256"], (str(path),actual,f["sha256"])
    frames[x["name"]]=paths
for i in range(1,6):assert any(x["sha256"]!=y["sha256"] for x,y in zip(a["frames"],b["frames"]) if x["name"].startswith("stage-"+str(i)+"-"))
def make_pair(names,path,reverse=False,w=620,h=660):
    import io
    canvas=Image.new("RGB",(w*2,(h+32)*len(names)),(27,28,30));d=ImageDraw.Draw(canvas)
    for j,n in enumerate(names):
        paths=frames[n][::-1] if reverse else frames[n]
        for k,p in enumerate(paths):
            with Image.open(p) as source:
                im=source.convert("RGB")
            im.thumbnail((w-8,h-8));x=k*w;y=j*(h+32)
            canvas.paste(im,(x+(w-im.width)//2,y+32+(h-im.height)//2))
            d.text((x+10,y+8),n+" | "+("A" if k==0 else "B"),fill="white")
    buf=io.BytesIO()
    canvas.save(buf,format="JPEG",quality=94)
    payload=buf.getvalue()
    assert payload[:2]==bytes([255,216]) and len(payload)>10000,(str(path),len(payload))
    path.write_bytes(payload)
    assert path.is_file() and path.stat().st_size==len(payload),str(path)
    print("SHEET",path.name,len(payload),hashlib.sha256(payload).hexdigest(),flush=True)
    return payload
# User delivery: explicit original/candidate labels in report, A original B new.
for level in range(1,6):
    n="stage-"+str(level)+"-"
    make_pair([n+"home",n+"entry"],out/(n+"comparison.jpg"),w=470,h=850)
    make_pair([n+"keep",n+"mill",n+"junction"],out/(n+"contact-details.jpg"),w=680,h=680)
    make_pair([n+"framing-study"],out/(n+"framing-study.jpg"),w=470,h=850)
make_pair(["stage-5-postigo","stage-5-oficios","stage-5-peso","stage-5-adarve"],out/"stage-5-urban-details.jpg",w=620,h=620)
dump("evidence-summary.json",{"native_unity":True,"matched_frames":len(frames),"source_sha":p["source_head"],"levels":5,"camera_studies_separate":True,"actual_device":False,"playmode":False,"production_promoted":False,"artist_parameters":p["values"]})
expected=["stage-"+str(i)+"-"+n+".jpg" for i in range(1,6) for n in ("comparison","contact-details","framing-study")]+["stage-5-urban-details.jpg"]
assert all((out/n).is_file() for n in expected), ("Missing user sheets",list(out.iterdir()))
print("VERIFIED_116_NATIVE_AND_16_SHEETS",flush=True)
ledger=[{"stage":i,"status":"reserved","reserved_usd":".35"} for i in range(1,6)]
lock=threading.Lock();dump("audit-ledger.json",ledger)
weights={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
def judge(i):
    n="stage-"+str(i)+"-";reverse=i%2==0
    x=out/(n+"blind-general.jpg");y=out/(n+"blind-detail.jpg")
    first=make_pair([n+"home",n+"entry"],x,reverse,w=420,h=720)
    detail=[n+"keep",n+"junction",n+"mill"]
    if i>=3:detail.append(n+"market")
    if i==4:detail.append(n+"textile")
    if i==5:detail += [n+"postigo",n+"peso",n+"adarve"]
    second=make_pair(detail,y,reverse,w=410,h=410)
    brief="Auditor independiente de entorno 2.5D Valoria. Evalúa DOS variantes reales de Unity del Bastión "+str(i)+" etiquetadas A/B (orden ciego), mismas cámaras y progresión, sin HUD. Mismos sprites de edificios, aserradero aprobado C, terreno continuo y reservas de crecimiento deliberadas. Puntúa A y B por resources,integration,coherence,composition,materials,readability,polish de 0 a 10; no premiar diferencias invisibles, no atribuir cambios no visibles, no asumir que la nueva gana, no calificar pruebas de dispositivo. Examina umbrales, jerarquía calles, pozo, patio, contacto, granero y postigo cuando aparezcan. Devuelve exclusivamente JSON {A:{7 scores},B:{7 scores},observed_improvements:[strings],regressions:[strings],unchanged:[strings],recommendation:string}. Español, identifica variantes explícitamente. Si son similares, puntúa similar."
    content=[{"type":"input_text","text":brief}]
    for payload in (first,second):content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(payload).decode(),"detail":"high"})
    with lock:ledger[i-1]["status"]="in_flight";dump("audit-ledger.json",ledger)
    r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":1800,"store":False},os.environ["OPENAI_API_KEY"])
    cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard")
    with lock:ledger[i-1].update(status="complete",estimated_usd=str(cost),usage=r["usage"]);dump("audit-ledger.json",ledger)
    assert cost<=D(".35") and r["status"]=="completed"
    s="\n".join(c["text"] for j in r["output"] if j.get("type")=="message" for c in j["content"] if c.get("type")=="output_text").strip()
    (out/(n+"audit.txt")).write_text(s)
    if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
    v=json.loads(s)
    scores={}
    for k in ("A","B"):
        assert set(v[k])==set(weights) and all(type(z) in (int,float) and 0<=z<=10 for z in v[k].values())
        scores[k]=round(sum(v[k][m]*w for m,w in weights.items())/100,2)
    baseline="B" if reverse else "A";candidate="A" if reverse else "B"
    result={"stage":i,"baseline_label":baseline,"candidate_label":candidate,"before":scores[baseline],"after":scores[candidate],"delta":round(scores[candidate]-scores[baseline],2),"audit":v}
    dump(n+"audit.json",result);return result
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as ex: results=list(ex.map(judge,range(1,6)))
total=prior+sum(D(r["estimated_usd"]) for r in ledger)
assert total<=D("5")
summary={"native_frames_per_variant":len(frames),"stage_results":results,"mean_before":round(sum(x["before"] for x in results)/5,2),"mean_after":round(sum(x["after"] for x in results)/5,2),"total_estimated_usd":str(total),"new_audit_estimated_usd":str(total-prior),"billing_verified":False,"candidate_promoted":False,"art_approved":False,"device_tested":False,"playmode_tested":False}
dump("summary.json",summary)
print("CYCLE_PRODUCTION_FINAL "+json.dumps(summary,ensure_ascii=False),flush=True)
