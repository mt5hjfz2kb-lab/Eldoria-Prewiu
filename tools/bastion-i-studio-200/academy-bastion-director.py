"""Director consumes the actual Academy technique and Valoria evidence.
Bounded parameters feed the real alpha-hull solver; not another cosmetic edit."""
import os,json,base64,hashlib,io
from pathlib import Path
from decimal import Decimal as D
from PIL import Image
import studio,economics,guard
root=Path(os.environ["RUNNER_TEMP"]);out=root/"academy-plan";out.mkdir(exist_ok=True)
prior=D(json.loads((root/"academy/clearance-summary.json").read_text())["estimated_total_usd"])
assert prior==D("1.8678118") and prior+D(".65")<=D("5")
bounds={"wellLimit":[.25,.80],"marketCartLimit":[.30,1.0],"quarryCartLimit":[.25,.85],"targetMobilePx":[3,6]}
prompt="""You are Valoria's art director. Apply the validated academy C03 practice to a NEW Unity scene, not the previous Blender laboratory. Actual native baseline stage V screenshots follow (left half of previous A/B sheets); preserve all architecture, terrain, canonical paths, materials and approved mill, do not move CasasDelPeso. We will solve actual imported sprite alpha hulls and preserve clearance from nearby props and route centers. Only well, market cart, quarry cart are eligible for small reversible moves in V when built. The solver measures and may reject all moves; do not claim it has improved. Select bounded maximum displacements (Unity world units) and target mobile gap in px, using actual visual evidence and realistic limits. Do not demand global zero overlap (intentional layering). Return strict JSON {settings:{wellLimit:number,marketCartLimit:number,quarryCartLimit:number,targetMobilePx:number},strengths:[3 or more],hypothesis:string,risks:[strings],native_checks:[strings]}. Bounds: """+json.dumps(bounds)+"\n"+guard.role_brief("director",academy=True,competencies=["composition","projection"])
content=[{"type":"input_text","text":prompt}]
for n,rows in [("stage-5-comparison.jpg",2),("stage-5-urban-details.jpg",4),("stage-5-contact-details.jpg",3)]:
    p=root/"prior"/n;assert p.is_file()
    with Image.open(p) as im:
        w=im.width//2;rh=im.height//rows
        q=Image.new("RGB",(w,(rh-32)*rows))
        for row in range(rows):q.paste(im.crop((0,row*rh+32,w,(row+1)*rh)),(0,row*(rh-32)))
        q.thumbnail((1000,1800));b=io.BytesIO();q.save(b,format="JPEG",quality=94)
        content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(b.getvalue()).decode(),"detail":"high"})
def dump(n,v):(out/n).write_text(json.dumps(v,indent=2,ensure_ascii=False),encoding="utf-8")
dump("ledger.json",{"status":"in_flight","reserved_usd":".30"})
r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":1600,"store":False},os.environ["OPENAI_API_KEY"])
cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard")
dump("ledger.json",{"status":"complete","estimated_usd":str(cost),"usage":r["usage"]})
assert cost<=D(".30") and r["status"]=="completed"
s="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
j=json.loads(s);assert set(j["settings"])==set(bounds)
for k,(lo,hi) in bounds.items():assert type(j["settings"][k]) in (float,int) and lo<=j["settings"][k]<=hi
assert len(j["strengths"])>=3 and len(j["native_checks"])>=1
dump("director.json",j);dump("academy-settings.json",j["settings"])
dump("parameters.json",{"source_head":"f27f4774e32efc5ad046478a07747eead4256c3e","estimated_prior_usd":str(prior),"estimated_new_usd":str(cost),"estimated_total_usd":str(prior+cost),"settings":j["settings"],"academy_C03":True,"billing_verified":False})
print("ACADEMY_DIRECTOR_PLAN",json.dumps(j,ensure_ascii=False),flush=True)
