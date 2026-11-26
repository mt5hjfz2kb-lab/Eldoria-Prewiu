"""Two genuinely independent blinded visual A/B judgments on real native room frames."""
import json,os,sys,hashlib,subprocess,base64,math,statistics
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
SOURCE=os.environ["GITHUB_SHA"]
BRANCH="candidate/corona-march-studio-r2-20261126"
WEIGHTS=[20,20,15,15,10,10,10]
DIMENSIONS=["resources","integration","language","composition","light_material","mobile","finish"]
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=40))
def read(p):return json.loads(Path(p).read_text())
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def parse(s):
    s=s.strip()
    if s.startswith("\x60\x60\x60json") and s.endswith("\x60\x60\x60"):s=s[7:-3].strip()
    return json.loads(s)
def total(v):
    assert isinstance(v,list) and len(v)==7 and all(type(x) in (int,float) and math.isfinite(x) and 0<=x<=10 for x in v)
    return float(sum(D(str(x))*w for x,w in zip(v,WEIGHTS))/D(100))
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    assert os.environ["GITHUB_RUN_ATTEMPT"]=="1"
    assert gh(f"repos/{repo}/git/ref/heads/{BRANCH}")["object"]["sha"]==head
    assert gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-high-impact-studio-20261126")["object"]["sha"]=="0a31a5f5f00a7e4d3ec583d07c7c78adaeb6e643"
    assert gh(f"repos/{repo}/git/ref/heads/experiment/valoria-ciudad-region-unificada-20261121")["object"]["sha"]=="91b8de9874aed87219d733efa6e8e0aa56207f1e"
    run=os.environ["GITHUB_RUN_ID"]
    info=gh(f"repos/{repo}/actions/runs/{run}");assert info["head_sha"]==SOURCE
    jobs=gh(f"repos/{repo}/actions/runs/{run}/jobs")["jobs"]
    assert next(j for j in jobs if j["name"]=="native-march")["conclusion"]=="success"
    assert any(x["name"]=="march-studio-native-evidence" for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"])
    # The previous .30 reservation was superseded before paid requests.
    old=gh(f"repos/{repo}/actions/runs/45867301712");assert old["status"]=="completed" and old["conclusion"]=="failure"
    assert gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-studio-native-review-20261126")["object"]["sha"]=="e5eb810ac5de2496f86761807cd9e8903d06da8d"
    assert not any(x["name"]=="corona-march-studio-blind-scorecard" for x in gh(f"repos/{repo}/actions/runs/45867301712/artifacts")["artifacts"])
    asset=read("artist/artist.json");assert asset["image_sha256"]=="a42b20c2ca7bd57a8b09d95a7442ccc81bb33c9f23f2e1869112b10e4071cab7"
    assert read("artist/review.json")["verdict"]=="accept"
    ledger=read("artist/ledger.json");assert len(ledger["reservations"])==2 and all(x["state"]=="completed" for x in ledger["reservations"])
    assert D("5.00")+D(".87")+D(".30")+D(".83")==D("7.00")
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    assert lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18
    native=read("native/evidence.json");assert native["sourceSha"]==SOURCE and native["actualPrefab"] and native["actualGateway"] and native["engineVersion"]=="6000.3.23f1" and not native["artApproved"]
    assert len(native["frames"])==6
    frames={}
    for f in native["frames"]:
        p=Path("native")/f["file"];assert hashlib.sha256(p.read_bytes()).hexdigest()==f["sha256"] and f["actualCamera"] and f["playMode"]
        with Image.open(p) as im:
            assert im.size==(f["width"],f["height"])
            frames[f["file"]]=im.convert("RGB")
    for fixture in ("t1","mixed","landscape"):
        a,b=[next(x for x in native["frames"] if x["file"]==fixture+"-"+side+".png") for side in ("before","after")]
        for k in ("t1","t2","selectedHero","revision","width","height","selectedQuantity","selectedTier","metrics"):assert a[k]==b[k],(fixture,k)
    out=Path("review");out.mkdir(exist_ok=False)
    write(out/"native-provenance.json",native)
    write(out/"budget.json",{"previous_cap":"5.00","additional_cap":"2.00","artist_held":".87","review_held":".30","unallocated":".83","no_auto_retry":True,"invoice_verified":False})
    refs=read("refs/reference-manifest.json");valid=[x for x in refs if "sha256" in x];idx=next(i for i,x in enumerate(valid) if x["label"]=="WOS heroes/march")
    with Image.open("refs/reference.jpg") as im:
        x=idx%2*620;y=idx//2*632
        im.crop((x,y,x+620,y+632)).convert("RGB").save(out/"reference-march.jpg",quality=93)
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()));import mission,studio
    studio.MAX_OUTPUT=2600;mission.RESERVES["auditor"]=D(".15")
    old_make=studio.make_payload
    def high(*a,**kw):
        p,m=old_make(*a,**kw)
        for i in p["input"][0]["content"]:
            if i["type"]=="input_image":i["detail"]="high"
        return p,m
    studio.make_payload=high
    records=[];cost=D(0)
    for idx,model in enumerate(("gpt-6.1-terra","gpt-6.2-sol")):
        folder=out/("reviewer-"+str(idx+1));folder.mkdir()
        order=("before","after") if idx==0 else ("after","before")
        canvas=Image.new("RGB",(792,1740),(23,32,40));d=ImageDraw.Draw(canvas)
        for j,fixture in enumerate(("t1","mixed")):
            y=j*870
            for x,side,label in ((0,order[0],"A"),(402,order[1],"B")):
                canvas.paste(frames[fixture+"-"+side+".png"],(x,y+26))
                d.text((x+6,y+5),fixture.upper()+" / "+label,fill="white")
        montage=folder/"blinded-pairs.jpg"
        for q in (94,90,85,78):
            canvas.save(montage,quality=q,subsampling=0,optimize=True)
            if montage.stat().st_size<studio.MAX_IMAGE_BYTES:break
        assert montage.stat().st_size<studio.MAX_IMAGE_BYTES
        info=mission.textfile(folder/"instructions-evidence.txt",json.dumps({"fixtures":["t1","mixed"],"same_fixture_per_pair":True,"original_rubric":dict(zip(DIMENSIONS,WEIGHTS)),"reference":"Whiteout Survival heroes/march screenshot, analogous context; real third-party image, different date/state","motion":"not measured; finish includes only visible static presentation"},ensure_ascii=False))
        prompt="Eres auditor artístico senior independiente de un juego móvil de estrategia. Evalúa a ciegas los pares A/B auténticos de la misma pantalla de preparación de marchas; no se te dice cuál es la candidata. Imagen 1: dos pares nativos 390×844 (T1 y MIXED). Imagen 2: referente comercial real WOS. Puntúa exactamente siete dimensiones de 0 a 10: recursos gráficos 20%, integración espacial/material de la UI 20%, coherencia del lenguaje visual 15%, composición y jerarquía 15%, iluminación/color/materiales 10%, nitidez y legibilidad móvil 10%, pulido/presentación 10% (estático; no evalúes animación). 9-10 primer nivel comercial, 8-8.9 competitivo, 7-7.9 bueno mejorable, 6-6.9 prototipo avanzado. No premies cambiar por cambiar ni imitar WOS. Compara claridad de los tres héroes/estado bloqueado, cantidad y selector, seis valores, disponibilidad/capacidad y confirmación, uso de espacio y carácter propio. Detecta solapamientos o pérdidas. JSON puro con claves fixtures,reference,reference_kind,reference_limit,overall. fixtures es objeto con t1 y mixed, cada uno {a:[7 números],b:[7 números],observation:string,defects:string}. reference=[7 números] o null, reference_kind direct/analog/none. No apruebes interacción, física ni producción por imagen. Evalúa ambas alternativas imparcialmente y no ajustes notas a ningún objetivo."
        ledger=mission.Ledger(folder/"ledger.json",D(".15"))
        raw=mission.ask("visual",model,info,(str(montage),str(out/"reference-march.jpg")),ledger,"auditor",folder,prompt,effort="none" if idx==0 else "low")
        r=parse(raw);assert set(r)=={"fixtures","reference","reference_kind","reference_limit","overall"} and set(r["fixtures"])=={"t1","mixed"}
        assert r["reference_kind"] in ("direct","analog","none") and (r["reference"] is None)==(r["reference_kind"]=="none")
        mapped={}
        for fixture,v in r["fixtures"].items():
            assert set(v)=={"a","b","observation","defects"}
            scores={order[0]:total(v["a"]),order[1]:total(v["b"])}
            subscores={order[0]:v["a"],order[1]:v["b"]}
            mapped[fixture]={"before":round(scores["before"],2),"after":round(scores["after"],2),"delta":round(scores["after"]-scores["before"],2),"subscores":subscores,"observation":v["observation"],"defects":v["defects"]}
        result={"model":model,"mapping":order,"fixtures":mapped,"reference":None if r["reference"] is None else round(total(r["reference"]),2),"reference_kind":r["reference_kind"],"reference_limit":r["reference_limit"],"overall":r["overall"]}
        write(folder/"unblinded-scores.json",result)
        records.append(result);cost+=D(ledger.rows[0]["estimated_usd"])
        write(out/"partial.json",{"reviews":records,"estimated_usd":str(cost),"complete":False})
    values=[v for r in records for v in r["fixtures"].values()]
    b=statistics.mean(v["before"] for v in values);a=statistics.mean(v["after"] for v in values);delta=statistics.mean(v["delta"] for v in values)
    report={"source_sha":SOURCE,"complete":True,"reviews":records,"mean_before":round(b,2),"mean_after":round(a,2),"mean_delta":round(delta,2),"target_8_met":a>=8,"one_point_met":delta>=1,"native_defects":native["defects"],"native_controls":native["controls"],"interaction_native_gate":not native["defects"],"commercial_gate":a>=8 and delta>=1 and not native["defects"],"art_approved":False,"full_browser_journey":False,"physical_iphone":False,"estimated_usd":str(cost),"scope":"Real production prefab in Unity PlayMode disposable LocalGateway fixtures, not browser build or physical phone"}
    write(out/"final-march-studio-scorecard.json",report)
    print("MARCH_STUDIO_FINAL_SCORECARD",json.dumps(report,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
