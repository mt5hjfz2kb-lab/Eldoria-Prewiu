"""Zero-cost static acceptance for Qwen14B text director + VL8B eyes + real Blender beams."""
import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT))
from test_contract import good
from contract import RejectedPlan,validate_plan
from artist_capabilities_v1 import capability_catalog
from professional_source_schema import source_artist_action_schema
from source_geometry_gate import verify_source_plan
from architectural_component_guard import verify_volume_and_complexity
from architectural_world_placement_gate import reject_obviously_bad_component_placement
from artist_loop import observe_source_with_vl


def original():
    return {"name":"Genuine inherited timber mesh","faces":8000,"vertices":6000,
            "evaluated_triangles":9000,"edges":14000,
            "world_bounds_min_m":[-1.2,-1.2,0],
            "world_bounds_max_m":[1.2,1.2,3.6],
            "camera_visible_face_landmarks":[
                {"face_index":31,"camera_visible":True,"camera_ray_hit_original_face":True,
                 "world_center_m":[-1.0,1.05,2.3],"camera_screen_uv":[0.3,0.5],
                 "world_height_fraction":.63},
                {"face_index":92,"camera_visible":True,"camera_ray_hit_original_face":True,
                 "world_center_m":[1.0,1.05,2.9],"camera_screen_uv":[0.7,0.7],
                 "world_height_fraction":.81},
                {"face_index":0,"camera_visible":False,"camera_ray_hit_original_face":False,
                 "world_center_m":[0,0,0],"camera_screen_uv":[.5,.5],
                 "world_height_fraction":.1},
            ]}


def beam():
    plan=good()
    plan["asset_decision"]="MODIFY"
    plan["actions"]=[{
        "id":"beam_01","op":"beam_between_faces",
        "target":"Eldoria_Added_C01_TimberBrace",
        "reason":"Connect original exterior timber contact faces with meaningful bracing",
        "params":{"source":original()["name"],"start_face":31,
                  "end_face":92,"width_m":.2,"depth_m":.18}}]
    return plan


class Qwen14BGroundedBeamTests(unittest.TestCase):
    def test_catalog_has_verified_beam(self):
        self.assertIn("beam_between_faces",capability_catalog()["atomic_blender_ops"])
        self.assertIn("beam_between_faces",
          capability_catalog()["skills"]["timber_bracing_and_joinery"]["allowed_ops"])

    def test_contract_and_source_stay_intact(self):
        doc=beam()
        self.assertEqual(validate_plan(doc)["geometric_actions"],1)
        self.assertTrue(verify_source_plan(doc,[original()]))
        self.assertTrue(verify_volume_and_complexity(doc,[original()]))
        result=reject_obviously_bad_component_placement(doc,[original()])
        self.assertFalse(result["art_approved"])
        self.assertEqual(result["changes_to_artist_values"],0)

    def test_beam_rejects_identical_and_hidden_faces(self):
        bad=beam();bad["actions"][0]["params"]["end_face"]=31
        with self.assertRaises(RejectedPlan):validate_plan(bad)
        bad=beam();bad["actions"][0]["params"]["end_face"]=0
        with self.assertRaises(RejectedPlan):verify_source_plan(bad,[original()])

    def test_beam_rejects_micro_profile(self):
        bad=beam();bad["actions"][0]["params"]["width_m"]=.001
        with self.assertRaises(RejectedPlan):validate_plan(bad)

    def test_source_typed_schema_only_uses_camera_proven_faces(self):
        variants=source_artist_action_schema([original()])["oneOf"]
        beams=[v for v in variants if v["properties"]["op"].get("const")=="beam_between_faces"]
        self.assertEqual(len(beams),1)
        props=beams[0]["properties"]["params"]["properties"]
        self.assertEqual(props["start_face"]["enum"],[31,92])
        self.assertEqual(props["end_face"]["enum"],[31,92])
        self.assertNotIn(0,props["start_face"]["enum"])

    def test_distinct_local_models_and_actual_picture_bytes(self):
        from PIL import Image
        with tempfile.TemporaryDirectory() as d:
            root=Path(d)
            for i,n in enumerate(("reference","hero","top","side")):
                Image.new("RGB",(32,24),(i*40,90,100)).save(root/(n+".png"))
            args=SimpleNamespace(endpoint="http://127.0.0.1:11434/api/chat",
               vision_model="qwen3-vl:8b-instruct-q4_K_M",model="qwen3:14b")
            response={"source_material":"Wood timber photographed in source",
                      "source_architecture":"Raised wooden watchtower with eaves",
                      "supported_defect":"Side support lacks sufficient bracing",
                      "candidate_attachment_surfaces":"Two visible roof-edge wooden face contacts",
                      "protected_access_roof_stairs":"Keep doorway stairs and roof unmodified",
                      "uncertainties":"Cannot tell whether back edge is visible"}
            observed=[]
            def fake(endpoint,model,messages,max_tokens,timeout,schema):
                observed.append((model,messages))
                return json.dumps(response)
            with patch("artist_loop.request_ollama",side_effect=fake):
                proof=observe_source_with_vl(args,root/"reference.png",root/"hero.png",
                      [root/"top.png",root/"side.png"],"role",root)
                second=observe_source_with_vl(args,root/"reference.png",root/"hero.png",
                      [root/"top.png",root/"side.png"],"role",root)
            self.assertEqual(len(observed),1)
            self.assertEqual(observed[0][0],"qwen3-vl:8b-instruct-q4_K_M")
            self.assertEqual(len(observed[0][1][1]["images"]),3)
            self.assertFalse(proof["director_image_input"])
            self.assertEqual(proof["director_model"],"qwen3:14b")
            self.assertEqual(second["source_camera_sha256"],proof["source_camera_sha256"])
            self.assertFalse(proof["art_approved"])


if __name__=="__main__":
    unittest.main()
