import hashlib,importlib.util,json,pathlib,tempfile,unittest
script=pathlib.Path(__file__).with_name("verify_visual_backup.py")
spec=importlib.util.spec_from_file_location("verify_visual_backup",script)
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
class TestBackup(unittest.TestCase):
 def test_hash_and_missing_and_traversal(self):
  with tempfile.TemporaryDirectory() as t:
   root=pathlib.Path(t);(root/"model.glb").write_bytes(b"model")
   digest=hashlib.sha256(b"model").hexdigest()
   self.assertEqual(m.verify(root,{"assets":[{"path":"model.glb","sha256":digest}]}),[])
   self.assertTrue(m.verify(root,{"assets":[{"path":"model.glb","sha256":"bad"}]}))
   self.assertTrue(m.verify(root,{"assets":[{"path":"missing.glb","sha256":digest}]}))
   self.assertTrue(m.verify(root,{"assets":[{"path":"../outside.glb","sha256":digest}]}))
if __name__=="__main__":unittest.main()
