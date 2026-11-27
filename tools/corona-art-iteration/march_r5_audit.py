"""R5 independent paired scores versus the exact validated R3 player; receipt-bound budget."""
import json,os,hashlib,sys,math,subprocess,base64
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
WEIGHTS=[20,20,15,15,10,10,10]
DIMS=["resources","integration","language","composition","light_material","mobile","finish"]
def read(p):return json.loads(Path(p).read_text(encoding="utf-8"))
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def need(ok,msg):
    if not ok:raise ValueError(msg)
def total(v):
    need(isinstance(v,list) and len(v)==7 and all(type(x) in (int,float) and math.isfinite(x) and 0<=x<=10 for x in v),"Invalid original-weight subscores")
    return round(float(sum(D(str(x))*w for x,w in zip(v,WEIGHTS))/D(100)),2)
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head="f4cba2d776f012c4e108e938c1f37e130a502d3a";run=os.environ["GITHUB_RUN_ID"]
    need(os.environ["GITHUB_RUN_ATTEMPT"]=="1","No paid retries")
    need(gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-r5-scene-hierarchy-20261127")["object"]["sha"]==head,"Wrong R5 source branch")
    need(gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-r5-independent-proof-20261127")["object"]["sha"]==os.environ["GITHUB_SHA"],"Wrong audit branch")
    need(gh(f"repos/{repo}/actions/runs/{run}")["head_sha"]==os.environ["GITHUB_SHA"],"Wrong harness run")
    need(gh(f"repos/{repo}/actions/runs/45900917700")["conclusion"]=="success","R3 proof unavailable")
    source_run=gh(f"repos/{repo}/actions/runs/45957940610")
    need(source_run["head_sha"]==head and source_run["run_attempt"]==1 and source_run["conclusion"]=="success","R5 exact proof incomplete")
    need(next(j for j in gh(f"repos/{repo}/actions/runs/45957940610/jobs")["jobs"] if j["name"]=="native-march")["conclusion"]=="success","Native incomplete")
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    need(lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18,"Studio lease changed")
    from verify_march_r5_budget import verify_budget
    budget=verify_budget(repo,gh)
    e=read("native/evidence.json")
    need(e["sourceSha"]==head and e["harnessSha"]==head and e["uiOnlyNativeCamera"] and e["actualPrefab"] and e["actualGateway"] and len(e["frames"])==6,"Invalid native provenance")
    need(e["gallerySha"]=="9ae5396ad36dcb5053967bad6c463b44abfc9df55ea7125e46bdf7c8d338a263" and e["contactSha"]=="4c7378411b261257c70c2e10d9d10fc820b3c0a0a15cc64cfaa57028ac6bd8bb","Wrong integrated asset hash")
    need(not e["defects"],"R5 has native defects; no new paid review")
    old=read("r3/evidence.json")
    need(old["sourceSha"]=="0aa89f7dda367ba3afc3a666a8c0574198ad9c8f" and old["uiOnlyNativeCamera"] and not old["defects"],"Wrong exact R3 comparison")
    oldrows={f["file"]:f for f in old["frames"]}
    rows={f["file"]:f for f in e["frames"]}
    for name,f in oldrows.items():
        need(hashlib.sha256((Path("r3")/name).read_bytes()).hexdigest()==f["sha256"],"Invalid original R3 frame")
    for name,f in rows.items():
        p=Path("native")/name
        need(hashlib.sha256(p.read_bytes()).hexdigest()==f["sha256"] and f["actualCamera"] and f["playMode"],"Invalid source frame")
        with Image.open(p) as im:need(im.size==(f["width"],f["height"]),"Wrong dimensions")
    for role in ("t1","mixed","landscape"):
        a,b=oldrows[role+"-after.png"],rows[role+"-after.png"]
        for field in ("t1","t2","selectedQuantity","selectedTier","selectedHero","metrics","width","height","revision"):
            need(a[field]==b[field],"Different state "+role+"/"+field)
    need(oldrows["landscape-after.png"]["sha256"]==rows["landscape-after.png"]["sha256"],"R5 landscape regression; no new paid review")
    out=Path("march-r5-audit");out.mkdir(exist_ok=False)
    pairs=Image.new("RGB",(792,1740),"#172028");d=ImageDraw.Draw(pairs)
    for j,role in enumerate(("t1","mixed")):
        for x,v in ((0,"before"),(402,"after")):
            frame=("r3/" if v=="before" else "native/")+role+"-after.png"
            with Image.open(frame) as im:pairs.paste(im.convert("RGB"),(x,j*870+26))
            d.text((x+5,j*870+5),role+" / "+("R3 actual" if v=="before" else "R5 actual"),fill="white")
    pairs.save(out/"actual-pairs.jpg",quality=94,subsampling=0)
    ref=Image.open("refs/reference.jpg").convert("RGB");ref.thumbnail((1200,1800));ref.save(out/"references.jpg",quality=92)
    write(out/"provenance.json",{"source":head,"run":int(run),"frames":rows,"native_defects":e["defects"],"landscape_identical":oldrows["landscape-after.png"]["sha256"]==rows["landscape-after.png"]["sha256"],"weights":dict(zip(DIMS,WEIGHTS)),"physical_iphone":False,"full_browser":False,"no_automatic_merge":True})
    write(out/"budget.json",budget)
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import studio,mission
    original=studio.make_payload
    def high(*args,**kwargs):
        p,m=original(*args,**kwargs)
        for item in p["input"][0]["content"]:
            if item["type"]=="input_image":item["detail"]="high"
        return p,m
    studio.make_payload=high;studio.MAX_OUTPUT=2200
    mission.RESERVES["auditor"]=D(".12")
    prompt="""Auditor artístico INDEPENDIENTE, no has participado en el diseño. Imagen 1: capturas reales Unity de la misma pantalla Marchas, R3 validada a la izquierda, R5 candidata a la derecha; filas T1 y mixto. Evalúa si la composición de comandante protagonista, los dos expedientes secundarios, la arquera T1/T2 a mayor escala y el resumen compacto aportan integración y lectura real. No presupongas mejora. Analiza si los elementos parecen un escenario integrado o un collage; verifica legibilidad, definición de celdas, silueta y estados. Imagen 2: referentes comerciales. Primero comprueba si existe contaminación delante de la UI, artefactos graves o zonas ocultas que impidan puntuar; si ocurre, valid_visual=false y los tres vectores de notas null. No deduzcas mejora de tests ni apruebes por ser un nuevo diseño. Si son válidas, usa exactamente la rúbrica original 24/11: recursos 20%, integración espacial 20%, lenguaje visual 15%, composición/jerarquía 15%, iluminación/color/materiales 10%, nitidez/legibilidad móvil 10%, pulido/animación/presentación 10% (sólo presentación estática, motion no probado). Puntúa antes, después y referente comparable en siete valores 0–10 en ese orden, sin forzar mejora ni conocer notas anteriores. Si no hay referencia comparable, reference=null. 9–10 primer nivel; 8–8.9 competitivo; 7–7.9 bueno; 6–6.9 prototipo avanzado; 5–5.9 insuficiente. Evalúa recursos reales, retratos, profundidad, jerarquía, legibilidad y CTA. Señala explícitamente cualquier regresión o función visualmente ilegible. JSON exacto {valid_visual:boolean,before:[7 números]|null,after:[7 números]|null,reference:[7 números]|null,reference_label:string,observation:string,remaining:string}. No afirmes iPhone físico, animación ni que la prueba de clicks demuestra jugabilidad completa."""
    evidence=mission.textfile(out/"evidence.txt",json.dumps({"source":head,"rubric":dict(zip(DIMS,WEIGHTS)),"states":"same T1 and mixed, isolated scratch native capture","technical_defects":e["defects"],"note":"No previous artistic scores. Physical phone and full WebGL untested."},ensure_ascii=False))
    results=[]
    for i in range(2):
        folder=out/("review-"+str(i+1));folder.mkdir()
        ledger=mission.Ledger(folder/"ledger.json",D(".12"))
        raw=mission.ask("visual","gpt-6.1-terra",evidence,(str(out/"actual-pairs.jpg"),str(out/"references.jpg")),ledger,"auditor",folder,prompt,effort="none")
        r=json.loads(raw.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
        need(set(r)=={"valid_visual","before","after","reference","reference_label","observation","remaining"} and type(r["valid_visual"]) is bool,"Bad visual response")
        if r["valid_visual"]:
            for k in ("before","after","reference"):r[k+"_total"]=None if r[k] is None else total(r[k])
            need(r["before_total"] is not None and r["after_total"] is not None,"Missing scores")
            r["delta"]=round(r["after_total"]-r["before_total"],2)
        else:
            need(all(r[k] is None for k in ("before","after","reference")),"Contaminated evidence cannot be scored")
            r["delta"]=None
        r["estimated_usd"]=ledger.rows[0]["estimated_usd"];write(folder/"scores.json",r);results.append(r)
    valid=all(r["valid_visual"] for r in results)
    mean=lambda k:round(sum(r[k] for r in results)/len(results),2)
    final={"source":head,"run":int(run),"reviews":results,"valid_visual":valid,"before":mean("before_total") if valid else None,"after":mean("after_total") if valid else None,"mean_paired_delta":mean("delta") if valid else None,"native_defects":e["defects"],"landscape_identical":oldrows["landscape-after.png"]["sha256"]==rows["landscape-after.png"]["sha256"],"motion_evaluated":False,"physical_iphone":False,"full_browser_journey":False,"estimated_usd":str(sum(D(r["estimated_usd"]) for r in results)),"invoice_verified":False,"visual_approval":False}
    references=[r["reference_total"] for r in results if r["valid_visual"] and r["reference_total"] is not None]
    final["reference_mean"]=round(sum(references)/len(references),2) if len(references)==2 else None
    final["artistic_target_met"]=valid and final["after"]>=8 and final["mean_paired_delta"]>0 and final["reference_mean"] is not None and final["reference_mean"]-final["after"]<=.5
    final["native_target_met"]=not e["defects"] and final["landscape_identical"]
    final["previous_source"]="0aa89f7dda367ba3afc3a666a8c0574198ad9c8f"
    write(out/"scorecard.json",final);print("MARCH_R5_SCORECARD",json.dumps(final,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
