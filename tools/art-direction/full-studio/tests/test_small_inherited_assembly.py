"""Actual 3m source allows tasteful small exterior details, not flat scraps."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from contract import RejectedPlan
from architectural_component_guard import verify_volume_and_complexity

SOURCE=[{"world_bounds_min_m":[-1.22387,-1.325,0],
         "world_bounds_max_m":[1.22387,1.325,3.27077]}]
def plan(scales):
    return {"actions":[{"op":"primitive","target":"Eldoria_Added_C01_RoofWoodBracket",
                        "params":{"shape":"cube","location":[0,0,2.9],
                                  "rotation":[0,0,0],"scale":scales}}]}

class SmallAttachment(unittest.TestCase):
    def test_source_allows_modest_legible_volumetric_wood_bracket(self):
        self.assertTrue(verify_volume_and_complexity(plan([.2,.2,.2]),SOURCE))
    def test_without_real_source_standalone_structure_remains_larger(self):
        with self.assertRaisesRegex(RejectedPlan,"too thin or tiny"):
            verify_volume_and_complexity(plan([.2,.2,.2]))
    def test_not_a_zero_width_sticker_even_when_source_small(self):
        with self.assertRaisesRegex(RejectedPlan,"too thin or tiny"):
            verify_volume_and_complexity(plan([.2,.2,.01]),SOURCE)

if __name__=="__main__":unittest.main()
