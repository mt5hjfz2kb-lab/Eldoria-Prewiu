"""Package a fresh, natively certified structural candidate with the existing beta shell.
The engine comes exclusively from the fresh build. Never mixes old WASM with new data.
No publication and no claim of browser/mobile certification.
"""
import argparse, gzip, hashlib, importlib.util, json, shutil, struct, re
from pathlib import Path

def sha(b): return hashlib.sha256(b).hexdigest()
def only(build, pattern):
    matches=list((build/"Build").glob(pattern))
    assert len(matches)==1, (pattern, matches)
    return matches[0]
def decoded(p):
    b=p.read_bytes()
    assert b[:2]==b"\x1f\x8b", "Expected canonical gzip WebGL output"
    return gzip.decompress(b)
def extract(build, out):
    out.mkdir(parents=True,exist_ok=False)
    raw=decoded(only(build,"*.data.unityweb"))
    assert raw[:16]==b"UnityWebData1.0\0"
    end=struct.unpack_from("<I",raw,16)[0];pos=20;found=[]
    while pos<end:
        offset,size,n=struct.unpack_from("<III",raw,pos);pos+=12
        name=raw[pos:pos+n];pos+=n
        assert offset>=end and offset+size<=len(raw)
        if name==b"data.unity3d":found.append(raw[offset:offset+size])
    assert pos==end and len(found)==1
    (out/"data.bin").write_bytes(raw)
    (out/"data.unity3d").write_bytes(found[0])
    proof={"data_sha256":sha(raw),"bundle_sha256":sha(found[0]),"bytes":len(raw)}
    (out/"extraction.json").write_text(json.dumps(proof,indent=2))
    print(json.dumps(proof))
