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
