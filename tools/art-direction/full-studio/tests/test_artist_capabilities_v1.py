"""No-network/no-Blender regressions for the existing artist capability bridge."""
import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from artist_capabilities_v1 import (
    capability_catalog, artist_skill_prompt, strict_preflight_plan,
    check_source_environment, visual_intent_payload,
    is_semantically_reviewable, failure_category, source_contact_facts)
from contract import RejectedPlan
from test_contract import good


class ArtistCapabilityBridgeTests(unittest.TestCase):
    def test_catalog_never_invents_geometry_tool(self):
        catalog = capability_catalog()
        self.assertIn("masonry_and_parapet", catalog["skills"])
        self.assertIn("sculpt", catalog["atomic_blender_ops"])
        self.assertNotIn("geometry", catalog["atomic_blender_ops"])
        self.assertFalse(catalog["unsafe_arbitrary_python"])

    def test_timber_skill_is_real_and_source_grounded(self):
        catalog = capability_catalog(existing_source=True)
        self.assertIn("timber_bracing_and_joinery", catalog["skills"])
        self.assertIn("timber_roof_and_eaves", catalog["skills"])
        prompt = artist_skill_prompt(existing_source=True, phase="full")
        self.assertIn("NOT stone masonry", prompt)
        self.assertIn("WORLD +Z", prompt)

    def test_world_anchors_reject_hidden_or_unverified_points(self):
        inventory = [{"name":"Original timber tower",
                      "camera_visible_face_landmarks":[
            {"face_index":325,"camera_visible":True,
             "camera_ray_hit_original_face":True,
             "camera_screen_uv":[0.7161,0.6237],
             "world_center_m":[0.8313,1.091,0.8972],
             "world_height_fraction":0.2743},
            {"face_index":99,"camera_visible":False,
             "camera_ray_hit_original_face":True,
             "camera_screen_uv":[0.1,0.1],"world_center_m":[1,2,3]},
            {"face_index":100,"camera_visible":True,
             "camera_ray_hit_original_face":False,
             "camera_screen_uv":[0.1,0.1],"world_center_m":[1,2,3]},
            {"face_index":101,"camera_visible":True,
             "camera_ray_hit_original_face":True,
             "camera_screen_uv":[0.1,0.1],"world_center_m":None}
            ]}]
        result = source_contact_facts(inventory)
        self.assertEqual(len(result),1)
        self.assertEqual(len(result[0]["faces"]),1)
        self.assertEqual(result[0]["faces"][0]["id"],325)
        self.assertEqual(result[0]["faces"][0]["world_xyz_m"],
                         [0.8313,1.091,0.8972])

    def test_new_source_gates_inherited_geometry(self):
        c = capability_catalog(existing_source=False)
        self.assertNotIn("extrude", c["skills"]["masonry_and_parapet"]["allowed_ops"])

    def test_actual_architectural_skill_prompt(self):
        txt = artist_skill_prompt(existing_source=True,phase="full")
        self.assertIn("buttress_and_support", txt)
        self.assertIn("not procedural templates", txt.lower())
        self.assertEqual(artist_skill_prompt(phase="mesh_edit"), "")

    def test_typed_valid_plan(self):
        self.assertEqual(strict_preflight_plan(good())["op_families"], ["primitive"])

    def test_typed_invalid_op(self):
        bad = copy.deepcopy(good())
        bad["actions"][0]["op"] = "geometry"
        with self.assertRaises(RejectedPlan):
            strict_preflight_plan(bad)

    def test_cannot_self_approve(self):
        bad = copy.deepcopy(good())
        bad["art_approved"] = True
        with self.assertRaises(RejectedPlan):
            strict_preflight_plan(bad)

    def test_environment_contract_has_all_actions(self):
        result = check_source_environment(ROOT.parents[2])
        self.assertEqual(result["preflight"], "STATIC_PASS")
        self.assertGreater(result["ops_count"], 20)
        self.assertFalse(result["blender_executed"])

    def test_failure_classification(self):
        self.assertEqual(failure_category("JSONDecodeError extra data"), "INTEGRATION")
        self.assertEqual(failure_category("unsupported Blender operation geometry"), "SCHEMA_OR_PLAN")
        self.assertEqual(failure_category("SOURCE_SCULPT_NOT_VISIBLE_FROM_REVIEW_CAMERA"), "VISIBILITY")
        self.assertEqual(failure_category("MODEL_ART_BLOCKS_PROTECTED_ENTRY"), "SPATIAL_SAFETY")
        self.assertEqual(failure_category("MCP socket disconnected"), "EXECUTION")

    def test_visual_intent_real_images_and_size_guard(self):
        from PIL import Image
        with tempfile.TemporaryDirectory() as d:
            root = Path(d)
            for name, w in (("ref",16),("before",16),("after",16)):
                Image.new("RGB",(w,12),(50 if name=="before" else 80,70,30)).save(root/(name+".png"))
            out = visual_intent_payload(
                original_reference=root/"ref.png",before_png=root/"before.png",
                after_png=root/"after.png",plan=good())
            self.assertEqual(out["images"]["before"]["size"],[16,12])
            self.assertEqual(out["intent"]["operations"],["primitive"])
            Image.new("RGB",(17,12)).save(root/"after.png")
            with self.assertRaisesRegex(ValueError, "RESOLUTION_MISMATCH"):
                visual_intent_payload(original_reference=root/"ref.png",
                    before_png=root/"before.png",after_png=root/"after.png",plan=good())

    def test_no_synthetic_image_headers(self):
        with tempfile.TemporaryDirectory() as d:
            root = Path(d)
            for name in ("ref","before","after"):
                (root/(name+".png")).write_bytes(bytes.fromhex("89504e470d0a1a0a")+b"garbage")
            with self.assertRaises(Exception):
                visual_intent_payload(original_reference=root/"ref.png",
                    before_png=root/"before.png",after_png=root/"after.png",plan=good())

    def test_review_mismatch_is_hold(self):
        critique={"intent_alignment":"MISMATCH","clearly_failed":False,
                  "visible_change_observed":"Clearly visible block",
                  "plan_vs_geometry_reason":"Desired beams became an oversized brick"}
        self.assertFalse(is_semantically_reviewable(critique))
        self.assertFalse(is_semantically_reviewable({"clearly_failed":False}))
        critique["intent_alignment"]="MATCH"
        self.assertTrue(is_semantically_reviewable(critique))


class TransportParityProofTests(unittest.TestCase):
    def test_same_plan_can_be_compared_without_claiming_art(self):
        from transport_parity_v1 import compare_manifests
        baseline = {"transport":"direct","blender_execution":True,
                    "matched_review_camera":True,"action_count":2,
                    "execution":[{"op":"sculpt","geometry_changed":True}],
                    "source_component_geometry":[["tower","abc"]]}
        via = dict(baseline, transport="blendermcp")
        result = compare_manifests(baseline, via)
        self.assertTrue(result["same_action_effects"])
        self.assertTrue(result["same_component_geometry"])
        self.assertFalse(result["art_approved"])

    def test_mismatched_geometry_never_gets_parity_pass(self):
        from transport_parity_v1 import compare_manifests
        left = {"transport":"direct","blender_execution":True,
                "matched_review_camera":True,"action_count":1,
                "execution":[{"geometry_changed":True}],
                "source_component_geometry":[["tower","hash-one"]]}
        right = dict(left, transport="blendermcp",
                     source_component_geometry=[["tower","hash-two"]])
        result = compare_manifests(left, right)
        self.assertFalse(result["same_component_geometry"])



if __name__ == "__main__":
    unittest.main()
