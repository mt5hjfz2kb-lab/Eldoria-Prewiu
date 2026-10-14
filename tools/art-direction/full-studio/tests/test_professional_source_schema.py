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
 def test_real_observed_faces_and_edges_are_model_choices_not_random_indices(self):
  source=[{"name":"Stone source","faces":6000,"edges":8400,
           "face_landmarks":[{"face_index":14},{"face_index":484}],
           "edge_landmarks":[{"edge_index":35},{"edge_index":91}]}]
  schema=source_artist_action_schema(source)["oneOf"]
  byop={x["properties"]["op"]["const"]:x for x in schema}
  faces=byop["extrude"]["properties"]["params"]["properties"]["faces"]["items"]
  edges=byop["bevel"]["properties"]["params"]["properties"]["edges"]["items"]
  self.assertEqual(faces,{"type":"integer","enum":[14,484]})
  self.assertEqual(edges,{"type":"integer","enum":[35,91]})
 def test_invalid_landmark_indices_do_not_enter_schema(self):
  source=[{"name":"Tower","faces":4,"edges":8,
           "face_landmarks":[{"face_index":11},{"face_index":2}],
           "edge_landmarks":[{"edge_index":90},{"edge_index":7}]}]
  variants=source_artist_action_schema(source)["oneOf"]
  byop={v["properties"]["op"]["const"]:v for v in variants}
  self.assertEqual(byop["extrude"]["properties"]["params"]["properties"]["faces"]["items"]["enum"],[2])
  self.assertEqual(byop["bevel"]["properties"]["params"]["properties"]["edges"]["items"]["enum"],[7])
 def test_non_source_creation_stays_unaffected(self):
  s=schema_for_plan(5,False,"full","architecture")
  self.assertEqual(s["properties"]["actions"]["items"]["properties"]["op"]["const"],"primitive")
if __name__=="__main__":unittest.main()
