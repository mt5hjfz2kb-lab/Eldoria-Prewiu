"""Independent R3 creative direction, using real evidence and a $7 cumulative owner cap.
The contaminated native screenshots are not a valid artistic scorecard.
"""
import hashlib,json,os,subprocess,sys
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
def read(p):return json.loads(Path(p).read_text(encoding="utf-8"))
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def gh(path):return json.loads(subprocess.check_output(["gh","api",path],timeout=60))
def check(cond,message):
    if not cond:raise ValueError(message)
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    check(os.environ["GITHUB_RUN_ATTEMPT"]=="1","No paid retry")
    check(gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-r3-studio-20261126")["object"]["sha"]==head,"Branch changed")
    for run,artifact,source in [(45872970373,15146218340,"034997e4bc5f4e9cd95bb092d604d8053f7b9fd3"),(45866050061,15140472209,"6d58a4064ca8cb56b76d3588bf9d6a645ff6f116"),(45851071206,15136075692,None),(45812100129,15113766981,None)]:
        r=gh(f"repos/{repo}/actions/runs/{run}")
        check(r["conclusion"]=="success" and (source is None or r["head_sha"]==source),"Unexpected evidence run")
        check(any(a["id"]==artifact and not a["expired"] for a in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"]),"Missing exact evidence")
    import base64
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    check(lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18,"Studio lease changed")
    check(D("5")+D(".87")+D(".30")+D(".20")+D(".63")==D("7"),"Budget arithmetic")
    e=read("native/evidence.json")
    check(e["sourceSha"]=="fd340c5868f8169261e6c72c38f94df234a21627" and len(e["frames"])==6,"Wrong native proof")
    for f in e["frames"]:
        p=Path("native")/f["file"]
        check(hashlib.sha256(p.read_bytes()).hexdigest()==f["sha256"] and f["actualCamera"] and f["playMode"],"Invalid native frame")
    out=Path("r3-direction");out.mkdir(exist_ok=False)
    write(out/"budget.json",{"authorized_by_owner":"2026-11-26: additional 2 USD, cumulative 7 USD","prior_held":"5.00","existing_artist_and_preflight_held":".87","existing_final_review_held":".30","new_direction_held":".20","unallocated":".63","invoice_verified":False,"no_retry":True})
    # Native pair is contaminated. Preserve the pixels; do not inpaint, crop
    # away the defect, or present it as an acceptable final screen.
    canvas=Image.new("RGB",(792,870),"#172028")
    draw=ImageDraw.Draw(canvas)
    for x,v in ((0,"before"),(402,"after")):
        with Image.open("native/mixed-"+v+".png") as im:canvas.paste(im.convert("RGB"),(x,26))
        draw.text((x+5,6),v.upper()+" - native fixture, obstructed",fill="white")
    canvas.save(out/"obstructed-native-pair.png")
    # Include clean actual old game and commercial reference in a single sheet.
    old=Image.open("actual/sistema-march.png").convert("RGB")
    refs=Image.open("refs/reference.jpg").convert("RGB")
    refs.thumbnail((1000,1500))
    comparison=Image.new("RGB",(old.width+refs.width+12,max(old.height,refs.height)+26),"#172028")
    comparison.paste(old,(0,26));comparison.paste(refs,(old.width+12,26))
    draw=ImageDraw.Draw(comparison);draw.text((4,6),"Original real WebGL",fill="white");draw.text((old.width+16,6),"Commercial references",fill="white")
    comparison.save(out/"clean-original-and-references.jpg",quality=90)
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import studio,mission
    mission.RESERVES["director"]=D(".10");mission.RESERVES["auditor"]=D(".10")
    ledger=mission.Ledger(out/"ledger.json",D(".20"))
    original=studio.make_payload
    def high(*args,**kwargs):
        p,m=original(*args,**kwargs)
        for item in p["input"][0]["content"]:
            if item["type"]=="input_image":item["detail"]="high"
        return p,m
    studio.make_payload=high
    studio.MAX_OUTPUT=1600
    evidence=mission.textfile(out/"evidence.txt",json.dumps({"defects":e["defects"],"checks":e["checks"],"original_art":"march_command_table, secondary_quiet, action_arcane, panel_obsidian, existing hero portraits and archer","scope":"portrait March only, maintain real controls and values. Old WebGL is a different fixture from native. Native images share a severe obstructing foreground render. No numerical score is admissible."},ensure_ascii=False))
    prompt="""Actúa como director artístico de un juego de estrategia móvil de primer nivel. La misión es un salto visible, no otra pasada de márgenes. Primera imagen: juego anterior real y referentes comerciales. Segunda: comparación nativa real que tiene una obstrucción 3D ajena a las tarjetas, tanto antes como después. No puntúes esas capturas obstruidas ni presupongas que el defecto está en el juego WebGL. Analiza las zonas que sí se ven: retratos demasiado cortos, tarjetas planas azuladas, jerarquía del resumen, acción principal sin identidad. Da una propuesta R3 concreta que reutilice el arte original del estudio y las texturas Corona existentes para una mesa de guerra noble, no una plantilla SaaS. Conserva 3 ranuras, cierre, acceso héroes, entrenamiento si existe, selector T1/T2, cantidad/%, menos/más, deslizador, seis métricas, estado y confirmar. No inventes valores, controles ni mecánicas. Elige decisiones implementables en Unity, con áreas táctiles 44px, contraste, identidad, profundidad y foco. JSON puro {direction:string,hero_treatment:string,troop_treatment:string,metric_treatment:string,cta_treatment:string,material_treatment:string,acceptance:string,critical_risks:[string]}. No código ni imágenes ficticias."""
    raw=mission.ask("visual","gpt-6.1-terra",evidence,(str(out/"clean-original-and-references.jpg"),str(out/"obstructed-native-pair.png")),ledger,"director",out,prompt,effort="none")
    plan=json.loads(raw.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
    check(set(plan)=={"direction","hero_treatment","troop_treatment","metric_treatment","cta_treatment","material_treatment","acceptance","critical_risks"},"Invalid direction")
    check(all(isinstance(v,str) and v.strip() for k,v in plan.items() if k!="critical_risks"),"Missing direction")
    check(isinstance(plan["critical_risks"],list) and all(isinstance(v,str) and v.strip() for v in plan["critical_risks"]),"Missing risks")
    write(out/"plan.json",plan)
    print("MARCH_R3_PLAN",json.dumps(plan,ensure_ascii=False),flush=True)
    studio.MAX_OUTPUT=1300
    brief=mission.textfile(out/"challenge-input.txt",json.dumps({"plan":plan,"native_defects":e["defects"],"native_controls":e["controls"],"constraint":"same real state, no altered camera proof, preserve actual interactions and no automatic promotion"},ensure_ascii=False))
    raw=mission.ask("programming","gpt-6-luna",brief,(),ledger,"auditor",out,"Technical Artist: critique this exact plan and the recorded real defects. Distinguish capture contamination from production defects; require actual hit-testing at 390x844, equivalent state, clean camera, six metrics, preserved hero locks, existing controls. Recommend concrete rect, layer, hit target, overflow and fallback corrections. JSON only {verdict:string,corrections:[string],tests:[string]}. No test claims.",effort="none")
    result=json.loads(raw.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
    check(set(result)=={"verdict","corrections","tests"},"Invalid technical critique")
    write(out/"technical.json",result);print("MARCH_R3_TECHNICAL",json.dumps(result,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
