#!/usr/bin/env python3
"""Read-only binary integrity/structural check for 26 shortlisted canonical GLBs.

Checks GLB header, glTF JSON, primitives and exact previously recorded SHA-256.
No Unity, no Blender, no license or artistic/animation approval.
"""
import argparse,hashlib,json,struct
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
SHELF=ROOT/"pipeline/asset-vault/clean-production-shelf-v1.json"
GLB_MAGIC=b"glTF"
JSON_CHUNK=0x4E4F534A

def audit(path):
    sha=hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda:f.read(1024*1024),b""):
            sha.update(block)
    with path.open("rb") as f:
        header=f.read(12)
        if len(header)!=12 or header[:4]!=GLB_MAGIC:raise ValueError("not an actual GLB")
        magic,version,total=struct.unpack("<4sII",header)
        if version!=2 or total!=path.stat().st_size:raise ValueError("GLB v2 length mismatch")
        chunk_header=f.read(8)
        if len(chunk_header)!=8:raise ValueError("GLB JSON chunk header missing")
        length,kind=struct.unpack("<II",chunk_header)
        if kind!=JSON_CHUNK or length>25_000_000:raise ValueError("invalid GLB JSON chunk")
        manifest=json.loads(f.read(length).decode("utf-8"))
    nodes=manifest.get("nodes",[])
    meshes=manifest.get("meshes",[])
    accessors=manifest.get("accessors",[])
    prims=[p for m in meshes for p in m.get("primitives",[])]
    if not meshes or not prims:raise ValueError("no draw primitives")
    missing_positions=sum("POSITION" not in p.get("attributes",{}) for p in prims)
    if missing_positions:raise ValueError("missing POSITION in "+str(missing_positions)+" primitives")
    triangles=0
    for p in prims:
        mode=p.get("mode",4)
        if mode!=4:continue
        accessor=p.get("indices")
        if accessor is not None and accessor<len(accessors):
            triangles+=accessors[accessor].get("count",0)//3
        else:
            x=p["attributes"]["POSITION"]
            if x<len(accessors):triangles+=accessors[x].get("count",0)//3
    if triangles<1:raise ValueError("zero triangle geometry")
    return dict(sha256=sha.hexdigest(),file_bytes=path.stat().st_size,
                nodes=len(nodes),meshes=len(meshes),primitives=len(prims),
                triangles=triangles,materials=len(manifest.get("materials",[])),
                images=len(manifest.get("images",[])),
                animations=len(manifest.get("animations",[])),
                skins=len(manifest.get("skins",[])))

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--out",required=True,type=Path)
    p.add_argument("--strict",action="store_true")
    args=p.parse_args()
    doc=json.loads(SHELF.read_text(encoding="utf-8-sig"))
    seen=set();reports=[]
    for group in doc["groups"]:
        for item in group["items"]:
            path=item["path"]
            if path in seen:raise ValueError("Duplicated canonical file in clean shelf "+path)
            seen.add(path)
            result={"name":item["name"],"path":path,"shelf":group["id"],"status":"CHECK_PENDING",
                    "art_approved_by_this_audit":False,"license_certified_by_this_audit":False}
            try:
                raw=ROOT/path
                data=audit(raw)
                result.update(data)
                if not Path(str(raw)+".meta").is_file():raise ValueError("Unity .meta missing")
                if data["sha256"] != item["recorded_sha256"]:raise ValueError("SHA256 mismatch: file changed relative to certified inventory")
                result["status"]="STRUCTURAL_SOURCE_VERIFIED"
            except (ValueError,OSError,KeyError,UnicodeError) as error:
                result["status"]="FAIL";result["failure"]=str(error)
            reports.append(result)
            print(result["status"],item["name"],result.get("triangles",0))
    status={"schema_version":1,"scope":"canonical_glb_binary_and_structure_only",
            "shelf":doc["title"],"checked":len(reports),
            "valid":sum(r["status"]=="STRUCTURAL_SOURCE_VERIFIED" for r in reports),
            "failed":sum(r["status"]=="FAIL" for r in reports),
            "no_artistic_or_license_approval":True,"entries":reports}
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps(status,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if args.strict and status["failed"]:raise SystemExit(2)
    if status["checked"]!=26:raise SystemExit("Shelf must account for 26 canonical GLBs")
if __name__=="__main__":main()
