"""Strict Qwen-selectable actual Blender tool contracts; no fabricated shapes."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from professional_source_schema import source_artist_action_schema
from artist_loop import schema_for_plan
SOURCE=[{"name":"Authentic Valoria source","faces":5338,"vertices":11400}]
class TypedArtistTests(unittest.TestCase):
 def test_model_has_eleven_real_creative_blender_operation_families(self):
  schema=source_artist_action_schema(SOURCE)
  variations=schema["oneOf"]
  ops={variant["properties"]["op"]["const"] for variant in variations}
  self.assertEqual(ops,{"sculpt_anchor","sculpt","extrude","inset","bevel",
                        "material","uv","modifier","native_operator","primitive","mesh"})
  for v in variations:
   name=v["properties"]["op"]["const"]
   if name in ("primitive","mesh"):
    self.assertEqual(v["properties"]["target"]["type"],"string")
   else:
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
 def test_dense_original_mesh_forbids_whole_object_bevel_modifier(self):
  observed=[{"name":"Large source","faces":5338,"edges":10000}]
  variants=source_artist_action_schema(observed)["oneOf"]
  modifier=next(v for v in variants if v["properties"]["op"]["const"]=="modifier")
  opts=modifier["properties"]["params"]["properties"]
  self.assertNotIn("BEVEL",opts["kind"]["enum"])
  self.assertTrue(opts["options"]["additionalProperties"] is False)
  self.assertNotIn("apply",opts["options"]["properties"])
 def test_low_poly_source_can_still_select_a_bevel_modifier(self):
  observed=[{"name":"Small new structure","faces":36,"edges":75}]
  variants=source_artist_action_schema(observed)["oneOf"]
  modifier=next(v for v in variants if v["properties"]["op"]["const"]=="modifier")
  self.assertIn("BEVEL",modifier["properties"]["params"]["properties"]["kind"]["enum"])
 def test_invalid_landmark_indices_do_not_enter_schema(self):
  source=[{"name":"Tower","faces":4,"edges":8,
           "face_landmarks":[{"face_index":11},{"face_index":2}],
           "edge_landmarks":[{"edge_index":90},{"edge_index":7}]}]
  variants=source_artist_action_schema(source)["oneOf"]
  byop={v["properties"]["op"]["const"]:v for v in variants}
  self.assertEqual(byop["extrude"]["properties"]["params"]["properties"]["faces"]["items"]["enum"],[2])
  self.assertEqual(byop["bevel"]["properties"]["params"]["properties"]["edges"]["items"]["enum"],[7])
 def test_optional_safe_source_sequence_still_leaves_artist_choice(self):
  source=[{"name":"Tower","faces":5338,"vertices":11400,
           "edges":14000,"face_landmarks":[{"face_index":2326}],
           "edge_landmarks":[{"edge_index":3989}]}]
  schema=schema_for_plan(5,True,"full","architecture",["Tower"],source,True)
  arr=schema["properties"]["actions"]
  self.assertEqual((arr["minItems"],arr["maxItems"]),(1,1),
                   "Dense inherited atlas cannot receive forced global UV or material edits")
  self.assertNotIn("items",arr,"Ollama prefixItems must not combine with unsupported items:false")
  structurals={v["properties"]["op"]["const"] for v in arr["prefixItems"][0]["oneOf"]}
  self.assertEqual(structurals,{"sculpt_anchor","sculpt","extrude","inset","bevel","primitive","mesh"})
  self.assertEqual(len(arr["prefixItems"]),1,"Dense atlas source must not be wholly repainted")
  extrude=next(v for v in arr["prefixItems"][0]["oneOf"] if v["properties"]["op"]["const"]=="extrude")
  self.assertEqual(extrude["properties"]["params"]["properties"]["faces"]["items"]["enum"],[2326])
 def test_source_build_phase_is_author_selected_primitive_or_mesh(self):
  source=[{"name":"Inherited Tower","faces":5338,"vertices":11400}]
  schema=schema_for_plan(4,True,"full","architecture",["Inherited Tower"],source,True,True)
  options=schema["properties"]["actions"]["prefixItems"][0]["oneOf"]
  self.assertEqual({v["properties"]["op"]["const"] for v in options},{"primitive","mesh"})
  primitive=next(v for v in options if v["properties"]["op"]["const"]=="primitive")
  self.assertIn("location",primitive["properties"]["params"]["properties"])
  self.assertIn("scale",primitive["properties"]["params"]["properties"])
  authored=next(v for v in options if v["properties"]["op"]["const"]=="mesh")
  self.assertEqual(authored["properties"]["params"]["properties"]["vertices"]["maxItems"],12)
  self.assertTrue(authored["properties"]["params"]["properties"]["vertices"]["uniqueItems"])
 def test_new_assembly_namespace_and_surface_target_binding(self):
  from professional_source_schema import NEW_COMPONENT_PATTERN
  import re
  src=[{"name":"Valoria_Tower_CohV1 · EA03_Village_Tower_LOD0","faces":5338,"vertices":11000}]
  s=schema_for_plan(4,True,"full","architecture",[src[0]["name"]],src,True,True)
  one,two,*tail=s["properties"]["actions"]["prefixItems"]
  self.assertFalse(re.fullmatch(NEW_COMPONENT_PATTERN,src[0]["name"]))
  self.assertTrue(re.fullmatch(NEW_COMPONENT_PATTERN,"Eldoria_Added_StoneCanopy"))
  for choice in one["oneOf"]:
   self.assertEqual(choice["properties"]["target"]["pattern"],NEW_COMPONENT_PATTERN)
  for slot in [two]+tail:
   for choice in slot["oneOf"]:
    self.assertEqual(choice["properties"]["target"]["pattern"],NEW_COMPONENT_PATTERN)
  ordinary=schema_for_plan(4,True,"full","architecture",[src[0]["name"]],src,True,False)
  stages=ordinary["properties"]["actions"]["prefixItems"]
  self.assertEqual(len(stages),1,"Original dense GLB has one safe source edit before later finishing")
  inherited=next(v for v in stages[0]["oneOf"]
                 if v["properties"]["op"]["const"]=="sculpt_anchor")
  self.assertEqual(inherited["properties"]["target"]["enum"],[src[0]["name"]])
 def test_each_cycle_distinct_model_authored_names_and_adaptive_safe_operator(self):
  src=[{"name":"Tower","faces":5340,"vertices":10000}]
  first=schema_for_plan(4,True,"full","architecture",["Tower"],src,True,True,1)
  second=schema_for_plan(4,True,"full","architecture",["Tower"],src,True,True,2)
  def structure(s):return s["properties"]["actions"]["prefixItems"][0]["oneOf"]
  a=structure(first)[0]["properties"]["target"]["pattern"]
  b=structure(second)[0]["properties"]["target"]["pattern"]
  import re
  self.assertNotEqual(a,b)
  self.assertTrue(re.fullmatch(a,"Eldoria_Added_C01_RoofTruss"))
  self.assertFalse(re.fullmatch(b,"Eldoria_Added_C01_RoofTruss"))
  self.assertTrue(re.fullmatch(b,"Eldoria_Added_C02_ArchSupport"))
  fallback=schema_for_plan(4,True,"full","architecture",["Tower"],src,True,True,2,True)
  kinds={v["properties"]["op"]["const"] for v in structure(fallback)}
  self.assertEqual(kinds,{"primitive"})
  self.assertEqual(len(fallback["properties"]["actions"]["prefixItems"][1]["oneOf"]),2)
 def test_non_source_creation_stays_unaffected(self):
  s=schema_for_plan(5,False,"full","architecture")
  self.assertEqual(s["properties"]["actions"]["items"]["properties"]["op"]["const"],"primitive")
if __name__=="__main__":unittest.main()
