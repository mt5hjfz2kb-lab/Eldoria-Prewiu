#!/usr/bin/env python3
"""Verify a new owner-only route against the actual preceding Pages archive and live files."""
import argparse,concurrent.futures,hashlib,json,os,shutil,sys,urllib.request
from pathlib import Path
sys.path.insert(0,str(Path(__file__).with_name("iphone_ab")))
from stage_compact_delta_ab import restore_tar
EXPECTED="4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71"
NEW="beta-iphone-p0-lazy-world";OLD="beta-iphone-p0-city"
ORIGIN="https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/"
def need(ok,message):
 if not ok:raise RuntimeError(message)
def sha(path):
 with Path(path).open("rb") as f:return hashlib.file_digest(f,"sha256").hexdigest()
def get_json(path):return json.loads(Path(path).read_text(encoding="utf-8-sig"))
def api(path):
 req=urllib.request.Request("https://api.github.com/repos/"+os.environ["GITHUB_REPOSITORY"]+"/"+path,headers={"Authorization":"Bearer "+os.environ["GH_TOKEN"],"Accept":"application/vnd.github+json","User-Agent":"Eldoria-owner-preserved-publication"})
 with urllib.request.urlopen(req,timeout=40) as response:return json.load(response)
def live_hash(item):
 rel,wanted=item
 request=urllib.request.Request(ORIGIN+rel+"?owner_preservation="+os.environ["GITHUB_RUN_ID"],headers={"Accept-Encoding":"identity","Cache-Control":"no-cache","User-Agent":"Eldoria-existing-route-byte-verification"})
 with urllib.request.urlopen(request,timeout=50) as response:
  value=hashlib.file_digest(response,"sha256").hexdigest()
 need(value==wanted,"Live baseline differs from preserved archive; refuse publication: "+rel)
 return rel
def run(a):
 site=Path(a.site);base=Path(a.baseline);mac=Path(a.mac)
 for runid,source in ((43150432049,"f4bb48862f5a9f25310c7999c4877e47402d2993"),(43150932880,"a035e5ccd63ac02c2b2e6df1cf8138eba381c2b5")):
  r=api("actions/runs/"+str(runid));need(r["head_sha"]==source and r["status"]=="completed" and r["conclusion"]=="success","Pinned staging/runtime run did not pass")
 gate=api("contents/pipeline/tester-readiness/release-request.json?ref=main")
 import base64
 gate=json.loads(base64.b64decode(gate["content"]));need(gate["enabled"] is False and gate["status"]=="BLOCKED","External tester gate unexpectedly changed")
 p=get_json(site/"beta-lazy-world-owner-proof.json");identity=get_json(site/NEW/"release-identity.json")
 need(p["combined_bytes"]==972044910 and p["original_files_preserved"]==144 and not p["published"] and not p["external_testers_enabled"],"Wrong preserved staging artifact")
 need(identity["source_sha"]==p["source_sha"]==EXPECTED and not identity["release_for_external_testers"],"Wrong actual candidate identity")
 need(identity["candidate_bundle_sha256"]=="49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a","Actual native profile changed")
 reports=[x for x in mac.rglob("report.json") if x.parent.name=="deferred"];need(len(reports)==1,"Actual staged Mac arm missing")
 r=get_json(reports[0]);need(r["passed"] and not r["errors"] and r["entry"]["reloaded"] and r["lazyWorldDeferredToFirstEntryVerified"] and r["identity"]["source_sha"]==EXPECTED,"Actual staged Apple GPU check failed")
 need(len(r["storyAdvanceAttempt"]["actualCanvasClicks"])==4,"Recorded real story input missing")
 restore_tar(a.archive,base)
 files=[x for x in base.rglob("*") if x.is_file()];need(len(files)==144,"Original complete Pages archive changed")
 original={x.relative_to(base).as_posix():sha(x) for x in files};recovered=[]
 for rel,wanted in original.items():
  dest=site/rel
  if not dest.exists():
   need(any(x.startswith(".") for x in Path(rel).parts),"Staged artifact lost a non-hidden existing file: "+rel)
   dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(base/rel,dest);recovered.append(rel)
  need(sha(dest)==wanted,"An existing file changed in staged delivery: "+rel)
 live=[(rel,wanted) for rel,wanted in original.items() if not any(x.startswith(".") for x in Path(rel).parts) and rel!="CNAME"]
 with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:checked=list(pool.map(live_hash,live))
 need(get_json(site/OLD/"release-identity.json")["source_sha"]=="63fb2095412a3066e18f3a8a31c2a03f40ef4944","Working beta14 city changed")
 need(get_json(site/"beta-native325/release-identity.json")["source_sha"]=="be49d62e3daf490f41dba0bf8123a8e2eee64480","Working beta10 changed")
 total=sum(x.stat().st_size for x in site.rglob("*") if x.is_file());need(total<980000000,"Preserved owner site exceeds existing Pages budget")
 proof={"role":"OWNER_ONLY_QA_PUBLICATION_INPUT_VERIFIED","source_sha":EXPECTED,"staging_run":43150432049,"actual_staged_mac_run":43150932880,"old_pages_run":43006294342,"original_files_preserved":144,"live_original_files_sha256_checked":len(checked),"recovered_exact_hidden_files":recovered,"site_bytes":total,"physical_iphone_verified":False,"world_entry_verified":False,"art_approved":False,"external_testers_enabled":False,"owner_route":NEW+"/?eldoria_p0_lazy_world=1&eldoria_diag=1","publication_workflow_run":os.environ["GITHUB_RUN_ID"],"original_fingerprints":original}
 (site/"beta-mobile16-owner-publication-proof.json").write_text(json.dumps(proof,indent=2)+"\n")
 need(sum(x.stat().st_size for x in site.rglob("*") if x.is_file())<980000000,"Proof exceeded preserved budget")
 print("OWNER_ONLY_EXACT_ALL_EXISTING_LIVE_FILES_PRESERVED",json.dumps({k:v for k,v in proof.items() if k!="original_fingerprints"}),flush=True)
if __name__=="__main__":
 p=argparse.ArgumentParser()
 for key in ("site","baseline","mac","archive"):p.add_argument("--"+key,required=True)
 run(p.parse_args())
