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
