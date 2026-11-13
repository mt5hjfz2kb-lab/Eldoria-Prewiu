"""Expose exact failed Hall screenshot and original approved UI sources, never approve art."""
import base64,hashlib,json,os,struct,urllib.request,urllib.error
from pathlib import Path
REPO=os.environ["GITHUB_REPOSITORY"];BRANCH="feature/forja-oscura-native-proof-20261107"
def api(path,data=None,method=None):
 req=urllib.request.Request("https://api.github.com/repos/"+REPO+"/"+path,data=None if data is None else json.dumps(data).encode(),method=method,headers={"Authorization":"Bearer "+os.environ["GH_TOKEN"],"Accept":"application/vnd.github+json","Content-Type":"application/json"})
 with urllib.request.urlopen(req,timeout=45) as response:return json.load(response)
def content(path,ref):
 d=api("contents/"+path+"?ref="+ref)
 if not d.get("content"):d=api("git/blobs/"+d["sha"])
 return base64.b64decode(d["content"])
run=api("actions/runs/43161203990")
assert run["status"]=="completed" and run["conclusion"]=="failure" and run["head_sha"]=="2c1d70fb2a1e6271e4853f7f7eabcd6d84aa4a4c"
artifact=api("actions/artifacts/13917529325")
assert artifact["workflow_run"]["id"]==43161203990 and artifact["digest"]=="sha256:5cceb77603ab98acf945873dc3d6e1b9277e20fbc14d3516fa85f6c4ddf91da9"
paths=list(Path("actual-failed-ui").rglob("real-unity-heroes-1600x900.png"));assert len(paths)==1
source="af658a6cc1a02d45d7efc9c57dafef24e8512564";base="docs/ui-v3/remote-native-review/43161203990/"
files={"real-unity-heroes-1600x900.png":paths[0].read_bytes()}
for name in ("panel","resource"):
 files["original-approved-"+name+".png"]=content("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/CleanRoom/"+name+".png",source)
head=api("git/ref/heads/"+BRANCH)["object"]["sha"]
try:
 existing=json.loads(content(base+"proof.json",head))
except urllib.error.HTTPError as error:
 if error.code!=404:raise
else:
 assert existing["actual_native_run"]==43161203990;print("EXACT_HALL_AND_REFERENCE_INPUTS_ALREADY_PRESERVED");raise SystemExit(0)
proof={"actual_native_run":43161203990,"native_source":run["head_sha"],"native_result":"FAILURE_HALL_GLYPH_GUTTER_2219_PIXELS","original_reference_source":source,"art_approved":False,"full_mailbox_verified":False,"images":[]};entries=[]
for name,data in files.items():
 assert data[:8]==b"\x89PNG\r\n\x1a\n" and 10000<len(data)<12000000
 width,height=struct.unpack(">II",data[16:24])
 if name.startswith("real-unity"):assert (width,height)==(1600,900)
 sha=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})["sha"]
 entries.append({"path":base+name,"mode":"100644","type":"blob","sha":sha})
 for i,start in enumerate(range(0,len(data),600000)):
  part=api("git/blobs",{"content":base64.b64encode(data[start:start+600000]).decode(),"encoding":"base64"})["sha"]
  entries.append({"path":base+name+".bin-%02d"%i,"mode":"100644","type":"blob","sha":part})
 proof["images"].append({"file":name,"width":width,"height":height,"bytes":len(data),"sha256":hashlib.sha256(data).hexdigest()})
b=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})["sha"]
entries.append({"path":base+"proof.json","mode":"100644","type":"blob","sha":b})
tree=api("git/trees",{"base_tree":head,"tree":entries})["sha"]
commit=api("git/commits",{"message":"Preserve untouched failed Hall render and original approved source panels for direct art diagnosis","tree":tree,"parents":[head]})["sha"]
api("git/refs/heads/"+BRANCH,{"sha":commit,"force":False},"PATCH")
print("EXACT_FAILED_HALL_SOURCE_REVIEW",commit,json.dumps(proof))
