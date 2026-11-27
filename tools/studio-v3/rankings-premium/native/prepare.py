"""Real source and prior agent output -> isolated native input. No arbitrary agent code."""
import base64,datetime as dt,hashlib,json,os,pathlib,re,shutil,subprocess,sys,urllib.request,zipfile
from decimal import Decimal as D
from urllib.parse import urlencode,quote
from PIL import Image,ImageDraw
P=pathlib.Path;HERE=P(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent));import run as v
ID=v.ID;SOURCE=v.SOURCE;OWNER=v.OWNER;REPO=v.REPO;BRANCH=v.BRANCH
PRIOR=45960681379;PRIOR_SHA="e0caff4f5bed2f14a6e6f4d065a50050bdfb0e77";AID=15181211611
ZIP_SHA="49a16a47523a7d7895fb0428cf21a527027eb2529ba0815618f4c5e1c3079fdd"
PROBE=45966455876;MESSAGE="Launch PR198 isolated real native tool mission"
REQ="tools/studio-v3/rankings-premium/native/launch.json"
SECTIONS=["Overview","Rankings","Trophies","Discoveries","Heraldry","Monuments","DiscoveryDetail"]
SLOTS={"native-director":D("0.30"),"native-artist":D("0.60"),"native-technical-artist":D("0.35")}
def need(x,m):
    if not x:raise ValueError(m)
def claim():
    meta=v.gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(v.gh("repos/"+REPO+"/git/blobs/"+meta["sha"])["content"]))
    w=next(x for x in reg["active"] if x["id"]==ID);holders=[x for x in reg["active"] if "windows-runner-heavy" in x.get("resources",[])]
    need(w["owner"]==OWNER and w["ownership_revision"]==12 and len(holders)==1 and holders[0]["id"]==ID and w["ranking_mission"]["native_request"]["probe_run"]==PROBE,"Native lease changed")
    return w
def gate():
    claim();e=v.read(os.environ["GITHUB_EVENT_PATH"]);head=os.environ["GITHUB_SHA"];before=e["before"]
    need(os.environ.get("GITHUB_REPOSITORY")==REPO and os.environ.get("GITHUB_REF")=="refs/heads/"+BRANCH and os.environ.get("GITHUB_RUN_ATTEMPT")=="1" and os.environ.get("GITHUB_EVENT_NAME")=="push","Wrong dispatch")
    need(e["after"]==head and not e.get("forced") and v.git("rev-parse","HEAD")==head and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"Not exact launch")
    need(v.git("diff","--name-status",before,head)=="A\t"+REQ and P(REQ).read_bytes()==(HERE/"grant.json").read_bytes(),"Immutable request mismatch")
    need(not v.gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQ,"per_page":1})),"Already used")
    runs=v.gh("repos/"+REPO+"/actions/workflows/studio-v3-rankings-native.yml/runs?"+urlencode({"head_sha":before,"status":"success","per_page":10}))["workflow_runs"]
    need(any(r["head_sha"]==before and r["conclusion"]=="success" for r in runs),"No exact native preparation CI")
    need(v.gh("repos/"+REPO+"/pulls/198")["head"]["sha"]==SOURCE,"PR198 moved")
    need(v.gh("repos/"+REPO+"/git/ref/heads/"+BRANCH)["object"]["sha"]==head,"Branch moved")
    need(dt.datetime.now(dt.timezone.utc).date()<dt.date(2026,12,2),"Grant expired")
    need(D(".52504140")+sum(SLOTS.values())<=D("5"),"Aggregate reservation")
    print("PR198_NATIVE_GRANT_PASS",head,flush=True)
