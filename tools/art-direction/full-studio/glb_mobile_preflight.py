"""Native glTF 2.0 integrity + mobile-BUDGET preflight for real artist GLBs.

This is a source-level compatibility gate; it does NOT claim a native Unity
import, URP appearance, gameplay or mobile-device certification. Blender GLB
may be syntactically correct but artistically rejected.
"""
import argparse,json,pathlib,struct,hashlib

class InvalidGameAsset(ValueError):
    pass

def require(ok,message):
    if not ok:raise InvalidGameAsset(message)

def analyze_glb(file,triangle_budget=220000,material_required=False):
    path=pathlib.Path(file)
    raw=path.read_bytes()
    require(len(raw)>=20 and len(raw)%4==0,"GLB header alignment/length")
    magic,version,length=struct.unpack_from("<4sII",raw,0)
    require(magic==b"glTF" and version==2 and length==len(raw),
            "GLB 2.0 header incorrect")
    offset=12;chunks=[]
    while offset<len(raw):
        require(offset+8<=len(raw),"Truncated GLB chunk header")
        size,kind=struct.unpack_from("<I4s",raw,offset)
        require(size%4==0 and offset+8+size<=len(raw),"GLB chunk size invalid")
        chunks.append((kind,raw[offset+8:offset+8+size]))
        offset+=8+size
    require(chunks and chunks[0][0]==b"JSON","First GLB chunk must be JSON")
    doc=json.loads(chunks[0][1].decode("utf-8").rstrip(" \x00"))
    require(doc.get("asset",{}).get("version")=="2.0","GLB JSON asset version")
    require(len(chunks)>=2 and chunks[1][0]==b"BIN\x00", "GLB is missing embedded binary mesh buffers")
    buffers=doc.get("buffers",[])
    views=doc.get("bufferViews",[])
    require(len(buffers)>=1 and len(views)>=1,"Missing genuine binary accessors")
    require(buffers[0].get("byteLength",0)>0 and buffers[0]["byteLength"]<=len(chunks[1][1]),"Inconsistent GLB binary byte length")
    for view in views:
        require(type(view.get("buffer")) is int and 0<=view["buffer"]<len(buffers),
                "GLB bufferView refers to nonexistent binary buffer")
        require(type(view.get("byteLength")) is int and view["byteLength"]>0,"Empty GLB bufferView")
        require(view.get("byteOffset",0)+view["byteLength"]<=buffers[view["buffer"]].get("byteLength",0),
                "GLB bufferView byte range exceeds embedded binary payload")
    accessors=doc.get("accessors",[])
    meshes=doc.get("meshes",[])
    require(len(meshes)>=1,"GLB contains no authored mesh")
    materials=doc.get("materials",[])
    nodes=doc.get("nodes",[])
    triangles=0;mesh_primitives=0
    uv_primitives=0;normals_primitives=0;missing_material=0
    for mesh in meshes:
        for prim in mesh.get("primitives",[]):
            require(prim.get("mode",4)==4,"Only triangulated mesh primitives accepted")
            attrs=prim.get("attributes",{})
            pos=attrs.get("POSITION")
            require(type(pos) is int and 0<=pos<len(accessors),
                    "GLB primitive missing valid POSITION buffer")
            require(type(accessors[pos].get("bufferView")) is int and 0<=accessors[pos]["bufferView"]<len(views),"Missing real GLB positions buffer view")
            vertex_count=accessors[pos].get("count",0)
            require(type(vertex_count) is int and vertex_count>=3,
                    "GLB geometry lacks valid vertices")
            ix=prim.get("indices")
            if ix is not None:
                require(type(ix) is int and 0<=ix<len(accessors),
                        "Invalid GLB index accessor")
                require(type(accessors[ix].get("bufferView")) is int and 0<=accessors[ix]["bufferView"]<len(views),"Missing real GLB index data")
                count=accessors[ix].get("count",0)
            else:
                count=vertex_count
            require(count%3==0,"GLB contains a non-triangular primitive")
            triangles+=count//3
            mesh_primitives+=1
            uv_primitives+=int("TEXCOORD_0" in attrs)
            normals_primitives+=int("NORMAL" in attrs)
            mat=prim.get("material")
            if mat is None:missing_material+=1
            else:require(type(mat) is int and 0<=mat<len(materials),
                          "Broken PBR material reference")
    require(1<=triangles<=triangle_budget,"Real game-asset triangle budget exceeded")
    require(not any(str(n.get("name","")).startswith("_EldoriaReview")
                    for n in nodes),"Art studio preview objects leaked into mobile asset")
    if material_required:
        require(missing_material==0 and len(materials)>0,
                "Production candidate has missing authored PBR materials")
    result={"schema_version":1,"scope":"GLB_TECHNICAL_MOBILE_SOURCE_PREFLIGHT_ONLY",
        "file":path.name,"file_sha256":hashlib.sha256(raw).hexdigest(),
        "bytes":len(raw),"mesh_count":len(meshes),"primitive_count":mesh_primitives,
        "triangle_count":triangles,"total_materials":len(materials),
        "missing_materials":missing_material,"uv0_primitives":uv_primitives,
        "normal_primitives":normals_primitives,
        "all_primitives_have_normals":normals_primitives==mesh_primitives,
        "all_primitives_have_uv0":uv_primitives==mesh_primitives,
        "mobile_triangle_budget":triangle_budget,"under_triangle_budget":True,
        "unity_import_verified":False,"mobile_device_verified":False,
        "artistically_approved":False}
    return result

def main():
    p=argparse.ArgumentParser()
    p.add_argument("glb",type=pathlib.Path)
    p.add_argument("--out",type=pathlib.Path,required=True)
    p.add_argument("--require-materials",action="store_true")
    p.add_argument("--triangles",type=int,default=220000)
    args=p.parse_args()
    report=analyze_glb(args.glb,args.triangles,args.require_materials)
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_TRUE_GLB_SOURCE_PREFLIGHT",json.dumps(report))
if __name__=="__main__":main()
