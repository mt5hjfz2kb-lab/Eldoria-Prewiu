#!/usr/bin/env python3
"""Copy untouched actual normal-player WebKit journey evidence, preserving failures honestly."""
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
run=api("actions/runs/43171823166");assert run["status"]=="completed" and run["conclusion"] in ("success","failure") and run["head_sha"]=="a74dc1ac7a5ddc43fd5950ebc90d905ceafd53c7"
base="docs/eldoria-private-beta/remote-native-review/43171823166/"
entries=[];proof={"source_sha":"4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71","native_mac_run":43171823166,"physical_iphone_verified":False,"world_entry_verified":False,"visual_name_verification":"pending image inspection","art_approved":False,"images":[],"arms":{}}
for mode in ("deferred",):
 reports=[p for p in Path("actual-mac").rglob("report.json") if p.parent.name==mode];assert len(reports)==1
 r=json.loads(reports[0].read_text());assert r["identity"]["source_sha"]==proof["source_sha"] and r["identity"]["candidate_bundle_sha256"]=="49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a"
 proof["arms"][mode]=r
 # A failed OCR or map click remains a failure; screenshots are not a pass.
 proof["workflow_conclusion"]=run["conclusion"]
 proof["actual_ui_journey_passed"]=bool(r.get("passed"))
 for phase in ("story-step-4","city-intro-complete","actual-sawmill-offer","actual-sawmill-after-wait","actual-world-control","actual-world-after-entry","actual-world-ready","actual-city-return"):
  if not (reports[0].parent/(phase+".png")).is_file():continue
  p=reports[0].parent/(phase+".png");data=p.read_bytes();assert data[:8]==b"\x89PNG\r\n\x1a\n" and len(data)>10000
  w,h=struct.unpack(">II",data[16:24]);name=mode+"-"+phase+".png"
  sha=api("git/blobs",{"content":base64.b64encode(data).decode(),"encoding":"base64"})["sha"]
  entries.append({"path":base+name,"mode":"100644","type":"blob","sha":sha})
  for index,start in enumerate(range(0,len(data),600000)):
   chunk=data[start:start+600000];part=api("git/blobs",{"content":base64.b64encode(chunk).decode(),"encoding":"base64"})["sha"]
   entries.append({"path":base+name+".bin-%02d"%index,"mode":"100644","type":"blob","sha":part})
  proof["images"].append({"file":name,"bytes":len(data),"width":w,"height":h,"sha256":hashlib.sha256(data).hexdigest()})
  ocr=reports[0].parent/(phase+"-ocr.json")
  if ocr.is_file():
   ocr_blob=api("git/blobs",{"content":ocr.read_text(),"encoding":"utf-8"})["sha"]
   entries.append({"path":base+mode+"-"+phase+"-ocr.json","mode":"100644","type":"blob","sha":ocr_blob})
sha=api("git/blobs",{"content":json.dumps(proof,indent=2)+"\n","encoding":"utf-8"})["sha"]
entries.append({"path":base+"proof.json","mode":"100644","type":"blob","sha":sha})
parent=api("git/ref/heads/"+BRANCH)["object"]["sha"]
tree=api("git/trees",{"base_tree":parent,"tree":entries})["sha"]
commit=api("git/commits",{"message":"Preserve untouched actual player UI journey screenshots and honest world entry outcome","tree":tree,"parents":[parent]})["sha"]
api("git/refs/heads/"+BRANCH,{"sha":commit,"force":False},"PATCH")
print("ACTUAL_MAC_PNG_EXPORTED_NO_ART_OR_PHYSICAL_APPROVAL",commit,json.dumps(proof["images"]))
