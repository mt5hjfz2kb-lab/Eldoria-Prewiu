"""Screen-visible source IDs outrank arbitrary percentile landmarks for Qwen."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from professional_source_schema import source_artist_action_schema
from artist_loop import schema_for_plan

BASE={"name":"Original frozen tower","faces":5300,"edges":6000,
      "face_landmarks":[{"face_index":2326},{"face_index":137}],
      "edge_landmarks":[{"edge_index":72}]}

def face_items(src):
    variants=source_artist_action_schema(src)["oneOf"]
    e=next(x for x in variants if x["properties"]["op"]["const"]=="extrude")
    return e["properties"]["params"]["properties"]["faces"]["items"]

class CameraFaces(unittest.TestCase):
    def test_verified_review_camera_raycast_takes_precedence(self):
        src=copy.deepcopy(BASE)
        src["camera_visible_face_landmarks"]=[
            {"face_index":400,"camera_visible":True,
             "camera_ray_hit_original_face":True,"camera_screen_uv":[.4,.6],
             "world_height_fraction":.75},
            {"face_index":233,"camera_visible":True,
             "camera_ray_hit_original_face":True,"camera_screen_uv":[.6,.7],
             "world_height_fraction":.8},
            {"face_index":12,"camera_visible":True,
             "camera_ray_hit_original_face":False}]
        self.assertEqual(face_items([src]),{"type":"integer","enum":[233,400]})
        s=schema_for_plan(3,True,"full","architecture",[src["name"]],[src],True)
        choice=next(x for x in s["properties"]["actions"]["prefixItems"][0]["oneOf"]
                    if x["properties"]["op"]["const"]=="extrude")
        self.assertEqual(choice["properties"]["params"]["properties"]["faces"]["items"]["enum"],[233,400])
    def test_old_landmarks_only_when_no_raycast_results(self):
        self.assertEqual(face_items([copy.deepcopy(BASE)]),{"type":"integer","enum":[137,2326]})
    def test_fake_occluded_ray_or_out_of_range_unusable(self):
        source=copy.deepcopy(BASE)
        source["camera_visible_face_landmarks"]=[
           {"face_index":6200,"camera_visible":True,"camera_ray_hit_original_face":True},
           {"face_index":12,"camera_visible":False,"camera_ray_hit_original_face":True}]
        self.assertEqual(face_items([source])["enum"],[137,2326])
if __name__=="__main__":unittest.main()
