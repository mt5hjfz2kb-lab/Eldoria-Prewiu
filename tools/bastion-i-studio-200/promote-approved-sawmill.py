"""Owner-approved sawmill only, atomic experimental commit, exact artifact, no new spend."""
import os,json,subprocess,base64,hashlib,sys
from pathlib import Path
from datetime import datetime,timezone
repo=os.environ["REPO"];run=int(os.environ["GITHUB_RUN_ID"])
base="91b8de9874aed87219d733efa6e8e0aa56207f1e"
branch="experiment/valoria-ciudad-region-unificada-20261121"
id="eldoria-valoria-modular25d-bastion1-20261119"
pfx="Unity/Assets/Experimentos/ValoriaModular25D/"
png=pfx+"Resources/ValoriaModular25D/sawmill_i_hd.png"
plan=pfx+"Scripts/ValoriaUrbanPlan.cs"
doc="docs/valoria-modular-25d/ISSUE200_APPROVED_SAWMILL_20261127.json"
root=Path(os.environ["RUNNER_TEMP"]);assets=root/"mill-input";out=root/"mill-promotion";out.mkdir(exist_ok=True)
def gh(path,method="GET",payload=None):
    args=["gh","api","--method",method,"repos/"+repo+"/"+path]
    if payload is not None:args+=["--input","-"]
    return json.loads(subprocess.check_output(args,input=json.dumps(payload) if payload is not None else None,text=True))
def check_coord():
    m=gh("contents/pipeline/active-workstreams.json?ref=main");b=gh("git/blobs/"+m["sha"])
    reg=json.loads(base64.b64decode(b["content"]));w=next(w for w in reg["active"] if w["id"]==id);r=reg["resource_rotation"]
    assert w["owner"]=="chatgpt-corona-brecha-commercial-20261126" and w["ownership_state"]=="claimed" and w["ownership_revision"]==23
    assert r["current_workstream"]==id and r["current_run_id"]==run and r["status"]=="issue200_sawmill_adoption"
def sha(p):return hashlib.sha256(p).hexdigest()
def raw(path):
    return subprocess.check_output(["curl","--fail","--silent","--show-error","--location","-H","Authorization: Bearer "+os.environ["GH_TOKEN"],"-H","Accept: application/vnd.github.raw+json","https://api.github.com/repos/"+repo+"/contents/"+path+"?ref="+base])
def blob(data):
    return gh("git/blobs","POST",{"content":base64.b64encode(data).decode(),"encoding":"base64"})["sha"]
assert gh("actions/runs/46032921829")["head_sha"]=="a3f9ef733ce4326a8302100cf54e5bbff537cb66"
art=gh("actions/artifacts/15214917793")
assert art["digest"]=="sha256:fd6b1a7a6353563232cdb95530170619abf3e36179edc6c0d940ee9ae3ed9cd6" and not art["expired"]
m=json.loads((assets/"fit-manifest.json").read_text())["sawmill"]
new=(assets/"sawmill-fit.png").read_bytes()
assert sha(new)==m["fitted_sha256"] and m["canvas"]==[1086,819] and m["pixels_per_unit"]==288 and m["pivot"]==[0.5,0.07]
old=raw(png);assert sha(old)==m["source_sha256"] and old!=new
from PIL import Image
import io
a=Image.open(io.BytesIO(new));assert a.mode=="RGBA" and a.size==(1086,819)
assert a.getchannel("A").getextrema()[0]==0
oldplan=raw(plan);s=oldplan.decode("utf-8");needle="public Vector2 MillBench => Mill.Foot+new Vector2(1.02f,-.46f);"
assert s.count(needle)==1
# Same local sawyer adjustment already exercised in the V2 native capture.
updated=s.replace(needle,"public Vector2 MillBench => Mill.Foot+new Vector2(1.34f,-.86f);").encode()
assert gh("git/ref/heads/"+branch)["object"]["sha"]==base
check_coord()
meta=raw(png+".meta")
provenance={"owner_decision":"Keep the experimental castle and adopt only the sawmill C that owner preferred in native A/B; no new gameplay or release","source_sha":base,"native_review_run":46032921829,"approved_sprite_artifact":15214917793,"reference_art_run":46032398265,"original_sha256":sha(old),"adopted_sha256":sha(new),"meta_unchanged_sha256":sha(meta),"sprite_canvas":[1086,819],"ppu":288,"pivot":[.5,.07],"worker_anchor_before":[1.02,-.46],"worker_anchor_after":[1.34,-.86],"ground_and_castle":"unchanged; existing projected alpha shadow, yard and roads retained","new_api_spend_usd":0,"native_post_adoption":"pending this run","physical_device":"not tested"}
d=(json.dumps(provenance,ensure_ascii=False,indent=2)+"\n").encode()
treebase=gh("git/commits/"+base)["tree"]["sha"]
tree=gh("git/trees","POST",{"base_tree":treebase,"tree":[{"path":p,"mode":"100644","type":"blob","sha":blob(data)} for p,data in [(png,new),(plan,updated),(doc,d)]]})
commit=gh("git/commits","POST",{"message":"Adopt owner-selected C sawmill only; preserve original Bastion and terrain","tree":tree["sha"],"parents":[base]})
check_coord()
assert gh("git/ref/heads/"+branch)["object"]["sha"]==base
gh("git/refs/heads/"+branch,"PATCH",{"sha":commit["sha"],"force":False})
assert gh("git/ref/heads/"+branch)["object"]["sha"]==commit["sha"]
diff=gh("compare/"+base+"..."+commit["sha"])
assert {f["filename"] for f in diff["files"]}=={png,plan,doc}
provenance["candidate_sha"]=commit["sha"];provenance["modified_paths"]=[png,plan,doc]
(out/"promotion.json").write_text(json.dumps(provenance,indent=2))
with open(os.environ["GITHUB_OUTPUT"],"a") as f:f.write("candidate_sha="+commit["sha"]+"\n")
print("ISSUE200_SAWMILL_COMMITTED "+commit["sha"],flush=True)
