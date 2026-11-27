"""Separate read-only prep and authenticated one-shot API execution."""
import pathlib,json,hashlib,sys,os,datetime as dt,subprocess,base64
from urllib.parse import urlencode
from decimal import Decimal as D
P=pathlib.Path
HERE=P(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent.parent/"openai-development-studio"))
import studio,generate_candidate as image,economics,budget_guard
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
BRANCH="pilot/eldoria-studio-v3-autonomous-execution-20261126"
SOURCE="230bdf286c8b0b2cc0a1fe4aef0af797fcb9d84a"
OWNER="chatgpt-studio-v3-rankings-premium-20261127"
ID="eldoria-studio-v3-autonomous-execution-20261126"
ROOT="tools/studio-v3/rankings-premium/"
REQUEST="tools/studio-v3/live-requests/pr198-premium-20261127.json"
MESSAGE="Authorize isolated Studio PR198 premium production within USD 5.00"
SLOTS={"director":D("1.20"),"ux":D("0.60"),"ux-repair":D("0.60"),"hall":D("0.55"),"relic":D("0.55"),"integrator":D("0.55"),"integrator-repair":D("0.55"),"audit":D("0.20"),"audit-final":D("0.20")}
def need(x,m):
    if not x:raise ValueError(m)
def read(p):return json.loads(P(p).read_text())
def dump(p,v):P(p).write_text(json.dumps(v,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
def sha(b):return hashlib.sha256(b).hexdigest()
def run(*c):return subprocess.check_output(c,timeout=90,stderr=subprocess.PIPE).decode()
def gh(p):return json.loads(run("gh","api","--method","GET",p))
def git(*args):return run("git",*args).strip()
def preflight():
    need(os.environ.get("GITHUB_REPOSITORY")==REPO and os.environ.get("GITHUB_REF")=="refs/heads/"+BRANCH and os.environ.get("GITHUB_EVENT_NAME")=="push" and os.environ.get("GITHUB_RUN_ATTEMPT")=="1","Not authorized first push")
    ev=read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=ev["before"]
    need(ev["after"]==h and ev["ref"]=="refs/heads/"+BRANCH and not ev.get("forced") and not ev.get("deleted"),"Wrong event")
    need(git("rev-parse","HEAD")==h and git("rev-parse","HEAD^")==before and git("log","-1","--format=%s")==MESSAGE,"Wrong launch")
    need(git("diff","--name-status",before,h)=="A\t"+REQUEST,"Only immutable launch file allowed")
    raw=(HERE/"mission.json").read_bytes();m=json.loads(raw);need(P(REQUEST).read_bytes()==raw and m["owner_authorized"] is True and m["source_sha"]==SOURCE and m["cap_usd"]=="5.00","Grant bytes mismatch")
    need(dt.datetime.now(dt.timezone.utc).date()<dt.date.fromisoformat(m["expiry"]),"Expired")
    need(not gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1})),"Request already used")
    need(gh("repos/"+REPO+"/git/ref/heads/"+BRANCH)["object"]["sha"]==h,"Branch moved")
    ci=gh("repos/"+REPO+"/actions/workflows/studio-v3-rankings-premium.yml/runs?"+urlencode({"head_sha":before,"status":"success","per_page":20}))["workflow_runs"]
    need(any(x["head_sha"]==before and x["conclusion"]=="success" and x["event"]=="push" for x in ci),"Exact preparation CI missing")
    pr=gh("repos/"+REPO+"/pulls/198");need(pr["state"]=="open" and pr["head"]["sha"]==SOURCE,"PR198 head moved")
    meta=gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh("repos/"+REPO+"/git/blobs/"+meta["sha"])["content"]))
    rows=[w for w in reg["active"] if w["id"]==ID];need(len(rows)==1,"Missing claim")
    w=rows[0];need(w["owner"]==OWNER and w["ownership_revision"]==11 and w["ownership_state"]=="claimed" and w["ranking_mission"]["cap_usd"]=="5.00" and w["ranking_mission"]["source_sha"]==SOURCE,"Claim changed")
    need(sum(SLOTS.values())==D("5.00"),"Slot budget")
    b={"cycle":{"starts":"2026-11-27","ends_exclusive":m["expiry"],"pilot_cap_usd":"5.00","automatic_reset":False},"reservations":[{"id":m["id"],"run_id":m["id"],"reserve_usd":"5.00","status":"authorized_once"}]}
    budget_guard.authorize(b,dt.datetime.now(dt.timezone.utc).date(),m["id"],m["id"],1,D("5.00"))
    print("PR198_AUTHORIZED",json.dumps({"head":h,"source":SOURCE,"grant_hash":sha(raw),"cap":"5.00","ci":ci[0]["id"]}),flush=True)
    return m
