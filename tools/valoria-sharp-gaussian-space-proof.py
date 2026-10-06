import json, os, shutil, requests, hashlib
from PIL import Image

OUT=os.environ.get("OUT_DIR","/tmp/sharp-proof")
os.makedirs(OUT,exist_ok=True)
cfg=json.load(open('pipeline/valoria-sharp-gaussian-fullframe-v1.json'))
authority=json.load(open(cfg['source_authority_manifest']))
src=cfg['input']
def sha(path): return hashlib.sha256(open(path,'rb').read()).hexdigest()
if sha(authority['authority_path']) != authority['authority_sha256']:
    raise RuntimeError('Canonical authority SHA mismatch')
os.makedirs(os.path.dirname(src),exist_ok=True)
# Use the exact committed clean PNG; re-encoding on another OS changes its byte SHA.
if not os.path.exists(src):
    Image.open(authority['authority_path']).convert('RGB').crop(authority['crop_box']).save(src)
if src != authority['clean_input_path'] or sha(src) != authority['clean_input_sha256']:
    raise RuntimeError('SHARP clean input is not derived from the current exact authority')
ground=cfg.get("parcel_ground_input")
if ground:
    if sha(ground["path"]) != ground["sha256"]: raise RuntimeError("Parcel ground source SHA mismatch")
    src=ground["path"]
im=Image.open(src).convert("RGB")
im.thumbnail((1280,1280),Image.LANCZOS)
inp=os.path.join(OUT,"canonical-target.png"); im.save(inp)
source_manifest=dict(authority)
source_manifest.update(source_kind="parcel_ground_derivative" if ground else "canonical_authority", parcel_ground_input=ground, submitted_input_sha256=sha(inp),submitted_dimensions=list(im.size),paid_credits=0)
open(os.path.join(OUT,'source-authority.json'),'w').write(json.dumps(source_manifest,indent=2)+'\n')

if cfg.get("inference_backend", "space") == "local":
    import platform, subprocess, sys, time
    from pathlib import Path
    import torch
    local=cfg["local_inference"]
    cache=Path(os.environ["SHARP_CHECKPOINT_CACHE"]);cache.mkdir(parents=True,exist_ok=True)
    checkpoint=cache/"sharp_2572gikvuh.pt"
    ledger=cache/"checkpoint-lineage.json"
    url=local["checkpoint_url"]
    if not checkpoint.exists():
        temporary=checkpoint.with_suffix(".download")
        print("Downloading official checkpoint:",url,flush=True)
        with requests.get(url,stream=True,timeout=(30,300)) as response:
            response.raise_for_status()
            with temporary.open("wb") as file:
                for chunk in response.iter_content(8*1024*1024):file.write(chunk)
        temporary.replace(checkpoint)
        ledger.write_text(json.dumps({"url":url,"sha256":sha(checkpoint)},indent=2))
    fingerprint=sha(checkpoint)
    cached=json.loads(ledger.read_text())
    if cached["url"]!=url or cached["sha256"]!=fingerprint:raise RuntimeError("Cached checkpoint fingerprint/lineage mismatch")
    if local.get("checkpoint_sha256") and local["checkpoint_sha256"]!=fingerprint:raise RuntimeError("Official checkpoint SHA mismatch")
    device="cuda" if torch.cuda.is_available() else "cpu"
    print("LOCAL_SHARP_DEVICE",device,"TORCH",torch.__version__,"CHECKPOINT_SHA256",fingerprint,flush=True)
    command=[str(Path(sys.executable).parent/("sharp.exe" if os.name=="nt" else "sharp")),"predict","-i",inp,"-o",str(Path(OUT)/"prediction"),"-c",str(checkpoint),"--device",device,"--no-render"]
    start=time.time();subprocess.run(command,check=True)
    plys=list((Path(OUT)/"prediction").glob("*.ply"))
    if len(plys)!=1:raise RuntimeError("Local SHARP did not produce exactly one PLY")
    target=Path(OUT)/"sharp-1.ply";shutil.copy2(plys[0],target)
    with target.open("rb") as file:
        header=file.read(2048)
    if b"format binary_little_endian 1.0" not in header or b"element vertex " not in header:raise RuntimeError("Invalid local SHARP PLY")
    source_manifest.update(inference_backend="local",model="Apple SHARP",checkpoint_url=url,checkpoint_sha256=fingerprint,official_source_commit=local["source_commit"],device=device,torch_version=torch.__version__,python_version=platform.python_version(),inference_seconds=round(time.time()-start,3),ply_sha256=sha(target),ply_file=target.name,render_video=False)
    open(os.path.join(OUT,"source-authority.json"),"w").write(json.dumps(source_manifest,indent=2)+"\n")
    print("LOCAL_SHARP_PLY_PASS",target,"seconds",source_manifest["inference_seconds"],flush=True)
    raise SystemExit(0)

