"""Owner requirement: after two rejected Qwen attempts with same artistic tool,
switch the actual model's offered Blender operations; after five, diagnose.
"""
import json,pathlib,sys,tempfile,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artist_loop import track_rejected_strategy,rejected_plan_family

class ModelHypothesisSwitch(unittest.TestCase):
    def test_two_actual_invalid_sculpt_variants_disable_sculpt_family(self):
        with tempfile.TemporaryDirectory() as tmp:
            p=pathlib.Path(tmp)/"raw.json";counts={};blocked=set()
            for radius in (0.15,0.03):
                p.write_text(json.dumps({"actions":[
                    {"op":"sculpt_anchor","target":"Frozen real tower",
                     "params":{"radius":radius}}]}))
                result=track_rejected_strategy(p,counts,blocked)
                self.assertEqual(result,"sculpt_anchor")
            self.assertEqual(counts["sculpt_anchor"],2)
            self.assertIn("sculpt_anchor",blocked)
            self.assertNotIn("extrude",blocked)
    def test_different_source_geometry_remains_possible(self):
        with tempfile.TemporaryDirectory() as tmp:
            p=pathlib.Path(tmp)/"raw.json";counts={};blocked=set()
            for name in ("bevel","sculpt_anchor","mesh"):
                p.write_text(json.dumps({"actions":[{"op":name}]}))
                track_rejected_strategy(p,counts,blocked)
            self.assertFalse(blocked)
            self.assertEqual(counts,{"bevel":1,"sculpt_anchor":1,"mesh":1})
    def test_invalid_json_does_not_invent_qwen_tool(self):
        with tempfile.TemporaryDirectory() as tmp:
            p=pathlib.Path(tmp)/"raw.json";p.write_text("NOT JSON")
            counts={};blocked=set()
            self.assertIsNone(rejected_plan_family(p))
            self.assertIsNone(track_rejected_strategy(p,counts,blocked))
            self.assertEqual(counts,{})
            self.assertFalse(blocked)

if __name__=="__main__":unittest.main()
