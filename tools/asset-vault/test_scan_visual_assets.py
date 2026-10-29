import importlib.util, pathlib, subprocess, tempfile, unittest
SCRIPT=pathlib.Path(__file__).with_name("scan_visual_assets.py")
spec=importlib.util.spec_from_file_location("scan_visual_assets",SCRIPT)
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
class TestInventory(unittest.TestCase):
 def test_read_only_and_classification(self):
  with tempfile.TemporaryDirectory() as d:
   root=pathlib.Path(d)
   subprocess.run(["git","init","-q",str(root)],check=True)
   (root/"model.glb").write_bytes(b"test model")
   (root/"image.png").write_bytes(b"test image")
   (root/"ignore.txt").write_text("ignore")
   subprocess.run(["git","-C",str(root),"add","."],check=True)
   before={p.name:p.read_bytes() for p in root.iterdir() if p.is_file()}
   rows=list(module.scan(root))
   after={p.name:p.read_bytes() for p in root.iterdir() if p.is_file()}
   self.assertEqual(before,after)
   self.assertEqual({r["kind"] for r in rows},{"model3d","image2d"})
   self.assertTrue(all(r["approval"]=="UNREVIEWED" for r in rows))
if __name__=="__main__":unittest.main()
