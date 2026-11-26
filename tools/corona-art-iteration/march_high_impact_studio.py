"""A genuinely new, isolated portrait composition authored by the studio.
One visual director + one technical challenger; $0.01 cumulative unused reserve.
No arbitrary agent code execution, no automatic promotion or retries.
"""
import json,os,sys,subprocess,base64,hashlib,math
from pathlib import Path
from decimal import Decimal as D
from PIL import Image
SOURCE="41c124f4b9ef10cd9b4a9d87562a7d2727b730b6"
BRANCH="candidate/corona-march-high-impact-studio-20261126"
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=40))
def read(p):return json.loads(Path(p).read_text())
def write(p,v):Path(p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding="utf-8")
def parse(s):
    s=s.strip()
    if s.startswith("\x60\x60\x60json") and s.endswith("\x60\x60\x60"):s=s[7:-3].strip()
    return json.loads(s)
def validate(p):
    assert set(p)=={"hero_band","troop_band","hero_arrangement","troop_art_share","metric_columns","metric_font","caption_font","action_font","material","hierarchy","reason","reject_if"}
    for name,lo,hi in [("hero_band",.22,.31),("troop_band",.29,.36),("troop_art_share",.18,.27)]:
        v=p[name];assert type(v) in (float,int) and math.isfinite(v) and lo<=v<=hi,name
    assert p["hero_band"]+p["troop_band"]<=.64
    assert p["hero_arrangement"] in ("equal_triptych","commander_and_wings")
    assert p["metric_columns"] in (2,3)
    for name,lo,hi in [("metric_font",34,43),("caption_font",27,30),("action_font",30,36)]:
        assert type(p[name]) is int and lo<=p[name]<=hi,name
    assert p["material"] in ("quiet_slate","obsidian")
    for k in ("hierarchy","reason","reject_if"):
        assert isinstance(p[k],str) and 8<=len(p[k])<=650
    return p
