import copy,json,pathlib,unittest
from joinery_contract import validate,JoineryRejected
from grounded_review import (manifest,validate_critique,validate_identification,
    identification_messages)

PLAN=pathlib.Path(__file__).with_name("gpt6-joinery-plan-r3.json")
GOOD=json.loads(PLAN.read_text(encoding="utf8"))

class WorkshopContractTests(unittest.TestCase):
 def test_authored_plan_valid(self):
    self.assertEqual(validate(GOOD)["joint_count"],2)
 def test_missing_artist_false_approval(self):
    p=copy.deepcopy(GOOD);p["art_approved"]=True
    with self.assertRaises(JoineryRejected):validate(p)
 def test_cannot_have_disconnected_tenon(self):
    p=copy.deepcopy(GOOD);p["joints"][0]["center_m"][2]+=0.3
    with self.assertRaisesRegex(JoineryRejected,"TENON_NOT_ANCHORED"):validate(p)
 def test_cannot_have_unfittable_joint(self):
    p=copy.deepcopy(GOOD);p["joints"][0]["socket_size_m"]=[.06,.06,.06]
    with self.assertRaisesRegex(JoineryRejected,"TENON_SOCKET"):validate(p)
 def test_cannot_break_roof_facing_lintel_top(self):
    p=copy.deepcopy(GOOD);p["joints"][0]["socket_size_m"][2]=.105
    with self.assertRaises(JoineryRejected):validate(p)
 def test_source_original_never_part_of_new_inventory(self):
    names=[x["id"] for x in GOOD["members"]]
    cat={"artist_plan_sha256":"a"*64,"new_components":names,
      "components":[{"name":"Valoria_Tower_FROZEN","derived_from_source_object":True}]+
       [{"name":n,"derived_from_source_object":False,"vertices":20,
         "dimensions_m":[.122,.122,1.31]} for n in names]}
    contact={"objects":[{"name":n,"min_real_mesh_surface_distance_m":.005,
      "contact_tolerance_m":.07,"surface_contact_candidate":True} for n in names]}
    inv=manifest(GOOD,cat,contact)
    self.assertEqual(inv["counts"]["new"],3)
    hashes={"reference":"x","before":"y","after":"z"}
    msgs=identification_messages(inv,hashes)
    self.assertNotIn("diagonal",msgs[-1]["content"].lower())
    observations=[{"component_id":n,"visibility":"visible","visual_joinery":"poor",
      "measured_contact":"NEAR_SOURCE","visual_reason":"Unclear actual joint at game zoom."}
      for n in names]
    data={"image_sha256":hashes,"observations":observations}
    self.assertEqual(validate_identification(data,inv,hashes)["status"],"IDENTIFICATION_TRUSTED")
    data["observations"][0]["visual_reason"]="Diagonal fascia is broken"
    self.assertEqual(validate_identification(data,inv,hashes)["status"],"IDENTIFICATION_UNRELIABLE")
if __name__=="__main__":unittest.main()
