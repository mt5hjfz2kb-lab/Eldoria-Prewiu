"""Regression of the real failed full-artist Qwen proposals on Windows PC."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from source_geometry_gate import verify_source_plan,RejectedPlan
NAME="Valoria Tower"
SOURCE=[{"name":NAME,"vertices":11400,"faces":5338,"local_extrema":{
  "x_max":{"local_position_m":[4.06219,1.60329,-3.76578]},
  "z_max":{"local_position_m":[-3.73754,6.10871,4.26061]},
  "y_max":{"local_position_m":[-0.3498,10.35283,0.21904]}}}]
def op(kind,params):return {"op":kind,"target":NAME,"params":params}
class Grounding(unittest.TestCase):
 def test_original_33k_triangles_are_not_rejected_by_wrong_face_budget(self):
  from source_geometry_gate import relative_triangle_limit
  mesh=[{"faces":5344,"evaluated_triangles":33510}]
  self.assertGreaterEqual(relative_triangle_limit(mesh),33522)
  self.assertLess(relative_triangle_limit(mesh),60000)
 def test_triangulation_facts_required_fail_closed(self):
  from source_geometry_gate import relative_triangle_limit
  with self.assertRaisesRegex(RejectedPlan,"evaluated source triangle counts"):
   relative_triangle_limit([{"faces":5344}])
 def test_pathological_density_growth_remains_bounded(self):
  from source_geometry_gate import relative_triangle_limit
  self.assertLessEqual(relative_triangle_limit([{"evaluated_triangles":200000}]),220000)

 def test_original_invalid_vertex_id_used_as_face(self):
  p={"actions":[op("extrude",{"faces":[11243],"offset":[0,0,1]})]}; before=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"5338 real Blender faces"):verify_source_plan(p,SOURCE)
  self.assertEqual(p,before)
 def test_real_valid_face_not_modified(self):
  p={"actions":[op("extrude",{"faces":[805],"offset":[0,0,1]})]};b=copy.deepcopy(p)
  self.assertTrue(verify_source_plan(p,SOURCE));self.assertEqual(p,b)
 def test_original_empty_space_sculpt_fails_fast(self):
  with self.assertRaisesRegex(RejectedPlan,"no verified inherited mesh landmark"):
   verify_source_plan({"actions":[op("sculpt",{"center":[0,1.5,0],"delta":[0,1,0],"radius":.5})]},SOURCE)
 def test_real_landmark_sculpt_passes(self):
  self.assertTrue(verify_source_plan({"actions":[op("sculpt",{"center":[-3.73754,6.10871,4.26061],"radius":1.8})]},SOURCE))
 def test_preserve_original_imported_atlas_in_mixed_source_operations(self):
  p={"actions":[op("sculpt_anchor",{"anchor":"z_max","direction":"+Z","distance":1.0,"radius":3}),
     op("material",{"base_color":[.4,.4,.4,1],"roughness":.9,"metallic":0}),
     op("uv",{"method":"smart"})]};before=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"PRESERVE_INHERITED_TEXTURE_ATLAS"):
   verify_source_plan(p,SOURCE)
  self.assertEqual(p,before)
 def test_source_local_mesh_edit_remains_authorized_without_full_uv_repaint(self):
  p={"actions":[op("sculpt_anchor",{"anchor":"z_max","direction":"+Z","distance":1.0,"radius":3})]}
  self.assertTrue(verify_source_plan(p,SOURCE))
 def test_topology_changes_force_new_inspection_before_face_ids(self):
  p={"actions":[op("extrude",{"faces":[13],"offset":[0,0,1]}),op("inset",{"faces":[15],"thickness":.04})]}
  with self.assertRaisesRegex(RejectedPlan,"re-inspect"):verify_source_plan(p,SOURCE)
 def test_source_grounded_selective_bevel_not_whole_mesh(self):
  dense=copy.deepcopy(SOURCE)
  dense[0]["edges"]=14000
  dense[0]["edge_landmarks"]=[{"edge_index":120,"local_midpoint_m":[0,0,1]}]
  plan={"actions":[op("bevel",{"edges":[120],"width":.03,"segments":2,"apply":True})]}
  before=copy.deepcopy(plan)
  self.assertTrue(verify_source_plan(plan,dense))
  self.assertEqual(plan,before)
  with self.assertRaisesRegex(RejectedPlan,"global bevel forbidden"):
   verify_source_plan({"actions":[op("bevel",{"width":.08,"segments":2,"apply":True})]},dense)
  with self.assertRaisesRegex(RejectedPlan,"edge_index"):
   verify_source_plan({"actions":[op("bevel",{"edges":[15000],"width":.03,"segments":2,"apply":True})]},dense)
 def test_dense_original_cannot_global_subdivide_or_modifier(self):
  for action in (op("modifier",{"kind":"SUBSURF","options":{"levels":1},"apply":False}),
                 op("modifier",{"kind":"BEVEL","options":{"width":0.1},"apply":True}),
                 op("native_operator",{"operator":"mesh.subdivide","kwargs":{},"selection":"ALL"})):
   with self.assertRaisesRegex(RejectedPlan,"dense source forbids"):
    verify_source_plan({"actions":[action]},SOURCE)
 def test_no_imaginary_target(self):
  p={"actions":[{"op":"bevel","target":"Fictional new building","params":{"width":.04}}]}
  with self.assertRaisesRegex(RejectedPlan,"unknown target"):verify_source_plan(p,SOURCE)
if __name__=="__main__":unittest.main()