def download_prior(out):
    r=v.gh("repos/"+REPO+"/actions/runs/"+str(PRIOR));need(r["head_sha"]==PRIOR_SHA and r["conclusion"]=="success","Prior run")
    a=next(x for x in v.gh("repos/"+REPO+"/actions/runs/"+str(PRIOR)+"/artifacts")["artifacts"] if x["id"]==AID)
    need(a["digest"]=="sha256:"+ZIP_SHA and not a["expired"],"Prior artifact")
    zpath=out/"prior.zip"
    with zpath.open("xb") as dest:subprocess.run(["gh","api","repos/"+REPO+"/actions/artifacts/"+str(AID)+"/zip"],stdout=dest,stderr=subprocess.PIPE,check=True,timeout=120)
    need(zpath.stat().st_size<50*1024*1024 and v.sha(zpath.read_bytes())==ZIP_SHA,"Prior ZIP hash")
    with zipfile.ZipFile(zpath) as z:
        summary=json.loads(z.read("summary.json"));ledger=json.loads(z.read("ledger.json"))
        need(summary["source_sha"]==SOURCE and summary["cumulative_estimated_usd"]=="0.52504140" and not summary["quality_target_met"],"Prior decision/cost")
        need(len(ledger["entries"])==9 and all(e["state"]=="completed" and e["estimated_usd"] is not None for e in ledger["entries"]) and sum(D(e["estimated_usd"]) for e in ledger["entries"])==D(".34647300"),"Prior ledger uncertain")
        proof=json.loads(z.read("round-2/capture-proof.json"));need(len(proof)==21,"Prior frames")
        for f in proof:
            name="round-2/captures/"+f["view"]+"-"+f["section"]+".png"
            need(v.sha(z.read(name))==f["sha256"],"Previous frame changed")
        (out/"prior-audit.json").write_text(json.dumps(summary["final_audit"],ensure_ascii=False),encoding="utf-8")
        for n in ("audit-1.jpg","audit-2.jpg"):
            (out/n).write_bytes(z.read("round-2/"+n))
        assets=out/"assets";assets.mkdir()
        for n in z.namelist():
            if re.fullmatch(r"round-2/assets/[a-zA-Z0-9_-]+\.(png|jpg)",n):
                p=assets/P(n).name;p.write_bytes(z.read(n))
                with Image.open(p) as im:im.verify()
    zpath.unlink()
    return summary
def validate(d):
    need(set(d)=={"concept","metal","enamel","gem","points","bevel","screens"} and isinstance(d["concept"],str) and 20<=len(d["concept"])<=1400,"Plan fields")
    for k in ["metal","enamel","gem"]:
        need(isinstance(d[k],list) and len(d[k])==3 and all(type(v) in (float,int) and 0<=v<=1 for v in d[k]),"Material "+k)
    need(type(d["points"]) is int and 8<=d["points"]<=16 and type(d["bevel"]) in (float,int) and .02<=d["bevel"]<=.12,"Blender plan")
    need(isinstance(d["screens"],list) and len(d["screens"])==7 and [x["section"] for x in d["screens"]]==SECTIONS,"Sections")
    for x in d["screens"]:
        need(set(x)=={"section","hero","heroHeight","rowHeight","columns","iconSize","titleSize","artSide"},"Screen keys")
        need(x["hero"] in ("hall","relic","vault") and x["artSide"] in ("left","right") and type(x["columns"]) is int and x["columns"] in (1,2),"Layout")
        for k,a,b in [("heroHeight",128,240),("rowHeight",60,144),("iconSize",32,88),("titleSize",24,34)]:
            need(type(x[k]) is int and a<=x[k]<=b,"Dimension "+k)
        if x["section"]=="Rankings":need(x["columns"]==1,"Rankings full-width notice")
    return d
def fixture():
    return {"concept":"Offline native authoring contract fixture only, not specialist result","metal":[.64,.35,.10],"enamel":[.035,.022,.065],"gem":[.38,.08,.61],"points":12,"bevel":.055,"screens":[{"section":s,"hero":"hall" if i in (0,1,5) else "relic","heroHeight":170,"rowHeight":88 if i<2 else 120,"columns":1 if i in (0,1,3,6) else 2,"iconSize":56,"titleSize":29,"artSide":"right"} for i,s in enumerate(SECTIONS)]}
def get_fonts(out):
    for r in v.read(HERE/"fonts.json"):
        url="https://raw.githubusercontent.com/"+REPO+"/"+r["commit"]+"/"+quote(r["path"],safe="/")
        with urllib.request.urlopen(url,timeout=40) as f:raw=f.read(r["size"]+1)
        need(len(raw)==r["size"] and hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==r["blob_sha"],"Font provenance")
        (out/r["destination"]).write_bytes(raw)
