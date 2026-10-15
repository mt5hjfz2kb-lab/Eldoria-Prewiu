"""Regression: actual Qwen 8B circular cap at apex is not a timber eave."""
import copy, pathlib, sys, unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from architectural_world_placement_gate import reject_obviously_bad_component_placement
from contract import RejectedPlan

SOURCE=[{"name":"Real 3.27m Valoria watchtower","faces":5338,
    "world_bounds_min_m":[-1.22387,-1.325,0.0],
    "world_bounds_max_m":[1.22387,1.325,3.27077]}]

def qwen(name,location,scale):
    return {"actions":[{"op":"primitive","target":name,
        "params":{"shape":"cylinder","location":location,"rotation":[0,0,0],"scale":scale}}]}

class EaveContactTests(unittest.TestCase):
    def test_genuine_qwen4b_new_timber_support_floating_next_to_real_source_rejected(self):
        # Original RAW Qwen4B cycle-2 run #38652707944 added a detached
        # 0.6m timber-looking box at WORLD [0.5,2.5,1.2].
        # Source WORLD Y is [-1.325, 1.325], so gap is 0.875m.
        p=qwen("Eldoria_Added_C02_XXX_001",[.5,2.5,1.2],[.3,.3,.3])
        before=copy.deepcopy(p)
        with self.assertRaisesRegex(RejectedPlan,"NO_SOURCE_CONTACT_AABB"):
            reject_obviously_bad_component_placement(p,SOURCE)
        self.assertEqual(p,before,"Validator never repairs the model's art values")
    def test_detached_candidate_named_bracket_cannot_bypass_with_good_label(self):
        with self.assertRaisesRegex(RejectedPlan,"NO_SOURCE_CONTACT_AABB"):
            reject_obviously_bad_component_placement(
                qwen("Eldoria_Added_C02_TimberRoofBracket",
                     [.5,2.5,1.2],[.3,.3,.3]),SOURCE)
    def test_source_touching_world_aabb_only_potential_not_art_approved(self):
        proposed=qwen("Eldoria_Added_C02_OriginalWoodSupport",
                      [1.18,0.8,2.30],[.25,.25,.35])
        valid=reject_obviously_bad_component_placement(proposed,SOURCE)
        self.assertEqual(valid["bbox_contact_axis_gaps_m"],[0,0,0])
        self.assertFalse(valid["art_approved"])
    def test_real_model_roof_apex_disc_rejected_without_repair(self):
        plan=qwen("Eldoria_Added_C01_RoofOverhang",[0.0,0.0,3.27],[.5,.5,.1])
        before=copy.deepcopy(plan)
        with self.assertRaisesRegex(RejectedPlan,"roof-edge"):
            reject_obviously_bad_component_placement(plan,SOURCE)
        self.assertEqual(plan,before)
    def test_real_model_second_apex_circle_rejected(self):
        with self.assertRaisesRegex(RejectedPlan,"roof-edge"):
            reject_obviously_bad_component_placement(
                qwen("Eldoria_Added_C02_RoofEaveDetail",[0.,0.,3.27],[.2,.2,.1]),SOURCE)
    def test_source_reachable_eave_below_apex_allowed_only_as_candidate(self):
        p=qwen("Eldoria_Added_C01_TimberEaveBracket",[1.15,0,2.35],[.20,.15,.20])
        result=reject_obviously_bad_component_placement(p,SOURCE)
        self.assertFalse(result["art_approved"])
        self.assertEqual(result["changes_to_artist_values"],0)
    def test_genuine_unrelated_roof_finial_still_possible(self):
        p=qwen("Eldoria_Added_C01_RoofFinial",[0,0,3.27],[.2,.2,.3])
        self.assertFalse(reject_obviously_bad_component_placement(p,SOURCE)["art_approved"])

if __name__=="__main__":unittest.main()
