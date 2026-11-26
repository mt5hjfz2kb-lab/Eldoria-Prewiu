"""Actual before/after screen montages from source-pinned original PNG evidence."""
import hashlib,json,os,subprocess,sys
from pathlib import Path
from PIL import Image,ImageDraw
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
SHA="91b8de9874aed87219d733efa6e8e0aa56207f1e"
RUN=45802472999
BASE="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
BASE_RUN=45638738009
GROUPS={
 "hud":[("hud-b1","bastion-1.png","HUD B1 7.4 -> 8.5"),("hud-b5","bastion-5-completo.png","HUD B5 7.4 -> 8.5"),("tutorial","guia-iniciar-obra.png","Tutorial"),("construction","temporizador-en-parcela.png","Construction timer")],
 "management":[("march","sistema-march.png","March 7.4 -> 8.3"),("bag","sistema-bag.png","Bag 7.7 -> 8.5"),("mail","sistema-mail.png","Mailbox 7.4 -> 8.3"),("heroes","sistema-heroes.png","Heroes 8.0 -> 8.5"),("training","sistema-training.png","Training 8.0 -> 8.5")],
 "narrative":[("name","nombre-inicial.png","Name 7.5 -> 8.3"),("daily","sistema-daily.png","Daily 7.8 -> 8.5"),("scouts","sistema-scouts.png","Scouts 8.3 -> 8.8"),("vigilants","sistema-vigilants.png","Vigilants 8.2 -> 8.7")]}
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(data):return hashlib.sha256(data).hexdigest()
def main(group,before,after,ab,out):
    before,after,ab,out=map(Path,(before,after,ab,out))
    run=gh(f"repos/{REPO}/actions/runs/{RUN}")
    assert run["head_sha"]==SHA and run["status"]=="completed" and run["conclusion"]=="success"
    old=gh(f"repos/{REPO}/actions/runs/{BASE_RUN}")
    assert old["head_sha"]==BASE and old["conclusion"]=="success"
    manifest=json.loads((ab/"manifest.json").read_text())
    assert manifest["baseline_sha"]==BASE and manifest["candidate_sha"]==SHA and manifest["dpr"]==1 and not manifest["missing"] and len(manifest["pairs"])==13
    pairs={p["role"]:p for p in manifest["pairs"]}
    out.mkdir()
    images=[]
    screens=GROUPS[group]
    for side,root in (("baseline",before),("candidate",after)):
        sheet=Image.new("RGB",(780,870*((len(screens)+1)//2)),(18,24,31))
        draw=ImageDraw.Draw(sheet)
        for i,(role,name,label) in enumerate(screens):
            raw=(root/name).read_bytes()
            assert sha(raw)==pairs[role][side+"_sha256"]
            with Image.open(root/name) as im:
                assert im.size==(390,844)
                x=(i%2)*390;y=(i//2)*870
                sheet.paste(im.convert("RGB"),(x,y+26))
                draw.text((x+5,y+5),side.upper()+" / "+label,fill="white")
        file=out/(side+".jpg")
        sheet.save(file,quality=96,subsampling=0)
        assert file.stat().st_size<3*1024*1024
        images.append({"path":file.name,"sha256":sha(file.read_bytes())})
    journey=json.loads((after/"journey.json").read_text())
    log=journey.get("log",[])
    selected=[x for x in log if isinstance(x,dict) and any(k in x for k in ("room_landscape","room_traversed","daily_claim_ceremony"))]
    doc="Actual A/B evidence, "+group+"\nBaseline SHA "+BASE+"\nCandidate SHA "+SHA+"\nScreens: "+json.dumps(screens)+"\nJourney observations: "+json.dumps(selected,ensure_ascii=False)+"\nAll source PNGs verified by SHA256 against artifact manifest. Two labeled JPEG montages preserve 390x844 per frame; not original pixel measurements.\n"
    path="docs/valoria-modular-25d/CORONA_STUDIO_SECONDARY_DECISIONS_20261126.md"
    decisions=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/{path}?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode()
    source=(doc+decisions).encode()[:11800]
    (out/"source.txt").write_bytes(source.decode("utf-8",errors="ignore").encode())
    source=(out/"source.txt").read_bytes()
    packet={"schema":1,"target_id":"CORONA_AB_"+group.upper(),"source_sha":SHA,"source_run":RUN,"mode":"review","routing_profile":"standard",
      "objective":"Spanish. Independently judge genuine baseline and candidate WebGL captures of every listed screen. Baseline montage is image 1, candidate image 2. Use the historic score and stated target, judge actual visible quality, hierarchy, recognizable icons, composition, spacing, legibility and identity. Provide screen-by-screen score with visible evidence, difference vs historical baseline and precise deficiencies; do not inflate scores to meet targets. Secondary minimum 8, HUD target 8.5. If evidence insufficient for a score, say so.",
      "scope":"Only the supplied 390x844 WebGL stills and journey trace, no physical iPhone or commercial game reference images. No artistic approval from test success; do not infer animation quality from stills. Do not change game rules, buttons, art direction, navigation, main or publish. Assess no regression on Heroes, Training, Scouts and Vigilants. Distinguish measurements from observations; compare the matching screen only. Do not claim source code implementation beyond supplied record.",
      "acceptance":"Director and independent auditor separately give actual visual observations, provisional scores or evidence gaps for each labeled screen, reject regressions, identify failures to hit targets. Programmer proposes bounded fixes only for observed deficits. Coordinator preserves disagreements and records exact remaining criteria. No merging, approval, execution claim or invented reference comparison.",
      "source":{"path":"source.txt","sha256":sha(source)},"images":images}
    (out/"packet.json").write_text(json.dumps(packet,ensure_ascii=False,indent=2),encoding="utf-8")
if __name__=="__main__":main(*sys.argv[1:])
