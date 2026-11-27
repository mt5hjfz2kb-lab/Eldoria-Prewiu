"""Independent artists diagnose the real Valoria I-V cycle. No game writes or image generation."""
import os,json,hashlib,base64,threading,sys,concurrent.futures
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]); out=root/"cycle-audit";out.mkdir(exist_ok=True)
hist=root/"historical";current=root/"current";source=root/"sources"
HIST="348b19f958d5bbd447db59e16fc30e7c554cc72a";HEAD="f27f4774e32efc5ad046478a07747eead4256c3e"
PRIOR=D("0.6889613"); CAP=D("5")
slots={**{"stage-"+str(i):D(".40") for i in range(1,6)},"cross-stage-director":D(".65")}
assert PRIOR+sum(slots.values())<=CAP
def dump(name,obj):
    (out/name).write_text(json.dumps(obj,indent=2,ensure_ascii=False),encoding="utf-8")
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def manifest(dir,expected,required):
    found=[]
    for p in dir.rglob("evidence.json"):
        try:
            e=json.loads(p.read_text(encoding="utf-8-sig"))
            if e.get("engine")=="Unity" and e.get("sourceSha")==expected and any(f.get("name")==required for f in e.get("frames",[])):
                found.append((p,e))
        except (ValueError,OSError):pass
    assert len(found)==1,("Missing/ambiguous Unity manifest",str(dir),expected,required,len(found))
    return found[0]
hm,he=manifest(hist,HIST,"stage-5-fixture-390x844")
cm,ce=manifest(current,HEAD,"home")
assert he["engineVersion"]==ce["engineVersion"]=="6000.3.23f1" and he["nativeWorldRender"] and ce["nativeWorldRender"]
def frame(m,e,name):
    a=[x for x in e["frames"] if x["name"]==name]
    assert len(a)==1,(name,"not unique")
    f=a[0];p=m.parent/f["file"]
    assert p.is_file() and sha(p)==f["sha256"],("frame invalid",name)
    with Image.open(p) as im:assert im.size==(f["width"],f["height"])
    return (name,p,{"source_sha":e["sourceSha"],"sha256":f["sha256"],"width":f["width"],"height":f["height"],"cameraX":f["cameraX"],"cameraY":f["cameraY"],"orthographicSize":f["orthographicSize"]})
def overview(level):
    if level==1:return frame(cm,ce,"retina")
    return frame(hm,he,"stage-"+str(level)+"-fixture-1170x2532")
