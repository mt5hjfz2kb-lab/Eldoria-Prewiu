"""Original 24-Nov rubric; source-pinned rendered pairs; seven bounded independent calls."""
import argparse, hashlib, json, os, subprocess, sys, base64, math
from decimal import Decimal as D
from pathlib import Path
from PIL import Image, ImageDraw, ImageChops
BASE="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
UI="3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c"
WORLD="ff38763376006ede276c875e498a8557d07509b3"
WEIGHTS=[20,20,15,15,10,10,10]
DIMENSIONS=["resources","integration","language","composition","light_material","mobile","finish"]
GROUPS={
"hud":["hud-b1","hud-b5","tutorial","construction"],
"management":["march","bag","mail","heroes","training"],
"narrative":["name","daily","scouts","vigilants"],
"auxiliary":["council-b1","council-b2","sawmill-built","bastion-upgrade","barracks-plot"],
"city":["bastion-1","bastion-2","bastion-3","bastion-4","bastion-5","bastion-5-panorama"],
"world-close":["home","wood","stone","woodland","river"],
"world-strategic":["region","breach","panorama","breach-panorama"]}
UI_FILES={"hud-b1":"bastion-1","hud-b5":"bastion-5-completo","tutorial":"guia-iniciar-obra","construction":"temporizador-en-parcela","name":"nombre-inicial"}
HIST={"hud-b1":7.4,"march":7.4,"bag":7.7,"mail":7.4,"name":7.5,"daily":7.8,"heroes":8,"training":8,"scouts":8.3,"vigilants":8.2}
def read(p):return json.loads(p.read_text(encoding="utf-8"))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def im(p):
    with Image.open(p) as x:
        if x.size!=(390,844):raise ValueError("Wrong screen size "+str(p))
        return x.convert("RGB")
def same(a,b):return ImageChops.difference(a,b).getbbox() is None
def pair_file(p,left,right):
    with Image.open(p) as x:
        x=x.convert("RGB")
        if x.size!=(792,870) or not same(left,x.crop((0,26,390,870))) or not same(right,x.crop((402,26,792,870))):raise ValueError("Mismatch pair "+str(p))
def total(v):
    if not isinstance(v,list) or len(v)!=7 or any(type(x) not in (int,float) or not math.isfinite(x) or not 0<=x<=10 for x in v):raise ValueError("Seven numeric grades required")
    return float(sum(D(str(x))*w for x,w in zip(v,WEIGHTS))/D(100))
def parse(s):
    s=s.strip()
    if s.startswith("\x60\x60\x60json") and s.endswith("\x60\x60\x60"):s=s[7:-3].strip()
    return json.loads(s)
def score_rows(raw,keys):
    if set(raw)!={"screens","overall"} or set(raw["screens"])!=set(keys):raise ValueError("Incomplete group")
    for k,r in raw["screens"].items():
        if set(r)!={"before","after","reference","reference_kind","reference_label","evidence","remaining"}:raise ValueError("Bad row "+k)
        if r["reference_kind"] not in ("direct","analog","none"):raise ValueError("Bad reference kind")
        if (r["reference_kind"]=="none")!=(r["reference"] is None):raise ValueError("Unproven reference")
        if not all(isinstance(r[x],str) and r[x].strip() for x in ["evidence","remaining"]):raise ValueError("Missing visual rationale")
        if not isinstance(r["reference_label"],str) or (r["reference_kind"]!="none" and not r["reference_label"].strip()):raise ValueError("Missing comparable reference label")
        for v in ["before","after","reference"]:
            r[v+"_total"]=None if r[v] is None else round(total(r[v]),2)
        r["delta"]=None if r["before_total"] is None or r["after_total"] is None else round(r["after_total"]-r["before_total"],2)
        r["target_met"]=r["after_total"] is not None and r["after_total"] >= (8.5 if k.startswith("hud") else 8)
        r["motion_evaluated"]=False
    return raw
