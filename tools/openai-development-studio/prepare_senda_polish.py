"""Exact rendered web evidence, exact source excerpt; no invented concept pixels."""
import json,hashlib,subprocess,base64,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"; SHA="6668a00adef5c27a27c0775f0fd237e1729e1afb"; RUN=45704914735
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(b):return hashlib.sha256(b).hexdigest()
root=Path(sys.argv[1]);out=Path(sys.argv[2]);out.mkdir()
r=gh(f"repos/{REPO}/actions/runs/{RUN}");assert r["head_sha"]==SHA and r["conclusion"]=="success"
refs=[]
for name,digest in [("iphone-battle.png","45faec91c5d590d66f95767c8412588572b1b17577d7fe5fab8cce6bec6cad30"),("iphone-gates.png","bcb81627be46a262dd8b156c722b913f4441e84d06ef6a5cf182948841c96512")]:
    raw=(root/name).read_bytes();assert sha(raw)==digest
    (out/name).write_bytes(raw);refs.append({"path":name,"sha256":digest})
p="tools/senda-brecha/visual-3d/index.html"
f=gh(f"repos/{REPO}/contents/{p}?ref={SHA}");raw=base64.b64decode(f["content"]);assert hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==f["sha"]
s=raw.decode();parts=[]
for a,b in [("function buildGate(index)","function gateColor"),("function buildValoriaLandscape()","function addAtmosphere()"),("function makeScene()","function resize()")]:
    i=s.index(a);j=s.index(b,i);parts.append(s[i:j][:3500])
excerpt="\n".join(parts).encode();assert 0<len(excerpt)<=12000
(out/"source.txt").write_bytes(excerpt)
packet={"schema":1,"target_id":"SENDA_CONCEPT_POLISH","source_sha":SHA,"source_run":RUN,"mode":"art_candidate","routing_profile":"standard",
"objective":"Spanish reports, max 250 words per role. Close the largest visual gap in actual PR195 renders versus approved concept (described, not supplied): painterly worn rectangular stone causeway with fine brass lane inlay, realistic carved masonry portals with teal/gold/red light, layered dark misty forest, Valoria castle on a credible hillside, violet geological Breach, warm lanterns, cohesive brass/teal HUD. Diagnose the supplied genuine gate and battle frames: current road is repetitive bright cobbles, repeated conifers, empty horizon, thin portals, dark rigid figures. Deliver a specific high-impact change using original geometry and actual runtime shaders.",
"scope":"Preserve six waves, gate values, lane steering, victory/defeat and actual Aldric rig. No new gameplay, no beta/main, no flattened full scene. Existing source excerpts, not entire source. No claims of implementation or approval. Artist produces one surface texture only; programmer must propose actual runtime use and measurable visual/performance checks.",
"acceptance":"Prioritize material, environment hierarchy and contrast; reject any image with perspective, text, scene or transparent borders as unsuitable for tiling. Auditor receives actual generated image and gate frame; distinguish material suitability from final scene proof. Give concrete implementation parameters and fail conditions.",
"asset_brief":"A single seamless square orthographic TOP-DOWN game PBR base-color material tile, edge-to-edge opaque: aged medieval rectangular slate flagstones, large staggered hand-cut rectangular slabs with subtle irregular edges and fine cracked mortar, dark warm neutral gray brown, restrained muted ochre wear, subtle moss only in sparse joints, painterly realistic high-end fantasy. Flat neutral lighting, no directional shadows, no perspective, no borders, no scene, no objects, no characters, no letters. Tile must repeat without obvious border. Not a road illustration: just stone surface albedo.",
"source":{"path":"source.txt","sha256":sha(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(packet,ensure_ascii=False,indent=2));print("SENDA_POLISH_SOURCE_AND_TWO_FRAMES_VERIFIED",sha(excerpt))
