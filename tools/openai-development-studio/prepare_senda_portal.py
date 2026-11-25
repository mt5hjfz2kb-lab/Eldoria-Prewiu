"""Pinned actual web captures and source. Approved concept is described, not embedded."""
import json,hashlib,subprocess,base64,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
SHA="ebac2a7c3219dc02d1a28cd3dcd504e7fa3d486b";RUN=45708451082
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def sha(x):return hashlib.sha256(x).hexdigest()
root,out=map(Path,sys.argv[1:]);out.mkdir()
r=gh(f"repos/{REPO}/actions/runs/{RUN}");assert r["head_sha"]==SHA and r["conclusion"]=="success"
refs=[]
for name,digest in [("iphone-battle.png","99a7a47a4fdc8e777941b853e4b35a9cf42db90c8496bc31e72eb06324a445bb"),("iphone-gates.png","9ff134a2b927ac66e3f2bb1867c60247a5cb236ab1165b851efee916b5a59351")]:
    data=(root/name).read_bytes();assert sha(data)==digest
    (out/name).write_bytes(data);refs.append({"path":name,"sha256":digest})
f=gh(f"repos/{REPO}/contents/tools/senda-brecha/visual-3d/index.html?ref={SHA}")
raw=base64.b64decode(f["content"]);assert hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==f["sha"]
s=raw.decode()
a=s.index("function gateLabel(");b=s.index("function syncGates(",a)
excerpt=s[a:b].encode();assert 0<len(excerpt)<=12000
(out/"source.txt").write_bytes(excerpt)
d={"schema":1,"target_id":"SENDA_VALORIA_PORTAL_FRAME","source_sha":SHA,"source_run":RUN,"mode":"art_candidate","routing_profile":"standard",
"objective":"Spanish reports, maximum 220 words per role. The real scene's gates look thin, generic and flat. The approved concept features tall substantial carved Valorian stone portals, gilded pointed capstones, blue-green heraldic flags, fine aged brass trims, elegant crest, a glowing colored magical interior, and legible independent labels. Design a modular front facade to improve the physical gate, not replace it with a scene. Identify specific changes from actual supplied frames. Give exact fit criteria.",
"scope":"Keep three separate physical 3D portal frames, lane spacing, gate mechanics, live labels, current six waves and real animated characters. Only front-facing cutout stone cladding candidate; real 3D side/back thickness stays, actual energy shader remains in opening. No rendered screenshot overlay, no new gameplay, no claims of approval. Need transparent central opening, no glow baked in, no text. Reject if mismatched or unsuitable.",
"acceptance":"Require correct alpha silhouette, open hole, no floor/background, symmetry, readable texture at mobile scale, no protrusions into neighboring lanes. Programmer proposes matched native camera A/B and front mesh placement with retained 3D depth; auditor must examine actual generated PNG and reject if incompatible. No commercial-grade claim until game render.",
"asset_brief":"Original game-ready isolated FRONT ORTHOGRAPHIC stone portal facade, transparent exterior AND large transparent arched central opening. Symmetric, vertically elongated elegant medieval Valoria military arch: dark weathered slate-gray stone blocks, richly engraved but restrained aged bronze filigree, a small embossed stylized fortress crest at top, slender teal-blue enamel accents in pillars, substantial carved capitals and bases. Painterly realistic commercial fantasy game asset, same identity as noble fortified medieval city. Flat even diffuse illumination, visible fine chisel detail, no directional shadows, no perspective tilt, no doors or glow inside opening, no text, no letters, no numbers, no floor, no scenery, no people. Frame alone; opening spans at least 55% of width and 60% of height. Keep within canvas, alpha clear.",
"source":{"path":"source.txt","sha256":sha(excerpt)},"images":refs}
(out/"packet.json").write_text(json.dumps(d,ensure_ascii=False,indent=2));print("SENDA_PORTAL_PACKET_VERIFIED",sha(excerpt))
