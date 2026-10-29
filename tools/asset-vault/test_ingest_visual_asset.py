import importlib.util,pathlib,tempfile,unittest
s=pathlib.Path(__file__).with_name("ingest_visual_asset.py");spec=importlib.util.spec_from_file_location("ingest_visual_asset",s);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
class TestVault(unittest.TestCase):
 def test_copy_dedup_and_source_preserved(self):
  with tempfile.TemporaryDirectory() as d:
   root=pathlib.Path(d);src=root/"source.glb";src.write_bytes(b"original")
   vault=root/"vault";first=m.ingest(src,vault,"aldric","model3d");second=m.ingest(src,vault,"aldric","model3d")
   self.assertEqual(first["sha256"],second["sha256"])
   self.assertEqual((vault/first["vault_path"]).read_bytes(),b"original")
   self.assertEqual(src.read_bytes(),b"original")
 def test_reject_bad_id(self):
  with tempfile.TemporaryDirectory() as d:
   root=pathlib.Path(d);src=root/"a.png";src.write_bytes(b"x")
   with self.assertRaises(ValueError):m.ingest(src,root/"vault","../escape","image2d")
if __name__=="__main__":unittest.main()
