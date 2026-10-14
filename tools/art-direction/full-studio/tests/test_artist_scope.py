"""Regression tests: prevent Qwen from replaying previous cliff tasks instead of new art."""
import json,sys,tempfile,unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import artist_loop
from contract import RejectedPlan

def new_asset_plan():
    return {"schema_version":2,"asset_id":"medieval-stone-watchpost",
      "asset_decision":"NEW",
      "observed_defect":"Current asset does not have any medieval defence silhouette.",
      "design_intent":"Design and model a stone watchpost with a separate roof.",
      "expected_visual_difference":"Two new connected medieval forms are clearly readable in game view.",
      "mobile_risks":"Two low polygon sources reduce mobile cost.",
      "actions":[
        {"id":"stone-base","op":"primitive","target":"Stone lookout wall",
         "reason":"Create actual vertical masonry for the new lookout",
         "params":{"shape":"cylinder","location":[0,0,1.5],"scale":[1.5,1.5,1.5]}},
        {"id":"roof","op":"primitive","target":"Timber lookout roof",
         "reason":"Give the lookout an original timber roof silhouette",
         "params":{"shape":"cone","location":[0,0,3.2],"scale":[2,2,1.2]}}]}

class AuthorScopeTests(unittest.TestCase):
    def test_first_cycle_schema_restricts_old_reuse_and_sculpt(self):
        schema=artist_loop.schema_for_plan(4,False)
        self.assertEqual(schema["properties"]["asset_decision"]["enum"],["NEW"])
        self.assertEqual(schema["properties"]["actions"]["items"]["properties"]["op"]["const"],"primitive")
        params=schema["properties"]["actions"]["items"]["properties"]["params"]
        self.assertEqual(params["properties"]["scale"]["items"]["maximum"],8)
        self.assertEqual(params["properties"]["location"]["items"]["maximum"],15)
        self.assertEqual(schema["properties"]["actions"]["minItems"],2)
        self.assertEqual(schema["properties"]["actions"]["maxItems"],4)
    def test_existing_cycle_supports_all_advanced_tools(self):
        schema=artist_loop.schema_for_plan(5,True)
        allowed=schema["properties"]["actions"]["items"]["properties"]["op"]["enum"]
        for op in ("sculpt","mesh","shader_graph","geometry_nodes","native_operator"):
            self.assertIn(op,allowed)
    def test_surface_phase_allows_artistic_materials_but_not_unsafe_code(self):
        schema=artist_loop.schema_for_plan(5,True,"surface")
        ops=schema["properties"]["actions"]["items"]["properties"]["op"]["enum"]
        self.assertIn("material",ops)
        self.assertIn("primitive",ops)
        self.assertIn("modifier",ops)
        self.assertNotIn("geometry_nodes",ops)
        self.assertNotIn("execute_python",ops)
    def test_fresh_valid_approval_free_plan(self):
        with tempfile.TemporaryDirectory() as temp:
            args=SimpleNamespace(max_actions=4,max_tokens=1200,endpoint="http://127.0.0.1:11434/api/chat",model="qwen3-vl:2b-instruct",brief="Design original watchtower with roof",asset_category="architecture",phase="full")
            with patch.object(artist_loop,"image_data",return_value="encoded"),patch.object(artist_loop,"request_ollama",return_value=json.dumps(new_asset_plan())):
                doc=artist_loop.ask_plan(args,Path("ref"),None,"","art director","",Path(temp),1,False)
                self.assertEqual(doc["asset_decision"],"NEW")
                self.assertEqual(len(doc["actions"]),2)
    def test_replay_old_cliff_rejected(self):
        p=new_asset_plan()
        p["asset_decision"]="REUSE"
        p["actions"]=[{"id":"old-cliff","op":"sculpt","target":"cliff_rocks",
                      "reason":"Replay stale old cliff commands rather than new asset",
                      "params":{"center":[-40,57,-21],"delta":[.5,.5,.5],"radius":10}}]
        with tempfile.TemporaryDirectory() as folder:
            args=SimpleNamespace(max_actions=4,max_tokens=1200,endpoint="http://127.0.0.1:11434/api/chat",model="qwen3-vl:2b-instruct",brief="Design original watchtower with roof",asset_category="architecture",phase="full")
            with patch.object(artist_loop,"image_data",return_value="encoded"),patch.object(artist_loop,"request_ollama",return_value=json.dumps(p)):
                with self.assertRaises((RejectedPlan,artist_loop.CompositionRejected)):
                    artist_loop.ask_plan(args,Path("ref"),None,"","art director","",Path(folder),1,False)

if __name__=="__main__":
    unittest.main()