def build(a):
    pairs={}; provenance={}
    m=read(a.ui/"manifest.json")
    assert m["baseline_sha"]==BASE and m["candidate_sha"]==UI and m["dpr"]==1 and not m["missing"]
    for r in m["pairs"]:
        k=r["role"]; n=UI_FILES.get(k,"sistema-"+k)+".png"
        l=a.baseline/n; q=a.candidate/n
        assert sha(l)==r["baseline_sha256"] and sha(q)==r["candidate_sha256"]
        pairs[k]=(im(l),im(q));pair_file(a.ui/r["file"],*pairs[k]);provenance[k]={"before_sha":BASE,"after_sha":UI,"source":"matched browser progression","before_png":sha(l),"after_png":sha(q)}
    m=read(a.aux/"manifest.json")
    assert [x["source_sha"] for x in m["source_runs"]]==[BASE,UI]
    for k in GROUPS["auxiliary"]:
        l=a.aux_base/("extra-"+k+".png");q=a.aux_after/l.name
        assert sha(l)==m["source_runs"][0]["frames"][k] and sha(q)==m["source_runs"][1]["frames"][k]
        pairs[k]=(im(l),im(q));pair_file(a.aux/(k+".png"),*pairs[k]);provenance[k]={"before_sha":BASE,"after_sha":UI,"source":"matched browser progression","before_png":sha(l),"after_png":sha(q)}
    for k in GROUPS["city"]:
        n=("bastion-5-completo" if k=="bastion-5" else k)+".png";l=a.baseline/n;q=a.candidate/n
        pairs[k]=(im(l),im(q));provenance[k]={"before_sha":BASE,"after_sha":UI,"source":"matched browser progression, landscape art only","before_png":sha(l),"after_png":sha(q)}
    assert sha(a.native/"before/routes.txt")==sha(a.native/"after/routes.txt") and sha(a.native/"before/grid.bin")==sha(a.native/"after/grid.bin")
    for k in GROUPS["world-close"]+GROUPS["world-strategic"]:
        l=a.native/"after"/("colour-original-"+k+".png");q=a.native/"after"/("colour-candidate-"+k+".png")
        x,y=read(l.with_suffix(".json")),read(q.with_suffix(".json"))
        for p,d in ((l,x),(q,y)):assert sha(p)==d["sha256"] and d["actualUnityCamera"] and d["playMode"] and (d["width"],d["height"])==(390,844)
        for f in ("cameraPosition","cameraRotation","worldTarget","zoom","aspect"):assert x[f]==y[f]
        pairs[k]=(im(l),im(q));provenance[k]={"before_sha":WORLD,"after_sha":WORLD,"source":"same native Unity frame/camera, palette off/on; not a complete revision A/B","before_png":sha(l),"after_png":sha(q)}
    assert set(pairs)=={x for v in GROUPS.values() for x in v} and len(pairs)==33
    return pairs,provenance
