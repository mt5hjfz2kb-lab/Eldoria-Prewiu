"""One bounded R4 studio mission with actual 2.5D evidence and source-linked art.
No arbitrary code generation, no copying commercial game imagery into the game.
"""
import hashlib,json,os,subprocess,sys,base64,math
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
BRANCH="candidate/corona-march-r4-modular25d-20261126"
PREV="0aa89f7dda367ba3afc3a666a8c0574198ad9c8f"
def read(p):return json.loads(Path(p).read_text(encoding="utf-8"))
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def need(ok,msg):
    if not ok:raise ValueError(msg)
def parse(raw):return json.loads(raw.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
def digest(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    need(os.environ["GITHUB_RUN_ATTEMPT"]=="1","No paid retry")
    need(gh(f"repos/{repo}/git/ref/heads/{BRANCH}")["object"]["sha"]==head,"Wrong candidate")
    need(gh(f"repos/{repo}/git/ref/heads/candidate/corona-march-r3-studio-20261126")["object"]["sha"]==PREV,"R3 no longer pinned")
    for run,artifact,sha in [(45900917700,15156507955,PREV),(45900917700,15156298587,PREV),(45851071206,15136075692,None),(45812100129,15113766981,None)]:
        r=gh(f"repos/{repo}/actions/runs/{run}")
        need(r["conclusion"]=="success" and (sha is None or r["head_sha"]==sha),"Unexpected source run")
        need(any(a["id"]==artifact and not a["expired"] for a in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"]),"Missing exact artifact")
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    need(lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18,"Studio lease changed")
    need(D("5")+D(".87")+D(".20")+D(".30")+D(".10")+D(".22")+D(".07")+D(".24")==D("7"),"Budget arithmetic")
    e=read("native/evidence.json")
    score=read("scores/scorecard.json")
    need(e["sourceSha"]==PREV and not e["defects"] and e["uiOnlyNativeCamera"] and score["source"]==PREV and score["valid_visual"] and score["after"]==7.0,"R3 unverified")
    for f in e["frames"]:need(digest(Path("native")/f["file"])==f["sha256"],"R3 pixel digest changed")
    proof=read("city/capture-source.json")
    need(proof["player_source_sha"]=="41c124f4b9ef10cd9b4a9d87562a7d2727b730b6" and proof["dpr"]==1,"Unexpected city source")
    out=Path("march-r4-production");out.mkdir(exist_ok=False)
    write(out/"budget.json",{"owner_cap":"7.00","previous_held":"6.37","director":".10","artist":".22","asset_review":".07","final_two_reviews":".24","unallocated":"0.00","invoice_verified":False,"no_retry":True})
    # Exact R3 and actual city reference, not generated mockups.
    im=Image.new("RGB",(792,870),"#172028")
    for x,p,label in [(0,"native/mixed-after.png","R3 actual Unity / mixed"),(402,"city/bastion-1.png","Bastion I actual WebGL")]:
        with Image.open(p) as f:
            need(f.size==(390,844),"Unexpected reference size")
            im.paste(f.convert("RGB"),(x,26))
        ImageDraw.Draw(im).text((x+5,5),label,fill="white")
    im.save(out/"r3-and-bastion1.png")
    ref=Image.open("refs/reference.jpg").convert("RGB");ref.thumbnail((1240,1800));ref.save(out/"commercial.jpg",quality=93)
    docs=[]
    for name in ("ART_GROUNDING_RULES.md","PRODUCTION_GUIDE_BASTION_I_20261120.md","CORONA_BRECHA_ART_DIRECTION_20261120.md"):
        p=Path("docs/valoria-modular-25d")/name
        docs.append({"name":name,"sha256":digest(p)})
    write(out/"source.json",{"r3_run":45900917700,"r3_source":PREV,"city_run":45851071206,"bastion1_sha256":digest("city/bastion-1.png"),"commercial_artifact":15113766981,"commercial_sha256":digest("refs/reference.jpg"),"manuals":docs})
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import studio,mission,generate_candidate as artist
    studio.MAX_OUTPUT=2200
    original=studio.make_payload
    def high(*args,**kwargs):
        p,m=original(*args,**kwargs)
        for item in p["input"][0]["content"]:
            if item["type"]=="input_image":item["detail"]="high"
        return p,m
    studio.make_payload=high
    mission.RESERVES["director"]=D(".10");mission.RESERVES["artist"]=D(".22");mission.RESERVES["auditor"]=D(".07")
    ledger=mission.Ledger(out/"ledger.json",D(".39"))
    evidence=mission.textfile(out/"learned-method.txt", """Bastion I production lessons from the actual source-pinned manuals:
One continuous environment, independent objects, feet/pivots, coherent projected depth, warm NW light/cool shadow, contact footprints with alpha transitions, shared textures/materials, no flattened full scene and no stacking unrelated art. Restrained terrain/space; architecture focal, supporting objects subordinate. Separate real controls and canonical game data. No layer of old render under another competing render. Original painted art, not procedural SVG as final. Never force 9-slice on whole illustrations; preserve aspect/visible crop. Verify actual native/mobile frames, states, hashes, real input and persistence, not just compilation. Commercial refs for hierarchy/composition, no copied IP.
R3 actual weighted evaluation 7.00: improved organization, gold/obsidian CTA, native defect count 0; both auditors still report leader reduced, narrow side cards, tiny troop thumbnail and secondary status/labels. Reference about 8.29.
We cannot simply put a 2.5D city under a UI: adapt modular technique to a coherent command room with an independent illustrated backdrop, existing real portrait sprites, separate contact shadows, controlled depth and real controls. Painted portraits have baked backgrounds; keep them as physical framed command dossiers, not cutout people unless source alpha is genuine. No replacements of identities, no fake stats.
At 390x844: retain all existing actions, three hero slots including lock, CTA, tier, quantity, slider and six metrics. Keep 44px real target at 390x844. Keep landscape unchanged. Current 800-unit portrait layout. Compact hero art scene, troop band, summary. Preserve actual portrait and original troop art. Source guides and asset hashes in source.json.""")
    prompt="""Director artístico senior. Estudia imagen 1 (Marchas R3 real y Bastión I 2.5D real) e imagen 2 (referentes comerciales, incluyendo Whiteout Survival y Call of Dragons donde estén presentes). Aplica explícitamente el aprendizaje del Bastión: escenario continuo, perspectiva y luz compartidas, elementos independientes, profundidad y sombras de contacto, composición de un lugar y no collage. La pantalla R3 sigue pareciendo una sucesión de cajas. Encarga una ilustración original única de ambiente para la ZONA DE HÉROES, 1536×1024 horizontal, no toda la pantalla. Sin personajes, retratos, texto, números, marcos de tarjetas, controles, siluetas falsas ni UI. Las figuras/retablos reales se montarán por separado y sus pies se anclarán a una repisa/mesa, con sombras alpha independientes. Debe ser un rincón del bastión, piedra crema cálida, madera envejecida, metal pizarra, luz cálida lateral izquierda y sombras azul violeta, un sutil acento turquesa. La zona del medio está tranquila, la orilla inferior tiene repisa física, sin falsa cuadrícula. Diseña jerarquía visual atrevida: líder más protagonista, lados en segundo plano, tropa con mayor tamaño, resumen de dos columnas con menos recuadros, datos legibles y CTA existente. Elige leader_share entre .43 y .49 y hero_fraction entre .33 y .36, dado que el espacio operativo debe mantenerse. Decide stage_tone 'warm_stone' o 'twilight_stone', portrait_rim 'thin_brass' o 'dark_iron'. Integra física visual sin simular un escenario 3D ni cambiar reglas. JSON puro con exactamente leader_share,hero_fraction,stage_tone,portrait_rim,lessons,stage_brief,hero_composition,troop_composition,metric_composition,acceptance. Los 6 últimos campos texto breve, stage_brief en inglés máximo 1400 caracteres. No inventar resultados; describe cómo lo refutarías."""
    plan=parse(mission.ask("visual","gpt-6.1-terra",evidence,(str(out/"r3-and-bastion1.png"),str(out/"commercial.jpg")),ledger,"director",out,prompt,effort="none"))
    keys={"leader_share","hero_fraction","stage_tone","portrait_rim","lessons","stage_brief","hero_composition","troop_composition","metric_composition","acceptance"}
    need(set(plan)==keys,"Unrecognized plan")
    need(type(plan["leader_share"]) in (int,float) and .43<=plan["leader_share"]<=.49 and type(plan["hero_fraction"]) in (int,float) and .33<=plan["hero_fraction"]<=.36,"Geometry outside safe bounds")
    need(plan["stage_tone"] in ("warm_stone","twilight_stone") and plan["portrait_rim"] in ("thin_brass","dark_iron"),"Unrecognized material")
    need(all(isinstance(plan[k],str) and 1<=len(plan[k])<=1400 for k in keys-{"leader_share","hero_fraction","stage_tone","portrait_rim"}),"Bad text")
    write(out/"plan.json",plan);print("MARCH_R4_DIRECTION",json.dumps(plan,ensure_ascii=False),flush=True)
    brief="""Original modular scenic asset, a single continuous architectural interior for a hero command gallery in Eldoria. Horizontal 3:2, high-quality painted 2.5D fantasy strategy environment. Same warm stone, weathered dark slate and aged wood as the actual Valoria Bastion I environment. A believable deep medieval hall, slightly elevated front three-quarter viewpoint, shared warm upper-left daylight and cool ambient shadows. Quiet center, broad single wooden/stone ledge across the lower edge where separate physical hero portraits will sit, stone columns at outer edges, restrained architecture and subtle turquoise reflections. Background only, no people, portraits, heroes, troops, silhouettes, written text, letters, numbers, graphic frames, slots, cards, icons, controls, interface, mockup, distinct divided panels, floating props or copy of another game's art. Keep the middle and lower parts uncluttered so live UI and independent objects can sit in front. Painted believable perspective and consistent light; full bleed; no white outline. Artist direction: """+plan["stage_brief"]
    (out/"artist-brief.txt").write_text(brief,encoding="utf-8")
    payload=artist.make_payload(brief);payload.update(quality="high",complexity="medium",size="1536x1024")
    row=ledger.reserve("artist")
    raw,meta=artist.unpack(artist.generate(payload,os.environ.get("OPENAI_API_KEY")))
    (out/"march_command_gallery.png").write_bytes(raw)
    cost=mission.image_cost(meta.get("usage"))
    meta.update(quality=payload["quality"],size=payload["size"],estimated_usd=str(cost) if cost is not None else None,brief_sha256=hashlib.sha256(brief.encode()).hexdigest())
    write(out/"artist.json",meta);ledger.complete(row,cost,meta.get("usage"))
    with Image.open(out/"march_command_gallery.png") as f:
        need(f.size==(1536,1024),"Unexpected artist dimensions")
        f.convert("RGB").resize((1152,768),Image.Resampling.LANCZOS).save(out/"gallery-review.jpg",quality=92)
    studio.MAX_OUTPUT=1100
    b=mission.textfile(out/"asset-review.txt",json.dumps({"direction":plan,"purpose":"Environment layer only, placed behind real independent portraits and controls. Compare with real Bastion I and R3; do not approve final UI."},ensure_ascii=False))
    v=parse(mission.ask("visual","gpt-6.1-terra",b,(str(out/"r3-and-bastion1.png"),str(out/"gallery-review.jpg")),ledger,"auditor",out,"Independent artist. Compare actual R3/Bastion I with the generated gallery. Assess material family, shared illumination, physical ledge, perspective, negative space and whether actual separate objects can integrate. Reject if text, fake UI, baked portraits, copied art, bad perspective, excessive detail or wrong visual family. JSON exact {verdict:'accept'|'reject',reason:string,risks:string,placement:string}. No final art approval.",effort="none"))
    need(set(v)=={"verdict","reason","risks","placement"} and v["verdict"] in ("accept","reject"),"Bad asset review")
    write(out/"review.json",v)
    print("MARCH_R4_ASSET",json.dumps(meta,ensure_ascii=False),flush=True);print("MARCH_R4_ASSET_REVIEW",json.dumps(v,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