def main(source,out,live):
    source=P(source);out=P(out);out.mkdir(parents=True,exist_ok=False)
    prior=download_prior(out);get_fonts(out)
    domain=out/"domain";shutil.copytree(source/"domain",domain)
    shutil.copy2(source/"contracts/LegacyProfileRow.cs",domain/"LegacyProfileRow.cs")
    for n in ("NativeProfile.cs","NativeCapture.cs","blender_author.py","unity_mcp_probe.py","native.ps1"):shutil.copy2(HERE/n,out/n)
    p=fixture()
    if live:
        gate()
        key=os.environ.get("OPENAI_API_KEY");need(bool(key),"Missing API key")
        v.SLOTS=SLOTS;l=v.Ledger(out)
        def ask(slot,model,ins,content,images):
            path=out/(slot+"-input.txt");path.write_text(content.encode("utf-8")[:22500].decode("utf-8",errors="ignore"),encoding="utf-8")
            payload,m=v.studio.make_payload("visual",model,path,images);payload["instructions"]=ins;payload["max_output_tokens"]=3500
            for c in payload["input"][0]["content"]:
                if c["type"]=="input_image":c["detail"]="high"
            row=l.reserve(slot);raw=v.studio.request_api(payload,key);l.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
            report=v.studio.report("visual",payload,m,raw);v.dump(out/(slot+"-receipt.json"),report)
            res=v.json_result(report["response"]);v.dump(out/(slot+"-output.json"),res);print("NATIVE_ROLE_COMPLETE",slot,row["estimated_usd"],flush=True);return res
        common=(HERE.parent/"BRIEF.md").read_text()+"\n"+(source/"contracts/ART_DIRECTION.md").read_text()+"\n"+(source/"contracts/SHARED_STANDARD.md").read_text()+"\nActual rejected native predecessor audit (web): "+json.dumps(prior["final_audit"],ensure_ascii=False)
        pics=(out/"audit-1.jpg",out/"audit-2.jpg")
        d=ask("native-director","gpt-6.1-terra","Director artístico senior: redirige la PR198 hacia un conjunto nativo de Unity con joyería y arquitectura física en Blender, no otros paneles planos. Considera las imágenes rechazadas. Define composición diferenciada, integración del blasón y medallas y un recurso original de entorno/galería para la cabecera. Devuelve JSON exactamente {concept: string, vault_prompt: string}. No inventes mecánicas o nombres; nada de texto incrustado en el arte.",common,pics)
        need(set(d)=={"concept","vault_prompt"} and all(isinstance(x,str) and 20<len(x)<2200 for x in d.values()),"Director response")
        prompt="Original game art asset only; no text, numbers, buttons, panels or complete UI. Cinematic exquisitely hand-painted and material-rich medieval fantasy architectural interior for Eldoria Valoria: "+d["vault_prompt"]+". Dark obsidian, warm aged brass, amethyst, amber candlelight, restrained sophisticated atmosphere, architectural craftsmanship and depth. Not a copy of any game."
        pl=v.image.make_payload(prompt);pl["quality"]="high";row=l.reserve("native-artist");raw=v.image.generate(pl,key);l.finish(row,raw.get("usage"),v.image.MODEL)
        bits,meta=v.image.unpack(raw);(out/"assets/vault.png").write_bytes(bits);meta.update(quality="high",estimated_usd=row["estimated_usd"],prompt=pl["prompt"]);v.dump(out/"native-artist-receipt.json",meta)
        with Image.open(out/"assets/vault.png") as im:im.verify()
        with Image.open(out/"assets/vault.png") as im:im.convert("RGB").thumbnail((1024,1024))
        with Image.open(out/"assets/vault.png") as im:
            im.thumbnail((900,900));im.convert("RGB").save(out/"vault-reference.jpg",quality=91)
        ins="Technical artist / Unity and Blender specialist. You direct real geometry and native view layout, not prose. Native authoring adapter will execute these parameters through actual installed tools. Return exactly JSON {concept (20..1400 chars),metal:[r,g,b],enamel:[r,g,b],gem:[r,g,b],points (integer 8..16),bevel (0.02..0.12),screens:[7 objects in order Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments,DiscoveryDetail]}. RGB linear values in 0..1. Each screen exactly {section,hero (hall|relic|vault),heroHeight integer 128..240,rowHeight integer 60..144,columns 1|2 (Rankings 1),iconSize integer 32..88,titleSize integer 24..34,artSide left|right}. Deliberately make profile a prestige hall, categories a royal record, collections distinct. Keep values and titles from exact ProfilePresentation. Native Blender will author relief metal frames with actual materials. Do not claim execution or add functionality."
        p=validate(ask("native-technical-artist","gpt-6.2-sol",ins,common[:14000]+"\nDirector: "+json.dumps(d,ensure_ascii=False),(out/"audit-1.jpg",out/"vault-reference.jpg")))
        cost=sum(D(x["estimated_usd"]) for x in l.rows);v.dump(out/"role-proof.json",{"real_roles":True,"api_calls":len(l.rows),"estimated_usd":str(cost),"cumulative_estimated_usd":str(D(".52504140")+cost),"billing_verified":False,"source_sha":SOURCE})
    else:
        with Image.open(out/"assets/hall.jpg") as im:im.save(out/"assets/vault.png")
        v.dump(out/"role-proof.json",{"real_roles":False,"api_calls":0,"fixture":True})
    validate(p);v.dump(out/"native-design.json",p)
    print("PR198_NATIVE_INPUT_READY",json.dumps({"live":live,"screens":len(p["screens"]),"source":SOURCE}),flush=True)
if __name__=="__main__":
    if sys.argv[1:]==["--gate"]:gate()
    else:
        need(len(sys.argv)==4 and sys.argv[1] in ("--fixture","--live"),"Explicit mode")
        main(sys.argv[2],sys.argv[3],sys.argv[1]=="--live")
