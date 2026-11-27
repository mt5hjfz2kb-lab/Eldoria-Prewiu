"""One new, owner-authorized, profile-only creative experiment. Never writes game sources."""
import sys, pathlib, json, os, subprocess, zipfile, re, hashlib, datetime as dt
from decimal import Decimal as D
from urllib.parse import urlencode
from PIL import Image, ImageDraw
P=pathlib.Path
sys.path.insert(0,str(P(__file__).resolve().parent))
sys.path.insert(0,str(P(__file__).resolve().parent/"pivot"))
import run as v
import layout
ID="pr198-profile-proof-20261127-pm-recovery"
LAUNCH=".github/workflows/studio-v3-profile-proof-recovery-20261127.yml"
MESSAGE="Launch recovered PR198 profile proof after zero-spend gate failure"
SOURCE=v.SOURCE
BASE_RUN=45960681379
BASE_ART=15181211611
BASE_DIGEST="49a16a47523a7d7895fb0428cf21a527027eb2529ba0815618f4c5e1c3079fdd"
SLOTS={"director":D(".45"),**{"art-"+x:D(".55") for x in "ABC"},**{"design-"+x:D(".65") for x in "ABC"},"audit":D(".75")}
def need(ok,msg):
    if not ok: raise ValueError(msg)
