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
 def test_missing_lfs_and_external_symlink_are_not_backups(self):
  with tempfile.TemporaryDirectory() as d, tempfile.TemporaryDirectory() as outside:
   root=pathlib.Path(d)
   subprocess.run(["git","init","-q",str(root)],check=True)
   (root/"missing.glb").write_bytes(b"source")
   (root/"pointer.glb").write_text("version https://git-lfs.github.com/spec/v1\n"+"oid sha256:"+"a"*64+"\nsize 9000\n")
   external=pathlib.Path(outside)/"private.png";external.write_bytes(b"must not be read")
   (root/"link.png").symlink_to(external)
   subprocess.run(["git","-C",str(root),"add","."],check=True)
   (root/"missing.glb").unlink()
   rows={r["path"]:r for r in module.scan(root)}
   self.assertEqual(rows["missing.glb"]["availability"],"MISSING_WORKTREE")
   self.assertEqual(rows["pointer.glb"]["availability"],"LFS_POINTER_ONLY")
   self.assertEqual(rows["link.png"]["availability"],"SYMLINK_NOT_FOLLOWED")
   self.assertIsNone(rows["link.png"]["sha256"])
 def test_guid_usage_does_not_imply_approval(self):
  with tempfile.TemporaryDirectory() as d:
   root=pathlib.Path(d);subprocess.run(["git","init","-q",str(root)],check=True)
   (root/"model.glb").write_bytes(b"model")
   (root/"model.glb.meta").write_text("guid: "+"1"*32)
   (root/"scene.unity").write_text("m_SourcePrefab: {guid: "+"1"*32+"}")
   subprocess.run(["git","-C",str(root),"add","."],check=True)
   row=list(module.scan(root))[0]
   self.assertEqual(row["unity_references"],["scene.unity"])
   self.assertEqual(row["approval"],"UNREVIEWED")
if __name__=="__main__":unittest.main()
