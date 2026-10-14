"""Real Blender 3m tower: reject whole-object deformations, preserve Qwen choice."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from sculpt_scale_contract import bounds_for_dimensions, validate_source_anchor_brush
from source_geometry_gate import verify_source_plan, RejectedPlan
from professional_source_schema import source_artist_action_schema
from artist_loop import schema_for_plan

SOURCE=[{"name":"Actual Valoria Tower", "faces":5338, "edges":12400,
         "world_dimensions_m":[2.44774,3.27077,2.65],
         "local_extrema":{"z_max":{"local_position_m":[0,0,1.2]},
                          "x_min":{"local_position_m":[-1.2,0,0]}}}]
def action(distance,radius):
    return {"op":"sculpt_anchor","target":SOURCE[0]["name"],
            "params":{"anchor":"z_max","direction":"+Z",
                      "distance":distance,"radius":radius}}

class RealScaleTests(unittest.TestCase):
    def test_real_tower_sculpt_ranges_are_local_not_entire_building(self):
        limits=bounds_for_dimensions(SOURCE[0]["world_dimensions_m"])
        self.assertLess(limits["distance"]["maximum"],.31)
        self.assertLess(limits["radius"]["maximum"],1.2)
        self.assertGreater(limits["distance"]["minimum"],0)
        self.assertGreater(limits["radius"]["minimum"],0)
    def test_old_giant_brush_rejected_without_modifying_original_plan(self):
        plan={"actions":[action(1.2,4.0)]};snapshot=copy.deepcopy(plan)
        with self.assertRaisesRegex(RejectedPlan,"SCULPT_SCALE_REJECTED"):
            verify_source_plan(plan,SOURCE)
        self.assertEqual(plan,snapshot)
    def test_qwen_still_selects_valid_original_small_brush(self):
        plan={"actions":[action(.12,.62)]};snapshot=copy.deepcopy(plan)
        self.assertTrue(verify_source_plan(plan,SOURCE))
        self.assertEqual(plan,snapshot)
    def test_actual_inspected_geometry_limits_typed_full_and_mesh_edit_schema(self):
        full=source_artist_action_schema(SOURCE)["oneOf"]
        anchor=next(v for v in full if v["properties"]["op"]["const"]=="sculpt_anchor")
        scale=anchor["properties"]["params"]["properties"]
        self.assertLess(scale["distance"]["maximum"],.31)
        self.assertLess(scale["radius"]["maximum"],1.2)
        narrow=schema_for_plan(3,True,"mesh_edit","architecture",[SOURCE[0]["name"]],SOURCE)
        params=narrow["properties"]["actions"]["items"]["properties"]["params"]["properties"]
        self.assertLess(params["distance"]["maximum"],.31)
        self.assertLess(params["radius"]["maximum"],1.2)
    def test_missing_source_dimensions_not_falsely_invented(self):
        p={"world_dimensions_m":[0,3,2]}
        with self.assertRaises(ValueError):validate_source_anchor_brush(action(.12,.5),p)
        self.assertIsNone(__import__("sculpt_scale_contract").source_brush_bounds({"name":"legacy"}))
if __name__=="__main__":unittest.main()
