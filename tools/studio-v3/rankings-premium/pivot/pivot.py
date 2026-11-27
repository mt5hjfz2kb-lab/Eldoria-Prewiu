"""Creative second stage, same owner-authorized $5 mission, no previous budget reuse."""
import sys,pathlib,json,hashlib,os,base64,subprocess,zipfile,datetime as dt
from decimal import Decimal as D
P=pathlib.Path;HERE=P(__file__).resolve().parent;sys.path.insert(0,str(HERE.parent))
import run as v
from layout import compose,validate_pages,validate_css,ATLAS,PAGES
from PIL import Image,ImageDraw
REPO=v.REPO;BRANCH=v.BRANCH;SOURCE=v.SOURCE;OWNER=v.OWNER
MESSAGE="Authorize PR198 creative pivot within remaining owner USD 5 ceiling"
REQUEST="tools/studio-v3/rankings-premium/pivot/launch.json"
FIRST=45959632425;FIRST_SHA="0fb7c09a2ea541012c93f41951225ae0a4eed25f";ART=15180067001;DIGEST="5a435a276cbf7a3c3f82a3bfb4b5629774ca90edd7ec95da3995447eeec7b07d"
SLOTS={"director":D(".25"),"atlas-1":D(".45"),"atlas-2":D(".45"),"atlas-3":D(".45"),"ux":D(".35"),"integrator":D(".45"),"audit":D(".15"),"css-repair":D(".30"),"audit-final":D(".15")}
def need(x,m):
    if not x:raise ValueError(m)
def prepare(dest):
    dest=P(dest);dest.mkdir()
    run=v.gh("repos/"+REPO+"/actions/runs/"+str(FIRST));need(run["head_sha"]==FIRST_SHA and run["conclusion"]=="success","First proof")
    items=v.gh("repos/"+REPO+"/actions/runs/"+str(FIRST)+"/artifacts")["artifacts"];a=next(x for x in items if x["id"]==ART)
    need(a["digest"]=="sha256:"+DIGEST and a["expired"] is False,"Artifact mismatch")
    f=dest/"first.zip"
    with f.open("xb") as out:subprocess.run(["gh","api","repos/"+REPO+"/actions/artifacts/"+str(ART)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
    need(f.stat().st_size<25*1024*1024 and v.sha(f.read_bytes())==DIGEST,"Zip bytes mismatch")
    names=["summary.json","ledger.json","round-2/audit-1.jpg","round-2/audit-2.jpg","art/hall.png","art/relic.png","director-output.json"]
    with zipfile.ZipFile(f) as z:
        for n in names:
            p=dest/n;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    f.unlink()
    r=v.read(dest/"summary.json");l=v.read(dest/"ledger.json")
    need(r["quality_target_met"] is False and r["source_sha"]==SOURCE and r["estimated_usd"]=="0.17856840","Prior result changed")
    need(len(l["entries"])==9 and all(e["state"]=="completed" and e["estimated_usd"] is not None for e in l["entries"]) and sum((D(e["estimated_usd"]) for e in l["entries"]),D(0))==D(r["estimated_usd"]),"Cost uncertainty")
    print("PIVOT_PRIOR_VERIFIED",flush=True)
def gate():
    need(os.environ.get("GITHUB_REPOSITORY")==REPO and os.environ.get("GITHUB_REF")=="refs/heads/"+BRANCH and os.environ.get("GITHUB_EVENT_NAME")=="push" and os.environ.get("GITHUB_RUN_ATTEMPT")=="1","Wrong dispatch")
    ev=v.read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=ev["before"]
    need(ev["after"]==h and not ev.get("forced") and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"Not exact launch")
    need(v.git("diff","--name-status",before,h)=="A\t"+REQUEST and P(REQUEST).read_bytes()==(HERE/"grant.json").read_bytes(),"Immutable grant")
    from urllib.parse import urlencode
    need(not v.gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1})),"Previously dispatched")
    need(v.gh("repos/"+REPO+"/git/ref/heads/"+BRANCH)["object"]["sha"]==h,"Head changed")
    ci=v.gh("repos/"+REPO+"/actions/workflows/studio-v3-rankings-pivot.yml/runs?"+urlencode({"head_sha":before,"status":"success","per_page":20}))["workflow_runs"]
    need(any(x["head_sha"]==before and x["conclusion"]=="success" for x in ci),"No offline proof")
    need(v.gh("repos/"+REPO+"/pulls/198")["head"]["sha"]==SOURCE,"PR moved")
    meta=v.gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main");reg=json.loads(base64.b64decode(v.gh("repos/"+REPO+"/git/blobs/"+meta["sha"])["content"]));w=next(x for x in reg["active"] if x["id"]==v.ID)
    need(w["owner"]==OWNER and w["ownership_revision"]==11 and w["ranking_mission"]["followup"]["cap_usd"]=="3.00" and w["ranking_mission"]["first_run"]["estimated_usd"]=="0.17856840","Lease or grant moved")
    need(sum(SLOTS.values())==D("3.00") and D("0.17856840")+sum(SLOTS.values())<=D("5.00"),"Total mission cap")
    need(dt.datetime.now(dt.timezone.utc).date()<dt.date(2026,12,2),"Expired")
    print("PIVOT_AUTHORIZED",h,flush=True)
