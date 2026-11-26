"""Bounded visual agents: only enumerated presentation edits; no generated code execution."""
import argparse,datetime,hashlib,json,os,subprocess,sys,urllib.request,unittest
from decimal import Decimal
from io import BytesIO
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parent
CFG=json.loads((ROOT/"contract.json").read_text())
D=Decimal
def dump(path,data):Path(path).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding="utf-8")
def digest(data):return hashlib.sha256(data).hexdigest()
def gh(path):
    return json.loads(subprocess.check_output(["gh","api",path],timeout=60))
def load_json_text(text):
    text=text.strip()
    if text.startswith("```json") and text.endswith("```"):text=text[7:-3].strip()
    data=json.loads(text)
    if not isinstance(data,dict):raise ValueError("Expected JSON object")
    return data
def build_sheet(items,out,columns=4,width=390,height=844):
    sheet=Image.new("RGB",(columns*width,((len(items)+columns-1)//columns)*(height+32)),(16,23,31))
    draw=ImageDraw.Draw(sheet)
    for i,(label,image) in enumerate(items):
        im=image.convert("RGB");im.thumbnail((width-6,height))
        x=(i%columns)*width;y=(i//columns)*(height+32)
        draw.text((x+4,y+6),label,fill="white")
        sheet.paste(im,(x+(width-im.width)//2,y+32))
    sheet.save(out,quality=91,subsampling=1,optimize=True)
    assert out.stat().st_size<3*1024*1024
def game_sheet(source,out,check=True):
    items=[];manifest=[]
    for name,file,screen in CFG["screens"]:
        data=(source/file).read_bytes()
        im=Image.open(BytesIO(data))
        if check and im.size!=(390,844):raise ValueError("Unexpected screenshot size "+file)
        items.append((name,im.copy()))
        manifest.append({"screen":name,"file":file,"sha256":digest(data),"size":im.size})
    build_sheet(items,out,columns=4,width=390,height=844)
    return manifest
def reference_sheet(out):
    items=[];manifest=[]
    for label,scope,url,page in CFG["references"]:
        try:
            req=urllib.request.Request(url,headers={"User-Agent":"Mozilla/5.0"})
            with urllib.request.urlopen(req,timeout=20) as response:
                raw=response.read(8*1024*1024+1)
            if len(raw)>8*1024*1024:raise ValueError("reference too large")
            im=Image.open(BytesIO(raw));im.load()
            items.append((label,im.copy()))
            manifest.append({"label":label,"scope":scope,"page":page,"url":url,"sha256":digest(raw),"size":im.size,"kind":"third-party gameplay capture; age and viewport vary"})
        except Exception as e:
            manifest.append({"label":label,"scope":scope,"page":page,"url":url,"unavailable":type(e).__name__})
    if not items:raise ValueError("No reference images obtained; no paid call")
    if not any(x["label"].startswith("WOS") for x in manifest if "sha256" in x) or not any(x["label"].startswith("COD") for x in manifest if "sha256" in x):raise ValueError("Both games required")
    build_sheet(items,out,columns=2,width=620,height=600)
    return manifest
def assert_lease():
    repo=os.environ["GITHUB_REPOSITORY"]
    reg=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    import base64
    blob=gh(f"repos/{repo}/git/blobs/{reg['sha']}")
    assert blob["sha"]==reg["sha"]
    data=json.loads(base64.b64decode(blob["content"]))
    active=next(x for x in data["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    assert active["owner"]=="chatgpt-corona-brecha-commercial-20261126" and active["ownership_revision"]==18
    ref=gh(f"repos/{repo}/git/ref/heads/pilot/eldoria-openai-development-studio-20261125")
    assert ref["object"]["sha"]==CFG["studio_sha"]
    assert gh(f"repos/{repo}/git/ref/heads/experiment/valoria-ciudad-region-unificada-20261121")["object"]["sha"]==CFG["source_sha"]
    assert os.environ["GITHUB_RUN_ATTEMPT"]=="1"
def budget_check(studio_root):
    sys.path.insert(0,str(studio_root));import budget_guard
    data=json.loads((studio_root/"corona-owner-five-budget.json").read_text())
    ledger=budget_guard.validate(data,datetime.datetime.now(datetime.timezone.utc).date())
    assert D(ledger["reserved_usd"])==D("4.90")
    assert D(ledger["cap_usd"])==D("5")
    assert sum(D(CFG["budget"][x]) for x in ("prior_reserved","plan","audit"))==D("5")
def call(studio_root,out,stage,instructions,evidence,images):
    assert_lease();budget_check(studio_root)
    import studio
    # Fixed low output bound and exactly two bounded visual contact sheets.
    studio.MAX_OUTPUT=2800
    payload,manifest=studio.make_payload("visual","gpt-6.1-terra",str(evidence),tuple(map(str,images)))
    payload["instructions"]=instructions
    for item in payload["input"][0]["content"]:
        if item["type"]=="input_image":item["detail"]="high"
    receipt=out/(stage+"-reservation.json")
    with receipt.open("x") as f:json.dump({"reserved_usd":CFG["budget"][stage],"state":"reserved","no_retry":True},f)
    raw=studio.request_api(payload,os.environ.get("OPENAI_API_KEY"))
    result=studio.report("visual",payload,manifest,raw)
    dump(out/(stage+"-receipt.json"),result)
    cost=D(result["cost_estimate_usd"])
    assert cost.is_finite() and D(0)<=cost<=D(CFG["budget"][stage]),"Actual estimate exceeded reservation: stop"
    dump(receipt,{"reserved_usd":CFG["budget"][stage],"estimated_usd":str(cost),"state":"completed","invoice_verified":False,"no_retry":True})
    return load_json_text(result["response"])
def validate_plan(plan):
    if set(plan)!={"screens","choices"}:raise ValueError("Plan keys")
    if not isinstance(plan["screens"],dict) or set(plan["screens"])!={x[0] for x in CFG["screens"]}:raise ValueError("Every screen needs an evaluation")
    for name,v in plan["screens"].items():
        if set(v)!={"score","reference_score","evidence","reference","high_impact"}:raise ValueError("Screen fields")
        for key in ("score","reference_score"):
            val=v[key]
            if val is not None and (type(val) not in (float,int) or not 0<=val<=10):raise ValueError("Invalid score")
        for key in ("evidence","reference","high_impact"):
            if not isinstance(v[key],str) or not 1<=len(v[key])<=500:raise ValueError("Invalid rationale")
    if not isinstance(plan["choices"],dict) or set(plan["choices"])-set(CFG["knobs"]):raise ValueError("Unknown edits")
    for k,v in plan["choices"].items():
        if v not in CFG["knobs"][k]["variants"]:raise ValueError("Unknown variant")
    return plan
def apply(plan,root,report):
    validate_plan(plan)
    changes={}; originals={}
    for key,variant in plan["choices"].items():
        spec=CFG["knobs"][key];path=spec["file"]
        if variant=="keep":continue
        if path not in originals:
            originals[path]=(root/path).read_text(encoding="utf-8")
        src=changes.get(path,originals[path])
        for a,b in spec["variants"][variant]:
            if src.count(a)!=1:raise ValueError("Stale or ambiguous presentation literal: "+key)
            src=src.replace(a,b,1)
        changes[path]=src
    for path,src in changes.items():
        (root/path).write_text(src,encoding="utf-8")
    dump(report,{"choices":plan["choices"],"changed_files":list(changes),"agent_plan_applied":True,"visual_approval":False,"mechanics_edited":False})
def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("mode",choices=["plan","apply","audit"])
    parser.add_argument("--studio",type=Path);parser.add_argument("--source",type=Path)
    parser.add_argument("--out",type=Path,required=True);parser.add_argument("--plan",type=Path);parser.add_argument("--root",type=Path)
    args=parser.parse_args()
    if args.mode=="apply":
        apply(json.loads(args.plan.read_text()),args.root,args.out);return
    args.out.mkdir(parents=True,exist_ok=False)
    game=game_sheet(args.source,args.out/"game.jpg")
    dump(args.out/"game-manifest.json",game)
    if args.mode=="plan":
        ref=reference_sheet(args.out/"reference.jpg")
        dump(args.out/"reference-manifest.json",ref)
        evidence={"screens":CFG["screens"],"references":ref,"choices":{k:{"screen":v["screen"],"variants":{n:[a+" -> "+b for a,b in edits] for n,edits in v["variants"].items()}} for k,v in CFG["knobs"].items()}}
        dump(args.out/"brief.json",evidence)
        prompt="Director artístico de Eldoria. Imagen 1: 13 pantallas WebGL actuales. Imagen 2: referencias de gameplay, algunas históricas/landscape o análogas. Sólo diseño visual, no mecánicas, no copiar arte. Evalúa cada pantalla con nota 0-10 sustentada en lo visible y nota de la referencia comparable si existe. Usa null cuando no se pueda comparar o leer. Propon una mejora de alto impacto concreta, aunque esté fuera de opciones. Ejecutarás directamente únicamente opciones enumeradas, selecciona sólo las sustentadas por la evidencia y que no arriesguen una regresión. NO fuerces cambios por cumplir una cuota. JSON puro {screens:{cada id:{score:number|null,reference_score:number|null,evidence:string,reference:string,high_impact:string}},choices:{id de opción:variante}}. Frases concisas, max 500 caracteres cada una. No inventes resultados."
        plan=validate_plan(call(args.studio,args.out,"plan",prompt,args.out/"brief.json",[args.out/"game.jpg",args.out/"reference.jpg"]))
        dump(args.out/"plan.json",plan)
    else:
        old=args.plan.parent/"game.jpg"
        plan=json.loads(args.plan.read_text())
        dump(args.out/"brief.json",{"original":plan,"manifest":game,"screens":CFG["screens"]})
        prompt="Auditor independiente. Imagen 1: ANTES, imagen 2: DESPUÉS (capturas WebGL reales del mismo recorrido). Revisa cada pantalla etiquetada, no premies el mero cambio de código. Evalúa mejora o regresión por criterios artísticos; null cuando no sea legible. No declares pruebas físicas, animación o aprobación basándote en estáticos. JSON puro {screens:{id:{before:number|null,after:number|null,delta:number|null,evidence:string,remaining:string}},overall:string}. Conserva las discrepancias con evaluación previa; no inventes una puntuación de referencia."
        result=call(args.studio,args.out,"audit",prompt,args.out/"brief.json",[old,args.out/"game.jpg"])
        assert set(result.get("screens",{}))=={x[0] for x in CFG["screens"]}
        dump(args.out/"audit.json",result)
if __name__=="__main__":main()
