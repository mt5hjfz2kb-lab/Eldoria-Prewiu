#!/usr/bin/env python3
"""Copy four byte-identical Mac/WebKit screenshots into the released beta review scope."""
import base64,hashlib,json,os,struct,urllib.request
from pathlib import Path
REPO=os.environ["GITHUB_REPOSITORY"];BRANCH="feature/eldoria-private-beta-i-v-20261107"
ROOT="https://api.github.com/repos/"+REPO+"/"
def api(path,data=None,method=None):
 req=urllib.request.Request(ROOT+path,data=None if data is None else json.dumps(data).encode(),method=method,headers={"Authorization":"Bearer "+os.environ["GH_TOKEN"],"Accept":"application/vnd.github+json","Content-Type":"application/json","User-Agent":"Eldoria-actual-Mac-review"})
 with urllib.request.urlopen(req,timeout=40) as r:return json.load(r)
reg=api("contents/pipeline/active-workstreams.json?ref=main")
if not reg.get("content"):reg=api("git/blobs/"+reg["sha"])
reg=json.loads(base64.b64decode(reg["content"]))
owner=[x for x in reg["active"] if x["id"]=="eldoria-private-beta-i-v-20261107"]
assert len(owner)==1 and owner[0]["ownership_state"]=="claimed" and "windows-runner-heavy" not in owner[0].get("resources",[])
run=api("actions/runs/43150932880");assert run["status"]=="completed" and run["conclusion"]=="success" and run["head_sha"]=="a035e5ccd63ac02c2b2e6df1cf8138eba381c2b5"
base="docs/eldoria-private-beta/remote-native-review/43150932880/"
entries=[];proof={"source_sha":"4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71","native_mac_run":43150932880,"physical_iphone_verified":False,"world_entry_verified":False,"visual_name_verification":"pending image inspection","art_approved":False,"images":[],"arms":{}}
for mode in ("deferred",):
 reports=[p for p in Path("actual-mac").rglob("report.json") if p.parent.name==mode];assert len(reports)==1
 r=json.loads(reports[0].read_text());assert r["passed"] and not r["errors"] and r["entry"]["reloaded"] and r["identity"]["source_sha"]==proof["source_sha"] and r["identity"]["candidate_bundle_sha256"]=="49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a"
 assert r["lazyWorldDeferredToFirstEntryVerified"] and len(r["storyAdvanceAttempt"]["actualCanvasClicks"])==4
 proof["arms"][mode]={"passed":True,"entry":r["entry"],"identity":r["identity"],"storyAttempt":r["storyAdvanceAttempt"]}
 for phase in ("after-name","reloaded","story-step-0","story-step-1","story-step-2","story-step-3"):
  p=reports[0].parent/(phase+".png");data=p.read_bytes();assert data[:8]==b"\x89PNG\r\n\x1a\n" and len(data)>10000
  w,h=struct.unpack(">II",data[16:24]);name=mode+"-"+phase+".png"
  sha=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})["sha"]
  entries.append({"path":base+name,"mode":"100644","type":"blob","sha":sha})
  for index,start in enumerate(range(0,len(data),600000)):
   chunk=data[start:start+600000];part=api("git/blobs",{"content":base64.b64encode(chunk).decode(),"encoding":"base64"})["sha"]
   entries.append({"path":base+name+".bin-%02d"%index,"mode":"100644","type":"blob","sha":part})
  proof["images"].append({"file":name,"bytes":len(data),"width":w,"height":h,"sha256":hashlib.sha256(data).hexdigest()})
sha=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})["sha"]
entries.append({"path":base+"proof.json","mode":"100644","type":"blob","sha":sha})
parent=api("git/ref/heads/"+BRANCH)["object"]["sha"]
tree=api("git/trees",{"base_tree":parent,"tree":entries})["sha"]
commit=api("git/commits",{"message":"Preserve actual staged mobile16 entry/reload/story screenshots for visual review","tree":tree,"parents":[parent]})["sha"]
api("git/refs/heads/"+BRANCH,{"sha":commit,"force":False},"PATCH")
print("ACTUAL_MAC_PNG_EXPORTED_NO_ART_OR_PHYSICAL_APPROVAL",commit,json.dumps(proof["images"]))