def main():
    out=Path("studio-march");out.mkdir(exist_ok=False)
    repo=os.environ["GITHUB_REPOSITORY"];head=os.environ["GITHUB_SHA"]
    assert os.environ["GITHUB_RUN_ATTEMPT"]=="1"
    assert gh(f"repos/{repo}/git/ref/heads/{BRANCH}")["object"]["sha"]==head
    assert gh(f"repos/{repo}/git/ref/heads/candidate/corona-art-depth-followup-20261126")["object"]["sha"]==SOURCE
    assert gh(f"repos/{repo}/git/ref/heads/experiment/valoria-ciudad-region-unificada-20261121")["object"]["sha"]=="91b8de9874aed87219d733efa6e8e0aa56207f1e"
    assert gh(f"repos/{repo}/git/ref/heads/pilot/eldoria-openai-development-studio-20261125")["object"]["sha"]=="1657b2caa6b1a955319538ce3a8b2906398599b3"
    for run,art in [(45851071206,15134642987),(45851071206,15136075692),(45812100129,15113766981)]:
        assert gh(f"repos/{repo}/actions/runs/{run}")["conclusion"]=="success"
        assert any(x["id"]==art and not x["expired"] for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"])
    for run,paid in [(45824434222,"final-art-audit"),(45834241279,"spatial-art-audit")]:
        j=next(x for x in gh(f"repos/{repo}/actions/runs/{run}/jobs")["jobs"] if x["name"]==paid)
        assert j["conclusion"]=="failure"
        assert not any(x["name"].startswith("corona-final-independent") for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"])
    meta=gh(f"repos/{repo}/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")["content"]))
    lease=next(x for x in reg["active"] if x["id"]=="eldoria-openai-development-studio-20261125")
    assert lease["owner"]=="chatgpt-corona-brecha-commercial-20261126" and lease["ownership_revision"]==18
    # Never release completed reservations or assume invoice settlement.
    assert D("4.15")+D(".42")+D(".42")+D(".01")==D("5.00")
    proof=read("actual/capture-source.json")
    assert proof["player_source_sha"]==SOURCE and proof["player_artifact"]==15132786548 and proof["dpr"]==1
    sc=read("scores/calibrated-scorecard.json")
    assert sc["complete"] and sc["candidate_sha"]==SOURCE
    image=Path("actual/sistema-march.png")
    row=sc["screens"]["march"]
    assert hashlib.sha256(image.read_bytes()).hexdigest()==row["provenance"]["after_png"]
    with Image.open(image) as im:
        assert im.size==(390,844)
    refs=read("refs/reference-manifest.json")
    successful=[x for x in refs if "sha256" in x]
    idx=next(i for i,x in enumerate(successful) if x["label"]=="WOS heroes/march")
    with Image.open("refs/reference.jpg") as im:
        assert im.width==1240
        x=idx%2*620;y=idx//2*632
        im.crop((x,y,x+620,y+632)).convert("RGB").save(out/"reference-march.jpg",quality=92)
    write(out/"source.json",{"player_sha":SOURCE,"screen_sha256":row["provenance"]["after_png"],"screen_artifact":15136075692,"reference_artifact":15113766981,"score":row["after_total"],"reference_score":row["reference_total"],"owner_cap":"5.00","new_reserve":".01","prior_held":"4.99","invoice_verified":False})
    sys.path.insert(0,str(Path("studio-source/tools/openai-development-studio").resolve()))
    import studio,mission
    mission.RESERVES["director"]=D(".007")
    mission.RESERVES["programmer"]=D(".003")
    ledger=mission.Ledger(out/"ledger.json",D(".010"))
    # The agent authors the actual macro layout and chooses the material.
    # The existing button handlers/data never enter the generated patch.
    prompt="""Director artístico de videojuegos móviles comerciales. Tu encargo no es retocar márgenes sino REDISEÑAR integralmente la única pantalla de preparación de marchas de Eldoria. Imagen 1 es el jugador real actual, imagen 2 es WOS. Nota calibrada actual 6.78 frente a referente 7.83; buscamos al menos +1.0 en A/B. Toma una decisión artística concreta y valiente. No copies WOS ni cambies mecánicas. Conserva 3 botones reales de héroe (tercero bloqueado), acceso HÉROES, selector T1/T2, cantidad/%, menos/más, slider, seis estadísticas reales, capacidad/ruptura/disponibles/marchas/preparados y confirmación. Sin nueva imagen ni fondos con texto; usa los retratos y el arte Corona ya existentes. El cuerpo usable es aproximadamente 740×1480 unidades lógicas, a 390px de ancho. Las tres bandas tienen alturas relativas hero_band, troop_band y el resto resumen, dejando margen; héroes 0.22–0.31; tropas 0.29–0.36, suma <=0.64. Elige equal_triptych o commander_and_wings, fracción ancho ilustración tropa 0.18–0.27. Rediseñaremos el orden como banda real a ancho completo para cantidad y slider, con etiquetas y números independientes, sin rich text. Elige 2 o 3 columnas de métricas, fuentes lógicas valor 34–43, etiqueta 27–30, CTA 30–36; material quiet_slate (placa secundaria del artista) u obsidian. Especifica jerarquía, por qué es un salto y qué evidencia lo refutaría. JSON puro con claves exactas hero_band,troop_band,hero_arrangement,troop_art_share,metric_columns,metric_font,caption_font,action_font,material,hierarchy,reason,reject_if. Tres textos cortos. Evita cambios cosméticos sin impacto."""
    evidence=mission.textfile(out/"director-evidence.txt",json.dumps({"real_screen":row["evidence"],"remaining":row["remaining"],"source":SOURCE,"rubric":sc["weights"]},ensure_ascii=False))
    studio.MAX_OUTPUT=720
    p=validate(parse(mission.ask("visual","gpt-6.1-terra",evidence,(str(image),str(out/"reference-march.jpg")),ledger,"director",out,prompt,effort="none")))
    write(out/"plan.json",p)
    print("MARCH_STUDIO_PLAN",json.dumps(p,ensure_ascii=False),flush=True)
    studio.MAX_OUTPUT=1000
    brief=mission.textfile(out/"challenger-evidence.txt",json.dumps({"plan":p,"constraints":"740x1480 body. Keep all real controls, UI labels and values. Separate metric value and label Text components, no rich text. Preserve landscape and non-Corona. All touch areas >=44 physical px on 390x844; no overflows, compare true live screenshots. Status carries 3 lines with actual values. No execution claims."},ensure_ascii=False))
    response=mission.ask("programming","gpt-6-luna",brief,(),ledger,"programmer",out,"Technical Artist challenger. Critique the director's actual plan against dimensions and controls. Return JSON with verdict (accept or revise), critical_constraints array, implementation_notes array, required_visual_checks array. Do not claim tests. No code execution, no additional paid requests.",effort="none")
    challenge=parse(response)
    assert set(challenge)=={"verdict","critical_constraints","implementation_notes","required_visual_checks"} and challenge["verdict"] in ("accept","revise")
    assert all(isinstance(challenge[k],list) and all(isinstance(x,str) and x.strip() for x in challenge[k]) for k in ("critical_constraints","implementation_notes","required_visual_checks"))
    write(out/"challenge.json",challenge)
    print("MARCH_STUDIO_CHALLENGE",json.dumps(challenge,ensure_ascii=False),flush=True)
if __name__=="__main__":main()
