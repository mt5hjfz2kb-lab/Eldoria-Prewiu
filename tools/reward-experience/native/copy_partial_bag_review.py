"""Preserve one exact partial real bag screenshot; never certify a failed combined run."""
import base64,hashlib,json,os,struct,urllib.request,urllib.error
from pathlib import Path
REPO=os.environ["GITHUB_REPOSITORY"];assert REPO=="mt5hjfz2kb-lab/Eldoria-Prewiu"
def api(path,data=None,method=None):
 req=urllib.request.Request("https://api.github.com/repos/"+REPO+"/"+path,data=None if data is None else json.dumps(data).encode(),method=method,headers={"Authorization":"Bearer "+os.environ["GH_TOKEN"],"Accept":"application/vnd.github+json","Content-Type":"application/json","User-Agent":"Eldoria-exact-partial-bag-review"})
 with urllib.request.urlopen(req,timeout=40) as response:return json.load(response)
def contents(path,ref):
 x=api("contents/"+path+"?ref="+ref)
 if not x.get("content"):x=api("git/blobs/"+x["sha"])
 return base64.b64decode(x["content"])
runid=43155182543;source="134cf1a029800481510f1784cf30393dbd0806f6";artifact=13915329220
run=api("actions/runs/"+str(runid));assert run["head_sha"]==source and run["status"]=="completed" and run["conclusion"]=="failure"
a=api("actions/artifacts/"+str(artifact));assert a["workflow_run"]["id"]==runid and a["workflow_run"]["head_sha"]==source and a["digest"]=="sha256:cc48637e9e4e538a32de3284a7bdea6824f454b2ff4f7914f180f6d117cac287"
root=Path("partial-native");paths=list(root.rglob("combined-bag/report.json"));assert len(paths)==1
r=json.loads(paths[0].read_text(encoding="utf-8-sig"));assert r["resourceSource"]==source and r["nativeRun"]==str(runid)
assert r["status"]=="REAL_VIGILIA_SAVED_BAG_FAIL" and r["actualBagOnVigilia"] and r["seenDoesNotUse"] and r["skipDoesNotComplete"] and r["useCreditsOnce"] and not r["fullMailboxUiFlowVerified"]
assert len(r["images"])==1 and r["images"][0]["phase"]=="new" and r["realFirstHitClicks"]==5
shot=r["images"][0];data=(paths[0].parent/shot["file"]).read_bytes()
assert len(data)==shot["bytes"] and hashlib.sha256(data).hexdigest()==shot["sha256"] and data[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",data[16:24])==(390,844)
reg=json.loads(contents("pipeline/active-workstreams.json","main"));owners=[x for x in reg["active"] if x["id"]=="eldoria-issue186-reward-attention-guidance-20261111"]
assert len(owners)==1 and owners[0]["owner"]=="chatgpt-central-resource-art-20261111"
if "windows-runner-heavy" in owners[0].get("resources",[]):
 print("PARTIAL_REVIEW_DEFERRED_WHILE_NATIVE_SOURCE_FROZEN");raise SystemExit(0)
branch="feature/issue186-reward-attention-guidance-20261111";head=api("git/ref/heads/"+branch)["object"]["sha"]
prefix="docs/issue186/remote-native-review/"+str(runid)+"/"
try:
 prior=json.loads(contents(prefix+"partial-bag-proof.json",head))
except urllib.error.HTTPError as e:
 if e.code!=404:raise
else:
 assert prior["sha256"]==shot["sha256"];print("EXACT_PARTIAL_BAG_ALREADY_PRESERVED");raise SystemExit(0)
blob=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})["sha"]
proof={"source":source,"native_run":runid,"artifact":artifact,"combined_status":r["status"],"error":r["error"],"real_first_hit_bag_clicks":5,"bag_seen_skip_use_back_proven":True,"full_mailbox_ui_verified":False,"art_approved":False,"physical_iphone_verified":False,"file":shot["file"],"sha256":shot["sha256"],"git_blob":blob}
p=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})["sha"]
tree=api("git/trees",{"base_tree":head,"tree":[{"path":prefix+shot["file"],"mode":"100644","type":"blob","sha":blob},{"path":prefix+"partial-bag-proof.json","mode":"100644","type":"blob","sha":p}]})["sha"]
commit=api("git/commits",{"message":"Preserve exact partial actual saved bag picture without certifying failed mailbox flow","tree":tree,"parents":[head]})["sha"]
api("git/refs/heads/"+branch,{"sha":commit,"force":False},"PATCH")
print("ACTUAL_PARTIAL_BAG_PNG_EXPORTED_NO_MAIL_ART_OR_DEVICE_APPROVAL",commit,json.dumps(proof))
