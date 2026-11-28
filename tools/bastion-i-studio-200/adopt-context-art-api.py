"""Adopt exact native-tested art runtime bytes; exclude proof code. No gameplay migration."""
import os,json,hashlib,subprocess,re,uuid
from pathlib import Path
root=Path(os.environ["RUNNER_TEMP"]);src=root/"approved-source";audit=root/"approved-audit";native=root/"approved-native";out=root/"adoption-receipt";out.mkdir(exist_ok=True)
base=os.environ["BASE_SHA"];branch=os.environ["TARGET_BRANCH"];repo=os.environ["REPO"]
assert base=="f27f4774e32efc5ad046478a07747eead4256c3e"
prefix="Unity/Assets/Experimentos/ValoriaModular25D/"
manifest=json.loads((src/"exact-source-manifest.json").read_text())
params=json.loads((src/"parameters.json").read_text());review=json.loads((audit/"summary.json").read_text())
assert manifest["base"]==params["source_head"]==review["source"]==base
assert review["native_frames"]==132 and review["native"]["bundle"]["candidate"]==8.5 and review["native"]["bundle"]["baseline"]==7.8
assert review["native"]["bundle"]["preferred"]=="candidate"
assert review["promoted"] is False and review["playmode"] is False
# The native runner copied this manifest into its evidence before rendering.
assert json.loads((native/"exact-source-manifest.json").read_text())==manifest
assert len(manifest["files"])==11
names=("AceraZapateros","ArmeriaDelPatio","CorteCaliza")
runtime={"Scripts/Modular25DWorld.cs","Scripts/Issue200CompetitionArt.cs","Resources/ValoriaModular25D/DistrictGround.shader","Resources/Issue200Competition.meta"}
for n in names:runtime.update({"Resources/Issue200Competition/"+n+".png","Resources/Issue200Competition/"+n+".png.meta"})
allowed={prefix+x for x in runtime|{"Editor/Issue200CompetitionProof.cs"}}
assert {x["path"] for x in manifest["files"]}==allowed
assert len({x["path"] for x in manifest["files"]})==11
def sha(b):return hashlib.sha256(b).hexdigest()
def blob(b):return hashlib.sha1(b"blob "+str(len(b)).encode()+bytes(1)+b).hexdigest()
def gh(path):return json.loads(subprocess.check_output(["gh","api",f"repos/{repo}/{path}"],text=True))
def preflight():
    assert gh("git/ref/heads/"+branch)["object"]["sha"]==base,"Experimental HEAD changed; no overwrite"
    m=gh("contents/pipeline/active-workstreams.json?ref=main")
    import base64
    reg=json.loads(base64.b64decode(gh("git/blobs/"+m["sha"])["content"]))
    w=next(x for x in reg["active"] if x["id"]=="eldoria-valoria-modular25d-bastion1-20261119")
    assert w["branch"]==branch and w["owner"]=="chatgpt-corona-brecha-commercial-20261126" and w["ownership_revision"]==40
    assert reg["resource_rotation"]["status"]=="corona_native_reserved" and reg["resource_rotation"]["current_run_id"] is None
    for status in ("in_progress","queued","waiting","pending","requested"):
        for r in gh(f"actions/runs?status={status}&per_page=100")["workflow_runs"]:
            if str(r["id"])==os.environ["GITHUB_RUN_ID"]:continue
            assert r["head_branch"]!=branch,"Other active experimental run"
# Use Git data APIs rather than a partial clone. Commit is still atomic and
# non-forced; if the earlier attempt landed the exact content, verify it instead.
import base64
def write_api(method,path,payload):
    p=subprocess.run(["gh","api","--method",method,f"repos/{repo}/{path}","--input","-"],input=json.dumps(payload),text=True,capture_output=True,timeout=90)
    if p.returncode:raise RuntimeError(p.stderr[:1000])
    return json.loads(p.stdout)
def get_head():return gh("git/ref/heads/"+branch)["object"]["sha"]
def content_sha_at(head,path):
    try:return gh("contents/"+path+"?ref="+head)["sha"]
    except subprocess.CalledProcessError:return None
receipt=[]
entries=[]
for row in manifest["files"]:
    b=(src/row["file"]).read_bytes()
    assert sha(b)==row["sha256"] and blob(b)==row["new_blob"]
    path=row["path"]
    if path.endswith("/Editor/Issue200CompetitionProof.cs"):continue
    assert content_sha_at(base,path)==row["old_blob"]
    if path.endswith(".png"):
        assert sha(b)==params["assets"][Path(path).stem]["final_sha256"]
    entries.append((path,b))
    receipt.append({"path":path,"sha256":sha(b),"blob":blob(b)})
meta=prefix+"Scripts/Issue200CompetitionArt.cs.meta"
guid=uuid.uuid5(uuid.NAMESPACE_URL,"eldoria/issue200/competition/runtime-art-helper").hex
b=("fileFormatVersion: 2\nguid: "+guid+"\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n").encode()
assert content_sha_at(base,meta) is None
entries.append((meta,b));receipt.append({"path":meta,"sha256":sha(b),"blob":blob(b)})
assert len(entries)==11
head=get_head()
if head!=base:
    c=gh("commits/"+head)
    assert [v["sha"] for v in c["parents"]]==[base]
    assert {x["filename"] for x in c["files"]}=={x["path"] for x in receipt}
    assert all(content_sha_at(head,x["path"])==x["blob"] for x in receipt)
    commit=head
    print("ALREADY_ADOPTED_EXACTLY",head,flush=True)
else:
    preflight()
    tree=[]
    for path,b in entries:
        oid=write_api("POST","git/blobs",{"content":base64.b64encode(b).decode(),"encoding":"base64"})["sha"]
        assert oid==blob(b)
        tree.append({"path":path,"mode":"100644","type":"blob","sha":oid})
    parent=gh("git/commits/"+base)
    tid=write_api("POST","git/trees",{"base_tree":parent["tree"]["sha"],"tree":tree})["sha"]
    commit=write_api("POST","git/commits",{"message":"Adopt native-tested Bastion V art refinement (Issue200; visual only)","tree":tid,"parents":[base]})["sha"]
    c=gh("commits/"+commit)
    assert {x["filename"] for x in c["files"]}=={x["path"] for x in receipt}
    assert all(content_sha_at(commit,x["path"])==x["blob"] for x in receipt)
    preflight()
    write_api("PATCH","git/refs/heads/"+branch,{"sha":commit,"force":False})
    assert get_head()==commit
result={"base":base,"head":commit,"branch":branch,"adopted_native_run":46160199450,"source_artifact":15269420973,"native_artifact":15270340577,"audit_artifact":15269982140,"runtime_files":receipt,"native_tested_content_exact":True,"new_metadata_only":meta,"proof_code_excluded":True,"main_changed":False,"gameplay_changed":False,"new_head_playmode_tested":False,"physical_device_tested":False,"owner_authorized_art_adoption":True}
(out/"summary.json").write_text(json.dumps(result,indent=2))
print("ART_ADOPTION_FINAL",json.dumps(result),flush=True)
