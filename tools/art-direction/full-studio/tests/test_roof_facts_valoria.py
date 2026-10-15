"""Real Valoria tower: Qwen roof design cannot rely on first STAIRCASE faces."""
import unittest,pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artist_loop import candidate_visible_roof_surface_facts

SOURCE=[{"name":"Valoria_Tower_CohV1 · EA03_Village_Tover_01a_LOD0",
"camera_visible_face_landmarks":[
 {"face_index":325,"camera_visible":True,"camera_ray_hit_original_face":True,
  "world_center_m":[0.8313,1.091,0.8972],"world_height_fraction":.2743,
  "camera_screen_uv":[.7161,.6237]},
 {"face_index":324,"camera_visible":True,"camera_ray_hit_original_face":True,
  "world_center_m":[0.5135,1.0906,1.2309],"world_height_fraction":.3763,
  "camera_screen_uv":[.672,.7075]},
 {"face_index":5325,"camera_visible":True,"camera_ray_hit_original_face":True,
  "world_center_m":[-.4535,-.5566,2.7422],"world_height_fraction":.8384,
  "camera_screen_uv":[.3857,.9228]},
 {"face_index":5317,"camera_visible":True,"camera_ray_hit_original_face":True,
  "world_center_m":[-.9216,-1.0231,2.2859],"world_height_fraction":.6989,
  "camera_screen_uv":[.2776,.8195]},
 {"face_index":5190,"camera_visible":True,"camera_ray_hit_original_face":True,
  "world_center_m":[.6744,.5416,2.408],"world_height_fraction":.7362,
  "camera_screen_uv":[.6436,.8857]},
 {"face_index":99999,"camera_visible":True,"camera_ray_hit_original_face":False,
  "world_center_m":[0,0,2.6],"world_height_fraction":.77}]}]

class RoofGrounding(unittest.TestCase):
    def test_source_memo_high_eave_facts_exclude_staircase_325_and_324(self):
        actual=candidate_visible_roof_surface_facts(SOURCE)
        self.assertEqual(actual[0]["original_face_id"],5190)
        self.assertEqual({x["original_face_id"] for x in actual},{5190,5317,5325})
        self.assertEqual(actual[0]["world_surface_point_m"],[.6744,.5416,2.408])
    def test_no_genuine_ray_coverage_means_no_invented_source_points(self):
        self.assertEqual(candidate_visible_roof_surface_facts([]),[])
        self.assertEqual(candidate_visible_roof_surface_facts([{"name":"tower"}]),[])
    def test_facts_are_references_not_design_values(self):
        result=candidate_visible_roof_surface_facts(SOURCE)
        self.assertNotIn("proposed_support_location",result[0])
        self.assertNotIn("new_component_geometry",result[0])

if __name__=="__main__":unittest.main()