def run(source,projection,old,out,live):
    source,old,out=map(P,(source,old,out));out.mkdir()
    data=v.read(projection);need(data["sourceSha"]==SOURCE and data["fixture"] is True,"Projection")
    art=out/"art";art.mkdir()
    for n in ("hall","relic"):(art/(n+".png")).write_bytes((old/"art"/(n+".png")).read_bytes())
    # Offline proof uses original assets as atlas cell fixtures, not paid.
    if not live:
        for n in range(3):
            with Image.open(art/"hall.png") as im:im.save(art/("atlas-"+str(n+1)+".png"))
        pages={k:{"outer":'<section class="x-main">{{art}}{{heading}}'+("{{back}}" if k=="DiscoveryDetail" else "")+'<div class="x-lead">{{lead}}</div><div class="x-rest">{{rows}}</div></section>',"lead":'<div class="x-entry">{{icon}}<div class="x-copy">{{title}}{{detail}}{{state}}</div></div>',"row":'<div class="x-entry">{{icon}}<div class="x-copy">{{title}}{{detail}}{{state}}</div></div>',"art":"hall" if k in ("Overview","Rankings") else "relic"} for k in PAGES}
        css="body{background:#1a1428;color:#f1e9d4;font-family:system-ui}header{padding:6px;display:flex;align-items:center;gap:8px;background:#30223f}header img{width:32px}header strong{flex:1}header select{max-width:100px}#nav{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:4px;padding:6px}#nav button{min-height:44px;background:#30223f;color:#f1e9d4;border:1px solid #c99c54}#nav img{width:22px;vertical-align:middle}main{padding:10px}.section-art{height:100px;overflow:hidden}.artwork{width:100%;height:100%;object-fit:cover}.row{border:1px solid #c99c54;padding:8px;margin-bottom:8px;background:#30223f;color:#f1e9d4;width:100%;text-align:left}.x-entry{display:flex;gap:12px;align-items:center}.icon{width:45px;flex:none}.x-copy{min-width:0}.title,.detail,.state{display:block;font-size:14px}.state{font-size:11px}.empty{display:none}footer{text-align:center;font-size:10px;padding:8px}.back{height:44px;width:100%} @media(orientation:landscape){#nav{grid-template-columns:repeat(6,1fr)}}"
        compose(data,source,art,pages,css,out/"round-1");print("PIVOT_FIXTURE_PASS",flush=True);return
    gate()
    key=os.environ.get("OPENAI_API_KEY");need(bool(key),"Missing key")
    v.SLOTS=SLOTS
    l=v.Ledger(out)
    def ask(slot,model,instructions,content,images=(),maxout=3000):
        p=out/(slot+"-input.txt");p.write_text(content.encode("utf-8")[:22500].decode("utf-8",errors="ignore"),encoding="utf-8")
        payload,manifest=v.studio.make_payload("visual" if images else "programming",model,p,images);payload["instructions"]=instructions+" Return one strict JSON object, no Markdown."
        payload["max_output_tokens"]=maxout
        for c in payload["input"][0]["content"]:
            if c["type"]=="input_image":c["detail"]="high"
        row=l.reserve(slot);print("PIVOT_SLOT",slot,flush=True);raw=v.studio.request_api(payload,key);l.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
        report=v.studio.report("visual" if images else "programming",payload,manifest,raw);v.dump(out/(slot+"-receipt.json"),report)
        obj=v.json_result(report["response"]);v.dump(out/(slot+"-output.json"),obj);return obj
    common=(HERE.parent/"BRIEF.md").read_text()+"\n"+(source/"contracts/IMPLEMENTATION.md").read_text()[:4000]+"\n"+(source/"contracts/ART_DIRECTION.md").read_text()[:4500]+"\nActual previous audit: "+json.dumps(v.read(old/"summary.json")["final_audit"],ensure_ascii=False)+"\nActual C# QA rows: "+json.dumps(data["profiles"][-1],ensure_ascii=False)[:6500]
    priorpics=(old/"round-2/audit-1.jpg",old/"round-2/audit-2.jpg")
    d=ask("director","gpt-6.1-terra","Director artístico de Valoria: el candidato anterior fracasó. Cambia la composición, no solo color. Dales libertad real de estilo: composición de perfil protagonista y estadísticas destacadas, ranking honesto y legible, vitrinas de trofeos, hallazgos, heráldica y monumentos con iconografía individual. Art assets y texto dinámico separados. Devuelve {direction: texto <=2500 caracteres, atlas_style: texto <=1200 caracteres}; critica paneles repetidos y exceso de nav. Usa evidencia real.",common,priorpics)
    need(set(d)=={"direction","atlas_style"} and all(isinstance(x,str) and 0<len(x)<=3000 for x in d.values()),"Director output")
    descriptions=[
      ["an ornate crossed sword power crest","miniature fortified castle","two crossed shields and spears","watchtower with lantern","boar tusk hunter's medal","map and compass","Valoria royal amethyst crest","golden laurel trophy","sealed dark unknown medallion"],
      ["burned ash parchment with ember","amethyst geological fissure","frontier map and watchtower","noble knightly honor medal","purple embroidered banner","fortress milestone medallion","silver sentry shield","bronze hunting tusk medal","explorer compass medal"],
      ["miniature founder stone statue","carved stone fountain","mounted hunting antlers trophy sculpture","memorial stone with amethyst fissure","ash memorial brazier","radiant gold prestige crown","open chronicle book","locked dark wax seal","faceted amethyst crystal"]
    ]
    for i,ids in enumerate(descriptions):
        slot="atlas-"+str(i+1);row=l.reserve(slot)
        brief="Exactly nine original hand-painted inventory item icons arranged in a precise 3 by 3 grid on a square canvas. Each icon fully contained in its own equal square cell, with generous transparent padding, no overlaps. Each independent sculptural emblem is large, ornate yet readable, tactile aged gold, stone, silver and subtle amethyst, dark adult fantasy Valoria. No letters, numerals, labels, UI frame, captions, borders or separators. Row-major cell subjects: "+ "; ".join(str(j+1)+". "+x for j,x in enumerate(ids))+". Style direction: "+d["atlas_style"][:1200]
        payload=v.image.make_payload(brief);payload["quality"]="medium";payload["background"]="transparent"
        print("PIVOT_SLOT",slot,flush=True);raw=v.image.generate(payload,key);l.finish(row,raw.get("usage"),v.image.MODEL);b,meta=v.image.unpack(raw);(art/(slot+".png")).write_bytes(b);meta.update(estimated_usd=row["estimated_usd"],quality="medium",prompt=payload["prompt"]);v.dump(out/(slot+"-receipt.json"),meta)
    sheet=Image.new("RGB",(1200,400),"#20182e")
    for i in range(3):
        with Image.open(art/("atlas-"+str(i+1)+".png")) as im:
            im=im.convert("RGBA");im.thumbnail((400,400));sheet.paste(im,(i*400,0),im)
    sheet.save(out/"atlas-reference.jpg",quality=90)
    template_contract="""Author original HTML structure for seven existing views; no executable code or hardcoded visible text. JSON exact keys Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments,DiscoveryDetail. Each exactly {outer,lead,row,art}. art is hall or relic. outer is HTML using tags div,section,article,span,aside,figure,header ONLY; attributes ONLY class="x-... x-..." lowercase hyphen classes. Must contain each token exactly once: {{heading}},{{art}},{{lead}},{{rows}} (DiscoveryDetail also {{back}}). lead must precede rows. lead and row templates each contain {{icon}},{{title}},{{detail}},{{state}} exactly once. No other text, no scripts, no styles. Different page structures; group content in intentional layouts. Runtime wraps every lead/row in .row, lead .lead, state .state, icon .icon, title .title, detail .detail. Whole page .page-Overview etc. Actual art, data and state injected automatically. Design different hierarchy for each page; no fabricated functions."""
    pages=validate_pages(ask("ux","gpt-6.2-sol",template_contract,common[:12500]+"\nDirector: "+json.dumps(d,ensure_ascii=False),(priorpics[0],out/"atlas-reference.jpg"),5500))
    css_contract="""You are the visual implementation specialist, NOT consultant. Author complete CSS for this exact dynamic DOM and authored templates. Return JSON exactly {css: string}. Use real layout: #app flex column, header.global-header with .brand,strong,#badge,#level; #nav buttons each .nav-icon and span, aria-current=true; main scroll area containing .page.page-Overview etc, all x-* classes from templates. {{heading}} yields .section-heading with .chapter,h1. {{art}} yields .section-art img.artwork (original scenic art). {{lead}},{{rows}} are .row data-state attributes, first .lead, possible .action. {{icon}} img.icon original distinct rendered icon; .title,.detail,.state,.state.empty. .back in detail. No url(), @import, @font-face, expression, angle brackets, backslashes, comments or @keyframes. No fabricated labels, content only empty or none. Make a strong premium visual design: true ornament via gradients, depth, distinct page arrangements, large meaningful illustrations and medals, high legibility (all title/detail at least 11px even compact), no clipping, responsive 320x720,390x844,844x390. Nav button widths >=44; use 3 columns portrait, 6 horizontal. Preserve order of rows, show every title/detail, scroll content, no hidden or clipped items. Do NOT use external links or fake player rows. CSS actual authored design, not prose."""
    css=validate_css(ask("integrator","gpt-6.2-sol",css_contract,common[:9500]+"\nDirector:"+json.dumps(d,ensure_ascii=False)+"\nExact templates:"+json.dumps(pages),(out/"atlas-reference.jpg",priorpics[1]),7500)["css"])
    def render(n):
        dest=out/("round-"+str(n));rows=compose(data,source,art,pages,css,dest);return dest,rows
    def audit(slot,dest):
        a=ask(slot,"gpt-6.1-terra","Independent art auditor of actual screenshots. Score all six sections 0..10 on visual quality relative to commercial Valoria target, no technical-to-art equivalence. JSON {scores:{Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments},findings:[concrete observations],decision:'candidate'|'revise'}. Candidate only if each >=8.5, no significant defects.",common[:12000],(dest/"audit-1.jpg",dest/"audit-2.jpg"))
        need(set(a)=={"scores","findings","decision"} and set(a["scores"])==set(PAGES[:-1]) and all(type(x) in (int,float) and 0<=x<=10 for x in a["scores"].values()),"Audit")
        return a
    # If actual layout fails, use a single repair against the failure, never another paid image automatically.
    repaired=False
    try:dest,rows=render(1)
    except Exception as e:
        css=validate_css(ask("css-repair","gpt-6.2-sol",css_contract+" Correct the actual technical failure, preserve design.",json.dumps({"failure":str(e)[:400],"templates":pages,"css":css},ensure_ascii=False),(out/"atlas-reference.jpg",priorpics[0]),7500)["css"])
        repaired=True;dest,rows=render(2)
    a=audit("audit",dest)
    if not repaired and (a["decision"]=="revise" or min(a["scores"].values())<8.5):
        css=validate_css(ask("css-repair","gpt-6.2-sol",css_contract+" Fix the actual independent visual critique with high impact. Do not repeat minor color edits.",json.dumps({"audit":a,"templates":pages,"css":css},ensure_ascii=False),(dest/"audit-1.jpg",dest/"audit-2.jpg"),7500)["css"])
        repaired=True;dest,rows=render(2);a=audit("audit-final",dest)
    cost=sum((D(e["estimated_usd"]) for e in l.rows),D(0));report={"live":True,"pr":198,"source_sha":SOURCE,"api_calls":len(l.rows),"estimated_usd":str(cost),"cumulative_estimated_usd":str(D("0.17856840")+cost),"billing_verified":False,"final_path":dest.name,"frames":len(rows),"final_audit":a,"quality_target_met":a["decision"]=="candidate" and min(a["scores"].values())>=8.5,"native_unity":False,"mcp_executed":False,"pr_integrated":False,"art_approved":False,"published":False}
    v.dump(out/"summary.json",report);print("PR198_PIVOT_PROOF "+json.dumps(report,ensure_ascii=False),flush=True)
if __name__=="__main__":
    if sys.argv[1]=="--prepare":prepare(sys.argv[2])
    elif sys.argv[1]=="--gate":gate()
    else:
        need(sys.argv[1] in ("--fixture","--live") and len(sys.argv)==6,"Explicit mode")
        run(*sys.argv[2:6],sys.argv[1]=="--live")
