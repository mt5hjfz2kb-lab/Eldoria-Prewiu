"""Copy four exact validated native PNGs to owned review docs; no Unity/mutation of art."""
import base64,hashlib,json,os,pathlib,struct,urllib.request,urllib.error
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
assert os.environ["GITHUB_REPOSITORY"]==REPO
token=os.environ["GH_TOKEN"]
def api(path,data=None,method=None):
    body=None if data is None else json.dumps(data).encode()
    req=urllib.request.Request("https://api.github.com/repos/"+REPO+"/"+path,data=body,
        method=method,headers={"Authorization":"Bearer "+token,
        "Accept":"application/vnd.github+json","Content-Type":"application/json",
        "User-Agent":"eldoria-owned-native-review"})
    with urllib.request.urlopen(req,timeout=40) as r:return json.load(r)
def contents(path,ref):
    d=api("contents/"+path+"?ref="+ref)
    if d.get("encoding")!="base64" or not d.get("content"):d=api("git/blobs/"+d["sha"])
    return base64.b64decode(d["content"])
request=json.loads(pathlib.Path("tools/daily-treasures/native-art-review-request.json").read_text())
assert request["state"]=="REVIEW_EXISTING_NATIVE_ARTIFACT"
run_id=int(request["review_run_id"]); artifact_id=int(request["review_artifact_id"])
source=request["review_source_sha"]
assert len(source)==40 and all(c in "0123456789abcdef" for c in source)
run=api("actions/runs/"+str(run_id))
assert run["head_sha"]==source and run["head_branch"]=="feature/eldoria-daily-treasures-20261108"
jobs=api("actions/runs/"+str(run_id)+"/jobs")["jobs"]
assert any(j["name"]=="native-ui" and j["status"]=="completed" and j["conclusion"]=="success" for j in jobs)
artifact=api("actions/artifacts/"+str(artifact_id))
assert artifact["workflow_run"]["id"]==run_id and artifact["workflow_run"]["head_sha"]==source
root=pathlib.Path("reviewed-native").resolve()
reports=list(root.rglob("city-art-report.json"))
assert len(reports)==1,"Require one exact actual-city report"
report=json.loads(reports[0].read_text(encoding="utf-8-sig"))
assert report["source"]==source and report["status"]=="CITY_ART_CONTEXT_RENDERED_NOT_GAMEPLAY_CERTIFIED"
assert report["pngCount"]==20 and report["realGatewayClaims"]==0 and report["artApproved"] is False
shots={s["file"]:s for s in report["shots"]}; selected=[]
for width,height in [(390,844),(844,390)]:
    for variant in ["real-bastion-camera-baseline","real-bastion-camera-ceremony"]:
        name=f"real-city-daily-{variant}-{width}x{height}.png"
        shot=shots[name]; path=(reports[0].parent/name).resolve()
        assert path.is_relative_to(root)
        data=path.read_bytes()
        assert len(data)==shot["bytes"] and hashlib.sha256(data).hexdigest()==shot["sha256"]
        assert data[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",data[16:24])==(width,height)
        selected.append((name,data,shot))
# The canonical target must be released before writing ANY review evidence.
registry=json.loads(contents("pipeline/active-workstreams.json","main"))
target="feature/eldoria-daily-treasures-20261108"
owners=[w for w in registry["active"] if w["id"]=="eldoria-daily-treasures-20261108"]
assert len(owners)==1 and owners[0]["owner"]=="chatgpt-central-treasures-art-20261111"
assert "windows-runner-heavy" not in owners[0].get("resources",[]),"Target source remains frozen during native work"
pr=api("pulls/181")
assert pr["state"]=="open" and pr["head"]["ref"]==target
head=api("git/ref/heads/"+target)["object"]["sha"]
assert api("compare/"+source+"..."+head)["status"] in ["identical","ahead"]
prefix="docs/daily-treasures/remote-native-review/"+str(run_id)+"/"
try:
    prior=json.loads(contents(prefix+"proof.json",head))
except urllib.error.HTTPError as error:
    if error.code!=404:raise
else:
    if (prior.get("source")==source and prior.get("native_run")==run_id and
        prior.get("artifact_digest")==artifact.get("digest") and len(prior.get("files",[]))==4):
        for record in prior["files"]:
            assert api("contents/"+prefix+record["file"]+"?ref="+head)["sha"]==record["git_blob"]
        print("EXACT_NATIVE_REVIEW_ALREADY_PRESERVED",run_id,head)
        raise SystemExit(0)
records=[];elements=[]
for name,data,shot in selected:
    blob=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})
    elements.append({"path":prefix+name,"mode":"100644","type":"blob","sha":blob["sha"]})
    records.append({"file":name,"git_blob":blob["sha"],"native":shot})
proof={"source":source,"native_run":run_id,"artifact_id":artifact_id,"artifact_digest":artifact.get("digest"),
       "method":"BYTE_IDENTICAL_NATIVE_PNG_NO_RETOUCH","artApproved":False,"fullSceneVerified":False,
       "physicalMobileVerified":False,"files":records}
proof_blob=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})
elements.append({"path":prefix+"proof.json","mode":"100644","type":"blob","sha":proof_blob["sha"]})
tree=api("git/trees",{"base_tree":api("git/commits/"+head)["tree"]["sha"],"tree":elements})
commit=api("git/commits",{"message":"Preserve exact actual native Bastion A/B for owner visual review",
    "tree":tree["sha"],"parents":[head]})
# Non-force plus exact old parent rejects concurrent advancement.
api("git/refs/heads/"+target,{"sha":commit["sha"],"force":False},"PATCH")
print(json.dumps({"review_commit":commit["sha"],"source":source,"native_run":run_id,"files":records}))
