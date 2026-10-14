"""Consumer adapters never bypass source/art or Unity approval."""
import json,pathlib,sys,tempfile,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from consumer_handoff import build_handoff,NotReadyForHandoff

class HandoffTests(unittest.TestCase):
 def test_missing_inference_cannot_promote(self):
  with tempfile.TemporaryDirectory() as d:
   path=pathlib.Path(d);ref=path/"ref.jpg";ref.write_bytes(b"owner-reference")
   with self.assertRaises(NotReadyForHandoff):
    build_handoff(path,ref,"bastion1")
 def test_fabricated_art_approval_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   path=pathlib.Path(d);ref=path/"ref.jpg";ref.write_bytes(b"owner-reference")
   (path/"full-studio-result.json").write_text(json.dumps({
    "art_approved":True,"blender_real_execution":True,"cycles":[{"status":"ART_REVIEW_REQUIRED"}]}))
   with self.assertRaisesRegex(NotReadyForHandoff,"never self-approve"):
    build_handoff(path,ref,"world4x")
 def test_missing_real_qwen_output_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   path=pathlib.Path(d);ref=path/"ref.jpg";ref.write_bytes(b"owner-reference")
   (path/"full-studio-result.json").write_text(json.dumps({
    "art_approved":False,"blender_real_execution":True,"cycles":[{"status":"ART_REVIEW_REQUIRED"}]}))
   with self.assertRaisesRegex(NotReadyForHandoff,"Missing required"):
    build_handoff(path,ref,"asset-library")
 def test_unknown_consumer_denied(self):
  with tempfile.TemporaryDirectory() as d:
   path=pathlib.Path(d);ref=path/"ref.jpg";ref.write_bytes(b"owner-reference")
   with self.assertRaisesRegex(NotReadyForHandoff,"Unknown"):
    build_handoff(path,ref,"random-company")
if __name__=="__main__":unittest.main()
