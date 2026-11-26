import json,hashlib,subprocess,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu";SHA="caa270d54ee77359cf9adcf38c223c5104f60d8e";RUN=45775081888
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(b):return hashlib.sha256(b).hexdigest()
root,out=map(Path,sys.argv[1:]);out.mkdir()
r=gh(f"repos/{REPO}/actions/runs/{RUN}");assert r["head_sha"]==SHA and r["conclusion"]=="success"
refs=[]
for name in ("iphone-gates.png","iphone-battle.png"):
 b=(root/name).read_bytes();assert b[:8]==b"\x89PNG\r\n\x1a\n";(out/name).write_bytes(b);refs.append({"path":name,"sha256":sha(b)})
s=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/tools/senda-brecha/visual-3d/index.html?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode()
parts=[]
for a,b in [("function buildDistantCauseway()","function buildCombatSparks()"),("function makeScene()","function resize()"),("function present(t)","function loop(now)")]:
 i=s.index(a);j=s.index(b,i);parts.append(s[i:j][:3200])
excerpt="\n".join(parts).encode();assert len(excerpt)<=12000
(out/"source.txt").write_bytes(excerpt)
d={"schema":1,"target_id":"SENDA_COMMERCIAL_FINAL_REVIEW","source_sha":SHA,"source_run":RUN,"mode":"review","routing_profile":"standard","objective":"Spanish, maximum 200 words per role. Independently assess the two actual rendered mobile frames for commercial visual quality, Valoria identity and battle readability. Identify the single highest-impact concrete visual change possible with current geometry/assets (lighting, composition, ground contact, effects, framing). Describe evidence, not generic praise. The conceptual target is painterly high-end medieval fortress road, atmospheric depth and readable monumental magical portals, noble brass/teal identity, distinct animated troops. New lane spring is being tested separately; do not infer movement from still frames.","scope":"Read-only source-specific review. Existing 3D gameplay, portal values, original hero and rig, separate scenery objects and performance budget preserved. No new image generation, no gameplay changes, no main. Do not pretend to see the approved concept itself, device performance or code not supplied.","acceptance":"Identify a visible deficiency and name exact relevant source symbols, bounded reversible correction and quantitative/visual acceptance checks. Independent auditor should explicitly reject any claim of commercial approval if unsupported. Prioritize an improvement usable in both Unity and WebGL. Report what cannot be judged from screenshots.","source":{"path":"source.txt","sha256":sha(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(d,ensure_ascii=False,indent=2));print("SENDA_FINAL_REVIEW_PACKET",sha(excerpt))