def montage(pairs,keys,p,limit):
    sheet=Image.new("RGB",(792*2,870*((len(keys)+1)//2)),(20,27,32));d=ImageDraw.Draw(sheet)
    for i,k in enumerate(keys):
        x=i%2*792;y=i//2*870
        sheet.paste(pairs[k][0],(x,y+26));sheet.paste(pairs[k][1],(x+402,y+26))
        d.text((x+6,y+6),"ANTES / "+k,fill="white");d.text((x+408,y+6),"DESPUES / "+k,fill="white")
    for quality in (93,88,80,70):
        sheet.save(p,quality=quality,subsampling=0,optimize=True)
        if p.stat().st_size<limit:return
    raise ValueError("Oversized montage")
def preflight():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    assert os.environ["GITHUB_RUN_ATTEMPT"]=="1"
    assert gh(f"repos/{repo}/git/ref/heads/candidate/corona-calibrated-rubric-20261126")["object"]["sha"]==head
    assert gh(f"repos/{repo}/git/ref/heads/pilot/eldoria-openai-development-studio-20261125")["object"]["sha"]=="1657b2caa6b1a955319538ce3a8b2906398599b3"
    for branch,head2,run,src in [
        ("candidate/corona-auxiliary-modals-20261126","4cf80d439bba248b0e0125f4cd42798602d43d7d",45824434222,UI),
        ("candidate/corona-world-art-completion-20261126","fa7833bfe07f328b7c24931b6f6f52316698130e",45834241279,WORLD)]:
        assert gh(f"repos/{repo}/git/ref/heads/{branch}")["object"]["sha"]==head2
        assert gh(f"repos/{repo}/actions/runs/{run}")["head_sha"]==src
        jobs=gh(f"repos/{repo}/actions/runs/{run}/jobs")["jobs"]
        # Both old paid jobs had not been created AFTER ref movement, as recorded.
        # Their first source-ref assertion is now false. Fail closed on any old paid receipt.
        for j in jobs:
            if j["name"] in ("final-art-audit","spatial-art-audit"):
                if j["conclusion"]=="success":raise ValueError("Old paid audit unexpectedly succeeded")
        artifacts=gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"]
        assert not any(x["name"] in ("corona-final-independent-visual-review","corona-final-independent-spatial-review") for x in artifacts)
    for run,src,artifact in [(45638738009,BASE,15037184906),(45824434222,UI,15125483003),(45824434222,UI,15125587841),(45834241279,WORLD,15127316953),(45837717094,"5cb21e2a403c35cd6f5df10a70e8e82171ac8851",15126077699),(45837717094,"5cb21e2a403c35cd6f5df10a70e8e82171ac8851",15127267038),(45837717094,"5cb21e2a403c35cd6f5df10a70e8e82171ac8851",15125344049),(45812100129,"3cdbc481b2e5d65a38bede3c3c560f56c4d1f622",15113766981)]:
        assert gh(f"repos/{repo}/actions/runs/{run}")["head_sha"]==src
        assert any(x["id"]==artifact for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"])
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    registry=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in registry["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    assert lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18
    assert D("2.90")+D("1.25")+D(".42")+D(".43")==D("5.00")
def main(a):
    preflight()
    repo=os.environ["GITHUB_REPOSITORY"]
    old=gh(f"repos/{repo}/actions/runs/45843367417");assert old["head_sha"]=="82d220f3a2dd47e65ea7f60a8832ab311ec6eb64" and old["status"]=="completed" and old["conclusion"]=="failure"
    assert any(x["id"]==15128947585 for x in gh(f"repos/{repo}/actions/runs/45843367417/artifacts")["artifacts"])
    rubric=read(a.rubric);assert [d[0] for d in rubric["dimensions"]]==DIMENSIONS and [d[1] for d in rubric["dimensions"]]==WEIGHTS
    refs=read(a.references/"reference-manifest.json")
    assert any(x["label"].startswith("WOS") and "sha256" in x for x in refs) and any(x["label"].startswith("COD") and "sha256" in x for x in refs)
    pairs,provenance=build(a)
    a.out.mkdir(exist_ok=False);write(a.out/"provenance.json",provenance)
    write(a.out/"budget.json",{"owner_cap":"5.00","previous_held":"4.15","superseded_unstarted":".85","current_reserved":".42","remaining_for_final":".43","no_retry":True,"invoice_verified":False})
    sys.path.insert(0,str(a.studio.resolve()));import studio,mission
    original=studio.make_payload
    def high(*args,**kwargs):
        p,m=original(*args,**kwargs)
        for item in p["input"][0]["content"]:
            if item["type"]=="input_image":item["detail"]="high"
        return p,m
    studio.make_payload=high;studio.MAX_OUTPUT=3600;mission.RESERVES["auditor"]=D(".06")
    prepared={}
    for g,keys in GROUPS.items():
        folder=a.out/g;folder.mkdir();p=folder/"actual-ab.jpg";montage(pairs,keys,p,studio.MAX_IMAGE_BYTES)
        info={"roles":keys,"rubric":rubric["dimensions"],"bands":rubric["bands"],"scope":"city art excluding HUD" if g=="city" else "world colour-only same-camera comparison" if g.startswith("world") else "visible UI including actual background integration","provenance":{k:provenance[k] for k in keys},"references":[{k:x[k] for k in ("label","scope","kind") if k in x} for x in refs],"animation":"static images only; assess visible presentation, mark motion untested"}
        evidence=mission.textfile(folder/"evidence.json",json.dumps(info,ensure_ascii=False))
        prepared[g]=(folder,keys,p,evidence)
    results={};cost=D(0)
    # Recover the completed first call exactly; never pay for it twice.
    prior=read(a.resume/"hud/auditor.json");oldledger=read(a.resume/"hud/ledger.json")
    assert oldledger["cap_usd"]=="0.06" and len(oldledger["reservations"])==1 and oldledger["reservations"][0]["state"]=="completed"
    assert D(oldledger["reservations"][0]["estimated_usd"])==D(prior["cost_estimate_usd"])==D("0.0151455")
    assert read(a.resume/"provenance.json")==provenance
    folder,keys,p,evidence=prepared["hud"]
    assert sha(a.resume/"hud/actual-ab.jpg")==sha(p)
    result=score_rows(parse(prior["response"]),keys)
    write(folder/"auditor.json",prior);write(folder/"ledger.json",oldledger);write(folder/"scores.json",result)
    results.update(result["screens"]);cost+=D(prior["cost_estimate_usd"])
    write(a.out/"partial-scorecard.json",{"rubric_id":rubric["id"],"screens":results,"estimated_usd":str(cost),"complete":False})
    for g,(folder,keys,p,evidence) in prepared.items():
        if g=="hud":continue
        ledger=mission.Ledger(folder/"ledger.json",D(".06"))
        instructions="Auditor artístico independiente de Eldoria. Aplica EXACTAMENTE la rúbrica recuperada del 24/11: recursos 20%, integración 20%, lenguaje 15%, composición 15%, iluminación/color/materiales 10%, nitidez móvil 10%, pulido/animaciones/presentación 10%. Escala 9-10 comercial primer nivel, 8-8.9 competitivo, 7-7.9 bueno mejorable, 6-6.9 prototipo avanzado, 5-5.9 insuficiente. No penalices técnica 2.5D frente a 3D. Imagen 1 pares reales ANTES izquierda / DESPUÉS derecha. Imagen 2 referentes WOS/COD reales; compara con misma rúbrica, si no hay equivalente directo marca analog, o none y null (reference_label puede ser vacío sólo cuando none); no inventes referencia para mundo o paneles. En HUD juzga interfaz y su integración, no cantidad de edificios; en ciudad excluye HUD; en mundo juzga arte de la escena. No infieras animación: sé explícito que finish es sólo presentación visible. Puntúa 7 componentes cada uno 0-10, mismo criterio en antes/después/referente, sin sesgo de cambio ni obligación de subir. Se aceptan decimales, null para una vista no evaluable. Devuelve JSON puro {screens:{id:{before:[7 números]|null,after:[7 números]|null,reference:[7 números]|null,reference_kind:'direct'|'analog'|'none',reference_label:string,evidence:string,remaining:string}},overall:string}. Justifica cambios concretos, incluso ausencia de cambios y defectos. Todos los ids: "+",".join(keys)
        answer=mission.ask("visual","gpt-6.1-terra",evidence,(str(p),str(a.references/"reference.jpg")),ledger,"auditor",folder,instructions,effort="none")
        r=score_rows(parse(answer),keys);write(folder/"scores.json",r)
        results.update(r["screens"]);cost+=D(ledger.rows[0]["estimated_usd"])
        write(a.out/"partial-scorecard.json",{"rubric_id":rubric["id"],"screens":results,"estimated_usd":str(cost),"complete":False})
    # Only after blind scores are frozen, expose old totals to calibrate drift.
    for k,r in results.items():
        r["historical_total"]=HIST.get(k)
        r["historical_baseline_drift"]=None if k not in HIST or r["before_total"] is None else round(r["before_total"]-HIST[k],2)
        r["provenance"]=provenance[k]
    report={"rubric_id":rubric["id"],"weights":dict(zip(DIMENSIONS,WEIGHTS)),"screens":results,"estimated_usd":str(cost),"complete":True,"motion_evaluated":False,"physical_iphone":"NOT_TESTED","visual_approval":False,"scope":"33 rendered roles, mixed revision scopes explicitly indicated; not the new depth candidate"}
    write(a.out/"calibrated-scorecard.json",report);print("CALIBRATED_SCORECARD",json.dumps(report,ensure_ascii=False),flush=True)
if __name__=="__main__":
    p=argparse.ArgumentParser()
    for k in ("ui","baseline","candidate","aux","aux-base","aux-after","native","references","rubric","studio","out","resume"):p.add_argument("--"+k,type=Path,required=True)
    main(p.parse_args())
