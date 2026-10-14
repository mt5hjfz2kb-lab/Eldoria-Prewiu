"""Strict Qwen-selectable actual Blender tool contracts; no fabricated shapes."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from professional_source_schema import source_artist_action_schema
from artist_loop import schema_for_plan
SOURCE=[{"name":"Authentic Valoria source","faces":5338,"vertices":11400}]
class TypedArtistTests(unittest.TestCase):
 def test_model_has_nine_distinct_creative_blender_operations(self):
  schema=source_artist_action_schema(SOURCE)
  variations=schema["oneOf"]
  ops={variant["properties"]["op"]["const"] for variant in variations}
  self.assertEqual(ops,{"sculpt_anchor","sculpt","extrude","inset","bevel",
                        "material","uv","modifier","native_operator"})
  for v in variations:
   self.assertEqual(v["properties"]["target"]["enum"],["Authentic Valoria source"])
   self.assertTrue(v["properties"]["params"]["additionalProperties"] is False)
 def test_face_range_is_blender_face_count_not_vertex_count(self):
  z=source_artist_action_schema(SOURCE)["oneOf"]
  extrude=next(v for v in z if v["properties"]["op"]["const"]=="extrude")
  self.assertEqual(extrude["properties"]["params"]["properties"]["faces"]["items"]["maximum"],5337)
 def test_artistic_full_mode_dispatches_strict_schema(self):
  s=schema_for_plan(5,True,"full","architecture",["Authentic Valoria source"],SOURCE)
  self.assertIn("oneOf",s["properties"]["actions"]["items"])
 def test_non_source_creation_stays_unaffected(self):
  s=schema_for_plan(5,False,"full","architecture")
  self.assertEqual(s["properties"]["actions"]["items"]["properties"]["op"]["const"],"primitive")
if __name__=="__main__":unittest.main()
