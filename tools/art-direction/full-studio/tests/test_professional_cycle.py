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
            1, 3, False, {"clearly_failed":True}))
        self.assertFalse(artist_loop.should_continue_after_critique(
            1, 1, True, {"clearly_failed":True}))
        self.assertTrue(artist_loop.should_continue_after_critique(
            1, 2, False, {}))


if __name__ == "__main__":
    unittest.main()
