"""Negative/positive contract tests run without Blender or paid models."""
import json, sys, unittest
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT))
from contract import validate_plan, RejectedPlan

def good():
    return {"schema_version":2,"asset_id":"sample-masonry-tower",
            "observed_defect":"The scene lacks a convincing medieval defensive tower silhouette.",
            "design_intent":"Model a reusable stone tower with a separate roof and authored details.",
            "asset_decision":"NEW","expected_visual_difference":
            "A clearly visible and grounded multi-part fortified tower silhouette.",
            "mobile_risks":"Keep triangles and alpha overdraw within mobile budget.",
            "actions":[{"id":"tower_01","op":"primitive","target":"Tower stone shaft",
                        "reason":"A tapered defensive vertical mass is essential",
                        "params":{"shape":"cylinder","location":[0,0,2],
                                  "rotation":[0,0,0],"scale":[1.2,1.2,2]}}]}

class ContractTests(unittest.TestCase):
    def test_valid(self):self.assertEqual(validate_plan(good())["geometric_actions"],1)
    def test_reject_code_execution(self):
        p=good();p["actions"][0]["op"]="execute_python"
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_missing_authoring(self):
        p=good();p["actions"][0]={"id":"m","op":"material","target":"Existing","reason":"Needs a stronger stone PBR material surface","params":{"base_color":[.5,.5,.5,1],"roughness":.7}}
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_self_approval(self):
        p=good();p["art_approved"]=True
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_wrong_vertices(self):
        p=good();p["actions"][0]={"id":"m","op":"mesh","target":"bad","reason":"Shape has fictional faces and cannot export","params":{"vertices":[[0,0,0],[1,0,0],[0,1,0]],"faces":[[0,1,200]]}}
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_absolute_texture(self):
        p=good();p["actions"].append({"id":"mat","op":"material","target":"Tower stone shaft","reason":"Texture must stay in approved source tree","params":{"base_color":[.5,.5,.5,1],"roughness":.8,"texture_path":"/etc/passwd"}})
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_path_traversal(self):
        p=good();p["actions"].append({"id":"mat","op":"material","target":"Tower stone shaft","reason":"Texture must not leave approved source tree","params":{"base_color":[.5,.5,.5,1],"roughness":.8,"texture_path":"../../x.jpg"}})
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_zero_sculpt(self):
        p=good();p["actions"].append({"id":"sc","op":"sculpt","target":"Tower stone shaft","reason":"Sculpting must move real vertices not a zero vector","params":{"center":[0,0,0],"delta":[0,0,0],"radius":2}})
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reject_excessive_mesh_budget(self):
        p=good();p["actions"]*=161
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_distinct_operation_names(self):
        p=good();p["actions"].append(p["actions"][0])
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_schema_reject_non_numeric_color(self):
        p=good();p["actions"].append({"id":"mat","op":"material","target":"Tower stone shaft","reason":"Surface color must be numeric and bounded","params":{"base_color":[1,2,3,4],"roughness":.6}})
        with self.assertRaises(RejectedPlan):validate_plan(p)
    def test_reuse_audit_requires_geometry(self):
        p=good();p["asset_decision"]="REUSE"
        self.assertGreater(validate_plan(p)["geometric_actions"],0)
if __name__=="__main__":unittest.main()
