"""Model chooses the geometry; executor rejects fake/degenerated originals."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from contract import validate_plan,RejectedPlan
def make(vertices,faces):
 return {"schema_version":2,"asset_id":"Authored tower extension",
 "observed_defect":"The inherited tower lacks sufficiently articulated details",
 "design_intent":"A genuinely original architectural subcomponent with depth",
 "asset_decision":"MODIFY","expected_visual_difference":"A clearly visible masonry extension to the existing tower",
 "mobile_risks":"Mesh density remains bounded",
 "actions":[{"id":"new_component","op":"mesh","target":"Artist_Arch_Original",
 "reason":"Create an independently authored attached component with real faces",
 "params":{"vertices":vertices,"faces":faces}}]}
class NewMeshIntegrity(unittest.TestCase):
 def test_unique_noncollinear_original_vertices_allowed(self):
  p=make([[0,0,0],[1,0,0],[0,1,0],[0,0,1]],[[0,1,2],[0,1,3]])
  prior=copy.deepcopy(p);validate_plan(p);self.assertEqual(p,prior)
 def test_repeated_vertices_rejected_not_repaired(self):
  p=make([[0,0,0],[0,0,0],[0,0,0]],[[0,1,2]])
  before=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"DUPLICATE 3D"):validate_plan(p)
  self.assertEqual(p,before)
 def test_collinear_nonduplicate_vertices_rejected(self):
  p=make([[0,0,0],[1,0,0],[2,0,0]],[[0,1,2]])
  with self.assertRaisesRegex(RejectedPlan,"ZERO-AREA"):validate_plan(p)
 def test_existing_primitive_authoring_still_valid(self):
  p=make([[0,0,0],[1,0,0],[0,1,0]],[[0,1,2]])
  p["actions"][0]["op"]="primitive"
  p["actions"][0]["params"]={"shape":"cube","location":[0,1,0],
                              "rotation":[0,0,0],"scale":[0.4,0.3,1]}
  validate_plan(p)
if __name__=="__main__":unittest.main()