def gate():
    need(os.getenv("GITHUB_REPOSITORY")==v.REPO and os.getenv("GITHUB_REF")=="refs/heads/"+v.BRANCH and os.getenv("GITHUB_EVENT_NAME")=="push" and os.getenv("GITHUB_RUN_ATTEMPT")=="1","Not one-shot push")
    e=v.read(os.environ["GITHUB_EVENT_PATH"]); h=os.environ["GITHUB_SHA"]; before=e["before"]
    need(not e.get("forced") and not e.get("deleted") and e["after"]==h and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"Wrong commit")
    need(v.git("diff","--name-status",before,h)=="A\t"+LAUNCH,"Only new workflow permitted")
    need(not v.gh("repos/"+v.REPO+"/commits?"+urlencode({"sha":before,"path":LAUNCH,"per_page":1})),"Already launched")
    need(v.gh("repos/"+v.REPO+"/git/ref/heads/"+v.BRANCH)["object"]["sha"]==h,"Branch moved")
    pr=v.gh("repos/"+v.REPO+"/pulls/198")
    need(pr["state"]=="open" and pr["head"]["sha"]==SOURCE,"PR moved")
    reg=v.gh("repos/"+v.REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    import base64
    doc=json.loads(base64.b64decode(v.gh("repos/"+v.REPO+"/git/blobs/"+reg["sha"])["content"]))
    w=next(x for x in doc["active"] if x["id"]==v.ID)
    need(w["owner"]==v.OWNER and w["ownership_revision"]==14 and w["ownership_state"]=="claimed","Studio ownership moved")
    need(dt.datetime.now(dt.timezone.utc).date()<dt.date(2026,12,2),"Expired")
    # Previous one-shot aborted before the credential-bearing step; do not pay twice.
    previous=v.gh("repos/"+v.REPO+"/actions/runs/46024224894")
    need(previous["head_sha"]=="d640b511c4fa4c7a79718b33099d581cdfc591a6" and previous["conclusion"]=="failure","Previous run not conclusively failed")
    jobs=v.gh("repos/"+v.REPO+"/actions/runs/46024224894/jobs")["jobs"]
    need(len(jobs)==1 and jobs[0]["id"]==140591339766,"Prior job identity")
    steps={x["name"]:x["conclusion"] for x in jobs[0]["steps"]}
    need(steps.get("Validate unique authorization, exact source, studio lease and budget")=="failure" and steps.get("Three independent artistic proposals and independent weighted audit")=="skipped","Prior spend not excluded")
    need(sum(SLOTS.values())<=D("5.00"),"Reservation cap")
    print("PROFILE_PROOF_AUTHORIZED",ID,"USD 5.00 independent ceiling",flush=True)
def baseline(dest):
    dest=P(dest);dest.mkdir()
    run=v.gh("repos/"+v.REPO+"/actions/runs/"+str(BASE_RUN))
    need(run["conclusion"]=="success","Baseline not complete")
    a=next(x for x in v.gh("repos/"+v.REPO+"/actions/runs/"+str(BASE_RUN)+"/artifacts")["artifacts"] if x["id"]==BASE_ART)
    need(a["digest"]=="sha256:"+BASE_DIGEST and not a["expired"],"Baseline manifest mismatch")
    f=dest/"base.zip"
    with f.open("xb") as out:subprocess.run(["gh","api","repos/"+v.REPO+"/actions/artifacts/"+str(BASE_ART)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
    need(f.stat().st_size<60*1024*1024 and v.sha(f.read_bytes())==BASE_DIGEST,"Baseline bytes mismatch")
    names=["summary.json","round-2/theme.json","round-2/captures/phone-Overview.png","round-2/captures/compact-Overview.png","round-2/captures/landscape-Overview.png"]+["art/"+x+".png" for x in ("hall","relic","atlas-1","atlas-2","atlas-3")]
    with zipfile.ZipFile(f) as z:
        for n in names:
            p=dest/n;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    f.unlink()
    s=v.read(dest/"summary.json")
    need(s["source_sha"]==SOURCE and s["final_path"]=="round-2" and not s["quality_target_met"],"Incorrect baseline")
    print("PROFILE_BASELINE_VERIFIED",flush=True)
def exercise(source,projection,old,out,live):
    source,old,out=map(P,(source,old,out));out.mkdir()
    data=v.read(projection);need(data["sourceSha"]==SOURCE and data["fixture"] is True and not any(p["publicRankings"] for p in data["profiles"]),"Invalid canonical projection")
    theme=v.read(old/"round-2/theme.json")
    layout.validate_pages(theme["pages"]);layout.validate_css(theme["css"])
    v.dump(out/"projection.json",data)
    if not live:
        layout.compose(data,source,old/"art",theme["pages"],theme["css"],out/"fixture")
        print("PROFILE_OFFLINE_PROOF_OK",flush=True);return
    gate()
    key=os.environ.get("OPENAI_API_KEY");need(bool(key),"No key")
    v.SLOTS=SLOTS;l=v.Ledger(out)
    def ask(slot,model,instructions,content,images=(),maxout=3000):
        p=out/(slot+"-input.txt");p.write_text(content.encode("utf-8")[:22000].decode("utf-8",errors="ignore"),encoding="utf-8")
        payload,manifest=v.studio.make_payload("visual" if images else "programming",model,p,images)
        payload["instructions"]=instructions+" Return exactly one JSON object, no markdown."
        payload["max_output_tokens"]=maxout
        for c in payload["input"][0]["content"]:
            if c["type"]=="input_image":c["detail"]="high"
        row=l.reserve(slot);print("PROFILE_SLOT_START",slot,flush=True)
        raw=v.studio.request_api(payload,key);l.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
        rep=v.studio.report("visual" if images else "programming",payload,manifest,raw);v.dump(out/(slot+"-receipt.json"),rep)
        obj=v.json_result(rep["response"]);v.dump(out/(slot+"-output.json"),obj);print("PROFILE_SLOT_COMPLETE",slot,row["estimated_usd"],flush=True);return obj
    basepic=old/"round-2/captures/phone-Overview.png"
    common="Only Overview/Perfil y Prestigio. No new mechanics, labels, invented rankings or hidden data. Actual source: "+SOURCE+"\n"+(source/"contracts/ART_DIRECTION.md").read_text()[:4000]+"\n"+(source/"contracts/IMPLEMENTATION.md").read_text()[:3000]+"\nExact C# QA profile: "+json.dumps(data["profiles"][-1]["sections"][0],ensure_ascii=False)[:7000]+"\nEarlier rejected baseline 6.5/10: routine metric grid, generic frames, insufficient identity/status. Commercial aspiration Whiteout Survival and Call of Dragons: hierarchy, tactile materials, legibility, distinct Valoria identity, not copies. No commercial screenshots supplied; do not claim a direct comparison."
    d=ask("director","gpt-6.1-terra","Senior art director: propose THREE fundamentally different compositions for the SAME actual profile. A monumental heraldic identity, B prestige chronicle / progression, C immersive court / identity composition, or better alternatives. Each must distinguish structure not just color. Return {A:{concept,art_prompt},B:{concept,art_prompt},C:{concept,art_prompt}}. Art prompt is ONE standalone original backdrop or hero object, no words, numbers or UI. Real text and controls stay separate. Be highly specific about focal composition and visual contrast.",common,(basepic,source/"assets/identity_crest.png"))
    need(set(d)==set("ABC") and all(set(x)=={"concept","art_prompt"} and all(isinstance(y,str) and 20<=len(y)<=2400 for y in x.values()) for x in d.values()),"Director contract")
    results={}
    import shutil
    for x in "ABC":
        a=out/("art-"+x);shutil.copytree(old/"art",a)
        row=l.reserve("art-"+x);print("PROFILE_SLOT_START","art-"+x,flush=True)
        payload=v.image.make_payload("Standalone premium Valoria game UI illustration, original adult medieval fantasy, cinematic depth, tactile material, aged gold, restrained amethyst, no words, letters, numerals, interface or frames. "+d[x]["art_prompt"])
        payload["quality"]="high"
        raw=v.image.generate(payload,key);l.finish(row,raw.get("usage"),v.image.MODEL)
        b,meta=v.image.unpack(raw);(a/"hall.png").write_bytes(b);meta.update(estimated_usd=row["estimated_usd"],prompt=payload["prompt"]);v.dump(out/("art-"+x+"-receipt.json"),meta)
        with Image.open(a/"hall.png") as im:
            im.thumbnail((1000,1000));im.convert("RGB").save(a/"hero-input.jpg",quality=90)
        instr="""You are an independent senior UI artist AND technical integrator. Produce a distinctive Overview composition, not a description or recolor. Return {page:{outer,lead,row,art},css:string}. art MUST be hall. outer is HTML only tags div,section,article,span,aside,figure,header; attributes ONLY class="x-..." (lowercase). Tokens {{heading}},{{art}},{{lead}},{{rows}} exactly once; lead before rows. lead and row HTML each contains {{icon}},{{title}},{{detail}},{{state}} exactly once; no visible hardcoded text. CSS MUST scope EVERY selector to .page-Overview, no @ rules, no url, no external assets, no comments, no generated content except empty. Runtime wraps lead/rows in .row with data-key, .lead, .icon,.title,.detail,.state. Use data-key selectors to distinguish real prestige metrics, creative hierarchy, original hero art. Preserve every row, no display:none, no clipped text, >=11px text, 320-390px mobile, landscape. Make primary visual hierarchy radically different from baseline. CSS 500-12000 characters. Other pages must not change."""
        obj=ask("design-"+x,"gpt-6.2-sol",instr,common+"\nYour unique direction: "+json.dumps(d[x],ensure_ascii=False)+"\nPrevious CSS for context:\n"+theme["css"][:6500],(basepic,a/"hero-input.jpg"),6500)
        try:
            need(set(obj)=={"page","css"} and obj["page"]["art"]=="hall","Candidate contract")
            css=layout.validate_css(obj["css"])
            need("@" not in css and len(css)<=12000,"Only scoped plain CSS")
            for selectors in re.findall(r"([^{}]+)\{",css):
                need(all(s.strip().startswith(".page-Overview") for s in selectors.split(",")),"CSS escaped Overview scope")
            pages=dict(theme["pages"]);pages["Overview"]=obj["page"];layout.validate_pages(pages)
            dest=out/("candidate-"+x)
            rows=layout.compose(data,source,a,pages,theme["css"]+"\n"+css,dest)
            results[x]={"passed":True,"capture_count":len(rows),"concept":d[x]["concept"],"frame":str(dest/"captures/phone-Overview.png")}
        except Exception as e:
            results[x]={"passed":False,"error":str(e)[:600],"concept":d[x]["concept"]}
        v.dump(out/"candidate-status.json",results)
    passed=[x for x in "ABC" if results[x]["passed"]]
    need(passed,"No candidate passed technical QA; stop without more paid calls")
    sheet=Image.new("RGB",(390*(1+len(passed)),868),"#101018");draw=ImageDraw.Draw(sheet)
    for i,(label,p) in enumerate([("BASELINE",basepic)]+[(x,P(results[x]["frame"])) for x in passed]):
        sheet.paste(Image.open(p).convert("RGB"),(390*i,24));draw.text((390*i+6,4),label,fill="white")
    sheet.save(out/"comparison.jpg",quality=92)
    audit=ask("audit","gpt-6.1-terra","Independent art auditor. Inspect the ACTUAL matched 390x844 screenshots; score baseline and every visible candidate separately 0..10 for resources, integration, coherence, composition, materials, readability, polish. Return {scores:{BASELINE:{resources,integration,coherence,composition,materials,readability,polish},...},findings:{BASELINE: string,...},winner: candidate label or null}. Critique concrete visible defects and differences; no inflated scores, no production/native claim, no direct competitor screenshot comparison. A candidate below 8 weighted is not premium.",common,(out/"comparison.jpg",))
    weights={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
    need(set(audit["scores"])==set(["BASELINE"]+passed),"Audit missing candidates")
    scores={}
    for k,s in audit["scores"].items():
        need(set(s)==set(weights) and all(type(n) in (float,int) and 0<=n<=10 for n in s.values()),"Invalid rubric")
        scores[k]=round(sum(s[c]*weights[c] for c in weights)/100,3)
    winner=max(passed,key=lambda x:scores[x]);accepted=scores[winner]>=8 and scores[winner]>scores["BASELINE"]
    report={"mission":ID,"source_sha":SOURCE,"candidates":results,"weighted_scores":scores,"winner":winner,"browser_art_gate_met":accepted,"native_unity":False,"commercial_reference_screenshots":False,"production_approved":False,"estimated_usd":str(sum(D(r["estimated_usd"]) for r in l.rows)),"billing_verified":False,"api_calls":len(l.rows)}
    v.dump(out/"summary.json",report);print("PROFILE_PROOF_RESULT",json.dumps(report,ensure_ascii=False),flush=True)
if __name__=="__main__":
    mode=sys.argv[1]
    if mode=="--gate":gate()
    elif mode=="--baseline":baseline(sys.argv[2])
    elif mode in ("--fixture","--live"):exercise(*sys.argv[2:6],mode=="--live")
    else:raise ValueError("Explicit mode required")
