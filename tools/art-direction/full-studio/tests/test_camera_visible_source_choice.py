"""Camera-selected source face IDs replace invisible polygon quantiles."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from contract import RejectedPlan
from professional_source_schema import source_artist_action_schema
from source_geometry_gate import verify_source_plan

SOURCE=[{"name":"Valoria source timber","faces":5338,"edges":15000,
         "face_landmarks":[{"face_index":2326,"local_center_m":[0,0,0]}],
         "camera_visible_face_landmarks":[
             {"face_index":100,"screen_xy":[.25,.45]},
             {"face_index":321,"screen_xy":[.7,.6]}],
         "camera_visibility_verified_by_raycast":True}]
def plan(index):
    return {"actions":[{"op":"extrude","target":SOURCE[0]["name"],
        "params":{"faces":[index],"offset":[0,0,.10]}}]}
class VisibleSourceTests(unittest.TestCase):
    def test_qwen_faces_are_restricted_to_actual_camera_visible_faces(self):
        types=source_artist_action_schema(SOURCE)["oneOf"]
        extrude=next(t for t in types if t["properties"]["op"]["const"]=="extrude")
        item=extrude["properties"]["params"]["properties"]["faces"]["items"]
        self.assertEqual(item["enum"],[100,321])
        self.assertNotIn(2326,item["enum"])
    def test_hidden_face_is_rejected_without_repair(self):
        d=plan(2326);before=copy.deepcopy(d)
        with self.assertRaisesRegex(RejectedPlan,"NOT CAMERA-VISIBLE"):
            verify_source_plan(d,SOURCE)
        self.assertEqual(d,before)
    def test_visible_face_can_still_be_chosen_by_artist(self):
        d=plan(321);self.assertTrue(verify_source_plan(d,SOURCE))
    def test_older_inspector_without_visibility_proof_does_not_claim_visibility(self):
        old=copy.deepcopy(SOURCE)
        del old[0]["camera_visible_face_landmarks"]
        del old[0]["camera_visibility_verified_by_raycast"]
        self.assertTrue(verify_source_plan(plan(2326),old))
if __name__=="__main__":unittest.main()