class Ledger:
    def __init__(self,out):
        self.path=out/"ledger.json";need(not self.path.exists(),"Existing ledger");self.rows=[];self.save()
    def save(self):
        tmp=self.path.with_suffix(".tmp")
        dump(tmp,{"cap_usd":"5.00","entries":self.rows,"billed_verified":False});tmp.replace(self.path)
    def reserve(self,slot):
        need(slot in SLOTS and not any(x["slot"]==slot for x in self.rows),"Duplicate slot")
        need(all(x["state"]=="completed" for x in self.rows),"Uncertain previous cost; stop")
        need(sum((D(x["reserve_usd"]) for x in self.rows),SLOTS[slot])<=D("5.00"),"Cap")
        row={"slot":slot,"reserve_usd":str(SLOTS[slot]),"state":"reserved_before_api","estimated_usd":None}
        self.rows.append(row);self.save();return row
    def finish(self,row,usage,model,tier="standard"):
        row.update(usage=usage,model=model,tier=tier)
        try:cost=economics.estimate(model,usage,tier)
        except Exception:self.save();raise
        row["estimated_usd"]=str(cost);row["state"]="completed" if cost<=D(row["reserve_usd"]) else "over_reservation";self.save()
        need(row["state"]=="completed","Request exceeded estimate; stop")
def json_result(s):
    s=s.strip()
    if s.startswith("```json") and s.endswith("```"):s=s[7:-3].strip()
    return json.loads(s)
