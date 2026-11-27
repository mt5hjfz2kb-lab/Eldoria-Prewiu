"""R6 bounded original-alpha hero experiment, only after verified R5 shortfall.
Canonical images inform director and reviewer; the proven artist generation
endpoint receives text briefs, not unverified image-edit parameters.
"""
import hashlib,json,os,subprocess,sys,base64,math
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
from verify_march_r5_budget import verify_budget
R5="f4cba2d776f012c4e108e938c1f37e130a502d3a"
R5_AUDIT="91a8a09a1a1052c9327ad8cdd8632db20cbf7476"
BRANCH="candidate/corona-march-r6-alpha-stage-20261127"
ROOT=Path("Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrecha")
def read(p):return json.loads(Path(p).read_text(encoding="utf-8"))
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def need(ok,msg):
    if not ok:raise ValueError(msg)
def parse(s):return json.loads(s.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
def digest(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def validate_r5_receipts(repo):
    r=gh(f"repos/{repo}/actions/runs/45959243027")
    need(r["head_sha"]==R5_AUDIT and r["conclusion"]=="success" and r["run_attempt"]==1,"Wrong R5 audit")
    p=Path("r5audit/march-r5-audit")
    score=read(p/"scorecard.json")
    need(score["source"]==R5 and score["valid_visual"] and score["native_target_met"] and not score["artistic_target_met"],"R5 not a valid artistic shortfall")
    cost=D(0)
    for j in (1,2):
        l=read(p/f"review-{j}/ledger.json");a=read(p/f"review-{j}/auditor.json")
        need(D(l["cap_usd"])==D(".12") and len(l["reservations"])==1,"Wrong R5 reserve")
        x=l["reservations"][0];v=D(x["estimated_usd"])
        need(x["state"]=="completed" and D(0)<=v<=D(".12") and D(a["cost_estimate_usd"])==v and a["usage"] is not None,"Unverified R5 call")
        cost+=v
    return score,cost
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    need(os.environ["GITHUB_RUN_ATTEMPT"]=="1" and gh(f"repos/{repo}/git/ref/heads/{BRANCH}")["object"]["sha"]==head,"Branch/attempt changed")
    prior=verify_budget(repo,gh)
    score,r5cost=validate_r5_receipts(repo)
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    need(lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18,"Studio lease changed")
    need(D("5")+D("1")+D(".24")+D(".52")+D(".24")==D("7"),"Owner cap exceeded")
    e=read("native/evidence.json")
    need(e["sourceSha"]==R5 and e["actualPrefab"] and e["uiOnlyNativeCamera"] and not e["defects"],"R5 native not valid")
    for f in e["frames"]:need(digest(Path("native")/f["file"])==f["sha256"],"R5 image changed")
    out=Path("r6-production");out.mkdir(exist_ok=False)
    write(out/"budget.json",{"owner_cap":"7.00","original_held":"5.00","conservative_additional_history":"1.00","r5_review_held":".24","r5_estimated_usd":str(r5cost),"this_production":".52","final_review":".24","unallocated":"0.00","invoice_verified":False,"no_retry":True})
    aldric=Image.open(ROOT/"aldric_guardian.jpg").convert("RGB")
    master=Image.open("Unity/Assets/Eldoria/Resources/EldoriaV41/HeroesApprovedMaster.png").convert("RGB")
    need(master.size==(1536,1024),"Canonical master changed")
    # Same exact bottom-origin crop used by RoyalGoldV41SourceArt.HeroPortrait.
    lyra=master.crop((1224,1024-182-180,1372,1024-182))
    ref=Image.new("RGB",(1100,844),"#242329")
    ref.paste(Image.open("native/mixed-after.png").convert("RGB"),(0,0))
    for x,im,name in ((410,aldric,"CANONICAL ALDRIC"),(760,lyra,"CANONICAL LYRA")):
        im=im.copy();im.thumbnail((335,530),Image.Resampling.LANCZOS)
        if name.endswith("LYRA"):im=im.resize((min(300,im.width*2),min(365,im.height*2)),Image.Resampling.LANCZOS)
        ref.paste(im,(x,50));ImageDraw.Draw(ref).text((x,20),name,fill="white")
    ref.save(out/"canonical-and-r5.jpg",quality=92)
    comm=Image.open("refs/reference.jpg").convert("RGB");comm.thumbnail((1240,1800));comm.save(out/"commercial.jpg",quality=92)
    write(out/"source.json",{"r5_run":45957940610,"r5_source":R5,"r5_audit_run":45959243027,"r5_audit_source":R5_AUDIT,"actual_aldric_sha256":digest(ROOT/"aldric_guardian.jpg"),"actual_lyra_master_sha256":digest("Unity/Assets/Eldoria/Resources/EldoriaV41/HeroesApprovedMaster.png"),"lyra_crop_top_origin":[1224,662,1372,842],"references_sha256":digest("refs/reference.jpg")})
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import studio,mission,generate_candidate as artist
    original=studio.make_payload
    def high(*args,**kw):
        p,m=original(*args,**kw)
        for x in p["input"][0]["content"]:
            if x["type"]=="input_image":x["detail"]="high"
        return p,m
    studio.make_payload=high;studio.MAX_OUTPUT=1600
    mission.RESERVES.update({"director":D(".08"),"artist-aldric":D(".18"),"artist-lyra":D(".18"),"auditor":D(".08")})
    ledger=mission.Ledger(out/"ledger.json",D(".52"))
    prior_r4=read("receipts/r4-review/scorecard.json")
    evidence=mission.textfile(out/"learned-evidence.txt",json.dumps({"r5_audit":score["reviews"],"r4_audit":prior_r4["reviews"],"Bastion_2_5D_lessons":"One continuous spatial setting, separate real alpha objects, shared perspective and light, true contact shadows, not baked portraits pasted as scenery. Never copy commercial artwork. R4 failed precisely because baked backgrounds and tiny dark archer made a collage. Retain R5 controls, larger troop, live stats, original hero IDs. Do not redesign shared hero hall.","scope":"Portrait March only; author two genuine transparent waist-up foregrounds; exact existing canon identity must pass independent asset review."},ensure_ascii=False))
    prompt="""Senior Eldoria character art director. Image 1: actual R5 March and actual canonical Aldric and Lyra portraits. Image 2: commercial strategy references for art quality and hierarchy, not IP copying. Write two precise identity-preserving artist briefs for separate original foreground assets. Both need real transparent background, waist-up 3:4-like subject crop, full visible head and shoulders, coherent warm upper-left light, cool subtle violet shadow, painterly semi-realistic premium fantasy strategy aesthetic matching Valoria. Aldric keeps silver hair/beard, exact mature facial identity and steel/gold blue cloak; Lyra keeps the actual canonical heroine face, hair, emerald leather/cloth and archer identity as shown. Do not invent a different hero, age, ethnicity, costume or pose radically. Strong contrast and large faces for mobile. No environment, frames, cards, UI, words, weapons protruding out of canvas, no duplicated figures or backgrounds. Art must integrate on same independent gallery plane and preserve actual hero data. Return JSON exact keys aldric_brief,lyra_brief,shared_light,identity_check,acceptance. Each string <=1400 characters. Honest: briefs do not prove final artwork."""
    plan=parse(mission.ask("visual","gpt-6.1-terra",evidence,(str(out/"canonical-and-r5.jpg"),str(out/"commercial.jpg")),ledger,"director",out,prompt,effort="none"))
    need(set(plan)=={"aldric_brief","lyra_brief","shared_light","identity_check","acceptance"} and all(isinstance(v,str) and 0<len(v)<=1400 for v in plan.values()),"Invalid director plan")
    write(out/"plan.json",plan)
    assets={}
    for who in ("aldric","lyra"):
        brief=("Original Eldoria "+who+" character foreground. Exactly one canonical hero, matching source identity as described by the director. Premium painterly semi-realistic fantasy strategy game, bright warm key from upper left, cool soft violet ambient shadow, same perspective and palette as the companion asset. Waist-up portrait, large face and shoulder armor/cloth, graceful self-contained lower edge, entire head visible, no background or environment, genuine transparent alpha PNG. No frame, text, logo, UI, border, multiple figures, other franchise IP. Do not invent an alternate character. Director brief: "+plan[who+"_brief"]+" Shared light: "+plan["shared_light"])
        (out/(who+"-brief.txt")).write_text(brief,encoding="utf-8")
        payload=artist.make_payload(brief);payload.update(quality="high",complexity="medium",size="1024x1536",background="transparent")
        row=ledger.reserve("artist-"+who)
        raw,meta=artist.unpack(artist.generate(payload,os.environ.get("OPENAI_API_KEY")))
        (out/(who+"-original.png")).write_bytes(raw)
        cost=mission.image_cost(meta.get("usage"))
        meta.update(quality=payload["quality"],size=payload["size"],estimated_usd=str(cost) if cost is not None else None,brief_sha256=hashlib.sha256(brief.encode()).hexdigest(),background_requested="transparent")
        write(out/("artist-"+who+".json"),meta);ledger.complete(row,cost,meta.get("usage"))
        with Image.open(out/(who+"-original.png")) as im:
            need(im.mode=="RGBA" and im.size==(1024,1536),"Artist did not produce genuine RGBA")
            alpha=im.getchannel("A");hist=alpha.histogram();n=im.width*im.height
            need(sum(hist[:8])>=n*.10 and sum(hist[240:])>=n*.20,"Missing usable transparent or opaque pixels")
            bbox=alpha.point(lambda v:255 if v>15 else 0).getbbox()
            need(bbox and bbox[2]-bbox[0]>=300 and bbox[3]-bbox[1]>=600,"Subject coverage too small")
            x0,y0,x1,y1=bbox;pad=16
            cut=im.crop((max(0,x0-pad),max(0,y0-pad),min(im.width,x1+pad),min(im.height,y1+pad)))
            cut.thumbnail((1024,1280),Image.Resampling.LANCZOS)
            dest=out/f"march_{who}_foreground.png";cut.save(dest,optimize=True)
            assets[dest.name]={"source_sha256":meta["image_sha256"],"derived_sha256":digest(dest),"alpha_bbox":list(bbox),"size":list(cut.size),"source_transparent_fraction":sum(hist[:8])/n}
    sheet=Image.new("RGB",(1000,1080),"#3c3432")
    d=ImageDraw.Draw(sheet)
    for j,(who,orig) in enumerate((("aldric",aldric),("lyra",lyra))):
        x=j*500
        orig=orig.copy();orig.thumbnail((230,330),Image.Resampling.LANCZOS);sheet.paste(orig,(x+10,32));d.text((x+12,10),who.upper()+" CANON",fill="white")
        with Image.open(out/f"march_{who}_foreground.png") as new:
            new=new.copy();new.thumbnail((475,680),Image.Resampling.LANCZOS);sheet.paste(new, (x+(500-new.width)//2,380),new)
        d.text((x+12,360),who.upper()+" NEW INDEPENDENT ALPHA",fill="white")
    sheet.save(out/"identity-review.jpg",quality=94)
    studio.MAX_OUTPUT=1400
    inp=mission.textfile(out/"asset-audit-input.txt",json.dumps({"actual_director_plan":plan,"asset_manifest":assets,"rule":"Both hero identities must remain recognizably exact. Reject if either is not faithful, inconsistent light/perspective, low quality, bad alpha, frame, background or fake UI. No approval of final game."},ensure_ascii=False))
    review=parse(mission.ask("visual","gpt-6.1-terra",inp,(str(out/"identity-review.jpg"),str(out/"canonical-and-r5.jpg")),ledger,"auditor",out,"Independent senior character artist, compare each original canonical portrait against the new actual transparent foreground. Strictly reject if recognizable identity, hair, costume, age, face, visual family or shared perspective/light is materially different, or subject cutout/edges appear poor. Assess actual silhouette and mobile readability; no guessed tests. JSON exact {verdict:'accept'|'reject',aldric_faithful:boolean,lyra_faithful:boolean,coherent:boolean,reason:string,risks:string}. Never approve final game from this asset sheet.",effort="none"))
    need(set(review)=={"verdict","aldric_faithful","lyra_faithful","coherent","reason","risks"} and review["verdict"] in ("accept","reject") and all(type(review[k]) is bool for k in ("aldric_faithful","lyra_faithful","coherent")),"Invalid asset review")
    accepted=review["verdict"]=="accept" and all(review[k] for k in ("aldric_faithful","lyra_faithful","coherent"))
    write(out/"review.json",review);write(out/"manifest.json",{"source":head,"assets":assets,"accepted_for_native_candidate_only":accepted,"art_approved":False})
    print("MARCH_R6_ASSET_REVIEW",json.dumps(review,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
