"""Eldoria director gate test fixtures: no PC and no Blender required."""
import sys,pathlib,copy,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
from evidence_gate import validate, RejectedArtDirection, EXPECTED
class TestEvidenceGate(unittest.TestCase):
    def setUp(self):
        self.obs={"source_sha256":EXPECTED,"frames":[
            {"view":"front_three_quarter","sha256":"a"*64},
            {"view":"reverse_three_quarter","sha256":"b"*64},
            {"view":"elevated_roof","sha256":"c"*64}]}
        self.geo={"source_sha256":EXPECTED,"immutable_source_verified":True,
                  "all_cameras_same_exact_source_sha256":True}
        self.good={"schema_version":1,"original_sha256":EXPECTED,
            "mode":"DIAGNOSTIC_ONLY","art_approved":False,"editor_actions":[],
            "proposals":[{"decision":"INSPECT_ONLY",
                "evidence":[{"camera_id":"front_three_quarter","png_sha256":"a"*64}],
                "geometry_refs":["bounding_box.world_dimensions_m"],
                "contact_verified":False,"protected_changes":[],
                "requires_manual_visual_review":True,
                "uncertainty":"No está verificado el contacto entre superficies; inspeccionar malla original."}]}
    def test_neutral_evidence_does_not_authorize_blender(self):
        response=validate(self.good,self.geo,self.obs)
        self.assertEqual(response["checked_proposals"],1)
        self.assertFalse(response["can_edit_blender"])
        self.assertFalse(response["art_approved"])
    def test_empty_diagnosis_acceptable_without_fake_improvements(self):
        x=copy.deepcopy(self.good);x["proposals"]=[]
        response=validate(x,self.geo,self.obs)
        self.assertEqual(response["checked_proposals"],0)
        self.assertFalse(response["can_edit_blender"])
    def test_wrong_source_denied(self):
        x=copy.deepcopy(self.good);x["original_sha256"]="0"*64
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_spatial_context_mismatch_denied(self):
        x=copy.deepcopy(self.geo);x["source_sha256"]="f"*64
        with self.assertRaises(RejectedArtDirection):validate(self.good,x,self.obs)
    def test_camera_belongs_to_other_image_denied(self):
        x=copy.deepcopy(self.good);x["proposals"][0]["evidence"][0]["png_sha256"]="0"*64
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_no_modeling_commands(self):
        x=copy.deepcopy(self.good);x["editor_actions"]=[{"op":"move_staircase"}]
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_no_protected_changes(self):
        x=copy.deepcopy(self.good);x["proposals"][0]["protected_changes"]=["stair"]
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_no_guessed_contact(self):
        x=copy.deepcopy(self.good);x["proposals"][0]["contact_verified"]=True
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_unproved_geometry_reference_denied(self):
        x=copy.deepcopy(self.good)
        x["proposals"][0]["geometry_refs"]=["roof_has_four_slopes_exact"]
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_edit_authorization_denied_even_if_approval_requested(self):
        x=copy.deepcopy(self.good);x["art_approved"]=True
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_noninspection_proposal_denied(self):
        x=copy.deepcopy(self.good);x["proposals"][0]["decision"]="EXECUTE_BLENDER"
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)
    def test_no_review_denied(self):
        x=copy.deepcopy(self.good);x["proposals"][0]["requires_manual_visual_review"]=False
        with self.assertRaises(RejectedArtDirection):validate(x,self.geo,self.obs)

if __name__=="__main__":unittest.main()