def main(source,data_path,out,live):
    from render import validate_director,validate_ux,validate_integration,make_preview,capture,SECTIONS
    from PIL import Image
    source=P(source);out=P(out);out.mkdir(parents=True,exist_ok=False)
    data=read(data_path)
    need(data["sourceSha"]==SOURCE and data["fixture"] is True and not any(p["publicRankings"] for p in data["profiles"]),"Bad domain projection")
    dump(out/"projection.json",data)
    m=read(HERE/"mission.json")
    if live:need(preflight()==m,"Mission mismatch")
    art=out/"art";art.mkdir()
    common=(HERE/"BRIEF.md").read_text()+"\nPR: "+(source/"contracts/IMPLEMENTATION.md").read_text()+"\nCorona: "+(source/"contracts/ART_DIRECTION.md").read_text()+"\nShared standard: "+(source/"contracts/SHARED_STANDARD.md").read_text()
    # Real labels, states and gates, no rival fixtures. Data is input evidence.
    common=common[:13000]+"\nCompiled B5 projection (exact C#; QA fixture): "+json.dumps(data["profiles"][-1],ensure_ascii=False)[:7000]
    def fixture():
        d={"concept":"Calibración técnica, no autoría de agentes","palette":{"ink":"#1A1428","surface":"#30223F","trim":"#C99C54","text":"#F1E9D4","muted":"#D1C7DB","accent":"#A85BEB","action":"#63D9CF"},"hall_prompt":"Calibration only. Original Valoria material test; not generated art.","relic_prompt":"Calibration only. Original Valoria material test; not generated art."}
        u={k:{"layout":"featured" if k in ("Overview","Rankings") else "tiles","art":"hall" if k in ("Overview","Rankings") else "relic","hero_height":134,"density":"compact"} for k in SECTIONS}
        t={"gutter":14,"gap":8,"radius":7,"title_px":19,"body_px":15,"detail_px":12,"art_strength":55,"ornament":"crest","frame":"gold"}
        for k in ("hall","relic"):
            with Image.open(source/"assets/panel_obsidian.png") as im:im.convert("RGB").save(art/(k+".png"))
        make_preview(data,source,art,{"director":d,"ux":u,"integration":t},out/"round-1");capture(out/"round-1")
        dump(out/"summary.json",{"live":False,"api_calls":0,"note":"Fixture only, not agent result"})
    if not live:return fixture()
    key=os.environ.get("OPENAI_API_KEY");need(bool(key),"Missing API key")
    l=Ledger(out)
    def ask(slot,model,instructions,content,images=()):
        p=out/(slot+"-input.txt");p.write_text(content.encode("utf-8")[:22000].decode("utf-8",errors="ignore"),encoding="utf-8")
        payload,manifest=studio.make_payload("visual" if images else "programming",model,p,images)
        payload["instructions"]=instructions+" Devuelve un solo objeto JSON válido, sin Markdown ni texto fuera del objeto."
        payload["max_output_tokens"]=2600
        for c in payload["input"][0]["content"]:
            if c["type"]=="input_image":c["detail"]="high"
        row=l.reserve(slot);print("PR198_ROLE_START",slot,flush=True)
        raw=studio.request_api(payload,key)
        l.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
        report=studio.report("visual" if images else "programming",payload,manifest,raw)
        dump(out/(slot+"-receipt.json"),report)
        result=json_result(report["response"]);dump(out/(slot+"-output.json"),result)
        print("PR198_ROLE_COMPLETE",slot,row["estimated_usd"],flush=True);return result
    references=(source/"assets/identity_crest.png",source/"assets/panel_obsidian.png")
    d=validate_director(ask("director","gpt-6.1-terra","Director artístico senior. Estudia los contratos y recursos reales. Decide una dirección premium única, con libertad creativa. JSON exacto: concept (<=900 chars), palette con ink,surface,trim,text,muted,accent,action (hex #RRGGBB, contraste móvil; mantener ADN Corona), hall_prompt y relic_prompt (cada uno 50..1800 caracteres, brief de una ilustración original sin textos/GUI; hero background y bodegón de legado). No inventes mecánicas. Las herramientas Unity/Blender MCP están permitidas solo con conexión verificada; aquí se ejecutará arte original y prototipo hospedado.",common,references))
    def paint(slot,brief):
        p=image.make_payload("Original art asset for premium Valoria legacy-profile UI, NOT a screen or card, no letters, numbers, text or UI. Detailed cinematic hand-painted adult medieval fantasy, strong recognizable composition, natural materials, aged gold and amethyst restraint, rich depth. "+brief)
        p["quality"]="medium"
        row=l.reserve(slot);print("PR198_ROLE_START",slot,flush=True)
        raw=image.generate(p,key);l.finish(row,raw.get("usage"),image.MODEL)
        b,meta=image.unpack(raw);(art/(slot+".png")).write_bytes(b)
        with Image.open(art/(slot+".png")) as im:im.verify()
        meta.update(quality="medium",estimated_usd=row["estimated_usd"],prompt=p["prompt"]);dump(out/(slot+"-receipt.json"),meta)
        print("PR198_ROLE_COMPLETE",slot,row["estimated_usd"],flush=True)
    paint("hall",d["hall_prompt"]);paint("relic",d["relic_prompt"])
    # actual artwork, not previous mockup; send reduced copies under size cap
    def input_image(p):
        dest=out/(p.stem+"-input.jpg")
        with Image.open(p) as im:
            im.thumbnail((1000,1000));im.convert("RGB").save(dest,quality=91)
        return dest
    pics=tuple(input_image(art/(k+".png")) for k in ("hall","relic"))
    uinstr="UI/UX especialista senior. Define composición específica y rica para cada una de Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments. Devuelve objeto con exactamente esas 6 claves; cada valor exactamente layout (list|tiles|featured; Rankings no tiles), art (hall|relic), hero_height entero 100..172, density (compact|normal). Mantén todos los datos y estados del dominio. Usa las ilustraciones reales adjuntas. No inventar ninguna fila."
    iinstr="Technical Artist/integrador UI. Decide medidas para todas las pantallas, coherencia, jerarquía y legibilidad. JSON exacto gutter entero 12..20, gap 6..12, radius 4..12, title_px 17..22, body_px 14..17, detail_px 12..14, art_strength 25..70 (opacidad oscura), ornament crest|medallion|minimal, frame gold|subtle. Respeta las capturas y el contrato. No afirmes uso de Unity MCP ni ejecución nativa."
    u=validate_ux(ask("ux","gpt-6.2-sol",uinstr,common+"\nDirector: "+json.dumps(d,ensure_ascii=False),pics))
    t=validate_integration(ask("integrator","gpt-6.2-sol",iinstr,common+"\nDirector: "+json.dumps(d,ensure_ascii=False)+"\nUX: "+json.dumps(u),pics))
    def render(n):
        dest=out/("round-"+str(n));make_preview(data,source,art,{"director":d,"ux":u,"integration":t},dest);rows=capture(dest);return dest,rows
    def audit(slot,dest):
        a=ask(slot,"gpt-6.1-terra","Auditor visual independiente. Evalúa SOLO las capturas reales y los contratos. Exigimos que aspire a las mejores pantallas de Valoria; no equipares funcionamiento a calidad. Evalúa cada una de las 6 secciones, coherencia, materiales, legibilidad, jerarquía y defectos. JSON exacto: scores objeto con las seis claves Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments, valores 0..10; findings array de 3..10 observaciones concretas; decision 'revise'|'candidate'. 'candidate' solo si todas >=8.5 y no defectos relevantes. No declarar aprobación artística.",common[:14000],(dest/"audit-1.jpg",dest/"audit-2.jpg"))
        need(set(a)=={"scores","findings","decision"} and set(a["scores"])==set(SECTIONS) and all(type(v) in (int,float) and 0<=v<=10 for v in a["scores"].values()) and a["decision"] in ("revise","candidate") and isinstance(a["findings"],list),"Audit shape")
        return a
    dest,rows=render(1);a=audit("audit",dest);rounds=1
    if a["decision"]=="revise" or min(a["scores"].values())<8.5:
        fb="\nActual auditor feedback: "+json.dumps(a,ensure_ascii=False)+"\nPrevious UX: "+json.dumps(u)+"\nPrevious integration: "+json.dumps(t)
        u=validate_ux(ask("ux-repair","gpt-6.2-sol",uinstr+" Corrige la causa principal de la auditoría, no un cambio cosmético sin efecto.",common[:13000]+fb,pics))
        t=validate_integration(ask("integrator-repair","gpt-6.2-sol",iinstr+" Ajusta en respuesta a la auditoría.",common[:13000]+fb+"\nRevised UX: "+json.dumps(u),pics))
        dest,rows=render(2);a=audit("audit-final",dest);rounds=2
    total=sum((D(x["estimated_usd"]) for x in l.rows),D(0))
    report={"live":True,"source_sha":SOURCE,"pr":198,"api_calls":len(l.rows),"estimated_usd":str(total),"billing_verified":False,"rounds":rounds,"final_path":dest.name,"frames":len(rows),"final_audit":a,"quality_target_met":a["decision"]=="candidate" and min(a["scores"].values())>=8.5,"native_unity":False,"mcp_executed":False,"pr_integrated":False,"art_approved":False,"published":False}
    dump(out/"summary.json",report);print("PR198_STUDIO_PROOF "+json.dumps(report,ensure_ascii=False),flush=True)
if __name__=="__main__":
    if sys.argv[1:]==["--preflight"]:preflight()
    else:
        need(len(sys.argv)==5 and sys.argv[1] in ("--fixture","--live"),"Explicit run required")
        main(sys.argv[2],sys.argv[3],sys.argv[4],sys.argv[1]=="--live")
