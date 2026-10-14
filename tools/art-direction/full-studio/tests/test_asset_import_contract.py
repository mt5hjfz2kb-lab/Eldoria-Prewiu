"""Test reuse decisions against exact filenames, immutable SHA256 and budget."""
import sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from contract import validate_plan,RejectedPlan

def plan(path="approved/watchtower.glb",sha="a"*64):
 return {"schema_version":2,"asset_id":"reuse-qualified-medieval-architecture",
  "observed_defect":"The game needs its verified original medieval tower mesh in a new asset candidate.",
  "design_intent":"Reuse the locally approved original source and preserve its recognisable authored details.",
  "asset_decision":"REUSE","expected_visual_difference":"The full imported modular stone tower appears as complete editable meshes.",
  "mobile_risks":"Check actual imported triangle count and evaluate LOD before integrating.",
  "actions":[{"id":"reuse-tower","op":"import_asset","target":"Source Approved Tower",
             "reason":"Reuse the existing licensed original model rather than regenerating geometry.",
             "params":{"relative_path":path,"sha256":sha,
                       "location":[0,0,0],"rotation":[0,0,0],"scale":[1,1,1]}}]}

class ImportContractTests(unittest.TestCase):
 def test_approved_shape(self):self.assertEqual(validate_plan(plan())["geometric_actions"],1)
 def test_path_escape(self):
  with self.assertRaises(RejectedPlan):validate_plan(plan("../../private.glb"))
 def test_absolute_path(self):
  with self.assertRaises(RejectedPlan):validate_plan(plan("/etc/passwd"))
 def test_invalid_file_type(self):
  with self.assertRaises(RejectedPlan):validate_plan(plan("asset/evil.py"))
 def test_sha_missing(self):
  with self.assertRaises(RejectedPlan):validate_plan(plan(sha="0"*6))
 def test_transform_not_finite(self):
  p=plan();p["actions"][0]["params"]["scale"]=[float("inf"),1,1]
  with self.assertRaises(RejectedPlan):validate_plan(p)
if __name__=="__main__":unittest.main()