def sheet(items,path,cols=2,w=600,h=690):
    rows=(len(items)+cols-1)//cols
    a=Image.new("RGB",(cols*w,rows*(h+32)),(29,30,32));d=ImageDraw.Draw(a)
    for k,(label,p,_) in enumerate(items):
        x=(k%cols)*w;y=(k//cols)*(h+32)
        with Image.open(p) as im:
            im=im.convert("RGB");im.thumbnail((w-12,h-6));a.paste(im,(x+(w-im.width)//2,y+32+(h-im.height)//2))
        d.text((x+7,y+8),label,fill="white")
    a.save(path,quality=94)
stage_images={}
index={}
for level in range(1,6):
    primary=overview(level)
    if level==1:
        details=[frame(cm,ce,n) for n in ("keep-close","mill-close","registered-entry")]
    else:
        details=[frame(hm,he,"stage-"+str(level)+"-keep-detail"),frame(hm,he,"stage-"+str(level)+"-registered-entry")]
        if level==5:details.extend(frame(hm,he,"stage-5-cerca-"+n) for n in ("postigo","oficios","peso","adarve","zapateros","gremio"))
    p=out/("stage-"+str(level)+"-overview.jpg")
    with Image.open(primary[1]) as im:
        im=im.convert("RGB");im.thumbnail((1000,1600));im.save(p,quality=95)
    q=out/("stage-"+str(level)+"-details.jpg")
    sheet(details,q,cols=2 if level!=5 else 3,w=560,h=560)
    stage_images[level]=(p,q)
    index[str(level)]={n:m for n,_,m in [primary]+details}
cross=out/"cycle-overview.jpg"
sheet([overview(i) for i in range(1,6)],cross,cols=5,w=340,h=750)
mill=out/"current-mill.jpg"
sheet([frame(cm,ce,"mill-close"),frame(cm,ce,"registered-entry")],mill,w=550,h=650)
dump("frame-index.json",index)
comparison=json.loads((source/"source-diff.json").read_text())
assert comparison["base_commit"]["sha"]==HIST and comparison["head_commit"]["sha"]==HEAD if "head_commit" in comparison else comparison["base_commit"]["sha"]==HIST
files=[f["filename"] for f in comparison["files"]]
city=[f for f in files if "/ValoriaModular25D/" in f and not f.endswith("Modular25DInterface.cs")]
assert len(city)==2 and all(f.endswith(("sawmill_i_hd.png","ValoriaUrbanPlan.cs")) for f in city),city
dump("scope.json",{"head":HEAD,"historical":HIST,"stage1_current":True,"stages2to5_historical":True,"city_diff":city,"hud_excluded":True,"prior_estimate_usd":str(PRIOR),"reserved_usd":str(sum(slots.values())),"game_writes":False})
print("CYCLE_NATIVE_INPUTS_VERIFIED",flush=True)
if sys.argv[1]=="prepare":sys.exit(0)
assert sys.argv[1]=="live" and os.environ.get("OPENAI_API_KEY")
key=os.environ["OPENAI_API_KEY"]
# Prepare mode has already verified immutable inputs. All six reservations are bounded.
ledger=[{"slot":slot,"reserved_usd":str(cap),"status":"reserved"} for slot,cap in slots.items()]
lock=threading.Lock()
def update(slot,**kw):
    with lock:
        row=next(r for r in ledger if r["slot"]==slot);row.update(kw);dump("ledger.json",ledger)
dump("ledger.json",ledger)
def ask(slot,instructions,brief,imgs,maxout):
    update(slot,status="in_flight");print("ARTIST_START "+slot,flush=True)
    content=[{"type":"input_text","text":brief}]
    for p in imgs:content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(p.read_bytes()).decode(),"detail":"high"})
    model="gpt-6.2-sol"
    raw=studio.request_api({"model":model,"instructions":instructions,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":maxout,"store":False},key)
    model=raw.get("model") or model
    cost=economics.estimate(model,raw["usage"],raw.get("service_tier") or "standard")
    update(slot,status="complete",estimated_usd=str(cost),model=model,usage=raw["usage"])
    assert cost<=slots[slot] and raw.get("status")=="completed"
    txt="\n".join(c["text"] for i in raw.get("output",[]) if i.get("type")=="message" for c in i.get("content",[]) if c.get("type")=="output_text").strip()
    (out/(slot+".txt")).write_text(txt,encoding="utf-8")
    if txt.startswith("```"):txt=txt.split("\n",1)[1].rsplit("```",1)[0]
    obj=json.loads(txt);dump(slot+".json",obj);print("ARTIST_DONE "+slot+" "+str(cost),flush=True);return obj
plan=(source/"ValoriaUrbanPlan.cs").read_text()
guide=(source/"PRODUCTION_GUIDE_BASTION_I_20261120.md").read_text()
facts="Actual source HEAD "+HEAD+". Stage I current native capture. Stages II-V historical native "+HIST+"; only city source changes since then are approved C sawmill sprite and worker position. Therefore old sawmill in historical II-V is superseded; do NOT evaluate it as current. Use exact stage-specific views, no HUD or commercial screenshots. Whiteout Survival and Call of Dragons are qualitative standards only, not direct evidence in this audit. Maintain canon: no new mechanics or buildings, empty growth land intentional; no flat city plate, keep independent sprites and projected shadows. Current code plan:\n"+plan[:18000]+"\nProduction guide:\n"+guide[:8500]
weights={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
roles={1:"senior environment artist specializing in early game settlements",2:"senior architecture and readable progression artist",3:"senior production district and materials artist",4:"senior urban composition and environmental storytelling artist",5:"senior city technical artist specializing in streets and multi-layer spatial integration"}
def inspect(level):
    brief=facts+"\nAudit ONLY stage "+str(level)+". Exact frame names and camera provenance: "+json.dumps(index[str(level)],ensure_ascii=False)
    instr="Act as independent "+roles[level]+". Spanish. Inspect both real screenshots, not concept art. Return one JSON object with keys stage (int), scores (resources,integration,coherence,composition,materials,readability,polish each numeric 0..10), strengths (array of strings), issues (array of 5-9 distinct objects each containing element, location, evidence (exact capture name), visible_defect, likely_cause, solution_2_5d, preserve, action (conservar/retocar/rehacer/reubicar), priority (P1/P2/P3), effort (bajo/medio/alto), expected_impact (qualitative), verification (specific native A/B test)), progression (string), limitations (array). Name real asset/prop only if evidence and code support it; otherwise describe visible location and state uncertainty. Be precise on implementation: silhouette, texture, palette, material, sprite foot/pivot, shadow, transition to mesh/road, sorting, placement, size, camera, occlusion. No fabricated tests or improvements, no generic recommendations. Distinguish independent asset quality and scene composition. Preserve the newly adopted C sawmill. Use only provided evidence. Priorities P1 highest visual impact, not fatal bug."
    a=ask("stage-"+str(level),instr,brief,stage_images[level],4300)
    assert a["stage"]==level and set(a["scores"])==set(weights) and all(type(v) in (float,int) and 0<=v<=10 for v in a["scores"].values())
    assert len(a["issues"])>=5
    for x in a["issues"]:
        assert all(k in x for k in ("element","location","evidence","visible_defect","likely_cause","solution_2_5d","preserve","action","priority","effort","expected_impact","verification"))
        assert x["priority"] in ("P1","P2","P3")
    a["weighted_score"]=round(sum(a["scores"][k]*w for k,w in weights.items())/100,2)
    dump("stage-"+str(level)+".json",a);return a
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as ex:
    results=list(ex.map(inspect,range(1,6)))
assert all(x["status"]=="complete" for x in ledger if x["slot"].startswith("stage-"))
brief="These are the five independent artist audits. Review consistency against the actual cycle overview and current mill. Do not claim the historical stages are exact current HEAD. Aggregate distinct defects, preserve canon, no new mechanics. "+json.dumps(results,ensure_ascii=False)[:80000]
director=ask("cross-stage-director","Independent senior art director. In Spanish, return JSON with keys executive_summary (string), top_priorities (array of 6-10 objects with element, stages, evidence, visible_defect, concrete_solution, preserve, priority, effort, impact_hypothesis, native_validation), production_batches (array of objects with title, scope, dependencies, validation, expected_impact), keep_unchanged (array), disagreements (array), limitations (array). Rank by actual visual impact and deduplicate repeated issues. Identify specific named assets and scene problems. Distinguish changing resource versus placement/terrain integration; don't invent features or guarantee scores. Keep approved C sawmill, propose local integration only. Make recommendations achievable with original modular 2.5D, not full scene illustration. Evaluate stages as progression, not five unrelated villages.",brief,[cross,mill],4700)
total=PRIOR+sum(D(r["estimated_usd"]) for r in ledger);assert total<=CAP
summary={"stage_scores":{str(a["stage"]):a["weighted_score"] for a in results},"stages":results,"director":director,"estimated_prior_usd":str(PRIOR),"estimated_new_usd":str(total-PRIOR),"estimated_total_usd":str(total),"billing_verified":False,"source_head":HEAD,"historical_source":HIST,"implemented":False,"art_approved":False,"physical_device":False}
dump("summary.json",summary)
lines=["# Valoria — auditoría artística Bastión I–V","",director["executive_summary"],"",f"HEAD: {HEAD}; II–V: evidencia histórica {HIST}.","", "## Evaluación por etapa"]
for a in results:
    lines.extend(["",f"### Bastión {a['stage']} — {a['weighted_score']}/10","", "Fortalezas: "+"; ".join(a["strengths"]), "",a["progression"]])
    for x in a["issues"]:
        lines.extend(["",f"**{x['priority']} · {x['element']} — {x['location']}**",f"- Evidencia: {x['evidence']}",f"- Defecto: {x['visible_defect']}",f"- Causa plausible: {x['likely_cause']}",f"- Solución: {x['solution_2_5d']}",f"- Conservar: {x['preserve']}",f"- Acción: {x['action']}; esfuerzo {x['effort']}; impacto: {x['expected_impact']}",f"- Verificación: {x['verification']}"])
    lines.extend(["","Limitaciones: "+"; ".join(a["limitations"])])
lines.extend(["","## Prioridades consolidadas"])
for x in director["top_priorities"]:
    lines.extend(["",f"**{x['priority']} · {x['element']} ({x['stages']})**",f"- Evidencia: {x['evidence']}",f"- Problema: {x['visible_defect']}",f"- Solución: {x['concrete_solution']}",f"- Conservar: {x['preserve']}",f"- Esfuerzo: {x['effort']}. Impacto estimado: {x['impact_hypothesis']}",f"- Prueba: {x['native_validation']}"])
lines.extend(["","## Plan de producción"])
for x in director["production_batches"]:lines.extend(["",f"**{x['title']}**",f"- Alcance: {x['scope']}",f"- Dependencias: {x['dependencies']}",f"- Prueba: {x['validation']}",f"- Impacto: {x['expected_impact']}"])
for k,t in [("keep_unchanged","Conservar"),("disagreements","Discrepancias"),("limitations","Limitaciones")]:
    lines.extend(["","## "+t]+["- "+x for x in director[k]])
lines.extend(["",f"Coste estimado nuevo: USD {total-PRIOR}. Acumulado: USD {total}. Factura no conciliada. Sin cambios de juego ni aprobación artística."])
(out/"AUDITORIA_BASTION_I_V.md").write_text("\n".join(lines),encoding="utf-8")
print("CYCLE_AUDIT_COMPLETED "+json.dumps({"scores":summary["stage_scores"],"new_usd":summary["estimated_new_usd"],"total_usd":summary["estimated_total_usd"],"issues":sum(len(a["issues"]) for a in results),"priorities":len(director["top_priorities"])},ensure_ascii=False),flush=True)
