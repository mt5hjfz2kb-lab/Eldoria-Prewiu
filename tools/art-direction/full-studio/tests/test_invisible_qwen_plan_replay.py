"""Real Qwen face 2326 extruded identically in three rejected Blender cycles."""
import copy, pathlib, sys, unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artist_loop import (executed_action_signature,
                         should_reject_repeated_invisible_plan, schema_for_plan)
from contract import RejectedPlan

FACE="Valoria_Tower_CohV1 · EA03_Village_Tover_01a_LOD0"
SOURCE=[{"name":FACE,"faces":5338,"vertices":11400,"edges":14000,
         "face_landmarks":[{"face_index":2326}],
         "edge_landmarks":[{"edge_index":3989}]}]

def plan(op="extrude",face=2326,offset=.15,display_id="first"):
    return {"actions":[{"id":display_id,"op":op,"target":FACE,
                        "params":{"faces":[face],"offset":[0,0,offset]}}]}
class RejectReplayedInvisibleGeometry(unittest.TestCase):
    def test_real_qwen_same_face_and_offset_after_zero_pixel_blender(self):
        first=plan();history={executed_action_signature(first)}
        replay=plan(display_id="a_new_name")
        original=copy.deepcopy(replay)
        with self.assertRaisesRegex(RejectedPlan,"ZERO_PIXEL_CHANGE_REPLAY_REJECTED"):
            should_reject_repeated_invisible_plan(replay,history)
        self.assertEqual(replay,original)
    def test_artist_may_create_new_geometry_intent_not_one_fixed_recipe(self):
        h={executed_action_signature(plan())}
        self.assertTrue(should_reject_repeated_invisible_plan(plan(offset=.08),h))
    def test_camera_invisible_extrude_removed_but_other_blender_tools_available(self):
        schema=schema_for_plan(5,True,"full","architecture",[FACE],SOURCE,True,False,0,False,{"extrude"})
        ops={v["properties"]["op"]["const"]
             for v in schema["properties"]["actions"]["prefixItems"][0]["oneOf"]}
        self.assertNotIn("extrude",ops)
        self.assertIn("sculpt_anchor",ops)
        self.assertIn("bevel",ops)
        self.assertIn("primitive",ops)
        self.assertEqual(schema["properties"]["actions"]["minItems"],1)
    def test_default_menu_still_offers_extrude_on_other_unrelated_source(self):
        schema=schema_for_plan(5,True,"full","architecture",[FACE],SOURCE,True)
        ops={v["properties"]["op"]["const"]
             for v in schema["properties"]["actions"]["prefixItems"][0]["oneOf"]}
        self.assertIn("extrude",ops)
if __name__=="__main__":unittest.main()
