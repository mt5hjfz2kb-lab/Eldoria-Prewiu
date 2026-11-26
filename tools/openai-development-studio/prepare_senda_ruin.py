import json,hashlib,subprocess,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu";SHA="dbe15e6c7f3f5c7400663103b0e0b0514a9c8fa1";RUN=45772369711
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(b):return hashlib.sha256(b).hexdigest()
root,out=map(Path,sys.argv[1:]);out.mkdir()
r=gh(f"repos/{REPO}/actions/runs/{RUN}");assert r["head_sha"]==SHA and r["conclusion"]=="success"
refs=[]
for name in ("iphone-gates.png","iphone-battle.png"):
 b=(root/name).read_bytes();assert b[:8]==b"\x89PNG\r\n\x1a\n";(out/name).write_bytes(b);refs.append({"path":name,"sha256":sha(b)})
s=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/tools/senda-brecha/visual-3d/index.html?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode()
a=s.index("function buildValoriaLandscape()");b=s.index("function makeScene()",a);excerpt=s[a:b][:10500].encode();(out/"source.txt").write_bytes(excerpt)
d={"schema":1,"target_id":"SENDA_VALORIA_ROADSIDE_RUIN","source_sha":SHA,"source_run":RUN,"mode":"art_candidate","routing_profile":"standard","objective":"Spanish, maximum 180 words per role. Actual new scene contains Valoria and the Breach as separate spatial objects, three physical portal arches, authentic animated guard rigs and worn slate. Main gap to concept is foreground composition: roadside repeated primitive blocks, flat bright grass and uniform tree silhouettes. Produce a detailed isolated noble Valoria roadside ruin and propose changes to lighting, depth, terrain and contact that maintain lane readability.",
"scope":"One isolated spatial 2.5D ruined military stone architectural prop, front three-quarter view, used only at road verges with physical base/contact shadow and depth. No scene, no roadway illustration, no background, no new gameplay, no main. Preserve original Valoria assets and current 3D gates and models.",
"acceptance":"Full isolated shape with alpha, no backdrop, no ground rectangle, integrated masonry and restrained vegetation at foot, no text. Reject if it occupies playable lane or overwhelms hero. Validate same fixture A/B and fixed graphics budget, actual mobile-sized render.",
"asset_brief":"A single original game-ready isolated Valoria medieval roadside ruin prop, tall broken weathered stone arch and partial masonry pillar with ancient noble fortress heraldry and hanging small deep teal cloth banner with golden stylized keep insignia, moss and ivy on the stone, a small antique iron lantern with warm amber glow at the foot. Rich painterly realistic commercial fantasy game art matching detailed dark fantasy medieval Valoria. Frontal three-quarter view, visible side thickness, broad natural base with scattered attached broken stones, no large flat ground plane. Dark warm gray slate stone, restrained aged brass and moss green. Transparent background. No sky, no forest backdrop, no scene, no text or numbers, no characters. Entire prop inside canvas, transparent interior opening. Strong readable silhouette with high resolution chisel detail, realistic light from upper right. One contiguous asset.",
"source":{"path":"source.txt","sha256":sha(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(d,ensure_ascii=False,indent=2));print("SENDA_LANDMARK_PACKET",sha(excerpt),refs)
