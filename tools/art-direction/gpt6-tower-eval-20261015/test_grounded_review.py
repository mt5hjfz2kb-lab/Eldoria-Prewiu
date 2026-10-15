"""Adversarial and regression tests: previously false 'diagonal fascia' must HOLD."""
import copy, json, unittest
from grounded_review import manifest, validate_critique, critic_messages

SOURCE="Valoria_Tower_CohV1"
LEFT="Eldoria_GPT6_R2_LeftFramePost"
RIGHT="Eldoria_GPT6_R2_RightFramePost"
LINTEL="Eldoria_GPT6_R2_FrontLogLintel"
IDS=(LEFT,RIGHT,LINTEL)

def evidence():
    plan={"asset_id":"test_r2","actions":[{"id":n,"op":"primitive","target":n,"params":{
        "shape":"cube","location":[0,0,0],"scale":[.05,.05,1.],"rotation":[0,0,0]}} for n in IDS]}
    catalog={"artist_plan_sha256":"a"*64,"new_components":list(IDS),
       "components":[{"name":SOURCE,"derived_from_source_object":True}]+
          [{"name":n,"derived_from_source_object":False,"vertices":8,
            "dimensions_m":[.1,.1,1.]} for n in IDS]}
    contacts={"objects":[{"name":n,"min_real_mesh_surface_distance_m":.004,
       "contact_tolerance_m":.07,"surface_contact_candidate":True} for n in IDS]}
    return manifest(plan,catalog,contacts)

def review(inv):
    hashes={"reference":"a"*64,"before":"b"*64,"after":"c"*64}
    return hashes,{"image_sha256":dict(hashes),"observations":[{"component_id":n,
      "visibility":"visible","visual_joinery":"uncertain",
      "measured_contact":"NEAR_SOURCE","visual_reason":"Appears as a simple cuboid in this camera."} for n in IDS],
      "decision":"INCONCLUSIVE","scores":{"silhouette":5,"architectural_contact":5,
      "materials":5,"style":5,"game_camera":5},
      "reasons":["Unable to determine aesthetic improvement."],"uncertainties":["Fine-scale joints obscured."]}

class CriticTests(unittest.TestCase):
    def setUp(self):
        self.inv=evidence()
        self.hashes,self.p=review(self.inv)
    def decision(self,p):
        return validate_critique(p,self.inv,self.hashes)
    def test_valid_grounded_review_provisional_only(self):
        r=self.decision(self.p);self.assertEqual(r["status"],"PROVISIONAL_REVIEW_GROUNDED")
        self.assertFalse(r["art_approved"])
    def test_old_hallucinated_diagonals(self):
        p=copy.deepcopy(self.p);p["reasons"]=["The diagonal supports float in the image."]
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_old_fascia(self):
        p=copy.deepcopy(self.p);p["uncertainties"]=["New fascia seems large."]
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_no_first_candidate_contamination_in_prompt(self):
        msgs=critic_messages(self.inv,self.hashes)
        user=msgs[-1]["content"];self.assertNotIn("fascia",user.lower())
        self.assertNotIn("diagonal",user.lower())
        self.assertIn(LINTEL,user)
        self.assertIn("NEAR_SOURCE",user)
    def test_unknown_object_rejected(self):
        p=copy.deepcopy(self.p);p["observations"][0]["component_id"]="FAKE"
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_missing_object_rejected(self):
        p=copy.deepcopy(self.p);p["observations"].pop()
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_contact_visual_and_physical_are_separate(self):
        p=copy.deepcopy(self.p);p["observations"][0]["visual_joinery"]="poor"
        self.assertEqual(self.decision(p)["status"],"PROVISIONAL_REVIEW_GROUNDED")
        p["observations"][0]["measured_contact"]="NOT_NEAR_SOURCE"
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_bad_scores_hold(self):
        p=copy.deepcopy(self.p);p["scores"]["style"]=11
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_missing_image_provenance_hold(self):
        p=copy.deepcopy(self.p);p["image_sha256"]["after"]="bogus"
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")
    def test_manifest_inventory_mismatch(self):
        p={"asset_id":"test","actions":[{"op":"primitive","target":"something","params":{}}]}
        c={"artist_plan_sha256":"a"*64,"new_components":list(IDS),"components":[]}
        with self.assertRaisesRegex(ValueError,"MISMATCH"):
            manifest(p,c,{"objects":[]})
    def test_duplicate_observations_hold(self):
        p=copy.deepcopy(self.p);p["observations"][1]=copy.deepcopy(p["observations"][0])
        self.assertEqual(self.decision(p)["status"],"REVIEW_UNGROUNDED")

if __name__=="__main__": unittest.main()
