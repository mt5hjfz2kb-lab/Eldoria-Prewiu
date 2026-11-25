"""Prepare authentic baseline packet; source and frames verified, never run downloaded code."""
import json,hashlib,subprocess,base64,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
SHA="cb0193132fc6ed67c19f616f7fa9facf1155b79e"
RUN=43732245691
SOURCE="Unity/Assets/Eldoria/SendaBrecha/SendaBrechaPrototype.cs"
BLOB="83417aa02ba5016b71f45c6b238ed42a05307cf5"
def gh(path):return json.loads(subprocess.check_output(["gh","api",path],timeout=60))
def digest(raw):return hashlib.sha256(raw).hexdigest()
root=Path(sys.argv[1]);out=Path(sys.argv[2]);out.mkdir()
run=gh(f"repos/{REPO}/actions/runs/{RUN}")
assert run["head_sha"]==SHA and run["conclusion"]=="success"
proof=(root/"proof.json").read_bytes()
assert digest(proof)=="b8f871df0aabf802c8a44a8f47151a23e223466ca68f6e00af6faef97130a186"
p=json.loads(proof)
assert p["capturedPng"]==9 and p["positiveGateChangesVisibleActors"] and p["victoryRouteChecked"] and p["defeatRouteChecked"] and not p["artApproved"]
refs=[]
for n,h in [
 ("02-choose-lane-390x844.png","08b2e7e7612f3b6f02a614e12a3620010dd73911acd92632cbbd09d3e1ab19c0"),
 ("04-real-enemy-render-390x844.png","c4afebebee4467cd71f86c5a3f04b9165d981883a581e790f7644eb0dc7db9fe")]:
    raw=(root/n).read_bytes();assert digest(raw)==h
    (out/n).write_bytes(raw);refs.append({"path":n,"sha256":h})
source=gh(f"repos/{REPO}/contents/{SOURCE}?ref={SHA}")
assert source["sha"]==BLOB and source["encoding"]=="base64"
raw=base64.b64decode(source["content"])
assert hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==BLOB
s=raw.decode()
a=s.index("        private void BuildCameraAndLighting()");b=s.index("        public void Begin()")
excerpt=s[a:b].encode()
assert len(excerpt)<=12000
(out/"source.txt").write_bytes(excerpt)
packet={"schema":1,"target_id":"SENDA_PR195_ART","source_sha":SHA,"source_run":RUN,"mode":"art_candidate","routing_profile":"standard",
"objective":"Elevate the existing three-lane continuous medieval action minigame from gray primitive demo to coherent dark medieval adventure. Deliver a practical, high-impact visual direction and source-grounded geometry/lighting changes. Write in Spanish. Limit each role's response to 350 words, concrete and concise; do not enumerate every possible change. The art asset is a small original Valorian heraldic emblem used in HUD only; it is not the solution for 3D geometry.",
"scope":"Keep the existing six waves, gate values, route rules, movement and combat behavior. No production state, currency, beta, main, copying commercial IP, or claim of native/mobile acceptance. Use genuine 3D geometry and legibility, avoid flat image scenery. Suggest changes that fit the existing procedural Unity and Three.js presentation. Existing source is exact visual methods excerpt.",
"acceptance":"Prioritize stronger hero and enemies silhouette, lane legibility, physical gates, textured-looking ground, reduced violet wash and real movement. Independent auditor must reject irrelevant image or proposals; no quality approval before rendered before/after. Make suggestions concrete.",
"asset_brief":"One isolated small original medieval heraldic UI emblem for the Valorian expedition: aged brass shield-shaped insignia with a narrow fractured violet crystal in center, restrained dark iron edging, symmetrical readable silhouette, painterly engraved metal, no letters, no scene, transparent background. Only a HUD crest, not world geometry.",
"source":{"path":"source.txt","sha256":digest(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(packet,ensure_ascii=False,indent=2))
print("SENDA_BASELINE_VERIFIED",digest(excerpt))
