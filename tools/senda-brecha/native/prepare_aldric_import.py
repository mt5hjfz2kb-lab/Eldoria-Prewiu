"""Assemble the exact, already validated Aldric normal-import derivative.
No source bytes or other glTF semantics changed. Input resources are pinned."""
import argparse,copy,hashlib,json,struct
from pathlib import Path
HERO="c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b"
SCENE="5a9e6a66e1113e2c40cba8fa385484cb1822b253a41cd45adb4b33061e6cff34"
NORMAL="5c1626f7951fb45a0c36a8a45ecf0be4b3406afe0ba13ad4ca30ead0991b9833"
RESULT="d64f765625340365ad760e7e8d0639d640ebdd828d55c0635b3aefc0c974081b"
def sha(x):return hashlib.sha256(x).hexdigest()
def prepare(source,scene,normals):
    assert sha(source)==HERO and sha(scene)==SCENE and sha(normals)==NORMAL
    assert struct.unpack_from("<III",source)==(0x46546c67,2,len(source))
    n,typ=struct.unpack_from("<II",source,12);assert typ==0x4e4f534a
    size,kind=struct.unpack_from("<II",source,20+n);assert kind==0x004e4942
    original=json.loads(source[20:20+n]);binary=source[28+n:]
    assert len(binary)==size
    compatible=json.loads(scene);restored=copy.deepcopy(compatible)
    count=0
    for m in restored["meshes"]:
        for p in m["primitives"]:
            normal=p["attributes"].pop("NORMAL")
            assert normal>=len(original["accessors"]);count+=1
    assert count==10
    restored["accessors"]=restored["accessors"][:len(original["accessors"])]
    restored["bufferViews"]=restored["bufferViews"][:len(original["bufferViews"])]
    restored["buffers"]=original["buffers"]
    assert restored==original
    assert len(compatible["skins"][0]["joints"])==26 and len(compatible["animations"])==5
    padded=scene+b" "*(-len(scene)%4);combined=binary+normals
    out=struct.pack("<III",0x46546c67,2,28+len(padded)+len(combined))+struct.pack("<II",len(padded),0x4e4f534a)+padded+struct.pack("<II",len(combined),0x004e4942)+combined
    assert sha(out)==RESULT
    return out,{"source_sha256":sha(source),"derived_sha256":sha(out),"unchanged_binary_prefix":True,"only_added_normals":True,"skin_joints":26,"animation_clips":5,"meshes":count}
if __name__=="__main__":
    p=argparse.ArgumentParser()
    for s in ("source","scene","normals","out"):p.add_argument("--"+s,required=True,type=Path)
    a=p.parse_args();out,proof=prepare(a.source.read_bytes(),a.scene.read_bytes(),a.normals.read_bytes())
    a.out.mkdir(parents=True,exist_ok=False)
    (a.out/"SirAldric_Approved_RiggedAnimated_v1.glb").write_bytes(out)
    (a.out/"normal-import-proof.json").write_text(json.dumps(proof,indent=2))
    print("SENDA_CANONICAL_NORMAL_IMPORT_BYTES_PASS",json.dumps(proof))
