"""Regression: a new AI-built blockout must not terminate before higher-level art finishing."""
import importlib.util
import pathlib
import sys
import unittest

ROOT=pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT))
import artist_loop


class ProfessionalArtistCycleTests(unittest.TestCase):
    def test_full_source_tools_are_discoverable_without_preset_artistic_values(self):
        brief=artist_loop.contract_brief(5, source_exists=True, phase="full")
        for token in ("FULL PROFESSIONAL ARTIST MODE", "sculpt=", "material=",
                      "extrude=", "inset=", "modifier=", "native_operator=",
                      "geometry_nodes", "shader_graph", "source"):
            self.assertIn(token, brief)
        self.assertIn("choose which REAL Blender", brief)
        self.assertIn("NOT a fixed primitive recipe", brief)

    def test_source_full_mode_is_discriminated_by_actual_blender_tool(self):
        import json
        schema=artist_loop.schema_for_plan(
            max_actions=4,source_exists=True,phase="full",
            category="architecture",known_objects=["Real Tower"])
        variants=schema["properties"]["actions"]["items"]["oneOf"]
        by_op={x["properties"]["op"]["const"]:x for x in variants}
        for op in ("material","sculpt","sculpt_anchor","modifier","bevel",
                   "uv","native_operator","shader_graph","geometry_nodes",
                   "extrude","inset","subdivide","boolean","primitive","duplicate"):
            self.assertIn(op,by_op)
        self.assertEqual(by_op["modifier"]["properties"]["target"]["enum"],["Real Tower"])
        self.assertNotIn("INSET",by_op["modifier"]["properties"]["params"]["properties"]["kind"]["enum"])
        self.assertIn("SOLIDIFY",by_op["modifier"]["properties"]["params"]["properties"]["kind"]["enum"])
        self.assertEqual(
            by_op["uv"]["properties"]["params"]["properties"]["method"]["enum"],
            ["smart","cube","sphere","cylinder"])
        self.assertNotIn("smart_project",by_op["uv"]["properties"]["params"]["properties"]["method"]["enum"])
        self.assertEqual(by_op["primitive"]["properties"]["target"]["type"],"string")
        self.assertGreater(len(json.dumps(schema)),6000)

    def test_repeated_material_overwrite_is_rejected_not_artistically_repaired(self):
        from contract import RejectedPlan
        actions=[
          {"op":"material","target":"Real Tower"},
          {"op":"uv","target":"Real Tower"},
          {"op":"material","target":"Real Tower"}
        ]
        with self.assertRaisesRegex(RejectedPlan,"only overwrites"):
            artist_loop.reject_overwriting_material_actions(actions)
        self.assertEqual(actions[0]["target"],"Real Tower")
        artist_loop.reject_overwriting_material_actions(
            [{"op":"material","target":"Tower"},
             {"op":"material","target":"Roof"}])

    def test_modifier_specific_options_forbid_ignored_artistic_parameters(self):
        from contract import RejectedPlan,validate_plan
        data={
          "schema_version":2,
          "asset_id":"actual-existing-architecture",
          "observed_defect":"The inherited historic stone has unnaturally sharp edges.",
          "design_intent":"Use the real Blender tool signature for a meaningful stone edit.",
          "asset_decision":"MODIFY",
          "expected_visual_difference":"The existing masonry catches light correctly at existing edges.",
          "mobile_risks":"triangle budget",
          "actions":[{"id":"source_bevel","op":"modifier","target":"Real Tower",
                      "reason":"Physically soften inherited stone mesh edges",
                      "params":{"kind":"BEVEL",
                                "options":{"width":0.1,"segments":2,
                                           "offset":[0,0,1]},"apply":False}}]
        }
        with self.assertRaisesRegex(RejectedPlan,"rejects ignored"):
            validate_plan(data)
        del data["actions"][0]["params"]["options"]["offset"]
        self.assertEqual(validate_plan(data)["action_count"],1)

    def test_real_mesh_authoring_requirement_rejects_shallow_bevel_only(self):
        import tempfile
        import json
        from types import SimpleNamespace
        from pathlib import Path
        from unittest.mock import patch
        from contract import RejectedPlan
        data={
          "schema_version":2,
          "asset_id":"real-existing-stone-tower",
          "observed_defect":"The inherited tower is almost visually flat with no shape improvements.",
          "design_intent":"Introduce meaningful silhouette reauthoring and geometric changes.",
          "asset_decision":"MODIFY",
          "expected_visual_difference":"Genuine inherited masonry vertices would change on disk.",
          "mobile_risks":"keep triangles bounded",
          "actions":[{"id":"only_bevel","op":"bevel","target":"Real Tower",
                      "reason":"Subtly smooth only the edges of the building",
                      "params":{"width":0.08,"segments":2,"apply":False}}]
        }
        args=SimpleNamespace(max_actions=4,phase="full",asset_category="architecture",
          brief="Change original tower silhouette",max_tokens=1500,
          model="qwen3-vl:4b-instruct",endpoint="http://127.0.0.1:11434/api/chat",
          require_inherited_geometry_edit=True)
        with tempfile.TemporaryDirectory() as temp:
            with patch.object(artist_loop,"image_data",return_value="FAKE_TEST_IMAGE"),patch.object(
              artist_loop,"request_ollama",return_value=json.dumps(data)):
                with self.assertRaisesRegex(RejectedPlan,"REAL inherited source-mesh redesign"):
                    artist_loop.ask_plan(args,Path(temp)/"no-network.png",None,
                        "Real source inspected","artist","verified",Path(temp),1,
                        source_exists=True,known_objects=["Real Tower"])

    def test_initial_new_asset_cannot_self_certify_primitive_blockout(self):
        self.assertTrue(artist_loop.should_continue_after_critique(
            1, 3, True, {"clearly_failed":False}))
        self.assertFalse(artist_loop.should_continue_after_critique(
            2, 3, True, {"clearly_failed":False}))
        self.assertTrue(artist_loop.should_continue_after_critique(
            2, 3, True, {"clearly_failed":True}))
        self.assertFalse(artist_loop.should_continue_after_critique(
            3, 3, True, {"clearly_failed":True}))

    def test_existing_asset_still_respects_visual_critique_and_requested_limit(self):
        self.assertFalse(artist_loop.should_continue_after_critique(
            1, 3, False, {"clearly_failed":False}))
        self.assertTrue(artist_loop.should_continue_after_critique(
            1, 3, False, {"clearly_failed":False}, min_cycles=2))
        self.assertFalse(artist_loop.should_continue_after_critique(
            2, 3, False, {"clearly_failed":False}, min_cycles=2))
        self.assertTrue(artist_loop.should_continue_after_critique(
            1, 3, False, {"clearly_failed":True}))
        self.assertFalse(artist_loop.should_continue_after_critique(
            1, 1, True, {"clearly_failed":True}))
        self.assertTrue(artist_loop.should_continue_after_critique(
            1, 2, False, {}))


if __name__ == "__main__":
    unittest.main()
