import json,hashlib,subprocess,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu";SHA="1bdff784ff961ddfe76a48cd82b9b991e1695e52";RUN=45718667425
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(b):return hashlib.sha256(b).hexdigest()
root,out=map(Path,sys.argv[1:]);out.mkdir()
r=gh(f"repos/{REPO}/actions/runs/{RUN}");assert r["head_sha"]==SHA and r["conclusion"]=="success"
refs=[]
for name in ("iphone-gates.png","iphone-battle.png"):
 b=(root/name).read_bytes();assert b[:8]==b"\x89PNG\r\n\x1a\n";(out/name).write_bytes(b);refs.append({"path":name,"sha256":sha(b)})
s=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/tools/senda-brecha/visual-3d/index.html?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode()
a=s.index("function buildValoriaLandscape()");b=s.index("function makeScene()",a);excerpt=s[a:b][:10500].encode();(out/"source.txt").write_bytes(excerpt)
d={"schema":1,"target_id":"SENDA_BREACH_LANDMARK","source_sha":SHA,"source_run":RUN,"mode":"art_candidate","routing_profile":"standard","objective":"Spanish, maximum 180 words per role. Actual game lacks monumental geographic focal point, forest hierarchy and directional light. Approved concept features distant Valoria on the left of a military causeway and towering jagged geological Breach glowing violet on right. Propose high-impact environment changes with spatial grounding, fog depth, actual silhouettes and shadows. Do not overstate current quality.",
"scope":"One isolated scenic rock landmark sprite to be integrated as a distant 2.5D world object outside playable road, anchored to real terrain. No full scene or screenshot overlay. Preserve all gameplay, geometry lane gates, Valoria identity, canonical hero. Keep original assets. Test matched camera A/B and draw calls.",
"acceptance":"Transparent silhouette, no rectangular backdrop, no detached rock feet, no text. Artist creates only isolated mountain landmark; auditor must reject unsuitable art. Programmer proposes explicit spatial placement and independently testable criteria.",
"asset_brief":"Game-ready isolated painterly dark fantasy geological landmark for Valoria: one large jagged mountain mass of fractured dark slate and basalt, steep uneven silhouette, thin glowing violet fissure splitting the mountain from its foot to its peak, a few embedded violet crystals, broken angular rock spires. Rich detailed hand-painted commercial fantasy illustration, muted slate blue and purple, warm subtle rim light, compatible with medieval isometric strategy game Eldoria. Front-facing distant scenery asset, wide grounded base. Transparent background, no sky, no fog background, no floor plane, no forest, no castles, no buildings, no people, no text. Whole rock mass fully inside frame, irregular outline, no flat rectangular edges; no floating parts.",
"source":{"path":"source.txt","sha256":sha(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(d,ensure_ascii=False,indent=2));print("SENDA_LANDMARK_PACKET",sha(excerpt),refs)