def package(a):
    build,wrapper,out=map(Path,(a.build,a.wrapper,a.output))
    native=json.loads(Path(a.native_proof).read_text())
    web=json.loads(Path(a.web_proof).read_text())
    texture=json.loads(Path(a.texture_proof).read_text())
    assert native["status"]=="NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION"
    assert web["status"]=="WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED"
    assert web["source_sha"]==a.overlay_commit
    assert texture["candidate_sha256"]==sha(Path(a.bundle).read_bytes())
    audits={row[mode]["inputAudit"] for row in native["passes"] for mode in ("baseline","staged")}
    assert len(audits)==1 and re.fullmatch(r"[A-Fa-f0-9]{64}",next(iter(audits))), "Audited native input provenance missing"
    # Bind the transformed input to the actual freshly built data bundle.
    data=Path(a.data).read_bytes()
    assert data==decoded(only(build,"*.data.unityweb")), "Data/engine build mismatch"
    pos=20;end=struct.unpack_from("<I",data,16)[0];hashes=[]
    while pos<end:
        offset,size,n=struct.unpack_from("<III",data,pos);pos+=12
        name=data[pos:pos+n];pos+=n
        if name==b"data.unity3d":hashes.append(sha(data[offset:offset+size]))
    assert hashes==[texture["source_sha256"]], "Texture transform input mismatch"
    del data
    out.mkdir(parents=True,exist_ok=False)
    for name in ("index.html","style.css","city.png","play.js"):
        shutil.copyfile(wrapper/name,out/name)
    script=(out/"play.js").read_text()
    script,count=re.subn(r'window\.__eldoriaDiagnosticRevision="[^"]*";', "window.__eldoriaDiagnosticRevision="+json.dumps(a.revision)+";", script, count=1)
    assert count==1, "Expected existing local diagnostic revision marker"
    label=re.search(r"mobile(\d+)",a.revision)
    assert label, "Mobile candidate revision missing"
    script,count=re.subn(r'button\.textContent\s*=\s*"Diagnóstico[^"]*";', 'button.textContent = "Diagnóstico · beta '+label.group(1)+'";',script,count=1)
    assert count==1, "Expected one diagnostic version label"
    (out/"play.js").write_text(script)
    script_name="play-"+sha(script.encode())[:16]+".js"
    (out/script_name).write_text(script)
    html=(out/"index.html").read_text()
    html,count=re.subn(r'src="play(?:-[a-f0-9]+)?\.js(?:\?[^"]*)?"', 'src="'+script_name+'"',html)
    assert count==1, "Expected one beta startup script in real wrapper"
    (out/"index.html").write_text(html)
    for directory in ("StreamingAssets","TemplateData"):
        if (build/directory).is_dir():shutil.copytree(build/directory,out/directory)
    # Preserve only a shell icon absent from the native minimal template.
    icon=wrapper/"TemplateData/webmemd-icon.png"
    if icon.exists() and not (out/"TemplateData/webmemd-icon.png").exists():
        (out/"TemplateData").mkdir(exist_ok=True);shutil.copyfile(icon,out/"TemplateData/webmemd-icon.png")
    (out/"Build").mkdir(exist_ok=True)
    m={"revision":a.revision,"chunks":{},"decoded_sizes":{},"decoded_parts":{},"delivery_adapter":"github-pages-exact-gzip-structural-v1"}
    proofs=[]
    for key,pattern in (("loader","*.loader.js"),("framework","*.framework.js.unityweb"),("code","*.wasm.unityweb")):
        p=only(build,pattern);b=p.read_bytes();path="Build/"+p.name
        (out/path).write_bytes(b);m[key]=path+"?v="+sha(b)[:16]
        if key=="loader":proofs.append({"path":path,"sha256":sha(b),"bytes":len(b)});continue
        raw=gzip.decompress(b)
        part={"url":path,"encoding":"gzip","decoded_bytes":len(raw),"decoded_sha256":sha(raw)}
        m["decoded_sizes"][m[key]]=len(raw);m["decoded_parts"][m[key]]=[part];proofs.append(part)
    spec=importlib.util.spec_from_file_location("mobile_site",Path(__file__).with_name("mobile_site.py"))
    mobile=importlib.util.module_from_spec(spec);spec.loader.exec_module(mobile)
    packed=out/"packed-data.bin";mobile.pack_data(a.data,a.bundle,packed)
    m["data"]="Build/"+only(build,"*.data.unityweb").name+"?revision="+a.revision
    m["decoded_sizes"][m["data"]]=packed.stat().st_size
    parts=[];(out/"payload-decoded").mkdir()
    with packed.open("rb") as f:
        while block:=f.read(16*1024*1024):
            path="payload-decoded/"+a.revision+"-"+str(len(parts))+".gz"
            (out/path).write_bytes(gzip.compress(block,compresslevel=6,mtime=0))
            parts.append({"url":path,"encoding":"gzip","decoded_bytes":len(block),"decoded_sha256":sha(block)})
    packed.unlink();m["decoded_parts"][m["data"]]=parts
    (out/"build-manifest.json").write_text(json.dumps(m,indent=2))
    identity={"revision":a.revision,"source_sha":a.overlay_commit,"frozen_base_sha":"948ffa9837168a0b68dab80be76b66350422b23a",
              "source_composition":"frozen code plus hash-pinned existing generated assets and normalized metadata plus six startup/query C# overlays and the natively certified original terrain response tape",
              "existing_input_audit_sha256":next(iter(audits)),
              "game_bytes_unchanged":False,"gameplay_code_unchanged":False,"tutorial_unchanged":True,"clock_unchanged":True,
              "native_parity_verified":True,"native_journey_verified":True,"native_run":a.native_run,
              "texture_payload_modified":True,"candidate_bundle_sha256":texture["candidate_sha256"],
              "decoded_files":proofs+parts,"physical_mobile_tested":False,"runtime_verified":False,
              "release_for_external_testers":False,"telemetry_gameplay_connected":False,"candidate":True}
    (out/"release-identity.json").write_text(json.dumps(identity,indent=2))
    print(json.dumps({"revision":a.revision,"files":sum(p.is_file() for p in out.rglob("*")),"published":False}))
if __name__=="__main__":
    p=argparse.ArgumentParser();sub=p.add_subparsers(dest="command",required=True)
    x=sub.add_parser("extract");x.add_argument("--build",required=True);x.add_argument("--output",required=True)
    x=sub.add_parser("package")
    for name in ("build","wrapper","output","data","bundle","texture-proof","native-proof","web-proof","overlay-commit","revision","native-run"):
        x.add_argument("--"+name,required=True)
    a=p.parse_args()
    if a.command=="extract":extract(Path(a.build),Path(a.output))
    else:package(a)
