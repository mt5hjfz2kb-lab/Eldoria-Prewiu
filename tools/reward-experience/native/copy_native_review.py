"""Preserve exact validated native PNGs in released owned review docs; no Unity or art mutation."""
import base64,hashlib,json,os,pathlib,struct,urllib.request
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
request=dict(line.split("=",1) for line in pathlib.Path("tools/reward-experience/native-request.txt").read_text().splitlines() if "=" in line and not line.startswith("#"))
assert request["state"]=="REVIEW_EXISTING_NATIVE_ARTIFACT"
run_id=int(request["review_run_id"]); artifact_id=int(request["review_artifact_id"])
source=request["review_source_sha"]
assert len(source)==40 and all(c in "0123456789abcdef" for c in source)
kind=request.get("review_kind","daily_bastion")
assert kind in ("daily_bastion","issue186_components","issue186_combined")
combined=kind=="issue186_combined"
resource=kind.startswith("issue186_")
target=("feature/issue186-reward-attention-guidance-20261111" if resource else
        "feature/eldoria-daily-treasures-20261108")
workstream=("eldoria-issue186-reward-attention-guidance-20261111" if resource else
            "eldoria-daily-treasures-20261108")
owner="chatgpt-central-art-execution-20261111"
pr_number=187 if resource else 181
run=api("actions/runs/"+str(run_id))
assert run["head_sha"]==source and run["head_branch"]==target
jobs=api("actions/runs/"+str(run_id)+"/jobs")["jobs"]
job_name="native" if resource else "native-ui"
assert any(j["name"]==job_name and j["status"]=="completed" and j["conclusion"]=="success" for j in jobs)
artifact=api("actions/artifacts/"+str(artifact_id))
assert not artifact["expired"]
assert artifact["workflow_run"]["id"]==run_id and artifact["workflow_run"]["head_sha"]==source
root=pathlib.Path("reviewed-native").resolve()
reports=([root/"combined-bag"/"report.json"] if combined else list(root.rglob("report.json" if resource else "city-art-report.json")))
assert all(p.is_file() for p in reports)
assert len(reports)==1,"Require one exact actual native report"
report=json.loads(reports[0].read_text(encoding="utf-8-sig"))
selected=[]
def verified(name,shot,width,height):
    path=(reports[0].parent/name).resolve()
    assert path.is_relative_to(root)
    data=path.read_bytes()
    assert len(data)==shot["bytes"] and hashlib.sha256(data).hexdigest()==shot["sha256"]
    assert data[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",data[16:24])==(width,height)
    selected.append((name,data,shot))
if combined:
    assert report["resourceSource"]==source and str(report["nativeRun"])==str(run_id)
    assert report["status"]=="REAL_VIGILIA_SAVED_BAG_PASS_ART_REVIEW_PENDING"
    assert report["uiSource"]==request["review_ui_source_sha"]
    assert report["actualIntegratedCityFrames"]==4 and report["realFirstHitClicks"]==52
    for flag in ("actualBagOnVigilia","seenDoesNotUse","skipDoesNotComplete","useCreditsOnce",
                 "jsonReloadVerified","mailReadDoesNotClaim","mailClaimDoesNotCredit","fullMailboxUiFlowVerified"):
        assert report[flag] is True,flag
    assert report["artApproved"] is False and report["physicalIphoneVerified"] is False
    restoration=json.loads((root/"vigilia-overlay.json.restored.json").read_text(encoding="utf-8-sig"))
    assert restoration["restored"] is True and restoration["files"]==245
    assert restoration["ui_source"]==report["uiSource"]
    shots={shot["file"]:shot for shot in report["images"]}
    assert len(shots)==20 and len(report["images"])==20
    for width,height in [(390,844),(844,390),(900,1600),(1600,900)]:
        for phase in ("actual-integrated-city","new","mail-read-unclaimed","mail-claimed-no-credit","used-reloaded"):
            name=f"real-vigilia-bag-{phase}-{width}x{height}.png"
            shot=shots[name]
            assert shot["phase"]==phase and (shot["width"],shot["height"])==(width,height)
            verified(name,shot,width,height)
elif resource:
    assert report["sourceSha"]==source and report["status"]=="UNITY_ISOLATED_IMPORT_PASS_ART_REVIEW_PENDING"
    assert report["captures"]==4 and report["realImportedSprites"]==16
    assert report["pointerRaycastHits"]==4 and report["allResourcesImported"] is True
    assert report["artApproved"] is False and report["actualCityTested"] is False
    shots={shot["file"]:shot for shot in report["images"]}
    assert len(shots)==4
    for width,height in [(1600,900),(900,1600),(844,390),(390,844)]:
        name=f"issue186-real-unity-{width}x{height}.png"
        shot=shots[name]
        assert shot["actualResourceSprites"]==12
        for flag in ("indicatorReady","newIndicatorReady","firstUsePointer",
                     "pointerLeavesCtaCenterClear","realUiClick",
                     "blockedUiClickRejected","visiblePixels","renderTargetCorrect",
                     "noOverlappingCards"):
            assert shot[flag] is True,(name,flag)
        assert shot["firstUseCompleted"] is False
        verified(name,shot,width,height)
else:
    assert report["source"]==source and report["status"]=="CITY_ART_CONTEXT_RENDERED_NOT_GAMEPLAY_CERTIFIED"
    assert report["pngCount"]==20 and report["realGatewayClaims"]==0 and report["artApproved"] is False
    shots={shot["file"]:shot for shot in report["shots"]}
    for width,height in [(390,844),(844,390)]:
        for variant in ["real-bastion-camera-baseline","real-bastion-camera-ceremony"]:
            name=f"real-city-daily-{variant}-{width}x{height}.png"
            verified(name,shots[name],width,height)
# The canonical target must be released before writing ANY review evidence.
registry=json.loads(contents("pipeline/active-workstreams.json","main"))
owners=[w for w in registry["active"] if w["id"]==workstream]
assert len(owners)==1 and owners[0]["owner"]==owner
assert "windows-runner-heavy" not in owners[0].get("resources",[]),"Target source remains frozen during native work"
pr=api("pulls/"+str(pr_number))
assert pr["state"]=="open" and pr["head"]["ref"]==target
head=api("git/ref/heads/"+target)["object"]["sha"]
assert api("compare/"+source+"..."+head)["status"] in ["identical","ahead"]
prefix=("docs/issue186/" if resource else "docs/daily-treasures/")+"remote-native-review/"+str(run_id)+"/"
records=[];elements=[]
for name,data,shot in selected:
    blob=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})
    elements.append({"path":prefix+name,"mode":"100644","type":"blob","sha":blob["sha"]})
    records.append({"file":name,"git_blob":blob["sha"],"native":shot})
proof={"source":source,"native_run":run_id,"artifact_id":artifact_id,"artifact_digest":artifact.get("digest"),
       "method":"BYTE_IDENTICAL_NATIVE_PNG_NO_RETOUCH","artApproved":False,"fullSceneVerified":False,"actualIntegratedCityFrames":4 if combined else 0,
       "realFirstHitClicks":52 if combined else 0,"sourceOverlayRestored":combined,
       "physicalMobileVerified":False,"files":records}
proof_blob=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})
elements.append({"path":prefix+"proof.json","mode":"100644","type":"blob","sha":proof_blob["sha"]})
tree=api("git/trees",{"base_tree":api("git/commits/"+head)["tree"]["sha"],"tree":elements})
commit=api("git/commits",{"message":"Preserve byte-identical actual Unity captures for owner visual review",
    "tree":tree["sha"],"parents":[head]})
# Non-force plus exact old parent rejects concurrent advancement.
api("git/refs/heads/"+target,{"sha":commit["sha"],"force":False},"PATCH")
print(json.dumps({"review_commit":commit["sha"],"source":source,"native_run":run_id,"files":records}))