# Explicit fallback only. Production defaults to the same official model locally.
from gradio_client import Client, handle_file
spaces=[{'id':'gagndeep/Apple-Sharp-Image-to-3D-View-Synthesis'}]
ids=[]
for s in spaces:
    sid=s.get("id") or s.get("name") or ""
    title=(s.get("cardData") or {}).get("title","") if isinstance(s,dict) else ""
    if "sharp" in sid.lower() or "sharp" in str(title).lower():
        ids.append(sid)
open(os.path.join(OUT,"spaces.json"),"w").write(json.dumps(spaces,indent=2,default=str))
print("SHARP_CANDIDATES",ids)
if not ids: raise RuntimeError("No gagndeep SHARP Space discovered")

last=None
for sid in ids:
    try:
        client=Client(sid)
        api=client.view_api(return_format="dict")
        open(os.path.join(OUT,"api.json"),"w").write(json.dumps({"space":sid,"api":api},indent=2,default=str))
        eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
        print("SPACE",sid,"ENDPOINTS",list(eps))
        candidates=[]
        for name,spec in eps.items():
            ps=spec.get("parameters",[])
            has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps)
            if has_img: candidates.append((name,spec))
        for name,spec in candidates:
            args=[]
            for p in spec.get("parameters",[]):
                comp=p.get("component"); pname=str(p.get("parameter_name","")).lower()
                if comp=="Image" or ("image" in pname and "num" not in pname): args.append(handle_file(inp))
                elif "seed" in pname: args.append(1234)
                elif p.get("parameter_has_default"): args.append(p.get("parameter_default"))
                elif comp=="Checkbox": args.append(False)
                elif comp in ("Slider","Number"): args.append(0)
                elif comp in ("Textbox",): args.append("")
                else: args.append(None)
            try:
                print("TRY",name,args)
                res=client.predict(*args,api_name=name)
                print("RESULT",repr(res))
                open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"space":sid,"endpoint":name,"result":res},indent=2,default=str))
                files=[]
                def walk(v):
                    if isinstance(v,str) and os.path.exists(v) and os.path.isfile(v): files.append(v)
                    elif isinstance(v,dict):
                        p=v.get("path") or v.get("name")
                        if isinstance(p,str) and os.path.exists(p): files.append(p)
                        for x in v.values(): walk(x)
                    elif isinstance(v,(list,tuple)):
                        for x in v: walk(x)
                walk(res)
                copied=0
                for p in files:
                    ext=os.path.splitext(p)[1].lower()
                    if ext in (".ply",".splat",".spz",".zip",".mp4",".webm",".glb",".obj",".png",".jpg",".jpeg"):
                        shutil.copy2(p,os.path.join(OUT,f"sharp-{copied}{ext}")); copied+=1
                if copied:
                    plys=[os.path.join(OUT,n) for n in os.listdir(OUT) if n.endswith('.ply')]
                    if not plys: raise RuntimeError('SHARP returned no PLY')
                    source_manifest.update(ply_sha256=sha(plys[0]),ply_file=os.path.basename(plys[0]),space=sid,endpoint=name)
                    open(os.path.join(OUT,'source-authority.json'),'w').write(json.dumps(source_manifest,indent=2)+'\n')
                    print("COPIED",copied)
                    raise SystemExit(0)
            except SystemExit: raise
            except Exception as e:
                last=repr(e); print("ENDPOINT_FAILED",name,last)
    except SystemExit: raise
    except Exception as e:
        last=repr(e); print("SPACE_FAILED",sid,last)
raise RuntimeError("No SHARP output produced; last="+str(last))
