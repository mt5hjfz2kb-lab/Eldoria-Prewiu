"""Prepare a real WebGL mobile review; no claim of native Editor capture."""
import hashlib,json,struct,subprocess,sys
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
SHA="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
RUN=45638738009
def gh(endpoint):return json.loads(subprocess.check_output(["gh","api",endpoint],timeout=60))
def sha(data):return hashlib.sha256(data).hexdigest()
def main(root,out):
    out.mkdir()
    run=gh(f"repos/{REPO}/actions/runs/{RUN}")
    assert run["head_sha"]==SHA and run["status"]=="completed" and run["conclusion"]=="success"
    artifacts=gh(f"repos/{REPO}/actions/runs/{RUN}/artifacts?per_page=100")["artifacts"]
    assert any(a["id"]==15037184906 and a["name"]=="ciudad-region-recorrido" and not a["expired"] for a in artifacts)
    manifest=json.loads((root/"journey.json").read_text(encoding="utf-8"))
    assert manifest["viewport"]==[390,844]
    images=[]
    for name in ("bastion-1.png","bastion-5-completo.png"):
        data=(root/name).read_bytes()
        assert data[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",data[16:24])==(390,844)
        (out/name).write_bytes(data)
        images.append({"path":name,"sha256":sha(data)})
    path="Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DInterface.cs"
    src=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/{path}?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode("utf-8")
    a=src.index("public void Layout(float width,float left,float safeWidth,float top)")
    b=src.index("private static void AccentHairline",a)
    c=src.index("private static RectTransform DockButton")
    d=src.index("private static Sprite NeedCorona",c)
    excerpt=(src[a:b]+"\n"+src[c:d]).encode()
    assert 0<len(excerpt)<=12000
    (out/"source.txt").write_bytes(excerpt)
    packet={"schema":1,"target_id":"CORONA_HUD_COMMERCIAL_REVIEW","source_sha":SHA,"source_run":RUN,"mode":"review","routing_profile":"standard",
      "objective":"Spanish. Assess two authentic 390x844 WebGL mobile frames, Bastion I and V, and exact existing PlayerRibbon/Layout, resource, DockButton, ButtonPlate and Label source. Objective HUD 7.4 to 8.5/10. Identify highest-impact interventions for commercial readability, hierarchy, recognition of silhouettes, consistent dark medieval Corona material and responsive safe-area layout. Provide exact bounded source-level recommendations and reusable control semantics, prioritizing genuine visual impact over microadjustments. A candidate already protects resource icon/value gap (not yet visually verified); do not duplicate that correction.",
      "scope":"Read-only baseline review. Preserve approved Corona sprites, portraits, original buttons/navigation, callbacks, mechanics, state and art direction. No new functions or buttons. Whiteout Survival and Call of Dragons are quality references, not supplied images: do not pretend to inspect them. Frames are real browser captures from completed Unity WebGL build, not physical iPhone or Unity Editor; no claims about animation or performance from stills. Other secondary screens must be evaluated separately.",
      "acceptance":"Director: prioritize 2-3 visible high-impact changes with evidence. Programmer: specify exact existing symbols, reusable approach and regression tests; distinguish proposed from tested. Auditor: independently challenge director/programmer claims, identify risk to approved screens, no invented scores or technical PASS. Coordinator: actionable Spanish implementation sequence, A/B criteria and limitations. No approval of unimplemented work.",
      "source":{"path":"source.txt","sha256":sha(excerpt)},"images":images}
    (out/"packet.json").write_text(json.dumps(packet,ensure_ascii=False,indent=2),encoding="utf-8")
    print("CORONA_WEBGL_BASELINE_PACKET",sha(excerpt),flush=True)
if __name__=="__main__":main(*map(Path,sys.argv[1:]))
