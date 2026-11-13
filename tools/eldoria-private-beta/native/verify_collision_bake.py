"""Verify real WebGL collision data and unchanged Bastion mesh bytes; read-only."""
import argparse, gzip, hashlib, json, pathlib, struct
import UnityPy

EXPECTED = {
 "Bastion_I": "5fa376ac44869b90935ae3f6b63652c3671a815f0a695e43ac731111e6913b1b",
 "Bastion_II_GI": "30e4ec63d249c9657fa7dfb01b6cb71c937a8794b2956af64efd7e9a96f79605",
}
def sha(data): return hashlib.sha256(data).hexdigest()
def verify(root, output):
 root=pathlib.Path(root)
 candidates=sorted({p for pattern in ("*.data.gz","*.data","*.data.unityweb")
                    for p in root.rglob(pattern)})
 assert len(candidates)==1,("Expected exactly one Unity data payload",candidates)
 payload=candidates[0].read_bytes()
 if payload[:2]==b"\x1f\x8b": payload=gzip.decompress(payload)
 assert payload[:16]==b"UnityWebData1.0\0"
 end=struct.unpack_from("<I",payload,16)[0];pos=20;bundle=None
 while pos<end:
  offset,size,n=struct.unpack_from("<III",payload,pos);pos+=12
  name=payload[pos:pos+n].decode();pos+=n
  assert offset+size<=len(payload)
  if name=="data.unity3d":
   assert bundle is None
   bundle=payload[offset:offset+size]
 assert bundle is not None
 payload_hash=sha(payload);bundle_hash=sha(bundle);del payload
 env=UnityPy.load(bundle);del bundle
 rows={}
 for obj in env.objects:
  if obj.type.name!="Mesh":continue
  tree=obj.read_typetree();name=tree["m_Name"]
  if name not in EXPECTED:continue
  assert name not in rows,("Ambiguous target mesh",name)
  triangle_bytes=len(tree["m_BakedTriangleCollisionMesh"])
  assert triangle_bytes>0,("Static mesh still requires runtime cooking",name)
  assert tree["m_CookingOptions"]==30,("Changed cooking options",name)
  # In-memory serialization only. Never save or mutate the build.
  # Remove baked acceleration data and prove EVERY remaining byte is original.
  tree["m_BakedTriangleCollisionMesh"]=[]
  tree["m_BakedConvexCollisionMesh"]=[]
  obj.save_typetree(tree)
  normalized_hash=sha(obj.data)
  assert normalized_hash==EXPECTED[name],("Mesh changed beyond cooked collision data",name,normalized_hash)
  rows[name]={"baked_triangle_bytes":triangle_bytes,"non_collision_bytes_sha256":normalized_hash}
 assert set(rows)==set(EXPECTED),("Missing Bastion meshes",rows.keys())
 result={"data_sha256":payload_hash,"bundle_sha256":bundle_hash,
  "meshes":rows,"status":"REAL_WEBGL_COLLISIONS_PREBAKED_ORIGINAL_MESH_BYTES_VERIFIED",
  "physical_iphone_verified":False,"external_testers_enabled":False}
 pathlib.Path(output).write_text(json.dumps(result,indent=2)+"\n")
 print(json.dumps(result,indent=2))
if __name__=="__main__":
 p=argparse.ArgumentParser();p.add_argument("web_root");p.add_argument("output")
 a=p.parse_args();verify(a.web_root,a.output)
