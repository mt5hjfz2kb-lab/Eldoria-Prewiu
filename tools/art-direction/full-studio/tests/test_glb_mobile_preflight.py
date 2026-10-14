"""GLB native binary integrity regression tests; never pretend to certify Unity."""
import pathlib,sys,struct,json,tempfile,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from glb_mobile_preflight import analyze_glb,InvalidGameAsset

def glb(mesh=True, materials=True, preview=False):
    doc={"asset":{"version":"2.0"},
         "buffers":[{"byteLength":60}],
         "bufferViews":[{"buffer":0,"byteOffset":0,"byteLength":36},
                        {"buffer":0,"byteOffset":36,"byteLength":24}],
         "accessors":[{"bufferView":0,"componentType":5126,"count":3,"type":"VEC3"},
                      {"bufferView":1,"componentType":5123,"count":3,"type":"SCALAR"}],
         "nodes":[{"name":"_EldoriaReviewFloor" if preview else "Stone tower"}]}
    doc["meshes"]=[{"primitives":[{"attributes":{"POSITION":0,"NORMAL":0,"TEXCOORD_0":0},
                                    "indices":1,**({"material":0} if materials else {})}]}] if mesh else []
    doc["materials"]=[{"pbrMetallicRoughness":{"baseColorFactor":[.5,.5,.5,1]}}] if materials else []
    raw=json.dumps(doc,separators=(",",":")).encode()
    raw+=b" " * ((-len(raw))%4)
    bin_chunk=b"\x00"*60
    content=struct.pack("<I4s",len(raw),b"JSON")+raw+struct.pack("<I4s",len(bin_chunk),b"BIN\x00")+bin_chunk
    return struct.pack("<4sII",b"glTF",2,12+len(content))+content

class GLBInspectorTests(unittest.TestCase):
    def test_owned_mesh_has_actual_binary_and_pbr(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"model.glb";p.write_bytes(glb())
            r=analyze_glb(p,material_required=True)
            self.assertEqual(r["triangle_count"],1)
            self.assertTrue(r["all_primitives_have_normals"])
            self.assertFalse(r["unity_import_verified"])
            self.assertFalse(r["artistically_approved"])
    def test_rejects_missing_model(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"empty.glb";p.write_bytes(glb(mesh=False))
            with self.assertRaises(InvalidGameAsset):analyze_glb(p)
    def test_rejects_unlicensed_looking_preview_geometry_export(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"preview.glb";p.write_bytes(glb(preview=True))
            with self.assertRaises(InvalidGameAsset):analyze_glb(p)
    def test_rejects_missing_art_materials_when_required(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"naked.glb";p.write_bytes(glb(materials=False))
            with self.assertRaises(InvalidGameAsset):analyze_glb(p,material_required=True)
    def test_rejects_mismatched_actual_binary(self):
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"broken.glb";p.write_bytes(glb()[:-4])
            with self.assertRaises(InvalidGameAsset):analyze_glb(p)
if __name__=="__main__":unittest.main()
