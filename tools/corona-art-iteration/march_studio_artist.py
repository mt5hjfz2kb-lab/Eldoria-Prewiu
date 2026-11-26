"""One original studio asset: purposeful command-table illustration, not a baked UI."""
import json,os,sys,subprocess,base64,hashlib
from pathlib import Path
from decimal import Decimal as D
from PIL import Image
SOURCE="41c124f4b9ef10cd9b4a9d87562a7d2727b730b6"
BRANCH="candidate/corona-march-high-impact-studio-20261126"
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=40))
def read(p):return json.loads(Path(p).read_text())
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def main():
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    assert os.environ["GITHUB_RUN_ATTEMPT"]=="1"
    assert gh(f"repos/{repo}/git/ref/heads/{BRANCH}")["object"]["sha"]==head
    assert gh(f"repos/{repo}/git/ref/heads/candidate/corona-art-depth-followup-20261126")["object"]["sha"]==SOURCE
    assert gh(f"repos/{repo}/git/ref/heads/experiment/valoria-ciudad-region-unificada-20261121")["object"]["sha"]=="91b8de9874aed87219d733efa6e8e0aa56207f1e"
    for run,art in [(45864197708,15139487023),(45851071206,15136075692)]:
        assert gh(f"repos/{repo}/actions/runs/{run}")["conclusion"]=="success"
        assert any(x["id"]==art and not x["expired"] for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"])
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    assert lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18
    plan=read("plan/plan.json");assert plan["hero_arrangement"]=="equal_triptych" and plan["metric_columns"]==3 and plan["material"]=="quiet_slate"
    challenge=read("plan/challenge.json");assert challenge["verdict"]=="revise"
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import mission,studio,generate_candidate as artist
    out=Path("march-artist");out.mkdir(exist_ok=False)
    # Owner explicitly authorized additional funds on 26 November. $2 additive
    # cap; preserve original $5 reservations. Reserve .75 for this image and
    # .12 for review of actual output, .30 for native before/after reviewers.
    write(out/"budget.json",{"previous_reserved_usd":"5.00","additional_owner_cap_usd":"2.00","artist_reserved_usd":".75","preflight_review_reserved_usd":".12","final_review_reserved_usd":".30","unallocated_usd":".83","invoice_verified":False,"no_auto_retry":True})
    mission.RESERVES["artist"]=D(".75");mission.RESERVES["auditor"]=D(".12")
    ledger=mission.Ledger(out/"ledger.json",D(".87"))
    brief="""An ORIGINAL beautiful game environment illustration for Eldoria's march preparation chamber, portrait-friendly, one coherent scene. A close almost overhead / slightly isometric view of a finely crafted medieval command table, weathered midnight-blue and charcoal slate, dark walnut, muted antique brass and desaturated turquoise inlays. At the top margin only, a subtly sculpted Valorian compass and rolled campaign map under soft blue candlelight. An open expanse of refined dark slate across the center and bottom, softly illuminated, quiet enough for highly legible live UI. Noble fantasy tactical identity rather than burgundy velvet or generic blue plastic. Sophisticated painterly realism, exquisite restrained material rendering, contact shadows and broad controlled values. Do not draw a screen, panels, buttons, characters, typography, letters, numbers, symbols that look like text, grids, menus, HUD, game stats, pseudo-controls, gold rectangular frames or independent picture boxes. Full bleed seamless atmospheric backdrop, not a mockup. It will be cropped to a portrait aspect with its center preserved and used only BEHIND real controls. Keep the middle 70 percent horizontally subdued, nearly dark with no bright focal objects. All essential ornament near the upper central edge, no clutter. No imitation of another game's IP."""
    (out/"artist-brief.txt").write_text(brief,encoding="utf-8")
    payload=artist.make_payload(brief)
    payload["quality"]="medium";payload["complexity"]="medium";payload["size"]="1024x1536"
    row=ledger.reserve("artist")
    raw,meta=artist.unpack(artist.generate(payload,os.environ.get("OPENAI_API_KEY")))
    (out/"command_table.png").write_bytes(raw)
    cost=mission.image_cost(meta.get("usage"))
    meta.update(quality=payload["quality"],size=payload["size"],estimated_usd=str(cost) if cost is not None else None,brief_sha256=hashlib.sha256(brief.encode()).hexdigest())
    write(out/"artist.json",meta)
    ledger.complete(row,cost,meta.get("usage"))
    with Image.open(out/"command_table.png") as im:
        assert im.width>512 and im.height>512
        im.thumbnail((768,1152))
        im.convert("RGB").save(out/"review.jpg",quality=91)
    evidence=mission.textfile(out/"review-evidence.txt",json.dumps({"director":plan,"technical_challenge":challenge,"purpose":"background only behind actual separate controls; no screen mockup. Preserve existing hero portraits and true data."},ensure_ascii=False))
    studio.MAX_OUTPUT=1400
    result=mission.ask("visual","gpt-6.1-terra",evidence,("actual/sistema-march.png",str(out/"review.jpg")),ledger,"auditor",out,"Independent senior game art reviewer. First image is actual current Eldoria March UI, second the new original background only. Judge whether the new material is coherent with Eldoria and usable behind the director's actual redesigned UI. No art approval of final UI. Reject if fake controls, illegible contrast, noisy, text, inappropriate art, obvious reproduction. JSON only {verdict:'accept'|'reject',reason:string,placement:string,contrast_risks:string}. Inspect actual image; no execution claims.",effort="none")
    x=json.loads(result.strip().removeprefix("\x60\x60\x60json").removesuffix("\x60\x60\x60").strip())
    assert set(x)=={"verdict","reason","placement","contrast_risks"} and x["verdict"] in ("accept","reject")
    write(out/"review.json",x)
    print("MARCH_ARTIST_ASSET",json.dumps(meta,ensure_ascii=False),flush=True)
    print("MARCH_ARTIST_REVIEW",json.dumps(x,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
